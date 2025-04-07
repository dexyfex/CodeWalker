using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CodeWalker.ErrorReport
{
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            InitializeComponent();
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {
            ErrorTextBox.Clear();

            EventLog appLog = new EventLog("Application");
            bool found = false;

            for (int i = appLog.Entries.Count - 1; i >= 0; i--)
            {
                EventLogEntry entry = appLog.Entries[i];
                if (entry.EntryType == EventLogEntryType.Error && entry.Source == ".NET Runtime")
                {
                    string message = entry.Message;
                    string[] lines = message.Split(new[] { '\n' }, StringSplitOptions.None);

                    if (lines.Length > 0 &&
                        (lines[0].Contains("CodeWalker.exe") ||
                         lines[0].Contains("CodeWalker RPF Explorer.exe") ||
                         lines[0].Contains("CodeWalker Ped Viewer.exe") ||
                         lines[0].Contains("CodeWalker Vehicle Viewer.exe")))
                    {
                        found = true;

                        AppendColoredText(ErrorTextBox, "Time: ", Color.DimGray);
                        AppendColoredText(ErrorTextBox, entry.TimeGenerated.ToString() + "\r\n", Color.DimGray);

                        AppendColoredText(ErrorTextBox, "=====================\r\n", Color.LightGray);

                        foreach (var line in lines)
                        {
                            AppendColoredText(ErrorTextBox, line.Trim() + "\r\n", Color.Black);
                        }

                        break;
                    }
                }
            }

            if (!found)
            {
                AppendColoredText(ErrorTextBox, "Event Log entry not found!\r\n", Color.Gray);
                MessageBox.Show("Unable to find the last CodeWalker error in the Event Log.");
            }
        }

        private void AppendColoredText(RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor; // Reset to default color
        }

        private void SaveLogButton_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save Error Log";
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog.FileName = "log.txt";
                saveFileDialog.DefaultExt = "txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    try
                    {
                        File.WriteAllText(filePath, ErrorTextBox.Text);
                        MessageBox.Show($"Log saved successfully to:\n{filePath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save the log file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CopyToClipboardButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ErrorTextBox.Text))
            {
                string wrappedText = $"```\r\n{ErrorTextBox.Text}\r\n```";
                Clipboard.SetText(wrappedText);
            }
            else
            {
                MessageBox.Show("There is no text to copy.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
