﻿using CodeWalker.GameFiles;
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
using System.Xml;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Project.Panels
{
    public partial class EditProjectManifestPanel : ProjectPanel
    {
        public ProjectForm ProjectForm { get; set; }
        public ProjectFile CurrentProjectFile { get; set; }

        public EditProjectManifestPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
            Tag = "_manifest.ymf";
        }

        public override void SetTheme(ThemeBase theme)
        {
            base.SetTheme(theme);

            var txtback = SystemColors.Window;
            var indback = Color.WhiteSmoke;

            if (theme is VS2015DarkTheme)
            {
                txtback = theme.ColorPalette.MainWindowActive.Background;
                indback = theme.ColorPalette.MainWindowActive.Background;
            }

            ProjectManifestTextBox.BackColor = txtback;
            ProjectManifestTextBox.IndentBackColor = indback;

        }


        public void SetProject(ProjectFile project)
        {
            //TODO: include _manifest.ymf in project and load/save

            CurrentProjectFile = project;

            GenerateProjectManifest();
        }




        private void GenerateProjectManifest()
        {
            var sb = new StringBuilder();
            var mapdeps = new Dictionary<string, YtypFile>();
            var typdeps = new Dictionary<string, Dictionary<string, YtypFile>>();
            var interiors = new List<string>();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
            sb.AppendLine("<CPackFileMetaData>");
            sb.AppendLine("  <MapDataGroups/>");
            sb.AppendLine("  <HDTxdBindingArray/>");
            sb.AppendLine("  <imapDependencies/>");


            var getYtypName = new Func<YtypFile, string>((ytyp) =>
            {
                var ytypname = ytyp?.RpfFileEntry?.NameLower;
                if (ytyp != null)
                {
                    if (string.IsNullOrEmpty(ytypname))
                    {
                        ytypname = ytyp.RpfFileEntry?.Name?.ToLowerInvariant();
                        if (ytypname == null) ytypname = "";
                    }
                    if (ytypname.EndsWith(".ytyp"))
                    {
                        ytypname = ytypname.Substring(0, ytypname.Length - 5);
                    }
                }
                return ytypname;
            });


            if (CurrentProjectFile != null)
            {
                if (CurrentProjectFile.YmapFiles.Count > 0)
                {
                    sb.AppendLine("  <imapDependencies_2>");
                    foreach (var ymap in CurrentProjectFile.YmapFiles)
                    {
                        var ymapname = ymap.RpfFileEntry?.NameLower;
                        if (string.IsNullOrEmpty(ymapname))
                        {
                            ymapname = ymap.Name.ToLowerInvariant();
                        }
                        if (ymapname.EndsWith(".ymap"))
                        {
                            ymapname = ymapname.Substring(0, ymapname.Length - 5);
                        }

                        mapdeps.Clear();
                        bool ismilo = false;
                        if (ymap.AllEntities != null)
                        {
                            foreach (var ent in ymap.AllEntities)
                            {
                                var ytyp = ent.Archetype?.Ytyp;
                                var ytypname = getYtypName(ytyp);
                                if (ytyp != null)
                                {
                                    mapdeps[ytypname] = ytyp;
                                }

                                if (ent.IsMlo)
                                {
                                    ismilo = true;
                                    if (ent.MloInstance?.Entities != null)
                                    {
                                        Dictionary<string, YtypFile> typdepdict;
                                        if (!typdeps.TryGetValue(ytypname, out typdepdict))
                                        {
                                            typdepdict = new Dictionary<string, YtypFile>();
                                            typdeps[ytypname] = typdepdict;
                                        }
                                        foreach (var ient in ent.MloInstance.Entities)
                                        {
                                            var iytyp = ient.Archetype?.Ytyp;
                                            var iytypname = getYtypName(iytyp);
                                            if ((iytyp != null) && (iytypname != ytypname))
                                            {
                                                typdepdict[iytypname] = iytyp;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        if (ymap.GrassInstanceBatches != null)
                        {
                            foreach (var batch in ymap.GrassInstanceBatches)
                            {
                                var ytyp = batch.Archetype?.Ytyp;
                                var ytypname = getYtypName(ytyp);
                                if (ytyp != null)
                                {
                                    mapdeps[ytypname] = ytyp;
                                }
                            }
                        }

                        sb.AppendLine("    <Item>");
                        sb.AppendLine("      <imapName>" + ymapname + "</imapName>");
                        if (ismilo)
                        {
                            sb.AppendLine("      <manifestFlags>INTERIOR_DATA</manifestFlags>");
                        }
                        else
                        {
                            sb.AppendLine("      <manifestFlags/>");
                        }
                        sb.AppendLine("      <itypDepArray>");
                        foreach (var kvp in mapdeps)
                        {
                            sb.AppendLine("        <Item>" + kvp.Key + "</Item>");
                        }
                        sb.AppendLine("      </itypDepArray>");
                        sb.AppendLine("    </Item>");
                    }
                    sb.AppendLine("  </imapDependencies_2>");
                }
                else
                {
                    sb.AppendLine("  <imapDependencies_2/>");
                }

                if ((CurrentProjectFile.YtypFiles.Count > 0) && (ProjectForm?.GameFileCache != null))
                {
                    foreach (var ytyp in CurrentProjectFile.YtypFiles)
                    {
                        var ytypname = getYtypName(ytyp);
                        foreach (var archm in ytyp.AllArchetypes)
                        {
                            var mloa = archm as MloArchetype;
                            if (mloa != null)
                            {
                                interiors.Add(mloa.Name);
                                Dictionary<string, YtypFile> typdepdict;
                                if (!typdeps.TryGetValue(ytypname, out typdepdict))
                                {
                                    typdepdict = new Dictionary<string, YtypFile>();
                                    typdeps[ytypname] = typdepdict;
                                }
                                if (mloa.entities != null)
                                {
                                    foreach (var ent in mloa.entities)
                                    {
                                        var archname = ent._Data.archetypeName;
                                        var arch = ProjectForm.GameFileCache.GetArchetype(archname);
                                        var iytyp = arch?.Ytyp;
                                        var iytypname = getYtypName(iytyp);
                                        if ((iytyp != null) && (iytypname != ytypname))
                                        {
                                            typdepdict[iytypname] = iytyp;
                                        }
                                    }
                                }
                                if (mloa.entitySets != null)
                                {
                                    foreach (var entset in mloa.entitySets)
                                    {
                                        if (entset.Entities != null)
                                        {
                                            foreach (var ent in entset.Entities)
                                            {
                                                var archname = ent._Data.archetypeName;
                                                var arch = ProjectForm.GameFileCache.GetArchetype(archname);
                                                var iytyp = arch?.Ytyp;
                                                var iytypname = getYtypName(iytyp);
                                                if ((iytyp != null) && (iytypname != ytypname))
                                                {
                                                    typdepdict[iytypname] = iytyp;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }

            if (typdeps.Count > 0)
            {
                sb.AppendLine("  <itypDependencies_2>");
                foreach (var kvp1 in typdeps)
                {
                    sb.AppendLine("    <Item>");
                    sb.AppendLine("      <itypName>" + kvp1.Key + "</itypName>");
                    sb.AppendLine("      <manifestFlags>INTERIOR_DATA</manifestFlags>");
                    sb.AppendLine("      <itypDepArray>");
                    foreach (var kvp2 in kvp1.Value)
                    {
                        sb.AppendLine("        <Item>" + kvp2.Key + "</Item>");
                    }
                    sb.AppendLine("      </itypDepArray>");
                    sb.AppendLine("    </Item>");
                }
                sb.AppendLine("  </itypDependencies_2>");
            }
            else
            {
                sb.AppendLine("  <itypDependencies_2/>");
            }

            if (interiors.Count > 0)
            {
                sb.AppendLine("  <Interiors itemType=\"CInteriorBoundsFiles\">");
                foreach (var interior in interiors)
                {
                    sb.AppendLine("    <Item>");
                    sb.AppendLine("      <Name>" + interior + "</Name>");
                    sb.AppendLine("      <Bounds>");
                    sb.AppendLine("        <Item>" + interior + "</Item>");
                    sb.AppendLine("      </Bounds>");
                    sb.AppendLine("    </Item>");
                }
                sb.AppendLine("  </Interiors>");
            }
            else
            {
                sb.AppendLine("  <Interiors/>");
            }
            sb.AppendLine("</CPackFileMetaData>");

            ProjectManifestTextBox.Text = sb.ToString();
            Text = "_manifest.ymf*";
        }

        private void ProjectManifestGenerateButton_Click(object sender, EventArgs e)
        {
            CurrentProjectFile = ProjectForm.CurrentProjectFile;
            GenerateProjectManifest();
        }

        private void SaveManifestButton_Click(object sender, EventArgs e)
        {

            if (SaveFileDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                var filename = SaveFileDialog.FileName;
                var xml = ProjectManifestTextBox.Text;
                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);
                var pso = XmlPso.GetPso(xmldoc);
                var bytes = pso.Save();
                File.WriteAllBytes(filename, bytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving _manifest.ymf file:\n" + ex.ToString());
            }

        }
    }
}
