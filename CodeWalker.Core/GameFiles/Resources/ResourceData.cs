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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

        private readonly int systemSize = 0;
        private readonly int graphicsSize = 0;

        public RpfResourceFileEntry FileEntry { get; set; }

        // this is a dictionary that contains all the resource blocks
        // which were read from this resource reader
        // This is needed to avoid a stack overflow because a ResourcePointerArray will try to read itself
        private Dictionary<long, IResourceBlock> _blockPool;
        public Dictionary<long, IResourceBlock> blockPool => _blockPool ??= new Dictionary<long, IResourceBlock>(17);

#if DEBUG
        public Dictionary<long, object> arrayPool => _arrayPool ??= new Dictionary<long, object>();

        private Dictionary<long, object> _arrayPool;
#endif
        
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
            : base((Stream?)null, endianess)
        {
            this.systemStream = systemStream;
            this.graphicsStream = graphicsStream;
            this.systemSize = (int)systemStream.Length;
            this.graphicsSize = (int)graphicsStream.Length;
        }

        public ResourceDataReader(RpfResourceFileEntry resentry, byte[] data, Endianess endianess = Endianess.LittleEndian)
            : this(resentry.SystemSize, resentry.GraphicsSize, data, endianess)
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

            if (systemSize > data.Length)
            {
                throw new ArgumentException($"systemSize {systemSize} is larger than data length ({data.Length})", nameof(resentry));
            }
            if (graphicsSize > data.Length)
            {
                throw new ArgumentException($"graphicsSize {graphicsSize} is larger than data length ({data.Length})", nameof(resentry));
            }
            this.systemStream = new MemoryStream(data, 0, (int)systemSize);
            this.graphicsStream = new MemoryStream(data, (int)systemSize, (int)graphicsSize);
        }

        public ResourceDataReader(int systemSize, int graphicsSize, byte[] data, Endianess endianess = Endianess.LittleEndian)
            : base((Stream)null, endianess)
        {
            if (systemSize > data.Length)
            {
                throw new ArgumentException($"systemSize {systemSize} is larger than data length ({data.Length})", nameof(systemSize));
            }
            if (graphicsSize > data.Length)
            {
                throw new ArgumentException($"graphicsSize {graphicsSize} is larger than data length ({data.Length})", nameof(graphicsSize));
            }
            if (systemSize + graphicsSize > data.Length)
            {
                throw new ArgumentException($"systemSize + graphicsSize {systemSize} is larger than data length ({data.Length})", nameof(systemSize));
            }

            this.systemStream = new MemoryStream(data, 0, systemSize);
            this.graphicsStream = new MemoryStream(data, systemSize, graphicsSize);
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
        public T ReadBlock<T>(params object[] parameters) where T : IResourceBlock, new()
        {
            var usepool = usePool<T>();
            if (usepool)
            {
                // make sure to return the same object if the same
                // block is read again...
                if (blockPool.TryGetValue(Position, out IResourceBlock? value))
                {
                    var cachedBlock = value;
                    if (cachedBlock is T tblk)
                    {
                        Position += cachedBlock.BlockLength;
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
            if (result is IResourceXXSystemBlock block)
            {
                result = (T)block.GetType(this, parameters);
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
        public T? ReadBlockAt<T>(ulong position, params object[] parameters) where T : IResourceBlock, new()
        {
            if (position == 0)
            {
                return default;
            }

            var positionBackup = Position;

            Position = (long)position;
            var result = ReadBlock<T>(parameters);
            Position = positionBackup;

            return result;
        }

        [return: NotNullIfNotNull(nameof(pointers))]
        public T[]? ReadBlocks<T>(ulong[]? pointers) where T : IResourceBlock, new()
        {
            if (pointers is null)
                return null;

            var count = pointers.Length;
            var items = new T[count];
            for (int i = 0; i < count; i++)
            {
                items[i] = ReadBlockAt<T>(pointers[i]);
            }
            return items;
        }

#if DEBUG
        public static int EntryAddedToCache = 0;
#endif
        // Only used for ResourceAnalyzer so can be made conditional, this optimizes away the if branch and arrayPool adition
        [Conditional("DEBUG")]
        private void AddEntryToArrayPool(long position, object result)
        {
#if DEBUG
            Interlocked.Increment(ref EntryAddedToCache);
            arrayPool[position] = result;
#endif
        }

        public unsafe byte[]? ReadBytesAt(ulong position, uint count, bool cache = true, byte[]? buffer = null)
        {
            long pos = (long)position;
            if ((pos <= 0) || (count == 0))
                return null;

            var posbackup = Position;
            Position = pos;
            var result = ReadFromStream((int)count, true, buffer);
            Position = posbackup;

            if (cache)
            {
                AddEntryToArrayPool((long)position, result);
            } 
            return result;
        }
        public ushort[]? ReadUshortsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0))
                return null;

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

            if (cache)
            {
                AddEntryToArrayPool((long)position, result);
            }

            return result;
        }
        public short[]? ReadShortsAt(ulong position, uint count, bool cache = true)
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


            if (cache)
            {
                AddEntryToArrayPool((long)position, result);
            }

            return result;
        }
        public uint[]? ReadUintsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0))
                return null;

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


            if (cache)
            {
                AddEntryToArrayPool((long)position, result);
            }

            return result;
        }
        public ulong[]? ReadUlongsAt(ulong position, uint count, bool cache = true)
        {
            if (position <= 0 || count == 0)
                return null;

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

            if (cache)
            {
                AddEntryToArrayPool((long)position, result);
            }

            return result;
        }
        public float[]? ReadFloatsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0))
                return null;

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

            if (cache)
            {
                AddEntryToArrayPool((long)position, result);
            }

            return result;
        }
        public T[]? ReadStructsAt<T>(ulong position, uint count, bool cache = true) where T : struct
        {
            if ((position <= 0) || (count == 0))
                return null;

            uint structsize = (uint)Marshal.SizeOf(typeof(T));
            var length = count * structsize;
            var data = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                ReadBytesAt(position, length, false, data);

                var result = new T[count];

                var resultSpan = MemoryMarshal.Cast<byte, T>(data.AsSpan(0, (int)length));
                resultSpan.CopyTo(result);

                if (cache)
                {
                    AddEntryToArrayPool((long)position, result);
                }

                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(data);
            }
        }
        public T[]? ReadStructs<T>(uint count) where T : struct
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

        public bool TryReadStruct<T>(out T result) where T : struct
        {
            var structsize = Marshal.SizeOf(typeof(T));
            var length = structsize;
            var data = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                var buffer = data.AsSpan(0, length);
                ReadBytes(buffer);
                return MemoryMarshal.TryRead(buffer, out result);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(data);
            }
        }

        public T ReadStruct<T>() where T : struct
        {
            TryReadStruct<T>(out var result);
            return result;
        }

        public bool TryReadStructAt<T>(long position, out T result) where T : struct
        {
            if (position <= 0)
            {
                result = default;
                return false;
            }

            var posbackup = Position;
            try
            {
                Position = position;
                return TryReadStruct(out result);
            }
            finally
            {
                Position = posbackup;
            }
        }

        public T ReadStructAt<T>(long position) where T : struct
        {
            TryReadStructAt<T>(position, out var result);
            return result;
        }

        public string? ReadStringAt(ulong position)
        {
            long newpos = (long)position;
            if (newpos <= 0)
                return null;

            var lastpos = Position;
            Position = newpos;
            var result = ReadString();
            Position = lastpos;
            AddEntryToArrayPool((long)position, result);
            return result;
        }

        public override void Dispose()
        {
            base.Dispose();

            systemStream?.Dispose();
            graphicsStream?.Dispose();
            GC.SuppressFinalize(this);
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

            MemoryMarshal.TryWrite(arr, in val);

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
            GC.SuppressFinalize(this);
        }
    }



    public interface IResourceBlockSpan
    {
        void Read(ref SequenceReader<byte> reader, params object[] parameters);
    }

    public interface IResourceXXSytemBlockSpan
    {
        IResourceSystemBlock GetType(ref SequenceReader<byte> reader, params object[] parameters);
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
        (long, IResourceBlock)[] GetParts();

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
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class ResourceSystemBlock : IResourceSystemBlock
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
        public virtual (long, IResourceBlock)[] GetParts()
        {
            return Array.Empty<(long, IResourceBlock)>();
        }

        /// <summary>
        /// Returns a list of data blocks that are referenced by this block.
        /// </summary>
        public virtual IResourceBlock[] GetReferences()
        {
            return Array.Empty<IResourceBlock>();
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
