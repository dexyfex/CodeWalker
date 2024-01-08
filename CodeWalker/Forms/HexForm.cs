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
    public partial class HexForm : Form
    {

        private byte[] data;
        public byte[] Data
        {
            get { return data; }
            set
            {
                data = value;
                UpdateTextBoxFromData();
            }
        }

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


        public HexForm()
        {
            InitializeComponent();

            LineSizeDropDown.Text = "16";
        }


        public void LoadData(string filename, string filepath, byte[] data)
        {
            FileName = filename;
            FilePath = filepath;
            Data = data;
        }

        private void UpdateFormTitle()
        {
            Text = fileName + " - Hex Viewer - CodeWalker by dexyfex";
        }

        private void UpdateTextBoxFromData()
        {
            if (data == null)
            {
                HexTextBox.Text = "";
                return;
            }
            if (data.Length > (1048576 * 5))
            {
                HexTextBox.Text = "[File size > 5MB - Not shown due to performance limitations - Please use an external viewer for this file.]";
                return;
            }


            Cursor = Cursors.WaitCursor;

            //int selline = -1;
            //int selstartc = -1;
            //int selendc = -1;

            bool ishex = (LineSizeDropDown.Text != "Text");


            if (ishex)
            {
                int charsperln = int.Parse(LineSizeDropDown.Text);
                int lines = (data.Length / charsperln) + (((data.Length % charsperln) > 0) ? 1 : 0);
                StringBuilder hexb = new StringBuilder();
                StringBuilder texb = new StringBuilder();
                StringBuilder finb = new StringBuilder();

                //if (offset > 0)
                //{
                //    selline = offset / charsperln;
                //}
                for (int i = 0; i < lines; i++)
                {
                    int pos = i * charsperln;
                    int poslim = pos + charsperln;
                    hexb.Clear();
                    texb.Clear();
                    hexb.Append($"{pos:X4}: ");
                    for (int c = pos; c < poslim; c++)
                    {
                        if (c < data.Length)
                        {
                            byte b = data[c];
                            hexb.Append($"{b:X2} ");
                            if (char.IsControl((char)b))
                            {
                                texb.Append('.');
                            }
                            else
                            {
                                texb.Append(Encoding.ASCII.GetString(data, c, 1));
                            }
                        }
                        else
                        {
                            hexb.Append("   ");
                            texb.Append(' ');
                        }
                    }

                    //if (i == selline) selstartc = finb.Length;
                    finb.AppendLine($"{hexb}| {texb}");
                    //if (i == selline) selendc = finb.Length - 1;
                }

                HexTextBox.Text = finb.ToString();

            }
            else
            {

                string text = Encoding.UTF8.GetString(data);


                HexTextBox.Text = text;

                //if (offset > 0)
                //{
                //    selstartc = offset;
                //    selendc = offset + length;
                //}
            }

            Cursor = Cursors.Default;
        }

        private void LineSizeDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTextBoxFromData();
        }
    }
}
