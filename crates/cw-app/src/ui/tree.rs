use eframe::egui::{self, Align2, FontId, ScrollArea, Sense, Ui, pos2, vec2};

use super::fit_text;
use crate::{
    app::App,
    icons::{self, Icon},
    model::{NodeId, RowKind},
    theme,
};

const INDENT: f32 = 14.0;

impl App {
    pub(super) fn show_tree(&mut self, ui: &mut Ui) {
        let loose_rows = self.loose.len().min(5);
        let loose_h = if self.loose.is_empty() { 0.0 } else { 52.0 + loose_rows as f32 * theme::ROW_H + 8.0 };
        let tree_h = (ui.available_height() - loose_h).max(80.0);
        let width = ui.available_width();

        ui.allocate_ui(vec2(width, tree_h), |ui| {
            ui.spacing_mut().item_spacing.y = 0.0;
            if self.ex.rows.is_empty() {
                ui.add_space(12.0);
                ui.label(egui::RichText::new("No archives open").color(theme::FAINT).size(12.0));
                return;
            }
            let mut area = ScrollArea::vertical().id_salt("tree-scroll").auto_shrink([false, false]);
            if let Some(row) = self.tree_scroll_to.take() {
                let y = row as f32 * theme::ROW_H;
                if y < self.tree_offset {
                    area = area.vertical_scroll_offset(y);
                } else if y + theme::ROW_H > self.tree_offset + tree_h {
                    area = area.vertical_scroll_offset(y + theme::ROW_H - tree_h);
                }
            }

            let font = FontId::proportional(13.0);
            let mut click: Option<NodeId> = None;
            let mut toggle: Option<NodeId> = None;
            let total = self.ex.rows.len();
            let out = area.show_rows(ui, theme::ROW_H, total, |ui, range| {
                for i in range {
                    let row = &self.ex.rows[i];
                    let current = row.node == self.ex.current && self.ex.search.is_none();
                    let (rect, resp) =
                        ui.allocate_exact_size(vec2(ui.available_width(), theme::ROW_H), Sense::click());
                    let p = ui.painter();
                    if current {
                        p.rect_filled(rect, 0.0, theme::ACTIVE);
                        p.rect_filled(
                            egui::Rect::from_min_size(rect.min, vec2(2.0, rect.height())),
                            0.0,
                            theme::ACCENT,
                        );
                    } else if resp.hovered() {
                        p.rect_filled(rect, 0.0, theme::RAISED);
                    }

                    let x0 = rect.left() + 8.0 + row.depth as f32 * INDENT;
                    let chev = egui::Rect::from_min_size(pos2(x0, rect.center().y - 8.0), vec2(16.0, 16.0));
                    if row.expandable {
                        let cr =
                            ui.interact(chev.expand(4.0), ui.scope_id().with(("chev", i)), Sense::click());
                        let col = if cr.hovered() { theme::TEXT } else { theme::CONTROL };
                        let icon = if row.expanded { Icon::ChevronDown } else { Icon::ChevronRight };
                        icons::draw(ui.painter(), chev.shrink(2.0), icon, col);
                        if cr.clicked() {
                            toggle = Some(row.node.clone());
                        }
                    }
                    let icon = match row.kind {
                        RowKind::Archive => Icon::Archive,
                        RowKind::Folder | RowKind::Dir if row.expanded => Icon::FolderOpen,
                        _ => Icon::Folder,
                    };
                    let ic =
                        egui::Rect::from_min_size(pos2(x0 + 20.0, rect.center().y - 8.0), vec2(16.0, 16.0));
                    let p = ui.painter();
                    icons::draw(p, ic, icon, if current { theme::TEXT } else { theme::CONTROL });
                    let tx = ic.right() + 8.0;
                    let label = fit_text(p, &row.label, &font, rect.right() - tx - 8.0);
                    p.text(
                        pos2(tx, rect.center().y),
                        Align2::LEFT_CENTER,
                        label,
                        font.clone(),
                        if current { theme::TEXT } else { theme::MUTED },
                    );
                    super::focus_ring(p, rect.shrink(1.0), &resp);

                    if resp.clicked() && toggle.is_none() {
                        click = Some(row.node.clone());
                    }
                    if resp.double_clicked() && row.expandable {
                        toggle = Some(row.node.clone());
                    }
                    if row.label.chars().count() > 24 {
                        resp.on_hover_text(&row.label);
                    }
                }
            });
            self.tree_offset = out.state.clamped_offset().y;
            if let Some(n) = toggle {
                self.ex.toggle(&n);
            } else if let Some(n) = click {
                self.ex.navigate(n);
            }
        });

        if !self.loose.is_empty() {
            self.show_loose_files(ui);
        }
    }

