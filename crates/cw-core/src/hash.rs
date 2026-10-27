//! Hash functions used by the game and by CodeWalker.

/// Jenkins one-at-a-time hash ("joaat") over UTF-16 code units truncated to a
/// byte. Matches `JenkHash.GenHash(string)` in the C# code base, which does
/// not lower-case its input.
pub fn joaat(text: &str) -> u32 {
    let mut h = 0u32;
    for unit in text.encode_utf16() {
        h = step(h, unit as u8);
    }
    finish(h)
}

/// `joaat` over raw bytes (`JenkHash.GenHash(byte[])`).
pub fn joaat_bytes(data: &[u8]) -> u32 {
    let mut h = 0u32;
    for &b in data {
        h = step(h, b);
    }
    finish(h)
}

/// `joaat` of the lower-cased text, as used for file names and game hashes.
pub fn joaat_lower(text: &str) -> u32 {
    joaat(&text.to_lowercase())
}

#[inline]
fn step(mut h: u32, byte: u8) -> u32 {
    h = h.wrapping_add(byte as u32);
    h = h.wrapping_add(h << 10);
    h ^ (h >> 6)
}

#[inline]
fn finish(mut h: u32) -> u32 {
    h = h.wrapping_add(h << 3);
    h ^= h >> 11;
    h.wrapping_add(h << 15)
}

/// Hash used to pick the NG key of an archive entry (`GTA5Hash.CalculateHash`).
/// `lut` is the 256-byte hash lookup table recovered from the game keys.
pub fn ng_name_hash(text: &str, lut: &[u8; 256]) -> u32 {
    let mut result = 0u32;
    for unit in text.encode_utf16() {
        let temp = 1025u32.wrapping_mul((lut[(unit & 0xFF) as usize] as u32).wrapping_add(result));
        result = (temp >> 6) ^ temp;
    }
    let r9 = result.wrapping_mul(9);
    32769u32.wrapping_mul((r9 >> 11) ^ r9)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn joaat_known_vectors() {
        assert_eq!(joaat(""), 0);
        assert_eq!(joaat("a"), 0xCA2E_9442);
        assert_eq!(joaat("The quick brown fox jumps over the lazy dog"), 0x519E_91F5);
    }

    #[test]
    fn joaat_bytes_matches_str() {
        assert_eq!(joaat_bytes(b"adder"), joaat("adder"));
    }

    #[test]
    fn ng_hash_is_deterministic_and_lut_dependent() {
        let mut lut = [0u8; 256];
        for (i, v) in lut.iter_mut().enumerate() {
            *v = (i as u8).wrapping_mul(31).wrapping_add(7);
        }
        let a = ng_name_hash("common.rpf", &lut);
        assert_eq!(a, ng_name_hash("common.rpf", &lut));
        lut[b'c' as usize] ^= 1;
        assert_ne!(a, ng_name_hash("common.rpf", &lut));
    }
}
