//! Navigation model of the RPF Explorer. Pure data and logic (no egui), so
//! it can be tested without a window.
//!
//! The tree has two kinds of nodes: virtual **folders** (the directories of
//! the game installation that contain `.rpf` files) and **containers**
//! (an archive's root, or a directory inside an archive).

use std::{
    collections::{BTreeSet, HashMap, HashSet},
    sync::Arc,
};

use cw_core::{EntryKind, EntryRef, RpfTree, filekind::FileKind};

use crate::format::natural_cmp;

/// Something the tree can show and the list can display the children of.
#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub enum NodeId {
    /// Directory of the scanned game folder; `""` is the root.
    Folder(String),
    /// Archive root (`index == 0`) or a directory entry inside an archive.
    Cont(EntryRef),
}

/// One row of the file list.
#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub enum Item {
    Folder(String),
    Entry(EntryRef),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum SortKey {
    Name,
    Type,
    Size,
    Packed,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum RowKind {
    Folder,
    Archive,
    Dir,
}

#[derive(Debug, Clone)]
pub struct TreeRow {
    pub node: NodeId,
    pub depth: u16,
    pub label: String,
    pub expandable: bool,
    pub expanded: bool,
    pub kind: RowKind,
}

#[derive(Debug, Clone)]
pub struct SearchResults {
    pub query: String,
    pub total: usize,
    pub items: Vec<EntryRef>,
}

#[derive(Default)]
struct FsDir {
    dirs: BTreeSet<String>,
    archives: Vec<u32>,
}

pub struct Explorer {
    pub tree: Option<Arc<RpfTree>>,
    pub root_label: String,
    fs: HashMap<String, FsDir>,
    pub expanded: HashSet<NodeId>,
    pub current: NodeId,
    pub rows: Vec<TreeRow>,
    pub listing: Vec<Item>,
    pub sort: SortKey,
    pub descending: bool,
    pub filter: String,
    pub selected: Option<Item>,
    pub search: Option<SearchResults>,
    history: Vec<NodeId>,
    pos: usize,
}

impl Default for Explorer {
    fn default() -> Self {
        let root = NodeId::Folder(String::new());
        Self {
            tree: None,
            root_label: "Game files".into(),
            fs: HashMap::new(),
            expanded: HashSet::new(),
            current: root.clone(),
            rows: Vec::new(),
            listing: Vec::new(),
            sort: SortKey::Name,
            descending: false,
            filter: String::new(),
            selected: None,
            search: None,
            history: vec![root],
            pos: 0,
        }
    }
}

/// A node named by path, so it can be found again after the archives are rescanned.
enum Stable {
    Folder(String),
    Path(String),
}

/// What [`Explorer::set_tree`] carries over from the previous tree.
struct Remembered {
    current: Option<Stable>,
    history: Vec<Option<Stable>>,
    pos: usize,
    expanded: Vec<Stable>,
    selected: Option<Stable>,
}

impl Remembered {
    fn capture(e: &Explorer) -> Self {
        Self {
            current: e.stable(&e.current),
            history: e.history.iter().map(|n| e.stable(n)).collect(),
            pos: e.pos,
            expanded: e.expanded.iter().filter_map(|n| e.stable(n)).collect(),
            selected: match &e.selected {
                Some(Item::Folder(p)) => Some(Stable::Folder(p.clone())),
                Some(Item::Entry(r)) => e.entry_path(*r).map(Stable::Path),
                None => None,
            },
        }
    }
}

fn dirname(path: &str) -> &str {
    path.rsplit_once('/').map_or("", |(d, _)| d)
}

fn basename(path: &str) -> &str {
    path.rsplit_once('/').map_or(path, |(_, b)| b)
}

impl Explorer {
    // ------------------------------------------------------------ tree data

    /// Replaces the open archives. Where the user was, the open branches, the
    /// back/forward trail and the selection are carried over by *path*: entry
    /// indices belong to one scan and mean something else in the next.
    pub fn set_tree(&mut self, tree: Option<Arc<RpfTree>>) {
        let before = Remembered::capture(self);
        self.tree = tree;
        self.fs.clear();
        if let Some(t) = &self.tree {
            for &root in &t.roots {
                let a = t.archive(root);
                let dir = dirname(&a.path).to_string();
                // register every ancestor folder
                let mut child = dir.clone();
                self.fs.entry(dir.clone()).or_default().archives.push(root);
                while !child.is_empty() {
                    let parent = dirname(&child).to_string();
                    self.fs.entry(parent.clone()).or_default().dirs.insert(child.clone());
                    child = parent;
                }
            }
        }
        self.fs.entry(String::new()).or_default();
        self.search = None;

        let root = NodeId::Folder(String::new());
        match before.current.as_ref().and_then(|s| self.resolve(s)) {
            Some(current) => {
                let mut trail: Vec<NodeId> = Vec::new();
                let mut pos = 0;
                for (i, s) in before.history.iter().enumerate() {
                    if let Some(n) = s.as_ref().and_then(|s| self.resolve(s)) {
                        trail.push(n);
                        if i <= before.pos {
                            pos = trail.len() - 1;
                        }
                    }
                }
                if trail.get(pos) != Some(&current) {
                    trail = vec![current.clone()];
                    pos = 0;
                }
                self.current = current;
                self.history = trail;
                self.pos = pos;
            }
            None => {
                self.current = root.clone();
                self.history = vec![root.clone()];
                self.pos = 0;
            }
        }
        self.expanded = before.expanded.iter().filter_map(|s| self.resolve(s)).collect();
        self.expanded.insert(root);
        self.selected = match &before.selected {
            Some(Stable::Folder(p)) if self.fs.contains_key(p) => Some(Item::Folder(p.clone())),
            Some(Stable::Path(p)) => self.tree.as_ref().and_then(|t| t.find(p)).map(Item::Entry),
            _ => None,
        };
        self.refresh();
        // a selection that is not in the listing (search results, another folder) goes
        if let Some(sel) = &self.selected
            && !self.listing.contains(sel)
        {
            self.selected = None;
        }
    }

    fn stable(&self, n: &NodeId) -> Option<Stable> {
        match n {
            NodeId::Folder(p) => Some(Stable::Folder(p.clone())),
            NodeId::Cont(r) => self.entry_path(*r).map(Stable::Path),
        }
    }

    fn entry_path(&self, r: EntryRef) -> Option<String> {
        let a = self.tree.as_ref()?.archives.get(r.archive as usize)?;
        Some(a.entries.get(r.index as usize)?.path.clone())
    }

    /// The node `s` names in the current tree, if it is still there.
    fn resolve(&self, s: &Stable) -> Option<NodeId> {
        match s {
            Stable::Folder(p) => self.fs.contains_key(p).then(|| NodeId::Folder(p.clone())),
            Stable::Path(p) => self.container_of(self.tree.as_ref()?.find(p)?),
        }
    }

    pub fn is_empty(&self) -> bool {
        self.tree.as_ref().is_none_or(|t| t.archives.is_empty())
    }

    pub fn container_of(&self, r: EntryRef) -> Option<NodeId> {
        let t = self.tree.as_ref()?;
        let e = t.entry(r);
        match (&e.kind, e.child_archive) {
            (_, Some(c)) => Some(NodeId::Cont(EntryRef { archive: c, index: 0 })),
            (EntryKind::Directory { .. }, None) => Some(NodeId::Cont(r)),
            _ => None,
        }
    }

    /// Entries directly inside a container.
    pub fn children_of(&self, c: EntryRef) -> Vec<EntryRef> {
        let Some(t) = &self.tree else { return Vec::new() };
        let a = t.archive(c.archive);
        let EntryKind::Directory { first, .. } = a.entries[c.index as usize].kind else {
            return Vec::new();
        };
        (0..a.dir_children(c.index).len() as u32)
            .map(|i| EntryRef { archive: c.archive, index: first + i })
            .collect()
    }

    fn label(&self, n: &NodeId) -> String {
        match n {
            NodeId::Folder(p) if p.is_empty() => self.root_label.clone(),
            NodeId::Folder(p) => basename(p).to_string(),
            NodeId::Cont(r) => self.tree.as_ref().map(|t| t.entry(*r).name.clone()).unwrap_or_default(),
        }
    }

    /// Child nodes shown under `n` in the tree.
    fn child_nodes(&self, n: &NodeId) -> Vec<NodeId> {
        if self.tree.is_none() {
            return Vec::new();
        }
        let mut out: Vec<NodeId> = match n {
            NodeId::Folder(p) => {
                let Some(d) = self.fs.get(p) else { return Vec::new() };
                let mut v: Vec<NodeId> = d.dirs.iter().cloned().map(NodeId::Folder).collect();
                v.extend(d.archives.iter().map(|&a| NodeId::Cont(EntryRef { archive: a, index: 0 })));
                v
            }
            NodeId::Cont(c) => {
                self.children_of(*c).into_iter().filter_map(|r| self.container_of(r)).collect()
            }
        };
        out.sort_by(|a, b| {
            let (fa, fb) = (matches!(a, NodeId::Folder(_)), matches!(b, NodeId::Folder(_)));
            fb.cmp(&fa).then_with(|| natural_cmp(&self.label(a), &self.label(b)))
        });
        out
    }

    pub fn parent_of(&self, n: &NodeId) -> Option<NodeId> {
        let t = self.tree.as_ref()?;
        match n {
            NodeId::Folder(p) if p.is_empty() => None,
            NodeId::Folder(p) => Some(NodeId::Folder(dirname(p).to_string())),
            NodeId::Cont(r) => {
                let a = t.archive(r.archive);
                match t.entry(*r).parent {
                    Some(p) => Some(NodeId::Cont(EntryRef { archive: r.archive, index: p })),
                    None => match (a.parent, a.parent_entry) {
                        (Some(pa), Some(pe)) => {
                            let host = t.archive(pa).entries[pe as usize].parent?;
                            Some(NodeId::Cont(EntryRef { archive: pa, index: host }))
                        }
                        _ => Some(NodeId::Folder(dirname(&a.path).to_string())),
                    },
                }
            }
        }
    }

    pub fn breadcrumbs(&self, n: &NodeId) -> Vec<(String, NodeId)> {
        let mut chain = vec![n.clone()];
        while let Some(p) = self.parent_of(chain.last().unwrap()) {
            chain.push(p);
        }
        chain.reverse();
        chain.into_iter().map(|n| (self.label(&n), n)).collect()
    }

    // ------------------------------------------------------------- rebuilds

    /// Recomputes the flattened tree and the list after any state change.
    pub fn refresh(&mut self) {
        self.rebuild_rows();
        self.rebuild_listing();
    }

    fn rebuild_rows(&mut self) {
        let mut rows = Vec::new();
        if self.tree.is_some() {
            let root = NodeId::Folder(String::new());
            self.push_rows(&root, 0, &mut rows);
        }
        self.rows = rows;
    }

    fn push_rows(&self, n: &NodeId, depth: u16, out: &mut Vec<TreeRow>) {
        let children = self.child_nodes(n);
        let expanded = self.expanded.contains(n);
        let kind = match n {
            NodeId::Folder(_) => RowKind::Folder,
            NodeId::Cont(r) if r.index == 0 => RowKind::Archive,
            NodeId::Cont(_) => RowKind::Dir,
        };
        out.push(TreeRow {
            node: n.clone(),
            depth,
            label: self.label(n),
            expandable: !children.is_empty(),
            expanded: expanded && !children.is_empty(),
            kind,
        });
        if expanded {
            for c in &children {
                self.push_rows(c, depth + 1, out);
            }
        }
    }

    fn rebuild_listing(&mut self) {
        let Some(t) = self.tree.clone() else {
            self.listing.clear();
            return;
        };
        let mut items: Vec<Item> = if let Some(s) = &self.search {
            s.items.iter().copied().map(Item::Entry).collect()
        } else {
            match &self.current {
                NodeId::Folder(p) => {
                    let mut v = Vec::new();
                    if let Some(d) = self.fs.get(p) {
                        v.extend(d.dirs.iter().cloned().map(Item::Folder));
                        v.extend(d.archives.iter().map(|&a| Item::Entry(EntryRef { archive: a, index: 0 })));
                    }
                    v
                }
                NodeId::Cont(c) => self.children_of(*c).into_iter().map(Item::Entry).collect(),
            }
        };

        let f = self.filter.trim().to_lowercase();
        if !f.is_empty() {
            items.retain(|i| match i {
                Item::Folder(p) => basename(p).to_lowercase().contains(&f),
                Item::Entry(r) => t.entry(*r).name_lower.contains(&f),
            });
        }

        // 0 = folder, 1 = nested archive, 2 = file: folders first, then archives.
        let rank = |i: &Item| match i {
            Item::Folder(_) => 0,
            Item::Entry(r) => {
                let e = t.entry(*r);
                if r.index == 0 || e.child_archive.is_some() {
                    1 // an archive (the root entry of a top-level one, or a nested one)
                } else if e.is_dir() {
                    0
                } else {
                    2
                }
            }
        };
        let name = |i: &Item| match i {
            Item::Folder(p) => basename(p).to_string(),
            Item::Entry(r) => t.entry(*r).name.clone(),
        };
        let kind_label = |i: &Item| match i {
            Item::Folder(_) => "Folder",
            Item::Entry(r) => {
                let e = t.entry(*r);
                if e.is_dir() { "Folder" } else { FileKind::from_name(&e.name_lower).label() }
            }
        };
        let size = |i: &Item| match i {
            Item::Folder(_) => 0,
            Item::Entry(r) => t.entry(*r).display_size(),
        };
        let packed = |i: &Item| match i {
            Item::Folder(_) => 0,
            Item::Entry(r) => t.entry(*r).stored_size(),
        };

        let (key, desc) = (self.sort, self.descending);
        let search_mode = self.search.is_some();
        items.sort_by(|a, b| {
            let by_name = || natural_cmp(&name(a), &name(b));
            let ord = match key {
                SortKey::Name => by_name(),
                SortKey::Type => kind_label(a).cmp(kind_label(b)).then_with(by_name),
                SortKey::Size => size(a).cmp(&size(b)).then_with(by_name),
                SortKey::Packed => packed(a).cmp(&packed(b)).then_with(by_name),
            };
            let ord = if desc { ord.reverse() } else { ord };
            if search_mode {
                return ord;
            }
            // folders, then archives, then files, whatever the direction
            let (ra, rb) = (rank(a), rank(b));
            ra.cmp(&rb).then_with(|| if ra < 2 { by_name() } else { ord })
        });
        self.listing = items;
    }

    // ----------------------------------------------------------- navigation

    pub fn set_sort(&mut self, key: SortKey) {
        if self.sort == key {
            self.descending = !self.descending;
        } else {
            self.sort = key;
            self.descending = false;
        }
        self.rebuild_listing();
    }

    pub fn set_filter(&mut self, text: String) {
        self.filter = text;
        self.rebuild_listing();
    }

    pub fn toggle(&mut self, n: &NodeId) {
        if !self.expanded.remove(n) {
            self.expanded.insert(n.clone());
        }
        self.refresh();
    }

    /// Number of direct children of a folder, archive or directory item.
    pub fn child_count(&self, item: &Item) -> Option<usize> {
        match item {
            Item::Folder(p) => self.fs.get(p).map(|d| d.dirs.len() + d.archives.len()),
            Item::Entry(r) => {
                let t = self.tree.as_ref()?;
                let e = t.entry(*r);
                let host = match (&e.kind, e.child_archive) {
                    (_, Some(c)) => EntryRef { archive: c, index: 0 },
                    (EntryKind::Directory { .. }, None) => *r,
                    _ => return None,
                };
                match t.entry(host).kind {
                    EntryKind::Directory { .. } => {
                        Some(t.archive(host.archive).dir_children(host.index).len())
                    }
                    _ => None,
                }
            }
        }
    }

    pub fn navigate(&mut self, n: NodeId) {
        if n == self.current && self.search.is_none() {
            return;
        }
        self.history.truncate(self.pos + 1);
        self.history.push(n.clone());
        self.pos = self.history.len() - 1;
        self.go(n);
    }

    fn go(&mut self, n: NodeId) {
        self.current = n.clone();
        self.search = None;
        self.filter.clear();
        self.selected = None;
        // expanding the ancestors keeps the tree row of the location visible
        let mut p = self.parent_of(&n);
        while let Some(a) = p {
            self.expanded.insert(a.clone());
            p = self.parent_of(&a);
        }
        self.refresh();
    }

    pub fn can_back(&self) -> bool {
        self.pos > 0
    }

    pub fn can_forward(&self) -> bool {
        self.pos + 1 < self.history.len()
    }

    pub fn back(&mut self) {
        if self.can_back() {
            self.pos -= 1;
            self.go(self.history[self.pos].clone());
        }
    }

    pub fn forward(&mut self) {
        if self.can_forward() {
            self.pos += 1;
            self.go(self.history[self.pos].clone());
        }
    }

    pub fn up(&mut self) {
        if let Some(p) = self.parent_of(&self.current.clone()) {
            self.navigate(p);
        }
    }

    /// Opens a list item: containers are entered, files are only selected.
    /// Returns true when it navigated.
    pub fn open_item(&mut self, item: &Item) -> bool {
        match item {
            Item::Folder(p) => {
                self.navigate(NodeId::Folder(p.clone()));
                true
            }
            Item::Entry(r) => match self.container_of(*r) {
                Some(n) => {
                    self.navigate(n);
                    true
                }
                None => {
                    self.selected = Some(item.clone());
                    false
                }
            },
        }
    }

    /// Navigates to the folder holding `r` and selects it (used by search results).
    pub fn reveal(&mut self, r: EntryRef) {
        let Some(t) = self.tree.clone() else { return };
        let e = t.entry(r);
        let host = match e.parent {
            Some(p) => Some(NodeId::Cont(EntryRef { archive: r.archive, index: p })),
            None => self.parent_of(&NodeId::Cont(r)),
        };
        if let Some(host) = host {
            self.search = None;
            self.navigate(host);
            if self.current == self.history[self.pos] {
                self.refresh();
            }
        }
        self.selected = Some(Item::Entry(r));
    }

    pub fn show_search(&mut self, query: String, total: usize, items: Vec<EntryRef>) {
        self.search = Some(SearchResults { query, total, items });
        self.selected = None;
        self.rebuild_listing();
    }

    pub fn clear_search(&mut self) {
        if self.search.take().is_some() {
            self.rebuild_listing();
        }
    }

    // ------------------------------------------------------------ selection

    pub fn selected_index(&self) -> Option<usize> {
        let s = self.selected.as_ref()?;
        self.listing.iter().position(|i| i == s)
    }

    /// Moves the selection by `delta` rows (keyboard navigation), clamped.
    pub fn move_selection(&mut self, delta: isize) {
        if self.listing.is_empty() {
            return;
        }
        let cur = self
            .selected_index()
            .map_or(if delta >= 0 { -1 } else { self.listing.len() as isize }, |i| i as isize);
        let next = (cur + delta).clamp(0, self.listing.len() as isize - 1) as usize;
        self.selected = Some(self.listing[next].clone());
    }

    pub fn item_path(&self, item: &Item) -> String {
        match (item, &self.tree) {
            (Item::Folder(p), _) => p.clone(),
            (Item::Entry(r), Some(t)) => t.entry(*r).path.clone(),
            _ => String::new(),
        }
    }
}

#[cfg(test)]
mod tests;
