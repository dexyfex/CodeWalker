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
    public class YcdFile : GameFile, PackedFile
    {
        public ClipDictionary ClipDictionary { get; set; }

        public Dictionary<MetaHash, ClipMapEntry> ClipMap { get; set; }
        public Dictionary<MetaHash, AnimationMapEntry> AnimMap { get; set; }

        public ClipMapEntry[] ClipMapEntries { get; set; }
        public AnimationMapEntry[] AnimMapEntries { get; set; }


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

            ResourceDataReader rd = new ResourceDataReader(resentry, data);


            ClipDictionary = rd.ReadBlock<ClipDictionary>();

            ClipMap = new Dictionary<MetaHash, ClipMapEntry>();
            AnimMap = new Dictionary<MetaHash, AnimationMapEntry>();
            if (ClipDictionary != null)
            {
                if ((ClipDictionary.Clips != null) && (ClipDictionary.Clips.data_items != null))
                {
                    foreach (var cme in ClipDictionary.Clips.data_items)
                    {
                        if (cme != null)
                        {
                            ClipMap[cme.Hash] = cme;
                            var nxt = cme.Next;
                            while (nxt != null)
                            {
                                ClipMap[nxt.Hash] = nxt;
                                nxt = nxt.Next;
                            }
                        }
                    }
                }
                if ((ClipDictionary.Animations != null) && (ClipDictionary.Animations.Animations != null) && (ClipDictionary.Animations.Animations.data_items != null))
                {
                    foreach (var ame in ClipDictionary.Animations.Animations.data_items)
                    {
                        if (ame != null)
                        {
                            AnimMap[ame.Hash] = ame;
                            var nxt = ame.NextEntry;
                            while (nxt != null)
                            {
                                AnimMap[nxt.Hash] = nxt;
                                nxt = nxt.NextEntry;
                            }
                        }
                    }
                }
            }

            foreach (var cme in ClipMap.Values)
            {
                var clip = cme.Clip;
                if (clip == null) continue;
                clip.Ycd = this;
                if (string.IsNullOrEmpty(clip.Name)) continue;
                string name = clip.Name.Replace('\\', '/');
                var slidx = name.LastIndexOf('/');
                if ((slidx >= 0) && (slidx < name.Length - 1))
                {
                    name = name.Substring(slidx + 1);
                }
                var didx = name.LastIndexOf('.');
                if ((didx > 0) && (didx < name.Length))
                {
                    name = name.Substring(0, didx);
                }
                clip.ShortName = name;
                name = name.ToLowerInvariant();
                JenkIndex.Ensure(name);


                //if (name.EndsWith("_uv_0")) //hash for these entries match string with this removed, +1
                //{
                //}
                //if (name.EndsWith("_uv_1")) //same as above, but +2
                //{
                //}

            }
            foreach (var ame in AnimMap.Values)
            {
                var anim = ame.Animation;
                if (anim == null) continue;
                anim.Ycd = this;
            }


            ClipMapEntries = ClipMap.Values.ToArray();
            AnimMapEntries = AnimMap.Values.ToArray();
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
                    if (chList[0] is AnimChannelStaticSmallestThreeQuaternion)
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
                        else if (chan is AnimChannelStaticFloat || chan is AnimChannelStaticVector3 || chan is AnimChannelStaticSmallestThreeQuaternion)
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
                                var q = seq.Sequences[i].EvaluateQuaternion(0);

                                return $"					{q[index]}\r\n";
                            }

                            StringBuilder db = new StringBuilder();
                            for (int f = 0; f < seq.NumFrames; f++)
                            {
                                db.AppendLine($"					{seq.Sequences[i].EvaluateQuaternion(f)[index]}");
                            }

                            return db.ToString();
                        }

                        switch (chan)
                        {
                            case AnimChannelStaticFloat sf:
                                return $"					{sf.FloatValue}\r\n";
                            case AnimChannelStaticVector3 v3:
                                return $"					{v3.Value[0]} {v3.Value[1]} {v3.Value[2]}\r\n";
                            case AnimChannelStaticSmallestThreeQuaternion q3:
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
}