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
        private const int SKIP_SIZE = 16;//512;//256;//64;
        private const int ALIGN_SIZE = 16;//512;//64;


        public static void GetBlocks(IResourceBlock rootBlock, out IList<IResourceBlock> sys, out IList<IResourceBlock> gfx)
        {
            var systemBlocks = new HashSet<IResourceBlock>();
            var graphicBlocks = new HashSet<IResourceBlock>();
            var processed = new HashSet<IResourceBlock>();



            //var protectedBlocks = new List<IResourceBlock>();
            //var stack = new Stack<IResourceBlock>();
            //stack.Push(rootBlock);
            //processed.Add(rootBlock);
            //while (stack.Count > 0)
            //{
            //    var block = stack.Pop();
            //    if (block == null)
            //        continue;
            //    if (block is IResourceSystemBlock)
            //    {
            //        if (!systemBlocks.Contains(block))
            //            systemBlocks.Add(block);
            //        // for system blocks, also process references...
            //        var references = ((IResourceSystemBlock)block).GetReferences();
            //        //Array.Reverse(references);
            //        foreach (var reference in references)
            //            if (!processed.Contains(reference))
            //            {
            //                stack.Push(reference);
            //                processed.Add(reference);
            //            }
            //        var subs = new Stack<IResourceSystemBlock>();
            //        foreach (var part in ((IResourceSystemBlock)block).GetParts())
            //            subs.Push((IResourceSystemBlock)part.Item2);
            //        while (subs.Count > 0)
            //        {
            //            var sub = subs.Pop();
            //            foreach (var x in sub.GetReferences())
            //                if (!processed.Contains(x))
            //                {
            //                    stack.Push(x);
            //                    processed.Add(x);
            //                }
            //            foreach (var x in sub.GetParts())
            //                subs.Push((IResourceSystemBlock)x.Item2);
            //            protectedBlocks.Add(sub);
            //        }
            //    }
            //    else
            //    {
            //        if (!graphicBlocks.Contains(block))
            //            graphicBlocks.Add(block);
            //    }
            //}
            //// there are now sys-blocks in the list that actually
            //// only substructures and therefore must not get
            //// a new position!
            //// -> remove them from the list
            //foreach (var q in protectedBlocks)
            //    if (systemBlocks.Contains(q))
            //        systemBlocks.Remove(q);










            void addBlock(IResourceBlock block)
            {
                if (block is IResourceSystemBlock)
                {
                    if (!systemBlocks.Contains(block)) systemBlocks.Add(block);
                }
                else if(block is IResourceGraphicsBlock)
                {
                    if (!graphicBlocks.Contains(block)) graphicBlocks.Add(block);
                }
            }
            void addChildren(IResourceBlock block)
            {
                if (block is IResourceSystemBlock sblock)
                {
                    var references = sblock.GetReferences();
                    foreach (var reference in references)
                    {
                        if (!processed.Contains(reference))
                        {
                            processed.Add(reference);
                            addBlock(reference);
                            addChildren(reference);
                        }
                    }
                    var parts = sblock.GetParts();
                    foreach (var part in parts)
                    {
                        addChildren(part.Item2);
                    }
                }
            }

            addBlock(rootBlock);
            addChildren(rootBlock);





            sys = new List<IResourceBlock>();
            foreach (var s in systemBlocks)
            {
                sys.Add(s);
            }
            gfx = new List<IResourceBlock>();
            foreach (var s in graphicBlocks)
            {
                gfx.Add(s);
            }
        }

        public static void AssignPositions(IList<IResourceBlock> blocks, uint basePosition, ref int pageSize, out int pageCount)
        {

            IResourceBlock getFirstBlock()
            {
                if (blocks.Count > 0)
                {
                    return blocks[0];
                }
                return null;
            }
            HashSet<IResourceBlock> getBlockSet()
            {
                var blockset = new HashSet<IResourceBlock>();
                for (int i = 1; i < blocks.Count; i++)
                {
                    blockset.Add(blocks[i]);
                }
                return blockset;
            }
            IResourceBlock takeBestBlock(long maxSize, HashSet<IResourceBlock> blockset)
            {
                if (maxSize <= 0) return null;
                IResourceBlock r = null;
                long rlen = 0;
                foreach (var block in blockset)
                {
                    var blockLength = block.BlockLength;
                    if ((blockLength <= maxSize) && (blockLength > rlen))
                    {
                        r = block;
                        rlen = blockLength;
                    }
                }
                if (r != null)
                {
                    blockset.Remove(r);
                }
                return r;
            }


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

            long currentPageCount = 0;
            long currentPosition = 0;
            while (true)
            {
                if (blocks.Count == 0) break;

                // reset all positions
                foreach (var block in blocks)
                    block.FilePosition = -1;

                //currentPageCount = 0;
                //currentPosition = 0;
                //foreach (var block in blocks)
                //{
                //    //if (block.FilePosition != -1)
                //    //    throw new Exception("Block was already assigned a position!");
                //    //if (block.Length == 0)
                //    //    throw new Exception("A length of 0 is not allowed!");
                //
                //    // check if new page is necessary...
                //    // if yes, add a new page and align to it
                //    long maxSpace = currentPageCount * currentPageSize - currentPosition;
                //    if (maxSpace < (block.BlockLength + SKIP_SIZE))
                //    {
                //        currentPageCount++;
                //        currentPosition = currentPageSize * (currentPageCount - 1);
                //    }
                //
                //    // set position
                //    block.FilePosition = basePosition + currentPosition;
                //    currentPosition += block.BlockLength + SKIP_SIZE;
                //
                //    // align...
                //    if ((currentPosition % ALIGN_SIZE) != 0)
                //        currentPosition += (ALIGN_SIZE - (currentPosition % ALIGN_SIZE));
                //}



                currentPageCount = 1;
                currentPosition = 0;
                var blockset = getBlockSet();
                while (true)
                {
                    var maxSize = currentPageCount * currentPageSize - currentPosition;
                    var isroot = (currentPosition == 0);
                    var block = isroot ? getFirstBlock() : takeBestBlock(maxSize, blockset);
                    if (block != null)
                    {
                        block.FilePosition = basePosition + currentPosition;

                        currentPosition += block.BlockLength;

                        if ((currentPosition % ALIGN_SIZE) != 0)
                        {
                            currentPosition += (ALIGN_SIZE - (currentPosition % ALIGN_SIZE));
                        }
                    }
                    else if (blockset.Count > 0)
                    {
                        currentPosition = currentPageSize * currentPageCount;
                        currentPageCount++;
                    }
                    else
                    {
                        break;
                    }
                }



                // break if everything fits...
                if (currentPageCount < 128)
                    break;

                currentPageSize *= 2;
            }

            pageSize = (int)currentPageSize;
            pageCount = (int)currentPageCount;
        }

        public static void AssignPositions(IList<IResourceBlock> blocks, uint basePosition, out RpfResourcePageFlags pageFlags)
        {
            var sys = (basePosition == 0x50000000);

            IResourceBlock getRootBlock()
            {
                if (sys && (blocks.Count > 0))
                {
                    return blocks[0];
                }
                return null;
            }
            HashSet<IResourceBlock> getBlockSet()
            {
                var blockset = new HashSet<IResourceBlock>();
                int start = sys ? 1 : 0;
                for (int i = start; i < blocks.Count; i++)
                {
                    blockset.Add(blocks[i]);
                }
                return blockset;
            }
            IResourceBlock findBestBlock(long maxSize, HashSet<IResourceBlock> blockset)
            {
                if (maxSize <= 0) return null;
                IResourceBlock r = null;
                long rlen = 0;
                foreach (var block in blockset)
                {
                    var blockLength = block.BlockLength;
                    if ((blockLength <= maxSize) && (blockLength > rlen))
                    {
                        r = block;
                        rlen = blockLength;
                    }
                }
                return r;
            }
            IResourceBlock takeBestBlock(long maxSize, HashSet<IResourceBlock> blockset)
            {
                var r = findBestBlock(maxSize, blockset);
                if (r != null)
                {
                    blockset.Remove(r);
                }
                return r;
            }
            long pad(long p)
            {
                return ((ALIGN_SIZE - (p % ALIGN_SIZE)) % ALIGN_SIZE);
            }


            long largestBlockSize = 0; // find largest structure
            long startPageSize = BASE_SIZE;// 0x2000; // find starting page size
            long totalBlockSize = 0;
            foreach (var block in blocks)
            {
                var blockLength = block.BlockLength;
                totalBlockSize += blockLength;
                totalBlockSize += pad(totalBlockSize);
                if (largestBlockSize < blockLength)
                {
                    largestBlockSize = blockLength;
                }
            }
            while (startPageSize < largestBlockSize)
            {
                startPageSize *= 2;
            }


            pageFlags = new RpfResourcePageFlags();

            while (true)
            {
                if (blocks.Count == 0) break;

                var currentPosition = 0L;
                var currentPageSize = startPageSize;
                var currentPageStart = 0L;
                var currentPageSpace = startPageSize;
                var currentRemainder = totalBlockSize;
                var rootblock = getRootBlock();
                var blockset = getBlockSet();

                var pageCount = 1;
                var pageCounts = new uint[9];
                var pageCountIndex = 0;
                var targetPageSize = Math.Max(65536, startPageSize >> 5);
                var minPageSize = Math.Max(512, Math.Min(targetPageSize, startPageSize) >> 4);
                var baseShift = 0u;
                var baseSize = 512;
                while (baseSize < minPageSize)
                {
                    baseShift++;
                    baseSize *= 2;
                    if (baseShift >= 0xF) break;
                }
                var baseSizeMax = baseSize << 8;
                var baseSizeMaxTest = startPageSize;
                while (baseSizeMaxTest < baseSizeMax)
                {
                    pageCountIndex++;
                    baseSizeMaxTest *= 2;
                }
                pageCounts[pageCountIndex] = 1;

                while (true)
                {
                    var isroot = sys && (currentPosition == 0);
                    var block = isroot ? rootblock : takeBestBlock(currentPageSpace, blockset);
                    var blockLength = block?.BlockLength ?? 0;
                    if (block != null)
                    {
                        //add this block to the current page.
                        block.FilePosition = basePosition + currentPosition;
                        var opos = currentPosition;
                        currentPosition += blockLength;
                        currentPosition += pad(currentPosition);
                        var usedspace = currentPosition - opos;
                        currentPageSpace -= usedspace;
                        currentRemainder -= usedspace;//blockLength;// 

                    }
                    else if (blockset.Count > 0)
                    {
                        //allocate a new page
                        currentPageStart += currentPageSize;
                        currentPosition = currentPageStart;
                        block = findBestBlock(long.MaxValue, blockset);//just find the biggest block
                        blockLength = block?.BlockLength ?? 0;
                        while (blockLength <= (currentPageSize >> 1))//determine best new page size
                        {
                            if (currentPageSize <= minPageSize) break;
                            if (pageCountIndex >= 8) break;
                            if ((currentPageSize <= targetPageSize) && (currentRemainder >= (currentPageSize - minPageSize))) break;

                            currentPageSize = currentPageSize >> 1;
                            pageCountIndex++;
                        }
                        currentPageSpace = currentPageSize;
                        pageCounts[pageCountIndex]++;
                        pageCount++;
                    }
                    else
                    {
                        break;
                    }
                }


                pageFlags = new RpfResourcePageFlags(pageCounts, baseShift);

                if ((pageCount == pageFlags.Count) && (pageFlags.Size >= currentPosition)) //make sure page counts fit in the flags value
                {
                    break;
                }

                startPageSize *= 2;
            }

        }


        public static byte[] Build(ResourceFileBase fileBase, int version, bool compress = true)
        {

            fileBase.FilePagesInfo = new ResourcePagesInfo();

            IList<IResourceBlock> systemBlocks;
            IList<IResourceBlock> graphicBlocks;
            GetBlocks(fileBase, out systemBlocks, out graphicBlocks);

            //int systemPageSize = BASE_SIZE;// *4;
            //int systemPageCount;
            //AssignPositions(systemBlocks, 0x50000000, ref systemPageSize, out systemPageCount);

            //int graphicsPageSize = BASE_SIZE;
            //int graphicsPageCount;
            //AssignPositions(graphicBlocks, 0x60000000, ref graphicsPageSize, out graphicsPageCount);

            
            RpfResourcePageFlags systemPageFlags;
            AssignPositions(systemBlocks, 0x50000000, out systemPageFlags);
            
            RpfResourcePageFlags graphicsPageFlags;
            AssignPositions(graphicBlocks, 0x60000000, out graphicsPageFlags);




            //fileBase.FilePagesInfo.SystemPagesCount = 0;
            //if (systemPageCount > 0)
            //    fileBase.FilePagesInfo.SystemPagesCount = 1; // (byte)systemPageCount; //1
            fileBase.FilePagesInfo.SystemPagesCount = (byte)systemPageFlags.Count;// systemPageCount;
            fileBase.FilePagesInfo.GraphicsPagesCount = (byte)graphicsPageFlags.Count;// graphicsPageCount;



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




            var sysDataSize = (int)systemPageFlags.Size;// systemPageCount * systemPageSize;
            var sysData = new byte[sysDataSize];
            systemStream.Flush();
            systemStream.Position = 0;
            systemStream.Read(sysData, 0, (int)systemStream.Length);


            var gfxDataSize = (int)graphicsPageFlags.Size;// graphicsPageCount * graphicsPageSize;
            var gfxData = new byte[gfxDataSize];
            graphicsStream.Flush();
            graphicsStream.Position = 0;
            graphicsStream.Read(gfxData, 0, (int)graphicsStream.Length);



            uint uv = (uint)version;
            uint sv = (uv >> 4) & 0xF;
            uint gv = (uv >> 0) & 0xF;

            //uint sf = RpfResourceFileEntry.GetFlagsFromSize(sysDataSize, sv);
            //uint gf = RpfResourceFileEntry.GetFlagsFromSize(gfxDataSize, gv); //TODO: might be broken...
            //uint sf = RpfResourceFileEntry.GetFlagsFromBlocks((uint)systemPageCount, (uint)systemPageSize, sv);
            //uint gf = RpfResourceFileEntry.GetFlagsFromBlocks((uint)graphicsPageCount, (uint)graphicsPageSize, gv);
            uint sf = systemPageFlags.Value + (sv << 28);
            uint gf = graphicsPageFlags.Value + (gv << 28);


            var tdatasize = sysDataSize + gfxDataSize;
            var tdata = new byte[tdatasize];
            Buffer.BlockCopy(sysData, 0, tdata, 0, sysDataSize);
            Buffer.BlockCopy(gfxData, 0, tdata, sysDataSize, gfxDataSize);


            var cdata = compress ? Compress(tdata) : tdata;


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
