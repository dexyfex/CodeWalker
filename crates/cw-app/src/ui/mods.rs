use eframe::egui::{self, Align2, FontId, Frame, Margin, Rect, ScrollArea, Sense, Ui, pos2, vec2};
use serde_json::json;

use super::{Btn, Tone, fit_text, hairline};
use crate::{
    actions::Action,
    app::{App, Pick},
    format,
    icons::{self, Icon},
    theme,
};

const ROW_H: f32 = 36.0;

impl App {
    pub(super) fn show_mods(&mut self, ui: &mut Ui) {
        Frame::new().inner_margin(Margin::symmetric(24, 20)).show(ui, |ui| {
            ui.set_min_size(ui.available_size());
            ui.spacing_mut().item_spacing = vec2(8.0, 8.0);

            ui.label(egui::RichText::new("Mods").size(20.0).color(theme::TEXT));
            let Some(game) = self.config_game_folder.clone() else {
                self.mods_message(
                    ui,
                    "Choose your game folder first",
                    "Mods live in a mods folder inside your GTA V folder, so CodeWalker needs to know where that is.",
                );
                if Btn::new("Choose game folder…").icon(Icon::FolderOpen).tone(Tone::Primary).show(ui).clicked() {
                    self.perform(Action::OpenGameFolder);
                }
                return;
            };
            let dir = self.mods.dir.clone().unwrap_or_else(|| game.join("mods"));
            ui.label(
                egui::RichText::new("Files in the mods folder replace the originals in your game folder. Turning one off keeps it on disk and the game uses the original again.")
                    .size(13.0)
                    .color(theme::MUTED),
            );
            ui.label(egui::RichText::new(dir.display().to_string()).monospace().size(12.0).color(theme::FAINT));
            ui.add_space(4.0);

            let mut refresh = false;
            ui.horizontal(|ui| {
                if Btn::new("Install files…").icon(Icon::Plus).tone(Tone::Secondary).tip("Copy files into the mods folder").show(ui).clicked() {
                    self.pick(Pick::InstallFiles);
                }
                if Btn::new("Install folder…")
                    .icon(Icon::FolderOpen)
                    .tone(Tone::Secondary)
                    .tip("Copy a folder laid out like the game folder")
                    .show(ui)
                    .clicked()
                {
                    self.pick(Pick::InstallFolder);
                }
                if Btn::new("Open folder").icon(Icon::Folder).tone(Tone::Ghost).enabled(self.mods.dir_exists).show(ui).clicked() {
                    self.open_folder(&dir);
                }
                if Btn::new("Refresh").tone(Tone::Ghost).show(ui).clicked() {
                    refresh = true;
                }
            });
            if refresh {
                self.refresh_mods();
            }
            ui.add_space(4.0);
            hairline(ui);

            if self.mods.rows.is_empty() {
                if self.busy.is_some() && !self.mods.loaded {
                    for f in [0.9, 0.7, 0.8] {
                        let (r, _) = ui.allocate_exact_size(vec2(ui.available_width() * f, 14.0), Sense::hover());
                        ui.painter().rect_filled(r, 3, theme::RAISED);
                    }
                } else {
                    self.mods_message(
                        ui,
                        "No mods installed",
                        "To edit an archive, select it in the Explorer and use “Copy archive to mods folder” (Ctrl+M). To add a downloaded mod, use Install.",
                    );
                    if Btn::new("Go to Explorer").tone(Tone::Secondary).show(ui).clicked() {
                        self.perform(Action::ShowExplorer);
                    }
                }
                return;
            }
            self.mods_table(ui);
        });
    }

    fn mods_message(&self, ui: &mut Ui, title: &str, body: &str) {
        ui.add_space(24.0);
        ui.label(egui::RichText::new(title).size(14.0).color(theme::TEXT));
        ui.label(egui::RichText::new(body).size(13.0).color(theme::MUTED));
        ui.add_space(4.0);
    }

