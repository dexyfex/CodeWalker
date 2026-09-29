using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Windows.Forms;
using System.Collections;
/*
MIT License

Copyright (c) 2021 DebugST@crystal_lz

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */
/*
 * create: 2021-12-08
 * modify: 2021-03-02
 * Author: Crystal_lz
 * blog: http://st233.com
 * Gitee: https://gitee.com/DebugST
 * Github: https://github.com/DebugST
 */
namespace ST.Library.UI.NodeEditor
{
    public abstract class STNode
    {
        private STNodeEditor _Owner;
        /// <summary>
        /// Get the current Node owner
        /// </summary>
        public STNodeEditor Owner {
            get { return _Owner; }
            internal set {
                if (value == _Owner) return;
                if (_Owner != null) {
                    foreach (STNodeOption op in this._InputOptions.ToArray()) op.DisConnectionAll();
                    foreach (STNodeOption op in this._OutputOptions.ToArray()) op.DisConnectionAll();
                }
                _Owner = value;
                if (!this._AutoSize) this.SetOptionsLocation();
                this.BuildSize(true, true, false);
                this.OnOwnerChanged();
            }
        }

        private bool _IsSelected;
        /// <summary>
        /// Gets or sets whether Node is selected
        /// </summary>
        public bool IsSelected {
            get { return _IsSelected; }
            set {
                if (value == _IsSelected) return;
                _IsSelected = value;
                this.Invalidate();
                this.OnSelectedChanged();
                if (this._Owner != null) this._Owner.OnSelectedChanged(EventArgs.Empty);
            }
        }

        private bool _IsActive;
        /// <summary>
        /// Gets if Node is active
        /// </summary>
        public bool IsActive {
            get { return _IsActive; }
            internal set {
                if (value == _IsActive) return;
                _IsActive = value;
                this.OnActiveChanged();
            }
        }

        private Color _TitleColor;
        /// <summary>
        /// Gets or sets the title background color
        /// </summary>
        public Color TitleColor {
            get { return _TitleColor; }
            protected set {
                _TitleColor = value;
                this.Invalidate(new Rectangle(0, 0, this._Width, this._TitleHeight));
            }
        }

        private Color _MarkColor;
        /// <summary>
        /// Gets or sets the background color of the marker information
        /// </summary>
        public Color MarkColor {
            get { return _MarkColor; }
            protected set {
                _MarkColor = value;
                this.Invalidate(this._MarkRectangle);
            }
        }

        private Color _ForeColor = Color.White;
        /// <summary>
        /// Get or set the current Node foreground color
        /// </summary>
        public Color ForeColor {
            get { return _ForeColor; }
            protected set {
                _ForeColor = value;
                this.Invalidate();
            }
        }

        private Color _BackColor;
        /// <summary>
        /// Gets or sets the current Node background color
        /// </summary>
        public Color BackColor {
            get { return _BackColor; }
            protected set {
                _BackColor = value;
                this.Invalidate();
            }
        }

        private string _Title;
        /// <summary>
        /// Gets or sets the Node title
        /// </summary>
        public string Title {
            get { return _Title; }
            protected set {
                _Title = value;
                if (this._AutoSize) this.BuildSize(true, true, true);
                //this.Invalidate(this.TitleRectangle);
            }
        }

        private string _Mark;
        /// <summary>
        /// Get or set Node tag information
        /// </summary>
        public string Mark {
            get { return _Mark; }
            set {
                _Mark = value;
                if (value == null)
                    _MarkLines = null;
                else
                    _MarkLines = (from s in value.Split('\n') select s.Trim()).ToArray();
                this.Invalidate(new Rectangle(-5, -5, this._MarkRectangle.Width + 10, this._MarkRectangle.Height + 10));
            }
        }

        private string[] _MarkLines;//Store line data separately without splitting it every time you draw
        /// <summary>
        /// Get Node tag information row data
        /// </summary>
        public string[] MarkLines {
            get { return _MarkLines; }
        }

        private int _Left;
        /// <summary>
        /// Get or set the left coordinate of Node
        /// </summary>
        public int Left {
            get { return _Left; }
            set {
                if (this._LockLocation || value == _Left) return;
                _Left = value;
                this.SetOptionsLocation();
                this.BuildSize(false, true, false);
                this.OnMove(EventArgs.Empty);
                if (this._Owner != null) {
                    this._Owner.BuildLinePath();
                    this._Owner.BuildBounds();
                }
            }
        }

        private int _Top;
        /// <summary>
        /// Gets or sets the coordinates on the top of the Node
        /// </summary>
        public int Top {
            get { return _Top; }
            set {
                if (this._LockLocation || value == _Top) return;
                _Top = value;
                this.SetOptionsLocation();
                this.BuildSize(false, true, false);
                this.OnMove(EventArgs.Empty);
                if (this._Owner != null) {
                    this._Owner.BuildLinePath();
                    this._Owner.BuildBounds();
                }
            }
        }

        private int _Width = 100;
        /// <summary>
        /// Gets or sets the Node width. This value cannot be set when AutoSize is set
        /// </summary>
        public int Width {
            get { return _Width; }
            protected set {
                if (value < 50) return;
                if (this._AutoSize || value == _Width) return;
                _Width = value;
                this.SetOptionsLocation();
                this.BuildSize(false, true, false);
                this.OnResize(EventArgs.Empty);
                if (this._Owner != null) {
                    this._Owner.BuildLinePath();
                    this._Owner.BuildBounds();
                }
                this.Invalidate();
            }
        }

