//! Bit-exact port of `System.Random(int seed)` (the Knuth subtractive
//! generator that .NET keeps for seeded instances). CodeWalker de-obfuscates
//! its embedded key blob with it, so the byte stream must match exactly.

const MBIG: i32 = i32::MAX;
const MSEED: i32 = 161_803_398;

pub struct DotNetRandom {
    seed_array: [i32; 56],
    inext: usize,
    inextp: usize,
}

impl DotNetRandom {
    pub fn new(seed: i32) -> Self {
        let mut seed_array = [0i32; 56];
        let subtraction = if seed == i32::MIN { i32::MAX } else { seed.abs() };
        let mut mj = MSEED - subtraction;
        seed_array[55] = mj;
        let mut mk = 1i32;
        for i in 1..55 {
            let ii = (21 * i) % 55;
            seed_array[ii] = mk;
            mk = mj - mk;
            if mk < 0 {
                mk += MBIG;
            }
            mj = seed_array[ii];
        }
        for _ in 1..5 {
            for i in 1..56 {
                seed_array[i] = seed_array[i].wrapping_sub(seed_array[1 + (i + 30) % 55]);
                if seed_array[i] < 0 {
                    seed_array[i] += MBIG;
                }
            }
        }
        Self { seed_array, inext: 0, inextp: 21 }
    }

    fn internal_sample(&mut self) -> i32 {
        self.inext += 1;
        if self.inext >= 56 {
            self.inext = 1;
        }
        self.inextp += 1;
        if self.inextp >= 56 {
            self.inextp = 1;
        }
        let mut ret = self.seed_array[self.inext].wrapping_sub(self.seed_array[self.inextp]);
        if ret == MBIG {
            ret -= 1;
        }
        if ret < 0 {
            ret += MBIG;
        }
        self.seed_array[self.inext] = ret;
        ret
    }

    /// `Random.Next()`.
    pub fn next_i32(&mut self) -> i32 {
        self.internal_sample()
    }

    /// `Random.NextBytes(buffer)`.
    pub fn next_bytes(&mut self, buf: &mut [u8]) {
        for b in buf {
            *b = self.internal_sample() as u8;
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    // Reference values produced by .NET (`new Random(seed).Next()`).
    #[test]
    fn matches_dotnet_for_known_seeds() {
        assert_eq!(DotNetRandom::new(42).next_i32(), 1_434_747_710);
        assert_eq!(DotNetRandom::new(0).next_i32(), 1_559_595_546);
    }

    #[test]
    fn negative_seed_uses_absolute_value() {
        let mut a = DotNetRandom::new(-1234);
        let mut b = DotNetRandom::new(1234);
        assert_eq!(a.next_i32(), b.next_i32());
    }

    #[test]
    fn next_bytes_is_low_byte_of_samples() {
        let mut a = DotNetRandom::new(7);
        let mut b = DotNetRandom::new(7);
        let mut buf = [0u8; 16];
        a.next_bytes(&mut buf);
        for byte in buf {
            assert_eq!(byte, b.next_i32() as u8);
        }
    }
}
