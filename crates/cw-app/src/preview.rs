//! Loading a file for preview (off the UI thread) and the views that show it.

use std::{ops::Range, sync::Arc};

use cw_core::{
    EntryKind, EntryRef, RpfTree,
    filekind::FileKind,
    resource,
    rpf::{ResourceData, page_flags_size},
    texture::{self, Texture},
    ytd,
};
use eframe::egui::{
    self, Align2, Color32, FontId, Rect, ScrollArea, Sense, Stroke, TextureHandle, TextureOptions, Ui, pos2,
    vec2,
};

use crate::{format, theme};

/// Files larger than this are not previewed (Extract still works): loading one means holding
/// the whole decompressed file in memory, for the hex view as much as for anything else.
const MAX_PREVIEW_BYTES: u64 = 256 * 1024 * 1024;
/// Text shown beyond this many bytes is cut off, to stay responsive.
const MAX_TEXT_BYTES: usize = 32 * 1024 * 1024;
/// Textures wider than this are downscaled before upload.
const MAX_IMAGE_SIDE: u32 = 4096;
const MAX_LINE_CHARS: usize = 4000;

#[derive(Debug, Clone)]
pub struct ResInfo {
    pub version: u32,
    pub sys_size: u32,
    pub gfx_size: u32,
}

pub struct TextDoc {
    pub text: Arc<str>,
    pub lines: Vec<Range<u32>>,
    pub widest: usize,
    pub truncated: bool,
}

pub struct Decoded {
    pub width: usize,
    pub height: usize,
    pub rgba: Vec<u8>,
    pub original: (u32, u32),
}

/// One texture of a dictionary (or the only one of a DDS file), with a small picture for lists.
pub struct TexEntry {
    pub name: String,
    pub texture: Arc<Texture>,
    pub thumb: Option<Decoded>,
}

/// The textures of a `.ytd` or `.dds`, parsed. The full pictures are decoded on demand.
pub struct TexSet {
    pub entries: Vec<TexEntry>,
}

/// Longest side of the list pictures, in pixels.
const THUMB_SIDE: u32 = 96;

/// The smallest mip that is still at least [`THUMB_SIDE`] wide or tall (or the last one).
fn thumbnail(t: &Texture) -> Option<Decoded> {
    let level = (0..t.levels.max(1))
        .find(|&l| {
            let (w, h) = texture::mip_dims(t.width, t.height, l);
            w.max(h) <= THUMB_SIDE * 2
        })
        .unwrap_or(t.levels.max(1) - 1);
    let px = t.decode(level).ok()?;
    Some(Decoded {
        width: px.width as usize,
        height: px.height as usize,
        rgba: px.pixels,
        original: (t.width, t.height),
    })
}

fn tex_entry(name: String, texture: Texture) -> TexEntry {
    let thumb = thumbnail(&texture);
    TexEntry { name, texture: Arc::new(texture), thumb }
}

/// Reads a dictionary (resource) or a DDS file. Not being able to is reported, not fatal: the
/// hex and info views still work.
fn read_textures(
    kind: FileKind,
    name: &str,
    resource: Option<&ResourceData>,
    bytes: &[u8],
) -> Result<Option<TexSet>, String> {
    match (kind, resource) {
        (FileKind::TextureDict, Some(rd)) => {
            let list = ytd::parse_ytd(rd).map_err(|e| e.to_string())?;
            Ok(Some(TexSet { entries: list.into_iter().map(|t| tex_entry(t.name, t.texture)).collect() }))
        }
        (FileKind::Dds, _) => {
            let t = texture::parse_dds(bytes).map_err(|e| e.to_string())?;
            let stem = name.rsplit_once('.').map_or(name, |(s, _)| s).to_string();
            Ok(Some(TexSet { entries: vec![tex_entry(stem, t)] }))
        }
        _ => Ok(None),
    }
}

/// Everything the preview needs; built on a worker thread.
pub struct Loaded {
    pub name: String,
    pub path: String,
    pub kind: FileKind,
    pub bytes: Arc<Vec<u8>>,
    pub stored_size: Option<u64>,
    pub encrypted: bool,
    pub container: Option<String>,
    pub resource: Option<ResInfo>,
    pub text: Option<TextDoc>,
    pub image: Option<Decoded>,
    pub textures: Option<TexSet>,
    /// Why a texture file could not be read as textures, when it could not.
    pub texture_error: Option<String>,
}

