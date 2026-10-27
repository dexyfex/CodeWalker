use eframe::egui::{self, Align, FontId, Layout, Sense, TextEdit, Ui, pos2, vec2};

use super::{Btn, Tone, icon_button, segmented};
use crate::{
    actions::Action,
    app::{App, Mode},
    format,
    icons::Icon,
    model::NodeId,
    theme,
};

impl App {
    pub(super) fn show_toolbar(&mut self, ui: &mut Ui) {
        ui.add_space(0.0);
        ui.with_layout(Layout::right_to_left(Align::Center), |ui| {
            ui.add_space(8.0);
            // below ~1180 px the labels give way to icons so the breadcrumbs keep room
            let roomy = ui.max_rect().width() >= 1180.0;
            if Btn::new(if roomy { "Open files" } else { "" })
                .icon(Icon::File)
                .tip("Open archives or files (Ctrl+O)")
                .tone(Tone::Ghost)
                .show(ui)
                .clicked()
            {
                self.perform(Action::OpenFiles);
            }
            if Btn::new(if roomy { "Game folder" } else { "" })
                .icon(Icon::FolderOpen)
                .tip("Choose your GTA V folder (Ctrl+Shift+O)")
                .tone(Tone::Secondary)
                .show(ui)
                .clicked()
            {
                self.perform(Action::OpenGameFolder);
            }
            ui.add_space(8.0);
            if self.mode == Mode::Explorer {
                self.show_search_box(ui);
                ui.add_space(8.0);
            }

            ui.with_layout(Layout::left_to_right(Align::Center), |ui| {
                ui.add_space(8.0);
                let mode = match self.mode {
                    Mode::Explorer => 0,
                    Mode::Mods => 1,
                    Mode::World => 2,
                };
                if let Some(i) = segmented(ui, "mode", &["Explorer", "Mods", "World"], mode) {
                    self.perform([Action::ShowExplorer, Action::ShowMods, Action::ShowWorld][i]);
                }
                ui.add_space(8.0);
                if self.mode != Mode::Explorer {
                    return;
                }
                if icon_button(ui, Icon::Back, "Back (Alt+←)", self.enabled(Action::Back)).clicked() {
                    self.perform(Action::Back);
                }
                if icon_button(ui, Icon::Forward, "Forward (Alt+→)", self.enabled(Action::Forward)).clicked()
                {
                    self.perform(Action::Forward);
                }
                if icon_button(ui, Icon::Up, "Parent folder (Backspace)", self.enabled(Action::Up)).clicked()
                {
                    self.perform(Action::Up);
                }
                ui.add_space(6.0);
                self.show_breadcrumbs(ui);
            });
        });
    }

    fn show_search_box(&mut self, ui: &mut Ui) {
        // right-to-left: scope control first, then the field to its left
        let scope = if self.search_everywhere { 1 } else { 0 };
        let before = scope;
        let clicked =
            ui.scope(|ui| segmented(ui, "search-scope", &["This folder", "All archives"], scope)).inner;
        if let Some(i) = clicked {
            self.search_everywhere = i == 1;
        }
        ui.add_space(6.0);
        let hint = if self.search_everywhere { "Search every archive…" } else { "Filter this folder…" };
        let id = egui::Id::unique("search-box");
        let resp = ui.add_sized(
            [220.0, 28.0],
            TextEdit::singleline(&mut self.search_text)
                .id(id)
                .hint_text(hint)
                .margin(egui::Margin::symmetric(10, 6))
                .font(FontId::proportional(13.0)),
        );
        if self.focus_search {
            self.focus_search = false;
            resp.request_focus();
        }
        // clear button inside the field (Esc does the same)
        if !self.search_text.is_empty() {
            let r = egui::Rect::from_center_size(
                pos2(resp.rect.right() - 16.0, resp.rect.center().y),
                vec2(20.0, 20.0),
            );
            let c = ui.interact(r, resp.id.with("clear"), Sense::click());
            crate::icons::draw(
                ui.painter(),
                r.shrink(4.0),
                Icon::Close,
                if c.hovered() { theme::TEXT } else { theme::MUTED },
            );
            if c.on_hover_text("Clear (Esc)").clicked() {
                self.clear_search();
            }
        }
        let scope_changed = clicked.is_some_and(|i| i != before);
        if resp.changed() || scope_changed {
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

    fn show_breadcrumbs(&mut self, ui: &mut Ui) {
        if let Some(s) = &self.ex.search {
            let shown = format::thousands(s.items.len() as u64);
            let total = format::thousands(s.total as u64);
            let text = if s.items.len() < s.total {
                format!("{shown} of {total} results for “{}”", s.query)
            } else {
                format!("{} for “{}”", format::plural(s.total as u64, "result", "results"), s.query)
            };
            ui.label(egui::RichText::new(text).color(theme::MUTED));
            return;
        }
        if self.ex.tree.is_none() {
            ui.label(egui::RichText::new("Nothing open").color(theme::FAINT));
            return;
        }
        let crumbs = self.ex.breadcrumbs(&self.ex.current);
        let font = FontId::proportional(13.0);
        let budget = ui.available_width() - 8.0;
        let width_of = |ui: &Ui, s: &str| {
            ui.painter().layout_no_wrap(s.to_string(), font.clone(), theme::TEXT).size().x + 14.0
        };
        let sep = 14.0;

        // keep as many trailing segments as fit; the last one is always shown
        let mut used = 0.0;
        let mut first = crumbs.len();
        for (i, (label, _)) in crumbs.iter().enumerate().rev() {
            let w = width_of(ui, label) + if i + 1 < crumbs.len() { sep } else { 0.0 };
            if used + w > budget && first < crumbs.len() {
                break;
            }
            used += w;
            first = i;
        }

        let mut go: Option<NodeId> = None;
        if first > 0 {
            ui.label(egui::RichText::new("…").color(theme::FAINT));
            self.sep(ui);
        }
        for (i, (label, node)) in crumbs.iter().enumerate().skip(first) {
            let last = i + 1 == crumbs.len();
            let w = width_of(ui, label).min((budget - used).max(120.0)).max(28.0);
            let (rect, resp) = ui.allocate_exact_size(
                vec2(w.min(budget.max(60.0)), 28.0),
                if last { Sense::hover() } else { Sense::click() },
            );
            let p = ui.painter();
            if resp.hovered() && !last {
                p.rect_filled(rect, 5, theme::RAISED);
            }
            let text = super::fit_text(p, label, &font, rect.width() - 14.0);
            p.text(
                pos2(rect.left() + 7.0, rect.center().y),
                egui::Align2::LEFT_CENTER,
                text,
                font.clone(),
                if last { theme::TEXT } else { theme::MUTED },
            );
            super::focus_ring(p, rect, &resp);
            if resp.clicked() {
                go = Some(node.clone());
            }
            if !last {
                self.sep(ui);
            }
        }
        if let Some(n) = go {
            self.ex.navigate(n);
        }
    }

    fn sep(&self, ui: &mut Ui) {
        let (rect, _) = ui.allocate_exact_size(vec2(14.0, 28.0), Sense::hover());
        crate::icons::draw(
            ui.painter(),
            egui::Rect::from_center_size(rect.center(), vec2(10.0, 10.0)),
            Icon::ChevronRight,
            theme::CONTROL,
        );
    }
}
