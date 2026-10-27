use cw_core::filekind::FileKind;
use eframe::egui::{self, Align2, FontId, Key, Rect, ScrollArea, Sense, Ui, pos2, vec2};

use super::fit_text;
use crate::{
    actions::Action,
    app::App,
    format,
    icons::{self, Icon},
    model::{Item, SortKey},
    theme,
};

const HEADER_H: f32 = 28.0;
const PAD: f32 = 12.0;

struct Cols {
    name_right: f32,
    type_x: Option<f32>,
    size_right: f32,
    packed_right: Option<f32>,
}

fn columns(rect: Rect, searching: bool) -> Cols {
    let wide = rect.width() >= 640.0;
    let mid = rect.width() >= 460.0;
    let packed_right = wide.then_some(rect.right() - PAD);
    let size_right = rect.right() - PAD - if wide { 96.0 } else { 0.0 };
    let type_w = if searching { 220.0 } else { 150.0 };
    let type_x = mid.then_some(size_right - 96.0 - type_w);
    let name_right = type_x.unwrap_or(size_right - 80.0) - 12.0;
    Cols { name_right, type_x, size_right, packed_right }
}

impl App {
    pub(super) fn show_list(&mut self, ui: &mut Ui) {
        let full = ui.available_rect_before_wrap();
        let searching = self.ex.search.is_some();
        let cols = columns(full, searching);
        self.list_header(ui, &cols, searching);
        super::hairline(ui);

        if self.ex.listing.is_empty() {
            self.list_empty_state(ui);
            return;
        }

        ui.spacing_mut().item_spacing.y = 0.0;
        let rows = self.ex.listing.len();
        let view_h = ui.available_height();
        let mut area = ScrollArea::vertical().id_salt("list-scroll").auto_shrink([false, false]);
        if let Some(i) = self.list_scroll_to.take() {
            let y = i as f32 * theme::ROW_H;
            if y < self.list_offset {
                area = area.vertical_scroll_offset(y);
            } else if y + theme::ROW_H > self.list_offset + view_h {
                area = area.vertical_scroll_offset(y + theme::ROW_H - view_h);
            }
        }

        let name_font = FontId::proportional(13.0);
        let meta_font = FontId::proportional(12.0);
        let mut open: Option<Item> = None;
        let mut select: Option<Item> = None;
        let mut extract: Option<Item> = None;
        let mut copy: Option<Item> = None;
        let mut to_mods = false;
        let mut reveal: Option<cw_core::EntryRef> = None;

        let out = area.show_rows(ui, theme::ROW_H, rows, |ui, range| {
            for i in range {
                let item = self.ex.listing[i].clone();
                let selected = self.ex.selected.as_ref() == Some(&item);
                let (rect, resp) = ui.allocate_exact_size(vec2(ui.available_width(), theme::ROW_H), Sense::click());
                let p = ui.painter();
                if selected {
                    p.rect_filled(rect, 0.0, theme::ACTIVE);
                    p.rect_filled(Rect::from_min_size(rect.min, vec2(2.0, rect.height())), 0.0, theme::ACCENT);
                } else if resp.hovered() {
                    p.rect_filled(rect, 0.0, theme::RAISED);
                }

                let (name, kind, size, packed, is_container, path) = self.describe(&item);
                let y = rect.center().y;
                let ic = Rect::from_min_size(pos2(rect.left() + PAD, y - 8.0), vec2(16.0, 16.0));
                let icon = if is_container {
                    if matches!(&item, Item::Entry(r) if r.index == 0 || self.ex.tree.as_ref().is_some_and(|t| t.entry(*r).child_archive.is_some())) {
                        Icon::Archive
                    } else {
                        Icon::Folder
                    }
                } else {
                    icons::for_kind(FileKind::from_name(&name.to_lowercase()))
                };
                icons::draw(p, ic, icon, if selected { theme::TEXT } else { theme::CONTROL });
                let nx = ic.right() + 8.0;
                let label = fit_text(p, &name, &name_font, cols_name_w(&cols_for(rect, searching), nx));
                p.text(pos2(nx, y), Align2::LEFT_CENTER, label, name_font.clone(), theme::TEXT);

                let c = cols_for(rect, searching);
                if let Some(tx) = c.type_x {
                    let text = if searching { fit_text(p, &parent_path(&path), &meta_font, 210.0) } else { kind.to_string() };
                    p.text(pos2(tx, y), Align2::LEFT_CENTER, text, meta_font.clone(), theme::FAINT);
                }
                let count = self.ex.child_count(&item);
                let size_text = match (is_container, count) {
                    (true, Some(n)) => format::plural(n as u64, "item", "items"),
                    (true, None) => "—".into(),
                    _ => format::size(size),
                };
                p.text(pos2(c.size_right, y), Align2::RIGHT_CENTER, size_text, meta_font.clone(), theme::MUTED);
                if let Some(px) = c.packed_right {
                    let t = if is_container { "—".to_string() } else { format::size(packed) };
                    p.text(pos2(px, y), Align2::RIGHT_CENTER, t, meta_font.clone(), theme::FAINT);
                }
                super::focus_ring(p, rect.shrink(1.0), &resp);

                if resp.clicked() {
                    select = Some(item.clone());
                }
                if resp.double_clicked() {
                    open = Some(item.clone());
                }
                resp.context_menu(|ui| {
                    if ui.button("Open").clicked() {
                        open = Some(item.clone());
                        ui.close();
                    }
                    if ui.button("Extract…").clicked() {
                        select = Some(item.clone());
                        extract = Some(item.clone());
                        ui.close();
                    }
                    if ui.add_enabled(self.mods_copy_source().is_some(), egui::Button::new("Copy to mods folder")).clicked() {
                        select = Some(item.clone());
                        to_mods = true;
                        ui.close();
                    }
                    if ui.button("Copy path").clicked() {
                        copy = Some(item.clone());
                        ui.close();
                    }
                    if searching && let Item::Entry(r) = &item && ui.button("Show in folder").clicked() {
                        reveal = Some(*r);
                        ui.close();
                    }
                });
            }
        });
        self.list_offset = out.state.clamped_offset().y;

        if let Some(i) = select {
            self.ex.selected = Some(i);
        }
        if let Some(i) = open {
            if let Item::Entry(r) = &i
                && searching
                && self.ex.container_of(*r).is_none()
            {
                self.ex.reveal(*r);
            } else {
                self.ex.open_item(&i);
            }
        }
        if extract.is_some() {
            self.perform(Action::ExtractSelected);
        }
        if to_mods {
            self.pending_mods_copy = true;
        }
        if let Some(i) = copy {
            let text = self.ex.item_path(&i);
            self.ctx.copy_text(text);
            self.toast("Path copied", crate::ui::toasts::ToastKind::Success);
        }
        if let Some(r) = reveal {
            self.ex.reveal(r);
        }
        self.list_keyboard(ui);
    }

