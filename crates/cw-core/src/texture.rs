//! Texture formats used by GTA V, mip-chain sizing, decoding to RGBA8 and DDS
//! reading/writing.
//!
//! Mip sizes use the exact block-compression rules (as `DDSIO.ComputePitch`
//! does), not the legacy `Stride * Height` loop of `TextureData.Read`, which
//! over-reads about 4x for block-compressed formats.

use crate::error::{Error, Result};

/// D3D9-style format codes stored in the file (`Texture.cs`).
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Format {
    A8R8G8B8,
    X8R8G8B8,
    A1R5G5B5,
    A8,
    L8,
    A8B8G8R8,
    Dxt1,
    Dxt3,
    Dxt5,
    Ati1,
    Ati2,
    Bc7,
    Unknown(u32),
}

const fn fourcc(s: &[u8; 4]) -> u32 {
    u32::from_le_bytes(*s)
}

impl Format {
    pub fn from_d3d(v: u32) -> Self {
        match v {
            21 => Self::A8R8G8B8,
            22 => Self::X8R8G8B8,
            25 => Self::A1R5G5B5,
            28 => Self::A8,
            32 => Self::A8B8G8R8,
            50 => Self::L8,
            x if x == fourcc(b"DXT1") => Self::Dxt1,
            x if x == fourcc(b"DXT3") => Self::Dxt3,
            x if x == fourcc(b"DXT5") => Self::Dxt5,
            x if x == fourcc(b"ATI1") => Self::Ati1,
            x if x == fourcc(b"ATI2") => Self::Ati2,
            x if x == fourcc(b"BC7 ") => Self::Bc7,
            other => Self::Unknown(other),
        }
    }

    pub fn to_d3d(self) -> u32 {
        match self {
            Self::A8R8G8B8 => 21,
            Self::X8R8G8B8 => 22,
            Self::A1R5G5B5 => 25,
            Self::A8 => 28,
            Self::A8B8G8R8 => 32,
            Self::L8 => 50,
            Self::Dxt1 => fourcc(b"DXT1"),
            Self::Dxt3 => fourcc(b"DXT3"),
            Self::Dxt5 => fourcc(b"DXT5"),
            Self::Ati1 => fourcc(b"ATI1"),
            Self::Ati2 => fourcc(b"ATI2"),
            Self::Bc7 => fourcc(b"BC7 "),
            Self::Unknown(v) => v,
        }
    }

    pub fn label(self) -> &'static str {
        match self {
            Self::A8R8G8B8 => "A8R8G8B8",
            Self::X8R8G8B8 => "X8R8G8B8",
            Self::A1R5G5B5 => "A1R5G5B5",
            Self::A8 => "A8",
            Self::L8 => "L8",
            Self::A8B8G8R8 => "A8B8G8R8",
            Self::Dxt1 => "DXT1 (BC1)",
            Self::Dxt3 => "DXT3 (BC2)",
            Self::Dxt5 => "DXT5 (BC3)",
            Self::Ati1 => "ATI1 (BC4)",
            Self::Ati2 => "ATI2 (BC5)",
            Self::Bc7 => "BC7",
            Self::Unknown(_) => "Unknown",
        }
    }

    /// Bytes per 4x4 block for block-compressed formats.
    fn block_bytes(self) -> Option<usize> {
        match self {
            Self::Dxt1 | Self::Ati1 => Some(8),
            Self::Dxt3 | Self::Dxt5 | Self::Ati2 | Self::Bc7 => Some(16),
            _ => None,
        }
    }

    /// Bytes per pixel for uncompressed formats.
    fn pixel_bytes(self) -> Option<usize> {
        match self {
            Self::A8R8G8B8 | Self::X8R8G8B8 | Self::A8B8G8R8 => Some(4),
            Self::A1R5G5B5 => Some(2),
            Self::A8 | Self::L8 => Some(1),
            _ => None,
        }
    }

    pub fn is_known(self) -> bool {
        !matches!(self, Self::Unknown(_))
    }

    /// Size in bytes of one mip level of `w` x `h` pixels (0 for unknown formats).
    pub fn level_size(self, w: u32, h: u32) -> usize {
        let (w, h) = (w.max(1) as usize, h.max(1) as usize);
        if let Some(b) = self.block_bytes() {
            w.div_ceil(4) * h.div_ceil(4) * b
        } else if let Some(p) = self.pixel_bytes() {
            w * h * p
        } else {
            0
        }
    }
}

/// Dimensions of mip `level`.
pub fn mip_dims(w: u32, h: u32, level: u32) -> (u32, u32) {
    ((w >> level.min(31)).max(1), (h >> level.min(31)).max(1))
}

