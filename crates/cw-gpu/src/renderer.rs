use std::num::NonZeroU64;

use bytemuck::{Pod, Zeroable};
use wgpu::util::DeviceExt;

const COLOR_FORMAT: wgpu::TextureFormat = wgpu::TextureFormat::Rgba8Unorm;
const DEPTH_FORMAT: wgpu::TextureFormat = wgpu::TextureFormat::Depth32Float;
pub const SAMPLES: u32 = 4;

/// Background of the viewport: a hair lighter than the UI ground so its edge reads.
const CLEAR: wgpu::Color = wgpu::Color { r: 0.075, g: 0.075, b: 0.085, a: 1.0 };

#[repr(C)]
#[derive(Clone, Copy, Pod, Zeroable)]
pub struct Globals {
    pub view_proj: [[f32; 4]; 4],
    pub camera: [f32; 4],
    pub grid: [f32; 4],
}

/// One box: a unit cube (side 1, centred) placed by `model`.
#[repr(C)]
#[derive(Clone, Copy, Pod, Zeroable, Debug)]
pub struct BoxInstance {
    pub model: [[f32; 4]; 4],
    pub color: [f32; 4],
}

#[repr(C)]
#[derive(Clone, Copy, Pod, Zeroable, Debug)]
pub struct LineVertex {
    pub pos: [f32; 3],
    pub color: [f32; 4],
}

#[repr(C)]
#[derive(Clone, Copy, Pod, Zeroable)]
struct MeshVertex {
    pos: [f32; 3],
    normal: [f32; 3],
}

pub struct GpuScene<'a> {
    pub boxes: &'a [BoxInstance],
    /// Depth-tested lines (hidden behind nearer geometry).
    pub lines: &'a [LineVertex],
    /// Lines drawn on top of everything: selection outlines and gizmos.
    pub overlay: &'a [LineVertex],
}

/// Grid cell size that suits a camera at height `h`: powers of ten, so the
/// major lines (every 10 cells) land on round numbers.
pub fn grid_spacing(height: f32) -> f32 {
    let h = height.abs().max(1.0);
    10f32.powf((h / 4.0).log10().round().max(-1.0))
}

pub struct Target {
    pub size: (u32, u32),
    pub color: wgpu::Texture,
    pub view: wgpu::TextureView,
    msaa_view: wgpu::TextureView,
    depth_view: wgpu::TextureView,
}

impl Target {
    pub fn new(device: &wgpu::Device, width: u32, height: u32) -> Self {
        let (w, h) = (width.clamp(1, 8192), height.clamp(1, 8192));
        let extent = wgpu::Extent3d { width: w, height: h, depth_or_array_layers: 1 };
        let make = |label, format, samples, usage| {
            device.create_texture(&wgpu::TextureDescriptor {
                label: Some(label),
                size: extent,
                mip_level_count: 1,
                sample_count: samples,
                dimension: wgpu::TextureDimension::D2,
                format,
                usage,
                view_formats: &[],
            })
        };
        let color = make(
            "viewport color",
            COLOR_FORMAT,
            1,
            wgpu::TextureUsages::RENDER_ATTACHMENT
                | wgpu::TextureUsages::TEXTURE_BINDING
                | wgpu::TextureUsages::COPY_SRC,
        );
        let msaa = make("viewport msaa", COLOR_FORMAT, SAMPLES, wgpu::TextureUsages::RENDER_ATTACHMENT);
        let depth = make("viewport depth", DEPTH_FORMAT, SAMPLES, wgpu::TextureUsages::RENDER_ATTACHMENT);
        let v = |t: &wgpu::Texture| t.create_view(&wgpu::TextureViewDescriptor::default());
        Self { size: (w, h), view: v(&color), msaa_view: v(&msaa), depth_view: v(&depth), color }
    }
}

struct Growable {
    buffer: wgpu::Buffer,
    capacity: u64,
    usage: wgpu::BufferUsages,
    label: &'static str,
}

