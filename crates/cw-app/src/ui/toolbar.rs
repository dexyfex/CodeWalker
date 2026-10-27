//! The Explorer's bar: back / forward / up, where you are, and the filter. The row is planned from
//! measured widths (`layout.rs`) and every group gets its own rect, so nothing can overlap: the
//! path takes what the other groups leave, and when that is too little they give way in turn.

use eframe::egui::{
    self, Align, Align2, Button, Color32, FontId, Layout, Margin, Popup, Rect, Sense, TextEdit, Ui, pos2,
    vec2,
};

use super::{
    CONTROL_H, Face, GAP, bar_row, cluster, fit_text, focus_ring, icon_button, segmented_faces,
    segmented_width, text_width,
};
use crate::{
    actions::Action,
    app::App,
    format,
    icons::{self, Icon},
    layout::{Crumb, CrumbGaps, Give, fit_crumbs, squeeze},
    model::NodeId,
    theme,
};

/// Back, forward and up, 4 px apart.
const NAV_W: f32 = 3.0 * CONTROL_H + 2.0 * 4.0;
const FILTER_W: f32 = 220.0;
/// The filter shrinks this far before the scope control drops its words.
const FILTER_MIN: f32 = 150.0;
/// The room the path is owed before the filter and the scope control give way for it.
const PATH_WANT: f32 = 320.0;
/// Padding a crumb adds to its text.
const CRUMB_PAD: f32 = 14.0;
const CRUMBS: CrumbGaps = CrumbGaps { sep: 14.0, fold: 28.0, min_last: 96.0 };

impl App {
    pub(super) fn show_toolbar(&mut self, ui: &mut Ui) {
        let row = bar_row(ui);
        let words = [Face::Text("This folder"), Face::Text("All archives")];
        let icons = [
            Face::Icon(Icon::Folder, "Filter this folder"),
            Face::Icon(Icon::Archive, "Search every archive"),
        ];
        let (words_w, icons_w) = (segmented_width(ui, &words), segmented_width(ui, &icons));

        let path_room = row.width() - NAV_W - GAP - (FILTER_W + GAP + words_w) - GAP;
        let given = squeeze(
            PATH_WANT - path_room,
            &[Give::UpTo(FILTER_W - FILTER_MIN), Give::All(words_w - icons_w)],
        );
        let filter_w = FILTER_W - given[0];
        let compact = given[1] > 0.0;
        let scope_w = if compact { icons_w } else { words_w };

        let at = |left: f32, w: f32| Rect::from_min_size(pos2(left, row.top()), vec2(w, CONTROL_H));
        let nav = at(row.left(), NAV_W);
        let scope = at(row.right() - scope_w, scope_w);
        let filter = at(scope.left() - GAP - filter_w, filter_w);
        let path_left = nav.right() + GAP;
        let path = Rect::from_min_max(
            pos2(path_left, row.top()),
            pos2((filter.left() - GAP).max(path_left), row.bottom()),
        );

        let centred = Layout::left_to_right(Align::Center);
        cluster(ui, nav, centred, |ui| {
            ui.spacing_mut().item_spacing.x = 4.0;
            if icon_button(ui, Icon::Back, "Back (Alt+←)", self.enabled(Action::Back)).clicked() {
                self.perform(Action::Back);
            }
            if icon_button(ui, Icon::Forward, "Forward (Alt+→)", self.enabled(Action::Forward)).clicked() {
                self.perform(Action::Forward);
            }
            if icon_button(ui, Icon::Up, "Parent folder (Backspace)", self.enabled(Action::Up)).clicked() {
                self.perform(Action::Up);
            }
        });
        cluster(ui, path, centred, |ui| self.show_path(ui, path.width()));
        cluster(ui, filter, centred, |ui| self.show_filter(ui, filter_w));
        cluster(ui, scope, centred, |ui| {
            let faces = if compact { &icons } else { &words };
            let selected = usize::from(self.search_everywhere);
            if let Some(i) = segmented_faces(ui, "search-scope", faces, selected)
                && i != selected
            {
                self.search_everywhere = i == 1;
                self.search_changed();
            }
        });
    }

    fn show_filter(&mut self, ui: &mut Ui, width: f32) {
        let hint = if self.search_everywhere { "Search every archive…" } else { "Filter this folder…" };
        let id = egui::Id::unique("search-box");
        // the clear button sits inside the field, so typed text stops short of it
        let right = if self.search_text.is_empty() { 10 } else { 30 };
        let resp = ui.add_sized(
            [width, CONTROL_H],
            TextEdit::singleline(&mut self.search_text)
                .id(id)
                .hint_text(hint)
                .margin(Margin { left: 10, right, top: 6, bottom: 6 })
                .font(FontId::proportional(13.0)),
        );
        if self.focus_search {
            self.focus_search = false;
            resp.request_focus();
        }
        // clear button inside the field (Esc does the same)
        if !self.search_text.is_empty() {
            let r = Rect::from_center_size(
                pos2(resp.rect.right() - 16.0, resp.rect.center().y),
                vec2(20.0, 20.0),
            );
            let c = ui.interact(r, resp.id.with("clear"), Sense::click());
            icons::draw(
                ui.painter(),
                r.shrink(4.0),
                Icon::Close,
                if c.hovered() { theme::TEXT } else { theme::MUTED },
            );
            if c.on_hover_text("Clear (Esc)").clicked() {
                self.clear_search();
            }
        }
        if resp.changed() {
            self.search_changed();
        }
        if resp.has_focus() && ui.input(|i| i.key_pressed(egui::Key::Escape)) {
            self.clear_search();
            resp.surrender_focus();
        }
        if resp.has_focus()
            && ui.input(|i| i.key_pressed(egui::Key::ArrowDown) || i.key_pressed(egui::Key::Enter))
        {
            resp.surrender_focus();
            self.ex.move_selection(1);
        }
    }

