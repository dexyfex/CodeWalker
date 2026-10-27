//! Self-describing commands: the same description feeds `--help`, the JSON
//! protocol's `commands` listing and the MCP `tools/list` schema.

use serde::Serialize;
use serde_json::{Value, json};

#[derive(Debug, Clone, Copy, Serialize, PartialEq, Eq)]
#[serde(rename_all = "lowercase")]
pub enum ParamKind {
    String,
    Path,
    Integer,
    Boolean,
}

#[derive(Debug, Clone, Serialize)]
pub struct ParamSpec {
    pub name: &'static str,
    pub kind: ParamKind,
    pub required: bool,
    pub description: &'static str,
}

impl ParamSpec {
    pub const fn new(name: &'static str, kind: ParamKind, description: &'static str) -> Self {
        Self { name, kind, required: false, description }
    }

    pub const fn required(mut self) -> Self {
        self.required = true;
        self
    }
}

#[derive(Debug, Clone, Serialize)]
pub struct CommandSpec {
    pub name: &'static str,
    pub category: &'static str,
    pub summary: &'static str,
    pub params: Vec<ParamSpec>,
    /// Changes files on disk or application state (as opposed to read-only).
    pub mutates: bool,
}

impl CommandSpec {
    pub fn new(name: &'static str, category: &'static str, summary: &'static str) -> Self {
        Self { name, category, summary, params: Vec::new(), mutates: false }
    }

    pub fn param(mut self, p: ParamSpec) -> Self {
        self.params.push(p);
        self
    }

    pub fn mutating(mut self) -> Self {
        self.mutates = true;
        self
    }

    /// JSON Schema for the arguments object (used by MCP).
    pub fn input_schema(&self) -> Value {
        let props: serde_json::Map<String, Value> = self
            .params
            .iter()
            .map(|p| {
                let ty = match p.kind {
                    ParamKind::String | ParamKind::Path => "string",
                    ParamKind::Integer => "integer",
                    ParamKind::Boolean => "boolean",
                };
                (p.name.to_string(), json!({ "type": ty, "description": p.description }))
            })
            .collect();
        let required: Vec<&str> = self.params.iter().filter(|p| p.required).map(|p| p.name).collect();
        json!({ "type": "object", "properties": props, "required": required, "additionalProperties": false })
    }
}
