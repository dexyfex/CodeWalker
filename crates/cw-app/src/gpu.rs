//! The graphics handles egui already owns, for the views that draw with wgpu
//! themselves (the map viewport, the texture viewer).

use std::sync::Arc;

use eframe::{
    egui::{self, TextureId},
    egui_wgpu, wgpu,
};

#[derive(Clone)]
pub struct Gpu {
    pub device: wgpu::Device,
    pub queue: wgpu::Queue,
    egui: Arc<egui::mutex::RwLock<egui_wgpu::Renderer>>,
}

impl Gpu {
    pub fn new(rs: &egui_wgpu::RenderState) -> Self {
        Self { device: rs.device.clone(), queue: rs.queue.clone(), egui: rs.renderer.clone() }
    }

    /// Makes `view` drawable by egui. The first call registers it and stores the id in `slot`;
    /// later calls (after the target was recreated, say) point the same id at the new view.
    pub fn show(&self, slot: &mut Option<TextureId>, view: &wgpu::TextureView) {
        let mut egui = self.egui.write();
        match *slot {
            Some(id) => {
                egui.update_egui_texture_from_wgpu_texture(&self.device, view, wgpu::FilterMode::Linear, id)
            }
            None => *slot = Some(egui.register_native_texture(&self.device, view, wgpu::FilterMode::Linear)),
        }
    }
}
