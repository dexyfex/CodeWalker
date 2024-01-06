using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CodeWalker.Test
{
    public class RpfFileTests(ITestOutputHelper testOutputHelper)
    {
        public static IEnumerable<object[]> RpfFiles => new List<object[]>
        {
            new object[] { new RpfResourceFileEntry() },
            new object[] { new RpfBinaryFileEntry() },
        };

        [Theory]
        [MemberData(nameof(RpfFiles))]
        public void SettingFileSizeShouldUpdateHFileSize(RpfFileEntry fileEntry)
        {
            fileEntry.FileSize = 0x64U;

            Assert.Equal(0x64U, fileEntry.FileSize);
        }

        [Theory]
        [MemberData(nameof(RpfFiles))]
        public void SettingFileOffsetShouldRemoveSystemPosition(RpfFileEntry fileEntry)
        {
            fileEntry.FileOffset = 0x021D30U;

            Assert.Equal(0x021D30U, fileEntry.FileOffset);
        }

        [Theory]
        [MemberData(nameof(RpfFiles))]
        public void SettingNameOffsetShouldUpdateNameOffset(RpfFileEntry fileEntry)
        {
            fileEntry.NameOffset = 0x022E;

            Assert.Equal(0x022EU, fileEntry.NameOffset);
        }

        [Theory]
        [MemberData(nameof(RpfFiles))]
        public void HeaderRelatedChangesShouldBeReflectedInHeader(RpfFileEntry fileEntry)
        {
            fileEntry.NameOffset = 0x022E;
            fileEntry.FileOffset = 0x021D30U;
            fileEntry.FileSize = 0x147067;

            Assert.Equal(0x821D30147067022EUL, fileEntry.Header);
        }
    }
}