        private int _Height = 40;
        /// <summary>
        /// Gets or sets the height of Node. This value cannot be set when AutoSize is set
        /// </summary>
        public int Height {
            get { return _Height; }
            protected set {
                if (value < 40) return;
                if (this._AutoSize || value == _Height) return;
                _Height = value;
                this.SetOptionsLocation();
                this.BuildSize(false, true, false);
                this.OnResize(EventArgs.Empty);
                if (this._Owner != null) {
                    this._Owner.BuildLinePath();
                    this._Owner.BuildBounds();
                }
                this.Invalidate();
            }
        }

        private int _ItemHeight = 20;
        /// <summary>
        /// Gets or sets the height of each option of Node
        /// </summary>
        public int ItemHeight {
            get { return _ItemHeight; }
            protected set {
                if (value < 16) value = 16;
                if (value > 200) value = 200;
                if (value == _ItemHeight) return;
                _ItemHeight = value;
                if (this._AutoSize) {
                    this.BuildSize(true, false, true);
                } else {
                    this.SetOptionsLocation();
                    if (this._Owner != null) this._Owner.Invalidate();
                }
            }
        }

        private bool _AutoSize = true;
        /// <summary>
        /// Gets or sets whether Node automatically calculates width and height
        /// </summary>
        public bool AutoSize {
            get { return _AutoSize; }
            protected set { _AutoSize = value; }
        }
        /// <summary>
        /// Get the coordinates of the right edge of Node
        /// </summary>
        public int Right {
            get { return _Left + _Width; }
        }
        /// <summary>
        /// Get the coordinates below the Node
        /// </summary>
        public int Bottom {
            get { return _Top + _Height; }
        }
        /// <summary>
        /// Get the Node rectangle area
        /// </summary>
        public Rectangle Rectangle {
            get {
                return new Rectangle(this._Left, this._Top, this._Width, this._Height);
            }
        }
        /// <summary>
        /// Get the Node title rectangle area
        /// </summary>
        public Rectangle TitleRectangle {
            get {
                return new Rectangle(this._Left, this._Top, this._Width, this._TitleHeight);
            }
        }

        private Rectangle _MarkRectangle;
        /// <summary>
        /// Get the Node marked rectangular area
        /// </summary>
        public Rectangle MarkRectangle {
            get { return _MarkRectangle; }
        }

        private int _TitleHeight = 20;
        /// <summary>
        /// Gets or sets the height of the Node title
        /// </summary>
        public int TitleHeight {
            get { return _TitleHeight; }
            protected set { _TitleHeight = value; }
        }

        private STNodeOptionCollection _InputOptions;
        /// <summary>
        /// Get the set of input options
        /// </summary>
        protected internal STNodeOptionCollection InputOptions {
            get { return _InputOptions; }
        }
        /// <summary>
        /// Get the number of input options set
        /// </summary>
        public int InputOptionsCount { get { return _InputOptions.Count; } }

        private STNodeOptionCollection _OutputOptions;
        /// <summary>
        /// Get output options
        /// </summary>
        protected internal STNodeOptionCollection OutputOptions {
            get { return _OutputOptions; }
        }
        /// <summary>
        /// Get the number of output options
        /// </summary>
        public int OutputOptionsCount { get { return _OutputOptions.Count; } }

        private STNodeControlCollection _Controls;
        /// <summary>
        /// Get the set of controls contained in Node
        /// </summary>
        protected STNodeControlCollection Controls {
            get { return _Controls; }
        }
        /// <summary>
        /// Get the number of control sets contained in Node
        /// </summary>
        public int ControlsCount { get { return _Controls.Count; } }
        /// <summary>
        /// Get the Node coordinate position
        /// </summary>
        public Point Location {
            get { return new Point(this._Left, this._Top); }
            set {
                this.Left = value.X;
                this.Top = value.Y;
            }
        }
        /// <summary>
        /// Get the Node size
        /// </summary>
        public Size Size {
            get { return new Size(this._Width, this._Height); }
            set {
                this.Width = value.Width;
                this.Height = value.Height;
            }
        }

        private Font _Font;
        /// <summary>
        /// Get or set the Node font
        /// </summary>
        protected Font Font {
            get { return _Font; }
            set {
                if (value == _Font) return;
                this._Font.Dispose();
                _Font = value;
            }
        }

        private bool _LockOption;
        /// <summary>
        /// Get or set whether to lock the Option option and not accept connections after locking
        /// </summary>
        public bool LockOption {
            get { return _LockOption; }
            set {
                _LockOption = value;
                this.Invalidate(new Rectangle(0, 0, this._Width, this._TitleHeight));
            }
        }

        private bool _LockLocation;
        /// <summary>
        /// Gets or sets whether to lock the Node position and cannot move after it is locked
        /// </summary>
        public bool LockLocation {
            get { return _LockLocation; }
            set {
                _LockLocation = value;
                this.Invalidate(new Rectangle(0, 0, this._Width, this._TitleHeight));
            }
        }

        private ContextMenuStrip _ContextMenuStrip;
        /// <summary>
        /// Gets or sets the current Node context menu
        /// </summary>
        public ContextMenuStrip ContextMenuStrip {
            get { return _ContextMenuStrip; }
            set { _ContextMenuStrip = value; }
        }

