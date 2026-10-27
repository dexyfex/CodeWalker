//! The map viewport: scene list, 3D view and inspector. Every change goes
//! through the `world.*` commands; the only local state is a live drag
//! preview, which becomes one `world.set` (one undo step) when released.

use cw_gpu::{Camera, Globals, GpuScene, Renderer, Target, grid_spacing};
use cw_world::{Entity, EntityId, Transform, Vec3};
use eframe::{
    egui::{
        self, Align2, Color32, CursorIcon, FontId, Frame, Key, Margin, Panel, PointerButton, Rect,
        ScrollArea, Sense, TextEdit, TextureId, Ui, pos2, vec2,
    },
    egui_wgpu,
};
use serde_json::json;

use super::{Btn, Tone, fit_text, hairline};
use crate::{
    actions::Action,
    app::App,
    format,
    gpu::Gpu,
    icons::{self, Icon},
    theme,
    world_view::{self, Axis},
};

struct WorldGpu {
    shared: Gpu,
    renderer: Renderer,
    target: Option<Target>,
    texture: Option<TextureId>,
}

impl WorldGpu {
    fn new(rs: &egui_wgpu::RenderState) -> Self {
        Self { shared: Gpu::new(rs), renderer: Renderer::new(&rs.device), target: None, texture: None }
    }

    /// Makes sure the offscreen target matches the viewport size (in pixels).
    fn ensure_target(&mut self, w: u32, h: u32) {
        let (w, h) = (w.max(1), h.max(1));
        if self.target.as_ref().is_some_and(|t| t.size == (w, h)) {
            return;
        }
        let target = Target::new(&self.shared.device, w, h);
        self.shared.show(&mut self.texture, &target.view);
        self.target = Some(target);
    }
}

struct GizmoDrag {
    id: EntityId,
    axis: Axis,
    start: Transform,
    t0: f32,
}

pub struct WorldUi {
    gpu: Option<WorldGpu>,
    pub camera: Camera,
    hover_axis: Option<Axis>,
    drag: Option<GizmoDrag>,
    /// Live position of the entity being dragged (not in the world until released).
    preview: Option<(EntityId, Transform)>,
    built: Option<(u64, Option<(EntityId, Transform)>)>,
    instances: Vec<cw_gpu::BoxInstance>,
    outlines: Vec<cw_gpu::LineVertex>,
    list_filter: String,
    pub framed: bool,
    last_viewport: Rect,
    /// Inspector edit in progress: committed when the field is released.
    pending: Option<(EntityId, Transform)>,
    rename_buf: Option<(EntityId, String)>,
}

impl WorldUi {
    pub fn new(rs: Option<&egui_wgpu::RenderState>) -> Self {
        Self {
            gpu: rs.map(WorldGpu::new),
            camera: Camera::default(),
            hover_axis: None,
            drag: None,
            preview: None,
            built: None,
            instances: Vec::new(),
            outlines: Vec::new(),
            list_filter: String::new(),
            framed: false,
            last_viewport: Rect::from_min_size(pos2(0.0, 0.0), vec2(1280.0, 720.0)),
            pending: None,
            rename_buf: None,
        }
    }
}

impl App {
    fn world_cmd(&mut self, name: &'static str, args: serde_json::Value) -> Option<serde_json::Value> {
        match self.engine.execute(name, args) {
            Ok(v) => Some(v),
            Err(e) => {
                self.toast(e.message, super::toasts::ToastKind::Error);
                None
            }
        }
    }

    fn selected_entities(&self) -> Vec<Entity> {
        let s = self.engine.session();
        let sc = &s.world.scene;
        sc.entities.iter().filter(|e| sc.selection.contains(&e.id)).cloned().collect()
    }

    /// A point in front of the camera on the ground, or 20 m ahead if looking up.
    fn camera_focus_point(&self) -> Vec3 {
        let cam = &self.world.camera;
        let ray = cam.ray(0.0, 0.0, 16.0 / 9.0);
        match cw_gpu::pick::ray_plane(&ray, Vec3::ZERO, Vec3::Z) {
            Some(t) if t < 800.0 => ray.at(t),
            _ => cam.position + cam.forward() * 20.0,
        }
    }

