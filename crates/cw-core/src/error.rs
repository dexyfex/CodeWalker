use std::io;

pub type Result<T> = std::result::Result<T, Error>;

#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("I/O error: {0}")]
    Io(#[from] io::Error),
    #[error("invalid RPF: {0}")]
    InvalidRpf(String),
    #[error("this archive is encrypted and no game keys are loaded")]
    MissingKeys,
    #[error("could not find the AES key in the executable (unsupported or modified game build?)")]
    KeyNotFound,
    #[error("invalid key data: {0}")]
    InvalidKeyData(String),
    #[error("could not decompress: {0}")]
    Decompress(String),
    #[error("entry is not a file")]
    NotAFile,
    #[error("entry not found: {0}")]
    NotFound(String),
}