impl Growable {
    fn new(device: &wgpu::Device, label: &'static str, usage: wgpu::BufferUsages) -> Self {
        let capacity = 4096;
        Self { buffer: Self::alloc(device, label, usage, capacity), capacity, usage, label }
    }

    fn alloc(
        device: &wgpu::Device,
        label: &'static str,
        usage: wgpu::BufferUsages,
        size: u64,
    ) -> wgpu::Buffer {
        device.create_buffer(&wgpu::BufferDescriptor {
            label: Some(label),
            size,
            usage: usage | wgpu::BufferUsages::COPY_DST,
            mapped_at_creation: false,
        })
    }

    fn upload(&mut self, device: &wgpu::Device, queue: &wgpu::Queue, bytes: &[u8]) {
        if bytes.len() as u64 > self.capacity {
            self.capacity = (bytes.len() as u64).next_power_of_two();
            self.buffer = Self::alloc(device, self.label, self.usage, self.capacity);
        }
        if !bytes.is_empty() {
            queue.write_buffer(&self.buffer, 0, bytes);
        }
    }
}

pub struct Renderer {
    globals: wgpu::Buffer,
    bind_group: wgpu::BindGroup,
    boxes: wgpu::RenderPipeline,
    lines: wgpu::RenderPipeline,
    overlay: wgpu::RenderPipeline,
    grid: wgpu::RenderPipeline,
    cube_vertices: wgpu::Buffer,
    cube_indices: wgpu::Buffer,
    instances: Growable,
    line_buffer: Growable,
    overlay_buffer: Growable,
}

