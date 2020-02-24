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

namespace CodeWalker.Forms
{
    public partial class FxcForm : Form
    {
        private FxcFile Fxc;

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




        public FxcForm()
        {
            InitializeComponent();
        }




        private void UpdateFormTitle()
        {
            Text = fileName + " - FXC Viewer - CodeWalker by dexyfex";
        }


        public void LoadFxc(FxcFile fxc)
        {
            Fxc = fxc;

            fileName = fxc?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = fxc?.FileEntry?.Name;
            }

            UpdateFormTitle();

            DetailsPropertyGrid.SelectedObject = fxc;


            ShadersListView.Items.Clear();
            TechniquesListView.Items.Clear();
            if ((fxc == null) || (fxc.Shaders == null)) return;

            foreach (var shader in fxc.Shaders)
            {
                var item = ShadersListView.Items.Add(shader.Name);
                item.Tag = shader;
            }

            if (fxc.Techniques != null)
            {
                foreach (var technique in fxc.Techniques)
                {
                    var item = TechniquesListView.Items.Add(technique.ToString());
                    item.Tag = technique;
                }
            }


            if (!string.IsNullOrEmpty(fxc.LastError))
            {
                StatusLabel.Text = fxc.LastError;
            }
            else
            {
                StatusLabel.Text = (fxc.Shaders?.Length??0).ToString() + " shaders, " + (fxc.Techniques?.Length??0).ToString() + " techniques";
            }
        }


        private void LoadShader(FxcShader s)
        {
            if (s == null)
            {
                ShaderPanel.Enabled = false;
                ShaderTextBox.Text = string.Empty;
            }
            else
            {
                ShaderPanel.Enabled = true;
                FxcParser.ParseShader(s);
                if (!string.IsNullOrEmpty(s.LastError))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Error: ");
                    sb.AppendLine(s.LastError);
                    sb.AppendLine();
                    sb.AppendLine(s.Disassembly);
                    ShaderTextBox.Text = sb.ToString();
                }
                else
                {
                    ShaderTextBox.Text = s.Disassembly;
                }
            }
        }

        private void LoadTechnique(FxcTechnique t)
        {
            if (t == null)
            {
                TechniquePanel.Enabled = false;
                TechniqueTextBox.Text = string.Empty;
            }
            else
            {
                TechniquePanel.Enabled = true;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("technique " + t.Name);
                sb.AppendLine("{");
                if (t.Passes != null)
                {
                    for (int i = 0; i < t.Passes.Length; i++)
                    {
                        var pass = t.Passes[i];
                        sb.AppendLine(" pass p" + i.ToString());// + pass.ToString());
                        sb.AppendLine(" {");

                        var vs = Fxc?.GetVS(pass.VS);
                        var ps = Fxc?.GetPS(pass.PS);
                        var cs = Fxc?.GetCS(pass.CS);
                        var ds = Fxc?.GetDS(pass.DS);
                        var gs = Fxc?.GetGS(pass.GS);
                        var hs = Fxc?.GetHS(pass.HS);

                        if (vs != null) sb.AppendLine("  vertexShader = " + vs.Name + "();");
                        if (ps != null) sb.AppendLine("  pixelShader = " + ps.Name + "();");
                        if (cs != null) sb.AppendLine("  computeShader = " + cs.Name + "();");
                        if (ds != null) sb.AppendLine("  domainShader = " + ds.Name + "();");
                        if (gs != null) sb.AppendLine("  geometryShader = " + gs.Name + "();");
                        if (hs != null) sb.AppendLine("  hullShader = " + hs.Name + "();");

                        if ((pass.Params != null) && (pass.Params.Length > 0))
                        {
                            //TODO: properly display the params (what are they all? cbuffers etc)

                            //sb.AppendLine();
                            //foreach (var param in pass.Params)
                            //{
                            //    sb.AppendLine("  " + param.ToString());
                            //}
                        }
                        sb.AppendLine(" }");
                    }
                }
                sb.AppendLine("}");
                TechniqueTextBox.Text = sb.ToString();
            }
        }


        private void ShadersListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            FxcShader s = null;
            if (ShadersListView.SelectedItems.Count == 1)
            {
                s = ShadersListView.SelectedItems[0].Tag as FxcShader;
            }

            LoadShader(s);

        }

        private void TechniquesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            FxcTechnique t = null;
            if (TechniquesListView.SelectedItems.Count == 1)
            {
                t = TechniquesListView.SelectedItems[0].Tag as FxcTechnique;
            }

            LoadTechnique(t);

        }
    }
}