    pub fn world_add_entity(&mut self) {
        let p = self.camera_focus_point();
        let n = self.engine.session().world.scene.entities.len() + 1;
        if let Some(v) = self.world_cmd(
            "world.add",
            json!({ "archetype": format!("new_entity_{n:02}"), "x": p.x, "y": p.y, "z": p.z + 0.5 }),
        ) && let Some(id) = v["id"].as_u64()
        {
            self.world_cmd("world.select", json!({ "id": id }));
        }
    }

    pub fn world_frame(&mut self) {
        let sel = self.selected_entities();
        let s = self.engine.session();
        let pts: Vec<Vec3> = if sel.is_empty() {
            s.world.scene.entities.iter().map(|e| e.transform.position).collect()
        } else {
            sel.iter().map(|e| e.transform.position).collect()
        };
        drop(s);
        let Some(first) = pts.first().copied() else { return };
        let (lo, hi) = pts.iter().fold((first, first), |(lo, hi), p| (lo.min(*p), hi.max(*p)));
        self.world.camera.frame(lo - Vec3::splat(1.0), hi + Vec3::splat(1.0));
    }

    pub fn world_delete_selection(&mut self) {
        for e in self.selected_entities() {
            self.world_cmd("world.remove", json!({ "id": e.id }));
        }
    }

    pub fn world_undo(&mut self) {
        self.world_cmd("world.undo", json!({}));
    }

    pub fn world_redo(&mut self) {
        self.world_cmd("world.redo", json!({}));
    }

    pub(super) fn show_world(&mut self, ui: &mut Ui) {
        if !self.world.framed {
            self.world.framed = true;
            if !self.engine.session().world.scene.entities.is_empty() {
                self.world_frame();
            }
        }
        Panel::left("world-scene")
            .resizable(true)
            .default_size(260.0)
            .size_range(200.0..=420.0)
            .frame(Frame::new().fill(theme::PANEL))
            .show(ui, |ui| {
                self.show_scene_list(ui);
                super::edge(ui, super::Side::Right);
            });
        Panel::right("world-inspector")
            .resizable(true)
            .default_size(300.0)
            .size_range(260.0..=480.0)
            .frame(Frame::new().fill(theme::PANEL))
            .show(ui, |ui| {
                self.show_inspector(ui);
                super::edge(ui, super::Side::Left);
            });
        eframe::egui::CentralPanel::default().frame(Frame::new().fill(theme::GROUND)).show(ui, |ui| {
            self.show_viewport_toolbar(ui);
            self.show_viewport(ui);
        });
    }

    // ------------------------------------------------------------ toolbar

