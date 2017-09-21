using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.World
{
    public class PopZones : BasePathData
    {
        public volatile bool Inited = false;
        public GameFileCache GameFileCache;

        public Dictionary<string, PopZone> Groups = new Dictionary<string, PopZone>();

        public Vector4[] GetNodePositions()
        {
            return null;
        }
        public VertexTypePC[] GetPathVertices()
        {
            return null;
        }
        public VertexTypePC[] GetTriangleVertices()
        {
            return TriangleVerts;
        }

        public VertexTypePC[] TriangleVerts;



        public void Init(GameFileCache gameFileCache, Action<string> updateStatus)
        {
            Inited = false;

            GameFileCache = gameFileCache;

            var rpfman = gameFileCache.RpfMan;

            string filename = "common.rpf\\data\\levels\\gta5\\popzone.ipl";
            if (gameFileCache.EnableDlc)
            {
                filename = "update\\update.rpf\\common\\data\\levels\\gta5\\popzone.ipl";
            }

            string ipltext = rpfman.GetFileUTF8Text(filename);

            if (string.IsNullOrEmpty(ipltext))
            {
                ipltext = "";
            }

            Groups.Clear();

            var ipllines = ipltext.Split('\n');
            bool inzone = false;
            foreach (var iplline in ipllines)
            {
                var linet = iplline.Trim();
                if (linet == "zone")
                {
                    inzone = true;
                }
                else if (linet == "end")
                {
                    inzone = false;
                }
                else if (inzone)
                {
                    PopZoneBox box = new PopZoneBox();
                    box.Init(linet);

                    PopZone group;
                    if (!Groups.TryGetValue(box.NameLabel, out group))
                    {
                        group = new PopZone();
                        group.NameLabel = box.NameLabel;
                        Groups[box.NameLabel] = group;
                    }

                    group.Boxes.Add(box);
                }
            }


            foreach (var group in Groups.Values)
            {
                var hash = JenkHash.GenHash(group.NameLabel.ToLowerInvariant());
                group.Name = GlobalText.TryGetString(hash);
            }


            BuildVertices();

            Inited = true;
        }



        public void BuildVertices()
        {

            foreach (var group in Groups.Values)
            {


            }


        }

    }




    public class PopZone
    {
        public string NameLabel { get; set; }
        public string Name { get; set; } //lookup from gxt2 with label..?
        public List<PopZoneBox> Boxes { get; set; } = new List<PopZoneBox>();

        public override string ToString()
        {
            return NameLabel + ": " + Name;
        }
    }


    public class PopZoneBox
    {
        public string ID { get; set; }
        public BoundingBox Box { get; set; }
        public string NameLabel { get; set; }
        public float UnkVal { get; set; }


        public void Init(string iplline)
        {
            var parts = iplline.Split(',');
            if (parts.Length >= 9)
            {
                ID = parts[0].Trim();
                BoundingBox b = new BoundingBox();
                b.Minimum.X = FloatUtil.Parse(parts[1].Trim());
                b.Minimum.Y = FloatUtil.Parse(parts[2].Trim());
                b.Minimum.Z = FloatUtil.Parse(parts[3].Trim());
                b.Maximum.X = FloatUtil.Parse(parts[4].Trim());
                b.Maximum.Y = FloatUtil.Parse(parts[5].Trim());
                b.Maximum.Z = FloatUtil.Parse(parts[6].Trim());
                Box = b;
                NameLabel = parts[7].Trim();
                UnkVal = FloatUtil.Parse(parts[8].Trim());
            }
        }

        public override string ToString()
        {
            return ID + ": " + NameLabel + ": " + Box.ToString();
        }
    }

}
