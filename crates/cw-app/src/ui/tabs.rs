//! The strip above the toolbar: the tabs on the left and, on the right, the buttons that open
//! things. Like the toolbar it is planned from measured widths (`layout.rs`): long titles shrink
//! and then wait in an overflow menu, and only then do the buttons lose their words, so the tabs
//! and the buttons never meet.

use eframe::egui::{
    self, Align, Align2, Button, CornerRadius, FontId, Layout, Popup, Rect, Sense, Ui, pos2, vec2,
};

use super::{Btn, CONTROL_H, GAP, Tone, bar_row, cluster, fit_text, focus_ring, text_width};
use crate::{
    actions::Action,
    app::App,
    icons::{self, Icon},
    layout::{Give, TabRoom, fit_tabs, squeeze},
    tabs::{DocKind, Tab, Tabs},
    theme,
};

/// Space between tabs.
const TAB_GAP: f32 = 4.0;
/// Document tabs shrink to this before the rest wait in the overflow menu, and never grow past
/// [`MAX_DOC`]: a long title ends in an ellipsis and has a tooltip.
const MIN_DOC: f32 = 150.0;
const MAX_DOC: f32 = 260.0;
/// Hit area of the close button, inside the tab.
const CLOSE: f32 = 24.0;
/// What a tab adds to its title: padding, the icon and its gap, and for documents the close button.
const FIXED_CHROME: f32 = 10.0 + 16.0 + 6.0 + 12.0;
const DOC_CHROME: f32 = 10.0 + 16.0 + 6.0 + 4.0 + CLOSE + 4.0;

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

/// What a tab shows of itself.
struct TabView {
    tab: Tab,
    title: String,
    icon: Icon,
}

/// What was clicked on a tab.
enum Click {
    Show,
    Close,
}

impl App {
    pub(super) fn show_tabs(&mut self, ui: &mut Ui) {
        let row = bar_row(ui);
        let faces: Vec<TabView> = self.tabs.tabs().map(|tab| self.tab_face(tab)).collect();
        let natural: Vec<f32> = faces
            .iter()
            .map(|f| match f.tab {
                Tab::Document(_) => (text_width(ui, &f.title, 13.0) + DOC_CHROME).min(MAX_DOC),
                _ => text_width(ui, &f.title, 13.0) + FIXED_CHROME,
            })
            .collect();

        // the tabs shrink before the buttons give up their words
        let squeezed: f32 = natural
            .iter()
            .enumerate()
            .map(|(i, w)| if i < Tabs::FIXED { *w } else { w.min(MIN_DOC) } + TAB_GAP)
            .sum();
        let wide = game_folder(true).width(ui) + GAP + open_files(true).width(ui);
        let narrow = game_folder(false).width(ui) + GAP + open_files(false).width(ui);
        let given = squeeze(squeezed + wide - row.width(), &[Give::All(wide - narrow)]);
        let labeled = given[0] == 0.0;
        let buttons_w = if labeled { wide } else { narrow };

        let strip = Rect::from_min_max(row.min, pos2(row.right() - buttons_w - GAP, row.bottom()));
        let buttons =
            Rect::from_min_size(pos2(row.right() - buttons_w, row.top()), vec2(buttons_w, CONTROL_H));
        let centred = Layout::left_to_right(Align::Center);
        cluster(ui, strip, centred, |ui| self.draw_tabs(ui, strip, &faces, &natural));
        cluster(ui, buttons, centred, |ui| {
            if game_folder(labeled).show(ui).clicked() {
                self.perform(Action::OpenGameFolder);
            }
            if open_files(labeled).show(ui).clicked() {
                self.perform(Action::OpenFiles);
            }
        });
    }

    fn tab_face(&self, tab: Tab) -> TabView {
        let (title, icon) = match tab {
            Tab::Explorer => ("Explorer".to_string(), Icon::Folder),
            Tab::World => ("World".to_string(), Icon::Map),
            Tab::Document(id) => match self.tabs.document(id) {
                Some(doc) => (
                    doc.title.clone(),
                    match doc.kind {
                        DocKind::Model => Icon::Cube,
                    },
                ),
                None => (String::new(), Icon::File),
            },
        };
        TabView { tab, title, icon }
    }

    fn draw_tabs(&mut self, ui: &mut Ui, strip: Rect, faces: &[TabView], natural: &[f32]) {
        let active = self.tabs.position(self.tabs.active()).unwrap_or(0);
        // the window of document tabs that shows when they do not all fit is remembered, so it
        // does not jump about while the active tab stays in view
        let memory = egui::Id::unique("tab-strip-window");
        let first = ui.data(|d| d.get_temp::<usize>(memory)).unwrap_or(Tabs::FIXED);
        let room =
            TabRoom { avail: strip.width(), gap: TAB_GAP, min_doc: MIN_DOC, menu: TAB_GAP + CONTROL_H };
        let fit = fit_tabs(natural, Tabs::FIXED, active, first, room);
        ui.data_mut(|d| d.insert_temp(memory, fit.first));

        let mut clicked = None;
        let mut x = strip.left();
        for &(i, width) in &fit.shown {
            let rect = Rect::from_min_size(pos2(x, strip.top()), vec2(width, CONTROL_H));
            x += width + TAB_GAP;
            if let Some(click) = tab(ui, rect, &faces[i], faces[i].tab == self.tabs.active()) {
                clicked = Some((faces[i].tab, click));
            }
        }
        if !fit.hidden.is_empty() {
            let rect = Rect::from_min_size(pos2(x, strip.top()), vec2(CONTROL_H, CONTROL_H));
            if let Some(tab) = overflow_menu(ui, rect, fit.hidden.iter().map(|&i| &faces[i])) {
                clicked = Some((tab, Click::Show));
            }
        }
        match clicked {
            Some((Tab::Explorer, Click::Show)) => self.perform(Action::ShowExplorer),
            Some((Tab::World, Click::Show)) => self.perform(Action::ShowWorld),
            Some((tab, Click::Show)) => {
                self.tabs.activate(tab);
            }
            Some((tab, Click::Close)) => {
                self.tabs.close(tab);
            }
            None => {}
        }
    }
}