/// Total bytes of a mip chain stored back to back, largest first.
pub fn chain_size(format: Format, w: u32, h: u32, levels: u32) -> usize {
    (0..levels.max(1))
        .map(|l| {
            let (mw, mh) = mip_dims(w, h, l);
            format.level_size(mw, mh)
        })
        .sum()
}

/// Byte offset of mip `level` inside the chain, and its size.
pub fn mip_range(format: Format, w: u32, h: u32, level: u32) -> (usize, usize) {
    let offset: usize = (0..level)
        .map(|l| {
            let (mw, mh) = mip_dims(w, h, l);
            format.level_size(mw, mh)
        })
        .sum();
    let (mw, mh) = mip_dims(w, h, level);
    (offset, format.level_size(mw, mh))
}

/// A texture with its whole mip chain.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Texture {
    pub width: u32,
    pub height: u32,
    pub levels: u32,
    pub format: Format,
    pub data: Vec<u8>,
}

/// Decoded pixels, always RGBA8.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Rgba {
    pub width: u32,
    pub height: u32,
    pub pixels: Vec<u8>,
}

/// Refuse to allocate more than this many pixels when decoding (a hostile
/// header could otherwise ask for gigabytes).
const MAX_PIXELS: u64 = 8192 * 8192;

impl Texture {
    /// Decodes mip `level` to RGBA8. Data missing from the chain decodes as zeros.
    pub fn decode(&self, level: u32) -> Result<Rgba> {
        if level >= self.levels.max(1) {
            return Err(Error::InvalidRpf(format!("texture has no mip level {level}")));
        }
        let (w, h) = mip_dims(self.width, self.height, level);
        let (off, len) = mip_range(self.format, self.width, self.height, level);
        let end = (off + len).min(self.data.len());
        let slice = self.data.get(off..end).unwrap_or(&[]);
        decode_rgba(self.format, w, h, slice)
    }
}

pub fn decode_rgba(format: Format, w: u32, h: u32, data: &[u8]) -> Result<Rgba> {
    if w == 0 || h == 0 || (w as u64) * (h as u64) > MAX_PIXELS {
        return Err(Error::InvalidRpf(format!("unsupported texture size {w}x{h}")));
    }
    if !format.is_known() {
        return Err(Error::InvalidRpf(format!("unsupported texture format 0x{:08X}", format.to_d3d())));
    }
    let (wu, hu) = (w as usize, h as usize);
    let mut out = vec![0u8; wu * hu * 4];

    if let Some(block) = format.block_bytes() {
        let blocks_x = wu.div_ceil(4);
        let blocks_y = hu.div_ceil(4);
        let mut raw = vec![0u8; block];
        for by in 0..blocks_y {
            for bx in 0..blocks_x {
                let start = (by * blocks_x + bx) * block;
                raw.fill(0);
                if start < data.len() {
                    let n = block.min(data.len() - start);
                    raw[..n].copy_from_slice(&data[start..start + n]);
                }
                let px = decode_block(format, &raw);
                for y in 0..4 {
                    for x in 0..4 {
                        let (ox, oy) = (bx * 4 + x, by * 4 + y);
                        if ox < wu && oy < hu {
                            let d = (oy * wu + ox) * 4;
                            out[d..d + 4].copy_from_slice(&px[(y * 4 + x) * 4..(y * 4 + x) * 4 + 4]);
                        }
                    }
                }
            }
        }
    } else {
        let bpp = format.pixel_bytes().expect("known uncompressed format");
        for (i, dst) in out.as_chunks_mut::<4>().0.iter_mut().enumerate() {
            let at = i * bpp;
            let src = |k: usize| data.get(at + k).copied().unwrap_or(0);
            let rgba = match format {
                Format::A8R8G8B8 => [src(2), src(1), src(0), src(3)],
                Format::X8R8G8B8 => [src(2), src(1), src(0), 255],
                Format::A8B8G8R8 => [src(0), src(1), src(2), src(3)],
                Format::A1R5G5B5 => {
                    let v = u16::from_le_bytes([src(0), src(1)]);
                    let e5 = |x: u16| ((x & 0x1F) * 255 / 31) as u8;
                    [e5(v >> 10), e5(v >> 5), e5(v), if v & 0x8000 != 0 { 255 } else { 0 }]
                }
                Format::A8 => [src(0), src(0), src(0), 255],
                Format::L8 => [src(0), src(0), src(0), 255],
                _ => unreachable!(),
            };
            dst.copy_from_slice(&rgba);
        }
    }
    Ok(Rgba { width: w, height: h, pixels: out })
}