    fn list_keyboard(&mut self, ui: &mut Ui) {
        let ctx = ui.ctx();
        if ctx.egui_wants_keyboard_input() || self.palette.is_some() {
            return;
        }
        let page = ((ui.min_rect().height() / theme::ROW_H) as isize - 1).max(1);
        let (mut delta, mut enter, mut home, mut end) = (0isize, false, false, false);
        ctx.input(|i| {
            if i.key_pressed(Key::ArrowDown) {
                delta += 1;
            }
            if i.key_pressed(Key::ArrowUp) {
                delta -= 1;
            }
            if i.key_pressed(Key::PageDown) {
                delta += page;
            }
            if i.key_pressed(Key::PageUp) {
                delta -= page;
            }
            enter = i.key_pressed(Key::Enter);
            home = i.key_pressed(Key::Home);
            end = i.key_pressed(Key::End);
        });
        if home {
            delta = isize::MIN / 2;
        }
        if end {
            delta = isize::MAX / 2;
        }
        if delta != 0 {
            self.ex.move_selection(delta.clamp(-(1 << 40), 1 << 40));
            self.list_scroll_to = self.ex.selected_index();
        }
        if enter && let Some(item) = self.ex.selected.clone() {
            if let Item::Entry(r) = &item
                && self.ex.search.is_some()
                && self.ex.container_of(*r).is_none()
            {
                self.ex.reveal(*r);
            } else {
                self.ex.open_item(&item);
            }
        }
    }

