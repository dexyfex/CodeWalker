//! Design tokens and egui styling. See `docs/DESIGN.md` for the reasoning.

use eframe::egui::{
    self, Color32, CornerRadius, FontData, FontDefinitions, FontFamily, FontId, Margin, Stroke, TextStyle,
    epaint::Shadow,
};

pub const GROUND: Color32 = Color32::from_rgb(0x0D, 0x0D, 0x0E);
pub const PANEL: Color32 = Color32::from_rgb(0x14, 0x14, 0x16);
pub const RAISED: Color32 = Color32::from_rgb(0x1C, 0x1C, 0x1F);
pub const ACTIVE: Color32 = Color32::from_rgb(0x26, 0x26, 0x2B);
pub const BORDER: Color32 = Color32::from_rgb(0x26, 0x26, 0x2A);
/// The two greys of the checkerboard that shows through transparent pixels.
pub const CHECKER_A: Color32 = Color32::from_rgb(0x1A, 0x1A, 0x1D);
pub const CHECKER_B: Color32 = Color32::from_rgb(0x24, 0x24, 0x28);
pub const CONTROL: Color32 = Color32::from_rgb(0x66, 0x66, 0x6E);
pub const TEXT: Color32 = Color32::from_rgb(0xF2, 0xF2, 0xF3);
pub const MUTED: Color32 = Color32::from_rgb(0xB8, 0xB8, 0xBE);
pub const FAINT: Color32 = Color32::from_rgb(0x8F, 0x8F, 0x98);
pub const ACCENT: Color32 = Color32::from_rgb(0x6E, 0xA8, 0xFE);
pub const POSITIVE: Color32 = Color32::from_rgb(0x3F, 0xC0, 0x7A);
pub const NEGATIVE: Color32 = Color32::from_rgb(0xE5, 0x56, 0x6F);
pub const WARNING: Color32 = Color32::from_rgb(0xE0, 0xA4, 0x3A);

/// Row pitch of lists and the tree.
pub const ROW_H: f32 = 28.0;
pub const HEX_ROW_H: f32 = 20.0;
pub const TOOLBAR_H: f32 = 40.0;
pub const STATUS_H: f32 = 26.0;

pub fn install(ctx: &egui::Context) {
    ctx.set_fonts(fonts());
    ctx.set_visuals(visuals());
    ctx.all_styles_mut(|style| {
        style.text_styles = [
            (TextStyle::Small, FontId::new(11.0, FontFamily::Proportional)),
            (TextStyle::Body, FontId::new(13.0, FontFamily::Proportional)),
            (TextStyle::Button, FontId::new(13.0, FontFamily::Proportional)),
            (TextStyle::Heading, FontId::new(16.0, FontFamily::Proportional)),
            (TextStyle::Monospace, FontId::new(12.5, FontFamily::Monospace)),
        ]
        .into();
        style.spacing.item_spacing = egui::vec2(8.0, 4.0);
        style.spacing.button_padding = egui::vec2(10.0, 5.0);
        style.spacing.interact_size = egui::vec2(24.0, 28.0);
        style.spacing.window_margin = Margin::same(16);
        style.spacing.menu_margin = Margin::same(6);
        style.spacing.scroll.floating = false;
        style.spacing.scroll.bar_width = 8.0;
        style.interaction.tooltip_delay = 0.35;
        style.animation_time = 0.0; // frequent interactions never animate; see docs/DESIGN.md
    });
}

fn fonts() -> FontDefinitions {
    let mut fonts = FontDefinitions::default(); // egui fallbacks (symbols) stay at the end
    fonts.font_data.insert(
        "inter".into(),
        std::sync::Arc::new(FontData::from_static(include_bytes!("../assets/fonts/Inter-Variable.ttf"))),
    );
    fonts.font_data.insert(
        "jetbrains-mono".into(),
        std::sync::Arc::new(FontData::from_static(include_bytes!(
            "../assets/fonts/JetBrainsMono-Regular.ttf"
        ))),
    );
    fonts.families.entry(FontFamily::Proportional).or_default().insert(0, "inter".into());
    fonts.families.entry(FontFamily::Monospace).or_default().insert(0, "jetbrains-mono".into());
    fonts
}