    fn show_viewport_toolbar(&mut self, ui: &mut Ui) {
        let (can_undo, can_redo, has) = {
            let s = self.engine.session();
            (s.world.can_undo(), s.world.can_redo(), !s.world.scene.entities.is_empty())
        };
        Frame::new().fill(theme::PANEL).inner_margin(Margin::symmetric(8, 6)).show(ui, |ui| {
            ui.set_width(ui.available_width());
            ui.horizontal(|ui| {
                if Btn::new("Add entity")
                    .icon(Icon::Plus)
                    .tone(Tone::Secondary)
                    .tip("Add an entity in front of the camera (N)")
                    .show(ui)
                    .clicked()
                {
                    self.perform(Action::AddEntity);
                }
                ui.add_space(8.0);
                if Btn::new("")
                    .icon(Icon::Undo)
                    .tone(Tone::Ghost)
                    .tip("Undo (Ctrl+Z)")
                    .enabled(can_undo)
                    .show(ui)
                    .clicked()
                {
                    self.perform(Action::Undo);
                }
                if Btn::new("")
                    .icon(Icon::Redo)
                    .tone(Tone::Ghost)
                    .tip("Redo (Ctrl+Y)")
                    .enabled(can_redo)
                    .show(ui)
                    .clicked()
                {
                    self.perform(Action::Redo);
                }
                ui.add_space(8.0);
                if Btn::new("Frame")
                    .icon(Icon::Frame)
                    .tone(Tone::Ghost)
                    .tip("Frame the selection, or everything (F)")
                    .enabled(has)
                    .show(ui)
                    .clicked()
                {
                    self.perform(Action::FrameSelection);
                }
                ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                    if Btn::new("Load").tone(Tone::Ghost).tip("Open a saved scene").show(ui).clicked() {
                        self.perform(Action::LoadScene);
                    }
                    if Btn::new("Save").tone(Tone::Ghost).tip("Save the scene as a file").show(ui).clicked() {
                        self.perform(Action::SaveScene);
                    }
                });
            });
        });
        hairline(ui);
    }

    // --------------------------------------------------------- scene list

    fn show_scene_list(&mut self, ui: &mut Ui) {
        let (name, total, dirty) = {
            let s = self.engine.session();
            (s.world.scene.name.clone(), s.world.scene.entities.len(), s.world.is_dirty())
        };
        Frame::new().inner_margin(Margin::symmetric(12, 10)).show(ui, |ui| {
            ui.set_width(ui.available_width());
            ui.spacing_mut().item_spacing = vec2(8.0, 6.0);
            ui.label(
                egui::RichText::new(if name.is_empty() { "Scene" } else { &name })
                    .size(14.0)
                    .color(theme::TEXT),
            );
            let note = if dirty {
                format!("{} · unsaved changes", format::plural(total as u64, "entity", "entities"))
            } else {
                format::plural(total as u64, "entity", "entities")
            };
            super::faint(ui, note);
            ui.add_sized(
                [ui.available_width(), 28.0],
                TextEdit::singleline(&mut self.world.list_filter)
                    .hint_text("Filter entities…")
                    .margin(Margin::symmetric(10, 6)),
            );
        });
        hairline(ui);

        let filter = self.world.list_filter.trim().to_lowercase();
        let rows: Vec<(EntityId, String, bool)> = {
            let s = self.engine.session();
            let sc = &s.world.scene;
            sc.entities
                .iter()
                .filter(|e| filter.is_empty() || e.archetype.to_lowercase().contains(&filter))
                .map(|e| (e.id, e.archetype.clone(), sc.selection.contains(&e.id)))
                .collect()
        };

        if rows.is_empty() {
            ui.add_space(24.0);
            ui.vertical_centered(|ui| {
                if total == 0 {
                    ui.label(egui::RichText::new("No entities yet").color(theme::TEXT).size(14.0));
                    ui.add_space(2.0);
                    ui.label(
                        egui::RichText::new("Add one, or load a saved scene.").color(theme::FAINT).size(12.0),
                    );
                    ui.add_space(10.0);
                    if Btn::new("Add entity").icon(Icon::Plus).tone(Tone::Secondary).show(ui).clicked() {
                        self.perform(Action::AddEntity);
                    }
                } else {
                    ui.label(
                        egui::RichText::new(format!("No entity matches “{}”", self.world.list_filter.trim()))
                            .color(theme::MUTED)
                            .size(13.0),
                    );
                }
            });
            ui.add_space(16.0);
            self.scratch_note(ui);
            return;
        }

        let mut click: Option<(EntityId, bool, bool)> = None;
        ui.spacing_mut().item_spacing.y = 0.0;
        let font = FontId::proportional(13.0);
        let avail = ui.available_height() - 64.0;
        ui.allocate_ui(vec2(ui.available_width(), avail.max(60.0)), |ui| {
            ScrollArea::vertical().id_salt("scene-scroll").auto_shrink([false, false]).show_rows(
                ui,
                theme::ROW_H,
                rows.len(),
                |ui, range| {
                    for i in range {
                        let (id, name, selected) = &rows[i];
                        let (rect, resp) =
                            ui.allocate_exact_size(vec2(ui.available_width(), theme::ROW_H), Sense::click());
                        let p = ui.painter();
                        if *selected {
                            p.rect_filled(rect, 0.0, theme::ACTIVE);
                            p.rect_filled(
                                Rect::from_min_size(rect.min, vec2(2.0, rect.height())),
                                0.0,
                                theme::ACCENT,
                            );
                        } else if resp.hovered() {
                            p.rect_filled(rect, 0.0, theme::RAISED);
                        }
                        let ic = Rect::from_min_size(
                            pos2(rect.left() + 12.0, rect.center().y - 8.0),
                            vec2(16.0, 16.0),
                        );
                        icons::draw(p, ic, Icon::Cube, if *selected { theme::TEXT } else { theme::CONTROL });
                        let id_text = format!("#{id}");
                        let idr = p.text(
                            pos2(rect.right() - 12.0, rect.center().y),
                            Align2::RIGHT_CENTER,
                            id_text,
                            FontId::monospace(11.5),
                            theme::FAINT,
                        );
                        let label = fit_text(p, name, &font, idr.left() - ic.right() - 24.0);
                        p.text(
                            pos2(ic.right() + 8.0, rect.center().y),
                            Align2::LEFT_CENTER,
                            label,
                            font.clone(),
                            theme::TEXT,
                        );
                        if resp.clicked() {
                            click =
                                Some((*id, ui.input(|i| i.modifiers.command || i.modifiers.shift), false));
                        }
                        if resp.double_clicked() {
                            click = Some((*id, false, true));
                        }
                    }
                },
            );
        });
        if let Some((id, additive, frame)) = click {
            self.world_cmd("world.select", json!({ "id": id, "additive": additive }));
            if frame {
                self.world_frame();
            }
        }
        self.scratch_note(ui);
    }

    fn scratch_note(&self, ui: &mut Ui) {
        Frame::new().inner_margin(Margin::symmetric(12, 8)).show(ui, |ui| {
            ui.label(
                egui::RichText::new("Scratch scene: entities here are not linked to game data yet. Importing a .ymap is the next step.")
                    .size(11.5)
                    .color(theme::FAINT),
            );
        });
    }

    // ---------------------------------------------------------- inspector

    fn show_inspector(&mut self, ui: &mut Ui) {
        Frame::new().inner_margin(Margin::symmetric(16, 12)).show(ui, |ui| {
            ui.set_min_size(ui.available_size());
            ui.spacing_mut().item_spacing = vec2(8.0, 8.0);
            let sel = self.selected_entities();
            match sel.len() {
                0 => {
                    ui.add_space(24.0);
                    ui.vertical_centered(|ui| {
                        ui.label(egui::RichText::new("Nothing selected").size(14.0).color(theme::TEXT));
                        ui.label(
                            egui::RichText::new("Click an entity in the view or the list.")
                                .size(12.0)
                                .color(theme::FAINT),
                        );
                    });
                }
                1 => self.inspect_entity(ui, &sel[0]),
                n => {
                    ui.label(
                        egui::RichText::new(format!("{n} entities selected")).size(14.0).color(theme::TEXT),
                    );
                    super::faint(ui, "Select a single entity to edit it.");
                    if Btn::new("Delete selected").tone(Tone::Secondary).show(ui).clicked() {
                        self.perform(Action::DeleteSelection);
                    }
                }
            }
        });
    }

    fn inspect_entity(&mut self, ui: &mut Ui, e: &Entity) {
        // what the fields show: the edit in progress, else the stored value
        let t = match self.world.pending {
            Some((id, t)) if id == e.id => t,
            _ => e.transform,
        };
        ui.label(egui::RichText::new("Entity").size(12.0).color(theme::FAINT));

        // model name
        let buf = match &mut self.world.rename_buf {
            Some((id, b)) if *id == e.id => b,
            slot => {
                *slot = Some((e.id, e.archetype.clone()));
                &mut slot.as_mut().unwrap().1
            }
        };
        let name = ui.add_sized(
            [ui.available_width(), 30.0],
            TextEdit::singleline(buf).margin(Margin::symmetric(10, 7)).font(FontId::monospace(12.5)),
        );
        let new_name = buf.trim().to_string();
        if name.lost_focus() && new_name != e.archetype && !new_name.is_empty() {
            self.world_cmd("world.rename", json!({ "id": e.id, "archetype": new_name }));
        }
        super::faint(ui, format!("#{}  ·  hash 0x{:08X}", e.id, e.archetype_hash));
        hairline(ui);

        let mut edited = t;
        let mut changed = false;
        let mut released = false;
        let mut row = |ui: &mut Ui, label: &str, v: &mut Vec3, speed: f32, suffix: &str, min: Option<f32>| {
            ui.label(egui::RichText::new(label).size(12.0).color(theme::FAINT));
            ui.horizontal(|ui| {
                let w = (ui.available_width() - 16.0) / 3.0;
                for (axis, comp) in [("X", &mut v.x), ("Y", &mut v.y), ("Z", &mut v.z)] {
                    let mut dv = egui::DragValue::new(comp).speed(speed).max_decimals(3).suffix(suffix);
                    if let Some(m) = min {
                        dv = dv.range(m..=f32::MAX);
                    }
                    let r = ui.add_sized([w, 28.0], dv).on_hover_text(axis);
                    changed |= r.changed();
                    released |= r.drag_stopped() || r.lost_focus();
                }
            });
        };
        let mut pos = edited.position;
        row(ui, "Position", &mut pos, 0.1, " m", None);
        let mut rot = edited.euler_degrees();
        row(ui, "Rotation", &mut rot, 0.5, "°", None);
        let mut scale = edited.scale;
        row(ui, "Scale", &mut scale, 0.01, "", Some(0.01));
        edited.position = pos;
        edited = edited.with_euler_degrees(rot);
        edited.scale = scale;

        if changed {
            self.world.pending = Some((e.id, edited));
        }
        if released
            && let Some((id, t)) = self.world.pending.take()
            && id == e.id
            && t != e.transform
        {
            {
                let r = t.euler_degrees();
                self.world_cmd(
                    "world.set",
                    json!({ "id": id, "x": t.position.x, "y": t.position.y, "z": t.position.z,
                            "rx": r.x, "ry": r.y, "rz": r.z, "sx": t.scale.x, "sy": t.scale.y, "sz": t.scale.z }),
                );
            }
        }
        hairline(ui);
        ui.horizontal(|ui| {
            if Btn::new("Duplicate").icon(Icon::Copy).tone(Tone::Secondary).show(ui).clicked() {
                let p = e.transform.position + Vec3::X * (e.transform.scale.x.max(1.0) + 0.5);
                if let Some(v) = self
                    .world_cmd("world.add", json!({ "archetype": e.archetype, "x": p.x, "y": p.y, "z": p.z }))
                    && let Some(id) = v["id"].as_u64()
                {
                    self.world_cmd("world.select", json!({ "id": id }));
                }
            }
            if Btn::new("Delete")
                .icon(Icon::Close)
                .tone(Tone::Secondary)
                .tip("Delete (Del)")
                .show(ui)
                .clicked()
            {
                self.perform(Action::DeleteSelection);
            }
        });
    }

    // ------------------------------------------------------------ viewport

    fn show_viewport(&mut self, ui: &mut Ui) {
        let avail = ui.available_size();
        let (rect, resp) = ui.allocate_exact_size(avail, Sense::click_and_drag());
        self.world.last_viewport = rect;
        if rect.width() < 8.0 || rect.height() < 8.0 {
            return;
        }
        if self.world.gpu.is_none() {
            ui.painter().text(
                rect.center(),
                Align2::CENTER_CENTER,
                "The 3D view needs the GPU (wgpu) renderer, which is not available here.",
                FontId::proportional(13.0),
                theme::MUTED,
            );
            return;
        }
        let aspect = rect.width() / rect.height();
        let ppp = ui.ctx().pixels_per_point();
        let ndc = |pos: egui::Pos2| {
            (
                (pos.x - rect.left()) / rect.width() * 2.0 - 1.0,
                1.0 - (pos.y - rect.top()) / rect.height() * 2.0,
            )
        };

        self.viewport_input(ui, &resp, aspect, &ndc);

        // build geometry only when the scene (or the drag preview) changed
        let (rev, key) = (self.engine.session().world.revision(), self.world.preview);
        if self.world.built != Some((rev, key)) {
            let s = self.engine.session();
            self.world.instances = world_view::instances(&s.world.scene, key);
            self.world.outlines = world_view::selection_outlines(&s.world.scene, key);
            drop(s);
            self.world.built = Some((rev, key));
        }

        let cam = self.world.camera;
        let mut overlay = self.world.outlines.clone();
        if let Some(sel) = self.selected_entities().first().filter(|_| self.selected_entities().len() == 1) {
            let origin = match self.world.preview {
                Some((id, t)) if id == sel.id => t.position,
                _ => sel.transform.position,
            };
            let len = world_view::gizmo_length(cam.position, origin);
            overlay.extend(world_view::gizmo_lines(
                origin,
                len,
                self.world.hover_axis,
                self.world.drag.as_ref().map(|d| d.axis),
            ));
        }

        let globals = Globals {
            view_proj: cam.view_proj(aspect).to_cols_array_2d(),
            camera: [cam.position.x, cam.position.y, cam.position.z, 1.0],
            grid: [grid_spacing(cam.position.z), 3000.0, 0.0, 0.0],
        };
        let gpu = self.world.gpu.as_mut().expect("checked above");
        gpu.ensure_target((rect.width() * ppp) as u32, (rect.height() * ppp) as u32);
        let target = gpu.target.as_ref().expect("just ensured");
        gpu.renderer.render(
            &gpu.shared.device,
            &gpu.shared.queue,
            target,
            &globals,
            &GpuScene { boxes: &self.world.instances, lines: &[], overlay: &overlay },
        );
        if let Some(tex) = gpu.texture {
            ui.painter().image(tex, rect, Rect::from_min_max(pos2(0.0, 0.0), pos2(1.0, 1.0)), Color32::WHITE);
        }
        self.viewport_hud(ui, rect);
    }

    fn viewport_input(
        &mut self,
        ui: &mut Ui,
        resp: &egui::Response,
        aspect: f32,
        ndc: &dyn Fn(egui::Pos2) -> (f32, f32),
    ) {
        let ctx = ui.ctx().clone();
        let dt = ctx.input(|i| i.stable_dt).min(0.1);
        let pointer =
            resp.hover_pos().or_else(|| ctx.input(|i| i.pointer.latest_pos()).filter(|_| resp.dragged()));
        let ray = pointer.map(|p| {
            let (x, y) = ndc(p);
            self.world.camera.ray(x, y, aspect)
        });

        // --- look
        if resp.dragged_by(PointerButton::Secondary) || resp.dragged_by(PointerButton::Middle) {
            let d = resp.drag_delta();
            self.world.camera.look(d.x, d.y, 0.0035);
            ctx.set_cursor_icon(CursorIcon::None);
        }
        // --- move (WASD/QE) while the pointer is over the view and no text field is active
        if resp.hovered() && !ctx.egui_wants_keyboard_input() {
            let (fwd, right, up, boost) = ctx.input(|i| {
                let k = |key| i.key_down(key) as i8 as f32;
                let boost = if i.modifiers.shift {
                    4.0
                } else if i.modifiers.ctrl {
                    0.25
                } else {
                    1.0
                };
                (k(Key::W) - k(Key::S), k(Key::D) - k(Key::A), k(Key::E) - k(Key::Q), boost)
            });
            if fwd != 0.0 || right != 0.0 || up != 0.0 {
                let speed = (self.world.camera.position.z.abs() * 0.8).clamp(4.0, 400.0) * boost * dt;
                self.world.camera.fly(fwd * speed, right * speed, up * speed);
                ctx.request_repaint();
            }
            let scroll = ctx.input(|i| i.smooth_scroll_delta.y);
            if scroll != 0.0 {
                let step = (self.world.camera.position.z.abs() * 0.15).clamp(1.0, 120.0);
                self.world.camera.fly(scroll * 0.02 * step, 0.0, 0.0);
            }
        }

        // --- gizmo hover / drag
        let sel = self.selected_entities();
        let single = (sel.len() == 1).then(|| sel[0].clone());
        if self.world.drag.is_none() {
            self.world.hover_axis = None;
            if let (Some(e), Some(ray), true) =
                (&single, &ray, resp.hovered() && !resp.dragged_by(PointerButton::Secondary))
            {
                let len = world_view::gizmo_length(self.world.camera.position, e.transform.position);
                self.world.hover_axis = world_view::pick_axis(ray, e.transform.position, len);
            }
        }
        if resp.drag_started_by(PointerButton::Primary)
            && let (Some(axis), Some(e), Some(ray)) = (self.world.hover_axis, &single, &ray)
            && let Some(t0) = world_view::axis_param(ray, e.transform.position, axis)
        {
            self.world.drag = Some(GizmoDrag { id: e.id, axis, start: e.transform, t0 });
        }
        if let Some(d) = &self.world.drag {
            if resp.dragged_by(PointerButton::Primary) {
                if let Some(t) =
                    ray.as_ref().and_then(|r| world_view::axis_param(r, d.start.position, d.axis))
                {
                    let mut delta = t - d.t0;
                    if ctx.input(|i| i.modifiers.ctrl) {
                        delta = delta.round(); // Ctrl snaps to whole metres
                    }
                    let mut moved = d.start;
                    moved.position += d.axis.dir() * delta;
                    self.world.preview = Some((d.id, moved));
                }
                ctx.set_cursor_icon(CursorIcon::Grabbing);
            }
            if resp.drag_stopped() {
                let (id, start) = (d.id, d.start);
                self.world.drag = None;
                if let Some((pid, t)) = self.world.preview.take()
                    && pid == id
                    && t != start
                {
                    let p = t.position;
                    self.world_cmd("world.set", json!({ "id": id, "x": p.x, "y": p.y, "z": p.z }));
                }
            }
        } else if self.world.hover_axis.is_some() {
            ctx.set_cursor_icon(CursorIcon::Grab);
        }

        // --- pick
        if resp.clicked_by(PointerButton::Primary) && self.world.hover_axis.is_none() {
            let additive = ctx.input(|i| i.modifiers.shift || i.modifiers.command);
            let hit = ray.as_ref().and_then(|r| {
                let s = self.engine.session();
                world_view::pick_entity(&s.world.scene, r, None)
            });
            match hit {
                Some(id) => {
                    self.world_cmd("world.select", json!({ "id": id, "additive": additive }));
                }
                None if !additive => {
                    self.world_cmd("world.select", json!({}));
                }
                None => {}
            }
        }
        if resp.double_clicked_by(PointerButton::Primary) {
            self.perform(Action::FrameSelection);
        }

        // --- keys over the view
        if resp.hovered() && !ctx.egui_wants_keyboard_input() {
            let (frame, del, add, undo, redo) = ctx.input_mut(|i| {
                (
                    i.consume_key(egui::Modifiers::NONE, Key::F),
                    i.consume_key(egui::Modifiers::NONE, Key::Delete),
                    i.consume_key(egui::Modifiers::NONE, Key::N),
                    i.consume_key(egui::Modifiers::COMMAND, Key::Z),
                    i.consume_key(egui::Modifiers::COMMAND, Key::Y)
                        || i.consume_key(egui::Modifiers::COMMAND | egui::Modifiers::SHIFT, Key::Z),
                )
            });
            if frame {
                self.perform(Action::FrameSelection);
            }
            if del {
                self.perform(Action::DeleteSelection);
            }
            if add {
                self.perform(Action::AddEntity);
            }
            if undo {
                self.perform(Action::Undo);
            }
            if redo {
                self.perform(Action::Redo);
            }
        }
    }

    fn viewport_hud(&self, ui: &mut Ui, rect: Rect) {
        let p = ui.painter_at(rect);
        let c = self.world.camera.position;
        let mono = FontId::monospace(11.5);
        p.text(
            pos2(rect.left() + 12.0, rect.top() + 12.0),
            Align2::LEFT_TOP,
            format!("x {:.1}  y {:.1}  z {:.1}", c.x, c.y, c.z),
            mono.clone(),
            theme::MUTED,
        );
        let spacing = grid_spacing(c.z);
        p.text(
            pos2(rect.left() + 12.0, rect.top() + 28.0),
            Align2::LEFT_TOP,
            format!("grid {} m", spacing),
            mono,
            theme::FAINT,
        );
        p.text(
            pos2(rect.left() + 12.0, rect.bottom() - 12.0),
            Align2::LEFT_BOTTOM,
            "Right-drag to look  ·  W A S D Q E to move (Shift faster)  ·  click to select  ·  drag an arrow to move",
            FontId::proportional(11.5),
            theme::FAINT,
        );
    }
}
