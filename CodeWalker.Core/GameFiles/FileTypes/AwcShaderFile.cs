using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

// AWC Shader Library (SGD2 / Shader Group Data v2) reader & writer.
// Used by GTA V Enhanced (Gen9) compiled-shader containers. Distinct from the
// audio Audio Wave Container (AwcFile.cs / magic ADAT) which shares the .awc
// extension.

namespace CodeWalker.GameFiles
{
    public enum AwcShaderValueType : ushort
    {
        Bool     = 0,
        Uint     = 1,
        Uint2    = 2,
        Uint3    = 3,
        Uint4    = 4,
        Int      = 5,
        Int2     = 6,
        Int3     = 7,
        Int4     = 8,
        Float    = 9,
        Float2   = 10,
        Float3   = 11,
        Float4   = 12,
        Float4x3 = 13,
        Float4x4 = 14,
    }

    public enum AwcShaderResourceType : ushort
    {
        Texture2D                = 0x0102,
        Texture2DArray           = 0x0142,
        TextureCube              = 0x0202,
        Texture3D                = 0x0302,
        Buffer                   = 0x0401,
        StructuredBuffer         = 0x0405,
        ByteAddressBuffer        = 0x0407,
        RWTexture2D              = 0x011C,
        RWTexture2DArray         = 0x015C,
        RWStructuredBufferAppend = 0x040E,
        RWStructuredBuffer       = 0x0414,
        RWStructuredBufferConsume= 0x0416,
        RWByteAddressBuffer      = 0x0418,
        SamplerState             = 0x0423,
        ConstantBuffer           = 0x0430,
    }

    public enum AwcShaderStage
    {
        Vertex,
        Pixel,
        Geometry,
        Domain,
        Hull,
        Compute,
    }

    [TC(typeof(EXP))]
    public class AwcShaderCBufferData
    {
        public AwcShaderValueType Type { get; set; }
        public ushort ArraySize { get; set; }
        public ushort PackOffset { get; set; }
        public uint NameOffset { get; set; }
        public string Name { get; set; }
        [Browsable(false)] public byte[] NameHashData { get; set; } = new byte[14];

        public string TypeName => Type.ToString();

        public override string ToString() => Name + " : " + Type + (ArraySize > 1 ? "[" + ArraySize + "]" : string.Empty);
    }

    [TC(typeof(EXP))]
    public class AwcShaderRegister
    {
        public AwcShaderResourceType ResourceType { get; set; }
        public ushort RegisterSlot { get; set; }
        public byte CBufferCount { get; set; }
        public byte NumDescriptors { get; set; }
        public byte RegisterSpace { get; set; }
        public byte Reserved { get; set; }
        public ushort CBufferDataOffset { get; set; }
        public ushort RegStringOffset { get; set; }
        public string Name { get; set; }
        [Browsable(false)] public byte[] ExtraData { get; set; } = new byte[16];
        public AwcShaderCBufferData[] CBuffers { get; set; }

        public string RegisterPrefix
        {
            get
            {
                switch (ResourceType)
                {
                    case AwcShaderResourceType.ConstantBuffer: return "b";
                    case AwcShaderResourceType.SamplerState:   return "s";
                    case AwcShaderResourceType.Texture2D:
                    case AwcShaderResourceType.Texture2DArray:
                    case AwcShaderResourceType.TextureCube:
                    case AwcShaderResourceType.Texture3D:
                    case AwcShaderResourceType.Buffer:
                    case AwcShaderResourceType.StructuredBuffer:
                    case AwcShaderResourceType.ByteAddressBuffer:
                        return "t";
                    default: return "u";
                }
            }
        }

        public string Slot => RegisterPrefix + RegisterSlot + (RegisterSpace != 0 ? ",space" + RegisterSpace : string.Empty);

        public override string ToString() => Slot + " " + Name + " (" + ResourceType + ")";
    }

