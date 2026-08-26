using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker
{

    public static class DarkTheme
    {
        public static readonly Color Window = Color.FromArgb(37, 37, 38);
        public static readonly Color Surface = Color.FromArgb(45, 45, 48);
        public static readonly Color Field = Color.FromArgb(30, 30, 30);
        public static readonly Color Border = Color.FromArgb(63, 63, 70);
        public static readonly Color Text = Color.FromArgb(241, 241, 241);
        public static readonly Color DimText = Color.FromArgb(150, 150, 150);
        public static readonly Color Accent = Color.FromArgb(0, 122, 204);

        private const int DwmUseImmersiveDarkMode = 20;
        private const int DwmUseImmersiveDarkModeLegacy = 19;

        private static readonly HashSet<Control> ThemedControls = new HashSet<Control>();
        private static ThemeBase dockTheme;
        private static bool installed;


        public static ThemeBase DockTheme
        {
            get { return dockTheme ?? (dockTheme = new VS2015DarkTheme()); }
        }


        public static void Install()
        {
            if (installed) return;
            installed = true;

            ToolStripManager.Renderer = new DarkToolStripRenderer();
            Application.Idle += ApplicationIdle;
        }

        public static void Apply(Control control)
        {
            if (control == null) return;
            if (control.IsDisposed) return;
            if (ThemedControls.Add(control) == false) return;

            control.Disposed += ControlDisposed;
            control.ControlAdded += ControlAdded;

            StyleControl(control);

            foreach (Control child in control.Controls)
            {
                Apply(child);
            }
        }


        private static void ApplicationIdle(object sender, EventArgs e)
        {
            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                Apply(Application.OpenForms[i]);
            }
        }

        private static void ControlAdded(object sender, ControlEventArgs e)
        {
            Apply(e.Control);
        }

        private static void ControlDisposed(object sender, EventArgs e)
        {
            var control = sender as Control;
            if (control == null) return;
            control.Disposed -= ControlDisposed;
            control.ControlAdded -= ControlAdded;
            ThemedControls.Remove(control);
        }

        private static void StyleControl(Control control)
        {
            var form = control as Form;
            if (form != null)
            {
                form.BackColor = Window;
                form.ForeColor = Text;
                StyleTitleBar(form);
            }

            StyleMenu(control.ContextMenuStrip);

            var textBox = control as TextBoxBase;
            if (textBox != null)
            {
                textBox.BackColor = textBox.ReadOnly ? Surface : Field;
                textBox.ForeColor = textBox.ReadOnly ? DimText : Text;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                return;
            }

            var comboBox = control as ComboBox;
            if (comboBox != null)
            {
                comboBox.FlatStyle = FlatStyle.Flat;
                comboBox.BackColor = Field;
                comboBox.ForeColor = Text;
                return;
            }

            var listBox = control as ListBox;
            if (listBox != null)
            {
                listBox.BackColor = Field;
                listBox.ForeColor = Text;
                listBox.BorderStyle = BorderStyle.FixedSingle;
                return;
            }

            var listView = control as ListView;
            if (listView != null)
            {
                StyleListView(listView);
                return;
            }

            var treeView = control as TreeView;
            if (treeView != null)
            {
                treeView.BackColor = Field;
                treeView.ForeColor = Text;
                treeView.LineColor = DimText;
                treeView.BorderStyle = BorderStyle.FixedSingle;
                return;
            }

            var grid = control as DataGridView;
            if (grid != null)
            {
                StyleDataGrid(grid);
                return;
            }

            var propertyGrid = control as PropertyGrid;
            if (propertyGrid != null)
            {
                StylePropertyGrid(propertyGrid);
                return;
            }

            var upDown = control as UpDownBase;
            if (upDown != null)
            {
                upDown.BackColor = Field;
                upDown.ForeColor = Text;
                upDown.BorderStyle = BorderStyle.FixedSingle;
                return;
            }

            var dateTime = control as DateTimePicker;
            if (dateTime != null)
            {
                dateTime.CalendarMonthBackground = Field;
                dateTime.CalendarForeColor = Text;
                dateTime.CalendarTitleBackColor = Surface;
                dateTime.CalendarTitleForeColor = Text;
                return;
            }

            var button = control as Button;
            if (button != null)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.BackColor = Surface;
                button.ForeColor = Text;
                button.FlatAppearance.BorderColor = Border;
                button.FlatAppearance.MouseOverBackColor = Border;
                button.FlatAppearance.MouseDownBackColor = Accent;
                return;
            }

            var linkLabel = control as LinkLabel;
            if (linkLabel != null)
            {
                linkLabel.LinkColor = Accent;
                linkLabel.ActiveLinkColor = Text;
                linkLabel.VisitedLinkColor = Accent;
                return;
            }

            var tabControl = control as TabControl;
            if (tabControl != null)
            {
                StyleTabControl(tabControl);
                return;
            }

            var tabPage = control as TabPage;
            if (tabPage != null)
            {
                tabPage.BackColor = Window;
                tabPage.ForeColor = Text;
                return;
            }

            var splitContainer = control as SplitContainer;
            if (splitContainer != null)
            {
                splitContainer.BackColor = Border;
                return;
            }

            var strip = control as ToolStrip;
            if (strip != null)
            {
                strip.BackColor = Surface;
                strip.ForeColor = Text;
                return;
            }

            var editor = control as FastColoredTextBox;
            if (editor != null)
            {
                StyleTextEditor(editor);
                return;
            }
        }

        private static void StyleTitleBar(Form form)
        {
            if (form.IsHandleCreated == false)
            {
                form.HandleCreated += (s, e) => StyleTitleBar(form);
                return;
            }

            var enabled = 1;
            if (DwmSetWindowAttribute(form.Handle, DwmUseImmersiveDarkMode, ref enabled, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(form.Handle, DwmUseImmersiveDarkModeLegacy, ref enabled, sizeof(int));
            }
        }

        private static void StyleMenu(ToolStrip menu)
        {
            if (menu == null) return;
            menu.BackColor = Surface;
            menu.ForeColor = Text;
        }

        private static void StyleListView(ListView listView)
        {
            listView.BackColor = Field;
            listView.ForeColor = Text;
            listView.BorderStyle = BorderStyle.FixedSingle;

            if (listView.OwnerDraw) return;
            listView.OwnerDraw = true;
            listView.DrawColumnHeader += DrawListViewColumnHeader;
            listView.DrawItem += DrawListViewItem;
            listView.DrawSubItem += DrawListViewSubItem;
        }

        private static void DrawListViewColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var back = new SolidBrush(Surface))
            using (var fore = new SolidBrush(Text))
            using (var pen = new Pen(Border))
            using (var format = new StringFormat())
            {
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.NoWrap;

                e.Graphics.FillRectangle(back, e.Bounds);
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
                e.Graphics.DrawString(e.Header.Text, e.Font, fore, Rectangle.Inflate(e.Bounds, -4, 0), format);
            }
        }

        private static void DrawListViewItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private static void DrawListViewSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private static void StyleTabControl(TabControl tabControl)
        {
            if (tabControl.DrawMode == TabDrawMode.OwnerDrawFixed) return;
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += DrawTabControlItem;
        }

        private static void DrawTabControlItem(object sender, DrawItemEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null) return;
            if ((e.Index < 0) || (e.Index >= tabControl.TabPages.Count)) return;

            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var strip = new SolidBrush(Window))
            using (var back = new SolidBrush(selected ? Surface : Window))
            using (var fore = new SolidBrush(selected ? Text : DimText))
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                if (e.Index == 0)
                {
                    var display = tabControl.DisplayRectangle;
                    var client = tabControl.ClientRectangle;
                    e.Graphics.FillRectangle(strip, client.Left, client.Top, client.Width, display.Top - client.Top);
                }

                e.Graphics.FillRectangle(back, e.Bounds);
                e.Graphics.DrawString(tabControl.TabPages[e.Index].Text, e.Font, fore, e.Bounds, format);
            }
        }

        private static void StyleDataGrid(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = Window;
            grid.GridColor = Border;
            grid.BorderStyle = BorderStyle.FixedSingle;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Surface;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Border;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;

            grid.RowHeadersDefaultCellStyle.BackColor = Surface;
            grid.RowHeadersDefaultCellStyle.ForeColor = Text;
            grid.RowHeadersDefaultCellStyle.SelectionBackColor = Border;
            grid.RowHeadersDefaultCellStyle.SelectionForeColor = Text;

            grid.DefaultCellStyle.BackColor = Field;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = Accent;
            grid.DefaultCellStyle.SelectionForeColor = Text;
        }

        private static void StylePropertyGrid(PropertyGrid grid)
        {
            grid.BackColor = Surface;
            grid.ForeColor = Text;
            grid.ViewBackColor = Field;
            grid.ViewForeColor = Text;
            grid.ViewBorderColor = Border;
            grid.LineColor = Border;
            grid.CategoryForeColor = Text;
            grid.CategorySplitterColor = Border;
            grid.HelpBackColor = Surface;
            grid.HelpForeColor = Text;
            grid.HelpBorderColor = Border;
            grid.CommandsBackColor = Surface;
            grid.CommandsForeColor = Text;
            grid.CommandsBorderColor = Border;
            grid.DisabledItemForeColor = DimText;
        }

        private static void StyleTextEditor(FastColoredTextBox editor)
        {
            editor.BackColor = Field;
            editor.ForeColor = Text;
            editor.IndentBackColor = Window;
            editor.PaddingBackColor = Window;
            editor.TextAreaBorderColor = Border;
            editor.ServiceLinesColor = Border;
            editor.LineNumberColor = DimText;
            editor.CaretColor = Text;
            editor.CurrentLineColor = Surface;
            editor.ChangedLineColor = Color.FromArgb(60, 60, 40);
            editor.FoldingIndicatorColor = Accent;
            editor.BookmarkColor = Accent;
            editor.DisabledColor = Surface;
            editor.SelectionColor = Color.FromArgb(80, 0, 122, 204);

            StyleSyntax(editor);
        }

        private static void StyleSyntax(FastColoredTextBox editor)
        {
            var highlighter = editor.SyntaxHighlighter;
            if (highlighter == null) return;

            SetStyle(highlighter.BlackStyle, Text, FontStyle.Regular);
            SetStyle(highlighter.BlueStyle, Color.FromArgb(86, 156, 214), FontStyle.Regular);
            SetStyle(highlighter.BlueBoldStyle, Color.FromArgb(86, 156, 214), FontStyle.Bold);
            SetStyle(highlighter.BoldStyle, Text, FontStyle.Bold);
            SetStyle(highlighter.BrownStyle, Color.FromArgb(206, 145, 120), FontStyle.Regular);
            SetStyle(highlighter.GrayStyle, DimText, FontStyle.Regular);
            SetStyle(highlighter.GreenStyle, Color.FromArgb(106, 153, 85), FontStyle.Italic);
            SetStyle(highlighter.MagentaStyle, Color.FromArgb(197, 134, 192), FontStyle.Regular);
            SetStyle(highlighter.MaroonStyle, Color.FromArgb(156, 220, 254), FontStyle.Regular);
            SetStyle(highlighter.RedStyle, Color.FromArgb(214, 157, 133), FontStyle.Regular);

            editor.ClearStylesBuffer();
            editor.Range.ClearStyle(StyleIndex.All);
            editor.OnSyntaxHighlight(new TextChangedEventArgs(editor.Range));
            editor.Invalidate();
        }

        private static void SetStyle(Style style, Color colour, FontStyle fontStyle)
        {
            var textStyle = style as TextStyle;
            if (textStyle == null) return;
            textStyle.ForeBrush = new SolidBrush(colour);
            textStyle.FontStyle = fontStyle;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);


        private class DarkToolStripRenderer : ToolStripProfessionalRenderer
        {
            public DarkToolStripRenderer()
                : base(new DarkColourTable())
            {
                RoundedEdges = false;
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? Text : DimText;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = e.Item.Enabled ? Text : DimText;
                base.OnRenderArrow(e);
            }
        }

        private class DarkColourTable : ProfessionalColorTable
        {
            public override Color MenuStripGradientBegin { get { return Surface; } }
            public override Color MenuStripGradientEnd { get { return Surface; } }
            public override Color MenuBorder { get { return Border; } }
            public override Color MenuItemBorder { get { return Accent; } }
            public override Color MenuItemSelected { get { return Border; } }
            public override Color MenuItemSelectedGradientBegin { get { return Border; } }
            public override Color MenuItemSelectedGradientEnd { get { return Border; } }
            public override Color MenuItemPressedGradientBegin { get { return Surface; } }
            public override Color MenuItemPressedGradientEnd { get { return Surface; } }
            public override Color ImageMarginGradientBegin { get { return Surface; } }
            public override Color ImageMarginGradientMiddle { get { return Surface; } }
            public override Color ImageMarginGradientEnd { get { return Surface; } }
            public override Color ToolStripDropDownBackground { get { return Surface; } }
            public override Color ToolStripGradientBegin { get { return Surface; } }
            public override Color ToolStripGradientMiddle { get { return Surface; } }
            public override Color ToolStripGradientEnd { get { return Surface; } }
            public override Color ToolStripBorder { get { return Border; } }
            public override Color ToolStripContentPanelGradientBegin { get { return Window; } }
            public override Color ToolStripContentPanelGradientEnd { get { return Window; } }
            public override Color ToolStripPanelGradientBegin { get { return Surface; } }
            public override Color ToolStripPanelGradientEnd { get { return Surface; } }
            public override Color ButtonSelectedHighlight { get { return Border; } }
            public override Color ButtonSelectedGradientBegin { get { return Border; } }
            public override Color ButtonSelectedGradientMiddle { get { return Border; } }
            public override Color ButtonSelectedGradientEnd { get { return Border; } }
            public override Color ButtonPressedGradientBegin { get { return Accent; } }
            public override Color ButtonPressedGradientMiddle { get { return Accent; } }
            public override Color ButtonPressedGradientEnd { get { return Accent; } }
            public override Color ButtonCheckedGradientBegin { get { return Accent; } }
            public override Color ButtonCheckedGradientMiddle { get { return Accent; } }
            public override Color ButtonCheckedGradientEnd { get { return Accent; } }
            public override Color CheckBackground { get { return Accent; } }
            public override Color CheckSelectedBackground { get { return Accent; } }
            public override Color CheckPressedBackground { get { return Accent; } }
            public override Color SeparatorDark { get { return Border; } }
            public override Color SeparatorLight { get { return Border; } }
            public override Color StatusStripGradientBegin { get { return Surface; } }
            public override Color StatusStripGradientEnd { get { return Surface; } }
            public override Color GripDark { get { return Border; } }
            public override Color GripLight { get { return Surface; } }
        }

    }

}
