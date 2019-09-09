using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SharpDX;

namespace CodeWalker.World
{
    public class Trains
    {
        public volatile bool Inited = false;
        public GameFileCache GameFileCache;

        public List<TrainTrack> TrainTracks { get; set; } = new List<TrainTrack>();


        public void Init(GameFileCache gameFileCache, Action<string> updateStatus)
        {
            GameFileCache = gameFileCache;

            var rpfman = gameFileCache.RpfMan;

            string trainsfilename = "common.rpf\\data\\levels\\gta5\\trains.xml";
            XmlDocument trainsxml = rpfman.GetFileXml(trainsfilename);
            XmlElement trainsdata = trainsxml.DocumentElement;
            //TODO: parse train_configs


            string tracksfilename = "common.rpf\\data\\levels\\gta5\\traintracks.xml";
            XmlDocument tracksxml = rpfman.GetFileXml(tracksfilename);
            XmlElement tracksdata = tracksxml.DocumentElement;
            XmlNodeList tracks = tracksdata.SelectNodes("train_track");

            TrainTracks.Clear();
            for (int i = 0; i < tracks.Count; i++)
            {
                var trackxml = tracks[i];
                TrainTrack tt = new TrainTrack();
                tt.Load(gameFileCache, trackxml);
                TrainTracks.Add(tt);
            }


            Inited = true;
        }

    }

    public class TrainTrack : BasePathData
    {
        public string filename { get; set; }
        public string trainConfigName { get; set; }
        public bool isPingPongTrack { get; set; }
        public bool stopsAtStations { get; set; }
        public bool MPstopsAtStations { get; set; }
        public float speed { get; set; }
        public float brakingDist { get; set; }


        public List<TrainTrackNode> Nodes { get; set; }
        public int NodeCount { get; set; }


        public int StationCount
        {
            get
            {
                int sc = 0;
                if (Nodes != null)
                {
                    foreach (var node in Nodes)
                    {
                        if ((node.NodeType == 1) || (node.NodeType == 2) || (node.NodeType == 5))
                        {
                            sc++;
                        }
                    }
                }
                return sc;
            }
        }


        public EditorVertex[] LinkedVerts { get; set; }
        public Vector4[] NodePositions { get; set; }

        public EditorVertex[] GetPathVertices()
        {
            return LinkedVerts;
        }
        public EditorVertex[] GetTriangleVertices()
        {
            return null;
        }
        public Vector4[] GetNodePositions()
        {
            return NodePositions;
        }

        public PathBVH BVH { get; set; }

        public string NodesString { get; set; }
        public RpfFileEntry RpfFileEntry { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public bool HasChanged { get; set; }
        public bool Loaded { get; set; }




        public void Load(GameFileCache gameFileCache, XmlNode node)
        {
            //load from game file cache

            filename = Xml.GetStringAttribute(node, "filename");
            trainConfigName = Xml.GetStringAttribute(node, "trainConfigName");
            isPingPongTrack = Xml.GetBoolAttribute(node, "isPingPongTrack");
            stopsAtStations = Xml.GetBoolAttribute(node, "stopsAtStations");
            MPstopsAtStations = Xml.GetBoolAttribute(node, "MPstopsAtStations");
            speed = Xml.GetFloatAttribute(node, "speed");
            brakingDist = Xml.GetFloatAttribute(node, "brakingDist");

            RpfFileEntry = gameFileCache.RpfMan.GetEntry(filename) as RpfFileEntry;
            NodesString = gameFileCache.RpfMan.GetFileUTF8Text(filename);
            SetNameFromFilename();
            FilePath = Name;

            Load(NodesString);

            BuildVertices();

            BuildBVH();

            Loaded = true;
        }

        public void Load(byte[] data)
        {
            filename = string.Empty;
            trainConfigName = string.Empty;
            RpfFileEntry = new RpfBinaryFileEntry();
            

            string str = Encoding.UTF8.GetString(data);
            Load(str);

            BuildVertices();

            BuildBVH();

            Loaded = true;
        }

        public byte[] Save()
        {
            NodeCount = Nodes.Count;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Nodes.Count.ToString());
            foreach (var node in Nodes)
            {
                var nstr = FloatUtil.GetVector3String(node.Position).Replace(",","") + " " + node.NodeType.ToString();
                sb.AppendLine(nstr);
            }
            string str = sb.ToString();
            return Encoding.UTF8.GetBytes(str);
        }


        public void SetNameFromFilename()
        {
            string[] fparts = filename.Replace('\\', '/').Split('/');
            if ((fparts == null) || (fparts.Length == 0))
            {
                Name = filename;
            }
            else
            {
                Name = fparts[fparts.Length - 1];
            }
        }


