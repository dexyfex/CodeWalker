using System;
using System.IO;
using System.Collections.Generic;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Linq;
using System.Text;
using System.Xml;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class MrfFile : GameFile, PackedFile
    {
        public byte[] RawFileData { get; set; }
        public uint Magic { get; set; } = 0x45566F4D; // 'MoVE'
        public uint Version { get; set; } = 2;
        public uint HeaderUnk1 { get; set; } = 0;
        public uint HeaderUnk2 { get; set; } = 0;
        public uint HeaderUnk3 { get; set; } = 0;
        public uint DataLength { get; set; } // doesn't include the header (Magic to Unk1_Items) nor UnkBytes
        public uint UnkBytesCount { get; set; }
        public uint Unk1_Count { get; set; }
        public uint MoveNetworkTriggerCount { get; set; }
        public uint MoveNetworkFlagCount { get; set; }

        public MrfHeaderUnk1[] Unk1_Items { get; set; }
        public MrfMoveNetworkBit[] MoveNetworkTriggers { get; set; }
        public MrfMoveNetworkBit[] MoveNetworkFlags { get; set; }
        public byte[] UnkBytes { get; set; }

        public MrfNode[] AllNodes { get; set; }
        public MrfNodeStateBase RootState { get; set; }

        // DOT graphs to visualize the move network
        public string DebugTreeGraph { get; set; }
        public string DebugStateGraph { get; set; }

        public MrfFile() : base(null, GameFileType.Mrf)
        {
        }

        public MrfFile(RpfFileEntry entry) : base(entry, GameFileType.Mrf)
        {
            RpfFileEntry = entry;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RawFileData = data;
            if (entry != null)
            {
                RpfFileEntry = entry;
                Name = entry.Name;
            }

            using (MemoryStream ms = new MemoryStream(data))
            {
                DataReader r = new DataReader(ms, Endianess.LittleEndian);

                Read(r);
            };
        }

        public byte[] Save()
        {
            MemoryStream s = new MemoryStream();
            DataWriter w = new DataWriter(s);
            NoOpDataWriter nw = new NoOpDataWriter();

            Write(nw, updateOffsets: true); // first pass to calculate relative offsets
            DataLength = (uint)(nw.Length - 32 - (Unk1_Items?.Sum(i => i.Size) ?? 0) - UnkBytesCount);
            Write(w, updateOffsets: false); // now write the MRF

            var buf = new byte[s.Length];
            s.Position = 0;
            s.Read(buf, 0, buf.Length);
            return buf;
        }

        private void Write(DataWriter w, bool updateOffsets)
        {
            if (Magic != 0x45566F4D || Version != 2 || HeaderUnk1 != 0 || HeaderUnk2 != 0)
                throw new Exception("Failed to write MRF, header is invalid!");

            w.Write(Magic);
            w.Write(Version);
            w.Write(HeaderUnk1);
            w.Write(HeaderUnk2);
            w.Write(HeaderUnk3);
            w.Write(DataLength);
            w.Write(UnkBytesCount);

            // Unused in final game
            w.Write(Unk1_Count);
            if (Unk1_Count > 0)
            {
                foreach (var entry in Unk1_Items)
                {
                    w.Write(entry.Size);
                    w.Write(entry.Bytes);
                }
            }

            w.Write(MoveNetworkTriggerCount);
            if (MoveNetworkTriggerCount > 0)
            {
                foreach (var entry in MoveNetworkTriggers)
                {
                    w.Write(entry.Name);
                    w.Write(entry.BitPosition);
                }
            }

            w.Write(MoveNetworkFlagCount);
            if (MoveNetworkFlagCount > 0)
            {
                foreach (var entry in MoveNetworkFlags)
                {
                    w.Write(entry.Name);
                    w.Write(entry.BitPosition);
                }
            }

            if (AllNodes != null)
            {
                foreach (var node in AllNodes)
                {
                    if (updateOffsets) node.FileOffset = (int)w.Position;
                    node.Write(w);
                    if (updateOffsets) node.FileDataSize = (int)(w.Position - node.FileOffset);
                }

                if (updateOffsets)
                {
                    foreach (var node in AllNodes)
                    {
                        node.UpdateRelativeOffsets();
                    }
                }
            }

            for (int i = 0; i < UnkBytesCount; i++)
                w.Write(UnkBytes[i]);
        }

        private void Read(DataReader r)
        {
            Magic = r.ReadUInt32(); // Should be 'MoVE'
            Version = r.ReadUInt32(); // GTA5 = 2, RDR3 = 11
            HeaderUnk1 = r.ReadUInt32(); // Should be 0
            HeaderUnk2 = r.ReadUInt32();
            HeaderUnk3 = r.ReadUInt32(); // Should be 0
            DataLength = r.ReadUInt32();
            UnkBytesCount = r.ReadUInt32();

            if (Magic != 0x45566F4D || Version != 2 || HeaderUnk1 != 0 || HeaderUnk2 != 0)
                throw new Exception("Failed to read MRF, header is invalid!");

            // Unused in final game
            Unk1_Count = r.ReadUInt32();
            if (Unk1_Count > 0)
            {
                Unk1_Items = new MrfHeaderUnk1[Unk1_Count];

                for (int i = 0; i < Unk1_Count; i++)
                    Unk1_Items[i] = new MrfHeaderUnk1(r);
            }

            MoveNetworkTriggerCount = r.ReadUInt32();
            if (MoveNetworkTriggerCount > 0)
            {
                MoveNetworkTriggers = new MrfMoveNetworkBit[MoveNetworkTriggerCount];

                for (int i = 0; i < MoveNetworkTriggerCount; i++)
                    MoveNetworkTriggers[i] = new MrfMoveNetworkBit(r);
            }

            MoveNetworkFlagCount = r.ReadUInt32();
            if (MoveNetworkFlagCount > 0)
            {
                MoveNetworkFlags = new MrfMoveNetworkBit[MoveNetworkFlagCount];

                for (int i = 0; i < MoveNetworkFlagCount; i++)
                    MoveNetworkFlags[i] = new MrfMoveNetworkBit(r);
            }

            var nodes = new List<MrfNode>();

            while (true)
            {
                var index = nodes.Count;

                var node = ReadNode(r);
                
                if (node == null) break;

                node.FileIndex = index;
                nodes.Add(node);
            }

            AllNodes = nodes.ToArray();

            if (UnkBytesCount != 0)
            {
                UnkBytes = new byte[UnkBytesCount];

                for (int i = 0; i < UnkBytesCount; i++)
                    UnkBytes[i] = r.ReadByte();
            }

            RootState = AllNodes.Length > 0 ? (MrfNodeStateBase)AllNodes[0] : null; // the first node is always a state or state machine node (not inlined state machine)
            ResolveRelativeOffsets();

            DebugTreeGraph = DumpTreeGraph();
            DebugStateGraph = DumpStateGraph();

            if (r.Length != (DataLength + 32 + (Unk1_Items?.Sum(i => i.Size) ?? 0) + UnkBytesCount))
            { } // no hits

            if (r.Position != r.Length)
                throw new Exception($"Failed to read MRF ({r.Position} / {r.Length})");
        }

        private MrfNode ReadNode(DataReader r)
        {
            var startPos = r.Position;
            var nodeType = (MrfNodeType)r.ReadUInt16();
            r.Position = startPos;

            if (nodeType <= MrfNodeType.None || nodeType >= MrfNodeType.Max)
            {
                if (r.Position != r.Length)//should only be at EOF
                { }
                return null;
            }

            var node = CreateNode(nodeType);
            node.FileOffset = (int)startPos;
            node.Read(r);
            node.FileDataSize = (int)(r.Position - node.FileOffset);

            return node;
        }

        public static MrfNode CreateNode(MrfNodeType infoType)
        {
            switch (infoType)
            {
                case MrfNodeType.StateMachine:
                    return new MrfNodeStateMachine();
                case MrfNodeType.Tail:
                    return new MrfNodeTail();
                case MrfNodeType.InlinedStateMachine:
                    return new MrfNodeInlinedStateMachine();
                case MrfNodeType.Animation:
                    return new MrfNodeAnimation();
                case MrfNodeType.Blend:
                    return new MrfNodeBlend();
                case MrfNodeType.AddSubtract:
                    return new MrfNodeAddSubtract();
                case MrfNodeType.Filter:
                    return new MrfNodeFilter();
                case MrfNodeType.Mirror:
                    return new MrfNodeMirror();
                case MrfNodeType.Frame:
                    return new MrfNodeFrame();
                case MrfNodeType.Ik:
                    return new MrfNodeIk();
                case MrfNodeType.BlendN:
                    return new MrfNodeBlendN();
                case MrfNodeType.Clip:
                    return new MrfNodeClip();
                case MrfNodeType.Pm:
                    return new MrfNodePm();
                case MrfNodeType.Extrapolate:
                    return new MrfNodeExtrapolate();
                case MrfNodeType.Expression:
                    return new MrfNodeExpression();
                case MrfNodeType.Capture:
                    return new MrfNodeCapture();
                case MrfNodeType.Proxy:
                    return new MrfNodeProxy();
                case MrfNodeType.AddN:
                    return new MrfNodeAddN();
                case MrfNodeType.Identity:
                    return new MrfNodeIdentity();
                case MrfNodeType.Merge:
                    return new MrfNodeMerge();
                case MrfNodeType.Pose:
                    return new MrfNodePose();
                case MrfNodeType.MergeN:
                    return new MrfNodeMergeN();
                case MrfNodeType.State:
                    return new MrfNodeState();
                case MrfNodeType.Invalid:
                    return new MrfNodeInvalid();
                case MrfNodeType.JointLimit:
                    return new MrfNodeJointLimit();
                case MrfNodeType.SubNetwork:
                    return new MrfNodeSubNetwork();
                case MrfNodeType.Reference:
                    return new MrfNodeReference();
            }

            throw new Exception($"A handler for ({infoType}) mrf node type is not valid");
        }

        private void ResolveRelativeOffsets()
        {
            foreach (var n in AllNodes)
            {
                n.ResolveRelativeOffsets(this);
            }
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.WriteItemArray(sb, MoveNetworkTriggers?.Where(t => !t.IsEndMarker).ToArray(), indent, "MoveNetworkTriggers");
            MrfXml.WriteItemArray(sb, MoveNetworkFlags?.Where(t => !t.IsEndMarker).ToArray(), indent, "MoveNetworkFlags");
            MrfXml.WriteNode(sb, indent, "RootState", RootState);
            MrfXml.WriteItemArray(sb, Unk1_Items, indent, "Unk1");
            MrfXml.WriteRawArray(sb, UnkBytes, indent, "UnkBytes", "", MrfXml.FormatHexByte, 16);
        }
        public void ReadXml(XmlNode node)
        {
            var triggers = XmlMeta.ReadItemArray<MrfMoveNetworkBit>(node, "MoveNetworkTriggers");
            var flags = XmlMeta.ReadItemArray<MrfMoveNetworkBit>(node, "MoveNetworkFlags");
            MoveNetworkTriggers = SortMoveNetworkBitsArray(triggers);
            MoveNetworkFlags = SortMoveNetworkBitsArray(flags);
            RootState = (MrfNodeStateBase)XmlMrf.ReadChildNode(node, "RootState");
            Unk1_Items = XmlMeta.ReadItemArrayNullable<MrfHeaderUnk1>(node, "Unk1");
            UnkBytes = Xml.GetChildRawByteArrayNullable(node, "UnkBytes");
            MoveNetworkTriggerCount = (uint)(MoveNetworkTriggers?.Length ?? 0);
            MoveNetworkFlagCount = (uint)(MoveNetworkFlags?.Length ?? 0);
            Unk1_Count = (uint)(Unk1_Items?.Length ?? 0);
            UnkBytesCount = (uint)(UnkBytes?.Length ?? 0);

            AllNodes = BuildNodesArray(RootState);

            // At this point the TargetStates of most transitions have been resolved by MrfNodeStateBase.ResolveXmlTargetStatesInTransitions
            // but there is one transition in onfoothuman.mrf with a target state not in the parent StateMachine (not really sure why,
            // it points to a state in a sibling tree), so ResolveXmlTargetStatesInTransitions can't find it.
            // The source state is node hash_76D78558 and the transition target state is hash_1836C818.
            // Iterate all transitions and try to resolve them again if target state is still null to solve this edge case.
            var stateNodes = AllNodes.OfType<MrfNodeStateBase>();
            foreach (var state in stateNodes)
            {
                if (state.Transitions == null) continue;
                
                foreach (var t in state.Transitions)
                {
                    if (t.TargetState != null) continue;

                    t.TargetState = stateNodes.FirstOrDefault(n => n.Name == t.XmlTargetStateName);
                }
            }
            
            DebugTreeGraph = DumpTreeGraph();
            DebugStateGraph = DumpStateGraph();
        }

        private static MrfNode[] BuildNodesArray(MrfNodeStateBase root)
        {
            var nodes = new List<MrfNode>();
            AddRecursive(root);
            return nodes.ToArray();

            void AddRecursive(MrfNode node)
            {
                nodes.Add(node);

                IEnumerable<MrfNode> children = null;
                if (node is MrfNodeStateMachine sm)
                {
                    children = sm.States.Select(s => s.State);
                }
                else if (node is MrfNodeInlinedStateMachine ism)
                {
                    children = ism.States.Select(s => s.State);
                }
                else if (node is MrfNodeState ns)
                {
                    children = ns.GetChildren(excludeTailNodes: false);
                }

                if (children != null)
                {
                    foreach (var c in children.OrderBy(s => s is MrfNodeTail ? ushort.MaxValue : s.NodeIndex)) // NodeTail is placed after other nodes, their NodeIndex is ignored
                    {
                        AddRecursive(c);
                    }
                }
            }
        }

        public MrfNode FindNodeAtFileOffset(int fileOffset)
        {
            foreach (var n in AllNodes)
            {
                if (n.FileOffset == fileOffset) return n;
            }

            return null;
        }

        public MrfMoveNetworkBit? FindMoveNetworkTriggerForBit(int bitPosition)
        {
            return FindMoveNetworkBitByBitPosition(MoveNetworkTriggers, bitPosition);
        }
        public MrfMoveNetworkBit? FindMoveNetworkFlagForBit(int bitPosition)
        {
            return FindMoveNetworkBitByBitPosition(MoveNetworkFlags, bitPosition);
        }
        public static MrfMoveNetworkBit? FindMoveNetworkBitByBitPosition(MrfMoveNetworkBit[] bits, int bitPosition)
        {
            if (bits == null)
            {
                return null;
            }

            foreach (var flag in bits)
            {
                if (!flag.IsEndMarker && flag.BitPosition == bitPosition) return flag;
            }

            return null;
        }

        // MoveNetworkTriggers and MoveNetworkFlags getters by name for reference of how the arrays should be sorted in buckets
        public MrfMoveNetworkBit? FindMoveNetworkTriggerByName(MetaHash name)
        {
            return FindMoveNetworkBitByName(MoveNetworkTriggers, name);
        }
        public MrfMoveNetworkBit? FindMoveNetworkFlagByName(MetaHash name)
        {
            return FindMoveNetworkBitByName(MoveNetworkFlags, name);
        }
        public static MrfMoveNetworkBit? FindMoveNetworkBitByName(MrfMoveNetworkBit[] bits, MetaHash name)
        {
            if (bits == null)
            {
                return null;
            }

            for (int i = (int)(name.Hash % bits.Length); ; i = (i + 1) % bits.Length)
            {
                var b = bits[i];
                if (b.IsEndMarker) break;
                if (b.Name == name) return b;
            }

            return null;
        }

        public static MrfMoveNetworkBit[] SortMoveNetworkBitsArray(MrfMoveNetworkBit[] bits)
        {
            if (bits == null)
            {
                return null;
            }

            var bitsSorted = new MrfMoveNetworkBit[bits.Length + 1]; // +1 for the end marker

            bits = bits.OrderBy(b => b.BitPosition).ToArray();
            for (int i = 0; i < bits.Length; i++)
            {
                var sortedIdx = bits[i].Name % bitsSorted.Length;
                while (bitsSorted[sortedIdx].Name != 0)
                {
                    sortedIdx = (sortedIdx + 1) % bitsSorted.Length;
                }
                bitsSorted[sortedIdx] = bits[i];
            }

            // place the end marker in the only empty slot left
            for (int i = 0; i < bitsSorted.Length; i++)
            {
                if (bitsSorted[i].Name == 0)
                {
                    bitsSorted[i] = MrfMoveNetworkBit.EndMarker;
                    break;
                }
            }

            return bitsSorted;
        }

        /// <summary>
        /// Dump a DOT graph with the whole node hierarchy as a tree.
        /// </summary>
        public string DumpTreeGraph()
        {
            using (var w = new StringWriter())
            {
                w.WriteLine($@"digraph ""{Name}"" {{");
                w.WriteLine($@"    label=""{Name}""");
                w.WriteLine($@"    labelloc=""t""");
                w.WriteLine($@"    concentrate=true");
                w.WriteLine($@"    rankdir=""LR""");
                w.WriteLine($@"    graph[fontname = ""Consolas""];");
                w.WriteLine($@"    edge[fontname = ""Consolas""];");
                w.WriteLine($@"    node[fontname = ""Consolas""];");
                w.WriteLine();

                w.WriteLine("    root [label=\"root\"];");

                // nodes
                foreach (var n in AllNodes)
                {
                    var id = n.FileOffset;
                    var label = $"{n.NodeType} '{n.Name}'";
                    w.WriteLine("    n{0} [label=\"{1}\"];", id, label);
                }
                w.WriteLine();

                // edges
                w.WriteLine("    n{0} -> root [color = black]", RootState.FileOffset);
                foreach (var n in AllNodes)
                {
                    MrfStateTransition[] transitions = null;
                    MrfNode initial = null;
                    if (n is MrfNodeStateBase sb)
                    {
                        initial = sb.InitialNode;
                        transitions = sb.Transitions;
                    }


                    if (n is MrfNodeInlinedStateMachine im)
                    {
                        w.WriteLine("    n{1} -> n{0} [color = black, xlabel=\"fallback\"]", n.FileOffset, im.FallbackNode.FileOffset);
                    }

                    if (n is MrfNodeWithChildBase f)
                    {
                        w.WriteLine("    n{1} -> n{0} [color = black, xlabel=\"child\"]", n.FileOffset, f.Child.FileOffset);
                    }

                    if (n is MrfNodePairBase p)
                    {
                        w.WriteLine("    n{1} -> n{0} [color = black, xlabel=\"#0\"]", n.FileOffset, p.Child0.FileOffset);
                        w.WriteLine("    n{1} -> n{0} [color = black, xlabel=\"#1\"]", n.FileOffset, p.Child1.FileOffset);
                    }

                    if (n is MrfNodeNBase nn && nn.Children != null)
                    {
                        for (int i = 0; i < nn.Children.Length; i++)
                        {
                            w.WriteLine("    n{1} -> n{0} [color = black, xlabel=\"#{2}\"]", n.FileOffset, nn.Children[i].FileOffset, i);
                        }
                    }

                    if (transitions != null)
                    {
                        foreach (var transition in transitions)
                        {
                            var conditions = transition.Conditions == null ? "[]" : "[" + string.Join(" & ", transition.Conditions.Select(c => c.ToExpressionString(this))) + "]";
                            var target = transition.TargetState;
                            w.WriteLine("    n{1} -> n{0} [color = black, xlabel=\"T {2}\"]", n.FileOffset, target.FileOffset, conditions);
                        }
                    }

                    if (initial != null)
                    {
                        w.WriteLine("    n{1} -> n{0} [color = black, xlabel=\"init\"]", n.FileOffset, initial.FileOffset);
                    }
                }

                // footer
                w.WriteLine("}");

                return w.ToString();
            }
        }

        /// <summary>
        /// Dump a DOT graph of the state machines where nodes are placed inside their corresponding state.
        /// </summary>
        public string DumpStateGraph()
        {
            using (var w = new StringWriter())
            {
                w.WriteLine($@"digraph ""{Name}"" {{");
                w.WriteLine($@"    label=""{Name}""");
                w.WriteLine($@"    labelloc=""t""");
                w.WriteLine($@"    concentrate=true");
                w.WriteLine($@"    compound=true");
                w.WriteLine($@"    rankdir=""LR""");
                w.WriteLine($@"    graph[fontname = ""Consolas""];");
                w.WriteLine($@"    edge[fontname = ""Consolas""];");
                w.WriteLine($@"    node[fontname = ""Consolas""];");
                w.WriteLine();

                DumpNode(RootState, w, null);

                w.WriteLine("    root [label=\"root\",shape=\"diamond\"];");
                w.WriteLine("    root -> S{0} [color = black][lhead=\"clusterS{0}\"]", RootState.FileOffset);

                // footer
                w.WriteLine("}");

                return w.ToString();
            }
        }

        private void DumpStateMachineSubGraph(MrfNodeStateMachine sm, TextWriter w)
        {
            // header
            w.WriteLine($@"subgraph ""clusterS{sm.FileOffset}"" {{");
            w.WriteLine($@"    label=""State Machine '{sm.Name}'""");
            w.WriteLine($@"    labelloc=""t""");
            w.WriteLine($@"    concentrate=true");
            w.WriteLine($@"    rankdir=""RL""");
            w.WriteLine($@"    S{sm.FileOffset}[shape=""none""][style=""invis""][label=""""]"); // hidden node to be able to connect subgraphs
            w.WriteLine();

            if (sm.States != null)
            {
                foreach (var state in sm.States)
                {
                    var stateNode = state.State;
                    if (stateNode is MrfNodeState ns) DumpStateSubGraph(ns, w);
                    if (stateNode is MrfNodeStateMachine nsm) DumpStateMachineSubGraph(nsm, w);
                    if (stateNode is MrfNodeInlinedStateMachine ism) DumpInlinedStateMachineSubGraph(ism, w);
                }

                foreach (var state in sm.States)
                {
                    DumpStateTransitionsGraph(state.State, w);
                }
            }

            w.WriteLine("    startS{0} [label=\"start\",shape=\"diamond\"];", sm.FileOffset);
            w.WriteLine("    startS{0} -> S{1} [color = black][lhead=\"clusterS{1}\"]", sm.FileOffset, sm.InitialNode.FileOffset);

            // footer
            w.WriteLine("}");
        }

        private void DumpInlinedStateMachineSubGraph(MrfNodeInlinedStateMachine sm, TextWriter w)
        {
            // header
            w.WriteLine($@"subgraph ""clusterS{sm.FileOffset}"" {{");
            w.WriteLine($@"    label=""Inlined State Machine '{sm.Name}'""");
            w.WriteLine($@"    labelloc=""t""");
            w.WriteLine($@"    concentrate=true");
            w.WriteLine($@"    rankdir=""RL""");
            w.WriteLine($@"    S{sm.FileOffset}[shape=""none""][style=""invis""][label=""""]"); // hidden node to be able to connect subgraphs
            w.WriteLine();

            if (sm.States != null)
            {
                foreach (var state in sm.States)
                {
                    var stateNode = state.State;
                    if (stateNode is MrfNodeState ns) DumpStateSubGraph(ns, w);
                    if (stateNode is MrfNodeStateMachine nsm) DumpStateMachineSubGraph(nsm, w);
                    if (stateNode is MrfNodeInlinedStateMachine ism) DumpInlinedStateMachineSubGraph(ism, w);
                }

                foreach (var state in sm.States)
                {
                    DumpStateTransitionsGraph(state.State, w);
                }
            }

            w.WriteLine("    startS{0} [label=\"start\",shape=\"diamond\"];", sm.FileOffset);
            w.WriteLine("    startS{0} -> S{1} [color = black][lhead=\"clusterS{1}\"]", sm.FileOffset, sm.InitialNode.FileOffset);

            if (sm.FallbackNode != null)
            {
                var fn = sm.FallbackNode;
                DumpNode(fn, w, null);

                w.WriteLine("    fallbackS{0} [label=\"fallback\",shape=\"diamond\"];", sm.FileOffset);
                if (fn is MrfNodeStateBase) w.WriteLine("    fallbackS{0} -> S{1} [color = black][lhead=\"clusterS{1}\"]", sm.FileOffset, fn.FileOffset);
                else w.WriteLine("    fallbackS{0} -> n{1} [color = black]", sm.FileOffset, fn.FileOffset);
            }

            // footer
            w.WriteLine("}");
        }

        private void DumpStateTransitionsGraph(MrfNodeStateBase from, TextWriter w)
        {
            var transitions = from.Transitions;
            if (transitions != null)
            {
                int i = 0;
                foreach (var transition in transitions)
                {
                    var conditions = transition.Conditions == null ? "[]" : "[" + string.Join(" & ", transition.Conditions.Select(c => c.ToExpressionString(this))) + "]";
                    var target = transition.TargetState;
                    w.WriteLine("    S{0} -> S{1} [color = black, xlabel=\"T#{2} {3}\"][ltail=\"clusterS{0}\"][lhead=\"clusterS{1}\"]", from.FileOffset, target.FileOffset, i, conditions);
                    i++;
                }
            }
        }

        private void DumpStateSubGraph(MrfNodeStateBase state, TextWriter w)
        {
            if (state.InitialNode == null)
            {
                return;
            }

            // header
            w.WriteLine($@"subgraph clusterS{state.FileOffset} {{");
            w.WriteLine($@"    label=""State '{state.Name}'""");
            w.WriteLine($@"    labelloc=""t""");
            w.WriteLine($@"    concentrate=true");
            w.WriteLine($@"    rankdir=""LR""");
            w.WriteLine($@"    S{state.FileOffset}[shape=""none""][style=""invis""][label=""""]"); // hidden node to be able to connect subgraphs
            w.WriteLine();

            var initial = state.InitialNode;
            DumpNode(initial, w, null);

            w.WriteLine("    outputS{0} [label=\"output\",shape=\"point\"];", state.FileOffset);
            if (initial is MrfNodeStateBase) w.WriteLine("    S{0} -> outputS{1} [color = black][ltail=\"clusterS{0}\"]", initial.FileOffset, state.FileOffset);
            else w.WriteLine("    n{0} -> outputS{1} [color = black]", initial.FileOffset, state.FileOffset);

            // footer
            w.WriteLine("}");
        }

        private void DumpNodeGraph(MrfNode n, TextWriter w, HashSet<MrfNode> visitedNodes)
        {
            if (!visitedNodes.Add(n))
            {
                return;
            }

            var label = $"{n.NodeType} '{n.Name}'";
            if (n is MrfNodeSubNetwork sub)
            {
                label += $"\\n'{sub.SubNetworkParameterName}'";
            }

            w.WriteLine("    n{0} [label=\"{1}\"];", n.FileOffset, label);

            void writeConnection(MrfNode target, string connectionLabel)
            {
                if (target is MrfNodeStateBase) w.WriteLine("    S{1} -> n{0} [color = black, xlabel=\"{2}\"][ltail=\"clusterS{1}\"]", n.FileOffset, target.FileOffset, connectionLabel);
                else w.WriteLine("    n{1} -> n{0} [color = black, xlabel=\"{2}\"]", n.FileOffset, target.FileOffset, connectionLabel);
            }

            if (n is MrfNodeInlinedStateMachine im)
            {
                DumpNode(im.FallbackNode, w, visitedNodes);
                writeConnection(im.FallbackNode, "fallback");
            }

            if (n is MrfNodeWithChildBase f)
            {
                DumpNode(f.Child, w, visitedNodes);
                writeConnection(f.Child, "");
            }

            if (n is MrfNodePairBase p)
            {
                DumpNode(p.Child0, w, visitedNodes);
                DumpNode(p.Child1, w, visitedNodes);
                writeConnection(p.Child0, "#0");
                writeConnection(p.Child1, "#1");
            }

            if (n is MrfNodeNBase nn && nn.Children != null)
            {
                for (int i = 0; i < nn.Children.Length; i++)
                {
                    DumpNode(nn.Children[i], w, visitedNodes);
                    writeConnection(nn.Children[i], $"#{i}");
                }
            }
        }

        private void DumpNode(MrfNode n, TextWriter w, HashSet<MrfNode> visitedNodes)
        {
            if (n is MrfNodeState ns) DumpStateSubGraph(ns, w);
            else if (n is MrfNodeStateMachine nsm) DumpStateMachineSubGraph(nsm, w);
            else if (n is MrfNodeInlinedStateMachine ism) DumpInlinedStateMachineSubGraph(ism, w);
            else DumpNodeGraph(n, w, visitedNodes ?? new HashSet<MrfNode>());
        }

        /// <summary>
        /// Writer used to calculate where the nodes will be placed, so the relative offsets can be calculated before using the real writer.
        /// </summary>
        private class NoOpDataWriter : DataWriter
        {
            private long length;
            private long position;

            public override long Length => length;
            public override long Position { get => position; set => position = value; }

            public NoOpDataWriter() : base(null, Endianess.LittleEndian)
            {
            }

            protected override void WriteToStream(byte[] value, bool ignoreEndianess = false)
            {
                position += value.Length;
                length = Math.Max(length, position);
            }
        }
    }



    // Unused node indexes by GTAV: 11, 12, 14, 16
    // Exist in GTAV but not used in MRFs: 4, 8, 10, 17, 21, 22, 28, 29, 31, 32
    public enum MrfNodeType : ushort
    {
        None = 0,
        StateMachine = 1,
        Tail = 2,
        InlinedStateMachine = 3,
        Animation = 4,
        Blend = 5,
        AddSubtract = 6,
        Filter = 7,
        Mirror = 8,
        Frame = 9,
        Ik = 10,
        BlendN = 13,
        Clip = 15,
        Pm = 17,
        Extrapolate = 18,
        Expression = 19,
        Capture = 20,
        Proxy = 21,
        AddN = 22,
        Identity = 23,
        Merge = 24,
        Pose = 25,
        MergeN = 26,
        State = 27,
        Invalid = 28,
        JointLimit = 29,
        SubNetwork = 30,
        Reference = 31,
        Max = 32
    }

    public enum MrfNodeParameterId : ushort // node parameter IDs are specific to the node type
    {
        // StateMachine
        // none

        // Tail
        // none

        // InlinedStateMachine
        // none

        // Animation
        Animation_Animation = 0, // rage::crAnimation (only setter)
        Animation_Unk1 = 1, // float
        Animation_Unk2 = 2, // float
        Animation_Unk3 = 3, // float
        Animation_Unk4 = 4, // bool
        Animation_Unk5 = 5, // bool

        // Blend
        Blend_Filter = 0, // rage::crFrameFilter
        Blend_Weight = 1, // float

        // AddSubtract
        AddSubtract_Filter = 0, // rage::crFrameFilter
        AddSubtract_Weight = 1, // float

        // Filter
        Filter_Filter = 0, // rage::crFrameFilter

        // Mirror
        Mirror_Filter = 0, // rage::crFrameFilter

        // Frame
        Frame_Frame = 0, // rage::crFrame

        // Ik
        // none

        // BlendN
        BlendN_Filter = 1,      // rage::crFrameFilter
        BlendN_ChildWeight = 2, // float (extra arg is the child index)
        BlendN_ChildFilter = 3, // rage::crFrameFilter (extra arg is the child index)

        // Clip
        Clip_Clip = 0,   // rage::crClip
        Clip_Phase = 1,  // float
        Clip_Rate = 2,   // float
        Clip_Delta = 3,  // float
        Clip_Looped = 4, // bool

        // Pm
        Pm_Motion = 0, // rage::crpmMotion
        Pm_Unk1 = 1, // float
        Pm_Unk2 = 2, // float
        Pm_Unk3 = 3, // float
        Pm_Unk4 = 4, // float

        // Extrapolate
        Extrapolate_Damping = 0, // float

        // Expression
        Expression_Expression = 0, // rage::crExpressions
        Expression_Weight = 1,     // float
        Expression_Variable = 2,   // float (extra arg is the variable name)

        // Capture
        Capture_Frame = 0, // rage::crFrame

        // Proxy
        Proxy_Node = 0, // rage::crmtNode (the type resolver returns rage::crmtObserver but the getter/setter expect a rage::crmtNode, R* bug?)

        // AddN
        AddN_Filter = 1,      // rage::crFrameFilter
        AddN_ChildWeight = 2, // float (extra arg is the child index)
        AddN_ChildFilter = 3, // rage::crFrameFilter (extra arg is the child index)

        // Identity
        // none

        // Merge
        Merge_Filter = 0, // rage::crFrameFilter

        // Pose
        Pose_IsNormalized = 0, // bool (getter hardcoded to true, setter does nothing)

        // MergeN
        MergeN_Filter = 1,      // rage::crFrameFilter
        MergeN_ChildWeight = 2, // float (extra arg is the child index)
        MergeN_ChildFilter = 3, // rage::crFrameFilter (extra arg is the child index)

        // State
        // none

        // Invalid
        // none

        // JointLimit
        JointLimit_Filter = 0, // rage::crFrameFilter (only setter exists)

        // SubNetwork
        // none

        // Reference
        // none
    }

    public enum MrfNodeEventId : ushort // node event IDs are specific to the node type
    {
        // StateMachine
        // none

        // Tail
        // none

        // InlinedStateMachine
        // none

        // Animation
        Animation_Unk0 = 0,
        Animation_Unk1 = 1,

        // Blend
        // none

        // AddSubtract
        // none

        // Filter
        // none

        // Mirror
        // none

        // Frame
        // none

        // Ik
        // none

        // BlendN
        // none

        // Clip
        Clip_IterationFinished = 0, // triggered when a looped clip iteration finishes playing
        Clip_Finished = 1,          // triggered when a non-looped clip finishes playing
        Clip_Unk2 = 2,
        Clip_Unk3 = 3,
        Clip_Unk4 = 4,

        // Pm
        Pm_Unk0 = 0,
        Pm_Unk1 = 1,

        // Extrapolate
        // none

        // Expression
        // none

        // Capture
        // none

        // Proxy
        // none

        // AddN
        // none

        // Identity
        // none

        // Merge

        // Pose
        // none

        // MergeN
        // none

        // State
        // none

        // Invalid
        // none

        // JointLimit
        // none

        // SubNetwork
        // none

        // Reference
        // none
    }

#region mrf node abstractions

    [TC(typeof(EXP))] public abstract class MrfNode : IMetaXmlItem
    {
        public MrfNodeType NodeType { get; set; }
        public ushort NodeIndex { get; set; } //index in the parent state node
        public MetaHash Name { get; set; }

        public int FileIndex { get; set; } //index in the file
        public int FileOffset { get; set; } //offset in the file
        public int FileDataSize { get; set; } //number of bytes read from the file (this node only)

        public MrfNode(MrfNodeType type)
        {
            NodeType = type;
        }

        public virtual void Read(DataReader r)
        {
            NodeType = (MrfNodeType)r.ReadUInt16();
            NodeIndex = r.ReadUInt16();
            Name = r.ReadUInt32();
        }

        public virtual void Write(DataWriter w)
        {
            w.Write((ushort)NodeType);
            w.Write(NodeIndex);
            w.Write(Name);
        }

        public virtual void ReadXml(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            NodeIndex = (ushort)Xml.GetChildUIntAttribute(node, "NodeIndex");
        }

        public virtual void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.StringTag(sb, indent, "Name", MrfXml.HashString(Name));
            MrfXml.ValueTag(sb, indent, "NodeIndex", NodeIndex.ToString());
        }

        public override string ToString()
        {
            return /* FileIndex.ToString() + ":" + FileOffset.ToString() + "+" + FileDataSize.ToString() + ": " +  */
                NodeType.ToString() + " - " + NodeIndex.ToString() + " - " + Name.ToString();
        }

        public virtual void ResolveRelativeOffsets(MrfFile mrf)
        {
        }

        public virtual void UpdateRelativeOffsets()
        {
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodeWithFlagsBase : MrfNode
    {
        public uint Flags { get; set; }

        public MrfNodeWithFlagsBase(MrfNodeType type) : base(type) { }

        public override void Read(DataReader r)
        {
            base.Read(r);
            Flags = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(Flags);
        }

        public uint GetFlagsSubset(int bitOffset, uint mask)
        {
            return (Flags >> bitOffset) & mask;
        }

        public void SetFlagsSubset(int bitOffset, uint mask, uint value)
        {
            Flags = (Flags & ~(mask << bitOffset)) | ((value & mask) << bitOffset);
        }

        public override string ToString()
        {
            return base.ToString() + " - " + Flags.ToString("X8");
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodeStateBase : MrfNode
    {
        public int InitialNodeOffset { get; set; } // offset from the start of this field
        public int InitialNodeFileOffset { get; set; }
        public uint StateUnk3 { get; set; }
        public bool HasEntryParameter { get; set; }
        public bool HasExitParameter { get; set; }
        public byte StateChildCount { get; set; } // for Node(Inlined)StateMachine the number of states, for NodeState the number of children excluding NodeTails
        public byte TransitionCount { get; set; }
        public MetaHash EntryParameterName { get; set; } // bool parameter set to true when the network enters this node
        public MetaHash ExitParameterName { get; set; } // bool parameter set to true when the network leaves this node
        public int TransitionsOffset { get; set; } // offset from the start of this field
        public int TransitionsFileOffset { get; set; }

        public MrfNode InitialNode { get; set; } // for Node(Inlined)StateMachine this is a NodeStateBase, for NodeState it can be any node
        public MrfStateTransition[] Transitions { get; set; }

        public MrfNodeStateBase(MrfNodeType type) : base(type) { }

        public override void Read(DataReader r)
        {
            base.Read(r);
            InitialNodeOffset = r.ReadInt32();
            InitialNodeFileOffset = (int)(r.Position + InitialNodeOffset - 4);
            StateUnk3 = r.ReadUInt32();
            HasEntryParameter = r.ReadByte() != 0;
            HasExitParameter = r.ReadByte() != 0;
            StateChildCount = r.ReadByte();
            TransitionCount = r.ReadByte();
            EntryParameterName = r.ReadUInt32();
            ExitParameterName = r.ReadUInt32();
            TransitionsOffset = r.ReadInt32();
            TransitionsFileOffset = (int)(r.Position + TransitionsOffset - 4);
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(InitialNodeOffset);
            w.Write(StateUnk3);
            w.Write((byte)(HasEntryParameter ? 1 : 0));
            w.Write((byte)(HasExitParameter ? 1 : 0));
            w.Write(StateChildCount);
            w.Write(TransitionCount);
            w.Write(EntryParameterName);
            w.Write(ExitParameterName);
            w.Write(TransitionsOffset);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            StateUnk3 = Xml.GetChildUIntAttribute(node, "StateUnk3");
            EntryParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "EntryParameterName"));
            ExitParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "ExitParameterName"));
            HasEntryParameter = EntryParameterName != 0;
            HasExitParameter = ExitParameterName != 0;
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.ValueTag(sb, indent, "StateUnk3", StateUnk3.ToString());
            MrfXml.StringTag(sb, indent, "EntryParameterName", HasEntryParameter ? MrfXml.HashString(EntryParameterName) : null);
            MrfXml.StringTag(sb, indent, "ExitParameterName", HasExitParameter ? MrfXml.HashString(ExitParameterName) : null);
        }

        public override void ResolveRelativeOffsets(MrfFile mrf)
        {
            base.ResolveRelativeOffsets(mrf);

            var initNode = mrf.FindNodeAtFileOffset(InitialNodeFileOffset);
            if (initNode == null)
            { } // no hits

            if ((this is MrfNodeStateMachine || this is MrfNodeInlinedStateMachine) && !(initNode is MrfNodeStateBase))
            { } // no hits, state machines initial node is always a MrfNodeStateBase

            InitialNode = initNode;
        }

        public override void UpdateRelativeOffsets()
        {
            base.UpdateRelativeOffsets();

            InitialNodeFileOffset = InitialNode.FileOffset;
            InitialNodeOffset = InitialNodeFileOffset - (FileOffset + 0x8);
        }

        protected void ResolveNodeOffsetsInTransitions(MrfStateTransition[] transitions, MrfFile mrf)
        {
            if (transitions == null)
            {
                return;
            }

            foreach (var t in transitions)
            {
                var node = mrf.FindNodeAtFileOffset(t.TargetStateFileOffset);
                if (node == null)
                { } // no hits

                if (!(node is MrfNodeStateBase))
                { } // no hits

                t.TargetState = (MrfNodeStateBase)node;
            }
        }

        protected void ResolveNodeOffsetsInStates(MrfStateRef[] states, MrfFile mrf)
        {
            if (states == null)
            {
                return;
            }

            foreach (var s in states)
            {
                var node = mrf.FindNodeAtFileOffset(s.StateFileOffset);
                if (node == null)
                { } // no hits

                if (!(node is MrfNodeStateBase))
                { } // no hits

                s.State = (MrfNodeStateBase)node;
            }
        }

        protected int UpdateNodeOffsetsInTransitions(MrfStateTransition[] transitions, int transitionsArrayOffset, bool offsetSetToZeroIfNoTransitions)
        {
            int offset = transitionsArrayOffset;
            TransitionsFileOffset = offset;
            TransitionsOffset = TransitionsFileOffset - (FileOffset + 0x1C);
            if (transitions != null)
            {
                foreach (var transition in transitions)
                {
                    transition.TargetStateFileOffset = transition.TargetState.FileOffset;
                    transition.TargetStateOffset = transition.TargetStateFileOffset - (offset + 0x14);
                    transition.CalculateDataSize();
                    offset += (int)transition.DataSize;
                }
            }
            else if (offsetSetToZeroIfNoTransitions)
            {
                // Special case for some MrfNodeStateMachines with no transitions and MrfNodeInlinedStateMachines.
                // Unlike MrfNodeState, when these don't have transtions the TransititionsOffset is 0.
                // So we set it to 0 too to be able to compare Save() result byte by byte
                TransitionsOffset = 0;
                TransitionsFileOffset = FileOffset + 0x1C; // and keep FileOffset consistent with what Read() does
            }
            return offset;
        }

        protected int UpdateNodeOffsetsInStates(MrfStateRef[] states, int statesArrayOffset)
        {
            int offset = statesArrayOffset;
            if (states != null)
            {
                foreach (var state in states)
                {
                    state.StateFileOffset = state.State.FileOffset;
                    state.StateOffset = state.StateFileOffset - (offset + 4);
                    offset += 8; // sizeof(MrfStructStateMachineStateRef)
                }
            }
            return offset;
        }

        protected void ResolveXmlTargetStatesInTransitions(MrfStateRef[] states)
        {
            if (states == null)
            {
                return;
            }

            foreach (var state in states)
            {
                if (state.State.Transitions == null) continue;

                foreach (var t in state.State.Transitions)
                {
                    t.TargetState = states.FirstOrDefault(s => s.StateName == t.XmlTargetStateName)?.State;
                    if (t.TargetState == null)
                    { } // only 1 hit in onfoothuman.mrf, solved at the end of MrfFile.ReadXml
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() + " - " + Name.ToString()
                + " - Init:" + InitialNodeOffset.ToString()
                + " - CC:" + StateChildCount.ToString()
                + " - TC:" + TransitionCount.ToString()
                + " - " + StateUnk3.ToString()
                + " - OnEntry(" + HasEntryParameter.ToString() + "):" + EntryParameterName.ToString()
                + " - OnExit(" + HasExitParameter.ToString() + "):" + ExitParameterName.ToString()
                + " - TO:" + TransitionsOffset.ToString();
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodePairBase : MrfNodeWithFlagsBase
    {
        public int Child0Offset { get; set; }
        public int Child0FileOffset { get; set; }
        public int Child1Offset { get; set; }
        public int Child1FileOffset { get; set; }

        public MrfNode Child0 { get; set; }
        public MrfNode Child1 { get; set; }

        public MrfNodePairBase(MrfNodeType type) : base(type) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Child0Offset = r.ReadInt32();
            Child0FileOffset = (int)(r.Position + Child0Offset - 4);
            Child1Offset = r.ReadInt32();
            Child1FileOffset = (int)(r.Position + Child1Offset - 4);
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Child0Offset);
            w.Write(Child1Offset);
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.WriteNode(sb, indent, "Child0", Child0);
            MrfXml.WriteNode(sb, indent, "Child1", Child1);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Child0 = XmlMrf.ReadChildNode(node, "Child0");
            Child1 = XmlMrf.ReadChildNode(node, "Child1");
        }

        public override void ResolveRelativeOffsets(MrfFile mrf)
        {
            base.ResolveRelativeOffsets(mrf);

            var child0 = mrf.FindNodeAtFileOffset(Child0FileOffset);
            if (child0 == null)
            { } // no hits

            var child1 = mrf.FindNodeAtFileOffset(Child1FileOffset);
            if (child1 == null)
            { } // no hits

            Child0 = child0;
            Child1 = child1;
        }

        public override void UpdateRelativeOffsets()
        {
            base.UpdateRelativeOffsets();

            Child0FileOffset = Child0.FileOffset;
            Child0Offset = Child0FileOffset - (FileOffset + 0xC + 0);
            Child1FileOffset = Child1.FileOffset;
            Child1Offset = Child1FileOffset - (FileOffset + 0xC + 4);
        }
    }

    public enum MrfSynchronizerType
    {
        Phase = 0, // attaches a rage::mvSynchronizerPhase instance to the node
        Tag = 1,   // attaches a rage::mvSynchronizerTag instance to the node, sets the SynchronizerTagFlags field too
        None = 2,
    }

    [Flags] public enum MrfSynchronizerTagFlags : uint
    {
        LeftFootHeel = 0x20,  // adds rage::mvSynchronizerTag::sm_LeftFootHeel to a rage::mvSynchronizerTag instance attached to the node
        RightFootHeel = 0x40, // same but with rage::mvSynchronizerTag::sm_RightFootHeel
    }

    public enum MrfValueType
    {
        None = 0,
        Literal = 1,   // specific value (in case of expressions, clips or filters, a dictionary/name hash pair)
        Parameter = 2, // lookup value in the network parameters
    }

    public enum MrfInfluenceOverride
    {
        None = 0, // influence affected by weight (at least in NodeBlend case)
        Zero = 1, // influence = 0.0
        One  = 2, // influence = 1.0
    }

    [TC(typeof(EXP))] public abstract class MrfNodePairWeightedBase : MrfNodePairBase
    {
        // rage::mvNodePairDef

        public MrfSynchronizerTagFlags SynchronizerTagFlags { get; set; }
        public float Weight { get; set; }
        public MetaHash WeightParameterName { get; set; }
        public MetaHash FrameFilterDictionaryName { get; set; }
        public MetaHash FrameFilterName { get; set; }
        public MetaHash FrameFilterParameterName { get; set; }

        // flags getters and setters
        public MrfValueType WeightType
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }
        public MrfValueType FrameFilterType
        {
            get => (MrfValueType)GetFlagsSubset(2, 3);
            set => SetFlagsSubset(2, 3, (uint)value);
        }
        public bool UnkFlag6 // Transitional? RDR3's rage::mvNodePairDef::GetTransitionalFlagFrom(uint) reads these bits
        {                    // always 0
            get => GetFlagsSubset(6, 1) != 0;
            set => SetFlagsSubset(6, 1, value ? 1 : 0u);
        }
        public uint UnkFlag7 // Immutable? RDR3's rage::mvNodePairDef::GetImmutableFlagFrom(uint) reads these bits
        {                    // 0 or 1
            get => GetFlagsSubset(7, 3);
            set => SetFlagsSubset(7, 3, value);
        }
        public MrfInfluenceOverride Child0InfluenceOverride
        {
            get => (MrfInfluenceOverride)GetFlagsSubset(12, 3);
            set => SetFlagsSubset(12, 3, (uint)value);
        }
        public MrfInfluenceOverride Child1InfluenceOverride
        {
            get => (MrfInfluenceOverride)GetFlagsSubset(14, 3);
            set => SetFlagsSubset(14, 3, (uint)value);
        }
        public MrfSynchronizerType SynchronizerType
        {
            get => (MrfSynchronizerType)GetFlagsSubset(19, 3);
            set => SetFlagsSubset(19, 3, (uint)value);
        }
        public uint UnkFlag21 // OutputParameterRuleSet? RDR3's rage::mvNodePairDef::GetOutputParameterRuleSetFrom(uint) reads these bits
        {                     // always 0
            get => GetFlagsSubset(21, 3);
            set => SetFlagsSubset(21, 3, value);
        }
        public uint UnkFlag23 // FirstFrameSyncOnly? RDR3's rage::mvNodePairDef::GetFirstFrameSyncOnlyFrom(uint) reads these bits
        {                     // always 0
            get => GetFlagsSubset(23, 3);
            set => SetFlagsSubset(23, 3, value);
        }
        public bool UnkFlag25 // SortTargets? RDR3's rage::mvNodePairDef::GetSortTargetsFrom(uint) reads these bits
        {                     // always 0
            get => GetFlagsSubset(25, 1) != 0;
            set => SetFlagsSubset(25, 1, value ? 1 : 0u);
        }
        public bool MergeBlend
        {
            get => GetFlagsSubset(31, 1) != 0;
            set => SetFlagsSubset(31, 1, value ? 1 : 0u);
        }

        public MrfNodePairWeightedBase(MrfNodeType type) : base(type) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            if (SynchronizerType == MrfSynchronizerType.Tag)
                SynchronizerTagFlags = (MrfSynchronizerTagFlags)r.ReadUInt32();

            switch (WeightType)
            {
                case MrfValueType.Literal:
                    Weight = r.ReadSingle();
                    break;
                case MrfValueType.Parameter:
                    WeightParameterName = r.ReadUInt32();
                    break;
            }

            switch (FrameFilterType)
            {
                case MrfValueType.Literal:
                    FrameFilterDictionaryName = r.ReadUInt32();
                    FrameFilterName = r.ReadUInt32();
                    break;
                case MrfValueType.Parameter:
                    FrameFilterParameterName = r.ReadUInt32();
                    break;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            if (SynchronizerType == MrfSynchronizerType.Tag)
                w.Write((uint)SynchronizerTagFlags);

            switch (WeightType)
            {
                case MrfValueType.Literal:
                    w.Write(Weight);
                    break;
                case MrfValueType.Parameter:
                    w.Write(WeightParameterName);
                    break;
            }

            switch (FrameFilterType)
            {
                case MrfValueType.Literal:
                    w.Write(FrameFilterDictionaryName);
                    w.Write(FrameFilterName);
                    break;
                case MrfValueType.Parameter:
                    w.Write(FrameFilterParameterName);
                    break;
            }
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            Child0InfluenceOverride = Xml.GetChildEnumInnerText<MrfInfluenceOverride>(node, "Child0InfluenceOverride");
            Child1InfluenceOverride = Xml.GetChildEnumInnerText<MrfInfluenceOverride>(node, "Child1InfluenceOverride");
            (WeightType, Weight, WeightParameterName) = XmlMrf.GetChildParameterizedFloat(node, "Weight");
            (FrameFilterType, FrameFilterDictionaryName, FrameFilterName, FrameFilterParameterName) =  XmlMrf.GetChildParameterizedAsset(node, "FrameFilter");
            SynchronizerType = Xml.GetChildEnumInnerText<MrfSynchronizerType>(node, "SynchronizerType");
            if (SynchronizerType == MrfSynchronizerType.Tag)
            {
                SynchronizerTagFlags = Xml.GetChildEnumInnerText<MrfSynchronizerTagFlags>(node, "SynchronizerTagFlags");
            }
            MergeBlend = Xml.GetChildBoolAttribute(node, "MergeBlend");
            UnkFlag6 = Xml.GetChildBoolAttribute(node, "UnkFlag6");
            UnkFlag7 = Xml.GetChildUIntAttribute(node, "UnkFlag7");
            UnkFlag21 = Xml.GetChildUIntAttribute(node, "UnkFlag21");
            UnkFlag23 = Xml.GetChildUIntAttribute(node, "UnkFlag23");
            UnkFlag25 = Xml.GetChildBoolAttribute(node, "UnkFlag25");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.StringTag(sb, indent, "Child0InfluenceOverride", Child0InfluenceOverride.ToString());
            MrfXml.StringTag(sb, indent, "Child1InfluenceOverride", Child1InfluenceOverride.ToString());
            MrfXml.ParameterizedFloatTag(sb, indent, "Weight", WeightType, Weight, WeightParameterName);
            MrfXml.ParameterizedAssetTag(sb, indent, "FrameFilter", FrameFilterType, FrameFilterDictionaryName, FrameFilterName, FrameFilterParameterName);
            MrfXml.StringTag(sb, indent, "SynchronizerType", SynchronizerType.ToString());
            if (SynchronizerType == MrfSynchronizerType.Tag)
            {
                MrfXml.StringTag(sb, indent, "SynchronizerTagFlags", SynchronizerTagFlags.ToString());
            }
            MrfXml.ValueTag(sb, indent, "MergeBlend", MergeBlend.ToString());
            MrfXml.ValueTag(sb, indent, "UnkFlag6", UnkFlag6.ToString());
            MrfXml.ValueTag(sb, indent, "UnkFlag7", UnkFlag7.ToString());
            MrfXml.ValueTag(sb, indent, "UnkFlag21", UnkFlag21.ToString());
            MrfXml.ValueTag(sb, indent, "UnkFlag23", UnkFlag23.ToString());
            MrfXml.ValueTag(sb, indent, "UnkFlag25", UnkFlag25.ToString());
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodeWithChildBase : MrfNodeWithFlagsBase
    {
        public int ChildOffset { get; set; }
        public int ChildFileOffset { get; set; }

        public MrfNode Child { get; set; }

        public MrfNodeWithChildBase(MrfNodeType type) : base(type) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            ChildOffset = r.ReadInt32();
            ChildFileOffset = (int)(r.Position + ChildOffset - 4);
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(ChildOffset);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Child = XmlMrf.ReadChildNode(node, "Child");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.WriteNode(sb, indent, "Child", Child);
        }

        public override void ResolveRelativeOffsets(MrfFile mrf)
        {
            base.ResolveRelativeOffsets(mrf);

            var node = mrf.FindNodeAtFileOffset(ChildFileOffset);
            if (node == null)
            { } // no hits

            Child = node;
        }

        public override void UpdateRelativeOffsets()
        {
            base.UpdateRelativeOffsets();

            ChildFileOffset = Child.FileOffset;
            ChildOffset = ChildFileOffset - (FileOffset + 0xC);
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodeWithChildAndFilterBase : MrfNodeWithChildBase
    {
        public MetaHash FrameFilterDictionaryName { get; set; }
        public MetaHash FrameFilterName { get; set; }
        public MetaHash FrameFilterParameterName { get; set; }

        // flags getters and setters
        public MrfValueType FrameFilterType
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }

        public MrfNodeWithChildAndFilterBase(MrfNodeType type) : base(type) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            switch (FrameFilterType)
            {
                case MrfValueType.Literal:
                    FrameFilterDictionaryName = r.ReadUInt32();
                    FrameFilterName = r.ReadUInt32();
                    break;
                case MrfValueType.Parameter:
                    FrameFilterParameterName = r.ReadUInt32();
                    break;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            switch (FrameFilterType)
            {
                case MrfValueType.Literal:
                    w.Write(FrameFilterDictionaryName);
                    w.Write(FrameFilterName);
                    break;
                case MrfValueType.Parameter:
                    w.Write(FrameFilterParameterName);
                    break;
            }
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            (FrameFilterType, FrameFilterDictionaryName, FrameFilterName, FrameFilterParameterName) = XmlMrf.GetChildParameterizedAsset(node, "FrameFilter");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.ParameterizedAssetTag(sb, indent, "FrameFilter", FrameFilterType, FrameFilterDictionaryName, FrameFilterName, FrameFilterParameterName);
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodeNBase : MrfNodeWithFlagsBase
    {
        // rage::mvNodeNDef

        public MrfSynchronizerTagFlags SynchronizerTagFlags { get; set; }
        public byte[] Unk2 { get; set; } // unused
        public MetaHash Unk2ParameterName { get; set; } // unused
        public MetaHash FrameFilterDictionaryName { get; set; }
        public MetaHash FrameFilterName { get; set; }
        public MetaHash FrameFilterParameterName { get; set; }
        public int[] ChildrenOffsets { get; set; }
        public int[] ChildrenFileOffsets { get; set; }
        public MrfNodeNChildData[] ChildrenData { get; set; }
        public uint[] ChildrenFlags { get; set; } // 8 bits per child

        public MrfNode[] Children { get; set; }

        // flags getters and setters
        public MrfValueType Unk2Type
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }
        public MrfValueType FrameFilterType
        {
            get => (MrfValueType)GetFlagsSubset(2, 3);
            set => SetFlagsSubset(2, 3, (uint)value);
        }
        public bool ZeroDestination // name is correct based on RDR3 symbols but not sure about its use
        {                           // only difference I found is when true the weight of child #0 returns 1.0 instead of the weight in ChildrenData
            get => GetFlagsSubset(4, 1) != 0;
            set => SetFlagsSubset(4, 1, value ? 1 : 0u);
        }
        public MrfSynchronizerType SynchronizerType
        {
            get => (MrfSynchronizerType)GetFlagsSubset(19, 3);
            set => SetFlagsSubset(19, 3, (uint)value);
        }
        public uint ChildrenCount
        {
            get => GetFlagsSubset(26, 0x3F);
            set => SetFlagsSubset(26, 0x3F, value);
        }
        
        // ChildrenFlags getters and setters
        public byte GetChildFlags(int index)
        {
            int blockIndex = 8 * index / 32;
            int bitOffset = 8 * index % 32;
            uint block = ChildrenFlags[blockIndex];
            return (byte)((block >> bitOffset) & 0xFF);
        }
        public void SetChildFlags(int index, byte flags)
        {
            int blockIndex = 8 * index / 32;
            int bitOffset = 8 * index % 32;
            uint block = ChildrenFlags[blockIndex];
            block = (block & ~(0xFFu << bitOffset)) | ((uint)flags << bitOffset);
            ChildrenFlags[blockIndex] = block;
        }
        public MrfValueType GetChildWeightType(int index)
        {
            return (MrfValueType)(GetChildFlags(index) & 3);
        }
        public void SetChildWeightType(int index, MrfValueType type)
        {
            var flags = GetChildFlags(index);
            flags = (byte)(flags & ~3u | ((uint)type & 3u));
            SetChildFlags(index, flags);
        }
        public MrfValueType GetChildFrameFilterType(int index)
        {
            return (MrfValueType)((GetChildFlags(index) >> 4) & 3);
        }
        public void SetChildFrameFilterType(int index, MrfValueType type)
        {
            var flags = GetChildFlags(index);
            flags = (byte)(flags & ~(3u << 4) | (((uint)type & 3u) << 4));
            SetChildFlags(index, flags);
        }

        public MrfNodeNBase(MrfNodeType type) : base(type) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            if (SynchronizerType == MrfSynchronizerType.Tag)
                SynchronizerTagFlags = (MrfSynchronizerTagFlags)r.ReadUInt32();

            switch (Unk2Type)
            {
                case MrfValueType.Literal:
                    Unk2 = r.ReadBytes(76); // Unused?
                    break;
                case MrfValueType.Parameter:
                    Unk2ParameterName = r.ReadUInt32();
                    break;
            }

            switch (FrameFilterType)
            {
                case MrfValueType.Literal:
                    FrameFilterDictionaryName = r.ReadUInt32();
                    FrameFilterName = r.ReadUInt32();
                    break;
                case MrfValueType.Parameter:
                    FrameFilterParameterName = r.ReadUInt32();
                    break;
            }

            var childrenCount = ChildrenCount;
            if (childrenCount > 0)
            {
                ChildrenOffsets = new int[childrenCount];
                ChildrenFileOffsets = new int[childrenCount];

                for (int i = 0; i < childrenCount; i++)
                {
                    ChildrenOffsets[i] = r.ReadInt32();
                    ChildrenFileOffsets[i] = (int)(r.Position + ChildrenOffsets[i] - 4);
                }
            }

            var childrenFlagsBlockCount = childrenCount * 8 / 32 + 1;

            if (childrenFlagsBlockCount > 0)
            {
                ChildrenFlags = new uint[childrenFlagsBlockCount];

                for (int i = 0; i < childrenFlagsBlockCount; i++)
                    ChildrenFlags[i] = r.ReadUInt32();
            }

            if (ChildrenCount == 0)
                return;

            ChildrenData = new MrfNodeNChildData[childrenCount];

            for (int i = 0; i < childrenCount; i++)
            {
                var item = new MrfNodeNChildData();

                switch (GetChildWeightType(i))
                {
                    case MrfValueType.Literal:
                        item.Weight = r.ReadSingle();
                        break;
                    case MrfValueType.Parameter:
                        item.WeightParameterName = r.ReadUInt32();
                        break;
                }

                switch (GetChildFrameFilterType(i))
                {
                    case MrfValueType.Literal:
                        item.FrameFilterDictionaryName = r.ReadUInt32();
                        item.FrameFilterName = r.ReadUInt32();
                        break;
                    case MrfValueType.Parameter:
                        item.FrameFilterParameterName = r.ReadUInt32();
                        break;
                }

                ChildrenData[i] = item;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            if (SynchronizerType == MrfSynchronizerType.Tag)
                w.Write((uint)SynchronizerTagFlags);

            switch (Unk2Type)
            {
                case MrfValueType.Literal:
                    w.Write(Unk2);
                    break;
                case MrfValueType.Parameter:
                    w.Write(Unk2ParameterName);
                    break;
            }

            switch (FrameFilterType)
            {
                case MrfValueType.Literal:
                    w.Write(FrameFilterDictionaryName);
                    w.Write(FrameFilterName);
                    break;
                case MrfValueType.Parameter:
                    w.Write(FrameFilterParameterName);
                    break;
            }

            var childrenCount = ChildrenCount;
            if (childrenCount > 0)
            {
                foreach (var value in ChildrenOffsets)
                    w.Write(value);
            }

            var childrenFlagsBlockCount = childrenCount * 8 / 32 + 1;

            if (childrenFlagsBlockCount > 0)
            {
                foreach (var value in ChildrenFlags)
                    w.Write(value);
            }

            if (childrenCount == 0)
                return;

            for (int i = 0; i < childrenCount; i++)
            {
                var item = ChildrenData[i];

                switch (GetChildWeightType(i))
                {
                    case MrfValueType.Literal:
                        w.Write(item.Weight);
                        break;
                    case MrfValueType.Parameter:
                        w.Write(item.WeightParameterName);
                        break;
                }

                switch (GetChildFrameFilterType(i))
                {
                    case MrfValueType.Literal:
                        w.Write(item.FrameFilterDictionaryName);
                        w.Write(item.FrameFilterName);
                        break;
                    case MrfValueType.Parameter:
                        w.Write(item.FrameFilterParameterName);
                        break;
                }
            }
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            (FrameFilterType, FrameFilterDictionaryName, FrameFilterName, FrameFilterParameterName) = XmlMrf.GetChildParameterizedAsset(node, "FrameFilter");
            SynchronizerType = Xml.GetChildEnumInnerText<MrfSynchronizerType>(node, "SynchronizerType");
            if (SynchronizerType == MrfSynchronizerType.Tag)
            {
                SynchronizerTagFlags = Xml.GetChildEnumInnerText<MrfSynchronizerTagFlags>(node, "SynchronizerTagFlags");
            }
            ZeroDestination = Xml.GetChildBoolAttribute(node, "ZeroDestination");
            Children = null;
            ChildrenData = null;
            ChildrenFlags = null;
            ChildrenOffsets = null;
            ChildrenCount = 0;
            var statesNode = node.SelectSingleNode("Children");
            if (statesNode != null)
            {
                var inodes = statesNode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    ChildrenCount = (uint)inodes.Count;
                    Children = new MrfNode[ChildrenCount];
                    ChildrenData = new MrfNodeNChildData[ChildrenCount];
                    ChildrenFlags = new uint[ChildrenCount * 8 / 32 + 1];
                    ChildrenOffsets = new int[ChildrenCount];
                    int i = 0;
                    foreach (XmlNode inode in inodes)
                    {
                        var weight = XmlMrf.GetChildParameterizedFloat(inode, "Weight");
                        var filter = XmlMrf.GetChildParameterizedAsset(inode, "FrameFilter");

                        ChildrenData[i].Weight = weight.Value;
                        ChildrenData[i].WeightParameterName = weight.ParameterName;
                        SetChildWeightType(i, weight.Type);
                        ChildrenData[i].FrameFilterDictionaryName = filter.DictionaryName;
                        ChildrenData[i].FrameFilterName = filter.AssetName;
                        ChildrenData[i].FrameFilterParameterName = filter.ParameterName;
                        SetChildFrameFilterType(i, filter.Type);
                        Children[i] = XmlMrf.ReadChildNode(inode, "Node");
                        i++;
                    }
                }
            }
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            Unk2Type = MrfValueType.None;
            Unk2 = null;
            Unk2ParameterName = 0;

            MrfXml.ParameterizedAssetTag(sb, indent, "FrameFilter", FrameFilterType, FrameFilterDictionaryName, FrameFilterName, FrameFilterParameterName);
            MrfXml.StringTag(sb, indent, "SynchronizerType", SynchronizerType.ToString());
            if (SynchronizerType == MrfSynchronizerType.Tag)
            {
                MrfXml.StringTag(sb, indent, "SynchronizerTagFlags", SynchronizerTagFlags.ToString());
            }
            MrfXml.ValueTag(sb, indent, "ZeroDestination", ZeroDestination.ToString());
            int cindent = indent + 1;
            int cindent2 = cindent + 1;
            int childIndex = 0;
            MrfXml.OpenTag(sb, indent, "Children");
            foreach (var child in Children)
            {
                var childData = ChildrenData[childIndex];
                MrfXml.OpenTag(sb, cindent, "Item");
                MrfXml.ParameterizedFloatTag(sb, cindent2, "Weight", GetChildWeightType(childIndex), childData.Weight, childData.WeightParameterName);
                MrfXml.ParameterizedAssetTag(sb, cindent2, "FrameFilter", GetChildFrameFilterType(childIndex), childData.FrameFilterDictionaryName, childData.FrameFilterName, childData.FrameFilterParameterName);
                MrfXml.WriteNode(sb, cindent2, "Node", child);
                MrfXml.CloseTag(sb, cindent, "Item");
                childIndex++;
            }
            MrfXml.CloseTag(sb, indent, "Children");
        }

        public override void ResolveRelativeOffsets(MrfFile mrf)
        {
            base.ResolveRelativeOffsets(mrf);

            if (ChildrenFileOffsets != null)
            {
                Children = new MrfNode[ChildrenFileOffsets.Length];
                for (int i = 0; i < ChildrenFileOffsets.Length; i++)
                {
                    var node = mrf.FindNodeAtFileOffset(ChildrenFileOffsets[i]);
                    if (node == null)
                    { } // no hits

                    Children[i] = node;
                }
            }
        }

        public override void UpdateRelativeOffsets()
        {
            base.UpdateRelativeOffsets();

            var offset = FileOffset + 0xC/*sizeof(MrfNodeWithFlagsBase)*/;
            offset += SynchronizerType == MrfSynchronizerType.Tag ? 4 : 0;
            offset += Unk2Type == MrfValueType.Literal ? 76 : 0;
            offset += Unk2Type == MrfValueType.Parameter ? 4 : 0;
            offset += FrameFilterType == MrfValueType.Literal ? 8 : 0;
            offset += FrameFilterType == MrfValueType.Parameter ? 4 : 0;
            
            if (Children != null)
            {
                ChildrenOffsets = new int[Children.Length];
                ChildrenFileOffsets = new int[Children.Length];
                for (int i = 0; i < Children.Length; i++)
                {
                    var node = Children[i];
                    ChildrenFileOffsets[i] = node.FileOffset;
                    ChildrenOffsets[i] = node.FileOffset - offset;
                    offset += 4;
                }
            }
        }
    }
    
    [TC(typeof(EXP))] public struct MrfNodeNChildData
    {
        public float Weight { get; set; }
        public MetaHash WeightParameterName { get; set; }
        public MetaHash FrameFilterDictionaryName { get; set; }
        public MetaHash FrameFilterName { get; set; }
        public MetaHash FrameFilterParameterName { get; set; }

        public override string ToString()
        {
            return $"{FloatUtil.ToString(Weight)} - {WeightParameterName} - {FrameFilterDictionaryName} - {FrameFilterName} - {FrameFilterParameterName}";
        }
    }



#endregion

#region mrf node structs

    [TC(typeof(EXP))] public class MrfHeaderUnk1 : IMetaXmlItem
    {
        public uint Size { get; set; }
        public byte[] Bytes { get; set; }

        public MrfHeaderUnk1()
        {
        }

        public MrfHeaderUnk1(DataReader r)
        {
            Size = r.ReadUInt32();
            Bytes = r.ReadBytes((int)Size);
        }

        public void Write(DataWriter w)
        {
            w.Write(Size);
            w.Write(Bytes);
        }

        public void ReadXml(XmlNode node)
        {
            Bytes = Xml.GetChildRawByteArrayNullable(node, "Bytes");
            Size = (uint)(Bytes?.Length ?? 0);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.WriteRawArray(sb, Bytes, indent, "Bytes", "", MrfXml.FormatHexByte, 16);
        }

        public override string ToString()
        {
            return Size.ToString() + " bytes";
        }
    }

    /// <summary>
    /// If used as <see cref="MrfFile.MoveNetworkTriggers"/>:
    /// Parameter that can be triggered by the game to control transitions.
    /// Only active for 1 tick.
    /// The native `REQUEST_TASK_MOVE_NETWORK_STATE_TRANSITION` uses these triggers but appends "request" to the passed string,
    /// e.g. `REQUEST_TASK_MOVE_NETWORK_STATE_TRANSITION(ped, "running")` will trigger "runningrequest".
    /// <para>
    /// If used as <see cref="MrfFile.MoveNetworkFlags"/>:
    /// Parameter that can be toggled by the game to control transitions.
    /// Can be enabled with fwClipSet.moveNetworkFlags too (seems like only if the game uses it as a MrfClipContainerType.VariableClipSet).
    /// </para>
    /// </summary>
    [TC(typeof(EXP))] public struct MrfMoveNetworkBit : IMetaXmlItem
    {
        public static MrfMoveNetworkBit EndMarker => new MrfMoveNetworkBit { Name = 0xFFFFFFFF, BitPosition = 0 };

        public MetaHash Name { get; set; }
        public int BitPosition { get; set; }

        public bool IsEndMarker => Name == 0xFFFFFFFF;

        public MrfMoveNetworkBit(DataReader r)
        {
            Name = r.ReadUInt32();
            BitPosition = r.ReadInt32();
        }

        public void Write(DataWriter w)
        {
            w.Write(Name);
            w.Write(BitPosition);
        }

        public override string ToString()
        {
            return IsEndMarker ? "--- end marker ---" : $"{Name} - {BitPosition}";
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.StringTag(sb, indent, "Name", MrfXml.HashString(Name));
            MrfXml.ValueTag(sb, indent, "BitPosition", BitPosition.ToString());
        }

        public void ReadXml(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            BitPosition = Xml.GetChildIntAttribute(node, "BitPosition");
        }
    }

    public enum MrfWeightModifierType
    {
        SlowInSlowOut = 0,
        SlowOut = 1,
        SlowIn = 2,
        None = 3,
    }

    [TC(typeof(EXP))] public class MrfStateTransition : IMetaXmlItem
    {
        // rage::mvTransitionDef

        public uint Flags { get; set; }
        public MrfSynchronizerTagFlags SynchronizerTagFlags { get; set; }
        public float Duration { get; set; } // time in seconds it takes for the transition to blend between the source and target states
        public MetaHash DurationParameterName { get; set; }
        public MetaHash ProgressParameterName { get; set; } // parameter where to store the transition progress percentage (0.0 to 1.0)
        public int TargetStateOffset { get; set; } // offset from the start of this field
        public int TargetStateFileOffset { get; set; }
        public MetaHash FrameFilterDictionaryName { get; set; }
        public MetaHash FrameFilterName { get; set; }
        public MrfCondition[] Conditions { get; set; }

        public MrfNodeStateBase TargetState { get; set; }

        // flags getters and setters
        public bool HasProgressParameter // if set, the transition progress percentage (0.0 to 1.0) is stored in ProgressParameterName
        {
            get => GetFlagsSubset(1, 1) != 0;
            set => SetFlagsSubset(1, 1, value ? 1 : 0u);
        }
        public bool UnkFlag2_DetachUpdateObservers // if set, executes rage::DetachUpdateObservers on the source state
        {
            get => GetFlagsSubset(2, 1) != 0;
            set => SetFlagsSubset(2, 1, value ? 1 : 0u);
        }
        public bool HasDurationParameter // if set use DurationParameterName instead of Duration. Duration is used as default if the paramter is not found
        {
            get => GetFlagsSubset(3, 1) != 0;
            set => SetFlagsSubset(3, 1, value ? 1 : 0u);
        }
        public uint DataSize // number of bytes this transition takes, used to iterate the transitions array
        {
            get => GetFlagsSubset(4, 0x3FFF); 
            set => SetFlagsSubset(4, 0x3FFF, value);
        }
        public bool UnkFlag18
        {
            get => GetFlagsSubset(18, 1) != 0;
            set => SetFlagsSubset(18, 1, value ? 1 : 0u);
        }
        public bool UnkFlag19
        {
            get => GetFlagsSubset(19, 1) != 0;
            set => SetFlagsSubset(19, 1, value ? 1 : 0u);
        }
        public uint ConditionCount
        {
            get => GetFlagsSubset(20, 0xF);
            set => SetFlagsSubset(20, 0xF, value);
        }
        public MrfWeightModifierType BlendModifier // modifier for the blend between source and target states
        {
            get => (MrfWeightModifierType)GetFlagsSubset(24, 7);
            set => SetFlagsSubset(24, 7, (uint)value);
        }
        public MrfSynchronizerType SynchronizerType
        {
            get => (MrfSynchronizerType)GetFlagsSubset(28, 3);
            set => SetFlagsSubset(28, 3, (uint)value);
        }
        public bool HasFrameFilter
        {
            get => GetFlagsSubset(30, 1) != 0;
            set => SetFlagsSubset(30, 1, value ? 1 : 0u);
        }

        [System.ComponentModel.Browsable(false)]
        public MetaHash XmlTargetStateName { get; set; } // for XML loading

        public MrfStateTransition()
        {
        }

        public MrfStateTransition(DataReader r)
        {
            var startReadPosition = r.Position;

            Flags = r.ReadUInt32();
            SynchronizerTagFlags = (MrfSynchronizerTagFlags)r.ReadUInt32();
            Duration = r.ReadSingle();
            DurationParameterName = r.ReadUInt32();
            ProgressParameterName = r.ReadUInt32();
            TargetStateOffset = r.ReadInt32();
            TargetStateFileOffset = (int)(r.Position + TargetStateOffset - 4);

            if (ConditionCount > 0)
            {
                Conditions = new MrfCondition[ConditionCount];
                for (int i = 0; i < ConditionCount; i++)
                {
                    var startPos = r.Position;
                    var conditionType = (MrfConditionType)r.ReadUInt16();
                    r.Position = startPos;
                    
                    MrfCondition cond;
                    switch (conditionType)
                    {
                        case MrfConditionType.ParameterInsideRange:    cond = new MrfConditionParameterInsideRange(r); break;
                        case MrfConditionType.ParameterOutsideRange:   cond = new MrfConditionParameterOutsideRange(r); break;
                        case MrfConditionType.MoveNetworkTrigger:      cond = new MrfConditionMoveNetworkTrigger(r); break;
                        case MrfConditionType.MoveNetworkFlag:         cond = new MrfConditionMoveNetworkFlag(r); break;
                        case MrfConditionType.EventOccurred:           cond = new MrfConditionEventOccurred(r); break;
                        case MrfConditionType.ParameterGreaterThan:    cond = new MrfConditionParameterGreaterThan(r); break;
                        case MrfConditionType.ParameterGreaterOrEqual: cond = new MrfConditionParameterGreaterOrEqual(r); break;
                        case MrfConditionType.ParameterLessThan:       cond = new MrfConditionParameterLessThan(r); break;
                        case MrfConditionType.ParameterLessOrEqual:    cond = new MrfConditionParameterLessOrEqual(r); break;
                        case MrfConditionType.TimeGreaterThan:         cond = new MrfConditionTimeGreaterThan(r); break;
                        case MrfConditionType.TimeLessThan:            cond = new MrfConditionTimeLessThan(r); break;
                        case MrfConditionType.BoolParameterExists:     cond = new MrfConditionBoolParameterExists(r); break;
                        case MrfConditionType.BoolParameterEquals:     cond = new MrfConditionBoolParameterEquals(r); break;
                        default: throw new Exception($"Unknown condition type ({conditionType})");
                    }

                    Conditions[i] = cond;
                }
            }

            if (HasFrameFilter)
            {
                FrameFilterDictionaryName = r.ReadUInt32();
                FrameFilterName = r.ReadUInt32();
            }
            else
            {
                FrameFilterDictionaryName = 0;
                FrameFilterName = 0;
            }

            if ((r.Position - startReadPosition) != DataSize)
            { } // not hits
        }

        public void Write(DataWriter w)
        {
            ConditionCount = (uint)(Conditions?.Length ?? 0);

            w.Write(Flags);
            w.Write((uint)SynchronizerTagFlags);
            w.Write(Duration);
            w.Write(DurationParameterName);
            w.Write(ProgressParameterName);
            w.Write(TargetStateOffset);

            if (Conditions != null)
                for (int i = 0; i < Conditions.Length; i++)
                    Conditions[i].Write(w);

            if (HasFrameFilter)
            {
                w.Write(FrameFilterDictionaryName);
                w.Write(FrameFilterName);
            }
        }

        public void ReadXml(XmlNode node)
        {
            XmlTargetStateName = XmlMrf.ReadChildNodeRef(node, "TargetState");
            Duration = Xml.GetChildFloatAttribute(node, "Duration");
            DurationParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "DurationParameterName"));
            ProgressParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "ProgressParameterName"));
            HasDurationParameter = DurationParameterName != 0;
            HasProgressParameter = ProgressParameterName != 0;
            BlendModifier = Xml.GetChildEnumInnerText<MrfWeightModifierType>(node, "BlendModifier");

            SynchronizerType = Xml.GetChildEnumInnerText<MrfSynchronizerType>(node, "SynchronizerType");
            if (SynchronizerType == MrfSynchronizerType.Tag)
            {
                SynchronizerTagFlags = Xml.GetChildEnumInnerText<MrfSynchronizerTagFlags>(node, "SynchronizerTagFlags");
            }
            else
            {
                SynchronizerTagFlags = (MrfSynchronizerTagFlags)0xFFFFFFFF;
            }
            
            var filter = XmlMrf.GetChildParameterizedAsset(node, "FrameFilter");
            if (filter.Type == MrfValueType.Literal)
            {
                HasFrameFilter = true;
                FrameFilterDictionaryName = filter.DictionaryName;
                FrameFilterName = filter.AssetName;
            }
            else
            {
                HasFrameFilter = false;
                FrameFilterDictionaryName = 0;
                FrameFilterName = 0;
            }
            
            UnkFlag2_DetachUpdateObservers = Xml.GetChildBoolAttribute(node, "UnkFlag2_DetachUpdateObservers");
            UnkFlag18 = Xml.GetChildBoolAttribute(node, "UnkFlag18");
            UnkFlag19 = Xml.GetChildBoolAttribute(node, "UnkFlag19");

            Conditions = null;
            var conditionsNode = node.SelectSingleNode("Conditions");
            if (conditionsNode != null)
            {
                var inodes = conditionsNode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    Conditions = new MrfCondition[inodes.Count];
                    int i = 0;
                    foreach (XmlNode inode in inodes)
                    {
                        Conditions[i] = XmlMrf.ReadCondition(inode);
                        i++;
                    }
                }
            }

            CalculateDataSize();
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            //MrfXml.ValueTag(sb, indent, "Flags", Flags.ToString());
            MrfXml.WriteNodeRef(sb, indent, "TargetState", TargetState);
            MrfXml.ValueTag(sb, indent, "Duration", FloatUtil.ToString(Duration));
            MrfXml.StringTag(sb, indent, "DurationParameterName", HasDurationParameter ? MrfXml.HashString(DurationParameterName) : null);
            MrfXml.StringTag(sb, indent, "ProgressParameterName", HasProgressParameter ? MrfXml.HashString(ProgressParameterName) : null);
            MrfXml.StringTag(sb, indent, "BlendModifier", BlendModifier.ToString());

            MrfXml.StringTag(sb, indent, "SynchronizerType", SynchronizerType.ToString());
            if (SynchronizerType == MrfSynchronizerType.Tag)
            {
                MrfXml.StringTag(sb, indent, "SynchronizerTagFlags", SynchronizerTagFlags.ToString());
            }

            if (HasFrameFilter)
            {
                MrfXml.ParameterizedAssetTag(sb, indent, "FrameFilter", MrfValueType.Literal, FrameFilterDictionaryName, FrameFilterName, 0);
            }
            else
            {
                MrfXml.SelfClosingTag(sb, indent, "FrameFilter");
            }

            MrfXml.ValueTag(sb, indent, "UnkFlag2_DetachUpdateObservers", UnkFlag2_DetachUpdateObservers.ToString());
            MrfXml.ValueTag(sb, indent, "UnkFlag18", UnkFlag18.ToString());
            MrfXml.ValueTag(sb, indent, "UnkFlag19", UnkFlag19.ToString());
            
            if (Conditions != null)
            {
                int cindent = indent + 1;
                MrfXml.OpenTag(sb, indent, "Conditions");
                foreach (var c in Conditions)
                {
                    MrfXml.WriteCondition(sb, cindent, "Item", c);
                }
                MrfXml.CloseTag(sb, indent, "Conditions");
            }
            else
            {
                MrfXml.SelfClosingTag(sb, indent, "Conditions");
            }
        }

        public uint GetFlagsSubset(int bitOffset, uint mask)
        {
            return (Flags >> bitOffset) & mask;
        }

        public void SetFlagsSubset(int bitOffset, uint mask, uint value)
        {
            Flags = (Flags & ~(mask << bitOffset)) | ((value & mask) << bitOffset);
        }

        public void CalculateDataSize()
        {
            uint dataSize = 0x18;
            if (Conditions != null)
            {
                dataSize += (uint)Conditions.Sum(c => c.DataSize);
            }

            if (HasFrameFilter)
            {
                dataSize += 8;
            }

            DataSize = dataSize;
        }

        public override string ToString()
        {
            return $"{TargetState?.Name.ToString() ?? TargetStateFileOffset.ToString()} - {FloatUtil.ToString(Duration)} - {Conditions?.Length ?? 0} conditions";
        }
    }

    public enum MrfConditionType : ushort
    {
        ParameterInsideRange = 0,     // condition = Param > MinValue && Param < MaxValue
        ParameterOutsideRange = 1,    // condition = Param < MinValue || Param > MaxValue
        MoveNetworkTrigger = 2,       // condition = bittest(rage::mvMotionWeb.field_8, BitPosition) != Invert (each bit of field_8 represents a MrfMoveNetworkTrigger)
        MoveNetworkFlag = 3,          // condition = bittest(rage::mvMotionWeb.field_C, BitPosition) != Invert (each bit of field_C represents a MrfMoveNetworkFlag)
        EventOccurred = 4,            // condition = same behaviour as BoolParamExists but seems to be used with event names only
        ParameterGreaterThan = 5,     // condition = Param > Value
        ParameterGreaterOrEqual = 6,  // condition = Param >= Value
        ParameterLessThan = 7,        // condition = Param < Value
        ParameterLessOrEqual = 8,     // condition = Param <= Value
        TimeGreaterThan = 9,          // condition = Time > Value (time since tha state started in seconds)
        TimeLessThan = 10,            // condition = Time < Value
        BoolParameterExists = 11,     // condition = exists(Param) != Invert
        BoolParameterEquals = 12,     // condition = Param == Value
    }

    [TC(typeof(EXP))] public abstract class MrfCondition : IMetaXmlItem
    {
        // rage::mvConditionDef

        public MrfConditionType Type { get; set; }
        public short Unk2 { get; set; } = 0; // always 0

        public abstract uint DataSize { get; }

        public MrfCondition(MrfConditionType type)
        {
            Type = type;
        }

        public MrfCondition(DataReader r)
        {
            Type = (MrfConditionType)r.ReadUInt16();
            Unk2 = r.ReadInt16();
        }

        public virtual void Write(DataWriter w)
        {
            w.Write((ushort)Type);
            w.Write(Unk2);
        }

        public virtual void WriteXml(StringBuilder sb, int indent)
        {
        }

        public virtual void ReadXml(XmlNode node)
        {
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        /// <summary>
        /// Returns the condition as a C-like expression. Mainly to include it in the debug DOT graphs.
        /// </summary>
        public abstract string ToExpressionString(MrfFile mrf);

        public static MrfCondition CreateCondition(MrfConditionType conditionType)
        {
            switch (conditionType)
            {
                case MrfConditionType.ParameterInsideRange:    return new MrfConditionParameterInsideRange();
                case MrfConditionType.ParameterOutsideRange:   return new MrfConditionParameterOutsideRange();
                case MrfConditionType.MoveNetworkTrigger:      return new MrfConditionMoveNetworkTrigger();
                case MrfConditionType.MoveNetworkFlag:         return new MrfConditionMoveNetworkFlag();
                case MrfConditionType.EventOccurred:           return new MrfConditionEventOccurred();
                case MrfConditionType.ParameterGreaterThan:    return new MrfConditionParameterGreaterThan();
                case MrfConditionType.ParameterGreaterOrEqual: return new MrfConditionParameterGreaterOrEqual();
                case MrfConditionType.ParameterLessThan:       return new MrfConditionParameterLessThan();
                case MrfConditionType.ParameterLessOrEqual:    return new MrfConditionParameterLessOrEqual();
                case MrfConditionType.TimeGreaterThan:         return new MrfConditionTimeGreaterThan();
                case MrfConditionType.TimeLessThan:            return new MrfConditionTimeLessThan();
                case MrfConditionType.BoolParameterExists:     return new MrfConditionBoolParameterExists();
                case MrfConditionType.BoolParameterEquals:     return new MrfConditionBoolParameterEquals();
                default: throw new Exception($"Unknown condition type ({conditionType})");
            }
        }
    }
    [TC(typeof(EXP))] public abstract class MrfConditionWithParameterAndRangeBase : MrfCondition
    {
        public override uint DataSize => 16;
        public MetaHash ParameterName { get; set; }
        public float MaxValue { get; set; }
        public float MinValue { get; set; }

        public MrfConditionWithParameterAndRangeBase(MrfConditionType type) : base(type) { }
        public MrfConditionWithParameterAndRangeBase(DataReader r) : base(r)
        {
            ParameterName = r.ReadUInt32();
            MaxValue = r.ReadSingle();
            MinValue = r.ReadSingle();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(ParameterName);
            w.Write(MaxValue);
            w.Write(MinValue);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            ParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "ParameterName"));
            MinValue = Xml.GetChildFloatAttribute(node, "Min");
            MaxValue = Xml.GetChildFloatAttribute(node, "Max");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.StringTag(sb, indent, "ParameterName", MrfXml.HashString(ParameterName));
            MrfXml.ValueTag(sb, indent, "Min", FloatUtil.ToString(MinValue));
            MrfXml.ValueTag(sb, indent, "Max", FloatUtil.ToString(MaxValue));
        }

        public override string ToString()
        {
            return base.ToString() + $" - {{ {nameof(ParameterName)} = {ParameterName}, {nameof(MaxValue)} = {FloatUtil.ToString(MaxValue)}, {nameof(MinValue)} = {FloatUtil.ToString(MinValue)} }}";
        }
    }
    [TC(typeof(EXP))] public abstract class MrfConditionWithParameterAndValueBase : MrfCondition
    {
        public override uint DataSize => 12;
        public MetaHash ParameterName { get; set; }
        public float Value { get; set; }

        public MrfConditionWithParameterAndValueBase(MrfConditionType type) : base(type) { }
        public MrfConditionWithParameterAndValueBase(DataReader r) : base(r)
        {
            ParameterName = r.ReadUInt32();
            Value = r.ReadSingle();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(ParameterName);
            w.Write(Value);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            ParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "ParameterName"));
            Value = Xml.GetChildFloatAttribute(node, "Value");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.StringTag(sb, indent, "ParameterName", MrfXml.HashString(ParameterName));
            MrfXml.ValueTag(sb, indent, "Value", FloatUtil.ToString(Value));
        }

        public override string ToString()
        {
            return base.ToString() + $" - {{ {nameof(ParameterName)} = {ParameterName}, {nameof(Value)} = {FloatUtil.ToString(Value)} }}";
        }
    }
    [TC(typeof(EXP))] public abstract class MrfConditionWithValueBase : MrfCondition
    {
        public override uint DataSize => 8;
        public float Value { get; set; }

        public MrfConditionWithValueBase(MrfConditionType type) : base(type) { }
        public MrfConditionWithValueBase(DataReader r) : base(r)
        {
            Value = r.ReadSingle();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(Value);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Value = Xml.GetChildFloatAttribute(node, "Value");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.ValueTag(sb, indent, "Value", FloatUtil.ToString(Value));
        }

        public override string ToString()
        {
            return base.ToString() + $" - {{ {nameof(Value)} = {FloatUtil.ToString(Value)} }}";
        }
    }
    [TC(typeof(EXP))] public abstract class MrfConditionWithParameterAndBoolValueBase : MrfCondition
    {
        public override uint DataSize => 12;
        public MetaHash ParameterName { get; set; }
        public bool Value { get; set; }

        public MrfConditionWithParameterAndBoolValueBase(MrfConditionType type) : base(type) { }
        public MrfConditionWithParameterAndBoolValueBase(DataReader r) : base(r)
        {
            ParameterName = r.ReadUInt32();
            Value = r.ReadUInt32() != 0;
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(ParameterName);
            w.Write(Value ? 1 : 0u);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            ParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "ParameterName"));
            Value = Xml.GetChildBoolAttribute(node, "Value");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.StringTag(sb, indent, "ParameterName", MrfXml.HashString(ParameterName));
            MrfXml.ValueTag(sb, indent, "Value", Value.ToString());
        }

        public override string ToString()
        {
            return base.ToString() + $" - {{ {nameof(ParameterName)} = {ParameterName}, {nameof(Value)} = {Value} }}";
        }
    }
    [TC(typeof(EXP))] public abstract class MrfConditionBitTestBase : MrfCondition
    {
        public override uint DataSize => 12;
        public int BitPosition { get; set; }
        public bool Invert { get; set; }

        public MrfConditionBitTestBase(MrfConditionType type) : base(type) { }
        public MrfConditionBitTestBase(DataReader r) : base(r)
        {
            BitPosition = r.ReadInt32();
            Invert = r.ReadUInt32() != 0;
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(BitPosition);
            w.Write(Invert ? 1u : 0u);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            BitPosition = Xml.GetChildIntAttribute(node, "BitPosition");
            Invert = Xml.GetChildBoolAttribute(node, "Invert");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.ValueTag(sb, indent, "BitPosition", BitPosition.ToString());
            MrfXml.ValueTag(sb, indent, "Invert", Invert.ToString());
        }

        public string FindBitName(MrfFile mrf)
        {
            MetaHash? bitNameHash = null;
            if (mrf != null)
            {
                bitNameHash = Type == MrfConditionType.MoveNetworkTrigger ?
                                    mrf.FindMoveNetworkTriggerForBit(BitPosition)?.Name :
                                    mrf.FindMoveNetworkFlagForBit(BitPosition)?.Name;
            }
            return bitNameHash.HasValue ? $"'{bitNameHash.Value}'" : BitPosition.ToString();
        }

        public override string ToString()
        {
            return base.ToString() + $" - {{ {nameof(BitPosition)} = {BitPosition}, {nameof(Invert)} = {Invert} }}";
        }
    }

    [TC(typeof(EXP))] public class MrfConditionParameterInsideRange : MrfConditionWithParameterAndRangeBase
    {
        public MrfConditionParameterInsideRange() : base(MrfConditionType.ParameterInsideRange) { }
        public MrfConditionParameterInsideRange(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return $"{FloatUtil.ToString(MinValue)} < '{ParameterName}' < {FloatUtil.ToString(MaxValue)}";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionParameterOutsideRange : MrfConditionWithParameterAndRangeBase
    {
        public MrfConditionParameterOutsideRange() : base(MrfConditionType.ParameterOutsideRange) { }
        public MrfConditionParameterOutsideRange(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return $"'{ParameterName}' < {FloatUtil.ToString(MinValue)} < {FloatUtil.ToString(MaxValue)} < '{ParameterName}'";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionMoveNetworkTrigger : MrfConditionBitTestBase
    {
        public MrfConditionMoveNetworkTrigger() : base(MrfConditionType.MoveNetworkTrigger) { }
        public MrfConditionMoveNetworkTrigger(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return (Invert ? "!" : "") + $"trigger({FindBitName(mrf)})";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionMoveNetworkFlag : MrfConditionBitTestBase
    {
        public MrfConditionMoveNetworkFlag() : base(MrfConditionType.MoveNetworkFlag) { }
        public MrfConditionMoveNetworkFlag(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return (Invert ? "!" : "") + $"flag({FindBitName(mrf)})";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionParameterGreaterThan : MrfConditionWithParameterAndValueBase
    {
        public MrfConditionParameterGreaterThan() : base(MrfConditionType.ParameterGreaterThan) { }
        public MrfConditionParameterGreaterThan(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return $"'{ParameterName}' > {FloatUtil.ToString(Value)}";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionParameterGreaterOrEqual : MrfConditionWithParameterAndValueBase
    {
        public MrfConditionParameterGreaterOrEqual() : base(MrfConditionType.ParameterGreaterOrEqual) { }
        public MrfConditionParameterGreaterOrEqual(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return $"'{ParameterName}' >= {FloatUtil.ToString(Value)}";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionParameterLessThan : MrfConditionWithParameterAndValueBase
    {
        public MrfConditionParameterLessThan() : base(MrfConditionType.ParameterLessThan) { }
        public MrfConditionParameterLessThan(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return $"'{ParameterName}' < {FloatUtil.ToString(Value)}";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionParameterLessOrEqual : MrfConditionWithParameterAndValueBase
    {
        public MrfConditionParameterLessOrEqual() : base(MrfConditionType.ParameterLessOrEqual) { }
        public MrfConditionParameterLessOrEqual(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return $"'{ParameterName}' <= {FloatUtil.ToString(Value)}";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionTimeGreaterThan : MrfConditionWithValueBase
    {
        public MrfConditionTimeGreaterThan() : base(MrfConditionType.TimeGreaterThan) { }
        public MrfConditionTimeGreaterThan(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return $"Time > {FloatUtil.ToString(Value)}";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionTimeLessThan : MrfConditionWithValueBase
    {
        public MrfConditionTimeLessThan() : base(MrfConditionType.TimeLessThan) { }
        public MrfConditionTimeLessThan(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return $"Time < {FloatUtil.ToString(Value)}";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionEventOccurred : MrfConditionWithParameterAndBoolValueBase
    {
        public bool Invert { get => Value; set => Value = value; }

        public MrfConditionEventOccurred() : base(MrfConditionType.EventOccurred) { }
        public MrfConditionEventOccurred(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return (Invert ? "!" : "") + $"event('{ParameterName}')";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionBoolParameterExists : MrfConditionWithParameterAndBoolValueBase
    {
        public bool Invert { get => Value; set => Value = value; }

        public MrfConditionBoolParameterExists() : base(MrfConditionType.BoolParameterExists) { }
        public MrfConditionBoolParameterExists(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return (Invert ? "!" : "") + $"exists('{ParameterName}')";
        }
    }
    [TC(typeof(EXP))] public class MrfConditionBoolParameterEquals : MrfConditionWithParameterAndBoolValueBase
    {
        public MrfConditionBoolParameterEquals() : base(MrfConditionType.BoolParameterEquals) { }
        public MrfConditionBoolParameterEquals(DataReader r) : base(r) { }

        public override string ToExpressionString(MrfFile mrf)
        {
            return $"'{ParameterName}' == {Value}";
        }
    }

    /// <summary>
    /// Before the target node updates, sets the target node parameter to the source network parameter value.
    /// </summary>
    [TC(typeof(EXP))] public class MrfStateInputParameter : IMetaXmlItem
    {
        // rage::mvNodeStateDef::InputParameter

        public MetaHash SourceParameterName { get; set; }
        public ushort TargetNodeIndex { get; set; }
        public /*MrfNodeParameterId*/ushort TargetNodeParameterId { get; set; }
        public uint TargetNodeParameterExtraArg { get; set; } // some node parameters require an additional argument to be passed (e.g. a name hash)

        public MrfStateInputParameter() { }
        public MrfStateInputParameter(DataReader r)
        {
            SourceParameterName = r.ReadUInt32();
            TargetNodeIndex = r.ReadUInt16();
            TargetNodeParameterId = r.ReadUInt16();
            TargetNodeParameterExtraArg = r.ReadUInt32();
        }

        public void Write(DataWriter w)
        {
            w.Write(SourceParameterName);
            w.Write(TargetNodeIndex);
            w.Write(TargetNodeParameterId);
            w.Write(TargetNodeParameterExtraArg);
        }

        public void ReadXml(XmlNode node)
        {
            SourceParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "SourceParameterName"));
            TargetNodeIndex = (ushort)Xml.GetChildUIntAttribute(node, "TargetNodeIndex");
            TargetNodeParameterId = (ushort)Xml.GetChildUIntAttribute(node, "TargetNodeParameterId");
            TargetNodeParameterExtraArg = Xml.GetChildUIntAttribute(node, "TargetNodeParameterExtraArg");
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.StringTag(sb, indent, "SourceParameterName", MrfXml.HashString(SourceParameterName));
            MrfXml.ValueTag(sb, indent, "TargetNodeIndex", TargetNodeIndex.ToString());
            MrfXml.ValueTag(sb, indent, "TargetNodeParameterId", TargetNodeParameterId.ToString());
            MrfXml.ValueTag(sb, indent, "TargetNodeParameterExtraArg", TargetNodeParameterExtraArg.ToString());
        }

        public override string ToString()
        {
            return SourceParameterName.ToString() + " - " + TargetNodeIndex.ToString() + " - " + TargetNodeParameterId.ToString() + " - " + TargetNodeParameterExtraArg.ToString();
        }
    }

    /// <summary>
    /// Sets a network bool parameter named <see cref="ParameterName"/> to true when the event occurs on the specified node.
    /// </summary>
    [TC(typeof(EXP))] public class MrfStateEvent : IMetaXmlItem
    {
        // rage::mvNodeStateDef::Event

        public ushort NodeIndex { get; set; }
        public /*MrfNodeEventId*/ushort NodeEventId { get; set; }
        public MetaHash ParameterName { get; set; }

        public MrfStateEvent() { }
        public MrfStateEvent(DataReader r)
        {
            NodeIndex = r.ReadUInt16();
            NodeEventId = r.ReadUInt16();
            ParameterName = r.ReadUInt32();
        }

        public void Write(DataWriter w)
        {
            w.Write(NodeIndex);
            w.Write(NodeEventId);
            w.Write(ParameterName);
        }

        public void ReadXml(XmlNode node)
        {
            NodeIndex = (ushort)Xml.GetChildUIntAttribute(node, "NodeIndex");
            NodeEventId = (ushort)Xml.GetChildUIntAttribute(node, "NodeEventId");
            ParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "ParameterName"));
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.ValueTag(sb, indent, "NodeIndex", NodeIndex.ToString());
            MrfXml.ValueTag(sb, indent, "NodeEventId", NodeEventId.ToString());
            MrfXml.StringTag(sb, indent, "ParameterName", MrfXml.HashString(ParameterName));
        }

        public override string ToString()
        {
            return NodeIndex.ToString() + " - " + NodeEventId.ToString() + " - " + ParameterName.ToString();
        }
    }

    /// <summary>
    /// After the source node updates, sets the target network parameter to the source node parameter value.
    /// </summary>
    [TC(typeof(EXP))] public class MrfStateOutputParameter : IMetaXmlItem
    {
        // rage::mvNodeStateDef::OutputParameter

        public MetaHash TargetParameterName { get; set; }
        public ushort SourceNodeIndex { get; set; }
        public /*MrfNodeParameterId*/ushort SourceNodeParameterId { get; set; } // if 0xFFFF, it stores the node itself, so it can be used by NodeProxy
        public uint SourceNodeParameterExtraArg { get; set; }

        public MrfStateOutputParameter() { }
        public MrfStateOutputParameter(DataReader r)
        {
            TargetParameterName = r.ReadUInt32();
            SourceNodeIndex = r.ReadUInt16();
            SourceNodeParameterId = r.ReadUInt16();
            SourceNodeParameterExtraArg = r.ReadUInt32();
        }

        public void Write(DataWriter w)
        {
            w.Write(TargetParameterName);
            w.Write(SourceNodeIndex);
            w.Write(SourceNodeParameterId);
            w.Write(SourceNodeParameterExtraArg);
        }

        public void ReadXml(XmlNode node)
        {
            TargetParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "TargetParameterName"));
            SourceNodeIndex = (ushort)Xml.GetChildUIntAttribute(node, "SourceNodeIndex");
            SourceNodeParameterId = (ushort)Xml.GetChildUIntAttribute(node, "SourceNodeParameterId");
            SourceNodeParameterExtraArg = Xml.GetChildUIntAttribute(node, "SourceNodeParameterExtraArg");
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.StringTag(sb, indent, "TargetParameterName", MrfXml.HashString(TargetParameterName));
            MrfXml.ValueTag(sb, indent, "SourceNodeIndex", SourceNodeIndex.ToString());
            MrfXml.ValueTag(sb, indent, "SourceNodeParameterId", SourceNodeParameterId.ToString());
            MrfXml.ValueTag(sb, indent, "SourceNodeParameterExtraArg", SourceNodeParameterExtraArg.ToString());
        }

        public override string ToString()
        {
            return TargetParameterName.ToString() + " - " + SourceNodeIndex.ToString() + " - " + SourceNodeParameterId.ToString() + " - " + SourceNodeParameterExtraArg.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfStateRef
    {
        public MetaHash StateName { get; set; }
        public int StateOffset { get; set; } // offset from the start of this field
        public int StateFileOffset { get; set; }

        public MrfNodeStateBase State { get; set; }

        public MrfStateRef()
        {
        }

        public MrfStateRef(DataReader r)
        {
            StateName = r.ReadUInt32();
            StateOffset = r.ReadInt32();
            StateFileOffset = (int)(r.Position + StateOffset - 4);
        }

        public void Write(DataWriter w)
        {
            w.Write(StateName);
            w.Write(StateOffset);
        }

        public override string ToString()
        {
            return StateName.ToString();
        }
    }

    public enum MrfOperatorType : uint
    {
        Finish = 0,         // finish the operation
        PushLiteral = 1,    // push a specific value, not used in vanilla MRFs
        PushParameter = 2,  // push a network parameter value
        Add = 3,            // adds the two values at the top of the stack, not used in vanilla MRFs
        Multiply = 4,       // multiplies the two values at the top of the stack
        Remap = 5,          // remaps the value at the top of the stack to another range
    }

    [TC(typeof(EXP))] public abstract class MrfStateOperator : IMetaXmlItem
    {
        // rage::mvNodeStateDef::Operator

        public MrfOperatorType Type { get; set; }      //0, 2, 4, 5

        public MrfStateOperator(MrfOperatorType type)
        {
            Type = type;
        }
        public MrfStateOperator(DataReader r)
        {
            Type = (MrfOperatorType)r.ReadUInt32();
        }

        public virtual void Write(DataWriter w)
        {
            w.Write((uint)Type);
        }

        public virtual void ReadXml(XmlNode node) { }
        public virtual void WriteXml(StringBuilder sb, int indent) { }

        public override string ToString()
        {
            return Type.ToString();
        }

        public static MrfStateOperator CreateOperator(MrfOperatorType type)
        {
            switch (type)
            {
                case MrfOperatorType.Finish:        return new MrfStateOperatorFinish();
                case MrfOperatorType.PushLiteral:   return new MrfStateOperatorPushLiteral();
                case MrfOperatorType.PushParameter: return new MrfStateOperatorPushParameter();
                case MrfOperatorType.Add:           return new MrfStateOperatorAdd();
                case MrfOperatorType.Multiply:      return new MrfStateOperatorMultiply();
                case MrfOperatorType.Remap:         return new MrfStateOperatorRemap();
                default: throw new Exception($"Unknown operator type ({type})");
            }
        }
    }

    [TC(typeof(EXP))] public class MrfStateOperatorFinish : MrfStateOperator
    {
        public uint Unk1_Unused { get; set; }

        public MrfStateOperatorFinish() : base(MrfOperatorType.Finish) { }
        public MrfStateOperatorFinish(DataReader r) : base(r)
        {
            Unk1_Unused = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(Unk1_Unused);
        }
    }

    [TC(typeof(EXP))] public class MrfStateOperatorPushLiteral : MrfStateOperator
    {
        public float Value { get; set; }

        public MrfStateOperatorPushLiteral() : base(MrfOperatorType.PushLiteral) { }
        public MrfStateOperatorPushLiteral(DataReader r) : base(r)
        {
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(Value);
        }

        public override void ReadXml(XmlNode node)
        {
            Value = Xml.GetChildFloatAttribute(node, "Value");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.ValueTag(sb, indent, "Value", FloatUtil.ToString(Value));
        }

        public override string ToString()
        {
            return Type + " " + FloatUtil.ToString(Value);
        }
    }

    [TC(typeof(EXP))] public class MrfStateOperatorPushParameter : MrfStateOperator
    {
        public MetaHash ParameterName { get; set; }

        public MrfStateOperatorPushParameter() : base(MrfOperatorType.PushParameter) { }
        public MrfStateOperatorPushParameter(DataReader r) : base(r)
        {
            ParameterName = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(ParameterName);
        }

        public override void ReadXml(XmlNode node)
        {
            ParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "ParameterName"));
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.StringTag(sb, indent, "ParameterName", MrfXml.HashString(ParameterName));
        }

        public override string ToString()
        {
            return Type + " '" + ParameterName + "'";
        }
    }

    [TC(typeof(EXP))] public class MrfStateOperatorAdd : MrfStateOperator
    {
        public uint Unk1_Unused { get; set; }

        public MrfStateOperatorAdd() : base(MrfOperatorType.Add) { }
        public MrfStateOperatorAdd(DataReader r) : base(r)
        {
            Unk1_Unused = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(Unk1_Unused);
        }
    }

    [TC(typeof(EXP))] public class MrfStateOperatorMultiply : MrfStateOperator
    {
        public uint Unk1_Unused { get; set; }

        public MrfStateOperatorMultiply() : base(MrfOperatorType.Multiply) { }
        public MrfStateOperatorMultiply(DataReader r) : base(r)
        {
            Unk1_Unused = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(Unk1_Unused);
        }
    }

    [TC(typeof(EXP))] public class MrfStateOperatorRemap : MrfStateOperator
    {
        public int DataOffset { get; set; } = 4; // offset from the start of this field to Min field (always 4)
        public float Min { get; set; }     // minimum of input range
        public float Max { get; set; }     // maximum of input range
        public uint RangeCount { get; set; }
        public int RangesOffset { get; set; } = 4; // offset from the start of this field to Ranges array (always 4)

        public MrfStateOperatorRemapRange[] Ranges { get; set; } // output ranges to choose from

        public MrfStateOperatorRemap() : base(MrfOperatorType.Remap) { }
        public MrfStateOperatorRemap(DataReader r) : base(r)
        {
            DataOffset = r.ReadInt32();
            Min = r.ReadSingle();
            Max = r.ReadSingle();
            RangeCount = r.ReadUInt32();
            RangesOffset = r.ReadInt32();
            Ranges = new MrfStateOperatorRemapRange[RangeCount];
            for (int i = 0; i < RangeCount; i++)
            {
                Ranges[i] = new MrfStateOperatorRemapRange(r);
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            RangeCount = (uint)(Ranges?.Length ?? 0);

            w.Write(DataOffset);
            w.Write(Min);
            w.Write(Max);
            w.Write(RangeCount);
            w.Write(RangesOffset);

            foreach (var item in Ranges)
                item.Write(w);
        }

        public override void ReadXml(XmlNode node)
        {
            Min = Xml.GetChildFloatAttribute(node, "Min");
            Max = Xml.GetChildFloatAttribute(node, "Max");
            Ranges = XmlMeta.ReadItemArray<MrfStateOperatorRemapRange>(node, "Ranges");
            RangeCount = (uint)(Ranges?.Length ?? 0);
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.ValueTag(sb, indent, "Min", FloatUtil.ToString(Min));
            MrfXml.ValueTag(sb, indent, "Max", FloatUtil.ToString(Max));
            MrfXml.WriteItemArray(sb, Ranges, indent, "Ranges");
        }

        public override string ToString()
        {
            return Type + " (" + FloatUtil.ToString(Min) + ".." + FloatUtil.ToString(Max) + ") -> [" + string.Join(",", Ranges.AsEnumerable()) + "]";
        }
    }

    [TC(typeof(EXP))] public class MrfStateOperatorRemapRange : IMetaXmlItem
    {
        public uint Unk1_Unused { get; set; } // always 0, seems unused
        public float Percent { get; set; } // if less than or equal to ((origValue - origMin) / (origMax - origMin)), this range is selected for the remap operation
        public float Length { get; set; } // Length = Max - Min
        public float Min { get; set; }

        public MrfStateOperatorRemapRange() { }
        public MrfStateOperatorRemapRange(DataReader r)
        {
            Unk1_Unused = r.ReadUInt32();
            Percent = r.ReadSingle();
            Length = r.ReadSingle();
            Min = r.ReadSingle();
        }

        public void Write(DataWriter w)
        {
            w.Write(Unk1_Unused);
            w.Write(Percent);
            w.Write(Length);
            w.Write(Min);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.ValueTag(sb, indent, "Percent", FloatUtil.ToString(Percent));
            MrfXml.ValueTag(sb, indent, "Min", FloatUtil.ToString(Min));
            MrfXml.ValueTag(sb, indent, "Length", FloatUtil.ToString(Length));
        }

        public void ReadXml(XmlNode node)
        {
            Percent = Xml.GetChildFloatAttribute(node, "Percent");
            Min = Xml.GetChildFloatAttribute(node, "Min");
            Length = Xml.GetChildFloatAttribute(node, "Length");
        }

        public override string ToString()
        {
            return FloatUtil.ToString(Percent) + " - (" + FloatUtil.ToString(Min) + ".." + FloatUtil.ToString(Min+Length) + ")";
        }
    }

    /// <summary>
    /// Before the node updates, calculates the specified operations and stores the value in a node parameter.
    /// </summary>
    [TC(typeof(EXP))] public class MrfStateOperation : IMetaXmlItem
    {
        // rage::mvNodeStateDef::Operation

        public ushort NodeIndex { get; set; }
        public /*MrfNodeParameterId*/ushort NodeParameterId { get; set; }
        public ushort StackSize { get; set; } // in bytes, Operators.Length * 8
        public ushort NodeParameterExtraArg { get; set; }
        public MrfStateOperator[] Operators { get; set; }

        public MrfStateOperation() { }
        public MrfStateOperation(DataReader r)
        {
            NodeIndex = r.ReadUInt16();
            NodeParameterId = r.ReadUInt16();
            StackSize = r.ReadUInt16();
            NodeParameterExtraArg = r.ReadUInt16();

            var operators = new List<MrfStateOperator>();

            while (true)
            {
                var startPos = r.Position;
                var opType = (MrfOperatorType)r.ReadUInt32();
                r.Position = startPos;

                MrfStateOperator op;
                switch (opType)
                {
                    case MrfOperatorType.Finish:        op = new MrfStateOperatorFinish(r); break;
                    case MrfOperatorType.PushLiteral:   op = new MrfStateOperatorPushLiteral(r); break;
                    case MrfOperatorType.PushParameter: op = new MrfStateOperatorPushParameter(r); break;
                    case MrfOperatorType.Add:           op = new MrfStateOperatorAdd(r); break;
                    case MrfOperatorType.Multiply:      op = new MrfStateOperatorMultiply(r); break;
                    case MrfOperatorType.Remap:         op = new MrfStateOperatorRemap(r); break;
                    default: throw new Exception($"Unknown operator type ({opType})");
                }

                operators.Add(op);
                if (opType == MrfOperatorType.Finish)
                {
                    break;
                }
            }

            Operators = operators.ToArray();

            if (StackSize != Operators.Length * 8)
            { } // no hit
        }

        public void Write(DataWriter w)
        {
            w.Write(NodeIndex);
            w.Write(NodeParameterId);
            w.Write(StackSize);
            w.Write(NodeParameterExtraArg);

            foreach (var op in Operators)
                op.Write(w);
        }

        public void ReadXml(XmlNode node)
        {
            NodeIndex = (ushort)Xml.GetChildUIntAttribute(node, "NodeIndex");
            NodeParameterId = (ushort)Xml.GetChildUIntAttribute(node, "NodeParameterId");
            NodeParameterExtraArg = (ushort)Xml.GetChildUIntAttribute(node, "NodeParameterExtraArg");
            Operators = null;
            var operatorsNode = node.SelectSingleNode("Operators");
            if (operatorsNode != null)
            {
                var inodes = operatorsNode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    Operators = new MrfStateOperator[inodes.Count];
                    int i = 0;
                    foreach (XmlNode inode in inodes)
                    {
                        Operators[i] = XmlMrf.ReadOperator(inode);
                        i++;
                    }
                }
            }
            StackSize = (ushort)((Operators?.Length ?? 0) * 8);
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.ValueTag(sb, indent, "NodeIndex", NodeIndex.ToString());
            MrfXml.ValueTag(sb, indent, "NodeParameterId", NodeParameterId.ToString());
            MrfXml.ValueTag(sb, indent, "NodeParameterExtraArg", NodeParameterExtraArg.ToString());
            int cindent = indent + 1;
            MrfXml.OpenTag(sb, indent, "Operators");
            foreach (var op in Operators)
            {
                MrfXml.WriteOperator(sb, cindent, "Item", op);
            }
            MrfXml.CloseTag(sb, indent, "Operators");
        }

        public override string ToString()
        {
            return NodeIndex.ToString() + " - " + NodeParameterId.ToString() + " - " + StackSize.ToString() + " - " + NodeParameterExtraArg.ToString() + " - " +
                (Operators?.Length ?? 0).ToString() + " operators";
        }
    }
    
#endregion

#region mrf node classes
    
    [TC(typeof(EXP))] public class MrfNodeStateMachine : MrfNodeStateBase
    {
        // rage__mvNodeStateMachineClass (1)

        public MrfStateRef[] States { get; set; }

        public MrfNodeStateMachine() : base(MrfNodeType.StateMachine) { }

        public override void Read(DataReader r)
        {
            base.Read(r);


            if (StateChildCount > 0)
            {
                States = new MrfStateRef[StateChildCount];
                for (int i = 0; i < StateChildCount; i++)
                    States[i] = new MrfStateRef(r);

            }

            if (TransitionCount > 0)
            {
                if (r.Position != TransitionsFileOffset)
                { } // no hits

                Transitions = new MrfStateTransition[TransitionCount];
                for (int i = 0; i < TransitionCount; i++)
                    Transitions[i] = new MrfStateTransition(r);
            }
        }

        public override void Write(DataWriter w)
        {
            StateChildCount = (byte)(States?.Length ?? 0);
            TransitionCount = (byte)(Transitions?.Length ?? 0);

            base.Write(w);

            if (States != null)
                foreach (var state in States)
                    state.Write(w);

            if (Transitions != null)
                foreach(var transition in Transitions)
                    transition.Write(w);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            States = null;
            var statesNode = node.SelectSingleNode("States");
            if (statesNode != null)
            {
                var inodes = statesNode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    States = new MrfStateRef[inodes.Count];
                    int i = 0;
                    foreach (XmlNode inode in inodes)
                    {
                        var s = new MrfStateRef();
                        s.State = (MrfNodeStateBase)XmlMrf.ReadNode(inode);
                        s.StateName = s.State.Name;
                        States[i] = s;
                        i++;
                    }
                }
            }
            StateChildCount = (byte)(States?.Length ?? 0);
            var initialStateName = XmlMrf.ReadChildNodeRef(node, "InitialState");
            InitialNode = States?.FirstOrDefault(s => s.StateName == initialStateName)?.State;
            Transitions = XmlMeta.ReadItemArray<MrfStateTransition>(node, "Transitions");
            TransitionCount = (byte)(Transitions?.Length ?? 0);

            ResolveXmlTargetStatesInTransitions(States);
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.WriteNodeRef(sb, indent, "InitialState", InitialNode);
            int cindent = indent + 1;
            MrfXml.OpenTag(sb, indent, "States");
            foreach (var s in States)
            {
                MrfXml.WriteNode(sb, cindent, "Item", s.State);
            }
            MrfXml.CloseTag(sb, indent, "States");
            MrfXml.WriteItemArray(sb, Transitions, indent, "Transitions");
        }

        public override void ResolveRelativeOffsets(MrfFile mrf)
        {
            base.ResolveRelativeOffsets(mrf);

            ResolveNodeOffsetsInTransitions(Transitions, mrf);
            ResolveNodeOffsetsInStates(States, mrf);
        }

        public override void UpdateRelativeOffsets()
        {
            base.UpdateRelativeOffsets();

            var offset = (int)(FileOffset + 0x20/*sizeof(MrfNodeStateBase)*/);
            offset = UpdateNodeOffsetsInStates(States, offset);

            offset = UpdateNodeOffsetsInTransitions(Transitions, offset,
                        offsetSetToZeroIfNoTransitions: TransitionsOffset == 0); // MrfNodeStateMachine doesn't seem consistent on whether TransitionsOffset
                                                                                 // should be 0 if there are no transitions, so if it's already zero don't change it
        }
    }

    [TC(typeof(EXP))] public class MrfNodeTail : MrfNode
    {
        // rage__mvNodeTail (2)

        public MrfNodeTail() : base(MrfNodeType.Tail) { }
    }

    [TC(typeof(EXP))] public class MrfNodeInlinedStateMachine : MrfNodeStateBase
    {
        // rage__mvNodeInlinedStateMachine (3)

        public int FallbackNodeOffset { get; set; }
        public int FallbackNodeFileOffset { get; set; }
        public MrfStateRef[] States { get; set; }

        public MrfNode FallbackNode { get; set; } // node used when a NodeTail is reached (maybe in some other cases too?). This node is considered a child
                                                  // of the parent NodeState, so FallbackNode and its children (including their NodeIndex) should be
                                                  // included in the parent NodeState.StateChildCount, not in this NodeInlinedStateMachine.StateChildCount

        public MrfNodeInlinedStateMachine() : base(MrfNodeType.InlinedStateMachine) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            FallbackNodeOffset = r.ReadInt32();
            FallbackNodeFileOffset = (int)(r.Position + FallbackNodeOffset - 4);

            if (StateChildCount > 0)
            {
                States = new MrfStateRef[StateChildCount];
                for (int i = 0; i < StateChildCount; i++)
                    States[i] = new MrfStateRef(r);
            }
        }

        public override void Write(DataWriter w)
        {
            StateChildCount = (byte)(States?.Length ?? 0);

            base.Write(w);

            w.Write(FallbackNodeOffset);

            if (States != null)
                foreach (var item in States)
                    item.Write(w);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            States = null;
            var statesNode = node.SelectSingleNode("States");
            if (statesNode != null)
            {
                var inodes = statesNode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    States = new MrfStateRef[inodes.Count];
                    int i = 0;
                    foreach (XmlNode inode in inodes)
                    {
                        var s = new MrfStateRef();
                        s.State = (MrfNodeStateBase)XmlMrf.ReadNode(inode);
                        s.StateName = s.State.Name;
                        States[i] = s;
                        i++;
                    }
                }
            }
            StateChildCount = (byte)(States?.Length ?? 0);
            var initialStateName = XmlMrf.ReadChildNodeRef(node, "InitialState");
            InitialNode = States?.FirstOrDefault(s => s.StateName == initialStateName)?.State;
            FallbackNode = XmlMrf.ReadChildNode(node, "FallbackNode");

            ResolveXmlTargetStatesInTransitions(States);
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.WriteNodeRef(sb, indent, "InitialState", InitialNode);
            int cindent = indent + 1;
            MrfXml.OpenTag(sb, indent, "States");
            foreach (var s in States)
            {
                MrfXml.WriteNode(sb, cindent, "Item", s.State);
            }
            MrfXml.CloseTag(sb, indent, "States");
            MrfXml.WriteNode(sb, indent, "FallbackNode", FallbackNode);
        }

        public override void ResolveRelativeOffsets(MrfFile mrf)
        {
            base.ResolveRelativeOffsets(mrf);

            var fallbackNode = mrf.FindNodeAtFileOffset(FallbackNodeFileOffset);
            if (fallbackNode == null)
            { } // no hits

            FallbackNode = fallbackNode;

            ResolveNodeOffsetsInStates(States, mrf);
        }

        public override void UpdateRelativeOffsets()
        {
            base.UpdateRelativeOffsets();

            var offset = FileOffset + 0x20/*sizeof(MrfNodeStateBase)*/;

            FallbackNodeFileOffset = FallbackNode.FileOffset;
            FallbackNodeOffset = FallbackNodeFileOffset - offset;

            offset += 4;
            offset = UpdateNodeOffsetsInStates(States, offset);

            offset = UpdateNodeOffsetsInTransitions(null, offset, offsetSetToZeroIfNoTransitions: true);
        }

        public override string ToString()
        {
            return base.ToString() + " - " + FallbackNodeOffset.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfNodeAnimation : MrfNodeWithFlagsBase
    {
        // rage__mvNode* (4) not used in final game
        // Probably not worth researching further. Seems like the introduction of NodeClip (and rage::crClip), made this node obsolete.
        // Even the function pointer used to lookup the rage::crAnimation when AnimationType==Literal is null, so the only way to get animations is through a parameter.

        public uint AnimationUnkDataLength { get; set; }
        public byte[] AnimationUnkData { get; set; }
        public MetaHash AnimationName => AnimationUnkData == null ? 0 : BitConverter.ToUInt32(AnimationUnkData, 0);
        public MetaHash AnimationParameterName { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6 { get; set; }
        public uint Unk7 { get; set; }

        // flags getters and setters
        public MrfValueType AnimationType
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }

        public MrfNodeAnimation() : base(MrfNodeType.Animation) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            switch (AnimationType)
            {
                case MrfValueType.Literal:
                    {
                        AnimationUnkDataLength = r.ReadUInt32();
                        AnimationUnkData = r.ReadBytes((int)AnimationUnkDataLength);
                        break;
                    }
                case MrfValueType.Parameter:
                    AnimationParameterName = r.ReadUInt32();
                    break;
            }

            if (((Flags >> 2) & 3) != 0)
                Unk3 = r.ReadUInt32();

            if (((Flags >> 4) & 3) != 0)
                Unk4 = r.ReadUInt32();

            if (((Flags >> 6) & 3) != 0)
                Unk5 = r.ReadUInt32();

            if (((Flags >> 8) & 3) != 0)
                Unk6 = r.ReadUInt32();

            if (((Flags >> 10) & 3) != 0)
                Unk7 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            switch (AnimationType)
            {
                case MrfValueType.Literal:
                    {
                        w.Write(AnimationUnkDataLength);
                        w.Write(AnimationUnkData);
                        break;
                    }
                case MrfValueType.Parameter:
                    w.Write(AnimationParameterName);
                    break;
            }

            if (((Flags >> 2) & 3) != 0)
                w.Write(Unk3);

            if (((Flags >> 4) & 3) != 0)
                w.Write(Unk4);

            if (((Flags >> 6) & 3) != 0)
                w.Write(Unk5);

            if (((Flags >> 8) & 3) != 0)
                w.Write(Unk6);

            if (((Flags >> 10) & 3) != 0)
                w.Write(Unk7);
        }

        public override void ReadXml(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))] public class MrfNodeBlend : MrfNodePairWeightedBase
    {
        // rage__mvNodeBlend (5)

        public MrfNodeBlend() : base(MrfNodeType.Blend) { }
    }

    [TC(typeof(EXP))] public class MrfNodeAddSubtract : MrfNodePairWeightedBase
    {
        // rage__mvNodeAddSubtract (6)

        public MrfNodeAddSubtract() : base(MrfNodeType.AddSubtract) { }
    }

    [TC(typeof(EXP))] public class MrfNodeFilter : MrfNodeWithChildAndFilterBase
    {
        // rage__mvNodeFilter (7)

        public MrfNodeFilter() : base(MrfNodeType.Filter) { }
    }

    [TC(typeof(EXP))] public class MrfNodeMirror : MrfNodeWithChildAndFilterBase
    {
        // rage__mvNodeMirror (8)

        public MrfNodeMirror() : base(MrfNodeType.Mirror) { }
    }

    [TC(typeof(EXP))] public class MrfNodeFrame : MrfNodeWithFlagsBase
    {
        // rage__mvNodeFrame (9)

        public MetaHash FrameParameterName { get; set; }
        public MetaHash Unk3 { get; set; } // unused

        // flags getters and setters
        public MrfValueType FrameType // only Parameter type is supported
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }
        public MrfValueType Unk3Type
        {
            get => (MrfValueType)GetFlagsSubset(4, 3);
            set => SetFlagsSubset(4, 3, (uint)value);
        }

        public MrfNodeFrame() : base(MrfNodeType.Frame) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            if (FrameType != MrfValueType.None)
                FrameParameterName = r.ReadUInt32();

            if (Unk3Type != MrfValueType.None)
                Unk3 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            if (FrameType != MrfValueType.None)
                w.Write(FrameParameterName);

            if (Unk3Type != MrfValueType.None)
                w.Write(Unk3);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            Unk3 = 0;
            Unk3Type = MrfValueType.None;

            (FrameType, _, _, FrameParameterName) = XmlMrf.GetChildParameterizedAsset(node, "Frame");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.ParameterizedAssetTag(sb, indent, "Frame", FrameType, 0, 0, FrameParameterName);
        }
    }

    [TC(typeof(EXP))] public class MrfNodeIk : MrfNode
    {
        // rage__mvNodeIk (10)

        public MrfNodeIk() : base(MrfNodeType.Ik) { }
    }

    [TC(typeof(EXP))] public class MrfNodeBlendN : MrfNodeNBase
    {
        // rage__mvNodeBlendN (13)

        public MrfNodeBlendN() : base(MrfNodeType.BlendN) { }
    }

    public enum MrfClipContainerType : uint
    {
        VariableClipSet = 0, // a fwClipSet stored in the move network by the game code (when this clipset is added to the network it enables its fwClipSet.moveNetworkFlags, when removed they are disabled)
        ClipSet = 1,         // a fwClipSet
        ClipDictionary = 2,  // a .ycd
        Unk3 = 3,            // unknown, only ClipContainerName is set when used (only used in minigame_drilling_bag.mrf)
    }

    [TC(typeof(EXP))] public class MrfNodeClip : MrfNodeWithFlagsBase
    {
        // rage__mvNodeClip (15)

        public MetaHash ClipParameterName { get; set; }
        public MrfClipContainerType ClipContainerType { get; set; }
        public MetaHash ClipContainerName { get; set; }
        public MetaHash ClipName { get; set; }
        public float Phase { get; set; }
        public MetaHash PhaseParameterName { get; set; }
        public float Rate { get; set; }
        public MetaHash RateParameterName { get; set; }
        public float Delta { get; set; }
        public MetaHash DeltaParameterName { get; set; }
        public bool Looped { get; set; }
        public MetaHash LoopedParameterName { get; set; }

        // flags getters and setters
        public MrfValueType ClipType
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }
        public MrfValueType PhaseType
        {
            get => (MrfValueType)GetFlagsSubset(2, 3);
            set => SetFlagsSubset(2, 3, (uint)value);
        }
        public MrfValueType RateType
        {
            get => (MrfValueType)GetFlagsSubset(4, 3);
            set => SetFlagsSubset(4, 3, (uint)value);
        }
        public MrfValueType DeltaType
        {
            get => (MrfValueType)GetFlagsSubset(6, 3);
            set => SetFlagsSubset(6, 3, (uint)value);
        }
        public MrfValueType LoopedType
        {
            get => (MrfValueType)GetFlagsSubset(8, 3);
            set => SetFlagsSubset(8, 3, (uint)value);
        }
        public uint UnkFlag10
        {
            get => GetFlagsSubset(10, 3);
            set => SetFlagsSubset(10, 3, value);
        }

        public MrfNodeClip() : base(MrfNodeType.Clip) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            switch (ClipType)
            {
                case MrfValueType.Literal:
                    {
                        ClipContainerType = (MrfClipContainerType)r.ReadUInt32();
                        ClipContainerName = r.ReadUInt32();

                        if (ClipContainerType != MrfClipContainerType.Unk3)
                            ClipName = r.ReadUInt32();
                        break;
                    }
                case MrfValueType.Parameter:
                    ClipParameterName = r.ReadUInt32();
                    break;
            }

            switch (PhaseType)
            {
                case MrfValueType.Literal:
                    Phase = r.ReadSingle();
                    break;
                case MrfValueType.Parameter:
                    PhaseParameterName = r.ReadUInt32();
                    break;
            }

            switch (RateType)
            {
                case MrfValueType.Literal:
                    Rate = r.ReadSingle();
                    break;
                case MrfValueType.Parameter:
                    RateParameterName = r.ReadUInt32();
                    break;
            }

            switch (DeltaType)
            {
                case MrfValueType.Literal:
                    Delta = r.ReadSingle();
                    break;
                case MrfValueType.Parameter:
                    DeltaParameterName = r.ReadUInt32();
                    break;
            }

            switch (LoopedType)
            {
                case MrfValueType.Literal:
                    Looped = r.ReadUInt32() != 0;
                    break;
                case MrfValueType.Parameter:
                    LoopedParameterName = r.ReadUInt32();
                    break;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            switch (ClipType)
            {
                case MrfValueType.Literal:
                    {
                        w.Write((uint)ClipContainerType);
                        w.Write(ClipContainerName);

                        if (ClipContainerType != MrfClipContainerType.Unk3)
                            w.Write(ClipName);

                        break;
                    }
                case MrfValueType.Parameter:
                    w.Write(ClipParameterName);
                    break;
            }

            switch (PhaseType)
            {
                case MrfValueType.Literal:
                    w.Write(Phase);
                    break;
                case MrfValueType.Parameter:
                    w.Write(PhaseParameterName);
                    break;
            }

            switch (RateType)
            {
                case MrfValueType.Literal:
                    w.Write(Rate);
                    break;
                case MrfValueType.Parameter:
                    w.Write(RateParameterName);
                    break;
            }

            switch (DeltaType)
            {
                case MrfValueType.Literal:
                    w.Write(Delta);
                    break;
                case MrfValueType.Parameter:
                    w.Write(DeltaParameterName);
                    break;
            }

            switch (LoopedType)
            {
                case MrfValueType.Literal:
                    w.Write(Looped ? 0x01000000 : 0u); // bool originally stored as a big-endian uint, game just checks != 0. Here we do it to output the same bytes as the input
                    break;
                case MrfValueType.Parameter:
                    w.Write(LoopedParameterName);
                    break;
            }
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            (ClipType, ClipContainerType, ClipContainerName, ClipName, ClipParameterName) = XmlMrf.GetChildParameterizedClip(node, "Clip");
            (PhaseType, Phase, PhaseParameterName) = XmlMrf.GetChildParameterizedFloat(node, "Phase");
            (RateType, Rate, RateParameterName) = XmlMrf.GetChildParameterizedFloat(node, "Rate");
            (DeltaType, Delta, DeltaParameterName) = XmlMrf.GetChildParameterizedFloat(node, "Delta");
            (LoopedType, Looped, LoopedParameterName) = XmlMrf.GetChildParameterizedBool(node, "Looped");
            UnkFlag10 = Xml.GetChildUIntAttribute(node, "UnkFlag10");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.ParameterizedClipTag(sb, indent, "Clip", ClipType, ClipContainerType, ClipContainerName, ClipName, ClipParameterName);
            MrfXml.ParameterizedFloatTag(sb, indent, "Phase", PhaseType, Phase, PhaseParameterName);
            MrfXml.ParameterizedFloatTag(sb, indent, "Rate", RateType, Rate, RateParameterName);
            MrfXml.ParameterizedFloatTag(sb, indent, "Delta", DeltaType, Delta, DeltaParameterName);
            MrfXml.ParameterizedBoolTag(sb, indent, "Looped", LoopedType, Looped, LoopedParameterName);
            MrfXml.ValueTag(sb, indent, "UnkFlag10", UnkFlag10.ToString());
        }
    }

    [TC(typeof(EXP))] public class MrfNodePm : MrfNodeWithFlagsBase
    {
        // rage__mvNode* (17) not used in final game
        // The backing node is rage::crmtNodePm
        // Pm = Parameterized Motion
        // In RDR3 renamed to rage::mvNodeMotion/rage::crmtNodeMotion?

        // Seems similar to NodeClip but for rage::crpmMotion/.#pm files (added in RDR3, WIP in GTA5/MP3?)
        // In GTA5 the function pointer used to lookup the rage::crpmMotion is null

        public uint MotionUnkDataLength { get; set; }
        public byte[] MotionUnkData { get; set; }
        public MetaHash MotionName => MotionUnkData == null ? 0 : BitConverter.ToUInt32(MotionUnkData, 0);
        public MetaHash MotionParameterName { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint[] Unk6 { get; set; }

        // flags getters and setters
        public MrfValueType MotionType
        {
            // Literal not supported, the function pointer used to lookup the motion is null
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }

        public MrfNodePm() : base(MrfNodeType.Pm) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            switch (MotionType)
            {
                case MrfValueType.Literal:
                    {
                        MotionUnkDataLength = r.ReadUInt32();
                        MotionUnkData = r.ReadBytes((int)MotionUnkDataLength);
                        break;
                    }
                case MrfValueType.Parameter:
                    MotionParameterName = r.ReadUInt32();
                    break;
            }

            if (((Flags >> 2) & 3) != 0)
                Unk3 = r.ReadUInt32();

            if (((Flags >> 4) & 3) != 0)
                Unk4 = r.ReadUInt32();

            if ((Flags >> 6) != 0)
                Unk5 = r.ReadUInt32();

            var flags = Flags >> 19;
            var unk6Count = (Flags >> 10) & 0xF;

            Unk6 = new uint[unk6Count];

            for (int i = 0; i < unk6Count; i++)
            {
                if ((flags & 3) != 0)
                    Unk6[i] = r.ReadUInt32();

                flags >>= 2;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            switch (MotionType)
            {
                case MrfValueType.Literal:
                    {
                        w.Write(MotionUnkDataLength);
                        w.Write(MotionUnkData);
                        break;
                    }
                case MrfValueType.Parameter:
                    w.Write(MotionParameterName);
                    break;
            }

            if (((Flags >> 2) & 3) != 0)
                w.Write(Unk3);

            if (((Flags >> 4) & 3) != 0)
                w.Write(Unk4);

            if ((Flags >> 6) != 0)
                w.Write(Unk5);

            var unk6Count = (Flags >> 10) & 0xF;

            if (unk6Count > 0)
            {
                foreach (var value in Unk6)
                    w.Write(value);
            }
        }

        public override void ReadXml(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))] public class MrfNodeExtrapolate : MrfNodeWithChildBase
    {
        // rage__mvNodeExtrapolate (18)

        public float Damping { get; set; }
        public MetaHash DampingParameterName { get; set; }

        // flags getters and setters
        public MrfValueType DampingType
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }

        public MrfNodeExtrapolate() : base(MrfNodeType.Extrapolate) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            switch (DampingType)
            {
                case MrfValueType.Literal:
                    Damping = r.ReadSingle();
                    break;
                case MrfValueType.Parameter:
                    DampingParameterName = r.ReadUInt32();
                    break;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            switch (DampingType)
            {
                case MrfValueType.Literal:
                    w.Write(Damping);
                    break;
                case MrfValueType.Parameter:
                    w.Write(DampingParameterName);
                    break;
            }
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            (DampingType, Damping, DampingParameterName) = XmlMrf.GetChildParameterizedFloat(node, "Damping");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.ParameterizedFloatTag(sb, indent, "Damping", DampingType, Damping, DampingParameterName);
        }
    }

    [TC(typeof(EXP))] public class MrfNodeExpression : MrfNodeWithChildBase
    {
        // rage__mvNodeExpression (19)

        public MetaHash ExpressionDictionaryName { get; set; }
        public MetaHash ExpressionName { get; set; }
        public MetaHash ExpressionParameterName { get; set; }
        public float Weight { get; set; }
        public MetaHash WeightParameterName { get; set; }
        public MrfNodeExpressionVariable[] Variables { get; set; }

        // flags getters and setters
        public MrfValueType ExpressionType
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }
        public MrfValueType WeightType
        {
            get => (MrfValueType)GetFlagsSubset(2, 3);
            set => SetFlagsSubset(2, 3, (uint)value);
        }
        public uint VariableFlags
        {
            get => GetFlagsSubset(4, 0xFFFFFF);
            set => SetFlagsSubset(4, 0xFFFFFF, value);
        }
        public uint VariableCount
        {
            get => GetFlagsSubset(28, 0xF);
            set => SetFlagsSubset(28, 0xF, value);
        }

        // VariableFlags accessors by index
        public MrfValueType GetVariableType(int index)
        {
            return (MrfValueType)GetFlagsSubset(4 + 2 * index, 3);
        }
        public void SetVariableType(int index, MrfValueType type)
        {
            SetFlagsSubset(4 + 2 * index, 3, (uint)type);
        }

        public MrfNodeExpression() : base(MrfNodeType.Expression) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            switch (ExpressionType)
            {
                case MrfValueType.Literal:
                    ExpressionDictionaryName = r.ReadUInt32();
                    ExpressionName = r.ReadUInt32();
                    break;
                case MrfValueType.Parameter:
                    ExpressionParameterName = r.ReadUInt32();
                    break;
            }

            switch (WeightType)
            {
                case MrfValueType.Literal:
                    Weight = r.ReadSingle();
                    break;
                case MrfValueType.Parameter:
                    WeightParameterName = r.ReadUInt32();
                    break;
            }

            var varCount = VariableCount;

            if (varCount == 0)
                return;

            Variables = new MrfNodeExpressionVariable[varCount];

            for (int i = 0; i < varCount; i++)
            {
                var type = GetVariableType(i);
                var name = r.ReadUInt32();
                float value = 0.0f;
                uint valueParameterName = 0;

                switch (type)
                {
                    case MrfValueType.Literal:
                        value = r.ReadSingle();
                        break;
                    case MrfValueType.Parameter:
                        valueParameterName = r.ReadUInt32();
                        break;
                }

                Variables[i] = new MrfNodeExpressionVariable(name, value, valueParameterName);
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            switch (ExpressionType)
            {
                case MrfValueType.Literal:
                    w.Write(ExpressionDictionaryName);
                    w.Write(ExpressionName);
                    break;
                case MrfValueType.Parameter:
                    w.Write(ExpressionParameterName);
                    break;
            }

            switch (WeightType)
            {
                case MrfValueType.Literal:
                    w.Write(Weight);
                    break;
                case MrfValueType.Parameter:
                    w.Write(WeightParameterName);
                    break;
            }

            var varCount = VariableCount;

            if (varCount == 0)
                return;

            for (int i = 0; i < varCount; i++)
            {
                var type = GetVariableType(i);
                var variable = Variables[i];
                w.Write(variable.Name);

                switch (type)
                {
                    case MrfValueType.Literal:
                        w.Write(variable.Value);
                        break;
                    case MrfValueType.Parameter:
                        w.Write(variable.ValueParameterName);
                        break;
                }
            }
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            (WeightType, Weight, WeightParameterName) = XmlMrf.GetChildParameterizedFloat(node, "Weight");
            (ExpressionType, ExpressionDictionaryName, ExpressionName, ExpressionParameterName) = XmlMrf.GetChildParameterizedAsset(node, "Expression");

            Variables = null;
            VariableFlags = 0;
            VariableCount = 0;
            var variablesNode = node.SelectSingleNode("Variables");
            if (variablesNode != null)
            {
                var inodes = variablesNode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    VariableCount = (uint)inodes.Count;
                    Variables = new MrfNodeExpressionVariable[VariableCount];
                    int i = 0;
                    foreach (XmlNode inode in inodes)
                    {
                        var name = XmlMeta.GetHash(Xml.GetChildInnerText(inode, "Name"));
                        var value = XmlMrf.GetChildParameterizedFloat(inode, "Value");
                        Variables[i] = new MrfNodeExpressionVariable(name, value.Value, value.ParameterName);
                        SetVariableType(i, value.Type);
                        i++;
                    }
                }
            }
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.ParameterizedFloatTag(sb, indent, "Weight", WeightType, Weight, WeightParameterName);
            MrfXml.ParameterizedAssetTag(sb, indent, "Expression", ExpressionType, ExpressionDictionaryName, ExpressionName, ExpressionParameterName);
            if (Variables != null)
            {
                int cindent = indent + 1;
                int cindent2 = cindent + 1;
                int varIndex = 0;
                MrfXml.OpenTag(sb, indent, "Variables");
                foreach (var v in Variables)
                {
                    MrfXml.OpenTag(sb, cindent, "Item");
                    MrfXml.StringTag(sb, cindent2, "Name", MrfXml.HashString(v.Name));
                    MrfXml.ParameterizedFloatTag(sb, cindent2, "Value", GetVariableType(varIndex), v.Value, v.ValueParameterName);
                    MrfXml.CloseTag(sb, cindent, "Item");
                    varIndex++;
                }
                MrfXml.CloseTag(sb, indent, "Variables");
            }
            else
            {
                MrfXml.SelfClosingTag(sb, indent, "Variables");
            }
        }
    }

    [TC(typeof(EXP))] public struct MrfNodeExpressionVariable
    {
        public MetaHash Name { get; set; }
        public float Value { get; set; } // used if type == Literal
        public MetaHash ValueParameterName { get; set; } // used if type == Parameter

        public MrfNodeExpressionVariable(MetaHash name, float value, MetaHash valueParameterName)
        {
            Name = name;
            Value = value;
            ValueParameterName = valueParameterName;
        }
        
        public override string ToString()
        {
            return Name.ToString() + " - " + FloatUtil.ToString(Value) + " | " + ValueParameterName.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfNodeCapture : MrfNodeWithChildBase
    {
        // rage::mvNodeCaptureDef (20)

        public MetaHash FrameParameterName { get; set; }
        public MetaHash Unk3 { get; set; } // unused

        // flags getters and setters
        public MrfValueType FrameType // only Parameter type is supported
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }
        public MrfValueType Unk3Type
        {
            get => (MrfValueType)GetFlagsSubset(4, 3);
            set => SetFlagsSubset(4, 3, (uint)value);
        }

        public MrfNodeCapture() : base(MrfNodeType.Capture) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            if (FrameType != MrfValueType.None)
                FrameParameterName = r.ReadUInt32();

            if (Unk3Type != MrfValueType.None)
                Unk3 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            if (FrameType != MrfValueType.None)
                w.Write(FrameParameterName);

            if (Unk3Type != MrfValueType.None)
                w.Write(Unk3);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            Unk3 = 0;
            Unk3Type = MrfValueType.None;

            (FrameType, _, _, FrameParameterName) = XmlMrf.GetChildParameterizedAsset(node, "Frame");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.ParameterizedAssetTag(sb, indent, "Frame", FrameType, 0, 0, FrameParameterName);
        }
    }

    [TC(typeof(EXP))] public class MrfNodeProxy : MrfNode
    {
        // rage__mvNodeProxy (21)

        public MetaHash NodeParameterName { get; set; } // lookups a rage::crmtObserver parameter, then gets the observed node

        public MrfNodeProxy() : base(MrfNodeType.Proxy) { }

        public override void Read(DataReader r)
        {
            base.Read(r);
            NodeParameterName = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(NodeParameterName);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            NodeParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "NodeParameterName"));
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.StringTag(sb, indent, "NodeParameterName", MrfXml.HashString(NodeParameterName));
        }

        public override string ToString()
        {
            return base.ToString() + " - " + NodeParameterName.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfNodeAddN : MrfNodeNBase
    {
        // rage__mvNodeAddN (22)

        public MrfNodeAddN() : base(MrfNodeType.AddN) { }
    }

    [TC(typeof(EXP))] public class MrfNodeIdentity : MrfNode
    {
        // rage__mvNodeIdentity (23)

        public MrfNodeIdentity() : base(MrfNodeType.Identity) { }
    }

    [TC(typeof(EXP))] public class MrfNodeMerge : MrfNodePairBase
    {
        // rage::mvNodeMergeDef (24)

        public MrfSynchronizerTagFlags SynchronizerTagFlags { get; set; }
        public MetaHash FrameFilterDictionaryName { get; set; }
        public MetaHash FrameFilterName { get; set; }
        public MetaHash FrameFilterParameterName { get; set; }

        // flags getters and setters
        public MrfValueType FrameFilterType
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }
        public MrfInfluenceOverride Child0InfluenceOverride
        {
            get => (MrfInfluenceOverride)GetFlagsSubset(2, 3);
            set => SetFlagsSubset(2, 3, (uint)value);
        }
        public MrfInfluenceOverride Child1InfluenceOverride
        {
            get => (MrfInfluenceOverride)GetFlagsSubset(4, 3);
            set => SetFlagsSubset(4, 3, (uint)value);
        }
        public bool UnkFlag6 // Transitional? RDR3's rage::mvNodeMergeDef::GetTransitionalFlagFrom(uint) reads these bits
        {                    // always 0
            get => GetFlagsSubset(6, 1) != 0;
            set => SetFlagsSubset(6, 1, value ? 1 : 0u);
        }
        public uint UnkFlag7 // Immutable? RDR3's rage::mvNodeMergeDef::GetImmutableFlagFrom(uint) reads these bits
        {                    // 0 or 2
            get => GetFlagsSubset(7, 3);
            set => SetFlagsSubset(7, 3, value);
        }
        public MrfSynchronizerType SynchronizerType
        {
            get => (MrfSynchronizerType)GetFlagsSubset(19, 3);
            set => SetFlagsSubset(19, 3, (uint)value);
        }
        public uint UnkFlag21 // OutputParameterRuleSet? RDR3's rage::mvNodeMergeDef::GetOutputParameterRuleSetFrom(uint) reads these bits
        {                     // always 0
            get => GetFlagsSubset(21, 3);
            set => SetFlagsSubset(21, 3, value);
        }

        public MrfNodeMerge() : base(MrfNodeType.Merge) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            if (SynchronizerType == MrfSynchronizerType.Tag)
                SynchronizerTagFlags = (MrfSynchronizerTagFlags)r.ReadUInt32();

            switch (FrameFilterType)
            {
                case MrfValueType.Literal:
                    FrameFilterDictionaryName = r.ReadUInt32();
                    FrameFilterName = r.ReadUInt32();
                    break;
                case MrfValueType.Parameter:
                    FrameFilterParameterName = r.ReadUInt32();
                    break;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            if (SynchronizerType == MrfSynchronizerType.Tag)
                w.Write((uint)SynchronizerTagFlags);

            switch (FrameFilterType)
            {
                case MrfValueType.Literal:
                    w.Write(FrameFilterDictionaryName);
                    w.Write(FrameFilterName);
                    break;
                case MrfValueType.Parameter:
                    w.Write(FrameFilterParameterName);
                    break;
            }
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            Child0InfluenceOverride = Xml.GetChildEnumInnerText<MrfInfluenceOverride>(node, "Child0InfluenceOverride");
            Child1InfluenceOverride = Xml.GetChildEnumInnerText<MrfInfluenceOverride>(node, "Child1InfluenceOverride");
            (FrameFilterType, FrameFilterDictionaryName, FrameFilterName, FrameFilterParameterName) = XmlMrf.GetChildParameterizedAsset(node, "FrameFilter");
            SynchronizerType = Xml.GetChildEnumInnerText<MrfSynchronizerType>(node, "SynchronizerType");
            if (SynchronizerType == MrfSynchronizerType.Tag)
            {
                SynchronizerTagFlags = Xml.GetChildEnumInnerText<MrfSynchronizerTagFlags>(node, "SynchronizerTagFlags");
            }
            UnkFlag6 = Xml.GetChildBoolAttribute(node, "UnkFlag6");
            UnkFlag7 = Xml.GetChildUIntAttribute(node, "UnkFlag7");
            UnkFlag21 = Xml.GetChildUIntAttribute(node, "UnkFlag21");
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.StringTag(sb, indent, "Child0InfluenceOverride", Child0InfluenceOverride.ToString());
            MrfXml.StringTag(sb, indent, "Child1InfluenceOverride", Child1InfluenceOverride.ToString());
            MrfXml.ParameterizedAssetTag(sb, indent, "FrameFilter", FrameFilterType, FrameFilterDictionaryName, FrameFilterName, FrameFilterParameterName);
            MrfXml.StringTag(sb, indent, "SynchronizerType", SynchronizerType.ToString());
            if (SynchronizerType == MrfSynchronizerType.Tag)
            {
                MrfXml.StringTag(sb, indent, "SynchronizerTagFlags", SynchronizerTagFlags.ToString());
            }
            MrfXml.ValueTag(sb, indent, "UnkFlag6", UnkFlag6.ToString());
            MrfXml.ValueTag(sb, indent, "UnkFlag7", UnkFlag7.ToString());
            MrfXml.ValueTag(sb, indent, "UnkFlag21", UnkFlag21.ToString());
        }
    }

    [TC(typeof(EXP))] public class MrfNodePose : MrfNodeWithFlagsBase
    {
        // rage__mvNodePose (25)

        public uint Unk1 { get; set; } // unused, with type==Literal always 0x01000000, probably a bool for the Pose_IsNormalized parameter hardcoded to true in the final game

        // flags getters and setters
        public MrfValueType Unk1Type
        {
            get => (MrfValueType)GetFlagsSubset(0, 3);
            set => SetFlagsSubset(0, 3, (uint)value);
        }

        public MrfNodePose() : base(MrfNodeType.Pose) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            if (Unk1Type != MrfValueType.None)
                Unk1 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            if (Unk1Type != MrfValueType.None)
                w.Write(Unk1);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unk1Type = MrfValueType.Literal; // for roundtripness
            Unk1 = 0x01000000;
        }
    }

    [TC(typeof(EXP))] public class MrfNodeMergeN : MrfNodeNBase
    {
        // rage__mvNodeMergeN (26)

        public MrfNodeMergeN() : base(MrfNodeType.MergeN) { }
    }

    [TC(typeof(EXP))] public class MrfNodeState : MrfNodeStateBase
    {
        // rage__mvNodeState (27)

        public int InputParametersOffset { get; set; }
        public int InputParametersFileOffset { get; set; }
        public uint InputParameterCount { get; set; }
        public int EventsOffset { get; set; }
        public int EventsFileOffset { get; set; }
        public uint EventCount { get; set; }
        public int OutputParametersOffset { get; set; }
        public int OutputParametersFileOffset { get; set; }
        public uint OutputParameterCount { get; set; }
        public int OperationsOffset { get; set; }
        public int OperationsFileOffset { get; set; }
        public uint OperationCount { get; set; }

        public MrfStateInputParameter[] InputParameters { get; set; }
        public MrfStateEvent[] Events { get; set; }
        public MrfStateOutputParameter[] OutputParameters { get; set; }
        public MrfStateOperation[] Operations { get; set; }

        public MrfNodeState() : base(MrfNodeType.State) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            InputParametersOffset = r.ReadInt32();
            InputParametersFileOffset = (int)(r.Position + InputParametersOffset - 4);
            InputParameterCount = r.ReadUInt32();
            EventsOffset = r.ReadInt32();
            EventsFileOffset = (int)(r.Position + EventsOffset - 4);
            EventCount = r.ReadUInt32();
            OutputParametersOffset = r.ReadInt32();
            OutputParametersFileOffset = (int)(r.Position + OutputParametersOffset - 4);
            OutputParameterCount = r.ReadUInt32();
            OperationsOffset = r.ReadInt32();
            OperationsFileOffset = (int)(r.Position + OperationsOffset - 4);
            OperationCount = r.ReadUInt32();


            if (TransitionCount > 0)
            {
                if (r.Position != TransitionsFileOffset)
                { } // no hits

                Transitions = new MrfStateTransition[TransitionCount];
                for (int i = 0; i < TransitionCount; i++)
                    Transitions[i] = new MrfStateTransition(r);
            }

            if (InputParameterCount > 0)
            {
                if (r.Position != InputParametersFileOffset)
                { } // no hits

                InputParameters = new MrfStateInputParameter[InputParameterCount];
                for (int i = 0; i < InputParameterCount; i++)
                    InputParameters[i] = new MrfStateInputParameter(r);
            }

            if (EventCount > 0)
            {
                if (r.Position != EventsFileOffset)
                { } // no hits

                Events = new MrfStateEvent[EventCount];
                for (int i = 0; i < EventCount; i++)
                    Events[i] = new MrfStateEvent(r);
            }

            if (OutputParameterCount > 0)
            {
                if (r.Position != OutputParametersFileOffset)
                { } // no hits

                OutputParameters = new MrfStateOutputParameter[OutputParameterCount];
                for (int i = 0; i < OutputParameterCount; i++)
                    OutputParameters[i] = new MrfStateOutputParameter(r);
            }

            if (OperationCount > 0)
            {
                if (r.Position != OperationsFileOffset)
                { } // no hits

                Operations = new MrfStateOperation[OperationCount];
                for (int i = 0; i < OperationCount; i++)
                    Operations[i] = new MrfStateOperation(r);
            }
        }

        public override void Write(DataWriter w)
        {
            TransitionCount = (byte)(Transitions?.Length ?? 0);
            InputParameterCount = (uint)(InputParameters?.Length ?? 0);
            EventCount = (uint)(Events?.Length ?? 0);
            OutputParameterCount = (uint)(OutputParameters?.Length ?? 0);
            OperationCount = (uint)(Operations?.Length ?? 0);

            base.Write(w);

            w.Write(InputParametersOffset);
            w.Write(InputParameterCount);
            w.Write(EventsOffset);
            w.Write(EventCount);
            w.Write(OutputParametersOffset);
            w.Write(OutputParameterCount);
            w.Write(OperationsOffset);
            w.Write(OperationCount);

            if (Transitions != null)
                foreach (var transition in Transitions)
                    transition.Write(w);

            if (InputParameters != null)
                foreach (var item in InputParameters)
                    item.Write(w);

            if (Events != null)
                foreach (var item in Events)
                    item.Write(w);

            if (OutputParameters != null)
                foreach (var item in OutputParameters)
                    item.Write(w);

            if (Operations != null)
                foreach (var item in Operations)
                    item.Write(w);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            
            InitialNode = XmlMrf.ReadChildNode(node, "InitialNode");
            Transitions = XmlMeta.ReadItemArray<MrfStateTransition>(node, "Transitions");
            InputParameters = XmlMeta.ReadItemArray<MrfStateInputParameter>(node, "InputParameters");
            OutputParameters = XmlMeta.ReadItemArray<MrfStateOutputParameter>(node, "OutputParameters");
            Events = XmlMeta.ReadItemArray<MrfStateEvent>(node, "Events");
            Operations = XmlMeta.ReadItemArray<MrfStateOperation>(node, "Operations");
            TransitionCount = (byte)(Transitions?.Length ?? 0);
            InputParameterCount = (byte)(InputParameters?.Length ?? 0);
            OutputParameterCount = (byte)(InputParameters?.Length ?? 0);
            EventCount = (byte)(InputParameters?.Length ?? 0);
            OperationCount = (byte)(Operations?.Length ?? 0);
            StateChildCount = (byte)GetChildren(excludeTailNodes: true).Count;
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.WriteNode(sb, indent, "InitialNode", InitialNode);
            MrfXml.WriteItemArray(sb, Transitions, indent, "Transitions");
            MrfXml.WriteItemArray(sb, InputParameters, indent, "InputParameters");
            MrfXml.WriteItemArray(sb, OutputParameters, indent, "OutputParameters");
            MrfXml.WriteItemArray(sb, Events, indent, "Events");
            MrfXml.WriteItemArray(sb, Operations, indent, "Operations");
        }

        public override void ResolveRelativeOffsets(MrfFile mrf)
        {
            base.ResolveRelativeOffsets(mrf);

            ResolveNodeOffsetsInTransitions(Transitions, mrf);
        }

        public override void UpdateRelativeOffsets()
        {
            base.UpdateRelativeOffsets();

            var offset = FileOffset + 0x20/*sizeof(MrfNodeStateBase)*/ + 8*4/*all offsets/counts*/;

            offset = UpdateNodeOffsetsInTransitions(Transitions, offset, offsetSetToZeroIfNoTransitions: false);

            InputParametersFileOffset = offset;
            InputParametersOffset = InputParametersFileOffset - (FileOffset + 0x20 + 0);
            offset += (int)InputParameterCount * 0xC;

            EventsFileOffset = offset;
            EventsOffset = EventsFileOffset - (FileOffset + 0x20 + 8);
            offset += (int)EventCount * 0x8;

            OutputParametersFileOffset = offset;
            OutputParametersOffset = OutputParametersFileOffset - (FileOffset + 0x20 + 0x10);
            offset += (int)OutputParameterCount * 0xC;

            OperationsFileOffset = offset;
            OperationsOffset = OperationsFileOffset - (FileOffset + 0x20 + 0x18);
        }

        public List<MrfNode> GetChildren(bool excludeTailNodes)
        {
            var result = new List<MrfNode>();
            if (InitialNode == null) return result;

            var q = new Queue<MrfNode>();
            q.Enqueue(InitialNode);
            while (q.Count > 0)
            {
                var n = q.Dequeue();
                if (!excludeTailNodes || !(n is MrfNodeTail))
                {
                    result.Add(n);
                }

                if (n is MrfNodeWithChildBase nc)
                {
                    q.Enqueue(nc.Child);
                }
                else if (n is MrfNodePairBase np)
                {
                    q.Enqueue(np.Child0);
                    q.Enqueue(np.Child1);
                }
                else if (n is MrfNodeNBase nn)
                {
                    foreach (var c in nn.Children)
                    {
                        q.Enqueue(c);
                    }
                }
                else if (n is MrfNodeInlinedStateMachine ism)
                {
                    q.Enqueue(ism.FallbackNode);
                }
            }

            return result;
        }
    }

    [TC(typeof(EXP))] public class MrfNodeInvalid : MrfNode
    {
        // rage__mvNodeInvalid (28)

        public MrfNodeInvalid() : base(MrfNodeType.Invalid) { }
    }

    [TC(typeof(EXP))] public class MrfNodeJointLimit : MrfNodeWithChildAndFilterBase
    {
        // rage__mvNodeJointLimit (29)

        public MrfNodeJointLimit() : base(MrfNodeType.JointLimit) { }
    }

    [TC(typeof(EXP))] public class MrfNodeSubNetwork : MrfNode
    {
        // rage__mvNodeSubNetworkClass (30)

        public MetaHash SubNetworkParameterName { get; set; } // parameter of type rage::mvSubNetwork to lookup

        public MrfNodeSubNetwork() : base(MrfNodeType.SubNetwork) { }

        public override void Read(DataReader r)
        {
            base.Read(r);
            SubNetworkParameterName = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(SubNetworkParameterName);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            SubNetworkParameterName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "SubNetworkParameterName"));
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            MrfXml.StringTag(sb, indent, "SubNetworkParameterName", MrfXml.HashString(SubNetworkParameterName));
        }
    }

    [TC(typeof(EXP))] public class MrfNodeReference : MrfNode
    {
        // rage__mvNodeReference (31)

        // Unused in the final game but from testing, seems to work fine initially but when it finishes it crashes calling a pure virtual function rage::crmtNode::GetNodeTypeInfo
        // Maybe some kind of double-free/use-after-free bug, not sure if a R* bug or an issue with the generated MRF file.

        public MetaHash MoveNetworkName { get; set; } // .mrf file to lookup. Its RootState must be MrfNodeState, not MrfNodeStateMachine or it will crash
        public int InitialParametersOffset { get; set; }
        public int InitialParametersFileOffset { get; set; }
        public uint InitialParameterCount { get; set; }
        public MrfNodeReferenceInitialParameter[] InitialParameters { get; set; } // parameters added when the new network is created
        public uint ImportedParameterCount { get; set; }
        public MrfNodeReferenceNamePair[] ImportedParameters { get; set; } // each update these parameters are copied from the parent network to the new network
        public uint MoveNetworkFlagCount { get; set; }
        public MrfNodeReferenceNamePair[] MoveNetworkFlags { get; set; } // each update copies flag 'Name' state in the parent network to another bit in the *parent* network flags, the new bit position is defined by 'NewName' in the MrfFile.MoveNetworkFlags of the new network
        public uint MoveNetworkTriggerCount { get; set; }
        public MrfNodeReferenceNamePair[] MoveNetworkTriggers { get; set; } // same as with the flags

        public MrfNodeReference() : base(MrfNodeType.Reference) { }

        public override void Read(DataReader r)
        {
            base.Read(r);

            MoveNetworkName = r.ReadUInt32();
            InitialParametersOffset = r.ReadInt32();
            InitialParametersFileOffset = (int)(r.Position + InitialParametersOffset - 4);
            InitialParameterCount = r.ReadUInt32();
            ImportedParameterCount = r.ReadUInt32();
            MoveNetworkFlagCount = r.ReadUInt32();
            MoveNetworkTriggerCount = r.ReadUInt32();

            if (ImportedParameterCount > 0)
            {
                ImportedParameters = new MrfNodeReferenceNamePair[ImportedParameterCount];

                for (int i = 0; i < ImportedParameterCount; i++)
                {
                    var name = r.ReadUInt32();
                    var newName = r.ReadUInt32();
                    ImportedParameters[i] = new MrfNodeReferenceNamePair(name, newName);
                }
            }

            if (MoveNetworkFlagCount > 0)
            {
                MoveNetworkFlags = new MrfNodeReferenceNamePair[MoveNetworkFlagCount];

                for (int i = 0; i < MoveNetworkFlagCount; i++)
                {
                    var name = r.ReadUInt32();
                    var newName = r.ReadUInt32();
                    MoveNetworkFlags[i] = new MrfNodeReferenceNamePair(name, newName);
                }
            }

            if (MoveNetworkTriggerCount > 0)
            {
                MoveNetworkTriggers = new MrfNodeReferenceNamePair[MoveNetworkTriggerCount];

                for (int i = 0; i < MoveNetworkTriggerCount; i++)
                {
                    var name = r.ReadUInt32();
                    var newName = r.ReadUInt32();
                    MoveNetworkTriggers[i] = new MrfNodeReferenceNamePair(name, newName);
                }
            }

            if (InitialParameterCount > 0)
            {
                if (r.Position != InitialParametersFileOffset)
                { }

                InitialParameters = new MrfNodeReferenceInitialParameter[InitialParameterCount];

                for (int i = 0; i < InitialParameterCount; i++)
                {
                    var type = r.ReadUInt32();
                    var name = r.ReadUInt32();
                    var data = r.ReadInt32();
                    InitialParameters[i] = new MrfNodeReferenceInitialParameter(type, name, data);
                }
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            InitialParameterCount = (uint)(InitialParameters?.Length ?? 0);
            ImportedParameterCount = (uint)(ImportedParameters?.Length ?? 0);
            MoveNetworkFlagCount = (uint)(MoveNetworkFlags?.Length ?? 0);
            MoveNetworkTriggerCount = (uint)(MoveNetworkTriggers?.Length ?? 0);

            w.Write(MoveNetworkName);
            w.Write(InitialParametersOffset);
            w.Write(InitialParameterCount);
            w.Write(ImportedParameterCount);
            w.Write(MoveNetworkFlagCount);
            w.Write(MoveNetworkTriggerCount);

            if (ImportedParameterCount > 0)
            {
                foreach (var entry in ImportedParameters)
                {
                    w.Write(entry.Name);
                    w.Write(entry.NewName);
                }
            }

            if (MoveNetworkFlagCount > 0)
            {
                foreach (var entry in MoveNetworkFlags)
                {
                    w.Write(entry.Name);
                    w.Write(entry.NewName);
                }
            }

            if (MoveNetworkTriggerCount > 0)
            {
                foreach (var entry in MoveNetworkTriggers)
                {
                    w.Write(entry.Name);
                    w.Write(entry.NewName);
                }
            }

            if (InitialParameterCount > 0)
            {
                // FIXME: Data when used as offset is not updated and not sure where what it would point to should be written
                foreach (var entry in InitialParameters)
                {
                    w.Write(entry.Type);
                    w.Write(entry.Name);
                    w.Write(entry.Data);
                }
            }
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);

            MoveNetworkName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "MoveNetworkName"));
            ImportedParameters = XmlMeta.ReadItemArray<MrfNodeReferenceNamePair>(node, "ImportedParameters");
            MoveNetworkFlags = XmlMeta.ReadItemArray<MrfNodeReferenceNamePair>(node, "MoveNetworkFlags");
            MoveNetworkTriggers = XmlMeta.ReadItemArray<MrfNodeReferenceNamePair>(node, "MoveNetworkTriggers");
            InitialParameters = XmlMeta.ReadItemArray<MrfNodeReferenceInitialParameter>(node, "InitialParameters");
            ImportedParameterCount = (uint)(ImportedParameters?.Length ?? 0);
            MoveNetworkFlagCount = (uint)(MoveNetworkFlags?.Length ?? 0);
            MoveNetworkTriggerCount = (uint)(MoveNetworkTriggers?.Length ?? 0);
            InitialParameterCount = (uint)(InitialParameters?.Length ?? 0);
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);

            MrfXml.StringTag(sb, indent, "MoveNetworkName", MrfXml.HashString(MoveNetworkName));
            MrfXml.WriteItemArray(sb, ImportedParameters, indent, "ImportedParameters");
            MrfXml.WriteItemArray(sb, MoveNetworkFlags, indent, "MoveNetworkFlags");
            MrfXml.WriteItemArray(sb, MoveNetworkTriggers, indent, "MoveNetworkTriggers");
            MrfXml.WriteItemArray(sb, InitialParameters, indent, "InitialParameters");
        }

        public override void UpdateRelativeOffsets()
        {
            base.UpdateRelativeOffsets();

            var offset = FileOffset + 0x20;
            offset += (int)ImportedParameterCount * 8;
            offset += (int)MoveNetworkFlagCount * 8;
            offset += (int)MoveNetworkTriggerCount * 8;
            InitialParametersFileOffset = offset;
            InitialParametersOffset = InitialParametersFileOffset - (FileOffset + 0xC);
        }
    }

    [TC(typeof(EXP))] public struct MrfNodeReferenceNamePair : IMetaXmlItem
    {
        public MetaHash Name { get; set; } // name in the parent network
        public MetaHash NewName { get; set; } // name in the new network

        public MrfNodeReferenceNamePair(MetaHash name, MetaHash newName)
        {
            Name = name;
            NewName = newName;
        }

        public void ReadXml(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            NewName = XmlMeta.GetHash(Xml.GetChildInnerText(node, "NewName"));
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.StringTag(sb, indent, "Name", MrfXml.HashString(Name));
            MrfXml.StringTag(sb, indent, "NewName", MrfXml.HashString(NewName));
        }

        public override string ToString()
        {
            return $"{Name} - {NewName}";
        }
    }

    [TC(typeof(EXP))] public struct MrfNodeReferenceInitialParameter : IMetaXmlItem
    {
        public uint Type { get; set; } // Animation = 0, Clip = 1, Expression = 2, Motion = 4, Float = 7
        public MetaHash Name { get; set; }
        public int Data { get; set; } //  For Type==Float, this the float value. For the other types, this is the offset to the actual data needed for the lookup (e.g. dictionary/name hash pair).

        public MrfNodeReferenceInitialParameter(uint type, MetaHash name, int data)
        {
            Type = type;
            Name = name;
            Data = data;
        }

        public void ReadXml(XmlNode node)
        {
            Type = Xml.GetChildUIntAttribute(node, "Type");
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            Data = Xml.GetChildIntAttribute(node, "Data");
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            MrfXml.ValueTag(sb, indent, "Type", Type.ToString());
            MrfXml.StringTag(sb, indent, "Name", MrfXml.HashString(Name));
            MrfXml.ValueTag(sb, indent, "Data", Data.ToString());
        }

        public override string ToString()
        {
            return $"{Type} - {Name} - {Data}";
        }
    }

#endregion



    public class MrfXml : MetaXmlBase
    {
        public static string GetXml(MrfFile mrf)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (mrf != null)
            {
                var name = "MoveNetwork";

                OpenTag(sb, 0, name);

                mrf.WriteXml(sb, 1);

                CloseTag(sb, 0, name);
            }

            return sb.ToString();
        }

        public static void WriteNode(StringBuilder sb, int indent, string name, MrfNode node)
        {
            OpenTag(sb, indent, name + " type=\"" + node.NodeType + "\"");
            node.WriteXml(sb, indent + 1);
            CloseTag(sb, indent, name);
        }

        public static void WriteNodeRef(StringBuilder sb, int indent, string name, MrfNode node)
        {
            Indent(sb, indent);
            sb.Append("<");
            sb.Append(name);
            sb.Append(" ref=\"");
            sb.Append(HashString(node.Name));
            sb.Append("\" />");
            sb.AppendLine();
        }
        public static void WriteCondition(StringBuilder sb, int indent, string name, MrfCondition condition)
        {
            OpenTag(sb, indent, name + " type=\"" + condition.Type + "\"");
            condition.WriteXml(sb, indent + 1);
            CloseTag(sb, indent, name);
        }
        public static void WriteOperator(StringBuilder sb, int indent, string name, MrfStateOperator op)
        {
            OpenTag(sb, indent, name + " type=\"" + op.Type + "\"");
            op.WriteXml(sb, indent + 1);
            CloseTag(sb, indent, name);
        }

        public static void ParameterizedFloatTag(StringBuilder sb, int indent, string name, MrfValueType type, float value, MetaHash parameter)
        {
            switch (type)
            {
                case MrfValueType.None: SelfClosingTag(sb, indent, name); break;
                case MrfValueType.Literal: ValueTag(sb, indent, name, FloatUtil.ToString(value), "value"); break;
                case MrfValueType.Parameter: ValueTag(sb, indent, name, HashString(parameter), "parameter"); break;
            }
        }

        public static void ParameterizedBoolTag(StringBuilder sb, int indent, string name, MrfValueType type, bool value, MetaHash parameter)
        {
            switch (type)
            {
                case MrfValueType.None: SelfClosingTag(sb, indent, name); break;
                case MrfValueType.Literal: ValueTag(sb, indent, name, value.ToString(), "value"); break;
                case MrfValueType.Parameter: ValueTag(sb, indent, name, HashString(parameter), "parameter"); break;
            }
        }

        public static void ParameterizedAssetTag(StringBuilder sb, int indent, string name, MrfValueType type, MetaHash dictionaryName, MetaHash assetName, MetaHash parameter)
        {
            switch (type)
            {
                case MrfValueType.None: SelfClosingTag(sb, indent, name); break;
                case MrfValueType.Literal:
                    OpenTag(sb, indent, name);
                    StringTag(sb, indent + 1, "DictionaryName", HashString(dictionaryName));
                    StringTag(sb, indent + 1, "Name", HashString(assetName));
                    CloseTag(sb, indent, name);
                    break;
                case MrfValueType.Parameter: ValueTag(sb, indent, name, HashString(parameter), "parameter"); break;
            }
        }

        public static void ParameterizedClipTag(StringBuilder sb, int indent, string name, MrfValueType type, MrfClipContainerType containerType, MetaHash containerName, MetaHash clipName, MetaHash parameter)
        {
            switch (type)
            {
                case MrfValueType.None: SelfClosingTag(sb, indent, name); break;
                case MrfValueType.Literal:
                    OpenTag(sb, indent, name);
                    StringTag(sb, indent + 1, "ContainerType", containerType.ToString());
                    StringTag(sb, indent + 1, "ContainerName", HashString(containerName));
                    StringTag(sb, indent + 1, "Name", HashString(clipName));
                    CloseTag(sb, indent, name);
                    break;
                case MrfValueType.Parameter: ValueTag(sb, indent, name, HashString(parameter), "parameter"); break;
            }
        }
    }

    public class XmlMrf
    {
        public static MrfFile GetMrf(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetMrf(doc);
        }

        public static MrfFile GetMrf(XmlDocument doc)
        {
            MrfFile mrf = new MrfFile();
            mrf.ReadXml(doc.DocumentElement);
            return mrf;
        }

        public static MrfNode ReadChildNode(XmlNode node, string name)
        {
            return ReadNode(node.SelectSingleNode(name));
        }
        public static MrfNode ReadNode(XmlNode node)
        {
            if (node != null && Enum.TryParse<MrfNodeType>(Xml.GetStringAttribute(node, "type"), out var type))
            {
                var n = MrfFile.CreateNode(type);
                n.ReadXml(node);
                return n;
            }

            return null;
        }
        public static MetaHash ReadChildNodeRef(XmlNode node, string name)
        {
            return ReadNodeRef(node.SelectSingleNode(name));
        }
        public static MetaHash ReadNodeRef(XmlNode node)
        {
            var name = XmlMeta.GetHash(Xml.GetStringAttribute(node, "ref"));
            return name;
        }
        public static MrfCondition ReadCondition(XmlNode node)
        {
            if (node != null && Enum.TryParse<MrfConditionType>(Xml.GetStringAttribute(node, "type"), out var type))
            {
                var n = MrfCondition.CreateCondition(type);
                n.ReadXml(node);
                return n;
            }

            return null;
        }
        public static MrfStateOperator ReadOperator(XmlNode node)
        {
            if (node != null && Enum.TryParse<MrfOperatorType>(Xml.GetStringAttribute(node, "type"), out var type))
            {
                var op = MrfStateOperator.CreateOperator(type);
                op.ReadXml(node);
                return op;
            }

            return null;
        }

        public static (MrfValueType Type, float Value, MetaHash ParameterName) GetChildParameterizedFloat(XmlNode node, string name)
        {
            var type = MrfValueType.None;
            var value = 0.0f;
            var parameter = default(MetaHash);

            var childNode = node.SelectSingleNode(name);
            if (childNode?.Attributes["value"] != null)
            {
                type = MrfValueType.Literal;
                value = Xml.GetFloatAttribute(childNode, "value");
            }
            else if (childNode?.Attributes["parameter"] != null)
            {
                type = MrfValueType.Parameter;
                parameter = XmlMeta.GetHash(Xml.GetStringAttribute(childNode, "parameter"));
            }

            return (type, value, parameter);
        }

        public static (MrfValueType Type, bool Value, MetaHash ParameterName) GetChildParameterizedBool(XmlNode node, string name)
        {
            var type = MrfValueType.None;
            var value = false;
            var parameter = default(MetaHash);

            var childNode = node.SelectSingleNode(name);
            if (childNode?.Attributes["value"] != null)
            {
                type = MrfValueType.Literal;
                value = Xml.GetBoolAttribute(childNode, "value");
            }
            else if (childNode?.Attributes["parameter"] != null)
            {
                type = MrfValueType.Parameter;
                parameter = XmlMeta.GetHash(Xml.GetStringAttribute(childNode, "parameter"));
            }

            return (type, value, parameter);
        }

        public static (MrfValueType Type, MetaHash DictionaryName, MetaHash AssetName, MetaHash ParameterName) GetChildParameterizedAsset(XmlNode node, string name)
        {
            var type = MrfValueType.None;
            var dictionaryName = default(MetaHash);
            var assetName = default(MetaHash);
            var parameter = default(MetaHash);

            var childNode = node.SelectSingleNode(name);
            var dictionaryNode = childNode?.SelectSingleNode("DictionaryName");
            var nameNode = childNode?.SelectSingleNode("Name");
            if (dictionaryNode != null && nameNode != null)
            {
                type = MrfValueType.Literal;
                dictionaryName = XmlMeta.GetHash(dictionaryNode.InnerText);
                assetName = XmlMeta.GetHash(nameNode.InnerText);
            }
            else if (childNode?.Attributes["parameter"] != null)
            {
                type = MrfValueType.Parameter;
                parameter = XmlMeta.GetHash(Xml.GetStringAttribute(childNode, "parameter"));
            }

            return (type, dictionaryName, assetName, parameter);
        }

        public static (MrfValueType Type, MrfClipContainerType ContainerType, MetaHash ContainerName, MetaHash ClipName, MetaHash ParameterName) GetChildParameterizedClip(XmlNode node, string name)
        {
            var type = MrfValueType.None;
            var containerType = default(MrfClipContainerType);
            var containerName = default(MetaHash);
            var assetName = default(MetaHash);
            var parameter = default(MetaHash);

            var childNode = node.SelectSingleNode(name);
            var containerTypeNode = childNode?.SelectSingleNode("ContainerType");
            var containerNode = childNode?.SelectSingleNode("ContainerName");
            var nameNode = childNode?.SelectSingleNode("Name");
            if (containerTypeNode != null && containerNode != null && nameNode != null)
            {
                type = MrfValueType.Literal;
                containerType = Xml.GetEnumValue<MrfClipContainerType>(containerTypeNode.InnerText);
                containerName = XmlMeta.GetHash(containerNode.InnerText);
                assetName = XmlMeta.GetHash(nameNode.InnerText);
            }
            else if (childNode?.Attributes["parameter"] != null)
            {
                type = MrfValueType.Parameter;
                parameter = XmlMeta.GetHash(Xml.GetStringAttribute(childNode, "parameter"));
            }

            return (type, containerType, containerName, assetName, parameter);
        }
    }
}
