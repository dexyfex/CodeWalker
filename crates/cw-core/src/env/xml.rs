//! A small layer over `roxmltree` that reads the way the C# `Xml` helpers (`CodeWalker.Core/Utils/Xml.cs`)
//! read: a missing element or attribute is a zero, an empty string or `false`, never an error, because
//! the game's files omit what does not apply. What it does refuse is hostile input: files over
//! [`MAX_XML_BYTES`], more than [`MAX_NODES`] nodes, nesting deeper than [`MAX_DEPTH`], DTDs (so no
//! entity tricks), and binary containers that merely have an `.xml` name.

use roxmltree::{Document, Node, ParsingOptions};

use super::{EnvError, Result};

/// The biggest XML file read. The game's are well under a megabyte.
pub const MAX_XML_BYTES: usize = 32 << 20;
const MAX_NODES: u32 = 1_000_000;
/// The game's files nest four or five levels; this leaves room for anything legitimate.
const MAX_DEPTH: usize = 64;

/// Parses `bytes` as UTF-8 XML (a leading byte order mark is dropped, invalid bytes become U+FFFD, as
/// `TextUtil.GetUTF8Text` does) and hands the root element to `f`.
pub fn with_root<T>(bytes: &[u8], f: impl FnOnce(Node<'_, '_>) -> Result<T>) -> Result<T> {
    if bytes.len() > MAX_XML_BYTES {
        return Err(EnvError::TooLarge(bytes.len()));
    }
    let body = bytes.strip_prefix(&[0xEF, 0xBB, 0xBF]).unwrap_or(bytes);
    if let Some(kind) = binary_kind(body) {
        return Err(EnvError::Binary(kind));
    }
    let text = String::from_utf8_lossy(body);
    if text.trim().is_empty() {
        return Err(EnvError::Empty);
    }
    let options = ParsingOptions { allow_dtd: false, nodes_limit: MAX_NODES, ..Default::default() };
    let doc = Document::parse_with_options(&text, options).map_err(|e| EnvError::Xml(e.to_string()))?;
    check_depth(&doc)?;
    f(doc.root_element())
}

/// The container a file starts as, when it is not text: the formats the game uses for its other
/// data (`RBF0` binary XML, `PSIN` PSO metadata, `RSC7` resources).
fn binary_kind(body: &[u8]) -> Option<&'static str> {
    match body.get(..4)? {
        b"RBF0" => Some("RBF (binary XML)"),
        b"PSIN" => Some("PSO"),
        b"RSC7" => Some("RSC7 resource"),
        _ => None,
    }
}

/// Depth of every node, parents before children (document order), in one pass: a deep file costs
/// memory proportional to its node count and nothing recurses on it.
fn check_depth(doc: &Document) -> Result<()> {
    let nodes = doc.descendants().count();
    let mut depth = vec![0u32; nodes + 1];
    for node in doc.descendants() {
        let d = node.parent().map_or(0, |p| depth[p.id().get_usize()] + 1);
        if d as usize > MAX_DEPTH {
            return Err(EnvError::Invalid(format!("elements are nested more than {MAX_DEPTH} levels deep")));
        }
        depth[node.id().get_usize()] = d;
    }
    Ok(())
}

pub type N<'a, 'i> = Node<'a, 'i>;

/// First child element called `name` (`SelectSingleNode(name)`).
pub fn child<'a, 'i>(n: N<'a, 'i>, name: &str) -> Option<N<'a, 'i>> {
    n.children().find(|c| c.is_element() && c.tag_name().name() == name)
}

/// Every child element called `name`.
pub fn children<'a, 'i>(n: N<'a, 'i>, name: &str) -> Vec<N<'a, 'i>> {
    n.children().filter(|c| c.is_element() && c.tag_name().name() == name).collect()
}

/// Every `Item` below every `group` child (`SelectNodes("group/Item")`).
pub fn items<'a, 'i>(n: N<'a, 'i>, group: &str) -> Vec<N<'a, 'i>> {
    children(n, group).into_iter().flat_map(|g| children(g, "Item")).collect()
}

/// All text below the node, as `InnerText` gives it (comments left out).
pub fn inner_text(n: N) -> String {
    n.descendants().filter(|d| d.is_text()).filter_map(|d| d.text()).collect()
}

/// `GetChildInnerText`: the text of the child called `name`, if there is one.
pub fn child_text(n: N, name: &str) -> Option<String> {
    child(n, name).map(inner_text)
}

pub fn child_string(n: N, name: &str) -> String {
    child_text(n, name).unwrap_or_default()
}

/// `FloatUtil.TryParse`: a number, or zero. Infinities and NaN count as unparseable, so a damaged value
/// cannot poison every colour it is later mixed into.
pub fn parse_f32(s: &str) -> f32 {
    s.trim().parse::<f32>().ok().filter(|f| f.is_finite()).unwrap_or(0.0)
}

