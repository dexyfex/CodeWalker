//! Finding the GTA V installation.
//!
//! The C# tool checked the Steam, Retail and OpenIV registry keys only. This
//! adds the Epic Games launcher (via its install manifests) and Steam library
//! folders on Linux/Proton.

use std::path::{Path, PathBuf};

use serde::Serialize;

#[derive(Debug, Clone, PartialEq, Eq, Serialize)]
pub struct Candidate {
    pub path: PathBuf,
    pub source: &'static str,
    /// `gta5_enhanced.exe` found instead of `gta5.exe`.
    pub enhanced: bool,
}

/// A folder is a GTA V install when it holds `gta5.exe` or `gta5_enhanced.exe`.
/// Returns whether it is the Enhanced edition.
pub fn validate(dir: &Path) -> Option<bool> {
    let names: Vec<String> = std::fs::read_dir(dir)
        .ok()?
        .flatten()
        .map(|e| e.file_name().to_string_lossy().to_lowercase())
        .collect();
    if names.iter().any(|n| n == "gta5_enhanced.exe") {
        Some(true)
    } else if names.iter().any(|n| n == "gta5.exe") {
        Some(false)
    } else {
        None
    }
}

/// `InstallLocation` of the Epic manifest when it describes Grand Theft Auto V.
pub fn epic_manifest_install(json: &str) -> Option<PathBuf> {
    let v: serde_json::Value = serde_json::from_str(json).ok()?;
    let name = v.get("DisplayName")?.as_str()?.to_lowercase();
    if !name.contains("grand theft auto v") {
        return None;
    }
    let loc = v.get("InstallLocation")?.as_str()?;
    (!loc.is_empty()).then(|| PathBuf::from(loc))
}

/// Library folders listed in Steam's `libraryfolders.vdf` (`"path"  "..."` lines).
pub fn steam_library_paths(vdf: &str) -> Vec<PathBuf> {
    vdf.lines()
        .filter_map(|l| {
            let l = l.trim();
            let rest = l.strip_prefix("\"path\"")?.trim();
            let inner = rest.strip_prefix('"')?.strip_suffix('"')?;
            Some(PathBuf::from(inner.replace("\\\\", "\\")))
        })
        .collect()
}

pub fn detect() -> Vec<Candidate> {
    let mut raw: Vec<(PathBuf, &'static str)> = Vec::new();

    #[cfg(windows)]
    {
        use winreg::{RegKey, enums::*};
        let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);
        for view in [KEY_READ | KEY_WOW64_64KEY, KEY_READ | KEY_WOW64_32KEY] {
            if let Ok(k) = hklm.open_subkey_with_flags(r"SOFTWARE\Rockstar Games\GTAV", view)
                && let Ok(p) = k.get_value::<String, _>("InstallFolderSteam")
            {
                // the value points at ...\Grand Theft Auto V\GTAV
                raw.push((
                    PathBuf::from(
                        p.trim_end_matches(['\\', '/'])
                            .trim_end_matches("GTAV")
                            .trim_end_matches(['\\', '/']),
                    ),
                    "Steam",
                ));
            }
            if let Ok(k) = hklm.open_subkey_with_flags(r"SOFTWARE\Rockstar Games\Grand Theft Auto V", view)
                && let Ok(p) = k.get_value::<String, _>("InstallFolder")
            {
                raw.push((PathBuf::from(p), "Rockstar launcher"));
            }
        }
        if let Ok(k) = RegKey::predef(HKEY_CURRENT_USER)
            .open_subkey(r"Software\NewTechnologyStudio\OpenIV.exe\BrowseForFolder")
            && let Ok(p) = k.get_value::<String, _>("game_path_Five_pc")
        {
            raw.push((PathBuf::from(p), "OpenIV"));
        }
        let manifests = PathBuf::from(r"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests");
        if let Ok(rd) = std::fs::read_dir(manifests) {
            for e in rd.flatten() {
                if e.path().extension().is_some_and(|x| x == "item")
                    && let Ok(text) = std::fs::read_to_string(e.path())
                    && let Some(p) = epic_manifest_install(&text)
                {
                    raw.push((p, "Epic Games"));
                }
            }
        }
        for lib in [r"C:\Program Files (x86)\Steam", r"C:\Program Files\Steam"] {
            raw.push((Path::new(lib).join("steamapps/common/Grand Theft Auto V"), "Steam"));
        }
    }

    #[cfg(not(windows))]
    if let Some(home) = dirs::home_dir() {
        let steam_roots = [
            home.join(".steam/steam"),
            home.join(".local/share/Steam"),
            home.join(".var/app/com.valvesoftware.Steam/.local/share/Steam"),
        ];
        for root in steam_roots {
            raw.push((root.join("steamapps/common/Grand Theft Auto V"), "Steam"));
            if let Ok(vdf) = std::fs::read_to_string(root.join("steamapps/libraryfolders.vdf")) {
                for lib in steam_library_paths(&vdf) {
                    raw.push((lib.join("steamapps/common/Grand Theft Auto V"), "Steam"));
                }
            }
        }
    }

    let mut out: Vec<Candidate> = Vec::new();
    for (path, source) in raw {
        if let Some(enhanced) = validate(&path)
            && !out.iter().any(|c| c.path == path)
        {
            out.push(Candidate { path, source, enhanced });
        }
    }
    out
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn epic_manifest_must_be_gta_v() {
        let gta = r#"{"DisplayName":"Grand Theft Auto V","InstallLocation":"D:\\Games\\GTAV"}"#;
        assert_eq!(epic_manifest_install(gta), Some(PathBuf::from(r"D:\Games\GTAV")));
        assert_eq!(epic_manifest_install(r#"{"DisplayName":"Fortnite","InstallLocation":"D:\\F"}"#), None);
        assert_eq!(
            epic_manifest_install(r#"{"DisplayName":"Grand Theft Auto V","InstallLocation":""}"#),
            None
        );
        assert_eq!(epic_manifest_install("not json"), None);
    }

    #[test]
    fn parses_steam_library_folders() {
        let vdf = "\"libraryfolders\"\n{\n\t\"0\"\n\t{\n\t\t\"path\"\t\t\"C:\\\\Program Files (x86)\\\\Steam\"\n\t}\n\t\"1\"\n\t{\n\t\t\"path\"\t\t\"/mnt/games/SteamLibrary\"\n\t}\n}\n";
        assert_eq!(
            steam_library_paths(vdf),
            [PathBuf::from(r"C:\Program Files (x86)\Steam"), PathBuf::from("/mnt/games/SteamLibrary")]
        );
    }

    #[test]
    fn validation_distinguishes_editions_and_rejects_other_folders() {
        let dir = std::env::temp_dir().join(format!("cw-gf-{}", std::process::id()));
        let _ = std::fs::remove_dir_all(&dir);
        std::fs::create_dir_all(&dir).unwrap();
        assert_eq!(validate(&dir), None);
        std::fs::write(dir.join("GTA5.exe"), b"").unwrap();
        assert_eq!(validate(&dir), Some(false));
        std::fs::write(dir.join("GTA5_Enhanced.exe"), b"").unwrap();
        assert_eq!(validate(&dir), Some(true), "Enhanced wins when both exist");
        assert_eq!(validate(&dir.join("missing")), None);
        let _ = std::fs::remove_dir_all(&dir);
    }
}
