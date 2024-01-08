using CodeWalker.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CodeWalker.Test
{
    public class TestSpanLineEnumerator
    {
        private readonly ITestOutputHelper output;
        public TestSpanLineEnumerator(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void IteratorShouldSplitLinesCorrectly()
        {
            var lines = "kaas\nsaak\nnog\neen\nline".AsSpan();

            foreach(var line in lines.EnumerateSplit('\n'))
            {
                output.WriteLine(line.ToString());
            }
        }
    }
}
