using CodeWalker.GameFiles;
using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.World
{
    public partial class WorldSearchForm : Form
    {
        private WorldForm WorldForm;

        private CancellationTokenSource AbortOperation = new CancellationTokenSource();

        private List<Archetype> ArchetypeResults = new List<Archetype>();
        private List<YmapEntityDef> EntityResults = new List<YmapEntityDef>();

        public WorldSearchForm(WorldForm worldForm)
        {
            WorldForm = worldForm;
            InitializeComponent();
        }

        private void WorldSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            WorldForm.OnSearchFormClosed();
        }

        private void ArchetypeSearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                ArchetypeSearchButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ArchetypeSearchButton_Click(object sender, EventArgs e)
        {
            var s = ArchetypeSearchTextBox.Text;
            var loadedOnly = false;// ArchetypeSearchLoadedOnlyCheckBox.Checked; //NOT WORKING...

            var gfc = WorldForm.GameFileCache;
            if (!gfc.IsInited)
            {
                MessageBox.Show("Please wait for CodeWalker to initialise.");
                return;
            }
            if (s.Length == 0)
            {
                MessageBox.Show("Please enter a search term.");
                return;
            }
            if (s.Length < 2)
            {
                MessageBox.Show("You don't really want to search for that do you?");
                return;
            }

            ArchetypeSearchTextBox.Enabled = false;
            ArchetypeSearchButton.Enabled = false;
            ArchetypeSearchAbortButton.Enabled = true;
            ArchetypeSearchExportResultsButton.Enabled = false;
            AbortOperation.Cancel();
            AbortOperation = new CancellationTokenSource();
            ArchetypeResults.Clear();
            ArchetypeResultsListView.VirtualListSize = 0;

            s = s.ToLowerInvariant();

            _ = Task.Run(async () =>
            {

                var rpfman = gfc.RpfMan;
                RpfFile[] rpflist = (loadedOnly ? (IEnumerable<RpfFile>)gfc.ActiveMapRpfFiles.Values : rpfman.AllRpfs).ToArray();
                var results = new ConcurrentBag<Archetype>();

                var token = AbortOperation.Token;

                var hash = JenkHash.GenHashLower(s);

                var archetype = GameFileCache.Instance?.GetArchetype(hash);

                if (archetype is not null)
                {
                    ArchetypeSearchAddResult(archetype);
                    results.Add(archetype);
                }

                try
                {
                    await Parallel.ForAsync(0, rpflist.Length, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2, CancellationToken = token }, async (i, cancellationToken) =>
                    {
                        var rpf = rpflist[i];
                        foreach (var entry in rpf.AllEntries)
                        {
                            try
                            {
                                if (token.IsCancellationRequested)
                                {
                                    return;
                                }
                                if (entry.Name.EndsWith(".ytyp", StringComparison.OrdinalIgnoreCase))
                                {
                                    ArchetypeSearchUpdateStatus(entry.Path);

                                    YtypFile? ytyp = await RpfManager.GetFileAsync<YtypFile>(entry);

                                    if (ytyp == null)
                                        continue;

                                    foreach (var arch in ytyp.AllArchetypes)
                                    {
                                        if (arch.Name.Contains(s, StringComparison.OrdinalIgnoreCase)
                                            || arch.AssetName.Contains(s, StringComparison.OrdinalIgnoreCase))
                                        {
                                            ArchetypeSearchAddResult(arch);
                                            results.Add(arch);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ArchetypeSearchUpdateStatus(ex.Message);
                                Console.WriteLine(ex);
                            }
                        }
                    });

                    ArchetypeSearchUpdateStatus($"Search complete. {results.Count} archetypes found.");
                }
                catch (TaskCanceledException)
                {
                    ArchetypeSearchUpdateStatus("Search aborted!");
                }
                finally
                {
                    ArchetypeSearchComplete();
                }
            });

        }

        private async void ArchetypeSearchUpdateStatus(string text, bool force = false)
        {
            if (!force && LastUpdate.Elapsed < TimeSpan.FromSeconds(0.1))
            {
                return;
            }

            LastUpdate.Restart();
            try
            {
                await this.SwitchToUiContext();
                ArchetypeSearchStatusLabel.Text = text;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async void ArchetypeSearchAddResult(Archetype arch)
        {
            try
            {
                await this.SwitchToUiContext();
                if (ArchetypeResults.Contains(arch))
                    return;
                ArchetypeResults.Add(arch);
                ArchetypeResultsListView.VirtualListSize = ArchetypeResults.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async void ArchetypeSearchComplete()
        {
            try
            {
                await this.SwitchToUiContext();
                ArchetypeSearchTextBox.Enabled = true;
                ArchetypeSearchButton.Enabled = true;
                ArchetypeSearchAbortButton.Enabled = false;
                ArchetypeSearchExportResultsButton.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ArchetypeSearchAbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation.Cancel();
        }

        private void ArchetypeSearchExportResultsButton_Click(object sender, EventArgs e)
        {
            if (ArchetypeResults.Count == 0)
            {
                MessageBox.Show("Nothing to export!");
                return;
            }

            SaveFileDialog.FileName = $"Archetypes_{ArchetypeSearchTextBox.Text}";
            if (SaveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string fname = SaveFileDialog.FileName;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name, AssetName, YtypFile");
            foreach (var arch in ArchetypeResults)
            {
                sb.AppendLine($"{arch.Name}, {arch.AssetName}, {arch.Ytyp?.RpfFileEntry?.Path ?? ""}");
            }

            File.WriteAllText(fname, sb.ToString());
        }

        private void ArchetypeResultsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < ArchetypeResults.Count)
            {
                var arch = ArchetypeResults[e.ItemIndex];
                var li = new ListViewItem(new[] { arch.Name, arch.Ytyp?.RpfFileEntry?.Path ?? "" });
                li.Tag = arch;
                e.Item = li;
            }
            else
            {
                e.Item = new ListViewItem("Error retrieving Archetype! Please tell dexyfex");
            }
        }

        private void ArchetypeResultsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ArchetypeResultsListView.SelectedIndices.Count == 0)
            {
                ArchetypeResultPanel.Enabled = false;
                ArchetypeResultNameTextBox.Text = "";
                ArchetypeResultYtypTextBox.Text = "";
                ArchetypeResultPropertyGrid.SelectedObject = null;
            }
            else
            {
                var li = ArchetypeResultsListView.SelectedIndices[0];
                if (li < ArchetypeResults.Count)
                {
                    var arch = ArchetypeResults[li];
                    ArchetypeResultPanel.Enabled = true;
                    ArchetypeResultNameTextBox.Text = arch.Name;
                    ArchetypeResultYtypTextBox.Text = arch.Ytyp?.RpfFileEntry?.Path ?? "";
                    ArchetypeResultPropertyGrid.SelectedObject = arch;
                }
            }
        }

        private void ArchetypeResultsListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (ArchetypeResultsListView.SelectedIndices.Count > 0)
            {
                ArchetypeResultViewModelButton_Click(sender, e);
            }
        }

        private void ArchetypeResultFindEntitiesButton_Click(object sender, EventArgs e)
        {
            MainTabControl.SelectedTab = EntitySearchTabPage;
            EntitySearchTextBox.Text = ArchetypeResultNameTextBox.Text;
            EntitySearchButton_Click(sender, e);
        }

        private void ArchetypeResultViewModelButton_Click(object sender, EventArgs e)
        {
            WorldForm.ShowModel(ArchetypeResultNameTextBox.Text);
        }

        private void EntitySearchTextBox_TextChanged(object sender, EventArgs e)
        {
            //JenkHash h = new JenkHash(EntitySearchTextBox.Text, JenkHashInputEncoding.UTF8);
            //EntitySearchHashLabel.Text = "Hash: " + h.HashUint.ToString() + " (" + h.HashHex + ")";
        }

        private void EntitySearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                EntitySearchButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void EntitySearchButton_Click(object sender, EventArgs e)
        {
            var s = EntitySearchTextBox.Text;
            var loadedOnly = EntitySearchLoadedOnlyCheckBox.Checked;

            var gfc = WorldForm.GameFileCache;
            if (!gfc.IsInited)
            {
                MessageBox.Show("Please wait for CodeWalker to initialise.");
                return;
            }
            if (s.Length == 0)
            {
                MessageBox.Show("Please enter a search term.");
                return;
            }
            if (s.Length < 2)
            {
                MessageBox.Show("You don't really want to search for that do you?");
                return;
            }

            EntitySearchTextBox.Enabled = false;
            EntitySearchButton.Enabled = false;
            EntitySearchAbortButton.Enabled = true;
            EntitySearchLoadedOnlyCheckBox.Enabled = false;
            EntitySearchExportResultsButton.Enabled = false;
            EntitySearchSetMarkersButton.Enabled = false;
            AbortOperation.Cancel();
            AbortOperation = new CancellationTokenSource();
            EntityResults.Clear();
            EntityResultsListView.VirtualListSize = 0;

            s = s.ToLowerInvariant();
            //var h = JenkHash.GenHash(s, JenkHashInputEncoding.UTF8);

            _ = Task.Run(async () =>
            {

                var rpfman = gfc.RpfMan;
                RpfFile[] rpflist = (loadedOnly ? (IEnumerable<RpfFile>)gfc.ActiveMapRpfFiles.Values : rpfman.AllRpfs).ToArray();
                var results = new ConcurrentBag<YmapEntityDef>();

                var token = AbortOperation.Token;

                try
                {
                    await Parallel.ForAsync(0, rpflist.Length, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2, CancellationToken = token }, async (i, cancellationToken) =>
                    {
                        var rpf = rpflist[i];
                        foreach (var entry in rpf.AllEntries)
                        {
                            try
                            {
                                if (cancellationToken.IsCancellationRequested || AbortOperation.IsCancellationRequested)
                                    return;

                                if (entry.Name.EndsWith(".ymap", StringComparison.OrdinalIgnoreCase))
                                {
                                    EntitySearchUpdateStatus(entry.Path);

                                    var ymap = await GameFileCache.Instance.GetYmapAsync(entry.ShortNameHash);

                                    if (ymap is null || ymap.AllEntities.Length == 0)
                                        continue;

                                    foreach (var ent in ymap.AllEntities)
                                    {
                                        //if (ent._CEntityDef.archetypeName.Hash == h)
                                        if (ent.Name.Contains(s, StringComparison.OrdinalIgnoreCase))
                                        {
                                            EntitySearchAddResult(ent);
                                            results.Add(ent);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                EntitySearchUpdateStatus(ex.Message);
                                Console.WriteLine(ex);
                            }
                        }
                    });

                    EntitySearchUpdateStatus($"Search complete. {results.Count} entities found.", true);
                }
                catch(TaskCanceledException)
                {
                    EntitySearchUpdateStatus("Search aborted!", true);
                }
                finally
                {
                    EntitySearchComplete();
                }
            });
        }

        Stopwatch LastUpdate = Stopwatch.StartNew();
        private async void EntitySearchUpdateStatus(string text, bool force = false)
        {
            if (!force && LastUpdate.Elapsed < TimeSpan.FromSeconds(0.1))
            {
                return;
            }

            LastUpdate.Restart();
            try
            {
                await this.SwitchToUiContext();
                EntitySearchStatusLabel.Text = text;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async void EntitySearchAddResult(YmapEntityDef ent)
        {
            try
            {
                await this.SwitchToUiContext();
                if (EntityResults.Contains(ent))
                    return;
                EntityResults.Add(ent);
                EntityResultsListView.VirtualListSize = EntityResults.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async void EntitySearchComplete()
        {
            try
            {
                await this.SwitchToUiContext();
                EntitySearchTextBox.Enabled = true;
                EntitySearchButton.Enabled = true;
                EntitySearchAbortButton.Enabled = false;
                EntitySearchLoadedOnlyCheckBox.Enabled = true;
                EntitySearchExportResultsButton.Enabled = true;
                EntitySearchSetMarkersButton.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void EntitySearchAbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation.Cancel();
        }

        private void EntitySearchExportResultsButton_Click(object sender, EventArgs e)
        {
            if (EntityResults.Count == 0)
            {
                MessageBox.Show("Nothing to export!");
                return;
            }

            SaveFileDialog.FileName = "Entities_" + EntitySearchTextBox.Text;
            if (SaveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string fname = SaveFileDialog.FileName;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ArchetypeName, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, RotationW, YmapFile");
            foreach (var ent in EntityResults)
            {
                sb.AppendLine($"{ent.Name}, {FloatUtil.GetVector3String(ent._CEntityDef.position)}, {FloatUtil.GetVector4String(ent._CEntityDef.rotation)}, {ent.Ymap?.RpfFileEntry?.Path ?? ""}");
            }

            File.WriteAllText(fname, sb.ToString());
        }

        private void EntitySearchSetMarkersButton_Click(object sender, EventArgs e)
        {
            var usetextbox = EntityResults.Count < 250;
            if (!usetextbox) MessageBox.Show("Markers will not be placed into the markers textbox\nbecause there are too many.", "Too many markers", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Task.Run(() =>
            {
                foreach (var ent in EntityResults)
                {
                    WorldForm.AddMarker(ent.Position, ent.Name, usetextbox);
                }
            });
        }

        private void EntityResultsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < EntityResults.Count)
            {
                var ent = EntityResults[e.ItemIndex];
                var li = new ListViewItem(new[] { ent.Name, ent.Ymap?.RpfFileEntry?.Path ?? "" });
                li.Tag = ent;
                e.Item = li;
            }
            else
            {
                e.Item = new ListViewItem("Error retrieving YmapEntityDef! Please tell dexyfex");
            }
        }

        private void EntityResultsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EntityResultsListView.SelectedIndices.Count == 0)
            {
                EntityResultPanel.Enabled = false;
                EntityResultNameTextBox.Text = "";
                EntityResultYmapTextBox.Text = "";
                EntityResultPropertyGrid.SelectedObject = null;
            }
            else
            {
                var li = EntityResultsListView.SelectedIndices[0];
                if (li < EntityResults.Count)
                {
                    var ent = EntityResults[li];
                    EntityResultPanel.Enabled = true;
                    EntityResultNameTextBox.Text = ent.Name;
                    EntityResultYmapTextBox.Text = ent.Ymap?.RpfFileEntry?.Path ?? "";
                    EntityResultPropertyGrid.SelectedObject = ent;
                }
            }
        }

        private void EntityResultsListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (EntityResultsListView.SelectedIndices.Count > 0)
            {
                EntityResultGoToButton_Click(sender, e);
            }
        }

        private void EntityResultGoToButton_Click(object sender, EventArgs e)
        {
            if (EntityResultsListView.SelectedIndices.Count > 0)
            {
                var li = EntityResultsListView.SelectedIndices[0];
                if (li < EntityResults.Count)
                {
                    var ent = EntityResults[li];
                    if (ent.Archetype == null)
                    {
                        var gfc = WorldForm.GameFileCache;
                        ent.Archetype = gfc.GetArchetype(ent._CEntityDef.archetypeName);
                    }
                    WorldForm.GoToEntity(ent);
                }
            }
        }

        private void EntityResultViewModelButton_Click(object sender, EventArgs e)
        {
            WorldForm.ShowModel(EntityResultNameTextBox.Text);
        }
    }
}
