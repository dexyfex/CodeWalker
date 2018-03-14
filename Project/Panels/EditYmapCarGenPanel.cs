using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Project.Panels
{
    public partial class EditYmapCarGenPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YmapCarGen CurrentCarGen { get; set; }

        private bool populatingui = false;

        public EditYmapCarGenPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetCarGen(YmapCarGen cargen)
        {
            CurrentCarGen = cargen;
            Tag = cargen;
            LoadCarGen();
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            if (CurrentCarGen._CCarGen.carModel == 0)
            {
                Text = "Car Generator";
            }
            else
            {
                Text = CurrentCarGen?.NameString() ?? "Car Generator";
            }
        }


        private void LoadCarGen()
        {

            if (CurrentCarGen == null)
            {
                //CarGenPanel.Enabled = false;
                CarAddToProjectButton.Enabled = false;
                CarDeleteButton.Enabled = false;
                CarModelTextBox.Text = string.Empty;
                CarModelHashLabel.Text = "Hash: 0";
                CarPopGroupTextBox.Text = string.Empty;
                CarPopGroupHashLabel.Text = "Hash: 0";
                CarFlagsTextBox.Text = string.Empty;
                CarPositionTextBox.Text = string.Empty;
                CarOrientXTextBox.Text = string.Empty;
                CarOrientYTextBox.Text = string.Empty;
                CarPerpendicularLengthTextBox.Text = string.Empty;
                CarBodyColorRemap1TextBox.Text = string.Empty;
                CarBodyColorRemap2TextBox.Text = string.Empty;
                CarBodyColorRemap3TextBox.Text = string.Empty;
                CarBodyColorRemap4TextBox.Text = string.Empty;
                CarLiveryTextBox.Text = string.Empty;
                foreach (int i in CarFlagsCheckedListBox.CheckedIndices)
                {
                    CarFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                populatingui = true;
                var c = CurrentCarGen.CCarGen;
                //CarGenPanel.Enabled = true;
                CarAddToProjectButton.Enabled = !ProjectForm.YmapExistsInProject(CurrentCarGen.Ymap);
                CarDeleteButton.Enabled = !CarAddToProjectButton.Enabled;
                CarModelTextBox.Text = c.carModel.ToString();
                CarModelHashLabel.Text = "Hash: " + c.carModel.Hash.ToString();
                CarPopGroupTextBox.Text = c.popGroup.ToString();
                CarPopGroupHashLabel.Text = "Hash: " + c.popGroup.Hash.ToString();
                CarFlagsTextBox.Text = c.flags.ToString();
                CarPositionTextBox.Text = FloatUtil.GetVector3String(c.position);
                CarOrientXTextBox.Text = FloatUtil.ToString(c.orientX);
                CarOrientYTextBox.Text = FloatUtil.ToString(c.orientY);
                CarPerpendicularLengthTextBox.Text = FloatUtil.ToString(c.perpendicularLength);
                CarBodyColorRemap1TextBox.Text = c.bodyColorRemap1.ToString();
                CarBodyColorRemap2TextBox.Text = c.bodyColorRemap2.ToString();
                CarBodyColorRemap3TextBox.Text = c.bodyColorRemap3.ToString();
                CarBodyColorRemap4TextBox.Text = c.bodyColorRemap4.ToString();
                CarLiveryTextBox.Text = c.livery.ToString();
                for (int i = 0; i < CarFlagsCheckedListBox.Items.Count; i++)
                {
                    var cv = ((c.flags & (1u << i)) > 0);
                    CarFlagsCheckedListBox.SetItemCheckState(i, cv ? CheckState.Checked : CheckState.Unchecked);
                }
                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectCarGen(CurrentCarGen);
                }

                ////struct CCarGen:
                //Vector3 position { get; set; } //16   16: Float_XYZ: 0: position
                //float orientX { get; set; } //32   32: Float: 0: orientX=735213009
                //float orientY { get; set; } //36   36: Float: 0: orientY=979440342
                //float perpendicularLength { get; set; } //40   40: Float: 0: perpendicularLength=124715667
                //MetaHash carModel { get; set; } //44   44: Hash: 0: carModel
                //uint flags { get; set; } //48   48: UnsignedInt: 0: flags
                //int bodyColorRemap1 { get; set; } //52   52: SignedInt: 0: bodyColorRemap1=1429703670
                //int bodyColorRemap2 { get; set; } //56   56: SignedInt: 0: bodyColorRemap2=1254848286
                //int bodyColorRemap3 { get; set; } //60   60: SignedInt: 0: bodyColorRemap3=1880965569
                //int bodyColorRemap4 { get; set; } //64   64: SignedInt: 0: bodyColorRemap4=1719152247
                //MetaHash popGroup { get; set; } //68   68: Hash: 0: popGroup=911358791
                //sbyte livery { get; set; } //72   72: SignedByte: 0: livery
            }
        }

        private void CarModelTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            uint hash = 0;
            string name = CarModelTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            CarModelHashLabel.Text = "Hash: " + hash.ToString();

            //var model = GameFileCache.GetCarInfo(hash); //todo: something like this for car info?
            //if (model == null)
            //{
            //    CarModelHashLabel.Text += " (not found)";
            //}

            if (CurrentCarGen != null)
            {
                lock (ProjectForm.ProjectSyncRoot)
                {
                    var modelhash = new MetaHash(hash);
                    if (CurrentCarGen._CCarGen.carModel != modelhash)
                    {
                        CurrentCarGen._CCarGen.carModel = modelhash;
                        ProjectForm.SetYmapHasChanged(true);
                    }
                }
            }

            ProjectForm.ProjectExplorer?.UpdateCarGenTreeNode(CurrentCarGen);

        }

        private void CarPopGroupTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            uint hash = 0;
            string name = CarPopGroupTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            CarPopGroupHashLabel.Text = "Hash: " + hash.ToString();

            //var grp = GameFileCache.GetCarPopGroup(hash); //todo: something like this for popgroup info?
            //if (grp == null)
            //{
            //    CarPopGroupHashLabel.Text += " (not found)";
            //}

            if (CurrentCarGen != null)
            {
                lock (ProjectForm.ProjectSyncRoot)
                {
                    var pghash = new MetaHash(hash);
                    if (CurrentCarGen._CCarGen.popGroup != pghash)
                    {
                        CurrentCarGen._CCarGen.popGroup = pghash;
                        ProjectForm.SetYmapHasChanged(true);
                    }
                }
            }

            ProjectForm.ProjectExplorer?.UpdateCarGenTreeNode(CurrentCarGen);
        }

        private void CarFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            uint flags = 0;
            uint.TryParse(CarFlagsTextBox.Text, out flags);
            populatingui = true;
            for (int i = 0; i < CarFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((flags & (1u << i)) > 0);
                CarFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen._CCarGen.flags != flags)
                {
                    CurrentCarGen._CCarGen.flags = flags;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void CarFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            uint flags = 0;
            for (int i = 0; i < CarFlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        flags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (CarFlagsCheckedListBox.GetItemChecked(i))
                    {
                        flags += (uint)(1 << i);
                    }
                }
            }
            populatingui = true;
            CarFlagsTextBox.Text = flags.ToString();
            populatingui = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen._CCarGen.flags != flags)
                {
                    CurrentCarGen._CCarGen.flags = flags;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void CarPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            Vector3 v = FloatUtil.ParseVector3String(CarPositionTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen.Position != v)
                {
                    CurrentCarGen.SetPosition(v);
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetPosition(v);
                }
            }

            ProjectForm.ProjectExplorer?.UpdateCarGenTreeNode(CurrentCarGen);
        }

        private void CarOrientXTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            float ox = FloatUtil.Parse(CarOrientXTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen._CCarGen.orientX != ox)
                {
                    CurrentCarGen._CCarGen.orientX = ox;
                    CurrentCarGen.CalcOrientation();
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetRotation(CurrentCarGen.Orientation);
                }
            }
        }

        private void CarOrientYTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            float oy = FloatUtil.Parse(CarOrientYTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen._CCarGen.orientY != oy)
                {
                    CurrentCarGen._CCarGen.orientY = oy;
                    CurrentCarGen.CalcOrientation();
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetRotation(CurrentCarGen.Orientation);
                }
            }
        }

        private void CarPerpendicularLengthTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            float len = FloatUtil.Parse(CarPerpendicularLengthTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen._CCarGen.perpendicularLength != len)
                {
                    CurrentCarGen.SetLength(len);
                    ProjectForm.SetYmapHasChanged(true);
                    ProjectForm.WorldForm?.SetWidgetScale(new Vector3(len));
                }
            }
        }

        private void CarBodyColorRemap1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            int cr = 0;
            int.TryParse(CarBodyColorRemap1TextBox.Text, out cr);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen._CCarGen.bodyColorRemap1 != cr)
                {
                    CurrentCarGen._CCarGen.bodyColorRemap1 = cr;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void CarBodyColorRemap2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            int cr = 0;
            int.TryParse(CarBodyColorRemap2TextBox.Text, out cr);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen._CCarGen.bodyColorRemap2 != cr)
                {
                    CurrentCarGen._CCarGen.bodyColorRemap2 = cr;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void CarBodyColorRemap3TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            int cr = 0;
            int.TryParse(CarBodyColorRemap3TextBox.Text, out cr);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen._CCarGen.bodyColorRemap3 != cr)
                {
                    CurrentCarGen._CCarGen.bodyColorRemap3 = cr;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void CarBodyColorRemap4TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            int cr = 0;
            int.TryParse(CarBodyColorRemap4TextBox.Text, out cr);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen._CCarGen.bodyColorRemap4 != cr)
                {
                    CurrentCarGen._CCarGen.bodyColorRemap4 = cr;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void CarLiveryTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentCarGen == null) return;
            sbyte cr = 0;
            sbyte.TryParse(CarLiveryTextBox.Text, out cr);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentCarGen._CCarGen.livery != cr)
                {
                    CurrentCarGen._CCarGen.livery = cr;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateCarGenTreeNode(CurrentCarGen);
        }

        private void CarGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentCarGen == null) return;
            ProjectForm.WorldForm?.GoToPosition(CurrentCarGen.Position);
        }

        private void CarAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentCarGen);
            ProjectForm.AddCarGenToProject();
        }

        private void CarDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentCarGen);
            ProjectForm.DeleteCarGen();
        }
    }
}