impl Renderer {
    pub fn new(device: &wgpu::Device) -> Self {
        let shader = device.create_shader_module(wgpu::ShaderModuleDescriptor {
            label: Some("viewport shaders"),
            source: wgpu::ShaderSource::Wgsl(include_str!("shaders.wgsl").into()),
        });
        let globals = device.create_buffer(&wgpu::BufferDescriptor {
            label: Some("globals"),
            size: std::mem::size_of::<Globals>() as u64,
            usage: wgpu::BufferUsages::UNIFORM | wgpu::BufferUsages::COPY_DST,
            mapped_at_creation: false,
        });
        let bgl = device.create_bind_group_layout(&wgpu::BindGroupLayoutDescriptor {
            label: Some("globals layout"),
            entries: &[wgpu::BindGroupLayoutEntry {
                binding: 0,
                visibility: wgpu::ShaderStages::VERTEX | wgpu::ShaderStages::FRAGMENT,
                ty: wgpu::BindingType::Buffer {
                    ty: wgpu::BufferBindingType::Uniform,
                    has_dynamic_offset: false,
                    min_binding_size: NonZeroU64::new(std::mem::size_of::<Globals>() as u64),
                },
                count: None,
            }],
        });
        let bind_group = device.create_bind_group(&wgpu::BindGroupDescriptor {
            label: Some("globals"),
            layout: &bgl,
            entries: &[wgpu::BindGroupEntry { binding: 0, resource: globals.as_entire_binding() }],
        });
        let layout = device.create_pipeline_layout(&wgpu::PipelineLayoutDescriptor {
            label: Some("viewport layout"),
            bind_group_layouts: &[Some(&bgl)],
            immediate_size: 0,
        });

        let blend = Some(wgpu::BlendState::ALPHA_BLENDING);
        let target = [Some(wgpu::ColorTargetState {
            format: COLOR_FORMAT,
            blend,
            write_mask: wgpu::ColorWrites::ALL,
        })];
        let multisample =
            wgpu::MultisampleState { count: SAMPLES, mask: !0, alpha_to_coverage_enabled: false };
        let depth = |write: bool, cmp: wgpu::CompareFunction| {
            Some(wgpu::DepthStencilState {
                format: DEPTH_FORMAT,
                depth_write_enabled: Some(write),
                depth_compare: Some(cmp),
                stencil: wgpu::StencilState::default(),
                bias: wgpu::DepthBiasState::default(),
            })
        };

        let mesh_attrs = wgpu::vertex_attr_array![0 => Float32x3, 1 => Float32x3];
        let inst_attrs = wgpu::vertex_attr_array![2 => Float32x4, 3 => Float32x4, 4 => Float32x4, 5 => Float32x4, 6 => Float32x4];
        let line_attrs = wgpu::vertex_attr_array![0 => Float32x3, 1 => Float32x4];
        let box_buffers = [
            Some(wgpu::VertexBufferLayout {
                array_stride: std::mem::size_of::<MeshVertex>() as u64,
                step_mode: wgpu::VertexStepMode::Vertex,
                attributes: &mesh_attrs,
            }),
            Some(wgpu::VertexBufferLayout {
                array_stride: std::mem::size_of::<BoxInstance>() as u64,
                step_mode: wgpu::VertexStepMode::Instance,
                attributes: &inst_attrs,
            }),
        ];
        let line_buffers = [Some(wgpu::VertexBufferLayout {
            array_stride: std::mem::size_of::<LineVertex>() as u64,
            step_mode: wgpu::VertexStepMode::Vertex,
            attributes: &line_attrs,
        })];

        let pipeline = |label: &str,
                        vs: &str,
                        fs: &str,
                        buffers: &[Option<wgpu::VertexBufferLayout>],
                        topology,
                        depth_mode: (bool, wgpu::CompareFunction)| {
            device.create_render_pipeline(&wgpu::RenderPipelineDescriptor {
                label: Some(label),
                layout: Some(&layout),
                vertex: wgpu::VertexState {
                    module: &shader,
                    entry_point: Some(vs),
                    compilation_options: Default::default(),
                    buffers,
                },
                primitive: wgpu::PrimitiveState { topology, cull_mode: None, ..Default::default() },
                depth_stencil: depth(depth_mode.0, depth_mode.1),
                multisample,
                fragment: Some(wgpu::FragmentState {
                    module: &shader,
                    entry_point: Some(fs),
                    compilation_options: Default::default(),
                    targets: &target,
                }),
                multiview_mask: None,
                cache: None,
            })
        };
        use wgpu::{CompareFunction::*, PrimitiveTopology::*};
        let boxes = pipeline("boxes", "vs_box", "fs_box", &box_buffers, TriangleList, (true, LessEqual));
        let lines = pipeline("lines", "vs_line", "fs_line", &line_buffers, LineList, (true, LessEqual));
        let overlay = pipeline("overlay", "vs_line", "fs_line", &line_buffers, LineList, (false, Always));
        let grid = pipeline("grid", "vs_grid", "fs_grid", &[], TriangleList, (false, LessEqual));

        let (verts, idx) = cube();
        let cube_vertices = device.create_buffer_init(&wgpu::util::BufferInitDescriptor {
            label: Some("cube vertices"),
            contents: bytemuck::cast_slice(&verts),
            usage: wgpu::BufferUsages::VERTEX,
        });
        let cube_indices = device.create_buffer_init(&wgpu::util::BufferInitDescriptor {
            label: Some("cube indices"),
            contents: bytemuck::cast_slice(&idx),
            usage: wgpu::BufferUsages::INDEX,
        });

        Self {
            globals,
            bind_group,
            boxes,
            lines,
            grid,
            cube_vertices,
            cube_indices,
            instances: Growable::new(device, "box instances", wgpu::BufferUsages::VERTEX),
            line_buffer: Growable::new(device, "lines", wgpu::BufferUsages::VERTEX),
            overlay_buffer: Growable::new(device, "overlay lines", wgpu::BufferUsages::VERTEX),
            overlay,
        }
    }

