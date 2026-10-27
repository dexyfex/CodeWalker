use eframe::egui::{
    self, Align2, Area, FontId, Frame, Id, Key, Margin, Order, Sense, Stroke, TextEdit, pos2, vec2,
};

use crate::{
    actions::{Action, fuzzy_score},
    app::App,
    theme,
};

#[derive(Default)]
pub struct Palette {
    pub query: String,
    pub selected: usize,
    opened: bool,
}

impl Palette {
    fn matches(&self, app: &App) -> Vec<Action> {
        let mut scored: Vec<(usize, usize, Action)> = Action::ALL
            .iter()
            .enumerate()
            .filter(|(_, a)| app.enabled(**a))
            .filter_map(|(i, a)| fuzzy_score(&self.query, a.title()).map(|s| (s, i, *a)))
            .collect();
        scored.sort_by_key(|(s, i, _)| (*s, *i));
        scored.into_iter().map(|(_, _, a)| a).collect()
    }
}

impl App {
    pub(super) fn show_palette(&mut self, ctx: &egui::Context) {
        let Some(mut pal) = self.palette.take() else { return };
        let results = pal.matches(self);
        pal.selected = pal.selected.min(results.len().saturating_sub(1));

        let (esc, down, up, enter) = ctx.input(|i| {
            (
                i.key_pressed(Key::Escape),
                i.key_pressed(Key::ArrowDown),
                i.key_pressed(Key::ArrowUp),
                i.key_pressed(Key::Enter),
            )
        });
        if down && !results.is_empty() {
            pal.selected = (pal.selected + 1) % results.len();
        }
        if up && !results.is_empty() {
            pal.selected = (pal.selected + results.len() - 1) % results.len();
        }

        let screen = ctx.content_rect();
        let mut close = esc;
        let mut chosen: Option<Action> = None;

        // scrim: dims the app and closes on outside click
        Area::new(Id::unique("palette-scrim")).order(Order::Foreground).fixed_pos(screen.min).show(
            ctx,
            |ui| {
                let (rect, resp) = ui.allocate_exact_size(screen.size(), Sense::click());
                ui.painter().rect_filled(rect, 0.0, egui::Color32::from_black_alpha(130));
                if resp.clicked() {
                    close = true;
                }
            },
        );

        let w = 560.0_f32.min(screen.width() - 32.0);
        Area::new(Id::unique("palette"))
            .order(Order::Tooltip)
            .anchor(Align2::CENTER_TOP, vec2(0.0, 96.0))
            .show(ctx, |ui| {
                Frame::new()
                    .fill(theme::RAISED)
                    .stroke(Stroke::new(1.0, theme::BORDER))
                    .corner_radius(8)
                    .shadow(ctx.global_style().visuals.window_shadow)
                    .inner_margin(Margin::same(8))
                    .show(ui, |ui| {
                        ui.set_width(w - 16.0);
                        ui.spacing_mut().item_spacing = vec2(0.0, 4.0);
                        let edit = ui.add_sized(
                            [w - 16.0, 34.0],
                            TextEdit::singleline(&mut pal.query)
                                .hint_text("Type a command…")
                                .margin(Margin::symmetric(12, 8))
                                .font(FontId::proportional(14.0)),
                        );
                        if !pal.opened {
                            pal.opened = true;
                            edit.request_focus();
                        }
                        if edit.changed() {
                            pal.selected = 0;
                        }
                        ui.add_space(4.0);
                        if results.is_empty() {
                            ui.add_space(8.0);
                            ui.label(
                                egui::RichText::new("No command matches").color(theme::FAINT).size(13.0),
                            );
                            ui.add_space(8.0);
                        }
                        for (i, a) in results.iter().take(9).enumerate() {
                            let (rect, resp) = ui.allocate_exact_size(vec2(w - 16.0, 32.0), Sense::click());
                            let p = ui.painter();
                            if i == pal.selected {
                                p.rect_filled(rect, 5, theme::ACTIVE);
                            } else if resp.hovered() {
                                p.rect_filled(rect, 5, theme::PANEL);
                            }
                            p.text(
                                pos2(rect.left() + 12.0, rect.center().y),
                                Align2::LEFT_CENTER,
                                a.title(),
                                FontId::proportional(13.0),
                                theme::TEXT,
                            );
                            p.text(
                                pos2(rect.right() - 12.0, rect.center().y),
                                Align2::RIGHT_CENTER,
                                a.shortcut(),
                                FontId::monospace(11.5),
                                theme::FAINT,
                            );
                            if resp.hovered() && ui.input(|i| i.pointer.delta().length_sq() > 0.0) {
                                pal.selected = i;
                            }
                            if resp.clicked() {
                                chosen = Some(*a);
                            }
                        }
                    });
            });

        if enter && chosen.is_none() {
            chosen = results.get(pal.selected).copied();
        }
        if chosen.is_some() || close {
            if let Some(a) = chosen {
                self.perform(a);
            }
        } else {
            self.palette = Some(pal);
        }
    }
}
