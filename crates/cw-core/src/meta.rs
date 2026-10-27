//! Resource META files: the container behind `.ymap`, `.ytyp` and other data files.
//!
//! A META resource is a list of *data blocks*. Each block has a type (the Jenkins hash of the
//! structure's exact-case name, or a small code for built-in types such as pointer lists) and a
//! run of bytes. Structures refer to each other with 8-byte pointers made of a 1-based block id
//! and an offset into that block, never with addresses.
//!
//! ```text
//! META root @ 0x50000000
//!   +0x1C  i32  root block id (1-based)
//!   +0x30  u64  address of the data block table
//!   +0x4C  i16  number of data blocks
//! data block (16 bytes)
//!   +0x00  u32  type hash          +0x04 i32 length          +0x08 u64 address of the bytes
//! pointer (u64)
//!   bits 0..12  block id (1-based, 0 = null)        bits 12..32  byte offset into the block
//! Array_StructurePointer (16 bytes)
//!   +0x00  u64  pointer to a block of type POINTER   +0x08 u16 count   +0x0A u16 count   +0x0C u32
//! ```
//!
//! Offsets are transcribed from `MetaTypes/Meta.cs` and `MetaTypes.cs` of the C# source. Nothing
//! here has been checked against real game files; the layout is exercised by synthetic ones.
//! Every address and length is checked against the resource before it is followed.

use crate::{
    error::{Error, Result},
    resource::{ResourceReader, SYSTEM_BASE},
    rpf::ResourceData,
};

/// Block type of a list of 8-byte pointers (`MetaTypeName.POINTER`).
pub const TYPE_POINTER: u32 = 0x07;

/// More blocks than this is not a META file (the count is a 16-bit field).
const MAX_BLOCKS: usize = 8192;

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct MetaBlock {
    pub type_hash: u32,
    pub data: Vec<u8>,
}

/// A decoded pointer: which block and where in it.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct MetaPointer {
    /// 1-based; 0 is null.
    pub block: u32,
    pub offset: u32,
}

impl MetaPointer {
    pub fn from_raw(raw: u64) -> Self {
        Self { block: (raw & 0xFFF) as u32, offset: ((raw >> 12) & 0xFFFFF) as u32 }
    }

    pub fn to_raw(self) -> u64 {
        ((self.offset as u64 & 0xFFFFF) << 12) | (self.block as u64 & 0xFFF)
    }
}

#[derive(Debug, Clone, Default, PartialEq, Eq)]
pub struct Meta {
    /// 1-based id of the block that holds the file's root structure.
    pub root_block: i32,
    pub blocks: Vec<MetaBlock>,
}

impl Meta {
    pub fn parse(res: &ResourceData) -> Result<Meta> {
        let r = ResourceReader::new(&res.data, res.sys_flags, res.gfx_flags);
        let root = SYSTEM_BASE;
        let table = r.u64_at(root + 0x30);
        let count = r.u16_at(root + 0x4C) as usize;
        if table == 0 || count == 0 || count > MAX_BLOCKS {
            return Err(Error::InvalidRpf("not a META resource: no usable data block table".into()));
        }
        if !r.contains(table, count * 16) {
            return Err(Error::InvalidRpf("META data block table lies outside the resource".into()));
        }

        let mut blocks = Vec::with_capacity(count);
        let mut budget = res.data.len();
        for i in 0..count {
            let rec = table + i as u64 * 16;
            let type_hash = r.u32_at(rec);
            let len = r.u32_at(rec + 4) as i32;
            let addr = r.u64_at(rec + 8);
            // the bytes must lie inside the resource, and all blocks together cannot exceed it:
            // a hostile table of overlapping giant blocks would otherwise multiply memory
            let data = match usize::try_from(len) {
                Ok(0) => Vec::new(),
                Ok(n) if n <= budget && r.contains(addr, n) => {
                    budget -= n;
                    r.bytes_at(addr, n).unwrap_or_default()
                }
                _ => Vec::new(),
            };
            blocks.push(MetaBlock { type_hash, data });
        }
        Ok(Meta { root_block: r.u32_at(root + 0x1C) as i32, blocks })
    }

