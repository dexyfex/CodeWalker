//! AES (ECB) and "NG" decryption of RPF data.
//!
//! Only decryption is ported for now; NG *encryption* needs the inverse
//! tables that CodeWalker derives with a Gauss solver and is not required for
//! reading archives or for writing "OPEN" (unencrypted) archives.

use aes::Aes256;
use aes::cipher::{BlockCipherDecrypt, BlockCipherEncrypt, KeyInit};

use crate::keys::GtaKeys;

/// AES-256-ECB decrypt. A trailing partial block is left untouched, exactly
/// like the C# implementation.
pub fn decrypt_aes(data: &[u8], key: &[u8; 32]) -> Vec<u8> {
    let cipher = Aes256::new(key.into());
    let mut out = data.to_vec();
    for chunk in out.chunks_exact_mut(16) {
        let block: &mut [u8; 16] = chunk.try_into().unwrap();
        cipher.decrypt_block(block.into());
    }
    out
}

/// AES-256-ECB encrypt (used for tests and for writing AES archives).
pub fn encrypt_aes(data: &[u8], key: &[u8; 32]) -> Vec<u8> {
    let cipher = Aes256::new(key.into());
    let mut out = data.to_vec();
    for chunk in out.chunks_exact_mut(16) {
        let block: &mut [u8; 16] = chunk.try_into().unwrap();
        cipher.encrypt_block(block.into());
    }
    out
}

/// Selects the NG key for `name`/`length` (`GTACrypto.GetNGKey`).
pub fn ng_key<'k>(keys: &'k GtaKeys, name: &str, length: u32) -> &'k [u8; 272] {
    let hash = crate::hash::ng_name_hash(name, &keys.lut);
    let idx = hash.wrapping_add(length).wrapping_add(101 - 40) % 0x65;
    &keys.ng_keys[idx as usize]
}

/// NG decrypt of `data` for the entry called `name` whose size is `length`.
pub fn decrypt_ng(data: &[u8], name: &str, length: u32, keys: &GtaKeys) -> Vec<u8> {
    decrypt_ng_with_key(data, ng_key(keys, name, length), keys)
}

pub fn decrypt_ng_with_key(data: &[u8], key: &[u8; 272], keys: &GtaKeys) -> Vec<u8> {
    let mut subkeys = [[0u32; 4]; 17];
    for (i, sk) in subkeys.iter_mut().enumerate() {
        for (j, v) in sk.iter_mut().enumerate() {
            let o = (4 * i + j) * 4;
            *v = u32::from_le_bytes([key[o], key[o + 1], key[o + 2], key[o + 3]]);
        }
    }

    let mut out = data.to_vec();
    for chunk in out.chunks_exact_mut(16) {
        let block: [u8; 16] = (&*chunk).try_into().unwrap();
        chunk.copy_from_slice(&decrypt_ng_block(block, &subkeys, keys));
    }
    out
}

fn decrypt_ng_block(mut b: [u8; 16], sk: &[[u32; 4]; 17], keys: &GtaKeys) -> [u8; 16] {
    let t = &keys.ng_tables;
    b = round_a(b, &sk[0], &t[0]);
    b = round_a(b, &sk[1], &t[1]);
    for k in 2..=15 {
        b = round_b(b, &sk[k], &t[k]);
    }
    round_a(b, &sk[16], &t[16])
}

#[inline]
fn pack(x: [u32; 4]) -> [u8; 16] {
    let mut r = [0u8; 16];
    for (i, v) in x.iter().enumerate() {
        r[i * 4..i * 4 + 4].copy_from_slice(&v.to_le_bytes());
    }
    r
}

// rounds 1, 2 and 17
#[inline]
fn round_a(d: [u8; 16], k: &[u32; 4], t: &[[u32; 256]; 16]) -> [u8; 16] {
    let mut x = [0u32; 4];
    for (w, xw) in x.iter_mut().enumerate() {
        let o = w * 4;
        *xw = t[o][d[o] as usize]
            ^ t[o + 1][d[o + 1] as usize]
            ^ t[o + 2][d[o + 2] as usize]
            ^ t[o + 3][d[o + 3] as usize]
            ^ k[w];
    }
    pack(x)
}

