use std::{fs::File, path::Path, sync::Arc};

use memmap2::Mmap;

use crate::Result;

/// Cheaply clonable, read-only bytes: either a memory-mapped file or an
/// in-memory buffer. Archives share one `Blob` with all of their nested
/// archives, so scanning never copies game data.
#[derive(Clone)]
pub struct Blob(Arc<dyn AsRef<[u8]> + Send + Sync>);

impl Blob {
    pub fn from_vec(v: Vec<u8>) -> Self {
        Blob(Arc::new(v))
    }

    pub fn map_file(path: &Path) -> Result<Self> {
        let file = File::open(path)?;
        // SAFETY: the mapping is read-only. As with any mmap, truncating the
        // file from another process while it is open is undefined behaviour;
        // the game does not modify archives in place while we read them.
        let map = unsafe { Mmap::map(&file)? };
        Ok(Blob(Arc::new(map)))
    }

    pub fn as_slice(&self) -> &[u8] {
        (*self.0).as_ref()
    }

    pub fn len(&self) -> usize {
        self.as_slice().len()
    }

    pub fn is_empty(&self) -> bool {
        self.len() == 0
    }
}

impl std::fmt::Debug for Blob {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "Blob({} bytes)", self.len())
    }
}