    /// The location as crumbs that fit `room`: see [`fit_crumbs`] for what folds and what shrinks.
    fn show_path(&mut self, ui: &mut Ui, room: f32) {
        if let Some(s) = &self.ex.search {
            let shown = format::thousands(s.items.len() as u64);
            let total = format::thousands(s.total as u64);
            let text = if s.items.len() < s.total {
                format!("{shown} of {total} results for “{}”", s.query)
            } else {
                format!("{} for “{}”", format::plural(s.total as u64, "result", "results"), s.query)
            };
            note(ui, &text, theme::MUTED, room);
            return;
        }
        if self.ex.tree.is_none() {
            note(ui, "Nothing open", theme::FAINT, room);
            return;
        }
        let crumbs = self.ex.breadcrumbs(&self.ex.current);
        let widths: Vec<f32> = crumbs.iter().map(|(l, _)| text_width(ui, l, 13.0) + CRUMB_PAD).collect();
        let fit = fit_crumbs(&widths, room, CRUMBS);

        ui.spacing_mut().item_spacing.x = 0.0;
        let mut go: Option<NodeId> = None;
        for (k, slot) in fit.iter().enumerate() {
            if k > 0 {
                separator(ui);
            }
            match slot {
                Crumb::Shown { index, width } => {
                    let (label, node) = &crumbs[*index];
                    if crumb(ui, label, *width, *index + 1 == crumbs.len()) {
                        go = Some(node.clone());
                    }
                }
                Crumb::Folded(hidden) => {
                    if let Some(node) = fold(ui, &crumbs[hidden.clone()]) {
                        go = Some(node);
                    }
                }
            }
        }
        if let Some(n) = go {
            self.ex.navigate(n);
        }
    }
}

/// A line of text in the path's place, shortened in the middle to fit `room`.
fn note(ui: &mut Ui, text: &str, color: Color32, room: f32) {
    let font = FontId::proportional(13.0);
    let (rect, resp) = ui.allocate_exact_size(vec2(room, CONTROL_H), Sense::hover());
    let shown = fit_text(ui.painter(), text, &font, rect.width() - CRUMB_PAD);
    ui.painter().text(
        pos2(rect.left() + CRUMB_PAD / 2.0, rect.center().y),
        Align2::LEFT_CENTER,
        &shown,
        font,
        color,
    );
    if shown != text {
        resp.on_hover_text(text);
    }
}

/// One crumb, `width` wide. True when it was clicked. The last crumb is where you are, not a
/// link; a name too long for its width ends in an ellipsis and has the whole name as a tooltip.
fn crumb(ui: &mut Ui, label: &str, width: f32, last: bool) -> bool {
    let font = FontId::proportional(13.0);
    let sense = if last { Sense::hover() } else { Sense::click() };
    let (rect, resp) = ui.allocate_exact_size(vec2(width, CONTROL_H), sense);
    let p = ui.painter();
    if resp.hovered() && !last {
        p.rect_filled(rect, 5, theme::RAISED);
    }
    let text = fit_text(p, label, &font, rect.width() - CRUMB_PAD);
    let color = if last { theme::TEXT } else { theme::MUTED };
    p.text(pos2(rect.left() + CRUMB_PAD / 2.0, rect.center().y), Align2::LEFT_CENTER, &text, font, color);
    focus_ring(p, rect, &resp);
    let clicked = resp.clicked();
    if text != label {
        resp.on_hover_text(label);
    }
    clicked
}

/// The `…` crumb for folders that did not fit: it opens a menu of them. Returns the one picked.
fn fold(ui: &mut Ui, hidden: &[(String, NodeId)]) -> Option<NodeId> {
    let (rect, resp) = ui.allocate_exact_size(vec2(CRUMBS.fold, CONTROL_H), Sense::click());
    let p = ui.painter();
    if resp.is_pointer_button_down_on() {
        p.rect_filled(rect, 5, theme::ACTIVE);
    } else if resp.hovered() {
        p.rect_filled(rect, 5, theme::RAISED);
    }
    p.text(rect.center(), Align2::CENTER_CENTER, "…", FontId::proportional(13.0), theme::MUTED);
    focus_ring(p, rect, &resp);
    let resp = resp.on_hover_text("Folders in between");
    let mut go = None;
    Popup::menu(&resp).show(|ui| {
        // as wide as the longest name, which a button's padding adds to; long ones are cut
        let names = hidden.iter().map(|(label, _)| text_width(ui, label, 13.0));
        ui.set_width((names.fold(0.0, f32::max) + 24.0).clamp(120.0, 360.0));
        for (label, node) in hidden {
            if ui.add(Button::new(label).truncate()).clicked() {
                go = Some(node.clone());
            }
        }
    });
    go
}

fn separator(ui: &mut Ui) {
    let (rect, _) = ui.allocate_exact_size(vec2(CRUMBS.sep, CONTROL_H), Sense::hover());
    icons::draw(
        ui.painter(),
        Rect::from_center_size(rect.center(), vec2(10.0, 10.0)),
        Icon::ChevronRight,
        theme::CONTROL,
    );
}
