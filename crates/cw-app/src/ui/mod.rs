//! Hand-drawn widgets. egui's stock buttons carry their own padding, strokes
//! and states; drawing ours keeps every control on the same tokens (docs/DESIGN.md).

use eframe::egui::{
    self, Align2, Color32, CornerRadius, FontId, Painter, Response, Sense, Stroke, StrokeKind, Ui, pos2, vec2,
};

use crate::{
    format::middle_truncate,
    icons::{self, Icon},
    theme,
};

pub mod detail;
pub mod empty;
pub mod list;
pub mod mods;
pub mod palette;
pub mod status;
pub mod texview;
pub mod toasts;
pub mod toolbar;
pub mod tree;
pub mod world;

pub const CONTROL_H: f32 = 28.0;
const R: u8 = 5;

#[derive(Clone, Copy, PartialEq, Eq)]
pub enum Tone {
    /// Quiet: transparent until hovered (toolbar, row actions).
    Ghost,
    /// Outlined secondary control.
    Secondary,
    /// The one primary action of a screen: inverted (light fill, dark label).
    Primary,
}

pub struct Btn<'a> {
    pub icon: Option<Icon>,
    pub label: &'a str,
    pub tip: &'a str,
    pub enabled: bool,
    pub tone: Tone,
}

impl<'a> Btn<'a> {
    pub fn new(label: &'a str) -> Self {
        Self { icon: None, label, tip: "", enabled: true, tone: Tone::Secondary }
    }

    pub fn icon(mut self, i: Icon) -> Self {
        self.icon = Some(i);
        self
    }

    pub fn tip(mut self, t: &'a str) -> Self {
        self.tip = t;
        self
    }

    pub fn enabled(mut self, e: bool) -> Self {
        self.enabled = e;
        self
    }

    pub fn tone(mut self, t: Tone) -> Self {
        self.tone = t;
        self
    }

    pub fn show(self, ui: &mut Ui) -> Response {
        let font = FontId::proportional(13.0);
        let (fg, fill, hover, press, stroke) = match self.tone {
            Tone::Ghost => (theme::MUTED, Color32::TRANSPARENT, theme::RAISED, theme::ACTIVE, None),
            Tone::Secondary => (theme::TEXT, theme::RAISED, theme::ACTIVE, theme::PANEL, Some(theme::BORDER)),
            Tone::Primary => {
                (Color32::from_rgb(0x0D, 0x0D, 0x0E), theme::TEXT, Color32::WHITE, theme::MUTED, None)
            }
        };
        let fg = if self.enabled { fg } else { theme::FAINT };
        let galley = ui.painter().layout_no_wrap(self.label.to_string(), font, fg);
        let icon_w = if self.icon.is_some() { 16.0 } else { 0.0 };
        let gap = if self.icon.is_some() && !self.label.is_empty() { 6.0 } else { 0.0 };
        let pad = if self.label.is_empty() { 6.0 } else { 10.0 };
        let w = (pad * 2.0 + icon_w + gap + galley.size().x).max(CONTROL_H);
        let sense = if self.enabled { Sense::click() } else { Sense::hover() };
        let (rect, resp) = ui.allocate_exact_size(vec2(w, CONTROL_H), sense);

        if ui.is_rect_visible(rect) {
            let p = ui.painter();
            let bg = if !self.enabled {
                fill
            } else if resp.is_pointer_button_down_on() {
                press
            } else if resp.hovered() {
                hover
            } else {
                fill
            };
            let bg = if self.tone == Tone::Ghost && !self.enabled { Color32::TRANSPARENT } else { bg };
            p.rect_filled(rect, CornerRadius::same(R), bg);
            if let Some(s) = stroke {
                p.rect_stroke(rect, CornerRadius::same(R), Stroke::new(1.0, s), StrokeKind::Inside);
            }
            let content_w = icon_w + gap + galley.size().x;
            let mut x = rect.center().x - content_w / 2.0;
            if let Some(i) = self.icon {
                let ir = egui::Rect::from_min_size(pos2(x, rect.center().y - 8.0), vec2(16.0, 16.0));
                icons::draw(p, ir, i, fg);
                x += icon_w + gap;
            }
            if !self.label.is_empty() {
                p.galley(pos2(x, rect.center().y - galley.size().y / 2.0), galley, fg);
            }
            focus_ring(p, rect, &resp);
        }
        if !self.tip.is_empty() {
            return resp.on_hover_text(self.tip);
        }
        resp
    }
}

pub fn focus_ring(p: &Painter, rect: egui::Rect, resp: &Response) {
    if resp.has_focus() {
        p.rect_stroke(
            rect.expand(2.0),
            CornerRadius::same(R + 2),
            Stroke::new(2.0, theme::ACCENT),
            StrokeKind::Middle,
        );
    }
}

/// Toolbar-style icon-only button.
pub fn icon_button(ui: &mut Ui, icon: Icon, tip: &str, enabled: bool) -> Response {
    Btn::new("").icon(icon).tip(tip).enabled(enabled).tone(Tone::Ghost).show(ui)
}

/// `text` shortened in the middle so it fits `max_w` pixels. Measures real
/// glyph widths, so it works for any script.
pub fn fit_text(p: &Painter, text: &str, font: &FontId, max_w: f32) -> String {
    let width = |s: &str| p.layout_no_wrap(s.to_string(), font.clone(), Color32::WHITE).size().x;
    if max_w <= 0.0 {
        return String::new();
    }
    if width(text) <= max_w {
        return text.to_string();
    }
    let n = text.chars().count();
    let (mut lo, mut hi) = (3usize, n.saturating_sub(1));
    let mut best = middle_truncate(text, lo);
    while lo <= hi {
        let mid = (lo + hi) / 2;
        let cand = middle_truncate(text, mid);
        if width(&cand) <= max_w {
            best = cand;
            lo = mid + 1;
        } else {
            if mid == 0 {
                break;
            }
            hi = mid - 1;
        }
    }
    best
}

