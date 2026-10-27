//! Small, pure formatting helpers shared by the views.

pub use cw_core::text::natural_cmp;

/// `1,234,567` — thousands separators.
pub fn thousands(n: u64) -> String {
    let s = n.to_string();
    let mut out = String::with_capacity(s.len() + s.len() / 3);
    for (i, c) in s.chars().enumerate() {
        if i > 0 && (s.len() - i) % 3 == 0 {
            out.push(',');
        }
        out.push(c);
    }
    out
}

/// Binary units with one decimal: `0 B`, `1.5 KiB`, `12.0 MiB`.
pub fn size(bytes: u64) -> String {
    const UNITS: [&str; 5] = ["B", "KiB", "MiB", "GiB", "TiB"];
    if bytes < 1024 {
        return format!("{bytes} B");
    }
    let mut v = bytes as f64;
    let mut u = 0;
    while v >= 1024.0 && u < UNITS.len() - 1 {
        v /= 1024.0;
        u += 1;
    }
    format!("{v:.1} {}", UNITS[u])
}

/// `1 file`, `2 files`.
pub fn plural(n: u64, one: &str, many: &str) -> String {
    format!("{} {}", thousands(n), if n == 1 { one } else { many })
}

/// Shortens `text` to at most `max` characters by replacing the middle with
/// an ellipsis. Siblings often differ at the end, so the end is kept.
pub fn middle_truncate(text: &str, max: usize) -> String {
    let chars: Vec<char> = text.chars().collect();
    if chars.len() <= max || max < 3 {
        return text.to_string();
    }
    let keep = max - 1;
    let tail = keep / 2;
    let head = keep - tail;
    let mut out: String = chars[..head].iter().collect();
    out.push('…');
    out.extend(&chars[chars.len() - tail..]);
    out
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn thousands_groups_digits() {
        assert_eq!(thousands(0), "0");
        assert_eq!(thousands(999), "999");
        assert_eq!(thousands(1000), "1,000");
        assert_eq!(thousands(1_234_567), "1,234,567");
    }

    #[test]
    fn sizes_use_binary_units() {
        assert_eq!(size(0), "0 B");
        assert_eq!(size(1023), "1023 B");
        assert_eq!(size(1536), "1.5 KiB");
        assert_eq!(size(12 * 1024 * 1024), "12.0 MiB");
        assert_eq!(size(u64::MAX), "16777216.0 TiB");
    }

    #[test]
    fn plurals_follow_a_rule() {
        assert_eq!(plural(1, "file", "files"), "1 file");
        assert_eq!(plural(0, "file", "files"), "0 files");
        assert_eq!(plural(12_483, "file", "files"), "12,483 files");
    }

    #[test]
    fn middle_truncation_keeps_the_end() {
        assert_eq!(middle_truncate("short.ydr", 20), "short.ydr");
        let t = middle_truncate("prop_very_long_name_variant_07.ydr", 16);
        assert_eq!(t.chars().count(), 16);
        assert_eq!(t, "prop_ver…_07.ydr");
        assert_eq!(middle_truncate("日本語のファイル名前.ydr", 8).chars().count(), 8);
        assert_eq!(middle_truncate("abc", 2), "abc");
    }
}