fn visuals() -> egui::Visuals {
    let mut v = egui::Visuals::dark();
    v.panel_fill = PANEL;
    v.window_fill = RAISED;
    v.window_stroke = Stroke::new(1.0, BORDER);
    v.extreme_bg_color = GROUND;
    v.faint_bg_color = PANEL; // no zebra striping
    v.code_bg_color = GROUND;
    v.override_text_color = None;
    v.hyperlink_color = ACCENT;
    v.warn_fg_color = WARNING;
    v.error_fg_color = NEGATIVE;
    v.window_corner_radius = CornerRadius::same(8);
    v.menu_corner_radius = CornerRadius::same(6);
    v.window_shadow = Shadow { offset: [0, 8], blur: 24, spread: 0, color: Color32::from_black_alpha(110) };
    v.popup_shadow = Shadow { offset: [0, 6], blur: 18, spread: 0, color: Color32::from_black_alpha(100) };
    v.selection.bg_fill = ACTIVE;
    v.selection.stroke = Stroke::new(1.0, ACCENT);
    v.text_cursor.stroke = Stroke::new(1.5, ACCENT);

    let r = CornerRadius::same(5);
    let w = &mut v.widgets;
    w.noninteractive.bg_fill = PANEL;
    w.noninteractive.weak_bg_fill = PANEL;
    w.noninteractive.bg_stroke = Stroke::new(1.0, BORDER);
    w.noninteractive.fg_stroke = Stroke::new(1.0, MUTED);
    w.noninteractive.corner_radius = r;

    w.inactive.bg_fill = RAISED;
    w.inactive.weak_bg_fill = RAISED;
    w.inactive.bg_stroke = Stroke::new(1.0, CONTROL); // text fields need a >= 3:1 outline
    w.inactive.fg_stroke = Stroke::new(1.0, TEXT);
    w.inactive.corner_radius = r;

    w.hovered.bg_fill = ACTIVE;
    w.hovered.weak_bg_fill = ACTIVE;
    w.hovered.bg_stroke = Stroke::new(1.0, CONTROL);
    w.hovered.fg_stroke = Stroke::new(1.0, TEXT);
    w.hovered.corner_radius = r;
    w.hovered.expansion = 0.0;

    w.active.bg_fill = ACTIVE;
    w.active.weak_bg_fill = ACTIVE;
    w.active.bg_stroke = Stroke::new(1.0, ACCENT);
    w.active.fg_stroke = Stroke::new(1.0, TEXT);
    w.active.corner_radius = r;
    w.active.expansion = 0.0;

    w.open = w.active;
    v
}

/// WCAG relative luminance of an sRGB colour.
#[cfg(test)]
pub fn luminance(c: Color32) -> f64 {
    fn ch(v: u8) -> f64 {
        let s = v as f64 / 255.0;
        if s <= 0.03928 { s / 12.92 } else { ((s + 0.055) / 1.055).powf(2.4) }
    }
    0.2126 * ch(c.r()) + 0.7152 * ch(c.g()) + 0.0722 * ch(c.b())
}

/// WCAG contrast ratio between two colours (1.0 – 21.0).
#[cfg(test)]
pub fn contrast(a: Color32, b: Color32) -> f64 {
    let (la, lb) = (luminance(a), luminance(b));
    let (hi, lo) = if la > lb { (la, lb) } else { (lb, la) };
    (hi + 0.05) / (lo + 0.05)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn contrast_math_matches_wcag_examples() {
        assert!((contrast(Color32::WHITE, Color32::BLACK) - 21.0).abs() < 0.01);
        assert!((contrast(Color32::WHITE, Color32::WHITE) - 1.0).abs() < 1e-9);
    }

    #[test]
    fn text_tokens_meet_4_5_to_1_on_every_surface() {
        for surface in [GROUND, PANEL, RAISED, ACTIVE] {
            for (name, fg) in [("text", TEXT), ("muted", MUTED), ("faint", FAINT)] {
                let c = contrast(fg, surface);
                assert!(c >= 4.5, "{name} on {surface:?} is {c:.2}:1");
            }
        }
        assert!(contrast(TEXT, PANEL) >= 7.0, "primary text should clear 7:1");
    }

    #[test]
    fn status_and_accent_colours_are_readable_text_on_panels() {
        for (name, fg) in
            [("accent", ACCENT), ("positive", POSITIVE), ("negative", NEGATIVE), ("warning", WARNING)]
        {
            let c = contrast(fg, PANEL);
            assert!(c >= 4.5, "{name} is {c:.2}:1");
        }
    }

    #[test]
    fn control_outlines_and_focus_ring_clear_3_to_1() {
        assert!(contrast(CONTROL, PANEL) >= 3.0, "{:.2}", contrast(CONTROL, PANEL));
        assert!(contrast(CONTROL, GROUND) >= 3.0);
        assert!(contrast(ACCENT, PANEL) >= 3.0);
    }

    #[test]
    fn surfaces_step_up_in_lightness() {
        assert!(luminance(GROUND) < luminance(PANEL));
        assert!(luminance(PANEL) < luminance(RAISED));
        assert!(luminance(RAISED) < luminance(ACTIVE));
    }
}
