//! Newline-delimited JSON control channel.
//!
//! ```text
//! -> {"id":1,"cmd":"rpf.list","args":{"path":"x64a.rpf"}}
//! <- {"id":1,"ok":true,"result":{...}}
//! <- {"id":2,"ok":false,"error":{"code":"not_found","message":"..."}}
//! ```
//!
//! `{"cmd":"events.subscribe"}` additionally streams engine events as
//! `{"event":...}` lines. Over TCP the channel binds to loopback only and the
//! first line must be `{"auth":"<token>"}`: without it any web page could
//! POST command lines to `127.0.0.1` and extract files.

use std::{
    io::{self, BufRead, BufReader, Read, Write},
    net::{IpAddr, SocketAddr, TcpListener},
    sync::{
        Arc, Mutex,
        atomic::{AtomicUsize, Ordering},
    },
    thread,
    time::Duration,
};

use serde_json::{Value, json};

use crate::{CmdError, Engine};

pub fn handle_line(engine: &Engine, line: &str) -> Value {
    let req: Value = match serde_json::from_str(line) {
        Ok(v) => v,
        Err(e) => {
            return json!({ "id": null, "ok": false, "error": { "code": "parse", "message": e.to_string() } });
        }
    };
    let id = req.get("id").cloned().unwrap_or(Value::Null);
    let Some(cmd) = req.get("cmd").and_then(Value::as_str) else {
        return fail(id, CmdError::invalid("request needs a string `cmd`"));
    };
    match engine.execute(cmd, req.get("args").cloned().unwrap_or(Value::Null)) {
        Ok(result) => json!({ "id": id, "ok": true, "result": result }),
        Err(e) => fail(id, e),
    }
}

fn fail(id: Value, e: CmdError) -> Value {
    json!({ "id": id, "ok": false, "error": { "code": e.code, "message": e.message } })
}

/// Serves one client until EOF. Used for stdio and for each TCP connection.
pub fn serve<R: BufRead, W: Write + Send + 'static>(
    engine: &Arc<Engine>,
    reader: R,
    writer: W,
) -> io::Result<()> {
    let out = Arc::new(Mutex::new(writer));
    let mut subscribed = false;
    for line in reader.lines() {
        let line = line?;
        if line.trim().is_empty() {
            continue;
        }
        let is_sub = serde_json::from_str::<Value>(&line)
            .ok()
            .is_some_and(|v| v.get("cmd").and_then(Value::as_str) == Some("events.subscribe"));
        let reply = if is_sub {
            if !subscribed {
                subscribed = true;
                let rx = engine.subscribe();
                let out = out.clone();
                thread::spawn(move || {
                    for ev in rx {
                        let line = serde_json::to_string(&ev).unwrap_or_default();
                        if writeln!(out.lock().unwrap(), "{line}").is_err() {
                            break;
                        }
                    }
                });
            }
            json!({ "id": serde_json::from_str::<Value>(&line).ok().and_then(|v| v.get("id").cloned()), "ok": true, "result": { "subscribed": true } })
        } else {
            handle_line(engine, &line)
        };
        let mut w = out.lock().unwrap();
        writeln!(w, "{reply}")?;
        w.flush()?;
    }
    Ok(())
}

pub fn serve_stdio(engine: &Arc<Engine>) -> io::Result<()> {
    serve(engine, io::stdin().lock(), io::stdout())
}

pub fn random_token() -> String {
    let mut b = [0u8; 16];
    getrandom::fill(&mut b).expect("OS randomness unavailable");
    b.iter().map(|x| format!("{x:02x}")).collect()
}

/// How long a new connection may stay silent before it has to authenticate, and
/// how many may be open at once. Without these, anything on the machine (or any
/// web page that can reach loopback) could pile up idle threads.
#[derive(Debug, Clone, Copy)]
pub struct TcpLimits {
    pub auth_timeout: Duration,
    pub max_connections: usize,
}

impl Default for TcpLimits {
    fn default() -> Self {
        Self { auth_timeout: Duration::from_secs(5), max_connections: 16 }
    }
}

