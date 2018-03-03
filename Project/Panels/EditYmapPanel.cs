using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Project.Panels
{
    public partial class EditYmapPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YmapFile Ymap { get; set; }

        private bool populatingui = false;
        private bool waschanged = false;

        public EditYmapPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetYmap(YmapFile ymap)
        {
            Ymap = ymap;
            Tag = ymap;
            UpdateFormTitle();
            UpdateYmapUI();
            waschanged = ymap?.HasChanged ?? false;
        }

        public void UpdateFormTitleYmapChanged()
        {
            bool changed = Ymap.HasChanged;
            if (!waschanged && changed)
            {
                UpdateFormTitle();
                waschanged = true;
            }
            else if (waschanged && !changed)
            {
                UpdateFormTitle();
                waschanged = false;
            }
        }
        private void UpdateFormTitle()
        {
            string fn = Ymap.RpfFileEntry?.Name ?? Ymap.Name;
            if (string.IsNullOrEmpty(fn)) fn = "untitled.ymap";
            Text = fn + (Ymap.HasChanged ? "*" : "");
        }

        public void UpdateYmapUI()
        {
            if (Ymap == null)
            {
                YmapNameTextBox.Text = "<No ymap selected>";
                YmapNameHashLabel.Text = "Hash: 0";
                YmapParentTextBox.Text = string.Empty;
                YmapParentHashLabel.Text = "Hash: 0";
                YmapFlagsTextBox.Text = string.Empty;
                YmapContentFlagsTextBox.Text = string.Empty;
                YmapCFlagsHDCheckBox.Checked = false;
                YmapCFlagsLODCheckBox.Checked = false;
                YmapCFlagsSLOD2CheckBox.Checked = false;
                YmapCFlagsInteriorCheckBox.Checked = false;
                YmapCFlagsSLODCheckBox.Checked = false;
                YmapCFlagsOcclusionCheckBox.Checked = false;
                YmapCFlagsPhysicsCheckBox.Checked = false;
                YmapCFlagsLODLightsCheckBox.Checked = false;
                YmapCFlagsDistLightsCheckBox.Checked = false;
                YmapCFlagsCriticalCheckBox.Checked = false;
                YmapCFlagsGrassCheckBox.Checked = false;
                YmapFlagsScriptedCheckBox.Checked = false;
                YmapFlagsLODCheckBox.Checked = false;
                YmapPhysicsDictionariesTextBox.Text = string.Empty;
                YmapEntitiesExtentsMinTextBox.Text = string.Empty;
                YmapEntitiesExtentsMaxTextBox.Text = string.Empty;
                YmapStreamingExtentsMinTextBox.Text = string.Empty;
                YmapStreamingExtentsMaxTextBox.Text = string.Empty;
                YmapFileLocationTextBox.Text = string.Empty;
                YmapProjectPathTextBox.Text = string.Empty;
            }
            else
            {
                populatingui = true;
                var md = Ymap.CMapData;
                if (md.name.Hash == 0)
                {
                    string name = Path.GetFileNameWithoutExtension(Ymap.Name);
                    JenkIndex.Ensure(name);
                    md.name = new MetaHash(JenkHash.GenHash(name));
                }

                var project = ProjectForm?.CurrentProjectFile;

                YmapNameTextBox.Text = md.name.ToString();
                YmapNameHashLabel.Text = "Hash: " + md.name.Hash.ToString();
                YmapParentTextBox.Text = md.parent.ToString();
                YmapParentHashLabel.Text = "Hash: " + md.parent.Hash.ToString();
                YmapEntitiesExtentsMinTextBox.Text = FloatUtil.GetVector3String(md.entitiesExtentsMin);
                YmapEntitiesExtentsMaxTextBox.Text = FloatUtil.GetVector3String(md.entitiesExtentsMax);
                YmapStreamingExtentsMinTextBox.Text = FloatUtil.GetVector3String(md.streamingExtentsMin);
                YmapStreamingExtentsMaxTextBox.Text = FloatUtil.GetVector3String(md.streamingExtentsMax);
                YmapFileLocationTextBox.Text = Ymap.RpfFileEntry?.Path ?? Ymap.FilePath;
                YmapProjectPathTextBox.Text = (project != null) ? project.GetRelativePath(Ymap.FilePath) : Ymap.FilePath;

                UpdateYmapFlagsUI(true, true);

                UpdateYmapPhysicsDictionariesUI();

                populatingui = false;

                ////struct CMapData:
                //MetaHash name { get; set; } //8   8: Hash: 0: name
                //MetaHash parent { get; set; } //12   12: Hash: 0: parent
                //uint flags { get; set; } //16   16: UnsignedInt: 0: flags
                //uint contentFlags { get; set; } //20   20: UnsignedInt: 0: contentFlags//1785155637
                //Vector3 streamingExtentsMin { get; set; } //32   32: Float_XYZ: 0: streamingExtentsMin//3710026271
                //Vector3 streamingExtentsMax { get; set; } //48   48: Float_XYZ: 0: streamingExtentsMax//2720965429
                //Vector3 entitiesExtentsMin { get; set; } //64   64: Float_XYZ: 0: entitiesExtentsMin//477478129
                //Vector3 entitiesExtentsMax { get; set; } //80   80: Float_XYZ: 0: entitiesExtentsMax//1829192759
                //Array_StructurePointer entities { get; set; } //96   96: Array: 0: entities  {0: StructurePointer: 0: 256}
                //Array_Structure containerLods { get; set; } //112   112: Array: 0: containerLods//2935983381  {0: Structure: 372253349: 256}
                //Array_Structure boxOccluders { get; set; } //128   128: Array: 0: boxOccluders//3983590932  {0: Structure: SectionUNKNOWN7: 256}
                //Array_Structure occludeModels { get; set; } //144   144: Array: 0: occludeModels//2132383965  {0: Structure: SectionUNKNOWN5: 256}
                //Array_uint physicsDictionaries { get; set; } //160   160: Array: 0: physicsDictionaries//949589348  {0: Hash: 0: 256}
                //rage__fwInstancedMapData instancedData { get; set; } //176   176: Structure: rage__fwInstancedMapData: instancedData//2569067561
                //Array_Structure timeCycleModifiers { get; set; } //224   224: Array: 0: timeCycleModifiers  {0: Structure: CTimeCycleModifier: 256}
                //Array_Structure carGenerators { get; set; } //240   240: Array: 0: carGenerators//3254823756  {0: Structure: CCarGen: 256}
                //CLODLight LODLightsSOA { get; set; } //256   256: Structure: CLODLight: LODLightsSOA//1774371066
                //CDistantLODLight DistantLODLightsSOA { get; set; } //392   392: Structure: CDistantLODLight: DistantLODLightsSOA//2954466641
                //CBlockDesc block { get; set; } //440   440: Structure: CBlockDesc//3072355914: block

            }


        }

        private void UpdateYmapFlagsUI(bool updateCheckboxes, bool updateTextboxes)
        {
            if (Ymap == null) return;

            var md = Ymap.CMapData;
            var flags = md.flags;
            var contentFlags = md.contentFlags;

            if (updateCheckboxes)
            {
                YmapCFlagsHDCheckBox.Checked = BitUtil.IsBitSet(contentFlags, 0); //1
                YmapCFlagsLODCheckBox.Checked = BitUtil.IsBitSet(contentFlags, 1); //2
                YmapCFlagsSLOD2CheckBox.Checked = BitUtil.IsBitSet(contentFlags, 2); //4
                YmapCFlagsInteriorCheckBox.Checked = BitUtil.IsBitSet(contentFlags, 3); //8
                YmapCFlagsSLODCheckBox.Checked = BitUtil.IsBitSet(contentFlags, 4); //16
                YmapCFlagsOcclusionCheckBox.Checked = BitUtil.IsBitSet(contentFlags, 5); //32
                YmapCFlagsPhysicsCheckBox.Checked = BitUtil.IsBitSet(contentFlags, 6); //64
                YmapCFlagsLODLightsCheckBox.Checked = BitUtil.IsBitSet(contentFlags, 7); //128
                YmapCFlagsDistLightsCheckBox.Checked = BitUtil.IsBitSet(contentFlags, 8); //256
                YmapCFlagsCriticalCheckBox.Checked = BitUtil.IsBitSet(contentFlags, 9); //512
                YmapCFlagsGrassCheckBox.Checked = BitUtil.IsBitSet(contentFlags, 10); //1024

                YmapFlagsScriptedCheckBox.Checked = BitUtil.IsBitSet(flags, 0); //1
                YmapFlagsLODCheckBox.Checked = BitUtil.IsBitSet(flags, 1); //2
            }
            if (updateTextboxes)
            {
                YmapFlagsTextBox.Text = flags.ToString();
                YmapContentFlagsTextBox.Text = contentFlags.ToString();
            }
        }

        private void UpdateYmapPhysicsDictionariesUI()
        {
            if ((Ymap == null) || (Ymap.physicsDictionaries == null))
            {
                YmapPhysicsDictionariesTextBox.Text = string.Empty;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var hash in Ymap.physicsDictionaries)
                {
                    sb.AppendLine(hash.ToString());
                }
                YmapPhysicsDictionariesTextBox.Text = sb.ToString();
            }
        }

        private void SetYmapHasChanged(bool changed)
        {
            ProjectForm.SetYmapHasChanged(changed);
            UpdateFormTitleYmapChanged();
        }

        private void SetYmapPhysicsDictionariesFromTextbox()
        {
            if (populatingui) return;
            if (Ymap == null) return;

            List<MetaHash> hashes = new List<MetaHash>();

            var strs = YmapPhysicsDictionariesTextBox.Text.Split('\n');
            foreach (var str in strs)
            {
                var tstr = str.Trim();
                if (!string.IsNullOrEmpty(tstr))
                {
                    uint h = 0;
                    if (uint.TryParse(tstr, out h))
                    {
                        hashes.Add(h);
                    }
                    else
                    {
                        h = JenkHash.GenHash(tstr.ToLowerInvariant());
                        hashes.Add(h);
                    }
                }
            }

            lock (ProjectForm.ProjectSyncRoot)
            {
                Ymap.physicsDictionaries = (hashes.Count > 0) ? hashes.ToArray() : null;
                SetYmapHasChanged(true);
            }
        }

        private void SetYmapFlagsFromCheckBoxes()
        {
            if (populatingui) return;
            if (Ymap == null) return;

            uint flags = 0;
            uint contentFlags = 0;

            contentFlags = BitUtil.UpdateBit(contentFlags, 0, YmapCFlagsHDCheckBox.Checked);
            contentFlags = BitUtil.UpdateBit(contentFlags, 1, YmapCFlagsLODCheckBox.Checked);
            contentFlags = BitUtil.UpdateBit(contentFlags, 2, YmapCFlagsSLOD2CheckBox.Checked);
            contentFlags = BitUtil.UpdateBit(contentFlags, 3, YmapCFlagsInteriorCheckBox.Checked);
            contentFlags = BitUtil.UpdateBit(contentFlags, 4, YmapCFlagsSLODCheckBox.Checked);
            contentFlags = BitUtil.UpdateBit(contentFlags, 5, YmapCFlagsOcclusionCheckBox.Checked);
            contentFlags = BitUtil.UpdateBit(contentFlags, 6, YmapCFlagsPhysicsCheckBox.Checked);
            contentFlags = BitUtil.UpdateBit(contentFlags, 7, YmapCFlagsLODLightsCheckBox.Checked);
            contentFlags = BitUtil.UpdateBit(contentFlags, 8, YmapCFlagsDistLightsCheckBox.Checked);
            contentFlags = BitUtil.UpdateBit(contentFlags, 9, YmapCFlagsCriticalCheckBox.Checked);
            contentFlags = BitUtil.UpdateBit(contentFlags, 10, YmapCFlagsGrassCheckBox.Checked);

            flags = BitUtil.UpdateBit(flags, 0, YmapFlagsScriptedCheckBox.Checked);
            flags = BitUtil.UpdateBit(flags, 1, YmapFlagsLODCheckBox.Checked);


            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ymap._CMapData.flags != flags)
                {
                    Ymap._CMapData.flags = flags;
                    SetYmapHasChanged(true);
                }
                if (Ymap._CMapData.contentFlags != contentFlags)
                {
                    Ymap._CMapData.contentFlags = contentFlags;
                    SetYmapHasChanged(true);
                }
            }

            populatingui = true;
            UpdateYmapFlagsUI(false, true); //update textbox
            populatingui = false;
        }

        private void SetYmapFlagsFromTextBoxes()
        {
            if (populatingui) return;
            if (Ymap == null) return;

            uint flags = 0;
            uint contentFlags = 0;
            uint.TryParse(YmapFlagsTextBox.Text, out flags);
            uint.TryParse(YmapContentFlagsTextBox.Text, out contentFlags);

            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ymap._CMapData.flags != flags)
                {
                    Ymap._CMapData.flags = flags;
                    SetYmapHasChanged(true);
                }
                if (Ymap._CMapData.contentFlags != contentFlags)
                {
                    Ymap._CMapData.contentFlags = contentFlags;
                    SetYmapHasChanged(true);
                }
            }

            populatingui = true;
            UpdateYmapFlagsUI(true, false); //update checkboxes
            populatingui = false;
        }

        private void CalcYmapFlags()
        {
            if (populatingui) return;
            if (Ymap == null) return;

            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ymap.CalcFlags())
                {
                    SetYmapHasChanged(true);
                }
            }

            populatingui = true;
            UpdateYmapFlagsUI(true, true); //update checkboxes and textboxes
            populatingui = false;
        }

        private void CalcYmapExtents()
        {
            if (Ymap == null) return;

            var allents = Ymap.AllEntities;
            var allbatches = Ymap.GrassInstanceBatches;

            if ((allents == null) && (allbatches == null))
            {
                MessageBox.Show("No items to calculate extents from.");
                return;
            }

            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ymap.CalcExtents())
                {
                    SetYmapHasChanged(true);
                }
            }

            populatingui = true;
            var md = Ymap.CMapData;
            YmapEntitiesExtentsMinTextBox.Text = FloatUtil.GetVector3String(md.entitiesExtentsMin);
            YmapEntitiesExtentsMaxTextBox.Text = FloatUtil.GetVector3String(md.entitiesExtentsMax);
            YmapStreamingExtentsMinTextBox.Text = FloatUtil.GetVector3String(md.streamingExtentsMin);
            YmapStreamingExtentsMaxTextBox.Text = FloatUtil.GetVector3String(md.streamingExtentsMax);
            populatingui = false;
        }


        private void YmapNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            uint hash = 0;
            string name = YmapNameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            YmapNameHashLabel.Text = "Hash: " + hash.ToString();

            if (Ymap != null)
            {
                lock (ProjectForm.ProjectSyncRoot)
                {
                    string ymname = name + ".ymap";
                    if (Ymap.Name != ymname)
                    {
                        Ymap.Name = ymname;
                        Ymap._CMapData.name = new MetaHash(hash);
                        SetYmapHasChanged(true);
                        UpdateFormTitle();
                    }
                }
            }
        }

        private void YmapParentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            uint hash = 0;
            string name = YmapParentTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            YmapParentHashLabel.Text = "Hash: " + hash.ToString();

            if (hash != 0)
            {
                var entry = ProjectForm.FindParentYmapEntry(hash);
                if (entry == null)
                {
                    YmapParentHashLabel.Text += " (not found!)";
                }
            }

            if (Ymap != null)
            {
                lock (ProjectForm.ProjectSyncRoot)
                {
                    if (Ymap._CMapData.parent.Hash != hash)
                    {
                        Ymap._CMapData.parent = new MetaHash(hash);
                        SetYmapHasChanged(true);

                        //TODO: confirm entity parent linkage?
                    }
                }
            }

        }

        private void YmapFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromTextBoxes();
        }

        private void YmapContentFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromTextBoxes();
        }

        private void YmapCFlagsHDCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsLODCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsSLOD2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsInteriorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsSLODCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsOcclusionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsPhysicsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsLODLightsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsDistLightsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsCriticalCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCFlagsGrassCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapFlagsScriptedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapFlagsLODCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetYmapFlagsFromCheckBoxes();
        }

        private void YmapCalculateFlagsButton_Click(object sender, EventArgs e)
        {
            CalcYmapFlags();
        }

        private void YmapEntitiesExtentsMinTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (Ymap == null) return;
            Vector3 v = FloatUtil.ParseVector3String(YmapEntitiesExtentsMinTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ymap._CMapData.entitiesExtentsMin != v)
                {
                    Ymap._CMapData.entitiesExtentsMin = v;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void YmapEntitiesExtentsMaxTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (Ymap == null) return;
            Vector3 v = FloatUtil.ParseVector3String(YmapEntitiesExtentsMaxTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ymap._CMapData.entitiesExtentsMax != v)
                {
                    Ymap._CMapData.entitiesExtentsMax = v;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void YmapStreamingExtentsMinTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (Ymap == null) return;
            Vector3 v = FloatUtil.ParseVector3String(YmapStreamingExtentsMinTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ymap._CMapData.streamingExtentsMin != v)
                {
                    Ymap._CMapData.streamingExtentsMin = v;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void YmapStreamingExtentsMaxTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (Ymap == null) return;
            Vector3 v = FloatUtil.ParseVector3String(YmapStreamingExtentsMaxTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (Ymap._CMapData.streamingExtentsMax != v)
                {
                    Ymap._CMapData.streamingExtentsMax = v;
                    SetYmapHasChanged(true);
                }
            }
        }

        private void YmapCalculateExtentsButton_Click(object sender, EventArgs e)
        {
            CalcYmapExtents();
        }

        private void YmapPhysicsDictionariesTextBox_TextChanged(object sender, EventArgs e)
        {
            SetYmapPhysicsDictionariesFromTextbox();
        }
    }
}
