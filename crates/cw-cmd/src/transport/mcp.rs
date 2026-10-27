//! Model Context Protocol server over stdio (JSON-RPC 2.0, one message per
//! line). Every command becomes a tool, described by its [`CommandSpec`].

use std::io::{self, BufRead, Write};

use serde_json::{Value, json};

use crate::Engine;

const SUPPORTED: &[&str] = &["2025-06-18", "2025-03-26", "2024-11-05"];

pub fn serve<R: BufRead, W: Write>(engine: &Engine, reader: R, mut writer: W) -> io::Result<()> {
    for line in reader.lines() {
        let line = line?;
        if line.trim().is_empty() {
            continue;
        }
        if let Some(reply) = handle(engine, &line) {
            writeln!(writer, "{reply}")?;
            writer.flush()?;
        }
    }
    Ok(())
}

pub fn serve_stdio(engine: &Engine) -> io::Result<()> {
    serve(engine, io::stdin().lock(), io::stdout())
}

fn rpc_error(id: Value, code: i64, message: impl Into<String>) -> Value {
    json!({ "jsonrpc": "2.0", "id": id, "error": { "code": code, "message": message.into() } })
}

/// Returns the reply for a request, or `None` for notifications.
pub fn handle(engine: &Engine, line: &str) -> Option<Value> {
    let msg: Value = match serde_json::from_str(line) {
        Ok(v) => v,
        Err(e) => return Some(rpc_error(Value::Null, -32700, format!("parse error: {e}"))),
    };
    let id = msg.get("id").cloned();
    let method = msg.get("method").and_then(Value::as_str).unwrap_or("");
    let id = id?; // no id: a notification (e.g. notifications/initialized), which gets no reply
    let params = msg.get("params").cloned().unwrap_or(Value::Null);

    let result = match method {
        "initialize" => {
            let asked = params.get("protocolVersion").and_then(Value::as_str).unwrap_or("");
            let version = SUPPORTED.iter().find(|v| **v == asked).unwrap_or(&SUPPORTED[0]);
            json!({
                "protocolVersion": version,
                "capabilities": { "tools": { "listChanged": false } },
                "serverInfo": { "name": "codewalker", "version": env!("CARGO_PKG_VERSION") },
                "instructions": "Call `game_open` or `rpf_open` first, then browse with `rpf_list`, `rpf_search`, `rpf_read` and `rpf_extract`."
            })
        }
        "ping" => json!({}),
        "tools/list" => {
            let tools: Vec<Value> = engine
                .specs()
                .into_iter()
                .map(|s| {
                    json!({
                        "name": tool_name(s.name),
                        "description": s.summary,
                        "inputSchema": s.input_schema(),
                        "annotations": { "readOnlyHint": !s.mutates, "destructiveHint": false, "openWorldHint": false },
                    })
                })
                .collect();
            json!({ "tools": tools })
        }
        "tools/call" => {
            let name = params.get("name").and_then(Value::as_str).unwrap_or("");
            let Some(cmd) = engine.specs().into_iter().find(|s| tool_name(s.name) == name).map(|s| s.name)
            else {
                return Some(rpc_error(id, -32602, format!("unknown tool `{name}`")));
            };
            match engine.execute(cmd, params.get("arguments").cloned().unwrap_or(Value::Null)) {
                Ok(v) => {
                    json!({ "content": [{ "type": "text", "text": serde_json::to_string_pretty(&v).unwrap_or_default() }], "isError": false })
                }
                Err(e) => {
                    json!({ "content": [{ "type": "text", "text": format!("{}: {}", e.code, e.message) }], "isError": true })
                }
            }
        }
        other => return Some(rpc_error(id, -32601, format!("method not found: {other}"))),
    };
    Some(json!({ "jsonrpc": "2.0", "id": id, "result": result }))
}

/// MCP tool names allow `[A-Za-z0-9_-]`, so `rpf.list` becomes `rpf_list`.
pub fn tool_name(command: &str) -> String {
    command.replace('.', "_")
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn handshake_list_and_call() {
        let e = Engine::new();
        let init = handle(
            &e,
            r#"{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05"}}"#,
        )
        .unwrap();
        assert_eq!(init["result"]["protocolVersion"], "2024-11-05");
        assert!(handle(&e, r#"{"jsonrpc":"2.0","method":"notifications/initialized"}"#).is_none());

        let list = handle(&e, r#"{"jsonrpc":"2.0","id":2,"method":"tools/list"}"#).unwrap();
        let tools = list["result"]["tools"].as_array().unwrap();
        let search = tools.iter().find(|t| t["name"] == "rpf_search").unwrap();
        assert_eq!(search["inputSchema"]["required"][0], "query");
        assert_eq!(search["annotations"]["readOnlyHint"], true);

        let call = handle(&e, r#"{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"hash_joaat","arguments":{"text":"a"}}}"#).unwrap();
        assert_eq!(call["result"]["isError"], false);
        assert!(call["result"]["content"][0]["text"].as_str().unwrap().contains("0xCA2E9442"));

        let bad =
            handle(&e, r#"{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"rpf_list"}}"#)
                .unwrap();
        assert_eq!(bad["result"]["isError"], true);
        assert_eq!(
            handle(&e, r#"{"jsonrpc":"2.0","id":5,"method":"nope"}"#).unwrap()["error"]["code"],
            -32601
        );
    }
}
