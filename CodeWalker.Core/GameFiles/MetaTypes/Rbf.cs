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
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CodeWalker.Core.Utils;
using Collections.Pooled;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.ObjectPool;

namespace CodeWalker.GameFiles
{


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RbfFile : IDisposable
    {
        public const uint RBF_IDENT_LITTLE_ENDIAN = 0x30464252;

        public RbfStructure current { get; set; }
        public List<RbfEntryDescription> descriptors { get; set; }
        public Dictionary<string, int> outDescriptors { get; private set; } = new Dictionary<string, int>();


        public RbfStructure Load(byte[] data)
        {
            var sequence = new ReadOnlySequence<byte>(data);
            var reader = new SequenceReader<byte>(sequence);
            return Load(ref reader);
        }

        public RbfStructure Load(string fileName)
        {
            var data = File.ReadAllBytes(fileName);
            return Load(data);
        }

        public RbfStructure Load(ref SequenceReader<byte> reader)
        {
            var stack = new Stack<RbfStructure>();
            descriptors = new List<RbfEntryDescription>();

            //var reader = new DataReader(stream);
            var ident = reader.ReadInt32();
            if (ident != RBF_IDENT_LITTLE_ENDIAN)
            {
                ThrowHelper.ThrowInvalidOperationException("The file identifier does not match.");
                return default;
            }

            while (reader.Consumed < reader.Length)
            {
                var descriptorIndex = reader.ReadByte();
                if (descriptorIndex == 0xFF) // close tag
                {
                    var b = reader.ReadByte();
                    if (b != 0xFF)
                    {
                        ThrowHelper.ThrowInvalidOperationException($"Expected 0xFF but was {b:X2}");
                        return default;
                    }
                        

                    if (stack.Count > 0)
                    {
                        current = stack.Pop();
                    }
                    else
                    {
                        if (reader.Consumed != reader.Length)
                        {
                            ThrowHelper.ThrowInvalidOperationException("Expected end of stream but was not.");
                            return default;
                        }

                        return current;
                    }
                }
                else if (descriptorIndex == 0xFD) // bytes
                {
                    var b = reader.ReadByte();
                    if (b != 0xFF)
                    {
                        ThrowHelper.ThrowInvalidOperationException($"Expected 0xFF but was {b:X2}");
                        return default;
                    }

                    var dataLength = reader.ReadInt32();
                    var data = reader.ReadBytes(dataLength);

                    var bytesValue = new RbfBytes();
                    bytesValue.Value = data.ToArray();
                    current.AddChild(bytesValue);
                }
                else
                {
                    var dataType = reader.ReadByte();
                    if (descriptorIndex == descriptors.Count) // new descriptor + data
                    {
                        var nameLength = reader.ReadInt16();
                        var name = reader.ReadStringLength(nameLength);
                        //var name = Encoding.ASCII.GetString(nameBytes);

                        var descriptor = new RbfEntryDescription();
                        descriptor.Name = name;
                        descriptor.Type = dataType;
                        descriptors.Add(descriptor);

                        ParseElement(ref reader, descriptors.Count - 1, dataType, stack);
                    }
                    else // existing descriptor + data
                    {
                        if (descriptorIndex >= descriptors.Count)
                        {
                            ThrowHelper.ThrowInvalidOperationException("Index out of range");
                            return default;
                        }
                        if (dataType != descriptors[descriptorIndex].Type)
                        {
                            //throw new Exception("Data type does not match. Expected "
                            //    + descriptors[descriptorIndex].Type.ToString() + " but found "
                            //    + dataType.ToString() + ". Descriptor: " + descriptors[descriptorIndex].Name);
                        }

                        ParseElement(ref reader, descriptorIndex, dataType, stack);
                    }
                }
            }

            ThrowHelper.ThrowInvalidOperationException("Unexpected end of stream.");
            return default;
        }

        private void ParseElement(ref SequenceReader<byte> reader, int descriptorIndex, byte dataType, Stack<RbfStructure> stack)
        {
            var descriptor = descriptors[descriptorIndex];
            switch (dataType)
            {
                case 0: // open element...
                    {
                        var structureValue = new RbfStructure();
                        structureValue.Name = descriptor.Name;

                        if (current is not null)
                        {
                            current.AddChildOrAttribute(structureValue);
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
                        current.AddChildOrAttribute(intValue);
                        break;
                    }
                case 0x20:
                    {
                        var booleanValue = new RbfBoolean();
                        booleanValue.Name = descriptor.Name;
                        booleanValue.Value = true;
                        current.AddChildOrAttribute(booleanValue);
                        break;
                    }
                case 0x30:
                    {
                        var booleanValue = new RbfBoolean();
                        booleanValue.Name = descriptor.Name;
                        booleanValue.Value = false;
                        current.AddChildOrAttribute(booleanValue);
                        break;
                    }
                case 0x40:
                    {
                        var floatValue = new RbfFloat();
                        floatValue.Name = descriptor.Name;
                        floatValue.Value = reader.ReadSingle();
                        current.AddChildOrAttribute(floatValue);
                        break;
                    }
                case 0x50:
                    {
                        var floatVectorValue = new RbfFloat3();
                        floatVectorValue.Name = descriptor.Name;
                        floatVectorValue.X = reader.ReadSingle();
                        floatVectorValue.Y = reader.ReadSingle();
                        floatVectorValue.Z = reader.ReadSingle();
                        current.AddChildOrAttribute(floatVectorValue);
                        break;
                    }
                case 0x60:
                    {
                        var valueLength = reader.ReadInt16();
                        var value = reader.ReadStringLength(valueLength);
                        var stringValue = new RbfString();
                        stringValue.Name = descriptor.Name;
                        stringValue.Value = value;
                        current.AddChildOrAttribute(stringValue);
                        break;
                    }
                default:
                    ThrowHelper.ThrowInvalidOperationException("Unsupported data type.");
                    return;
            }
        }

        public static bool IsRBF(Stream stream)
        {
            var origpos = stream.Position;

            Span<byte> buffer = stackalloc byte[4];

            stream.Read(buffer);
            stream.Position = origpos;

            return IsRBF(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRBF(Span<byte> ident)
        {
            return IsRBF(BinaryPrimitives.ReadUInt32LittleEndian(ident));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRBF(uint ident)
        {
            return ident == RBF_IDENT_LITTLE_ENDIAN;
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
            writer.Write(RBF_IDENT_LITTLE_ENDIAN);

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

        public void Dispose()
        {
            current?.Dispose();
            GC.SuppressFinalize(this);
        }

        ~RbfFile()
        {
            Dispose();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RbfEntryDescription
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public override string ToString() => $"{Name}: {Type}";
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IRbfType
    {
        string Name { get; set; }
        byte DataType { get; }
        void Save(RbfFile file, DataWriter writer);
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RbfBytes : IRbfType
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

        public string GetNullTerminatedString()
        {
            var span = Value.AsSpan();
            var index = span.IndexOf((byte)0);
            if (index == -1)
                return Encoding.ASCII.GetString(span);

            return Encoding.ASCII.GetString(span.Slice(0, index));
        }

        public override string ToString() => $"{Name}: {Value}";
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
        public override string ToString() => $"{Name}: {Value}";
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RbfBoolean : IRbfType
    {
        public string Name { get; set; }
        public bool Value { get; set; }
        public byte DataType => (byte)((Value) ? 0x20 : 0x30);
        public void Save(RbfFile file, DataWriter writer)
        {
            file.WriteRecordId(this, writer);
        }
        public override string ToString() => $"{Name}: {Value}";
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
        public override string ToString() => $"{Name}: {Value}";
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RbfFloat3 : IRbfType
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
        public override string ToString() => $"{Name}: X:{X}, Y:{Y}, Z:{Z}";
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RbfString : IRbfType
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
        public override string ToString() => $"{Name}: {Value}";
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RbfStructure : IRbfType, IDisposable
    {
        public string Name { get; set; }

        public PooledList<IRbfType>? Children { get; set; }
        public PooledList<IRbfType>? Attributes { get; set; }
        internal int PendingAttributes { get; set; }
        public byte DataType => 0;
        public override string ToString() => $"{ Name }: {{{ Children?.Count ?? 0 }}}";
        public IRbfType? FindChild(string name)
        {
            if (Children is null || Children.Count == 0)
                return null;

            foreach (var child in Children)
            {
                if (child is null)
                    continue;

                if (child.Name == name)
                    return child;
            }
            return null;
        }
        public IRbfType? FindAttribute(string name)
        {
            if (Attributes is null || Attributes.Count == 0)
                return null;

            foreach (var attr in Attributes)
            {
                if (attr is null)
                    continue;
                if (attr.Name == name)
                    return attr;
            }

            return null;
        }
        public void Save(RbfFile root, DataWriter writer)
        {
            root.WriteRecordId(this, writer);

            writer.Write(new byte[4]); // 00

            // count of non-primitive fields in this (... attributes??)
            writer.Write((short)(Attributes?.Count ?? 0));   //writer.Write((short)Children.TakeWhile(a => !(a is RbfBytes || a is RbfStructure)).Count());

            if (Attributes is not null)
            {
                foreach (var attr in Attributes)
                {
                    attr.Save(root, writer);
                }
            }

            if (Children is not null)
            {
                foreach (var child in Children)
                {
                    child.Save(root, writer);
                }
            }

            writer.Write((byte)0xFF);
            writer.Write((byte)0xFF);
        }

        internal void AddChild(IRbfType value)
        {
            Children ??= PooledListPool<IRbfType>.Shared.Get();
            Children.Add(value);
        }

        internal void AddAttribute(IRbfType value)
        {
            Attributes ??= PooledListPool<IRbfType>.Shared.Get();
            Attributes.Add(value);
        }

        internal void AddChildOrAttribute(IRbfType value)
        {
            if (PendingAttributes > 0)
            {
                PendingAttributes--;
                AddAttribute(value);
            }
            else
            {
                AddChild(value);
            }
        }

        public void Dispose()
        {
            if (Children is PooledList<IRbfType> children)
            {
                PooledListPool<IRbfType>.Shared.Return(children);
            }
            if (Attributes is PooledList<IRbfType> attributes)
            {
                PooledListPool<IRbfType>.Shared.Return(attributes);
            }

            GC.SuppressFinalize(this);
        }

        ~RbfStructure()
        {
            Dispose();
        }
    }


}