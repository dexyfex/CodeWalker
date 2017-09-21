using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{

    public class RpfFile
    {
        public string Name { get; set; } //name of this RPF file/package
        public string NameLower { get; set; }
        public string Path { get; set; } //path within the RPF structure
        public string FilePath { get; set; } //full file path of the RPF
        public long FileSize { get; set; }
        public string LastError { get; set; }
        public Exception LastException { get; set; }

        public RpfDirectoryEntry Root { get; set; }

        public bool IsCompressed { get; set; }
        public bool IsAESEncrypted { get; set; }
        public bool IsNGEncrypted { get; set; }
        public long UncompressedSize { get; set; }

        public string RootFileName { get; set; }
        public long RootFileSize { get; set; }


        //offset in the current file
        public long StartPos { get; set; }

        //header data
        public uint Version { get; set; }
        public uint EntryCount { get; set; }
        public uint NamesLength { get; set; }
        public RpfEncryption Encryption { get; set; }

        //object linkage
        public List<RpfEntry> AllEntries { get; set; }
        public List<RpfFile> Children { get; set; }
        public RpfFile Parent { get; set; }

        public BinaryReader CurrentFileReader { get; set; } //for temporary use while reading header



        public uint TotalFileCount { get; set; }
        public uint TotalFolderCount { get; set; }
        public uint TotalResourceCount { get; set; }
        public uint TotalBinaryFileCount { get; set; }
        public uint GrandTotalRpfCount { get; set; }
        public uint GrandTotalFileCount { get; set; }
        public uint GrandTotalFolderCount { get; set; }
        public uint GrandTotalResourceCount { get; set; }
        public uint GrandTotalBinaryFileCount { get; set; }
        public long ExtractedByteCount { get; set; }


        public RpfFile(string fpath, string relpath)
        {
            FileInfo fi = new FileInfo(fpath);
            Name = fi.Name;
            NameLower = Name.ToLower();
            Path = relpath.ToLower();
            FilePath = fpath;
            FileSize = fi.Length;
            IsCompressed = false;
            IsAESEncrypted = false;
            RootFileName = Name;
            RootFileSize = FileSize;
        }
        public RpfFile(string name, string path, string filepath, long filesize, bool compressed, bool encrypted, string rootfn, long rootfs)
        {
            Name = name;
            NameLower = Name.ToLower();
            Path = path.ToLower();
            FilePath = filepath;
            FileSize = filesize;
            IsCompressed = compressed;
            IsAESEncrypted = encrypted;
            RootFileName = rootfn;
            RootFileSize = rootfs;
        }


        public string GetPhysicalFilePath()
        {
            RpfFile pfile = this;
            while (pfile.Parent != null)
            {
                pfile = pfile.Parent;
            }
            return pfile.FilePath;
        }




        private void ReadHeader(BinaryReader br)
        {
            CurrentFileReader = br;

            StartPos = br.BaseStream.Position;

            Version = br.ReadUInt32(); //RPF Version - GTAV should be 0x52504637 (1380992567)
            EntryCount = br.ReadUInt32(); //Number of Entries
            NamesLength = br.ReadUInt32();
            Encryption = (RpfEncryption)br.ReadUInt32(); //0x04E45504F (1313165391): none;  0x0ffffff9 (268435449): AES

            if (Version == 0xF00)
            {
                throw new Exception("Invalid Resource.");
            }
            if (Version != 0x52504637)
            {
                throw new Exception("Invalid Resource - not GTAV!");
            }


            uint entriestotalbytes = EntryCount * 16; //4x uints each
            uint entriesptr = (uint)br.BaseStream.Position;
            uint namesptr = entriesptr + entriestotalbytes;

            byte[] entriesdata = new byte[entriestotalbytes];
            int entread = br.Read(entriesdata, 0, (int)entriestotalbytes);

            byte[] namesdata = new byte[NamesLength];
            int namread = br.Read(namesdata, 0, (int)NamesLength);

            switch (Encryption)
            {
                case RpfEncryption.OPEN: //nothing to do.. OpenIV modified RPF with unencrypted TOC
                    break;
                case RpfEncryption.AES:
                    entriesdata = GTACrypto.DecryptAES(entriesdata);
                    namesdata = GTACrypto.DecryptAES(namesdata);
                    IsAESEncrypted = true;
                    break;
                case RpfEncryption.NG:
                    entriesdata = GTACrypto.DecryptNG(entriesdata, Name, (uint)FileSize);
                    namesdata = GTACrypto.DecryptNG(namesdata, Name, (uint)FileSize);
                    IsNGEncrypted = true;
                    break;
                default: //unknown encryption type? assume NG.. never seems to get here
                    entriesdata = GTACrypto.DecryptNG(entriesdata, Name, (uint)FileSize);
                    namesdata = GTACrypto.DecryptNG(namesdata, Name, (uint)FileSize);
                    break;
            }



            var entriesrdr = new DataReader(new MemoryStream(entriesdata));
            var namesrdr = new DataReader(new MemoryStream(namesdata));
            AllEntries = new List<RpfEntry>();
            TotalFileCount = 0;
            TotalFolderCount = 0;
            TotalResourceCount = 0;
            TotalBinaryFileCount = 0;

            for (uint i = 0; i < EntryCount; i++)
            {
                //entriesrdr.Position += 4;
                uint y = entriesrdr.ReadUInt32();
                uint x = entriesrdr.ReadUInt32();
                entriesrdr.Position -= 8;

                RpfEntry e;

                if (x == 0x7fffff00) //directory entry
                {
                    e = new RpfDirectoryEntry();
                    TotalFolderCount++;
                }
                else if ((x & 0x80000000) == 0) //binary file entry
                {
                    e = new RpfBinaryFileEntry();
                    TotalBinaryFileCount++;
                    TotalFileCount++;
                }
                else //assume resource file entry
                {
                    e = new RpfResourceFileEntry();
                    TotalResourceCount++;
                    TotalFileCount++;
                }

                e.File = this;
                e.H1 = y;
                e.H2 = x;

                e.Read(entriesrdr);

                namesrdr.Position = e.NameOffset;
                e.Name = namesrdr.ReadString();
                e.NameLower = e.Name.ToLower();

                if ((e is RpfFileEntry) && string.IsNullOrEmpty(e.Name))
                {
                }
                if ((e is RpfResourceFileEntry))// && string.IsNullOrEmpty(e.Name))
                {
                    var rfe = e as RpfResourceFileEntry;
                    rfe.IsEncrypted = rfe.NameLower.EndsWith(".ysc");//any other way to know..?
                }

                AllEntries.Add(e);
            }



            Root = (RpfDirectoryEntry)AllEntries[0];
            Root.Path = Path.ToLower();// + "\\" + Root.Name;
            var stack = new Stack<RpfDirectoryEntry>();
            stack.Push(Root);
            while (stack.Count > 0)
            {
                var item = stack.Pop();

                for (int i = (int)item.EntriesIndex; i < (item.EntriesIndex + item.EntriesCount); i++)
                {
                    RpfEntry e = AllEntries[i];
                    if (e is RpfDirectoryEntry)
                    {
                        RpfDirectoryEntry rde = e as RpfDirectoryEntry;
                        rde.Path = item.Path + "\\" + rde.NameLower;
                        item.Directories.Add(rde);
                        stack.Push(rde);
                    }
                    else if (e is RpfFileEntry)
                    {
                        RpfFileEntry rfe = e as RpfFileEntry;
                        rfe.Path = item.Path + "\\" + rfe.NameLower;
                        item.Files.Add(rfe);
                    }
                }
            }

            br.BaseStream.Position = StartPos;

            CurrentFileReader = null;

        }




        public void ScanStructure(Action<string> updateStatus, Action<string> errorLog)
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(FilePath)))
            {
                try
                {
                    ScanStructure(br, updateStatus, errorLog);
                }
                catch (Exception ex)
                {
                    LastError = ex.ToString();
                    LastException = ex;
                    errorLog(FilePath + ": " + LastError);
                }
            }
        }
        private void ScanStructure(BinaryReader br, Action<string> updateStatus, Action<string> errorLog)
        {
            ReadHeader(br);

            Children = new List<RpfFile>();

            GrandTotalRpfCount = 1; //count this file..
            GrandTotalFileCount = 1; //start with this one.
            GrandTotalFolderCount = 0;
            GrandTotalResourceCount = 0;
            GrandTotalBinaryFileCount = 0;


            updateStatus?.Invoke("Scanning " + Path + "...");

            foreach (RpfEntry entry in AllEntries)
            {
                try
                {
                    if (entry is RpfBinaryFileEntry)
                    {
                        RpfBinaryFileEntry binentry = entry as RpfBinaryFileEntry;
                        long l = binentry.FileSize;
                        if (l == 0) l = binentry.FileUncompressedSize;

                        //search all the sub resources for YSC files. (recurse!)
                        string lname = binentry.NameLower;
                        if (lname.EndsWith(".rpf"))
                        {
                            br.BaseStream.Position = StartPos + (binentry.FileOffset * 512);

                            RpfFile subfile = new RpfFile(binentry.Name, binentry.Path, binentry.Path, l, binentry.FileSize != 0, binentry.IsEncrypted, RootFileName, RootFileSize);
                            subfile.UncompressedSize = binentry.FileUncompressedSize;
                            subfile.Parent = this;

                            subfile.ScanStructure(br, updateStatus, errorLog);

                            GrandTotalRpfCount += subfile.GrandTotalRpfCount;
                            GrandTotalFileCount += subfile.GrandTotalFileCount;
                            GrandTotalFolderCount += subfile.GrandTotalFolderCount;
                            GrandTotalResourceCount += subfile.GrandTotalResourceCount;
                            GrandTotalBinaryFileCount += subfile.GrandTotalBinaryFileCount;


                            Children.Add(subfile);
                        }
                        else
                        {
                            //binary file that's not an rpf...
                            GrandTotalBinaryFileCount++;
                            GrandTotalFileCount++;
                        }
                    }
                    else if (entry is RpfResourceFileEntry)
                    {
                        GrandTotalResourceCount++;
                        GrandTotalFileCount++;
                    }
                    else if (entry is RpfDirectoryEntry)
                    {
                        GrandTotalFolderCount++;
                    }
                }
                catch (Exception ex)
                {
                    errorLog(entry.Path + ": " + ex.ToString());
                }
            }

        }


        public void ExtractScripts(string outputfolder, Action<string> updateStatus)
        {
            FileStream fs = File.OpenRead(FilePath);
            BinaryReader br = new BinaryReader(fs);

            try
            {
                ExtractScripts(br, outputfolder, updateStatus);
            }
            catch (Exception ex)
            {
                LastError = ex.ToString();
                LastException = ex;
            }

            br.Close();
            br.Dispose();
            fs.Dispose();
        }
        private void ExtractScripts(BinaryReader br, string outputfolder, Action<string> updateStatus)
        {
            updateStatus?.Invoke("Searching " + Name + "...");

            ReadHeader(br);

            //List<DataBlock> blocks = new List<DataBlock>();
            foreach (RpfEntry entry in AllEntries)
            {
                if (entry is RpfBinaryFileEntry)
                {
                    RpfBinaryFileEntry binentry = entry as RpfBinaryFileEntry;
                    long l = binentry.FileSize;
                    if (l == 0) l = binentry.FileUncompressedSize;

                    //search all the sub resources for YSC files. (recurse!)
                    string lname = binentry.NameLower;
                    if (lname.EndsWith(".rpf"))
                    {
                        br.BaseStream.Position = StartPos + (binentry.FileOffset * 512);

                        RpfFile subfile = new RpfFile(binentry.Name, binentry.Path, binentry.Path, l, binentry.FileSize != 0, binentry.IsEncrypted, RootFileName, RootFileSize);
                        subfile.UncompressedSize = binentry.FileUncompressedSize;
                        subfile.Parent = this;

                        subfile.ExtractScripts(br, outputfolder, updateStatus);
                    }

                }
                else if (entry is RpfResourceFileEntry)
                {

                    RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;

                    string lname = resentry.NameLower;

                    if (lname.EndsWith(".ysc"))
                    {
                        updateStatus?.Invoke("Extracting " + resentry.Name + "...");

                        //found a YSC file. extract it!
                        string ofpath = outputfolder + "\\" + resentry.Name;

                        br.BaseStream.Position = StartPos + (resentry.FileOffset * 512);

                        if (resentry.FileSize > 0)
                        {
                            uint offset = 0x10;
                            uint totlen = resentry.FileSize - offset;

                            byte[] tbytes = new byte[totlen];

                            br.BaseStream.Position += offset;

                            br.Read(tbytes, 0, (int)totlen);

                            byte[] decr;
                            if (IsAESEncrypted)
                            {
                                decr = GTACrypto.DecryptAES(tbytes);

                                //special case! probable duplicate pilot_school.ysc
                                ofpath = outputfolder + "\\" + Name + "___" + resentry.Name;
                            }
                            else
                            {
                                decr = GTACrypto.DecryptNG(tbytes, resentry.Name, resentry.FileSize);
                            }


                            try
                            {
                                MemoryStream ms = new MemoryStream(decr);
                                DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress);

                                MemoryStream outstr = new MemoryStream();
                                ds.CopyTo(outstr);
                                byte[] deflated = outstr.GetBuffer();
                                byte[] outbuf = new byte[outstr.Length]; //need to copy to the right size buffer for File.WriteAllBytes().
                                Array.Copy(deflated, outbuf, outbuf.Length);

                                bool pathok = true;
                                if (File.Exists(ofpath))
                                {
                                    ofpath = outputfolder + "\\" + Name + "_" + resentry.Name;
                                    if (File.Exists(ofpath))
                                    {
                                        LastError = "Output file " + ofpath + " already exists!";
                                        pathok = false;
                                    }
                                }
                                if (pathok)
                                {
                                    File.WriteAllBytes(ofpath, outbuf);
                                }
                            }
                            catch (Exception ex)
                            {
                                LastError = ex.ToString();
                                LastException = ex;
                            }


                        }
                    }

                }
            }





        }





        public byte[] ExtractFile(RpfFileEntry entry)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(GetPhysicalFilePath())))
                {
                    if (entry is RpfBinaryFileEntry)
                    {
                        return ExtractFileBinary(entry as RpfBinaryFileEntry, br);
                    }
                    else if (entry is RpfResourceFileEntry)
                    {
                        return ExtractFileResource(entry as RpfResourceFileEntry, br);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.ToString();
                LastException = ex;
                return null;
            }
        }
        public byte[] ExtractFileBinary(RpfBinaryFileEntry entry, BinaryReader br)
        {
            br.BaseStream.Position = StartPos + ((long)entry.FileOffset * 512);

            long l = entry.FileSize;
            if (l == 0) l = entry.FileUncompressedSize;

            if (l > 0)
            {
                uint offset = 0;// 0x10;
                uint totlen = (uint)l - offset;

                byte[] tbytes = new byte[totlen];

                br.BaseStream.Position += offset;
                br.Read(tbytes, 0, (int)totlen);

                byte[] decr = tbytes;

                if (entry.IsEncrypted)
                {
                    if (IsAESEncrypted)
                    {
                        decr = GTACrypto.DecryptAES(tbytes);
                    }
                    else //if (IsNGEncrypted) //assume the archive is set to NG encryption if not AES... (comment: fix for openIV modded files)
                    {
                        decr = GTACrypto.DecryptNG(tbytes, entry.Name, entry.FileUncompressedSize);
                    }
                    //else
                    //{ }
                }

                byte[] defl = decr;

                if (entry.FileSize > 0) //apparently this means it's compressed
                {
                    defl = DecompressBytes(decr);
                }
                else
                {
                }

                return defl;
            }

            return null;
        }
        public byte[] ExtractFileResource(RpfResourceFileEntry entry, BinaryReader br)
        {
            br.BaseStream.Position = StartPos + ((long)entry.FileOffset * 512);


            if (entry.FileSize > 0)
            {
                uint offset = 0x10;
                uint totlen = entry.FileSize - offset;

                byte[] tbytes = new byte[totlen];


                br.BaseStream.Position += offset;
                //byte[] hbytes = new byte[16]; //what are these 16 bytes actually used for?
                //br.Read(hbytes, 0, 16);
                //MetaHash h1 = br.ReadUInt32();
                //MetaHash h2 = br.ReadUInt32();
                //MetaHash h3 = br.ReadUInt32();
                //MetaHash h4 = br.ReadUInt32();
                //long l1 = br.ReadInt64();
                //long l2 = br.ReadInt64();


                br.Read(tbytes, 0, (int)totlen);

                byte[] decr = tbytes;
                if (entry.IsEncrypted)
                {
                    if (IsAESEncrypted)
                    {
                        decr = GTACrypto.DecryptAES(tbytes);
                    }
                    else //if (IsNGEncrypted) //assume the archive is set to NG encryption if not AES... (comment: fix for openIV modded files)
                    {
                        decr = GTACrypto.DecryptNG(tbytes, entry.Name, entry.FileSize);
                    }
                    //else
                    //{ }
                }

                byte[] deflated = DecompressBytes(decr);

                byte[] data = null;

                if (deflated != null)
                {
                    data = deflated;
                }
                else
                {
                    entry.FileSize -= offset;
                    data = decr;
                }


                return data;
            }

            return null;
        }

        public static T GetFile<T>(RpfEntry e) where T : class, PackedFile, new()
        {
            T file = null;
            byte[] data = null;
            RpfFileEntry entry = e as RpfFileEntry;
            if (entry != null)
            {
                data = entry.File.ExtractFile(entry);
            }
            if (data != null)
            {
                file = new T();
                file.Load(data, entry);
            }
            return file;
        }
        public static T GetFile<T>(RpfEntry e, byte[] data) where T : class, PackedFile, new()
        {
            T file = null;
            RpfFileEntry entry = e as RpfFileEntry;
            if ((data != null))
            {
                if (entry == null)
                {
                    entry = CreateResourceFileEntry(data, 0);
                }
                file = new T();
                file.Load(data, entry);
            }
            return file;
        }
        public static T GetResourceFile<T>(byte[] data) where T : class, PackedFile, new()
        {
            T file = null;
            RpfFileEntry entry = CreateResourceFileEntry(data, 0);
            if ((data != null) && (entry != null))
            {
                file = new T();
                file.Load(data, entry);
            }
            return file;
        }



        public static RpfResourceFileEntry CreateResourceFileEntry(byte[] data, uint ver)
        {
            var resentry = new RpfResourceFileEntry();

            //hopefully this format has an RSC7 header...
            uint rsc7 = BitConverter.ToUInt32(data, 0);
            if (rsc7 == 0x37435352) //RSC7 header present!
            {
                int version = BitConverter.ToInt32(data, 4);//use this instead of what was given...
                resentry.SystemFlags = BitConverter.ToUInt32(data, 8);
                resentry.GraphicsFlags = BitConverter.ToUInt32(data, 12);
                //if (data.Length > 16)
                //{
                //    int newlen = data.Length - 16; //trim the header from the data passed to the next step.
                //    byte[] newdata = new byte[newlen];
                //    Buffer.BlockCopy(data, 16, newdata, 0, newlen);
                //    data = newdata;
                //}
                //else
                //{
                //    data = null; //shouldn't happen... empty..
                //}
            }
            else
            {
                //direct load from file without the rpf header..
                //assume it's in resource meta format
                resentry.SystemFlags = RpfResourceFileEntry.GetFlagsFromSize(data.Length, 0);
                resentry.GraphicsFlags = RpfResourceFileEntry.GetFlagsFromSize(0, ver); //graphics type 2 for ymap
            }

            resentry.Name = "";
            resentry.NameLower = "";

            return resentry;
        }



        public string TestExtractAllFiles()
        {
            StringBuilder sb = new StringBuilder();
            ExtractedByteCount = 0;
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(GetPhysicalFilePath())))
                {
                    foreach (RpfEntry entry in AllEntries)
                    {
                        try
                        {
                            LastError = string.Empty;
                            LastException = null;
                            if (!entry.NameLower.EndsWith(".rpf")) //don't try to extract rpf's, they will be done separately..
                            {
                                if (entry is RpfBinaryFileEntry)
                                {
                                    RpfBinaryFileEntry binentry = entry as RpfBinaryFileEntry;
                                    byte[] data = ExtractFileBinary(binentry, br);
                                    if (data == null)
                                    {
                                        if (binentry.FileSize == 0)
                                        {
                                            sb.AppendFormat("{0} : Binary FileSize is 0.", entry.Path);
                                            sb.AppendLine();
                                        }
                                        else
                                        {
                                            sb.AppendFormat("{0} : {1}", entry.Path, LastError);
                                            sb.AppendLine();
                                        }
                                    }
                                    else if (data.Length == 0)
                                    {
                                        sb.AppendFormat("{0} : Decompressed output was empty.", entry.Path);
                                        sb.AppendLine();
                                    }
                                    else
                                    {
                                        ExtractedByteCount += data.Length;
                                    }
                                }
                                else if (entry is RpfResourceFileEntry)
                                {
                                    RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
                                    byte[] data = ExtractFileResource(resentry, br);
                                    if (data == null)
                                    {
                                        if (resentry.FileSize == 0)
                                        {
                                            sb.AppendFormat("{0} : Resource FileSize is 0.", entry.Path);
                                            sb.AppendLine();
                                        }
                                        else
                                        {
                                            sb.AppendFormat("{0} : {1}", entry.Path, LastError);
                                            sb.AppendLine();
                                        }
                                    }
                                    else if (data.Length == 0)
                                    {
                                        sb.AppendFormat("{0} : Decompressed output was empty.", entry.Path);
                                        sb.AppendLine();
                                    }
                                    else
                                    {
                                        ExtractedByteCount += data.Length;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LastError = ex.ToString();
                            LastException = ex;
                            sb.AppendFormat("{0} : {1}", entry.Path, ex.Message);
                            sb.AppendLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.ToString();
                LastException = ex;
                sb.AppendFormat("{0} : {1}", Path, ex.Message);
                sb.AppendLine();
                return null;
            }
            return sb.ToString();
        }




        public List<RpfFileEntry> GetFiles(string folder, bool recurse)
        {
            List<RpfFileEntry> result = new List<RpfFileEntry>();
            string[] parts = folder.ToLower().Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            RpfDirectoryEntry dir = Root;
            for (int i = 0; i < parts.Length; i++)
            {
                if (dir == null) break;
                dir = FindSubDirectory(dir, parts[i]);
            }
            if (dir != null)
            {
                GetFiles(dir, result, recurse);
            }
            return result;
        }
        public void GetFiles(RpfDirectoryEntry dir, List<RpfFileEntry> result, bool recurse)
        {
            if (dir.Files != null)
            {
                result.AddRange(dir.Files);
            }
            if (recurse)
            {
                if (dir.Directories != null)
                {
                    for (int i = 0; i < dir.Directories.Count; i++)
                    {
                        GetFiles(dir.Directories[i], result, recurse);
                    }
                }
            }
        }

        private RpfDirectoryEntry FindSubDirectory(RpfDirectoryEntry dir, string name)
        {
            if (dir == null) return null;
            if (dir.Directories == null) return null;
            for (int i = 0; i < dir.Directories.Count; i++)
            {
                var cdir = dir.Directories[i];
                if (cdir.Name.ToLower() == name)
                {
                    return cdir;
                }
            }
            return null;
        }




        public byte[] DecompressBytes(byte[] bytes)
        {
            try
            {
                using (DeflateStream ds = new DeflateStream(new MemoryStream(bytes), CompressionMode.Decompress))
                {
                    MemoryStream outstr = new MemoryStream();
                    ds.CopyTo(outstr);
                    byte[] deflated = outstr.GetBuffer();
                    byte[] outbuf = new byte[outstr.Length]; //need to copy to the right size buffer for output.
                    Array.Copy(deflated, outbuf, outbuf.Length);

                    if (outbuf.Length <= bytes.Length)
                    {
                        LastError = "Decompressed data was smaller than compressed data...";
                        return null;
                    }

                    return outbuf;
                }
            }
            catch (Exception ex)
            {
                LastError = "Could not decompress.";// ex.ToString();
                LastException = ex;
                return null;
            }
        }


        public override string ToString()
        {
            return Path;
        }
    }


    public enum RpfEncryption : uint
    {
        OPEN = 0x4E45504F, //1313165391 "OPEN", ie. "no encryption?"
        AES =  0x0FFFFFF9, //268435449
        NG =   0x0FEFFFFF, //267386879
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public abstract class RpfEntry
    {
        public RpfFile File { get; set; }

        public uint NameHash { get; set; }
        public uint ShortNameHash { get; set; }

        public uint NameOffset { get; set; }
        public string Name { get; set; }
        public string NameLower { get; set; }
        public string Path { get; set; }

        public uint H1; //first 2 header values from RPF table...
        public uint H2;

        public abstract void Read(DataReader reader);
        public abstract void Write(DataWriter writer);

        public override string ToString()
        {
            return Path;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class RpfDirectoryEntry : RpfEntry
    {
        public uint EntriesIndex { get; set; }
        public uint EntriesCount { get; set; }

        public List<RpfDirectoryEntry> Directories = new List<RpfDirectoryEntry>();
        public List<RpfFileEntry> Files = new List<RpfFileEntry>();

        public override void Read(DataReader reader)
        {
            NameOffset = reader.ReadUInt32();
            uint ident = reader.ReadUInt32();
            if (ident != 0x7FFFFF00u)
            {
                throw new Exception("Error in RPF7 directory entry.");
            }
            EntriesIndex = reader.ReadUInt32();
            EntriesCount = reader.ReadUInt32();
        }
        public override void Write(DataWriter writer)
        {
            writer.Write(NameOffset);
            writer.Write(0x7FFFFF00u);
            writer.Write(EntriesIndex);
            writer.Write(EntriesCount);
        }
        public override string ToString()
        {
            return "Directory: " + Path;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public abstract class RpfFileEntry : RpfEntry
    {
        public uint FileOffset { get; set; }
        public uint FileSize { get; set; }
        public bool IsEncrypted { get; set; }

        public virtual long GetFileSize()
        {
            return FileSize;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class RpfBinaryFileEntry : RpfFileEntry
    {
        public uint FileUncompressedSize { get; set; }

        public override void Read(DataReader reader)
        {
            NameOffset = reader.ReadUInt16();

            var buf1 = reader.ReadBytes(3);
            FileSize = (uint)buf1[0] + (uint)(buf1[1] << 8) + (uint)(buf1[2] << 16);

            var buf2 = reader.ReadBytes(3);
            FileOffset = (uint)buf2[0] + (uint)(buf2[1] << 8) + (uint)(buf2[2] << 16);

            FileUncompressedSize = reader.ReadUInt32();

            switch (reader.ReadUInt32())
            {
                case 0: IsEncrypted = false; break;
                case 1: IsEncrypted = true; break;
                default:
                    throw new Exception("Error in RPF7 file entry.");
            }
        }
        public override void Write(DataWriter writer)
        {
            writer.Write((ushort)NameOffset);

            var buf1 = new byte[] {
                (byte)((FileSize >> 0) & 0xFF),
                (byte)((FileSize >> 8) & 0xFF),
                (byte)((FileSize >> 16) & 0xFF)
            };
            writer.Write(buf1);

            var buf2 = new byte[] {
                (byte)((FileOffset >> 0) & 0xFF),
                (byte)((FileOffset >> 8) & 0xFF),
                (byte)((FileOffset >> 16) & 0xFF)
            };
            writer.Write(buf2);

            writer.Write(FileUncompressedSize);

            if (IsEncrypted)
                writer.Write((uint)1);
            else
                writer.Write((uint)0);
        }
        public override string ToString()
        {
            return "Binary file: " + Path;
        }

        public override long GetFileSize()
        {
            return FileUncompressedSize;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class RpfResourceFileEntry : RpfFileEntry
    {
        public uint SystemFlags { get; set; }
        public uint GraphicsFlags { get; set; }


        public static int GetSizeFromFlags(uint flags)
        {
            //dexfx simplified version
            var s0 = ((flags >> 27) & 0x1)  << 0;   // 1 bit  - 27        (*1)
            var s1 = ((flags >> 26) & 0x1)  << 1;   // 1 bit  - 26        (*2)
            var s2 = ((flags >> 25) & 0x1)  << 2;   // 1 bit  - 25        (*4)
            var s3 = ((flags >> 24) & 0x1)  << 3;   // 1 bit  - 24        (*8)
            var s4 = ((flags >> 17) & 0x7F) << 4;   // 7 bits - 17 - 23   (*16)   (max 127 * 16)
            var s5 = ((flags >> 11) & 0x3F) << 5;   // 6 bits - 11 - 16   (*32)   (max 63  * 32)
            var s6 = ((flags >> 7)  & 0xF)  << 6;   // 4 bits - 7  - 10   (*64)   (max 15  * 64)
            var s7 = ((flags >> 5)  & 0x3)  << 7;   // 2 bits - 5  - 6    (*128)  (max 3   * 128)
            var s8 = ((flags >> 4)  & 0x1)  << 8;   // 1 bit  - 4         (*256)
            var ss = ((flags >> 0)  & 0xF);         // 4 bits - 0  - 3
            var baseSize = 0x200 << (int)ss;
            var size = baseSize * (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8);
            return (int)size;


            //var type = flags >> 28;
            //var test = GetFlagsFromSize((int)size, type);
            //s0 = ((test >> 27) & 0x1) << 0;   // 1 bit  - 27        (*1)
            //s1 = ((test >> 26) & 0x1) << 1;   // 1 bit  - 26        (*2)
            //s2 = ((test >> 25) & 0x1) << 2;   // 1 bit  - 25        (*4)
            //s3 = ((test >> 24) & 0x1) << 3;   // 1 bit  - 24        (*8)
            //s4 = ((test >> 17) & 0x7F) << 4;   // 7 bits - 17 - 23   (*16)   (max 127 * 16)
            //s5 = ((test >> 11) & 0x3F) << 5;   // 6 bits - 11 - 16   (*32)   (max 63  * 32)
            //s6 = ((test >> 7) & 0xF) << 6;   // 4 bits - 7  - 10   (*64)   (max 15  * 64)
            //s7 = ((test >> 5) & 0x3) << 7;   // 2 bits - 5  - 6    (*128)  (max 3   * 128)
            //s8 = ((test >> 4) & 0x1) << 8;   // 1 bit  - 4         (*256)
            //ss = ((test >> 0) & 0xF);         // 4 bits - 0  - 3
            //baseSize = 0x200 << (int)ss;
            //var tsize = baseSize * (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8);
            //if (tsize != size)
            //{ }


            //if (s8 == 256)
            //{ }
            //if ((s0 != 0) || (s1 != 0) || (s2 != 0) || (s3 != 0))
            //{ }


            //return (int)size;

            //examples:
            //size:8192,    ss:0,             s4:1                              (ytd)
            //size:16384,   ss:0,                    s5:1                       (ytyp)
            //size:24576,   ss:0,             s4:1,  s5:1                       (ytyp)
            //size:40960,   ss:0,             s4:1,  s5:2                       (ytyp)
            //size:49152,   ss:0,             s4:2,  s5:2                       (ytyp)
            //size:237568,  ss:0,             s4:5,               s7:1, s8:1    (yft)
            //size:262144,  ss:1,                                       s8:1    (yft)
            //size:589824,  ss:1,                           s6:9                (ytd)
            //size:663552,  ss:1,       s3:1, s4:12,        s6:1, s7:3          (ydd) 
            //size:606208,  ss:2,       s3:1, s4:2,                     s8:1    (ydr)
            //size:958464,  ss:2, s2:1,       s4:1,         s6:3,       s8:1    (ydr)
            //size:966656,  ss:2,       s3:1, s4:1,         s6:3,       s8:1    (ydr)
            //size:1695744, ss:2, s2:1, s3:1, s4:5,  s5:3,        s7:3, s8:1    (ydr)
            //size:2768896, ss:3, s2:1,       s4:24, s5:1,  s6:4                (ydd)
            //size:4063232, ss:4,             s4:15,              s7:2          (ytd)
            //size:8650752, ss:5,             s4:13,        s6:5                (ytd)







            #region  original neo version (system)
            //const int RESOURCE_IDENT = 0x37435352;
            //const int BASE_SIZE = 0x2000;
            //var SystemPagesDiv16 = (int)(SystemFlags >> 27) & 0x1;
            //var SystemPagesDiv8 = (int)(SystemFlags >> 26) & 0x1;
            //var SystemPagesDiv4 = (int)(SystemFlags >> 25) & 0x1;
            //var SystemPagesDiv2 = (int)(SystemFlags >> 24) & 0x1;
            //var SystemPagesMul1 = (int)(SystemFlags >> 17) & 0x7F;
            //var SystemPagesMul2 = (int)(SystemFlags >> 11) & 0x3F;
            //var SystemPagesMul4 = (int)(SystemFlags >> 7) & 0xF;
            //var SystemPagesMul8 = (int)(SystemFlags >> 5) & 0x3;
            //var SystemPagesMul16 = (int)(SystemFlags >> 4) & 0x1;
            //var SystemPagesSizeShift = (int)(SystemFlags >> 0) & 0xF;
            //var systemBaseSize = BASE_SIZE << SystemPagesSizeShift;
            //return
            //    (systemBaseSize * SystemPagesDiv16) / 16 +
            //    (systemBaseSize * SystemPagesDiv8) / 8 +
            //    (systemBaseSize * SystemPagesDiv4) / 4 +
            //    (systemBaseSize * SystemPagesDiv2) / 2 +
            //    (systemBaseSize * SystemPagesMul1) * 1 +
            //    (systemBaseSize * SystemPagesMul2) * 2 +
            //    (systemBaseSize * SystemPagesMul4) * 4 +
            //    (systemBaseSize * SystemPagesMul8) * 8 +
            //    (systemBaseSize * SystemPagesMul16) * 16;
            #endregion


            #region  original neo version (graphics)
            //const int RESOURCE_IDENT = 0x37435352;
            //const int BASE_SIZE = 0x2000;
            //var GraphicsPagesDiv16 = (int)(GraphicsFlags >> 27) & 0x1;
            //var GraphicsPagesDiv8 = (int)(GraphicsFlags >> 26) & 0x1;
            //var GraphicsPagesDiv4 = (int)(GraphicsFlags >> 25) & 0x1;
            //var GraphicsPagesDiv2 = (int)(GraphicsFlags >> 24) & 0x1;
            //var GraphicsPagesMul1 = (int)(GraphicsFlags >> 17) & 0x7F;
            //var GraphicsPagesMul2 = (int)(GraphicsFlags >> 11) & 0x3F;
            //var GraphicsPagesMul4 = (int)(GraphicsFlags >> 7) & 0xF;
            //var GraphicsPagesMul8 = (int)(GraphicsFlags >> 5) & 0x3;
            //var GraphicsPagesMul16 = (int)(GraphicsFlags >> 4) & 0x1;
            //var GraphicsPagesSizeShift = (int)(GraphicsFlags >> 0) & 0xF;
            //var graphicsBaseSize = BASE_SIZE << GraphicsPagesSizeShift;
            //return
            //    graphicsBaseSize * GraphicsPagesDiv16 / 16 +
            //    graphicsBaseSize * GraphicsPagesDiv8 / 8 +
            //    graphicsBaseSize * GraphicsPagesDiv4 / 4 +
            //    graphicsBaseSize * GraphicsPagesDiv2 / 2 +
            //    graphicsBaseSize * GraphicsPagesMul1 * 1 +
            //    graphicsBaseSize * GraphicsPagesMul2 * 2 +
            //    graphicsBaseSize * GraphicsPagesMul4 * 4 +
            //    graphicsBaseSize * GraphicsPagesMul8 * 8 +
            //    graphicsBaseSize * GraphicsPagesMul16 * 16;
            #endregion

        }
        public static uint GetFlagsFromSize(int size, uint version)
        {
            //WIP - may make crashes :(
            //type: see SystemSize and GraphicsSize below

            //aim for s4: blocksize (0 remainder for 0x2000 block) 
            int origsize = size;
            int remainder = size & 0x1FF;
            int blocksize = 0x200;
            if (remainder != 0)
            {
                size = (size - remainder) + blocksize; //round up to the minimum blocksize
            }

            uint blockcount = (uint)size >> 9; //how many blocks of the minimum size (0x200)
            uint ss = 0;
            while (blockcount > 1024)
            {
                ss++;
                blockcount = blockcount >> 1;
            }
            if (ss > 0)
            {
                size = origsize;
                blocksize = blocksize << (int)ss; //adjust the block size to reduce the block count.
                remainder = size & blocksize;
                if(remainder!=0)
                {
                    size = (size - remainder) + blocksize; //readjust size with round-up
                }
            }

            var s0 = (blockcount >> 0) & 0x1;  //*1         X
            var s1 = (blockcount >> 1) & 0x1;  //*2          X
            var s2 = (blockcount >> 2) & 0x1;  //*4           X
            var s3 = (blockcount >> 3) & 0x1;  //*8            X
            var s4 = (blockcount >> 4) & 0x7F; //*16  7 bits    XXXXXXX
            var s5 = (blockcount >> 5) & 0x3F; //*32  6 bits           XXXXXX
            var s6 = (blockcount >> 6) & 0xF;  //*64  4 bits                 XXXX
            var s7 = (blockcount >> 7) & 0x3;  //*128 2 bits                     XX
            var s8 = (blockcount >> 8) & 0x1;  //*256                              X

            if (ss > 4)
            { }
            if (s4 > 0x7F)
            { } //too big...
            //needs more work to include higher bits..


            uint f = 0;
            f |= (version & 0xF) << 28;
            f |= (s0 & 0x1) << 27;
            f |= (s1 & 0x1) << 26;
            f |= (s2 & 0x1) << 25;
            f |= (s3 & 0x1) << 24;
            f |= (s4 & 0x7F) << 17;
            f |= (ss & 0xF);
            


            return f;


            //var s0 = ((flags >> 27) & 0x1) << 0;   // 1 bit  - 27        (*1)
            //var s1 = ((flags >> 26) & 0x1) << 1;   // 1 bit  - 26        (*2)
            //var s2 = ((flags >> 25) & 0x1) << 2;   // 1 bit  - 25        (*4)
            //var s3 = ((flags >> 24) & 0x1) << 3;   // 1 bit  - 24        (*8)
            //var s4 = ((flags >> 17) & 0x7F) << 4;   // 7 bits - 17 - 23   (*16)   (max 127 * 16)
            //var s5 = ((flags >> 11) & 0x3F) << 5;   // 6 bits - 11 - 16   (*32)   (max 63  * 32)
            //var s6 = ((flags >> 7) & 0xF) << 6;   // 4 bits - 7  - 10   (*64)   (max 15  * 64)
            //var s7 = ((flags >> 5) & 0x3) << 7;   // 2 bits - 5  - 6    (*128)  (max 3   * 128)
            //var s8 = ((flags >> 4) & 0x1) << 8;   // 1 bit  - 4         (*256)
            //var ss = ((flags >> 0) & 0xF);         // 4 bits - 0  - 3
            //var baseSize = 0x200 << (int)ss;
            //var size = baseSize * (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8);


        }
        public static uint GetFlagsFromBlocks(uint blockCount, uint blockSize, uint version)
        {

            //dexfx test version - seems to work mostly...

            uint s0 = 0;
            uint s1 = 0;
            uint s2 = 0;
            uint s3 = 0;
            uint s4 = 0;
            uint s5 = 0;
            uint s6 = 0;
            uint s7 = 0;
            uint s8 = 0;
            uint ss = 0;

            uint bst = blockSize;
            if (blockCount > 0)
            {
                while (bst > 0x200) //ss is number of bits to shift 0x200 to get blocksize...
                {
                    ss++;
                    bst = bst >> 1;
                }
            }
            s0 = (blockCount >> 0) & 0x1;  //*1         X
            s1 = (blockCount >> 1) & 0x1;  //*2          X
            s2 = (blockCount >> 2) & 0x1;  //*4           X
            s3 = (blockCount >> 3) & 0x1;  //*8            X
            s4 = (blockCount >> 4) & 0x7F; //*16  7 bits    XXXXXXX
            //s5 = (blockCount >> 5) & 0x3F; //*32  6 bits           XXXXXX
            //s6 = (blockCount >> 6) & 0xF;  //*64  4 bits                 XXXX
            //s7 = (blockCount >> 7) & 0x3;  //*128 2 bits                     XX
            //s8 = (blockCount >> 8) & 0x1;  //*256                              X


            //if (blockCount > 0)
            //{
            //    var curblocksize = 0x2000u;
            //    var totsize = blockCount * blockSize;
            //    var totcount = totsize / curblocksize;
            //    if ((totsize % curblocksize) > 0) totcount++;
            //    ss = 4;
            //    while (totcount > 0x7f)
            //    {
            //        ss++;
            //        curblocksize = curblocksize << 1;
            //        totcount = totsize / curblocksize;
            //        if ((totsize % curblocksize) > 0) totcount++;
            //        if (ss >= 16)
            //        { break; }
            //    }
            //    s4 = totcount >> 4;
            //    s3 = (totcount >> 3) & 1;
            //    s2 = (totcount >> 2) & 1;
            //    s1 = (totcount >> 1) & 1;
            //    s0 = (totcount >> 0) & 1;
            //}



            if (ss > 0xF)
            { } //too big...
            if (s4 > 0x7F)
            { } //too big...
            //needs more work to include higher bits..


            uint f = 0;
            f |= (version & 0xF) << 28;
            f |= (s0 & 0x1) << 27;
            f |= (s1 & 0x1) << 26;
            f |= (s2 & 0x1) << 25;
            f |= (s3 & 0x1) << 24;
            f |= (s4 & 0x7F) << 17;
            f |= (s5 & 0x3F) << 11;
            f |= (s6 & 0xF) << 7;
            f |= (s7 & 0x3) << 5;
            f |= (s8 & 0x1) << 4;
            f |= (ss & 0xF);



            return f;
        }
        public static int GetVersionFromFlags(uint sysFlags, uint gfxFlags)
        {
            var sv = (sysFlags >> 28) & 0xF;
            var gv = (gfxFlags >> 28) & 0xF;
            return (int)((sv << 4) + gv);
        }


        public int Version
        {
            get
            {
                return GetVersionFromFlags(SystemFlags, GraphicsFlags);
            }
        }


        public int SystemSize
        {
            get
            {
                var sv = (SystemFlags >> 28);
                switch(sv)
                {
                    case 0: break; //ytd, ytyp, ...
                    case 10:break;//ydr, ydd, yft
                    case 4:break; //ypt
                    case 2:break; //ycd
                    case 1:break;//yed
                    default:break;
                }

                return GetSizeFromFlags(SystemFlags);
            }
        }
        public int GraphicsSize
        {
            get
            {
                var gv = (GraphicsFlags >> 28);
                switch (gv)
                {
                    case 0: break; //empty? some vehicle addon yft
                    case 1: break; //ynd
                    case 2: break; //ytyp, ymap
                    case 4: break; //yfd
                    case 5: break; //ydd, ydr, yft
                    case 9: break; //yed
                    case 10: break; //ysc
                    case 11: break; //ybn
                    case 13: break; //ytd
                    case 14: break; //ycd
                    default: break;
                }

                return GetSizeFromFlags(GraphicsFlags);
            }
        }

        public override void Read(DataReader reader)
        {
            NameOffset = reader.ReadUInt16();

            var buf1 = reader.ReadBytes(3);
            FileSize = (uint)buf1[0] + (uint)(buf1[1] << 8) + (uint)(buf1[2] << 16);

            var buf2 = reader.ReadBytes(3);
            FileOffset = ((uint)buf2[0] + (uint)(buf2[1] << 8) + (uint)(buf2[2] << 16)) & 0x7FFFFF;

            SystemFlags = reader.ReadUInt32();
            GraphicsFlags = reader.ReadUInt32();

            // there are sometimes resources with length=0xffffff which actually
            // means length>=0xffffff
            if (FileSize == 0xFFFFFF)
            {
                BinaryReader cfr = File.CurrentFileReader;
                long opos = cfr.BaseStream.Position;
                cfr.BaseStream.Position = File.StartPos + ((long)FileOffset * 512); //need to use the base offset!!
                var buf = cfr.ReadBytes(16);
                FileSize = ((uint)buf[7] << 0) | ((uint)buf[14] << 8) | ((uint)buf[5] << 16) | ((uint)buf[2] << 24);
                cfr.BaseStream.Position = opos;
            }

        }
        public override void Write(DataWriter writer)
        {
            writer.Write((ushort)NameOffset);

            var buf1 = new byte[] {
                (byte)((FileSize >> 0) & 0xFF),
                (byte)((FileSize >> 8) & 0xFF),
                (byte)((FileSize >> 16) & 0xFF)
            };
            writer.Write(buf1);

            var buf2 = new byte[] {
                (byte)((FileOffset >> 0) & 0xFF),
                (byte)((FileOffset >> 8) & 0xFF),
                (byte)(((FileOffset >> 16) & 0xFF) | 0x80)
            };
            writer.Write(buf2);

            writer.Write(SystemFlags);
            writer.Write(GraphicsFlags);
        }
        public override string ToString()
        {
            return "Resource file: " + Path;
        }

        public override long GetFileSize()
        {
            return (FileSize == 0) ? (long)(SystemSize + GraphicsSize) : FileSize;
        }
    }



    public interface PackedFile //interface for the different file types to use
    {
        void Load(byte[] data, RpfFileEntry entry);
    }










}