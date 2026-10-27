//! Game key material.
//!
//! No key ships with this project. The 32-byte AES key is located inside the
//! user's own `GTA5.exe` by matching SHA-1 hashes of 32-byte windows (the same
//! technique as CodeWalker). The remaining NG material (101 keys, 17x16 tables,
//! hash LUT, AWC key) is stored in `magic.dat`, which is only readable with
//! that AES key.

use std::{io::Read, path::Path};

use flate2::read::DeflateDecoder;
use rayon::prelude::*;
use sha1::{Digest, Sha1};

use crate::{
    crypto::decrypt_aes,
    dotnet_random::DotNetRandom,
    error::{Error, Result},
    hash::joaat_bytes,
};

/// Obfuscated key blob taken from the original CodeWalker resources.
static MAGIC: &[u8] = include_bytes!("../data/magic.dat");

/// SHA-1 of the 32-byte AES key (the key itself is not stored anywhere).
const PC_AES_KEY_HASH: [u8; 20] = [
    0xA0, 0x79, 0x61, 0x28, 0xA7, 0x75, 0x72, 0x0A, 0xC2, 0x04, 0xD9, 0x81, 0x9F, 0x68, 0xC1, 0x72, 0xE3,
    0x95, 0x2C, 0x6D,
];

const NG_KEY_COUNT: usize = 101;
const NG_KEY_LEN: usize = 272;
const NG_ROUNDS: usize = 17;
const NG_KEYS_BYTES: usize = NG_KEY_COUNT * NG_KEY_LEN; // 27472
const NG_TABLES_BYTES: usize = NG_ROUNDS * 16 * 256 * 4; // 278528

pub type NgTables = [[[u32; 256]; 16]; NG_ROUNDS];

pub struct GtaKeys {
    pub aes_key: [u8; 32],
    pub ng_keys: Vec<[u8; NG_KEY_LEN]>,
    pub ng_tables: Box<NgTables>,
    pub lut: [u8; 256],
    pub awc_key: [u32; 4],
}

impl std::fmt::Debug for GtaKeys {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_str("GtaKeys { .. }") // never print key material
    }
}

impl GtaKeys {
    /// Derives every key from the executable's contents.
    pub fn from_exe_bytes(exe: &[u8]) -> Result<Self> {
        let aes = find_aes_key(exe).ok_or(Error::KeyNotFound)?;
        Self::from_aes_key(aes)
    }

    /// `gta5.exe` / `gta5_enhanced.exe`.
    pub fn from_exe_path(path: &Path) -> Result<Self> {
        let blob = crate::Blob::map_file(path)?;
        Self::from_exe_bytes(blob.as_slice())
    }

    /// Accepts the AES key as base64 (CodeWalker's "key" option).
    pub fn from_base64_key(b64: &str) -> Result<Self> {
        use base64::Engine;
        let bytes = base64::engine::general_purpose::STANDARD
            .decode(b64.trim())
            .map_err(|e| Error::InvalidKeyData(e.to_string()))?;
        let key: [u8; 32] =
            bytes.try_into().map_err(|_| Error::InvalidKeyData("AES key must be 32 bytes".into()))?;
        Self::from_aes_key(key)
    }

    /// Unpacks the embedded `magic.dat` with a known AES key
    /// (`GTA5Keys.UseMagicData`).
    pub fn from_aes_key(aes_key: [u8; 32]) -> Result<Self> {
        let mut rnd = DotNetRandom::new(joaat_bytes(&aes_key) as i32);
        let n = MAGIC.len();
        let mut rb = [vec![0u8; n], vec![0u8; n], vec![0u8; n], vec![0u8; n]];
        for r in rb.iter_mut() {
            rnd.next_bytes(r);
        }
        let mut db = MAGIC.to_vec();
        for (i, b) in db.iter_mut().enumerate() {
            *b =
                b.wrapping_sub(rb[0][i]).wrapping_sub(rb[1][i]).wrapping_sub(rb[2][i]).wrapping_sub(rb[3][i]);
        }
        let db = decrypt_aes(&db, &aes_key);

        let mut inflated = Vec::new();
        DeflateDecoder::new(db.as_slice()).read_to_end(&mut inflated).map_err(|e| {
            Error::InvalidKeyData(format!("magic data did not inflate (wrong AES key?): {e}"))
        })?;
        if inflated.len() < NG_KEYS_BYTES + NG_TABLES_BYTES + 256 + 16 {
            return Err(Error::InvalidKeyData("magic data is too short".into()));
        }

        let (keys_b, rest) = inflated.split_at(NG_KEYS_BYTES);
        let (tables_b, rest) = rest.split_at(NG_TABLES_BYTES);
        let (lut_b, rest) = rest.split_at(256);

        let ng_keys =
            keys_b.chunks_exact(NG_KEY_LEN).map(|c| <[u8; NG_KEY_LEN]>::try_from(c).unwrap()).collect();
        let mut ng_tables: Box<NgTables> =
            vec![[[0u32; 256]; 16]; NG_ROUNDS].into_boxed_slice().try_into().unwrap();
        let mut words = tables_b.chunks_exact(4).map(|c| u32::from_le_bytes(c.try_into().unwrap()));
        for round in ng_tables.iter_mut() {
            for table in round.iter_mut() {
                for v in table.iter_mut() {
                    *v = words.next().unwrap();
                }
            }
        }
        let mut awc_key = [0u32; 4];
        for (i, v) in awc_key.iter_mut().enumerate() {
            *v = u32::from_le_bytes(rest[i * 4..i * 4 + 4].try_into().unwrap());
        }

        Ok(Self { aes_key, ng_keys, ng_tables, lut: lut_b.try_into().unwrap(), awc_key })
    }

