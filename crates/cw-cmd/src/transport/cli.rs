//! Command-line front-end: `codewalker rpf.search --query adder`.
//!
//! Flags are generated from each command's parameters, so adding a command
//! adds its CLI automatically.

use serde_json::{Map, Value, json};

use crate::{CmdError, CommandSpec, Engine, ParamKind};

/// Turns `--key value` pairs into a JSON argument object, typed by the spec.
pub fn parse_args(spec: &CommandSpec, rest: &[String]) -> Result<Value, CmdError> {
    let mut obj = Map::new();
    let mut i = 0;
    while i < rest.len() {
        let raw = &rest[i];
        let Some(flag) = raw.strip_prefix("--") else {
            return Err(CmdError::invalid(format!(
                "unexpected argument `{raw}` (flags look like --name value)"
            )));
        };
        let (name, inline) = match flag.split_once('=') {
            Some((n, v)) => (n, Some(v.to_string())),
            None => (flag, None),
        };
        let name = name.replace('-', "_");
        let param = spec.params.iter().find(|p| p.name == name).ok_or_else(|| {
            CmdError::invalid(format!("`{}` has no option --{}", spec.name, name.replace('_', "-")))
        })?;
        let value = match (param.kind, inline) {
            (ParamKind::Boolean, Some(v)) => json!(parse_bool(&v)?),
            (ParamKind::Boolean, None) => {
                // `--flag` alone means true; `--flag false` is also accepted.
                match rest.get(i + 1).map(String::as_str) {
                    Some(v @ ("true" | "false")) => {
                        i += 1;
                        json!(v == "true")
                    }
                    _ => json!(true),
                }
            }
            (kind, inline) => {
                let v = match inline {
                    Some(v) => v,
                    None => {
                        i += 1;
                        rest.get(i).cloned().ok_or_else(|| {
                            CmdError::invalid(format!("--{} needs a value", name.replace('_', "-")))
                        })?
                    }
                };
                match kind {
                    ParamKind::Number => {
                        json!(v.parse::<f64>().ok().filter(|f| f.is_finite()).ok_or_else(|| {
                            CmdError::invalid(format!("--{} must be a number", name.replace('_', "-")))
                        })?)
                    }
                    ParamKind::Integer => json!(v.parse::<i64>().map_err(|_| CmdError::invalid(format!(
                        "--{} must be a number",
                        name.replace('_', "-")
                    )))?),
                    _ => json!(v),
                }
            }
        };
        obj.insert(param.name.to_string(), value);
        i += 1;
    }
    Ok(Value::Object(obj))
}

fn parse_bool(v: &str) -> Result<bool, CmdError> {
    match v {
        "true" | "1" | "yes" => Ok(true),
        "false" | "0" | "no" => Ok(false),
        _ => Err(CmdError::invalid(format!("`{v}` is not true/false"))),
    }
}

pub fn help(engine: &Engine, only: Option<&str>) -> String {
    let mut out = String::new();
    if let Some(name) = only {
        let Some(s) = engine.spec(name) else { return format!("unknown command `{name}`\n") };
        out.push_str(&format!("{}\n  {}\n", s.name, s.summary));
        for p in &s.params {
            out.push_str(&format!(
                "  --{:<14} {}{}\n",
                p.name.replace('_', "-"),
                p.description,
                if p.required { " (required)" } else { "" }
            ));
        }
        return out;
    }
    let mut last = "";
    for s in engine.specs() {
        if s.category != last {
            out.push_str(&format!("\n{}\n", s.category));
            last = s.category;
        }
        out.push_str(&format!("  {:<14} {}\n", s.name, s.summary));
    }
    out
}

#[cfg(test)]
mod tests {
    use super::*;

    fn spec(engine: &Engine, n: &str) -> CommandSpec {
        engine.spec(n).unwrap().clone()
    }

    #[test]
    fn flags_are_typed_by_the_spec() {
        let e = Engine::new();
        let s = spec(&e, "rpf.list");
        let v = parse_args(&s, &["--path".into(), "x64a.rpf".into(), "--limit=5".into()]).unwrap();
        assert_eq!(v, json!({ "path": "x64a.rpf", "limit": 5 }));
        assert!(parse_args(&s, &["--limit".into(), "many".into()]).is_err());
        assert!(parse_args(&s, &["--nope".into(), "1".into()]).is_err());
        assert!(parse_args(&s, &["stray".into()]).is_err());
        assert!(parse_args(&s, &["--path".into()]).is_err());
    }

    #[test]
    fn boolean_flags_default_to_true() {
        let e = Engine::new();
        let s = spec(&e, "hash.joaat");
        assert_eq!(
            parse_args(&s, &["--text".into(), "A".into(), "--lower".into()]).unwrap(),
            json!({ "text": "A", "lower": true })
        );
        assert_eq!(parse_args(&s, &["--lower".into(), "false".into()]).unwrap(), json!({ "lower": false }));
    }

    #[test]
    fn help_lists_every_command() {
        let e = Engine::new();
        let h = help(&e, None);
        for s in e.specs() {
            assert!(h.contains(s.name), "{} missing", s.name);
        }
        assert!(help(&e, Some("rpf.read")).contains("--length"));
    }
}