    /// Block by 1-based id.
    pub fn block(&self, id: u32) -> Option<&MetaBlock> {
        (id as usize).checked_sub(1).and_then(|i| self.blocks.get(i))
    }

    pub fn root(&self) -> Option<&MetaBlock> {
        u32::try_from(self.root_block).ok().and_then(|id| self.block(id))
    }

    /// The first block of a type.
    pub fn first_of(&self, type_hash: u32) -> Option<&MetaBlock> {
        self.blocks.iter().find(|b| b.type_hash == type_hash)
    }

    /// The pointers of an `Array_StructurePointer` stored at `at` inside `data`. A list that is
    /// missing, of the wrong block type, or cut short yields what could be read.
    pub fn pointer_array(&self, data: &[u8], at: usize) -> Vec<MetaPointer> {
        let Some(head) = data.get(at..at + 16) else { return Vec::new() };
        let ptr = MetaPointer::from_raw(u64::from_le_bytes(head[..8].try_into().unwrap()));
        let count = u16::from_le_bytes([head[8], head[9]]) as usize;
        let Some(block) = self.block(ptr.block).filter(|b| b.type_hash == TYPE_POINTER) else {
            return Vec::new();
        };
        (0..count)
            .map_while(|i| {
                let from = ptr.offset as usize + i * 8;
                let raw = block.data.get(from..from + 8)?;
                Some(MetaPointer::from_raw(u64::from_le_bytes(raw.try_into().unwrap())))
            })
            .collect()
    }

    /// `size` bytes of a structure of type `type_hash` where `ptr` points; `None` when the pointer
    /// leads to another type of block, or past its end.
    pub fn structure(&self, ptr: MetaPointer, type_hash: u32, size: usize) -> Option<&[u8]> {
        let block = self.block(ptr.block).filter(|b| b.type_hash == type_hash)?;
        block.data.get(ptr.offset as usize..(ptr.offset as usize).checked_add(size)?)
    }
}

/// Writes a META resource holding `blocks` (type hash and bytes each), with `root_block` (1-based)
/// as the root. For tests and tools that make maps; the reader above is its inverse.
pub fn build_meta(root_block: i32, blocks: &[(u32, Vec<u8>)]) -> ResourceData {
    use crate::rpf::{flags_for_size, page_flags_size, resource_version};

    let align = |v: &mut Vec<u8>| {
        while !v.len().is_multiple_of(16) {
            v.push(0);
        }
    };
    let mut sys = vec![0u8; 0x70]; // the root structure
    align(&mut sys);
    let table_at = sys.len();
    sys.resize(table_at + blocks.len() * 16, 0);
    for (i, (type_hash, data)) in blocks.iter().enumerate() {
        align(&mut sys);
        let at = sys.len();
        sys.extend_from_slice(data);
        let rec = table_at + i * 16;
        sys[rec..rec + 4].copy_from_slice(&type_hash.to_le_bytes());
        sys[rec + 4..rec + 8].copy_from_slice(&(data.len() as i32).to_le_bytes());
        sys[rec + 8..rec + 16].copy_from_slice(&(SYSTEM_BASE + at as u64).to_le_bytes());
    }
    sys[0x10..0x14].copy_from_slice(&0x5052_4430u32.to_le_bytes());
    sys[0x14..0x16].copy_from_slice(&0x0079u16.to_le_bytes());
    sys[0x1C..0x20].copy_from_slice(&root_block.to_le_bytes());
    sys[0x30..0x38].copy_from_slice(&(SYSTEM_BASE + table_at as u64).to_le_bytes());
    sys[0x4C..0x4E].copy_from_slice(&(blocks.len() as u16).to_le_bytes());

    // META files are resource version 2: system nibble 0, graphics nibble 2, nothing in graphics
    let (sys_flags, gfx_flags) = (flags_for_size(sys.len(), 0), flags_for_size(0, 2));
    sys.resize(page_flags_size(sys_flags) as usize, 0);
    sys.resize(sys.len() + page_flags_size(gfx_flags) as usize, 0);
    ResourceData { sys_flags, gfx_flags, version: resource_version(sys_flags, gfx_flags), data: sys }
}

#[cfg(test)]
mod tests;