// rounds 3 to 16
#[inline]
fn round_b(d: [u8; 16], k: &[u32; 4], t: &[[u32; 256]; 16]) -> [u8; 16] {
    let x = [
        t[0][d[0] as usize] ^ t[7][d[7] as usize] ^ t[10][d[10] as usize] ^ t[13][d[13] as usize] ^ k[0],
        t[1][d[1] as usize] ^ t[4][d[4] as usize] ^ t[11][d[11] as usize] ^ t[14][d[14] as usize] ^ k[1],
        t[2][d[2] as usize] ^ t[5][d[5] as usize] ^ t[8][d[8] as usize] ^ t[15][d[15] as usize] ^ k[2],
        t[3][d[3] as usize] ^ t[6][d[6] as usize] ^ t[9][d[9] as usize] ^ t[12][d[12] as usize] ^ k[3],
    ];
    pack(x)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn aes_roundtrip_and_partial_block_untouched() {
        let key = [0x42u8; 32];
        let plain: Vec<u8> = (0..53u8).collect();
        let enc = encrypt_aes(&plain, &key);
        assert_ne!(&enc[..48], &plain[..48]);
        assert_eq!(&enc[48..], &plain[48..]);
        assert_eq!(decrypt_aes(&enc, &key), plain);
    }

    #[test]
    fn aes_matches_fips197_vector() {
        // FIPS-197 appendix C.3 (AES-256).
        let key: [u8; 32] = core::array::from_fn(|i| i as u8);
        let plain: [u8; 16] = core::array::from_fn(|i| (i * 0x11) as u8);
        let expected =
            [0x8e, 0xa2, 0xb7, 0xca, 0x51, 0x67, 0x45, 0xbf, 0xea, 0xfc, 0x49, 0x90, 0x4b, 0x49, 0x60, 0x89];
        assert_eq!(encrypt_aes(&plain, &key), expected);
        assert_eq!(decrypt_aes(&expected, &key), plain);
    }

    /// Straightforward reference implementation of one NG block, used to check
    /// the unrolled rounds above against the original table layout.
    fn reference_block(data: [u8; 16], sk: &[[u32; 4]; 17], keys: &GtaKeys) -> [u8; 16] {
        let mut b = data;
        for (round, t) in keys.ng_tables.iter().enumerate() {
            let mut x = [0u32; 4];
            let a_round = matches!(round, 0 | 1 | 16);
            let idx: [[usize; 4]; 4] = if a_round {
                [[0, 1, 2, 3], [4, 5, 6, 7], [8, 9, 10, 11], [12, 13, 14, 15]]
            } else {
                [[0, 7, 10, 13], [1, 4, 11, 14], [2, 5, 8, 15], [3, 6, 9, 12]]
            };
            for w in 0..4 {
                x[w] = sk[round][w];
                for &i in &idx[w] {
                    x[w] ^= t[i][b[i] as usize];
                }
            }
            b = pack(x);
        }
        b
    }

    #[test]
    fn ng_block_matches_reference() {
        let keys = GtaKeys::synthetic();
        let key = &keys.ng_keys[3];
        let mut sk = [[0u32; 4]; 17];
        for (i, s) in sk.iter_mut().enumerate() {
            for (j, v) in s.iter_mut().enumerate() {
                let o = (4 * i + j) * 4;
                *v = u32::from_le_bytes(key[o..o + 4].try_into().unwrap());
            }
        }
        let data: Vec<u8> = (0..40u8).map(|i| i.wrapping_mul(13)).collect();
        let out = decrypt_ng_with_key(&data, key, &keys);
        for (blk, chunk) in data.chunks_exact(16).enumerate() {
            let want = reference_block(chunk.try_into().unwrap(), &sk, &keys);
            assert_eq!(&out[blk * 16..blk * 16 + 16], &want);
        }
        // trailing partial block passes through untouched
        assert_eq!(&out[32..], &data[32..]);
    }
}
