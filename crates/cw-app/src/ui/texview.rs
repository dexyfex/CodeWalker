//! The texture viewer: filter controls, the picture (composed on the GPU, with pan and zoom),
//! a pixel readout, and a strip of the other textures in the file. State lives in
//! [`crate::texview::TexView`]; this only draws it and reports what was asked for.

use cw_core::image::Channels;
use cw_gpu::compose::ImageView;
use eframe::egui::{
    self, Align2, Color32, CursorIcon, FontId, Rect, ScrollArea, Sense, Stroke, StrokeKind, Ui, pos2, vec2,
};

use super::{Btn, Tone, fit_text, icon_button, segmented};
use crate::{
    gpu::Gpu,
    icons::Icon,
    preview::TexSet,
    texview::{DecodeJob, Palette, TexView},
    theme,
};

/// What the viewer wants the app to do after the frame.
#[derive(Default)]
pub struct TexRequest {
    /// A decode to start (another texture, another mip).
    pub decode: Option<DecodeJob>,
    /// Save the texture on show: `true` for a filtered PNG, `false` for the DDS as stored.
    pub export: Option<bool>,
}

const CHANNEL_LABELS: [&str; 6] = ["RGBA", "RGB", "R", "G", "B", "A"];
const STRIP_CELL: egui::Vec2 = egui::vec2(76.0, 88.0);
const STRIP_H: f32 = 108.0;

fn palette() -> Palette {
    let f = |c: Color32| c.to_normalized_gamma_f32();
    Palette { ground: f(theme::GROUND), checker: (f(theme::CHECKER_A), f(theme::CHECKER_B)) }
}

pub fn texture_viewer(ui: &mut Ui, view: &mut TexView, set: &TexSet, gpu: Option<&Gpu>) -> TexRequest {
    let mut req = TexRequest::default();
    let Some(entry) = set.entries.get(view.index) else { return req };
    let tex = entry.texture.clone();
    let levels = tex.levels.max(1);

    // ---------------------------------------------------------- controls
    ui.horizontal(|ui| {
        let selected = Channels::ALL.iter().position(|c| *c == view.channels).unwrap_or(0);
        if let Some(i) = segmented(ui, "tex-channels", &CHANNEL_LABELS, selected) {
            view.channels = Channels::ALL[i];
        }
        let tone = if view.invert { Tone::Primary } else { Tone::Secondary };
        if Btn::new("Invert").tone(tone).tip("Invert the colours (alpha is kept)").show(ui).clicked() {
            view.invert = !view.invert;
        }
    });
    ui.horizontal(|ui| {
        if levels > 1 {
            let level = view.level.min(levels - 1);
            let (w, h) = cw_core::texture::mip_dims(tex.width, tex.height, level);
            if icon_button(ui, Icon::Back, "Larger mip", level > 0).clicked() {
                view.set_level(level - 1);
            }
            ui.label(egui::RichText::new(format!("Mip {level} · {w}×{h}")).color(theme::MUTED).size(12.0));
            if icon_button(ui, Icon::Forward, "Smaller mip", level + 1 < levels).clicked() {
                view.set_level(level + 1);
            }
            ui.add_space(8.0);
        }
        if Btn::new("Fit").icon(Icon::Frame).tone(Tone::Ghost).tip("Whole picture in view").show(ui).clicked()
        {
            view.fit = true;
        }
        if Btn::new("1:1").tone(Tone::Ghost).tip("One image pixel per screen pixel").show(ui).clicked()
            && let Some(size) = view.size()
        {
            view.view = ImageView::actual(size);
            view.fit = false;
        }
        ui.label(
            egui::RichText::new(format!("{:.0}%", view.view.zoom * 100.0)).color(theme::FAINT).size(12.0),
        );
    });

    // ----------------------------------------------------------- picture
    let strip = if set.entries.len() > 1 { STRIP_H + ui.spacing().item_spacing.y } else { 0.0 };
    let footer = 24.0 + 36.0 + 3.0 * ui.spacing().item_spacing.y + 8.0;
    let height = (ui.available_height() - strip - footer).max(120.0);
    let (rect, resp) = ui.allocate_exact_size(vec2(ui.available_width(), height), Sense::click_and_drag());
    picture(ui, view, gpu, rect, &resp);

    // ------------------------------------------------------------ footer
    let note = format!(
        "{} × {} · {} · {}",
        tex.width,
        tex.height,
        tex.format.label(),
        crate::format::plural(levels as u64, "mip", "mips")
    );
    let (r, _) = ui.allocate_exact_size(vec2(ui.available_width(), 24.0), Sense::hover());
    let font = FontId::proportional(12.0);
    let text = fit_text(ui.painter(), &format!("{} · {note}", entry.name), &font, r.width());
    ui.painter().text(r.left_center(), Align2::LEFT_CENTER, text, font, theme::FAINT);

    // ------------------------------------------------------------- strip
    if set.entries.len() > 1 {
        thumbnails(ui, view, set);
    }

    ui.horizontal(|ui| {
        if Btn::new("Save as PNG…").tip("The picture as shown: this mip, these channels").show(ui).clicked()
        {
            req.export = Some(true);
        }
        if Btn::new("Save as DDS…").tip("The whole texture as stored, every mip").show(ui).clicked() {
            req.export = Some(false);
        }
    });

    req.decode = view.next_job(set);
    req
}

