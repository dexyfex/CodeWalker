using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YcdFile : GameFile, PackedFile
    {
        public ClipDictionary ClipDictionary { get; set; }

        public Dictionary<MetaHash, ClipMapEntry> ClipMap { get; set; }
        public Dictionary<MetaHash, AnimationMapEntry> AnimMap { get; set; }
        public Dictionary<MetaHash, ClipMapEntry> CutsceneMap { get; set; } //used for ycd's that are indexed in cutscenes, since name hashes all appended with -n

        public ClipMapEntry[] ClipMapEntries { get; set; }
        public AnimationMapEntry[] AnimMapEntries { get; set; }

        public string LoadException { get; set; }

        public YcdFile() : base(null, GameFileType.Ycd)
        {
        }
        public YcdFile(RpfFileEntry entry) : base(entry, GameFileType.Ycd)
        {
        }


        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;
            //Hash = entry.ShortNameHash;


            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                throw new Exception("File entry wasn't a resource! (is it binary data?)");
            }

            ResourceDataReader rd = null;
            try
            {
                rd = new ResourceDataReader(resentry, data);
            }
            catch (Exception ex)
            {
                //data = entry.File.DecompressBytes(data); //??
                LoadException = ex.ToString();
            }

            ClipDictionary = rd?.ReadBlock<ClipDictionary>();

            InitDictionaries();
        }

        public void InitDictionaries()
        {
            ClipMap = ClipDictionary?.ClipMap ?? new Dictionary<MetaHash, ClipMapEntry>();
            AnimMap = ClipDictionary?.AnimMap ?? new Dictionary<MetaHash, AnimationMapEntry>();

            foreach (var cme in ClipMap.Values)
            {
                if (cme?.Clip != null) cme.Clip.Ycd = this;
            }
            foreach (var ame in AnimMap.Values)
            {
                if (ame?.Animation != null) ame.Animation.Ycd = this;
            }

            ClipMapEntries = ClipMap.Values.ToArray();
            AnimMapEntries = AnimMap.Values.ToArray();

        }

        public void BuildCutsceneMap(int cutIndex)
        {
            CutsceneMap = new Dictionary<MetaHash, ClipMapEntry>();

            var replstr = "-" + cutIndex.ToString();

            foreach (var cme in ClipMapEntries)
            {
                var sn = cme?.Clip?.ShortName ?? "";
                if (sn.EndsWith(replstr))
                {
                    sn = sn.Substring(0, sn.Length - replstr.Length);
                }
                if (sn.EndsWith("_dual"))
                {
                    sn = sn.Substring(0, sn.Length - 5);
                }
                JenkIndex.Ensure(sn);
                var h = JenkHash.GenHash(sn);
                CutsceneMap[h] = cme;
            }
        }




        public byte[] Save()
        {
            //if (BuildStructsOnSave)
            //{
            //    BuildStructs();
            //}

            byte[] data = ResourceBuilder.Build(ClipDictionary, 46); //ycd is 46...

            return data;
        }



        public void SaveOpenFormatsAnimation(Animation crAnim, Stream outStream)
        {
            var seqs = new int[(crAnim.Frames / crAnim.SequenceFrameLimit) + 1];
            for (int s = 0; s < seqs.Length; s++)
            {
                seqs[s] = crAnim.SequenceFrameLimit + 1;
            }

            seqs[seqs.Length - 1] = crAnim.Frames % crAnim.SequenceFrameLimit;

            var writer = new StreamWriter(outStream);
            writer.Write($@"Version 8 2
{{
	Flags FLAG_0 FLAG_4 FLAG_5 FLAG_6 FLAG_7 FLAG_8
	Frames {crAnim.Frames}
	SequenceFrameLimit {crAnim.SequenceFrameLimit}
	Duration {crAnim.Frames / 30.0}
	_f10 285410817
	ExtraFlags
	Sequences {string.Join(" ", seqs.Select(f => f.ToString()))}
	MaterialID -1
	Animation
	{{
");

            int i = 0;

            foreach (var list in crAnim.BoneIds.data_items)
            {
                // for now, we only support BonePosition and BoneRotation tracks
                if (list.Track != 0 && list.Track != 1)
                {
                    continue;
                }

                bool isRotation = false;

                var chList = crAnim.Sequences.data_items[0].Sequences[i].Channels;

                if (chList.Length == 4)
                {
                    isRotation = true;
                }
                else if (chList.Length == 1)
                {
                    if (chList[0] is AnimChannelStaticQuaternion)
                    {
                        isRotation = true;
                    }
                }

                // temp placeholder
                if (list.Track == 1 && !isRotation)
                {
                    continue;
                }

                var type = (!isRotation) ? "BonePosition" : "BoneRotation";
                var dataType = (!isRotation) ? "Float3" : "Float4";

                var sb = new StringBuilder();

                foreach (var seq in crAnim.Sequences.data_items)
                {
                    var channels = seq.Sequences[i].Channels;

                    string ChannelType(AnimChannel chan)
                    {
                        if (seq.Sequences[i].IsType7Quat)
                        {
                            if (seq.Sequences[i].Channels[0] is AnimChannelStaticFloat && seq.Sequences[i].Channels[1] is AnimChannelStaticFloat && seq.Sequences[i].Channels[2] is AnimChannelStaticFloat)
                            {
                                return " Static";
                            }
                        }
                        else if (chan is AnimChannelStaticFloat || chan is AnimChannelStaticVector3 || chan is AnimChannelStaticQuaternion)
                        {
                            return " Static";
                        }

                        return "";
                    }

                    string ChannelData(AnimChannel chan)
                    {
                        if (seq.Sequences[i].IsType7Quat)
                        {
                            // get index this weird way
                            int index = 0;

                            for (int j = 0; j < seq.Sequences[i].Channels.Length; j++)
                            {
                                if (seq.Sequences[i].Channels[j] == chan)
                                {
                                    index = j;
                                    break;
                                }
                            }

                            // actually we should only export Static for 'real' channels, but as mapping for these is stupid, we'll just repeat the same value even if one channel is supposed to be static
                            if (seq.Sequences[i].Channels[0] is AnimChannelStaticFloat && seq.Sequences[i].Channels[1] is AnimChannelStaticFloat && seq.Sequences[i].Channels[2] is AnimChannelStaticFloat)
                            {
                                var q = seq.Sequences[i].EvaluateQuaternionType7(0);

                                return $"					{q[index]}\r\n";
                            }

                            StringBuilder db = new StringBuilder();
                            for (int f = 0; f < seq.NumFrames; f++)
                            {
                                db.AppendLine($"					{seq.Sequences[i].EvaluateQuaternionType7(f)[index]}");
                            }

                            return db.ToString();
                        }

                        switch (chan)
                        {
                            case AnimChannelStaticFloat sf:
                                return $"					{sf.Value}\r\n";
                            case AnimChannelStaticVector3 v3:
                                return $"					{v3.Value[0]} {v3.Value[1]} {v3.Value[2]}\r\n";
                            case AnimChannelStaticQuaternion q3:
                                return $"					{q3.Value[0]} {q3.Value[1]} {q3.Value[2]} {q3.Value[3]}\r\n";
                            default:
                                {
                                    StringBuilder db = new StringBuilder();
                                    for (int f = 0; f < seq.NumFrames; f++)
                                    {
                                        db.AppendLine($"					{chan.EvaluateFloat(f)}");
                                    }

                                    return db.ToString();
                                }
                        }
                    }

                    if (channels.Length == 1)
                    {
                        var chanType = ChannelType(channels[0]);
                        var chanData = ChannelData(channels[0]);


                        sb.Append($@"			FramesData SingleChannel{chanType}
			{{
{chanData}            }}
");
                    }
                    else
                    {
                        sb.Append($@"			FramesData MultiChannel
			{{
{string.Join("", channels.Select(ch => $@"				channel{ChannelType(ch)}
				{{
{ChannelData(ch)}                }}
"))}            }}
");
                    }
                }

                var animBits = sb.ToString();

                writer.Write($@"		{type} {dataType} {list.BoneId}
        {{
{animBits}        }}
");

                i++;
            }

            writer.Write($@"}}
}}
");

            writer.Flush();
        }
    }










    public class YcdXml : MetaXmlBase
    {

        public static string GetXml(YcdFile ycd)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if ((ycd != null) && (ycd.ClipDictionary != null))
            {
                var name = "ClipDictionary";

                OpenTag(sb, 0, name);

                ycd.ClipDictionary.WriteXml(sb, 1);

                CloseTag(sb, 0, name);
            }

            return sb.ToString();
        }

    }

    public class XmlYcd
    {

        public static YcdFile GetYcd(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYcd(doc);
        }

        public static YcdFile GetYcd(XmlDocument doc)
        {
            YcdFile ycd = new YcdFile();
            ycd.ClipDictionary = new ClipDictionary();
            ycd.ClipDictionary.ReadXml(doc.DocumentElement);
            ycd.InitDictionaries();
            //ycd.BuildStructsOnSave = false; //structs don't need to be rebuilt here!
            return ycd;
        }

    }


}