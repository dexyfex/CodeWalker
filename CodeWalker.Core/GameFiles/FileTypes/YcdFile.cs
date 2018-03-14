using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    }
}
