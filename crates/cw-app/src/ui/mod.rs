//! Hand-drawn widgets. egui's stock buttons carry their own padding, strokes
//! and states; drawing ours keeps every control on the same tokens (docs/DESIGN.md).

use eframe::egui::{
    self, Align2, Color32, CornerRadius, FontId, Layout, Painter, Rect, Response, Sense, Stroke, StrokeKind,
    Ui, UiBuilder, pos2, vec2,
};

use crate::{
    format::middle_truncate,
    icons::{self, Icon},
    theme,
};

pub mod detail;
pub mod empty;
pub mod list;
pub mod palette;
pub mod status;
pub mod tabs;
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

    /// Width of the button as [`show`](Self::show) will draw it, so rows can be planned before they
    /// are drawn.
    pub fn width(&self, ui: &Ui) -> f32 {
        self.width_for(text_width(ui, self.label, 13.0))
    }

    /// The icon's width and the gap between it and the label.
    fn icon_space(&self) -> (f32, f32) {
        let icon_w = if self.icon.is_some() { 16.0 } else { 0.0 };
        (icon_w, if self.icon.is_some() && !self.label.is_empty() { 6.0 } else { 0.0 })
    }

    fn width_for(&self, text_w: f32) -> f32 {
        let (icon_w, gap) = self.icon_space();
        let pad = if self.label.is_empty() { 6.0 } else { 10.0 };
        (pad * 2.0 + icon_w + gap + text_w).max(CONTROL_H)
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
        let (icon_w, gap) = self.icon_space();
        let w = self.width_for(galley.size().x);
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

/// Width of `text` in the UI font at `size`.
pub fn text_width(ui: &Ui, text: &str, size: f32) -> f32 {
    ui.painter().layout_no_wrap(text.to_string(), FontId::proportional(size), theme::TEXT).size().x
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

/// What one option of a [`segmented`] control shows.
#[derive(Clone, Copy)]
pub enum Face<'a> {
    Text(&'a str),
    /// An icon alone, for when there is no room for words; the label is its tooltip.
    Icon(Icon, &'a str),
}

fn face_width(ui: &Ui, face: Face) -> f32 {
    match face {
        Face::Text(t) => text_width(ui, t, 12.0) + 20.0,
        Face::Icon(..) => CONTROL_H,
    }
}

/// Width of the control [`segmented_faces`] draws, for planning a row before drawing it.
pub fn segmented_width(ui: &Ui, faces: &[Face]) -> f32 {
    faces.iter().map(|f| face_width(ui, *f)).sum::<f32>() + 4.0
}

/// A row of mutually exclusive options. Returns the index clicked, if any. `salt` names the
/// control: two of them in one window must not share widget ids, and a ui's own id is not a
/// reliable way to tell them apart.
pub fn segmented(ui: &mut Ui, salt: &str, items: &[&str], selected: usize) -> Option<usize> {
    let faces: Vec<Face> = items.iter().map(|t| Face::Text(t)).collect();
    segmented_faces(ui, salt, &faces, selected)
}

/// [`segmented`] with options that may be icons.
pub fn segmented_faces(ui: &mut Ui, salt: &str, faces: &[Face], selected: usize) -> Option<usize> {
    let font = FontId::proportional(12.0);
    let mut clicked = None;
    let widths: Vec<f32> = faces.iter().map(|f| face_width(ui, *f)).collect();
    let (rect, _) = ui.allocate_exact_size(vec2(segmented_width(ui, faces), 28.0), Sense::hover());
    let p = ui.painter();
    p.rect_filled(rect, CornerRadius::same(R + 1), theme::GROUND);
    p.rect_stroke(rect, CornerRadius::same(R + 1), Stroke::new(1.0, theme::BORDER), StrokeKind::Inside);
    let mut x = rect.left() + 2.0;
    for (i, face) in faces.iter().enumerate() {
        let r = Rect::from_min_size(pos2(x, rect.top() + 2.0), vec2(widths[i], 24.0));
        let mut resp = ui.interact(r, egui::Id::unique(("segmented", salt, i)), Sense::click());
        if let Face::Icon(_, tip) = face {
            resp = resp.on_hover_text(*tip);
        }
        let p = ui.painter();
        let on = i == selected;
        if on {
            p.rect_filled(r, CornerRadius::same(R), theme::ACTIVE);
        } else if resp.hovered() {
            p.rect_filled(r, CornerRadius::same(R), theme::RAISED);
        }
        let color = if on { theme::TEXT } else { theme::MUTED };
        match face {
            Face::Text(label) => {
                p.text(r.center(), Align2::CENTER_CENTER, label, font.clone(), color);
            }
            Face::Icon(icon, _) => {
                icons::draw(p, Rect::from_center_size(r.center(), vec2(16.0, 16.0)), *icon, color)
            }
        }
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

/// Window edge to the first and last control of a bar.
const EDGE: f32 = 8.0;
/// Space between groups of controls.
const GAP: f32 = 8.0;

/// The row of controls inside a bar: the bar's width less the edge margins, one control high.
fn bar_row(ui: &Ui) -> Rect {
    let full = ui.max_rect();
    Rect::from_center_size(full.center(), vec2(full.width() - 2.0 * EDGE, CONTROL_H))
}

/// A child ui that fills exactly `rect`. Rows are planned from measured widths (see `layout.rs`)
/// and each group of controls gets its own rect, so one that turns out wider than planned is
/// clipped at its edge instead of being drawn over its neighbour.
fn cluster<R>(ui: &mut Ui, rect: Rect, layout: Layout, add: impl FnOnce(&mut Ui) -> R) -> R {
    let mut child = ui.new_child(UiBuilder::new().max_rect(rect).layout(layout));
    // room for the focus ring, which is drawn just outside a control
    child.shrink_clip_rect(rect.expand2(vec2(3.0, 4.0)));
    add(&mut child)
}

use eframe::egui::{CentralPanel, Frame, Margin, Panel};

use crate::{app::App, tabs::Tab};

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
        let active = self.tabs.active();
        let explorer = active == Tab::Explorer;
        // the Explorer's bar sits right under the tabs, so only the lower one gets the hairline
        Panel::top("tabs").exact_size(theme::TABS_H).frame(flat(theme::PANEL)).show(ui, |ui| {
            self.show_tabs(ui);
            if !explorer {
                edge(ui, Side::Bottom);
            }
        });
        if explorer {
            Panel::top("toolbar").exact_size(theme::TOOLBAR_H).frame(flat(theme::PANEL)).show(ui, |ui| {
                self.show_toolbar(ui);
                edge(ui, Side::Bottom);
            });
        }
        Panel::bottom("status").exact_size(theme::STATUS_H).frame(flat(theme::PANEL)).show(ui, |ui| {
            self.show_status(ui);
            edge(ui, Side::Top);
        });

        match active {
            Tab::World => self.show_world(ui),
            // the model viewer (docs/PLAN.md stage 4) draws a document tab's page here
            Tab::Document(_) => {
                CentralPanel::default().frame(flat(theme::PANEL)).show(ui, |_| {});
            }
            Tab::Explorer => self.show_explorer(ui),
        }

        self.show_palette(&ctx);
        self.show_toasts(&ctx);
    }

    fn show_explorer(&mut self, ui: &mut Ui) {
        if self.ex.is_empty() && self.loose.is_empty() {
            CentralPanel::default().frame(flat(theme::PANEL)).show(ui, |ui| self.show_empty(ui));
            return;
        }
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
}
