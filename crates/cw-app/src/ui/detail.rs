use cw_core::filekind::FileKind;
use eframe::egui::{self, Align2, FontId, Frame, Margin, Rect, Sense, Ui, pos2, vec2};

use super::{Btn, Tone, fit_text, segmented};
use crate::{
    actions::Action,
    app::{App, PState, Target, ViewMode},
    format,
    icons::{self, Icon},
    model::Item,
    preview, theme,
};

impl App {
    pub(super) fn show_detail(&mut self, ui: &mut Ui) {
        Frame::new().inner_margin(Margin::symmetric(16, 12)).show(ui, |ui| {
            ui.set_min_size(ui.available_size());
            ui.spacing_mut().item_spacing = vec2(8.0, 8.0);

            // Folders and archives are summarised, never loaded.
            if let Some(item) = self.ex.selected.clone()
                && self.is_container_item(&item)
            {
                self.show_container_summary(ui, &item);
                return;
            }
            if self.preview.target.is_none() {
                ui.add_space(24.0);
                ui.vertical_centered(|ui| {
                    ui.label(egui::RichText::new("No file selected").color(theme::TEXT).size(14.0));
                    ui.add_space(2.0);
                    ui.label(
                        egui::RichText::new(
                            "Select a file to see its contents, or press Ctrl+K for commands.",
                        )
                        .color(theme::FAINT)
                        .size(12.0),
                    );
                });
                return;
            }
            self.show_file_detail(ui);
        });
    }

    fn is_container_item(&self, item: &Item) -> bool {
        match item {
            Item::Folder(_) => true,
            Item::Entry(r) => self.ex.tree.as_ref().is_some_and(|t| {
                let e = t.entry(*r);
                e.is_dir() || e.child_archive.is_some()
            }),
        }
    }

    fn show_container_summary(&mut self, ui: &mut Ui, item: &Item) {
        let (name, path, kind) = match (item, &self.ex.tree) {
            (Item::Folder(p), _) => (p.rsplit('/').next().unwrap_or(p).to_string(), p.clone(), "Folder"),
            (Item::Entry(r), Some(t)) => {
                let e = t.entry(*r);
                (
                    e.name.clone(),
                    e.path.clone(),
                    if r.index == 0 || e.child_archive.is_some() { "Archive" } else { "Folder" },
                )
            }
            _ => return,
        };
        self.header(ui, &name, &path, if kind == "Archive" { Icon::Archive } else { Icon::Folder });
        let mut act = None;
        ui.horizontal(|ui| {
            if Btn::new("Open").icon(Icon::FolderOpen).tone(Tone::Secondary).show(ui).clicked() {
                act = Some(true);
            }
            if Btn::new("Extract")
                .icon(Icon::Extract)
                .tip("Extract everything inside (Ctrl+E)")
                .show(ui)
                .clicked()
            {
                self.perform(Action::ExtractSelected);
            }
        });
        if act.is_some() {
            self.ex.open_item(item);
        }
        super::hairline(ui);
        let mut rows = vec![("Type", kind.to_string())];
        if let Some(n) = self.ex.child_count(item) {
            rows.push(("Contains", format::plural(n as u64, "item", "items")));
        }
        if let (Item::Entry(r), Some(t)) = (item, &self.ex.tree) {
            let root = if r.index == 0 {
                *r
            } else {
                cw_core::EntryRef {
                    archive: t.entry(*r).child_archive.unwrap_or(r.archive),
                    index: if t.entry(*r).child_archive.is_some() { 0 } else { r.index },
                }
            };
            let a = t.archive(root.archive);
            if root.index == 0 {
                rows.push(("Encryption", a.encryption.label().to_string()));
                rows.push(("Entries", format::thousands(a.entries.len() as u64)));
                rows.push(("Size on disk", format::size(a.size)));
            }
        }
        kv(ui, &rows);
    }

    fn header(&self, ui: &mut Ui, name: &str, path: &str, icon: Icon) {
        let w = ui.available_width();
        let (rect, resp) = ui.allocate_exact_size(vec2(w, 40.0), Sense::hover());
        let p = ui.painter();
        icons::draw(
            p,
            Rect::from_min_size(pos2(rect.left(), rect.top() + 2.0), vec2(20.0, 20.0)),
            icon,
            theme::MUTED,
        );
        let title = fit_text(p, name, &FontId::proportional(15.0), w - 32.0);
        p.text(
            pos2(rect.left() + 30.0, rect.top() + 12.0),
            Align2::LEFT_CENTER,
            title,
            FontId::proportional(15.0),
            theme::TEXT,
        );
        let sub = fit_text(p, path, &FontId::proportional(12.0), w - 30.0);
        p.text(
            pos2(rect.left() + 30.0, rect.top() + 31.0),
            Align2::LEFT_CENTER,
            sub,
            FontId::proportional(12.0),
            theme::FAINT,
        );
        if name.chars().count() > 28 || path.chars().count() > 40 {
            resp.on_hover_text(format!("{name}\n{path}"));
        }
    }

