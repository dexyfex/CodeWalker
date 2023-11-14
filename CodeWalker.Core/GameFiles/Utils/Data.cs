

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



using SharpDX;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using CodeWalker.Core.Utils;
using System.Buffers.Binary;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

namespace CodeWalker.GameFiles
{
    public enum Endianess
    {
        LittleEndian = 0,
        BigEndian = 1,
    }

    public enum DataType
    {
        Byte = 0,
        Int16 = 1,
        Int32 = 2,
        Int64 = 3,
        Uint16 = 4,
        Uint32 = 5,
        Uint64 = 6,
        Float = 7,
        Double = 8,
        String = 9,
    }

    public class DataReader : IDisposable
    {
        private Stream baseStream;

        /// <summary>
        /// Gets or sets the endianess of the underlying stream.
        /// </summary>
        public Endianess Endianess { get; set; } = Endianess.LittleEndian;

        /// <summary>
        /// Gets the length of the underlying stream.
        /// </summary>
        public virtual long Length
        {
            get
            {
                return baseStream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the position within the underlying stream.
        /// </summary>
        public virtual long Position
        {
            get
            {
                return baseStream.Position;
            }
            set
            {
                baseStream.Position = value;
            }
        }

        public DataReader(Stream stream)
        {
            if (stream is not null)
            {
                this.baseStream = Stream.Synchronized(stream);
            }
        }

        /// <summary>
        /// Initializes a new data reader for the specified stream.
        /// </summary>
        public DataReader(Stream stream, Endianess endianess) : this(stream)
        {
            this.Endianess = endianess;
        }

        public virtual Stream GetStream()
        {
            return baseStream;
        }

        internal virtual void SetPositionAfterRead(Stream stream)
        {
            return;
        }

        protected virtual void ReadFromStream(Span<byte> buffer, bool ignoreEndianess = false)
        {
            var stream = GetStream();

            try
            {
                stream.Read(buffer);
            }
            finally
            {
                SetPositionAfterRead(stream);
            }

            if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
            {
                buffer.Reverse();
            }
        }

        /// <summary>
        /// Reads data from the underlying stream. This is the only method that directly accesses
        /// the data in the underlying stream.
        /// </summary>
        protected virtual byte[] ReadFromStream(int count, bool ignoreEndianess = false, byte[] buffer = null)
        {
            var stream = GetStream();
            buffer ??= new byte[count];

            try
            {
                stream.Read(buffer, 0, count);
            }
            finally
            {
                SetPositionAfterRead(stream);
            }

            // handle endianess
            if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
            {
                Array.Reverse(buffer, 0, count);
            }

            return buffer;
        }

        /// <summary>
        /// Reads a byte.
        /// </summary>
        public virtual byte ReadByte()
        {
            var stream = GetStream();
            int result;
            try
            {
                result = stream.ReadByte();
            }
            finally
            {
                SetPositionAfterRead(stream);
            }
            
            if (result == -1)
            {
                throw new InvalidOperationException("Tried to read from stream beyond end!");
            }

            return (byte) result;
        }

        /// <summary>
        /// Reads a sequence of bytes.
        /// </summary>
        public byte[] ReadBytes(int count, byte[] buffer = null)
        {
            return ReadFromStream(count, true, buffer);
        }

        /// <summary>
        /// Reads a signed 16-bit value.
        /// </summary>
        public short ReadInt16()
        {
            Span<byte> _buffer = stackalloc byte[sizeof(short)];
            ReadFromStream(_buffer, true);

            if (Endianess == Endianess.LittleEndian)
            {
                return BinaryPrimitives.ReadInt16LittleEndian(_buffer);
            }
            else
            {
                return BinaryPrimitives.ReadInt16BigEndian(_buffer);
            }
        }

        /// <summary>
        /// Reads a signed 32-bit value.
        /// </summary>
        public int ReadInt32()
        {
            Span<byte> _buffer = stackalloc byte[sizeof(int)];
            ReadFromStream(_buffer, true);

            if (Endianess == Endianess.LittleEndian)
            {
                return BinaryPrimitives.ReadInt32LittleEndian(_buffer);
            }
            else
            {
                return BinaryPrimitives.ReadInt32BigEndian(_buffer);
            }
        }

        /// <summary>
        /// Reads a signed 64-bit value.
        /// </summary>
        public long ReadInt64()
        {
            Span<byte> _buffer = stackalloc byte[sizeof(long)];
            ReadFromStream(_buffer, true);

            if (Endianess == Endianess.LittleEndian)
            {
                return BinaryPrimitives.ReadInt64LittleEndian(_buffer);
            }
            else
            {
                return BinaryPrimitives.ReadInt64BigEndian(_buffer);
            }
        }

        /// <summary>
        /// Reads an unsigned 16-bit value.
        /// </summary>
        public ushort ReadUInt16()
        {
            Span<byte> _buffer = stackalloc byte[sizeof(ushort)];
            ReadFromStream(_buffer, true);

            if (Endianess == Endianess.LittleEndian)
            {
                return BinaryPrimitives.ReadUInt16LittleEndian(_buffer);
            }
            else
            {
                return BinaryPrimitives.ReadUInt16BigEndian(_buffer);
            }
        }

        /// <summary>
        /// Reads an unsigned 32-bit value.
        /// </summary>
        public uint ReadUInt32()
        {
            Span<byte> _buffer = stackalloc byte[sizeof(uint)];
            ReadFromStream(_buffer, true);

            if (Endianess == Endianess.LittleEndian)
            {
                return BinaryPrimitives.ReadUInt32LittleEndian(_buffer);
            }
            else
            {
                return BinaryPrimitives.ReadUInt32BigEndian(_buffer);
            }
        }

        /// <summary>
        /// Reads an unsigned 64-bit value.
        /// </summary>
        public ulong ReadUInt64()
        {
            Span<byte> _buffer = stackalloc byte[sizeof(ulong)];
            ReadFromStream(_buffer, true);

            if (Endianess == Endianess.LittleEndian)
            {
                return BinaryPrimitives.ReadUInt64LittleEndian(_buffer);
            }
            else
            {
                return BinaryPrimitives.ReadUInt64BigEndian(_buffer);
            }
            //return BitConverter.ToUInt64(ReadFromStream(_buffer), 0);
        }

        /// <summary>
        /// Reads a single precision floating point value.
        /// </summary>
        public float ReadSingle()
        {
            Span<byte> _buffer = stackalloc byte[sizeof(float)];
            ReadFromStream(_buffer, true);

            if (Endianess == Endianess.LittleEndian)
            {
                return BinaryPrimitives.ReadSingleLittleEndian(_buffer);
            }
            else
            {
                return BinaryPrimitives.ReadSingleBigEndian(_buffer);
            }
        }

        /// <summary>
        /// Reads a double precision floating point value.
        /// </summary>
        public double ReadDouble()
        {
            Span<byte> _buffer = stackalloc byte[sizeof(double)];
            ReadFromStream(_buffer, true);

            if (Endianess == Endianess.LittleEndian)
            {
                return BinaryPrimitives.ReadDoubleLittleEndian(_buffer);
            }
            else
            {
                return BinaryPrimitives.ReadDoubleBigEndian(_buffer);
            }
        }

        /// <summary>
        /// Reads a string.
        /// </summary>
        [SkipLocalsInit]
        unsafe public string ReadStringLength(int length)
        {
            if (length == 0)
            {
                return string.Empty;
            }
            var bytes = stackalloc byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = ReadByte();
            }

            return new string((sbyte*)bytes, 0, length, Encoding.ASCII);
            //return Encoding.UTF8.GetString(bytes, Math.Min(charsRead, maxLength));
        }

        /// <summary>
        /// Reads a string.
        /// </summary>
        [SkipLocalsInit]
        unsafe public string ReadString(int maxLength = 1024)
        {
            Span<byte> bytes = stackalloc byte[Math.Min(maxLength, 1024)];
            Span<char> chars = stackalloc char[Math.Min(maxLength, 1024)];
            var temp = ReadByte();
            var bytesRead = 0;
            var length = Length;
            while (temp != 0 && (length == -1 || Position <= length))
            {
                if (bytesRead < maxLength && bytesRead < 1024)
                {
                    bytes[bytesRead] = temp;
                }
                temp = ReadByte();
                bytesRead++;
            }

            var charsRead = Encoding.UTF8.GetChars(bytes.Slice(0, bytesRead), chars);

            return chars.Slice(0, charsRead).ToString();
            //return Encoding.UTF8.GetString(bytes, Math.Min(charsRead, maxLength));
        }


        public Vector3 ReadVector3()
        {
            Vector3 v = new Vector3();
            v.X = ReadSingle();
            v.Y = ReadSingle();
            v.Z = ReadSingle();
            return v;
        }
        public Vector4 ReadVector4()
        {
            Vector4 v = new Vector4();
            v.X = ReadSingle();
            v.Y = ReadSingle();
            v.Z = ReadSingle();
            v.W = ReadSingle();
            return v;
        }

        public Matrix ReadMatrix()
        {
            Matrix m = new Matrix();
            m.M11 = ReadSingle();
            m.M21 = ReadSingle();
            m.M31 = ReadSingle();
            m.M41 = ReadSingle();
            m.M12 = ReadSingle();
            m.M22 = ReadSingle();
            m.M32 = ReadSingle();
            m.M42 = ReadSingle();
            m.M13 = ReadSingle();
            m.M23 = ReadSingle();
            m.M33 = ReadSingle();
            m.M43 = ReadSingle();
            m.M14 = ReadSingle();
            m.M24 = ReadSingle();
            m.M34 = ReadSingle();
            m.M44 = ReadSingle();
            return m;
        }




        //TODO: put this somewhere else...
        public static uint SizeOf(DataType type)
        {
            switch (type)
            {
                default:
                case DataType.Byte: return 1;
                case DataType.Int16: return 2;
                case DataType.Int32: return 4;
                case DataType.Int64: return 8;
                case DataType.Uint16: return 2;
                case DataType.Uint32: return 4;
                case DataType.Uint64: return 8;
                case DataType.Float: return 4;
                case DataType.Double: return 8;
                case DataType.String: return 0; //how long is a string..?
            }
        }

        public virtual void Dispose()
        {
            baseStream?.Dispose();
        }
    }

