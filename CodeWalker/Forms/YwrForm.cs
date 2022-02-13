using CodeWalker.GameFiles;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Forms
{
    public partial class YwrForm : Form
    {

        private YwrFile ywr;
        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                UpdateFormTitle();
            }
        }
        public string FilePath { get; set; }




        public YwrForm()
        {
            InitializeComponent();
        }



        private void UpdateFormTitle()
        {
            Text = fileName + " - Waypoint Records Viewer - CodeWalker by dexyfex";
        }


        public void LoadYwr(YwrFile ywr)
        {
            this.ywr = ywr;
            fileName = ywr?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = ywr?.RpfFileEntry?.Name;
            }

            UpdateFormTitle();

            if ((ywr != null) && (ywr.Waypoints != null) && (ywr.Waypoints.Entries != null))
            {
                LoadListView();
                ExportButton.Enabled = true;
                CopyClipboardButton.Enabled = true;
            }
            else
            {
                MessageBox.Show("Error", "Could not load ywr", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GenerateText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("PositionX, PositionY, PositionZ, Unk0, Unk1, Unk2, Unk3");
            foreach (var entry in ywr.Waypoints.Entries)
            {
                sb.Append(FloatUtil.ToString(entry.Position.X));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Position.Y));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Position.Z));
                sb.Append(", ");
                sb.Append(entry.Unk0.ToString());
                sb.Append(", ");
                sb.Append(entry.Unk1.ToString());
                sb.Append(", ");
                sb.Append(entry.Unk2.ToString());
                sb.Append(", ");
                sb.Append(entry.Unk3.ToString());
                sb.AppendLine();
            }
            return sb.ToString();
        }
        private void LoadListView()
        {
            MainListView.BeginUpdate();
            MainListView.Items.Clear();
            foreach (var entry in ywr.Waypoints.Entries)
            {
                string[] row =
                {
                    FloatUtil.ToString(entry.Position.X),
                    FloatUtil.ToString(entry.Position.Y),
                    FloatUtil.ToString(entry.Position.Z),
                    entry.Unk0.ToString(),
                    entry.Unk1.ToString(),
                    entry.Unk2.ToString(),
                    entry.Unk3.ToString()
                };
                MainListView.Items.Add(new ListViewItem(row));
            }
            MainListView.EndUpdate();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CopyClipboardButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(GenerateText());
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(fileName) + ".csv";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, GenerateText());
            }
        }
    }
}
