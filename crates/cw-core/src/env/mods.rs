//! Time-cycle modifiers: named sets of overrides that map entities, interiors and the underwater view
//! ask for. Ported from `CodeWalker.Core/World/TimecycleMods.cs`.
//!
//! The C# only reads them: they are looked up by name hash to show what a `.ymap` modifier does
//! (`YmapFile.cs`) and are never mixed into the evaluated sky or lights, so neither does this.
//!
//! The files (`common.rpf/data/timecycle/timecycle_mods_1.xml` to `_4`, the same names under
//! `update/update.rpf/common/data/timecycle`, and any `timecycle_mods_*.xml` in a DLC pack) are plain XML:
//!
//! ```xml
//! <timecycle_mods version="1">                       <!-- the root's name is not read -->
//!   <modifier name="underwater" numMods="2" userFlags="0">
//!     <sky_hdr>1 0.5</sky_hdr>                       <!-- two numbers per variable -->
//!   </modifier>
//! </timecycle_mods>
//! ```

use std::collections::HashMap;

use super::{Result, xml};
use crate::hash::joaat_lower;

/// One variable a modifier overrides: the C# keeps its two numbers as `value1` and `value2` and does not
/// say what they mean, so neither does this.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct ModValue {
    pub name: String,
    pub value1: f32,
    pub value2: f32,
}

#[derive(Debug, Clone, PartialEq, Default)]
pub struct TimecycleMod {
    pub name: String,
    /// `joaat` of the lower-case name, which is how a `.ymap` refers to it.
    pub name_hash: u32,
    pub num_mods: i32,
    pub user_flags: i32,
    pub values: Vec<ModValue>,
}

/// Every modifier loaded so far. A name defined again replaces the earlier definition in its place,
/// which is how later files (the update's, a DLC's) override the base ones.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct TimecycleMods {
    pub mods: Vec<TimecycleMod>,
    by_hash: HashMap<u32, usize>,
}

impl TimecycleMods {
    /// Adds the modifiers of one file; returns how many it held.
    pub fn add_xml(&mut self, bytes: &[u8]) -> Result<usize> {
        let found: Vec<TimecycleMod> = xml::with_root(bytes, |root| {
            Ok(xml::children(root, "modifier")
                .into_iter()
                .map(|m| {
                    let name = xml::attr_string(m, "name");
                    TimecycleMod {
                        name_hash: joaat_lower(&name),
                        name,
                        num_mods: xml::attr_i32(m, "numMods"),
                        user_flags: xml::attr_i32(m, "userFlags"),
                        values: m
                            .children()
                            .filter(|c| c.is_element())
                            .map(|v| {
                                let text = xml::inner_text(v);
                                let parts: Vec<&str> = text.split_ascii_whitespace().collect();
                                let (value1, value2) = match parts[..] {
                                    [a, b] => (xml::parse_f32(a), xml::parse_f32(b)),
                                    _ => (0.0, 0.0),
                                };
                                ModValue { name: v.tag_name().name().to_string(), value1, value2 }
                            })
                            .collect(),
                    }
                })
                .collect())
        })?;
        let count = found.len();
        for m in found {
            match self.by_hash.get(&m.name_hash) {
                Some(&slot) => self.mods[slot] = m,
                None => {
                    self.by_hash.insert(m.name_hash, self.mods.len());
                    self.mods.push(m);
                }
            }
        }
        Ok(count)
    }

    pub fn parse(bytes: &[u8]) -> Result<TimecycleMods> {
        let mut mods = TimecycleMods::default();
        mods.add_xml(bytes)?;
        Ok(mods)
    }

    pub fn find_hash(&self, hash: u32) -> Option<&TimecycleMod> {
        self.by_hash.get(&hash).map(|&i| &self.mods[i])
    }

    /// By name, in any case.
    pub fn find(&self, name: &str) -> Option<&TimecycleMod> {
        self.find_hash(joaat_lower(name))
    }

    /// A file [`parse`](Self::parse) reads back to the same modifiers.
    pub fn to_xml(&self) -> String {
        let mut s = format!("{}<timecycle_mods version=\"1\">\n", xml::HEADER);
        for m in &self.mods {
            s += &format!(
                "  <modifier name=\"{}\" numMods=\"{}\" userFlags=\"{}\">\n",
                xml::esc(&m.name),
                m.num_mods,
                m.user_flags
            );
            for v in &m.values {
                s += &format!("    <{0}>{1} {2}</{0}>\n", v.name, v.value1, v.value2);
            }
            s += "  </modifier>\n";
        }
        s + "</timecycle_mods>\n"
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::env::EnvError;

    const BASE: &str = "<?xml version=\"1.0\"?>
<timecycle_mods version=\"1\">
  <!-- a comment between modifiers -->
  <modifier name=\"Underwater\" numMods=\"2\" userFlags=\"1\">
    <sky_hdr>1 0.5</sky_hdr>
    <fog_start>-3.5   7</fog_start>
    <three>1 2 3</three>
  </modifier>
  <modifier name=\"cs_dark\" numMods=\"0\" userFlags=\"0\"/>
  <other>ignored</other>
</timecycle_mods>
";

    #[test]
    fn modifiers_read_their_values_and_are_found_by_name_or_hash() {
        let m = TimecycleMods::parse(BASE.as_bytes()).unwrap();
        assert_eq!(m.mods.len(), 2);
        let u = m.find("underwater").expect("names are looked up in lower case");
        assert_eq!((u.name.as_str(), u.num_mods, u.user_flags), ("Underwater", 2, 1));
        assert_eq!(u.name_hash, crate::hash::joaat("underwater"));
        assert_eq!(
            u.values.iter().map(|v| (v.name.as_str(), v.value1, v.value2)).collect::<Vec<_>>(),
            [("sky_hdr", 1.0, 0.5), ("fog_start", -3.5, 7.0), ("three", 0.0, 0.0)],
            "a value that is not two numbers is zero"
        );
        assert!(std::ptr::eq(m.find_hash(u.name_hash).unwrap(), u));
        assert!(m.find("nothing").is_none());
        assert!(m.find("cs_dark").unwrap().values.is_empty());
    }

    #[test]
    fn a_later_file_replaces_a_modifier_in_place() {
        let mut m = TimecycleMods::parse(BASE.as_bytes()).unwrap();
        let n = m
            .add_xml(b"<m><modifier name=\"UNDERWATER\" numMods=\"1\"><sky_hdr>9 9</sky_hdr></modifier><modifier name=\"new\"/></m>")
            .unwrap();
        assert_eq!(n, 2);
        assert_eq!(
            m.mods.iter().map(|m| m.name.as_str()).collect::<Vec<_>>(),
            ["UNDERWATER", "cs_dark", "new"]
        );
        assert_eq!(m.find("underwater").unwrap().values[0].value1, 9.0);
        assert!(m.add_xml(b"RBF0....").is_err(), "a bad file leaves what was loaded alone");
        assert_eq!(m.mods.len(), 3);
    }

    #[test]
    fn built_files_read_back_and_bad_files_are_refused() {
        let m = TimecycleMods::parse(BASE.as_bytes()).unwrap();
        assert_eq!(TimecycleMods::parse(m.to_xml().as_bytes()).unwrap(), m);
        assert!(matches!(TimecycleMods::parse(b""), Err(EnvError::Empty)));
        assert!(matches!(TimecycleMods::parse(b"<a><modifier>"), Err(EnvError::Xml(_))));
        for len in 0..BASE.len() {
            let _ = TimecycleMods::parse(&BASE.as_bytes()[..len]);
        }
    }
}