    public class DataWriter : IDisposable
    {
        private Stream baseStream;

        /// <summary>
        /// Gets or sets the endianess of the underlying stream.
        /// </summary>
        public Endianess Endianess
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the length of the underlying stream.
        /// </summary>
        public virtual long Length
        {
            get
            {
                return baseStream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the position within the underlying stream.
        /// </summary>
        public virtual long Position
        {
            get
            {
                return baseStream.Position;
            }
            set
            {
                baseStream.Position = value;
            }
        }

        internal virtual Stream GetStream()
        {
            return baseStream;
        }

        internal virtual void SetPositionAfterWrite(Stream stream)
        { }

        /// <summary>
        /// Initializes a new data writer for the specified stream.
        /// </summary>
        public DataWriter(Stream stream, Endianess endianess = Endianess.LittleEndian)
        {
            if (stream is not null)
            {
                this.baseStream = Stream.Synchronized(stream);
            }
            this.Endianess = endianess;
        }

        /// <summary>
        /// Writes data to the underlying stream. This is the only method that directly accesses
        /// the data in the underlying stream.
        /// </summary>
        protected virtual void WriteToStream(byte[] value, bool ignoreEndianess = false, int count = -1, int offset = 0)
        {
            var stream = GetStream();
            if (count == -1)
            {
                count = value.Length;
            }
            if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
            {
                var buffer = ArrayPool<byte>.Shared.Rent(count);
                try
                {
                    Array.Copy(value, offset, buffer, 0, count);
                    Array.Reverse(buffer, 0, count);
                    stream.Write(buffer, 0, count);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            else
            {
                stream.Write(value, offset, count);
            }
            SetPositionAfterWrite(stream);
        }

        protected virtual void WriteToStream(Span<byte> value, bool ignoreEndianess = false)
        {
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(value.Length);
            try
            {
                value.CopyTo(sharedBuffer);
                WriteToStream(sharedBuffer, count: value.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }

        protected virtual void WriteToStream(Memory<byte> buffer, bool ignoreEndianess = false)
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> array))
            {
                WriteToStream(array.Array!, offset: array.Offset, count: array.Count);
                return;
            }

            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.Span.CopyTo(sharedBuffer);
                WriteToStream(sharedBuffer, count: buffer.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }

        /// <summary>
        /// Writes a byte.
        /// </summary>
        public void Write(byte value)
        {
            WriteToStream(new byte[] { value });
        }

        /// <summary>
        /// Writes a sequence of bytes.
        /// </summary>
        public void Write(byte[] value)
        {
            WriteToStream(value, true);
        }

        public void Write(Span<byte> value)
        {
            WriteToStream(value, true);
        }

        public void Write(Memory<byte> value)
        {
            WriteToStream(value, true);
        }

        /// <summary>
        /// Writes a signed 16-bit value.
        /// </summary>
        public void Write(short value)
        {
            WriteToStream(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a signed 32-bit value.
        /// </summary>
        public void Write(int value)
        {
            WriteToStream(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a signed 64-bit value.
        /// </summary>
        public void Write(long value)
        {
            WriteToStream(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes an unsigned 16-bit value.
        /// </summary>
        public void Write(ushort value)
        {
            WriteToStream(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes an unsigned 32-bit value.
        /// </summary>
        public void Write(uint value)
        {
            WriteToStream(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes an unsigned 64-bit value.
        /// </summary>
        public void Write(ulong value)
        {
            WriteToStream(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a single precision floating point value.
        /// </summary>
        public void Write(float value)
        {
            WriteToStream(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a double precision floating point value.
        /// </summary>
        public void Write(double value)
        {
            WriteToStream(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a string.
        /// </summary>
        public void Write(string value)
        {
            foreach (var c in value)
                Write((byte)c);
            Write((byte)0);
        }



        public void Write(Vector3 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
        }
        public void Write(Vector4 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }

        public void Write(Matrix value)
        {
            Write(value.M11);
            Write(value.M21);
            Write(value.M31);
            Write(value.M41);
            Write(value.M12);
            Write(value.M22);
            Write(value.M32);
            Write(value.M42);
            Write(value.M13);
            Write(value.M23);
            Write(value.M33);
            Write(value.M43);
            Write(value.M14);
            Write(value.M24);
            Write(value.M34);
            Write(value.M44);
        }

        public virtual void Dispose()
        {
            baseStream?.Dispose();
        }

        public virtual void Close()
        {
            baseStream?.Close();
        }
    }



}
