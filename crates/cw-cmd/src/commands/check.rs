//! `game.check`: try the readers on the archives that are open and say what works.
//!
//! The formats here were ported from source code, and the test suite uses files this project made
//! itself. Real game files are the only real test, so this command samples them: key derivation,
//! archive scanning, decrypting and inflating entries, and parsing `.ytd`, `.ymap` files. It
//! prints what it tried and every kind of failure it met, so a problem can be reported precisely.

use std::collections::BTreeMap;

use cw_core::{EntryKind, EntryRef, RpfTree, rpf::RpfEncryption, ymap::parse_ymap, ytd::parse_ytd};
use rayon::prelude::*;
use serde::Deserialize;
use serde_json::{Value, json};

use super::args::parse;
use crate::{
    engine::{Ctx, Engine},
    error::CmdResult,
    spec::{CommandSpec, ParamKind as K, ParamSpec},
};

pub fn register(e: &mut Engine) {
    e.register(
        CommandSpec::new(
            "game.check",
            "archive",
            "Sample the open archives with every reader and report what works and what fails",
        )
        .param(ParamSpec::new("sample", K::Integer, "files to try per kind (default 40, max 500)")),
        check,
    );
}

/// `count` items spread evenly over `all`, so a sample covers the whole game and not one folder.
fn spread<T: Copy>(all: &[T], count: usize) -> Vec<T> {
    if all.len() <= count {
        return all.to_vec();
    }
    (0..count).map(|i| all[i * all.len() / count]).collect()
}

/// Failure reasons with how often each happened, and a few example paths.
#[derive(Default)]
struct Failures(BTreeMap<String, (usize, Vec<String>)>);

impl Failures {
    fn add(&mut self, reason: String, path: &str) {
        let (n, examples) = self.0.entry(reason).or_default();
        *n += 1;
        if examples.len() < 3 {
            examples.push(path.to_string());
        }
    }

    fn json(&self) -> Value {
        self.0
            .iter()
            .map(|(reason, (n, examples))| json!({ "reason": reason, "count": n, "examples": examples }))
            .collect()
    }

    fn total(&self) -> usize {
        self.0.values().map(|(n, _)| n).sum()
    }
}

fn entries_with(tree: &RpfTree, exts: &[&str]) -> Vec<EntryRef> {
    let mut out = Vec::new();
    for a in &tree.archives {
        for (i, e) in a.entries.iter().enumerate() {
            let ext = e.name_lower.rsplit_once('.').map(|(_, x)| x);
            if !e.is_dir() && ext.is_some_and(|x| exts.contains(&x)) {
                out.push(EntryRef { archive: a.id, index: i as u32 });
            }
        }
    }
    out
}

