use eframe::egui::{Align2, FontId, Sense, Ui, pos2};

use crate::{app::App, format, theme};

impl App {
    pub(super) fn show_status(&mut self, ui: &mut Ui) {
        let font = FontId::proportional(12.0);
        let rect = ui.max_rect();
        let y = rect.center().y;
        let p = ui.painter().clone();

        let left = if self.mode == crate::app::Mode::World && self.busy.is_none() {
            let w = self.engine.session();
            let n = w.world.scene.entities.len() as u64;
            let sel = w.world.scene.selection.len() as u64;
            let mut t = format::plural(n, "entity", "entities");
            if sel > 0 {
                t.push_str(&format!(" · {sel} selected"));
            }
            if w.world.is_dirty() {
                t.push_str(" · unsaved changes");
            }
            t
        } else if let Some(b) = &self.busy {
            let detail = self.progress.as_deref().map(|s| format!(" — {s}")).unwrap_or_default();
            format!("{b}{detail}")
        } else if let Some(t) = &self.ex.tree {
            let files: u64 = t.archives.iter().map(|a| a.file_count() as u64).sum();
            let shown = self.ex.listing.len() as u64;
            format!(
                "{} in view · {} in {}",
                format::plural(shown, "item", "items"),
                format::plural(files, "file", "files"),
                format::plural(t.archives.len() as u64, "archive", "archives"),
            )
        } else {
            "Nothing open".to_string()
        };
        let color = if self.busy.is_some() { theme::TEXT } else { theme::MUTED };
        p.text(pos2(rect.left() + 12.0, y), Align2::LEFT_CENTER, left, font.clone(), color);

        let mut right = rect.right() - 12.0;
        let hint = "Ctrl+K  commands";
        let g = p.text(pos2(right, y), Align2::RIGHT_CENTER, hint, font.clone(), theme::FAINT);
        right = g.left() - 16.0;

        let (text, col) = if self.keys_loaded {
            ("Keys loaded", theme::POSITIVE)
        } else if self.ex.tree.is_some() {
            ("No keys — encrypted archives are skipped", theme::WARNING)
        } else {
            ("", theme::FAINT)
        };
        if !text.is_empty() {
            let g = p.text(pos2(right, y), Align2::RIGHT_CENTER, text, font.clone(), col);
            // a dot as well as colour: colour is never the only carrier of meaning
            p.circle_filled(pos2(g.left() - 10.0, y), 3.0, col);
        }
        ui.allocate_rect(rect, Sense::hover());
    }
}
