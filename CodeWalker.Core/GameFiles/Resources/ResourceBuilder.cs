using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class ResourceBuilder
    {
        protected const int RESOURCE_IDENT = 0x37435352;
        protected const int BASE_SIZE = 0x2000;
        private const int SKIP_SIZE = 512;//256;//64;
        private const int ALIGN_SIZE = 512;//64;


        public static void GetBlocks(IResourceBlock rootBlock, out IList<IResourceBlock> sys, out IList<IResourceBlock> gfx)
        {
            var systemBlocks = new HashSet<IResourceBlock>();
            var graphicBlocks = new HashSet<IResourceBlock>();
            var protectedBlocks = new List<IResourceBlock>();

            var stack = new Stack<IResourceBlock>();
            stack.Push(rootBlock);

            var processed = new HashSet<IResourceBlock>();
            processed.Add(rootBlock);

            while (stack.Count > 0)
            {
                var block = stack.Pop();
                if (block == null)
                    continue;

                if (block is IResourceSystemBlock)
                {
                    if (!systemBlocks.Contains(block))
                        systemBlocks.Add(block);

                    // for system blocks, also process references...

                    var references = ((IResourceSystemBlock)block).GetReferences();
                    //Array.Reverse(references);
                    foreach (var reference in references)
                        if (!processed.Contains(reference))
                        {
                            stack.Push(reference);
                            processed.Add(reference);
                        }
                    var subs = new Stack<IResourceSystemBlock>();
                    foreach (var part in ((IResourceSystemBlock)block).GetParts())
                        subs.Push((IResourceSystemBlock)part.Item2);
                    while (subs.Count > 0)
                    {
                        var sub = subs.Pop();

                        foreach (var x in sub.GetReferences())
                            if (!processed.Contains(x))
                            {
                                stack.Push(x);
                                processed.Add(x);
                            }
                        foreach (var x in sub.GetParts())
                            subs.Push((IResourceSystemBlock)x.Item2);

                        protectedBlocks.Add(sub);
                    }

                }
                else
                {
                    if (!graphicBlocks.Contains(block))
                        graphicBlocks.Add(block);
                }
            }

            //var result = new List<IResourceBlock>();
            //result.AddRange(systemBlocks);
            //result.AddRange(graphicBlocks);
            //return result;

            // there are now sys-blocks in the list that actually
            // only substructures and therefore must not get
            // a new position!
            // -> remove them from the list
            foreach (var q in protectedBlocks)
                if (systemBlocks.Contains(q))
                    systemBlocks.Remove(q);


            sys = new List<IResourceBlock>();
            foreach (var s in systemBlocks)
                sys.Add(s);
            gfx = new List<IResourceBlock>();
            foreach (var s in graphicBlocks)
                gfx.Add(s);
        }

        public static void AssignPositions(IList<IResourceBlock> blocks, uint basePosition, ref int pageSize, out int pageCount)
        {
            // find largest structure
            long largestBlockSize = 0;
            foreach (var block in blocks)
            {
                if (largestBlockSize < block.BlockLength)
                    largestBlockSize = block.BlockLength;
            }

            // find minimum page size
            long currentPageSize = pageSize;// 0x2000;
            while (currentPageSize < largestBlockSize)
                currentPageSize *= 2;

            long currentPageCount;
            long currentPosition;
            while (true)
            {
                currentPageCount = 0;
                currentPosition = 0;

                // reset all positions
                foreach (var block in blocks)
                    block.FilePosition = -1;

                foreach (var block in blocks)
                {
                    if (block.FilePosition != -1)
                        throw new Exception("A position of -1 is not possible!");
                    //if (block.Length == 0)
                    //    throw new Exception("A length of 0 is not allowed!");

                    // check if new page is necessary...
                    // if yes, add a new page and align to it
                    long maxSpace = currentPageCount * currentPageSize - currentPosition;
                    if (maxSpace < (block.BlockLength + SKIP_SIZE))
                    {
                        currentPageCount++;
                        currentPosition = currentPageSize * (currentPageCount - 1);
                    }

                    // set position
                    block.FilePosition = basePosition + currentPosition;
                    currentPosition += block.BlockLength + SKIP_SIZE;

                    // align...
                    if ((currentPosition % ALIGN_SIZE) != 0)
                        currentPosition += (ALIGN_SIZE - (currentPosition % ALIGN_SIZE));
                }

                // break if everything fits...
                if (currentPageCount < 128)
                    break;

                currentPageSize *= 2;
            }

            pageSize = (int)currentPageSize;
            pageCount = (int)currentPageCount;
        }



        public static byte[] Build(ResourceFileBase fileBase, int version)
        {

            fileBase.FilePagesInfo = new ResourcePagesInfo();

            IList<IResourceBlock> systemBlocks;
            IList<IResourceBlock> graphicBlocks;
            GetBlocks(fileBase, out systemBlocks, out graphicBlocks);

            int systemPageSize = BASE_SIZE;// *4;
            int systemPageCount;
            AssignPositions(systemBlocks, 0x50000000, ref systemPageSize, out systemPageCount);

            int graphicsPageSize = BASE_SIZE;
            int graphicsPageCount;
            AssignPositions(graphicBlocks, 0x60000000, ref graphicsPageSize, out graphicsPageCount);




            fileBase.FilePagesInfo.SystemPagesCount = 0;
            if (systemPageCount > 0)
                fileBase.FilePagesInfo.SystemPagesCount = 1; // (byte)systemPageCount; //1
            fileBase.FilePagesInfo.GraphicsPagesCount = (byte)graphicsPageCount;



            var systemStream = new MemoryStream();
            var graphicsStream = new MemoryStream();
            var resourceWriter = new ResourceDataWriter(systemStream, graphicsStream);

            resourceWriter.Position = 0x50000000;
            foreach (var block in systemBlocks)
            {
                resourceWriter.Position = block.FilePosition;

                var pos_before = resourceWriter.Position;
                block.Write(resourceWriter);
                var pos_after = resourceWriter.Position;

                if ((pos_after - pos_before) != block.BlockLength)
                {
                    throw new Exception("error in system length");
                }
            }

            resourceWriter.Position = 0x60000000;
            foreach (var block in graphicBlocks)
            {
                resourceWriter.Position = block.FilePosition;

                var pos_before = resourceWriter.Position;
                block.Write(resourceWriter);
                var pos_after = resourceWriter.Position;

                if ((pos_after - pos_before) != block.BlockLength)
                {
                    throw new Exception("error in graphics length");
                }
            }




            var sysDataSize = 0x2000;
            while (sysDataSize < systemStream.Length)
            {
                sysDataSize *= 2;
            }
            var sysData = new byte[sysDataSize];
            systemStream.Flush();
            systemStream.Position = 0;
            systemStream.Read(sysData, 0, (int)systemStream.Length);


            var gfxPageSize = 0x2000;
            while (gfxPageSize != graphicsPageSize)
            {
                gfxPageSize *= 2;
            }
            var gfxDataSize = graphicsPageCount * gfxPageSize;
            var gfxData = new byte[gfxDataSize];
            graphicsStream.Flush();
            graphicsStream.Position = 0;
            graphicsStream.Read(gfxData, 0, (int)graphicsStream.Length);



            uint uv = (uint)version;
            uint sv = (uv >> 4) & 0xF;
            uint gv = (uv >> 0) & 0xF;

            //uint sf = RpfResourceFileEntry.GetFlagsFromSize(sysDataSize, sv);
            //uint gf = RpfResourceFileEntry.GetFlagsFromSize(gfxDataSize, gv); //TODO: might be broken...

            uint sf = RpfResourceFileEntry.GetFlagsFromBlocks((uint)systemPageCount, (uint)systemPageSize, sv);
            uint gf = RpfResourceFileEntry.GetFlagsFromBlocks((uint)graphicsPageCount, (uint)graphicsPageSize, gv);

            var tdatasize = sysDataSize + gfxDataSize;
            var tdata = new byte[tdatasize];
            Buffer.BlockCopy(sysData, 0, tdata, 0, sysDataSize);
            Buffer.BlockCopy(gfxData, 0, tdata, sysDataSize, gfxDataSize);


            var cdata = Compress(tdata);


            var dataSize = 16 + cdata.Length;// sysDataSize + gfxDataSize;
            var data = new byte[dataSize];

            byte[] h1 = BitConverter.GetBytes((uint)0x37435352);
            byte[] h2 = BitConverter.GetBytes((int)version);
            byte[] h3 = BitConverter.GetBytes(sf);
            byte[] h4 = BitConverter.GetBytes(gf);
            Buffer.BlockCopy(h1, 0, data, 0, 4);
            Buffer.BlockCopy(h2, 0, data, 4, 4);
            Buffer.BlockCopy(h3, 0, data, 8, 4);
            Buffer.BlockCopy(h4, 0, data, 12, 4);
            Buffer.BlockCopy(cdata, 0, data, 16, cdata.Length);
            //Buffer.BlockCopy(sysData, 0, data, 16, sysDataSize);
            //Buffer.BlockCopy(gfxData, 0, data, 16 + sysDataSize, gfxDataSize);

            return data;
        }






        public static byte[] AddResourceHeader(RpfResourceFileEntry entry, byte[] data)
        {
            if (data == null) return null;
            byte[] newdata = new byte[data.Length + 16];
            byte[] h1 = BitConverter.GetBytes((uint)0x37435352);
            byte[] h2 = BitConverter.GetBytes(entry.Version);
            byte[] h3 = BitConverter.GetBytes(entry.SystemFlags);
            byte[] h4 = BitConverter.GetBytes(entry.GraphicsFlags);
            Buffer.BlockCopy(h1, 0, newdata, 0, 4);
            Buffer.BlockCopy(h2, 0, newdata, 4, 4);
            Buffer.BlockCopy(h3, 0, newdata, 8, 4);
            Buffer.BlockCopy(h4, 0, newdata, 12, 4);
            Buffer.BlockCopy(data, 0, newdata, 16, data.Length);
            return newdata;
        }


        public static byte[] Compress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress, true);
                ds.Write(data, 0, data.Length);
                ds.Close();
                byte[] deflated = ms.GetBuffer();
                byte[] outbuf = new byte[ms.Length]; //need to copy to the right size buffer...
                Array.Copy(deflated, outbuf, outbuf.Length);
                return outbuf;
            }
        }
        public static byte[] Decompress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress);
                MemoryStream outstr = new MemoryStream();
                ds.CopyTo(outstr);
                byte[] deflated = outstr.GetBuffer();
                byte[] outbuf = new byte[outstr.Length]; //need to copy to the right size buffer...
                Array.Copy(deflated, outbuf, outbuf.Length);
                return outbuf;
            }
        }

    }
}