    fn mods_table(&mut self, ui: &mut Ui) {
        let total = self.mods.rows.len() as u64;
        let on = self.mods.rows.iter().filter(|r| r.enabled).count() as u64;
        ui.label(
            egui::RichText::new(format!("{} · {on} on", format::plural(total, "file", "files")))
                .size(12.0)
                .color(theme::FAINT),
        );

        let font = FontId::proportional(13.0);
        let meta = FontId::proportional(12.0);
        let mut toggle: Option<(String, bool)> = None;
        let mut ask: Option<String> = None;
        let mut confirm: Option<String> = None;
        let mut cancel = false;
        let rows = self.mods.rows.clone();
        let pending = self.mods.confirm_remove.clone();

        ui.spacing_mut().item_spacing.y = 0.0;
        ScrollArea::vertical().id_salt("mods-scroll").auto_shrink([false, false]).show_rows(
            ui,
            ROW_H,
            rows.len(),
            |ui, range| {
                for i in range {
                    let m = &rows[i];
                    let (rect, resp) =
                        ui.allocate_exact_size(vec2(ui.available_width(), ROW_H), Sense::hover());
                    let p = ui.painter();
                    if resp.hovered() {
                        p.rect_filled(rect, 0.0, theme::RAISED);
                    }
                    p.hline(rect.x_range(), rect.bottom() - 0.5, egui::Stroke::new(1.0, theme::BORDER));
                    let y = rect.center().y;
                    let ic = Rect::from_min_size(pos2(rect.left() + 8.0, y - 8.0), vec2(16.0, 16.0));
                    icons::draw(
                        p,
                        ic,
                        if m.archive { Icon::Archive } else { Icon::File },
                        if m.enabled { theme::MUTED } else { theme::CONTROL },
                    );

                    // right cluster: remove | toggle | size | state
                    let mut x = rect.right() - 8.0;
                    if pending.as_deref() == Some(m.path.as_str()) {
                        let del = btn_rect(&mut x, 64.0, y);
                        let can = btn_rect(&mut x, 64.0, y);
                        p.text(
                            pos2(can.left() - 12.0, y),
                            Align2::RIGHT_CENTER,
                            "Delete this file from disk?",
                            meta.clone(),
                            theme::WARNING,
                        );
                        if mini_button(ui, del, "Delete", theme::NEGATIVE, ("del", i)) {
                            confirm = Some(m.path.clone());
                        }
                        if mini_button(ui, can, "Cancel", theme::MUTED, ("cancel", i)) {
                            cancel = true;
                        }
                        let name_w = (can.left() - 220.0 - ic.right() - 16.0).max(80.0);
                        let name = fit_text(ui.painter(), &m.path, &font, name_w);
                        ui.painter().text(
                            pos2(ic.right() + 8.0, y),
                            Align2::LEFT_CENTER,
                            name,
                            font.clone(),
                            theme::TEXT,
                        );
                        continue;
                    }
                    let rm = btn_rect(&mut x, 64.0, y);
                    let tg = btn_rect(&mut x, 74.0, y);
                    if mini_button(ui, rm, "Remove", theme::MUTED, ("rm", i)) {
                        ask = Some(m.path.clone());
                    }
                    if mini_button(
                        ui,
                        tg,
                        if m.enabled { "Turn off" } else { "Turn on" },
                        theme::TEXT,
                        ("tg", i),
                    ) {
                        toggle = Some((m.path.clone(), !m.enabled));
                    }
                    let p = ui.painter();
                    x -= 12.0;
                    let size = p.text(
                        pos2(x, y),
                        Align2::RIGHT_CENTER,
                        format::size(m.size),
                        meta.clone(),
                        theme::MUTED,
                    );
                    x = size.left() - 20.0;
                    let state = p.text(
                        pos2(x, y),
                        Align2::RIGHT_CENTER,
                        if m.enabled { "On" } else { "Off" },
                        meta.clone(),
                        if m.enabled { theme::POSITIVE } else { theme::FAINT },
                    );
                    p.circle_filled(
                        pos2(state.left() - 9.0, y),
                        3.0,
                        if m.enabled { theme::POSITIVE } else { theme::CONTROL },
                    );
                    x = state.left() - 28.0;
                    let note = if m.overrides_game { "Replaces original" } else { "New file" };
                    let n = p.text(pos2(x, y), Align2::RIGHT_CENTER, note, meta.clone(), theme::FAINT);
                    let name_w = (n.left() - ic.right() - 24.0).max(80.0);
                    let name = fit_text(ui.painter(), &m.path, &font, name_w);
                    ui.painter().text(
                        pos2(ic.right() + 8.0, y),
                        Align2::LEFT_CENTER,
                        name,
                        font.clone(),
                        if m.enabled { theme::TEXT } else { theme::FAINT },
                    );
                }
            },
        );

        if let Some(path) = ask {
            self.mods.confirm_remove = Some(path);
        }
        if cancel {
            self.mods.confirm_remove = None;
        }
        if let Some(path) = confirm {
            self.mods.confirm_remove = None;
            self.run("Removing…", "mods.remove", json!({ "path": path }));
        }
        if let Some((path, enable)) = toggle {
            let cmd = if enable { "mods.enable" } else { "mods.disable" };
            self.run(if enable { "Turning on…" } else { "Turning off…" }, cmd, json!({ "path": path }));
        }
    }
}

/// Carves a `w`-wide button slot off the right edge, advancing `x` leftwards.
fn btn_rect(x: &mut f32, w: f32, y: f32) -> Rect {
    let r = Rect::from_min_size(pos2(*x - w, y - 12.0), vec2(w, 24.0));
    *x -= w + 4.0;
    r
}

fn mini_button(ui: &mut Ui, rect: Rect, label: &str, color: egui::Color32, id: (&str, usize)) -> bool {
    let resp = ui.interact(rect, ui.scope_id().with(id), Sense::click());
    let p = ui.painter();
    if resp.is_pointer_button_down_on() {
        p.rect_filled(rect, 5, theme::ACTIVE);
    } else if resp.hovered() {
        p.rect_filled(rect, 5, theme::ACTIVE);
        p.rect_stroke(rect, 5, egui::Stroke::new(1.0, theme::CONTROL), egui::StrokeKind::Inside);
    }
    p.text(rect.center(), Align2::CENTER_CENTER, label, FontId::proportional(12.0), color);
    super::focus_ring(p, rect, &resp);
    resp.clicked()
}
