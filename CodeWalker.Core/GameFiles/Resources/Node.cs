/*
    Copyright(c) 2016 Neodymium

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

//mangled to fit


using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))] public class NodeDictionary : ResourceFileBase, IMetaXmlItem
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        public ulong NodesPointer { get; set; }
        public uint NodesCount { get; set; }
        public uint NodesCountVehicle { get; set; }
        public uint NodesCountPed { get; set; }
        public uint Unk24 { get; set; } // 0x00000000
        public ulong LinksPtr { get; set; }
        public uint LinksCount { get; set; }
        public uint Unk34 { get; set; } // 0x00000000
        public ulong JunctionsPtr { get; set; }
        public ulong JunctionHeightmapBytesPtr { get; set; }
        public uint Unk48 { get; set; } = 1; // 0x00000001
        public uint Unk4C { get; set; } // 0x00000000
        public ulong JunctionRefsPtr { get; set; }
        public ushort JunctionRefsCount0 { get; set; }
        public ushort JunctionRefsCount1 { get; set; } // same as JunctionRefsCount0
        public uint Unk5C { get; set; } // 0x00000000
        public uint JunctionsCount { get; set; } // same as JunctionRefsCount0
        public uint JunctionHeightmapBytesCount { get; set; }
        public uint Unk68 { get; set; } // 0x00000000
        public uint Unk6C { get; set; } // 0x00000000

        public Node[] Nodes { get; set; }
        public NodeLink[] Links { get; set; }
        public NodeJunction[] Junctions { get; set; }
        public byte[] JunctionHeightmapBytes { get; set; }
        public NodeJunctionRef[] JunctionRefs { get; set; }


        private ResourceSystemStructBlock<Node> NodesBlock = null;
        private ResourceSystemStructBlock<NodeLink> LinksBlock = null;
        private ResourceSystemStructBlock<NodeJunction> JunctionsBlock = null;
        private ResourceSystemStructBlock<byte> JunctionHeightmapBytesBlock = null;
        private ResourceSystemStructBlock<NodeJunctionRef> JunctionRefsBlock = null;



        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            this.NodesPointer = reader.ReadUInt64();
            this.NodesCount = reader.ReadUInt32();
            this.NodesCountVehicle = reader.ReadUInt32();
            this.NodesCountPed = reader.ReadUInt32();
            this.Unk24 = reader.ReadUInt32();
            this.LinksPtr = reader.ReadUInt64();
            this.LinksCount = reader.ReadUInt32();
            this.Unk34 = reader.ReadUInt32();
            this.JunctionsPtr = reader.ReadUInt64();
            this.JunctionHeightmapBytesPtr = reader.ReadUInt64();
            this.Unk48 = reader.ReadUInt32();
            this.Unk4C = reader.ReadUInt32();
            this.JunctionRefsPtr = reader.ReadUInt64();
            this.JunctionRefsCount0 = reader.ReadUInt16();
            this.JunctionRefsCount1 = reader.ReadUInt16();
            this.Unk5C = reader.ReadUInt32();
            this.JunctionsCount = reader.ReadUInt32();
            this.JunctionHeightmapBytesCount = reader.ReadUInt32();
            this.Unk68 = reader.ReadUInt32();
            this.Unk6C = reader.ReadUInt32();

            this.Nodes = reader.ReadStructsAt<Node>(this.NodesPointer, this.NodesCount);
            this.Links = reader.ReadStructsAt<NodeLink>(this.LinksPtr, this.LinksCount);
            this.Junctions = reader.ReadStructsAt<NodeJunction>(this.JunctionsPtr, this.JunctionsCount);
            this.JunctionHeightmapBytes = reader.ReadBytesAt(this.JunctionHeightmapBytesPtr, this.JunctionHeightmapBytesCount);
            this.JunctionRefs = reader.ReadStructsAt<NodeJunctionRef>(this.JunctionRefsPtr, this.JunctionRefsCount1);



        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            NodesPointer = (ulong)(NodesBlock?.FilePosition ?? 0);
            NodesCount = (uint)(Nodes?.Length ?? 0); //assume NodesCountVehicle and Ped already updated..
            LinksPtr = (ulong)(LinksBlock?.FilePosition ?? 0);
            LinksCount = (uint)(Links?.Length ?? 0);
            JunctionsPtr = (ulong)(JunctionsBlock?.FilePosition ?? 0);
            JunctionHeightmapBytesPtr = (ulong)(JunctionHeightmapBytesBlock?.FilePosition ?? 0);
            JunctionRefsPtr = (ulong)(JunctionRefsBlock?.FilePosition ?? 0);
            JunctionRefsCount0 = (ushort)(JunctionRefs?.Length ?? 0);
            JunctionRefsCount1 = JunctionRefsCount1;
            JunctionsCount = (uint)(Junctions?.Length ?? 0);
            JunctionHeightmapBytesCount = (uint)(JunctionHeightmapBytes?.Length ?? 0);


            // write structure data
            writer.Write(this.NodesPointer);
            writer.Write(this.NodesCount);
            writer.Write(this.NodesCountVehicle);
            writer.Write(this.NodesCountPed);
            writer.Write(this.Unk24);
            writer.Write(this.LinksPtr);
            writer.Write(this.LinksCount);
            writer.Write(this.Unk34);
            writer.Write(this.JunctionsPtr);
            writer.Write(this.JunctionHeightmapBytesPtr);
            writer.Write(this.Unk48);
            writer.Write(this.Unk4C);
            writer.Write(this.JunctionRefsPtr);
            writer.Write(this.JunctionRefsCount0);
            writer.Write(this.JunctionRefsCount1);
            writer.Write(this.Unk5C);
            writer.Write(this.JunctionsCount);
            writer.Write(this.JunctionHeightmapBytesCount);
            writer.Write(this.Unk68);
            writer.Write(this.Unk6C);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());

            if ((JunctionRefs != null) && (JunctionRefs.Length > 0))
            {
                JunctionRefsBlock = new ResourceSystemStructBlock<NodeJunctionRef>(JunctionRefs);
                list.Add(JunctionRefsBlock);
            }
            if ((JunctionHeightmapBytes != null) && (JunctionHeightmapBytes.Length > 0))
            {
                JunctionHeightmapBytesBlock = new ResourceSystemStructBlock<byte>(JunctionHeightmapBytes);
                list.Add(JunctionHeightmapBytesBlock);
            }
            if ((Junctions != null) && (Junctions.Length > 0))
            {
                JunctionsBlock = new ResourceSystemStructBlock<NodeJunction>(Junctions);
                list.Add(JunctionsBlock);
            }
            if ((Links != null) && (Links.Length > 0))
            {
                LinksBlock = new ResourceSystemStructBlock<NodeLink>(Links);
                list.Add(LinksBlock);
            }
            if ((Nodes != null) && (Nodes.Length > 0))
            {
                NodesBlock = new ResourceSystemStructBlock<Node>(Nodes);
                list.Add(NodesBlock);
            }


            return list.ToArray();
        }




        public void WriteXml(StringBuilder sb, int indent)
        {
            YndXml.ValueTag(sb, indent, "VehicleNodeCount", NodesCountVehicle.ToString());
            YndXml.ValueTag(sb, indent, "PedNodeCount", NodesCountPed.ToString());

            XmlNodeWrapper[] nodes = null;
            int nodecount = Nodes?.Length ?? 0;
            if (nodecount > 0)
            {
                nodes = new XmlNodeWrapper[nodecount];
                for (int i = 0; i < nodecount; i++)
                {
                    nodes[i] = new XmlNodeWrapper(Nodes[i], Links);
                }
            }
            YndXml.WriteItemArray(sb, nodes, indent, "Nodes");


            XmlJunctionWrapper[] juncs = null;
            int junccount = Junctions?.Length ?? 0;
            if (junccount > 0)
            {
                juncs = new XmlJunctionWrapper[junccount];
                for (int i = 0; i < junccount; i++)
                {
                    juncs[i] = new XmlJunctionWrapper(Junctions[i], JunctionHeightmapBytes);
                }
            }
            YndXml.WriteItemArray(sb, juncs, indent, "Junctions");

            YndXml.WriteItemArray(sb, JunctionRefs, indent, "JunctionRefs");

        }
        public void ReadXml(XmlNode node)
        {
            NodesCountVehicle = Xml.GetChildUIntAttribute(node, "VehicleNodeCount", "value");
            NodesCountPed = Xml.GetChildUIntAttribute(node, "PedNodeCount", "value");

            List<Node> nodelist = new List<Node>();
            List<NodeLink> linklist = new List<NodeLink>();
            List<NodeJunction> junclist = new List<NodeJunction>();
            List<byte> jhmblist = new List<byte>();
            List<NodeJunctionRef> jreflist = new List<NodeJunctionRef>();

            var nodesnode = node.SelectSingleNode("Nodes");
            if (nodesnode != null)
            {
                var nodeitems = nodesnode.SelectNodes("Item");
                foreach (XmlNode nodeitem in nodeitems)
                {
                    XmlNodeWrapper n = new XmlNodeWrapper(linklist);
                    n.ReadXml(nodeitem);
                    nodelist.Add(n.Node);
                }
            }

            var juncsnode = node.SelectSingleNode("Junctions");
            if (juncsnode != null)
            {
                var juncitems = juncsnode.SelectNodes("Item");
                foreach (XmlNode juncitem in juncitems)
                {
                    XmlJunctionWrapper j = new XmlJunctionWrapper(jhmblist);
                    j.ReadXml(juncitem);
                    junclist.Add(j.Junction);
                }
            }

            var jrefsnode = node.SelectSingleNode("JunctionRefs");
            if (jrefsnode != null)
            {
                var jrefitems = jrefsnode.SelectNodes("Item");
                foreach (XmlNode jrefitem in jrefitems)
                {
                    NodeJunctionRef jref = new NodeJunctionRef();
                    jref.ReadXml(jrefitem);
                    jreflist.Add(jref);
                }
            }

            NodesCount = (uint)nodelist.Count;
            Nodes = nodelist.ToArray();
            LinksCount = (uint)linklist.Count;
            Links = linklist.ToArray();
            JunctionsCount = (uint)junclist.Count;
            Junctions = junclist.ToArray();
            JunctionHeightmapBytesCount = (uint)jhmblist.Count;
            JunctionHeightmapBytes = jhmblist.ToArray();
            JunctionRefsCount0 = (ushort)jreflist.Count;
            JunctionRefsCount1 = JunctionRefsCount0;
            JunctionRefs = jreflist.ToArray();

        }


        class XmlNodeWrapper : IMetaXmlItem
        {
            public Node Node;
            private NodeLink[] AllLinks;
            private List<NodeLink> AllLinksList;

            public XmlNodeWrapper(Node node, NodeLink[] allLinks)
            {
                Node = node;
                AllLinks = allLinks;
            }
            public XmlNodeWrapper(List<NodeLink> allLinksList)
            {
                AllLinksList = allLinksList;
            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                Node.WriteXml(sb, indent, AllLinks);
            }
            public void ReadXml(XmlNode node)
            {
                Node = new Node();
                Node.ReadXml(node, AllLinksList);
            }
        }
        class XmlJunctionWrapper : IMetaXmlItem
        {
            public NodeJunction Junction;
            private byte[] AllHeightmapData;
            private List<byte> AllHeightmapDataList;

            public XmlJunctionWrapper(NodeJunction junc, byte[] allHeightmapData)
            {
                Junction = junc;
                AllHeightmapData = allHeightmapData;
            }
            public XmlJunctionWrapper(List<byte> allHeightmapDataList)
            {
                AllHeightmapDataList = allHeightmapDataList;
            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                Junction.WriteXml(sb, indent, AllHeightmapData);
            }
            public void ReadXml(XmlNode node)
            {
                Junction = new NodeJunction();
                Junction.ReadXml(node, AllHeightmapDataList);
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct Node
    {
        public uint Unused0 { get; set; } // 0x00000000
        public uint Unused1 { get; set; } // 0x00000000
        public uint Unused2 { get; set; } // 0x00000000
        public uint Unused3 { get; set; } // 0x00000000
        public ushort AreaID { get; set; }
        public ushort NodeID { get; set; }
        public TextHash StreetName { get; set; }
        public ushort Unused4 { get; set; }
        public ushort LinkID { get; set; }
        public short PositionX { get; set; }
        public short PositionY { get; set; }
        public FlagsByte Flags0 { get; set; }
        public FlagsByte Flags1 { get; set; }
        public short PositionZ { get; set; }
        public FlagsByte Flags2 { get; set; }
        public FlagsByte LinkCountFlags { get; set; }
        public FlagsByte Flags3 { get; set; }
        public FlagsByte Flags4 { get; set; }

        public override string ToString()
        {
            //return Unused0.ToString() + ", " + Unused1.ToString() + ", " + Unused2.ToString() + ", " +
            //       Unused3.ToString() + ", " + AreaID.ToString() + ", " + NodeID.ToString() + ", " +
            //       UnknownInterp.ToString() + ", " + HeuristicCost.ToString() + ", " + LinkID.ToString() + ", " +
            //       PositionX.ToString() + ", " + PositionY.ToString() + ", " + Unk20.ToString() + ", " + Unk21.ToString() + ", " + 
            //       Unk22.ToString() + ", " + Unk24.ToString() + ", " + Unk26.ToString();

            return AreaID.ToString() + ", " + NodeID.ToString() + ", " + StreetName.ToString();// + ", X:" +
                                                                                               //PositionX.ToString() + ", Y:" + PositionY.ToString() + ", " + PositionZ.ToString();// + ", " + 
                                                                                               //Flags0.ToString() + ", " + Flags1.ToString() + ", Z:" +
                                                                                               //Flags2.ToString() + ", " + LinkCountFlags.ToString() + ", " + 
                                                                                               //Flags3.ToString() + ", " + Flags4.ToString();

        }

        public void WriteXml(StringBuilder sb, int indent, NodeLink[] allLinks)
        {
            Vector3 p = new Vector3();
            p.X = PositionX / 4.0f;
            p.Y = PositionY / 4.0f;
            p.Z = PositionZ / 32.0f;
            int linkCount = LinkCountFlags.Value >> 3;
            int linkCountUnk = LinkCountFlags.Value & 7;

            YndXml.ValueTag(sb, indent, "AreaID", AreaID.ToString());
            YndXml.ValueTag(sb, indent, "NodeID", NodeID.ToString());
            YndXml.StringTag(sb, indent, "StreetName", YndXml.HashString(StreetName));
            YndXml.SelfClosingTag(sb, indent, "Position " + FloatUtil.GetVector3XmlString(p));
            YndXml.ValueTag(sb, indent, "Flags0", Flags0.Value.ToString());
            YndXml.ValueTag(sb, indent, "Flags1", Flags1.Value.ToString());
            YndXml.ValueTag(sb, indent, "Flags2", Flags2.Value.ToString());
            YndXml.ValueTag(sb, indent, "Flags3", Flags3.Value.ToString());
            YndXml.ValueTag(sb, indent, "Flags4", Flags4.Value.ToString());
            YndXml.ValueTag(sb, indent, "Flags5", linkCountUnk.ToString());

            NodeLink[] links = null;
            if (linkCount > 0)
            {
                links = new NodeLink[linkCount];
                for (int i = 0; i < linkCount; i++)
                {
                    links[i] = allLinks[LinkID + i];
                }
            }
            YndXml.WriteItemArray(sb, links, indent, "Links");

        }
        public void ReadXml(XmlNode node, List<NodeLink> allLinksList)
        {
            AreaID = (ushort)Xml.GetChildUIntAttribute(node, "AreaID", "value");
            NodeID = (ushort)Xml.GetChildUIntAttribute(node, "NodeID", "value");
            StreetName = XmlYnd.GetTextHash(Xml.GetChildInnerText(node, "StreetName"));
            Vector3 p = Xml.GetChildVector3Attributes(node, "Position");
            PositionX = (short)(p.X * 4.0f);
            PositionY = (short)(p.Y * 4.0f);
            PositionZ = (short)(p.Z * 32.0f);
            Flags0 = (byte)Xml.GetChildUIntAttribute(node, "Flags0", "value");
            Flags1 = (byte)Xml.GetChildUIntAttribute(node, "Flags1", "value");
            Flags2 = (byte)Xml.GetChildUIntAttribute(node, "Flags2", "value");
            Flags3 = (byte)Xml.GetChildUIntAttribute(node, "Flags3", "value");
            Flags4 = (byte)Xml.GetChildUIntAttribute(node, "Flags4", "value");
            int linkCountUnk = (byte)Xml.GetChildUIntAttribute(node, "Flags5", "value");

            LinkID = (ushort)allLinksList.Count;
            int linkCount = 0;
            var linksnode = node.SelectSingleNode("Links");
            if (linksnode != null)
            {
                var linkitems = linksnode.SelectNodes("Item");
                foreach (XmlNode linkitem in linkitems)
                {
                    NodeLink link = new NodeLink();
                    link.ReadXml(linkitem);
                    allLinksList.Add(link);
                    linkCount++;
                }
            }
            LinkCountFlags = (byte)((linkCount << 3) + (linkCountUnk & 7));
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NodeLink : IMetaXmlItem
    {
        public ushort AreaID { get; set; }
        public ushort NodeID { get; set; }
        public FlagsByte Flags0 { get; set; }
        public FlagsByte Flags1 { get; set; }
        public FlagsByte Flags2 { get; set; }
        public FlagsByte LinkLength { get; set; }

        public override string ToString()
        {
            return AreaID.ToString() + ", " + NodeID.ToString() + ", " + Flags0.Value.ToString() + ", " + Flags1.Value.ToString() + ", " + Flags2.Value.ToString() + ", " + LinkLength.Value.ToString();
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            YndXml.ValueTag(sb, indent, "ToAreaID", AreaID.ToString());
            YndXml.ValueTag(sb, indent, "ToNodeID", NodeID.ToString());
            YndXml.ValueTag(sb, indent, "Flags0", Flags0.Value.ToString());
            YndXml.ValueTag(sb, indent, "Flags1", Flags1.Value.ToString());
            YndXml.ValueTag(sb, indent, "Flags2", Flags2.Value.ToString());
            YndXml.ValueTag(sb, indent, "LinkLength", LinkLength.Value.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            AreaID = (ushort)Xml.GetChildUIntAttribute(node, "ToAreaID", "value");
            NodeID = (ushort)Xml.GetChildUIntAttribute(node, "ToNodeID", "value");
            Flags0 = (byte)Xml.GetChildUIntAttribute(node, "Flags0", "value");
            Flags1 = (byte)Xml.GetChildUIntAttribute(node, "Flags1", "value");
            Flags2 = (byte)Xml.GetChildUIntAttribute(node, "Flags2", "value");
            LinkLength = (byte)Xml.GetChildUIntAttribute(node, "LinkLength", "value");
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NodeJunction
    {
        public short MaxZ { get; set; }
        public short PositionX { get; set; }
        public short PositionY { get; set; }
        public short MinZ { get; set; }
        public ushort HeightmapPtr { get; set; }
        public byte HeightmapDimX { get; set; }
        public byte HeightmapDimY { get; set; }

        public override string ToString()
        {
            return PositionX.ToString() + ", " + PositionY.ToString() + ": " + MinZ.ToString() + ", " + MaxZ.ToString() + ": " + HeightmapDimX.ToString() + " x " + HeightmapDimY.ToString();
        }

        public void WriteXml(StringBuilder sb, int indent, byte[] allHeightmapData)
        {
            Vector2 p = new Vector2();
            p.X = PositionX / 4.0f;
            p.Y = PositionY / 4.0f;
            float minz = MinZ / 32.0f;
            float maxz = MaxZ / 32.0f;

            YndXml.SelfClosingTag(sb, indent, "Position " + FloatUtil.GetVector2XmlString(p));
            YndXml.ValueTag(sb, indent, "MinZ", FloatUtil.ToString(minz));
            YndXml.ValueTag(sb, indent, "MaxZ", FloatUtil.ToString(maxz));
            YndXml.ValueTag(sb, indent, "SizeX", HeightmapDimX.ToString());
            YndXml.ValueTag(sb, indent, "SizeY", HeightmapDimY.ToString());

            byte[] hmdata = null;
            int hmbcount = HeightmapDimX * HeightmapDimY;
            if (hmbcount > 0)
            {
                hmdata = new byte[hmbcount];
                Buffer.BlockCopy(allHeightmapData, HeightmapPtr, hmdata, 0, hmbcount);
            }
            YndXml.WriteRawArray(sb, hmdata, indent, "Heightmap", "", RelXml.FormatHexByte, Math.Max(HeightmapDimX, (byte)1));

        }
        public void ReadXml(XmlNode node, List<byte> allHeightmapDataList)
        {
            Vector2 p = Xml.GetChildVector2Attributes(node, "Position");
            float minz = Xml.GetChildFloatAttribute(node, "MinZ", "value");
            float maxz = Xml.GetChildFloatAttribute(node, "MaxZ", "value");
            HeightmapDimX = (byte)Xml.GetChildUIntAttribute(node, "SizeX", "value");
            HeightmapDimY = (byte)Xml.GetChildUIntAttribute(node, "SizeY", "value");
            PositionX = (short)(p.X * 4.0f);
            PositionY = (short)(p.Y * 4.0f);
            MinZ = (short)(minz * 32.0f);
            MaxZ = (short)(maxz * 32.0f);

            byte[] hmdata = Xml.GetChildRawByteArray(node, "Heightmap");
            HeightmapPtr = (ushort)allHeightmapDataList.Count;
            if (hmdata != null)
            {
                allHeightmapDataList.AddRange(hmdata);
            }

        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public struct NodeJunctionRef : IMetaXmlItem
    {
        public ushort AreaID { get; set; }
        public ushort NodeID { get; set; }
        public ushort JunctionID { get; set; }
        public ushort Unk0 { get; set; }

        public override string ToString()
        {
            return AreaID.ToString() + ", " + NodeID.ToString() + ", " + JunctionID.ToString();
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            YndXml.ValueTag(sb, indent, "AreaID", AreaID.ToString());
            YndXml.ValueTag(sb, indent, "NodeID", NodeID.ToString());
            YndXml.ValueTag(sb, indent, "JunctionID", JunctionID.ToString());
            YndXml.ValueTag(sb, indent, "Unk0", Unk0.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            AreaID = (ushort)Xml.GetChildUIntAttribute(node, "AreaID", "value");
            NodeID = (ushort)Xml.GetChildUIntAttribute(node, "NodeID", "value");
            JunctionID = (ushort)Xml.GetChildUIntAttribute(node, "JunctionID", "value");
            Unk0 = (ushort)Xml.GetChildUIntAttribute(node, "Unk0", "value");
        }
    }








}