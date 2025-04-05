using CodeWalker.Core.Utils;
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
    public partial class ConvertAssetsForm : Form
    {
        public ConvertAssetsForm()
        {
            InitializeComponent();
        }

        private void SelectFolder(TextBox tb)
        {
            FolderBrowserDialog.SelectedPath = tb.Text;
            var res = FolderBrowserDialog.ShowDialogNew();
            if (res == DialogResult.OK)
            {
                tb.Text = FolderBrowserDialog.SelectedPath;
            }
        }

        private void InputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            SelectFolder(InputFolderTextBox);
        }

        private void OutputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            SelectFolder(OutputFolderTextBox);
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            LogTextBox.Clear();

            var converter = new Gen9Converter();
            converter.InputFolder = InputFolderTextBox.Text?.Replace('/', '\\');
            converter.OutputFolder = OutputFolderTextBox.Text?.Replace('/', '\\');
            converter.ProcessSubfolders = SubfoldersCheckbox.Checked;
            converter.OverwriteExisting = OverwriteCheckbox.Checked;
            converter.CopyUnconverted = true;
            converter.QuestionFunc = new Func<string, string, bool>((msg, title) =>
            {
                return MessageBox.Show(msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes;
            });
            converter.ErrorAction = new Action<string>((msg) =>
            {
                MessageBox.Show(msg);
            });
            converter.LogAction = new Action<string>((msg) =>
            {
                BeginInvoke(new Action(() =>
                {
                    LogTextBox.AppendText(msg + "\r\n");
                    LogTextBox.ScrollToCaret();
                }));
            });
            converter.StartStopAction = new Action<bool>((start) =>
            {
                var enable = !start;
                BeginInvoke(new Action(() =>
                {
                    InputFolderTextBox.Enabled = enable;
                    InputFolderBrowseButton.Enabled = enable;
                    OutputFolderTextBox.Enabled = enable;
                    OutputFolderBrowseButton.Enabled = enable;
                    SubfoldersCheckbox.Enabled = enable;
                    OverwriteCheckbox.Enabled = enable;
                    ProcessButton.Enabled = enable;
                }));
            });
            converter.Convert();

        }

    }
}
