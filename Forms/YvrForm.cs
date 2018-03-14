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

namespace CodeWalker.Forms
{
    public partial class YvrForm : Form
    {

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
            fileName = yvr?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = yvr?.RpfFileEntry?.Name;
            }

            UpdateFormTitle();


            StringBuilder sb = new StringBuilder();

            if ((yvr != null) && (yvr.Records != null) && (yvr.Records.Entries != null) && (yvr.Records.Entries.data_items != null))
            {
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
            }
            else
            {
                sb.AppendLine("Unable to load Vehicle Records.");
            }

            MainTextBox.Text = sb.ToString();

        }




        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
