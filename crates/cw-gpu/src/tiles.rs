//! GPU side of the copy-on-write tiled images (the images and their filters
//! are in `cw_core::image`).

use std::sync::Arc;

pub use cw_core::image::{Channels, TILE, Tile, TiledImage, channel_filter, invert_filter};

/// A GPU texture kept in sync with a [`TiledImage`], uploading only changed tiles.
pub struct TileTexture {
    pub texture: wgpu::Texture,
    pub view: wgpu::TextureView,
    pub size: (usize, usize),
    uploaded: Vec<Option<Tile>>,
}

impl TileTexture {
    pub fn new(device: &wgpu::Device, width: usize, height: usize) -> Self {
        let texture = device.create_texture(&wgpu::TextureDescriptor {
            label: Some("tiled image"),
            size: wgpu::Extent3d { width: width as u32, height: height as u32, depth_or_array_layers: 1 },
            mip_level_count: 1,
            sample_count: 1,
            dimension: wgpu::TextureDimension::D2,
            format: wgpu::TextureFormat::Rgba8Unorm,
            usage: wgpu::TextureUsages::TEXTURE_BINDING
                | wgpu::TextureUsages::COPY_DST
                | wgpu::TextureUsages::COPY_SRC,
            view_formats: &[],
        });
        let view = texture.create_view(&wgpu::TextureViewDescriptor::default());
        Self {
            texture,
            view,
            size: (width, height),
            uploaded: vec![None; width.div_ceil(TILE).max(1) * height.div_ceil(TILE).max(1)],
        }
    }

    /// Uploads the tiles that differ from what is already on the GPU; returns how many.
    pub fn sync(&mut self, queue: &wgpu::Queue, image: &TiledImage) -> usize {
        assert_eq!((image.width, image.height), self.size, "texture and image sizes differ");
        let mut sent = 0;
        for i in 0..image.tile_count() {
            if self.uploaded[i].as_ref().is_some_and(|t| Arc::ptr_eq(t, image.tile(i))) {
                continue;
            }
            let (x, y, w, h) = image.tile_rect(i);
            queue.write_texture(
                wgpu::TexelCopyTextureInfo {
                    texture: &self.texture,
                    mip_level: 0,
                    origin: wgpu::Origin3d { x: x as u32, y: y as u32, z: 0 },
                    aspect: wgpu::TextureAspect::All,
                },
                image.tile_data(i),
                wgpu::TexelCopyBufferLayout {
                    offset: 0,
                    bytes_per_row: Some((TILE * 4) as u32),
                    rows_per_image: Some(TILE as u32),
                },
                wgpu::Extent3d { width: w as u32, height: h as u32, depth_or_array_layers: 1 },
            );
            self.uploaded[i] = Some(image.tile(i).clone());
            sent += 1;
        }
        sent
    }

    /// Reads the texture back as tightly packed RGBA8 (tests).
    pub fn read_back(&self, device: &wgpu::Device, queue: &wgpu::Queue) -> Vec<u8> {
        let (w, h) = (self.size.0 as u32, self.size.1 as u32);
        let row = (w * 4).next_multiple_of(wgpu::COPY_BYTES_PER_ROW_ALIGNMENT);
        let buf = device.create_buffer(&wgpu::BufferDescriptor {
            label: Some("tile readback"),
            size: (row * h) as u64,
            usage: wgpu::BufferUsages::COPY_DST | wgpu::BufferUsages::MAP_READ,
            mapped_at_creation: false,
        });
        let mut enc =
            device.create_command_encoder(&wgpu::CommandEncoderDescriptor { label: Some("tile readback") });
        enc.copy_texture_to_buffer(
            self.texture.as_image_copy(),
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

#[cfg(test)]
mod tests;