pub fn parse_i32(s: &str) -> i32 {
    s.trim().parse().unwrap_or(0)
}

pub fn attr_string(n: N, name: &str) -> String {
    n.attribute(name).unwrap_or_default().to_string()
}

pub fn attr_f32(n: N, name: &str) -> f32 {
    n.attribute(name).map_or(0.0, parse_f32)
}

pub fn attr_i32(n: N, name: &str) -> i32 {
    n.attribute(name).map_or(0, parse_i32)
}

/// `GetChildFloatAttribute(node, name, "value")`: `<name value="1.5"/>`.
pub fn value_f32(n: N, name: &str) -> f32 {
    child(n, name).map_or(0.0, |c| attr_f32(c, "value"))
}

// ---- writing, for the builders that make test files

/// Text safe inside an element or an attribute.
pub fn esc(s: &str) -> String {
    s.replace('&', "&amp;").replace('<', "&lt;").replace('>', "&gt;").replace('"', "&quot;")
}

pub const HEADER: &str = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";

#[cfg(test)]
mod tests {
    use super::*;

    fn root_name(xml: &[u8]) -> Result<String> {
        with_root(xml, |r| Ok(r.tag_name().name().to_string()))
    }

    #[test]
    fn reads_text_with_or_without_a_byte_order_mark() {
        assert_eq!(root_name(b"<a/>").unwrap(), "a");
        assert_eq!(root_name(b"\xEF\xBB\xBF<?xml version=\"1.0\"?><b></b>").unwrap(), "b");
    }

    #[test]
    fn refuses_what_is_not_plain_xml() {
        assert!(matches!(root_name(b""), Err(EnvError::Empty)));
        assert!(matches!(root_name(b"  \n "), Err(EnvError::Empty)));
        assert!(matches!(root_name(b"RBF0\x01\x02"), Err(EnvError::Binary(k)) if k.starts_with("RBF")));
        assert!(matches!(root_name(b"PSIN\0\0"), Err(EnvError::Binary("PSO"))));
        assert!(matches!(root_name(b"RSC7\x05"), Err(EnvError::Binary(_))));
        assert!(matches!(root_name(b"<a><b></a>"), Err(EnvError::Xml(_))));
        assert!(matches!(root_name(b"no markup"), Err(EnvError::Xml(_))));
    }

    #[test]
    fn refuses_dtds_depth_and_size() {
        let dtd = b"<!DOCTYPE a [<!ENTITY x \"y\">]><a>&x;</a>";
        assert!(matches!(root_name(dtd), Err(EnvError::Xml(_))), "no entity definitions");
        let deep = format!("{}{}", "<a>".repeat(MAX_DEPTH + 2), "</a>".repeat(MAX_DEPTH + 2));
        assert!(matches!(root_name(deep.as_bytes()), Err(EnvError::Invalid(_))));
        let ok = format!("{}{}", "<a>".repeat(MAX_DEPTH), "</a>".repeat(MAX_DEPTH));
        assert!(root_name(ok.as_bytes()).is_ok());
        assert!(matches!(root_name(&vec![b' '; MAX_XML_BYTES + 1]), Err(EnvError::TooLarge(_))));
    }

    #[test]
    fn missing_pieces_read_as_zero_and_numbers_are_forgiving() {
        let xml = br#"<r><v value="2.5"/><t> one <!-- no --> two </t><bad value="inf"/><nan value="x"/></r>"#;
        with_root(xml, |r| {
            assert_eq!(value_f32(r, "v"), 2.5);
            assert_eq!(child_string(r, "t"), " one  two ");
            assert_eq!(value_f32(r, "missing"), 0.0);
            assert_eq!((value_f32(r, "bad"), value_f32(r, "nan")), (0.0, 0.0), "not finite, not a number");
            assert_eq!(child_string(r, "missing"), "");
            assert_eq!((parse_i32(" 7 "), parse_i32("x")), (7, 0));
            Ok(())
        })
        .unwrap();
    }

    #[test]
    fn group_items_and_escaping() {
        with_root(
            b"<r><G><Item n=\"1\"/><x/><Item n=\"2\"/></G><G><Item n=\"3\"/></G><Item n=\"no\"/></r>",
            |r| {
                let ns: Vec<_> = items(r, "G").iter().map(|n| attr_string(*n, "n")).collect();
                assert_eq!(ns, ["1", "2", "3"]);
                Ok(())
            },
        )
        .unwrap();
        assert_eq!(esc("a&b<c>\"d\""), "a&amp;b&lt;c&gt;&quot;d&quot;");
    }
}