    [TC(typeof(EXP))]
    public class AwcShader
    {
        public string Name { get; set; }
        public byte WaveSize { get; set; }
        public uint Size { get; set; }
        [Browsable(false)] public byte[] Binary { get; set; }
        public ulong Hash { get; set; }
        [Browsable(false)] public byte[] RootSigData { get; set; }
        public uint BlockSize { get; set; }
        public ushort RegCount { get; set; }
        public ushort CBufferCount { get; set; }
        public ushort TexCount { get; set; }
        public ushort BlockSizeCopy { get; set; }
        public AwcShaderRegister[] Registers { get; set; }
        public AwcShaderStage Stage { get; set; }

        // Original-on-disk fragments preserved so unchanged shaders round-trip
        // byte-for-byte. Stale once the user mutates Binary / Size / Registers.
        [Browsable(false)] public byte NameLengthByte { get; set; }
        [Browsable(false)] public byte[] NameBytes { get; set; }
        [Browsable(false)] public byte[] OriginalBlockData { get; set; }
        [Browsable(false)] public bool BinaryDirty { get; set; }
        [Browsable(false)] public bool MetadataDirty { get; set; }

        public string StageName
        {
            get
            {
                switch (Stage)
                {
                    case AwcShaderStage.Vertex:   return "VS";
                    case AwcShaderStage.Pixel:    return "PS";
                    case AwcShaderStage.Geometry: return "GS";
                    case AwcShaderStage.Domain:   return "DS";
                    case AwcShaderStage.Hull:     return "HS";
                    case AwcShaderStage.Compute:  return "CS";
                    default: return "?";
                }
            }
        }

        public string HashHex => "0x" + Hash.ToString("X16");

        public override string ToString() => StageName + " " + Name;
    }

    [TC(typeof(EXP))]
    public class AwcShaderFile : PackedFile
    {
        public const uint MagicSGD2 = 0x32444753; // "SGD2"

        public string Name { get; set; }
        public RpfFileEntry FileEntry { get; set; }
        public string Magic { get; set; }
        public AwcShader[] VertexShaders { get; set; }
        public AwcShader[] PixelShaders { get; set; }
        public AwcShader[] GeometryShaders { get; set; }
        public AwcShader[] DomainShaders { get; set; }
        public AwcShader[] HullShaders { get; set; }
        public AwcShader[] ComputeShaders { get; set; }

        [Browsable(false)] public byte[] FooterData { get; set; }

        public int VertexCount   => VertexShaders?.Length ?? 0;
        public int PixelCount    => PixelShaders?.Length ?? 0;
        public int GeometryCount => GeometryShaders?.Length ?? 0;
        public int DomainCount   => DomainShaders?.Length ?? 0;
        public int HullCount     => HullShaders?.Length ?? 0;
        public int ComputeCount  => ComputeShaders?.Length ?? 0;
        public int TotalShaderCount => VertexCount + PixelCount + GeometryCount + DomainCount + HullCount + ComputeCount;

