using CodeWalker.GameFiles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Tools
{
    public partial class AudioExplorerForm : Form
    {
        private static GameFileCache GameFileCache { get => GameFileCacheFactory.Instance; }

        private List<string> NameComboItems = new List<string>();
        private Dictionary<string, RelData> NameComboLookup = new Dictionary<string, RelData>();


        public AudioExplorerForm()
        {
            InitializeComponent();
            LoadDropDowns();
        }


        private void LoadDropDowns()
        {
            if (!GameFileCache.IsInited)
                return;

            NameComboLookup.Clear();
            NameComboBox.Items.Clear();
            NameComboBox.AutoCompleteCustomSource.Clear();
            NameComboItems = new List<string>();
            void addNameItem(RelData item, bool addToCombo = true)
            {
                if (item == null) return;
                var str = GetRelDataTitleString(item);
                NameComboLookup.Add(str, item);
                if (addToCombo) NameComboItems.Add(str);
            }
            if (GameFileCache.AudioSoundsDict != null)
            {
                foreach (var kvp in GameFileCache.AudioConfigDict) addNameItem(kvp.Value, false);
                foreach (var kvp in GameFileCache.AudioSpeechDict) addNameItem(kvp.Value, false);
                foreach (var kvp in GameFileCache.AudioSynthsDict) addNameItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioMixersDict) addNameItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioCurvesDict) addNameItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioCategsDict) addNameItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioSoundsDict) addNameItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioGameDict) addNameItem(kvp.Value);
            }
            NameComboBox.AutoCompleteCustomSource.AddRange(NameComboItems.ToArray());



            TypeComboBox.Items.Clear();
            TypeComboBox.Items.Add("(All types)");
            void addTypeItem(string filetype, object item)
            {
                var str = filetype + " : " + item.ToString();
                TypeComboBox.Items.Add(str);
            }
            foreach (var e in Enum.GetValues(typeof(Dat4ConfigType)))
                addTypeItem("Config", e);
            foreach (var e in Enum.GetValues(typeof(Dat4SpeechType)))
                addTypeItem("Speech", e);
            foreach (var e in Enum.GetValues(typeof(Dat10RelType)))
                addTypeItem("Synths", e);
            foreach (var e in Enum.GetValues(typeof(Dat15RelType)))
                addTypeItem("Mixers", e);
            foreach (var e in Enum.GetValues(typeof(Dat16RelType)))
                addTypeItem("Curves", e);
            foreach (var e in Enum.GetValues(typeof(Dat22RelType)))
                addTypeItem("Categories", e);
            foreach (var e in Enum.GetValues(typeof(Dat54SoundType)))
                addTypeItem("Sounds", e);
            foreach (var e in Enum.GetValues(typeof(Dat151RelType)))
                addTypeItem("Game", e);
            TypeComboBox.SelectedIndex = 0;


        }

        private void SelectType()
        {
            var typestr = TypeComboBox.Text;
            var typespl = typestr.Split(new[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<MetaHash, RelData> dict = null;
            byte typeid = 255;
            if (typespl.Length == 2)
            {
                switch (typespl[0])
                {
                    case "Config": { dict = GameFileCache.AudioConfigDict; if (Enum.TryParse(typespl[1], out Dat4ConfigType t)) typeid = (byte)t; break; }
                    case "Speech": { dict = GameFileCache.AudioSpeechDict; if (Enum.TryParse(typespl[1], out Dat4SpeechType t)) typeid = (byte)t; break; }
                    case "Synths": { dict = GameFileCache.AudioSynthsDict; if (Enum.TryParse(typespl[1], out Dat10RelType t)) typeid = (byte)t; break; }
                    case "Mixers": { dict = GameFileCache.AudioMixersDict; if (Enum.TryParse(typespl[1], out Dat15RelType t)) typeid = (byte)t; break; }
                    case "Curves": { dict = GameFileCache.AudioCurvesDict; if (Enum.TryParse(typespl[1], out Dat16RelType t)) typeid = (byte)t; break; }
                    case "Categories": { dict = GameFileCache.AudioCategsDict; if (Enum.TryParse(typespl[1], out Dat22RelType t)) typeid = (byte)t; break; }
                    case "Sounds": { dict = GameFileCache.AudioSoundsDict; if (Enum.TryParse(typespl[1], out Dat54SoundType t)) typeid = (byte)t; break; }
                    case "Game": { dict = GameFileCache.AudioGameDict; if (Enum.TryParse(typespl[1], out Dat151RelType t)) typeid = (byte)t; break; }
                }
            }
            if ((dict != null) && (typeid != 255))
            {
                NameComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
                NameComboBox.Text = "(Select item...)";
                NameComboBox.Items.Clear();
                var list = new List<string>();
                foreach (var kvp in dict)
                {
                    var item = kvp.Value;
                    if (item.TypeID == typeid)
                    {
                        var str = GetRelDataTitleString(item);
                        list.Add(str);
                    }
                }
                list.Sort();
                NameComboBox.Items.AddRange(list.ToArray());
            }
            else
            {
                NameComboBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                NameComboBox.Text = "(Start typing to search...)";
                NameComboBox.Items.Clear();
            }


        }

        private string GetRelDataTitleString(RelData item)
        {
            if (item is null)
                return "";
            var h = item.NameHash;
            var str = JenkIndex.TryGetString(h);
            if (string.IsNullOrEmpty(str))
                str = GlobalText.TryGetString(h);//is this necessary?
            if (string.IsNullOrEmpty(str))
                MetaNames.TryGetString(h, out str);
            if (string.IsNullOrEmpty(str))
                str = h.Hex;
            var typeid = item.TypeID.ToString();
            var rel = item.Rel;
            if (rel != null)
            {
                switch (rel.RelType)
                {
                    case RelDatFileType.Dat54DataEntries:
                        typeid = ((Dat54SoundType)item.TypeID).ToString();
                        break;
                    case RelDatFileType.Dat149:
                    case RelDatFileType.Dat150:
                    case RelDatFileType.Dat151:
                        typeid = ((Dat151RelType)item.TypeID).ToString();
                        break;
                    case RelDatFileType.Dat4:
                        if (rel.IsAudioConfig) typeid = ((Dat4ConfigType)item.TypeID).ToString();
                        else typeid = ((Dat4SpeechType)item.TypeID).ToString();
                        break;
                    case RelDatFileType.Dat10ModularSynth:
                        typeid = ((Dat10RelType)item.TypeID).ToString();
                        break;
                    case RelDatFileType.Dat15DynamicMixer:
                        typeid = ((Dat15RelType)item.TypeID).ToString();
                        break;
                    case RelDatFileType.Dat16Curves:
                        typeid = ((Dat16RelType)item.TypeID).ToString();
                        break;
                    case RelDatFileType.Dat22Categories:
                        typeid = ((Dat22RelType)item.TypeID).ToString();
                        break;
                    default:
                        break;
                }
            }
            return str + " : " + typeid;
        }

        private IEnumerable<MetaHash> GetUniqueHashes(MetaHash[] hashes, RelData item)
        {
            return hashes?.Distinct()?.Where(h => h != item.NameHash) ?? []; //try avoid infinite loops...
        }


        private void LoadItemHierarchy(RelData? item, TreeNode? parentNode = null)
        {
            TreeNode node;
            if (parentNode == null)
            {
                HierarchyTreeView.Nodes.Clear();
                if (item is null)
                    return;
                node = HierarchyTreeView.Nodes.Add(GetRelDataTitleString(item));
            }
            else
            {
                if (item is null)
                    return;
                node = parentNode.Nodes.Add(GetRelDataTitleString(item));
            }

            node.Tag = item;


            if (item is Dat22Category && parentNode != null && parentNode.Tag is not Dat22Category) //don't bother expanding out categories, too spammy!
            {
                return;
            }


            var speech = item.GetSpeechHashes();
            if (speech.Length > 0)
            {
                foreach (var h in GetUniqueHashes(speech, item))
                {
                    if (GameFileCache.AudioSpeechDict.TryGetValue(h, out RelData child))
                        LoadItemHierarchy(child, node);
                }
            }


            var synths = item.GetSynthHashes();
            if (synths.Length > 0)
            {
                foreach (var h in GetUniqueHashes(synths, item))
                {
                    if (GameFileCache.AudioSynthsDict.TryGetValue(h, out RelData child))
                        LoadItemHierarchy(child, node);
                }
            }

            var mixers = item.GetMixerHashes();
            if (mixers.Length > 0)
            {
                foreach (var h in GetUniqueHashes(mixers, item))
                {
                    if (GameFileCache.AudioMixersDict.TryGetValue(h, out RelData child))
                        LoadItemHierarchy(child, node);
                }
            }

            var curves = item.GetCurveHashes();
            if (curves.Length > 0)
            {
                foreach (var h in GetUniqueHashes(curves, item))
                {
                    if (GameFileCache.AudioCurvesDict.TryGetValue(h, out RelData child))
                        LoadItemHierarchy(child, node);
                }
            }

            var categs = item.GetCategoryHashes();
            if (categs.Length > 0)
            {
                foreach (var h in GetUniqueHashes(categs, item))
                {
                    if (GameFileCache.AudioCategsDict.TryGetValue(h, out RelData child))
                        LoadItemHierarchy(child, node);
                }
            }

            var sounds = item.GetSoundHashes();
            if (sounds.Length > 0)
            {
                foreach (var h in GetUniqueHashes(sounds, item))
                {
                    if (GameFileCache.AudioSoundsDict.TryGetValue(h, out RelData child))
                        LoadItemHierarchy(child, node);
                }
            }

            var games = item.GetGameHashes();
            if (games.Length > 0)
            {
                foreach (var h in GetUniqueHashes(games, item))
                {
                    if (GameFileCache.AudioGameDict.TryGetValue(h, out RelData child))
                        LoadItemHierarchy(child, node);
                }
            }


            if (parentNode == null)
            {
                var totnodes = node.GetNodeCount(true);
                if (totnodes > 100)
                {
                    node.Expand();
                    foreach (TreeNode cnode in node.Nodes)
                    {
                        foreach (TreeNode ccnode in cnode.Nodes)
                        {
                            ccnode.ExpandAll();
                        }
                    }
                }
                else
                {
                    node.ExpandAll();
                }
                HierarchyTreeView.SelectedNode = node;
            }

        }


        private void SelectItem(RelData item)
        {

            DetailsPropertyGrid.SelectedObject = item;

            XmlTextBox.Text = RelXml.GetXml(item);

        }


        private void NameComboBox_TextChanged(object sender, EventArgs e)
        {
            if (NameComboLookup.TryGetValue(NameComboBox.Text, out RelData item))
            {
                LoadItemHierarchy(item);
            }
        }

        private void TypeComboBox_TextChanged(object sender, EventArgs e)
        {
            SelectType();
        }

        private void HierarchyTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var item = HierarchyTreeView.SelectedNode?.Tag as RelData;
            SelectItem(item);
        }
    }
}
