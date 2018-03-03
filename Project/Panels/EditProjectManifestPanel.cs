using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            StringBuilder sb = new StringBuilder();
            Dictionary<string, YtypFile> deps = new Dictionary<string, YtypFile>();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
            sb.AppendLine("<CPackFileMetaData>");
            sb.AppendLine("  <MapDataGroups/>");
            sb.AppendLine("  <HDTxdBindingArray/>");
            sb.AppendLine("  <imapDependencies/>");


            if ((CurrentProjectFile != null) && (CurrentProjectFile.YmapFiles.Count > 0))
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

                    deps.Clear();
                    if (ymap.AllEntities != null)
                    {
                        foreach (var ent in ymap.AllEntities)
                        {
                            var ytyp = ent.Archetype?.Ytyp;
                            if (ytyp != null)
                            {
                                var ytypname = ytyp.RpfFileEntry?.NameLower;
                                if (string.IsNullOrEmpty(ytypname))
                                {
                                    ytypname = ytyp.RpfFileEntry?.Name?.ToLowerInvariant();
                                    if (ytypname == null) ytypname = "";
                                }
                                if (ytypname.EndsWith(".ytyp"))
                                {
                                    ytypname = ytypname.Substring(0, ytypname.Length - 5);
                                }
                                deps[ytypname] = ytyp;
                            }
                        }
                    }

                    sb.AppendLine("    <Item>");
                    sb.AppendLine("      <imapName>" + ymapname + "</imapName>");
                    sb.AppendLine("      <manifestFlags/>");
                    sb.AppendLine("      <itypDepArray>");
                    foreach (var kvp in deps)
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

            sb.AppendLine("  <itypDependencies_2/>");
            sb.AppendLine("  <Interiors/>");
            sb.AppendLine("</CPackFileMetaData>");

            ProjectManifestTextBox.Text = sb.ToString();
            Text = "_manifest.ymf*";
        }

        private void ProjectManifestGenerateButton_Click(object sender, EventArgs e)
        {
            CurrentProjectFile = ProjectForm.CurrentProjectFile;
            GenerateProjectManifest();
        }
    }
}
