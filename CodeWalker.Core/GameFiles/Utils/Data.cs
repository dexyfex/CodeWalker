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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public enum Endianess
    {
        LittleEndian,
        BigEndian
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

    public class DataReader
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

        /// <summary>
        /// Initializes a new data reader for the specified stream.
        /// </summary>
        public DataReader(Stream stream, Endianess endianess = Endianess.LittleEndian)
        {
            this.baseStream = stream;
            this.Endianess = endianess;
        }

        /// <summary>
        /// Reads data from the underlying stream. This is the only method that directly accesses
        /// the data in the underlying stream.
        /// </summary>
        protected virtual byte[] ReadFromStream(int count, bool ignoreEndianess = false)
        {
            var buffer = new byte[count];
            baseStream.Read(buffer, 0, count);

            // handle endianess
            if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
            {
                Array.Reverse(buffer);
            }

            return buffer;
        }

        /// <summary>
        /// Reads a byte.
        /// </summary>
        public byte ReadByte()
        {
            return ReadFromStream(1)[0];
        }

        /// <summary>
        /// Reads a sequence of bytes.
        /// </summary>
        public byte[] ReadBytes(int count)
        {
            return ReadFromStream(count, true);
        }

        /// <summary>
        /// Reads a signed 16-bit value.
        /// </summary>
        public short ReadInt16()
        {
            return BitConverter.ToInt16(ReadFromStream(2), 0);
        }

        /// <summary>
        /// Reads a signed 32-bit value.
        /// </summary>
        public int ReadInt32()
        {
            return BitConverter.ToInt32(ReadFromStream(4), 0);
        }

        /// <summary>
        /// Reads a signed 64-bit value.
        /// </summary>
        public long ReadInt64()
        {
            return BitConverter.ToInt64(ReadFromStream(8), 0);
        }

        /// <summary>
        /// Reads an unsigned 16-bit value.
        /// </summary>
        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(ReadFromStream(2), 0);
        }

        /// <summary>
        /// Reads an unsigned 32-bit value.
        /// </summary>
        public uint ReadUInt32()
        {
            return BitConverter.ToUInt32(ReadFromStream(4), 0);
        }

        /// <summary>
        /// Reads an unsigned 64-bit value.
        /// </summary>
        public ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(ReadFromStream(8), 0);
        }

        /// <summary>
        /// Reads a single precision floating point value.
        /// </summary>
        public float ReadSingle()
        {
            return BitConverter.ToSingle(ReadFromStream(4), 0);
        }

        /// <summary>
        /// Reads a double precision floating point value.
        /// </summary>
        public double ReadDouble()
        {
            return BitConverter.ToDouble(ReadFromStream(8), 0);
        }

        /// <summary>
        /// Reads a string.
        /// </summary>
        public string ReadString()
        {
            var bytes = new List<byte>();
            var temp = ReadFromStream(1)[0];
            while (temp != 0)
            {
                bytes.Add(temp);
                temp = ReadFromStream(1)[0];
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
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




    }

    public class DataWriter
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

        /// <summary>
        /// Initializes a new data writer for the specified stream.
        /// </summary>
        public DataWriter(Stream stream, Endianess endianess = Endianess.LittleEndian)
        {
            this.baseStream = stream;
            this.Endianess = endianess;
        }

        /// <summary>
        /// Writes data to the underlying stream. This is the only method that directly accesses
        /// the data in the underlying stream.
        /// </summary>
        protected virtual void WriteToStream(byte[] value, bool ignoreEndianess = false)
        {
            if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
            {
                var buffer = (byte[])value.Clone();
                Array.Reverse(buffer);
                baseStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                baseStream.Write(value, 0, value.Length);
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

    }



}
