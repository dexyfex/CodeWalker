using CodeWalker.GameFiles;
using CodeWalker.Utils;

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
using OxyPlot;
using OxyPlot.Series;
using System.Xml;
using Range = FastColoredTextBoxNS.Range;
using TextStyle = FastColoredTextBoxNS.TextStyle;

namespace CodeWalker.Forms
{
    public partial class RelForm : Form
    {
        private string xml;
        public string Xml
        {
            get { return xml; }
            set
            {
                xml = value;
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

        private RelFile CurrentFile { get; set; }


        private bool modified = false;
        private bool LoadingXml = false;
        private bool DelayHighlight = false;

        private ExploreForm exploreForm = null;
        public RpfFileEntry rpfFileEntry { get; private set; } = null;
        private MetaFormat metaFormat = MetaFormat.XML;

        private bool loadingSynth = false;
        private Dat10Synth currentSynth = null;


        public RelForm(ExploreForm owner)
        {
            exploreForm = owner;

            InitializeComponent();
        }



        private void UpdateFormTitle()
        {
            Text = fileName + " - Audio dat.rel Editor - CodeWalker by dexyfex";
        }

        private void UpdateTextBoxFromData()
        {
            LoadingXml = true;
            XmlTextBox.Text = "";
            XmlTextBox.Language = Language.XML;
            DelayHighlight = false;

            if (string.IsNullOrEmpty(xml))
            {
                LoadingXml = false;
                return;
            }
            //if (xml.Length > (1048576 * 5))
            //{
            //    XmlTextBox.Language = Language.Custom;
            //    XmlTextBox.Text = "[XML size > 10MB - Not shown due to performance limitations - Please use an external viewer for this file.]";
            //    return;
            //}
            //else 
            if (xml.Length > (1024 * 512))
            {
                XmlTextBox.Language = Language.Custom;
                DelayHighlight = true;
            }
            //else
            //{
            //    XmlTextBox.Language = Language.XML;
            //}


            Cursor = Cursors.WaitCursor;



            XmlTextBox.Text = xml;
            //XmlTextBox.IsChanged = false;
            XmlTextBox.ClearUndo();

            Cursor = Cursors.Default;
            LoadingXml = false;
        }


        public void LoadRel(RelFile rel)
        {

            fileName = rel?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = rel?.RpfFileEntry?.Name;
            }

            UpdateFormTitle();

            RelPropertyGrid.SelectedObject = rel;

            CurrentFile = rel;

            rpfFileEntry = rel?.RpfFileEntry;

            Xml = RelXml.GetXml(rel);

            metaFormat = MetaFormat.AudioRel;

            StringBuilder sb = new StringBuilder();
            if (rel != null)
            {
                if (rel.NameTable != null)
                {
                    sb.AppendLine("NameTable - " + rel.NameTable.Length.ToString() + " entries");
                    foreach (var name in rel.NameTable)
                    {
                        sb.AppendLine(name);
                    }
                    sb.AppendLine();
                }
                if (rel.IndexStrings != null)
                {
                    sb.AppendLine("IndexStrings - " + rel.IndexStrings.Length.ToString() + " entries");
                    foreach (var rstr in rel.IndexStrings)
                    {
                        sb.AppendLine(rstr.Name);
                    }
                    sb.AppendLine();
                }
                if (rel.IndexHashes != null)
                {
                    sb.AppendLine("IndexHashes - " + rel.IndexHashes.Length.ToString() + " entries");
                    foreach (var rhash in rel.IndexHashes)
                    {
                        uint h = rhash.Name;
                        var jstr = JenkIndex.TryGetString(h);
                        if (!string.IsNullOrEmpty(jstr))
                        {
                            sb.AppendLine(jstr);
                        }
                        else
                        {
                            sb.AppendLine("0x" + h.ToString("X").PadLeft(8, '0'));
                        }
                    }
                    sb.AppendLine();
                }
            }
            MainTextBox.Text = sb.ToString();


            SynthsComboBox.Items.Clear();
            SynthTextBox.Language = Language.Custom;
            SynthTextBox.Text = "";
            SynthTextBox.ClearUndo();
            if (rel.RelType == RelDatFileType.Dat10ModularSynth)
            {
                foreach (var relData in rel.RelDatasSorted)
                {
                    if ((Dat10RelType)relData.TypeID == Dat10RelType.Synth)
                    {
                        SynthsComboBox.Items.Add(relData);
                    }
                }
            }
            else
            {
                MainTabControl.TabPages.Remove(SynthsTabPage);
            }
        }




        private bool SaveRel(XmlDocument doc)
        {

            if (!(exploreForm?.EditMode ?? false)) return false;

            byte[] data = null;

#if !DEBUG
            try
#endif
            {
                switch (metaFormat)
                {
                    default:
                    case MetaFormat.XML:
                        return false;//what are we even doing here?
                    case MetaFormat.AudioRel:
                        var rel = XmlRel.GetRel(doc);
                        if ((rel?.RelDatasSorted == null) || (rel.RelDatasSorted.Length == 0))
                        {
                            MessageBox.Show("Schema not supported.", "Cannot import REL XML");
                            return false;
                        }
                        data = rel.Save();
                        break;
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageBox.Show("Exception encountered!\r\n" + ex.ToString(), "Cannot convert XML");
                return false;
            }
#endif
            if (data == null)
            {
                MessageBox.Show("Schema not supported. (Unspecified error - data was null!)", "Cannot convert XML");
                return false;
            }

            if (rpfFileEntry?.Parent != null)
            {
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

                    StatusLabel.Text = metaFormat.ToString() + " file saved successfully at " + DateTime.Now.ToString();

                    return true; //victory!
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving file to RPF! The RPF archive may be corrupted...\r\n" + ex.ToString(), "Really Bad Error");
                }
            }
            else if (!string.IsNullOrEmpty(rpfFileEntry?.Path))
            {
                try
                {
                    File.WriteAllBytes(rpfFileEntry.Path, data);

                    exploreForm?.RefreshMainListViewInvoke(); //update the file details in explorer...

                    modified = false;

                    StatusLabel.Text = metaFormat.ToString() + " file saved successfully at " + DateTime.Now.ToString();

                    return true; //victory!
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving file to filesystem!\r\n" + ex.ToString(), "File I/O Error");
                }
            }

            return false;
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
            Xml = "";
            RelPropertyGrid.SelectedObject = null;
            MainTextBox.Text = "";
            SynthsComboBox.Items.Clear();
            SynthTextBox.Text = "";
            modified = false;
            rpfFileEntry = null;

            return true;
        }
        private void NewDocument()
        {
            if (!CloseDocument()) return;

            FileName = "New.xml";
            rpfFileEntry = null;

            //TODO: decide XML/REL format..?
        }
        private void OpenDocument()
        {
            if (OpenFileDialog.ShowDialog() != DialogResult.OK) return;

            if (!CloseDocument()) return;

            var fn = OpenFileDialog.FileName;

            if (!File.Exists(fn)) return; //couldn't find file?

            Xml = File.ReadAllText(fn);

            modified = false;
            FilePath = fn;
            FileName = new FileInfo(fn).Name;
            RelPropertyGrid.SelectedObject = null;
            MainTextBox.Text = "";
            rpfFileEntry = null;

            //TODO: open raw REL..?
        }
        private void SaveDocument(bool saveAs = false)
        {
            if ((metaFormat != MetaFormat.XML) && (saveAs == false))
            {
                var doc = new XmlDocument();
                try
                {
                    doc.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There's something wrong with your XML document:\r\n" + ex.Message, "Unable to parse XML");
                    return;
                }
                if (SaveRel(doc))
                {
                    return;
                }
                //if REL saving failed for whatever reason, fallback to saving the XML in the filesystem.
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

            File.WriteAllText(fn, xml);

            modified = false;
            FilePath = fn;
            FileName = new FileInfo(fn).Name;
            metaFormat = MetaFormat.XML;
        }

        private byte[] ParseSynthOutputs()
        {
            var outputs = SynthOutputsTextBox.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var outputsIndices = new byte[4];
            for (int i = 0; i < outputsIndices.Length; i++)
            {
                if (outputs.Length <= i)
                    break;

                var output = "0";
                if (outputs[i].Length > 1)
                {
                    output = outputs[i].Substring(1);
                }
                if (byte.TryParse(output, out byte ind))
                {
                    outputsIndices[i] = ind;
                }
            }

            return outputsIndices;
        }

        private Dat10SynthVariable[] ParseSynthVariables()
        {
            var variablesLines = SynthVariablesTextBox.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var variables = new Dat10SynthVariable[variablesLines.Length];
            for (int i = 0; i < variablesLines.Length; i++)
            {
                var parts = variablesLines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                variables[i] = new Dat10SynthVariable
                {
                    Name = XmlMeta.GetHash((parts.Length > 0) ? parts[0] : ""),
                    Value = FloatUtil.Parse((parts.Length > 1) ? parts[1] : ""),
                };
            }

            return variables;
        }

        private Dat10Synth AssembleSynth()
        {
            var outputs = ParseSynthOutputs();
            var variables = ParseSynthVariables();

            bool error = false;
            var result = Dat10Synth.Assemble(SynthTextBox.Text, variables, (errorMsg, lineNumber) => error = true);

            if (error)
                return null;

            currentSynth = new Dat10Synth(CurrentFile)
            {
                BuffersCount = result.BuffersCount,
                RegistersCount = result.RegistersCount,
                StateBlocksCount = result.StateBlocksCount,
                ConstantsCount = result.Constants.Count,
                Constants = result.Constants.ToArray(),
                Bytecode = result.Bytecode,
                OutputsCount = outputs.Length,
                OutputsIndices = outputs,
                VariablesCount = variables.Length,
                Variables = variables,
                RuntimeCost = 123
            };

            return currentSynth;
        }



        Style BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        Style RedStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
        Style MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        Style OrangeRedStyle = new TextStyle(Brushes.OrangeRed, null, FontStyle.Regular);
        Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Regular);
        Style ErrorStyle = new TextStyle(Brushes.Red, null, FontStyle.Underline);

        private void HTMLSyntaxHighlight(Range range)
        {
            //clear style of changed range
            range.ClearStyle(BlueStyle, MaroonStyle, RedStyle);
            //tag brackets highlighting
            range.SetStyle(BlueStyle, @"<|/>|</|>");
            //tag name
            range.SetStyle(MaroonStyle, @"<(?<range>[!\w]+)");
            //end of tag
            range.SetStyle(MaroonStyle, @"</(?<range>\w+)>");
            //attributes
            range.SetStyle(RedStyle, @"(?<range>\S+?)='[^']*'|(?<range>\S+)=""[^""]*""|(?<range>\S+)=\S+");
            //attribute values
            range.SetStyle(BlueStyle, @"\S+?=(?<range>'[^']*')|\S+=(?<range>""[^""]*"")|\S+=(?<range>\S+)");
        }



        private void Search()
        {
            SearchResultsGrid.SelectedObject = null;

            if (CurrentFile?.RelDatasSorted == null) return;


            bool textsearch = SearchTextRadio.Checked;
            var text = SearchTextBox.Text;
            var textl = text.ToLowerInvariant();

            uint hash = 0;
            uint hashl = 0;
            if (!uint.TryParse(text, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(text);
                JenkIndex.Ensure(text);
                hashl = JenkHash.GenHash(textl);
                JenkIndex.Ensure(textl);
            }
            else
            {
                hashl = hash;
            }


            var results = new List<RelData>();

            foreach (var rd in CurrentFile.RelDatasSorted)
            {
                if (textsearch)
                {
                    if (((rd.Name?.ToLowerInvariant().Contains(textl)) ?? false) || (rd.NameHash == hash) || (rd.NameHash == hashl) ||
                        (rd.NameHash.ToString().ToLowerInvariant().Contains(textl)))
                    {
                        results.Add(rd);
                    }
                }
                else
                {
                    if ((rd.NameHash == hash) || (rd.NameHash == hashl))
                    {
                        SearchResultsGrid.SelectedObject = rd;
                        return;
                    }
                }
            }

            if (textsearch && (results.Count > 0))
            {
                SearchResultsGrid.SelectedObject = results.ToArray();
            }
            else
            {
                SearchResultsGrid.SelectedObject = null;
            }



        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search();
            }
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

        private void XmlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!LoadingXml)
            {
                xml = XmlTextBox.Text;
                modified = true;
            }
        }

