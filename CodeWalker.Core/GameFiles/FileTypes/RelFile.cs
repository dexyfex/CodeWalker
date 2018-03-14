using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using SharpDX;




/*

Parts of this are adapted from CamxxCore's RageAudioTool, although it's been completely reworked for CW.
-dexyfex


https://github.com/CamxxCore/RageAudioTool

MIT License

Copyright (c) 2017 Cameron Berry

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/



namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class RelFile : PackedFile
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
        public uint ContainerCount { get; set; }
        public uint[] ContainerUnkArr { get; set; }
        public MetaHash[] ContainerHashes { get; set; }

        public RelData[] RelDatas { get; set; }
        public RelData[] RelDatasSorted { get; set; }
        //testing zone for decoding .rel audio files.

        public Dictionary<uint, RelData> RelDataDict { get; set; } = new Dictionary<uint, RelData>();


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
            DataBlock = br.ReadBytes((int)DataLength); //main data block...

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

                    //JenkIndex.Ensure(names[i]); //really need both here..?
                    JenkIndex.Ensure(names[i].ToLowerInvariant());
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

            ContainerCount = br.ReadUInt32();
            if (ContainerCount != 0)
            {
                uint[] cunks = new uint[ContainerCount];
                MetaHash[] chashes = new MetaHash[ContainerCount];
                for (uint i = 0; i < ContainerCount; i++)
                {
                    cunks[i] = br.ReadUInt32();

                    var pos = ms.Position;
                    ms.Position = cunks[i];
                    chashes[i] = new MetaHash(br.ReadUInt32());
                    ms.Position = pos;
                }
                ContainerUnkArr = cunks;
                ContainerHashes = chashes;
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

            DataUnkVal = br.ReadUInt32(); //3 bytes used... for? ..version? flags?
            //switch (DataUnkVal)
            //{
            //    case 5252715: //dlcbusiness_amp.dat10.rel
            //    case 5301323: //dlcbeach_game.dat149.rel
            //    case 5378673: //dlcmpheist_game.dat150.rel
            //    case 5750395: //dlcbeach_game.dat150.rel
            //    case 6353778: //dlcbeach_game.dat151.rel
            //    case 6894089: //dlcpilotschool_game.dat151.rel
            //    case 6978435: //dlcxmas2_amp.dat10.rel
            //    case 7126027: //audioconfig.dat4.rel
            //    case 7314721: //dlcmpheist_amp.dat10.rel
            //    case 7516460: //dlcpd03_game.dat151.rel
            //    case 7917027: //dlcluxe_amp.dat10.rel
            //    case 7921508: //dlcluxe_game.dat151.rel
            //    case 8149475: //dlcluxe2_amp.dat10.rel
            //    case 8751734: //dlcsfx1_game.dat151.rel
            //    case 9028036: //dlchalloween_amp.dat10.rel
            //    case 9037528: //dlclowrider_amp.dat10.rel
            //    case 9458585: //dlcapartment_amp.dat10.rel
            //    case 9486222: //dlcapartment_mix.dat15.rel
            //    case 9806108: //mpvalentines2_amp.dat10.rel
            //    case 9813679: //dlcjanuary2016_amp.dat10.rel
            //    case 10269543://dlclow2_amp.dat10.rel
            //    case 10891463://dlcexec1_amp.dat10.rel
            //    case 11171338://dlcstunt_amp.dat10.rel
            //    case 11918985://dlcbiker_amp.dat10.rel
            //    case 12470522://dlcimportexport_amp.dat10.rel
            //    case 12974726://audioconfig.dat4.rel
            //    case 13117164://dlcspecialraces_amp.dat10.rel
            //        break;
            //    default:
            //        break;
            //}


            List<RelData> reldatas = new List<RelData>();
            if (IndexHashes != null)
            {
                foreach (var indexhash in IndexHashes)
                {
                    reldatas.Add(ReadRelData(br, indexhash));
                }
            }
            else if (IndexStrings != null)
            {
                foreach (var indexstr in IndexStrings)
                {
                    reldatas.Add(ReadRelData(br, indexstr));
                }
            }
            RelDatas = reldatas.ToArray();

            reldatas.Sort((d1, d2) => d1.DataOffset.CompareTo(d2.DataOffset));
            RelDatasSorted = reldatas.ToArray();


            br.Dispose();
            ms.Dispose();




            RelDataDict.Clear();
            foreach (var reldata in RelDatas)
            {
                if ((reldata.NameHash == 0) && !string.IsNullOrEmpty(reldata.Name))
                {
                    reldata.NameHash = JenkHash.GenHash(reldata.Name); //should this be lower case?
                    JenkIndex.Ensure(reldata.Name);
                    JenkIndex.Ensure(reldata.Name.ToLowerInvariant()); //which one to use?
                }
                
                //if (reldata.NameHash == 0)
                //{ }//no hits here
                //if (RelDataDict.ContainsKey(reldata.NameHash))
                //{ }//no hits here

                RelDataDict[reldata.NameHash] = reldata;
            }
            foreach (var reldata in RelDatas)
            {
                RelSound snd = reldata as RelSound;
                if (snd != null)
                {
                    if (snd.AudioTracksCount > 0)
                    {
                        snd.AudioTracks = new RelData[snd.AudioTracksCount];
                        for (int i = 0; i < snd.AudioTracksCount; i++)
                        {
                            var audhash = snd.AudioTrackHashes[i];
                            RelData auddata = null;
                            if (RelDataDict.TryGetValue(audhash, out auddata))
                            {
                                snd.AudioTracks[i] = auddata;
                            }
                            else
                            { }
                        }
                    }
                    if (snd.AudioContainers != null)
                    {
                        foreach (var cnt in snd.AudioContainers)
                        {
                            string cname = JenkIndex.TryGetString(cnt.Hash);
                            if (!string.IsNullOrEmpty(cname))
                            { }
                            else
                            { }
                        }
                    }
                }
            }

            #region test
            //foreach (var d in RelDatas)
            //{
            //    using (BinaryReader dbr = new BinaryReader(new MemoryStream(d.Data)))
            //    {
            //        switch (Type)
            //        {
            //            case 4:   //00000100  //speech.dat4.rel, audioconfig.dat4.rel
            //                ParseData4(d, dbr);
            //                break;
            //            case 10:  //00001010  //amp.dat10.rel
            //                ParseData10(d, dbr);
            //                break;
            //            case 15:  //00001111  //mix.dat15.rel
            //                ParseData15(d, dbr);
            //                break;
            //            case 16:  //00010000  //curves.dat16.rel
            //                ParseData16(d, dbr);
            //                break;
            //            case 22:  //00010110  //categories.dat22.rel
            //                ParseData22(d, dbr);
            //                break;
            //            case 54:  //00110110  //sounds.dat54.rel
            //                ParseData54(d, dbr);
            //                break;
            //            case 149: //10010101  //game.dat149.rel
            //                ParseData149(d, dbr);
            //                break;
            //            case 150: //10010110  //game.dat150.rel
            //                ParseData150(d, dbr);
            //                break;
            //            case 151: //10010111  //game.dat151.rel
            //                ParseData151(d, dbr);
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}
            #endregion

        }




        private RelData ReadRelData(BinaryReader br, RelIndexHash h)
        {
            return ReadRelData(br, null, h.Name, h.Offset, h.Length);
        }
        private RelData ReadRelData(BinaryReader br, RelIndexString s)
        {
            return ReadRelData(br, s.Name, 0, s.Offset, s.Length);
        }
        private RelData ReadRelData(BinaryReader br, string name, MetaHash hash, uint offset, uint length)
        {
            br.BaseStream.Position = offset;
            byte[] data = br.ReadBytes((int)length);


            RelData d = new RelData(); //use this base object to construct the derived one...
            d.Name = name;
            d.NameHash = hash;
            d.DataOffset = offset;
            d.DataLength = length;
            d.Data = data;


            using (BinaryReader dbr = new BinaryReader(new MemoryStream(data)))
            {
                d.ReadType(dbr);

                switch (Type)
                {
                    case 4:   //speech.dat4.rel, audioconfig.dat4.rel
                        return ReadData4(d, dbr);
                    case 10:  //amp.dat10.rel
                        return ReadData10(d, dbr);
                    case 15:  //mix.dat15.rel
                        return ReadData15(d, dbr);
                    case 16:  //curves.dat16.rel
                        return ReadData16(d, dbr);
                    case 22:  //categories.dat22.rel
                        return ReadData22(d, dbr);
                    case 54:  //sounds.dat54.rel
                        return ReadData54(d, dbr);
                    case 149: //game.dat149.rel
                        return ReadData149(d, dbr);
                    case 150: //game.dat150.rel
                        return ReadData150(d, dbr);
                    case 151: //game.dat151.rel
                        return ReadData151(d, dbr);
                    default:
                        return d; //shouldn't get here...
                }
            }
        }



        private RelData ReadData4(RelData d, BinaryReader br)
        {
            if (NameTableLength == 4) //(for audioconfig.dat4.rel)
            {
            }
            else //(for eg speech.dat4.rel)
            {
            }
            return d;
        }
        private RelData ReadData10(RelData d, BinaryReader br)
        {
            return d;
        }
        private RelData ReadData15(RelData d, BinaryReader br)
        {
            return d;
        }
        private RelData ReadData16(RelData d, BinaryReader br)
        {
            return d;
        }
        private RelData ReadData22(RelData d, BinaryReader br)
        {
            //RelSound s = new RelSound(d, br);
            //return s;
            return d;
        }
        private RelData ReadData54(RelData d, BinaryReader br)
        {
            switch ((Dat54SoundType)d.TypeID)
            {
                case Dat54SoundType.LoopingSound: return new Dat54LoopingSound(d, br);
                case Dat54SoundType.EnvelopeSound: return new Dat54EnvelopeSound(d, br);
                case Dat54SoundType.TwinLoopSound: return new Dat54TwinLoopSound(d, br);
                case Dat54SoundType.SpeechSound: return new Dat54SpeechSound(d, br);
                case Dat54SoundType.OnStopSound: return new Dat54OnStopSound(d, br);
                case Dat54SoundType.WrapperSound: return new Dat54WrapperSound(d, br);
                case Dat54SoundType.SequentialSound: return new Dat54SequentialSound(d, br);
                case Dat54SoundType.StreamingSound: return new Dat54StreamingSound(d, br);
                case Dat54SoundType.RetriggeredOverlappedSound: return new Dat54RetriggeredOverlappedSound(d, br);
                case Dat54SoundType.CrossfadeSound: return new Dat54CrossfadeSound(d, br);
                case Dat54SoundType.CollapsingStereoSound: return new Dat54CollapsingStereoSound(d, br);
                case Dat54SoundType.SimpleSound: return new Dat54SimpleSound(d, br);
                case Dat54SoundType.MultitrackSound: return new Dat54MultitrackSound(d, br);
                case Dat54SoundType.RandomizedSound: return new Dat54RandomizedSound(d, br);
                case Dat54SoundType.EnvironmentSound: return new Dat54EnvironmentSound(d, br);
                case Dat54SoundType.DynamicEntitySound: return new Dat54DynamicEntitySound(d, br);
                case Dat54SoundType.SequentialOverlapSound: return new Dat54SequentialOverlapSound(d, br);
                case Dat54SoundType.ModularSynthSound: return new Dat54ModularSynthSound(d, br);
                case Dat54SoundType.GranularSound: return new Dat54GranularSound(d, br);
                case Dat54SoundType.DirectionalSound: return new Dat54DirectionalSound(d, br);
                case Dat54SoundType.KineticSound: return new Dat54KineticSound(d, br);
                case Dat54SoundType.SwitchSound: return new Dat54SwitchSound(d, br);
                case Dat54SoundType.VariableCurveSound: return new Dat54VariableCurveSound(d, br);
                case Dat54SoundType.VariablePrintValueSound: return new Dat54VariablePrintValueSound(d, br);
                case Dat54SoundType.VariableBlockSound: return new Dat54VariableBlockSound(d, br);
                case Dat54SoundType.IfSound: return new Dat54IfSound(d, br);
                case Dat54SoundType.MathOperationSound: return new Dat54MathOperationSound(d, br);
                case Dat54SoundType.ParameterTransformSound: return new Dat54ParameterTransformSound(d, br);
                case Dat54SoundType.FluctuatorSound: return new Dat54FluctuatorSound(d, br);
                case Dat54SoundType.AutomationSound: return new Dat54AutomationSound(d, br);
                case Dat54SoundType.ExternalStreamSound: return new Dat54ExternalStreamSound(d, br);
                case Dat54SoundType.SoundSet: return new Dat54SoundSet(d, br);
                case Dat54SoundType.Unknown: return new Dat54UnknownSound(d, br);
                case Dat54SoundType.Unknown2: return new Dat54UnknownSound2(d, br);
                case Dat54SoundType.SoundList: return new Dat54SoundList(d, br);
                default:
                    return new Dat54Sound(d, br); //shouldn't get here
            }
        }
        private RelData ReadData149(RelData d, BinaryReader br)
        {
            //RelSound s = new RelSound(d, br);
            //return s;
            return d;
        }
        private RelData ReadData150(RelData d, BinaryReader br)
        {
            return d;
        }
        private RelData ReadData151(RelData d, BinaryReader br)
        {
            switch ((Dat151RelType)d.TypeID)
            {
                case Dat151RelType.Collision: //maybe for vehicle
                case Dat151RelType.Vehicle:
                case Dat151RelType.VehicleEngine:
                case Dat151RelType.Entity: //not sure about this
                case Dat151RelType.Stream: //generic audio stream?
                case Dat151RelType.Helicopter: //maybe
                case Dat151RelType.SpeechParams:
                case Dat151RelType.Weapon:
                case Dat151RelType.RadioStationsDLC: //
                case Dat151RelType.RadioDLC:
                case Dat151RelType.DLCMusic:
                case Dat151RelType.PedPVG: //maybe Ped Voice Group?
                case Dat151RelType.WeaponAudioItem:
                case Dat151RelType.Aeroplane:
                case Dat151RelType.Mood:
                case Dat151RelType.StartTrackAction:
                case Dat151RelType.StopTrackAction:
                case Dat151RelType.SetMoodAction:
                case Dat151RelType.PlayerAction:
                case Dat151RelType.StartOneShotAction:
                case Dat151RelType.StopOneShotAction:
                case Dat151RelType.AnimalParams:
                case Dat151RelType.VehicleScannerParams: //maybe not just vehicle
                case Dat151RelType.Explosion:
                case Dat151RelType.VehicleEngineGranular: //maybe not just vehicle
                case Dat151RelType.ShoreLinePool:
                case Dat151RelType.ShoreLineLake:
                case Dat151RelType.ShoreLineRiver:
                case Dat151RelType.ShoreLineOcean:
                case Dat151RelType.ShoreLineList:
                case Dat151RelType.RadioDjSpeechAction:
                case Dat151RelType.FadeOutRadioAction:
                case Dat151RelType.FadeInRadioAction:
                case Dat151RelType.ForceRadioTrackAction:
                case Dat151RelType.Unk2:
                case Dat151RelType.Unk7:
                case Dat151RelType.Unk9:
                case Dat151RelType.Unk11:
                case Dat151RelType.Unk12:
                case Dat151RelType.Unk13:
                case Dat151RelType.Unk15:
                case Dat151RelType.Unk16:
                case Dat151RelType.Unk18:
                case Dat151RelType.Unk22:
                case Dat151RelType.Unk23:
                case Dat151RelType.Unk27:
                case Dat151RelType.Unk28:
                case Dat151RelType.Unk29:
                case Dat151RelType.Unk31:
                case Dat151RelType.Unk33:
                case Dat151RelType.Unk35:
                case Dat151RelType.Unk36:
                case Dat151RelType.Unk40:
                case Dat151RelType.Unk41:
                case Dat151RelType.Unk42:
                case Dat151RelType.Interior:
                case Dat151RelType.Unk45:
                case Dat151RelType.InteriorRoom:
                case Dat151RelType.Unk47:
                case Dat151RelType.Unk48:
                case Dat151RelType.Unk49:
                case Dat151RelType.Unk51:
                case Dat151RelType.Mod:
                case Dat151RelType.Unk53:
                case Dat151RelType.Unk54:
                case Dat151RelType.Unk55:
                case Dat151RelType.Unk56:
                case Dat151RelType.Unk59:
                case Dat151RelType.Unk69:
                case Dat151RelType.Unk70:
                case Dat151RelType.Unk71:
                case Dat151RelType.Unk72:
                case Dat151RelType.Unk74:
                case Dat151RelType.Unk75:
                case Dat151RelType.Unk77:
                case Dat151RelType.Unk78:
                case Dat151RelType.Unk79:
                case Dat151RelType.Unk80:
                case Dat151RelType.Unk81:
                case Dat151RelType.Unk82:
                case Dat151RelType.Unk83:
                case Dat151RelType.Unk84:
                case Dat151RelType.Unk85:
                case Dat151RelType.Unk86:
                case Dat151RelType.Unk95:
                case Dat151RelType.Unk96:
                case Dat151RelType.Unk99:
                case Dat151RelType.Unk100:
                case Dat151RelType.Unk101:
                case Dat151RelType.Unk105:
                case Dat151RelType.Unk106:
                case Dat151RelType.Unk107:
                case Dat151RelType.Unk108:
                case Dat151RelType.Unk109:
                case Dat151RelType.Unk110:
                case Dat151RelType.Unk111:
                case Dat151RelType.Unk112:
                case Dat151RelType.Unk113:
                case Dat151RelType.Unk114:
                case Dat151RelType.Unk115:
                case Dat151RelType.Unk116:
                case Dat151RelType.Unk117:
                case Dat151RelType.Unk118:
                case Dat151RelType.Unk119:
                case Dat151RelType.Unk120:
                case Dat151RelType.Unk121:
                    return new Dat151RelData(d, br);

                case Dat151RelType.AmbientEmitterList: return new Dat151AmbientEmitterList(d, br);
                case Dat151RelType.AmbientZone: return new Dat151AmbientZone(d, br);
                case Dat151RelType.AmbientEmitter: return new Dat151AmbientEmitter(d, br);
                case Dat151RelType.AmbientZoneList: return new Dat151AmbientZoneList(d, br);
                default:
                    return new Dat151RelData(d, br);
            }
        }





        #region first research

        private void ParseData4(RelData d, BinaryReader br)
        {
            //speech.dat4.rel, audioconfig.dat4.rel

            if (d.DataLength == 1)
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
            if (d.DataLength == 2)
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
            if (d.DataLength == 4)
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

        #endregion



        public override string ToString()
        {
            return Name;
        }
    }

    [TC(typeof(EXP))] public struct RelIndexHash
    {
        public MetaHash Name { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }

        public override string ToString()
        {
            return Name.ToString() + ", " + Offset.ToString() + ", " + Length.ToString();
        }
    }


    [TC(typeof(EXP))] public struct RelIndexString
    {
        public string Name { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }

        public override string ToString()
        {
            return Name + ", " + Offset.ToString() + ", " + Length.ToString();
        }
    }



    [TC(typeof(EXP))] public class RelData
    {
        public MetaHash NameHash { get; set; }
        public string Name { get; set; }
        public uint DataOffset { get; set; }
        public uint DataLength { get; set; }
        public byte[] Data { get; set; }
        public byte TypeID { get; set; }

        public RelData() { }
        public RelData(RelData d)
        {
            NameHash = d.NameHash;
            Name = d.Name;
            DataOffset = d.DataOffset;
            DataLength = d.DataLength;
            Data = d.Data;
            TypeID = d.TypeID;
        }

        public void ReadType(BinaryReader br)
        {
            TypeID = br.ReadByte();
        }

        public string GetNameString()
        {
            return (string.IsNullOrEmpty(Name)) ? NameHash.ToString() : Name;
        }
        public string GetBaseString()
        {
            return DataOffset.ToString() + ", " + DataLength.ToString() + ": " + GetNameString();
        }
        public override string ToString()
        {
            return GetBaseString() + ": " + TypeID.ToString();
        }

        public static bool Bit(uint f, int b)
        {
            return ((f & (1u << b)) != 0); //just for handyness... maybe move this?
        }
        public static bool BadF(float f)
        {
            return ((f < -15000) || (f > 15000));
        }
    }

    [TC(typeof(EXP))] public class RelSoundHeader
    {
        public FlagsUint Flags { get; set; }

        public FlagsUint UnkFlags { get; set; }
        public ushort Unk01 { get; set; }
        public ushort Unk02 { get; set; }
        public ushort Unk03 { get; set; } //0xD-0xF
        public ushort Unk04 { get; set; } //0xF-0x11
        public ushort Unk05 { get; set; } //0x11-0x13
        public ushort Unk06 { get; set; } //0x13-0x15
        public ushort Unk07 { get; set; } //0x15-0x17
        public ushort Unk08 { get; set; } //0x17-0x19
        public ushort Unk09 { get; set; } //0x19-0x1B
        public MetaHash UnkHash1 { get; set; } //0x1B-0x1F
        public MetaHash UnkHash2 { get; set; } //0x1F-0x23
        public ushort Unk10 { get; set; } //0x23-0x25
        public ushort Unk11 { get; set; } //0x25-0x27
        public ushort Unk12 { get; set; } //0x27-0x29
        public MetaHash CategoryHash { get; set; } //0x29-0x2D
        public ushort Unk14 { get; set; } //0x2D-0x2F
        public ushort Unk15 { get; set; } //0x2F-0x31
        public ushort Unk16 { get; set; } //0x31-0x33
        public ushort Unk17 { get; set; } //0x33-0x35
        public MetaHash UnkHash3 { get; set; } //0x35-0x39
        public ushort Unk18 { get; set; } //0x39-0x3B
        public byte Unk19 { get; set; } //0x3B-0x3C
        public byte Unk20 { get; set; } //0x3C-0x3D
        public byte Unk21 { get; set; } //0x3D-0x3E
        public MetaHash UnkHash4 { get; set; } //0x3E-0x42
        public MetaHash UnkHash5 { get; set; } //0x42-0x46
        public ushort Unk22 { get; set; } //0x46-0x48
        public ushort Unk23 { get; set; } //0x48-0x4A
        public ushort Unk24 { get; set; } //0x4A-0x4C

        public ushort Unk25 { get; set; } //0x4A-0x4C
        public ushort Unk26 { get; set; } //0x4A-0x4C


        public RelSoundHeader(BinaryReader br)
        {
            Flags = br.ReadUInt32();


            //if (Flags.Value != 0xAAAAAAAA)
            if ((Flags.Value & 0xFF) != 0xAA)
            {
                if (Bit(0)) UnkFlags = br.ReadUInt32();
                if (Bit(1)) Unk01 = br.ReadUInt16();
                if (Bit(2)) Unk02 = br.ReadUInt16();
                if (Bit(3)) Unk03 = br.ReadUInt16();
                if (Bit(4)) Unk04 = br.ReadUInt16();
                if (Bit(5)) Unk05 = br.ReadUInt16();
                if (Bit(6)) Unk06 = br.ReadUInt16();
                if (Bit(7)) Unk07 = br.ReadUInt16();
            }
            if ((Flags.Value & 0xFF00) != 0xAA00)
            {
                if (Bit(8)) Unk08 = br.ReadUInt16();
                if (Bit(9)) Unk09 = br.ReadUInt16();
                if (Bit(10)) UnkHash1 = br.ReadUInt32();
                if (Bit(11)) UnkHash2 = br.ReadUInt32();
                if (Bit(12)) Unk10 = br.ReadUInt16();
                if (Bit(13)) Unk11 = br.ReadUInt16();
                if (Bit(14)) Unk12 = br.ReadUInt16();
                if (Bit(15)) CategoryHash = br.ReadUInt32();
            }
            if ((Flags.Value & 0xFF0000) != 0xAA0000)
            {
                if (Bit(16)) Unk14 = br.ReadUInt16();
                if (Bit(17)) Unk15 = br.ReadUInt16();
                if (Bit(18)) Unk16 = br.ReadUInt16();
                if (Bit(19)) Unk17 = br.ReadUInt16();
                if (Bit(20)) UnkHash3 = br.ReadUInt32();
                if (Bit(21)) Unk18 = br.ReadUInt16();
                if (Bit(22)) Unk19 = br.ReadByte();
                if (Bit(23)) Unk20 = br.ReadByte();
            }
            if ((Flags.Value & 0xFF000000) != 0xAA000000)
            {
                if (Bit(24)) Unk21 = br.ReadByte();
                if (Bit(25)) UnkHash4 = br.ReadUInt32();
                if (Bit(26)) UnkHash5 = br.ReadUInt32();
                if (Bit(27)) Unk22 = br.ReadUInt16();
                if (Bit(28)) Unk23 = br.ReadUInt16();
                if (Bit(29)) Unk24 = br.ReadUInt16();
                if (Bit(30)) Unk25 = br.ReadUInt16(); //maybe not
                if (Bit(31)) Unk26 = br.ReadUInt16(); //maybe not
            }
        }

        private bool Bit(int b)
        {
            return ((Flags.Value & (1u << b)) != 0);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}, {3}, {4}, {5}, {6}, {7}", Flags.Hex, UnkFlags.Hex, CategoryHash, UnkHash1, UnkHash2, UnkHash3, UnkHash4, UnkHash5);
        }
    }

    [TC(typeof(EXP))] public class RelSound : RelData
    {
        public RelSoundHeader Header { get; set; }
        public byte AudioTracksCount { get; set; }
        public RelData[] AudioTracks { get; set; }
        public MetaHash[] AudioTrackHashes { get; set; }
        public MetaHash[] AudioContainers { get; set; } //Relative path to parent wave container (i.e. "RESIDENT/animals")

        public RelSound(RelData d, BinaryReader br) : base(d)
        {
            Header = new RelSoundHeader(br);
        }

        public void ReadAudioTrackHashes(BinaryReader br)
        {
            AudioTracksCount = br.ReadByte();
            AudioTrackHashes = new MetaHash[AudioTracksCount];
            for (int i = 0; i < AudioTracksCount; i++)
            {
                AudioTrackHashes[i] = br.ReadUInt32();
            }
        }
    }



    public enum Dat54SoundType : byte
    {
        LoopingSound = 1,
        EnvelopeSound = 2,
        TwinLoopSound = 3,
        SpeechSound = 4,
        OnStopSound = 5,
        WrapperSound = 6,
        SequentialSound = 7,
        StreamingSound = 8,
        RetriggeredOverlappedSound = 9,
        CrossfadeSound = 10,
        CollapsingStereoSound = 11,
        SimpleSound = 12,
        MultitrackSound = 13,
        RandomizedSound = 14,
        EnvironmentSound = 15,
        DynamicEntitySound = 16,
        SequentialOverlapSound = 17,
        ModularSynthSound = 18,
        GranularSound = 19,
        DirectionalSound = 20,
        KineticSound = 21,
        SwitchSound = 22,
        VariableCurveSound = 23,
        VariablePrintValueSound = 24,
        VariableBlockSound = 25,
        IfSound = 26,
        MathOperationSound = 27,
        ParameterTransformSound = 28,
        FluctuatorSound = 29,
        AutomationSound = 30,
        ExternalStreamSound = 31,
        SoundSet = 32,
        Unknown = 33,
        Unknown2 = 34,
        SoundList = 35
    }

    [TC(typeof(EXP))] public class Dat54Sound : RelSound
    {
        public Dat54SoundType Type { get; set; }

        public Dat54Sound(RelData d, BinaryReader br) : base(d, br)
        {
            Type = (Dat54SoundType)TypeID;
        }

        public override string ToString()
        {
            return GetBaseString() + ": " + Type.ToString();
        }
    }

    [TC(typeof(EXP))] public class Dat54LoopingSound : Dat54Sound
    {
        public short UnkShort0 { get; set; } //0x0-0x2
        public short UnkShort1 { get; set; } //0x2-0x4
        public short UnkShort2 { get; set; } //0x4-0x6
        public MetaHash AudioHash { get; set; } //0x6-0xA
        public MetaHash ParameterHash { get; set; } //0xA-0xE

        public Dat54LoopingSound(RelData d, BinaryReader br) : base(d, br)
        {
            UnkShort0 = br.ReadInt16();
            UnkShort1 = br.ReadInt16();
            UnkShort2 = br.ReadInt16();
            AudioHash = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash };
            ParameterHash = br.ReadUInt32();
        }
    }
    [TC(typeof(EXP))] public class Dat54EnvelopeSound : Dat54Sound
    {
        public ushort UnkShortA { get; set; } //0x0-0x2
        public ushort UnkShortA1 { get; set; } //0x2-0x4
        public ushort UnkShortB { get; set; } //0x4-0x6
        public ushort UnkShortB1 { get; set; } //0x6-0x8
        public byte UnkByteA { get; set; } //0x8-0x9
        public byte UnkByteA1 { get; set; } //0x9-0xA
        public int UnkInt { get; set; } //0xA-0xE
        public ushort UnkShortC { get; set; } //0xE-0x10
        public int UnkIntA { get; set; } //0x10-0x14
        public int UnkIntA1 { get; set; } //0x14-0x18
        public MetaHash CurvesUnkHash0 { get; set; } //0x18-0x1C
        public MetaHash CurvesUnkHash1 { get; set; } //0x1C-0x20
        public MetaHash CurvesUnkHash2 { get; set; } //0x20-0x24
        public MetaHash ParameterHash0 { get; set; } //0x24-0x28
        public MetaHash ParameterHash1 { get; set; } //0x28-0x2C
        public MetaHash ParameterHash2 { get; set; } //0x2C-0x30
        public MetaHash ParameterHash3 { get; set; } //0x30-0x34
        public MetaHash ParameterHash4 { get; set; } //0x34-0x38
        public MetaHash AudioHash { get; set; }// audio track 0x38-0x3C
        public int UnkIntC { get; set; } //0x3C-0x40
        public MetaHash ParameterHash5 { get; set; } //0x40-0x44
        public float UnkFloat0 { get; set; } //0x44-0x48
        public float UnkFloat1 { get; set; } //0x48-0x4C

        public Dat54EnvelopeSound(RelData d, BinaryReader br) : base(d, br)
        {
            UnkShortA = br.ReadUInt16(); //0x0-0x2
            UnkShortA1 = br.ReadUInt16(); //0x2-0x4
            UnkShortB = br.ReadUInt16(); //0x4-0x6
            UnkShortB1 = br.ReadUInt16(); //0x6-0x8
            UnkByteA = br.ReadByte(); //0x8-0x9
            UnkByteA1 = br.ReadByte(); //0x9-0xA
            UnkInt = br.ReadInt32(); //0xA-0xE
            UnkShortC = br.ReadUInt16(); //0xE-0x10
            UnkIntA = br.ReadInt32(); //0x10-0x14
            UnkIntA1 = br.ReadInt32(); //0x14-0x18
            CurvesUnkHash0 = br.ReadUInt32(); //0x18-0x1C
            CurvesUnkHash1 = br.ReadUInt32(); //0x1C-0x20
            CurvesUnkHash2 = br.ReadUInt32(); //0x20-0x24
            ParameterHash0 = br.ReadUInt32(); //0x24-0x28
            ParameterHash1 = br.ReadUInt32(); //0x28-0x2C
            ParameterHash2 = br.ReadUInt32(); //0x2C-0x30
            ParameterHash3 = br.ReadUInt32(); //0x30-0x34
            ParameterHash4 = br.ReadUInt32(); //0x34-0x38
            AudioHash = br.ReadUInt32(); //0x38-0x3C
            UnkIntC = br.ReadInt32(); //0x3C-0x40
            ParameterHash5 = br.ReadUInt32(); //0x40-0x44
            UnkFloat0 = br.ReadSingle(); //0x44-0x48
            UnkFloat1 = br.ReadSingle(); //0x48-0x4C
            AudioTrackHashes = new[] { AudioHash };
        }
    }
    [TC(typeof(EXP))] public class Dat54TwinLoopSound : Dat54Sound
    {
        public ushort UnkShort0 { get; set; } //0x0-0x2
        public ushort UnkShort1 { get; set; } //0x2-0x4
        public ushort UnkShort2 { get; set; } //0x4-0x6
        public ushort UnkShort3 { get; set; } //0x6-0x8
        public MetaHash UnkHash { get; set; } //0x8-0xC
        public MetaHash ParameterHash0 { get; set; } //0xC-0x10
        public MetaHash ParameterHash1 { get; set; } //0x10-0x14
        public MetaHash ParameterHash2 { get; set; } //0x14-0x18
        public MetaHash ParameterHash3 { get; set; } //0x18-0x1C

        public Dat54TwinLoopSound(RelData d, BinaryReader br) : base(d, br)
        {
            UnkShort0 = br.ReadUInt16();
            UnkShort1 = br.ReadUInt16();
            UnkShort2 = br.ReadUInt16();
            UnkShort3 = br.ReadUInt16();
            UnkHash = br.ReadUInt32();
            ParameterHash0 = br.ReadUInt32();
            ParameterHash1 = br.ReadUInt32();
            ParameterHash2 = br.ReadUInt32();
            ParameterHash3 = br.ReadUInt32();

            ReadAudioTrackHashes(br);
        }
    }
    [TC(typeof(EXP))] public class Dat54SpeechSound : Dat54Sound
    {
        public int UnkInt0 { get; set; } //maybe file index?
        public int UnkInt1 { get; set; } //ox4-0x8
        public MetaHash VoiceDataHash { get; set; } //0x8-0xC
        public string SpeechName { get; set; } //0xD-...

        public Dat54SpeechSound(RelData d, BinaryReader br) : base(d, br)
        {
            UnkInt0 = br.ReadInt32();
            UnkInt1 = br.ReadInt32();
            VoiceDataHash = br.ReadUInt32();
            SpeechName = br.ReadString();
        }
    }
    [TC(typeof(EXP))] public class Dat54OnStopSound : Dat54Sound
    {
        public MetaHash AudioHash0 { get; set; }
        public MetaHash AudioHash1 { get; set; }
        public MetaHash AudioHash2 { get; set; }

        public Dat54OnStopSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash0 = br.ReadUInt32();
            AudioHash1 = br.ReadUInt32();
            AudioHash2 = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash0, AudioHash1, AudioHash2 };
        }
    }
    [TC(typeof(EXP))] public class Dat54WrapperSound : Dat54Sound
    {
        public MetaHash AudioHash0 { get; set; } //0x0-0x4
        public int FrameStartTime { get; set; } //0x4-0x8  // maybe start delay?
        public MetaHash AudioHash1 { get; set; } //0x8-0xC
        public short FrameTimeInterval { get; set; } //0xC-0xE  // [camxx:] My guess is that this is related to the time at which a child sound should start playin (or the length of the sound).
        public byte ItemCount { get; set; }
        public MetaHash[] Variables { get; set; } //0xF
        public byte[] UnkByteData { get; set; } // ...

        public Dat54WrapperSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash0 = br.ReadUInt32();
            FrameStartTime = br.ReadInt32();
            AudioHash1 = br.ReadUInt32();
            FrameTimeInterval = br.ReadInt16();
            ItemCount = br.ReadByte();
            Variables = new MetaHash[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                Variables[i] = br.ReadUInt32();
            }
            UnkByteData = br.ReadBytes(ItemCount);

            AudioTrackHashes = new[] { AudioHash0, AudioHash1 };
        }
    }
    [TC(typeof(EXP))] public class Dat54SequentialSound : Dat54Sound
    {
        public Dat54SequentialSound(RelData d, BinaryReader br) : base(d, br)
        {
            ReadAudioTrackHashes(br);
        }
    }
    [TC(typeof(EXP))] public class Dat54StreamingSound : Dat54Sound
    {
        int UnkInt { get; set; } //0x0-0x4

        public Dat54StreamingSound(RelData d, BinaryReader br) : base(d, br)
        {
            UnkInt = br.ReadInt32();

            ReadAudioTrackHashes(br);
        }
    }
    [TC(typeof(EXP))] public class Dat54RetriggeredOverlappedSound : Dat54Sound
    {
        public ushort UnkShort0 { get; set; } //0x0-0x2
        public ushort UnkShort1 { get; set; } //0x2-0x4
        public ushort UnkShort2 { get; set; } //0x4-0x6
        public ushort UnkShort3 { get; set; } // 0x6-0x8
        public MetaHash ParameterHash0 { get; set; } //0x8-0xC
        public MetaHash ParameterHash1 { get; set; } //0xC-0x10
        public MetaHash AudioHash0 { get; set; }
        public MetaHash AudioHash1 { get; set; }
        public MetaHash AudioHash2 { get; set; }

        public Dat54RetriggeredOverlappedSound(RelData d, BinaryReader br) : base(d, br)
        {
            UnkShort0 = br.ReadUInt16();
            UnkShort1 = br.ReadUInt16();
            UnkShort2 = br.ReadUInt16();
            UnkShort3 = br.ReadUInt16();
            ParameterHash0 = br.ReadUInt32();
            ParameterHash1 = br.ReadUInt32();
            AudioHash0 = br.ReadUInt32();
            AudioHash1 = br.ReadUInt32();
            AudioHash2 = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash0, AudioHash1, AudioHash2 };
        }
    }
    [TC(typeof(EXP))] public class Dat54CrossfadeSound : Dat54Sound
    {
        public MetaHash AudioHash0 { get; set; }
        public MetaHash AudioHash1 { get; set; }
        public byte UnkByte { get; set; } //0x8-0x9
        public float UnkFloat0 { get; set; } //0x9-0xD
        public float UnkFloat1 { get; set; } //0xD-0x11
        public int UnkInt2 { get; set; } //0xD-0x15
        public MetaHash UnkCurvesHash { get; set; } //0x15-0x19
        public MetaHash ParameterHash0 { get; set; } //0x19-0x1D
        public MetaHash ParameterHash1 { get; set; } //0x1D-0x21
        public MetaHash ParameterHash2 { get; set; } //0x21-0x25
        public MetaHash ParameterHash3 { get; set; } //0x25-0x29
        public MetaHash ParameterHash4 { get; set; } //0x29-0x2D

        public Dat54CrossfadeSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash0 = br.ReadUInt32();
            AudioHash1 = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash0, AudioHash1 };
            UnkByte = br.ReadByte();
            UnkFloat0 = br.ReadSingle();
            UnkFloat1 = br.ReadSingle();
            UnkInt2 = br.ReadInt32();
            UnkCurvesHash = br.ReadUInt32();
            ParameterHash0 = br.ReadUInt32();
            ParameterHash1 = br.ReadUInt32();
            ParameterHash2 = br.ReadUInt32();
            ParameterHash3 = br.ReadUInt32();
            ParameterHash4 = br.ReadUInt32();
        }
    }
    [TC(typeof(EXP))] public class Dat54CollapsingStereoSound : Dat54Sound
    {
        public MetaHash AudioHash0 { get; set; }
        public MetaHash AudioHash1 { get; set; }
        public float UnkFloat0 { get; set; }
        public float UnkFloat1 { get; set; }
        public MetaHash ParameterHash0 { get; set; } //0x10-0x14
        public MetaHash ParameterHash1 { get; set; } //0x14-0x18
        public MetaHash ParameterHash2 { get; set; } //0x18-0x1C
        public MetaHash ParameterHash3 { get; set; } //0x1C-0x20
        public MetaHash ParameterHash4 { get; set; } //0x20-0x24
        public MetaHash ParameterHash5 { get; set; } //0x28-0x2C
        public int UnkInt { get; set; } //0x24-0x28
        public byte UnkByte { get; set; } //0x2c-0x2D

        public Dat54CollapsingStereoSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash0 = br.ReadUInt32();
            AudioHash1 = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash0, AudioHash1 };
            UnkFloat0 = br.ReadSingle(); //0x8
            UnkFloat1 = br.ReadSingle(); //0xC
            ParameterHash0 = br.ReadUInt32(); //0x10
            ParameterHash1 = br.ReadUInt32(); //0x14
            ParameterHash2 = br.ReadUInt32(); //0x18
            ParameterHash3 = br.ReadUInt32(); //0x1C
            ParameterHash4 = br.ReadUInt32(); //0x20
            UnkInt = br.ReadInt32(); //0x24-0x28
            ParameterHash5 = br.ReadUInt32(); //0x28-0x2C
            UnkByte = br.ReadByte(); //0x2C-0x2D
        }
    }
    [TC(typeof(EXP))] public class Dat54SimpleSound : Dat54Sound
    {
        public MetaHash ContainerName { get; set; } //Relative path to parent wave container (i.e. "RESIDENT/animals")
        public MetaHash FileName { get; set; } //Name of the .wav file
        public byte WaveSlotNum { get; set; } //Internal index of wave (.awc) container

        public Dat54SimpleSound(RelData d, BinaryReader br) : base(d, br)
        {
            ContainerName = br.ReadUInt32();
            AudioContainers = new[] { ContainerName };
            FileName = br.ReadUInt32();
            WaveSlotNum = br.ReadByte();
        }
    }
    [TC(typeof(EXP))] public class Dat54MultitrackSound : Dat54Sound
    {
        public Dat54MultitrackSound(RelData d, BinaryReader br) : base(d, br)
        {
            ReadAudioTrackHashes(br);
        }
    }
    [TC(typeof(EXP))] public class Dat54RandomizedSound : Dat54Sound
    {
        public byte UnkByte { get; set; } //0x0-0x1 something count?
        public byte UnkBytesCount { get; set; } //0x1-0x2
        public byte[] UnkBytes { get; set; }
        public byte ItemCount { get; set; }
        public float[] AudioTrackUnkFloats { get; set; } //probability..?

        public Dat54RandomizedSound(RelData d, BinaryReader br) : base(d, br)
        {
            UnkByte = br.ReadByte();
            UnkBytesCount = br.ReadByte();
            UnkBytes = br.ReadBytes(UnkBytesCount);
            ItemCount = br.ReadByte();
            AudioTrackHashes = new MetaHash[ItemCount];
            AudioTrackUnkFloats = new float[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                AudioTrackHashes[i] = br.ReadUInt32();
                AudioTrackUnkFloats[i] = br.ReadSingle();
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54EnvironmentSound : Dat54Sound
    {
        public byte UnkByte { get; set; }

        public Dat54EnvironmentSound(RelData d, BinaryReader br) : base(d, br)
        {
            UnkByte = br.ReadByte();
        }
    }
    [TC(typeof(EXP))] public class Dat54DynamicEntitySound : Dat54Sound
    {
        public byte ItemCount { get; set; }
        public MetaHash[] UnkHashes { get; set; }

        public Dat54DynamicEntitySound(RelData d, BinaryReader br) : base(d, br)
        {
            ItemCount = br.ReadByte();
            UnkHashes = new MetaHash[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                UnkHashes[i] = br.ReadUInt32();
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54SequentialOverlapSound : Dat54Sound
    {
        public ushort UnkShort { get; set; }
        public MetaHash ParameterHash0 { get; set; } //0x2-0x6
        public MetaHash ParameterHash1 { get; set; } //0x6-0xA

        public Dat54SequentialOverlapSound(RelData d, BinaryReader br) : base(d, br)
        {
            UnkShort = br.ReadUInt16();
            ParameterHash0 = br.ReadUInt32();
            ParameterHash1 = br.ReadUInt32();

            ReadAudioTrackHashes(br);
        }
    }
    [TC(typeof(EXP))] public class Dat54ModularSynthSound : Dat54Sound
    {
        public MetaHash OptAmpUnkHash { get; set; } //0x0-0x4
        public MetaHash UnkHash { get; set; } //0x4-0x8
        public float UnkFloat { get; set; } //0x8-0xC
        public int UnkInt { get; set; } //0xC-0x10
        public int TrackCount { get; set; }
        public int UnkItemCount { get; set; }
        public Dat54ModularSynthSoundData[] UnkItems { get; set; } //0x28-..

        public Dat54ModularSynthSound(RelData d, BinaryReader br) : base(d, br)
        {
            OptAmpUnkHash = br.ReadUInt32(); //0x0-0x4
            UnkHash = br.ReadUInt32(); //0x4-0x8
            UnkFloat = br.ReadSingle(); //0x8-0xC
            UnkInt = br.ReadInt32(); //0xC-0x10
            TrackCount = br.ReadInt32(); //0x10-0x14
            AudioTrackHashes = new MetaHash[4];
            for (int i = 0; i < 4; i++)
            {
                AudioTrackHashes[i] = br.ReadUInt32();
            }
            UnkItemCount = br.ReadInt32();
            UnkItems = new Dat54ModularSynthSoundData[UnkItemCount];
            for (int i = 0; i < UnkItemCount; i++)
            {
                UnkItems[i] = new Dat54ModularSynthSoundData(br);
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54ModularSynthSoundData
    {
        public MetaHash UnkHash { get; set; }
        public MetaHash ParameterHash { get; set; }
        public float Value { get; set; }

        public Dat54ModularSynthSoundData(BinaryReader br)
        {
            UnkHash = br.ReadUInt32();
            ParameterHash = br.ReadUInt32();
            Value = br.ReadSingle();
        }

        public override string ToString()
        {
            return UnkHash.ToString() + ": " + ParameterHash.ToString() + ": " + FloatUtil.ToString(Value);
        }
    }
    [TC(typeof(EXP))] public class Dat54GranularSound : Dat54Sound
    {
        public int WaveSlotIndex { get; set; } //0x0-0x4
        public Dat54GranularSoundFile Wave1 { get; set; }
        public Dat54GranularSoundFile Wave2 { get; set; }
        public Dat54GranularSoundFile Wave3 { get; set; }
        public Dat54GranularSoundFile Wave4 { get; set; }
        public Dat54GranularSoundFile Wave5 { get; set; }
        public Dat54GranularSoundFile Wave6 { get; set; }
        public Dat54GranularSoundData DataItem1 { get; set; } //0x34-0x3C
        public Dat54GranularSoundData DataItem2 { get; set; } //0x3C-0x44
        public Dat54GranularSoundData DataItem3 { get; set; } //0x44-0x4C
        public Dat54GranularSoundData DataItem4 { get; set; } //0x4C-0x54
        public Dat54GranularSoundData DataItem5 { get; set; } //0x54-0x5C
        public Dat54GranularSoundData DataItem6 { get; set; } //0x5C-0x64
        public int UnkInt0 { get; set; } //0x64-0x68
        public int UnkInt1 { get; set; } //0x68-0x6C
        public ushort UnkShort0 { get; set; } //0x6C-0x6E
        public ushort UnkShort1 { get; set; } //0x6E-0x70
        public ushort UnkShort2 { get; set; } //0x70-0x72
        public ushort UnkShort3 { get; set; } //0x72-0x74
        public ushort UnkShort4 { get; set; } //0x74-0x76
        public ushort UnkShort5 { get; set; } //0x76-0x78
        public MetaHash TrackName { get; set; } //0x78-0x7C
        public byte UnkVecCount { get; set; } //0x7C-0x7D
        public Vector2[] UnkVecData { get; set; } //0x7D-...

        public Dat54GranularSound(RelData d, BinaryReader br) : base(d, br)
        {
            WaveSlotIndex = br.ReadInt32();

            Wave1 = new Dat54GranularSoundFile(br);
            Wave2 = new Dat54GranularSoundFile(br);
            Wave3 = new Dat54GranularSoundFile(br);
            Wave4 = new Dat54GranularSoundFile(br);
            Wave5 = new Dat54GranularSoundFile(br);
            Wave6 = new Dat54GranularSoundFile(br);

            AudioContainers = new[] {
                Wave1.ContainerName,
                Wave2.ContainerName,
                Wave3.ContainerName,
                Wave4.ContainerName,
                Wave5.ContainerName,
                Wave6.ContainerName
            };

            DataItem1 = new Dat54GranularSoundData(br);
            DataItem2 = new Dat54GranularSoundData(br);
            DataItem3 = new Dat54GranularSoundData(br);
            DataItem4 = new Dat54GranularSoundData(br);
            DataItem5 = new Dat54GranularSoundData(br);
            DataItem6 = new Dat54GranularSoundData(br);

            UnkInt0 = br.ReadInt32();
            UnkInt1 = br.ReadInt32();
            UnkShort0 = br.ReadUInt16();
            UnkShort1 = br.ReadUInt16();
            UnkShort2 = br.ReadUInt16();
            UnkShort3 = br.ReadUInt16();
            UnkShort4 = br.ReadUInt16();
            UnkShort5 = br.ReadUInt16();

            TrackName = br.ReadUInt32();

            AudioTrackHashes = new[] { TrackName };

            UnkVecCount = br.ReadByte();
            UnkVecData = new Vector2[UnkVecCount];
            for (int i = 0; i < UnkVecCount; i++)
            {
                UnkVecData[i] = new Vector2(br.ReadSingle(), br.ReadSingle());
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54GranularSoundFile
    {
        public MetaHash ContainerName { get; set; } //0x0-0x4
        public MetaHash FileName { get; set; } //0x4-0x8

        public Dat54GranularSoundFile(BinaryReader br)
        {
            ContainerName = br.ReadUInt32();
            FileName = br.ReadUInt32();
        }

        public override string ToString()
        {
            return ContainerName.ToString() + ": " + FileName.ToString();
        }
    }
    [TC(typeof(EXP))] public class Dat54GranularSoundData
    {
        public byte UnkFlags0 { get; set; } //0x0-0x1
        public byte UnkFlags1 { get; set; } //0x1-0x2
        public byte UnkByte0 { get; set; } //0x2-0x3
        public byte UnkByte1 { get; set; } //0x3-0x4
        public float UnkFloat { get; set; } //0x4-0x8

        public Dat54GranularSoundData(BinaryReader br)
        {
            UnkFlags0 = br.ReadByte();
            UnkFlags1 = br.ReadByte();
            UnkByte0 = br.ReadByte();
            UnkByte1 = br.ReadByte();
            UnkFloat = br.ReadSingle();
        }

        public override string ToString()
        {
            return UnkFlags0.ToString() + ": " + UnkFlags1.ToString() + ": " + UnkByte0.ToString() + ": " + UnkByte1.ToString() + ": " + FloatUtil.ToString(UnkFloat);
        }
    }
    [TC(typeof(EXP))] public class Dat54DirectionalSound : Dat54Sound
    {
        public MetaHash AudioHash { get; set; }
        public float UnkFloat0 { get; set; } //0x4-0x8
        public float UnkFloat1 { get; set; } //0x8-0xC
        public float UnkFloat2 { get; set; } //0xC-0x10
        public float UnkFloat3 { get; set; } //0x10-0x14
        public float UnkFloat4 { get; set; } //0x14-0x18

        public Dat54DirectionalSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash };
            UnkFloat0 = br.ReadSingle();
            UnkFloat1 = br.ReadSingle();
            UnkFloat2 = br.ReadSingle();
            UnkFloat3 = br.ReadSingle();
            UnkFloat4 = br.ReadSingle();
        }
    }
    [TC(typeof(EXP))] public class Dat54KineticSound : Dat54Sound
    {
        public MetaHash AudioHash { get; set; }
        public float UnkFloat0 { get; set; } //Maybe kinetic force vector?
        public float UnkFloat1 { get; set; }
        public float UnkFloat2 { get; set; }

        public Dat54KineticSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash };
            UnkFloat0 = br.ReadSingle();
            UnkFloat1 = br.ReadSingle();
            UnkFloat2 = br.ReadSingle();
        }
    }
    [TC(typeof(EXP))] public class Dat54SwitchSound : Dat54Sound
    {
        public MetaHash ParameterHash { get; set; } //0x0-0x4

        public Dat54SwitchSound(RelData d, BinaryReader br) : base(d, br)
        {
            ParameterHash = br.ReadUInt32();

            ReadAudioTrackHashes(br);
        }
    }
    [TC(typeof(EXP))] public class Dat54VariableCurveSound : Dat54Sound
    {
        public MetaHash AudioHash { get; set; }
        public MetaHash ParameterHash0 { get; set; } //0x4-0x8
        public MetaHash ParameterHash1 { get; set; } //0x8-0xC
        public MetaHash UnkCurvesHash { get; set; } //0xC-0x10

        public Dat54VariableCurveSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash };
            ParameterHash0 = br.ReadUInt32();
            ParameterHash1 = br.ReadUInt32();
            UnkCurvesHash = br.ReadUInt32();
        }
    }
    [TC(typeof(EXP))] public class Dat54VariablePrintValueSound : Dat54Sound
    {
        public MetaHash ParameterHash { get; set; } //0x0-0x4
        public string VariableString { get; set; }

        public Dat54VariablePrintValueSound(RelData d, BinaryReader br) : base(d, br)
        {
            ParameterHash = br.ReadUInt32();
            VariableString = br.ReadString();
        }
    }
    [TC(typeof(EXP))] public class Dat54VariableBlockSound : Dat54Sound
    {
        public MetaHash AudioHash { get; set; }
        public byte VariableCount { get; set; }
        public Dat54VariableData[] Variables { get; set; }

        public Dat54VariableBlockSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash };
            VariableCount = br.ReadByte();
            Variables = new Dat54VariableData[VariableCount];
            for (int i = 0; i < VariableCount; i++)
            {
                Variables[i] = new Dat54VariableData(br);
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54VariableData
    {
        public MetaHash Name { get; set; }
        public float Value { get; set; }
        public float UnkFloat { get; set; }
        public byte Flags { get; set; }

        public Dat54VariableData(BinaryReader br)
        {
            Name = br.ReadUInt32();
            Value = br.ReadSingle();
            UnkFloat = br.ReadSingle();
            Flags = br.ReadByte();
        }

        public override string ToString()
        {
            return Name + ": " + FloatUtil.ToString(Value) + ": " + FloatUtil.ToString(UnkFloat) + ": " + Flags.ToString();
        }
    }
    [TC(typeof(EXP))] public class Dat54IfSound : Dat54Sound
    {
        public MetaHash AudioHash1 { get; set; }
        public MetaHash AudioHash2 { get; set; }
        public MetaHash ParameterHash1 { get; set; }
        public byte UnkByte { get; set; }
        public float UnkFloat { get; set; }
        public MetaHash ParameterHash2 { get; set; }

        public Dat54IfSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash1 = br.ReadUInt32();
            AudioHash2 = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash1, AudioHash2 };
            ParameterHash1 = br.ReadUInt32();
            UnkByte = br.ReadByte();
            UnkFloat = br.ReadSingle();
            ParameterHash2 = br.ReadUInt32();
        }
    }
    [TC(typeof(EXP))] public class Dat54MathOperationSound : Dat54Sound
    {
        public MetaHash AudioHash { get; set; }
        public byte UnkDataCount { get; set; }
        public Dat54MathOperationSoundData[] UnkData { get; set; }

        public Dat54MathOperationSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash };
            UnkDataCount = br.ReadByte();
            UnkData = new Dat54MathOperationSoundData[UnkDataCount];
            for (int i = 0; i < UnkDataCount; i++)
            {
                UnkData[i] = new Dat54MathOperationSoundData(br);
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54MathOperationSoundData
    {
        public byte UnkByte { get; set; } //0x0-0x1
        public int UnkInt0 { get; set; } //0x1-0x5
        public int UnkInt1 { get; set; } //0x5-0x9
        public int UnkInt2 { get; set; } //0x9-0xD
        public int UnkInt3 { get; set; } //0xD-0x11
        public int UnkInt4 { get; set; } //0x11-0x15
        public MetaHash ParameterHash0 { get; set; } //0x15-0x19
        public MetaHash ParameterHash1 { get; set; } //0x19-0x1D

        public Dat54MathOperationSoundData(BinaryReader br)
        {
            UnkByte = br.ReadByte();
            UnkInt0 = br.ReadInt32();
            UnkInt1 = br.ReadInt32();
            UnkInt2 = br.ReadInt32();
            UnkInt3 = br.ReadInt32();
            UnkInt4 = br.ReadInt32();
            ParameterHash0 = br.ReadUInt32();
            ParameterHash1 = br.ReadUInt32();
        }

        public override string ToString()
        {
            return ParameterHash0.ToString() + ", " + ParameterHash1.ToString();
        }
    }
    [TC(typeof(EXP))] public class Dat54ParameterTransformSound : Dat54Sound
    {
        public MetaHash AudioHash { get; set; }
        public int ItemCount { get; set; }
        public Dat54ParameterTransformSoundData[] Items { get; set; }

        public Dat54ParameterTransformSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash };
            ItemCount = br.ReadInt32(); //0x4-0x8
            Items = new Dat54ParameterTransformSoundData[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                Items[i] = new Dat54ParameterTransformSoundData(br);
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54ParameterTransformSoundData
    {
        public MetaHash ParameterHash { get; set; } //0x0-0x4
        public float UnkFloat0 { get; set; } //0x4-0x8
        public float UnkFloat1 { get; set; } //0x8-0xC
        public int NestedDataCount { get; set; }
        public Dat54ParameterTransformSoundData2[] NestedData { get; set; } //0x10..

        public Dat54ParameterTransformSoundData(BinaryReader br)
        {
            ParameterHash = br.ReadUInt32();
            UnkFloat0 = br.ReadSingle();
            UnkFloat1 = br.ReadSingle();
            NestedDataCount = br.ReadInt32();
            NestedData = new Dat54ParameterTransformSoundData2[NestedDataCount];
            for (int i = 0; i < NestedDataCount; i++)
            {
                NestedData[i] = new Dat54ParameterTransformSoundData2(br);
            }
        }

        public override string ToString()
        {
            return ParameterHash.ToString() + ", " + NestedDataCount.ToString();
        }
    }
    [TC(typeof(EXP))] public class Dat54ParameterTransformSoundData2
    {
        public float UnkFloat0 { get; set; } //0x0-0x4
        public int UnkInt { get; set; } //0x4
        public MetaHash ParameterHash { get; set; } //0x8-0xC
        public float UnkFloat1 { get; set; } //0xC
        public float UnkFloat2 { get; set; } //0x10-0x14
        public int NestedItemCount { get; set; }
        public Vector2[] NestedItems { get; set; } //0x18-...

        public Dat54ParameterTransformSoundData2(BinaryReader br)
        {
            UnkFloat0 = br.ReadSingle();
            UnkInt = br.ReadInt32();
            ParameterHash = br.ReadUInt32();
            UnkFloat1 = br.ReadSingle();
            UnkFloat2 = br.ReadSingle();
            NestedItemCount = br.ReadInt32();
            NestedItems = new Vector2[NestedItemCount];
            for (int i = 0; i < NestedItemCount; i++)
            {
                NestedItems[i] = new Vector2(br.ReadSingle(), br.ReadSingle());
            }
        }

        public override string ToString()
        {
            return ParameterHash.ToString() + ", " + NestedItemCount.ToString();
        }
    }
    [TC(typeof(EXP))] public class Dat54FluctuatorSound : Dat54Sound
    {
        public MetaHash AudioHash { get; set; }
        public int ItemCount { get; set; }
        public Dat54FluctuatorSoundData[] Items { get; set; }

        public Dat54FluctuatorSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash };
            ItemCount = br.ReadInt32(); //0x4-0x8
            Items = new Dat54FluctuatorSoundData[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                Items[i] = new Dat54FluctuatorSoundData(br);
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54FluctuatorSoundData
    {
        public byte UnkByte0 { get; set; } //0x0-0x1
        public byte UnkByte1 { get; set; } //0x1-0x2
        public MetaHash ParameterHash { get; set; } //0x2-0x6
        public float UnkFloat00 { get; set; } //0x6-0xA
        public float UnkFloat01 { get; set; } //0xA-0xE
        public float UnkFloat02 { get; set; } //0xE-0x12
        public float UnkFloat03 { get; set; } //0x12-0x16
        public float UnkFloat04 { get; set; } //0x16-0x1A
        public float UnkFloat05 { get; set; } //0x1A-0x1E
        public float UnkFloat06 { get; set; } //0x1E-0x22
        public float UnkFloat07 { get; set; } //0x22-0x26
        public float UnkFloat08 { get; set; } //0x26-0x2A
        public float UnkFloat09 { get; set; } //0x2A-0x2E
        public float UnkFloat10 { get; set; } //0x2E-0x32

        public Dat54FluctuatorSoundData(BinaryReader br)
        {
            UnkByte0 = br.ReadByte();
            UnkByte1 = br.ReadByte();
            ParameterHash = br.ReadUInt32();
            UnkFloat00 = br.ReadSingle();
            UnkFloat01 = br.ReadSingle();
            UnkFloat02 = br.ReadSingle();
            UnkFloat03 = br.ReadSingle();
            UnkFloat04 = br.ReadSingle();
            UnkFloat05 = br.ReadSingle();
            UnkFloat06 = br.ReadSingle();
            UnkFloat07 = br.ReadSingle();
            UnkFloat08 = br.ReadSingle();
            UnkFloat09 = br.ReadSingle();
            UnkFloat10 = br.ReadSingle();
        }

        public override string ToString()
        {
            return ParameterHash.ToString();
        }
    }
    [TC(typeof(EXP))] public class Dat54AutomationSound : Dat54Sound
    {
        public MetaHash AudioHash0 { get; set; }
        public float UnkFloat0 { get; set; } //0x4-0x8
        public float UnkFloat1 { get; set; } //0x8-0xC
        public MetaHash ParameterHash { get; set; } //0xC-0x10
        public MetaHash AudioHash1 { get; set; }
        public int WaveSlotId { get; set; } //0x14-0x18
        public MetaHash UnkHash1 { get; set; } //0x18-0x1C
        public int UnkDataCount { get; set; } // array data count 0x1C-0x20
        public Dat54AutomationSoundData[] UnkData { get; set; } //0x20-

        public Dat54AutomationSound(RelData d, BinaryReader br) : base(d, br)
        {
            AudioHash0 = br.ReadUInt32();
            UnkFloat0 = br.ReadSingle();
            UnkFloat1 = br.ReadSingle();
            ParameterHash = br.ReadUInt32();
            AudioHash1 = br.ReadUInt32();
            AudioTrackHashes = new[] { AudioHash0, AudioHash1 };
            WaveSlotId = br.ReadInt32();
            UnkHash1 = br.ReadUInt32();
            UnkDataCount = br.ReadInt32();
            UnkData = new Dat54AutomationSoundData[UnkDataCount];
            for (int i = 0; i < UnkDataCount; i++)
            {
                UnkData[i] = new Dat54AutomationSoundData(br);
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54AutomationSoundData
    {
        public int UnkInt { get; set; } //0x0-0x1
        public MetaHash UnkHash { get; set; } //0x2-0x6

        public Dat54AutomationSoundData(BinaryReader br)
        {
            UnkInt = br.ReadInt32();
            UnkHash = br.ReadUInt32();
        }

        public override string ToString()
        {
            return UnkInt.ToString() + ", " + UnkHash.ToString();
        }
    }
    [TC(typeof(EXP))] public class Dat54ExternalStreamSound : Dat54Sound
    {
        public Dat54ExternalStreamSound(RelData d, BinaryReader br) : base(d, br)
        {
            ReadAudioTrackHashes(br);

            //FlagsUint u1 = br.ReadUInt32();
            //FlagsUint u2 = br.ReadUInt32();
            //FlagsUint u3 = br.ReadUInt32();
            //FlagsUint u4 = br.ReadUInt32();

            //TODO: could be more to read!
            if (br.BaseStream.Position != br.BaseStream.Length)
            { } //hits here!
        }
    }
    [TC(typeof(EXP))] public class Dat54SoundSet : Dat54Sound
    {
        public int ItemCount { get; set; }
        public Dat54SoundSetItem[] Items { get; set; }

        public Dat54SoundSet(RelData d, BinaryReader br) : base(d, br)
        {
            ItemCount = br.ReadInt32();
            Items = new Dat54SoundSetItem[ItemCount];
            AudioTrackHashes = new MetaHash[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                Items[i] = new Dat54SoundSetItem(br);
                AudioTrackHashes[i] = Items[i].SoundName;
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54SoundSetItem
    {
        public MetaHash ScriptName { get; set; }
        public MetaHash SoundName { get; set; }

        public Dat54SoundSetItem(BinaryReader br)
        {
            ScriptName = br.ReadUInt32();
            SoundName = br.ReadUInt32();
        }

        public override string ToString()
        {
            return ScriptName.ToString() + ": " + SoundName.ToString();
        }
    }
    [TC(typeof(EXP))] public class Dat54UnknownSound : Dat54Sound
    {
        public byte UnkDataCount { get; set; }
        public Dat54UnknownSoundData[] UnkData { get; set; }

        public Dat54UnknownSound(RelData d, BinaryReader br) : base(d, br)
        {
            UnkDataCount = br.ReadByte();
            UnkData = new Dat54UnknownSoundData[UnkDataCount];
            AudioTrackHashes = new MetaHash[UnkDataCount];
            for (int i = 0; i < UnkDataCount; i++)
            {
                UnkData[i] = new Dat54UnknownSoundData(br);
                AudioTrackHashes[i] = br.ReadUInt32();
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54UnknownSoundData
    {
        public byte UnkByte0 { get; set; }
        public byte UnkByte1 { get; set; }
        public byte UnkByte2 { get; set; }

        public Dat54UnknownSoundData(BinaryReader br)
        {
            UnkByte0 = br.ReadByte();
            UnkByte1 = br.ReadByte();
            UnkByte2 = br.ReadByte();
        }

        public override string ToString()
        {
            return UnkByte0.ToString() + ": " + UnkByte1.ToString() + ": " + UnkByte2.ToString();
        }
    }
    [TC(typeof(EXP))] public class Dat54UnknownSound2 : Dat54Sound
    {
        public uint UnkCount { get; set; }
        public MetaHash[] UnkItems { get; set; }

        public Dat54UnknownSound2(RelData d, BinaryReader br) : base(d, br)
        {
            UnkCount = br.ReadUInt32();
            UnkItems = new MetaHash[UnkCount];
            for (int i = 0; i < UnkCount; i++)
            {
                UnkItems[i] = br.ReadUInt32();
            }
        }
    }
    [TC(typeof(EXP))] public class Dat54SoundList : Dat54Sound
    {
        public ushort UnkShort { get; set; }
        public uint Count { get; set; }
        public MetaHash[] Items { get; set; }

        public Dat54SoundList(RelData d, BinaryReader br) : base(d, br)
        {
            UnkShort = br.ReadUInt16();
            Count = br.ReadUInt32();
            Items = new MetaHash[Count];
            for (int i = 0; i < Count; i++)
            {
                Items[i] = br.ReadUInt32();
            }
            if (br.BaseStream.Position != br.BaseStream.Length)
            { }
        }
    }





    public enum Dat151RelType : byte //not sure how correct these are?
    {
        Collision = 1, //maybe for vehicle
        Unk2 = 2,
        Vehicle = 3,
        VehicleEngine = 4,
        Entity = 5, //not sure about this
        Stream = 6,//possibly, generic audio stream
        Unk7 = 7,
        Helicopter = 8, //maybe
        Unk9 = 9,
        Unk11 = 11,
        Unk12 = 12,
        Unk13 = 13,
        SpeechParams = 14,
        Unk15 = 15,
        Unk16 = 16,
        Weapon = 17,
        Unk18 = 18,
        Unk22 = 22,
        Unk23 = 23,
        RadioStationsDLC = 24, //
        RadioDLC = 25, 
        DLCMusic = 26,
        Unk27 = 27,
        Unk28 = 28,
        Unk29 = 29,
        PedPVG = 30, //maybe Ped Voice Group?
        Unk31 = 31,
        AmbientEmitterList = 32,
        Unk33 = 33,
        Unk35 = 35,
        Unk36 = 36,
        AmbientZone = 37,
        AmbientEmitter = 38,
        AmbientZoneList = 39,
        Unk40 = 40,
        Unk41 = 41,
        Unk42 = 42,
        Interior = 44,
        Unk45 = 45,
        InteriorRoom = 46,
        Unk47 = 47,
        Unk48 = 48,
        Unk49 = 49,
        WeaponAudioItem = 50,
        Unk51 = 51,
        Mod = 52, //what actually is a "mod" here? a change in some audio settings maybe?
        Unk53 = 53,
        Unk54 = 54,
        Unk55 = 55,
        Unk56 = 56,
        Aeroplane = 57,
        Unk59 = 59,
        Mood = 62,
        StartTrackAction = 63,
        StopTrackAction = 64,
        SetMoodAction = 65,
        PlayerAction = 66,
        StartOneShotAction = 67,
        StopOneShotAction = 68,
        Unk69 = 69,
        Unk70 = 70,
        Unk71 = 71,
        Unk72 = 72,
        AnimalParams = 73,
        Unk74 = 74,
        Unk75 = 75,
        VehicleScannerParams = 76, //maybe not just vehicle
        Unk77 = 77,
        Unk78 = 78,
        Unk79 = 79,
        Unk80 = 80,
        Unk81 = 81,
        Unk82 = 82,
        Unk83 = 83,
        Unk84 = 84,
        Unk85 = 85,
        Unk86 = 86,
        Explosion = 87,
        VehicleEngineGranular = 88, //maybe not just vehicle
        ShoreLinePool = 90,
        ShoreLineLake = 91,
        ShoreLineRiver = 92,
        ShoreLineOcean = 93,
        ShoreLineList = 94,
        Unk95 = 95,
        Unk96 = 96,
        RadioDjSpeechAction = 98,
        Unk99 = 99,
        Unk100 = 100,
        Unk101 = 101,
        FadeOutRadioAction = 102,
        FadeInRadioAction = 103,
        ForceRadioTrackAction = 104,
        Unk105 = 105,
        Unk106 = 106,
        Unk107 = 107,
        Unk108 = 108,
        Unk109 = 109,
        Unk110 = 110,
        Unk111 = 111,
        Unk112 = 112,
        Unk113 = 113,
        Unk114 = 114,
        Unk115 = 115,
        Unk116 = 116,
        Unk117 = 117,
        Unk118 = 118,
        Unk119 = 119,
        Unk120 = 120,
        Unk121 = 121,
    }

    [TC(typeof(EXP))] public class Dat151RelData : RelData
    {
        public Dat151RelType Type { get; set; }


        public static int TotCount = 0; //###############DEBUGG
        public static List<string> FoundCoords = new List<string>(); //###############DEBUGG
        public void RecVec(Vector3 v)
        {
            float tol = 20.0f;
            if ((Math.Abs(v.X)>tol) || (Math.Abs(v.Y)>tol) || (Math.Abs(v.Z)>tol))
            {
                FoundCoords.Add(FloatUtil.GetVector3String(v) + ", " + GetNameString());
            }
        }



        public Dat151RelData() { }
        public Dat151RelData(RelData d, BinaryReader br) : base(d)
        {
            Type = (Dat151RelType)TypeID;
        }

        public override string ToString()
        {
            return GetBaseString() + ": " + Type.ToString();
        }
    }
    [TC(typeof(EXP))] public class Dat151Sound : RelSound
    {
        public Dat151RelType Type { get; set; }

        public Dat151Sound(RelData d, BinaryReader br) : base(d, br)
        {
            Type = (Dat151RelType)TypeID;
        }

        public override string ToString()
        {
            return GetBaseString() + ": " + Type.ToString();
        }
    }

    public enum Dat151ZoneShape : uint
    {
        Box = 0,
        Sphere = 1,
        Line = 2,
    }

    [TC(typeof(EXP))] public class Dat151AmbientEmitterList : Dat151RelData
    {
        public uint UnkOffset0 { get; set; }
        public uint EmitterCount { get; set; }
        public MetaHash[] EmitterHashes { get; set; }

        public Dat151AmbientEmitterList(RelData d, BinaryReader br) : base(d, br)
        {
            br.BaseStream.Position = 0; //1 byte was read already (TypeID)

            UnkOffset0 = ((br.ReadUInt32() >> 8) & 0xFFFFFF);
            EmitterCount = br.ReadUInt32();
            EmitterHashes = new MetaHash[EmitterCount];
            for (int i = 0; i < EmitterCount; i++)
            {
                EmitterHashes[i] = br.ReadUInt32();
            }

            long bytesleft = br.BaseStream.Length - br.BaseStream.Position;
            if (bytesleft != 0)
            { } //no hits here

        }
    }
    [TC(typeof(EXP))] public class Dat151AmbientZone : Dat151RelData
    {
        public uint UnkOffset0 { get; set; }
        public FlagsUint Flags00 { get; set; }
        public Dat151ZoneShape Shape { get; set; }
        public FlagsUint Flags02 { get; set; }
        public Vector3 OuterPos { get; set; }
        public float Unused01 { get; set; }
        public Vector3 OuterSize { get; set; }
        public float Unused02 { get; set; }
        public Vector4 OuterVec1 { get; set; }
        public Vector4 OuterVec2 { get; set; }
        public uint OuterAngle { get; set; }
        public Vector3 OuterVec3 { get; set; }
        public Vector3 InnerPos { get; set; }
        public float Unused06 { get; set; }
        public Vector3 InnerSize { get; set; }
        public float Unused07 { get; set; }
        public Vector4 InnerVec1 { get; set; }
        public Vector4 InnerVec2 { get; set; }
        public uint InnerAngle { get; set; }
        public Vector3 InnerVec3 { get; set; }
        public Vector4 Vec11 { get; set; }
        public Vector4 Vec12 { get; set; }
        public Vector4 Vec13 { get; set; }

        public FlagsUint Flags05 { get; set; }
        public byte Unk14 { get; set; }
        public byte Unk15 { get; set; }
        public ushort HashesCount { get; set; }
        public byte Unk16 { get; set; }
        public MetaHash[] Hashes { get; set; }

        public uint ExtParamsCount { get; set; }
        public ExtParam[] ExtParams { get; set; }
        public struct ExtParam
        {
            public MetaHash Hash { get; set; }
            public float Value { get; set; }
            public ExtParam(BinaryReader br)
            {
                Hash = br.ReadUInt32();
                Value = br.ReadSingle();
            }
            public override string ToString()
            {
                return Hash.ToString() + ": " + FloatUtil.ToString(Value);
            }
        }



        public Dat151AmbientZone(RelData d, BinaryReader br) : base(d, br)
        {
            br.BaseStream.Position = 0; //1 byte was read already (TypeID)

            UnkOffset0 = ((br.ReadUInt32() >> 8) & 0xFFFFFF);
            Flags00 = br.ReadUInt32();
            Shape = (Dat151ZoneShape)br.ReadUInt32();
            Flags02 = br.ReadUInt32();
            OuterPos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Unused01 = br.ReadSingle();
            OuterSize = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Unused02 = br.ReadSingle();
            OuterVec1 = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            OuterVec2 = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            OuterAngle = br.ReadUInt32();//###
            OuterVec3 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            InnerPos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Unused06 = br.ReadSingle();
            InnerSize = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Unused07 = br.ReadSingle();
            InnerVec1 = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            InnerVec2 = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            InnerAngle = br.ReadUInt32();//###
            InnerVec3 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Vec11 = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Vec12 = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Vec13 = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

            Flags05 = br.ReadUInt32();
            Unk14 = br.ReadByte();
            Unk15 = br.ReadByte();
            HashesCount = br.ReadByte();
            Unk16 = br.ReadByte();
            Hashes = new MetaHash[HashesCount];
            for (int i = 0; i < HashesCount; i++)
            {
                Hashes[i] = br.ReadUInt32();
            }

            ExtParamsCount = br.ReadUInt32();
            ExtParams = new ExtParam[ExtParamsCount];
            for (int i = 0; i < ExtParamsCount; i++)
            {
                ExtParams[i] = new ExtParam(br);
            }
            if (ExtParamsCount != 0)
            { }

            var data = this.Data;

            long bytesleft = br.BaseStream.Length - br.BaseStream.Position;
            if (bytesleft != 0)
            {
                //byte[] remainder = br.ReadBytes((int)bytesleft);
                //for (int i = 0; i < remainder.Length; i++)
                //{
                //    if (remainder[i] != 0)
                //    { } //no hits here! probably got everything, i'm assuming the block is padded to 0x10 or something.
                //}
            }


            //RecVec(Pos01);//debug coords output
            //RecVec(Pos06);


            if (Unused01 != 0)
            { }//no hit
            if (Unused02 != 0)
            { }//no hit
            if (Unused06 != 0)
            { }//no hit
            if (Unused07 != 0)
            { }//no hit
            if (Shape != 0)
            { }//eg 1, 2
            if (Flags02.Value != 0)
            { }//no hit
            if (OuterAngle > 360)
            { }//no hit
            if (InnerAngle > 360)
            { }//no hit
            if (Flags05.Value != 0)
            { }//eg 0xAE64583B, 0x61083310, 0xCAE96294, 0x1C376176
        }

    }
    [TC(typeof(EXP))] public class Dat151AmbientEmitter : Dat151RelData
    {
        public uint UnkOffset0 { get; set; }
        public FlagsUint Unk00 { get; set; }
        public FlagsUint Unk01 { get; set; }
        public FlagsUint Unk02 { get; set; }
        public Vector3 Position { get; set; }
        public FlagsUint Unk03 { get; set; }    //0
        public MetaHash Unk04 { get; set; }
        public MetaHash Unk05 { get; set; }
        public FlagsUint Unk06 { get; set; }    //0
        public FlagsUint Unk07 { get; set; }    //0xFFFFFFFF
        public FlagsUint Unk08 { get; set; }    //0
        public float Unk09 { get; set; }        //1, 5, 100, ...
        public float InnerRad { get; set; }        //0, 4,         ...     100 ... min value?
        public float OuterRad { get; set; }        //15, 16, 12, 10, 20,   300 ... max value?
        public FlagsByte Unk12 { get; set; }
        public FlagsByte Unk13 { get; set; }    //0,1,2,3,4,5
        public FlagsByte Unk14 { get; set; }
        public FlagsByte Unk15 { get; set; }    //0,1,2,3,4,5
        public FlagsUshort Unk16 { get; set; }  //0..600
        public FlagsUshort Unk17 { get; set; }  //0..150
        public FlagsByte Unk18 { get; set; }    //0,1,2
        public FlagsByte Unk19 { get; set; }    //0,1,2
        public FlagsByte Unk20 { get; set; }    //1,2,3,4,8,255
        public FlagsByte Unk21 { get; set; }    //1,2,3,4,5,6,8,10,255
        public FlagsByte Unk22 { get; set; }    //0, 50, 80, 100
        public FlagsByte Unk23 { get; set; }    //1,2,3,5
        public ushort ExtParamCount { get; set; } //0,1,2,4
        public ExtParam[] ExtParams { get; set; }

        public struct ExtParam
        {
            public MetaHash Hash;
            public float Value;
            public uint Flags;
            public ExtParam(BinaryReader br)
            {
                Hash = br.ReadUInt32();
                Value = br.ReadSingle();
                Flags = br.ReadUInt32();
            }
            public override string ToString()
            {
                return Hash.ToString() + ": " + FloatUtil.ToString(Value) + ": " + Flags.ToString();
            }
        }


        public Dat151AmbientEmitter(RelData d, BinaryReader br) : base(d, br)
        {
            br.BaseStream.Position = 0; //1 byte was read already (TypeID)

            UnkOffset0 = ((br.ReadUInt32() >> 8) & 0xFFFFFF);
            Unk00 = br.ReadUInt32();
            Unk01 = br.ReadUInt32();
            Unk02 = br.ReadUInt32();
            Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            Unk03 = br.ReadUInt32();    //0
            Unk04 = br.ReadUInt32();
            Unk05 = br.ReadUInt32();
            Unk06 = br.ReadUInt32();    //0
            Unk07 = br.ReadUInt32();    //0xFFFFFFFF
            Unk08 = br.ReadUInt32();    //0
            Unk09 = br.ReadSingle();    //1, 5, 100, ...
            InnerRad = br.ReadSingle();    //0, 4,         ...     100 ... min value?
            OuterRad = br.ReadSingle();    //15, 16, 12, 10, 20,   300 ... max value?
            Unk12 = br.ReadByte();     
            Unk13 = br.ReadByte();      //0,1,2,3,4,5
            Unk14 = br.ReadByte();     
            Unk15 = br.ReadByte();      //0,1,2,3,4,5
            Unk16 = br.ReadUInt16();    //0..600
            Unk17 = br.ReadUInt16();    //0..150
            Unk18 = br.ReadByte();      //0,1,2
            Unk19 = br.ReadByte();      //0,1,2
            Unk20 = br.ReadByte();      //1,2,3,4,8,255
            Unk21 = br.ReadByte();      //1,2,3,4,5,6,8,10,255
            Unk22 = br.ReadByte();      //0, 50, 80, 100
            Unk23 = br.ReadByte();      //1,2,3,5
            ExtParamCount = br.ReadUInt16();  //0,1,2,4

            if (ExtParamCount > 0)
            {
                ExtParams = new ExtParam[ExtParamCount];
                for (int i = 0; i < ExtParamCount; i++)
                {
                    ExtParams[i] = new ExtParam(br);
                }
                //array seems to be padded to multiples of 16 bytes. (read the rest here)
                int brem = (16 - ((ExtParamCount * 12) % 16)) % 16;
                if (brem > 0)
                {
                    byte[] brema = br.ReadBytes(brem);
                    //for (int i = 0; i < brem; i++)
                    //{
                    //    if (brema[i] != 0)
                    //    { } //check all remaining bytes are 0 - never hit here
                    //}
                }
            }


            switch (Unk12.Value)//no pattern?
            {
                default:
                    break;
            }
            switch (Unk13.Value)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    break;
                default:
                    break;
            }
            switch (Unk14.Value)//no pattern?
            {
                default:
                    break;
            }
            switch (Unk15.Value)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    break;
                default:
                    break;
            }
            switch (Unk16.Value)
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
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 18:
                case 20:
                case 22:
                case 24:
                case 25:
                case 26:
                case 30:
                case 32:
                case 35:
                case 40:
                case 45:
                case 48:
                case 50:
                case 51:
                case 54:
                case 55:
                case 57:
                case 60:
                case 64:
                case 65:
                case 70:
                case 75:
                case 80:
                case 90:
                case 95:
                case 97:
                case 100:
                case 120:
                case 125:
                case 130:
                case 135:
                case 140:
                case 145:
                case 150:
                case 160:
                case 170:
                case 178:
                case 180:
                case 190:
                case 200:
                case 220:
                case 225:
                case 240:
                case 245:
                case 250:
                case 300:
                case 350:
                case 500:
                case 600:
                    break;
                default:
                    break;
            }
            switch (Unk17.Value)
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
                case 10:
                case 12:
                case 15:
                case 17:
                case 20:
                case 21:
                case 22:
                case 25:
                case 27:
                case 30:
                case 32:
                case 35:
                case 40:
                case 50:
                case 60:
                case 100:
                case 150:
                    break;
                default:
                    break;
            }
            switch (Unk18.Value)
            {
                case 0:
                case 1:
                case 2:
                    break;
                default:
                    break;
            }
            switch (Unk19.Value)
            {
                case 0:
                case 1:
                case 2:
                    break;
                default:
                    break;
            }
            switch (Unk20.Value)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 8:
                case 255:
                    break;
                default:
                    break;
            }
            switch (Unk21.Value)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 8:
                case 10:
                case 255:
                    break;
                default:
                    break;
            }
            switch (Unk22.Value)
            {
                case 0:
                case 50:
                case 80:
                case 100:
                    break;
                default:
                    break;
            }
            switch (Unk23.Value)
            {
                case 1:
                case 2:
                case 3:
                case 5:
                    break;
                default:
                    break;
            }
            switch (ExtParamCount)
            {
                case 0:
                case 1:
                case 2:
                case 4:
                    break;
                default:
                    break;
            }



            //if ((Position.X != 0) || (Position.Y != 0) || (Position.Z != 0))
            //{
            //    FoundCoords.Add(FloatUtil.GetVector3String(Position) + ", " + GetNameString());
            //}

            long bytesleft = br.BaseStream.Length - br.BaseStream.Position;
            if (bytesleft != 0)
            { }
        }

    }
    [TC(typeof(EXP))] public class Dat151AmbientZoneList : Dat151RelData
    {
        public uint UnkOffset0 { get; set; }
        public uint ZoneCount { get; set; }
        public MetaHash[] ZoneHashes { get; set; }

        public Dat151AmbientZoneList(RelData d, BinaryReader br) : base(d, br)
        {
            br.BaseStream.Position = 0; //1 byte was read already (TypeID)

            UnkOffset0 = ((br.ReadUInt32() >> 8) & 0xFFFFFF);
            ZoneCount = br.ReadUInt32();
            ZoneHashes = new MetaHash[ZoneCount];
            for (int i = 0; i < ZoneCount; i++)
            {
                ZoneHashes[i] = br.ReadUInt32();
            }

            long bytesleft = br.BaseStream.Length - br.BaseStream.Position;
            if (bytesleft != 0)
            { } //no hits here

        }

    }

}
