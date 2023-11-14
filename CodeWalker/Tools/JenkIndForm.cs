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

namespace CodeWalker.Tools
{
    public partial class JenkIndForm : Form
    {
        Dictionary<uint, string> extraStrings = new Dictionary<uint, string>();

        private static GameFileCache GameFileCache => GameFileCacheFactory.Instance;

        public JenkIndForm()
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

                if (!GameFileCache.IsInited)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                            GameFileCache.DoFullStringIndex = true;
                            GameFileCache.Init(UpdateStatus, UpdateStatus);
                            IndexBuildComplete();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            throw;
                        }
                    });
                }
                else
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            UpdateStatus("Loading strings...");
                            GameFileCache.DoFullStringIndex = true;
                            await GameFileCache.InitStringDictsAsync();
                            IndexBuildComplete();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
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

        private async void LoadStringsButton_Click(object sender, EventArgs e)
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
                using var stream = File.OpenRead(file);

                using var reader = new StreamReader(stream);

                var lineCount = 0;

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    line = line.Trim();
                    if (line.Length > 2) //remove double quotes from start and end, if both present...
                    {
                        if ((line[0] == '\"') && (line[line.Length - 1] == '\"'))
                        {
                            line = line.Substring(1, line.Length - 2);
                        }
                    }
                    var hash = JenkHash.GenHash(line);
                    extraStrings[hash] = line;
                    lineCount++;
                }

                MessageBox.Show($"{lineCount} strings imported successfully.");
            }
            catch
            {
                MessageBox.Show("Error reading file.");
            }

        }

        private async void SaveStringsButton_Click(object sender, EventArgs e)
        {
            if (SaveFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string file = SaveFileDialog.FileName;

            try
            {
                var lines = JenkIndex.GetAllStrings();

                using var stream = File.OpenWrite(file);

                using var writer = new StreamWriter(stream);

                writer.AutoFlush = false;

                foreach(var line in lines)
                {
                    await writer.WriteLineAsync(line);
                }

                await writer.FlushAsync();

                MessageBox.Show(lines.Count.ToString() + " strings exported successfully.");
            }
            catch
            {
                MessageBox.Show("Error saving strings file.");
            }
        }
    }
}