        public void Load(string trackstr)
        {
            //load nodes from a text string...
            NodesString = trackstr;

            if (!string.IsNullOrEmpty(trackstr))
            {
                string[] trackstrs = trackstr.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (trackstrs.Length > 1)
                {
                    int nodecount;
                    int.TryParse(trackstrs[0], out nodecount);
                    NodeCount = nodecount;
                    List<TrainTrackNode> nodes = new List<TrainTrackNode>();
                    for (int i = 1; i < trackstrs.Length; i++)
                    {
                        var nodestr = trackstrs[i].Trim();
                        var nodevals = nodestr.Split(' ');
                        if (nodevals.Length == 4)
                        {
                            TrainTrackNode ttnode = new TrainTrackNode();
                            var x = FloatUtil.Parse(nodevals[0]);
                            var y = FloatUtil.Parse(nodevals[1]);
                            var z = FloatUtil.Parse(nodevals[2]);
                            int nodetype;
                            int.TryParse(nodevals[3], out nodetype);
                            ttnode.Position = new Vector3(x, y, z);
                            ttnode.NodeType = nodetype;
                            ttnode.Track = this;
                            ttnode.Index = nodes.Count;
                            ttnode.Links[0] = (nodes.Count > 0) ? nodes[nodes.Count - 1] : null;
                            if (ttnode.Links[0] != null)
                            {
                                ttnode.Links[0].Links[1] = ttnode;
                            }
                            nodes.Add(ttnode);
                        }
                        else
                        { }
                    }
                    Nodes = nodes;
                }
                else
                { }
            }
            else
            { }

            if (Nodes == null)
            {
                Nodes = new List<TrainTrackNode>();
            }

        }


        public void BuildVertices()
        {
            if ((Nodes != null) && (Nodes.Count > 0))
            {
                var nc = Nodes.Count;
                var lc = nc - 1;
                var lvc = lc * 2;
                var np = new Vector4[nc];
                var lv = new EditorVertex[lvc];
                for (int i = 0; i < nc; i++)
                {
                    np[i] = new Vector4(Nodes[i].Position, 1.0f);
                    if (i > 0)
                    {
                        var l = i - 1;
                        var li = l * 2;
                        var ni = li + 1;
                        lv[li].Position = Nodes[l].Position;
                        lv[ni].Position = Nodes[i].Position;
                        lv[li].Colour = (uint)Nodes[l].GetColour();
                        lv[ni].Colour = (uint)Nodes[i].GetColour();
                    }
                }
                NodePositions = np;
                LinkedVerts = lv;
            }

        }





        public void UpdateBvhForNode(TrainTrackNode node)
        {
            //this needs to be called when a node's position changes...
            //need to recalc the BVH for mouse intersection optimisation purposes.

            //if (BVH == null) return;
            //BVH.UpdateForNode(node);

            BuildBVH();

            //also updates the NodePositions for the visible vertex
            if (Nodes != null)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i] == node)
                    {
                        NodePositions[i] = new Vector4(node.Position, 1.0f);
                        break;
                    }
                }
            }

        }

        public void BuildBVH()
        {
            BVH = new PathBVH(Nodes, 10, 10);
        }






        public TrainTrackNode AddNode(TrainTrackNode afternode = null)
        {
            int cnt = Nodes?.Count ?? 0;
            TrainTrackNode tn = new TrainTrackNode();
            tn.Track = this;
            tn.Index = (afternode != null) ? afternode.Index + 1 : cnt;

            if (Nodes == null)
            {
                Nodes = new List<TrainTrackNode>();
            }

            if (afternode != null)
            {
                TrainTrackNode aln = afternode.Links[1];
                if (aln != null) aln.Links[0] = tn;
                afternode.Links[1] = tn;
                tn.Links[0] = afternode;
                tn.Links[1] = aln;

                int idx = tn.Index;
                Nodes.Insert(idx, tn);

                for (int i = 0; i < Nodes.Count; i++)
                {
                    Nodes[i].Index = i;
                }

            }
            else
            {
                if (cnt > 0)
                {
                    TrainTrackNode ln = Nodes[cnt - 1];
                    tn.Links[0] = ln;
                    ln.Links[1] = tn;
                }

                Nodes.Add(tn);
            }


            NodeCount = Nodes.Count;

            return tn;
        }

        public bool RemoveNode(TrainTrackNode node)
        {
            bool r = false;

            r = Nodes.Remove(node);

            NodeCount = Nodes.Count;
            
            if (r)
            {
                var l0 = node.Links[0];
                var l1 = node.Links[1];

                if (l0 != null)
                {
                    l0.Links[1] = l1;
                }
                if (l1 != null)
                {
                    l1.Links[0] = l0;
                }

                for (int i = 0; i < Nodes.Count; i++)
                {
                    Nodes[i].Index = i;
                }

                BuildVertices();
            }

            return r;
        }





        public override string ToString()
        {
            return Name + ": " + filename + " (" + NodeCount.ToString() + " nodes)";
        }
    }

    public class TrainTrackNode : BasePathNode
    {
        public Vector3 Position { get; set; }
        public int NodeType { get; set; }

        public TrainTrack Track { get; set; }
        public int Index { get; set; }
        public TrainTrackNode[] Links { get; set; } = new TrainTrackNode[2];

        public int GetColour()
        {
            switch (NodeType)
            {
                case 0: return new Color4(1.0f, 0.0f, 0.0f, 1.0f).ToRgba();
                case 1: return new Color4(1.0f, 1.0f, 0.0f, 1.0f).ToRgba();
                case 2: return new Color4(0.0f, 1.0f, 0.0f, 1.0f).ToRgba();
                case 3: return new Color4(0.0f, 1.0f, 1.0f, 1.0f).ToRgba();
                case 4: return new Color4(0.0f, 0.0f, 1.0f, 1.0f).ToRgba();
                case 5: return new Color4(1.0f, 0.0f, 1.0f, 1.0f).ToRgba();
                default: return new Color4(1.0f, 1.0f, 1.0f, 1.0f).ToRgba();
            }
        }


        public void SetPosition(Vector3 pos)
        {
            Position = pos;
        }

        public override string ToString()
        {
            return Index.ToString() + ": " + NodeType.ToString();// + ": " + FloatUtil.GetVector3String(Position);
        }
    }

}
