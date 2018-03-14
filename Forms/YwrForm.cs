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
    public partial class YwrForm : Form
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
            fileName = ywr?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = ywr?.RpfFileEntry?.Name;
            }

            UpdateFormTitle();


            StringBuilder sb = new StringBuilder();

            if ((ywr != null) && (ywr.Waypoints != null) && (ywr.Waypoints.Entries != null))
            {
                sb.AppendLine("PositionX, PositionY, PositionZ, Unk0, Unk1, Unk2, Unk3");
                foreach (var entry in ywr.Waypoints.Entries)
                {
                    sb.Append(FloatUtil.ToString(entry.PositionX));
                    sb.Append(", ");
                    sb.Append(FloatUtil.ToString(entry.PositionY));
                    sb.Append(", ");
                    sb.Append(FloatUtil.ToString(entry.PositionZ));
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
            }
            else
            {
                sb.AppendLine("Unable to load Waypoint Records.");
            }

            MainTextBox.Text = sb.ToString();
        }


        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
