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


using System;
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
    public class ResourceDataReader : DataReader
    {
        private const long SYSTEM_BASE = 0x50000000;
        private const long GRAPHICS_BASE = 0x60000000;

        private Stream systemStream;
        private Stream graphicsStream;

        public RpfResourceFileEntry FileEntry { get; set; }

        // this is a dictionary that contains all the resource blocks
        // which were read from this resource reader
        public Dictionary<long, IResourceBlock> blockPool = new Dictionary<long, IResourceBlock>();
        public Dictionary<long, object> arrayPool = new Dictionary<long, object>();

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
            get;
            set;
        }

        /// <summary>
        /// Initializes a new resource data reader for the specified system- and graphics-stream.
        /// </summary>
        public ResourceDataReader(Stream systemStream, Stream graphicsStream, Endianess endianess = Endianess.LittleEndian)
            : base((Stream)null, endianess)
        {
            this.systemStream = systemStream;
            this.graphicsStream = graphicsStream;
        }

        public ResourceDataReader(RpfResourceFileEntry resentry, byte[] data, Endianess endianess = Endianess.LittleEndian)
            : base((Stream)null, endianess)
        {
            FileEntry = resentry;
            var systemSize = resentry.SystemSize;
            var graphicsSize = resentry.GraphicsSize;

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

            this.systemStream = new MemoryStream(data, 0, systemSize);
            this.graphicsStream = new MemoryStream(data, systemSize, graphicsSize);
            Position = 0x50000000;
        }

        public ResourceDataReader(int systemSize, int graphicsSize, byte[] data, Endianess endianess = Endianess.LittleEndian)
            : base((Stream)null, endianess)
        {
            this.systemStream = new MemoryStream(data, 0, systemSize);
            this.graphicsStream = new MemoryStream(data, systemSize, graphicsSize);
            Position = 0x50000000;
        }



        /// <summary>
        /// Reads data from the underlying stream. This is the only method that directly accesses
        /// the data in the underlying stream.
        /// </summary>
        protected override byte[] ReadFromStream(int count, bool ignoreEndianess = false)
        {
            if ((Position & SYSTEM_BASE) == SYSTEM_BASE)
            {
                // read from system stream...

                systemStream.Position = Position & ~0x50000000;

                var buffer = new byte[count];
                systemStream.Read(buffer, 0, count);

                // handle endianess
                if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
                {
                    Array.Reverse(buffer);
                }

                Position = systemStream.Position | 0x50000000;
                return buffer;

            }
            if ((Position & GRAPHICS_BASE) == GRAPHICS_BASE)
            {
                // read from graphic stream...

                graphicsStream.Position = Position & ~0x60000000;

                var buffer = new byte[count];
                graphicsStream.Read(buffer, 0, count);

                // handle endianess
                if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
                {
                    Array.Reverse(buffer);
                }

                Position = graphicsStream.Position | 0x60000000;
                return buffer;
            }
            throw new Exception("illegal position!");
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
                return default(T);
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


        public byte[] ReadBytesAt(ulong position, uint count, bool cache = true)
        {
            long pos = (long)position;
            if ((pos <= 0) || (count == 0)) return null;
            var posbackup = Position;
            Position = pos;
            var result = ReadBytes((int)count);
            Position = posbackup;
            if (cache) arrayPool[(long)position] = result;
            return result;
        }
        public ushort[] ReadUshortsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0)) return null;

            var result = new ushort[count];
            var length = count * 2;
            byte[] data = ReadBytesAt(position, length, false);
            Buffer.BlockCopy(data, 0, result, 0, (int)length);

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
            var length = count * 2;
            byte[] data = ReadBytesAt(position, length, false);
            Buffer.BlockCopy(data, 0, result, 0, (int)length);

            if (cache) arrayPool[(long)position] = result;

            return result;
        }
        public uint[] ReadUintsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0)) return null;

            var result = new uint[count];
            var length = count * 4;
            byte[] data = ReadBytesAt(position, length, false);
            Buffer.BlockCopy(data, 0, result, 0, (int)length);

            //var posbackup = Position;
            //Position = position;
            //var result = new uint[count];
            //for (uint i = 0; i < count; i++)
            //{
            //    result[i] = ReadUInt32();
            //}
            //Position = posbackup;

            if (cache) arrayPool[(long)position] = result;

            return result;
        }
        public ulong[] ReadUlongsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0)) return null;

            var result = new ulong[count];
            var length = count * 8;
            byte[] data = ReadBytesAt(position, length, false);
            Buffer.BlockCopy(data, 0, result, 0, (int)length);

            //var posbackup = Position;
            //Position = position;
            //var result = new ulong[count];
            //for (uint i = 0; i < count; i++)
            //{
            //    result[i] = ReadUInt64();
            //}
            //Position = posbackup;

            if (cache) arrayPool[(long)position] = result;

            return result;
        }
        public float[] ReadFloatsAt(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0)) return null;

            var result = new float[count];
            var length = count * 4;
            byte[] data = ReadBytesAt(position, length, false);
            Buffer.BlockCopy(data, 0, result, 0, (int)length);

            //var posbackup = Position;
            //Position = position;
            //var result = new float[count];
            //for (uint i = 0; i < count; i++)
            //{
            //    result[i] = ReadSingle();
            //}
            //Position = posbackup;

            if (cache) arrayPool[(long)position] = result;

            return result;
        }
        public T[] ReadStructsAt<T>(ulong position, uint count, bool cache = true)
        {
            if ((position <= 0) || (count == 0)) return null;

            uint structsize = (uint)Marshal.SizeOf(typeof(T));
            var length = count * structsize;
            byte[] data = ReadBytesAt(position, length, false);

            //var result2 = new T[count];
            //Buffer.BlockCopy(data, 0, result2, 0, (int)length); //error: "object must be an array of primitives" :(

            //var result = new T[count];
            //GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            //var h = handle.AddrOfPinnedObject();
            //for (uint i = 0; i < count; i++)
            //{
            //    result[i] = Marshal.PtrToStructure<T>(h + (int)(i * structsize));
            //}
            //handle.Free();

            var result = new T[count];
            GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            var h = handle.AddrOfPinnedObject();
            Marshal.Copy(data, 0, h, (int)length);
            handle.Free();


            if (cache) arrayPool[(long)position] = result;

            return result;
        }
        public T[] ReadStructs<T>(uint count)
        {
            uint structsize = (uint)Marshal.SizeOf(typeof(T));
            var result = new T[count];
            var length = count * structsize;
            byte[] data = ReadBytes((int)length);

            //GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            //var h = handle.AddrOfPinnedObject();
            //for (uint i = 0; i < count; i++)
            //{
            //    result[i] = Marshal.PtrToStructure<T>(h + (int)(i * structsize));
            //}
            //handle.Free();

            GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            var h = handle.AddrOfPinnedObject();
            Marshal.Copy(data, 0, h, (int)length);
            handle.Free();


            return result;
        }

        public T ReadStruct<T>() where T : struct
        {
            uint structsize = (uint)Marshal.SizeOf(typeof(T));
            var length = structsize;
            byte[] data = ReadBytes((int)length);
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var h = handle.AddrOfPinnedObject();
            var result = Marshal.PtrToStructure<T>(h);
            handle.Free();
            return result;
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

    }



    /// <summary>
    /// Represents a resource data writer.
    /// </summary>
    public class ResourceDataWriter : DataWriter
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

        /// <summary>
        /// Gets or sets the position within the underlying stream.
        /// </summary>
        public override long Position
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new resource data reader for the specified system- and graphics-stream.
        /// </summary>
        public ResourceDataWriter(Stream systemStream, Stream graphicsStream, Endianess endianess = Endianess.LittleEndian)
            : base((Stream)null, endianess)
        {
            this.systemStream = systemStream;
            this.graphicsStream = graphicsStream;
        }

        /// <summary>
        /// Writes data to the underlying stream. This is the only method that directly accesses
        /// the data in the underlying stream.
        /// </summary>
        protected override void WriteToStream(byte[] value, bool ignoreEndianess = true)
        {
            if ((Position & SYSTEM_BASE) == SYSTEM_BASE)
            {
                // write to system stream...

                systemStream.Position = Position & ~SYSTEM_BASE;

                // handle endianess
                if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
                {
                    var buf = (byte[])value.Clone();
                    Array.Reverse(buf);
                    systemStream.Write(buf, 0, buf.Length);
                }
                else
                {
                    systemStream.Write(value, 0, value.Length);
                }

                Position = systemStream.Position | 0x50000000;
                return;

            }
            if ((Position & GRAPHICS_BASE) == GRAPHICS_BASE)
            {
                // write to graphic stream...

                graphicsStream.Position = Position & ~GRAPHICS_BASE;

                // handle endianess
                if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
                {
                    var buf = (byte[])value.Clone();
                    Array.Reverse(buf);
                    graphicsStream.Write(buf, 0, buf.Length);
                }
                else
                {
                    graphicsStream.Write(value, 0, value.Length);
                }

                Position = graphicsStream.Position | 0x60000000;
                return;
            }

            throw new Exception("illegal position!");
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
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(val, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            Write(arr);
        }
        public void WriteStructs<T>(T[] val) where T : struct
        {
            if (val == null) return;
            foreach (var v in val)
            {
                WriteStruct(v);
            }
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
