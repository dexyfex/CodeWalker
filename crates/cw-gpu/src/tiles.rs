//! Copy-on-write tiled images.
//!
//! An image is cut into 128x128 tiles, each an `Arc`'d buffer. Cloning an
//! image or applying a filter shares every tile that did not change, so
//! several views of one texture (colour, single channels, history steps) cost
//! only the tiles that differ, and the GPU upload sends only those.

use std::sync::Arc;

use rayon::prelude::*;

pub const TILE: usize = 128;
const TILE_BYTES: usize = TILE * TILE * 4;

type Tile = Arc<[u8]>;

#[derive(Clone)]
pub struct TiledImage {
    pub width: usize,
    pub height: usize,
    tiles_x: usize,
    tiles_y: usize,
    tiles: Vec<Tile>,
}

impl std::fmt::Debug for TiledImage {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "TiledImage {}x{} ({} tiles)", self.width, self.height, self.tiles.len())
    }
}

impl TiledImage {
    /// Cuts tightly packed RGBA8 into tiles. Pixels past the edge are transparent black.
    pub fn from_rgba(width: usize, height: usize, rgba: &[u8]) -> Self {
        assert_eq!(rgba.len(), width * height * 4, "pixel buffer does not match the size");
        let (tiles_x, tiles_y) = (width.div_ceil(TILE).max(1), height.div_ceil(TILE).max(1));
        let tiles: Vec<Tile> = (0..tiles_x * tiles_y)
            .into_par_iter()
            .map(|i| {
                let (tx, ty) = (i % tiles_x, i / tiles_x);
                let mut buf = vec![0u8; TILE_BYTES];
                let w = TILE.min(width.saturating_sub(tx * TILE));
                for row in 0..TILE.min(height.saturating_sub(ty * TILE)) {
                    let src = ((ty * TILE + row) * width + tx * TILE) * 4;
                    buf[row * TILE * 4..row * TILE * 4 + w * 4].copy_from_slice(&rgba[src..src + w * 4]);
                }
                Tile::from(buf)
            })
            .collect();
        Self { width, height, tiles_x, tiles_y, tiles }
    }

    pub fn tile_count(&self) -> usize {
        self.tiles.len()
    }

    pub fn tiles_x(&self) -> usize {
        self.tiles_x
    }

    pub fn tile_data(&self, index: usize) -> &[u8] {
        &self.tiles[index]
    }

    /// Pixel region `(x, y, w, h)` that tile `index` covers inside the image.
    pub fn tile_rect(&self, index: usize) -> (usize, usize, usize, usize) {
        let (tx, ty) = (index % self.tiles_x, index / self.tiles_x);
        let (x, y) = (tx * TILE, ty * TILE);
        (x, y, TILE.min(self.width - x), TILE.min(self.height - y))
    }

    /// Back to tightly packed RGBA8.
    pub fn to_rgba(&self) -> Vec<u8> {
        let mut out = vec![0u8; self.width * self.height * 4];
        for (i, tile) in self.tiles.iter().enumerate() {
            let (x, y, w, h) = self.tile_rect(i);
            for row in 0..h {
                let dst = ((y + row) * self.width + x) * 4;
                out[dst..dst + w * 4].copy_from_slice(&tile[row * TILE * 4..row * TILE * 4 + w * 4]);
            }
        }
        out
    }

    /// Whether `other` holds the very same buffer for tile `index` (not just equal bytes).
    pub fn shares_tile(&self, other: &TiledImage, index: usize) -> bool {
        self.tiles.get(index).zip(other.tiles.get(index)).is_some_and(|(a, b)| Arc::ptr_eq(a, b))
    }

    /// Number of tiles two same-sized images share.
    pub fn shared_tiles(&self, other: &TiledImage) -> usize {
        (0..self.tiles.len().min(other.tiles.len())).filter(|&i| self.shares_tile(other, i)).count()
    }

    /// Runs `filter` on every tile in parallel and returns the result. A tile
    /// the filter leaves unchanged stays shared with `self`; only changed
    /// tiles allocate.
    pub fn apply(&self, filter: &(dyn Fn(&mut [u8]) + Sync)) -> TiledImage {
        let tiles: Vec<Tile> = self
            .tiles
            .par_iter()
            .map(|t| {
                let mut scratch = t.to_vec();
                filter(&mut scratch);
                if scratch[..] == t[..] { t.clone() } else { Tile::from(scratch) }
            })
            .collect();
        TiledImage { tiles, ..self.clone() }
    }

    /// Edits one tile in place (copying it first only if something else still shares it).
    pub fn edit_tile(&mut self, index: usize, edit: impl FnOnce(&mut [u8])) {
        let mut buf = self.tiles[index].to_vec();
        edit(&mut buf);
        if buf[..] != self.tiles[index][..] {
            self.tiles[index] = Tile::from(buf);
        }
    }
}

// ----------------------------------------------------------------- filters

/// Which channels a view shows.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Channels {
    /// Colour with its alpha.
    Rgba,
    /// Colour, ignoring alpha (fully opaque).
    Rgb,
    Red,
    Green,
    Blue,
    /// Alpha shown as grey.
    Alpha,
}

/// Rewrites RGBA8 pixels so that only `channels` shows. Single channels become grey.
pub fn channel_filter(channels: Channels) -> impl Fn(&mut [u8]) + Sync {
    move |px: &mut [u8]| {
        for p in px.chunks_exact_mut(4) {
            match channels {
                Channels::Rgba => {}
                Channels::Rgb => p[3] = 255,
                Channels::Red => p.copy_from_slice(&[p[0], p[0], p[0], 255]),
                Channels::Green => p.copy_from_slice(&[p[1], p[1], p[1], 255]),
                Channels::Blue => p.copy_from_slice(&[p[2], p[2], p[2], 255]),
                Channels::Alpha => p.copy_from_slice(&[p[3], p[3], p[3], 255]),
            }
        }
    }
}

/// Inverts colour, keeping alpha.
pub fn invert_filter(px: &mut [u8]) {
    for p in px.chunks_exact_mut(4) {
        p[0] = 255 - p[0];
        p[1] = 255 - p[1];
        p[2] = 255 - p[2];
    }
}

// --------------------------------------------------------------------- GPU

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
            if self.uploaded[i].as_ref().is_some_and(|t| Arc::ptr_eq(t, &image.tiles[i])) {
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
                &image.tiles[i],
                wgpu::TexelCopyBufferLayout {
                    offset: 0,
                    bytes_per_row: Some((TILE * 4) as u32),
                    rows_per_image: Some(TILE as u32),
                },
                wgpu::Extent3d { width: w as u32, height: h as u32, depth_or_array_layers: 1 },
            );
            self.uploaded[i] = Some(image.tiles[i].clone());
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
