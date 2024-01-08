using CodeWalker.Core.Utils;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodeWalker.Test
{
    public class SequenceReaderTests
    {
        [Fact]
        public void SetPositionShouldSetConsumedToGivenValue()
        {
            var bytes = new byte[1024];
            BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(0, 8), 12.6d);
            var sequence = new ReadOnlySequence<byte>(bytes);
            var reader = new SequenceReader<byte>(sequence);

            Assert.Equal(0, reader.Consumed);
            Assert.Equal(12.6d, reader.ReadDouble());
            Assert.Equal(8, reader.Consumed);
            reader.SetPosition(2);
            Assert.Equal(2, reader.Consumed);

            reader.SetPosition(197);
            Assert.Equal(197, reader.Consumed);

        }
    }
}