/// One tab in `rect`. Documents have a close button, and a middle click closes them too.
fn tab(ui: &mut Ui, rect: Rect, face: &TabView, selected: bool) -> Option<Click> {
    let font = FontId::proportional(13.0);
    let closable = matches!(face.tab, Tab::Document(_));
    let id = egui::Id::unique(("tab", face.tab));
    // the close button is made after the tab, so it is on top of it where they overlap
    let resp = ui.interact(rect, id, Sense::click());
    let close_rect =
        Rect::from_center_size(pos2(rect.right() - 4.0 - CLOSE / 2.0, rect.center().y), vec2(CLOSE, CLOSE));
    let close = closable.then(|| ui.interact(close_rect, id.with("close"), Sense::click()));

    let p = ui.painter();
    let r = CornerRadius::same(5);
    if selected {
        p.rect_filled(rect, r, theme::ACTIVE);
    } else if resp.hovered() || close.as_ref().is_some_and(|c| c.hovered()) {
        p.rect_filled(rect, r, theme::RAISED);
    }
    let color = if selected { theme::TEXT } else { theme::MUTED };
    let icon_rect = Rect::from_min_size(pos2(rect.left() + 10.0, rect.center().y - 8.0), vec2(16.0, 16.0));
    icons::draw(p, icon_rect, face.icon, color);
    let text_left = icon_rect.right() + 6.0;
    let text_right = if closable { close_rect.left() - 4.0 } else { rect.right() - 12.0 };
    let title = fit_text(p, &face.title, &font, text_right - text_left);
    p.text(pos2(text_left, rect.center().y), Align2::LEFT_CENTER, &title, font, color);
    focus_ring(p, rect, &resp);

    let mut click = resp.clicked().then_some(Click::Show);
    if let Some(close) = close {
        let p = ui.painter();
        if close.hovered() {
            p.rect_filled(close_rect, r, if selected { theme::RAISED } else { theme::ACTIVE });
        }
        let ink = if close.hovered() { theme::TEXT } else { theme::MUTED };
        icons::draw(
            p,
            Rect::from_center_size(close_rect.center(), vec2(16.0, 16.0)).shrink(2.0),
            Icon::Close,
            ink,
        );
        focus_ring(p, close_rect, &close);
        if close.clicked() || resp.middle_clicked() {
            click = Some(Click::Close);
        }
        close.on_hover_text(format!("Close tab ({})", Action::CloseTab.shortcut()));
    }
    // the whole title is the tooltip when it was cut, and the shortcut for the fixed tabs
    match face.tab {
        Tab::Explorer => resp.on_hover_text(format!("Explorer ({})", Action::ShowExplorer.shortcut())),
        Tab::World => resp.on_hover_text(format!("World ({})", Action::ShowWorld.shortcut())),
        Tab::Document(_) if title != face.title => resp.on_hover_text(&face.title),
        Tab::Document(_) => resp,
    };
    click
}

/// The button after the tabs that did not fit, and its menu. Returns the tab picked from it.
fn overflow_menu<'a>(ui: &mut Ui, rect: Rect, hidden: impl Iterator<Item = &'a TabView>) -> Option<Tab> {
    let resp = ui.interact(rect, egui::Id::unique("tab-overflow"), Sense::click());
    let p = ui.painter();
    if resp.is_pointer_button_down_on() {
        p.rect_filled(rect, 5, theme::ACTIVE);
    } else if resp.hovered() {
        p.rect_filled(rect, 5, theme::RAISED);
    }
    icons::draw(p, Rect::from_center_size(rect.center(), vec2(16.0, 16.0)), Icon::ChevronDown, theme::MUTED);
    focus_ring(p, rect, &resp);
    let resp = resp.on_hover_text("More tabs");
    let hidden: Vec<&TabView> = hidden.collect();
    let mut picked = None;
    Popup::menu(&resp).show(|ui| {
        let names = hidden.iter().map(|f| text_width(ui, &f.title, 13.0));
        ui.set_width((names.fold(0.0, f32::max) + 24.0).clamp(160.0, 360.0));
        for face in &hidden {
            if ui.add(Button::new(&face.title).truncate()).clicked() {
                picked = Some(face.tab);
            }
        }
    });
    picked
}
