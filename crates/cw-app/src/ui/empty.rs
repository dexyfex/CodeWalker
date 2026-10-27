use eframe::egui::{self, Frame, Margin, Ui, vec2};

use super::{Btn, Tone};
use crate::{app::App, icons::Icon, theme};

impl App {
    pub(super) fn show_empty(&mut self, ui: &mut Ui) {
        let avail = ui.available_size();
        let card_w = 520.0_f32.min(avail.x - 48.0);
        ui.add_space(((avail.y - 360.0) / 3.0).max(24.0));
        ui.vertical_centered(|ui| {
            ui.allocate_ui(vec2(card_w, 0.0), |ui| {
                ui.set_width(card_w);
                ui.spacing_mut().item_spacing = vec2(8.0, 10.0);
                ui.label(egui::RichText::new("Open your GTA V folder").size(20.0).color(theme::TEXT));
                ui.label(
                    egui::RichText::new("CodeWalker reads the game's archives in place. Nothing is changed unless you extract or edit a file.")
                        .size(13.0)
                        .color(theme::MUTED),
                );

                if let Some(b) = &self.busy {
                    ui.add_space(8.0);
                    let msg = self.progress.clone().unwrap_or_else(|| b.clone());
                    ui.label(egui::RichText::new(msg).size(13.0).color(theme::TEXT));
                    for f in [0.9, 0.6] {
                        let (r, _) = ui.allocate_exact_size(vec2(card_w * f, 12.0), egui::Sense::hover());
                        ui.painter().rect_filled(r, 3, theme::RAISED);
                    }
                    return;
                }

                let mut open: Option<std::path::PathBuf> = None;
                if !self.candidates.is_empty() {
                    ui.add_space(6.0);
                    ui.label(egui::RichText::new("Found on this computer").size(12.0).color(theme::FAINT));
                    for c in &self.candidates {
                        Frame::new()
                            .fill(theme::RAISED)
                            .stroke(egui::Stroke::new(1.0, theme::BORDER))
                            .corner_radius(6)
                            .inner_margin(Margin::symmetric(12, 10))
                            .show(ui, |ui| {
                                ui.set_width(ui.available_width());
                                ui.horizontal(|ui| {
                                    ui.vertical(|ui| {
                                        ui.label(egui::RichText::new(c.path.display().to_string()).monospace().size(12.0).color(theme::TEXT));
                                        let edition = if c.enhanced { "Enhanced Edition" } else { "Legacy" };
                                        ui.label(egui::RichText::new(format!("{} · {edition}", c.source)).size(12.0).color(theme::FAINT));
                                    });
                                    ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                                        if Btn::new("Open").tone(Tone::Secondary).show(ui).clicked() {
                                            open = Some(c.path.clone());
                                        }
                                    });
                                });
                            });
                    }
                }
                if let Some(p) = open {
                    self.run("Scanning game files…", "game.open", serde_json::json!({ "folder": p }));
                    return;
                }

                ui.add_space(6.0);
                ui.horizontal(|ui| {
                    if Btn::new("Choose game folder…").icon(Icon::FolderOpen).tone(Tone::Primary).show(ui).clicked() {
                        self.perform(crate::actions::Action::OpenGameFolder);
                    }
                    if Btn::new("Open archives or files…").tone(Tone::Secondary).show(ui).clicked() {
                        self.perform(crate::actions::Action::OpenFiles);
                    }
                });
                ui.label(egui::RichText::new("You can also drop folders, .rpf archives or individual files anywhere in this window.").size(12.0).color(theme::FAINT));
            });
        });
    }
}
