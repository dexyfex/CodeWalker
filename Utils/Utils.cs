using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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



    public static class TextUtil
    {

        public static string GetBytesReadable(long i)
        {
            //shamelessly stolen from stackoverflow, and a bit mangled

            // Returns the human-readable file size for an arbitrary, 64-bit file size 
            // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 bytes"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);

            string fmt = "0.### ";
            if (readable > 1000)
            {
                fmt = "0";
            }
            else if (readable > 100)
            {
                fmt = "0.#";
            }
            else if (readable > 10)
            {
                fmt = "0.##";
            }

            // Return formatted number with suffix
            return readable.ToString(fmt) + suffix;
        }
    }



    public static class FloatUtil
    {
        public static bool TryParse(string s, out float f)
        {
            f = 0.0f;
            if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f))
            {
                return true;
            }
            return false;
        }
        public static float Parse(string s)
        {
            float f;
            TryParse(s, out f);
            return f;
        }
        public static string ToString(float f)
        {
            var c = CultureInfo.InvariantCulture;
            return f.ToString(c);
        }


        public static string GetVector2String(Vector2 v)
        {
            var c = CultureInfo.InvariantCulture;
            return v.X.ToString(c) + ", " + v.Y.ToString(c);
        }
        public static string GetVector3String(Vector3 v)
        {
            var c = CultureInfo.InvariantCulture;
            return v.X.ToString(c) + ", " + v.Y.ToString(c) + ", " + v.Z.ToString(c);
        }
        public static string GetVector3String(Vector3 v, string format)
        {
            var c = CultureInfo.InvariantCulture;
            return v.X.ToString(format, c) + ", " + v.Y.ToString(format, c) + ", " + v.Z.ToString(format, c);
        }

        public static Vector3 ParseVector3String(string s)
        {
            Vector3 p = new Vector3(0.0f);
            string[] ss = s.Split(',');
            if (ss.Length > 0)
            {
                FloatUtil.TryParse(ss[0].Trim(), out p.X);
            }
            if (ss.Length > 1)
            {
                FloatUtil.TryParse(ss[1].Trim(), out p.Y);
            }
            if (ss.Length > 2)
            {
                FloatUtil.TryParse(ss[2].Trim(), out p.Z);
            }
            return p;
        }



        public static string GetVector4String(Vector4 v)
        {
            var c = CultureInfo.InvariantCulture;
            return v.X.ToString(c) + ", " + v.Y.ToString(c) + ", " + v.Z.ToString(c) + ", " + v.W.ToString(c);
        }
        public static Vector4 ParseVector4String(string s)
        {
            Vector4 p = new Vector4(0.0f);
            string[] ss = s.Split(',');
            if (ss.Length > 0)
            {
                FloatUtil.TryParse(ss[0].Trim(), out p.X);
            }
            if (ss.Length > 1)
            {
                FloatUtil.TryParse(ss[1].Trim(), out p.Y);
            }
            if (ss.Length > 2)
            {
                FloatUtil.TryParse(ss[2].Trim(), out p.Z);
            }
            if (ss.Length > 3)
            {
                FloatUtil.TryParse(ss[3].Trim(), out p.W);
            }
            return p;
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