        private object _Tag;
        /// <summary>
        /// Get or set user-defined saved data
        /// </summary>
        public object Tag {
            get { return _Tag; }
            set { _Tag = value; }
        }

        private Guid _Guid;
        /// <summary>
        /// Get the global unique ID
        /// </summary>
        public Guid Guid {
            get { return _Guid; }
        }

        private bool _LetGetOptions = false;
        /// <summary>
        /// Gets or sets whether to allow external access to STNodeOption
        /// </summary>
        public bool LetGetOptions {
            get { return _LetGetOptions; }
            protected set { _LetGetOptions = value; }
        }

        private static Point m_static_pt_init = new Point(10, 10);

        public STNode() {
            this._Title = "Untitled";
            this._MarkRectangle.Height = this._Height;
            this._Left = this._MarkRectangle.X = m_static_pt_init.X;
            this._Top = m_static_pt_init.Y;
            this._MarkRectangle.Y = this._Top - 30;
            this._InputOptions = new STNodeOptionCollection(this, true);
            this._OutputOptions = new STNodeOptionCollection(this, false);
            this._Controls = new STNodeControlCollection(this);
            this._BackColor = Color.FromArgb(200, 64, 64, 64);
            this._TitleColor = Color.FromArgb(200, Color.DodgerBlue);
            this._MarkColor = Color.FromArgb(200, Color.Brown);
            this._Font = new Font("courier new", 8.25f);

            m_sf = new StringFormat();
            m_sf.Alignment = StringAlignment.Near;
            m_sf.LineAlignment = StringAlignment.Center;
            m_sf.FormatFlags = StringFormatFlags.NoWrap;
            m_sf.SetTabStops(0, new float[] { 40 });
            m_static_pt_init.X += 10;
            m_static_pt_init.Y += 10;
            this._Guid = Guid.NewGuid();
            this.OnCreate();
        }

        //private int m_nItemHeight = 30;
        protected StringFormat m_sf;
        /// <summary>
        /// Active controls in the current Node
        /// </summary>
        protected STNodeControl m_ctrl_active;
        /// <summary>
        /// The hover control in the current Node
        /// </summary>
        protected STNodeControl m_ctrl_hover;
        /// <summary>
        /// The control under the mouse click in the current Node
        /// </summary>
        protected STNodeControl m_ctrl_down;

        protected internal void BuildSize(bool bBuildNode, bool bBuildMark, bool bRedraw) {
            if (this._Owner == null) return;
            using (Graphics g = this._Owner.CreateGraphics()) {
                if (this._AutoSize && bBuildNode) {
                    Size sz = this.GetDefaultNodeSize(g);
                    if (this._Width != sz.Width || this._Height != sz.Height) {
                        this._Width = sz.Width;
                        this._Height = sz.Height;
                        this.SetOptionsLocation();
                        this.OnResize(EventArgs.Empty);
                    }
                }
                if (bBuildMark && !string.IsNullOrEmpty(this._Mark)) {
                    this._MarkRectangle = this.OnBuildMarkRectangle(g);
                }
            }
            if (bRedraw) this._Owner.Invalidate();
        }

        internal Dictionary<string, byte[]> OnSaveNode() {
            Dictionary<string, byte[]> dic = new Dictionary<string, byte[]>();
            dic.Add("Guid", this._Guid.ToByteArray());
            dic.Add("Left", BitConverter.GetBytes(this._Left));
            dic.Add("Top", BitConverter.GetBytes(this._Top));
            dic.Add("Width", BitConverter.GetBytes(this._Width));
            dic.Add("Height", BitConverter.GetBytes(this._Height));
            dic.Add("AutoSize", new byte[] { (byte)(this._AutoSize ? 1 : 0) });
            if (this._Mark != null) dic.Add("Mark", Encoding.UTF8.GetBytes(this._Mark));
            dic.Add("LockOption", new byte[] { (byte)(this._LockLocation ? 1 : 0) });
            dic.Add("LockLocation", new byte[] { (byte)(this._LockLocation ? 1 : 0) });
            Type t = this.GetType();
            foreach (var p in t.GetProperties()) {
                var attrs = p.GetCustomAttributes(true);
                foreach (var a in attrs) {
                    if (!(a is STNodePropertyAttribute)) continue;
                    var attr = a as STNodePropertyAttribute;
                    object obj = Activator.CreateInstance(attr.DescriptorType);
                    if (!(obj is STNodePropertyDescriptor))
                        throw new InvalidOperationException("[STNodePropertyAttribute.Type] parameter value must be the type of [STNodePropertyDescriptor] or its subclass");
                    var desc = (STNodePropertyDescriptor)Activator.CreateInstance(attr.DescriptorType);
                    desc.Node = this;
                    desc.PropertyInfo = p;
                    byte[] byData = desc.GetBytesFromValue();
                    if (byData == null) continue;
                    dic.Add(p.Name, byData);
                }
            }
            this.OnSaveNode(dic);
            return dic;
        }

