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


using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{



    [TypeConverter(typeof(ExpandableObjectConverter))] public class string_r : ResourceSystemBlock
    {
        // Represents a string that can be referenced in a resource structure.

        /// <summary>
        /// Gets the length of the string.
        /// </summary>
        public override long BlockLength
        {
            get { return Value.Length + 1; }
        }

        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        public string Value { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Value = reader.ReadString();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            writer.Write(Value);
        }

        public static explicit operator string(string_r value)
        {
            return value.Value;
        }

        public static explicit operator string_r(string value)
        {
            var x = new string_r();
            x.Value = value;
            return x;
        }
        public override string ToString()
        {
            return Value;
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public struct Matrix3_s
    {
        public Vector4 Row1 { get; set; }
        public Vector4 Row2 { get; set; }
        public Vector4 Row3 { get; set; }

        // structure data
        //public float Unknown_01 { get; set; }
        //public float Unknown_02 { get; set; }
        //public float Unknown_03 { get; set; }
        //public float Unknown_04 { get; set; }
        //public float Unknown_11 { get; set; }
        //public float Unknown_12 { get; set; }
        //public float Unknown_13 { get; set; }
        //public float Unknown_14 { get; set; }
        //public float Unknown_21 { get; set; }
        //public float Unknown_22 { get; set; }
        //public float Unknown_23 { get; set; }
        //public float Unknown_24 { get; set; }


        public Matrix3_s(float[] a)
        {
            if ((a != null) && (a.Length == 12))
            {
                Row1 = new Vector4(a[0], a[1], a[2], a[3]);
                Row2 = new Vector4(a[4], a[5], a[6], a[7]);
                Row3 = new Vector4(a[8], a[9], a[10], a[11]);
            }
            else
            {
                Row1 = new Vector4(1, 0, 0, 0);
                Row2 = new Vector4(0, 1, 0, 0);
                Row3 = new Vector4(0, 0, 1, 0);
            }
        }
        public float[] ToArray()
        {
            return new[] { Row1.X, Row1.Y, Row1.Z, Row1.W, Row2.X, Row2.Y, Row2.Z, Row2.W, Row3.X, Row3.Y, Row3.Z, Row3.W };
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct Matrix4F_s
    {
        public Vector3 Column1 { get; set; }
        public uint Flags1 { get; set; }
        public Vector3 Column2 { get; set; }
        public uint Flags2 { get; set; }
        public Vector3 Column3 { get; set; }
        public uint Flags3 { get; set; }
        public Vector3 Column4 { get; set; }
        public uint Flags4 { get; set; }

        public Matrix4F_s(bool identity)
        {
            if (identity)
            {
                Column1 = Vector3.UnitX;
                Column2 = Vector3.UnitY;
                Column3 = Vector3.UnitZ;
                Column4 = Vector3.Zero;
            }
            else
            {
                Column1 = Vector3.Zero;
                Column2 = Vector3.Zero;
                Column3 = Vector3.Zero;
                Column4 = Vector3.Zero;
            }
            Flags1 = 0x7f800001;
            Flags2 = 0x7f800001;
            Flags3 = 0x7f800001;
            Flags4 = 0x7f800001;
        }
        public Matrix4F_s(float v)
        {
            Column1 = new Vector3(v);
            Column2 = new Vector3(v);
            Column3 = new Vector3(v);
            Column4 = new Vector3(v);
            Flags1 = 0x7f800001;
            Flags2 = 0x7f800001;
            Flags3 = 0x7f800001;
            Flags4 = 0x7f800001;
        }
        public Matrix4F_s(float[] a)
        {
            if ((a != null) && (a.Length == 12))
            {
                Column1 = new Vector3(a[0], a[1], a[2]);
                Column2 = new Vector3(a[3], a[4], a[5]);
                Column3 = new Vector3(a[6], a[7], a[8]);
                Column4 = new Vector3(a[9], a[10], a[11]);
            }
            else
            {
                Column1 = Vector3.UnitX;
                Column2 = Vector3.UnitY;
                Column3 = Vector3.UnitZ;
                Column4 = Vector3.Zero;
            }
            Flags1 = 0x7f800001;
            Flags2 = 0x7f800001;
            Flags3 = 0x7f800001;
            Flags4 = 0x7f800001;
        }
        public Matrix4F_s(Matrix m)
        {
            Column1 = new Vector3(m.M11, m.M12, m.M13);
            Column2 = new Vector3(m.M21, m.M22, m.M23);
            Column3 = new Vector3(m.M31, m.M32, m.M33);
            Column4 = new Vector3(m.M41, m.M42, m.M43);
            Flags1 = 0x7f800001;
            Flags2 = 0x7f800001;
            Flags3 = 0x7f800001;
            Flags4 = 0x7f800001;
        }

        public float[] ToArray()
        {
            return new[] { Column1.X, Column1.Y, Column1.Z, Column2.X, Column2.Y, Column2.Z, Column3.X, Column3.Y, Column3.Z, Column4.X, Column4.Y, Column4.Z };
        }

        public Matrix ToMatrix()
        {
            return new Matrix(Column1.X, Column1.Y, Column1.Z, 0, Column2.X, Column2.Y, Column2.Z, 0, Column3.X, Column3.Y, Column3.Z, 0, Column4.X, Column4.Y, Column4.Z, 1);
        }


        public static Matrix4F_s Identity { get { return new Matrix4F_s(true); } }
        public static Matrix4F_s Zero { get { return new Matrix4F_s(false); } }



    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public struct AABB_s
    {
        public Vector4 Min { get; set; }
        public Vector4 Max { get; set; }
    }


    

    
    [TypeConverter(typeof(ExpandableObjectConverter))] public struct FlagsByte
    {
        public byte Value { get; set; }

        public string Hex
        {
            get
            {
                return Convert.ToString(Value, 16).ToUpper().PadLeft(2, '0');
            }
        }

        public string Bin
        {
            get
            {
                return Convert.ToString(Value, 2).PadLeft(8, '0');
            }
        }


        public FlagsByte(byte v)
        {
            Value = v;
        }

        public override string ToString()
        {
            return Bin + " | 0x" + Hex + " | " + Value.ToString();
        }
        public string ToShortString()
        {
            return Bin + " | 0x" + Hex;
        }
        public string ToHexString()
        {
            return "0x" + Hex;
        }

        public static implicit operator FlagsByte(byte v)
        {
            return new FlagsByte(v);
        }
        public static implicit operator byte(FlagsByte v)
        {
            return v.Value;  //implicit conversion
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct FlagsUshort
    {
        public ushort Value { get; set; }

        public string Hex
        {
            get
            {
                return Convert.ToString(Value, 16).ToUpper().PadLeft(4, '0');
            }
        }

        public string Bin
        {
            get
            {
                return Convert.ToString(Value, 2).PadLeft(16, '0');
            }
        }

        public FlagsUshort(ushort v)
        {
            Value = v;
        }

        public override string ToString()
        {
            return Bin + " | 0x" + Hex + " | " + Value.ToString();
        }
        public string ToShortString()
        {
            return Bin + " | 0x" + Hex;
        }

        public static implicit operator FlagsUshort(ushort v)
        {
            return new FlagsUshort(v);
        }
        public static implicit operator ushort(FlagsUshort v)
        {
            return v.Value;  //implicit conversion
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct FlagsUint
    {
        public uint Value { get; set; }

        public string Hex
        {
            get
            {
                return Convert.ToString(Value, 16).ToUpper().PadLeft(8, '0');
            }
        }

        public string Bin
        {
            get
            {
                return Convert.ToString(Value, 2).PadLeft(32, '0');
            }
        }

        public FlagsUint(uint v)
        {
            Value = v;
        }

        public override string ToString()
        {
            return Bin + " | 0x" + Hex + " | " + Value.ToString();
        }
        public string ToShortString()
        {
            return Bin + " | 0x" + Hex;
        }

        public static implicit operator FlagsUint(uint v)
        {
            return new FlagsUint(v);
        }
        public static implicit operator uint(FlagsUint v)
        {
            return v.Value;  //implicit conversion
        }

    }




    [TypeConverter(typeof(ExpandableObjectConverter))] public abstract class ListBase<T> : ResourceSystemBlock, ICustomTypeDescriptor, IList<T> where T : IResourceSystemBlock, new()
    {

        // this is the data...
        public List<T> Data { get; set; }





        public T this[int index]
        {
            get
            {
                return Data[index];
            }
            set
            {
                Data[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return Data?.Count ?? 0;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }






        public ListBase()
        {
            //Data = new List<T>();
        }





        public void Add(T item)
        {
            if (Data == null)
            {
                Data = new List<T>();
            }
            Data.Add(item);
        }

        public void Clear()
        {
            Data.Clear();
        }

        public bool Contains(T item)
        {
            return Data.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Data.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Data.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Data.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return Data.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Data.RemoveAt(index);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }




        public String GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public String GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }


        public PropertyDescriptorCollection GetProperties()
        {
            var pds = new PropertyDescriptorCollection(null);
            for (int i = 0; i < Data.Count; i++)
            {
                var pd = new ListBasePropertyDescriptor(this, i);
                pds.Add(pd);
            }
            return pds;
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }


        public class ListBasePropertyDescriptor : PropertyDescriptor
        {
            private ListBase<T> collection = null;
            private int index = -1;

            public ListBasePropertyDescriptor(ListBase<T> coll, int i) : base("#" + i.ToString(), null)
            {
                collection = coll;
                index = i;
            }

            public override AttributeCollection Attributes
            {
                get
                {
                    return new AttributeCollection(null);
                }
            }

            public override bool CanResetValue(object component)
            {
                return true;
            }

            public override Type ComponentType
            {
                get
                {
                    return this.collection.GetType();
                }
            }

            public override string DisplayName
            {
                get
                {
                    return "[" + index.ToString() + "]";
                }
            }

            public override string Description
            {
                get
                {
                    return collection[index].ToString();
                }
            }

            public override object GetValue(object component)
            {
                return this.collection[index];
            }

            public override bool IsReadOnly
            {
                get { return true; }
            }

            public override string Name
            {
                get { return "#" + index.ToString(); }
            }

            public override Type PropertyType
            {
                get { return this.collection[index].GetType(); }
            }

            public override void ResetValue(object component) { }

            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }

            public override void SetValue(object component, object value)
            {
                // this.collection[index] = value;
            }
        }

    }




    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourceSimpleArray<T> : ListBase<T>, IResourceNoCacheBlock where T : IResourceSystemBlock, new()
    {
        /// <summary>
        /// Gets the length of the data block.
        /// </summary>
        public override long BlockLength
        {
            get
            {
                long length = 0;
                if (Data != null)
                {
                    foreach (var x in Data)
                        length += x.BlockLength;
                }
                return length;
            }
        }


        public ResourceSimpleArray()
        {
            //Data = new List<T>();
        }







        /// <summary>
        /// Reads the data block.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            int numElements = Convert.ToInt32(parameters[0]);

            Data = new List<T>(numElements);
            for (int i = 0; i < numElements; i++)
            {
                T item = reader.ReadBlock<T>();
                Data.Add(item);
            }
        }

        /// <summary>
        /// Writes the data block.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            foreach (var f in Data)
                f.Write(writer);
        }




        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            var list = new List<Tuple<long, IResourceBlock>>();

            long length = 0;

            if (Data != null)
            {
                foreach (var x in Data)
                {
                    list.Add(new Tuple<long, IResourceBlock>(length, x));
                    length += x.BlockLength;
                }
            }
            

            return list.ToArray();
        }




        public override string ToString()
        {
            return "(Count: " + Count.ToString() + ")";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourceSimpleList64<T> : ResourceSystemBlock, IResourceNoCacheBlock where T : IResourceSystemBlock, new()
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong EntriesPointer { get; private set; }
        public ushort EntriesCount { get; private set; }
        public ushort EntriesCapacity { get; private set; }

        // reference data
        //public ResourceSimpleArray<T> Entries;
        public T[] data_items { get; set; }

        private ResourceSimpleArray<T> data_block;//used for saving.


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.EntriesPointer = reader.ReadUInt64();
            this.EntriesCount = reader.ReadUInt16();
            this.EntriesCapacity = reader.ReadUInt16();
            reader.Position += 4;

            // read reference data
            //this.Entries = reader.ReadBlockAt<ResourceSimpleArray<T>>(
            //    this.EntriesPointer, // offset
            //    this.EntriesCount
            //);

            //TODO: NEEDS TO BE TESTED!!!
            data_items = new T[EntriesCount];
            var posbckp = reader.Position;
            if (EntriesCount > 0)
            {
                reader.Position = (long)EntriesPointer;
                for (int i = 0; i < EntriesCount; i++)
                {
                    data_items[i] = reader.ReadBlock<T>();
                }
            }

            reader.Position = posbckp;

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data //TODO: fix
            this.EntriesPointer = (ulong)(this.data_block != null ? this.data_block.FilePosition : 0);
            this.EntriesCount = (ushort)(this.data_block != null ? this.data_block.Count : 0);
            this.EntriesCapacity = (ushort)(this.data_block != null ? this.data_block.Count : 0);

            // write structure data
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.EntriesCapacity);
            writer.Write((uint)0x00000000);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if (data_items?.Length > 0)
            {
                data_block = new ResourceSimpleArray<T>();
                data_block.Data = new List<T>();
                data_block.Data.AddRange(data_items);
                list.Add(data_block);
            }
            else
            {
                data_block = null;
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return "(Count: " + EntriesCount.ToString() + ")";
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourceSimpleList64_s<T> : ResourceSystemBlock, IResourceNoCacheBlock where T : struct
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong EntriesPointer { get; private set; }
        public ushort EntriesCount { get; private set; }
        public ushort EntriesCapacity { get; private set; }

        // reference data
        public T[] data_items { get; set; }

        private ResourceSystemStructBlock<T> data_block;//used for saving.


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.EntriesPointer = reader.ReadUInt64();
            this.EntriesCount = reader.ReadUInt16();
            this.EntriesCapacity = reader.ReadUInt16();
            reader.Position += 4;

            // read reference data

            //TODO: NEEDS TO BE TESTED!!!
            data_items = reader.ReadStructsAt<T>(EntriesPointer, EntriesCount);

            if (EntriesCount != EntriesCapacity)
            { }
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data //TODO: fix
            this.EntriesPointer = (ulong)(this.data_block != null ? this.data_block.FilePosition : 0);
            this.EntriesCount = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);
            this.EntriesCapacity = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);

            // write structure data
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.EntriesCapacity);
            writer.Write((uint)0x00000000);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if (data_items?.Length > 0)
            {
                data_block = new ResourceSystemStructBlock<T>(data_items);

                list.Add(data_block);
            }
            else
            {
                data_block = null;
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return "(Count: " + EntriesCount.ToString() + ")";
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourceSimpleList64b_s<T> : ResourceSystemBlock, IResourceNoCacheBlock where T : struct
    {
        //this version uses uints for the count/cap!

        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong EntriesPointer { get; private set; }
        public uint EntriesCount { get; set; } //this needs to be set manually for this type! make sure it's <= capacity
        public uint EntriesCapacity { get; private set; }

        // reference data
        public T[] data_items { get; set; }

        private ResourceSystemStructBlock<T> data_block;//used for saving.


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.EntriesPointer = reader.ReadUInt64();
            this.EntriesCount = reader.ReadUInt32();
            this.EntriesCapacity = reader.ReadUInt32();
            //reader.Position += 4;

            // read reference data

            //TODO: NEEDS TO BE TESTED!!!
            data_items = reader.ReadStructsAt<T>(EntriesPointer, EntriesCapacity);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data //TODO: fix
            this.EntriesPointer = (ulong)(this.data_block != null ? this.data_block.FilePosition : 0);
            //this.EntriesCount = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);
            this.EntriesCapacity = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);

            // write structure data
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.EntriesCapacity);
            //writer.Write((uint)0x00000000);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if (data_items?.Length > 0)
            {
                data_block = new ResourceSystemStructBlock<T>(data_items);

                list.Add(data_block);
            }
            else
            {
                data_block = null;
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return "(Count: " + EntriesCount.ToString() + ")";
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourceSimpleList64_byte : ResourceSystemBlock, IResourceNoCacheBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong EntriesPointer { get; private set; }
        public ushort EntriesCount { get; private set; }
        public ushort EntriesCapacity { get; private set; }

        // reference data
        public byte[] data_items { get; private set; }

        private ResourceSystemStructBlock<byte> data_block;//used for saving.


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.EntriesPointer = reader.ReadUInt64();
            this.EntriesCount = reader.ReadUInt16();
            this.EntriesCapacity = reader.ReadUInt16();
            reader.Position += 4;

            // read reference data

            //TODO: NEEDS TO BE TESTED!!!
            data_items = reader.ReadBytesAt(EntriesPointer, EntriesCount);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data //TODO: fix
            this.EntriesPointer = (ulong)(this.data_block != null ? this.data_block.FilePosition : 0);
            this.EntriesCount = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);
            this.EntriesCapacity = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);

            // write structure data
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.EntriesCapacity);
            writer.Write((uint)0x00000000);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if (data_items?.Length > 0)
            {
                data_block = new ResourceSystemStructBlock<byte>(data_items);

                list.Add(data_block);
            }
            else
            {
                data_block = null;
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return "(Count: " + EntriesCount.ToString() + ")";
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourceSimpleList64_ushort : ResourceSystemBlock, IResourceNoCacheBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong EntriesPointer { get; private set; }
        public ushort EntriesCount { get; private set; }
        public ushort EntriesCapacity { get; private set; }

        // reference data
        public ushort[] data_items { get; set; }

        private ResourceSystemStructBlock<ushort> data_block;//used for saving.


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.EntriesPointer = reader.ReadUInt64();
            this.EntriesCount = reader.ReadUInt16();
            this.EntriesCapacity = reader.ReadUInt16();
            reader.Position += 4;

            // read reference data

            //TODO: NEEDS TO BE TESTED!!!
            data_items = reader.ReadUshortsAt(EntriesPointer, EntriesCount);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data //TODO: fix
            this.EntriesPointer = (ulong)(this.data_block != null ? this.data_block.FilePosition : 0);
            this.EntriesCount = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);
            this.EntriesCapacity = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);

            // write structure data
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.EntriesCapacity);
            writer.Write((uint)0x00000000);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if (data_items?.Length > 0)
            {
                data_block = new ResourceSystemStructBlock<ushort>(data_items);

                list.Add(data_block);
            }
            else
            {
                data_block = null;
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return "(Count: " + EntriesCount.ToString() + ")";
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourceSimpleList64_uint : ResourceSystemBlock, IResourceNoCacheBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong EntriesPointer { get; private set; }
        public ushort EntriesCount { get; private set; }
        public ushort EntriesCapacity { get; private set; }

        // reference data
        public uint[] data_items { get; set; }

        private ResourceSystemStructBlock<uint> data_block;//used for saving.


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.EntriesPointer = reader.ReadUInt64();
            this.EntriesCount = reader.ReadUInt16();
            this.EntriesCapacity = reader.ReadUInt16();
            reader.Position += 4;

            // read reference data

            //TODO: NEEDS TO BE TESTED!!!
            data_items = reader.ReadUintsAt(EntriesPointer, EntriesCount);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data //TODO: fix
            this.EntriesPointer = (ulong)(this.data_block != null ? this.data_block.FilePosition : 0);
            this.EntriesCount = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);
            this.EntriesCapacity = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);

            // write structure data
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.EntriesCapacity);
            writer.Write((uint)0x00000000);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if (data_items?.Length > 0)
            {
                data_block = new ResourceSystemStructBlock<uint>(data_items);

                list.Add(data_block);
            }
            else
            {
                data_block = null;
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return "(Count: " + EntriesCount.ToString() + ")";
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourceSimpleList64_ulong : ResourceSystemBlock, IResourceNoCacheBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong EntriesPointer { get; private set; }
        public ushort EntriesCount { get; private set; }
        public ushort EntriesCapacity { get; private set; }

        // reference data
        public ulong[] data_items { get; private set; }

        private ResourceSystemStructBlock<ulong> data_block;//used for saving.


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.EntriesPointer = reader.ReadUInt64();
            this.EntriesCount = reader.ReadUInt16();
            this.EntriesCapacity = reader.ReadUInt16();
            reader.Position += 4;

            // read reference data

            //TODO: NEEDS TO BE TESTED!!!
            data_items = reader.ReadUlongsAt(EntriesPointer, EntriesCount);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data //TODO: fix
            this.EntriesPointer = (ulong)(this.data_block != null ? this.data_block.FilePosition : 0);
            this.EntriesCount = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);
            this.EntriesCapacity = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);

            // write structure data
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.EntriesCapacity);
            writer.Write((uint)0x00000000);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if (data_items?.Length > 0)
            {
                data_block = new ResourceSystemStructBlock<ulong>(data_items);

                list.Add(data_block);
            }
            else
            {
                data_block = null;
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return "(Count: " + EntriesCount.ToString() + ")";
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourceSimpleList64_float : ResourceSystemBlock, IResourceNoCacheBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong EntriesPointer { get; private set; }
        public ushort EntriesCount { get; private set; }
        public ushort EntriesCapacity { get; private set; }

        // reference data
        public float[] data_items { get; set; }

        private ResourceSystemStructBlock<float> data_block;//used for saving.


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.EntriesPointer = reader.ReadUInt64();
            this.EntriesCount = reader.ReadUInt16();
            this.EntriesCapacity = reader.ReadUInt16();
            reader.Position += 4;

            // read reference data

            //TODO: NEEDS TO BE TESTED!!!
            data_items = reader.ReadFloatsAt(EntriesPointer, EntriesCount);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data //TODO: fix
            this.EntriesPointer = (ulong)(this.data_block != null ? this.data_block.FilePosition : 0);
            this.EntriesCount = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);
            this.EntriesCapacity = (ushort)(this.data_block != null ? this.data_block.ItemCount : 0);

            // write structure data
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.EntriesCapacity);
            writer.Write((uint)0x00000000);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if (data_items?.Length > 0)
            {
                data_block = new ResourceSystemStructBlock<float>(data_items);

                list.Add(data_block);
            }
            else
            {
                data_block = null;
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return "(Count: " + EntriesCount.ToString() + ")";
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourcePointerArray64<T> : ResourceSystemBlock, IList<T> where T : IResourceSystemBlock, new()
    {

        public int GetNonEmptyNumber()
        {
            int i = 0;
            foreach (var q in data_items)
                if (q != null)
                    i++;
            return i;
        }

        public override long BlockLength
        {
            get
            {
                return (data_items != null) ? 8 * data_items.Length : 0;
            }
        }


        public ulong[] data_pointers { get; set; }
        public T[] data_items { get; set; }

        public bool ManualReferenceOverride = false;//use this if the items are embedded in something else


        public ResourcePointerArray64()
        {
        }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            int numElements = Convert.ToInt32(parameters[0]);


            data_pointers = reader.ReadUlongsAt((ulong)reader.Position, (uint)numElements, false);


            data_items = new T[numElements];
            for (int i = 0; i < numElements; i++)
            {
                data_items[i] = reader.ReadBlockAt<T>(data_pointers[i]);
            }


        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update...
            var list = new List<ulong>();
            foreach (var x in data_items)
            {
                if (x != null)
                {
                    list.Add((uint)x.FilePosition);
                }
                else
                {
                    list.Add(0);
                }
            }
            data_pointers = list.ToArray();


            // write...
            foreach (var x in data_pointers)
                writer.Write(x);
        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if (ManualReferenceOverride == false)
            {
                foreach (var x in data_items)
                {
                    list.Add(x);
                }
            }

            return list.ToArray();
        }





        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            //data_items.RemoveAt(index);
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                return data_items[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(T item)
        {
            //data_items.Add(item);
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            //get { return data_items.Count; }
            get { return (data_items != null) ? data_items.Length : 0; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            //return data_items.Remove(item);
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            //return data_items.GetEnumerator();
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }




        public override string ToString()
        {
            return "(Count: " + ((data_items != null) ? data_items.Length : 0).ToString() + ")";
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourcePointerList64<T> : ResourceSystemBlock, IList<T> where T : IResourceSystemBlock, new()
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong EntriesPointer { get; private set; }
        public ushort EntriesCount { get; set; }
        public ushort EntriesCapacity { get; set; }

        // reference data
        //public ResourcePointerArray64<T> Entries;

        public ulong[] data_pointers { get; private set; }
        public T[] data_items { get; set; }

        public bool ManualCountOverride = false; //use this to manually specify the count
        public bool ManualReferenceOverride = false; //use this if the items are embedded in something else

        private ResourcePointerArray64<T> data_block;//used for saving.


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            this.EntriesPointer = reader.ReadUInt64();
            this.EntriesCount = reader.ReadUInt16();
            this.EntriesCapacity = reader.ReadUInt16();
            reader.Position += 4;

            //this.Entries = reader.ReadBlockAt<ResourcePointerArray64<T>>(
            //    this.EntriesPointer, // offset
            //    this.EntriesCount
            //);

            data_pointers = reader.ReadUlongsAt(EntriesPointer, EntriesCapacity);
            data_items = new T[EntriesCount];
            for (int i = 0; i < EntriesCount; i++)
            {
                data_items[i] = reader.ReadBlockAt<T>(data_pointers[i]);
            }


        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update...
            this.EntriesPointer = (ulong)(this.data_block != null ? this.data_block.FilePosition : 0);
            if (ManualCountOverride == false)
            {
                this.EntriesCapacity = (ushort)(this.data_block != null ? this.data_block.Count : 0);
                this.EntriesCount = (ushort)(this.data_block != null ? this.data_block.Count : 0);
            }


            // write...
            writer.Write(EntriesPointer);
            writer.Write(EntriesCount);
            writer.Write(EntriesCapacity);
            writer.Write((uint)0x0000000);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();

            if (data_items?.Length > 0)
            {
                data_block = new ResourcePointerArray64<T>();
                data_block.data_items = data_items;
                data_block.ManualReferenceOverride = ManualReferenceOverride;
                list.Add(data_block);
            }
            else
            {
                data_block = null;
            }

            return list.ToArray();
        }




        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                return data_items[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return EntriesCount; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }


        public IResourceBlock CheckForCast(ResourceDataReader reader, params object[] parameters)
        {
            throw new NotImplementedException();
        }


        public override string ToString()
        {
            return "(Count: " + EntriesCount.ToString() + ")";
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public struct ResourcePointerListHeader
    {
        public ulong Pointer { get; set; }
        public ushort Count { get; set; }
        public ushort Capacity { get; set; }
        public uint Unknown { get; set; }
    }










    public class ResourceSystemDataBlock : ResourceSystemBlock //used for writing resources.
    {
        public byte[] Data { get; set; }
        public int DataLength { get; set; }

        public override long BlockLength
        {
            get
            {
                return (Data != null) ? Data.Length : DataLength;
            }
        }


        public ResourceSystemDataBlock(byte[] data)
        {
            Data = data;
            DataLength = (Data != null) ? Data.Length : 0;
        }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Data = reader.ReadBytes(DataLength);
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            writer.Write(Data);
        }
    }

    public class ResourceSystemStructBlock<T> : ResourceSystemBlock where T : struct //used for writing resources.
    {
        public T[] Items { get; set; }
        public int ItemCount { get; set; }
        public int StructureSize { get; set; }

        public override long BlockLength
        {
            get
            {
                return ((Items != null) ? Items.Length : ItemCount) * StructureSize;
            }
        }

        public ResourceSystemStructBlock(T[] items)
        {
            Items = items;
            ItemCount = (Items != null) ? Items.Length : 0;
            StructureSize = Marshal.SizeOf(typeof(T));
        }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            int datalength = ItemCount * StructureSize;
            byte[] data = reader.ReadBytes(datalength);
            Items = MetaTypes.ConvertDataArray<T>(data, 0, ItemCount).ToArray();
        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {

            byte[] data = MetaTypes.ConvertArrayToBytes(Items);
            if (data != null)
            {
                writer.Write(data);
            }
        }
    }



}
