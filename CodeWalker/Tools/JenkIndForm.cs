using CodeWalker.GameFiles;
using CodeWalker.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker
{
    public partial class JenkIndForm : Form
    {
        Dictionary<uint, string> extraStrings = new Dictionary<uint, string>();



        public JenkIndForm(GameFileCache gameFileCache = null)
        {
            InitializeComponent();

            if (GlobalText.FullIndexBuilt)
            {
                IndexBuildComplete();
            }
            else
            {
                MainPanel.Enabled = false;
                Cursor = Cursors.WaitCursor;

                if ((gameFileCache == null) || (gameFileCache.IsInited == false))
                {
                    Task.Run(() =>
                    {
                        GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                        GameFileCache gfc = GameFileCacheFactory.Create();
                        gfc.DoFullStringIndex = true;
                        gfc.Init(UpdateStatus, UpdateStatus);
                        IndexBuildComplete();
                    });
                }
                else
                {
                    Task.Run(() =>
                    {
                        UpdateStatus("Loading strings...");
                        gameFileCache.DoFullStringIndex = true;
                        gameFileCache.InitStringDicts();
                        IndexBuildComplete();
                    });
                }
            }

        }




        private void UpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { UpdateStatus(text); }));
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch { }
        }
        private void IndexBuildComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { IndexBuildComplete(); }));
                }
                else
                {
                    StatusLabel.Text = "Index built";
                    MainPanel.Enabled = true;
                    Cursor = Cursors.Default;
                }
            }
            catch { }
        }


        private void FindHash()
        {
            uint hash = 0;
            string hashtxt = HashTextBox.Text;
            MatchTextBox.Text = "";
            if (HexRadioButton.Checked)
            {
                try
                {
                    hash = Convert.ToUInt32(hashtxt, 16);
                }
                catch
                {
                    StatusLabel.Text = "Invalid hex value!";
                    return;
                }
            }
            else if (UnsignedRadioButton.Checked)
            {
                try
                {
                    hash = uint.Parse(hashtxt);
                }
                catch
                {
                    StatusLabel.Text = "Invalid unsigned int value!";
                    return;
                }
            }
            else if (SignedRadioButton.Checked)
            {
                try
                {
                    hash = (uint)int.Parse(hashtxt);
                }
                catch
                {
                    StatusLabel.Text = "Invalid signed int value!";
                    return;
                }
            }
            StatusLabel.Text = Convert.ToString(hash, 16).ToUpper().PadLeft(8, '0');


            var str = JenkIndex.TryGetString(hash);
            var txt = GlobalText.TryGetString(hash);
            var sta = StatsNames.TryGetString(hash);
            var ext = TryGetExtraString(hash);
            bool hasstr = !string.IsNullOrEmpty(str);
            bool hastxt = !string.IsNullOrEmpty(txt);
            bool hasext = !string.IsNullOrEmpty(ext);
            bool hassta = !string.IsNullOrEmpty(sta);

            if (hasstr && hastxt)
            {
                MatchTextBox.Text = string.Format("JenkIndex match:\r\n{0}\r\nGlobalText match:\r\n{1}", str, txt);
            }
            else if (hasstr)
            {
                MatchTextBox.Text = str;
            }
            else if (hastxt)
            {
                MatchTextBox.Text = "GlobalText match:\r\n" + txt;
            }
            else if (hasext)
            {
                MatchTextBox.Text = "Extra strings match:\r\n" + ext;
            }
            else if (hassta)
            {
                MatchTextBox.Text = "Stats match:\r\n" + sta;
            }
            else
            {
                MatchTextBox.Text = "[No match found]";
            }


        }


        private string TryGetExtraString(uint hash)
        {
            string str;
            extraStrings.TryGetValue(hash, out str);
            return str;
        }


        private void HashTextBox_TextChanged(object sender, EventArgs e)
        {
            FindHash();
        }

        private void HexRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            FindHash();
        }

        private void UnsignedRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            FindHash();
        }

        private void SignedRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            FindHash();
        }

        private void LoadStringsButton_Click(object sender, EventArgs e)
        {
            if (OpenFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string file = OpenFileDialog.FileName;
            if (!File.Exists(file))
            {
                return;
            }

            try
            {
                string txt = File.ReadAllText(file);
                string[] lines = txt.Split('\n');
                foreach (string line in lines)
                {
                    string str = line.Trim();
                    if (str.Length > 2) //remove double quotes from start and end, if both present...
                    {
                        if ((str[0] == '\"') && (str[str.Length - 1] == '\"'))
                        {
                            str = str.Substring(1, str.Length - 2);
                        }
                    }
                    var hash = JenkHash.GenHash(str);
                    extraStrings[hash] = str;
                }
                MessageBox.Show(lines.Length.ToString() + " strings imported successfully.");
            }
            catch
            {
                MessageBox.Show("Error reading file.");
            }

        }

        private void SaveStringsButton_Click(object sender, EventArgs e)
        {
            if (SaveFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string file = SaveFileDialog.FileName;

            try
            {
                string[] lines = JenkIndex.GetAllStrings();

                File.WriteAllLines(file, lines);

                MessageBox.Show(lines.Length.ToString() + " strings exported successfully.");
            }
            catch
            {
                MessageBox.Show("Error saving strings file.");
            }
        }
    }
}
