//! The window's tabs as plain data, no egui, so opening, closing and switching are unit-tested.
//! Explorer and World are always there and cannot be closed; document tabs (one per opened model,
//! docs/PLAN.md stage 4) come and go after them.

/// Names what a document tab shows. Whoever opens the tab chooses it (a hash of the path, say), so
/// opening the same thing twice finds the tab that is already there.
pub type DocId = u64;

#[derive(Clone, Copy, PartialEq, Eq, Hash, Debug, Default)]
pub enum Tab {
    #[default]
    Explorer,
    World,
    Document(DocId),
}

#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum DocKind {
    #[cfg_attr(
        not(test),
        expect(dead_code, reason = "the model viewer is the first document (PLAN.md stage 4)")
    )]
    Model,
}

#[derive(Clone, PartialEq, Eq, Debug)]
pub struct Document {
    pub id: DocId,
    pub kind: DocKind,
    pub title: String,
}

#[derive(Default)]
pub struct Tabs {
    docs: Vec<Document>,
    active: Tab,
}

impl Tabs {
    /// Explorer and World, which come first.
    pub const FIXED: usize = 2;

    pub fn active(&self) -> Tab {
        self.active
    }

    /// Every tab in the order they are drawn.
    pub fn tabs(&self) -> impl Iterator<Item = Tab> + '_ {
        [Tab::Explorer, Tab::World].into_iter().chain(self.docs.iter().map(|d| Tab::Document(d.id)))
    }

    pub fn position(&self, tab: Tab) -> Option<usize> {
        self.tabs().position(|t| t == tab)
    }

    pub fn document(&self, id: DocId) -> Option<&Document> {
        self.docs.iter().find(|d| d.id == id)
    }

    /// Shows `tab`; false when there is no such tab.
    pub fn activate(&mut self, tab: Tab) -> bool {
        let there = self.position(tab).is_some();
        if there {
            self.active = tab;
        }
        there
    }

    /// Adds a document tab after the others and shows it. When one with the same id is open
    /// already, that one is shown instead and `doc` is dropped. True for a new tab.
    #[cfg_attr(
        not(test),
        expect(dead_code, reason = "the model viewer is the first caller (PLAN.md stage 4)")
    )]
    pub fn open(&mut self, doc: Document) -> bool {
        let tab = Tab::Document(doc.id);
        let new = self.document(doc.id).is_none();
        if new {
            self.docs.push(doc);
        }
        self.active = tab;
        new
    }

    /// Closes a document tab; the fixed tabs stay (false). When the tab on show closes, the one
    /// that moves into its place is shown, or the one before it when it was the last.
    pub fn close(&mut self, tab: Tab) -> bool {
        let Tab::Document(id) = tab else { return false };
        let Some(at) = self.docs.iter().position(|d| d.id == id) else { return false };
        self.docs.remove(at);
        if self.active == tab {
            self.active = match self.docs.get(at) {
                Some(next) => Tab::Document(next.id),
                None => self.tabs().nth(Self::FIXED + at - 1).unwrap_or_default(),
            };
        }
        true
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn doc(id: DocId) -> Document {
        Document { id, kind: DocKind::Model, title: format!("model_{id}.ydr") }
    }

    fn with_docs(ids: &[DocId]) -> Tabs {
        let mut t = Tabs::default();
        for &id in ids {
            t.open(doc(id));
        }
        t
    }

    fn order(t: &Tabs) -> Vec<Tab> {
        t.tabs().collect()
    }

    #[test]
    fn it_starts_on_the_explorer_with_the_world_next_to_it() {
        let t = Tabs::default();
        assert_eq!(t.active(), Tab::Explorer);
        assert_eq!(order(&t), [Tab::Explorer, Tab::World]);
        assert_eq!(Tabs::FIXED, 2);
    }

    #[test]
    fn the_fixed_tabs_cannot_be_closed() {
        let mut t = with_docs(&[1]);
        assert!(!t.close(Tab::Explorer));
        assert!(!t.close(Tab::World));
        assert_eq!(order(&t).len(), 3);
        t.activate(Tab::World);
        assert!(!t.close(Tab::World));
        assert_eq!(t.active(), Tab::World);
    }

    #[test]
    fn opening_adds_a_tab_at_the_end_and_shows_it() {
        let mut t = Tabs::default();
        assert!(t.open(doc(7)));
        assert!(t.open(doc(3)));
        assert_eq!(order(&t), [Tab::Explorer, Tab::World, Tab::Document(7), Tab::Document(3)]);
        assert_eq!(t.active(), Tab::Document(3));
        assert_eq!(t.document(7).map(|d| d.title.as_str()), Some("model_7.ydr"));
        assert_eq!(t.position(Tab::Document(3)), Some(3));
        assert_eq!(t.position(Tab::Document(9)), None);
    }

    #[test]
    fn opening_what_is_open_shows_that_tab_instead() {
        let mut t = with_docs(&[1, 2]);
        t.activate(Tab::Explorer);
        let mut again = doc(1);
        again.title = "renamed".into();
        assert!(!t.open(again));
        assert_eq!(t.active(), Tab::Document(1));
        assert_eq!(order(&t).len(), 4, "no second tab");
        assert_eq!(t.document(1).unwrap().title, "model_1.ydr");
    }

    #[test]
    fn activating_needs_a_tab_that_exists() {
        let mut t = with_docs(&[1]);
        assert!(t.activate(Tab::World));
        assert_eq!(t.active(), Tab::World);
        assert!(!t.activate(Tab::Document(5)));
        assert_eq!(t.active(), Tab::World);
        assert!(t.activate(Tab::Document(1)));
        assert_eq!(t.active(), Tab::Document(1));
    }

    #[test]
    fn closing_a_tab_in_the_background_keeps_the_one_on_show() {
        let mut t = with_docs(&[1, 2, 3]);
        t.activate(Tab::Document(2));
        assert!(t.close(Tab::Document(3)));
        assert!(t.close(Tab::Document(1)));
        assert_eq!(t.active(), Tab::Document(2));
        assert_eq!(order(&t), [Tab::Explorer, Tab::World, Tab::Document(2)]);
        assert!(!t.close(Tab::Document(1)), "already gone");
    }

    #[test]
    fn closing_the_tab_on_show_shows_its_neighbour() {
        // the next one moves into its place
        let mut t = with_docs(&[1, 2, 3]);
        t.activate(Tab::Document(2));
        t.close(Tab::Document(2));
        assert_eq!(t.active(), Tab::Document(3));
        // at the end it is the one before
        t.close(Tab::Document(3));
        assert_eq!(t.active(), Tab::Document(1));
        // and with no document left, the fixed tab before it
        t.close(Tab::Document(1));
        assert_eq!(t.active(), Tab::World);
        assert_eq!(order(&t), [Tab::Explorer, Tab::World]);
    }
}
