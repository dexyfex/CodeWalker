using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            public ResourceSystemBlock SystemBlock { get; set; }
            public ResourceGraphicsBlock GraphicsBlock { get; set; }

            public override string ToString()
            {
                var type = "";
                if (SystemBlock != null)
                {
                    type = SystemBlock.GetType().Name;
                }
                if (GraphicsBlock != null)
                {
                    type = GraphicsBlock.GetType().Name;
                }
                return Offset.ToString() + " - " + Length.ToString() + " - " + type;
            }
        }

        public ResourceAnalyzer(ResourceDataReader reader)
        {
            var dlist = new List<ResourceAnalyzerItem>();
            var dict = reader.blockPool;
            foreach (var kvp in dict)
            {
                var item = new ResourceAnalyzerItem();
                item.Position = kvp.Key;
                item.Length = kvp.Value.BlockLength;
                item.SystemBlock = kvp.Value as ResourceSystemBlock;
                item.GraphicsBlock = kvp.Value as ResourceGraphicsBlock;
                dlist.Add(item);
            }

            dlist.Sort((a, b) => a.Position.CompareTo(b.Position));

            Blocks = dlist.ToArray();
        }

    }
}