    /// Draws the scene into `target` and submits the work immediately, so the
    /// texture is ready before the UI samples it later in the same frame.
    pub fn render(
        &mut self,
        device: &wgpu::Device,
        queue: &wgpu::Queue,
        target: &Target,
        globals: &Globals,
        scene: &GpuScene,
    ) {
        queue.write_buffer(&self.globals, 0, bytemuck::bytes_of(globals));
        self.instances.upload(device, queue, bytemuck::cast_slice(scene.boxes));
        self.line_buffer.upload(device, queue, bytemuck::cast_slice(scene.lines));
        self.overlay_buffer.upload(device, queue, bytemuck::cast_slice(scene.overlay));

        let mut enc =
            device.create_command_encoder(&wgpu::CommandEncoderDescriptor { label: Some("viewport") });
        {
            let mut pass = enc.begin_render_pass(&wgpu::RenderPassDescriptor {
                label: Some("viewport pass"),
                color_attachments: &[Some(wgpu::RenderPassColorAttachment {
                    view: &target.msaa_view,
                    depth_slice: None,
                    resolve_target: Some(&target.view),
                    ops: wgpu::Operations { load: wgpu::LoadOp::Clear(CLEAR), store: wgpu::StoreOp::Discard },
                })],
                depth_stencil_attachment: Some(wgpu::RenderPassDepthStencilAttachment {
                    view: &target.depth_view,
                    depth_ops: Some(wgpu::Operations {
                        load: wgpu::LoadOp::Clear(1.0),
                        store: wgpu::StoreOp::Discard,
                    }),
                    stencil_ops: None,
                }),
                timestamp_writes: None,
                occlusion_query_set: None,
                multiview_mask: None,
            });
            pass.set_bind_group(0, &self.bind_group, &[]);

            if !scene.boxes.is_empty() {
                pass.set_pipeline(&self.boxes);
                pass.set_vertex_buffer(0, self.cube_vertices.slice(..));
                pass.set_vertex_buffer(1, self.instances.buffer.slice(..));
                pass.set_index_buffer(self.cube_indices.slice(..), wgpu::IndexFormat::Uint16);
                pass.draw_indexed(0..36, 0, 0..scene.boxes.len() as u32);
            }
            // grid after opaque geometry: it blends and must not write depth
            pass.set_pipeline(&self.grid);
            pass.draw(0..6, 0..1);
            if !scene.lines.is_empty() {
                pass.set_pipeline(&self.lines);
                pass.set_vertex_buffer(0, self.line_buffer.buffer.slice(..));
                pass.draw(0..scene.lines.len() as u32, 0..1);
            }
            if !scene.overlay.is_empty() {
                pass.set_pipeline(&self.overlay);
                pass.set_vertex_buffer(0, self.overlay_buffer.buffer.slice(..));
                pass.draw(0..scene.overlay.len() as u32, 0..1);
            }
        }
        queue.submit([enc.finish()]);
    }