        private void XmlTextBox_VisibleRangeChangedDelayed(object sender, EventArgs e)
        {
            //this approach is much faster to load, but no outlining is available

            //highlight only visible area of text
            if (DelayHighlight)
            {
                HTMLSyntaxHighlight(XmlTextBox.VisibleRange);
            }
        }

        private void SynthAssemblySyntaxHighlight(Range range)
        {
            //clear style of changed range
            range.ClearStyle(GreenStyle, OrangeRedStyle, BlueStyle, MaroonStyle);

            // comments
            range.SetStyle(GreenStyle, @";.*");
            // registers/variables/buffers
            range.SetStyle(MaroonStyle, @"(?<![a-zA-Z_0-9])[RVB][0-9]+");
            // numbers
            range.SetStyle(BlueStyle, @"(?<![a-zA-Z_0-9])[0-9]+(.[0-9]+)*");
            // inputs/outputs separator
            range.SetStyle(OrangeRedStyle, @"=>");
        }

        private void SynthsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dat10Synth synth = (Dat10Synth)SynthsComboBox.SelectedItem;
            if (synth == null)
            {
                return;
            }

            loadingSynth = true;

            SynthTextBox.Text = Dat10Synth.Disassemble(synth.Bytecode, synth.Constants, synth.Variables, false).Disassembly;
            SynthTextBox.ClearUndo();
            SynthAssemblySyntaxHighlight(SynthTextBox.Range);

