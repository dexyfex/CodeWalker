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
    public partial class YvrForm : Form
    {

        private string fileName;
        private YvrFile yvr;
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




        public YvrForm()
        {
            InitializeComponent();
        }



        private void UpdateFormTitle()
        {
            Text = fileName + " - Vehicle Records Viewer - CodeWalker by dexyfex";
        }

        public void LoadYvr(YvrFile yvr)
        {
            this.yvr = yvr;
            fileName = yvr?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = yvr?.RpfFileEntry?.Name;
            }

            UpdateFormTitle();

            if ((yvr != null) && (yvr.Records != null) && (yvr.Records.Entries != null) && (yvr.Records.Entries.data_items != null))
            {
                ExportButton.Enabled = true;
                CopyClipboardButton.Enabled = true;
                LoadListView();
            }
        }


        private string GenerateText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("PositionX, PositionY, PositionZ, Time, VelocityX, VelocityY, VelocityZ, RightX, RightY, RightZ, TopX, TopY, TopZ, SteeringAngle, GasPedalPower, BrakePedalPower, HandbrakeUsed");
            foreach (var entry in yvr.Records.Entries.data_items)
            {
                sb.Append(FloatUtil.ToString(entry.PositionX));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.PositionY));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.PositionZ));
                sb.Append(", ");
                sb.Append(entry.Time.ToString());
                sb.Append(", ");
                sb.Append(entry.VelocityX.ToString());
                sb.Append(", ");
                sb.Append(entry.VelocityY.ToString());
                sb.Append(", ");
                sb.Append(entry.VelocityZ.ToString());
                sb.Append(", ");
                sb.Append(entry.RightX.ToString());
                sb.Append(", ");
                sb.Append(entry.RightY.ToString());
                sb.Append(", ");
                sb.Append(entry.RightZ.ToString());
                sb.Append(", ");
                sb.Append(entry.TopX.ToString());
                sb.Append(", ");
                sb.Append(entry.TopY.ToString());
                sb.Append(", ");
                sb.Append(entry.TopZ.ToString());
                sb.Append(", ");
                sb.Append(entry.SteeringAngle.ToString());
                sb.Append(", ");
                sb.Append(entry.GasPedalPower.ToString());
                sb.Append(", ");
                sb.Append(entry.BrakePedalPower.ToString());
                sb.Append(", ");
                sb.Append(entry.HandbrakeUsed.ToString());
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void LoadListView()
        {
            MainListView.BeginUpdate(); // performance
            foreach (var entry in yvr.Records.Entries.data_items)
            {
                string[] row =
                {
                    FloatUtil.ToString(entry.PositionX),
                    FloatUtil.ToString(entry.PositionY),
                    FloatUtil.ToString(entry.PositionZ),
                    entry.Time.ToString(),
                    entry.VelocityX.ToString(),
                    entry.VelocityY.ToString(),
                    entry.VelocityZ.ToString(),
                    entry.RightX.ToString(),
                    entry.RightY.ToString(),
                    entry.RightZ.ToString(),
                    entry.TopX.ToString(),
                    entry.TopY.ToString(),
                    entry.TopZ.ToString(),
                    entry.SteeringAngle.ToString(),
                    entry.GasPedalPower.ToString(),
                    entry.BrakePedalPower.ToString(),
                    entry.HandbrakeUsed.ToString(),
                };
                MainListView.Items.Add(new ListViewItem(row));
            }
            MainListView.EndUpdate();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(fileName) + ".csv";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, GenerateText());
            }
        }

        private void CopyClipboardButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(GenerateText());
        }
    }
}