    /// Reads the target back as tightly packed RGBA8 (tests, screenshots).
    pub fn read_pixels(device: &wgpu::Device, queue: &wgpu::Queue, target: &Target) -> Vec<u8> {
        let (w, h) = target.size;
        let row = (w * 4).next_multiple_of(wgpu::COPY_BYTES_PER_ROW_ALIGNMENT);
        let buf = device.create_buffer(&wgpu::BufferDescriptor {
            label: Some("readback"),
            size: (row * h) as u64,
            usage: wgpu::BufferUsages::COPY_DST | wgpu::BufferUsages::MAP_READ,
            mapped_at_creation: false,
        });
        let mut enc =
            device.create_command_encoder(&wgpu::CommandEncoderDescriptor { label: Some("readback") });
        enc.copy_texture_to_buffer(
            target.color.as_image_copy(),
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
}

/// Unit cube with per-face normals, 24 vertices / 36 indices.
fn cube() -> (Vec<MeshVertex>, Vec<u16>) {
    let faces: [([f32; 3], [f32; 3], [f32; 3]); 6] = [
        ([1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 1.0]),
        ([-1.0, 0.0, 0.0], [0.0, 0.0, 1.0], [0.0, 1.0, 0.0]),
        ([0.0, 1.0, 0.0], [0.0, 0.0, 1.0], [1.0, 0.0, 0.0]),
        ([0.0, -1.0, 0.0], [1.0, 0.0, 0.0], [0.0, 0.0, 1.0]),
        ([0.0, 0.0, 1.0], [1.0, 0.0, 0.0], [0.0, 1.0, 0.0]),
        ([0.0, 0.0, -1.0], [0.0, 1.0, 0.0], [1.0, 0.0, 0.0]),
    ];
    let (mut v, mut i) = (Vec::new(), Vec::new());
    for (n, u, w) in faces {
        let base = v.len() as u16;
        for (a, b) in [(-1.0f32, -1.0f32), (1.0, -1.0), (1.0, 1.0), (-1.0, 1.0)] {
            let p = [0, 1, 2].map(|k| (n[k] + u[k] * a + w[k] * b) * 0.5);
            v.push(MeshVertex { pos: p, normal: n });
        }
        i.extend_from_slice(&[base, base + 1, base + 2, base, base + 2, base + 3]);
    }
    (v, i)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn grid_spacing_steps_by_powers_of_ten() {
        assert_eq!(grid_spacing(1.0), 0.1);
        assert_eq!(grid_spacing(40.0), 10.0);
        assert_eq!(grid_spacing(400.0), 100.0);
        assert_eq!(grid_spacing(-400.0), 100.0, "height below the plane behaves the same");
        assert!(grid_spacing(0.0).is_finite());
    }

    #[test]
    fn cube_is_closed_with_outward_normals() {
        let (v, i) = cube();
        assert_eq!((v.len(), i.len()), (24, 36));
        for tri in i.chunks(3) {
            let p: Vec<glam::Vec3> = tri.iter().map(|&k| glam::Vec3::from(v[k as usize].pos)).collect();
            let n = glam::Vec3::from(v[tri[0] as usize].normal);
            let face_normal = (p[1] - p[0]).cross(p[2] - p[0]).normalize();
            assert!(face_normal.dot(n) > 0.99, "triangle winds against its normal: {tri:?}");
            assert!(p[0].dot(n) > 0.49, "face lies on the cube surface");
        }
        assert!(v.iter().all(|x| x.pos.iter().all(|c| c.abs() <= 0.5 + 1e-6)));
    }

    /// Needs a GPU/software adapter; skipped where none exists (CI without GL/Vulkan).
    fn adapter_device() -> Option<(wgpu::Device, wgpu::Queue)> {
        let instance = wgpu::Instance::new(wgpu::InstanceDescriptor::new_without_display_handle());
        let adapter =
            pollster::block_on(instance.request_adapter(&wgpu::RequestAdapterOptions::default())).ok()?;
        pollster::block_on(adapter.request_device(&wgpu::DeviceDescriptor::default())).ok()
    }

    #[test]
    fn renders_a_box_over_the_background() {
        let Some((device, queue)) = adapter_device() else {
            eprintln!("no wgpu adapter available; skipping GPU test");
            return;
        };
        let target = Target::new(&device, 64, 64);
        let mut r = Renderer::new(&device);
        let cam = crate::Camera {
            position: glam::Vec3::new(-6.0, 0.0, 0.0),
            yaw: 0.0,
            pitch: 0.0,
            ..Default::default()
        };
        let globals = Globals {
            view_proj: cam.view_proj(1.0).to_cols_array_2d(),
            camera: [cam.position.x, cam.position.y, cam.position.z, 1.0],
            grid: [10.0, 500.0, 0.0, 0.0],
        };
        let b = BoxInstance {
            model: glam::Mat4::from_scale(glam::Vec3::splat(2.0)).to_cols_array_2d(),
            color: [1.0, 0.2, 0.2, 1.0],
        };
        r.render(&device, &queue, &target, &globals, &GpuScene { boxes: &[b], lines: &[], overlay: &[] });
        let px = Renderer::read_pixels(&device, &queue, &target);
        let at = |x: usize, y: usize| &px[(y * 64 + x) * 4..(y * 64 + x) * 4 + 4];
        let centre = at(32, 32);
        let corner = at(1, 1);
        // the face toward the camera (normal -X) gets only the ambient term: 0.36 * 255 = 92
        assert!(
            (centre[0] as i32 - 92).abs() <= 4 && (centre[1] as i32 - 18).abs() <= 4,
            "the red box covers the middle: {centre:?}"
        );
        assert!(corner[0] < 60 && corner[3] == 255, "the corner is background: {corner:?}");
    }
}
