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
    public partial class EditMultiPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public MapSelection[] Items { get; set; }
        public MapSelection MultiItem;

        private bool populatingui = false;

        public EditMultiPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public void SetItems(MapSelection[] items)
        {
            Items = items;
            Tag = items;
            LoadItems();
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            Text = (Items?.Length ?? 0).ToString() + " item" + ((Items?.Length == 1) ? "" : "s");
        }


        private void LoadItems()
        {
            MultiItem = new MapSelection();
            MultiItem.Clear();
            MultiItem.SetMultipleSelectionItems(Items);

            if (Items == null)
            {
                PositionTextBox.Text = string.Empty;
                RotationTextBox.Text = string.Empty;
                ScaleTextBox.Text = string.Empty;
                ItemsListBox.Items.Clear();
            }
            else
            {
                populatingui = true;


                PositionTextBox.Text = FloatUtil.GetVector3String(MultiItem.MultipleSelectionCenter);
                RotationTextBox.Text = FloatUtil.GetVector4String(MultiItem.MultipleSelectionRotation.ToVector4());
                ScaleTextBox.Text = FloatUtil.GetVector3String(MultiItem.MultipleSelectionScale);
                ItemsListBox.Items.Clear();
                foreach (var item in Items)
                {
                    ItemsListBox.Items.Add(item);
                }

                populatingui = false;


            }


        }

        private void PositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Items == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(PositionTextBox.Text);

            var wf = ProjectForm.WorldForm;
            if (wf != null)
            {
                wf.BeginInvoke(new Action(() =>
                {
                    wf.ChangeMultiPosition(Items, v, false);
                }));
            }

        }

        private void RotationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Items == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector4String(RotationTextBox.Text);

            var wf = ProjectForm.WorldForm;
            if (wf != null)
            {
                wf.BeginInvoke(new Action(() =>
                {
                    wf.ChangeMultiRotation(Items, v.ToQuaternion(), false);
                }));
            }

        }

        private void ScaleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Items == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(ScaleTextBox.Text);

            var wf = ProjectForm.WorldForm;
            if (wf != null)
            {
                wf.BeginInvoke(new Action(() =>
                {
                    wf.ChangeMultiScale(Items, v, false);
                }));
            }

        }

        private void ItemsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
