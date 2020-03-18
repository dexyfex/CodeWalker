using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            EventLog myLog = new EventLog();
            myLog.Log = "Application";
            //myLog.Source = ".NET Runtime";

            var lastEntry = myLog.Entries[myLog.Entries.Count - 1];
            var last_error_Message = lastEntry.Message;

            bool found = false;

            for (int index = myLog.Entries.Count - 1; index > 0; index--)
            {
                var errLastEntry = myLog.Entries[index];
                if (errLastEntry.EntryType == EventLogEntryType.Error)
                {
                    if (errLastEntry.Source == ".NET Runtime")
                    {
                        var msg = errLastEntry.Message;
                        var lines = msg.Split('\n');
                        if (lines.Length > 0)
                        {
                            var l = lines[0];
                            if (l.Contains("CodeWalker.exe") ||
                                l.Contains("CodeWalker RPF Explorer.exe") ||
                                l.Contains("CodeWalker Ped Viewer.exe") ||
                                l.Contains("CodeWalker Vehicle Viewer.exe"))
                            {
                                ErrorTextBox.Text = msg.Replace("\n", "\r\n");
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                ErrorTextBox.Text = "Event Log entry not found!";
                MessageBox.Show("Unable to find the last CodeWalker.exe error in the Event Log.");
            }

        }
    }
}
