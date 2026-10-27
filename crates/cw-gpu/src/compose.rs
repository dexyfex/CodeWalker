//! Shows one [`TileTexture`] in a viewport: pan, zoom and a checkerboard under
//! transparency, drawn into an offscreen target the UI then displays.
//!
//! Channel isolation and inversion are not done here: they are filters over the
//! tiles ([`crate::tiles::channel_filter`]), run on all cores, and only the tiles
//! they change are uploaded again. This pass only places the result on screen.

use std::num::NonZeroU64;

use bytemuck::{Pod, Zeroable};

use crate::tiles::TileTexture;

const FORMAT: wgpu::TextureFormat = wgpu::TextureFormat::Rgba8Unorm;

/// How far the view may zoom out and in (screen pixels per image pixel).
pub const MIN_ZOOM: f32 = 1.0 / 32.0;
pub const MAX_ZOOM: f32 = 64.0;

// ------------------------------------------------------------- view maths

/// Where the viewport looks: `center` is the image pixel in the middle of it.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct ImageView {
    pub zoom: f32,
    pub center: (f32, f32),
}

impl ImageView {
    /// The whole image visible, as large as it can be without leaving the viewport. Small
    /// images are not blown up past 1:1.
    pub fn fit(image: (usize, usize), viewport: (f32, f32)) -> Self {
        let (iw, ih) = (image.0.max(1) as f32, image.1.max(1) as f32);
        let zoom = (viewport.0 / iw).min(viewport.1 / ih).clamp(MIN_ZOOM, 1.0);
        Self { zoom, center: (iw / 2.0, ih / 2.0) }
    }

    /// One image pixel per screen pixel, centred.
    pub fn actual(image: (usize, usize)) -> Self {
        Self { zoom: 1.0, center: (image.0 as f32 / 2.0, image.1 as f32 / 2.0) }
    }

    /// Top-left corner of the image in viewport pixels.
    pub fn offset(&self, viewport: (f32, f32)) -> (f32, f32) {
        (viewport.0 / 2.0 - self.center.0 * self.zoom, viewport.1 / 2.0 - self.center.1 * self.zoom)
    }

    /// The image pixel under a viewport position (fractional; may be outside the image).
    pub fn to_image(&self, at: (f32, f32), viewport: (f32, f32)) -> (f32, f32) {
        let o = self.offset(viewport);
        ((at.0 - o.0) / self.zoom, (at.1 - o.1) / self.zoom)
    }

    /// The whole image pixel under a viewport position, if it is inside the image.
    pub fn pixel_at(
        &self,
        at: (f32, f32),
        viewport: (f32, f32),
        image: (usize, usize),
    ) -> Option<(usize, usize)> {
        let (x, y) = self.to_image(at, viewport);
        (x >= 0.0 && y >= 0.0 && x < image.0 as f32 && y < image.1 as f32).then_some((x as usize, y as usize))
    }

    /// Zooms by `factor`, keeping the image point under `at` where it is.
    pub fn zoom_at(&mut self, at: (f32, f32), viewport: (f32, f32), factor: f32) {
        let before = self.to_image(at, viewport);
        self.zoom = (self.zoom * factor).clamp(MIN_ZOOM, MAX_ZOOM);
        // the same image point must sit under `at` again
        self.center = (
            before.0 - (at.0 - viewport.0 / 2.0) / self.zoom,
            before.1 - (at.1 - viewport.1 / 2.0) / self.zoom,
        );
    }

    /// Drags the image by a screen-space delta.
    pub fn pan(&mut self, dx: f32, dy: f32) {
        self.center = (self.center.0 - dx / self.zoom, self.center.1 - dy / self.zoom);
    }

    /// Keeps at least part of the image in view: the centre stays inside the image.
    pub fn clamp(&mut self, image: (usize, usize)) {
        self.center.0 = self.center.0.clamp(0.0, image.0 as f32);
        self.center.1 = self.center.1.clamp(0.0, image.1 as f32);
    }
}

// -------------------------------------------------------------------- gpu

/// What to draw and where, in target pixels.
#[derive(Debug, Clone, Copy)]
pub struct ViewParams {
    pub view: ImageView,
    /// Bilinear filtering; off shows exact pixels (what you want when zoomed in).
    pub smooth: bool,
    pub ground: [f32; 4],
    pub checker: ([f32; 4], [f32; 4]),
    /// Checker cell size in target pixels.
    pub checker_px: f32,
}