pub enum Source {
    Entry(EntryRef),
    Disk(std::path::PathBuf),
}

pub fn load(tree: Option<&RpfTree>, src: &Source) -> Result<Loaded, String> {
    match src {
        Source::Entry(r) => {
            let t = tree.ok_or("no archives are open")?;
            let e = t.entry(*r);
            if e.is_dir() {
                return Err("this is a folder".into());
            }
            too_large(e.display_size())?;
            let (bytes, resource, rd) = match e.kind {
                EntryKind::Resource { .. } => {
                    let rd = t.extract_resource(*r).map_err(|e| e.to_string())?;
                    let info = res_info(rd.sys_flags, rd.gfx_flags);
                    (rd.data.clone(), Some(info), Some(rd))
                }
                _ => (t.extract(*r).map_err(|e| e.to_string())?, None, None),
            };
            let encrypted = matches!(
                e.kind,
                EntryKind::Binary { encrypted: true, .. } | EntryKind::Resource { encrypted: true, .. }
            );
            let kind = FileKind::from_name(&e.name_lower);
            Ok(build(Parts {
                name: e.name.clone(),
                path: e.path.clone(),
                kind,
                bytes,
                stored_size: Some(e.stored_size()),
                encrypted,
                container: Some(t.archive(r.archive).name.clone()),
                resource,
                resource_data: rd,
            }))
        }
        Source::Disk(p) => {
            too_large(std::fs::metadata(p).map_err(|e| e.to_string())?.len())?;
            let raw = std::fs::read(p).map_err(|e| e.to_string())?;
            let name = p.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default();
            let kind = FileKind::from_name(&name.to_lowercase());
            let stored = raw.len() as u64;
            let (bytes, resource, rd) = if kind.is_resource() && resource::has_rsc7_header(&raw) {
                let rd = resource::parse_loose(&raw).map_err(|e| e.to_string())?;
                let info = res_info(rd.sys_flags, rd.gfx_flags);
                (rd.data.clone(), Some(info), Some(rd))
            } else {
                (raw, None, None)
            };
            Ok(build(Parts {
                name,
                path: p.display().to_string(),
                kind,
                bytes,
                stored_size: Some(stored),
                encrypted: false,
                container: None,
                resource,
                resource_data: rd,
            }))
        }
    }
}

fn too_large(size: u64) -> Result<(), String> {
    if size > MAX_PREVIEW_BYTES {
        return Err(format!(
            "This file is {}, too large to preview ({} at most). Extract it to open it elsewhere.",
            format::size(size),
            format::size(MAX_PREVIEW_BYTES)
        ));
    }
    Ok(())
}

fn res_info(sys_flags: u32, gfx_flags: u32) -> ResInfo {
    ResInfo {
        version: cw_core::rpf::resource_version(sys_flags, gfx_flags),
        sys_size: page_flags_size(sys_flags),
        gfx_size: page_flags_size(gfx_flags),
    }
}

/// What `build` turns into a [`Loaded`].
struct Parts {
    name: String,
    path: String,
    kind: FileKind,
    bytes: Vec<u8>,
    stored_size: Option<u64>,
    encrypted: bool,
    container: Option<String>,
    resource: Option<ResInfo>,
    /// The resource as read (flags and payload), for formats that parse it.
    resource_data: Option<ResourceData>,
}

fn build(p: Parts) -> Loaded {
    let Parts { name, path, kind, bytes, stored_size, encrypted, container, resource, resource_data } = p;
    let image = (kind == FileKind::Image).then(|| decode_image(&bytes)).flatten();
    let (textures, texture_error) = match read_textures(kind, &name, resource_data.as_ref(), &bytes) {
        Ok(t) => (t, None),
        Err(e) => (None, Some(e)),
    };
    let text = if image.is_none() && resource.is_none() && textures.is_none() {
        decode_text(&bytes, kind)
    } else {
        None
    };
    Loaded {
        name,
        path,
        kind,
        bytes: Arc::new(bytes),
        stored_size,
        encrypted,
        container,
        resource,
        text,
        image,
        textures,
        texture_error,
    }
}

