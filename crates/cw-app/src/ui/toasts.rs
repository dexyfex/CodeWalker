use eframe::egui::{self, Align2, Area, Color32, Frame, Id, Margin, Order, Sense, Stroke, vec2};

use crate::{app::App, icons::Icon, theme};

#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum ToastKind {
    Success,
    Warning,
    Error,
}

pub struct Toast {
    pub text: String,
    pub kind: ToastKind,
    born: f64,
}

impl Toast {
    pub fn new(text: String, kind: ToastKind, now: f64) -> Self {
        Self { text, kind, born: now }
    }

    /// Problems stay longer: people need time to read what went wrong.
    pub fn expired(&self, now: f64) -> bool {
        now - self.born > if self.kind == ToastKind::Success { 4.5 } else { 9.0 }
    }
}

impl App {
    pub(super) fn show_toasts(&mut self, ctx: &egui::Context) {
        if self.toasts.is_empty() {
            return;
        }
        let mut dismiss = None;
        Area::new(Id::unique("toasts"))
            .order(Order::Foreground)
            .anchor(Align2::RIGHT_BOTTOM, vec2(-16.0, -(theme::STATUS_H + 16.0)))
            .show(ctx, |ui| {
                ui.spacing_mut().item_spacing.y = 8.0;
                for (i, t) in self.toasts.iter().enumerate().rev().take(4) {
                    let (icon, color) = match t.kind {
                        ToastKind::Success => (Icon::Check, theme::POSITIVE),
                        ToastKind::Warning => (Icon::Alert, theme::WARNING),
                        ToastKind::Error => (Icon::Close, theme::NEGATIVE),
                    };
                    let r = Frame::new()
                        .fill(theme::RAISED)
                        .stroke(Stroke::new(1.0, theme::BORDER))
                        .corner_radius(6)
                        .shadow(ctx.global_style().visuals.popup_shadow)
                        .inner_margin(Margin::symmetric(12, 10))
                        .show(ui, |ui| {
                            ui.set_max_width(420.0);
                            ui.horizontal(|ui| {
                                let (rect, _) = ui.allocate_exact_size(vec2(16.0, 16.0), Sense::hover());
                                crate::icons::draw(ui.painter(), rect, icon, color);
                                ui.label(
                                    egui::RichText::new(&t.text)
                                        .size(13.0)
                                        .color(Color32::from_rgb(0xF2, 0xF2, 0xF3)),
                                );
                            });
                        });
                    if r.response.interact(Sense::click()).clicked() {
                        dismiss = Some(i);
                    }
                }
            });
        if let Some(i) = dismiss {
            self.toasts.remove(i);
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn problems_outlive_confirmations() {
        let ok = Toast::new("done".into(), ToastKind::Success, 0.0);
        let bad = Toast::new("failed".into(), ToastKind::Error, 0.0);
        assert!(ok.expired(5.0) && !bad.expired(5.0));
        assert!(bad.expired(9.5));
    }
}
