
/*
    Copyright(c) 2015 Neodymium

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

//shamelessly stolen

using CommunityToolkit.HighPerformance.Helpers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CodeWalker.GameFiles
{
    [SkipLocalsInit]
    public class GTACrypto
    {

        public static byte[] DecryptAES(byte[] data, int length)
        {
            return DecryptAESData(data, GTA5Keys.PC_AES_KEY, length, 1);
        }
        public static byte[] DecryptAES(byte[] data)
        {
            return DecryptAESData(data, GTA5Keys.PC_AES_KEY);
        }
        public static byte[] EncryptAES(byte[] data)
        {
            return EncryptAESData(data, GTA5Keys.PC_AES_KEY);
        }

        public static byte[] DecryptAESData(byte[] data, byte[] key, int rounds = 1)
        {
            return DecryptAESData(data, key, data.Length, rounds);
        }

        public static byte[] DecryptAESData(byte[] data, byte[] key, int length, int rounds = 1)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            //var rijndael = Rijndael.Create();
            //rijndael.KeySize = 256;
            //rijndael.Key = key;
            //rijndael.BlockSize = 128;
            //rijndael.Mode = CipherMode.ECB;
            //rijndael.Padding = PaddingMode.None;

            length = length - length % 16;

            // decrypt...
            if (length > 0)
            {
                using var decryptorAes = aes.CreateDecryptor();
                for (var roundIndex = 0; roundIndex < rounds; roundIndex++)
                {
                    decryptorAes.TransformBlock(data, 0, length, data, 0);
                }

                //var decryptor = rijndael.CreateDecryptor();
                //for (var roundIndex = 0; roundIndex < rounds; roundIndex++)
                //    decryptor.TransformBlock(data, 0, length, data, 0);
            }

            return data;
        }
        public static byte[] EncryptAESData(byte[] data, byte[] key, int rounds = 1)
        {
            var rijndael = Rijndael.Create();
            rijndael.KeySize = 256;
            rijndael.Key = key;
            rijndael.BlockSize = 128;
            rijndael.Mode = CipherMode.ECB;
            rijndael.Padding = PaddingMode.None;

            var buffer = (byte[])data.Clone();
            var length = data.Length - data.Length % 16;

            // encrypt...
            if (length > 0)
            {
                var encryptor = rijndael.CreateEncryptor();
                for (var roundIndex = 0; roundIndex < rounds; roundIndex++)
                {
                    encryptor.TransformBlock(buffer, 0, length, buffer, 0);
                }
            }

            return buffer;
        }




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint[][] GetNGKey(string name, uint length)
        {
            uint hash = GTA5Hash.CalculateHash(name);
            uint keyidx = (hash + length + (101 - 40)) % 0x65;
            return GTA5Keys.PC_NG_KEYS[keyidx];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecryptNG(byte[] data, string name, uint fileSize, int offset = 0, int? length = null)
        {
            var key = GetNGKey(name, fileSize);
            length ??= data.Length;
            DecryptNG(data.AsMemory(offset, length.Value), key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecryptNG(Memory<byte> data, string name, uint fileSize)
        {
            var key = GetNGKey(name, fileSize);
            DecryptNG(data, key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void DecryptNG(byte[] data, uint[][] key, int offset = 0, int? length = null)
        {
            length ??= data.Length;
            DecryptNG(data.AsMemory(offset, length.Value), key);
        }

        public readonly struct DecryptNGBLock : IAction
        {
            private readonly Memory<byte> arr;
            private readonly uint[][] key;

            public DecryptNGBLock(Memory<byte> array, uint[][] key)
            {
                this.arr = array;
                this.key = key;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Invoke(int blockIndex)
            {
                DecryptNGBlock(arr.Slice(16 * blockIndex, 16).Span, key);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecryptNG(in Memory<byte> data, uint[][] key)
        {
            var action = new DecryptNGBLock(data, key);
            ParallelHelper.For(new Range(0, data.Length / 16), in action, 25);
            //for (int blockIndex = 0; blockIndex < data.Length / 16; blockIndex++)
            //{
            //    DecryptNGBlock(data.Slice(16 * blockIndex, 16), key);
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecryptNGBlock(Span<byte> data, uint[][] key)
        {
            DecryptNGRoundA(data, key[0], GTA5Keys.PC_NG_DECRYPT_TABLES[0]);
            DecryptNGRoundA(data, key[1], GTA5Keys.PC_NG_DECRYPT_TABLES[1]);
            //for (int k = 2; k <= 15; k++)
            DecryptNGRoundB(data, key[2], GTA5Keys.PC_NG_DECRYPT_TABLES[2]);
            DecryptNGRoundB(data, key[3], GTA5Keys.PC_NG_DECRYPT_TABLES[3]);
            DecryptNGRoundB(data, key[4], GTA5Keys.PC_NG_DECRYPT_TABLES[4]);
            DecryptNGRoundB(data, key[5], GTA5Keys.PC_NG_DECRYPT_TABLES[5]);
            DecryptNGRoundB(data, key[6], GTA5Keys.PC_NG_DECRYPT_TABLES[6]);
            DecryptNGRoundB(data, key[7], GTA5Keys.PC_NG_DECRYPT_TABLES[7]);
            DecryptNGRoundB(data, key[8], GTA5Keys.PC_NG_DECRYPT_TABLES[8]);
            DecryptNGRoundB(data, key[9], GTA5Keys.PC_NG_DECRYPT_TABLES[9]);
            DecryptNGRoundB(data, key[10], GTA5Keys.PC_NG_DECRYPT_TABLES[10]);
            DecryptNGRoundB(data, key[11], GTA5Keys.PC_NG_DECRYPT_TABLES[11]);
            DecryptNGRoundB(data, key[12], GTA5Keys.PC_NG_DECRYPT_TABLES[12]);
            DecryptNGRoundB(data, key[13], GTA5Keys.PC_NG_DECRYPT_TABLES[13]);
            DecryptNGRoundB(data, key[14], GTA5Keys.PC_NG_DECRYPT_TABLES[14]);
            DecryptNGRoundB(data, key[15], GTA5Keys.PC_NG_DECRYPT_TABLES[15]);
            DecryptNGRoundA(data, key[16], GTA5Keys.PC_NG_DECRYPT_TABLES[16]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecryptNGRoundA(Span<byte> data, uint[] key, uint[][] table)
        {
            var x1 =
                table[0][data[0]] ^
                table[1][data[1]] ^
                table[2][data[2]] ^
                table[3][data[3]] ^
                key[0];
            var x2 =
                table[4][data[4]] ^
                table[5][data[5]] ^
                table[6][data[6]] ^
                table[7][data[7]] ^
                key[1];
            var x3 =
                table[8][data[8]] ^
                table[9][data[9]] ^
                table[10][data[10]] ^
                table[11][data[11]] ^
                key[2];
            var x4 =
                table[12][data[12]] ^
                table[13][data[13]] ^
                table[14][data[14]] ^
                table[15][data[15]] ^
                key[3];

            Unsafe.WriteUnaligned(ref data[0], x1 | (ulong)x2 << 32);
            Unsafe.WriteUnaligned(ref data[8], x3 | (ulong)x4 << 32);

            //Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(data[..4]), x1);
            //Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(data.Slice(4, 4)), x2);
            //Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(data.Slice(8, 4)), x3);
            //Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(data.Slice(12, 4)), x4);
        }

        // round 3-15

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void DecryptNGRoundB(Span<byte> data, Span<uint> key, uint[][] table)
        {
            var x1 =
                table[0][data[0]] ^
                table[7][data[7]] ^
                table[10][data[10]] ^
                table[13][data[13]] ^
                key[0];
            var x2 =
                table[1][data[1]] ^
                table[4][data[4]] ^
                table[11][data[11]] ^
                table[14][data[14]] ^
                key[1];
            var x3 =
                table[2][data[2]] ^
                table[5][data[5]] ^
                table[8][data[8]] ^
                table[15][data[15]] ^
                key[2];
            var x4 =
                table[3][data[3]] ^
                table[6][data[6]] ^
                table[9][data[9]] ^
                table[12][data[12]] ^
                key[3];

            Unsafe.WriteUnaligned(ref data[0], x1 | (ulong)x2 << 32);
            Unsafe.WriteUnaligned(ref data[8], x3 | (ulong)x4 << 32);

            //Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(data[..4]), x1);
            //Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(data.Slice(4, 4)), x2);
            //Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(data.Slice(8, 4)), x3);
            //Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(data.Slice(12, 4)), x4);
        }















        public static byte[] EncryptNG(byte[] data, string name, uint length)
        {
            var key = GetNGKey(name, length);
            return EncryptNG(data, key);
        }

        public static byte[] EncryptNG(byte[] data, uint[][] key)
        {
            if ((GTA5Keys.PC_NG_ENCRYPT_TABLES == null) || (GTA5Keys.PC_NG_ENCRYPT_LUTs == null))
            {
                throw new Exception("Unable to encrypt - tables not loaded.");
            }

            var encryptedData = new byte[data.Length];

            for (int blockIndex = 0; blockIndex < data.Length / 16; blockIndex++)
            {
                byte[] decryptedBlock = new byte[16];
                Array.Copy(data, 16 * blockIndex, decryptedBlock, 0, 16);
                byte[] encryptedBlock = EncryptBlock(decryptedBlock, key);
                Array.Copy(encryptedBlock, 0, encryptedData, 16 * blockIndex, 16);
            }

            if (data.Length % 16 != 0)
            {
                var left = data.Length % 16;
                Buffer.BlockCopy(data, data.Length - left, encryptedData, data.Length - left, left);
            }

            return encryptedData;
        }

        public static byte[] EncryptBlock(byte[] data, uint[][] key)
        {
            var buffer = data;

            buffer = EncryptRoundA(buffer, key[16], GTA5Keys.PC_NG_ENCRYPT_TABLES[16]);
            for (int k = 15; k >= 2; k--)
                buffer = EncryptRoundB_LUT(buffer, key[k], GTA5Keys.PC_NG_ENCRYPT_LUTs[k]);
            buffer = EncryptRoundA(buffer, key[1], GTA5Keys.PC_NG_ENCRYPT_TABLES[1]);
            buffer = EncryptRoundA(buffer, key[0], GTA5Keys.PC_NG_ENCRYPT_TABLES[0]);

            return buffer;
        }

        public static byte[] EncryptRoundA(byte[] data, uint[] key, uint[][] table)
        {
            // apply xor to data first...
            var xorbuf = new byte[16];
            Buffer.BlockCopy(key, 0, xorbuf, 0, 16);

            var x1 =
                table[0][data[0] ^ xorbuf[0]] ^
                table[1][data[1] ^ xorbuf[1]] ^
                table[2][data[2] ^ xorbuf[2]] ^
                table[3][data[3] ^ xorbuf[3]];
            var x2 =
                table[4][data[4] ^ xorbuf[4]] ^
                table[5][data[5] ^ xorbuf[5]] ^
                table[6][data[6] ^ xorbuf[6]] ^
                table[7][data[7] ^ xorbuf[7]];
            var x3 =
                table[8][data[8] ^ xorbuf[8]] ^
                table[9][data[9] ^ xorbuf[9]] ^
                table[10][data[10] ^ xorbuf[10]] ^
                table[11][data[11] ^ xorbuf[11]];
            var x4 =
                table[12][data[12] ^ xorbuf[12]] ^
                table[13][data[13] ^ xorbuf[13]] ^
                table[14][data[14] ^ xorbuf[14]] ^
                table[15][data[15] ^ xorbuf[15]];

            var buf = new byte[16];
            Array.Copy(BitConverter.GetBytes(x1), 0, buf, 0, 4);
            Array.Copy(BitConverter.GetBytes(x2), 0, buf, 4, 4);
            Array.Copy(BitConverter.GetBytes(x3), 0, buf, 8, 4);
            Array.Copy(BitConverter.GetBytes(x4), 0, buf, 12, 4);
            return buf;
        }

        public static byte[] EncryptRoundA_LUT(byte[] dataOld, uint[] key, GTA5NGLUT[] lut)
        {
            var data = (byte[])dataOld.Clone();

            // apply xor to data first...
            var xorbuf = new byte[16];
            Buffer.BlockCopy(key, 0, xorbuf, 0, 16);
            for (int y = 0; y < 16; y++)
            {
                data[y] ^= xorbuf[y];
            }

            return new byte[] {
                lut[0].LookUp(BitConverter.ToUInt32( new byte[] {  data[0],  data[1],  data[2],  data[3]  }, 0)),
                lut[1].LookUp(BitConverter.ToUInt32( new byte[] {  data[0],  data[1],  data[2],  data[3]  }, 0)),
                lut[2].LookUp(BitConverter.ToUInt32( new byte[] {  data[0],  data[1],  data[2],  data[3]  }, 0)),
                lut[3].LookUp(BitConverter.ToUInt32( new byte[] {  data[0],  data[1],  data[2],  data[3]  }, 0)),
                lut[4].LookUp(BitConverter.ToUInt32( new byte[] {  data[4],  data[5],  data[6],  data[7]  }, 0)),
                lut[5].LookUp(BitConverter.ToUInt32( new byte[] {  data[4],  data[5],  data[6],  data[7]  }, 0)),
                lut[6].LookUp(BitConverter.ToUInt32( new byte[] {  data[4],  data[5],  data[6],  data[7]  }, 0)),
                lut[7].LookUp(BitConverter.ToUInt32( new byte[] {  data[4],  data[5],  data[6],  data[7]  }, 0)),
                lut[8].LookUp(BitConverter.ToUInt32( new byte[] {  data[8],  data[9],  data[10], data[11] }, 0)),
                lut[9].LookUp(BitConverter.ToUInt32( new byte[] {  data[8],  data[9],  data[10], data[11] }, 0)),
                lut[10].LookUp(BitConverter.ToUInt32( new byte[] { data[8],  data[9],  data[10], data[11] }, 0)),
                lut[11].LookUp(BitConverter.ToUInt32( new byte[] { data[8],  data[9],  data[10], data[11] }, 0)),
                lut[12].LookUp(BitConverter.ToUInt32( new byte[] { data[12], data[13], data[14], data[15] }, 0)),
                lut[13].LookUp(BitConverter.ToUInt32( new byte[] { data[12], data[13], data[14], data[15] }, 0)),
                lut[14].LookUp(BitConverter.ToUInt32( new byte[] { data[12], data[13], data[14], data[15] }, 0)),
                lut[15].LookUp(BitConverter.ToUInt32( new byte[] { data[12], data[13], data[14], data[15] }, 0))
             };
        }

        public static byte[] EncryptRoundB_LUT(byte[] dataOld, uint[] key, GTA5NGLUT[] lut)
        {
            var data = (byte[])dataOld.Clone();

            // apply xor to data first...
            var xorbuf = new byte[16];
            Buffer.BlockCopy(key, 0, xorbuf, 0, 16);
            for (int y = 0; y < 16; y++)
            {
                data[y] ^= xorbuf[y];
            }

            return new byte[] {
                lut[0].LookUp(BitConverter.ToUInt32( new byte[] {  data[0],  data[1],  data[2],  data[3]  }, 0)),
                lut[1].LookUp(BitConverter.ToUInt32( new byte[] {  data[4],  data[5],  data[6],  data[7]  }, 0)),
                lut[2].LookUp(BitConverter.ToUInt32( new byte[] {  data[8],  data[9],  data[10], data[11] }, 0)),
                lut[3].LookUp(BitConverter.ToUInt32( new byte[] {  data[12], data[13], data[14], data[15] }, 0)),
                lut[4].LookUp(BitConverter.ToUInt32( new byte[] {  data[4],  data[5],  data[6],  data[7]  }, 0)),
                lut[5].LookUp(BitConverter.ToUInt32( new byte[] {  data[8],  data[9],  data[10], data[11] }, 0)),
                lut[6].LookUp(BitConverter.ToUInt32( new byte[] {  data[12], data[13], data[14], data[15] }, 0)),
                lut[7].LookUp(BitConverter.ToUInt32( new byte[] {  data[0],  data[1],  data[2],  data[3]  }, 0)),
                lut[8].LookUp(BitConverter.ToUInt32( new byte[] {  data[8],  data[9],  data[10], data[11] }, 0)),
                lut[9].LookUp(BitConverter.ToUInt32( new byte[] {  data[12], data[13], data[14], data[15] }, 0)),
                lut[10].LookUp(BitConverter.ToUInt32( new byte[] { data[0],  data[1],  data[2],  data[3]  }, 0)),
                lut[11].LookUp(BitConverter.ToUInt32( new byte[] { data[4],  data[5],  data[6],  data[7]  }, 0)),
                lut[12].LookUp(BitConverter.ToUInt32( new byte[] { data[12], data[13], data[14], data[15] }, 0)),
                lut[13].LookUp(BitConverter.ToUInt32( new byte[] { data[0],  data[1],  data[2],  data[3]  }, 0)),
                lut[14].LookUp(BitConverter.ToUInt32( new byte[] { data[4],  data[5], data[6],   data[7]  }, 0)),
                lut[15].LookUp(BitConverter.ToUInt32( new byte[] { data[8],  data[9], data[10], data[11]  }, 0))};
        }




    }
}
