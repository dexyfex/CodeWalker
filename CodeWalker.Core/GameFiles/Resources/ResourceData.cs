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

//shamelessly stolen and mangled


using CodeWalker.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    /// <summary>
    /// Represents a resource data reader.
    /// </summary>
    public class ResourceDataReader : DataReader, IDisposable
    {
        private const long SYSTEM_BASE = 0x50000000;
        private const long GRAPHICS_BASE = 0x60000000;

        private readonly Stream systemStream;
        private readonly Stream graphicsStream;

        private readonly long systemSize = 0;
        private readonly long graphicsSize = 0;

        public RpfResourceFileEntry FileEntry { get; set; }

        // this is a dictionary that contains all the resource blocks
        // which were read from this resource reader
        public Dictionary<long, IResourceBlock> blockPool
        {
            get
            {
                return _blockPool ??= new Dictionary<long, IResourceBlock>();
            }
        }
        public Dictionary<long, object> arrayPool {
            get
            {
                return _arrayPool ??= new Dictionary<long, object>();
            }
        }

        private Dictionary<long, object> _arrayPool;
        private Dictionary<long, IResourceBlock> _blockPool;
        private long position = SYSTEM_BASE;

        /// <summary>
        /// Gets the length of the underlying stream.
        /// </summary>
        public override long Length
        {
            get
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets or sets the position within the underlying stream.
        /// </summary>
        public override long Position
        {
            get => position;
            set {
                if ((value & SYSTEM_BASE) == SYSTEM_BASE)
                {
                    systemStream.Position = value & ~SYSTEM_BASE;
                }
                else if ((value & GRAPHICS_BASE) == GRAPHICS_BASE)
                {
                    graphicsStream.Position = value & ~GRAPHICS_BASE;
                } else
                {
                    throw new InvalidOperationException($"Invalid position {position}");
                }
                position = value;
            }
        }
        /// <summary>
        /// Initializes a new resource data reader for the specified system- and graphics-stream.
        /// </summary>
        public ResourceDataReader(Stream systemStream, Stream graphicsStream, Endianess endianess = Endianess.LittleEndian)
            : base((Stream)null, endianess)
        {
            this.systemStream = systemStream;
            this.graphicsStream = graphicsStream;
            this.systemSize = systemStream.Length;
            this.graphicsSize = graphicsStream.Length;
        }

        public ResourceDataReader(RpfResourceFileEntry resentry, byte[] data, Endianess endianess = Endianess.LittleEndian)
            : base((Stream)null, endianess)
        {
            FileEntry = resentry;
            this.systemSize = resentry.SystemSize;
            this.graphicsSize = resentry.GraphicsSize;

            //if (data != null)
            //{
            //    if (systemSize > data.Length)
            //    {
            //        systemSize = data.Length;
            //        graphicsSize = 0;
            //    }
            //    else if ((systemSize + graphicsSize) > data.Length)
            //    {
            //        graphicsSize = data.Length - systemSize;
            //    }
            //}

            if ((int)systemSize > data.Length)
            {
                throw new ArgumentException($"systemSize {systemSize} is larger than data length ({data.Length})", nameof(systemSize));
            }
            if ((int)graphicsSize > data.Length)
            {
                throw new ArgumentException($"graphicsSize {graphicsSize} is larger than data length ({data.Length})", nameof(graphicsSize));
            }
            this.systemStream = Stream.Synchronized(new MemoryStream(data, 0, (int)systemSize));
            this.graphicsStream = Stream.Synchronized(new MemoryStream(data, (int)systemSize, (int)graphicsSize));
        }

        public ResourceDataReader(int systemSize, int graphicsSize, byte[] data, Endianess endianess = Endianess.LittleEndian)
            : base((Stream)null, endianess)
        {
            this.systemStream = Stream.Synchronized(new MemoryStream(data, 0, systemSize));
            this.graphicsStream = Stream.Synchronized(new MemoryStream(data, systemSize, graphicsSize));
        }

        public override Stream GetStream()
        {
            if ((Position & SYSTEM_BASE) == SYSTEM_BASE)
            {
                return systemStream;
            }
            else if ((Position & GRAPHICS_BASE) == GRAPHICS_BASE)
            {
                return graphicsStream;
            }

            throw new InvalidOperationException("illegal position!");
        }

        internal override void SetPositionAfterRead(Stream stream)
        {

            if (stream == systemStream)
            {
                position = systemStream.Position | SYSTEM_BASE;
            }
            else if (stream == graphicsStream)
            {
                position = graphicsStream.Position | GRAPHICS_BASE;
            }

            if ((position & SYSTEM_BASE) != SYSTEM_BASE && (position & GRAPHICS_BASE) != GRAPHICS_BASE)
            {
                throw new InvalidOperationException($"Invalid position {position}");
            }
        }

        /// <summary>
        /// Reads a block.
        /// </summary>
        public T ReadBlock<T>(params object[] parameters) where T : IResourceBlock, new()
        {
            var usepool = !typeof(IResourceNoCacheBlock).IsAssignableFrom(typeof(T));
            if (usepool)
            {
                // make sure to return the same object if the same
                // block is read again...
                if (blockPool.ContainsKey(Position))
                {
                    var block = blockPool[Position];
                    if (block is T tblk)
                    {
                        Position += block.BlockLength;
                        return tblk;
                    }
                    else
                    {
                        usepool = false;
                    }
                }
            }

            var result = new T();


            // replace with correct type...
            if (result is IResourceXXSystemBlock)
            {
                result = (T)((IResourceXXSystemBlock)result).GetType(this, parameters);
            }

            if (result == null)
            {
                return default;
            }

            if (usepool)
            {
                blockPool[Position] = result;
            }

            result.Read(this, parameters);

            return result;
        }

        /// <summary>
        /// Reads a block at a specified position.
        /// </summary>
        public T ReadBlockAt<T>(ulong position, params object[] parameters) where T : IResourceBlock, new()
        {
            if (position != 0)
            {
                var positionBackup = Position;

                Position = (long)position;
                var result = ReadBlock<T>(parameters);
                Position = positionBackup;

                return result;
            }
            else
            {
                return default(T);
            }
        }

        public T[] ReadBlocks<T>(ulong[] pointers) where T : IResourceBlock, new()
        {
            if (pointers == null) return null;
            var count = pointers.Length;
            var items = new T[count];
            for (int i = 0; i < count; i++)
            {
                items[i] = ReadBlockAt<T>(pointers[i]);
            }
            return items;
        }

        internal const int StackallocThreshold = 512;
        public unsafe byte[] ReadBytesAt(ulong position, uint count, bool cache = true, byte[] buffer = null)
        {
            long pos = (long)position;
            if ((pos <= 0) || (count == 0)) return null;
            var posbackup = Position;
            Position = pos;
            var result = ReadBytes((int)count, buffer);
            Position = posbackup;
            if (cache) arrayPool[(long)position] = result;
            return result;
        }
        public ushort[] ReadUshortsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0)) return null;

            var result = new ushort[count];
            var length = count * sizeof(ushort);
            var data = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                ReadBytesAt(position, length, false, data);
                Buffer.BlockCopy(data, 0, result, 0, (int)length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(data);
            }


            //var posbackup = Position;
            //Position = position;
            //var result2 = new ushort[count];
            //for (uint i = 0; i < count; i++)
            //{
            //    result2[i] = ReadUInt16();
            //}
            //Position = posbackup;

            if (cache) arrayPool[(long)position] = result;

            return result;
        }
        public short[] ReadShortsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0)) return null;
            var result = new short[count];
            var length = count * sizeof(short);
            var buffer = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                ReadBytesAt(position, length, false, buffer);
                Buffer.BlockCopy(buffer, 0, result, 0, (int)length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }


            if (cache) arrayPool[(long)position] = result;

            return result;
        }
        public uint[] ReadUintsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0)) return null;

            var result = new uint[count];
            var length = count * sizeof(uint);
            var buffer = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                ReadBytesAt(position, length, false, buffer);
                Buffer.BlockCopy(buffer, 0, result, 0, (int)length);
            } finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }


            if (cache) arrayPool[(long)position] = result;

            return result;
        }
        public ulong[] ReadUlongsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0)) return null;

            var result = new ulong[count];
            var length = count * sizeof(ulong);
            var data = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                ReadBytesAt(position, length, false, data);
                Buffer.BlockCopy(data, 0, result, 0, (int)length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(data);
            }

            if (cache) arrayPool[(long)position] = result;

            return result;
        }
        public float[] ReadFloatsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0)) return null;

            var result = new float[count];
            var length = count * sizeof(float);
            var data = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                ReadBytesAt(position, length, false, data);
                Buffer.BlockCopy(data, 0, result, 0, (int)length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(data);
            }

            if (cache) arrayPool[(long)position] = result;

            return result;
        }
        public T[] ReadStructsAt<T>(ulong position, uint count, bool cache = true) where T : struct
        {
            if ((position <= 0) || (count == 0)) return null;

            uint structsize = (uint)Marshal.SizeOf(typeof(T));
            var length = count * structsize;
            var data = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                ReadBytesAt(position, length, false, data);

                var result = new T[count];

                var resultSpan = MemoryMarshal.Cast<byte, T>(data.AsSpan(0, (int)length));
                resultSpan.CopyTo(result);

                if (cache) arrayPool[(long)position] = result;

                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(data);
            }
        }
        public T[] ReadStructs<T>(uint count) where T : struct
        {
            uint structsize = (uint)Marshal.SizeOf(typeof(T));
            var result = new T[count];
            var length = count * structsize;
            var data = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                ReadBytes((int)length, data);

                var resultSpan = MemoryMarshal.Cast<byte, T>(data.AsSpan(0, (int)length));
                resultSpan.CopyTo(result);

                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(data);
            }

        }

        public T ReadStruct<T>() where T : struct
        {
            uint structsize = (uint)Marshal.SizeOf(typeof(T));
            var length = structsize;
            var data = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                ReadBytes((int)length, data);
                MemoryMarshal.TryRead<T>(data, out var value);

                return value;
            } finally
            {
                ArrayPool<byte>.Shared.Return(data);
            }

        }

        public T ReadStructAt<T>(long position) where T : struct
        {
            if ((position <= 0)) return default(T);
            var posbackup = Position;
            Position = (long)position;
            var result = ReadStruct<T>();
            Position = posbackup;
            return result;
        }

        public string ReadStringAt(ulong position)
        {
            long newpos = (long)position;
            if ((newpos <= 0)) return null;
            var lastpos = Position;
            Position = newpos;
            var result = ReadString();
            Position = lastpos;
            arrayPool[newpos] = result;
            return result;
        }

        public override void Dispose()
        {
            base.Dispose();

            systemStream?.Dispose();
            graphicsStream?.Dispose();
        }
    }



    /// <summary>
    /// Represents a resource data writer.
    /// </summary>
    public class ResourceDataWriter : DataWriter, IDisposable
    {
        private const long SYSTEM_BASE = 0x50000000;
        private const long GRAPHICS_BASE = 0x60000000;

        private Stream systemStream;
        private Stream graphicsStream;

        /// <summary>
        /// Gets the length of the underlying stream.
        /// </summary>
        public override long Length
        {
            get
            {
                return -1;
            }
        }

        private long position = SYSTEM_BASE;
        public override long Position
        {
            get => position;
            set
            {
                if ((value & SYSTEM_BASE) == SYSTEM_BASE)
                {
                    systemStream.Position = value & ~SYSTEM_BASE;
                }
                else if ((value & GRAPHICS_BASE) == GRAPHICS_BASE)
                {
                    graphicsStream.Position = value & ~GRAPHICS_BASE;
                }
                position = value;
            }
        }

        /// <summary>
        /// Initializes a new resource data reader for the specified system- and graphics-stream.
        /// </summary>
        public ResourceDataWriter(Stream systemStream, Stream graphicsStream, Endianess endianess = Endianess.LittleEndian)
            : base((Stream)null, endianess)
        {
            this.systemStream = Stream.Synchronized(systemStream);
            this.graphicsStream = Stream.Synchronized(graphicsStream);
        }

        internal override Stream GetStream()
        {
            if ((Position & SYSTEM_BASE) == SYSTEM_BASE)
            {
                return systemStream;
            }
            else if ((Position & GRAPHICS_BASE) == GRAPHICS_BASE)
            {
                return graphicsStream;
            }
            throw new InvalidOperationException("illegal position!");
        }

        internal override void SetPositionAfterWrite(Stream stream)
        {
            if (stream == systemStream)
            {
                position = systemStream.Position | SYSTEM_BASE;
            }
            else if (stream == graphicsStream)
            {
                position = graphicsStream.Position | GRAPHICS_BASE;
            }
        }

        /// <summary>
        /// Writes a block.
        /// </summary>
        public void WriteBlock(IResourceBlock value)
        {
            value.Write(this);
        }




        public void WriteStruct<T>(T val) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));

            var arr = new byte[size];

            MemoryMarshal.TryWrite(arr, ref val);

            Write(arr);

            //byte[] arr = new byte[size];
            //IntPtr ptr = Marshal.AllocHGlobal(size);
            //Marshal.StructureToPtr(val, ptr, true);
            //Marshal.Copy(ptr, arr, 0, size);
            //Marshal.FreeHGlobal(ptr);
            //Write(arr);
        }

        public void WriteStructs<T>(Span<T> val) where T : struct
        {
            if (val == null) return;

            var bytes = MemoryMarshal.AsBytes(val);

            Write(bytes);

            //foreach (var v in val)
            //{
            //    WriteStruct(v);
            //}
        }

        public void WriteStructs<T>(T[] val) where T : struct
        {
            if (val == null) return;
            var bytes = MemoryMarshal.AsBytes<T>(val);

            Write(bytes);
        }



        /// <summary>
        /// Write enough bytes to the stream to get to the specified alignment.
        /// </summary>
        /// <param name="alignment">value to align to</param>
        public void WritePadding(int alignment)
        {
            var pad = ((alignment - (Position % alignment)) % alignment);
            if (pad > 0) Write(new byte[pad]);
        }

        public void WriteUlongs(ulong[] val)
        {
            if (val == null) return;
            foreach (var v in val)
            {
                Write(v);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            systemStream?.Dispose();
            graphicsStream?.Dispose();
        }
    }





    /// <summary>
    /// Represents a data block in a resource file.
    /// </summary>
    public interface IResourceBlock
    {
        /// <summary>
        /// Gets or sets the position of the data block.
        /// </summary>
        long FilePosition { get; set; }

        /// <summary>
        /// Gets the length of the data block.
        /// </summary>
        long BlockLength { get; }

        /// <summary>
        /// Reads the data block.
        /// </summary>
        void Read(ResourceDataReader reader, params object[] parameters);

        /// <summary>
        /// Writes the data block.
        /// </summary>
        void Write(ResourceDataWriter writer, params object[] parameters);
    }

    /// <summary>
    /// Represents a data block of the system segement in a resource file.
    /// </summary>
    public interface IResourceSystemBlock : IResourceBlock
    {
        /// <summary>
        /// Returns a list of data blocks that are part of this block.
        /// </summary>
        Tuple<long, IResourceBlock>[] GetParts();

        /// <summary>
        /// Returns a list of data blocks that are referenced by this block.
        /// </summary>
        IResourceBlock[] GetReferences();
    }

    public interface IResourceXXSystemBlock : IResourceSystemBlock
    {
        IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters);
    }

    /// <summary>
    /// Represents a data block of the graphics segmenet in a resource file.
    /// </summary>
    public interface IResourceGraphicsBlock : IResourceBlock
    { }


    /// <summary>
    /// Represents a data block that won't get cached while loading.
    /// </summary>
    public interface IResourceNoCacheBlock : IResourceBlock
    { }



    /// <summary>
    /// Represents a data block of the system segement in a resource file.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))] public abstract class ResourceSystemBlock : IResourceSystemBlock
    {
        private long position;

        /// <summary>
        /// Gets or sets the position of the data block.
        /// </summary>
        public virtual long FilePosition
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                foreach (var part in GetParts())
                {
                    part.Item2.FilePosition = value + part.Item1;
                }
            }
        }

        /// <summary>
        /// Gets the length of the data block.
        /// </summary>
        public abstract long BlockLength
        {
            get;
        }

        /// <summary>
        /// Reads the data block.
        /// </summary>
        public abstract void Read(ResourceDataReader reader, params object[] parameters);

        /// <summary>
        /// Writes the data block.
        /// </summary>
        public abstract void Write(ResourceDataWriter writer, params object[] parameters);

        /// <summary>
        /// Returns a list of data blocks that are part of this block.
        /// </summary>
        public virtual Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[0];
        }

        /// <summary>
        /// Returns a list of data blocks that are referenced by this block.
        /// </summary>
        public virtual IResourceBlock[] GetReferences()
        {
            return new IResourceBlock[0];
        }
    }

    public abstract class ResourecTypedSystemBlock : ResourceSystemBlock, IResourceXXSystemBlock
    {
        public abstract IResourceSystemBlock GetType(ResourceDataReader reader, params object[] parameters);
    }

    /// <summary>
    /// Represents a data block of the graphics segmenet in a resource file.
    /// </summary>
    public abstract class ResourceGraphicsBlock : IResourceGraphicsBlock
    {
        /// <summary>
        /// Gets or sets the position of the data block.
        /// </summary>
        public virtual long FilePosition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the length of the data block.
        /// </summary>
        public abstract long BlockLength
        {
            get;
        }

        /// <summary>
        /// Reads the data block.
        /// </summary>
        public abstract void Read(ResourceDataReader reader, params object[] parameters);

        /// <summary>
        /// Writes the data block.
        /// </summary>
        public abstract void Write(ResourceDataWriter writer, params object[] parameters);
    }







    //public interface ResourceDataStruct
    //{
    //    void Read(ResourceDataReader reader);
    //    void Write(ResourceDataWriter writer);
    //}

}
