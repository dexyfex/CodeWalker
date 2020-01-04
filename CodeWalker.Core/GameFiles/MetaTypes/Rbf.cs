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
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{


    [TypeConverter(typeof(ExpandableObjectConverter))] public class RbfFile
    {
        private const int RBF_IDENT = 0x30464252;

        public RbfStructure current { get; set; }
        public Stack<RbfStructure> stack { get; set; }
        public List<RbfEntryDescription> descriptors { get; set; }
        public Dictionary<string, int> outDescriptors { get; private set; } = new Dictionary<string, int>();


        public void Load(byte[] data)
        {
            using (var ms = new MemoryStream(data))
                Load(ms);
        }

        public RbfStructure Load(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                return Load(fileStream);
            }
        }

        public RbfStructure Load(Stream stream)
        {
            stack = new Stack<RbfStructure>();
            descriptors = new List<RbfEntryDescription>();

            var reader = new DataReader(stream);
            var ident = reader.ReadInt32();
            if (ident != RBF_IDENT)
                throw new Exception("The file identifier does not match.");

            while (reader.Position < reader.Length)
            {
                var descriptorIndex = reader.ReadByte();
                if (descriptorIndex == 0xFF) // close tag
                {
                    var b = reader.ReadByte();
                    if (b != 0xFF)
                        throw new Exception("Expected 0xFF but was " + b.ToString("X2"));

                    if (stack.Count > 0)
                    {
                        current = stack.Pop();
                    }
                    else
                    {
                        if (reader.Position != reader.Length)
                            throw new Exception("Expected end of stream but was not.");
                        return current;
                    }
                }
                else if (descriptorIndex == 0xFD) // bytes
                {
                    var b = reader.ReadByte();
                    if (b != 0xFF)
                        throw new Exception("Expected 0xFF but was " + b.ToString("X2"));

                    var dataLength = reader.ReadInt32();
                    var data = reader.ReadBytes(dataLength);

                    var bytesValue = new RbfBytes();
                    bytesValue.Value = data;
                    current.Children.Add(bytesValue);
                }
                else
                {
                    var dataType = reader.ReadByte();
                    if (descriptorIndex == descriptors.Count) // new descriptor + data
                    {
                        var nameLength = reader.ReadInt16();
                        var nameBytes = reader.ReadBytes(nameLength);
                        var name = Encoding.ASCII.GetString(nameBytes);

                        var descriptor = new RbfEntryDescription();
                        descriptor.Name = name;
                        descriptor.Type = dataType;
                        descriptors.Add(descriptor);

                        ParseElement(reader, descriptors.Count - 1, dataType);
                    }
                    else // existing descriptor + data
                    {
                        if (dataType != descriptors[descriptorIndex].Type)
                        {
                            //throw new Exception("Data type does not match. Expected "
                            //    + descriptors[descriptorIndex].Type.ToString() + " but found "
                            //    + dataType.ToString() + ". Descriptor: " + descriptors[descriptorIndex].Name);
                        }

                        ParseElement(reader, descriptorIndex, dataType);
                    }
                }
            }

            throw new Exception("Unexpected end of stream.");
        }

        private void ParseElement(DataReader reader, int descriptorIndex, byte dataType)
        {
            var descriptor = descriptors[descriptorIndex];
            switch (dataType)
            {
                case 0: // open element...
                    {
                        var structureValue = new RbfStructure();
                        structureValue.Name = descriptor.Name;

                        if (current != null)
                        {
                            current.AddChild(structureValue);
                            stack.Push(current);
                        }

                        current = structureValue;

                        var x1 = reader.ReadInt16();
                        var x2 = reader.ReadInt16();
                        current.PendingAttributes = reader.ReadInt16();
                        break;
                    }
                case 0x10:
                    {
                        var intValue = new RbfUint32();
                        intValue.Name = descriptor.Name;
                        intValue.Value = reader.ReadUInt32();
                        current.AddChild(intValue);
                        break;
                    }
                case 0x20:
                    {
                        var booleanValue = new RbfBoolean();
                        booleanValue.Name = descriptor.Name;
                        booleanValue.Value = true;
                        current.AddChild(booleanValue);
                        break;
                    }
                case 0x30:
                    {
                        var booleanValue = new RbfBoolean();
                        booleanValue.Name = descriptor.Name;
                        booleanValue.Value = false;
                        current.AddChild(booleanValue);
                        break;
                    }
                case 0x40:
                    {
                        var floatValue = new RbfFloat();
                        floatValue.Name = descriptor.Name;
                        floatValue.Value = reader.ReadSingle();
                        current.AddChild(floatValue);
                        break;
                    }
                case 0x50:
                    {
                        var floatVectorValue = new RbfFloat3();
                        floatVectorValue.Name = descriptor.Name;
                        floatVectorValue.X = reader.ReadSingle();
                        floatVectorValue.Y = reader.ReadSingle();
                        floatVectorValue.Z = reader.ReadSingle();
                        current.AddChild(floatVectorValue);
                        break;
                    }
                case 0x60:
                    {
                        var valueLength = reader.ReadInt16();
                        var valueBytes = reader.ReadBytes(valueLength);
                        var value = Encoding.ASCII.GetString(valueBytes);
                        var stringValue = new RbfString();
                        stringValue.Name = descriptor.Name;
                        stringValue.Value = value;
                        current.AddChild(stringValue);
                        break;
                    }
                default:
                    throw new Exception("Unsupported data type.");
            }
        }

        public static bool IsRBF(Stream stream)
        {
            var reader = new DataReader(stream);
            var origpos = stream.Position;
            var ident = reader.ReadInt32();
            var isrbf = (ident == RBF_IDENT);
            stream.Position = origpos;
            return isrbf;
        }


        public byte GetDescriptorIndex(IRbfType t, out bool isNew)
        {
            var key = t.Name;// $"{t.Name}_{t.DataType}";
            isNew = false;

            if (!outDescriptors.TryGetValue(key, out var idx))
            {
                idx = outDescriptors.Count;
                outDescriptors.Add(key, idx);

                isNew = true;
            }

            return (byte)idx;
        }


        public byte[] Save()
        {
            var ms = new MemoryStream();
            Save(ms);

            var buf = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buf, 0, buf.Length);

            return buf;
        }

        public void Save(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                Save(fileStream);
            }
        }

        public void Save(Stream stream)
        {
            outDescriptors = new Dictionary<string, int>();

            var writer = new DataWriter(stream);
            writer.Write(RBF_IDENT);

            current.Save(this, writer);
        }


        public void WriteRecordId(IRbfType type, DataWriter writer)
        {
            writer.Write(GetDescriptorIndex(type, out var isNew));
            writer.Write((byte)type.DataType);

            if (isNew)
            {
                writer.Write((ushort)type.Name.Length);
                writer.Write(Encoding.ASCII.GetBytes(type.Name));
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class RbfEntryDescription
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public override string ToString() { return Name + ": " + Type.ToString(); }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public interface IRbfType
    {
        string Name { get; set; }
        byte DataType { get; }
        void Save(RbfFile file, DataWriter writer);
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class RbfBytes : IRbfType
    {
        public string Name { get; set; }
        public byte[] Value { get; set; }
        public byte DataType => 0;
        public void Save(RbfFile root, DataWriter writer)
        {
            writer.Write((byte)0xFD);
            writer.Write((byte)0xFF);
            writer.Write(Value.Length);
            writer.Write(Value);
        }
        public override string ToString() { return Name + ": " + Value.ToString(); }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class RbfUint32 : IRbfType
    {
        public string Name { get; set; }
        public uint Value { get; set; }
        public byte DataType => 0x10;
        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
            writer.Write(Value);
        }
        public override string ToString() { return Name + ": " + Value.ToString(); }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class RbfBoolean : IRbfType
    {
        public string Name { get; set; }
        public bool Value { get; set; }
        public byte DataType => (byte)((Value) ? 0x20 : 0x30);
        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
        }
        public override string ToString() { return Name + ": " + Value.ToString(); }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class RbfFloat : IRbfType
    {
        public string Name { get; set; }
        public float Value { get; set; }
        public byte DataType => 0x40;
        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
            writer.Write(Value);
        }
        public override string ToString() { return Name + ": " + Value.ToString(); }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class RbfFloat3 : IRbfType
    {
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public byte DataType => 0x50;
        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }
        public override string ToString() { return string.Format("{0}: X:{1}, Y:{2}, Z:{3}", Name, X, Y, Z); }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class RbfString : IRbfType
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public byte DataType => 0x60;
        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
            writer.Write((short)Value.Length);
            writer.Write(Encoding.ASCII.GetBytes(Value));
        }
        public override string ToString() { return Name + ": " + Value.ToString(); }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class RbfStructure : IRbfType
    {
        public string Name { get; set; }
        public List<IRbfType> Children { get; set; } = new List<IRbfType>();
        public List<IRbfType> Attributes { get; set; } = new List<IRbfType>();
        internal int PendingAttributes { get; set; }
        public byte DataType => 0;
        public override string ToString() { return Name + ": {" + Children.Count.ToString() + "}"; }
        public IRbfType FindChild(string name)
        {
            foreach (var child in Children)
            {
                if (child == null) continue;
                if (child.Name == name) return child;
            }
            return null;
        }
        public IRbfType FindAttribute(string name)
        {
            foreach (var attr in Attributes)
            {
                if (attr == null) continue;
                if (attr.Name == name) return attr;
            }
            return null;
        }
        public void Save(RbfFile root, DataWriter writer)
        {
            root.WriteRecordId(this, writer);

            writer.Write(new byte[4]); // 00

            // count of non-primitive fields in this (... attributes??)
            writer.Write((short)Attributes.Count);   //writer.Write((short)Children.TakeWhile(a => !(a is RbfBytes || a is RbfStructure)).Count());

            foreach (var attr in Attributes)
            {
                attr.Save(root, writer);
            }
            foreach (var child in Children)
            {
                child.Save(root, writer);
            }

            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
        }
        internal void AddChild(IRbfType value)
        {
            if (PendingAttributes > 0)
            {
                PendingAttributes--;
                Attributes.Add(value);
            }
            else
            {
                Children.Add(value);
            }
        }
    }


}