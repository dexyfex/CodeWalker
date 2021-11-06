using CodeWalker.GameFiles;
using CodeWalker.World;
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
    public partial class EditAudioEmitterPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public AudioPlacement CurrentEmitter { get; set; }

        private bool populatingui = false;


        public EditAudioEmitterPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetEmitter(AudioPlacement emitter)
        {
            CurrentEmitter = emitter;
            Tag = emitter;
            UpdateFormTitle();
            UpdateUI();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentEmitter?.NameHash.ToString() ?? "";
        }


        private void UpdateUI()
        {
            if (CurrentEmitter?.AudioEmitter == null)
            {
                AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;

                populatingui = true;
                NameTextBox.Text = string.Empty;
                PositionTextBox.Text = string.Empty;
                InnerRadTextBox.Text = string.Empty;
                OuterRadTextBox.Text = string.Empty;
                Hash1TextBox.Text = string.Empty;
                Hash2TextBox.Text = string.Empty;
                Unk01TextBox.Text = string.Empty;
                Unk02UpDown.Value = 0;
                Unk03UpDown.Value = 0;
                Unk04UpDown.Value = 0;
                Unk05UpDown.Value = 0;
                Unk06UpDown.Value = 0;
                Unk07UpDown.Value = 0;
                Unk08UpDown.Value = 0;
                Unk09UpDown.Value = 0;
                Unk10UpDown.Value = 0;
                Unk11UpDown.Value = 0;
                Unk12UpDown.Value = 0;
                Unk13UpDown.Value = 0;
                Flags0TextBox.Text = string.Empty;
                Flags1TextBox.Text = string.Empty;
                Flags2TextBox.Text = string.Empty;
                Flags3TextBox.Text = string.Empty;
                Flags4TextBox.Text = string.Empty;
                Flags5TextBox.Text = string.Empty;
                ExtParamsTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                AddToProjectButton.Enabled = CurrentEmitter?.RelFile != null ? !ProjectForm.AudioFileExistsInProject(CurrentEmitter.RelFile) : false;
                DeleteButton.Enabled = !AddToProjectButton.Enabled;

                populatingui = true;
                var e = CurrentEmitter.AudioEmitter;
                NameTextBox.Text = e.NameHash.ToString();
                PositionTextBox.Text = FloatUtil.GetVector3String(e.Position);
                InnerRadTextBox.Text = FloatUtil.ToString(e.InnerRad);
                OuterRadTextBox.Text = FloatUtil.ToString(e.OuterRad);
                Hash1TextBox.Text = e.Hash1.ToString();
                Hash2TextBox.Text = e.Hash2.ToString();
                Unk01TextBox.Text = FloatUtil.ToString(e.Unk01);
                Unk02UpDown.Value = e.Unk02.Value;
                Unk03UpDown.Value = e.Unk03.Value;
                Unk04UpDown.Value = e.Unk04.Value;
                Unk05UpDown.Value = e.Unk05.Value;
                Unk06UpDown.Value = e.Unk06.Value;
                Unk07UpDown.Value = e.Unk07.Value;
                Unk08UpDown.Value = e.Unk08.Value;
                Unk09UpDown.Value = e.Unk09.Value;
                Unk10UpDown.Value = e.Unk10.Value;
                Unk11UpDown.Value = e.Unk11.Value;
                Unk12UpDown.Value = e.Unk12.Value;
                Unk13UpDown.Value = e.Unk13.Value;
                Flags0TextBox.Text = e.Flags0.Hex;
                Flags1TextBox.Text = e.Flags1.Hex;
                Flags2TextBox.Text = e.Flags2.Hex;
                Flags3TextBox.Text = e.Flags3.Hex;
                Flags4TextBox.Text = e.Flags4.Hex;
                Flags5TextBox.Text = e.Flags5.Hex;

                StringBuilder sb = new StringBuilder();
                if (e.ExtParams != null)
                {
                    foreach (var extparam in e.ExtParams)
                    {
                        sb.Append(extparam.Hash.ToString());
                        sb.Append(", ");
                        sb.Append(FloatUtil.ToString(extparam.Value));
                        sb.Append(", ");
                        sb.Append(extparam.Flags.ToString());
                        sb.AppendLine();
                    }
                }
                ExtParamsTextBox.Text = sb.ToString();

                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectObject(CurrentEmitter);
                }

            }
        }

        private void ProjectItemChanged()
        {
            CurrentEmitter?.UpdateFromEmitter();//also update the placement wrapper

            if (CurrentEmitter?.RelFile != null)
            {
                ProjectForm.SetAudioFileHasChanged(true);
            }
        }


        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint hash = 0;
            string name = NameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //NameHashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.AudioEmitter.NameHash != hash)
            {
                CurrentEmitter.AudioEmitter.Name = NameTextBox.Text;
                CurrentEmitter.AudioEmitter.NameHash = hash;

                ProjectItemChanged();
                UpdateFormTitle();
            }
        }

        private void PositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            var vec = FloatUtil.ParseVector3String(PositionTextBox.Text);
            if (CurrentEmitter.AudioEmitter.Position != vec)
            {
                CurrentEmitter.AudioEmitter.Position = vec;

                ProjectItemChanged();

                //var wf = ProjectForm.WorldForm;
                //if (wf != null)
                //{
                //    wf.BeginInvoke(new Action(() =>
                //    {
                //        wf.SetWidgetPosition(CurrentEmitter.Position, true);
                //    }));
                //}
            }
        }

        private void InnerRadTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            float rad = FloatUtil.Parse(InnerRadTextBox.Text);
            if (CurrentEmitter.AudioEmitter.InnerRad != rad)
            {
                CurrentEmitter.AudioEmitter.InnerRad = rad;

                ProjectItemChanged();
            }
        }

        private void OuterRadTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            float rad = FloatUtil.Parse(OuterRadTextBox.Text);
            if (CurrentEmitter.AudioEmitter.OuterRad != rad)
            {
                CurrentEmitter.AudioEmitter.OuterRad = rad;

                ProjectItemChanged();
            }
        }

        private void Hash1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint hash = 0;
            string name = Hash1TextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.AudioEmitter.Hash1 != hash)
            {
                CurrentEmitter.AudioEmitter.Hash1 = hash;

                ProjectItemChanged();
            }
        }

        private void Hash2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint hash = 0;
            string name = Hash2TextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            //HashLabel.Text = "Hash: " + hash.ToString();

            if (CurrentEmitter.AudioEmitter.Hash2 != hash)
            {
                CurrentEmitter.AudioEmitter.Hash2 = hash;

                ProjectItemChanged();
            }
        }

        private void Unk01TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            float unk = FloatUtil.Parse(Unk01TextBox.Text);
            if (CurrentEmitter.AudioEmitter.Unk01 != unk)
            {
                CurrentEmitter.AudioEmitter.Unk01 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk02UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk02UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk02.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk02 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk03UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk03UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk03.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk03 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk04UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk04UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk04.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk04 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk05UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk05UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk05.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk05 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk06UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            ushort unk = (ushort)Unk06UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk06.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk06 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk07UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            ushort unk = (ushort)Unk07UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk07.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk07 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk08UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk08UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk08.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk08 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk09UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk09UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk09.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk09 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk10UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk10UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk10.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk10 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk11UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk11UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk11.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk11 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk12UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk12UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk12.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk12 = unk;

                ProjectItemChanged();
            }
        }

        private void Unk13UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            byte unk = (byte)Unk13UpDown.Value;
            if (CurrentEmitter.AudioEmitter.Unk13.Value != unk)
            {
                CurrentEmitter.AudioEmitter.Unk13 = unk;

                ProjectItemChanged();
            }
        }

        private void Flags0TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags0TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentEmitter.AudioEmitter.Flags0 != flags)
                {
                    CurrentEmitter.AudioEmitter.Flags0 = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Flags1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags1TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentEmitter.AudioEmitter.Flags1 != flags)
                {
                    CurrentEmitter.AudioEmitter.Flags1 = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Flags2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags2TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentEmitter.AudioEmitter.Flags2 != flags)
                {
                    CurrentEmitter.AudioEmitter.Flags2 = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Flags3TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags3TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentEmitter.AudioEmitter.Flags3 != flags)
                {
                    CurrentEmitter.AudioEmitter.Flags3 = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Flags4TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags4TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentEmitter.AudioEmitter.Flags4 != flags)
                {
                    CurrentEmitter.AudioEmitter.Flags4 = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void Flags5TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            uint flags = 0;
            if (uint.TryParse(Flags5TextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
            {
                if (CurrentEmitter.AudioEmitter.Flags5 != flags)
                {
                    CurrentEmitter.AudioEmitter.Flags5 = flags;

                    ProjectItemChanged();
                }
            }
        }

        private void ExtParamsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEmitter?.AudioEmitter == null) return;

            var paramstrs = ExtParamsTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (paramstrs?.Length > 0)
            {
                var paramlist = new List<Dat151AmbientRule.ExtParam>();
                foreach (var paramstr in paramstrs)
                {
                    var paramvals = paramstr.Split(',');
                    if (paramvals?.Length == 3)
                    {
                        var param = new Dat151AmbientRule.ExtParam();
                        var hashstr = paramvals[0].Trim();
                        var valstr = paramvals[1].Trim();
                        var flgstr = paramvals[2].Trim();
                        uint hash = 0;
                        if (!uint.TryParse(hashstr, out hash))//don't re-hash hashes
                        {
                            hash = JenkHash.GenHash(hashstr);
                            JenkIndex.Ensure(hashstr);
                        }
                        uint flags = 0;
                        uint.TryParse(flgstr, out flags);
                        param.Hash = hash;
                        param.Value = FloatUtil.Parse(valstr);
                        param.Flags = flags;
                        paramlist.Add(param);
                    }
                }

                CurrentEmitter.AudioEmitter.ExtParams = paramlist.ToArray();
                CurrentEmitter.AudioEmitter.ExtParamsCount = (ushort)paramlist.Count;

                ProjectItemChanged();
            }
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentEmitter == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentEmitter.Position, CurrentEmitter.AudioZone.PlaybackZoneSize);
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentEmitter);
            ProjectForm.AddAudioFileToProject(CurrentEmitter.RelFile);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentEmitter);
            ProjectForm.DeleteAudioEmitter();
        }

    }
}
