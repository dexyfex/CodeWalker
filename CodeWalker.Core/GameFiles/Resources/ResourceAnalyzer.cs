using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ResourceAnalyzer
    {

        public ResourceAnalyzerItem[] Blocks { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class ResourceAnalyzerItem
        {
            public long Position { get; set; }
            public long Length { get; set; }
            public long Offset { get { return Position & 0xFFFFFFF; } }
            public bool Overlapping { get; set; }
            public ResourceSystemBlock SystemBlock { get; set; }
            public ResourceGraphicsBlock GraphicsBlock { get; set; }
            public Array Array { get; set; }


            public override string ToString()
            {
                var type = "########## ??? ##########";
                if (SystemBlock != null)
                {
                    type = SystemBlock.GetType().Name;
                }
                if (GraphicsBlock != null)
                {
                    type = GraphicsBlock.GetType().Name;
                }
                if (Array != null)
                {
                    type = Array.GetType().Name + " (" + Array.Length.ToString() + ")";
                }
                return Offset.ToString() + " - " + Length.ToString() + " - " + type + (Overlapping ? "   (embedded)" : "");
            }
        }

        public ResourceAnalyzer(ResourceDataReader reader)
        {
            var dlist = new List<ResourceAnalyzerItem>();
            foreach (var kvp in reader.blockPool)
            {
                var item = new ResourceAnalyzerItem();
                item.Position = kvp.Key;
                item.Length = kvp.Value.BlockLength;
                item.SystemBlock = kvp.Value as ResourceSystemBlock;
                item.GraphicsBlock = kvp.Value as ResourceGraphicsBlock;
                dlist.Add(item);
            }
            foreach (var kvp in reader.arrayPool)
            {
                var item = new ResourceAnalyzerItem();
                item.Position = kvp.Key;
                item.Array = kvp.Value as Array;
                if (item.Array != null)
                {
                    var typ = item.Array.GetType().GetElementType();
                    var siz = Marshal.SizeOf(typ);
                    item.Length = item.Array.Length * siz;
                }
                dlist.Add(item);
            }

            dlist.Sort((a, b) => a.Position.CompareTo(b.Position));

            //Blocks = dlist.ToArray();


            var dlist2 = new List<ResourceAnalyzerItem>();
            long pos = 0;
            foreach (var item in dlist)
            {
                if (item.Offset > pos)
                {
                    var gap = new ResourceAnalyzerItem();
                    gap.Position = pos;
                    gap.Length = item.Offset - pos;
                    dlist2.Add(gap);
                    pos = item.Offset;
                }
                if (item.Offset == pos)
                {
                    dlist2.Add(item);
                    pos = item.Offset + item.Length;
                    if ((pos % 16) != 0) pos += (16 - (pos % 16));//ignore alignment paddings
                }
                else
                {
                    item.Overlapping = true;
                    dlist2.Add(item);
                    var pos2 = item.Offset + item.Length;
                    if (pos2 > pos) pos = pos2;
                }
            }


            Blocks = dlist2.ToArray();
        }


    }
}