/// One 4x4 block to 16 RGBA pixels.
fn decode_block(format: Format, raw: &[u8]) -> [u8; 64] {
    let mut px = [0u8; 64];
    match format {
        Format::Dxt1 => bcdec_rs::bc1(raw, &mut px, 16),
        Format::Dxt3 => bcdec_rs::bc2(raw, &mut px, 16),
        Format::Dxt5 => bcdec_rs::bc3(raw, &mut px, 16),
        Format::Bc7 => bcdec_rs::bc7(raw, &mut px, 16),
        Format::Ati1 => {
            // single channel: show as grey
            let mut r = [0u8; 16];
            bcdec_rs::bc4(raw, &mut r, 4, false);
            for (i, v) in r.iter().enumerate() {
                px[i * 4..i * 4 + 4].copy_from_slice(&[*v, *v, *v, 255]);
            }
        }
        Format::Ati2 => {
            // two channels: a tangent-space normal map; rebuild Z for display
            let mut rg = [0u8; 32];
            bcdec_rs::bc5(raw, &mut rg, 8, false);
            for i in 0..16 {
                let (x, y) = (rg[i * 2] as f32 / 255.0 * 2.0 - 1.0, rg[i * 2 + 1] as f32 / 255.0 * 2.0 - 1.0);
                let z = (1.0 - x * x - y * y).max(0.0).sqrt();
                px[i * 4..i * 4 + 4].copy_from_slice(&[
                    rg[i * 2],
                    rg[i * 2 + 1],
                    ((z * 0.5 + 0.5) * 255.0) as u8,
                    255,
                ]);
            }
        }
        _ => {}
    }
    px
}

// ------------------------------------------------------------------- DDS

const DDS_MAGIC: u32 = 0x2053_4444; // "DDS "
const DDSD_CAPS: u32 = 0x1;
const DDSD_HEIGHT: u32 = 0x2;
const DDSD_WIDTH: u32 = 0x4;
const DDSD_PIXELFORMAT: u32 = 0x1000;
const DDSD_MIPMAPCOUNT: u32 = 0x2_0000;
const DDSD_LINEARSIZE: u32 = 0x8_0000;
const DDPF_ALPHAPIXELS: u32 = 0x1;
const DDPF_ALPHA: u32 = 0x2;
const DDPF_FOURCC: u32 = 0x4;
const DDPF_RGB: u32 = 0x40;
const DDPF_LUMINANCE: u32 = 0x2_0000;
const DXGI_BC7_UNORM: u32 = 98;

fn le32(b: &[u8], o: usize) -> u32 {
    u32::from_le_bytes([b[o], b[o + 1], b[o + 2], b[o + 3]])
}

/// Reads a DDS file into a [`Texture`]. Supports the formats GTA uses; others are an error.
pub fn parse_dds(bytes: &[u8]) -> Result<Texture> {
    let bad = |m: &str| Error::InvalidRpf(format!("DDS: {m}"));
    if bytes.len() < 128 || le32(bytes, 0) != DDS_MAGIC || le32(bytes, 4) != 124 {
        return Err(bad("not a DDS file"));
    }
    let (height, width) = (le32(bytes, 12), le32(bytes, 16));
    let levels = le32(bytes, 28).clamp(1, 16);
    if width == 0 || height == 0 || width > 16384 || height > 16384 {
        return Err(bad("unsupported size"));
    }
    let (pf_flags, four, bits) = (le32(bytes, 80), le32(bytes, 84), le32(bytes, 88));
    let (rm, gm, bm, am) = (le32(bytes, 92), le32(bytes, 96), le32(bytes, 100), le32(bytes, 104));
    let mut header = 128;

    let format = if pf_flags & DDPF_FOURCC != 0 {
        if four == fourcc(b"DX10") {
            if bytes.len() < 148 {
                return Err(bad("truncated DX10 header"));
            }
            header = 148;
            match le32(bytes, 128) {
                DXGI_BC7_UNORM => Format::Bc7,
                other => return Err(bad(&format!("unsupported DXGI format {other}"))),
            }
        } else {
            match Format::from_d3d(four) {
                f @ (Format::Dxt1 | Format::Dxt3 | Format::Dxt5 | Format::Ati1 | Format::Ati2) => f,
                _ => match four {
                    x if x == fourcc(b"BC4U") => Format::Ati1,
                    x if x == fourcc(b"BC5U") => Format::Ati2,
                    _ => return Err(bad(&format!("unsupported FourCC 0x{four:08X}"))),
                },
            }
        }
    } else if pf_flags & DDPF_LUMINANCE != 0 && bits == 8 {
        Format::L8
    } else if pf_flags & DDPF_ALPHA != 0 && bits == 8 {
        Format::A8
    } else if pf_flags & (DDPF_RGB | DDPF_ALPHAPIXELS) != 0 {
        match (bits, rm, gm, bm, am) {
            (32, 0x00FF_0000, 0x0000_FF00, 0x0000_00FF, 0xFF00_0000) => Format::A8R8G8B8,
            (32, 0x00FF_0000, 0x0000_FF00, 0x0000_00FF, 0) => Format::X8R8G8B8,
            (32, 0x0000_00FF, 0x0000_FF00, 0x00FF_0000, 0xFF00_0000) => Format::A8B8G8R8,
            (16, 0x7C00, 0x03E0, 0x001F, 0x8000) => Format::A1R5G5B5,
            _ => return Err(bad("unsupported pixel layout")),
        }
    } else {
        return Err(bad("unsupported pixel format"));
    };

    let want = chain_size(format, width, height, levels);
    let data = bytes[header..].get(..want.min(bytes.len() - header)).unwrap_or(&[]).to_vec();
    Ok(Texture { width, height, levels, format, data })
}

