//! State of the texture viewer: which texture and mip is shown, how it is filtered, where the
//! view looks, and the GPU objects that draw it. Pure bookkeeping plus the GPU glue; the widgets
//! are in `ui::texview`.
//!
//! A picture takes this path: the worker decodes a mip to RGBA ([`DecodeJob`]), the UI thread cuts
//! it into copy-on-write tiles, filters run over the tiles on all cores, only the tiles a filter
//! changed are uploaded, and the compositor places the result in the viewport.

use std::sync::Arc;

use cw_core::{
    image::{Channels, TiledImage, channel_filter, invert_filter},
    texture::Texture,
};
use cw_gpu::{
    compose::{ComposeTarget, Compositor, ImageView, ViewParams},
    tiles::TileTexture,
};
use eframe::egui::{TextureHandle, TextureId};

use crate::{gpu::Gpu, preview::TexSet};

/// Decodes one mip of one texture on the worker thread.
pub struct DecodeJob {
    pub key: u64,
    pub texture: Arc<Texture>,
    pub level: u32,
}

impl DecodeJob {
    pub fn run(&self) -> Result<TiledImage, String> {
        let px = self.texture.decode(self.level).map_err(|e| e.to_string())?;
        Ok(TiledImage::from_rgba(px.width as usize, px.height as usize, &px.pixels))
    }
}

/// Colours the compositor paints with (the UI theme's, as 0..1 floats).
#[derive(Clone, Copy)]
pub struct Palette {
    pub ground: [f32; 4],
    pub checker: ([f32; 4], [f32; 4]),
}

struct TexGpu {
    gpu: Gpu,
    compositor: Compositor,
    texture: Option<TileTexture>,
    target: Option<ComposeTarget>,
    id: Option<TextureId>,
}

pub struct TexView {
    pub index: usize,
    pub level: u32,
    pub channels: Channels,
    pub invert: bool,
    /// Follows the viewport size until the user zooms or pans.
    pub fit: bool,
    pub view: ImageView,
    pub error: Option<String>,
    /// List pictures, made when first drawn.
    pub thumbs: Vec<Option<TextureHandle>>,
    /// Open at 1:1 instead of fitted (set by `apply_settings`, used once).
    start_at_actual: bool,
    requested: Option<(usize, u32)>,
    key: u64,
    base: Option<TiledImage>,
    shown: Option<TiledImage>,
    shown_with: (Channels, bool),
    gpu: Option<TexGpu>,
}

impl Default for TexView {
    fn default() -> Self {
        Self {
            index: 0,
            level: 0,
            channels: Channels::Rgba,
            invert: false,
            fit: true,
            view: ImageView { zoom: 1.0, center: (0.0, 0.0) },
            error: None,
            thumbs: Vec::new(),
            start_at_actual: false,
            requested: None,
            key: 0,
            base: None,
            shown: None,
            shown_with: (Channels::Rgba, false),
            gpu: None,
        }
    }
}

impl TexView {
    /// A new file was opened: start from its first texture with no filters.
    pub fn reset(&mut self, textures: usize) {
        let gpu = self.gpu.take(); // the GPU objects outlive any one file
        *self = Self { thumbs: vec![None; textures], gpu, ..Self::default() };
    }

    /// Shows another texture of the file, whole and unfiltered by mip.
    pub fn select(&mut self, index: usize) {
        if index != self.index {
            self.index = index;
            self.level = 0;
            self.fit = true;
        }
    }

    /// Developer aid (`CODEWALKER_TEX`): `index=2,level=1,channels=g,invert=1,actual=1`, for
    /// screenshots of a particular state. Unknown keys and bad values are ignored.
    pub fn apply_settings(&mut self, spec: &str) {
        for (key, value) in spec.split(',').filter_map(|kv| kv.split_once('=')) {
            match key.trim() {
                "index" => self.index = value.parse().unwrap_or(self.index),
                "level" => self.level = value.parse().unwrap_or(self.level),
                "channels" => self.channels = Channels::parse(value).unwrap_or(self.channels),
                "invert" => self.invert = matches!(value.trim(), "1" | "true"),
                "actual" => self.start_at_actual = matches!(value.trim(), "1" | "true"),
                _ => {}
            }
        }
    }