#[repr(C)]
#[derive(Clone, Copy, Pod, Zeroable)]
struct Params {
    target_size: [f32; 2],
    image_size: [f32; 2],
    offset: [f32; 2],
    zoom: f32,
    pad_zoom: f32,
    ground: [f32; 4],
    check_a: [f32; 4],
    check_b: [f32; 4],
    check_px: f32,
    pad: [f32; 3],
}

pub struct ComposeTarget {
    pub size: (u32, u32),
    pub texture: wgpu::Texture,
    pub view: wgpu::TextureView,
}

impl ComposeTarget {
    pub fn new(device: &wgpu::Device, width: u32, height: u32) -> Self {
        let (w, h) = (width.clamp(1, 8192), height.clamp(1, 8192));
        let texture = device.create_texture(&wgpu::TextureDescriptor {
            label: Some("image view"),
            size: wgpu::Extent3d { width: w, height: h, depth_or_array_layers: 1 },
            mip_level_count: 1,
            sample_count: 1,
            dimension: wgpu::TextureDimension::D2,
            format: FORMAT,
            usage: wgpu::TextureUsages::RENDER_ATTACHMENT
                | wgpu::TextureUsages::TEXTURE_BINDING
                | wgpu::TextureUsages::COPY_SRC,
            view_formats: &[],
        });
        let view = texture.create_view(&wgpu::TextureViewDescriptor::default());
        Self { size: (w, h), texture, view }
    }
}

pub struct Compositor {
    pipeline: wgpu::RenderPipeline,
    layout: wgpu::BindGroupLayout,
    params: wgpu::Buffer,
    nearest: wgpu::Sampler,
    bilinear: wgpu::Sampler,
}

impl Compositor {
    pub fn new(device: &wgpu::Device) -> Self {
        let shader = device.create_shader_module(wgpu::ShaderModuleDescriptor {
            label: Some("image view shader"),
            source: wgpu::ShaderSource::Wgsl(include_str!("compose.wgsl").into()),
        });
        let entry =
            |binding, visibility, ty| wgpu::BindGroupLayoutEntry { binding, visibility, ty, count: None };
        let layout = device.create_bind_group_layout(&wgpu::BindGroupLayoutDescriptor {
            label: Some("image view layout"),
            entries: &[
                entry(
                    0,
                    wgpu::ShaderStages::VERTEX_FRAGMENT,
                    wgpu::BindingType::Buffer {
                        ty: wgpu::BufferBindingType::Uniform,
                        has_dynamic_offset: false,
                        min_binding_size: NonZeroU64::new(std::mem::size_of::<Params>() as u64),
                    },
                ),
                entry(
                    1,
                    wgpu::ShaderStages::FRAGMENT,
                    wgpu::BindingType::Texture {
                        sample_type: wgpu::TextureSampleType::Float { filterable: true },
                        view_dimension: wgpu::TextureViewDimension::D2,
                        multisampled: false,
                    },
                ),
                entry(
                    2,
                    wgpu::ShaderStages::FRAGMENT,
                    wgpu::BindingType::Sampler(wgpu::SamplerBindingType::Filtering),
                ),
            ],
        });
        let pipeline_layout = device.create_pipeline_layout(&wgpu::PipelineLayoutDescriptor {
            label: Some("image view pipeline layout"),
            bind_group_layouts: &[Some(&layout)],
            immediate_size: 0,
        });
        let pipeline = device.create_render_pipeline(&wgpu::RenderPipelineDescriptor {
            label: Some("image view"),
            layout: Some(&pipeline_layout),
            vertex: wgpu::VertexState {
                module: &shader,
                entry_point: Some("vs"),
                compilation_options: Default::default(),
                buffers: &[],
            },
            primitive: wgpu::PrimitiveState::default(),
            depth_stencil: None,
            multisample: wgpu::MultisampleState::default(),
            fragment: Some(wgpu::FragmentState {
                module: &shader,
                entry_point: Some("fs"),
                compilation_options: Default::default(),
                targets: &[Some(wgpu::ColorTargetState {
                    format: FORMAT,
                    blend: None,
                    write_mask: wgpu::ColorWrites::ALL,
                })],
            }),
            multiview_mask: None,
            cache: None,
        });
        let sampler = |filter| {
            device.create_sampler(&wgpu::SamplerDescriptor {
                address_mode_u: wgpu::AddressMode::ClampToEdge,
                address_mode_v: wgpu::AddressMode::ClampToEdge,
                mag_filter: filter,
                min_filter: filter,
                ..Default::default()
            })
        };
        Self {
            pipeline,
            layout,
            params: device.create_buffer(&wgpu::BufferDescriptor {
                label: Some("image view params"),
                size: std::mem::size_of::<Params>() as u64,
                usage: wgpu::BufferUsages::UNIFORM | wgpu::BufferUsages::COPY_DST,
                mapped_at_creation: false,
            }),
            nearest: sampler(wgpu::FilterMode::Nearest),
            bilinear: sampler(wgpu::FilterMode::Linear),
        }
    }

