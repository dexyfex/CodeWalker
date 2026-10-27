//! UI-level commands. Each one is reachable from the command palette, from a
//! visible control and, when frequent, from a shortcut.

#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Action {
    OpenGameFolder,
    OpenFiles,
    ReloadGame,
    Back,
    Forward,
    Up,
    FocusSearch,
    SearchEverywhere,
    ShowPreview,
    ShowHex,
    ShowInfo,
    ExtractSelected,
    CopyPath,
    CopyToMods,
    ShowExplorer,
    ShowMods,
    Quit,
}

impl Action {
    pub const ALL: &'static [Action] = &[
        Action::OpenGameFolder,
        Action::OpenFiles,
        Action::ReloadGame,
        Action::Back,
        Action::Forward,
        Action::Up,
        Action::FocusSearch,
        Action::SearchEverywhere,
        Action::ShowPreview,
        Action::ShowHex,
        Action::ShowInfo,
        Action::ExtractSelected,
        Action::CopyPath,
        Action::CopyToMods,
        Action::ShowExplorer,
        Action::ShowMods,
        Action::Quit,
    ];

    pub fn title(self) -> &'static str {
        match self {
            Action::OpenGameFolder => "Open GTA V folder…",
            Action::OpenFiles => "Open archives or files…",
            Action::ReloadGame => "Rescan game folder",
            Action::Back => "Go back",
            Action::Forward => "Go forward",
            Action::Up => "Go to parent folder",
            Action::FocusSearch => "Filter this folder",
            Action::SearchEverywhere => "Search all archives",
            Action::ShowPreview => "Show preview",
            Action::ShowHex => "Show hex dump",
            Action::ShowInfo => "Show file information",
            Action::ExtractSelected => "Extract selection…",
            Action::CopyPath => "Copy path",
            Action::CopyToMods => "Copy archive to mods folder",
            Action::ShowExplorer => "Go to Explorer",
            Action::ShowMods => "Go to Mods",
            Action::Quit => "Quit",
        }
    }

    pub fn shortcut(self) -> &'static str {
        match self {
            Action::OpenGameFolder => "Ctrl+Shift+O",
            Action::OpenFiles => "Ctrl+O",
            Action::ReloadGame => "F5",
            Action::Back => "Alt+←",
            Action::Forward => "Alt+→",
            Action::Up => "Backspace",
            Action::FocusSearch => "Ctrl+F",
            Action::SearchEverywhere => "Ctrl+Shift+F",
            Action::ShowPreview => "Ctrl+1",
            Action::ShowHex => "Ctrl+2",
            Action::ShowInfo => "Ctrl+3",
            Action::ExtractSelected => "Ctrl+E",
            Action::CopyPath => "Ctrl+Shift+C",
            Action::CopyToMods => "Ctrl+M",
            Action::ShowExplorer => "Alt+1",
            Action::ShowMods => "Alt+2",
            Action::Quit => "Ctrl+Q",
        }
    }
}

/// Subsequence match: every character of `query` appears in `text` in order.
/// Lower is better; `None` means no match.
pub fn fuzzy_score(query: &str, text: &str) -> Option<usize> {
    let q: Vec<char> = query.to_lowercase().chars().filter(|c| !c.is_whitespace()).collect();
    if q.is_empty() {
        return Some(0);
    }
    let t: Vec<char> = text.to_lowercase().chars().collect();
    let (mut qi, mut score, mut last) = (0, 0usize, None::<usize>);
    for (i, c) in t.iter().enumerate() {
        if qi < q.len() && *c == q[qi] {
            score += match last {
                Some(l) if l + 1 == i => 0, // consecutive characters are free
                Some(l) => i - l,
                None => i, // a late first match costs more
            };
            last = Some(i);
            qi += 1;
        }
    }
    (qi == q.len()).then_some(score)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn every_action_has_a_unique_title_and_shortcut() {
        let mut titles: Vec<_> = Action::ALL.iter().map(|a| a.title()).collect();
        let mut keys: Vec<_> = Action::ALL.iter().map(|a| a.shortcut()).collect();
        titles.sort();
        keys.sort();
        titles.dedup();
        keys.dedup();
        assert_eq!(titles.len(), Action::ALL.len());
        assert_eq!(keys.len(), Action::ALL.len());
    }

    #[test]
    fn fuzzy_matching_prefers_tight_early_matches() {
        assert_eq!(fuzzy_score("", "anything"), Some(0));
        assert!(fuzzy_score("xyz", "Open folder").is_none());
        let tight = fuzzy_score("open", "Open GTA V folder").unwrap();
        let loose = fuzzy_score("open", "Rescan game folder, pick a new one").unwrap_or(usize::MAX);
        assert!(tight < loose);
        assert!(fuzzy_score("ogf", "Open GTA V folder").is_some(), "initials work");
        assert!(fuzzy_score("OPEN", "open").is_some(), "case-insensitive");
    }
}