    pub fn set_level(&mut self, level: u32) {
        if level != self.level {
            self.level = level;
            self.fit = true;
        }
    }

    /// The decode to start, if what is wanted differs from what was last asked for.
    pub fn next_job(&mut self, set: &TexSet) -> Option<DecodeJob> {
        let entry = set.entries.get(self.index)?;
        let level = self.level.min(entry.texture.levels.max(1) - 1);
        let wanted = (self.index, level);
        if self.requested == Some(wanted) {
            return None;
        }
        self.requested = Some(wanted);
        self.key += 1;
        self.error = None;
        Some(DecodeJob { key: self.key, texture: entry.texture.clone(), level })
    }

    /// A decode finished. Answers to anything but the latest request are dropped.
    pub fn decoded(&mut self, key: u64, result: Result<TiledImage, String>) {
        if key != self.key {
            return;
        }
        match result {
            Ok(image) => {
                self.view = ImageView::actual((image.width, image.height));
                self.fit = !std::mem::take(&mut self.start_at_actual);
                self.base = Some(image);
                self.shown_with = (Channels::Rgba, false);
                self.shown = self.base.clone(); // shares every tile
            }
            Err(e) => {
                self.base = None;
                self.shown = None;
                self.error = Some(e);
            }
        }
    }

    pub fn is_decoding(&self) -> bool {
        self.requested.is_some() && self.base.is_none() && self.error.is_none()
    }

    pub fn size(&self) -> Option<(usize, usize)> {
        self.base.as_ref().map(|i| (i.width, i.height))
    }

    /// The unfiltered pixel at an image position.
    pub fn pixel(&self, x: usize, y: usize) -> Option<[u8; 4]> {
        self.base.as_ref().filter(|i| x < i.width && y < i.height).map(|i| i.pixel(x, y))
    }

    #[cfg(test)]
    pub fn shown(&self) -> Option<&TiledImage> {
        self.shown.as_ref()
    }

    /// Brings the filtered image in line with the channel and invert settings. Filters run on
    /// all cores and leave untouched tiles shared with the source.
    pub fn ensure_filtered(&mut self) {
        let Some(base) = &self.base else { return };
        if self.shown_with == (self.channels, self.invert) && self.shown.is_some() {
            return;
        }
        let mut image = base.clone();
        if self.channels != Channels::Rgba {
            image = image.apply(&channel_filter(self.channels));
        }
        if self.invert {
            image = image.apply(&invert_filter);
        }
        self.shown = Some(image);
        self.shown_with = (self.channels, self.invert);
    }

    /// Draws the filtered image into an offscreen target of `viewport` pixels and returns the id
    /// egui shows it with. `None` until something is decoded.
    pub fn render(&mut self, gpu: &Gpu, viewport: (u32, u32), palette: Palette) -> Option<TextureId> {
        self.ensure_filtered();
        let image = self.shown.as_ref()?;
        let g = self.gpu.get_or_insert_with(|| TexGpu {
            gpu: gpu.clone(),
            compositor: Compositor::new(&gpu.device),
            texture: None,
            target: None,
            id: None,
        });

        if g.texture.as_ref().is_none_or(|t| t.size != (image.width, image.height)) {
            g.texture = Some(TileTexture::new(&g.gpu.device, image.width, image.height));
        }
        let texture = g.texture.as_mut().expect("just made");
        texture.sync(&g.gpu.queue, image); // only tiles that changed

        let (w, h) = (viewport.0.max(1), viewport.1.max(1));
        if g.target.as_ref().is_none_or(|t| t.size != (w, h)) {
            let target = ComposeTarget::new(&g.gpu.device, w, h);
            g.gpu.show(&mut g.id, &target.view);
            g.target = Some(target);
        }
        let target = g.target.as_ref().expect("just made");
        g.compositor.render(
            &g.gpu.device,
            &g.gpu.queue,
            target,
            texture,
            &ViewParams {
                view: self.view,
                smooth: self.view.zoom < 2.0,
                ground: palette.ground,
                checker: palette.checker,
                checker_px: 8.0,
            },
        );
        g.id
    }
}

#[cfg(test)]
mod tests;