    /// Draws `image` into `target` and submits the work, so the texture is ready when the UI
    /// samples it later in the same frame.
    pub fn render(
        &self,
        device: &wgpu::Device,
        queue: &wgpu::Queue,
        target: &ComposeTarget,
        image: &TileTexture,
        params: &ViewParams,
    ) {
        let viewport = (target.size.0 as f32, target.size.1 as f32);
        let offset = params.view.offset(viewport);
        let raw = Params {
            target_size: [viewport.0, viewport.1],
            image_size: [image.size.0 as f32, image.size.1 as f32],
            offset: [offset.0, offset.1],
            zoom: params.view.zoom,
            pad_zoom: 0.0,
            ground: params.ground,
            check_a: params.checker.0,
            check_b: params.checker.1,
            check_px: params.checker_px.max(1.0),
            pad: [0.0; 3],
        };
        queue.write_buffer(&self.params, 0, bytemuck::bytes_of(&raw));
        let group = device.create_bind_group(&wgpu::BindGroupDescriptor {
            label: Some("image view"),
            layout: &self.layout,
            entries: &[
                wgpu::BindGroupEntry { binding: 0, resource: self.params.as_entire_binding() },
                wgpu::BindGroupEntry {
                    binding: 1,
                    resource: wgpu::BindingResource::TextureView(&image.view),
                },
                wgpu::BindGroupEntry {
                    binding: 2,
                    resource: wgpu::BindingResource::Sampler(if params.smooth {
                        &self.bilinear
                    } else {
                        &self.nearest
                    }),
                },
            ],
        });
        let mut enc =
            device.create_command_encoder(&wgpu::CommandEncoderDescriptor { label: Some("image view") });
        {
            let mut pass = enc.begin_render_pass(&wgpu::RenderPassDescriptor {
                label: Some("image view pass"),
                color_attachments: &[Some(wgpu::RenderPassColorAttachment {
                    view: &target.view,
                    depth_slice: None,
                    resolve_target: None,
                    ops: wgpu::Operations {
                        load: wgpu::LoadOp::Clear(wgpu::Color::BLACK),
                        store: wgpu::StoreOp::Store,
                    },
                })],
                depth_stencil_attachment: None,
                timestamp_writes: None,
                occlusion_query_set: None,
                multiview_mask: None,
            });
            pass.set_pipeline(&self.pipeline);
            pass.set_bind_group(0, &group, &[]);
            pass.draw(0..3, 0..1);
        }
        queue.submit([enc.finish()]);
    }
}

/// Reads a target back as tightly packed RGBA8 (tests).
pub fn read_target(device: &wgpu::Device, queue: &wgpu::Queue, target: &ComposeTarget) -> Vec<u8> {
    let (w, h) = target.size;
    let row = (w * 4).next_multiple_of(wgpu::COPY_BYTES_PER_ROW_ALIGNMENT);
    let buf = device.create_buffer(&wgpu::BufferDescriptor {
        label: Some("image view readback"),
        size: (row * h) as u64,
        usage: wgpu::BufferUsages::COPY_DST | wgpu::BufferUsages::MAP_READ,
        mapped_at_creation: false,
    });
    let mut enc = device.create_command_encoder(&wgpu::CommandEncoderDescriptor { label: Some("readback") });
    enc.copy_texture_to_buffer(
        target.texture.as_image_copy(),
        wgpu::TexelCopyBufferInfo {
            buffer: &buf,
            layout: wgpu::TexelCopyBufferLayout {
                offset: 0,
                bytes_per_row: Some(row),
                rows_per_image: Some(h),
            },
        },
        wgpu::Extent3d { width: w, height: h, depth_or_array_layers: 1 },
    );
    queue.submit([enc.finish()]);
    let slice = buf.slice(..);
    slice.map_async(wgpu::MapMode::Read, |_| {});
    let _ = device.poll(wgpu::PollType::wait_indefinitely());
    let data = slice.get_mapped_range().expect("readback buffer was mapped");
    let mut out = Vec::with_capacity((w * h * 4) as usize);
    for y in 0..h {
        let start = (y * row) as usize;
        out.extend_from_slice(&data[start..start + (w * 4) as usize]);
    }
    out
}

#[cfg(test)]
mod tests;
