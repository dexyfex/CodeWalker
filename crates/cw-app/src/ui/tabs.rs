//! The strip above the toolbar: the tabs on the left and, on the right, the buttons that open
//! things. Like the toolbar it is planned from measured widths, so the tabs and the buttons never
//! meet; when they would, the buttons keep their icons and lose their words.

use eframe::egui::{Align, Layout, Rect, Ui, pos2, vec2};

use super::{Btn, CONTROL_H, GAP, Tone, bar_row, cluster, segmented};
use crate::{
    actions::Action,
    app::{App, Mode},
    icons::Icon,
    layout::{Give, squeeze},
};

fn game_folder(labeled: bool) -> Btn<'static> {
    Btn::new(if labeled { "Game folder" } else { "" })
        .icon(Icon::FolderOpen)
        .tip("Choose your GTA V folder (Ctrl+Shift+O)")
        .tone(Tone::Secondary)
}

fn open_files(labeled: bool) -> Btn<'static> {
    Btn::new(if labeled { "Open files" } else { "" })
        .icon(Icon::File)
        .tip("Open archives or files (Ctrl+O)")
        .tone(Tone::Ghost)
}

impl App {
    pub(super) fn show_tabs(&mut self, ui: &mut Ui) {
        let row = bar_row(ui);
        let tabs_w = super::segmented_width(ui, &[super::Face::Text("Explorer"), super::Face::Text("World")]);
        let wide = game_folder(true).width(ui) + GAP + open_files(true).width(ui);
        let narrow = game_folder(false).width(ui) + GAP + open_files(false).width(ui);
        let given = squeeze(tabs_w + GAP + wide - row.width(), &[Give::All(wide - narrow)]);
        let labeled = given[0] == 0.0;
        let buttons_w = if labeled { wide } else { narrow };

        let tabs = Rect::from_min_max(row.min, pos2(row.right() - buttons_w - GAP, row.bottom()));
        let buttons =
            Rect::from_min_size(pos2(row.right() - buttons_w, row.top()), vec2(buttons_w, CONTROL_H));
        let centred = Layout::left_to_right(Align::Center);
        cluster(ui, tabs, centred, |ui| {
            let mode = usize::from(self.mode == Mode::World);
            if let Some(i) = segmented(ui, "mode", &["Explorer", "World"], mode) {
                self.perform([Action::ShowExplorer, Action::ShowWorld][i]);
            }
        });
        cluster(ui, buttons, centred, |ui| {
            if game_folder(labeled).show(ui).clicked() {
                self.perform(Action::OpenGameFolder);
            }
            if open_files(labeled).show(ui).clicked() {
                self.perform(Action::OpenFiles);
            }
        });
    }
}
