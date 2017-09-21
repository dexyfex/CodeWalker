using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.WinForms
{
    public partial class ReadOnlyPropertyGrid : PropertyGridFix
    {
        public ReadOnlyPropertyGrid()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }



        private bool _readOnly = true;
        public bool ReadOnly
        {
            get { return _readOnly; }
            set
            {
                _readOnly = value;
                SetObjectAsReadOnly(SelectedObject, _readOnly);
            }
        }

        private TypeDescriptionProvider provider = null;
        private object providedObject = null;

        protected override void OnSelectedObjectsChanged(EventArgs e)
        {
            if (providedObject != null)
            {
                SetObjectAsReadOnly(providedObject, false);
            }
            SetObjectAsReadOnly(SelectedObject, _readOnly);
            base.OnSelectedObjectsChanged(e);
        }

        private void SetObjectAsReadOnly(object selectedObject, bool isReadOnly)
        {
            if (SelectedObject != null)
            {
                if (isReadOnly)
                {
                    provider = TypeDescriptor.AddAttributes(SelectedObject, new Attribute[] { new ReadOnlyAttribute(_readOnly) });
                    providedObject = SelectedObject;
                }
                else if (provider != null)
                {
                    TypeDescriptor.RemoveProvider(provider, this.SelectedObject);
                    provider = null;
                    providedObject = null;
                }
                Refresh();
            }
        }
    }
}
