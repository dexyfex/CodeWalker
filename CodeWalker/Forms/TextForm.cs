using CodeWalker.GameFiles;
using CodeWalker.Properties;
using FastColoredTextBoxNS;
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

namespace CodeWalker.Forms
{
    public partial class TextForm : Form
    {
        private string textValue;
        public string TextValue
        {
            get { return textValue; }
            set
            {
                textValue = value;
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

        private bool modified = false;

        private ExploreForm exploreForm = null;
        public RpfFileEntry rpfFileEntry { get; private set; } = null;


        public enum TextFileType
        {
            Text = 0,
            GXT2 = 1,
            Nametable = 2,
        }
        private TextFileType fileType = TextFileType.Text;



        public TextForm(ExploreForm owner)
        {
            exploreForm = owner;

            InitializeComponent();
        }



        public void LoadText(string filename, string filepath, string text, RpfFileEntry e)
        {
            fileType = TextFileType.Text;
            FileName = filename;
            FilePath = filepath;
            TextValue = text;
            rpfFileEntry = e;
            modified = false;
        }
        public void LoadGxt2(string filename, string filepath, Gxt2File gxt)
        {
            fileType = TextFileType.GXT2;
            FileName = filename;
            FilePath = filepath;
            TextValue = gxt?.ToText() ?? "";
            rpfFileEntry = gxt?.FileEntry;
            modified = false;
        }
        public void LoadNametable(string filename, string filepath, byte[] data, RpfFileEntry e)
        {
            fileType = TextFileType.Nametable;
            FileName = filename;
            FilePath = filepath;
            TextValue = Encoding.UTF8.GetString(data).Replace('\0', '\n');
            rpfFileEntry = e;
            modified = false;
        }


        private void UpdateFormTitle()
        {
            Text = fileName + " - " + fileType.ToString() + " Editor - CodeWalker by dexyfex";
        }

        private void UpdateTextBoxFromData()
        {
            if (string.IsNullOrEmpty(textValue))
            {
                MainTextBox.Text = "";
                return;
            }

            Cursor = Cursors.WaitCursor;



            MainTextBox.Text = textValue;
            //MainTextBox.IsChanged = false;
            MainTextBox.ClearUndo();

            Cursor = Cursors.Default;
        }



        private bool CloseDocument()
        {
            if (modified)
            {
                var res = MessageBox.Show("Do you want to save the current document before closing it?", "Save before closing", MessageBoxButtons.YesNoCancel);
                switch (res)
                {
                    case DialogResult.Yes:
                        SaveDocument();
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }

            fileType = TextFileType.Text;
            FilePath = "";
            FileName = "";
            TextValue = "";
            modified = false;

            return true;
        }
        private void NewDocument()
        {
            if (!CloseDocument()) return; //same thing really..

            fileType = TextFileType.Text;
            FileName = "New.txt";
        }
        private void OpenDocument()
        {
            if (OpenFileDialog.ShowDialog() != DialogResult.OK) return;

            if (!CloseDocument()) return;

            var fn = OpenFileDialog.FileName;

            if (!File.Exists(fn)) return; //couldn't find file?


            var fnl = fn.ToLowerInvariant();
            if (fnl.EndsWith(".gxt2"))
            {
                var gxt = new Gxt2File();
                gxt.Load(File.ReadAllBytes(fn), null);
                fileType = TextFileType.GXT2;
                TextValue = gxt.ToText();
            }
            else if (fnl.EndsWith(".nametable"))
            {
                fileType = TextFileType.Nametable;
                TextValue = File.ReadAllText(fn).Replace('\0', '\n');
            }
            else
            {
                fileType = TextFileType.Text;
                TextValue = File.ReadAllText(fn);
            }

            modified = false;
            FilePath = fn;
            FileName = new FileInfo(fn).Name;
        }
        private void SaveDocument(bool saveAs = false)
        {
            if (saveAs == false)
            {
                if (SaveToRPF(textValue))
                {
                    return;
                }
                //if saving to RPF failed for whatever reason, fallback to saving the file in the filesystem.
                saveAs = true;
            }

            if (string.IsNullOrEmpty(FileName)) saveAs = true;
            if (string.IsNullOrEmpty(FilePath)) saveAs = true;
            else if ((FilePath.ToLowerInvariant().StartsWith(GTAFolder.CurrentGTAFolder.ToLowerInvariant()))) saveAs = true;
            if (!File.Exists(FilePath)) saveAs = true;

            var fn = FilePath;
            if (saveAs)
            {
                if (!string.IsNullOrEmpty(fn))
                {
                    var dir = new FileInfo(fn).DirectoryName;
                    if (!Directory.Exists(dir)) dir = "";
                    SaveFileDialog.InitialDirectory = dir;
                }
                SaveFileDialog.FileName = FileName;
                if (SaveFileDialog.ShowDialog() != DialogResult.OK) return;
                fn = SaveFileDialog.FileName;
            }

            if (fileType == TextFileType.Text)
            {
                File.WriteAllText(fn, textValue);
            }
            else if (fileType == TextFileType.GXT2)
            {
                var gxt = Gxt2File.FromText(textValue);
                var data = gxt.Save();
                File.WriteAllBytes(fn, data);
            }
            else if (fileType == TextFileType.Nametable)
            {
                File.WriteAllText(fn, textValue.Replace("\r", "").Replace('\n', '\0'));
            }



            modified = false;
            FilePath = fn;
            FileName = new FileInfo(fn).Name;
        }




        private bool SaveToRPF(string txt)
        {

            if (!(exploreForm?.EditMode ?? false)) return false;
            if (rpfFileEntry?.Parent == null) return false;

            byte[] data = null;

            if (fileType == TextFileType.Text)
            {
                data = Encoding.UTF8.GetBytes(txt);
            }
            else if (fileType == TextFileType.GXT2)
            {
                var gxt = Gxt2File.FromText(txt);
                data = gxt.Save();
            }
            else if (fileType == TextFileType.Nametable)
            {
                if (!txt.EndsWith("\n")) txt = txt + "\n";
                data = Encoding.UTF8.GetBytes(txt.Replace("\r", "").Replace('\n', '\0'));
            }

            if (data == null)
            {
                MessageBox.Show("Unspecified error - data was null!", "Cannot save file");
                return false;
            }

            if (!rpfFileEntry.Path.ToLowerInvariant().StartsWith("mods"))
            {
                if (MessageBox.Show("This file is NOT located in the mods folder - Are you SURE you want to save this file?\r\nWARNING: This could cause permanent damage to your game!!!", "WARNING: Are you sure about this?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return false;//that was a close one
                }
            }

            try
            {
                if (!(exploreForm?.EnsureRpfValidEncryption(rpfFileEntry.File) ?? false)) return false;

                var newentry = RpfFile.CreateFile(rpfFileEntry.Parent, rpfFileEntry.Name, data);
                if (newentry != rpfFileEntry)
                { }
                rpfFileEntry = newentry;

                exploreForm?.RefreshMainListViewInvoke(); //update the file details in explorer...

                modified = false;

                StatusLabel.Text = fileType.ToString() + " file saved successfully at " + DateTime.Now.ToString();

                return true; //victory!
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file to RPF! The RPF archive may be corrupted...\r\n" + ex.ToString(), "Really Bad Error");
            }

            return false;
        }





        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            textValue = MainTextBox.Text;
            modified = true;
        }

        private void NewButton_ButtonClick(object sender, EventArgs e)
        {
            NewDocument();
        }

        private void OpenButton_ButtonClick(object sender, EventArgs e)
        {
            OpenDocument();
        }

        private void SaveButton_ButtonClick(object sender, EventArgs e)
        {
            SaveDocument();
        }

        private void FileNewMenu_Click(object sender, EventArgs e)
        {
            NewDocument();
        }

        private void FileOpenMenu_Click(object sender, EventArgs e)
        {
            OpenDocument();
        }

        private void FileSaveMenu_Click(object sender, EventArgs e)
        {
            SaveDocument();
        }

        private void FileSaveAsMenu_Click(object sender, EventArgs e)
        {
            SaveDocument(true);
        }

        private void FileCloseMenu_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TextForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseDocument();
        }
    }
}
