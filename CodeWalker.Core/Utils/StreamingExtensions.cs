using CodeWalker.GameFiles;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using SharpDX;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
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

    public static class SequenceReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadLittleEndian(ref this SequenceReader<byte> reader, out uint value)
        {
            reader.TryReadLittleEndian(out int _value);
            value = (uint) _value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadLittleEndian(ref this SequenceReader<byte> reader, out ushort value)
        {
            reader.TryReadLittleEndian(out short _value);
            value = (ushort)_value;
            return true;
        }

        public static long ReadInt64(ref this SequenceReader<byte> reader)
        {
            reader.TryRead(out long value);
            return value;
        }

        public static long ReadInt64BigEndian(ref this SequenceReader<byte> reader)
        {
            reader.TryReadBigEndian(out long value);
            return value;
        }

        public static ulong ReadUInt64(ref this SequenceReader<byte> reader)
        {
            reader.TryRead(out ulong value);
            return value;
        }

        public static ulong ReadUInt64BigEndian(ref this SequenceReader<byte> reader)
        {
            reader.TryReadBigEndian(out long value);
            return (ulong)value;
        }

        public static uint ReadUInt32(ref this SequenceReader<byte> reader)
        {
            reader.TryRead(out uint value);
            return value;
        }

        public static uint ReadUInt32BigEndian(ref this SequenceReader<byte> reader)
        {
            reader.TryReadBigEndian(out int value);
            return (uint)value;
        }

        public static int ReadInt32(ref this SequenceReader<byte> reader)
        {
            reader.TryReadLittleEndian(out int value);
            return value;
        }

        public static int ReadInt32BigEndian(ref this SequenceReader<byte> reader)
        {
            reader.TryReadBigEndian(out int value);
            return value;
        }

        public static short ReadInt16(ref this SequenceReader<byte> reader)
        {
            reader.TryReadLittleEndian(out short value);
            return value;
        }

        public static short ReadInt16BigEndian(ref this SequenceReader<byte> reader)
        {
            reader.TryReadBigEndian(out short value);
            return value;
        }

        public static ushort ReadUInt16(ref this SequenceReader<byte> reader)
        {
            reader.TryReadLittleEndian(out ushort value);
            return value;
        }

        public static ushort ReadUInt16BigEndian(ref this SequenceReader<byte> reader)
        {
            reader.TryReadBigEndian(out short value);
            return (ushort)value;
        }

        public static byte ReadByte(ref this SequenceReader<byte> reader)
        {
            reader.TryRead(out byte value);
            return value;
        }

        public static float ReadSingle(ref this SequenceReader<byte> reader)
        {
            reader.TryRead(out float value);
            return value;
        }

        public static double ReadDouble(ref this SequenceReader<byte> reader)
        {
            reader.TryRead(out double value);
            return value;
        }

        public static float ReadSingleBigEndian(ref this SequenceReader<byte> reader)
        {
            reader.TryRead(out int value);

            return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReverseEndianness(value));
        }

        public static double ReadDoubleBigEndian(ref this SequenceReader<byte> reader)
        {
            reader.TryRead(out long value);

            return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReverseEndianness(value));
        }

        public static Vector3 ReadVector3(ref this SequenceReader<byte> reader)
        {
            Vector3 v = new Vector3 {
                X = ReadSingle(ref reader),
                Y = ReadSingle(ref reader),
                Z = ReadSingle(ref reader),
            };
            return v;
        }
        public static Vector4 ReadVector4(ref this SequenceReader<byte> reader)
        {
            Vector4 v = new Vector4
            {
                X = ReadSingle(ref reader),
                Y = ReadSingle(ref reader),
                Z = ReadSingle(ref reader),
                W = ReadSingle(ref reader)
            };
            return v;
        }

        public static Matrix ReadMatrix(ref this SequenceReader<byte> reader)
        {
            Matrix m = new Matrix
            {
                M11 = ReadSingle(ref reader),
                M21 = ReadSingle(ref reader),
                M31 = ReadSingle(ref reader),
                M41 = ReadSingle(ref reader),
                M12 = ReadSingle(ref reader),
                M22 = ReadSingle(ref reader),
                M32 = ReadSingle(ref reader),
                M42 = ReadSingle(ref reader),
                M13 = ReadSingle(ref reader),
                M23 = ReadSingle(ref reader),
                M33 = ReadSingle(ref reader),
                M43 = ReadSingle(ref reader),
                M14 = ReadSingle(ref reader),
                M24 = ReadSingle(ref reader),
                M34 = ReadSingle(ref reader),
                M44 = ReadSingle(ref reader)
            };
            return m;
        }

        public static ReadOnlySpan<ulong> ReadUlongsAt(ref this SequenceReader<byte> reader, ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0))
                return ReadOnlySpan<ulong>.Empty;

            var length = count * sizeof(ulong);

            var data = reader.ReadBytesAt(position, length, false);
            if (data.IsEmpty)
            {
                return ReadOnlySpan<ulong>.Empty;
            }
            var result = MemoryMarshal.Cast<byte, ulong>(data);

            return result;
        }

        public static ReadOnlySpan<byte> ReadBytes(ref this SequenceReader<byte> reader, int count)
        {
            var unread = reader.UnreadSpan;
            if (unread.Length > count)
            {
                var resultSpan = unread.Slice(0, count);
                reader.Advance(count);
                return resultSpan;
            }

            var result = reader.UnreadSequence.Slice(0, count).ToArray();
            reader.Advance(count);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadBytesAt(ref this SequenceReader<byte> reader, ulong position, uint count, bool cache = true)
        {
            return reader.ReadBytesAt(position, (int)count, cache);
        }

        public static ReadOnlySpan<byte> ReadBytesAt(ref this SequenceReader<byte> reader, ulong position, int count, bool cache = true)
        {
            var positionBackup = reader.Consumed;

            reader.SetPosition((long)position);
            var result = reader.ReadBytes(count);
            reader.SetPosition((long)positionBackup);

            return result;
        }

        public static ReadOnlySequence<byte> ReadSubSequence(ref this SequenceReader<byte> reader, int count)
        {
            var result = reader.UnreadSequence.Slice(0, count);
            reader.Advance(count);
            return result;
        }

        [SkipLocalsInit]
        public static string ReadStringLength(ref this SequenceReader<byte> reader, int length, bool ignoreNullTerminator = true)
        {
            if (length == 0)
            {
                return string.Empty;
            }

            var bytes = reader.ReadBytes(length);

            if (!ignoreNullTerminator)
            {
                var nullTerminatorIndex = bytes.IndexOf((byte)0);

                if (nullTerminatorIndex != -1)
                {
                    bytes.Slice(0, nullTerminatorIndex);
                }
            }

            return Encoding.UTF8.GetString(bytes);
            //return Encoding.UTF8.GetString(bytes, Math.Min(charsRead, maxLength));
        }

        public static string ReadString(ref this SequenceReader<byte> reader, int maxLength)
        {
            reader.TryReadTo(out ReadOnlySpan<byte> span, 0);

            if (span.Length > maxLength)
            {
                reader.Rewind(span.Length - maxLength);
                return Encoding.UTF8.GetString(span.Slice(0, maxLength));
            }

            return Encoding.UTF8.GetString(span);
            //return Encoding.UTF8.GetString(bytes, Math.Min(charsRead, maxLength));
        }

        public static string ReadString(ref this SequenceReader<byte> reader)
        {
            reader.TryReadTo(out ReadOnlySpan<byte> span, 0);

            return Encoding.UTF8.GetString(span);
            //return Encoding.UTF8.GetString(bytes, Math.Min(charsRead, maxLength));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryRead<T>(ref this SequenceReader<byte> reader, out T value) where T : unmanaged
        {
            ReadOnlySpan<byte> span = reader.UnreadSpan;
            if (span.Length < sizeof(T))
                return TryReadMultisegment(ref reader, out value);

            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(span));
            reader.Advance(Marshal.SizeOf<T>());
            return true;
        }

        private static unsafe bool TryReadMultisegment<T>(ref SequenceReader<byte> reader, out T value) where T : unmanaged
        {
            Debug.Assert(reader.UnreadSpan.Length < sizeof(T));

            // Not enough data in the current segment, try to peek for the data we need.
            T buffer = default;
            Span<byte> tempSpan = new Span<byte>(&buffer, sizeof(T));

            if (!reader.TryCopyTo(tempSpan))
            {
                value = default;
                return false;
            }

            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(tempSpan));
            reader.Advance(sizeof(T));
            return true;
        }

        private static bool TryReadReverseEndianness(ref SequenceReader<byte> reader, out short value)
        {
            if (reader.TryRead(out value))
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                return true;
            }

            return false;
        }

        private static ConcurrentDictionary<Type, bool> cacheableTypes = new ConcurrentDictionary<Type, bool>();
        private static bool fetchUsePool(Type type)
        {
            return !typeof(IResourceNoCacheBlock).IsAssignableFrom(type);
        }
        private static bool usePool<T>() where T : IResourceBlock, new()
        {
            return cacheableTypes.GetOrAdd(typeof(T), fetchUsePool);
        }

        public static T validate<T>(Func<T> instantiator)
            where T : IResourceBlock
        {
            return instantiator();
        }

        /// <summary>
        /// Reads a block.
        /// </summary>
        public static T ReadBlock<T>(this ref SequenceReader<byte> reader, params object[] parameters) where T : IResourceBlockSpan, IResourceBlock, new()
        {
            var result = validate(() => new T());


            // replace with correct type...
            if (result is IResourceXXSytemBlockSpan block)
            {
                result = (T)block.GetType(ref reader, parameters);
            }

            result.Read(ref reader, parameters);

            return result;
        }

        public static void SetPosition(this ref SequenceReader<byte> reader, long position)
        {
            var consumed = reader.Consumed;
            if (position == consumed)
            {
                return;
            }

            if (consumed > position)
            {
                reader.Rewind(consumed - position);
            }
            else
            {
                reader.Advance(position - consumed);
            }
        }

        /// <summary>
        /// Reads a block at a specified position.
        /// </summary>
        public static T ReadBlockAt<T>(this ref SequenceReader<byte> reader, ulong position, params object[] parameters) where T : IResourceBlock, IResourceBlockSpan, new()
        {
            if (position != 0)
            {
                var positionBackup = reader.Consumed;

                reader.SetPosition((long)position);
                var result = reader.ReadBlock<T>(parameters);
                reader.SetPosition((long)positionBackup);

                return result;
            }
            else
            {
                return default(T);
            }
        }

        public static ReadOnlySpan<T> ReadStructsAt<T>(this ref SequenceReader<byte> reader, ulong position, uint count, bool cache = true) where T : struct
        {
            return reader.ReadStructsAt<T>(position, (int)count, cache);
        }

        public static ReadOnlySpan<T> ReadStructsAt<T>(this ref SequenceReader<byte> reader, ulong position, int count, bool cache = true) where T : struct
        {
            if ((position <= 0) || (count == 0))
                return null;

            var structsize = Marshal.SizeOf(typeof(T));
            var length = (int)(count * structsize);
            var data = reader.ReadBytesAt(position, length, false);

            var resultSpan = MemoryMarshal.Cast<byte, T>(data);

            return resultSpan;
        }
    }

    public static class SpanExtension
    {
        public static ReadOnlySpan<T> ReadTill<T>(this ReadOnlySpan<T> span, T searchFor) where T : IEquatable<T>?
        {
            var index = span.IndexOf(searchFor);
            if (index < 0)
            {
                return span;
            }

            return span.Slice(index);
        }

        public static Span<T> ReadTill<T>(this Span<T> span, T searchFor) where T : IEquatable<T>?
        {
            var index = span.IndexOf(searchFor);
            if (index < 0)
            {
                return span;
            }

            return span.Slice(0, index);
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
                throw new EndOfStreamException();
                return default;
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
        public unsafe System.Half ReadHalf() => BinaryPrimitives.ReadHalfLittleEndian(InternalRead(sizeof(System.Half)));
        public unsafe float ReadSingle() => BinaryPrimitives.ReadSingleLittleEndian(InternalRead(sizeof(float)));
        public unsafe double ReadDouble() => BinaryPrimitives.ReadDoubleLittleEndian(InternalRead(sizeof(double)));
    }
}