fn picture(ui: &mut Ui, view: &mut TexView, gpu: Option<&Gpu>, rect: Rect, resp: &egui::Response) {
    let painter = ui.painter_at(rect);
    painter.rect_filled(rect, 4, theme::GROUND);
    let ppp = ui.ctx().pixels_per_point();
    let viewport = (rect.width() * ppp, rect.height() * ppp);

    let Some(size) = view.size() else {
        let msg = match (&view.error, view.is_decoding()) {
            (Some(e), _) => format!("Could not decode this texture: {e}"),
            (None, true) => "Decoding…".to_string(),
            _ => String::new(),
        };
        let color = if view.error.is_some() { theme::NEGATIVE } else { theme::FAINT };
        painter.text(rect.center(), Align2::CENTER_CENTER, msg, FontId::proportional(13.0), color);
        painter.rect_stroke(rect, 4, Stroke::new(1.0, theme::BORDER), StrokeKind::Inside);
        return;
    };

    if view.fit {
        view.view = ImageView::fit(size, viewport);
    }
    let local = |p: egui::Pos2| ((p.x - rect.left()) * ppp, (p.y - rect.top()) * ppp);
    if resp.dragged() {
        let d = resp.drag_delta();
        view.view.pan(d.x * ppp, d.y * ppp);
        view.view.clamp(size);
        view.fit = false;
    }
    if resp.hovered() {
        let (scroll, pinch) = ui.input(|i| (i.smooth_scroll_delta.y, i.zoom_delta()));
        let factor = pinch * (scroll * 0.002).exp();
        if (factor - 1.0).abs() > 1e-4
            && let Some(p) = resp.hover_pos()
        {
            view.view.zoom_at(local(p), viewport, factor);
            view.view.clamp(size);
            view.fit = false;
        }
    }
    if resp.double_clicked() {
        if view.fit {
            view.view = ImageView::actual(size);
        }
        view.fit = !view.fit;
    }
    if resp.dragged() {
        ui.ctx().set_cursor_icon(CursorIcon::Grabbing);
    } else if resp.hovered() {
        ui.ctx().set_cursor_icon(CursorIcon::Grab);
    }

    if let Some(gpu) = gpu
        && let Some(id) = view.render(gpu, (viewport.0 as u32, viewport.1 as u32), palette())
    {
        painter.image(id, rect, Rect::from_min_max(pos2(0.0, 0.0), pos2(1.0, 1.0)), Color32::WHITE);
    } else {
        painter.text(
            rect.center(),
            Align2::CENTER_CENTER,
            "No GPU available for this view",
            FontId::proportional(13.0),
            theme::FAINT,
        );
    }
    painter.rect_stroke(rect, 4, Stroke::new(1.0, theme::BORDER), StrokeKind::Inside);

    // what is under the pointer: position and the colour as decoded (before any filter)
    if let Some(p) = resp.hover_pos()
        && let Some((x, y)) = view.view.pixel_at(local(p), viewport, size)
        && let Some(px) = view.pixel(x, y)
    {
        let text = format!(
            "{x}, {y}   {} {} {} {}   #{:02X}{:02X}{:02X}{:02X}",
            px[0], px[1], px[2], px[3], px[0], px[1], px[2], px[3]
        );
        let font = FontId::monospace(11.5);
        let galley = painter.layout_no_wrap(text, font, theme::TEXT);
        let at = pos2(rect.left() + 8.0, rect.bottom() - 8.0 - galley.size().y);
        let chip = Rect::from_min_size(at, galley.size()).expand2(vec2(6.0, 3.0));
        painter.rect_filled(chip, 4, Color32::from_black_alpha(190));
        painter.galley(at, galley, theme::TEXT);
    }
}