fn check(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        sample: Option<usize>,
    }
    let a: A = parse(args)?;
    let sample = a.sample.unwrap_or(40).clamp(1, 500);
    let tree = ctx.tree()?;
    let keys_loaded = ctx.session().keys.is_some();
    let mut report: Vec<String> = Vec::new();

    // ---- keys and archives
    report.push(if keys_loaded {
        "keys: derived (encrypted archives can be read)".into()
    } else {
        "keys: NOT derived — encrypted archives are skipped; check the game executable".into()
    });
    let (mut open, mut aes, mut ng, mut other) = (0, 0, 0, 0);
    for a in &tree.archives {
        match a.encryption {
            RpfEncryption::None | RpfEncryption::Open => open += 1,
            RpfEncryption::Aes => aes += 1,
            RpfEncryption::Ng => ng += 1,
            RpfEncryption::Unknown(_) => other += 1,
        }
    }
    report.push(format!(
        "archives: {} read ({} top-level), {} could not be opened",
        tree.archives.len(),
        tree.roots.len(),
        tree.errors.len()
    ));

    // ---- extracting entries of every kind (decrypt, inflate)
    let all_files: Vec<EntryRef> = tree
        .archives
        .iter()
        .flat_map(|a| {
            a.entries
                .iter()
                .enumerate()
                .filter(|(_, e)| !e.is_dir() && e.child_archive.is_none())
                .map(move |(i, _)| EntryRef { archive: a.id, index: i as u32 })
        })
        .collect();
    let encrypted: Vec<EntryRef> = all_files
        .iter()
        .copied()
        .filter(|r| {
            matches!(
                tree.entry(*r).kind,
                EntryKind::Binary { encrypted: true, .. } | EntryKind::Resource { encrypted: true, .. }
            )
        })
        .collect();
    let mut extract_failures = Failures::default();
    let tried: Vec<EntryRef> =
        spread(&all_files, sample).into_iter().chain(spread(&encrypted, sample)).collect();
    let results: Vec<(String, Result<usize, String>)> = tried
        .par_iter()
        .map(|r| (tree.entry(*r).path.clone(), tree.extract(*r).map(|d| d.len()).map_err(|e| e.to_string())))
        .collect();
    for (path, r) in &results {
        if let Err(reason) = r {
            extract_failures.add(reason.clone(), path);
        }
    }
    let encrypted_set: std::collections::HashSet<EntryRef> = encrypted.iter().copied().collect();
    report.push(format!(
        "extract: {} of {} sampled entries ({} of them encrypted) read fine",
        results.len() - extract_failures.total(),
        results.len(),
        tried.iter().filter(|r| encrypted_set.contains(r)).count()
    ));

    // ---- texture dictionaries
    let ytds = spread(&entries_with(&tree, &["ytd"]), sample);
    let mut ytd_failures = Failures::default();
    let (mut textures, mut decodable, mut parsed) = (0usize, 0usize, 0usize);
    let mut formats: BTreeMap<&'static str, usize> = BTreeMap::new();
    for r in &ytds {
        let path = &tree.entry(*r).path;
        match tree
            .extract_resource(*r)
            .map_err(|e| e.to_string())
            .and_then(|rd| parse_ytd(&rd).map_err(|e| e.to_string()))
        {
            // a dictionary that parses but holds nothing is as suspicious as one that fails
            Ok(list) if list.is_empty() => ytd_failures.add("parsed, but holds no textures".into(), path),
            Ok(list) => {
                parsed += 1;
                for t in &list {
                    textures += 1;
                    *formats.entry(t.texture.format.label()).or_default() += 1;
                    if t.texture.decode(0).is_ok() {
                        decodable += 1;
                    } else {
                        ytd_failures.add(format!("cannot decode {}", t.texture.format.label()), path);
                    }
                }
            }
            Err(e) => ytd_failures.add(e, path),
        }
    }
    report.push(format!("ytd: {parsed} of {} parsed, {decodable} of {textures} textures decode", ytds.len()));

    // ---- map data
    let ymaps = spread(&entries_with(&tree, &["ymap"]), sample);
    let mut ymap_failures = Failures::default();
    let (mut ymap_ok, mut entities) = (0usize, 0usize);
    for r in &ymaps {
        let path = &tree.entry(*r).path;
        match tree
            .extract_resource(*r)
            .map_err(|e| e.to_string())
            .and_then(|rd| parse_ymap(&rd).map_err(|e| e.to_string()))
        {
            Ok(m) => {
                ymap_ok += 1;
                entities += m.entities.len();
                // a map that parses but holds nothing is as suspicious as one that fails
                if m.entities.is_empty() {
                    ymap_failures.add("parsed, but holds no entities".into(), path);
                }
            }
            Err(e) => ymap_failures.add(e, path),
        }
    }
    report.push(format!("ymap: {ymap_ok} of {} parsed, {entities} entities read", ymaps.len()));

    Ok(json!({
        "report": report,
        "keys": keys_loaded,
        "archives": {
            "read": tree.archives.len(), "top_level": tree.roots.len(), "errors": tree.errors.len(),
            "first_errors": tree.errors.iter().take(5).map(|e| json!({ "path": e.path, "message": e.message })).collect::<Vec<_>>(),
            "encryption": { "open": open, "aes": aes, "ng": ng, "other": other },
            "files": all_files.len(), "encrypted_files": encrypted.len(),
        },
        "extract": { "tried": results.len(), "failed": extract_failures.total(), "failures": extract_failures.json() },
        "ytd": {
            "tried": ytds.len(), "parsed": parsed, "textures": textures, "decodable": decodable,
            "formats": formats, "failures": ytd_failures.json(),
        },
        "ymap": {
            "tried": ymaps.len(), "parsed": ymap_ok, "entities": entities, "failures": ymap_failures.json(),
        },
    }))
}