fn decode_image(bytes: &[u8]) -> Option<Decoded> {
    let img = image::load_from_memory(bytes).ok()?;
    let original = (img.width(), img.height());
    let img = if original.0.max(original.1) > MAX_IMAGE_SIDE {
        img.thumbnail(MAX_IMAGE_SIDE, MAX_IMAGE_SIDE)
    } else {
        img
    };
    let rgba = img.to_rgba8();
    Some(Decoded {
        width: rgba.width() as usize,
        height: rgba.height() as usize,
        rgba: rgba.into_raw(),
        original,
    })
}

/// Text if the content decodes as UTF-8/UTF-16 and has no control bytes.
pub fn decode_text(bytes: &[u8], kind: FileKind) -> Option<TextDoc> {
    let looks_textual =
        matches!(kind, FileKind::Xml | FileKind::Text | FileKind::Meta | FileKind::Gxt2 | FileKind::Unknown);
    if bytes.is_empty() || !looks_textual {
        return None;
    }
    let head = &bytes[..bytes.len().min(8192)];
    let (text, truncated) = if let Some(rest) = bytes.strip_prefix(&[0xFF, 0xFE]) {
        decode_utf16(rest, false)
    } else if let Some(rest) = bytes.strip_prefix(&[0xFE, 0xFF]) {
        decode_utf16(rest, true)
    } else {
        let body = bytes.strip_prefix(&[0xEF, 0xBB, 0xBF]).unwrap_or(bytes);
        let suspicious = head.iter().filter(|&&b| b < 0x09 || (b > 0x0D && b < 0x20)).count();
        if suspicious > 0 {
            return None;
        }
        let cut = body.len().min(MAX_TEXT_BYTES);
        // do not split a multibyte character at the cut
        let mut end = cut;
        while end > 0 && end < body.len() && (body[end] & 0xC0) == 0x80 {
            end -= 1;
        }
        (std::str::from_utf8(&body[..end]).ok().map(str::to_owned), end < body.len())
    };
    let text = text?;
    let mut lines = Vec::new();
    let mut start = 0usize;
    let mut widest = 0usize;
    for (i, b) in text.bytes().enumerate() {
        if b == b'\n' {
            let end = if i > start && text.as_bytes()[i - 1] == b'\r' { i - 1 } else { i };
            widest = widest.max(text[start..end].chars().count());
            lines.push(start as u32..end as u32);
            start = i + 1;
        }
    }
    if start <= text.len() {
        widest = widest.max(text[start..].chars().count());
        lines.push(start as u32..text.len() as u32);
    }
    Some(TextDoc { text: text.into(), lines, widest: widest.min(MAX_LINE_CHARS), truncated })
}

/// Decodes at most [`MAX_TEXT_BYTES`] of UTF-16; the flag says whether the rest was left out.
fn decode_utf16(bytes: &[u8], big_endian: bool) -> (Option<String>, bool) {
    // two bytes per unit, and a cut must not fall between the halves of a surrogate pair
    let mut cut = bytes.len().min(MAX_TEXT_BYTES) & !1;
    let unit = |i: usize| {
        let c = [bytes[i], bytes[i + 1]];
        if big_endian { u16::from_be_bytes(c) } else { u16::from_le_bytes(c) }
    };
    if cut < bytes.len() && cut >= 2 && (0xD800..0xDC00).contains(&unit(cut - 2)) {
        cut -= 2;
    }
    let units: Vec<u16> = (0..cut).step_by(2).map(unit).collect();
    (String::from_utf16(&units).ok(), cut < (bytes.len() & !1))
}

// ------------------------------------------------------------------- hex

/// `(offset, hex bytes, ascii)` of one dump row.
pub fn hex_row(offset: usize, chunk: &[u8], per_row: usize) -> (String, String, String) {
    let mut hex = String::with_capacity(per_row * 3 + 1);
    for i in 0..per_row {
        if i == per_row / 2 {
            hex.push(' ');
        }
        match chunk.get(i) {
            Some(b) => hex.push_str(&format!("{b:02X} ")),
            None => hex.push_str("   "),
        }
    }
    let ascii = chunk.iter().map(|&b| if (0x20..0x7F).contains(&b) { b as char } else { '.' }).collect();
    (format!("{offset:08X}"), hex, ascii)
}