        internal byte[] GetSaveData() {
            List<byte> lst = new List<byte>();
            Type t = this.GetType();
            byte[] byData = Encoding.UTF8.GetBytes(t.Module.Name + "|" + t.FullName);
            lst.Add((byte)byData.Length);
            lst.AddRange(byData);
            byData = Encoding.UTF8.GetBytes(t.GUID.ToString());
            lst.Add((byte)byData.Length);
            lst.AddRange(byData);

            var dic = this.OnSaveNode();
            if (dic != null) {
                foreach (var v in dic) {
                    byData = Encoding.UTF8.GetBytes(v.Key);
                    lst.AddRange(BitConverter.GetBytes(byData.Length));
                    lst.AddRange(byData);
                    lst.AddRange(BitConverter.GetBytes(v.Value.Length));
                    lst.AddRange(v.Value);
                }
            }
            return lst.ToArray();
        }

        #region protected
        /// <summary>
        /// Occurs when Node is constructed
        /// </summary>
        protected virtual void OnCreate() { }
        /// <summary>
        /// Draw the entire Node
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        protected internal virtual void OnDrawNode(DrawingTools dt) {
            dt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            //Fill background
            if (this._BackColor.A != 0) {
                dt.SolidBrush.Color = this._BackColor;
                dt.Graphics.FillRectangle(dt.SolidBrush, this._Left, this._Top + this._TitleHeight, this._Width, this.Height - this._TitleHeight);
            }
            this.OnDrawTitle(dt);
            this.OnDrawBody(dt);
        }
        /// <summary>
        /// Draw the Node header section
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        protected virtual void OnDrawTitle(DrawingTools dt) {
            m_sf.Alignment = StringAlignment.Center;
            m_sf.LineAlignment = StringAlignment.Center;
            Graphics g = dt.Graphics;
            SolidBrush brush = dt.SolidBrush;
            if (this._TitleColor.A != 0) {
                brush.Color = this._TitleColor;
                g.FillRectangle(brush, this.TitleRectangle);
            }
            if (this._LockOption) {
                //dt.Pen.Color = this.ForeColor;
                brush.Color = this._ForeColor;
                int n = this._Top + this._TitleHeight / 2 - 5;
                g.FillRectangle(dt.SolidBrush, this._Left + 4, n + 0, 2, 4);
                g.FillRectangle(dt.SolidBrush, this._Left + 6, n + 0, 2, 2);
                g.FillRectangle(dt.SolidBrush, this._Left + 8, n + 0, 2, 4);
                g.FillRectangle(dt.SolidBrush, this._Left + 3, n + 4, 8, 6);
                //g.DrawLine(dt.Pen, this._Left + 6, n + 5, this._Left + 6, n + 7);
                //g.DrawRectangle(dt.Pen, this._Left + 3, n + 0, 6, 3);
                //g.DrawRectangle(dt.Pen, this._Left + 2, n + 3, 8, 6);
                //g.DrawLine(dt.Pen, this._Left + 6, n + 5, this._Left + 6, n + 7);

            }
            if (this._LockLocation) {
                //dt.Pen.Color = this.ForeColor;
                brush.Color = this._ForeColor;
                int n = this._Top + this._TitleHeight / 2 - 5;
                g.FillRectangle(brush, this.Right - 9, n, 4, 4);
                g.FillRectangle(brush, this.Right - 11, n + 4, 8, 2);
                g.FillRectangle(brush, this.Right - 8, n + 6, 2, 4);
                //g.DrawLine(dt.Pen, this.Right - 10, n + 6, this.Right - 4, n + 6);
                //g.DrawLine(dt.Pen, this.Right - 10, n, this.Right - 4, n);
                //g.DrawLine(dt.Pen, this.Right - 11, n + 6, this.Right - 3, n + 6);
                //g.DrawLine(dt.Pen, this.Right - 7, n + 7, this.Right - 7, n + 9);
            }
            if (!string.IsNullOrEmpty(this._Title) && this._ForeColor.A != 0) {
                brush.Color = this._ForeColor;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawString(this._Title, this._Font, brush, this.TitleRectangle, m_sf);
            }
        }
        /// <summary>
        /// Draw the Node body part to remove the title part
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        protected virtual void OnDrawBody(DrawingTools dt) {
            SolidBrush brush = dt.SolidBrush;
            foreach (STNodeOption op in this._InputOptions) {
                if (op == STNodeOption.Empty) continue;
                this.OnDrawOptionDot(dt, op);
                this.OnDrawOptionText(dt, op);
            }
            foreach (STNodeOption op in this._OutputOptions) {
                if (op == STNodeOption.Empty) continue;
                this.OnDrawOptionDot(dt, op);
                this.OnDrawOptionText(dt, op);
            }
            if (this._Controls.Count != 0) { //Draw child controls
                // Align the origin of the coordinates with the node
                //dt.Graphics.ResetTransform();
                dt.Graphics.TranslateTransform(this._Left, this._Top + this._TitleHeight);
                Point pt = Point.Empty; //The current amount of offset needed
                Point pt_last = Point.Empty; //The coordinates of the last control relative to the node
                foreach (STNodeControl v in this._Controls) {
                    if (!v.Visable) continue;
                    pt.X = v.Left - pt_last.X;
                    pt.Y = v.Top - pt_last.Y;
                    pt_last = v.Location;
                    dt.Graphics.TranslateTransform(pt.X, pt.Y); //Move the origin coordinates to the control position
                    dt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    v.OnPaint(dt);
                }
                //dt.Graphics.TranslateTransform(-pt_last.X, -pt_last.Y); restore coordinates
                dt.Graphics.TranslateTransform(-this._Left - pt_last.X, -this._Top - this._TitleHeight - pt_last.Y);
                //dt.Graphics.
            }
        }
        /// <summary>
        /// Draw marker information
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        protected internal virtual void OnDrawMark(DrawingTools dt) {
            if (string.IsNullOrEmpty(this._Mark)) return;
            Graphics g = dt.Graphics;
            SolidBrush brush = dt.SolidBrush;
            m_sf.LineAlignment = StringAlignment.Center;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            brush.Color = this._MarkColor;
            g.FillRectangle(brush, this._MarkRectangle); //fill background color

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality; //Determine the size required for text drawing
            var sz = g.MeasureString(this.Mark, this.Font, this._MarkRectangle.Width);
            brush.Color = this._ForeColor;
            if (sz.Height > this._ItemHeight || sz.Width > this._MarkRectangle.Width) { //If it exceeds the drawing area, draw the part
                Rectangle rect = new Rectangle(this._MarkRectangle.Left + 2, this._MarkRectangle.Top + 2, this._MarkRectangle.Width - 20, 16);
                m_sf.Alignment = StringAlignment.Near;
                g.DrawString(this._MarkLines[0], this._Font, brush, rect, m_sf);
                m_sf.Alignment = StringAlignment.Far;
                rect.Width = this._MarkRectangle.Width - 5;
                g.DrawString("+", this._Font, brush, rect, m_sf); // + means beyond the drawing area
            } else {
                m_sf.Alignment = StringAlignment.Near;
                g.DrawString(this._MarkLines[0].Trim(), this._Font, brush, this._MarkRectangle, m_sf);
            }
        }
        /// <summary>
        /// The point where the option line is drawn
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        /// <param name="op">Specified options</param>
        protected virtual void OnDrawOptionDot(DrawingTools dt, STNodeOption op) {
            Graphics g = dt.Graphics;
            Pen pen = dt.Pen;
            SolidBrush brush = dt.SolidBrush;
            var t = typeof(object);
            if (op.DotColor != Color.Transparent) //Set the color
                brush.Color = op.DotColor;
            else {
                if (op.DataType == t)
                    pen.Color = this.Owner.UnknownTypeColor;
                else
                    brush.Color = this.Owner.TypeColor.ContainsKey(op.DataType) ? this.Owner.TypeColor[op.DataType] : this.Owner.UnknownTypeColor;
            }
            if (op.IsSingle) { //Single connection circle
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                if (op.DataType == t) { //unknown type to draw otherwise fill
                    g.DrawEllipse(pen, op.DotRectangle.X, op.DotRectangle.Y, op.DotRectangle.Width - 1, op.DotRectangle.Height - 1);
                } else
                    g.FillEllipse(brush, op.DotRectangle);
            } else { //Multiple connection rectangles
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                if (op.DataType == t) {
                    g.DrawRectangle(pen, op.DotRectangle.X, op.DotRectangle.Y, op.DotRectangle.Width - 1, op.DotRectangle.Height - 1);
                } else
                    g.FillRectangle(brush, op.DotRectangle);
            }
        }
        /// <summary>
        /// Text for drawing options
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        /// <param name="op">Specified options</param>
        protected virtual void OnDrawOptionText(DrawingTools dt, STNodeOption op) {
            Graphics g = dt.Graphics;
            SolidBrush brush = dt.SolidBrush;
            if (op.IsInput) {
                m_sf.Alignment = StringAlignment.Near;
            } else {
                m_sf.Alignment = StringAlignment.Far;
            }
            brush.Color = op.TextColor;
            g.DrawString(op.Text, this.Font, brush, op.TextRectangle, m_sf);
        }
        /// <summary>
        /// Occurs when calculating the position of the Option connection point
        /// </summary>
        /// <param name="op">Option to be calculated</param>
        /// <param name="pt">Automatically calculated position</param>
        /// <param name="nIndex">Index of the current Option</param>
        /// <returns>New location</returns>
        protected virtual Point OnSetOptionDotLocation(STNodeOption op, Point pt, int nIndex) {
            return pt;
        }
        /// <summary>
        /// Occurs when evaluating the Option text area
        /// </summary>
        /// <param name="op">Option to be calculated</param>
        /// <param name="rect">Automatically calculated area</param>
        /// <param name="nIndex">Index of the current Option</param>
        /// <returns>New area</returns>
        protected virtual Rectangle OnSetOptionTextRectangle(STNodeOption op, Rectangle rect, int nIndex) {
            return rect;
        }
        /// <summary>
        /// Get the default size required by the current STNode
        /// The returned size does not limit the drawing area and can still be drawn outside of this area
        /// But it will not be accepted by STNodeEditor and trigger the corresponding event
        /// </summary>
        /// <param name="g">Drawing panel</param>
        /// <returns>Calculated size</returns>
        protected virtual Size GetDefaultNodeSize(Graphics g) {
            int nInputHeight = 0, nOutputHeight = 0;
            foreach (STNodeOption op in this._InputOptions) nInputHeight += this._ItemHeight;
            foreach (STNodeOption op in this._OutputOptions) nOutputHeight += this._ItemHeight;
            int nHeight = this._TitleHeight + (nInputHeight > nOutputHeight ? nInputHeight : nOutputHeight);

            SizeF szf_input = SizeF.Empty, szf_output = SizeF.Empty;
            foreach (STNodeOption v in this._InputOptions) {
                if (string.IsNullOrEmpty(v.Text)) continue;
                SizeF szf = g.MeasureString(v.Text, this._Font);
                if (szf.Width > szf_input.Width) szf_input = szf;
            }
            foreach (STNodeOption v in this._OutputOptions) {
                if (string.IsNullOrEmpty(v.Text)) continue;
                SizeF szf = g.MeasureString(v.Text, this._Font);
                if (szf.Width > szf_output.Width) szf_output = szf;
            }
            int nWidth = (int)(szf_input.Width + szf_output.Width + 25);
            if (!string.IsNullOrEmpty(this.Title)) szf_input = g.MeasureString(this.Title, this.Font);
            if (szf_input.Width + 30 > nWidth) nWidth = (int)szf_input.Width + 30;
            return new Size(nWidth, nHeight);
        }
        /// <summary>
        /// Calculate the rectangular area required by the current Mark
        /// The returned size does not limit the drawing area and can still be drawn outside of this area
        /// But it will not be accepted by STNodeEditor and trigger the corresponding event
        /// </summary>
        /// <param name="g">Drawing panel</param>
        /// <returns>Calculated area</returns>
        protected virtual Rectangle OnBuildMarkRectangle(Graphics g) {
            //if (string.IsNullOrEmpty(this._Mark)) return Rectangle.Empty;
            return new Rectangle(this._Left, this._Top - 30, this._Width, 20);
        }
        /// <summary>
        /// What data does this Node need to save additionally when it needs to be saved?
        /// Note: When saving, it will not be serialized and restored. When only re-creating this Node through the empty parameter constructor
        /// Then call OnLoadNode() to restore the saved data
        /// </summary>
        /// <param name="dic">Data to be saved</param>
        protected virtual void OnSaveNode(Dictionary<string, byte[]> dic) { }
        /// <summary>
        /// When restoring the node, the data returned by OnSaveNode() will be re-introduced to this function
        /// </summary>
        /// <param name="dic">Save data</param>
        protected internal virtual void OnLoadNode(Dictionary<string, byte[]> dic) {
            if (dic.ContainsKey("AutoSize")) this._AutoSize = dic["AutoSize"][0] == 1;
            if (dic.ContainsKey("LockOption")) this._LockOption = dic["LockOption"][0] == 1;
            if (dic.ContainsKey("LockLocation")) this._LockLocation = dic["LockLocation"][0] == 1;
            if (dic.ContainsKey("Guid")) this._Guid = new Guid(dic["Guid"]);
            if (dic.ContainsKey("Left")) this._Left = BitConverter.ToInt32(dic["Left"], 0);
            if (dic.ContainsKey("Top")) this._Top = BitConverter.ToInt32(dic["Top"], 0);
            if (dic.ContainsKey("Width") && !this._AutoSize) this._Width = BitConverter.ToInt32(dic["Width"], 0);
            if (dic.ContainsKey("Height") && !this._AutoSize) this._Height = BitConverter.ToInt32(dic["Height"], 0);
            if (dic.ContainsKey("Mark")) this.Mark = Encoding.UTF8.GetString(dic["Mark"]);
            Type t = this.GetType();
            foreach (var p in t.GetProperties()) {
                var attrs = p.GetCustomAttributes(true);
                foreach (var a in attrs) {
                    if (!(a is STNodePropertyAttribute)) continue;
                    var attr = a as STNodePropertyAttribute;
                    object obj = Activator.CreateInstance(attr.DescriptorType);
                    if (!(obj is STNodePropertyDescriptor))
                        throw new InvalidOperationException("[STNodePropertyAttribute.Type] parameter value must be the type of [STNodePropertyDescriptor] or its subclass");
                    var desc = (STNodePropertyDescriptor)Activator.CreateInstance(attr.DescriptorType);
                    desc.Node = this;
                    desc.PropertyInfo = p;
                    try {
                        if (dic.ContainsKey(p.Name)) desc.SetValue(dic[p.Name]);
                    } catch (Exception ex) {
                        string strErr = "The value of attribute [" + this.Title + "." + p.Name + "] cannot be restored by overriding [STNodePropertyAttribute.GetBytesFromValue(), STNodePropertyAttribute.GetValueFromBytes(byte[])] to ensure preservation and The binary data is correct when loading";
                        Exception e = ex;
                        while (e != null) {
                            strErr += "\r\n----\r\n[" + e.GetType().Name + "] -> " + e.Message;
                            e = e.InnerException;
                        }
                        throw new InvalidOperationException(strErr, ex);
                    }
                }
            }
        }
        /// <summary>
        /// Occurs when the editor has loaded all nodes
        /// </summary>
        protected internal virtual void OnEditorLoadCompleted() { }
        /// <summary>
        /// Set the text information of Option
        /// </summary>
        /// <param name="op">Target Option</param>
        /// <param name="strText">text</param>
        /// <returns>Successful</returns>
        protected bool SetOptionText(STNodeOption op, string strText) {
            if (op.Owner != this) return false;
            op.Text = strText;
            return true;
        }
        /// <summary>
        /// Set the color of the Option text information
        /// </summary>
        /// <param name="op">Target Option</param>
        /// <param name="clr">color</param>
        /// <returns>Successful</returns>
        protected bool SetOptionTextColor(STNodeOption op, Color clr) {
            if (op.Owner != this) return false;
            op.TextColor = clr;
            return true;
        }
        /// <summary>
        /// Set the color of the Option connection point
        /// </summary>
        /// <param name="op">Target Option</param>
        /// <param name="clr">color</param>
        /// <returns>Successful</returns>
        protected bool SetOptionDotColor(STNodeOption op, Color clr) {
            if (op.Owner != this) return false;
            op.DotColor = clr;
            return false;
        }

