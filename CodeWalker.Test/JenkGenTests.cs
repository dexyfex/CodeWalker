using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodeWalker.Test
{
    public class JenkGenTests
    {
        [Fact]
        public void GenHashMustReturnHash()
        {
            Assert.Equal((uint)1044535224, JenkHash.GenHash("ASDF"));

            Assert.Equal((uint)1485398060, JenkHash.GenHashLower("asdf"));
        }
        [Fact]
        public void EnsureMustAddStringToDictionary()
        {
            JenkIndex.Index.Clear();
            Assert.Empty(JenkIndex.Index);
            JenkIndex.Ensure("asdf");

            Assert.Single(JenkIndex.Index);
        }

        [Fact]
        public void TryGetStringWithMissingHashMustReturnEmptyString()
        {
            JenkIndex.Index.Clear();
            Assert.Empty(JenkIndex.Index);

            var str = JenkIndex.TryGetString((uint)1485398060);

            Assert.Equal(string.Empty, str);
        }

        [Fact]
        public void GetStringWithMissingHashMustReturnHashAsString()
        {
            JenkIndex.Index.Clear();
            Assert.Empty(JenkIndex.Index);

            var str = JenkIndex.GetString((uint)1485398060);

            Assert.Equal(1485398060.ToString(), str);
        }

        [Fact]
        public void TryGetStringMustReturnAddedString()
        {
            JenkIndex.Ensure("asdf");

            var str = JenkIndex.TryGetString((uint)1485398060);

            Assert.Equal("asdf", str);
        }

        [Fact]
        public void EnsureLowerMustAddLoweredStringToDictionary()
        {
            JenkIndex.Index.Clear();
            Assert.Empty(JenkIndex.Index);
            JenkIndex.EnsureLower("ASDF");

            Assert.Single(JenkIndex.Index);

            var str = JenkIndex.TryGetString((uint)1485398060);

            Assert.Equal("ASDF", str);

            var missingStr = JenkIndex.TryGetString((uint)1044535224);
            Assert.Equal(string.Empty, missingStr);
        }

        [Fact]
        public void GetStringMustReturnAddedString()
        {
            JenkIndex.Index.Clear();
            Assert.Empty(JenkIndex.Index);
            JenkIndex.EnsureLower("ASDF");

            Assert.Single(JenkIndex.Index);

            var str = JenkIndex.TryGetString((uint)1485398060);

            Assert.Equal("ASDF", str);

            var missingStr = JenkIndex.TryGetString((uint)1044535224);
            Assert.Equal(string.Empty, missingStr);
        }
    }
}
