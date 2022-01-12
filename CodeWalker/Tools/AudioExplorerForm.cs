using CodeWalker.GameFiles;
using System;
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
        private GameFileCache GameFileCache { get; set; }

        private Dictionary<string, RelData> NameComboLookup = new Dictionary<string, RelData>();

        public AudioExplorerForm(GameFileCache gfc)
        {
            GameFileCache = gfc;
            InitializeComponent();
            LoadDropDowns();
        }


        private void LoadDropDowns()
        {
            if (!GameFileCache.IsInited) return;

            NameComboLookup.Clear();
            NameComboBox.Items.Clear();
            NameComboBox.AutoCompleteCustomSource.Clear();
            List<string> namelist = new List<string>();
            void addItem(RelData item)
            {
                if (item == null) return;
                var str = GetRelDataTitleString(item);
                namelist.Add(str);
                NameComboLookup.Add(str, item);
            }
            if (GameFileCache.AudioSoundsDict != null)
            {
                //foreach (var kvp in GameFileCache.AudioConfigDict) addItem(kvp.Value);
                //foreach (var kvp in GameFileCache.AudioSpeechDict) addItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioSynthsDict) addItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioMixersDict) addItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioCurvesDict) addItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioCategsDict) addItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioSoundsDict) addItem(kvp.Value);
                foreach (var kvp in GameFileCache.AudioGameDict) addItem(kvp.Value);
            }
            NameComboBox.AutoCompleteCustomSource.AddRange(namelist.ToArray());
            NameComboBox.Text = "";



        }

        private string GetRelDataTitleString(RelData item)
        {
            if (item == null) return "";
            var h = item.NameHash;
            var str = JenkIndex.TryGetString(h);
            if (string.IsNullOrEmpty(str)) str = GlobalText.TryGetString(h);//is this necessary?
            if (string.IsNullOrEmpty(str)) MetaNames.TryGetString(h, out str);
            if (string.IsNullOrEmpty(str)) str = h.Hex;
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


        private void LoadItemHierarchy(RelData item, TreeNode parentNode = null)
        {
            TreeNode node = null;
            if (parentNode == null)
            {
                HierarchyTreeView.Nodes.Clear();
                if (item == null) return;
                node = HierarchyTreeView.Nodes.Add(GetRelDataTitleString(item));
            }
            else
            {
                if (item == null) return;
                node = parentNode.Nodes.Add(GetRelDataTitleString(item));
            }

            node.Tag = item;

            var synths = item.GetSynthHashes();
            var mixers = item.GetMixerHashes();
            var curves = item.GetCurveHashes();
            var categs = item.GetCategoryHashes();
            var sounds = item.GetSoundHashes();
            var games = item.GetGameHashes();


            if (synths != null)
            {
                foreach (var h in synths)

                {
                    if (GameFileCache.AudioSynthsDict.TryGetValue(h, out RelData child)) LoadItemHierarchy(child, node);
                }
            }
            if (mixers != null)
            {
                foreach (var h in mixers)
                {
                    if (GameFileCache.AudioMixersDict.TryGetValue(h, out RelData child)) LoadItemHierarchy(child, node);
                }
            }
            if (curves != null)
            {
                foreach (var h in curves)
                {
                    if (GameFileCache.AudioCurvesDict.TryGetValue(h, out RelData child)) LoadItemHierarchy(child, node);
                }
            }
            if (categs != null)
            {
                foreach (var h in categs)
                {
                    if (GameFileCache.AudioCategsDict.TryGetValue(h, out RelData child)) LoadItemHierarchy(child, node);
                }
            }
            if (sounds != null)
            {
                foreach (var h in sounds)
                {
                    if (GameFileCache.AudioSoundsDict.TryGetValue(h, out RelData child)) LoadItemHierarchy(child, node);
                }
            }
            if (games != null)
            {
                foreach (var h in games)
                {
                    if (GameFileCache.AudioGameDict.TryGetValue(h, out RelData child)) LoadItemHierarchy(child, node);
                }
            }


            if (parentNode == null)
            {
                node.ExpandAll();
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

        private void HierarchyTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var item = HierarchyTreeView.SelectedNode?.Tag as RelData;
            SelectItem(item);
        }
    }
}