    fn show_file_detail(&mut self, ui: &mut Ui) {
        let mut action = None;
        let mut retry = false;
        let mut new_view = None;

        match &self.preview.state {
            PState::Idle => {}
            PState::Loading => {
                let name = self.target_name();
                self.header(ui, &name, "Loading…", Icon::File);
                skeleton(ui);
            }
            PState::Failed(msg) => {
                let name = self.target_name();
                self.header(ui, &name, "", Icon::File);
                super::hairline(ui);
                ui.label(
                    egui::RichText::new("This file could not be opened").color(theme::NEGATIVE).size(13.0),
                );
                ui.label(egui::RichText::new(msg).color(theme::MUTED).size(12.0));
                if Btn::new("Try again").show(ui).clicked() {
                    retry = true;
                }
            }
            PState::Ready(l) => {
                self.header(ui, &l.name, &l.path, icons::for_kind(l.kind));
                ui.horizontal(|ui| {
                    if Btn::new("Extract")
                        .icon(Icon::Extract)
                        .tip("Save to disk (Ctrl+E)")
                        .enabled(self.ex.selected.is_some())
                        .show(ui)
                        .clicked()
                    {
                        action = Some(Action::ExtractSelected);
                    }
                    if Btn::new("Copy path").icon(Icon::Copy).tip("Ctrl+Shift+C").show(ui).clicked() {
                        action = Some(Action::CopyPath);
                    }
                });
                let has_preview = l.image.is_some() || l.text.is_some();
                let labels: &[&str] =
                    if has_preview { &["Preview", "Hex", "Info"] } else { &["Info", "Hex"] };
                let selected = match (self.preview.view, has_preview) {
                    (ViewMode::Auto, true) => 0,
                    (ViewMode::Auto, false) => 0,
                    (ViewMode::Hex, true) => 1,
                    (ViewMode::Hex, false) => 1,
                    (ViewMode::Info, true) => 2,
                    (ViewMode::Info, false) => 0,
                };
                if let Some(i) = segmented(ui, labels, selected) {
                    new_view = Some(match (labels[i], has_preview) {
                        ("Preview", _) => ViewMode::Auto,
                        ("Hex", _) => ViewMode::Hex,
                        _ => ViewMode::Info,
                    });
                }
                super::hairline(ui);

                let id = l.path.clone();
                let effective = match (self.preview.view, has_preview) {
                    (ViewMode::Hex, _) => ViewMode::Hex,
                    (ViewMode::Info, _) | (ViewMode::Auto, false) => ViewMode::Info,
                    (ViewMode::Auto, true) => ViewMode::Auto,
                };
                match effective {
                    ViewMode::Hex => preview::hex_view(ui, &l.bytes, &format!("hex-{id}")),
                    ViewMode::Info => {
                        let mut rows = preview::info_rows(l);
                        if let Some(note) = viewer_note(l.kind, !l.bytes.is_empty()) {
                            rows.push(("Viewer", note.to_string()));
                        }
                        kv(ui, &rows);
                    }
                    ViewMode::Auto => {
                        if let (Some((tex, orig)), true) = (&self.preview.tex, l.image.is_some()) {
                            preview::image_view(ui, tex, *orig);
                        } else if let Some(doc) = &l.text {
                            preview::text_view(ui, doc, &format!("text-{id}"));
                        }
                    }
                }
            }
        }

        if let Some(v) = new_view {
            self.preview.view = v;
        }
        if let Some(a) = action {
            self.perform(a);
        }
        if retry {
            self.preview.target = None;
        }
    }

    fn target_name(&self) -> String {
        match (&self.preview.target, &self.ex.tree) {
            (Some(Target::Entry(r)), Some(t)) => t.entry(*r).name.clone(),
            (Some(Target::Disk(p)), _) => {
                p.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default()
            }
            _ => String::new(),
        }
    }
}

/// Honest note for formats that only have a raw view so far.
fn viewer_note(kind: FileKind, has_data: bool) -> Option<&'static str> {
    if !has_data {
        return Some("Empty file");
    }
    match kind {
        FileKind::Xml | FileKind::Text | FileKind::Image => None,
        k if k.is_resource() => Some("Hex only — no viewer yet"),
        _ => None,
    }
}

fn kv(ui: &mut Ui, rows: &[(&str, String)]) {
    ui.add_space(4.0);
    for (k, v) in rows {
        let w = ui.available_width();
        let (rect, resp) = ui.allocate_exact_size(vec2(w, 24.0), Sense::hover());
        let p = ui.painter();
        p.text(
            pos2(rect.left(), rect.center().y),
            Align2::LEFT_CENTER,
            k,
            FontId::proportional(12.0),
            theme::FAINT,
        );
        let text = fit_text(p, v, &FontId::proportional(13.0), w - 128.0);
        p.text(
            pos2(rect.left() + 120.0, rect.center().y),
            Align2::LEFT_CENTER,
            &text,
            FontId::proportional(13.0),
            theme::TEXT,
        );
        if &text != v {
            resp.on_hover_text(v);
        }
    }
}

/// Flat grey bars at the layout's real size; no shimmer (docs/DESIGN.md).
fn skeleton(ui: &mut Ui) {
    super::hairline(ui);
    for f in [0.92, 0.7, 0.8, 0.5] {
        let (rect, _) = ui.allocate_exact_size(vec2(ui.available_width() * f, 12.0), Sense::hover());
        ui.painter().rect_filled(rect, 3, theme::RAISED);
    }
}
