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


        public TextForm()
        {
            InitializeComponent();
        }



        public void LoadText(string filename, string filepath, string text)
        {
            FileName = filename;
            FilePath = filepath;
            TextValue = text;
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