    /// Like [`synthetic`](Self::synthetic), but the NG tables make every round an invertible
    /// byte permutation plus a key XOR, so [`crate::crypto::encrypt_ng_synthetic`] can build
    /// data that the real decryption code turns back into the original. The real tables have no
    /// inverse here. Tests only.
    #[doc(hidden)]
    pub fn synthetic_invertible() -> Self {
        let mut k = Self::synthetic();
        for round in k.ng_tables.iter_mut() {
            for (j, table) in round.iter_mut().enumerate() {
                for (b, v) in table.iter_mut().enumerate() {
                    *v = (b as u32) << (8 * (j % 4));
                }
            }
        }
        k
    }

    /// Deterministic fake material for tests. Decrypts nothing real.
    #[doc(hidden)]
    pub fn synthetic() -> Self {
        let mut rnd = DotNetRandom::new(1234);
        let mut aes_key = [0u8; 32];
        rnd.next_bytes(&mut aes_key);
        let mut ng_keys = vec![[0u8; NG_KEY_LEN]; NG_KEY_COUNT];
        for k in ng_keys.iter_mut() {
            rnd.next_bytes(k);
        }
        let mut ng_tables: Box<NgTables> =
            vec![[[0u32; 256]; 16]; NG_ROUNDS].into_boxed_slice().try_into().unwrap();
        for round in ng_tables.iter_mut() {
            for table in round.iter_mut() {
                for v in table.iter_mut() {
                    *v = rnd.next_i32() as u32;
                }
            }
        }
        let mut lut = [0u8; 256];
        rnd.next_bytes(&mut lut);
        Self { aes_key, ng_keys, ng_tables, lut, awc_key: [1, 2, 3, 4] }
    }
}

/// Scans `exe` for the 32-byte window (8-byte aligned) whose SHA-1 equals the
/// known AES key hash. Runs on all cores; a 100 MB executable takes a second
/// or two.
pub fn find_aes_key(exe: &[u8]) -> Option<[u8; 32]> {
    if exe.len() < 32 {
        return None;
    }
    let windows = (exe.len() - 32) / 8 + 1;
    (0..windows).into_par_iter().find_map_any(|i| {
        let w = &exe[i * 8..i * 8 + 32];
        let digest = Sha1::digest(w);
        (digest.as_slice() == PC_AES_KEY_HASH).then(|| <[u8; 32]>::try_from(w).unwrap())
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn magic_blob_has_expected_shape() {
        // 27472 + 278528 + 256 + 16 bytes once de-obfuscated, deflated and
        // AES-encrypted; the stored form is longer than the raw payload only
        // by padding, never shorter than a few KB.
        assert!(MAGIC.len() > 1024);
        assert_eq!(NG_KEYS_BYTES, 27472);
        assert_eq!(NG_TABLES_BYTES, 278528);
    }

    #[test]
    fn wrong_aes_key_is_rejected_cleanly() {
        let err = GtaKeys::from_aes_key([7u8; 32]).unwrap_err();
        assert!(matches!(err, Error::InvalidKeyData(_)), "{err:?}");
    }

    #[test]
    fn aes_key_search_ignores_noise() {
        let noise: Vec<u8> = (0..4096u32).map(|i| (i * 31 % 251) as u8).collect();
        assert!(find_aes_key(&noise).is_none());
        assert!(find_aes_key(&[0u8; 8]).is_none());
    }

    #[test]
    fn base64_key_must_be_32_bytes() {
        assert!(matches!(GtaKeys::from_base64_key("AAAA"), Err(Error::InvalidKeyData(_))));
        assert!(matches!(GtaKeys::from_base64_key("!!!"), Err(Error::InvalidKeyData(_))));
    }
}
