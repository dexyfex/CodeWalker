//! Copy-on-write tiled images and the filters that run over them.
//!
//! An image is cut into 128x128 tiles, each an `Arc`'d buffer. Cloning an
//! image or applying a filter shares every tile that did not change, so
//! several views of one texture (colour, single channels, history steps) cost
//! only the tiles that differ, and a GPU upload sends only those. Filters run
//! on all cores, one task per tile. This lives in the core crate so that the
//! window and the command line apply exactly the same code.

use std::sync::Arc;

use rayon::prelude::*;

pub const TILE: usize = 128;
const TILE_BYTES: usize = TILE * TILE * 4;

pub type Tile = Arc<[u8]>;

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

    pub fn tiles_y(&self) -> usize {
        self.tiles_y
    }

    pub fn tiles_x(&self) -> usize {
        self.tiles_x
    }

    pub fn tile_data(&self, index: usize) -> &[u8] {
        &self.tiles[index]
    }

    /// The shared buffer behind tile `index` (for caches that compare by identity).
    pub fn tile(&self, index: usize) -> &Tile {
        &self.tiles[index]
    }

    /// RGBA of one pixel; transparent black outside the image.
    pub fn pixel(&self, x: usize, y: usize) -> [u8; 4] {
        if x >= self.width || y >= self.height {
            return [0; 4];
        }
        let tile = &self.tiles[(y / TILE) * self.tiles_x + x / TILE];
        let at = ((y % TILE) * TILE + x % TILE) * 4;
        [tile[at], tile[at + 1], tile[at + 2], tile[at + 3]]
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

    /// The image as a PNG file (8-bit RGBA).
    pub fn to_png(&self) -> Vec<u8> {
        encode_png(self.width, self.height, &self.to_rgba())
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

/// Writes tightly packed RGBA8 as a PNG file.
pub fn encode_png(width: usize, height: usize, rgba: &[u8]) -> Vec<u8> {
    use std::io::Write;

    assert_eq!(rgba.len(), width * height * 4, "pixel buffer does not match the size");
    fn chunk(out: &mut Vec<u8>, kind: &[u8; 4], data: &[u8]) {
        out.extend_from_slice(&(data.len() as u32).to_be_bytes());
        out.extend_from_slice(kind);
        out.extend_from_slice(data);
        let mut crc = flate2::Crc::new();
        crc.update(kind);
        crc.update(data);
        out.extend_from_slice(&crc.sum().to_be_bytes());
    }
    let mut out = b"\x89PNG\r\n\x1a\n".to_vec();
    let mut header = Vec::with_capacity(13);
    header.extend_from_slice(&(width as u32).to_be_bytes());
    header.extend_from_slice(&(height as u32).to_be_bytes());
    header.extend_from_slice(&[8, 6, 0, 0, 0]); // 8 bits, RGBA, deflate, no filter method, no interlace
    chunk(&mut out, b"IHDR", &header);
    let mut z = flate2::write::ZlibEncoder::new(Vec::new(), flate2::Compression::fast());
    for row in rgba.chunks_exact((width * 4).max(1)) {
        z.write_all(&[0]).expect("writing to a Vec cannot fail"); // filter type: none
        z.write_all(row).expect("writing to a Vec cannot fail");
    }
    chunk(&mut out, b"IDAT", &z.finish().expect("writing to a Vec cannot fail"));
    chunk(&mut out, b"IEND", &[]);
    out
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

impl Channels {
    pub const ALL: [Channels; 6] =
        [Channels::Rgba, Channels::Rgb, Channels::Red, Channels::Green, Channels::Blue, Channels::Alpha];

    /// Short name, as used on the command line (`rgba`, `rgb`, `r`, `g`, `b`, `a`).
    pub fn label(self) -> &'static str {
        match self {
            Channels::Rgba => "rgba",
            Channels::Rgb => "rgb",
            Channels::Red => "r",
            Channels::Green => "g",
            Channels::Blue => "b",
            Channels::Alpha => "a",
        }
    }

    pub fn parse(name: &str) -> Option<Channels> {
        let n = name.trim().to_ascii_lowercase();
        Self::ALL.into_iter().find(|c| {
            c.label() == n
                || matches!(
                    (c, n.as_str()),
                    (Channels::Red, "red")
                        | (Channels::Green, "green")
                        | (Channels::Blue, "blue")
                        | (Channels::Alpha, "alpha")
                )
        })
    }
}

/// Rewrites RGBA8 pixels so that only `channels` shows. Single channels become grey.
pub fn channel_filter(channels: Channels) -> impl Fn(&mut [u8]) + Sync {
    move |px: &mut [u8]| {
        for p in px.as_chunks_mut::<4>().0 {
            match channels {
                Channels::Rgba => {}
                Channels::Rgb => p[3] = 255,
                Channels::Red => *p = [p[0], p[0], p[0], 255],
                Channels::Green => *p = [p[1], p[1], p[1], 255],
                Channels::Blue => *p = [p[2], p[2], p[2], 255],
                Channels::Alpha => *p = [p[3], p[3], p[3], 255],
            }
        }
    }
}

/// Inverts colour, keeping alpha.
pub fn invert_filter(px: &mut [u8]) {
    for p in px.as_chunks_mut::<4>().0 {
        p[0] = 255 - p[0];
        p[1] = 255 - p[1];
        p[2] = 255 - p[2];
    }
}

#[cfg(test)]
mod tests;