        //[event]===========================[event]==============================[event]============================[event]

        protected internal virtual void OnGotFocus(EventArgs e) { }

        protected internal virtual void OnLostFocus(EventArgs e) { }

        protected internal virtual void OnMouseEnter(EventArgs e) { }

        protected internal virtual void OnMouseDown(MouseEventArgs e) {
            Point pt = e.Location;
            pt.Y -= this._TitleHeight;
            for (int i = this._Controls.Count - 1; i >= 0; i--) {
                var c = this._Controls[i];
                if (c.DisplayRectangle.Contains(pt)) {
                    if (!c.Enabled) return;
                    if (!c.Visable) continue;
                    c.OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, e.X - c.Left, pt.Y - c.Top, e.Delta));
                    m_ctrl_down = c;
                    if (m_ctrl_active != c) {
                        c.OnGotFocus(EventArgs.Empty);
                        if (m_ctrl_active != null) m_ctrl_active.OnLostFocus(EventArgs.Empty);
                        m_ctrl_active = c;
                    }
                    return;
                }
            }
            if (m_ctrl_active != null) m_ctrl_active.OnLostFocus(EventArgs.Empty);
            m_ctrl_active = null;
        }

        protected internal virtual void OnMouseMove(MouseEventArgs e) {
            Point pt = e.Location;
            pt.Y -= this._TitleHeight;
            if (m_ctrl_down != null) {
                if (m_ctrl_down.Enabled && m_ctrl_down.Visable)
                    m_ctrl_down.OnMouseMove(new MouseEventArgs(e.Button, e.Clicks, e.X - m_ctrl_down.Left, pt.Y - m_ctrl_down.Top, e.Delta));
                return;
            }
            for (int i = this._Controls.Count - 1; i >= 0; i--) {
                var c = this._Controls[i];
                if (c.DisplayRectangle.Contains(pt)) {
                    if (m_ctrl_hover != this._Controls[i]) {
                        c.OnMouseEnter(EventArgs.Empty);
                        if (m_ctrl_hover != null) m_ctrl_hover.OnMouseLeave(EventArgs.Empty);
                        m_ctrl_hover = c;
                    }
                    m_ctrl_hover.OnMouseMove(new MouseEventArgs(e.Button, e.Clicks, e.X - c.Left, pt.Y - c.Top, e.Delta));
                    return;
                }
            }
            if (m_ctrl_hover != null) m_ctrl_hover.OnMouseLeave(EventArgs.Empty);
            m_ctrl_hover = null;
        }

        protected internal virtual void OnMouseUp(MouseEventArgs e) {
            Point pt = e.Location;
            pt.Y -= this._TitleHeight;
            if (m_ctrl_down != null && m_ctrl_down.Enabled && m_ctrl_down.Visable) {
                m_ctrl_down.OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, e.X - m_ctrl_down.Left, pt.Y - m_ctrl_down.Top, e.Delta));
            }
            //if (m_ctrl_active != null) {
            //    m_ctrl_active.OnMouseUp(new MouseEventArgs(e.Button, e.Clicks,
            //        e.X - m_ctrl_active.Left, pt.Y - m_ctrl_active.Top, e.Delta));
            //}
            m_ctrl_down = null;
        }

        protected internal virtual void OnMouseLeave(EventArgs e) {
            if (m_ctrl_hover != null && m_ctrl_hover.Enabled && m_ctrl_hover.Visable) m_ctrl_hover.OnMouseLeave(e);
            m_ctrl_hover = null;
        }

        protected internal virtual void OnMouseClick(MouseEventArgs e) {
            Point pt = e.Location;
            pt.Y -= this._TitleHeight;
            if (m_ctrl_active != null && m_ctrl_active.Enabled && m_ctrl_active.Visable)
                m_ctrl_active.OnMouseClick(new MouseEventArgs(e.Button, e.Clicks, e.X - m_ctrl_active.Left, pt.Y - m_ctrl_active.Top, e.Delta));
        }

        protected internal virtual void OnMouseWheel(MouseEventArgs e) {
            Point pt = e.Location;
            pt.Y -= this._TitleHeight;
            if (m_ctrl_hover != null && m_ctrl_active.Enabled && m_ctrl_hover.Visable) {
                m_ctrl_hover.OnMouseWheel(new MouseEventArgs(e.Button, e.Clicks, e.X - m_ctrl_hover.Left, pt.Y - m_ctrl_hover.Top, e.Delta));
                return;
            }
        }
        protected internal virtual void OnMouseHWheel(MouseEventArgs e) {
            if (m_ctrl_hover != null && m_ctrl_active.Enabled && m_ctrl_hover.Visable) {
                m_ctrl_hover.OnMouseHWheel(e);
                return;
            }
        }

        protected internal virtual void OnKeyDown(KeyEventArgs e) {
            if (m_ctrl_active != null && m_ctrl_active.Enabled && m_ctrl_active.Visable) m_ctrl_active.OnKeyDown(e);
        }
        protected internal virtual void OnKeyUp(KeyEventArgs e) {
            if (m_ctrl_active != null && m_ctrl_active.Enabled && m_ctrl_active.Visable) m_ctrl_active.OnKeyUp(e);
        }
        protected internal virtual void OnKeyPress(KeyPressEventArgs e) {
            if (m_ctrl_active != null && m_ctrl_active.Enabled && m_ctrl_active.Visable) m_ctrl_active.OnKeyPress(e);
        }

        protected virtual void OnMove(EventArgs e) { /*this.SetOptionLocation();*/ }
        protected virtual void OnResize(EventArgs e) { /*this.SetOptionLocation();*/ }


        /// <summary>
        /// Occurs when the owner changes
        /// </summary>
        protected virtual void OnOwnerChanged() { }
        /// <summary>
        /// Occurs when the selected state changes
        /// </summary>
        protected virtual void OnSelectedChanged() { }
        /// <summary>
        /// Occurs when the activity state changes
        /// </summary>
        protected virtual void OnActiveChanged() { }

        #endregion protected
        /// <summary>
        /// Calculate the position of each Option
        /// </summary>
        protected virtual void SetOptionsLocation() {
            int nIndex = 0;
            Rectangle rect = new Rectangle(this.Left + 10, this._Top + this._TitleHeight, this._Width - 20, this._ItemHeight);
            foreach (STNodeOption op in this._InputOptions) {
                if (op != STNodeOption.Empty) {
                    Point pt = this.OnSetOptionDotLocation(op, new Point(this.Left - op.DotSize / 2, rect.Y + (rect.Height - op.DotSize) / 2), nIndex);
                    op.TextRectangle = this.OnSetOptionTextRectangle(op, rect, nIndex);
                    op.DotLeft = pt.X;
                    op.DotTop = pt.Y;
                }
                rect.Y += this._ItemHeight;
                nIndex++;
            }
            rect.Y = this._Top + this._TitleHeight;
            m_sf.Alignment = StringAlignment.Far;
            foreach (STNodeOption op in this._OutputOptions) {
                if (op != STNodeOption.Empty) {
                    Point pt = this.OnSetOptionDotLocation(op, new Point(this._Left + this._Width - op.DotSize / 2, rect.Y + (rect.Height - op.DotSize) / 2), nIndex);
                    op.TextRectangle = this.OnSetOptionTextRectangle(op, rect, nIndex);
                    op.DotLeft = pt.X;
                    op.DotTop = pt.Y;
                }
                rect.Y += this._ItemHeight;
                nIndex++;
            }
        }

        /// <summary>
        /// Redraw Node
        /// </summary>
        public void Invalidate() {
            if (this._Owner != null) {
                this._Owner.Invalidate(this._Owner.CanvasToControl(new Rectangle(this._Left - 5, this._Top - 5, this._Width + 10, this._Height + 10)));
            }
        }
        /// <summary>
        /// Redraw the specified area of ​​Node
        /// </summary>
        /// <param name="rect">Node specified area</param>
        public void Invalidate(Rectangle rect) {
            rect.X += this._Left;
            rect.Y += this._Top;
            if (this._Owner != null) {
                rect = this._Owner.CanvasToControl(rect);
                rect.Width += 1; rect.Height += 1;//Coordinate system conversion may cause progress loss plus one more pixel
                this._Owner.Invalidate(rect);
            }
        }
        /// <summary>
        /// Get the input Option collection contained in this Node
        /// </summary>
        /// <returns>Option array</returns>
        public STNodeOption[] GetInputOptions() {
            if (!this._LetGetOptions) return null;
            STNodeOption[] ops = new STNodeOption[this._InputOptions.Count];
            for (int i = 0; i < this._InputOptions.Count; i++) ops[i] = this._InputOptions[i];
            return ops;
        }
        /// <summary>
        /// Get the output Option set contained in this Node
        /// </summary>
        /// <returns>Option array</returns>
        public STNodeOption[] GetOutputOptions() {
            if (!this._LetGetOptions) return null;
            STNodeOption[] ops = new STNodeOption[this._OutputOptions.Count];
            for (int i = 0; i < this._OutputOptions.Count; i++) ops[i] = this._OutputOptions[i];
            return ops;
        }
        /// <summary>
        /// Set the selected state of Node
        /// </summary>
        /// <param name="bSelected">Selected</param>
        /// <param name="bRedraw">Whether to redraw</param>
        public void SetSelected(bool bSelected, bool bRedraw) {
            if (this._IsSelected == bSelected) return;
            this._IsSelected = bSelected;
            if (this._Owner != null) {
                if (bSelected)
                    this._Owner.AddSelectedNode(this);
                else
                    this._Owner.RemoveSelectedNode(this);
            }
            if (bRedraw) this.Invalidate();
            this.OnSelectedChanged();
            if (this._Owner != null) this._Owner.OnSelectedChanged(EventArgs.Empty);
        }
        public IAsyncResult BeginInvoke(Delegate method) { return this.BeginInvoke(method, null); }
        public IAsyncResult BeginInvoke(Delegate method, params object[] args) {
            if (this._Owner == null) return null;
            return this._Owner.BeginInvoke(method, args);
        }
        public object Invoke(Delegate method) { return this.Invoke(method, null); }
        public object Invoke(Delegate method, params object[] args) {
            if (this._Owner == null) return null;
            return this._Owner.Invoke(method, args);
        }
    }
}