        public IEnumerable<AwcShader> AllShaders()
        {
            if (VertexShaders   != null) foreach (var s in VertexShaders)   yield return s;
            if (PixelShaders    != null) foreach (var s in PixelShaders)    yield return s;
            if (GeometryShaders != null) foreach (var s in GeometryShaders) yield return s;
            if (DomainShaders   != null) foreach (var s in DomainShaders)   yield return s;
            if (HullShaders     != null) foreach (var s in HullShaders)     yield return s;
            if (ComputeShaders  != null) foreach (var s in ComputeShaders)  yield return s;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            FileEntry = entry;
            Name = entry?.Name;

            if (data == null || data.Length < 4)
                throw new InvalidDataException("AWC Shader Library: empty data.");

            if (BitConverter.ToUInt32(data, 0) != MagicSGD2)
                throw new InvalidDataException("AWC Shader Library: not an SGD2 file.");

            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms, Encoding.GetEncoding("ISO-8859-1")))
            {
                Magic = Encoding.ASCII.GetString(br.ReadBytes(4));

                VertexShaders   = ReadShaderArray(br, AwcShaderStage.Vertex);
                PixelShaders    = ReadShaderArray(br, AwcShaderStage.Pixel);
                GeometryShaders = ReadShaderArray(br, AwcShaderStage.Geometry);
                DomainShaders   = ReadShaderArray(br, AwcShaderStage.Domain);
                HullShaders     = ReadShaderArray(br, AwcShaderStage.Hull);
                ComputeShaders  = ReadShaderArray(br, AwcShaderStage.Compute);

                long remaining = ms.Length - ms.Position;
                FooterData = remaining > 0 ? br.ReadBytes((int)remaining) : Array.Empty<byte>();
            }
        }

        private static AwcShader[] ReadShaderArray(BinaryReader br, AwcShaderStage stage)
        {
            uint count = br.ReadUInt32();
            var arr = new AwcShader[count];
            for (uint i = 0; i < count; i++)
                arr[i] = ReadShader(br, stage);
            return arr;
        }

        private static AwcShader ReadShader(BinaryReader br, AwcShaderStage stage)
        {
            byte slen = br.ReadByte();
            byte[] nameBytes = br.ReadBytes(slen);
            string name = Encoding.GetEncoding("ISO-8859-1").GetString(nameBytes).TrimEnd('\0');

            byte wave = br.ReadByte();
            uint size = br.ReadUInt32();
            byte[] binary = br.ReadBytes((int)size);
            ulong hash = br.ReadUInt64();
            byte[] rootSig = br.ReadBytes(144);
            uint blockSize = br.ReadUInt32();

            long blockStart = br.BaseStream.Position;
            byte[] blockData = br.ReadBytes((int)blockSize);

            // Re-parse the metadata block from blockData so the whole shader
            // stays self-contained (no further seeks into the outer stream).
            var (regCount, cbCount, texCount, blockSizeCopy, registers) = ParseBlock(blockData);

            return new AwcShader
            {
                Name = name,
                NameLengthByte = slen,
                NameBytes = nameBytes,
                WaveSize = wave,
                Size = size,
                Binary = binary,
                Hash = hash,
                RootSigData = rootSig,
                BlockSize = blockSize,
                BlockSizeCopy = blockSizeCopy,
                RegCount = regCount,
                CBufferCount = cbCount,
                TexCount = texCount,
                Registers = registers,
                OriginalBlockData = blockData,
                Stage = stage,
            };
        }

        private static (ushort reg, ushort cb, ushort tex, ushort blkSizeCopy, AwcShaderRegister[] regs) ParseBlock(byte[] block)
        {
            using (var ms = new MemoryStream(block))
            using (var br = new BinaryReader(ms))
            {
                ushort regCount = br.ReadUInt16();
                ushort cbCount  = br.ReadUInt16();
                ushort texCount = br.ReadUInt16();
                ushort blkCopy  = br.ReadUInt16();

                var regs = new AwcShaderRegister[regCount];
                for (int i = 0; i < regCount; i++)
                    regs[i] = ParseRegister(ms, br);

                return (regCount, cbCount, texCount, blkCopy, regs);
            }
        }

        private static AwcShaderRegister ParseRegister(MemoryStream ms, BinaryReader br)
        {
            long headerStart = ms.Position;

            ushort resType        = br.ReadUInt16();
            ushort regSlot        = br.ReadUInt16();
            byte   cbCount        = br.ReadByte();
            byte   numDesc        = br.ReadByte();
            byte   regSpace       = br.ReadByte();
            byte   reserved       = br.ReadByte();
            ushort cbDataOffset   = br.ReadUInt16();
            ushort regStringOff   = br.ReadUInt16();

            long afterHeader = ms.Position; // headerStart + 12
            byte[] extra = br.ReadBytes(16);

            // Offsets in the binary are relative to headerStart.
            string regName;
            long savedPos = ms.Position;
            ms.Position = headerStart + regStringOff;
            regName = ReadCString(br);
            ms.Position = savedPos;

            int validCb = cbDataOffset != 0 ? cbCount : 0;
            AwcShaderCBufferData[] cbs;
            if (validCb > 0)
            {
                cbs = new AwcShaderCBufferData[validCb];
                ms.Position = headerStart + cbDataOffset;
                for (int i = 0; i < validCb; i++)
                    cbs[i] = ParseCBufferData(ms, br);
            }
            else
            {
                cbs = Array.Empty<AwcShaderCBufferData>();
            }

            // Move past the 16-byte extra data area, ready for the next register.
            ms.Position = afterHeader + 16;

            return new AwcShaderRegister
            {
                ResourceType = (AwcShaderResourceType)resType,
                RegisterSlot = regSlot,
                CBufferCount = cbCount,
                NumDescriptors = numDesc,
                RegisterSpace = regSpace,
                Reserved = reserved,
                CBufferDataOffset = cbDataOffset,
                RegStringOffset = regStringOff,
                ExtraData = extra,
                Name = regName,
                CBuffers = cbs,
            };
        }

        private static AwcShaderCBufferData ParseCBufferData(MemoryStream ms, BinaryReader br)
        {
            long start = ms.Position;
            ushort type        = br.ReadUInt16();
            ushort arraySize   = br.ReadUInt16();
            ushort packOffset  = br.ReadUInt16();
            uint   nameOffset  = br.ReadUInt32();

            string cbName;
            long savedPos = ms.Position;
            ms.Position = start + nameOffset;
            cbName = ReadCString(br);
            ms.Position = savedPos;

            byte[] nameHashData = br.ReadBytes(14);

            return new AwcShaderCBufferData
            {
                Type = (AwcShaderValueType)type,
                ArraySize = arraySize,
                PackOffset = packOffset,
                NameOffset = nameOffset,
                Name = cbName,
                NameHashData = nameHashData,
            };
        }

        private static string ReadCString(BinaryReader br)
        {
            var sb = new StringBuilder();
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                byte b = br.ReadByte();
                if (b == 0) break;
                sb.Append((char)b);
            }
            return sb.ToString();
        }

        // ---------- Save ----------

        public byte[] Save()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(MagicSGD2);

                WriteShaderArray(bw, VertexShaders);
                WriteShaderArray(bw, PixelShaders);
                WriteShaderArray(bw, GeometryShaders);
                WriteShaderArray(bw, DomainShaders);
                WriteShaderArray(bw, HullShaders);
                WriteShaderArray(bw, ComputeShaders);

                if (FooterData != null && FooterData.Length > 0)
                    bw.Write(FooterData);

                return ms.ToArray();
            }
        }

        private static void WriteShaderArray(BinaryWriter bw, AwcShader[] arr)
        {
            uint count = (uint)(arr?.Length ?? 0);
            bw.Write(count);
            if (arr == null) return;
            for (int i = 0; i < arr.Length; i++)
                WriteShader(bw, arr[i]);
        }

        private static void WriteShader(BinaryWriter bw, AwcShader s)
        {
            // Preserve original name bytes/length (avoids Latin-1 round-trip risk).
            if (s.NameBytes != null)
            {
                bw.Write(s.NameLengthByte);
                bw.Write(s.NameBytes);
            }
            else
            {
                byte[] nb = Encoding.GetEncoding("ISO-8859-1").GetBytes(s.Name ?? string.Empty);
                byte[] nbz = new byte[nb.Length + 1];
                Array.Copy(nb, nbz, nb.Length);
                bw.Write((byte)nbz.Length);
                bw.Write(nbz);
            }

            bw.Write(s.WaveSize);
            uint binSize = (uint)(s.Binary?.Length ?? 0);
            bw.Write(binSize);
            if (binSize > 0) bw.Write(s.Binary);

            bw.Write(s.Hash);
            bw.Write(s.RootSigData ?? new byte[144]);

            byte[] block;
            if (s.MetadataDirty)
            {
                block = BuildMetadataBlock(s);
            }
            else if (s.OriginalBlockData != null)
            {
                block = s.OriginalBlockData;
            }
            else
            {
                block = BuildMetadataBlock(s);
            }

            bw.Write((uint)block.Length);
            bw.Write(block);
        }

        // Unused while MetadataDirty stays false (binary-only imports), but kept
        // so the data model is round-trippable end-to-end.
        private static byte[] BuildMetadataBlock(AwcShader s)
        {
            var regs = s.Registers ?? Array.Empty<AwcShaderRegister>();
            ushort regCount = (ushort)regs.Length;
            ushort cbCountTotal = 0;
            for (int i = 0; i < regs.Length; i++)
                cbCountTotal += (ushort)(regs[i].CBuffers?.Length ?? 0);

            var buf = new List<byte>(256);

            void WriteU16(ushort v) { buf.Add((byte)(v & 0xFF)); buf.Add((byte)(v >> 8)); }
            void WriteU32At(int pos, uint v) { buf[pos] = (byte)(v & 0xFF); buf[pos + 1] = (byte)((v >> 8) & 0xFF); buf[pos + 2] = (byte)((v >> 16) & 0xFF); buf[pos + 3] = (byte)((v >> 24) & 0xFF); }
            void WriteU16At(int pos, ushort v) { buf[pos] = (byte)(v & 0xFF); buf[pos + 1] = (byte)(v >> 8); }

            WriteU16(regCount);
            WriteU16(cbCountTotal);
            WriteU16(s.TexCount);
            WriteU16(0); // block_size_copy placeholder

            const int headerSize = 8;
            int regHeadersStart = headerSize;
            int regHeadersSize = regCount * 28;
            for (int i = 0; i < regHeadersSize; i++) buf.Add(0);

            int[] cbStructPos = new int[regCount];
            for (int i = 0; i < regs.Length; i++)
            {
                var cbs = regs[i].CBuffers;
                if (cbs != null && cbs.Length > 0)
                {
                    cbStructPos[i] = buf.Count;
                    for (int k = 0; k < cbs.Length * 24; k++) buf.Add(0);
                }
                else cbStructPos[i] = 0;
            }

            int[] regStringPos = new int[regCount];
            for (int i = 0; i < regs.Length; i++)
            {
                regStringPos[i] = buf.Count;
                byte[] nb = Encoding.GetEncoding("ISO-8859-1").GetBytes(regs[i].Name ?? string.Empty);
                buf.AddRange(nb);
                buf.Add(0);
            }

            for (int i = 0; i < regs.Length; i++)
            {
                var cbs = regs[i].CBuffers;
                if (cbs == null || cbs.Length == 0) continue;
                int cbBase = cbStructPos[i];
                for (int j = 0; j < cbs.Length; j++)
                {
                    var cb = cbs[j];
                    int pCb = cbBase + j * 24;

                    int cbStrPos = buf.Count;
                    byte[] nb = Encoding.GetEncoding("ISO-8859-1").GetBytes(cb.Name ?? string.Empty);
                    buf.AddRange(nb);
                    buf.Add(0);

                    uint cbNameOffset = (uint)(cbStrPos - pCb);
                    WriteU16At(pCb + 0, (ushort)cb.Type);
                    WriteU16At(pCb + 2, cb.ArraySize);
                    WriteU16At(pCb + 4, cb.PackOffset);
                    WriteU32At(pCb + 6, cbNameOffset);

                    byte[] hashBytes = cb.NameHashData;
                    if (hashBytes == null || hashBytes.Length != 14) hashBytes = new byte[14];
                    for (int k = 0; k < 14; k++) buf[pCb + 10 + k] = hashBytes[k];
                }
            }

            for (int i = 0; i < regs.Length; i++)
            {
                int pReg = regHeadersStart + i * 28;
                var r = regs[i];
                int cbCount = r.CBuffers?.Length ?? 0;
                ushort cbDataOffset = (ushort)(cbCount > 0 ? (cbStructPos[i] - pReg) : 0);
                ushort regStrOffset = (ushort)(regStringPos[i] - pReg);

                WriteU16At(pReg + 0, (ushort)r.ResourceType);
                WriteU16At(pReg + 2, r.RegisterSlot);
                buf[pReg + 4] = (byte)cbCount;
                buf[pReg + 5] = r.NumDescriptors;
                buf[pReg + 6] = r.RegisterSpace;
                buf[pReg + 7] = r.Reserved;
                WriteU16At(pReg + 8, cbDataOffset);
                WriteU16At(pReg + 10, regStrOffset);

                byte[] extra = r.ExtraData;
                if (extra == null || extra.Length != 16) extra = new byte[16];
                for (int k = 0; k < 16; k++) buf[pReg + 12 + k] = extra[k];
            }

            if ((buf.Count & 1) != 0) buf.Add(0);

            ushort total = (ushort)buf.Count;
            WriteU16At(6, total);

            return buf.ToArray();
        }
    }
}
