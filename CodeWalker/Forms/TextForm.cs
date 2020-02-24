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



        public TextForm(ExploreForm owner)
        {
            exploreForm = owner;

            InitializeComponent();
        }



        public void LoadText(string filename, string filepath, string text, RpfFileEntry e)
        {
            FileName = filename;
            FilePath = filepath;
            TextValue = text;
            rpfFileEntry = e;
            modified = false;
        }

        private void UpdateFormTitle()
        {
            Text = fileName + " - Text Editor - CodeWalker by dexyfex";
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

            FilePath = "";
            FileName = "";
            TextValue = "";
            modified = false;

            return true;
        }
        private void NewDocument()
        {
            if (!CloseDocument()) return; //same thing really..

            FileName = "New.txt";
        }
        private void OpenDocument()
        {
            if (OpenFileDialog.ShowDialog() != DialogResult.OK) return;

            if (!CloseDocument()) return;

            var fn = OpenFileDialog.FileName;

            if (!File.Exists(fn)) return; //couldn't find file?

            TextValue = File.ReadAllText(fn);

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

            File.WriteAllText(fn, textValue);

            modified = false;
            FilePath = fn;
            FileName = new FileInfo(fn).Name;
        }




        private bool SaveToRPF(string txt)
        {

            if (!(exploreForm?.EditMode ?? false)) return false;
            if (rpfFileEntry?.Parent == null) return false;

            byte[] data = null;

            data = Encoding.UTF8.GetBytes(txt);

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

                StatusLabel.Text = "Text file saved successfully at " + DateTime.Now.ToString();

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