pub fn hex_view(ui: &mut Ui, bytes: &[u8], id: &str) {
    ui.spacing_mut().item_spacing.y = 0.0;
    let mono = FontId::monospace(12.5);
    let char_w = ui.fonts_mut(|f| f.glyph_width(&mono, '0'));
    // 16 bytes per row needs ~77 columns; below that fall back to 8
    let per_row = if ui.available_width() >= char_w * 80.0 { 16 } else { 8 };
    let rows = bytes.len().div_ceil(per_row).max(1);
    ScrollArea::vertical().id_salt(id).auto_shrink([false, false]).show_rows(
        ui,
        theme::HEX_ROW_H,
        rows,
        |ui, range| {
            for row in range {
                let start = row * per_row;
                let chunk = &bytes[start.min(bytes.len())..(start + per_row).min(bytes.len())];
                let (off, hex, ascii) = hex_row(start, chunk, per_row);
                let (rect, _) =
                    ui.allocate_exact_size(vec2(ui.available_width(), theme::HEX_ROW_H), Sense::hover());
                let p = ui.painter_at(rect);
                let y = rect.center().y;
                let mut x = rect.left() + 4.0;
                p.text(pos2(x, y), Align2::LEFT_CENTER, off, mono.clone(), theme::FAINT);
                x += char_w * 10.0;
                p.text(pos2(x, y), Align2::LEFT_CENTER, hex, mono.clone(), theme::TEXT);
                x += char_w * (per_row * 3 + 3) as f32;
                p.text(pos2(x, y), Align2::LEFT_CENTER, ascii, mono.clone(), theme::MUTED);
            }
        },
    );
}

// ------------------------------------------------------------------ text

pub fn text_view(ui: &mut Ui, doc: &TextDoc, id: &str) {
    ui.spacing_mut().item_spacing.y = 0.0;
    let mono = FontId::monospace(12.5);
    let char_w = ui.fonts_mut(|f| f.glyph_width(&mono, '0'));
    let gutter = char_w * (doc.lines.len().to_string().len() as f32 + 2.0);
    let content_w = gutter + char_w * doc.widest as f32 + 16.0;
    if doc.truncated {
        ui.label(
            egui::RichText::new(format!("Showing the first {}", format::size(MAX_TEXT_BYTES as u64)))
                .color(theme::WARNING)
                .size(12.0),
        );
    }
    ScrollArea::both().id_salt(id).auto_shrink([false, false]).show_rows(
        ui,
        theme::HEX_ROW_H,
        doc.lines.len(),
        |ui, range| {
            ui.set_min_width(content_w);
            for i in range {
                let (rect, _) = ui.allocate_exact_size(
                    vec2(content_w.max(ui.available_width()), theme::HEX_ROW_H),
                    Sense::hover(),
                );
                let p = ui.painter_at(rect);
                let y = rect.center().y;
                p.text(
                    pos2(rect.left() + gutter - char_w, y),
                    Align2::RIGHT_CENTER,
                    i + 1,
                    mono.clone(),
                    theme::FAINT,
                );
                let r = &doc.lines[i];
                let line = &doc.text[r.start as usize..r.end as usize];
                let line: String = line.chars().take(MAX_LINE_CHARS).collect();
                p.text(
                    pos2(rect.left() + gutter + char_w, y),
                    Align2::LEFT_CENTER,
                    line.replace('\t', "    "),
                    mono.clone(),
                    theme::TEXT,
                );
            }
        },
    );
}

// ----------------------------------------------------------------- image

pub fn upload(ctx: &egui::Context, name: &str, d: &Decoded) -> TextureHandle {
    let img = egui::ColorImage::from_rgba_unmultiplied([d.width, d.height], &d.rgba);
    ctx.load_texture(name, img, TextureOptions::LINEAR)
}

