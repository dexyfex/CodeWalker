using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RelFile : PackedFile
    {
        public RpfFileEntry FileEntry { get; set; }
        public string Name { get; set; }

        public uint Type { get; set; }
        public uint DataLength { get; set; }
        public byte[] DataBlock { get; set; }
        public uint DataUnkVal { get; set; }
        public uint NameTableLength { get; set; }
        public uint NameTableCount { get; set; }
        public uint[] NameTableOffsets { get; set; }
        public string[] NameTable { get; set; }
        public uint IndexCount { get; set; }
        public uint IndexStringFlags { get; set; }
        public RelIndexHash[] IndexHashes { get; set; }
        public RelIndexString[] IndexStrings { get; set; }
        public uint Unk05Count { get; set; }
        public uint[] Unk05Arr { get; set; }
        public MetaHash[] Unk05Hashes { get; set; }
        public uint Unk06Count { get; set; }
        public uint[] Unk06Arr { get; set; }
        public MetaHash[] Unk06Hashes { get; set; }

        public RelData[] RelDatas { get; set; }
        public RelData[] RelDatasSorted { get; set; }
        //testing zone for decoding .rel audio files.

        public RelFile()
        {
        }
        public RelFile(RpfFileEntry entry)
        {
            FileEntry = entry;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            FileEntry = entry;
            Name = entry.Name;

            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms);
            StringBuilder sb = new StringBuilder();

            Type = br.ReadUInt32(); //type/version?

            DataLength = br.ReadUInt32(); //length of data block
            DataBlock = br.ReadBytes((int)DataLength); //data block... synth infos? script?

            NameTableLength = br.ReadUInt32(); //length of this nametable block
            NameTableCount = br.ReadUInt32();
            if (NameTableCount > 0)
            {
                uint[] d02 = new uint[NameTableCount]; //string offsets
                for (uint i = 0; i < NameTableCount; i++)
                {
                    d02[i] = br.ReadUInt32();
                }
                NameTableOffsets = d02;
                string[] names = new string[NameTableCount];
                for (uint i = 0; i < NameTableCount; i++)
                {
                    sb.Clear();
                    while (true)
                    {
                        char c = (char)br.ReadByte();
                        if (c != 0) sb.Append(c);
                        else break;
                    }
                    names[i] = sb.ToString();
                }
                NameTable = names;
            }

            IndexCount = br.ReadUInt32(); //count of index items
            if (IndexCount > 0)
            {
                //checking NameTableLength here doesn't make sense!
                if ((Type == 4) && (NameTableLength == 4))//audioconfig.dat4.rel
                {
                    IndexStringFlags = br.ReadUInt32(); //what is this?  2524
                    RelIndexString[] indexstrs = new RelIndexString[IndexCount];
                    for (uint i = 0; i < IndexCount; i++)
                    {
                        byte sl = br.ReadByte();
                        sb.Clear();
                        for (int j = 0; j < sl; j++)
                        {
                            char c = (char)br.ReadByte();
                            if (c != 0) sb.Append(c);
                        }
                        RelIndexString cunk01 = new RelIndexString();
                        cunk01.Name = sb.ToString();
                        cunk01.Offset = br.ReadUInt32();
                        cunk01.Length = br.ReadUInt32();
                        indexstrs[i] = cunk01;
                    }
                    IndexStrings = indexstrs;
                }
                else //for all other .rel files...
                {
                    RelIndexHash[] indexhashes = new RelIndexHash[IndexCount];
                    for (uint i = 0; i < IndexCount; i++)
                    {
                        RelIndexHash unk01 = new RelIndexHash();
                        unk01.Name = new MetaHash(br.ReadUInt32());
                        unk01.Offset = br.ReadUInt32();
                        unk01.Length = br.ReadUInt32();
                        indexhashes[i] = unk01;
                    }
                    IndexHashes = indexhashes;
                }
            }


            Unk05Count = br.ReadUInt32();
            if (Unk05Count != 0)
            {
                uint[] d05 = new uint[Unk05Count];
                MetaHash[] d05h = new MetaHash[Unk05Count];
                for (uint i = 0; i < Unk05Count; i++)
                {
                    d05[i] = br.ReadUInt32();

                    var pos = ms.Position;
                    ms.Position = d05[i];
                    d05h[i] = new MetaHash(br.ReadUInt32());
                    ms.Position = pos;
                }
                Unk05Arr = d05;
                Unk05Hashes = d05h;
            }

            Unk06Count = br.ReadUInt32();
            if (Unk06Count != 0)
            {
                uint[] d06 = new uint[Unk06Count];
                MetaHash[] d06h = new MetaHash[Unk06Count];
                for (uint i = 0; i < Unk06Count; i++)
                {
                    d06[i] = br.ReadUInt32();

                    var pos = ms.Position;
                    ms.Position = d06[i];
                    d06h[i] = new MetaHash(br.ReadUInt32());
                    ms.Position = pos;
                }
                Unk06Arr = d06;
                Unk06Hashes = d06h;
            }

            if (ms.Position != ms.Length)
            { }
            //EOF!

            br.Dispose();
            ms.Dispose();


            ParseDataBlock();
        }


        private void ParseDataBlock()
        {



            MemoryStream ms = new MemoryStream(DataBlock);
            BinaryReader br = new BinaryReader(ms);

            DataUnkVal = br.ReadUInt32(); //3 bytes used... for? ..version?
            switch (DataUnkVal)
            {
                case 5252715: //dlcbusiness_amp.dat10.rel
                case 5301323: //dlcbeach_game.dat149.rel
                case 5378673: //dlcmpheist_game.dat150.rel
                case 5750395: //dlcbeach_game.dat150.rel
                case 6353778: //dlcbeach_game.dat151.rel
                case 6894089: //dlcpilotschool_game.dat151.rel
                case 6978435: //dlcxmas2_amp.dat10.rel
                case 7126027: //audioconfig.dat4.rel
                case 7314721: //dlcmpheist_amp.dat10.rel
                case 7516460: //dlcpd03_game.dat151.rel
                case 7917027: //dlcluxe_amp.dat10.rel
                case 7921508: //dlcluxe_game.dat151.rel
                case 8149475: //dlcluxe2_amp.dat10.rel
                case 8751734: //dlcsfx1_game.dat151.rel
                case 9028036: //dlchalloween_amp.dat10.rel
                case 9037528: //dlclowrider_amp.dat10.rel
                case 9458585: //dlcapartment_amp.dat10.rel
                case 9486222: //dlcapartment_mix.dat15.rel
                case 9806108: //mpvalentines2_amp.dat10.rel
                case 9813679: //dlcjanuary2016_amp.dat10.rel
                case 10269543://dlclow2_amp.dat10.rel
                case 10891463://dlcexec1_amp.dat10.rel
                case 11171338://dlcstunt_amp.dat10.rel
                case 11918985://dlcbiker_amp.dat10.rel
                case 12470522://dlcimportexport_amp.dat10.rel
                case 12974726://audioconfig.dat4.rel
                case 13117164://dlcspecialraces_amp.dat10.rel
                    break;
                default:
                    break;
            }


            List<RelData> reldatas = new List<RelData>();
            if (IndexHashes != null)
            {
                foreach (var indexhash in IndexHashes)
                {
                    ms.Position = indexhash.Offset;
                    RelData d = new RelData();
                    d.NameHash = indexhash.Name;
                    d.Offset = indexhash.Offset;
                    d.Length = indexhash.Length;
                    d.Data = br.ReadBytes((int)indexhash.Length);
                    reldatas.Add(d);
                }
            }
            else if (IndexStrings != null)
            {
                foreach (var indexstr in IndexStrings)
                {
                    ms.Position = indexstr.Offset;
                    RelData d = new RelData();
                    d.Name = indexstr.Name;
                    d.Offset = indexstr.Offset;
                    d.Length = indexstr.Length;
                    d.Data = br.ReadBytes((int)indexstr.Length);
                    reldatas.Add(d);
                }
            }
            RelDatas = reldatas.ToArray();

            reldatas.Sort((d1, d2) => d1.Offset.CompareTo(d2.Offset));
            RelDatasSorted = reldatas.ToArray();


            br.Dispose();
            ms.Dispose();


            foreach (var d in RelDatas)
            {
                using (BinaryReader dbr = new BinaryReader(new MemoryStream(d.Data)))
                {
                    switch (Type)
                    {
                        case 4:   //00000100  //speech.dat4.rel, audioconfig.dat4.rel
                            ParseData4(d, dbr);
                            break;
                        case 10:  //00001010  //amp.dat10.rel
                            ParseData10(d, dbr);
                            break;
                        case 15:  //00001111  //mix.dat15.rel
                            ParseData15(d, dbr);
                            break;
                        case 16:  //00010000  //curves.dat16.rel
                            ParseData16(d, dbr);
                            break;
                        case 22:  //00010110  //categories.dat22.rel
                            ParseData22(d, dbr);
                            break;
                        case 54:  //00110110  //sounds.dat54.rel
                            ParseData54(d, dbr);
                            break;
                        case 149: //10010101  //game.dat149.rel
                            ParseData149(d, dbr);
                            break;
                        case 150: //10010110  //game.dat150.rel
                            ParseData150(d, dbr);
                            break;
                        case 151: //10010111  //game.dat151.rel
                            ParseData151(d, dbr);
                            break;
                        default:
                            break;
                    }
                }
            }


        }

        private void ParseData4(RelData d, BinaryReader br)
        {
            //speech.dat4.rel, audioconfig.dat4.rel

            if (d.Length == 1)
            {
                byte b = br.ReadByte();
                switch (b)
                {
                    case 0:
                    case 25:
                    case 28:
                    case 34:
                    case 89:
                    case 94:
                    case 178:
                        break;
                    default:
                        break;
                }
                return;
            }
            if (d.Length == 2)
            {
                byte b = br.ReadByte();
                switch (b)
                {
                    case 4:
                    case 1:
                    case 15:
                    case 12:
                    case 3:
                    case 2:
                    case 7:
                    case 5:
                    case 158:
                    case 25:
                    case 16:
                    case 64:
                    case 6:
                    case 8:
                    case 14:
                    case 22:
                    case 18:
                    case 20:
                    case 32:
                    case 17:
                    case 30:
                    case 9:
                    case 0:
                    case 47:
                    case 224:
                    case 200:
                    case 136:
                    case 45:
                    case 54:
                    case 28:
                    case 19:
                    case 37:
                    case 61:
                    case 38:
                    case 128:
                    case 24:
                    case 26:
                    case 40:
                    case 13:
                    case 36:
                    case 78:
                    case 34:
                    case 10:
                    case 21:
                    case 192:
                    case 60:
                    case 29:
                    case 33:
                    case 72:
                    case 57:
                    case 133:
                    case 11:
                        break;
                    default:
                        break;
                }
                return;
            }
            if (d.Length == 4)
            {
                uint h = br.ReadUInt32();
                return;
            }


            byte b00 = br.ReadByte();
            switch (b00)
            {
                case 4:
                case 1:
                case 0:
                case 6:
                case 3:
                case 2:
                case 5:
                case 7:
                case 15:
                case 10:
                case 8:
                case 9:
                    break;

                case 23:
                case 12:
                case 11:
                case 16:
                case 13:
                case 36:
                case 30:
                case 31:
                case 27:
                case 20:
                case 19:
                case 14:
                case 40:
                case 46:
                case 22:
                case 18:
                case 21:
                case 45:
                case 17:
                case 48:
                case 87:
                case 38:
                case 28:
                case 29:
                case 43:
                case 69:
                case 50:
                case 25:
                case 32:
                case 35:
                case 34:
                    break;
                default:
                    break;
            }
        }
        private void ParseData10(RelData d, BinaryReader br)
        {
            //amp.dat10.rel

            byte b00 = br.ReadByte();
            switch (b00)
            {
                case 1:
                case 3:
                    break;
                default:
                    break;
            }
        }
        private void ParseData15(RelData d, BinaryReader br)
        {
            //mix.dat15.rel

            byte b00 = br.ReadByte();
            switch (b00)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    break;
                default:
                    break;
            }
        }
        private void ParseData16(RelData d, BinaryReader br)
        {
            //curves.dat16.rel

            byte b00 = br.ReadByte();
            switch (b00)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 12:
                case 13:
                case 15:
                    break;
                default:
                    break;
            }
        }
        private void ParseData22(RelData d, BinaryReader br)
        {
            //categories.dat22.rel

            byte b00 = br.ReadByte();
            switch (b00)
            {
                case 0:
                    break;
                default:
                    break;
            }
        }
        private void ParseData54(RelData d, BinaryReader br)
        {
            //sounds.dat54.rel

            byte b00 = br.ReadByte();
            switch (b00)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                    break;
                default:
                    break;
            }
        }
        private void ParseData149(RelData d, BinaryReader br)
        {
            //game.dat149.rel

            byte b00 = br.ReadByte();
            switch (b00)
            {
                case 3:
                case 4:
                case 17:
                case 50:
                case 57:
                case 62:
                case 63:
                case 66:
                case 76:
                case 88:
                case 90:
                    break;
                default:
                    break;
            }
        }
        private void ParseData150(RelData d, BinaryReader br)
        {
            //game.dat150.rel

            byte b00 = br.ReadByte();
            switch (b00)
            {
                case 3:
                case 4:
                case 6:
                case 8:
                case 17:
                case 32:
                case 37:
                case 38:
                case 39:
                case 47:
                case 50:
                case 52:
                case 57:
                case 62:
                case 63:
                case 64:
                case 65:
                case 66:
                case 76:
                case 88:
                case 90:
                case 117:
                    break;
                default:
                    break;
            }
        }
        private void ParseData151(RelData d, BinaryReader br)
        {
            //game.dat151.rel

            byte b00 = br.ReadByte(); //???
            switch (b00)
            {
                case 1://new
                case 2://new
                case 3:
                case 4:
                case 5://new
                case 6:
                case 7://new
                case 8://
                case 9://new
                case 11://new
                case 12://new
                case 13://new
                case 14://new
                case 15://new
                case 16://new
                case 17:
                case 18://new
                case 22://new
                case 23://new
                case 24://new
                case 25://new
                case 26://new
                case 27://new
                case 28://new
                case 29://new
                case 30://new
                case 31://new
                case 32://
                case 33://new
                case 35://new
                case 36://new
                case 37://
                case 38://
                case 39://
                case 40://new
                case 41://new
                case 42://new
                case 44://new
                case 45://new
                case 46://new
                case 47://
                case 48://new
                case 49://new
                case 50:
                case 51://new
                case 52://
                case 53://new
                case 54://new
                case 55://new
                case 56://new
                case 57:
                case 59://new
                case 62:
                case 63:
                case 64:
                case 65://
                case 66:
                case 67://new
                case 68://new
                case 69://new
                case 70://new
                case 71://new
                case 72://new
                case 73://new
                case 74://new
                case 75://new
                case 76:
                case 77://new
                case 78://new
                case 79://new
                case 80://new
                case 81://new
                case 82://new
                case 83://new
                case 84://new
                case 85://new
                case 86://new
                case 87://new
                case 88:
                case 90:
                case 91://new
                case 92://new
                case 93://new
                case 94://new
                case 95://new
                case 96://new
                case 98://new
                case 99://new
                case 100://new
                case 101://new
                case 102://new
                case 103://new
                case 104://new
                case 105://new
                case 106://new
                case 107://new
                case 108://new
                case 109://new
                case 110://new
                case 111://new
                case 112://new
                case 113://new
                case 114://new
                case 115://new
                case 116://new
                case 117:
                case 118://new
                case 119://new
                case 120://new
                case 121://new
                    break;
                default:
                    break;
            }
        }





        public override string ToString()
        {
            return Name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct RelIndexHash
    {
        public MetaHash Name { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }

        public override string ToString()
        {
            return Name.ToString() + ", " + Offset.ToString() + ", " + Length.ToString();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct RelIndexString
    {
        public string Name { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }

        public override string ToString()
        {
            return Name + ", " + Offset.ToString() + ", " + Length.ToString();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RelData
    {
        public MetaHash NameHash { get; set; }
        public string Name { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }
        public byte[] Data { get; set; }

        public override string ToString()
        {
            string ol= ", " + Offset.ToString() + ", " + Length.ToString();
            if (!string.IsNullOrEmpty(Name)) return Name + ol;
            return NameHash.ToString() + ol;
        }
    }

}
