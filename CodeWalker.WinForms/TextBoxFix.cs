using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CodeWalker.WinForms
{
    public partial class TextBoxFix : TextBox
    {
        bool ignoreChange = true;
        List<string> storageUndo = null;
        List<string> storageRedo = null;
        public TextBoxFix()
        {
            InitializeComponent();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            storageRedo = new List<string>();
            storageUndo = new List<string> { Text };
            ignoreChange = false;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //Fix for Ctrl-A, Ctrl-Z and Ctrl-Y not working in multiline textboxes.
            const int WM_KEYDOWN = 0x100;
            var keyCode = (Keys)(msg.WParam.ToInt32() & Convert.ToInt32(Keys.KeyCode));
            if (msg.Msg == WM_KEYDOWN && (ModifierKeys == Keys.Control) && Focused)
            {
                if (keyCode == Keys.A)
                {
                    SelectAll();
                    return true;
                }
                if (keyCode == Keys.Z)
                {
                    ClearUndo();
                    ignoreChange = true;
                    Undo();
                    ignoreChange = false;
                }
                else if (keyCode == Keys.Y)
                {
                    ClearUndo();
                    ignoreChange = true;
                    Redo();
                    ignoreChange = false;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (!ignoreChange)
            {
                ClearUndo();
                if (storageUndo.Count > 2048) storageUndo.RemoveAt(0);
                if (storageRedo.Count > 2048) storageRedo.RemoveAt(0);

                storageUndo.Add(Text);

            }
        }

        public void Redo()
        {
            if (storageRedo.Count > 0)
            {
                ignoreChange = true;
                Text = storageRedo[storageRedo.Count - 1];
                storageUndo.Add(Text);
                storageRedo.RemoveAt(storageRedo.Count - 1);
                ignoreChange = false;
            }
        }

        public new void Undo()
        {
            if (storageUndo.Count > 1)
            {
                ignoreChange = true;
                storageRedo.Add(Text);
                Text = storageUndo[storageUndo.Count - 2];
                storageUndo.RemoveAt(storageUndo.Count - 1);
                ignoreChange = false;
            }
        }
    }
}
