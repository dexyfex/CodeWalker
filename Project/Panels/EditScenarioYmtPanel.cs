using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Project.Panels
{
    public partial class EditScenarioYmtPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YmtFile CurrentScenario { get; set; }

        private bool populatingui = false;
        private bool waschanged = false;

        public EditScenarioYmtPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetScenarioYmt(YmtFile scenario)
        {
            CurrentScenario = scenario;
            Tag = scenario;
            UpdateFormTitle();
            UpdateScenarioUI();
            waschanged = scenario?.HasChanged ?? false;
        }

        public void UpdateFormTitleYmtChanged()
        {
            bool changed = CurrentScenario.HasChanged;
            if (!waschanged && changed)
            {
                UpdateFormTitle();
                waschanged = true;
            }
            else if (waschanged && !changed)
            {
                UpdateFormTitle();
                waschanged = false;
            }
        }
        private void UpdateFormTitle()
        {
            string fn = CurrentScenario.RpfFileEntry?.Name ?? CurrentScenario.Name;
            if (string.IsNullOrEmpty(fn)) fn = "untitled.ymt";
            Text = fn + (CurrentScenario.HasChanged ? "*" : "");
        }


        public void UpdateScenarioUI()
        {
            if (CurrentScenario == null)
            {
                populatingui = true;
                //ScenarioYmtPanel.Enabled = false;
                ScenarioYmtNameTextBox.Text = string.Empty;
                ScenarioYmtVersionTextBox.Text = string.Empty;
                ScenarioYmtGridMinTextBox.Text = string.Empty;
                ScenarioYmtGridMaxTextBox.Text = string.Empty;
                ScenarioYmtGridScaleTextBox.Text = string.Empty;
                ScenarioYmtGridInfoLabel.Text = "Total grid points: 0";
                ScenarioYmtExtentsMinTextBox.Text = string.Empty;
                ScenarioYmtExtentsMaxTextBox.Text = string.Empty;
                ScenarioYmtFileLocationTextBox.Text = string.Empty;
                ScenarioYmtProjectPathTextBox.Text = string.Empty;
                populatingui = false;
            }
            else
            {
                var rgn = CurrentScenario.CScenarioPointRegion;
                var accg = rgn?._Data.AccelGrid ?? new rage__spdGrid2D();
                var bvh = CurrentScenario.ScenarioRegion?.BVH;
                var emin = bvh?.Box.Minimum ?? Vector3.Zero;
                var emax = bvh?.Box.Maximum ?? Vector3.Zero;

                populatingui = true;
                //ScenarioYmtPanel.Enabled = true;
                ScenarioYmtNameTextBox.Text = CurrentScenario.Name;
                ScenarioYmtVersionTextBox.Text = rgn?.VersionNumber.ToString() ?? "";
                ScenarioYmtGridMinTextBox.Text = FloatUtil.GetVector2String(accg.Min);
                ScenarioYmtGridMaxTextBox.Text = FloatUtil.GetVector2String(accg.Max);
                ScenarioYmtGridScaleTextBox.Text = FloatUtil.GetVector2String(accg.Scale);
                ScenarioYmtGridInfoLabel.Text = "Total grid points: " + (rgn?.Unk_3844724227?.Length ?? 0).ToString();
                ScenarioYmtExtentsMinTextBox.Text = FloatUtil.GetVector3String(emin);
                ScenarioYmtExtentsMaxTextBox.Text = FloatUtil.GetVector3String(emax);
                ScenarioYmtFileLocationTextBox.Text = CurrentScenario.RpfFileEntry?.Path ?? "";
                ScenarioYmtProjectPathTextBox.Text = (ProjectForm.CurrentProjectFile != null) ? ProjectForm.CurrentProjectFile.GetRelativePath(CurrentScenario.FilePath) : CurrentScenario.FilePath;
                populatingui = false;
            }
        }

        private void ScenarioYmtVersionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenario == null) return;
            if (CurrentScenario.CScenarioPointRegion == null) return;
            lock (ProjectForm.ProjectSyncRoot)
            {
                int v = 0;
                int.TryParse(ScenarioYmtVersionTextBox.Text, out v);
                if (CurrentScenario.CScenarioPointRegion.VersionNumber != v)
                {
                    CurrentScenario.CScenarioPointRegion.VersionNumber = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }
    }
}
