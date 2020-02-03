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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourceFileBase : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public uint FileVFT { get; set; }
        public uint FileUnknown { get; set; } = 1;
        public ulong FilePagesInfoPointer { get; set; }

        // reference data
        public ResourcePagesInfo FilePagesInfo { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.FileVFT = reader.ReadUInt32();
            this.FileUnknown = reader.ReadUInt32();
            this.FilePagesInfoPointer = reader.ReadUInt64();

            // read reference data
            this.FilePagesInfo = reader.ReadBlockAt<ResourcePagesInfo>(
                this.FilePagesInfoPointer // offset
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.FilePagesInfoPointer = (ulong)(this.FilePagesInfo != null ? this.FilePagesInfo.FilePosition : 0);

            // write structure data
            writer.Write(this.FileVFT);
            writer.Write(this.FileUnknown);
            writer.Write(this.FilePagesInfoPointer);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (FilePagesInfo != null) list.Add(FilePagesInfo);
            return list.ToArray();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ResourcePagesInfo : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 20 + (256 * 16); }
        }

        // structure data
        public uint Unknown_0h { get; set; }
        public uint Unknown_4h { get; set; }
        public byte SystemPagesCount { get; set; }
        public byte GraphicsPagesCount { get; set; }
        public ushort Unknown_Ah { get; set; }
        public uint Unknown_Ch { get; set; }
        public uint Unknown_10h { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.SystemPagesCount = reader.ReadByte();
            this.GraphicsPagesCount = reader.ReadByte();
            this.Unknown_Ah = reader.ReadUInt16();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_4h);
            writer.Write(this.SystemPagesCount);
            writer.Write(this.GraphicsPagesCount);
            writer.Write(this.Unknown_Ah);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);

            var pad = 256 * 16;
            writer.Write(new byte[pad]);
        }

        public override string ToString()
        {
            return SystemPagesCount.ToString() + ", " + GraphicsPagesCount.ToString();
        }
    }


}