/// A row of mutually exclusive options. Returns the index clicked, if any. `salt` names the
/// control: two of them in one window must not share widget ids, and a ui's own id is not a
/// reliable way to tell them apart.
pub fn segmented(ui: &mut Ui, salt: &str, items: &[&str], selected: usize) -> Option<usize> {
    let font = FontId::proportional(12.0);
    let mut clicked = None;
    let widths: Vec<f32> = items
        .iter()
        .map(|t| ui.painter().layout_no_wrap((*t).to_string(), font.clone(), theme::TEXT).size().x + 20.0)
        .collect();
    let total: f32 = widths.iter().sum::<f32>() + 4.0;
    let (rect, _) = ui.allocate_exact_size(vec2(total, 28.0), Sense::hover());
    let p = ui.painter();
    p.rect_filled(rect, CornerRadius::same(R + 1), theme::GROUND);
    p.rect_stroke(rect, CornerRadius::same(R + 1), Stroke::new(1.0, theme::BORDER), StrokeKind::Inside);
    let mut x = rect.left() + 2.0;
    for (i, label) in items.iter().enumerate() {
        let r = egui::Rect::from_min_size(pos2(x, rect.top() + 2.0), vec2(widths[i], 24.0));
        let resp = ui.interact(r, egui::Id::unique(("segmented", salt, i)), Sense::click());
        let p = ui.painter();
        let on = i == selected;
        if on {
            p.rect_filled(r, CornerRadius::same(R), theme::ACTIVE);
        } else if resp.hovered() {
            p.rect_filled(r, CornerRadius::same(R), theme::RAISED);
        }
        p.text(
            r.center(),
            Align2::CENTER_CENTER,
            label,
            font.clone(),
            if on { theme::TEXT } else { theme::MUTED },
        );
        focus_ring(p, r, &resp);
        if resp.clicked() {
            clicked = Some(i);
        }
        x += widths[i];
    }
    clicked
}

/// Thin horizontal rule in the border colour.
pub fn hairline(ui: &mut Ui) {
    let (rect, _) = ui.allocate_exact_size(vec2(ui.available_width(), 1.0), Sense::hover());
    ui.painter().rect_filled(rect, 0.0, theme::BORDER);
}

/// Small grey caption (metadata).
pub fn faint(ui: &mut Ui, text: impl Into<String>) {
    ui.label(egui::RichText::new(text.into()).color(theme::FAINT).size(12.0));
}

// ------------------------------------------------------------------ layout

use eframe::egui::{CentralPanel, Frame, Margin, Panel};

use crate::app::{App, Mode};

fn flat(fill: Color32) -> Frame {
    Frame::new().fill(fill).inner_margin(Margin::ZERO)
}

/// 1 px separator along one edge of a panel.
pub fn edge(ui: &Ui, side: Side) {
    let r = ui.max_rect();
    let s = Stroke::new(1.0, theme::BORDER);
    match side {
        Side::Right => ui.painter().vline(r.right() - 0.5, r.y_range(), s),
        Side::Left => ui.painter().vline(r.left() + 0.5, r.y_range(), s),
        Side::Bottom => ui.painter().hline(r.x_range(), r.bottom() - 0.5, s),
        Side::Top => ui.painter().hline(r.x_range(), r.top() + 0.5, s),
    };
}

#[derive(Clone, Copy)]
pub enum Side {
    Left,
    Right,
    Top,
    Bottom,
}

impl App {
    pub fn show(&mut self, ui: &mut Ui) {
        let ctx = ui.ctx().clone();
        Panel::top("toolbar").exact_size(theme::TOOLBAR_H).frame(flat(theme::PANEL)).show(ui, |ui| {
            self.show_toolbar(ui);
            edge(ui, Side::Bottom);
        });
        Panel::bottom("status").exact_size(theme::STATUS_H).frame(flat(theme::PANEL)).show(ui, |ui| {
            self.show_status(ui);
            edge(ui, Side::Top);
        });

        if self.mode == Mode::World {
            self.show_world(ui);
        } else if self.mode == Mode::Mods {
            CentralPanel::default().frame(flat(theme::PANEL)).show(ui, |ui| self.show_mods(ui));
        } else if self.ex.is_empty() && self.loose.is_empty() {
            CentralPanel::default().frame(flat(theme::PANEL)).show(ui, |ui| self.show_empty(ui));
        } else {
            Panel::left("tree")
                .resizable(true)
                .default_size(280.0)
                .size_range(220.0..=440.0)
                .frame(flat(theme::PANEL))
                .show(ui, |ui| {
                    self.show_tree(ui);
                    edge(ui, Side::Right);
                });
            Panel::right("detail")
                .resizable(true)
                .default_size(400.0)
                .size_range(300.0..=700.0)
                .frame(flat(theme::PANEL))
                .show(ui, |ui| {
                    self.show_detail(ui);
                    edge(ui, Side::Left);
                });
            CentralPanel::default().frame(flat(theme::PANEL)).show(ui, |ui| self.show_list(ui));
        }

        self.show_palette(&ctx);
        self.show_toasts(&ctx);
    }
}
