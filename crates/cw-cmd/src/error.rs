use serde::Serialize;

/// Stable, machine-readable failure of a command.
#[derive(Debug, Clone, Serialize, thiserror::Error)]
#[error("{code}: {message}")]
pub struct CmdError {
    pub code: &'static str,
    pub message: String,
}

impl CmdError {
    pub fn new(code: &'static str, message: impl Into<String>) -> Self {
        Self { code, message: message.into() }
    }

    pub fn invalid(message: impl Into<String>) -> Self {
        Self::new("invalid_args", message)
    }

    pub fn state(message: impl Into<String>) -> Self {
        Self::new("bad_state", message)
    }

    pub fn not_found(message: impl Into<String>) -> Self {
        Self::new("not_found", message)
    }
}

impl From<cw_core::Error> for CmdError {
    fn from(e: cw_core::Error) -> Self {
        let code = match &e {
            cw_core::Error::Io(_) => "io",
            cw_core::Error::MissingKeys | cw_core::Error::KeyNotFound | cw_core::Error::InvalidKeyData(_) => {
                "keys"
            }
            cw_core::Error::NotFound(_) => "not_found",
            _ => "archive",
        };
        Self::new(code, e.to_string())
    }
}

impl From<std::io::Error> for CmdError {
    fn from(e: std::io::Error) -> Self {
        Self::new("io", e.to_string())
    }
}

impl From<serde_json::Error> for CmdError {
    fn from(e: serde_json::Error) -> Self {
        Self::invalid(e.to_string())
    }
}

pub type CmdResult = Result<serde_json::Value, CmdError>;
