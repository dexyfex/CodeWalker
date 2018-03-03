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

namespace CodeWalker.Project.Panels
{
    public partial class EditYmapGrassPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YmapGrassInstanceBatch CurrentBatch { get; set; }

        //private bool populatingui = false;

        public EditYmapGrassPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetBatch(YmapGrassInstanceBatch batch)
        {
            CurrentBatch = batch;
            Tag = batch;
            LoadGrassBatch();
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentBatch?.Batch.archetypeName.ToString() ?? "Grass Batch";
        }



        private void LoadGrassBatch()
        {
        }

    }
}
