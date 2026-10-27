//! Text helpers shared by every front-end.

/// Case-insensitive compare where digit runs compare by value (`a2` < `a10`).
/// Ties (`a1` vs `A1`, `x01` vs `x1`) fall back to the raw strings, so the
/// order is total and a sorted list never reshuffles between frames.
pub fn natural_cmp(a: &str, b: &str) -> std::cmp::Ordering {
    natural_key_cmp(a, b).then_with(|| a.cmp(b))
}

fn natural_key_cmp(a: &str, b: &str) -> std::cmp::Ordering {
    use std::cmp::Ordering;
    let (mut x, mut y) = (a.chars().peekable(), b.chars().peekable());
    loop {
        match (x.peek().copied(), y.peek().copied()) {
            (None, None) => return Ordering::Equal,
            (None, _) => return Ordering::Less,
            (_, None) => return Ordering::Greater,
            (Some(c), Some(d)) if c.is_ascii_digit() && d.is_ascii_digit() => {
                let take = |it: &mut std::iter::Peekable<std::str::Chars>| {
                    let mut s = String::new();
                    while let Some(&c) = it.peek().filter(|c| c.is_ascii_digit()) {
                        s.push(c);
                        it.next();
                    }
                    s
                };
                let (nx, ny) = (take(&mut x), take(&mut y));
                let (tx, ty) = (nx.trim_start_matches('0'), ny.trim_start_matches('0'));
                let ord = tx.len().cmp(&ty.len()).then_with(|| tx.cmp(ty));
                if ord != Ordering::Equal {
                    return ord;
                }
            }
            (Some(c), Some(d)) => {
                let ord = c.to_lowercase().cmp(d.to_lowercase());
                if ord != Ordering::Equal {
                    return ord;
                }
                x.next();
                y.next();
            }
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn natural_sort_compares_numbers_by_value() {
        let mut v = vec!["prop_10", "Prop_2", "prop_1", "alpha", "prop_01"];
        v.sort_by(|a, b| natural_cmp(a, b));
        assert_eq!(v, ["alpha", "prop_01", "prop_1", "Prop_2", "prop_10"]);
    }

    #[test]
    fn is_case_insensitive_and_total() {
        use std::cmp::Ordering::*;
        assert_eq!(natural_cmp("ABC", "ABC"), Equal);
        assert_eq!(natural_cmp("ABC", "abd"), Less, "case does not outrank letters");
        assert_eq!(natural_cmp("abc", "ABC"), Greater, "ties break on the raw text");
        assert_eq!(natural_cmp("a", "ab"), Less);
        assert_eq!(natural_cmp("", ""), Equal);
    }
}