pub fn image_view(ui: &mut Ui, tex: &TextureHandle, original: (u32, u32)) {
    let avail = ui.available_size();
    let (w, h) = (tex.size()[0] as f32, tex.size()[1] as f32);
    let scale = (avail.x / w).min((avail.y - 24.0) / h).clamp(0.01, 1.0);
    let size = vec2(w * scale, h * scale);
    let (rect, _) = ui.allocate_exact_size(vec2(avail.x, size.y), Sense::hover());
    let img_rect = Rect::from_center_size(rect.center(), size);
    let p = ui.painter_at(rect);
    p.rect_filled(img_rect, 0.0, theme::GROUND);
    p.image(tex.id(), img_rect, Rect::from_min_max(pos2(0.0, 0.0), pos2(1.0, 1.0)), Color32::WHITE);
    p.rect_stroke(img_rect, 0.0, Stroke::new(1.0, theme::BORDER), egui::StrokeKind::Outside);
    ui.add_space(4.0);
    ui.label(
        egui::RichText::new(format!("{} × {} px", original.0, original.1)).color(theme::FAINT).size(12.0),
    );
}

// ------------------------------------------------------------------ info

pub fn info_rows(l: &Loaded) -> Vec<(&'static str, String)> {
    let mut rows = vec![("Type", l.kind.label().to_string()), ("Size", format::size(l.bytes.len() as u64))];
    if let Some(s) = l.stored_size {
        let ratio =
            if l.bytes.is_empty() || s == 0 { None } else { Some(s as f64 / l.bytes.len() as f64 * 100.0) };
        rows.push((
            "Stored",
            match ratio {
                Some(r) if (r - 100.0).abs() > 0.5 => format!("{} ({:.0}%)", format::size(s), r),
                _ => format::size(s),
            },
        ));
    }
    if let Some(c) = &l.container {
        rows.push(("Archive", c.clone()));
    }
    if l.encrypted {
        rows.push(("Encrypted", "Yes".into()));
    }
    if let Some(set) = &l.textures {
        rows.push(("Textures", format::plural(set.entries.len() as u64, "texture", "textures")));
    }
    if let Some(e) = &l.texture_error {
        rows.push(("Textures", format!("Could not be read: {e}")));
    }
    if let Some(r) = &l.resource {
        rows.push(("Resource version", r.version.to_string()));
        rows.push(("System pages", format::size(r.sys_size as u64)));
        rows.push(("Graphics pages", format::size(r.gfx_size as u64)));
    }
    rows
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn hex_rows_are_aligned_and_ascii_is_safe() {
        let (off, hex, ascii) = hex_row(0x10, b"AB\x00\xFFz", 8);
        assert_eq!(off, "00000010");
        // four bytes, the mid-row gap, then 7A and three blank cells
        assert_eq!(hex, format!("41 42 00 FF  7A {}", " ".repeat(9)));
        assert_eq!(ascii, "AB..z");
        let (_, full, _) = hex_row(0, &[0u8; 16], 16);
        assert_eq!(full.len(), 16 * 3 + 1, "16 bytes, 3 chars each, plus the mid gap");
    }

    #[test]
    fn utf8_text_is_indexed_by_line() {
        let d = decode_text(b"one\r\ntwo\nthree", FileKind::Text).unwrap();
        let lines: Vec<&str> = d.lines.iter().map(|r| &d.text[r.start as usize..r.end as usize]).collect();
        assert_eq!(lines, ["one", "two", "three"]);
        assert_eq!(d.widest, 5);
        assert!(!d.truncated);
    }

    #[test]
    fn bom_and_utf16_are_decoded() {
        let d = decode_text(b"\xEF\xBB\xBF<a/>", FileKind::Xml).unwrap();
        assert_eq!(&*d.text, "<a/>");
        let mut u16le = vec![0xFF, 0xFE];
        for u in "hi\n".encode_utf16() {
            u16le.extend_from_slice(&u.to_le_bytes());
        }
        assert_eq!(decode_text(&u16le, FileKind::Text).unwrap().lines.len(), 2);
    }

    #[test]
    fn utf16_is_capped_and_never_cut_inside_a_surrogate_pair() {
        // 'a' everywhere, except a pair that straddles the cap
        let units = MAX_TEXT_BYTES / 2 + 4;
        let mut bytes = vec![0xFF, 0xFE];
        for i in 0..units {
            let u: u16 = match i {
                i if i == MAX_TEXT_BYTES / 2 - 1 => 0xD83D,
                i if i == MAX_TEXT_BYTES / 2 => 0xDE00, // together: U+1F600
                _ => b'a' as u16,
            };
            bytes.extend_from_slice(&u.to_le_bytes());
        }
        let d = decode_text(&bytes, FileKind::Text).expect("valid up to the cut");
        assert!(d.truncated);
        assert!(d.text.len() <= MAX_TEXT_BYTES, "{} bytes of text", d.text.len());
        assert!(d.text.chars().all(|c| c == 'a'), "the split pair was left out, not mangled");

        let small = decode_text(&[0xFF, 0xFE, b'h', 0, b'i', 0], FileKind::Text).unwrap();
        assert!(!small.truncated);
        assert_eq!(&*small.text, "hi");
    }

    #[test]
    fn huge_files_are_refused_before_anything_is_read() {
        let file = std::env::temp_dir().join(format!("cw-huge-{}.bin", std::process::id()));
        std::fs::File::create(&file).unwrap().set_len(MAX_PREVIEW_BYTES + 1).unwrap(); // sparse
        let err = load(None, &Source::Disk(file.clone())).err().expect("too large");
        assert!(err.contains("too large to preview"), "{err}");
        let _ = std::fs::remove_file(file);
        assert!(too_large(MAX_PREVIEW_BYTES).is_ok());
    }

    #[test]
    fn binary_and_invalid_content_is_not_text() {
        assert!(decode_text(b"abc\x00def", FileKind::Unknown).is_none(), "NUL means binary");
        assert!(decode_text(&[0xC3, 0x28], FileKind::Text).is_none(), "invalid UTF-8");
        assert!(decode_text(b"", FileKind::Text).is_none());
        assert!(decode_text(b"text", FileKind::Drawable).is_none(), "model files are never shown as text");
    }

    #[test]
    fn png_is_decoded_and_garbage_is_not() {
        let mut png = Vec::new();
        image::DynamicImage::new_rgba8(3, 2)
            .write_to(&mut std::io::Cursor::new(&mut png), image::ImageFormat::Png)
            .unwrap();
        let d = decode_image(&png).unwrap();
        assert_eq!((d.width, d.height, d.original), (3, 2, (3, 2)));
        assert_eq!(d.rgba.len(), 3 * 2 * 4);
        assert!(decode_image(b"not an image").is_none());
    }

    #[test]
    fn loading_from_disk_unwraps_rsc7_resources() {
        use cw_core::rpf::build::{Item, RpfBuilder};
        let dir = std::env::temp_dir().join(format!("cw-prev-{}", std::process::id()));
        std::fs::create_dir_all(&dir).unwrap();
        let rpf = RpfBuilder::new(vec![Item::resource("a.ytd", &b"TXD-DATA"[..], 1 << 17, 0x9000_0000)])
            .build()
            .unwrap();
        let tree =
            RpfTree::scan_blob(cw_core::Blob::from_vec(rpf), "t.rpf".into(), "t.rpf".into(), "t.rpf", None)
                .unwrap();
        let exported = tree.export(tree.find("t.rpf/a.ytd").unwrap()).unwrap();
        let file = dir.join("a.ytd");
        std::fs::write(&file, exported).unwrap();

        let l = load(None, &Source::Disk(file)).unwrap();
        assert_eq!(&**l.bytes, b"TXD-DATA");
        assert_eq!(l.resource.as_ref().unwrap().version, 9);
        assert_eq!(l.kind, FileKind::TextureDict);
        assert!(l.text.is_none(), "resources are not shown as text");

        let via_tree = load(Some(&tree), &Source::Entry(tree.find("t.rpf/a.ytd").unwrap())).unwrap();
        assert_eq!(via_tree.bytes, l.bytes);
        assert!(
            load(Some(&tree), &Source::Entry(tree.find("t.rpf").unwrap())).is_err(),
            "folders have no preview"
        );
        let _ = std::fs::remove_dir_all(dir);
    }

    fn gradient_texture(w: u32, h: u32, levels: u32) -> Texture {
        let n = texture::chain_size(texture::Format::A8R8G8B8, w, h, levels);
        Texture {
            width: w,
            height: h,
            levels,
            format: texture::Format::A8R8G8B8,
            data: (0..n).map(|i| (i % 251) as u8).collect(),
        }
    }

    #[test]
    fn texture_dictionaries_and_dds_files_become_texture_sets() {
        use cw_core::rpf::build::{Item, RpfBuilder};
        let dir = std::env::temp_dir().join(format!("cw-prev-tex-{}", std::process::id()));
        std::fs::create_dir_all(&dir).unwrap();

        let (big, small) = (gradient_texture(512, 256, 5), gradient_texture(16, 16, 1));
        let res = ytd::build_ytd(&[("big", &big), ("small", &small)]);
        let rpf =
            RpfBuilder::new(vec![Item::resource("t.ytd", res.data.clone(), res.sys_flags, res.gfx_flags)])
                .build()
                .unwrap();
        let tree =
            RpfTree::scan_blob(cw_core::Blob::from_vec(rpf), "t.rpf".into(), "t.rpf".into(), "t.rpf", None)
                .unwrap();

        let l = load(Some(&tree), &Source::Entry(tree.find("t.rpf/t.ytd").unwrap())).unwrap();
        let set = l.textures.as_ref().expect("a .ytd is a set of textures");
        assert!(l.texture_error.is_none() && l.text.is_none());
        assert_eq!(set.entries.iter().map(|e| e.name.as_str()).collect::<Vec<_>>(), ["big", "small"]);
        let t0 = &set.entries[0];
        assert_eq!((t0.texture.width, t0.texture.height, t0.texture.levels), (512, 256, 5));
        let thumb = t0.thumb.as_ref().expect("a picture for the list");
        assert!(
            (thumb.width, thumb.height) == (128, 64) && thumb.original == (512, 256),
            "the smallest mip still 2x the thumbnail size: {}x{}",
            thumb.width,
            thumb.height
        );
        assert_eq!(thumb.rgba.len(), 128 * 64 * 4);
        assert_eq!(set.entries[1].thumb.as_ref().unwrap().width, 16, "small textures are not scaled");

        // the same file taken from disk (OpenIV style), and a DDS
        let ytd_file = dir.join("t.ytd");
        std::fs::write(&ytd_file, tree.export(tree.find("t.rpf/t.ytd").unwrap()).unwrap()).unwrap();
        let from_disk = load(None, &Source::Disk(ytd_file)).unwrap();
        assert_eq!(from_disk.textures.as_ref().unwrap().entries.len(), 2);

        let dds_file = dir.join("wall.dds");
        std::fs::write(&dds_file, texture::write_dds(&big).unwrap()).unwrap();
        let dds = load(None, &Source::Disk(dds_file)).unwrap();
        let one = &dds.textures.as_ref().unwrap().entries;
        assert_eq!((one.len(), one[0].name.as_str()), (1, "wall"), "named after the file");
        assert_eq!(*one[0].texture, big);
        let _ = std::fs::remove_dir_all(dir);
    }

    #[test]
    fn a_texture_file_that_cannot_be_read_still_opens_with_the_reason() {
        let dir = std::env::temp_dir().join(format!("cw-prev-badtex-{}", std::process::id()));
        std::fs::create_dir_all(&dir).unwrap();
        let file = dir.join("broken.dds");
        std::fs::write(&file, b"DDS but not really").unwrap();
        let l = load(None, &Source::Disk(file)).unwrap();
        assert!(l.textures.is_none());
        assert!(l.texture_error.as_deref().is_some_and(|e| !e.is_empty()));
        assert_eq!(&**l.bytes, b"DDS but not really", "hex and info views still have the bytes");
        let _ = std::fs::remove_dir_all(dir);
    }

    #[test]
    fn info_rows_describe_resources() {
        let l = build(Parts {
            name: "a.ydr".into(),
            path: "x/a.ydr".into(),
            kind: FileKind::Drawable,
            bytes: vec![0; 100],
            stored_size: Some(40),
            encrypted: false,
            container: Some("x.rpf".into()),
            resource: Some(res_info(1 << 17, 0x9000_0000)),
            resource_data: None,
        });
        let rows = info_rows(&l);
        assert!(rows.contains(&("Type", "Drawable".into())));
        assert!(rows.contains(&("Stored", "40 B (40%)".into())));
        assert!(rows.contains(&("Resource version", "9".into())));
        assert!(rows.contains(&("System pages", "8.0 KiB".into())));
    }
}