    fn show_loose_files(&mut self, ui: &mut Ui) {
        super::hairline(ui);
        ui.add_space(8.0);
        ui.horizontal(|ui| {
            ui.add_space(12.0);
            ui.label(egui::RichText::new("Opened files").color(theme::FAINT).size(12.0));
        });
        ui.add_space(4.0);
        ui.spacing_mut().item_spacing.y = 0.0;
        let font = FontId::proportional(13.0);
        let mut select = None;
        let mut close = None;
        // up to five rows are visible; any more scroll instead of being hidden
        ScrollArea::vertical()
            .id_salt("loose-files")
            .max_height(ui.available_height().min(5.0 * theme::ROW_H))
            .auto_shrink([false, true])
            .show(ui, |ui| {
                for path in self.loose.clone().iter() {
                    let name = path.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default();
                    let active =
                        matches!(&self.preview.target, Some(crate::app::Target::Disk(p)) if p == path);
                    let (rect, resp) =
                        ui.allocate_exact_size(vec2(ui.available_width(), theme::ROW_H), Sense::click());
                    let p = ui.painter();
                    if active {
                        p.rect_filled(rect, 0.0, theme::ACTIVE);
                        p.rect_filled(
                            egui::Rect::from_min_size(rect.min, vec2(2.0, rect.height())),
                            0.0,
                            theme::ACCENT,
                        );
                    } else if resp.hovered() {
                        p.rect_filled(rect, 0.0, theme::RAISED);
                    }
                    let kind = cw_core::filekind::FileKind::from_name(&name.to_lowercase());
                    let ic = egui::Rect::from_min_size(
                        pos2(rect.left() + 12.0, rect.center().y - 8.0),
                        vec2(16.0, 16.0),
                    );
                    icons::draw(
                        p,
                        ic,
                        icons::for_kind(kind),
                        if active { theme::TEXT } else { theme::CONTROL },
                    );
                    let label = fit_text(p, &name, &font, rect.width() - 80.0);
                    p.text(
                        pos2(ic.right() + 8.0, rect.center().y),
                        Align2::LEFT_CENTER,
                        label,
                        font.clone(),
                        if active { theme::TEXT } else { theme::MUTED },
                    );
                    if resp.hovered() || active {
                        let x = egui::Rect::from_center_size(
                            pos2(rect.right() - 18.0, rect.center().y),
                            vec2(20.0, 20.0),
                        );
                        let xr = ui.interact(x, ui.scope_id().with(("close", &name)), Sense::click());
                        icons::draw(
                            ui.painter(),
                            x.shrink(4.0),
                            Icon::Close,
                            if xr.hovered() { theme::TEXT } else { theme::CONTROL },
                        );
                        if xr.clicked() {
                            close = Some(path.clone());
                        }
                        xr.on_hover_text("Close file");
                    }
                    if resp.clicked() && close.is_none() {
                        select = Some(path.clone());
                    }
                    resp.on_hover_text(path.display().to_string());
                }
            });
        if let Some(p) = select {
            self.select_loose(p);
        }
        if let Some(p) = close {
            self.run("Closing file…", "file.close", serde_json::json!({ "path": p }));
        }
    }
}
