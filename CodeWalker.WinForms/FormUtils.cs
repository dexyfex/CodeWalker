using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Point = System.Drawing.Point;

namespace CodeWalker
{
    ////public static class Utils
    ////{
    ////    //unused
    ////    //public static Bitmap ResizeImage(Image image, int width, int height)
    ////    //{
    ////    //    var destRect = new Rectangle(0, 0, width, height);
    ////    //    var destImage = new Bitmap(width, height);
    ////    //    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
    ////    //    using (var graphics = Graphics.FromImage(destImage))
    ////    //    {
    ////    //        graphics.CompositingMode = CompositingMode.SourceCopy;
    ////    //        graphics.CompositingQuality = CompositingQuality.HighQuality;
    ////    //        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
    ////    //        graphics.SmoothingMode = SmoothingMode.HighQuality;
    ////    //        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
    ////    //        using (var wrapMode = new ImageAttributes())
    ////    //        {
    ////    //            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
    ////    //            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
    ////    //        }
    ////    //    }
    ////    //    return destImage;
    ////    //}
    ////}


    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ListViewExtensions
    {
        //from stackoverflow: 
        //https://stackoverflow.com/questions/254129/how-to-i-display-a-sort-arrow-in-the-header-of-a-list-view-column-using-c

        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public Mask mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public Format fmt;
            public IntPtr lParam;
            // _WIN32_IE >= 0x0300 
            public int iImage;
            public int iOrder;
            // _WIN32_IE >= 0x0500
            public uint type;
            public IntPtr pvFilter;
            // _WIN32_WINNT >= 0x0600
            public uint state;

            [Flags]
            public enum Mask
            {
                Format = 0x4,       // HDI_FORMAT
            };

            [Flags]
            public enum Format
            {
                SortDown = 0x200,   // HDF_SORTDOWN
                SortUp = 0x400,     // HDF_SORTUP
            };
        };

        public const int LVM_FIRST = 0x1000;
        public const int LVM_GETHEADER = LVM_FIRST + 31;