/// Writes a [`Texture`] as a DDS file other tools can open.
pub fn write_dds(t: &Texture) -> Result<Vec<u8>> {
    let mut h = vec![0u8; 128];
    let put = |h: &mut Vec<u8>, o: usize, v: u32| h[o..o + 4].copy_from_slice(&v.to_le_bytes());
    put(&mut h, 0, DDS_MAGIC);
    put(&mut h, 4, 124);
    let compressed = t.format.block_bytes().is_some();
    let mut flags = DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT;
    if t.levels > 1 {
        flags |= DDSD_MIPMAPCOUNT;
    }
    if compressed {
        flags |= DDSD_LINEARSIZE;
    }
    put(&mut h, 8, flags);
    put(&mut h, 12, t.height);
    put(&mut h, 16, t.width);
    let top = t.format.level_size(t.width, t.height) as u32;
    put(
        &mut h,
        20,
        if compressed { top } else { (t.width as usize * t.format.pixel_bytes().unwrap_or(4)) as u32 },
    );
    put(&mut h, 28, t.levels.max(1));
    put(&mut h, 76, 32);
    let mut dx10 = false;
    match t.format {
        Format::Bc7 => {
            put(&mut h, 80, DDPF_FOURCC);
            put(&mut h, 84, fourcc(b"DX10"));
            dx10 = true;
        }
        f if compressed => {
            put(&mut h, 80, DDPF_FOURCC);
            put(&mut h, 84, f.to_d3d());
        }
        Format::A8R8G8B8 => {
            rgb(&mut h, DDPF_RGB | DDPF_ALPHAPIXELS, 32, [0x00FF_0000, 0xFF00, 0xFF, 0xFF00_0000])
        }
        Format::X8R8G8B8 => rgb(&mut h, DDPF_RGB, 32, [0x00FF_0000, 0xFF00, 0xFF, 0]),
        Format::A8B8G8R8 => {
            rgb(&mut h, DDPF_RGB | DDPF_ALPHAPIXELS, 32, [0xFF, 0xFF00, 0x00FF_0000, 0xFF00_0000])
        }
        Format::A1R5G5B5 => rgb(&mut h, DDPF_RGB | DDPF_ALPHAPIXELS, 16, [0x7C00, 0x03E0, 0x001F, 0x8000]),
        Format::L8 => rgb(&mut h, DDPF_LUMINANCE, 8, [0xFF, 0, 0, 0]),
        Format::A8 => rgb(&mut h, DDPF_ALPHA, 8, [0, 0, 0, 0xFF]),
        Format::Unknown(v) => {
            return Err(Error::InvalidRpf(format!("cannot write DDS for format 0x{v:08X}")));
        }
        _ => unreachable!(),
    }
    put(&mut h, 108, 0x1000 | if t.levels > 1 { 0x40_0008 } else { 0 }); // TEXTURE (+ COMPLEX|MIPMAP)
    if dx10 {
        let mut ext = [0u8; 20];
        ext[..4].copy_from_slice(&DXGI_BC7_UNORM.to_le_bytes());
        ext[4..8].copy_from_slice(&3u32.to_le_bytes()); // texture2D
        ext[12..16].copy_from_slice(&1u32.to_le_bytes()); // array size
        h.extend_from_slice(&ext);
    }
    h.extend_from_slice(&t.data);
    return Ok(h);

    fn rgb(h: &mut [u8], flags: u32, bits: u32, m: [u32; 4]) {
        for (o, v) in [(80, flags), (88, bits), (92, m[0]), (96, m[1]), (100, m[2]), (104, m[3])] {
            h[o..o + 4].copy_from_slice(&v.to_le_bytes());
        }
    }
}

#[cfg(test)]
mod tests;