    /// `(name, type label, size, stored size, is container, path)` of a row.
    fn describe(&self, item: &Item) -> (String, &'static str, u64, u64, bool, String) {
        match (item, &self.ex.tree) {
            (Item::Folder(p), _) => {
                (p.rsplit('/').next().unwrap_or(p).to_string(), "Folder", 0, 0, true, p.clone())
            }
            (Item::Entry(r), Some(t)) => {
                let e = t.entry(*r);
                let container = e.is_dir() || e.child_archive.is_some();
                let kind = if r.index == 0 || e.child_archive.is_some() {
                    "Archive"
                } else if e.is_dir() {
                    "Folder"
                } else {
                    FileKind::from_name(&e.name_lower).label()
                };
                (e.name.clone(), kind, e.display_size(), e.stored_size(), container, e.path.clone())
            }
            _ => (String::new(), "", 0, 0, false, String::new()),
        }
    }

    fn list_header(&mut self, ui: &mut Ui, cols: &Cols, searching: bool) {
        let (rect, _) = ui.allocate_exact_size(vec2(ui.available_width(), HEADER_H), Sense::hover());
        let font = FontId::proportional(12.0);
        let mut set: Option<SortKey> = None;
        let mut cell = |ui: &mut Ui, key: Option<SortKey>, label: &str, x: f32, w: f32, right: bool| {
            let r = Rect::from_min_size(pos2(if right { x - w } else { x }, rect.top()), vec2(w, HEADER_H));
            let resp = ui.interact(
                r,
                ui.scope_id().with(("hdr", label)),
                if key.is_some() { Sense::click() } else { Sense::hover() },
            );
            let active = key == Some(self.ex.sort);
            let color = if active {
                theme::TEXT
            } else if resp.hovered() && key.is_some() {
                theme::MUTED
            } else {
                theme::FAINT
            };
            let p = ui.painter();
            let (align, tx) = if right {
                (Align2::RIGHT_CENTER, x - if active { 16.0 } else { 0.0 })
            } else {
                (Align2::LEFT_CENTER, x)
            };
            let galley = p.text(pos2(tx, rect.center().y), align, label, font.clone(), color);
            if active {
                let ix = if right { galley.left() - 14.0 } else { galley.right() + 4.0 };
                let icon = if self.ex.descending { Icon::ChevronDown } else { Icon::Up };
                let ir = Rect::from_min_size(pos2(ix, rect.center().y - 5.0), vec2(10.0, 10.0));
                icons::draw(p, ir, if self.ex.descending { Icon::ChevronDown } else { icon }, theme::MUTED);
            }
            super::focus_ring(p, r.shrink(1.0), &resp);
            if resp.clicked()
                && let Some(k) = key
            {
                set = Some(k);
            }
        };
        cell(ui, Some(SortKey::Name), "Name", rect.left() + PAD + 24.0, 120.0, false);
        if let Some(tx) = cols.type_x {
            cell(
                ui,
                if searching { None } else { Some(SortKey::Type) },
                if searching { "Location" } else { "Type" },
                tx,
                120.0,
                false,
            );
        }
        cell(ui, Some(SortKey::Size), "Size", cols.size_right, 90.0, true);
        if let Some(px) = cols.packed_right {
            cell(ui, Some(SortKey::Packed), "Stored", px, 90.0, true);
        }
        if let Some(k) = set {
            self.ex.set_sort(k);
        }
    }

    fn list_empty_state(&mut self, ui: &mut Ui) {
        ui.add_space(48.0);
        ui.vertical_centered(|ui| {
            let (title, hint) = if let Some(s) = &self.ex.search {
                (
                    format!("Nothing found for “{}”", s.query),
                    "Check the spelling, or search for part of the name.",
                )
            } else if !self.ex.filter.is_empty() {
                (
                    format!("No matches for “{}” in this folder", self.ex.filter),
                    "Search every archive instead, or clear the filter.",
                )
            } else {
                ("This folder is empty".to_string(), "Archives and folders you open appear here.")
            };
            ui.label(egui::RichText::new(title).color(theme::TEXT).size(14.0));
            ui.add_space(4.0);
            ui.label(egui::RichText::new(hint).color(theme::FAINT).size(12.0));
            if self.ex.search.is_none() && !self.ex.filter.is_empty() {
                ui.add_space(12.0);
                if super::Btn::new("Search all archives").icon(Icon::Search).show(ui).clicked() {
                    self.search_everywhere = true;
                    self.search_changed();
                }
            }
        });
    }
}

fn cols_for(rect: Rect, searching: bool) -> Cols {
    columns(rect, searching)
}

fn cols_name_w(c: &Cols, name_x: f32) -> f32 {
    (c.name_right - name_x).max(40.0)
}

fn parent_path(path: &str) -> String {
    path.rsplit_once('/').map_or(String::new(), |(d, _)| d.to_string())
}
