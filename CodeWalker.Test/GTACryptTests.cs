using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CodeWalker.Test
{
    public class GTACryptTests
    {
        private const string RawKey = "sVGBe8yn7a4jqG0Di35DL9kFdd8IxUrIRuAMhRuEHOybRfi3sBMmEzz/ijT4EX4P8BwMOjmghVJi/AGUqqhOQsZdUcvcyOo4phKlPkl+xGX1wafkzNxpRJFdvhImFz+4+glX8Yavo3o76wBWlFfkUXRqGtiLAdTsKtdqgFMalzmnkDZsL7CeFjQONS/3W5ZzS/aikwsOYLYCo/f8QaIemm385HvFDEeLgHvsigxYHd/dyICvpK0bUvhZfz1BUmmHLOlYFKll/HPa7q8KhL/bRT0FblZ0s26INHaIf4tZ3nnWq51YGR1SnddxqDP/L0314KxuI4ou4xmAiu67x+sv+cLxScJc83U1iRhngfn3s4w=";
        private uint[][] Key;

        private readonly ITestOutputHelper _output;

        [MemberNotNull(nameof(Key))]
        private void LoadKeys()
        {
            var bytes = Convert.FromBase64String(RawKey);

            Key = new uint[17][];
            var uints = MemoryMarshal.Cast<byte, uint>(bytes);
            for (int i = 0; i < Key.Length; i++)
            {
                Key[i] = uints.Slice(i * 4, 4).ToArray();
            }

            // GTA5Keys.LoadFromPath("C:\\Program Files\\Rockstar Games\\Grand Theft Auto V");

            var rawKey = File.ReadAllBytes(TestFiles.GetFilePath("raw_key.dat"));

            GTA5Keys.LoadPCNGDecryptTable(rawKey.AsSpan());
        }

        public GTACryptTests(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
            LoadKeys();


            //File.WriteAllBytes(TestFiles.GetFilePath("raw_key.dat"), rawKeyBytes.ToArray());
            //var base64Key = Convert.ToBase64String(rawKeyBytes);
            
        }

        // Sanity check, before running tests for decryption, ensure we actually have the correct key loaded in
        [Fact(DisplayName = "Key should match key which was used to create decryption tests")]
        public void KeyShouldBeExpectedKey()
        {
            // The full key is pretty long, so just check a small part of it
            var expectedKey = "l15p1+ANbFRRf+S3O0BHuSrwiWt9O4HBcLc7foJtA0v+GB5u0HV9T5V3b4CyFRJQMk9ZSIe+deW172Kp+pu6IAXNOKpWhZROt8Zk/rpK3kHIyq1sUgYwAP1hytlc814Iko0feUc1WpxDtv7SK6Bbi72wrrg/w+P3gz3Rq0vpMsM6EJVZXtpYX9ypFRD0btQoN5wv5k46RG2hjNrVnSijkUq54COuKWY9hMehUhnxTMd1ZE3Q7YHW6+p7phKr+hCTIq9Fej3q5aAIQYIVwcWznSP/l5rU9tkBoNwINbNFwLBoCOtd6FKgRcPstcrvqNC8c87vyUwTQjoCN0hTjJhtQw+78uz3FwCfWln8EfOUpNHEFsUzfGtTIUDPKmVv8pukScA0lELmLDL1PgbIcefpnhZU8C/8MRg5GoiYcItiHbrMSQkiJVU1g1hw+kbNGdvCGKGeJ1SskhkO6yAM4V2+tCCGQy2R9MvO8O1wZi5zLSWKMs9amPvVP6UPfpu0v7BJ8b2ihsmaf4wtCvmSJ3wz1B1y6IluoklEZNSDAt/QwaeARAUcTUOQ2pQnvWD54m6XZ61XtcrjqztqIe0KYFcnTKqqwnO84HxY43S44/9IzI5Bn/iFeOj3b4lLG+1ynj0pbItPE3YdmWemdqossWzG59oDtwnW399WPpMxF5rS02hm/YVVzmAPdYWXc7KvebTdCRFQ9Xm4JY+tULKKE4eGgdgqsV5rcT/q5d4a+vvLaMBbCS7xaVg5vVAvNleeUXcmm4IBiHdNS4difiEbOWlB7tl6Y76BFNf8YQf1rAFOnOST3c2ZH1vu3rhj2BYeCzw+ovUOYsCVYX2+yXoPj+G59NePDbaceHFx0SWvryxaK3J7kSPY6QJypSjZjzxETI4rFH32eJ8BpcaIG8kNU1bi4FkgKKZ6wfE4ZYRR4qRfrHsVLSSYV9VGrmMu8/sxNo3/T2qWjeSOyBrSXHsYCzhWoqOl3IJFHFzLDZL0u9tTZel0NJ8wKYld3PZH0n8RroDW0wyp+BD+UjZIkOZ0IdaRzawAYGoGtOwdA2eas0ZliHwcIjppG9hKkDy6N0CQpBkuLyP/xQpohELyxHYx6yt08pYOuzfPMN2VmasH312jjOhfioq/fxKHlgAeTgTGP8Nktpa2HrkzCvYzH4uo4iRqAzTl+1E2zP0GwrxnKsdvEYSNyL+j5qfOTb+ZqO8kBedjhu6nBcuzedtt253zqIPEJO74Alyp0xbEODmTDuzRBAtV/ED55/ccrbA8FAc1tSmx1aYL4RcEIs8MwiZbuxoMoRLXVGEwZl8f3fnH8MVGF9OOsWsUpyZ4zCYs4TQH5D79fkJVdt6AE0cEnepK+LK8dw==";

            var loadedKey = MemoryMarshal.AsBytes(GTA5Keys.PC_NG_DECRYPT_TABLES[5][5].AsSpan());
            var loadedKeyBase64 = Convert.ToBase64String(loadedKey);

            Assert.Equal(expectedKey, loadedKeyBase64);

            // This is the key which was just loaded, so these should be equivalent

            var fullKeyBytes = MemoryMarshal.AsBytes(GTA5Keys.PC_NG_DECRYPT_TABLES.SelectMany(p => p).SelectMany(p => p).ToArray().AsSpan());

            var expectedBytes = File.ReadAllBytes(TestFiles.GetFilePath("raw_key.dat"));
            var loadedBytes = fullKeyBytes.ToArray();

            Assert.Equal(expectedBytes.LongLength, loadedBytes.LongLength);

            // Use manual for loop, equivalent is quite slow compared to this (7 seconds vs 1 second)
            for (int i = 0; i < loadedBytes.Length; i++)
            {
                Assert.Equal(expectedBytes[i], loadedBytes[i]);
            }
        }

        private const string InputData = "6Zemmftaofsb2sEYb3AC9A==";
        private const string OutputData = "AAAAAAD//38BAAAAGAAAAA==";

        [Fact]
        public void DecryptWithArrayShouldReturnExpectedData()
        {
            var data = Convert.FromBase64String(InputData);



            GTACrypto.DecryptNG(data, Key);

            Assert.Equal(OutputData, Convert.ToBase64String(data));
        }

        [Fact]
        public void DecryptWithSpanShouldReturnExpectedData()
        {
            var data = Convert.FromBase64String(InputData);
            GTACrypto.DecryptNG(data.AsSpan(), Key);

            Assert.Equal(OutputData, Convert.ToBase64String(data));
        }
    }
}
