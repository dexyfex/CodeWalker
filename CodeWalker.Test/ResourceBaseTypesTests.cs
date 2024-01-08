using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodeWalker.Test
{
    public class ResourceBaseTypesTests
    {
        [Fact]
        public void AddingItemsToResourceSimpleArrayShouldAddItem()
        {
            var list = new ResourceSimpleArray<ClipAnimationsEntry>();

            var entry1 = new ClipAnimationsEntry();
            list.Add(entry1);

            var entry2 = new ClipAnimationsEntry();
            list.Add(entry2);

            Assert.Equal(entry1, list[0]);
            Assert.Equal(entry2, list[1]);

            list = new ResourceSimpleArray<ClipAnimationsEntry>();
            list.Data = new List<ClipAnimationsEntry>();

            list.Add(entry1);
            list.Add(entry2);

            Assert.Equal(entry1, list[0]);
            Assert.Equal(entry2, list[1]);
        }
    }
}
