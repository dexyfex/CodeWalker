using CodeWalker.Core.Utils;
using CodeWalker.GameFiles;
using Collections.Pooled;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.World
{
    public class PopZones : BasePathData
    {
        public volatile bool Inited = false;
        public GameFileCache GameFileCache;

        public Dictionary<string, PopZone> Groups = new Dictionary<string, PopZone>();

        public EditorVertex[] GetTriangleVertices()
        {
            return TriangleVerts;
        }

        public EditorVertex[] TriangleVerts = Array.Empty<EditorVertex>();



        public void Init(GameFileCache gameFileCache, Action<string> updateStatus)
        {
            using var _ = new DisposableTimer("PopZones Init");
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

            //var ipllines = ipltext.Split('\n');
            bool inzone = false;
            foreach (var iplline in ipltext.EnumerateSplit('\n'))
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

                    if (!Groups.TryGetValue(box.NameLabel, out var group))
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
                var hash = JenkHash.GenHashLower(group.NameLabel);
                group.Name = GlobalText.TryGetString(hash);
            }


            BuildVertices();

            Inited = true;
        }



        public void BuildVertices()
        {

            using var vlist = new PooledList<EditorVertex>();

            foreach (var group in Groups.Values)
            {
                var hash = JenkHash.GenHashLower(group.NameLabel);
                byte cr = (byte)((hash >> 8) & 0xFF);
                byte cg = (byte)((hash >> 16) & 0xFF);
                byte cb = (byte)((hash >> 24) & 0xFF);
                byte ca = 60;
                uint cv = (uint)new Color(cr, cg, cb, ca).ToRgba();

                foreach (var box in group.Boxes)
                {
                    var min = box.Box.Minimum;
                    var max = box.Box.Maximum;

                    var v1 = new EditorVertex(new Vector3(min.X, min.Y, 0), cv);
                    var v2 = new EditorVertex(new Vector3(max.X, min.Y, 0), cv);
                    var v3 = new EditorVertex(new Vector3(min.X, max.Y, 0), cv);
                    var v4 = new EditorVertex(new Vector3(max.X, max.Y, 0), cv);

                    vlist.Add(v1);
                    vlist.Add(v2);
                    vlist.Add(v3);
                    vlist.Add(v3);
                    vlist.Add(v2);
                    vlist.Add(v4);
                }
            }

            if (vlist.Count > 0)
            {
                TriangleVerts = vlist.ToArray();
            }
            else
            {
                TriangleVerts = [];
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
            return $"{NameLabel}: {Name}";
        }
    }


    public class PopZoneBox
    {
        public string ID { get; set; }
        public BoundingBox Box { get; set; }
        public string NameLabel { get; set; }
        public float UnkVal { get; set; }

        [SkipLocalsInit]
        public void Init(ReadOnlySpan<char> iplline)
        {
            Span<Range> parts = stackalloc Range[10];
            var numParts = iplline.Split(parts, ',', StringSplitOptions.TrimEntries);
            //var parts = iplline.Split(',');
            if (numParts >= 9)
            {
                ID = iplline[parts[0]].ToString();
                BoundingBox b = new BoundingBox();
                b.Minimum.X = FloatUtil.Parse(iplline[parts[1]]);
                b.Minimum.Y = FloatUtil.Parse(iplline[parts[2]]);
                b.Minimum.Z = FloatUtil.Parse(iplline[parts[3]]);
                b.Maximum.X = FloatUtil.Parse(iplline[parts[4]]);
                b.Maximum.Y = FloatUtil.Parse(iplline[parts[5]]);
                b.Maximum.Z = FloatUtil.Parse(iplline[parts[6]]);
                Box = b;
                NameLabel = iplline[parts[7]].ToString();
                UnkVal = FloatUtil.Parse(iplline[parts[8]]);
            }
        }

        public override string ToString()
        {
            return $"{ID}: {NameLabel}: {Box}";
        }
    }

}
