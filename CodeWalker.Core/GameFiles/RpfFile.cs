using CodeWalker.Core.Utils;
using CodeWalker.World;
using Collections.Pooled;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public struct FileCounts
    {
        public uint Rpfs;
        public uint Files;
        public uint Folders;
        public uint Resources;
        public uint BinaryFiles;

        public readonly bool Equals(in FileCounts b)
        {
            return
                Rpfs == b.Rpfs
                && Files == b.Files
                && Folders == b.Folders
                && Resources == b.Resources
                && BinaryFiles == b.BinaryFiles;
        }

        public static FileCounts operator +(in FileCounts a, in FileCounts b)
        {
            return new FileCounts
            {
                Rpfs = a.Rpfs + b.Rpfs,
                Files = a.Files + b.Files,
                Folders = a.Folders + b.Folders,
                Resources = a.Resources + b.Resources,
                BinaryFiles = a.BinaryFiles + b.BinaryFiles
            };
        }

        public static bool operator ==(in FileCounts left, in FileCounts right)
        {
            return left.Equals(in right);
        }

        public static bool operator !=(in FileCounts left, in FileCounts right)
        {
            return !(left == right);
        }
    }

    public class RpfFile
    {
        public string Name { get; set; } //name of this RPF file/package
        public string Path { get; set; } //path within the RPF structure
        public string FilePath { get; set; } //full file path of the RPF
        public long FileSize { get; set; }
        public string LastError { get; set; }
        public Exception LastException { get; set; }

        public RpfDirectoryEntry Root { get; set; }

        public bool IsAESEncrypted { get; set; }
        public bool IsNGEncrypted { get; set; }


        //offset in the current file
        public long StartPos { get; set; }

        //header data
        public uint Version { get; set; }
        public uint EntryCount { get; set; }
        public uint NamesLength { get; set; }
        public RpfEncryption Encryption { get; set; }

        //object linkage
        public PooledList<RpfEntry> AllEntries { get; set; }
        public PooledList<RpfFile> Children { get; set; }
        public RpfFile Parent { get; set; }
        public RpfBinaryFileEntry ParentFileEntry { get; set; }

        public BinaryReader CurrentFileReader { get; set; } //for temporary use while reading header



        public uint TotalFileCount { get; set; }

        public RpfFile(FileInfo fileInfo)
        {
            Name = fileInfo.Name;
            FilePath = fileInfo.FullName;
            FileSize = fileInfo.Length;
        }

        public RpfFile(string fpath, string relpath) //for a ROOT filesystem RPF
        {
            FileInfo fi = new FileInfo(fpath);
            Name = fi.Name;
            Path = relpath;
            FilePath = fpath;
            FileSize = fi.Length;
        }
        public RpfFile(string name, string path, long filesize) //for a child RPF
        {
            Name = name;
            Path = path;
            FilePath = path;
            FileSize = filesize;
        }

        // Returns string to new path
        public string CopyToModsFolder(out string status)
        {
            RpfFile parentFile = GetTopParent();
            string rel_parent_path = parentFile.Path;
            string full_parent_path = parentFile.FilePath;

            if(rel_parent_path.StartsWith(@"mods\", StringComparison.OrdinalIgnoreCase))
            {
                status = "already in mods folder";
                return null;
            }

            if(!full_parent_path.EndsWith(rel_parent_path))
            {
                throw new DirectoryNotFoundException("Expected full parent path to end with relative path");
            }

            string mods_base_path = full_parent_path.Replace(rel_parent_path, @"mods\");
            string dest_path = mods_base_path + rel_parent_path;

            try
            {
                File.Copy(full_parent_path, dest_path);
                status = $"copied \"{parentFile.Name}\" from \"{full_parent_path}\" to \"{dest_path}\"";
                return dest_path;
            } catch (IOException e)
            {
                status = $"unable to copy \"{parentFile.Name}\" from \"{full_parent_path}\" to \"{dest_path}\": {e.Message}";
                return null;
            } 
        }

        public bool IsInModsFolder()
        {
            return GetTopParent().Path.StartsWith(@"mods\", StringComparison.OrdinalIgnoreCase);
        }

        public RpfFile GetTopParent()
        {
            RpfFile pfile = this;
            while (pfile.Parent != null)
            {
                pfile = pfile.Parent;
            }
            return pfile;
        }
        
        public string GetPhysicalFilePath()
        {
            return GetTopParent().FilePath;
        }


        private void ThrowInvalidResource()
        {
            throw new Exception("Invalid Resource - not GTAV!");
        }

        [ThreadStatic]
        private static Stack<RpfDirectoryEntry> cachedStack;
        private void ReadHeader(BinaryReader br)
        {
            StartPos = br.BaseStream.Position;

            Version = br.ReadUInt32(); //RPF Version - GTAV should be 0x52504637 (1380992567)
            var entryCount = br.ReadUInt32(); //Number of Entries
            EntryCount = entryCount;
            NamesLength = br.ReadUInt32();
            Encryption = (RpfEncryption)br.ReadUInt32(); //0x04E45504F (1313165391): none;  0x0ffffff9 (268435449): AES

            if (Version != 0x52504637)
            {
                ThrowInvalidResource();
            }

            var entriesLength = (int)entryCount * 16;
            var namesLength = (int)NamesLength;
            byte[] entriesdata = ArrayPool<byte>.Shared.Rent(entriesLength);
            byte[] namesdata = ArrayPool<byte>.Shared.Rent(namesLength);

            br.BaseStream.Read(entriesdata, 0, entriesLength);
            br.BaseStream.Read(namesdata, 0, namesLength);

            switch (Encryption)
            {
                case RpfEncryption.NONE: //no encryption
                case RpfEncryption.OPEN: //OpenIV style RPF with unencrypted TOC
                case RpfEncryption.CFXP:
                    break;
                case RpfEncryption.AES:
                    GTACrypto.DecryptAES(entriesdata, entriesLength);
                    GTACrypto.DecryptAES(namesdata, namesLength);

                    IsAESEncrypted = true;
                    break;
                case RpfEncryption.NG:
                default:
                    GTACrypto.DecryptNG(entriesdata.AsMemory(0, entriesLength), Name, (uint)FileSize);
                    GTACrypto.DecryptNG(namesdata.AsMemory(0, namesLength), Name, (uint)FileSize);

                    IsNGEncrypted = true;
                    break;
            }


            var entriessequence = new ReadOnlySequence<byte>(entriesdata);
            var entriesrdr = new SequenceReader<byte>(entriessequence);

            var namessequence = new ReadOnlySequence<byte>(namesdata);
            var namesrdr = new SequenceReader<byte>(namessequence);


            var allEntries = new PooledList<RpfEntry>((int)entryCount);
            AllEntries = allEntries;
            var totalFileCount = 0u;

            for (uint i = 0; i < entryCount; i++)
            {
                ulong xy = entriesrdr.ReadUInt64();

                uint x = (uint)(xy >> 32);

                RpfEntry e;

                if (x == 0x7fffff00) //directory entry
                {
                    e = new RpfDirectoryEntry();
                }
                else if ((x & 0x80000000) == 0) //binary file entry
                {
                    e = new RpfBinaryFileEntry();
                    totalFileCount++;
                }
                else //assume resource file entry
                {
                    e = new RpfResourceFileEntry();
                    totalFileCount++;
                }

                e.File = this;
                e.Header = xy;

                e.Read(ref entriesrdr, br);

                namesrdr.SetPosition(e.NameOffset);

                namesrdr.TryReadTo(out ReadOnlySpan<byte> buffer, 0);
                if (buffer.Length > 256)
                {
                    buffer = buffer.Slice(0, 256);
                }

                e.Name = Encoding.UTF8.GetStringPooled(buffer);
                JenkIndex.EnsureLower(e.Name);
                if (e is RpfResourceFileEntry rfe)// && string.IsNullOrEmpty(e.Name))
                {
                    rfe.IsEncrypted = rfe.IsExtension(".ysc");//any other way to know..?
                }


                allEntries.Add(e);
            }

            TotalFileCount = totalFileCount;

            Root = (RpfDirectoryEntry)allEntries[0];
            Root.Path = Path;

            var entriesSpan = AllEntries.Span;

            var stack = new Stack<RpfDirectoryEntry>();;
            stack.Push(Root);
            while (stack.Count > 0)
            {
                var item = stack.Pop();

                int starti = (int)item.EntriesIndex;
                int endi = (int)(starti + item.EntriesCount);

                for (int i = starti; i < endi; i++)
                {
                    RpfEntry e = entriesSpan[i];
                    e.Parent = item;
                    if (e is RpfDirectoryEntry rde)
                    {
                        rde.Path = $"{item.Path}\\{rde.Name}";
                        item.Directories.Add(rde);
                        stack.Push(rde);
                    }
                    else if (e is RpfFileEntry rfe)
                    {
                        rfe.Path = $"{item.Path}\\{rfe.Name}";
                        item.Files.Add(rfe);
                    }
                }
            }

            br.BaseStream.Position = StartPos;

            ArrayPool<byte>.Shared.Return(entriesdata);
            ArrayPool<byte>.Shared.Return(namesdata);
            stack.Clear();
        }

        public bool ScanStructure(Action<string>? updateStatus, Action<string>? errorLog, out FileCounts fileCounts)
        {

            using var fileStream = File.OpenRead(FilePath);
            using var br = new BinaryReader(fileStream);
            try
            {
                return ScanStructure(br, updateStatus, errorLog, out fileCounts);
            }
            catch (Exception ex)
            {
                LastError = ex.ToString();
                LastException = ex;
                errorLog?.Invoke($"{FilePath}: {ex}");
                Console.WriteLine($"{FilePath}: {ex}");
                fileCounts = default;
                return false;
            }
        }
        private bool ScanStructure(BinaryReader br, Action<string>? updateStatus, Action<string>? errorLog, out FileCounts fileCounts)
        {
            if (FilePath == "update\\update.rpf\\dlc_patch\\patchday1ng\\x64\\patch\\data\\lang\\chinesesimp.rpf")
            {
                fileCounts = default;
                return false;
            }
            try
            {
                ReadHeader(br);
            }
            catch 
            {
                fileCounts = default;
                return false;
            }

            fileCounts = new FileCounts
            {
                Rpfs = 1,
                Files = 1
            };

            Children = new PooledList<RpfFile>();

            updateStatus?.Invoke($"Scanning {Path}...");

            foreach (RpfEntry entry in AllEntries)
            {
                try
                {
                    if (entry is RpfBinaryFileEntry binentry)
                    {
                        //search all the sub resources for YSC files. (recurse!)
                        if (binentry.IsExtension(".rpf") && binentry.Path.Length < 5000) // a long path is most likely an attempt to crash CW, so skip it
                        {
                            br.BaseStream.Position = StartPos + ((long)binentry.FileOffset * 512);

                            long l = binentry.GetFileSize();

                            RpfFile subfile = new RpfFile(binentry.Name, binentry.Path, l);
                            subfile.Parent = this;
                            subfile.ParentFileEntry = binentry;

                            var success = subfile.ScanStructure(br, updateStatus, errorLog, out var result);

                            if (success)
                            {
                                fileCounts += result;
                                Children.Add(subfile);
                            }
                        }
                        else
                        {
                            //binary file that's not an rpf...
                            fileCounts.BinaryFiles++;
                            fileCounts.Files++;
                        }
                    }
                    else if (entry is RpfResourceFileEntry)
                    {
                        fileCounts.Resources++;
                        fileCounts.Files++;
                    }
                    else if (entry is RpfDirectoryEntry)
                    {
                        fileCounts.Folders++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    errorLog?.Invoke($"{entry.Path}: {ex}");
                }
            }
            return true;
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
            updateStatus?.Invoke($"Searching {Name}...");

            ReadHeader(br);

            //List<DataBlock> blocks = new List<DataBlock>();
            foreach (RpfEntry entry in AllEntries)
            {
                if (entry is RpfBinaryFileEntry)
                {
                    RpfBinaryFileEntry binentry = entry as RpfBinaryFileEntry;
                    long l = binentry.GetFileSize();

                    //search all the sub resources for YSC files. (recurse!)
                    if (binentry.IsExtension(".rpf"))
                    {
                        br.BaseStream.Position = StartPos + ((long)binentry.FileOffset * 512);

                        RpfFile subfile = new RpfFile(binentry.Name, binentry.Path, l);
                        subfile.Parent = this;
                        subfile.ParentFileEntry = binentry;

                        subfile.ExtractScripts(br, outputfolder, updateStatus);
                    }

                }
                else if (entry is RpfResourceFileEntry)
                {

                    RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;

                    if (resentry.IsExtension(".ysc"))
                    {
                        updateStatus?.Invoke($"Extracting {resentry.Name}...");

                        //found a YSC file. extract it!
                        string ofpath = $"{outputfolder}\\{resentry.Name}";

                        br.BaseStream.Position = StartPos + ((long)resentry.FileOffset * 512);

                        if (resentry.FileSize > 0)
                        {
                            uint offset = 0x10;
                            uint totlen = resentry.FileSize - offset;

                            byte[] tbytes = new byte[totlen];

                            br.BaseStream.Position += offset;

                            br.Read(tbytes, 0, (int)totlen);

                            if (IsAESEncrypted)
                            {
                                GTACrypto.DecryptAES(tbytes);

                                //special case! probable duplicate pilot_school.ysc
                                ofpath = $"{outputfolder}\\{Name}___{resentry.Name}";
                            }
                            else
                            {
                                GTACrypto.DecryptNG(tbytes.AsMemory(0, (int)totlen), resentry.Name, resentry.FileSize);
                            }


                            try
                            {
                                MemoryStream ms = new MemoryStream(tbytes);
                                DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress);

                                MemoryStream outstr = recyclableMemoryStreamManager.GetStream();
                                ds.CopyTo(outstr);
                                byte[] outbuf = outstr.ToArray();

                                bool pathok = true;
                                if (File.Exists(ofpath))
                                {
                                    ofpath = $"{outputfolder}\\{Name}_{resentry.Name}";
                                    if (File.Exists(ofpath))
                                    {
                                        LastError = $"Output file {ofpath} already exists!";
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



        //public Stream ExtractFileStream(RpfFileEntry entry)
        //{
        //    try
        //    {
        //        using (BinaryReader br = new BinaryReader(File.OpenRead(GetPhysicalFilePath())))
        //        {
        //            if (entry is RpfBinaryFileEntry binaryFileEntry)
        //            {
        //                return ExtractFileBinary(binaryFileEntry, br);
        //            }
        //            else if (entry is RpfResourceFileEntry resourceFileEntry)
        //            {
        //                return ExtractFileResource(resourceFileEntry, br);
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LastError = ex.ToString();
        //        LastException = ex;
        //        return null;
        //    }
        //}

        public byte[]? ExtractFile(RpfFileEntry entry)
        {
            try
            {
                using var fileStream = new FileStream(GetPhysicalFilePath(), FileMode.Open, FileAccess.Read, FileShare.Read, 0);
                using (BinaryReader br = new BinaryReader(fileStream))
                {
                    if (entry is RpfBinaryFileEntry binaryFileEntry)
                    {
                        return ExtractFileBinary(binaryFileEntry, br);
                    }
                    else if (entry is RpfResourceFileEntry resourceFileEntry)
                    {
                        return ExtractFileResource(resourceFileEntry, br);
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
                Console.WriteLine(ex);
                return null;
            }
        }

        public async ValueTask<byte[]?> ExtractFileAsync(RpfFileEntry entry)
        {
            try
            {
                using var fileStream = new FileStream(GetPhysicalFilePath(), FileMode.Open, FileAccess.Read, FileShare.Read, 0, FileOptions.Asynchronous);
                if (entry is RpfBinaryFileEntry binaryFileEntry)
                {
                    return await ExtractFileBinaryAsync(binaryFileEntry, fileStream);
                }
                else if (entry is RpfResourceFileEntry resourceFileEntry)
                {
                    return await ExtractFileResourceAsync(resourceFileEntry, fileStream);
                }
                else
                {
                    Console.WriteLine($"{entry} is not a BinaryFileEntry of ResourceFileEntry");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LastError = ex.ToString();
                LastException = ex;
                return null;
            }
        }

        public async ValueTask<byte[]?> ExtractFileBinaryAsync(RpfBinaryFileEntry entry, Stream stream)
        {
            stream.Position = StartPos + ((long)entry.FileOffset * 512);

            long l = entry.GetFileSize();

            if (l <= 0)
            {
                return null;
            }

            uint offset = 0;// 0x10;
            uint totlen = (uint)l - offset;

            byte[] tbytes = ArrayPool<byte>.Shared.Rent((int)totlen);

            stream.Position += offset;
            await stream.ReadAsync(tbytes, 0, (int)totlen).ConfigureAwait(false);

            if (entry.IsEncrypted)
            {
                if (IsAESEncrypted)
                {
                    GTACrypto.DecryptAES(tbytes, (int)totlen);
                }
                else //if (IsNGEncrypted) //assume the archive is set to NG encryption if not AES... (comment: fix for openIV modded files)
                {
                    GTACrypto.DecryptNG(tbytes.AsMemory(0, (int)totlen), entry.Name, entry.FileUncompressedSize);
                }
            }

            byte[] defl;
            if (entry.FileSize > 0) //apparently this means it's compressed
            {
                defl = await DecompressBytesAsync(tbytes, entry.GetUncompressedFileSize()).ConfigureAwait(false);
            }
            else
            {
                defl = new byte[(int)totlen];
                Array.Copy(tbytes, defl, (int)totlen);
            }

            ArrayPool<byte>.Shared.Return(tbytes);

            return defl;
        }

        public ValueTask<byte[]?> ExtractFileBinaryAsync(RpfBinaryFileEntry entry, BinaryReader br)
        {
            return ExtractFileBinaryAsync(entry, br.BaseStream);
        }

        public byte[]? ExtractFileBinary(RpfBinaryFileEntry entry, BinaryReader br)
        {
            br.BaseStream.Position = StartPos + ((long)entry.FileOffset * 512);

            long l = entry.GetFileSize();

            if (l <= 0)
            {
                return null;
            }

            uint offset = 0;// 0x10;
            uint totlen = (uint)l - offset;

            byte[] tbytes = ArrayPool<byte>.Shared.Rent((int)totlen);

            br.BaseStream.Position += offset;
            br.Read(tbytes, 0, (int)totlen);

            if (entry.IsEncrypted)
            {
                if (IsAESEncrypted)
                {
                    GTACrypto.DecryptAES(tbytes, (int)totlen);
                }
                else //if (IsNGEncrypted) //assume the archive is set to NG encryption if not AES... (comment: fix for openIV modded files)
                {
                    GTACrypto.DecryptNG(tbytes.AsMemory(0, (int)totlen), entry.Name, entry.FileUncompressedSize);
                }
            }

            byte[] defl;
            if (entry.FileSize > 0) //apparently this means it's compressed
            {
                defl = DecompressBytes(tbytes, entry.GetUncompressedFileSize());
            }
            else
            {
                defl = new byte[(int)totlen];
                Buffer.BlockCopy(tbytes, 0, defl, 0, (int)totlen);
            }

            ArrayPool<byte>.Shared.Return(tbytes);

            return defl;
        }

        public async ValueTask<byte[]?> ExtractFileResourceAsync(RpfResourceFileEntry entry, Stream stream)
        {
            stream.Position = StartPos + ((long)entry.FileOffset * 512);

            if (entry.FileSize <= 0)
            {
                return null;
            }

            uint offset = 0x10;
            uint totlen = entry.FileSize - offset;

            byte[] tbytes = ArrayPool<byte>.Shared.Rent((int)totlen);


            stream.Position += offset;

            await stream.ReadAsync(tbytes, 0, (int)totlen).ConfigureAwait(false);
            if (entry.IsEncrypted)
            {
                if (IsAESEncrypted)
                {
                    GTACrypto.DecryptAES(tbytes, (int)totlen);
                }
                else //if (IsNGEncrypted) //assume the archive is set to NG encryption if not AES... (comment: fix for openIV modded files)
                {
                    GTACrypto.DecryptNG(tbytes.AsMemory(0, (int)totlen), entry.Name, entry.FileSize);
                }
            }

            byte[] deflated = await DecompressBytesAsync(tbytes, entry.GetUncompressedFileSize());

            byte[] data;
            if (deflated != null)
            {
                data = deflated;
            }
            else
            {
                entry.FileSize -= offset;

                data = new byte[(int)totlen];
                Buffer.BlockCopy(tbytes, 0, data, 0, (int)totlen);
            }

            ArrayPool<byte>.Shared.Return(tbytes);

            return data;
        }

        public ValueTask<byte[]?> ExtractFileResourceAsync(RpfResourceFileEntry entry, BinaryReader br)
        {
            return ExtractFileResourceAsync(entry, br.BaseStream);
        }

        public byte[]? ExtractFileResource(RpfResourceFileEntry entry, BinaryReader br)
        {
            br.BaseStream.Position = StartPos + ((long)entry.FileOffset * 512);

            if (entry.FileSize <= 0)
            {
                return null;
            }

            uint offset = 0x10;
            uint totlen = entry.FileSize - offset;

            byte[] tbytes = ArrayPool<byte>.Shared.Rent((int)totlen);


            br.BaseStream.Position += offset;


            br.Read(tbytes, 0, (int)totlen);
            if (entry.IsEncrypted)
            {
                if (IsAESEncrypted)
                {
                    GTACrypto.DecryptAES(tbytes, (int)totlen);
                }
                else //if (IsNGEncrypted) //assume the archive is set to NG encryption if not AES... (comment: fix for openIV modded files)
                {
                    GTACrypto.DecryptNG(tbytes.AsMemory(0, (int)totlen), entry.Name, entry.FileSize);
                }
            }

            byte[] deflated = DecompressBytes(tbytes, entry.GetUncompressedFileSize());

            byte[] data;
            if (deflated != null)
            {
                data = deflated;
            }
            else
            {
                entry.FileSize -= offset;

                data = new byte[(int)totlen];
                Buffer.BlockCopy(tbytes, 0, data, 0, (int)totlen);
            }

            ArrayPool<byte>.Shared.Return(tbytes);

            return data;
        }

        public static T? GetFile<T>(RpfEntry e) where T : class, PackedFile, new()
        {
            T? file = null;
            byte[]? data = null;
            RpfFileEntry? entry = e as RpfFileEntry;
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
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            if (e is not RpfFileEntry entry)
            {
                entry = CreateResourceFileEntry(ref data, 0);
            }
            var file = new T();
            file.Load(data, entry);
            return file;
        }

        public static T GetFile<T>(RpfEntry e, Stream data) where T : class, PackedFileStream, new()
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            T? file = null;
            if (data is not null)
            {
                if (e is not RpfFileEntry entry)
                {
                    entry = CreateResourceFileEntry(data, 0);
                }
                file = new T();
                file.Load(data, entry);
            }
            return file;
        }



        public static T? GetResourceFile<T>(byte[] data) where T : class, PackedFile, new()
        {
            T? file = null;
            RpfFileEntry entry = CreateResourceFileEntry(ref data, 0);
            if (data != null && entry != null)
            {
                data = ResourceBuilder.Decompress(data);
                file = new T();
                file.Load(data, entry);
            }
            return file;
        }


        public static void LoadResourceFile<T>(T file, Stream data, uint ver) where T : class, PackedFileStream
        {
            //direct load from a raw, compressed resource file (openIV-compatible format)

            RpfResourceFileEntry resentry = CreateResourceFileEntry(data, ver);

            if (file is GameFile)
            {
                GameFile gfile = file as GameFile;

                if (gfile.RpfFileEntry is RpfResourceFileEntry oldresentry) //update the existing entry with the new one
                {
                    oldresentry.SystemFlags = resentry.SystemFlags;
                    oldresentry.GraphicsFlags = resentry.GraphicsFlags;
                    resentry.Name = oldresentry.Name;
                }
                else
                {
                    gfile.RpfFileEntry = resentry; //just stick it in there for later...
                }
            }

            data = ResourceBuilder.Decompress(data);

            file.Load(data, resentry);
        }

        public static void LoadResourceFile<T>(T file, byte[] data, uint ver) where T : class, PackedFile
        {
            //direct load from a raw, compressed resource file (openIV-compatible format)

            RpfResourceFileEntry resentry = CreateResourceFileEntry(ref data, ver);

            if (file is GameFile gfile)
            {
                if (gfile.RpfFileEntry is RpfResourceFileEntry oldresentry) //update the existing entry with the new one
                {
                    oldresentry.SystemFlags = resentry.SystemFlags;
                    oldresentry.GraphicsFlags = resentry.GraphicsFlags;
                    resentry.Name = oldresentry.Name;
                }
                else
                {
                    gfile.RpfFileEntry = resentry; //just stick it in there for later...
                }
            }

            data = ResourceBuilder.Decompress(data);

            file.Load(data, resentry);
        }

        public static async Task LoadResourceFileAsync<T>(T file, byte[] data, uint ver) where T : class, PackedFile
        {
            RpfResourceFileEntry resentry = CreateResourceFileEntry(ref data, ver);

            if (file is GameFile gfile)
            {
                if (gfile.RpfFileEntry is RpfResourceFileEntry oldresentry) //update the existing entry with the new one
                {
                    oldresentry.SystemFlags = resentry.SystemFlags;
                    oldresentry.GraphicsFlags = resentry.GraphicsFlags;
                    resentry.Name = oldresentry.Name;
                }
                else
                {
                    gfile.RpfFileEntry = resentry; //just stick it in there for later...
                }
            }

            data = await ResourceBuilder.DecompressAsync(data);

            file.Load(data, resentry);
        }

        public static RpfResourceFileEntry CreateResourceFileEntry(Stream stream, uint ver, uint? header = null)
        {
            var resentry = new RpfResourceFileEntry();

            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            //hopefully this data has an RSC7 header...
            uint rsc7 = header ?? reader.ReadUInt32(); // BitConverter.ToUInt32(data, 0);
            if (rsc7 == 0x37435352) //RSC7 header present!
            {
                int version = reader.ReadInt32(); // BitConverter.ToInt32(data, 4);//use this instead of what was given...
                resentry.SystemFlags = reader.ReadUInt32();// BitConverter.ToUInt32(data, 8);
                resentry.GraphicsFlags = reader.ReadUInt32(); //BitConverter.ToUInt32(data, 12);
                //if (stream.Length > 16)
                //{
                //    int newlen = stream.Length - 16; //trim the header from the data passed to the next step.
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
                resentry.SystemFlags = RpfResourceFileEntry.GetFlagsFromSize((int)stream.Length, 0);
                resentry.GraphicsFlags = RpfResourceFileEntry.GetFlagsFromSize(0, ver);
            }

            resentry.Name = "";

            return resentry;
        }

        public static RpfResourceFileEntry CreateResourceFileEntry(ref byte[] data, uint ver)
        {
            var resentry = new RpfResourceFileEntry();

            //hopefully this data has an RSC7 header...
            uint rsc7 = BitConverter.ToUInt32(data, 0);
            if (rsc7 == 0x37435352) //RSC7 header present!
            {
                int version = BitConverter.ToInt32(data, 4);//use this instead of what was given...
                resentry.SystemFlags = BitConverter.ToUInt32(data, 8);
                resentry.GraphicsFlags = BitConverter.ToUInt32(data, 12);
                if (data.Length > 16)
                {
                    int newlen = data.Length - 16; //trim the header from the data passed to the next step.
                    byte[] newdata = new byte[newlen];
                    Buffer.BlockCopy(data, 16, newdata, 0, newlen);
                    data = newdata;
                }
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
                resentry.GraphicsFlags = RpfResourceFileEntry.GetFlagsFromSize(0, ver);
            }

            resentry.Name = "";

            return resentry;
        }



        public string TestExtractAllFiles()
        {
            StringBuilder sb = new StringBuilder();
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
                            if (!entry.IsExtension(".rpf")) //don't try to extract rpf's, they will be done separately..
                            {
                                if (entry is RpfBinaryFileEntry binentry)
                                {
                                    try
                                    {
                                        byte[]? data = ExtractFileBinary(binentry, br);
                                        if (data is null)
                                        {
                                            if (binentry.FileSize == 0)
                                            {
                                                sb.AppendLine($"{entry.Path} : Binary FileSize is 0.");
                                            }
                                            else
                                            {
                                                sb.AppendLine($"{entry.Path} : {LastError}");
                                            }
                                        }
                                        else if (data.Length == 0)
                                        {
                                            sb.AppendLine($"{entry.Path} : Decompressed output was empty.");
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        sb.AppendLine($"{entry.Path} : {ex}");
                                    }
                                }
                                else if (entry is RpfResourceFileEntry resentry)
                                {
                                    try
                                    {
                                        byte[]? data = ExtractFileResource(resentry, br);
                                        if (data == null)
                                        {
                                            if (resentry.FileSize == 0)
                                            {
                                                sb.AppendLine($"{entry.Path} : Resource FileSize is 0.");
                                            }
                                            else
                                            {
                                                sb.AppendLine($"{entry.Path} : {LastError}");
                                            }
                                        }
                                        else if (data.Length == 0)
                                        {
                                            sb.AppendLine($"{entry.Path} : Decompressed output was empty.");
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        sb.AppendLine($"{entry.Path} : {ex}");
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LastError = ex.ToString();
                            LastException = ex;
                            Console.WriteLine(ex);
                            sb.AppendLine($"{entry.Path} : {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.ToString();
                LastException = ex;
                Console.WriteLine(ex);
                sb.AppendLine($"{Path} : {ex.Message}");
                return null;
            }
            return sb.ToString();
        }




        public IReadOnlyCollection<RpfFileEntry> GetFiles(string folder, bool recurse)
        {
            if (Root == null)
            {
                return [];
            }
            PooledList<RpfFileEntry> result = new PooledList<RpfFileEntry>();
            //folder.AsSpan().Split
            RpfDirectoryEntry dir = Root;
            foreach(var part in folder.EnumerateSplit('\\'))
            {
                if (part.Length == 0)
                    continue;
                dir = FindSubDirectory(dir, part);
                //if (dir is null)
                //{
                //    return Array.Empty<RpfFileEntry>();
                //}
            }

            GetFiles(dir, result, recurse);

            return result;
        }
        public void GetFiles(RpfDirectoryEntry dir, PooledList<RpfFileEntry> result, bool recurse)
        {
            if (dir.Files != null)
            {
                result.AddRange(dir.Files);
            }
            if (recurse)
            {
                if (dir.Directories != null)
                {
                    foreach(var dirEntry in dir.Directories)
                    {
                        GetFiles(dirEntry, result, recurse);
                    }
                }
            }
        }

        private RpfDirectoryEntry? FindSubDirectory(RpfDirectoryEntry dir, ReadOnlySpan<char> name)
        {
            if (dir == null) return null;
            if (dir.Directories == null) return null;

            for (int i = 0; i < dir.Directories.Count; i++)
            {
                var cdir = dir.Directories[i];
                if (cdir.Name.AsSpan() == name || cdir.Name.AsSpan().Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return cdir;
                }
            }

            return null;
        }


        public static RecyclableMemoryStreamManager recyclableMemoryStreamManager = new RecyclableMemoryStreamManager(256 * 1024, 1024 * 1024, 128 * 1024 * 1024, false, 256 * 1024 * 100, 1024 * 1024 * 128 * 4);

        public byte[] DecompressBytes(byte[] bytes, long requiredSize = -1)
        {
            try
            {
                if (requiredSize == -1)
                {
                    requiredSize = bytes.Length;
                }
                using DeflateStream ds = new DeflateStream(new MemoryStream(bytes), CompressionMode.Decompress);
                using var outstr = new MemoryStream((int)requiredSize);

                ds.CopyTo(outstr, 524288);

                byte[] outbuf = Array.Empty<byte>();
                try
                {
                    outbuf = outstr.GetBuffer();
                }
                catch (Exception ex)
                {
                    // Failed to get buffer
                    Console.WriteLine(ex);
                    outbuf = outstr.ToArray();
                }

                if (outbuf.Length <= bytes.Length)
                {
                    LastError = "Warning: Decompressed data was smaller than compressed data...";
                    //return null; //could still be OK for tiny things!
                }

                if (outbuf.Length != requiredSize)
                {
                    Console.WriteLine($"Buffer was resized for expectedSize: {requiredSize}; actualSize: {outbuf.Length}");
                }

                // Ensure array is correct size (this ensures everything works even is expectedSize is incorrect, but adds allocation overhead)
                if (outbuf.Length != outstr.Length)
                {
                    Console.WriteLine($"Calling to array on MemoryStream");
                    outbuf = outbuf.ToArray();
                }

                return outbuf;
            }
            catch (Exception ex)
            {
                LastError = "Could not decompress.";// ex.ToString();
                LastException = ex;
                return null;
            }
        }

        public async Task<byte[]> DecompressBytesAsync(byte[] bytes, long expectedSize = -1)
        {
            try
            {
                if (expectedSize == -1)
                {
                    expectedSize = bytes.Length;
                }
                using DeflateStream ds = new DeflateStream(new MemoryStream(bytes), CompressionMode.Decompress);
                using var outstr = new MemoryStream((int)expectedSize);

                await ds.CopyToAsync(outstr, 524288).ConfigureAwait(false);
                byte[] outbuf = Array.Empty<byte>();
                try
                {
                    outbuf = outstr.GetBuffer();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    outbuf = outstr.ToArray();
                }

                if (outbuf.Length <= bytes.Length)
                {
                    LastError = "Warning: Decompressed data was smaller than compressed data...";
                    //return null; //could still be OK for tiny things!
                }

                if (outbuf.Length != expectedSize)
                {
                    Console.WriteLine($"Buffer was resized for expectedSize: {expectedSize}; actualSize: {outbuf.Length}");
                }

                // Ensure array is correct size (this ensures everything works even is expectedSize is incorrect, but adds allocation overhead)
                if (outbuf.Length != outstr.Length)
                {
                    Console.WriteLine($"Calling to array on MemoryStream");
                    outbuf = outbuf.ToArray();
                }

                return outbuf;
            }
            catch (Exception ex)
            {
                LastError = "Could not decompress.";// ex.ToString();
                LastException = ex;
                return null;
            }
        }

        public static byte[] CompressBytes(byte[] data) //TODO: refactor this with ResourceBuilder.Compress/Decompress
        {
            using (MemoryStream ms = recyclableMemoryStreamManager.GetStream("CompressBytes"))
            {
                using (var ds = new DeflateStream(ms, CompressionLevel.SmallestSize, true))
                {
                    ds.Write(data, 0, data.Length);
                    ds.Close();
                    byte[] outbuf = ms.ToArray(); //need to copy to the right size buffer...
                    return outbuf;
                }
            }
        }














        private void WriteHeader(BinaryWriter bw)
        {
            var namesdata = GetHeaderNamesData();
            NamesLength = (uint)namesdata.Length;

            //ensure there's enough space for the new header, move things if necessary
            var headersize = GetHeaderBlockCount() * 512;
            EnsureSpace(bw, null, headersize);

            //entries may have been updated, so need to do this after ensuring header space
            var entriesdata = GetHeaderEntriesData();

            //FileSize = ... //need to make sure this is updated for NG encryption...
            switch (Encryption)
            {
                case RpfEncryption.NONE: //no encryption
                case RpfEncryption.OPEN: //OpenIV style RPF with unencrypted TOC
                case RpfEncryption.CFXP:
                    break;
                case RpfEncryption.AES:
                    entriesdata = GTACrypto.EncryptAES(entriesdata);
                    namesdata = GTACrypto.EncryptAES(namesdata);
                    IsAESEncrypted = true;
                    break;
                case RpfEncryption.NG:
                    entriesdata = GTACrypto.EncryptNG(entriesdata, Name, (uint)FileSize);
                    namesdata = GTACrypto.EncryptNG(namesdata, Name, (uint)FileSize);
                    IsNGEncrypted = true;
                    break;
                default: //unknown encryption type? assume NG.. should never get here!
                    entriesdata = GTACrypto.EncryptNG(entriesdata, Name, (uint)FileSize);
                    namesdata = GTACrypto.EncryptNG(namesdata, Name, (uint)FileSize);
                    break;
            }

            //now there's enough space, it's safe to write the header data...
            bw.BaseStream.Position = StartPos;

            bw.Write(Version);
            bw.Write(EntryCount);
            bw.Write(NamesLength);
            bw.Write((uint)Encryption);
            bw.Write(entriesdata);
            bw.Write(namesdata);

            WritePadding(bw.BaseStream, StartPos + headersize); //makes sure the actual file can grow...
        }


        private static void WritePadding(Stream s, long upto)
        {
            int diff = (int)(upto - s.Position);
            if (diff > 0)
            {
                s.Write(new byte[diff], 0, diff);
            }
        }

        [MemberNotNull(nameof(AllEntries))]
        private void EnsureAllEntries()
        {
            if (AllEntries == null)
            {
                //assume this is a new RPF, create the root directory entry
                AllEntries = new PooledList<RpfEntry>();
                Root = new RpfDirectoryEntry();
                Root.File = this;
                Root.Name = string.Empty;
                Root.Path = Path;
            }
            if (Children == null)
            {
                Children = new PooledList<RpfFile>();
            }



            //re-build the AllEntries list from the root node.
            List<RpfEntry> temp = new List<RpfEntry>(); //for sorting

            //using var tempList = new PooledList<RpfEntry>();

            AllEntries.Clear();
            //tempList.Add(Root);
            AllEntries.Add(Root);
            Stack<RpfDirectoryEntry> stack = new Stack<RpfDirectoryEntry>();
            stack.Push(Root);
            while (stack.Count > 0)
            {
                var item = stack.Pop();

                item.EntriesCount = (uint)(item.Directories.Count + item.Files.Count);
                item.EntriesIndex = (uint)AllEntries.Count;

                //having items sorted by name is important for the game for some reason. (it crashes otherwise!)
                temp.Clear();
                temp.AddRange(item.Directories);
                temp.AddRange(item.Files);
                temp.Sort((a, b) => String.CompareOrdinal(a.Name, b.Name));

                foreach (var entry in temp)
                {
                    //tempList.Add(entry);
                    AllEntries.Add(entry);
                    if (entry is RpfDirectoryEntry dir)
                    {
                        stack.Push(dir);
                    }
                }
            }

            //AllEntries.AddRange(tempRange);

            EntryCount = (uint)AllEntries.Count;

        }
        private byte[] GetHeaderNamesData()
        {
            using MemoryStream namesstream = new MemoryStream();
            using DataWriter nameswriter = new DataWriter(namesstream);
            var namedict = new Dictionary<string, uint>();
            foreach (var entry in AllEntries)
            {
                uint nameoffset;
                string name = entry.Name ?? "";
                if (namedict.TryGetValue(name, out nameoffset))
                {
                    entry.NameOffset = nameoffset;
                }
                else
                {
                    entry.NameOffset = (uint)namesstream.Length;
                    namedict.Add(name, entry.NameOffset);
                    nameswriter.Write(name);
                }
            }
            var buf = new byte[Math.Max(namesstream.Length, 16)];
            namesstream.Position = 0;
            namesstream.Read(buf, 0, (int)namesstream.Length);
            return buf;
        }
        private byte[] GetHeaderEntriesData()
        {
            MemoryStream entriesstream = new MemoryStream();
            DataWriter entrieswriter = new DataWriter(entriesstream);
            foreach (var entry in AllEntries)
            {
                entry.Write(entrieswriter);
            }
            var buf = new byte[entriesstream.Length];
            entriesstream.Position = 0;
            entriesstream.Read(buf, 0, buf.Length);
            return buf;
        }
        private uint GetHeaderBlockCount()//make sure EntryCount and NamesLength are updated before calling this...
        {
            uint headerusedbytes = 16 + (EntryCount * 16) + NamesLength;
            uint headerblockcount = GetBlockCount(headerusedbytes);
            return headerblockcount;
        }
        private static uint PadLength(uint l, uint n)//round up to nearest n bytes
        {
            uint rem = l % n;
            return l + ((rem > 0) ? (n - rem) : 0);
        }
        private static uint GetBlockCount(long bytecount)
        {
            uint b0 = (uint)(bytecount & 0x1FF); //511;
            uint b1 = (uint)(bytecount >> 9);
            if (b0 == 0) return b1;
            return b1 + 1;
        }
        private RpfFileEntry FindFirstFileAfter(uint block)
        {
            RpfFileEntry nextentry = null;
            foreach (var entry in AllEntries)
            {
                if ((entry is RpfFileEntry fe) && (fe.FileOffset > block))
                {
                    if ((nextentry == null) || (fe.FileOffset < nextentry.FileOffset))
                    {
                        nextentry = fe;
                    }
                }
            }
            return nextentry;
        }
        private uint FindHole(uint reqblocks, uint ignorestart, uint ignoreend)
        {
            //find the block index of a hole that can fit the required number of blocks.
            //return 0 if no hole found (0 is the header block, it can't be used for files!)
            //make sure any found hole is not within the ignore range
            //(i.e. area where space is currently being made)

            //gather and sort the list of files to allow searching for holes
            List<RpfFileEntry> allfiles = new List<RpfFileEntry>();
            foreach (var entry in AllEntries)
            {
                if (entry is RpfFileEntry rfe)
                {
                    allfiles.Add(rfe);
                }
            }
            allfiles.Sort((e1, e2) => e1.FileOffset.CompareTo(e2.FileOffset));

            //find the smallest available hole from the list.
            uint found = 0;
            uint foundsize = 0xFFFFFFFF;
            
            for (int i = 1; i < allfiles.Count(); i++)
            {
                RpfFileEntry e1 = allfiles[i - 1];
                RpfFileEntry e2 = allfiles[i];

                uint e1cnt = GetBlockCount(e1.GetFileSize());
                uint e1end = e1.FileOffset + e1cnt;
                uint e2beg = e2.FileOffset;
                if ((e2beg > ignorestart) && (e1end < ignoreend))
                {
                    continue; //this space is in the ignore area.
                }
                if (e1end < e2beg)
                {
                    uint space = e2beg - e1end;
                    if ((space >= reqblocks) && (space < foundsize))
                    {
                        found = e1end;
                        foundsize = space;
                    }
                }
            }

            return found;
        }
        private uint FindEndBlock()
        {
            //find the next available block after all other files (or after header if there's no files)
            uint endblock = 0;
            foreach (var entry in AllEntries)
            {
                if (entry is RpfFileEntry e)
                {
                    uint ecnt = GetBlockCount(e.GetFileSize());
                    uint eend = e.FileOffset + ecnt;
                    if (eend > endblock)
                    {
                        endblock = eend;
                    }
                }
            }

            if (endblock == 0)
            {
                //must be no files present, end block comes directly after the header.
                endblock = GetHeaderBlockCount();
            }

            return endblock;
        }
        private void GrowArchive(BinaryWriter bw, uint newblockcount)
        {
            uint newsize = newblockcount * 512;
            if (newsize < FileSize)
            {
                return;//already bigger than it needs to be, can happen if last file got moved into a hole...
            }
            if (FileSize == newsize)
            {
                return;//nothing to do... correct size already
            }

            FileSize = newsize;


            //ensure enough space in the parent if there is one...
            if (Parent != null)
            {
                if (ParentFileEntry == null)
                {
                    throw new Exception("Can't grow archive " + Path + ": ParentFileEntry was null!");
                }


                //parent's header will be updated with these new values.
                ParentFileEntry.FileUncompressedSize = newsize;
                ParentFileEntry.FileSize = 0; //archives have FileSize==0 in parent...

                Parent.EnsureSpace(bw, ParentFileEntry, newsize);
            }
        }
        private void RelocateFile(BinaryWriter bw, RpfFileEntry f, uint newblock)
        {
            //directly move this file. does NOT update the header!
            //enough space should already be allocated for this move.

            uint flen = GetBlockCount(f.GetFileSize());
            uint fbeg = f.FileOffset;
            uint fend = fbeg + flen;
            uint nend = newblock + flen;
            if ((nend > fbeg) && (newblock < fend))//can't move to somewhere within itself!
            {
                throw new Exception("Unable to relocate file " + f.Path + ": new position was inside the original!");
            }

            var stream = bw.BaseStream;
            long origpos = stream.Position;
            long source = StartPos + ((long)fbeg * 512);
            long dest = StartPos + ((long)newblock * 512);
            long newstart = dest;
            long length = (long)flen * 512;
            long destend = dest + length;
            const int BUFFER_SIZE = 16384;//what buffer size is best for HDD copy?
            var buffer = new byte[BUFFER_SIZE];
            while (length > 0)
            {
                stream.Position = source;
                int i = stream.Read(buffer, 0, (int)Math.Min(length, BUFFER_SIZE));
                stream.Position = dest;
                stream.Write(buffer, 0, i);
                source += i;
                dest += i;
                length -= i;
            }

            WritePadding(stream, destend);//makes sure the stream can grow if necessary

            stream.Position = origpos;//reset this just to be nice

            f.FileOffset = newblock;

            //if this is a child RPF archive, need to update its StartPos...
            var child = FindChildArchive(f);
            if (child != null)
            {
                child.UpdateStartPos(newstart);
            }

        }
        private void EnsureSpace(BinaryWriter bw, RpfFileEntry e, long bytecount)
        {
            //(called with null entry for ensuring header space)

            uint blockcount = GetBlockCount(bytecount);
            uint startblock = e?.FileOffset ?? 0; //0 is always header block
            uint endblock = startblock + blockcount;

            RpfFileEntry nextentry = FindFirstFileAfter(startblock);

            while (nextentry != null) //just deal with relocating one entry at a time.
            {
                //move this nextentry to somewhere else... preferably into a hole otherwise at the end
                //if the RPF needs to grow, space needs to be ensured in the parent rpf (if there is one)...
                //keep moving further entries until enough space is gained.

                if (nextentry.FileOffset >= endblock)
                {
                    break; //already enough space for this entry, don't go further.
                }

                uint entryblocks = GetBlockCount(nextentry.GetFileSize());
                uint newblock = FindHole(entryblocks, startblock, endblock);
                if (newblock == 0)
                {
                    //no hole was found, move this entry to the end of the file.
                    newblock = FindEndBlock();
                    GrowArchive(bw, newblock + entryblocks);
                }

                //now move the file contents and update the entry's position.
                RelocateFile(bw, nextentry, newblock);

                //move on to the next file...
                nextentry = FindFirstFileAfter(startblock);
            }

            if (nextentry == null)
            {
                //last entry in the RPF, so just need to grow the RPF enough to fit.
                //this could be the header (for an empty RPF)...
                uint newblock = FindEndBlock();
                GrowArchive(bw, newblock + ((e != null) ? blockcount : 0));
            }

            //changing a file's size (not the header size!) - need to update the header..!
            //also, files could have been moved. so always update the header if we aren't already
            if (e != null)
            {
                WriteHeader(bw);
            }

        }
        private void InsertFileSpace(BinaryWriter bw, RpfFileEntry entry)
        {
            //to insert a new entry. find space in the archive for it and assign the FileOffset.

            uint blockcount = GetBlockCount(entry.GetFileSize());
            entry.FileOffset = FindHole(blockcount, 0, 0);
            if (entry.FileOffset == 0)
            {
                entry.FileOffset = FindEndBlock();
                GrowArchive(bw, entry.FileOffset + blockcount);
            }
            EnsureAllEntries();
            WriteHeader(bw);
        }

        private void WriteNewArchive(BinaryWriter bw, RpfEncryption encryption)
        {
            var stream = bw.BaseStream;
            Encryption = encryption;
            Version = 0x52504637; //'RPF7'
            IsAESEncrypted = (encryption == RpfEncryption.AES);
            IsNGEncrypted = (encryption == RpfEncryption.NG);
            StartPos = stream.Position;
            EnsureAllEntries();
            WriteHeader(bw);
            FileSize = stream.Position - StartPos;
        }

        private void UpdatePaths(RpfDirectoryEntry dir = null)
        {
            //recursively update paths, including in child RPFs.
            if (dir == null)
            {
                Root.Path = Path;
                dir = Root;
            }
            foreach (var file in dir.Files)
            {
                file.Path = dir.Path + "\\" + file.Name;

                if ((file is RpfBinaryFileEntry binf) && file.IsExtension(".rpf"))
                {
                    RpfFile childrpf = FindChildArchive(binf);
                    if (childrpf != null)
                    {
                        childrpf.Path = binf.Path;
                        childrpf.FilePath = binf.Path;
                        childrpf.UpdatePaths();
                    }
                    else
                    { }//couldn't find child RPF! problem..!
                }

            }
            foreach (var subdir in dir.Directories)
            {
                subdir.Path = dir.Path + "\\" + subdir.Name;
                UpdatePaths(subdir);
            }
        }

        public RpfFile FindChildArchive(RpfFileEntry f)
        {
            RpfFile c = null;
            if (Children != null)
            {
                foreach (var child in Children)//kinda messy, but no other option really...
                {
                    if (child.ParentFileEntry == f)
                    {
                        c = child;
                        break;
                    }
                }
            }
            return c;
        }


        public long GetDefragmentedFileSize()
        {
            //this represents the size the file would be when fully defragmented.
            uint blockcount = GetHeaderBlockCount();

            foreach (var entry in AllEntries)
            {
                if (entry is RpfFileEntry fentry)
                {
                    blockcount += GetBlockCount(fentry.GetFileSize());
                }
            }

            return (long)blockcount * 512;
        }


        private void UpdateStartPos(long newpos)
        {
            StartPos = newpos;

            if (Children != null)
            {
                //make sure children also get their StartPos updated!
                foreach (var child in Children)
                {
                    if (child.ParentFileEntry == null) continue;//shouldn't really happen...
                    var cpos = StartPos + (long)child.ParentFileEntry.FileOffset * 512;
                    child.UpdateStartPos(cpos);
                }
            }
        }




        public static RpfFile CreateNew(string gtafolder, string relpath, RpfEncryption encryption = RpfEncryption.OPEN)
        {
            //create a new, empty RPF file in the filesystem
            //this will assume that the folder the file is going into already exists!

            string fpath = gtafolder;
            fpath = fpath.EndsWith("\\") ? fpath : fpath + "\\";
            fpath = fpath + relpath;

            if (File.Exists(fpath))
            {
                throw new Exception("File " + fpath + " already exists!");
            }

            File.Create(fpath).Dispose(); //just write a placeholder, will fill it out later

            RpfFile file = new RpfFile(fpath, relpath);

            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var bw = new BinaryWriter(fstream))
                {
                    file.WriteNewArchive(bw, encryption);
                }
            }

            return file;
        }

        public static RpfFile CreateNew(RpfDirectoryEntry dir, string name, RpfEncryption encryption = RpfEncryption.OPEN)
        {
            //create a new empty RPF inside the given parent RPF directory.

            string namel = name.ToLowerInvariant();
            RpfFile parent = dir.File;
            string fpath = parent.GetPhysicalFilePath();
            string rpath = dir.Path + "\\" + namel;

            if (!File.Exists(fpath))
            {
                throw new Exception("Root RPF file " + fpath + " does not exist!");
            }


            RpfFile file = new RpfFile(name, rpath, 512);//empty RPF is 512 bytes...
            file.Parent = parent;
            file.ParentFileEntry = new RpfBinaryFileEntry();

            RpfBinaryFileEntry entry = file.ParentFileEntry;
            entry.Parent = dir;
            entry.FileOffset = 0;//InsertFileSpace will update this
            entry.FileSize = 0;
            entry.FileUncompressedSize = (uint)file.FileSize;
            entry.EncryptionType = 0;
            entry.IsEncrypted = false;
            entry.File = parent;
            entry.Path = rpath;
            entry.Name = name;

            dir.Files.Add(entry);

            parent.Children.Add(file);

            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var bw = new BinaryWriter(fstream))
                {
                    parent.InsertFileSpace(bw, entry);

                    fstream.Position = parent.StartPos + entry.FileOffset * 512;

                    file.WriteNewArchive(bw, encryption);
                }
            }


            return file;
        }

        public static RpfDirectoryEntry CreateDirectory(RpfDirectoryEntry dir, string name)
        {
            //create a new directory inside the given parent dir

            RpfFile parent = dir.File;
            string fpath = parent.GetPhysicalFilePath();
            string rpath = dir.Path + "\\" + name;

            if (!File.Exists(fpath))
            {
                throw new Exception("Root RPF file " + fpath + " does not exist!");
            }

            RpfDirectoryEntry entry = new RpfDirectoryEntry();
            entry.Parent = dir;
            entry.File = parent;
            entry.Path = rpath;
            entry.Name = name;

            foreach (var exdir in dir.Directories)
            {
                if (exdir.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("RPF Directory \"" + entry.Name + "\" already exists!");
                }
            }

            dir.Directories.Add(entry);

            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var bw = new BinaryWriter(fstream))
                {
                    parent.EnsureAllEntries();
                    parent.WriteHeader(bw);
                }
            }

            return entry;
        }

        public static RpfFileEntry CreateFileEntry(string name, string path, Stream data)
        {
            //this should only really be used when loading a file from the filesystem.
            RpfFileEntry e;
            using var reader = new BinaryReader(data, Encoding.UTF8, true);

            uint rsc7 = (reader.BaseStream.Length > 4) ? reader.ReadUInt32() : 0;
            if (rsc7 == 0x37435352) //RSC7 header present! create RpfResourceFileEntry and decompress data...
            {
                e = RpfFile.CreateResourceFileEntry(data, 0);//"version" should be loadable from the header in the data..
                data = ResourceBuilder.Decompress(data);
            }
            else
            {
                var be = new RpfBinaryFileEntry
                {
                    FileSize = (uint)(data?.Length ?? 0),
                    FileUncompressedSize = (uint)(data?.Length ?? 0),
                };
                e = be;
            }
            e.Name = name;
            e.Path = path;
            return e;
        }

        public static RpfFileEntry CreateFileEntry(string name, string path, ref byte[] data)
        {
            //this should only really be used when loading a file from the filesystem.
            RpfFileEntry e = null;
            uint rsc7 = (data?.Length > 4) ? BitConverter.ToUInt32(data, 0) : 0;
            if (rsc7 == 0x37435352) //RSC7 header present! create RpfResourceFileEntry and decompress data...
            {
                e = RpfFile.CreateResourceFileEntry(ref data, 0);//"version" should be loadable from the header in the data..
                data = ResourceBuilder.Decompress(data);
            }
            else
            {
                var be = new RpfBinaryFileEntry();
                be.FileSize = (uint)data?.Length;
                be.FileUncompressedSize = be.FileSize;
                e = be;
            }
            e.Name = name;
            e.Path = path;
            return e;
        }

        public static RpfFileEntry CreateFile(RpfDirectoryEntry dir, string name, byte[] data, bool overwrite = true)
        {
            if (overwrite)
            {
                foreach (var exfile in dir.Files)
                {
                    if (exfile.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        //file already exists. delete the existing one first!
                        //this should probably be optimised to just replace the existing one...
                        //TODO: investigate along with ExploreForm.ReplaceSelected()
                        DeleteEntry(exfile);
                        break;
                    }
                }
            }
            //else fail if already exists..? items with the same name allowed?

            RpfFile parent = dir.File;
            string fpath = parent.GetPhysicalFilePath();
            string namel = name.ToLowerInvariant();
            string rpath = dir.Path + "\\" + namel;
            if (!File.Exists(fpath))
            {
                throw new Exception("Root RPF file " + fpath + " does not exist!");
            }


            RpfFileEntry entry = null;
            uint len = (uint)data.Length;


            bool isrpf = false;
            bool isawc = false;
            uint hdr = 0;
            if (len >= 16)
            {
                hdr = BitConverter.ToUInt32(data, 0);
            }

            if (hdr == 0x37435352) //'RSC7'
            {
                //RSC header is present... import as resource
                var rentry = new RpfResourceFileEntry();
                rentry.SystemFlags = BitConverter.ToUInt32(data, 8);
                rentry.GraphicsFlags = BitConverter.ToUInt32(data, 12);
                rentry.FileSize = len;
                if (len >= 0xFFFFFF)
                {
                    //just....why
                    //FileSize = (buf[7] << 0) | (buf[14] << 8) | (buf[5] << 16) | (buf[2] << 24);
                    data[7] = (byte)((len >> 0) & 0xFF);
                    data[14] = (byte)((len >> 8) & 0xFF);
                    data[5] = (byte)((len >> 16) & 0xFF);
                    data[2] = (byte)((len >> 24) & 0xFF);
                }

                entry = rentry;
            }

            if ((hdr == 0x52504637) && name.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase)) //'RPF7'
            {
                isrpf = true;
            }
            if (name.EndsWith(".awc", StringComparison.OrdinalIgnoreCase))
            {
                isawc = true;
            }

            if (entry == null)
            {
                //no RSC7 header present, import as a binary file.
                var compressed = (isrpf||isawc) ? data : CompressBytes(data);
                var bentry = new RpfBinaryFileEntry();
                bentry.EncryptionType = 0;//TODO: binary encryption
                bentry.IsEncrypted = false;
                bentry.FileUncompressedSize = (uint)data.Length;
                bentry.FileSize = (isrpf||isawc) ? 0 : (uint)compressed.Length;
                if (bentry.FileSize > 0xFFFFFF)
                {
                    bentry.FileSize = 0;
                    compressed = data; 
                    //can't compress?? since apparently FileSize>0 means compressed...
                }
                data = compressed;
                entry = bentry;
            }

            entry.Parent = dir;
            entry.File = parent;
            entry.Path = rpath;
            entry.Name = name;




            foreach (var exfile in dir.Files)
            {
                if (exfile.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("File \"" + entry.Name + "\" already exists!");
                }
            }



            dir.Files.Add(entry);


            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using var bw = new BinaryWriter(fstream);
                parent.InsertFileSpace(bw, entry);
                long bbeg = parent.StartPos + (entry.FileOffset * 512);
                long bend = bbeg + (GetBlockCount(entry.GetFileSize()) * 512);
                fstream.Position = bbeg;
                fstream.Write(data, 0, data.Length);
                WritePadding(fstream, bend); //write 0's until the end of the block.
            }


            if (isrpf)
            {
                //importing a raw RPF archive. create the new RpfFile object, and read its headers etc.
                RpfFile file = new RpfFile(name, rpath, data.LongLength);
                file.Parent = parent;
                file.ParentFileEntry = entry as RpfBinaryFileEntry;
                file.StartPos = parent.StartPos + (entry.FileOffset * 512);
                parent.Children.Add(file);

                using var fstream = File.OpenRead(fpath);
                using var br = new BinaryReader(fstream);
                fstream.Position = file.StartPos;
                file.ScanStructure(br, null, null, out _);
            }

            return entry;
        }


        public static void RenameArchive(RpfFile file, string newname)
        {
            //updates all items in the RPF with the new path - no actual file changes made here
            //(since all the paths are generated at runtime and not stored)

            file.Name = newname;
            file.Path = GetParentPath(file.Path) + newname;
            file.FilePath = GetParentPath(file.FilePath) + newname;

            file.UpdatePaths();

        }

        public static void RenameEntry(RpfEntry entry, string newname)
        {
            //rename the entry in the RPF header... 
            //also make sure any relevant child paths are updated...

            string dirpath = GetParentPath(entry.Path);

            entry.Name = newname;
            entry.Path = dirpath + newname;

            JenkIndex.EnsureLower(entry.ShortName);//could be anything... but it needs to be there

            RpfFile parent = entry.File;
            string fpath = parent.GetPhysicalFilePath();

            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var bw = new BinaryWriter(fstream))
                {
                    parent.EnsureAllEntries();
                    parent.WriteHeader(bw);
                }
            }

            if (entry is RpfDirectoryEntry)
            {
                //a folder was renamed, make sure all its children's paths get updated
                parent.UpdatePaths(entry as RpfDirectoryEntry);
            }

        }


        public static void DeleteEntry(RpfEntry entry)
        {
            //delete this entry from the RPF header.
            //also remove any references to this item in its parent directory...
            //if this is a directory entry, this will delete the contents first

            RpfFile parent = entry.File;
            string fpath = parent.GetPhysicalFilePath();
            if (!File.Exists(fpath))
            {
                throw new Exception("Root RPF file " + fpath + " does not exist!");
            }

            RpfDirectoryEntry entryasdir = entry as RpfDirectoryEntry;
            //it has to be one or the other...

            if (entryasdir != null)
            {
                var deldirs = entryasdir.Directories.ToArray();
                var delfiles = entryasdir.Files.ToArray();
                foreach (var deldir in deldirs)
                {
                    DeleteEntry(deldir);
                }
                foreach (var delfile in delfiles)
                {
                    DeleteEntry(delfile);
                }
            }

            if (entry.Parent == null)
            {
                throw new Exception("Parent directory is null! This shouldn't happen - please refresh the folder!");
            }

            if (entryasdir != null)
            {
                entry.Parent.Directories.Remove(entryasdir);
            }
            if (entry is RpfFileEntry entryasfile)
            {
                entry.Parent.Files.Remove(entryasfile);

                var child = parent.FindChildArchive(entryasfile);
                if (child != null)
                {
                    parent.Children.Remove(child); //RPF file being deleted...
                }
            }

            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var bw = new BinaryWriter(fstream))
                {
                    parent.EnsureAllEntries();
                    parent.WriteHeader(bw);
                }
            }

        }


        public static bool EnsureValidEncryption(RpfFile file, Func<RpfFile, bool> confirm)
        {
            if (file == null) return false;

            //currently assumes OPEN is the valid encryption type.
            //TODO: support other encryption types!

            bool needsupd = false;
            var f = file;
            List<RpfFile> files = new List<RpfFile>();
            while (f != null)
            {
                if (f.Encryption != RpfEncryption.OPEN)
                {
                    if (!confirm(f))
                    {
                        return false;
                    }
                    needsupd = true;
                }
                if (needsupd)
                {
                    files.Add(f);
                }
                f = f.Parent;
            }

            //change encryption types, starting from the root rpf.
            files.Reverse();
            foreach (var cfile in files)
            {
                SetEncryptionType(cfile, RpfEncryption.OPEN);
            }

            return true;
        }

        public static void SetEncryptionType(RpfFile file, RpfEncryption encryption)
        {
            file.Encryption = encryption;
            string fpath = file.GetPhysicalFilePath();
            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var bw = new BinaryWriter(fstream))
                {
                    file.WriteHeader(bw);
                }
            }
        }


        public static void Defragment(RpfFile file, Action<string, float> progress = null)
        {
            if (file?.AllEntries == null) return;

            string fpath = file.GetPhysicalFilePath();
            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var bw = new BinaryWriter(fstream))
                {
                    uint destblock = file.GetHeaderBlockCount();

                    const int BUFFER_SIZE = 16384;//what buffer size is best for HDD copy?
                    var buffer = new byte[BUFFER_SIZE];

                    var allfiles = new List<RpfFileEntry>();
                    for (int i = 0; i < file.AllEntries.Count; i++)
                    {
                        if (file.AllEntries[i] is RpfFileEntry entry) allfiles.Add(entry);
                    }
                    //make sure we process everything in the current order that they are in the archive
                    allfiles.Sort((a, b) => { return a.FileOffset.CompareTo(b.FileOffset); });

                    for (int i = 0; i < allfiles.Count; i++)
                    {
                        var entry = allfiles[i];
                        float prog = (float)i / allfiles.Count;
                        string txt = "Relocating " + entry.Name + "...";
                        progress?.Invoke(txt, prog);

                        var sourceblock = entry.FileOffset;
                        var blockcount = GetBlockCount(entry.GetFileSize());

                        if (sourceblock > destblock) //should only be moving things toward the start
                        {
                            var source = file.StartPos + (long)sourceblock * 512;
                            var dest = file.StartPos + (long)destblock * 512;
                            var remlength = (long)blockcount * 512;
                            while (remlength > 0)
                            {
                                fstream.Position = source;
                                int n = fstream.Read(buffer, 0, (int)Math.Min(remlength, BUFFER_SIZE));
                                fstream.Position = dest;
                                fstream.Write(buffer, 0, n);
                                source += n;
                                dest += n;
                                remlength -= n;
                            }
                            entry.FileOffset = destblock;

                            var entryrpf = file.FindChildArchive(entry);
                            if (entryrpf != null)
                            {
                                entryrpf.UpdateStartPos(file.StartPos + (long)entry.FileOffset * 512);
                            }
                        }
                        else if (sourceblock != destblock)
                        { }//shouldn't get here...

                        destblock += blockcount;
                    }

                    file.FileSize = (long)destblock * 512;

                    file.WriteHeader(bw);

                    if (file.ParentFileEntry != null)
                    {
                        //make sure to also update the parent archive file entry, if there is one
                        file.ParentFileEntry.FileUncompressedSize = (uint)file.FileSize;
                        file.ParentFileEntry.FileSize = 0;
                        if (file.Parent != null)
                        {
                            file.Parent.WriteHeader(bw);
                        }
                    }
                    if (file.Parent == null)
                    {
                        //this is a root archive, so update the file's length to the new size.
                        fstream.SetLength(file.FileSize);
                    }
                }
            }
        }



        private static string GetParentPath(string path)
        {
            string dirpath = path.Replace('/', '\\');//just to make sure..
            int lidx = dirpath.LastIndexOf('\\');
            if (lidx > 0)
            {
                dirpath = dirpath.Substring(0, lidx + 1);
            }
            if (!dirpath.EndsWith("\\"))
            {
                dirpath += "\\";
            }
            return dirpath;
        }


        public override string ToString()
        {
            return Path;
        }
    }


    public enum RpfEncryption : uint
    {
        NONE = 0, //some modded RPF's may use this
        OPEN = 0x4E45504F, //1313165391 "OPEN", ie. "no encryption"
        AES =  0x0FFFFFF9, //268435449
        NG =   0x0FEFFFFF, //267386879
        CFXP = 0x50584643,
    }

    public enum FileHeader : uint
    {
        RSC7 = 0x37435352,
        FXAP = 0x50415846,
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public abstract class RpfEntry
    {
        public RpfFile File { get; set; }
        public RpfDirectoryEntry Parent { get; set; }

        public uint NameHash {
            get
            {
                if (nameHash == 0 && !string.IsNullOrEmpty(Name))
                {
                    nameHash = JenkHash.GenHashLower(Name);
                }
                return nameHash;
            }
            set
            {
                nameHash = value;
            }
        }
        public uint ShortNameHash {
            get
            {
                if (shortNameHash == 0 && !ShortName.IsEmpty)
                {
                    shortNameHash = JenkHash.GenHashLower(ShortName);
                }
                return shortNameHash;
            }
            set
            {
                shortNameHash = value;
            }
        }

        public virtual uint NameOffset
        {
            get => (uint)(Header & 0xFFFF);
            set => Header = (Header & ~0xFFFFUL) | (value & 0xFFFF);
        }

        public string Name {
            get => name;
            set
            {
                if (name == value)
                {
                    return;
                }
                name = value;
                nameHash = 0;
                shortNameHash = 0;
                shortNameIndex = -1;
            }
        }

        public ReadOnlySpan<char> ShortName {
            get
            {
                if (shortNameIndex == -1 && !string.IsNullOrEmpty(Name))
                {
                    int length = Name.Length;
                    for (int i = length; --i >= 0;)
                    {
                        char ch = Name[i];
                        if (ch == '.')
                        {
                            shortNameIndex = i;
                            //shortName = Name.Substring(0, i);
                            break;
                        }
                        if (ch == System.IO.Path.DirectorySeparatorChar || ch == System.IO.Path.AltDirectorySeparatorChar)
                        {
                            shortNameIndex = Name.Length;
                            break;
                        }
                    }
                }

                if (shortNameIndex == -1 || shortNameIndex == Name.Length)
                {
                    return Name;
                }

                return Name.AsSpan(0, shortNameIndex);
            }
        }

        public string Path { get; set; }

        //public uint H1; //first 2 header values from RPF table...
        //public uint H2;
        public ulong Header;
        private string name;
        private uint shortNameHash;
        private uint nameHash;

        private int shortNameIndex = -1;

        public abstract void Read(DataReader reader, BinaryReader br);

        public abstract void Read(ref SequenceReader<byte> reader, BinaryReader br);

        public abstract void Write(DataWriter writer);

        public override string ToString()
        {
            return Path;
        }

        public bool IsExtension(string ext)
        {
            return Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RpfDirectoryEntry : RpfEntry
    {
        public uint EntriesIndex { get; set; }
        public uint EntriesCount { get; set; }

        public override uint NameOffset => (uint)(Header & 0xFFFFFFFF);

        public uint Ident
        {
            get => (uint)((Header >> 32) & 0xFFFFFFFF);
            set => Header = (Header & 0xFFFFFFFF) | (value << 32);
        }

        public PooledList<RpfDirectoryEntry> Directories = new PooledList<RpfDirectoryEntry>();
        public PooledList<RpfFileEntry> Files = new PooledList<RpfFileEntry>();


        public override void Read(DataReader reader, BinaryReader _)
        {
            if (Ident != 0x7FFFFF00u)
            {
                throw new Exception("Error in RPF7 directory entry.");
            }
            EntriesIndex = reader.ReadUInt32();
            EntriesCount = reader.ReadUInt32();
        }

        public override void Read(ref SequenceReader<byte> reader, BinaryReader br)
        {
            if (Ident != 0x7FFFFF00u)
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

        public void Dispose()
        {
            Directories.Dispose();
            Files.Dispose();
            GC.SuppressFinalize(this);
        }

        //[MemberNotNull(nameof(files), nameof(directories))]
        //public void InitFilesAndDirectories()
        //{
        //    int starti = (int)EntriesIndex;
        //    int endi = (int)(EntriesIndex + EntriesCount);

        //    var AllEntries = File.AllEntries;

        //    files = new List<RpfFileEntry>();
        //    directories = new List<RpfDirectoryEntry>();

        //    for (int i = starti; i < endi; i++)
        //    {
        //        RpfEntry e = AllEntries[i];
        //        e.Parent = this;
        //        if (e is RpfDirectoryEntry rde)
        //        {
        //            directories.Add(rde);
        //        }
        //        else if (e is RpfFileEntry rfe)
        //        {
        //            files.Add(rfe);
        //        }
        //    }
        //}

        public override string ToString()
        {
            return $"Directory: {Path}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class RpfFileEntry : RpfEntry
    {
        public uint FileOffset {
            get => (uint) ((Header >> 40) & 0x7FFFFFU);
            set => Header = (Header & ~(0x7FFFFFUL << 40)) | (((value & 0x7FFFFFUL) | 0x800000UL) << 40);
        }
        public abstract uint FileSize { get; set; }
        public bool IsEncrypted { get; set; }

        public abstract long GetUncompressedFileSize();
        public abstract long GetFileSize();
        public abstract void SetFileSize(uint s);
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RpfBinaryFileEntry : RpfFileEntry
    {
        public uint FileUncompressedSize { get; set; }
        public uint EncryptionType { get; set; }

        public override uint FileSize
        {
            get => (uint)((Header >> 16) & 0xFFFFFF);
            set => Header = (Header & ~0xFFFFFF0000UL) | ((value & 0xFFFFFFUL) << 16);
        }

        public override void Read(DataReader reader, BinaryReader _)
        {
            FileUncompressedSize = reader.ReadUInt32();

            EncryptionType = reader.ReadUInt32();

            switch (EncryptionType)
            {
                case 0: IsEncrypted = false; break;
                case 1: IsEncrypted = true; break;
                default:
                    throw new Exception($"Error in RPF7 file entry. {EncryptionType}");
            }

        }

        public override void Read(ref SequenceReader<byte> reader, BinaryReader _)
        {
            FileUncompressedSize = reader.ReadUInt32();

            EncryptionType = reader.ReadUInt32();

            switch (EncryptionType)
            {
                case 0: IsEncrypted = false; break;
                case 1: IsEncrypted = true; break;
                default:
                    throw new Exception($"Error in RPF7 file entry. {EncryptionType}");
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
            return $"Binary file: {Path}";
        }

        public override long GetUncompressedFileSize()
        {
            return FileUncompressedSize;
        }

        public override long GetFileSize()
        {
            return (FileSize == 0) ? FileUncompressedSize : FileSize;
        }
        public override void SetFileSize(uint s)
        {
            //FileUncompressedSize = s;
            FileSize = s;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class RpfResourceFileEntry : RpfFileEntry
    {
        public RpfResourcePageFlags SystemFlags { get; set; }
        public RpfResourcePageFlags GraphicsFlags { get; set; }


        public static int GetSizeFromFlags(uint flags)
        {
            //dexfx simplified version
            var s0 = ((flags >> 27) & 0x1) << 0;   // 1 bit  - 27        (*1)
            var s1 = ((flags >> 26) & 0x1) << 1;   // 1 bit  - 26        (*2)
            var s2 = ((flags >> 25) & 0x1) << 2;   // 1 bit  - 25        (*4)
            var s3 = ((flags >> 24) & 0x1) << 3;   // 1 bit  - 24        (*8)
            var s4 = ((flags >> 17) & 0x7F) << 4;   // 7 bits - 17 - 23   (*16)   (max 127 * 16)
            var s5 = ((flags >> 11) & 0x3F) << 5;   // 6 bits - 11 - 16   (*32)   (max 63  * 32)
            var s6 = ((flags >> 7) & 0xF) << 6;   // 4 bits - 7  - 10   (*64)   (max 15  * 64)
            var s7 = ((flags >> 5) & 0x3) << 7;   // 2 bits - 5  - 6    (*128)  (max 3   * 128)
            var s8 = ((flags >> 4) & 0x1) << 8;   // 1 bit  - 4         (*256)
            var ss = ((flags >> 0) & 0xF);         // 4 bits - 0  - 3
            var baseSize = 0x200 << (int)ss;
            var size = baseSize * (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8);
            return (int)size;


            #region dexyfex testing
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




            #endregion



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
                if (remainder != 0)
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


        public int Version => GetVersionFromFlags(SystemFlags, GraphicsFlags);

        public uint FileSizeHeader {
                get => (uint) ((Header >> 16) & 0xFFFFFF);
                set => Header = (Header & ~0xFFFFFF0000UL) | ((value & 0xFFFFFFUL) << 16);
        }

        public uint fileSize;
        public override uint FileSize
        {
            get => fileSize;
            set {

                if (value > 0xFFFFFF)
                {
                    FileSizeHeader = 0xFFFFFF;
                } else
                {
                    FileSizeHeader = value;
                }
                fileSize = value;
            }
        }

        public int SystemSize => (int)SystemFlags.Size;
        public int GraphicsSize => (int)GraphicsFlags.Size;

        [SkipLocalsInit]
        public override void Read(DataReader reader, BinaryReader cfr)
        {
            fileSize = FileSizeHeader;

            SystemFlags = reader.ReadUInt32();
            GraphicsFlags = reader.ReadUInt32();

            // there are sometimes resources with length=0xffffff which actually
            // means length>=0xffffff
            if (fileSize == 0xFFFFFF)
            {
                long opos = cfr.BaseStream.Position;
                cfr.BaseStream.Position = File.StartPos + ((long)FileOffset * 512); //need to use the base offset!!
                Span<byte> buf = stackalloc byte[16];
                cfr.BaseStream.ReadAtLeast(buf, 16);
                fileSize = ((uint)buf[7] << 0) | ((uint)buf[14] << 8) | ((uint)buf[5] << 16) | ((uint)buf[2] << 24);
                cfr.BaseStream.Position = opos;
            }
        }

        [SkipLocalsInit]
        public override void Read(ref SequenceReader<byte> reader, BinaryReader cfr)
        {
            fileSize = FileSizeHeader;
            SystemFlags = reader.ReadUInt32();
            GraphicsFlags = reader.ReadUInt32();

            // there are sometimes resources with length=0xffffff which actually
            // means length>=0xffffff
            if (fileSize == 0xFFFFFF)
            {
                long opos = cfr.BaseStream.Position;
                cfr.BaseStream.Position = File.StartPos + ((long)FileOffset * 512); //need to use the base offset!!
                Span<byte> buf = stackalloc byte[16];
                cfr.BaseStream.ReadAtLeast(buf, 16);
                fileSize = ((uint)buf[7] << 0) | ((uint)buf[14] << 8) | ((uint)buf[5] << 16) | ((uint)buf[2] << 24);
                cfr.BaseStream.Position = opos;
            }
        }

        public override void Write(DataWriter writer)
        {
            writer.Write((ushort)NameOffset);

            var fs = FileSize;
            if (fs > 0xFFFFFF)
                fs = 0xFFFFFF;//will also need to make sure the RSC header is updated...

            var buf1 = new byte[] {
                (byte)((fs >> 0) & 0xFF),
                (byte)((fs >> 8) & 0xFF),
                (byte)((fs >> 16) & 0xFF)
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
            return $"Resource file: {Path}";
        }

        public override long GetFileSize()
        {
            return (FileSize == 0) ? (long)(SystemSize + GraphicsSize) : FileSize;
        }

        public override long GetUncompressedFileSize()
        {
            return (long)(SystemSize + GraphicsSize);
        }

        public override void SetFileSize(uint s)
        {
            FileSize = s;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct RpfResourcePageFlags
    {
        public uint Value { get; set; }
        
        public readonly RpfResourcePage[] Pages
        {
            get
            {
                var count = Count;
                if (count == 0)
                    return null;
                var pages = new RpfResourcePage[count];
                var counts = PageCounts;
                var sizes = BaseSizes;
                int n = 0;
                uint o = 0;
                for (int i = 0; i < counts.Length; i++)
                {
                    var c = counts[i];
                    var s = sizes[i];
                    for (int p = 0; p < c; p++)
                    {
                        pages[n] = new RpfResourcePage(s, o);
                        o += s;
                        n++;
                    }
                }
                return pages;
            }
        }

        public readonly uint TypeVal => (Value >> 28) & 0xF;
        public readonly uint BaseShift => (Value & 0xF);
        public readonly uint BaseSize => (0x200u << (int)BaseShift);
        public readonly uint[] BaseSizes
        {
            get
            {
                var baseSize = BaseSize;
                return new uint[]
                {
                    baseSize << 8,
                    baseSize << 7,
                    baseSize << 6,
                    baseSize << 5,
                    baseSize << 4,
                    baseSize << 3,
                    baseSize << 2,
                    baseSize << 1,
                    baseSize << 0,
                };
            }
        }
        public readonly uint[] PageCounts
        {
            get
            {
                return new uint[]
                {
                    ((Value >> 4)  & 0x1),
                    ((Value >> 5)  & 0x3),
                    ((Value >> 7)  & 0xF),
                    ((Value >> 11) & 0x3F),
                    ((Value >> 17) & 0x7F),
                    ((Value >> 24) & 0x1),
                    ((Value >> 25) & 0x1),
                    ((Value >> 26) & 0x1),
                    ((Value >> 27) & 0x1),
                };
            }
        }
        public readonly uint[] PageSizes
        {
            get
            {
                var counts = PageCounts;
                var baseSizes = BaseSizes;
                return new uint[]
                {
                    baseSizes[0] * counts[0],
                    baseSizes[1] * counts[1],
                    baseSizes[2] * counts[2],
                    baseSizes[3] * counts[3],
                    baseSizes[4] * counts[4],
                    baseSizes[5] * counts[5],
                    baseSizes[6] * counts[6],
                    baseSizes[7] * counts[7],
                    baseSizes[8] * counts[8],
                };
            }
        }
        public readonly uint Count
        {
            get
            {
                return Vector256.Sum(Vector256.Create<uint>(PageCounts.AsSpan())) + PageCounts[8];
                //var c = PageCounts;
                //return c[0] + c[1] + c[2] + c[3] + c[4] + c[5] + c[6] + c[7] + c[8];
            }
        }
        public readonly uint Size 
        { 
            get 
            {
                var flags = Value;
                var s0 = ((flags >> 27) & 0x1)  << 0;
                var s1 = ((flags >> 26) & 0x1)  << 1;
                var s2 = ((flags >> 25) & 0x1)  << 2;
                var s3 = ((flags >> 24) & 0x1)  << 3;
                var s4 = ((flags >> 17) & 0x7F) << 4;
                var s5 = ((flags >> 11) & 0x3F) << 5;
                var s6 = ((flags >> 7)  & 0xF)  << 6;
                var s7 = ((flags >> 5)  & 0x3)  << 7;
                var s8 = ((flags >> 4)  & 0x1)  << 8;
                var ss = ((flags >> 0)  & 0xF);
                var baseSize = 0x200u << (int)ss;
                return baseSize * (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8);
            }
        }



        public RpfResourcePageFlags(uint v)
        {
            Value = v;
        }

        public RpfResourcePageFlags(uint[] pageCounts, uint baseShift)
        {
            var v = baseShift & 0xF;
            v += (pageCounts[0] & 0x1)  << 4;
            v += (pageCounts[1] & 0x3)  << 5;
            v += (pageCounts[2] & 0xF)  << 7;
            v += (pageCounts[3] & 0x3F) << 11;
            v += (pageCounts[4] & 0x7F) << 17;
            v += (pageCounts[5] & 0x1)  << 24;
            v += (pageCounts[6] & 0x1)  << 25;
            v += (pageCounts[7] & 0x1)  << 26;
            v += (pageCounts[8] & 0x1)  << 27;
            Value = v;
        }


        public static implicit operator uint(RpfResourcePageFlags f)
        {
            return f.Value;  //implicit conversion
        }
        public static implicit operator RpfResourcePageFlags(uint v)
        {
            return new RpfResourcePageFlags(v);
        }

        public override readonly string ToString()
        {
            return $"Size: {Size}, Pages: {Count}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public readonly struct RpfResourcePage(uint size, uint offset)
    {
        public uint Size { get; init; } = size;
        public uint Offset { get; init; } = offset;

        public override string ToString()
        {
            return $"{Size}: {Offset}";
        }
    }

    public interface PackedFile //interface for the different file types to use
    {
        void Load(byte[] data, RpfFileEntry entry);
    }

    public interface PackedFileStream : PackedFile
    {
        void Load(Stream stream, RpfFileEntry entry);
    }









}