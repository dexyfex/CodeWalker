using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWalker.Core.Utils
{
    public static class StreamingExtensions
    {
        public static Task<int> ReadAsync(this BinaryReader br, byte[] buffer, int index, int count)
        {
            return br.BaseStream.ReadAsync(buffer, index, count);
        }
    }

    public ref struct SpanStream
    {
        public Span<byte> Buffer { get; private set; }
        private int _position;

        public SpanStream(Span<byte> buffer)
        {
            Buffer = buffer;
            _position = 0;
        }

        private ReadOnlySpan<byte> InternalRead(int count)
        {
            int origPos = _position;
            int newPos = origPos + count;

            if ((uint)newPos > (uint)Buffer.Length)
            {
                _position = Buffer.Length;
                ThrowHelper.ThrowEndOfFileException();
            }

            var span = Buffer.Slice(origPos, count);
            _position = newPos;
            return span;
        }

        public short ReadInt16() => BinaryPrimitives.ReadInt16LittleEndian(InternalRead(sizeof(short)));

        public ushort ReadUInt16() => BinaryPrimitives.ReadUInt16LittleEndian(InternalRead(sizeof(ushort)));

        public int ReadInt32() => BinaryPrimitives.ReadInt32LittleEndian(InternalRead(sizeof(int)));
        public uint ReadUInt32() => BinaryPrimitives.ReadUInt32LittleEndian(InternalRead(sizeof(uint)));
        public long ReadInt64() => BinaryPrimitives.ReadInt64LittleEndian(InternalRead(sizeof(long)));
        public ulong ReadUInt64() => BinaryPrimitives.ReadUInt64LittleEndian(InternalRead(sizeof(ulong)));
        public unsafe Half ReadHalf() => BinaryPrimitives.ReadHalfLittleEndian(InternalRead(sizeof(Half)));
        public unsafe float ReadSingle() => BinaryPrimitives.ReadSingleLittleEndian(InternalRead(sizeof(float)));
        public unsafe double ReadDouble() => BinaryPrimitives.ReadDoubleLittleEndian(InternalRead(sizeof(double)));
    }
}