            SynthVariablesTextBox.Clear();
            SynthVariablesTextBox.Text = string.Join(Environment.NewLine, synth.Variables.Select(v => $"{RelXml.HashString(v.Name)} {FloatUtil.ToString(v.Value)}"));

            SynthOutputsTextBox.Clear();
            SynthOutputsTextBox.Text = string.Join(" ", synth.OutputsIndices.Take(synth.OutputsCount).Select(bufferIdx => $"B{bufferIdx}"));

            currentSynth = synth;

            loadingSynth = false;
        }

        private void SynthTextBox_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            string errors = "";
            SynthTextBox.Range.ClearStyle(ErrorStyle);
            SynthAssemblySyntaxHighlight(SynthTextBox.Range);
            var variables = ParseSynthVariables();
            Dat10Synth.Assemble(SynthTextBox.Text, variables, (errorMsg, lineNumber) =>
            {
                errors += $"Line {lineNumber}: {errorMsg}" + Environment.NewLine;
                var line = SynthTextBox.GetLine(lineNumber - 1);
                line.ClearStyle(GreenStyle, OrangeRedStyle, BlueStyle, MaroonStyle, RedStyle);
                line.SetStyle(ErrorStyle);
            });
            StatusLabel.Text = errors;
        }

        private void SynthCopyXMLButton_Click(object sender, EventArgs e)
        {
            var newSynth = AssembleSynth();
            if (newSynth == null)
            {
                StatusLabel.Text = "Failed to assemble synth";
            }
            else
            {
                var sb = new StringBuilder();
                newSynth.WriteXml(sb, 0);
                Clipboard.SetText(sb.ToString());
            }
        }

        private Synthesizer synthesizer = null; // TODO(alexguirre): dispose synthesizer

        private void SynthPlayButton_Click(object sender, EventArgs e)
        {
            var newSynth = AssembleSynth();
            if (newSynth == null)
            {
                StatusLabel.Text = "Failed to assemble synth";
            }
            else
            {
//#if !DEBUG
                try
                {
//#endif
                    if (synthesizer == null)
                    {
                        synthesizer = new Synthesizer();
                        synthesizer.Stopped += (t, _) =>
                        {
                            BeginInvoke((Action)(() =>
                            {
                                SynthPlayButton.Enabled = true;
                                SynthStopButton.Enabled = false;
                            }));
                        };
                        synthesizer.FrameSynthesized += (t, _) =>
                        {
                            float[][] buffersCopy = new float[synthesizer.Buffers.Length][];
                            for (int i = 0; i < buffersCopy.Length; i++)
                            {
                                buffersCopy[i] = new float[synthesizer.Buffers[i].Length];
                                Array.Copy(synthesizer.Buffers[i], buffersCopy[i], synthesizer.Buffers[i].Length);
                            }

                            BeginInvoke((Action)(() =>
                            {
                                //for (int i = 0; i < buffersCopy.Length; i++)
                                int i = synthesizer.Synth.OutputsIndices[0];
                                try
                                {
                                    var series = plotView1.Model.Series.FirstOrDefault(p => p.Title == $"B{i}") as LineSeries;
                                    if (series != null)
                                    {
                                        series.Points.Clear();
                                        var thisBuffer = buffersCopy[i];
                                        for (int j = 0; j < thisBuffer.Length; j++)
                                        {
                                            series.Points.Add(new DataPoint(j, Math.Max(Math.Min(thisBuffer[j], 2.0f), -2.0f)));
                                        }
                                        plotView1.InvalidatePlot(true);
                                    }
                                }
                                catch { }
                            }));
                        };
                    }

                    plotView1.Model.Series.Clear();
                    //plotView1.Model.GetLegend("Main");
                    for (int i = 0; i < newSynth.BuffersCount; i++)
                    {
                        
                        plotView1.Model.Series.Add(new LineSeries { Title = $"B{i}", RenderInLegend = true, LegendKey = $"Main" });
                        //plotView1.Model.Legends.Add(new OxyPlot.Legends.Legend { LegendPosition = OxyPlot.Legends.LegendPosition.RightMiddle, LegendPlacement = OxyPlot.Legends.LegendPlacement.Outside, LegendOrientation = OxyPlot.Legends.LegendOrientation.Vertical, Key = $"B{i}" });
                    }

                    SynthPlayButton.Enabled = false;
                    SynthStopButton.Enabled = true;
                    synthesizer.Play(newSynth);
//#if !DEBUG
                }
                catch (Exception ex)
                {
                    SynthPlayButton.Enabled = true;
                    SynthStopButton.Enabled = false;
                    StatusLabel.Text = $"Synthesizer error: {ex}";
                }
//#endif
            }
        }

        private void SynthStopButton_Click(object sender, EventArgs e)
        {
            if (synthesizer != null)
                synthesizer.Stop();
        }

        private void SynthVariablesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (loadingSynth) return;
            if (currentSynth == null) return;

            currentSynth.Variables = ParseSynthVariables();

        }

        private void SynthOutputsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (loadingSynth) return;
            if (currentSynth == null) return;

            currentSynth.OutputsIndices = ParseSynthOutputs();

        }
    }
}