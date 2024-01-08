using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Tools
{
    public partial class ImportFbxForm : Form
    {

        private Dictionary<string, byte[]> InputFiles { get; set; }
        private Dictionary<string, byte[]> OutputFiles { get; set; }

        public ImportFbxForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            OutputTypeCombo.Text = "YDR";
        }


        public void SetInputFiles(Dictionary<string, byte[]> fdict)
        {
            InputFiles = fdict;

            FbxFilesListBox.Items.Clear();
            foreach (var kvp in fdict)
            {
                FbxFilesListBox.Items.Add(kvp.Key);
            }

        }

        public Dictionary<string, byte[]> GetOutputFiles()
        {
            return OutputFiles;
        }


        private void ConvertFiles()
        {
            if (InputFiles == null) return;

            Cursor = Cursors.WaitCursor;


            Task.Run(() =>
            {

                OutputFiles = new Dictionary<string, byte[]>();

                foreach (var kvp in InputFiles)
                {
                    var fname = kvp.Key;
                    var idata = kvp.Value;

                    UpdateStatus("Converting " + fname + "...");

                    FbxConverter fc = new FbxConverter();

                    var ydr = fc.ConvertToYdr(fname, idata);


                    if (ydr == null)
                    {
                        UpdateStatus("Converting " + fname + " failed!"); //TODO: error message

                        continue; //something went wrong..
                    }

                    byte[] odata = ydr.Save();

                    OutputFiles.Add(fname + ".ydr", odata);
                }

                UpdateStatus("Process complete.");

                ConvertComplete();

            });
        }

        private void ConvertComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(ConvertComplete);
                }
                else
                {
                    Cursor = Cursors.Default;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void UpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(UpdateStatus, text);
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private void CancelThisButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            ConvertFiles();
        }
    }
}
