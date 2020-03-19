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
        public RpfResourceFileEntry FileEntry { get; set; }
        public ResourcePagesInfo FilePagesInfo { get; set; }
        public RpfResourcePage[] SystemPages { get; set; }
        public RpfResourcePage[] GraphicsPages { get; set; }
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
            public string String { get; set; }


            public override string ToString()
            {
                var type = "########## ??? ##########";
                var val = "";
                if (SystemBlock != null)
                {
                    type = SystemBlock.GetType().Name;
                    //val = SystemBlock.ToString();
                }
                else if (GraphicsBlock != null)
                {
                    type = GraphicsBlock.GetType().Name;
                    //val = GraphicsBlock.ToString();
                }
                else if (Array != null)
                {
                    type = Array.GetType().Name + " (" + Array.Length.ToString() + ")";
                }
                else if (String != null)
                {
                    type = "string";
                    val = "\"" + String + "\"";
                }
                var valstr = (string.IsNullOrEmpty(val) ? "" : " - " + val);
                return Offset.ToString() + " - " + Length.ToString() + " - " + type + valstr + (Overlapping ? "   (embedded)" : "");
            }
        }

        public ResourceAnalyzer(ResourceDataReader reader)
        {
            FileEntry = reader.FileEntry;
            SystemPages = FileEntry?.SystemFlags.Pages;
            GraphicsPages = FileEntry?.GraphicsFlags.Pages;

            var dlist = new List<ResourceAnalyzerItem>();
            foreach (var kvp in reader.blockPool)
            {
                var item = new ResourceAnalyzerItem();
                item.Position = kvp.Key;
                item.Length = kvp.Value.BlockLength;
                item.SystemBlock = kvp.Value as ResourceSystemBlock;
                item.GraphicsBlock = kvp.Value as ResourceGraphicsBlock;
                if (kvp.Value is ResourcePagesInfo rpi)
                {
                    item.Length = 16 + (rpi.SystemPagesCount + rpi.GraphicsPagesCount) * 8;
                    FilePagesInfo = rpi;
                }
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
                else
                {
                    item.String = kvp.Value as string;
                    if (item.String != null)
                    {
                        item.Length = item.String.Length + 1;
                    }
                }
                dlist.Add(item);
            }

            dlist.Sort((a, b) => a.Position.CompareTo(b.Position));

            //Blocks = dlist.ToArray();


            var dlist2 = new List<ResourceAnalyzerItem>();
            long pos = 0;
            bool gfx = false;
            foreach (var item in dlist)
            {
                if ((item.GraphicsBlock != null) && (!gfx))
                {
                    pos = 0;
                    gfx = true;
                }
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
                    if (item.String == null)
                    {
                        if ((pos % 16) != 0) pos += (16 - (pos % 16));//ignore alignment paddings
                    }
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
