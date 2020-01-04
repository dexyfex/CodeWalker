using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
    Copyright(c) 2017 Neodymium
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


//ruthlessly stolen


namespace CodeWalker.GameFiles
{

    [TypeConverter(typeof(ExpandableObjectConverter))] public class FrameFilterDictionary : ResourceFileBase
    {
        // pgDictionaryBase
        // pgDictionary<crFrameFilter>
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000001
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ResourceSimpleList64_uint FilterNameHashes { get; set; }
        public ResourcePointerList64<FrameFilter> Filters { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.FilterNameHashes = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Filters = reader.ReadBlock<ResourcePointerList64<FrameFilter>>();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteBlock(this.FilterNameHashes);
            writer.WriteBlock(this.Filters);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, FilterNameHashes),
                new Tuple<long, IResourceBlock>(0x30, Filters)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class FrameFilter : ResourceSystemBlock
    {
        // crFrameFilter -> this is polymorphic, there are many filters
        public override long BlockLength => 0x40;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000001
        public uint Unknown_Ch { get; set; }
        public uint Unknown_10h { get; set; } // 0x00000004
        public uint Unknown_14h { get; set; } // 0x00000000
        public ResourceSimpleList64_ulong Unknown_18h { get; set; }
        public ResourceSimpleList64_uint Unknown_28h { get; set; }
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadBlock<ResourceSimpleList64_ulong>();
            this.Unknown_28h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.WriteBlock(this.Unknown_18h);
            writer.WriteBlock(this.Unknown_28h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x18, Unknown_18h),
                new Tuple<long, IResourceBlock>(0x28, Unknown_28h)
            };
        }
    }


}
