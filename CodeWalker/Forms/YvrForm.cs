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
            sb.AppendLine("PositionX, PositionY, PositionZ, Time, VelocityX, VelocityY, VelocityZ, RightX, RightY, RightZ, ForwardX, ForwardY, ForwardZ, SteeringAngle, GasPedalPower, BrakePedalPower, HandbrakeUsed");
            foreach (var entry in yvr.Records.Entries.data_items)
            {
                sb.Append(FloatUtil.ToString(entry.Position.X));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Position.Y));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Position.Z));
                sb.Append(", ");
                sb.Append(entry.Time.ToString());
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Velocity.X));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Velocity.Y));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Velocity.Z));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Right.X));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Right.Y));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Right.Z));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Forward.X));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Forward.Y));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Forward.Z));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.Steering));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.GasPedal));
                sb.Append(", ");
                sb.Append(FloatUtil.ToString(entry.BrakePedal));
                sb.Append(", ");
                sb.Append(entry.Handbrake.ToString());
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
                    FloatUtil.ToString(entry.Position.X),
                    FloatUtil.ToString(entry.Position.Y),
                    FloatUtil.ToString(entry.Position.Z),
                    entry.Time.ToString(),
                    FloatUtil.ToString(entry.Velocity.X),
                    FloatUtil.ToString(entry.Velocity.Y),
                    FloatUtil.ToString(entry.Velocity.Z),
                    FloatUtil.ToString(entry.Right.X),
                    FloatUtil.ToString(entry.Right.Y),
                    FloatUtil.ToString(entry.Right.Z),
                    FloatUtil.ToString(entry.Forward.X),
                    FloatUtil.ToString(entry.Forward.Y),
                    FloatUtil.ToString(entry.Forward.Z),
                    FloatUtil.ToString(entry.Steering),
                    FloatUtil.ToString(entry.GasPedal),
                    FloatUtil.ToString(entry.BrakePedal),
                    entry.Handbrake.ToString(),
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