/// Listens on a loopback address; every connection must authenticate with `token`.
pub fn serve_tcp(engine: Arc<Engine>, addr: SocketAddr, token: String) -> io::Result<()> {
    if !is_loopback(addr.ip()) {
        return Err(io::Error::new(
            io::ErrorKind::InvalidInput,
            "the control channel only binds to loopback addresses",
        ));
    }
    serve_listener(engine, TcpListener::bind(addr)?, token, TcpLimits::default())
}

/// Frees a connection slot when the connection ends, however it ends.
struct Slot(Arc<AtomicUsize>);

impl Drop for Slot {
    fn drop(&mut self) {
        self.0.fetch_sub(1, Ordering::SeqCst);
    }
}

pub fn serve_listener(
    engine: Arc<Engine>,
    listener: TcpListener,
    token: String,
    limits: TcpLimits,
) -> io::Result<()> {
    let active = Arc::new(AtomicUsize::new(0));
    for stream in listener.incoming() {
        let Ok(stream) = stream else { continue };
        if active.fetch_add(1, Ordering::SeqCst) >= limits.max_connections {
            active.fetch_sub(1, Ordering::SeqCst);
            continue; // dropped: the client sees the connection close
        }
        let slot = Slot(active.clone());
        let (engine, token) = (engine.clone(), token.clone());
        thread::spawn(move || {
            let _slot = slot;
            let Ok(write_half) = stream.try_clone() else { return };
            // the clock only runs until the client has proved it knows the token
            if stream.set_read_timeout(Some(limits.auth_timeout)).is_err() {
                return;
            }
            let mut reader = BufReader::new(stream);
            let mut first = String::new();
            if reader.by_ref().take(512).read_line(&mut first).is_err() || !authorized(&first, &token) {
                return;
            }
            if reader.get_ref().set_read_timeout(None).is_err() {
                return;
            }
            let _ = serve(&engine, reader, write_half);
        });
    }
    Ok(())
}

fn is_loopback(ip: IpAddr) -> bool {
    ip.is_loopback()
}

fn authorized(line: &str, token: &str) -> bool {
    serde_json::from_str::<Value>(line.trim())
        .ok()
        .and_then(|v| {
            v.get("auth").and_then(Value::as_str).map(|t| constant_time_eq(t.as_bytes(), token.as_bytes()))
        })
        .unwrap_or(false)
}

fn constant_time_eq(a: &[u8], b: &[u8]) -> bool {
    a.len() == b.len() && a.iter().zip(b).fold(0u8, |acc, (x, y)| acc | (x ^ y)) == 0
}

#[cfg(test)]
mod tests {
    use super::*;

    fn engine() -> Arc<Engine> {
        Arc::new(Engine::ephemeral())
    }

