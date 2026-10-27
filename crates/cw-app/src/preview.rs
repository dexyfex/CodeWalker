//! Loading a file for preview (off the UI thread) and the views that show it.

use std::{ops::Range, sync::Arc};

use cw_core::{EntryKind, EntryRef, RpfTree, filekind::FileKind, resource, rpf::page_flags_size};
use eframe::egui::{
    self, Align2, Color32, FontId, Rect, ScrollArea, Sense, Stroke, TextureHandle, TextureOptions, Ui, pos2,
    vec2,
};

use crate::{format, theme};

/// Larger files are still extracted, but previews stop here to stay responsive.
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
            let (bytes, resource) = match e.kind {
                EntryKind::Resource { .. } => {
                    let rd = t.extract_resource(*r).map_err(|e| e.to_string())?;
                    let info = res_info(rd.sys_flags, rd.gfx_flags);
                    (rd.data, Some(info))
                }
                _ => (t.extract(*r).map_err(|e| e.to_string())?, None),
            };
            let encrypted = matches!(
                e.kind,
                EntryKind::Binary { encrypted: true, .. } | EntryKind::Resource { encrypted: true, .. }
            );
            let kind = FileKind::from_name(&e.name_lower);
            Ok(build(
                e.name.clone(),
                e.path.clone(),
                kind,
                bytes,
                Some(e.stored_size()),
                encrypted,
                Some(t.archive(r.archive).name.clone()),
                resource,
            ))
        }
        Source::Disk(p) => {
            let raw = std::fs::read(p).map_err(|e| e.to_string())?;
            let name = p.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default();
            let kind = FileKind::from_name(&name.to_lowercase());
            let stored = raw.len() as u64;
            let (bytes, resource) = if kind.is_resource() && resource::has_rsc7_header(&raw) {
                let rd = resource::parse_loose(&raw).map_err(|e| e.to_string())?;
                let info = res_info(rd.sys_flags, rd.gfx_flags);
                (rd.data, Some(info))
            } else {
                (raw, None)
            };
            Ok(build(name, p.display().to_string(), kind, bytes, Some(stored), false, None, resource))
        }
    }
}

fn res_info(sys_flags: u32, gfx_flags: u32) -> ResInfo {
    ResInfo {
        version: cw_core::rpf::resource_version(sys_flags, gfx_flags),
        sys_size: page_flags_size(sys_flags),
        gfx_size: page_flags_size(gfx_flags),
    }
}

#[allow(clippy::too_many_arguments)]
fn build(
    name: String,
    path: String,
    kind: FileKind,
    bytes: Vec<u8>,
    stored_size: Option<u64>,
    encrypted: bool,
    container: Option<String>,
    resource: Option<ResInfo>,
) -> Loaded {
    let image = (kind == FileKind::Image).then(|| decode_image(&bytes)).flatten();
    let text = if image.is_none() && resource.is_none() { decode_text(&bytes, kind) } else { None };
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
        (decode_utf16(rest, false), false)
    } else if let Some(rest) = bytes.strip_prefix(&[0xFE, 0xFF]) {
        (decode_utf16(rest, true), false)
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

fn decode_utf16(bytes: &[u8], big_endian: bool) -> Option<String> {
    let units: Vec<u16> = bytes
        .chunks_exact(2)
        .map(|c| if big_endian { u16::from_be_bytes([c[0], c[1]]) } else { u16::from_le_bytes([c[0], c[1]]) })
        .collect();
    String::from_utf16(&units).ok()
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

    #[test]
    fn info_rows_describe_resources() {
        let l = build(
            "a.ydr".into(),
            "x/a.ydr".into(),
            FileKind::Drawable,
            vec![0; 100],
            Some(40),
            false,
            Some("x.rpf".into()),
            Some(res_info(1 << 17, 0x9000_0000)),
        );
        let rows = info_rows(&l);
        assert!(rows.contains(&("Type", "Drawable".into())));
        assert!(rows.contains(&("Stored", "40 B (40%)".into())));
        assert!(rows.contains(&("Resource version", "9".into())));
        assert!(rows.contains(&("System pages", "8.0 KiB".into())));
    }
}