fn thumbnails(ui: &mut Ui, view: &mut TexView, set: &TexSet) {
    let font = FontId::proportional(11.0);
    let mut chosen = None;
    ScrollArea::horizontal().id_salt("tex-strip").max_height(STRIP_H).auto_shrink([false, true]).show(
        ui,
        |ui| {
            ui.horizontal(|ui| {
                ui.spacing_mut().item_spacing.x = 6.0;
                for (i, entry) in set.entries.iter().enumerate() {
                    let (rect, resp) = ui.allocate_exact_size(STRIP_CELL, Sense::click());
                    if !ui.is_rect_visible(rect) {
                        continue; // hundreds of textures: only the visible ones get a picture
                    }
                    let on = i == view.index;
                    let p = ui.painter();
                    p.rect_filled(
                        rect,
                        5,
                        if on {
                            theme::ACTIVE
                        } else if resp.hovered() {
                            theme::RAISED
                        } else {
                            theme::PANEL
                        },
                    );
                    if on {
                        p.rect_stroke(rect, 5, Stroke::new(1.5, theme::ACCENT), StrokeKind::Inside);
                    }
                    let frame = Rect::from_min_size(rect.min + vec2(6.0, 6.0), vec2(64.0, 56.0));
                    if let (Some(d), slot) = (&entry.thumb, &mut view.thumbs[i]) {
                        let handle = slot.get_or_insert_with(|| {
                            let img = egui::ColorImage::from_rgba_unmultiplied([d.width, d.height], &d.rgba);
                            ui.ctx().load_texture(format!("thumb-{i}"), img, egui::TextureOptions::LINEAR)
                        });
                        let scale = (frame.width() / d.width as f32).min(frame.height() / d.height as f32);
                        let shown = Rect::from_center_size(
                            frame.center(),
                            vec2(d.width as f32, d.height as f32) * scale,
                        );
                        let p = ui.painter();
                        p.rect_filled(shown, 2, theme::CHECKER_A);
                        p.image(
                            handle.id(),
                            shown,
                            Rect::from_min_max(pos2(0.0, 0.0), pos2(1.0, 1.0)),
                            Color32::WHITE,
                        );
                    }
                    let p = ui.painter();
                    let label = if entry.name.is_empty() { format!("#{i}") } else { entry.name.clone() };
                    let text = fit_text(p, &label, &font, rect.width() - 10.0);
                    p.text(
                        pos2(rect.center().x, rect.bottom() - 14.0),
                        Align2::CENTER_CENTER,
                        text,
                        font.clone(),
                        if on { theme::TEXT } else { theme::MUTED },
                    );
                    if resp.clicked() {
                        chosen = Some(i);
                    }
                    resp.on_hover_text(format!(
                        "{} · {} × {} · {}",
                        label,
                        entry.texture.width,
                        entry.texture.height,
                        entry.texture.format.label()
                    ));
                }
            });
        },
    );
    if let Some(i) = chosen {
        view.select(i);
    }
}