    #[test]
    fn executes_a_command_and_echoes_the_id() {
        let r = handle_line(&engine(), r#"{"id":7,"cmd":"hash.joaat","args":{"text":"a"}}"#);
        assert_eq!(r["id"], 7);
        assert_eq!(r["ok"], true);
        assert_eq!(r["result"]["hex"], "0xCA2E9442");
    }

    #[test]
    fn reports_errors_with_stable_codes() {
        let e = engine();
        assert_eq!(handle_line(&e, "not json")["error"]["code"], "parse");
        assert_eq!(handle_line(&e, r#"{"cmd":"nope"}"#)["error"]["code"], "unknown_command");
        assert_eq!(handle_line(&e, r#"{"cmd":"hash.joaat"}"#)["error"]["code"], "invalid_args");
        assert_eq!(
            handle_line(&e, r#"{"cmd":"hash.joaat","args":{"text":"a","bogus":1}}"#)["error"]["code"],
            "invalid_args"
        );
        assert_eq!(handle_line(&e, r#"{"cmd":"rpf.list"}"#)["error"]["code"], "bad_state");
    }

    #[test]
    fn serves_a_whole_session_over_buffers() {
        let e = engine();
        let input = "{\"id\":1,\"cmd\":\"hash.joaat\",\"args\":{\"text\":\"\"}}\n\n{\"id\":2,\"cmd\":\"commands.list\"}\n";
        let out = Arc::new(Mutex::new(Vec::<u8>::new()));
        struct W(Arc<Mutex<Vec<u8>>>);
        impl Write for W {
            fn write(&mut self, b: &[u8]) -> io::Result<usize> {
                self.0.lock().unwrap().write(b)
            }
            fn flush(&mut self) -> io::Result<()> {
                Ok(())
            }
        }
        serve(&e, input.as_bytes(), W(out.clone())).unwrap();
        let text = String::from_utf8(out.lock().unwrap().clone()).unwrap();
        let lines: Vec<&str> = text.lines().collect();
        assert_eq!(lines.len(), 2);
        assert!(lines[1].contains("rpf.search"));
    }

    #[test]
    fn tcp_requires_loopback_and_a_valid_token() {
        let err = serve_tcp(engine(), "0.0.0.0:0".parse().unwrap(), "t".into()).unwrap_err();
        assert_eq!(err.kind(), io::ErrorKind::InvalidInput);
        assert!(authorized("{\"auth\":\"abc\"}\n", "abc"));
        assert!(!authorized("{\"auth\":\"abd\"}\n", "abc"));
        assert!(!authorized("POST / HTTP/1.1\r\n", "abc"));
        assert!(!authorized("{\"cmd\":\"rpf.extract\"}", "abc"));
        assert_eq!(random_token().len(), 32);
    }

    fn listen(limits: TcpLimits) -> SocketAddr {
        let listener = TcpListener::bind("127.0.0.1:0").unwrap();
        let addr = listener.local_addr().unwrap();
        thread::spawn(move || serve_listener(engine(), listener, "secret".into(), limits));
        addr
    }

    /// True when the server has closed the connection (EOF or reset) within `wait`.
    fn closed_within(stream: &mut std::net::TcpStream, wait: Duration) -> bool {
        stream.set_read_timeout(Some(wait)).unwrap();
        match stream.read(&mut [0u8; 1]) {
            Ok(n) => n == 0,
            Err(e) => !matches!(e.kind(), io::ErrorKind::WouldBlock | io::ErrorKind::TimedOut),
        }
    }

    #[test]
    fn a_silent_connection_is_dropped_and_an_authenticated_one_is_served() {
        let addr = listen(TcpLimits { auth_timeout: Duration::from_millis(150), max_connections: 4 });

        let mut idle = std::net::TcpStream::connect(addr).unwrap();
        assert!(closed_within(&mut idle, Duration::from_secs(3)), "no auth line: the server hangs up");

        let mut good = std::net::TcpStream::connect(addr).unwrap();
        writeln!(good, "{{\"auth\":\"secret\"}}").unwrap();
        // authenticated clients may think for longer than the auth window
        thread::sleep(Duration::from_millis(400));
        writeln!(good, "{{\"id\":1,\"cmd\":\"hash.joaat\",\"args\":{{\"text\":\"a\"}}}}").unwrap();
        let mut line = String::new();
        BufReader::new(good.try_clone().unwrap()).read_line(&mut line).unwrap();
        assert!(line.contains("0xCA2E9442"), "{line}");
    }

    #[test]
    fn connections_beyond_the_cap_are_refused_until_a_slot_frees_up() {
        let addr = listen(TcpLimits { auth_timeout: Duration::from_millis(800), max_connections: 1 });
        let first = std::net::TcpStream::connect(addr).unwrap(); // holds the only slot
        thread::sleep(Duration::from_millis(100));
        let mut second = std::net::TcpStream::connect(addr).unwrap();
        assert!(closed_within(&mut second, Duration::from_millis(500)), "over the cap: closed at once");
        drop(first);
        thread::sleep(Duration::from_millis(1000)); // the first one's slot is released when its thread ends
        let mut third = std::net::TcpStream::connect(addr).unwrap();
        writeln!(third, "{{\"auth\":\"secret\"}}").unwrap();
        writeln!(third, "{{\"id\":2,\"cmd\":\"commands.list\"}}").unwrap();
        let mut line = String::new();
        BufReader::new(third).read_line(&mut line).unwrap();
        assert!(line.contains("\"ok\":true"), "a freed slot is reusable: {line}");
    }
}
