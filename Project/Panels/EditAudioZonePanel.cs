﻿using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Project.Panels
{
    public partial class EditAudioZonePanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public AudioPlacement CurrentZone { get; set; }

        private bool populatingui = false;


        public EditAudioZonePanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetZone(AudioPlacement zone)
        {
            CurrentZone = zone;
            Tag = zone;
            UpdateFormTitle();
            UpdateUI();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentZone?.NameHash.ToString() ?? "";
        }


        private void UpdateUI()
        {

            if (CurrentZone?.AudioZone == null)
            {
                AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;

                populatingui = true;
                NameTextBox.Text = string.Empty;
                ShapeComboBox.Text = string.Empty;
                InnerPosTextBox.Text = string.Empty;
                InnerSizeTextBox.Text = string.Empty;
                InnerAngleTextBox.Text = string.Empty;
                InnerVec1TextBox.Text = string.Empty;
                InnerVec2TextBox.Text = string.Empty;
                InnerVec3TextBox.Text = string.Empty;
                OuterPosTextBox.Text = string.Empty;
                OuterSizeTextBox.Text = string.Empty;
                OuterAngleTextBox.Text = string.Empty;
                OuterVec1TextBox.Text = string.Empty;
                OuterVec2TextBox.Text = string.Empty;
                OuterVec3TextBox.Text = string.Empty;
                UnkVec1TextBox.Text = string.Empty;
                UnkVec2TextBox.Text = string.Empty;
                UnkVec3TextBox.Text = string.Empty;
                UnkBytesTextBox.Text = string.Empty;
                Flags0TextBox.Text = string.Empty;
                Flags1TextBox.Text = string.Empty;
                Flags2TextBox.Text = string.Empty;
                Hash0TextBox.Text = string.Empty;
                Hash1TextBox.Text = string.Empty;
                HashesTextBox.Text = string.Empty;
                ExtParamsTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                AddToProjectButton.Enabled = CurrentZone?.RelFile != null ? !ProjectForm.AudioFileExistsInProject(CurrentZone.RelFile) : false;
                DeleteButton.Enabled = !AddToProjectButton.Enabled;

                populatingui = true;
                var z = CurrentZone.AudioZone;
                NameTextBox.Text = z.NameHash.ToString();
                ShapeComboBox.Text = z.Shape.ToString();
                InnerPosTextBox.Text = FloatUtil.GetVector3String(z.InnerPos);
                InnerSizeTextBox.Text = FloatUtil.GetVector3String(z.InnerSize);
                InnerAngleTextBox.Text = z.InnerAngle.ToString();
                InnerVec1TextBox.Text = FloatUtil.GetVector4String(z.InnerVec1);
                InnerVec2TextBox.Text = FloatUtil.GetVector4String(z.InnerVec2);
                InnerVec3TextBox.Text = FloatUtil.GetVector3String(z.InnerVec3);
                OuterPosTextBox.Text = FloatUtil.GetVector3String(z.OuterPos);
                OuterSizeTextBox.Text = FloatUtil.GetVector3String(z.OuterSize);
                OuterAngleTextBox.Text = z.OuterAngle.ToString();
                OuterVec1TextBox.Text = FloatUtil.GetVector4String(z.OuterVec1);
                OuterVec2TextBox.Text = FloatUtil.GetVector4String(z.OuterVec2);
                OuterVec3TextBox.Text = FloatUtil.GetVector3String(z.OuterVec3);
                UnkVec1TextBox.Text = FloatUtil.GetVector4String(z.UnkVec1);
                UnkVec2TextBox.Text = FloatUtil.GetVector4String(z.UnkVec2);
                UnkVec3TextBox.Text = FloatUtil.GetVector2String(z.UnkVec3);
                UnkBytesTextBox.Text = string.Format("{0}, {1}, {2}", z.Unk14, z.Unk15, z.Unk16);
                Flags0TextBox.Text = z.Flags0.Hex;
                Flags1TextBox.Text = z.Flags1.Hex;
                Flags2TextBox.Text = z.Flags2.Hex;
                Hash0TextBox.Text = z.UnkHash0.ToString();
                Hash1TextBox.Text = z.UnkHash1.ToString();

                StringBuilder sb = new StringBuilder();
                if (z.Hashes != null)
                {
                    foreach (var hash in z.Hashes)
                    {
                        sb.AppendLine(hash.ToString());
                    }
                }
                HashesTextBox.Text = sb.ToString();

                sb.Clear();
                if (z.ExtParams != null)
                {
                    foreach (var extparam in z.ExtParams)
                    {
                        sb.Append(extparam.Hash.ToString());
                        sb.Append(", ");
                        sb.Append(FloatUtil.ToString(extparam.Value));
                        sb.AppendLine();
                    }
                }
                ExtParamsTextBox.Text = sb.ToString();

                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectObject(CurrentZone);
                }

            }

        }


        private void ProjectItemChanged()
        {
            CurrentZone?.UpdateFromZone();//also update the placement wrapper

            if (CurrentZone?.RelFile != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }


        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            uint hash = 0;
            string name = NameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentZone.AudioZone.NameHash != hash)
            {
                CurrentZone.AudioZone.Name = NameTextBox.Text;
                CurrentZone.AudioZone.NameHash = hash;

                ProjectItemChanged();
                UpdateFormTitle();
            }

        }

        private void ShapeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            Dat151ZoneShape shape = Dat151ZoneShape.Box;
            if (Enum.TryParse<Dat151ZoneShape>(ShapeComboBox.Text, out shape))
            {
                if (CurrentZone.AudioZone.Shape != shape)
                {
                    CurrentZone.AudioZone.Shape = shape;

                    ProjectItemChanged();
                }
            }

        }

        private void InnerPosTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector3String(InnerPosTextBox.Text);
            if (CurrentZone.AudioZone.InnerPos != vec)
            {
                CurrentZone.AudioZone.InnerPos = vec;

                ProjectItemChanged();

                //var wf = ProjectForm.WorldForm;
                //if (wf != null)
                //{
                //    wf.BeginInvoke(new Action(() =>
                //    {
                //        wf.SetWidgetPosition(CurrentZone.Position, true);
                //    }));
                //}
            }
        }

        private void InnerSizeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector3String(InnerSizeTextBox.Text);
            if (CurrentZone.AudioZone.InnerSize != vec)
            {
                CurrentZone.AudioZone.InnerSize = vec;

                ProjectItemChanged();
            }
        }

        private void InnerAngleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            uint ang = 0;
            if (uint.TryParse(InnerAngleTextBox.Text, out ang))
            {
                if (CurrentZone.AudioZone.InnerAngle != ang)
                {
                    CurrentZone.AudioZone.InnerAngle = ang;

                    ProjectItemChanged();
                }
            }
        }

        private void InnerVec1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector4String(InnerVec1TextBox.Text);
            if (CurrentZone.AudioZone.InnerVec1 != vec)
            {
                CurrentZone.AudioZone.InnerVec1 = vec;

                ProjectItemChanged();
            }
        }

        private void InnerVec2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector4String(InnerVec2TextBox.Text);
            if (CurrentZone.AudioZone.InnerVec2 != vec)
            {
                CurrentZone.AudioZone.InnerVec2 = vec;

                ProjectItemChanged();
            }
        }

        private void InnerVec3TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector3String(InnerVec3TextBox.Text);
            if (CurrentZone.AudioZone.InnerVec3 != vec)
            {
                CurrentZone.AudioZone.InnerVec3 = vec;

                ProjectItemChanged();
            }
        }

        private void OuterPosTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector3String(OuterPosTextBox.Text);
            if (CurrentZone.AudioZone.OuterPos != vec)
            {
                CurrentZone.AudioZone.OuterPos = vec;

                ProjectItemChanged();
            }
        }

        private void OuterSizeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector3String(OuterSizeTextBox.Text);
            if (CurrentZone.AudioZone.OuterSize != vec)
            {
                CurrentZone.AudioZone.OuterSize = vec;

                ProjectItemChanged();
            }
        }

        private void OuterAngleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            uint ang = 0;
            if (uint.TryParse(OuterAngleTextBox.Text, out ang))
            {
                if (CurrentZone.AudioZone.OuterAngle != ang)
                {
                    CurrentZone.AudioZone.OuterAngle = ang;

                    ProjectItemChanged();
                }
            }
        }

        private void OuterVec1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector4String(OuterVec1TextBox.Text);
            if (CurrentZone.AudioZone.OuterVec1 != vec)
            {
                CurrentZone.AudioZone.OuterVec1 = vec;

                ProjectItemChanged();
            }
        }

        private void OuterVec2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector4String(OuterVec2TextBox.Text);
            if (CurrentZone.AudioZone.OuterVec2 != vec)
            {
                CurrentZone.AudioZone.OuterVec2 = vec;

                ProjectItemChanged();
            }
        }

        private void OuterVec3TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector3String(OuterVec3TextBox.Text);
            if (CurrentZone.AudioZone.OuterVec3 != vec)
            {
                CurrentZone.AudioZone.OuterVec3 = vec;

                ProjectItemChanged();
            }
        }

        private void UnkVec1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector4String(UnkVec1TextBox.Text);
            if (CurrentZone.AudioZone.UnkVec1 != vec)
            {
                CurrentZone.AudioZone.UnkVec1 = vec;

                ProjectItemChanged();
            }
        }

        private void UnkVec2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector4String(UnkVec2TextBox.Text);
            if (CurrentZone.AudioZone.UnkVec2 != vec)
            {
                CurrentZone.AudioZone.UnkVec2 = vec;

                ProjectItemChanged();
            }
        }

        private void UnkVec3TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var vec = FloatUtil.ParseVector2String(UnkVec3TextBox.Text);
            if (CurrentZone.AudioZone.UnkVec3 != vec)
            {
                CurrentZone.AudioZone.UnkVec3 = vec;

                ProjectItemChanged();
            }
        }

        private void UnkBytesTextBox_TextChanged(object sender, EventArgs e)
        {
            var vals = UnkBytesTextBox.Text.Split(',');
            if (vals?.Length == 3)
            {
                byte val0 = 0, val1 = 0, val2 = 0;
                byte.TryParse(vals[0].Trim(), out val0);
                byte.TryParse(vals[1].Trim(), out val1);
                byte.TryParse(vals[2].Trim(), out val2);

                CurrentZone.AudioZone.Unk14 = val0;
                CurrentZone.AudioZone.Unk15 = val1;
                CurrentZone.AudioZone.Unk16 = val2;

                ProjectItemChanged();
            }
        }

        private void Flags0TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags0TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentZone.AudioZone.Flags0 != flags)
                {
                    CurrentZone.AudioZone.Flags0 = flags;

                    ProjectItemChanged();
                }
            }

        }

        private void Flags1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags1TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentZone.AudioZone.Flags1 != flags)
                {
                    CurrentZone.AudioZone.Flags1 = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Flags2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags2TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentZone.AudioZone.Flags2 != flags)
                {
                    CurrentZone.AudioZone.Flags2 = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Hash0TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var hashstr = Hash0TextBox.Text;
            uint hash = 0;
            if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(hashstr);
                JenkIndex.Ensure(hashstr);
            }

            if (CurrentZone.AudioZone.UnkHash0 != hash)
            {
                CurrentZone.AudioZone.UnkHash0 = hash;

                ProjectItemChanged();
            }
        }

        private void Hash1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var hashstr = Hash1TextBox.Text;
            uint hash = 0;
            if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(hashstr);
                JenkIndex.Ensure(hashstr);
            }

            if (CurrentZone.AudioZone.UnkHash1 != hash)
            {
                CurrentZone.AudioZone.UnkHash1 = hash;

                ProjectItemChanged();
            }
        }

        private void HashesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var hashstrs = HashesTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (hashstrs?.Length > 0)
            {
                var hashlist = new List<MetaHash>();
                foreach (var hashstr in hashstrs)
                {
                    uint hash = 0;
                    if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
                    {
                        hash = JenkHash.GenHash(hashstr);
                        JenkIndex.Ensure(hashstr);
                    }
                    hashlist.Add(hash);
                }

                CurrentZone.AudioZone.Hashes = hashlist.ToArray();
                CurrentZone.AudioZone.HashesCount = (byte)hashlist.Count;

                ProjectItemChanged();
            }
        }

        private void ExtParamsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentZone?.AudioZone == null) return;

            var paramstrs = ExtParamsTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (paramstrs?.Length > 0)
            {
                var paramlist = new List<Dat151AmbientZone.ExtParam>();
                foreach (var paramstr in paramstrs)
                {
                    var paramvals = paramstr.Split(',');
                    if (paramvals?.Length == 2)
                    {
                        var param = new Dat151AmbientZone.ExtParam();
                        var hashstr = paramvals[0].Trim();
                        var valstr = paramvals[1].Trim();
                        uint hash = 0;
                        if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
                        {
                            hash = JenkHash.GenHash(hashstr);
                            JenkIndex.Ensure(hashstr);
                        }
                        param.Hash = hash;
                        param.Value = FloatUtil.Parse(valstr);
                        paramlist.Add(param);
                    }
                }

                CurrentZone.AudioZone.ExtParams = paramlist.ToArray();
                CurrentZone.AudioZone.ExtParamsCount = (uint)paramlist.Count;

                ProjectItemChanged();
            }
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentZone == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentZone.Position, CurrentZone.AudioZone.InnerSize);
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentZone);
            ProjectForm.AddAudioFileToProject(CurrentZone.RelFile);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentZone);
            ProjectForm.DeleteAudioZone();
        }
    }
}