        public const int HDM_FIRST = 0x1200;
        public const int HDM_GETITEM = HDM_FIRST + 11;
        public const int HDM_SETITEM = HDM_FIRST + 12;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, ref HDITEM lParam);

        public static void SetSortIcon(this ListView listViewControl, int columnIndex, SortOrder order)
        {
            IntPtr columnHeader = SendMessage(listViewControl.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            for (int columnNumber = 0; columnNumber <= listViewControl.Columns.Count - 1; columnNumber++)
            {
                var columnPtr = new IntPtr(columnNumber);
                var item = new HDITEM
                {
                    mask = HDITEM.Mask.Format
                };

                if (SendMessage(columnHeader, HDM_GETITEM, columnPtr, ref item) == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                if (order != SortOrder.None && columnNumber == columnIndex)
                {
                    switch (order)
                    {
                        case SortOrder.Ascending:
                            item.fmt &= ~HDITEM.Format.SortDown;
                            item.fmt |= HDITEM.Format.SortUp;
                            break;
                        case SortOrder.Descending:
                            item.fmt &= ~HDITEM.Format.SortUp;
                            item.fmt |= HDITEM.Format.SortDown;
                            break;
                    }
                }
                else
                {
                    item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
                }

                if (SendMessage(columnHeader, HDM_SETITEM, columnPtr, ref item) == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
            }
        }











        //private const int LVM_FIRST = 0x1000;
        private const int LVM_SETITEMSTATE = LVM_FIRST + 43;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct LVITEM
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public int cColumns;
            public IntPtr puColumns;
        };

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageLVItem(IntPtr hWnd, int msg, int wParam, ref LVITEM lvi);

        /// <summary>
        /// Select all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be selected</param>
        public static void SelectAllItems(this ListView list)
        {
            SetItemState(list, -1, 2, 2);
        }

        /// <summary>
        /// Deselect all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be deselected</param>
        public static void DeselectAllItems(this ListView list)
        {
            SetItemState(list, -1, 2, 0);
        }

        /// <summary>
        /// Set the item state on the given item
        /// </summary>
        /// <param name="list">The listview whose item's state is to be changed</param>
        /// <param name="itemIndex">The index of the item to be changed</param>
        /// <param name="mask">Which bits of the value are to be set?</param>
        /// <param name="value">The value to be set</param>
        public static void SetItemState(ListView list, int itemIndex, int mask, int value)
        {
            LVITEM lvItem = new LVITEM();
            lvItem.stateMask = mask;
            lvItem.state = value;
            SendMessageLVItem(list.Handle, LVM_SETITEMSTATE, itemIndex, ref lvItem);
        }


    }


    public static class TextBoxExtension
    {
        private const int EM_SETTABSTOPS = 0x00CB;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int[] lParam);

        public static Point GetCaretPosition(this TextBox textBox)
        {
            Point point = new Point(0, 0);

            if (textBox.Focused)
            {
                point.X = textBox.SelectionStart - textBox.GetFirstCharIndexOfCurrentLine() + 1;
                point.Y = textBox.GetLineFromCharIndex(textBox.SelectionStart) + 1;
            }

            return point;
        }

        public static void SetTabStopWidth(this TextBox textbox, int width)
        {
            SendMessage(textbox.Handle, EM_SETTABSTOPS, 1, new int[] { width * 4 });
        }
    }


    public static class FolderBrowserExtension
    {

        public static DialogResult ShowDialogNew(this FolderBrowserDialog fbd)
        {
            return ShowDialogNew(fbd, (IntPtr)0);
        }
        public static DialogResult ShowDialogNew(this FolderBrowserDialog fbd, IntPtr hWndOwner)
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "Folders|\n";
                ofd.AddExtension = false;
                ofd.CheckFileExists = false;
                ofd.DereferenceLinks = true;
                ofd.Multiselect = false;
                ofd.InitialDirectory = fbd.SelectedPath;

                int result = 0;
                var ns = "System.Windows.Forms";
                var asmb = Assembly.GetAssembly(typeof(OpenFileDialog));
                var dialogint = GetType(asmb, ns, "FileDialogNative.IFileDialog");
                var dialog = Call(typeof(OpenFileDialog), ofd, "CreateVistaDialog");
                Call(typeof(OpenFileDialog), ofd, "OnBeforeVistaDialog", dialog);
                var options = Convert.ToUInt32(Call(typeof(FileDialog), ofd, "GetOptions"));
                options |= Convert.ToUInt32(GetEnumValue(asmb, ns, "FileDialogNative.FOS", "FOS_PICKFOLDERS"));
                Call(dialogint, dialog, "SetOptions", options);
                var pfde = New(asmb, ns, "FileDialog.VistaDialogEvents", ofd);
                var parameters = new object[] { pfde, (uint)0 };
                Call(dialogint, dialog, "Advise", parameters);
                var adviseres = Convert.ToUInt32(parameters[1]);
                try { result = Convert.ToInt32(Call(dialogint, dialog, "Show", hWndOwner)); }
                finally { Call(dialogint, dialog, "Unadvise", adviseres); }
                GC.KeepAlive(pfde);

                fbd.SelectedPath = ofd.FileName;

                return (result == 0) ? DialogResult.OK : DialogResult.Cancel;
            }
            else
            {
                return fbd.ShowDialog();
            }
        }


        private static Type GetType(Assembly asmb, string ns, string name)
        {
            Type type = null;
            string[] names = name.Split('.');
            if (names.Length > 0)
            {
                type = asmb.GetType(ns + "." + names[0]);
            }
            for (int i = 1; i < names.Length; i++)
            {
                type = type.GetNestedType(names[i], BindingFlags.NonPublic);
            }
            return type;
        }
        private static object Call(Type type, object obj, string func, params object[] parameters)
        {
            var mi = type.GetMethod(func, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi == null) return null;
            return mi.Invoke(obj, parameters);
        }
        private static object GetEnumValue(Assembly asmb, string ns, string typeName, string name)
        {
            var type = GetType(asmb, ns, typeName);
            var fieldInfo = type.GetField(name);
            return fieldInfo.GetValue(null);
        }
        private static object New(Assembly asmb, string ns, string name, params object[] parameters)
        {
            var type = GetType(asmb, ns, name);
            var ctorInfos = type.GetConstructors();
            foreach (ConstructorInfo ci in ctorInfos)
            {
                try { return ci.Invoke(parameters); }
                catch { }
            }
            return null;
        }
    }


    public static class Prompt
    {
        //handy utility to get a string from the user...
        public static string ShowDialog(IWin32Window owner, string text, string caption, string defaultvalue = "")
        {
            Form prompt = new Form()
            {
                Width = 450,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var textLabel = new Label() { Left = 30, Top = 20, Width = 370, Height = 20, Text = text, };
            var textBox = new TextBox() { Left = 30, Top = 40, Width = 370, Text = defaultvalue };
            var cancel = new Button() { Text = "Cancel", Left = 230, Width = 80, Top = 70, DialogResult = DialogResult.Cancel };
            var confirmation = new Button() { Text = "Ok", Left = 320, Width = 80, Top = 70, DialogResult = DialogResult.OK };
            cancel.Click += (sender, e) => { prompt.Close(); };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog(owner) == DialogResult.OK ? textBox.Text : "";
        }
    }




    public static class FormTheme
    {
        public static void SetTheme(Control form, ThemeBase theme)
        {
            form.BackColor = SystemColors.Control;
            form.ForeColor = SystemColors.ControlText;
            var txtback = SystemColors.Window;
            var wndback = SystemColors.Window;
            var disback = SystemColors.Control;
            var disfore = form.ForeColor;
            var btnback = Color.Transparent;

            if (theme is VS2015DarkTheme)
            {
                form.BackColor = theme.ColorPalette.MainWindowActive.Background;
                form.ForeColor = Color.White;
                txtback = Color.FromArgb(72, 75, 82);// form.BackColor;
                wndback = theme.ColorPalette.MainWindowActive.Background;
                disback = form.BackColor;// Color.FromArgb(32,32,32);
                disfore = Color.DarkGray;
                btnback = form.BackColor;
            }

            var allcontrols = new List<Control>();
            RecurseControls(form, allcontrols);

            foreach (var c in allcontrols)
            {
                if (c is TabPage)
                {
                    c.ForeColor = form.ForeColor;
                    c.BackColor = wndback;
                }
                else if ((c is CheckedListBox) || (c is ListBox))
                {
                    c.ForeColor = form.ForeColor;
                    c.BackColor = txtback;
                }
                else if ((c is ListView))
                {
                    c.ForeColor = form.ForeColor;
                    c.BackColor = wndback;
                }
                else if ((c is TextBox))
                {
                    var txtbox = c as TextBox;
                    c.ForeColor = txtbox.ReadOnly ? disfore : form.ForeColor;
                    c.BackColor = txtbox.ReadOnly ? disback : txtback;
                }
                else if ((c is Button) || (c is GroupBox))
                {
                    c.ForeColor = form.ForeColor;
                    c.BackColor = btnback;
                }
                else if (c is TreeView)
                {
                    c.ForeColor = form.ForeColor;
                    c.BackColor = wndback;
                    (c as TreeView).LineColor = form.ForeColor;
                }

            }

        }
        private static void RecurseControls(Control c, List<Control> l)
        {
            foreach (Control cc in c.Controls)
            {
                l.Add(cc);
                RecurseControls(cc, l);
            }
        }

    }






    public static class MessageBoxEx
    {
        //custom version of MessageBox to center in the parent, and apply theming
        //TODO: handle MessageBoxIcon and MessageBoxOptions
        private static DialogResult ShowCore(Form owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            if (owner == null) return MessageBox.Show(text, caption, buttons, icon, defaultButton, options);//fallback case

            var box = new Form()
            {
                Text = caption,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
            };
            var label = new Label();
            var font = label.Font;
            var btns = new Button[3];
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    btns[0] = new Button() { Text = "OK", DialogResult = DialogResult.OK };
                    break;
                case MessageBoxButtons.OKCancel:
                    btns[0] = new Button() { Text = "OK", DialogResult = DialogResult.OK };
                    btns[1] = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel };
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    btns[0] = new Button() { Text = "Abort", DialogResult = DialogResult.Abort };
                    btns[1] = new Button() { Text = "Retry", DialogResult = DialogResult.Retry };
                    btns[2] = new Button() { Text = "Ignore", DialogResult = DialogResult.Ignore };
                    box.ControlBox = false;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    btns[0] = new Button() { Text = "Yes", DialogResult = DialogResult.Yes };
                    btns[1] = new Button() { Text = "No", DialogResult = DialogResult.No };
                    btns[2] = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel };
                    break;
                case MessageBoxButtons.YesNo:
                    btns[0] = new Button() { Text = "Yes", DialogResult = DialogResult.Yes };
                    btns[1] = new Button() { Text = "No", DialogResult = DialogResult.No };
                    box.ControlBox = false;
                    break;
                case MessageBoxButtons.RetryCancel:
                    btns[0] = new Button() { Text = "Retry", DialogResult = DialogResult.Retry };
                    btns[1] = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel };
                    break;
                //case MessageBoxButtons.CancelTryContinue:
                //    btns[0] = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel };
                //    btns[1] = new Button() { Text = "Try Again", DialogResult = DialogResult.TryAgain };
                //    btns[2] = new Button() { Text = "Continue", DialogResult = DialogResult.Continue };
                //    break;
            }
            var tpad = 25;//padding above text
            var bpad = 15;//padding below text
            var lpad = 10;//padding left of text
            var rpad = 15;// padding right of text (and some extra?)
            var btnh = 45;//height of the button row
            var btnw = 75;//width of a button
            var btnp = 10;//spacing between buttons
            var btnl = 25;//spacing left of buttons
            var minw = 120;//absolute minimum width
            var btnsw = btnl;
            for (int i = 0; i < btns.Length; i++)
            {
                if (btns[i] == null) continue;
                btnsw += btnw + btnp;
            }
            minw = Math.Max(minw, btnsw);
            var maxw = Math.Min(Math.Max(150, (owner.Width * 5) / 8), label.LogicalToDeviceUnits(400));
            var size = TextRenderer.MeasureText(text, font);
            if (size.Width > maxw)
            {
                size.Width = maxw;
                size = TextRenderer.MeasureText(text, font, size, TextFormatFlags.WordBreak);
            }
            var maxh = Math.Max(250, (owner.Height * 7) / 8);
            var exth = bpad + tpad + btnh;//total extra height which isn't the label
            var toth = size.Height + exth;
            var w = Math.Max(minw, size.Width + lpad + rpad);
            var h = Math.Min(maxh, toth);
            var tw = size.Width;
            var th = h - exth;
            box.ClientSize = new Size(w, h);
            label.Location = new Point(lpad, tpad);
            label.Size = new Size(tw, th);
            label.AutoEllipsis = true;
            label.Text = text;
            box.Controls.Add(label);
            var btnbg = new Control();
            btnbg.Size = new Size(w, btnh);
            btnbg.Location = new Point(0, h - btnh);
            box.Controls.Add(btnbg);
            var x = w - rpad - btnw;
            var y = btnp;
            for (int i = btns.Length - 1; i >= 0; i--)
            {
                var btn = btns[i];
                if (btn == null) continue;
                btn.Width = btnw;
                btn.Location = new Point(x, y);
                btnbg.Controls.Add(btn);
                x -= (btnw + btnp);
            }
            var selbut = btns[0];
            switch (defaultButton)
            {
                case MessageBoxDefaultButton.Button1: selbut = btns[0]; break;
                case MessageBoxDefaultButton.Button2: selbut = btns[1]; break;
                case MessageBoxDefaultButton.Button3: selbut = btns[3]; break;
            }
            box.ActiveControl = selbut;

            //Themes.Theme.Apply(box);
            //var theme = Themes.Theme.GetTheme();
            //btnbg.BackColor = theme.WindowBack;

            return box.ShowDialog(owner);
            //MessageBox.Show(owner, res.ToString());
            //return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton, options);
        }

        /// <summary>
        ///  Displays a message box with specified text, caption, and style.
        /// </summary>
        public static DialogResult Show(Form owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, options);
        }

        /// <summary>
        ///  Displays a message box with specified text, caption, and style.
        /// </summary>
        public static DialogResult Show(Form owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, 0);
        }

        /// <summary>
        ///  Displays a message box with specified text, caption, and style.
        /// </summary>
        public static DialogResult Show(Form owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return ShowCore(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        ///  Displays a message box with specified text, caption, and style.
        /// </summary>
        public static DialogResult Show(Form owner, string text, string caption, MessageBoxButtons buttons)
        {
            return ShowCore(owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        ///  Displays a message box with specified text and caption.
        /// </summary>
        public static DialogResult Show(Form owner, string text, string caption)
        {
            return ShowCore(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        ///  Displays a message box with specified text.
        /// </summary>
        public static DialogResult Show(Form owner, string text)
        {
            return ShowCore(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
        }

    }




    //unused
    //public class AccurateTimer
    //{
    //    private delegate void TimerEventDel(int id, int msg, IntPtr user, int dw1, int dw2);
    //    private const int TIME_PERIODIC = 1;
    //    private const int EVENT_TYPE = TIME_PERIODIC;// + 0x100;  // TIME_KILL_SYNCHRONOUS causes a hang ?!
    //    [DllImport("winmm.dll")]
    //    private static extern int timeBeginPeriod(int msec);
    //    [DllImport("winmm.dll")]
    //    private static extern int timeEndPeriod(int msec);
    //    [DllImport("winmm.dll")]
    //    private static extern int timeSetEvent(int delay, int resolution, TimerEventDel handler, IntPtr user, int eventType);
    //    [DllImport("winmm.dll")]
    //    private static extern int timeKillEvent(int id);
    //    Action mAction;
    //    Form mForm;
    //    private int mTimerId;
    //    private TimerEventDel mHandler;  // NOTE: declare at class scope so garbage collector doesn't release it!!!
    //    public AccurateTimer(Form form, Action action, int delay)
    //    {
    //        mAction = action;
    //        mForm = form;
    //        timeBeginPeriod(1);
    //        mHandler = new TimerEventDel(TimerCallback);
    //        mTimerId = timeSetEvent(delay, 0, mHandler, IntPtr.Zero, EVENT_TYPE);
    //    }
    //    public void Stop()
    //    {
    //        int err = timeKillEvent(mTimerId);
    //        timeEndPeriod(1);
    //        System.Threading.Thread.Sleep(100);// Ensure callbacks are drained
    //    }
    //    private void TimerCallback(int id, int msg, IntPtr user, int dw1, int dw2)
    //    {
    //        if (mTimerId != 0) mForm.BeginInvoke(mAction);
    //    }
    //}

}
