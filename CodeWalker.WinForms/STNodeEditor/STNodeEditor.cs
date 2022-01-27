using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.ComponentModel;
using System.Reflection;
using System.IO.Compression;
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
 * create: 2020-12-08
 * modify: 2021-04-12
 * Author: Crystal_lz
 * blog: http://st233.com
 * Gitee: https://gitee.com/DebugST
 * Github: https://github.com/DebugST
 */
namespace ST.Library.UI.NodeEditor
{
    public class STNodeEditor : Control
    {
        private const UInt32 WM_MOUSEHWHEEL = 0x020E;
        protected static readonly Type m_type_node = typeof(STNode);

        #region protected enum,struct --------------------------------------------------------------------------------------

        protected enum CanvasAction //Which of the following actions is performed by the current mouse movement operation
        {
            None, //None
            MoveNode, // is moving Node
            ConnectOption, //Connecting Option
            SelectRectangle, // is selecting a rectangular area
            DrawMarkDetails //Drawing mark information details
        }

        protected struct MagnetInfo
        {
            public bool XMatched; //Whether there is a magnet matching on the X axis
            public bool YMatched;
            public int X; //match the number on the X axis
            public int Y;
            public int OffsetX; //The relative offset between the current node X position and the matching X
            public int OffsetY;
        }

        #endregion

        #region Properties ------------------------------------------------------------------------------------------------------

        private float _CanvasOffsetX;
        /// <summary>
        /// Get the offset position of the canvas origin relative to the X direction of the control
        /// </summary>
        [Browsable(false)]
        public float CanvasOffsetX {
            get { return _CanvasOffsetX; }
        }

        private float _CanvasOffsetY;
        /// <summary>
        /// Get the offset position of the canvas origin relative to the Y direction of the control
        /// </summary>
        [Browsable(false)]
        public float CanvasOffsetY {
            get { return _CanvasOffsetY; }
        }

        private PointF _CanvasOffset;
        /// <summary>
        /// Get the offset position of the canvas origin relative to the control
        /// </summary>
        [Browsable(false)]
        public PointF CanvasOffset {
            get {
                _CanvasOffset.X = _CanvasOffsetX;
                _CanvasOffset.Y = _CanvasOffsetY;
                return _CanvasOffset;
            }
        }

        private Rectangle _CanvasValidBounds;
        /// <summary>
        /// Get the valid area in the canvas that is used
        /// </summary>
        [Browsable(false)]
        public Rectangle CanvasValidBounds {
            get { return _CanvasValidBounds; }
        }

        private float _CanvasScale = 1;
        /// <summary>
        /// Get the zoom ratio of the canvas
        /// </summary>
        [Browsable(false)]
        public float CanvasScale {
            get { return _CanvasScale; }
        }

        private float _Curvature = 0.3F;
        /// <summary>
        /// Gets or sets the curvature of the connection between Option
        /// </summary>
        [Browsable(false)]
        public float Curvature {
            get { return _Curvature; }
            set {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                _Curvature = value;
                if (m_dic_gp_info.Count != 0) this.BuildLinePath();
            }
        }

        private bool _ShowMagnet = true;
        /// <summary>
        /// Gets or sets whether to enable the magnet effect when moving the Node in the canvas
        /// </summary>
        [Description("Get or set whether to enable the magnet effect when moving Node in the canvas"), DefaultValue(true)]
        public bool ShowMagnet {
            get { return _ShowMagnet; }
            set { _ShowMagnet = value; }
        }

        private bool _ShowBorder = true;
        /// <summary>
        /// Gets or sets whether to display the Node border in the mobile canvas
        /// </summary>
        [Description("Get or set whether to display the Node border in the mobile canvas"), DefaultValue(true)]
        public bool ShowBorder {
            get { return _ShowBorder; }
            set {
                _ShowBorder = value;
                this.Invalidate();
            }
        }

        private bool _ShowGrid = true;
        /// <summary>
        /// Gets or sets whether to draw background grid lines in the canvas
        /// </summary>
        [Description("Get or set whether to draw background grid lines in the canvas"), DefaultValue(true)]
        public bool ShowGrid {
            get { return _ShowGrid; }
            set {
                _ShowGrid = value;
                this.Invalidate();
            }
        }

        private bool _ShowLocation = true;
        /// <summary>
        /// Gets or sets whether to display Node position information beyond the viewing angle at the edge of the canvas
        /// </summary>
        [Description("Get or set whether to display Node position information beyond the viewing angle at the edge of the canvas"), DefaultValue(true)]
        public bool ShowLocation {
            get { return _ShowLocation; }
            set {
                _ShowLocation = value;
                this.Invalidate();
            }
        }

        private STNodeCollection _Nodes;
        /// <summary>
        /// Get the Node collection in the canvas
        /// </summary>
        [Browsable(false)]
        public STNodeCollection Nodes {
            get {
                return _Nodes;
            }
        }

        private STNode _ActiveNode;
        /// <summary>
        /// Get the active Node selected in the current canvas
        /// </summary>
        [Browsable(false)]
        public STNode ActiveNode {
            get { return _ActiveNode; }
            //set {
            //    if (value == _ActiveSelectedNode) return;
            //    if (_ActiveSelectedNode != null) _ActiveSelectedNode.OnLostFocus(EventArgs.Empty);
            //    _ActiveSelectedNode = value;
            //    _ActiveSelectedNode.IsActive = true;
            //    this.Invalidate();
            //    this.OnSelectedChanged(EventArgs.Empty);
            //}
        }

        private STNode _HoverNode;
        /// <summary>
        /// Get the Node that the mouse hovers over in the current canvas
        /// </summary>
        [Browsable(false)]
        public STNode HoverNode {
            get { return _HoverNode; }
        }
        //========================================color================================
        private Color _GridColor = Color.Black;
        /// <summary>
        /// Gets or sets the grid line color when drawing the canvas background
        /// </summary>
        [Description("Get or set the grid line color when drawing the canvas background"), DefaultValue(typeof(Color), "Black")]
        public Color GridColor {
            get { return _GridColor; }
            set {
                _GridColor = value;
                this.Invalidate();
            }
        }

        private Color _BorderColor = Color.Black;
        /// <summary>
        /// Gets or sets the border color of the Node in the canvas
        /// </summary>
        [Description("Get or set the border color of Node in the canvas"), DefaultValue(typeof(Color), "Black")]
        public Color BorderColor {
            get { return _BorderColor; }
            set {
                _BorderColor = value;
                if (m_img_border != null) m_img_border.Dispose();
                m_img_border = this.CreateBorderImage(value);
                this.Invalidate();
            }
        }

        private Color _BorderHoverColor = Color.Gray;
        /// <summary>
        /// Gets or sets the border color of the hovering Node in the canvas
        /// </summary>
        [Description("Get or set the border color of the hovering Node in the canvas"), DefaultValue(typeof(Color), "Gray")]
        public Color BorderHoverColor {
            get { return _BorderHoverColor; }
            set {
                _BorderHoverColor = value;
                if (m_img_border_hover != null) m_img_border_hover.Dispose();
                m_img_border_hover = this.CreateBorderImage(value);
                this.Invalidate();
            }
        }

        private Color _BorderSelectedColor = Color.Orange;
        /// <summary>
        /// Gets or sets the border color of the selected Node in the canvas
        /// </summary>
        [Description("Get or set the border color of the selected Node in the canvas"), DefaultValue(typeof(Color), "Orange")]
        public Color BorderSelectedColor {
            get { return _BorderSelectedColor; }
            set {
                _BorderSelectedColor = value;
                if (m_img_border_selected != null) m_img_border_selected.Dispose();
                m_img_border_selected = this.CreateBorderImage(value);
                this.Invalidate();
            }
        }

        private Color _BorderActiveColor = Color.OrangeRed;
        /// <summary>
        /// Gets or sets the border color of the active Node in the canvas
        /// </summary>
        [Description("Get or set the border color of the active Node in the canvas"), DefaultValue(typeof(Color), "OrangeRed")]
        public Color BorderActiveColor {
            get { return _BorderActiveColor; }
            set {
                _BorderActiveColor = value;
                if (m_img_border_active != null) m_img_border_active.Dispose();
                m_img_border_active = this.CreateBorderImage(value);
                this.Invalidate();
            }
        }

        private Color _MarkForeColor = Color.White;
        /// <summary>
        /// Gets or sets the foreground color used by the canvas to draw the Node tag details
        /// </summary>
        [Description("Get or set the foreground color of the canvas drawing Node tag details"), DefaultValue(typeof(Color), "White")]
        public Color MarkForeColor {
            get { return _MarkBackColor; }
            set {
                _MarkBackColor = value;
                this.Invalidate();
            }
        }

        private Color _MarkBackColor = Color.FromArgb(180, Color.Black);
        /// <summary>
        /// Gets or sets the background color used by the canvas to draw the Node tag details
        /// </summary>
        [Description("Get or set the background color used for drawing Node tag details on canvas")]
        public Color MarkBackColor {
            get { return _MarkBackColor; }
            set {
                _MarkBackColor = value;
                this.Invalidate();
            }
        }

        private Color _MagnetColor = Color.Lime;
        /// <summary>
        /// Gets or sets the color of the magnet marker when moving the Node in the canvas
        /// </summary>
        [Description("Get or set the magnet mark color when moving Node in the canvas"), DefaultValue(typeof(Color), "Lime")]
        public Color MagnetColor {
            get { return _MagnetColor; }
            set { _MagnetColor = value; }
        }

        private Color _SelectedRectangleColor = Color.DodgerBlue;
        /// <summary>
        /// Gets or sets the color of the selection rectangle in the canvas
        /// </summary>
        [Description("Get or set the color of the selection rectangle in the canvas"), DefaultValue(typeof(Color), "DodgerBlue")]
        public Color SelectedRectangleColor {
            get { return _SelectedRectangleColor; }
            set { _SelectedRectangleColor = value; }
        }

        private Color _HighLineColor = Color.Cyan;
        /// <summary>
        /// Gets or sets the color of the highlighted line in the canvas
        /// </summary>
        [Description("Get or set the color of the highlighted line in the canvas"), DefaultValue(typeof(Color), "Cyan")]
        public Color HighLineColor {
            get { return _HighLineColor; }
            set { _HighLineColor = value; }
        }

        private Color _LocationForeColor = Color.Red;
        /// <summary>
        /// Get or set the foreground color of the edge position hint area in the canvas
        /// </summary>
        [Description("Get or set the foreground color of the edge position hint area in the canvas"), DefaultValue(typeof(Color), "Red")]
        public Color LocationForeColor {
            get { return _LocationForeColor; }
            set {
                _LocationForeColor = value;
                this.Invalidate();
            }
        }

        private Color _LocationBackColor = Color.FromArgb(120, Color.Black);
        /// <summary>
        /// Get or set the background color of the edge position hint area in the canvas
        /// </summary>
        [Description("Get or set the background color of the edge position hint area in the canvas")]
        public Color LocationBackColor {
            get { return _LocationBackColor; }
            set {
                _LocationBackColor = value;
                this.Invalidate();
            }
        }

        private Color _UnknownTypeColor = Color.Gray;
        /// <summary>
        /// Gets or sets the color that should be used in the canvas when the Option data type in Node cannot be determined
        /// </summary>
        [Description("Get or set the color that should be used when the Option data type in Node cannot be determined"), DefaultValue(typeof(Color), "Gray")]
        public Color UnknownTypeColor {
            get { return _UnknownTypeColor; }
            set {
                _UnknownTypeColor = value;
                this.Invalidate();
            }
        }

        private Dictionary<Type, Color> _TypeColor = new Dictionary<Type, Color>();
        /// <summary>
        /// Get or set the preset color of the Option data type in the Node in the canvas
        /// </summary>
        [Browsable(false)]
        public Dictionary<Type, Color> TypeColor {
            get { return _TypeColor; }
        }

        #endregion

        #region protected properties ----------------------------------------------------------------------------------------
        /// <summary>
        /// The real-time position of the current mouse in the control
        /// </summary>
        protected Point m_pt_in_control;
        /// <summary>
        /// The real-time position of the current mouse in the canvas
        /// </summary>
        protected PointF m_pt_in_canvas;
        /// <summary>
        /// The position on the control when the mouse is clicked
        /// </summary>
        protected Point m_pt_down_in_control;
        /// <summary>
        /// The position in the canvas when the mouse is clicked
        /// </summary>
        protected PointF m_pt_down_in_canvas;
        /// <summary>
        /// Used for the coordinate position of the canvas when the mouse is clicked when the mouse is clicked to move the canvas
        /// </summary>
        protected PointF m_pt_canvas_old;
        /// <summary>
        /// Used to save the starting point coordinates of the Option under the save point during the connection process
        /// </summary>
        protected Point m_pt_dot_down;
        /// <summary>
        /// Used to save the starting point of the mouse click during the connection process. Option When MouseUP determines whether to connect this node
        /// </summary>
        protected STNodeOption m_option_down;
        /// <summary>
        /// STNode under the current mouse click
        /// </summary>
        protected STNode m_node_down;
        /// <summary>
        /// Whether the current mouse is in the control
        /// </summary>
        protected bool m_mouse_in_control;

        #endregion

        public STNodeEditor() {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this._Nodes = new STNodeCollection(this);
            this.BackColor = Color.FromArgb(255, 34, 34, 34);
            this.MinimumSize = new Size(100, 100);
            this.Size = new Size(200, 200);
            this.AllowDrop = true;

            m_real_canvas_x = this._CanvasOffsetX = 10;
            m_real_canvas_y = this._CanvasOffsetY = 10;
        }

        #region private fields --------------------------------------------------------------------------------------

        private DrawingTools m_drawing_tools;
        private NodeFindInfo m_find = new NodeFindInfo();
        private MagnetInfo m_mi = new MagnetInfo ();

        private RectangleF m_rect_select = new RectangleF();
        //Node border preset pattern
        private Image m_img_border;
        private Image m_img_border_hover;
        private Image m_img_border_selected;
        private Image m_img_border_active;
        //Used for the animation effect when the mouse scrolls or the touchpad moves the canvas. This value is the real coordinate address that needs to be moved to view ->MoveCanvasThread()
        private float m_real_canvas_x;
        private float m_real_canvas_y;
        //Used to save the initial coordinates of the node selected when the mouse is clicked when moving the node
        private Dictionary<STNode, Point> m_dic_pt_selected = new Dictionary<STNode, Point>();
        //For the magnet effect to move the node, the statistics of the non-selected nodes need to participate in the coordinate view of the magnet effect ->BuildMagnetLocation()
        private List<int> m_lst_magnet_x = new List<int>();
        private List<int> m_lst_magnet_y = new List<int>();
        //Used for the magnet effect to move the node when the active selection node is counted to check the coordinates that need to participate in the magnet effect ->CheckMagnet()
        private List<int> m_lst_magnet_mx = new List<int>();
        private List<int> m_lst_magnet_my = new List<int>();
        //Used to calculate the time trigger interval in mouse scrolling. View the displacement generated by different canvases according to the interval -> OnMouseWheel(), OnMouseHWheel()
        private DateTime m_dt_vw = DateTime.Now;
        private DateTime m_dt_hw = DateTime.Now;
        // current behavior during mouse movement
        private CanvasAction m_ca;
        // save the selected node
        private HashSet<STNode> m_hs_node_selected = new HashSet<STNode>();

        private bool m_is_process_mouse_event = true; //Whether to pass down (Node or NodeControls) mouse related events such as disconnection related operations should not pass down
        private bool m_is_buildpath; //The path used to determine whether to re-establish the cache connection this time during the redraw process
        private Pen m_p_line = new Pen(Color.Cyan, 2f); // used to draw connected lines
        private Pen m_p_line_hover = new Pen(Color.Cyan, 4f); //Used to draw the line when the mouse is hovered
        private GraphicsPath m_gp_hover; //The path of the current mouse hover
        private StringFormat m_sf = new StringFormat(); //The text format is used to set the text format when Mark draws
        //Save the node relationship corresponding to each connection line
        private Dictionary<GraphicsPath, ConnectionInfo> m_dic_gp_info = new Dictionary<GraphicsPath, ConnectionInfo>();
        //Save the position of Node beyond the visual area
        private List<Point> m_lst_node_out = new List<Point>();
        //The Node type loaded by the current editor is used to load nodes from files or data
        private Dictionary<string, Type> m_dic_type = new Dictionary<string, Type>();

        private int m_time_alert;
        private int m_alpha_alert;
        private string m_str_alert;
        private Color m_forecolor_alert;
        private Color m_backcolor_alert;
        private DateTime m_dt_alert;
        private Rectangle m_rect_alert;
        private AlertLocation m_al;

        #endregion

        #region event ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the active node changes
        /// </summary>
        [Description("Occurs when the active node changes")]
        public event EventHandler ActiveChanged;
        /// <summary>
        /// Occurs when the selected node changes
        /// </summary>
        [Description("Occurs when the selected node changes")]
        public event EventHandler SelectedChanged;
        /// <summary>
        /// Occurs when the hovered node changes
        /// </summary>
        [Description("Occurs when the hovered node changes")]
        public event EventHandler HoverChanged;
        /// <summary>
        /// Occurs when a node is added
        /// </summary>
        [Description("Occurs when a node is added")]
        public event STNodeEditorEventHandler NodeAdded;
        /// <summary>
        /// Occurs when the node is removed
        /// </summary>
        [Description("Occurs when a node is removed")]
        public event STNodeEditorEventHandler NodeRemoved;
        /// <summary>
        /// Occurs when the origin of the canvas is moved
        /// </summary>
        [Description("Occurs when moving the origin of the canvas")]
        public event EventHandler CanvasMoved;
        /// <summary>
        /// Occurs when the canvas is zoomed
        /// </summary>
        [Description("Occurs when zooming the canvas")]
        public event EventHandler CanvasScaled;
        /// <summary>
        /// Occurs when connecting node options
        /// </summary>
        [Description("Occurs when connecting node options")]
        public event STNodeEditorOptionEventHandler OptionConnected;
        /// <summary>
        /// Occurs when connecting node options
        /// </summary>
        [Description("Occurs when connecting node options")]
        public event STNodeEditorOptionEventHandler OptionConnecting;
        /// <summary>
        /// Occurs when the node option is disconnected
        /// </summary>
        [Description("Occurs when the node option is disconnected")]
        public event STNodeEditorOptionEventHandler OptionDisConnected;
        /// <summary>
        /// Occurs when disconnecting node options
        /// </summary>
        [Description("Occurs when disconnecting node options")]
        public event STNodeEditorOptionEventHandler OptionDisConnecting;

        protected virtual internal void OnSelectedChanged(EventArgs e) {
            if (this.SelectedChanged != null) this.SelectedChanged(this, e);
        }
        protected virtual void OnActiveChanged(EventArgs e) {
            if (this.ActiveChanged != null) this.ActiveChanged(this, e);
        }
        protected virtual void OnHoverChanged(EventArgs e) {
            if (this.HoverChanged != null) this.HoverChanged(this, e);
        }
        protected internal virtual void OnNodeAdded(STNodeEditorEventArgs e) {
            if (this.NodeAdded != null) this.NodeAdded(this, e);
        }
        protected internal virtual void OnNodeRemoved(STNodeEditorEventArgs e) {
            if (this.NodeRemoved != null) this.NodeRemoved(this, e);
        }
        protected virtual void OnCanvasMoved(EventArgs e) {
            if (this.CanvasMoved != null) this.CanvasMoved(this, e);
        }
        protected virtual void OnCanvasScaled(EventArgs e) {
            if (this.CanvasScaled != null) this.CanvasScaled(this, e);
        }
        protected internal virtual void OnOptionConnected(STNodeEditorOptionEventArgs e) {
            if (this.OptionConnected != null) this.OptionConnected(this, e);
        }
        protected internal virtual void OnOptionDisConnected(STNodeEditorOptionEventArgs e) {
            if (this.OptionDisConnected != null) this.OptionDisConnected(this, e);
        }
        protected internal virtual void OnOptionConnecting(STNodeEditorOptionEventArgs e) {
            if (this.OptionConnecting != null) this.OptionConnecting(this, e);
        }
        protected internal virtual void OnOptionDisConnecting(STNodeEditorOptionEventArgs e) {
            if (this.OptionDisConnecting != null) this.OptionDisConnecting(this, e);
        }

        #endregion event

        #region override -----------------------------------------------------------------------------------------------------

        protected override void OnCreateControl() {
            m_drawing_tools = new DrawingTools() {
                Pen = new Pen(Color.Black, 1),
                SolidBrush = new SolidBrush(Color.Black)
            };
            m_img_border = this.CreateBorderImage(this._BorderColor);
            m_img_border_active = this.CreateBorderImage(this._BorderActiveColor);
            m_img_border_hover = this.CreateBorderImage(this._BorderHoverColor);
            m_img_border_selected = this.CreateBorderImage(this._BorderSelectedColor);
            base.OnCreateControl();
            new Thread(this.MoveCanvasThread) { IsBackground = true }.Start();
            new Thread(this.ShowAlertThread) { IsBackground = true }.Start();
            m_sf = new StringFormat();
            m_sf.Alignment = StringAlignment.Near;
            m_sf.FormatFlags = StringFormatFlags.NoWrap;
            m_sf.SetTabStops(0, new float[] { 40 });
        }

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            try {
                Point pt = new Point(((int)m.LParam) >> 16, (ushort)m.LParam);
                pt = this.PointToClient(pt);
                if (m.Msg == WM_MOUSEHWHEEL) { //Get the horizontal scrolling message
                    MouseButtons mb = MouseButtons.None;
                    int n = (ushort)m.WParam;
                    if ((n & 0x0001) == 0x0001) mb |= MouseButtons.Left;
                    if ((n & 0x0010) == 0x0010) mb |= MouseButtons.Middle;
                    if ((n & 0x0002) == 0x0002) mb |= MouseButtons.Right;
                    if ((n & 0x0020) == 0x0020) mb |= MouseButtons.XButton1;
                    if ((n & 0x0040) == 0x0040) mb |= MouseButtons.XButton2;
                    this.OnMouseHWheel(new MouseEventArgs(mb, 0, pt.X, pt.Y, ((int)m.WParam) >> 16));
                }
            } catch { /*add code*/ }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(this.BackColor);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            m_drawing_tools.Graphics = g;
            SolidBrush brush = m_drawing_tools.SolidBrush;

            if (this._ShowGrid) this.OnDrawGrid(m_drawing_tools, this.Width, this.Height);

            g.TranslateTransform(this._CanvasOffsetX, this._CanvasOffsetY); //Move the coordinate system
            g.ScaleTransform(this._CanvasScale, this._CanvasScale); //Scale the drawing surface

            this.OnDrawConnectedLine(m_drawing_tools);
            this.OnDrawNode(m_drawing_tools, this.ControlToCanvas(this.ClientRectangle));

            if (m_ca == CanvasAction.ConnectOption) { //If connecting
                m_drawing_tools.Pen.Color = this._HighLineColor;
                g.SmoothingMode = SmoothingMode.HighQuality;
                if (m_option_down.IsInput)
                    this.DrawBezier(g, m_drawing_tools.Pen, m_pt_in_canvas, m_pt_dot_down, this._Curvature);
                else
                    this.DrawBezier(g, m_drawing_tools.Pen, m_pt_dot_down, m_pt_in_canvas, this._Curvature);
            }
            //Reset the drawing coordinates I think other decoration-related drawing other than nodes should not be drawn in the Canvas coordinate system but should be drawn using the coordinates of the control, otherwise it will be affected by the zoom ratio
            g.ResetTransform();

            switch (m_ca) {
                case CanvasAction.MoveNode: //Draw alignment guides during movement
                    if (this._ShowMagnet && this._ActiveNode != null) this.OnDrawMagnet(m_drawing_tools, m_mi);
                    break;
                case CanvasAction.SelectRectangle: //Draw rectangle selection
                    this.OnDrawSelectedRectangle(m_drawing_tools, this.CanvasToControl(m_rect_select));
                    break;
                case CanvasAction.DrawMarkDetails: //Draw mark information details
                    if (!string.IsNullOrEmpty(m_find.Mark)) this.OnDrawMark(m_drawing_tools);
                    break;
            }

            if (this._ShowLocation) this.OnDrawNodeOutLocation(m_drawing_tools, this.Size, m_lst_node_out);
            this.OnDrawAlert(g);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            this.Focus();
            m_ca = CanvasAction.None;
            m_mi.XMatched = m_mi.YMatched = false;
            m_pt_down_in_control = e.Location;
            m_pt_down_in_canvas.X = ((e.X - this._CanvasOffsetX) / this._CanvasScale);
            m_pt_down_in_canvas.Y = ((e.Y - this._CanvasOffsetY) / this._CanvasScale);
            m_pt_canvas_old.X = this._CanvasOffsetX;
            m_pt_canvas_old.Y = this._CanvasOffsetY;

            if (m_gp_hover != null && e.Button == MouseButtons.Right) { //Disconnect
                this.DisConnectionHover();
                m_is_process_mouse_event = false; // Terminate the downward transmission of MouseClick and MouseUp
                return;
            }

            NodeFindInfo nfi = this.FindNodeFromPoint(m_pt_down_in_canvas);
            if (!string.IsNullOrEmpty(nfi.Mark)) { //If the point is marked information
                m_ca = CanvasAction.DrawMarkDetails;
                this.Invalidate();
                return;
            }

            if (nfi.NodeOption != null) { //If the connection point of the Option under the point
                this.StartConnect(nfi.NodeOption);
                return;
            }

            if (nfi.Node != null) {
                nfi.Node.OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, (int)m_pt_down_in_canvas.X - nfi.Node.Left, (int)m_pt_down_in_canvas.Y - nfi.Node.Top, e.Delta));
                bool bCtrlDown = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                if (bCtrlDown) {
                    if (nfi.Node.IsSelected) {
                        if (nfi.Node == this._ActiveNode) {
                            this.SetActiveNode(null);
                        }
                    } else {
                        nfi.Node.SetSelected(true, true);
                    }
                    return;
                } else if (!nfi.Node.IsSelected) {
                    foreach (var n in m_hs_node_selected.ToArray()) n.SetSelected(false, false);
                }
                nfi.Node.SetSelected(true, false); //Add to the selected node
                this.SetActiveNode(nfi.Node);
                if (this.PointInRectangle(nfi.Node.TitleRectangle, m_pt_down_in_canvas.X, m_pt_down_in_canvas.Y)) {
                    if (e.Button == MouseButtons.Right) {
                        if (nfi.Node.ContextMenuStrip != null) {
                            nfi.Node.ContextMenuStrip.Show(this.PointToScreen(e.Location));
                        }
                    } else {
                        m_dic_pt_selected.Clear();
                        lock (m_hs_node_selected) {
                            foreach (STNode n in m_hs_node_selected) //Record the position of the selected node, which will be useful if you need to move the selected node
                                m_dic_pt_selected.Add(n, n.Location);
                        }
                        m_ca = CanvasAction.MoveNode; //If the title of the node is under the point, the node can be moved
                        if (this._ShowMagnet && this._ActiveNode != null) this.BuildMagnetLocation(); //The coordinates needed to build the magnet will be useful if you need to move the selected node
                    }
                } else
                    m_node_down = nfi.Node;
            } else {
                this.SetActiveNode(null);
                foreach (var n in m_hs_node_selected.ToArray()) n.SetSelected(false, false);//Empty the selected node without clicking anything
                m_ca = CanvasAction.SelectRectangle; //Enter rectangular area selection mode
                m_rect_select.Width = m_rect_select.Height = 0;
                m_node_down = null;
            }
            //this.SetActiveNode(nfi.Node);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            m_pt_in_control = e.Location;
            m_pt_in_canvas.X = ((e.X - this._CanvasOffsetX) / this._CanvasScale);
            m_pt_in_canvas.Y = ((e.Y - this._CanvasOffsetY) / this._CanvasScale);

            if (m_node_down != null) {
                m_node_down.OnMouseMove(new MouseEventArgs(e.Button, e.Clicks,
                    (int)m_pt_in_canvas.X - m_node_down.Left,
                    (int)m_pt_in_canvas.Y - m_node_down.Top, e.Delta));
                return;
            }

            if (e.Button == MouseButtons.Middle) { //The middle mouse button moves the canvas
                this._CanvasOffsetX = m_real_canvas_x = m_pt_canvas_old.X + (e.X - m_pt_down_in_control.X);
                this._CanvasOffsetY = m_real_canvas_y = m_pt_canvas_old.Y + (e.Y - m_pt_down_in_control.Y);
                this.Invalidate();
                return;
            }
            if (e.Button == MouseButtons.Left) { //If the left mouse button is clicked to judge the behavior
                m_gp_hover = null;
                switch (m_ca) {
                    case CanvasAction.MoveNode: this.MoveNode(e.Location); return; //The current moving node
                    case CanvasAction.ConnectOption: this.Invalidate(); return; //Currently connecting
                    case CanvasAction.SelectRectangle: //Currently being selected
                        m_rect_select.X = m_pt_down_in_canvas.X < m_pt_in_canvas.X ? m_pt_down_in_canvas.X : m_pt_in_canvas.X;
                        m_rect_select.Y = m_pt_down_in_canvas.Y < m_pt_in_canvas.Y ? m_pt_down_in_canvas.Y : m_pt_in_canvas.Y;
                        m_rect_select.Width = Math.Abs(m_pt_in_canvas.X - m_pt_down_in_canvas.X);
                        m_rect_select.Height = Math.Abs(m_pt_in_canvas.Y - m_pt_down_in_canvas.Y);
                        foreach (STNode n in this._Nodes) {
                            n.SetSelected(m_rect_select.IntersectsWith(n.Rectangle), false);
                        }
                        this.Invalidate();
                        return;
                }
            }
            //If there is no behavior, determine whether there are other objects under the mouse
            NodeFindInfo nfi = this.FindNodeFromPoint(m_pt_in_canvas);
            bool bRedraw = false;
            if (this._HoverNode != nfi.Node) { //Mouse over Node
                if (nfi.Node != null) nfi.Node.OnMouseEnter(EventArgs.Empty);
                if (this._HoverNode != null)
                    this._HoverNode.OnMouseLeave(new MouseEventArgs(e.Button, e.Clicks,
                        (int)m_pt_in_canvas.X - this._HoverNode.Left,
                        (int)m_pt_in_canvas.Y - this._HoverNode.Top, e.Delta));
                this._HoverNode = nfi.Node;
                this.OnHoverChanged(EventArgs.Empty);
                bRedraw = true;
            }
            if (this._HoverNode != null) {
                this._HoverNode.OnMouseMove(new MouseEventArgs(e.Button, e.Clicks,
                    (int)m_pt_in_canvas.X - this._HoverNode.Left,
                    (int)m_pt_in_canvas.Y - this._HoverNode.Top, e.Delta));
                m_gp_hover = null;
            } else {
                GraphicsPath gp = null;
                foreach (var v in m_dic_gp_info) { //Determine whether the mouse hovers over the connection path
                    if (v.Key.IsOutlineVisible(m_pt_in_canvas, m_p_line_hover)) {
                        gp = v.Key;
                        break;
                    }
                }
                if (m_gp_hover != gp) {
                    m_gp_hover = gp;
                    bRedraw = true;
                }
            }
            if (bRedraw) this.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            var nfi = this.FindNodeFromPoint(m_pt_in_canvas);
            switch (m_ca) { //Judgment behavior when the mouse is raised
                case CanvasAction.MoveNode: //If the Node is being moved, re-record the current position
                    foreach (STNode n in m_dic_pt_selected.Keys.ToList()) m_dic_pt_selected[n] = n.Location;
                    break;
                case CanvasAction.ConnectOption: //If it is connecting, end the connection
                    if (e.Location == m_pt_down_in_control) break;
                    if (nfi.NodeOption != null) {
                        if (m_option_down.IsInput)
                            nfi.NodeOption.ConnectOption(m_option_down);
                        else
                            m_option_down.ConnectOption(nfi.NodeOption);
                    }
                    break;
            }
            if (m_is_process_mouse_event && this._ActiveNode != null) {
                var mea = new MouseEventArgs(e.Button, e.Clicks,
                    (int)m_pt_in_canvas.X - this._ActiveNode.Left,
                    (int)m_pt_in_canvas.Y - this._ActiveNode.Top, e.Delta);
                this._ActiveNode.OnMouseUp(mea);
                m_node_down = null;
            }
            m_is_process_mouse_event = true; //Currently, no event delivery is performed for the disconnection operation, and the event will be accepted next time
            m_ca = CanvasAction.None;
            this.Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) {
            base.OnMouseEnter(e);
            m_mouse_in_control = true;
        }

        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            m_mouse_in_control = false;
            if (this._HoverNode != null) this._HoverNode.OnMouseLeave(e);
            this._HoverNode = null;
            this.Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control) {
                float f = this._CanvasScale + (e.Delta < 0 ? -0.1f : 0.1f);
                this.ScaleCanvas(f, this.Width / 2, this.Height / 2);
            } else {
                if (!m_mouse_in_control) return;
                var nfi = this.FindNodeFromPoint(m_pt_in_canvas);
                if (this._HoverNode != null) {
                    this._HoverNode.OnMouseWheel(new MouseEventArgs(e.Button, e.Clicks,
                        (int)m_pt_in_canvas.X - this._HoverNode.Left,
                        (int)m_pt_in_canvas.Y - this._HoverNode.Top, e.Delta));
                    return;
                }
                int t = (int)DateTime.Now.Subtract(m_dt_vw).TotalMilliseconds;
                if (t <= 30) t = 40;
                else if (t <= 100) t = 20;
                else if (t <= 150) t = 10;
                else if (t <= 300) t = 4;
                else t = 2;
                this.MoveCanvas(this._CanvasOffsetX, m_real_canvas_y + (e.Delta < 0 ? -t : t), true, CanvasMoveArgs.Top);//process mouse mid
                m_dt_vw = DateTime.Now;
            }
        }

        protected virtual void OnMouseHWheel(MouseEventArgs e) {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control) return;
            if (!m_mouse_in_control) return;
            if (this._HoverNode != null) {
                this._HoverNode.OnMouseWheel(new MouseEventArgs(e.Button, e.Clicks,
                    (int)m_pt_in_canvas.X - this._HoverNode.Left,
                    (int)m_pt_in_canvas.Y - this._HoverNode.Top, e.Delta));
                return;
            }
            int t = (int)DateTime.Now.Subtract(m_dt_hw).TotalMilliseconds;
            if (t <= 30) t = 40;
            else if (t <= 100) t = 20;
            else if (t <= 150) t = 10;
            else if (t <= 300) t = 4;
            else t = 2;
            this.MoveCanvas(m_real_canvas_x + (e.Delta > 0 ? -t : t), this._CanvasOffsetY, true, CanvasMoveArgs.Left);
            m_dt_hw = DateTime.Now;
        }
        //===========================for node other event==================================
        protected override void OnMouseClick(MouseEventArgs e) {
            base.OnMouseClick(e);
            if (this._ActiveNode != null && m_is_process_mouse_event) {
                if (!this.PointInRectangle(this._ActiveNode.Rectangle, m_pt_in_canvas.X, m_pt_in_canvas.Y)) return;
                this._ActiveNode.OnMouseClick(new MouseEventArgs(e.Button, e.Clicks,
                    (int)m_pt_down_in_canvas.X - this._ActiveNode.Left,
                    (int)m_pt_down_in_canvas.Y - this._ActiveNode.Top, e.Delta));
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (this._ActiveNode != null) this._ActiveNode.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);
            if (this._ActiveNode != null) this._ActiveNode.OnKeyUp(e);
            m_node_down = null;
        }

        protected override void OnKeyPress(KeyPressEventArgs e) {
            base.OnKeyPress(e);
            if (this._ActiveNode != null) this._ActiveNode.OnKeyPress(e);
        }

        #endregion

        protected override void OnDragEnter(DragEventArgs drgevent) {
            base.OnDragEnter(drgevent);
            if (this.DesignMode) return;
            if (drgevent.Data.GetDataPresent("STNodeType"))
                drgevent.Effect = DragDropEffects.Copy;
            else
                drgevent.Effect = DragDropEffects.None;

        }

        protected override void OnDragDrop(DragEventArgs drgevent) {
            base.OnDragDrop(drgevent);
            if (this.DesignMode) return;
            if (drgevent.Data.GetDataPresent("STNodeType")) {
                object data = drgevent.Data.GetData("STNodeType");
                if (!(data is Type)) return;
                var t = (Type)data;
                if (!t.IsSubclassOf(typeof(STNode))) return;
                STNode node = (STNode)Activator.CreateInstance((t));
                Point pt = new Point(drgevent.X, drgevent.Y);
                pt = this.PointToClient(pt);
                pt = this.ControlToCanvas(pt);
                node.Left = pt.X; node.Top = pt.Y;
                this.Nodes.Add(node);
            }
        }

        #region protected ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the background grid lines are drawn
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        /// <param name="nWidth">Need to draw width</param>
        /// <param name="nHeight">Need to draw height</param>
        protected virtual void OnDrawGrid(DrawingTools dt, int nWidth, int nHeight) {
            Graphics g = dt.Graphics;
            using (Pen p_2 = new Pen(Color.FromArgb(65, this._GridColor))) {
                using (Pen p_1 = new Pen(Color.FromArgb(30, this._GridColor))) {
                    float nIncrement = (20 * this._CanvasScale); //The interval between grids is drawn according to the scale
                    int n = 5 - (int)(this._CanvasOffsetX / nIncrement);
                    for (float f = this._CanvasOffsetX % nIncrement; f < nWidth; f += nIncrement)
                        g.DrawLine((n++ % 5 == 0 ? p_2 : p_1), f, 0, f, nHeight);
                    n = 5 - (int)(this._CanvasOffsetY / nIncrement);
                    for (float f = this._CanvasOffsetY % nIncrement; f < nHeight; f += nIncrement)
                        g.DrawLine((n++ % 5 == 0 ? p_2 : p_1), 0, f, nWidth, f);
                    // two antennas at the origin
                    p_1.Color = Color.FromArgb(this._Nodes.Count == 0 ? 255 : 120, this._GridColor);
                    g.DrawLine(p_1, this._CanvasOffsetX, 0, this._CanvasOffsetX, nHeight);
                    g.DrawLine(p_1, 0, this._CanvasOffsetY, nWidth, this._CanvasOffsetY);
                }
            }
        }
        /// <summary>
        /// Occurs when the Node is drawn
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        /// <param name="rect">Visible canvas area size</param>
        protected virtual void OnDrawNode(DrawingTools dt, Rectangle rect) {
            m_lst_node_out.Clear(); //Clear the coordinates of Node beyond the visual area
            foreach (STNode n in this._Nodes) {
                if (this._ShowBorder) this.OnDrawNodeBorder(dt, n);
                n.OnDrawNode(dt); //Call Node to draw the main part by itself
                if (!string.IsNullOrEmpty(n.Mark)) n.OnDrawMark(dt); //Call Node to draw the Mark area by itself
                if (!rect.IntersectsWith(n.Rectangle)) {
                    m_lst_node_out.Add(n.Location); //Determine whether this Node is beyond the visual area
                }
            }
        }
        /// <summary>
        /// Occurs when the Node border is drawn
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        /// <param name="node">Target node</param>
        protected virtual void OnDrawNodeBorder(DrawingTools dt, STNode node) {
            Image img_border = null;
            if (this._ActiveNode == node) img_border = m_img_border_active;
            else if (node.IsSelected) img_border = m_img_border_selected;
            else if (this._HoverNode == node) img_border = m_img_border_hover;
            else img_border = m_img_border;
            this.RenderBorder(dt.Graphics, node.Rectangle, img_border);
            if (!string.IsNullOrEmpty(node.Mark)) this.RenderBorder(dt.Graphics, node.MarkRectangle, img_border);
        }
        /// <summary>
        /// Occurs when a connected path is drawn
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        protected virtual void OnDrawConnectedLine(DrawingTools dt) {
            Graphics g = dt.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            m_p_line_hover.Color = Color.FromArgb(50, 0, 0, 0);
            var t = typeof(object);
            foreach (STNode n in this._Nodes) {
                foreach (STNodeOption op in n.OutputOptions) {
                    if (op == STNodeOption.Empty) continue;
                    if (op.DotColor != Color.Transparent) //Determine the line color
                        m_p_line.Color = op.DotColor;
                    else {
                        if (op.DataType == t)
                            m_p_line.Color = this._UnknownTypeColor;
                        else
                            m_p_line.Color = this._TypeColor.ContainsKey(op.DataType) ? this._TypeColor[op.DataType] : this._UnknownTypeColor;//value can not be null
                    }
                    foreach (var v in op.ConnectedOption) {
                        this.DrawBezier(g, m_p_line_hover, op.DotLeft + op.DotSize, op.DotTop + op.DotSize / 2,
                            v.DotLeft - 1, v.DotTop + v.DotSize / 2, this._Curvature);
                        this.DrawBezier(g, m_p_line, op.DotLeft + op.DotSize, op.DotTop + op.DotSize / 2,
                            v.DotLeft - 1, v.DotTop + v.DotSize / 2, this._Curvature);
                        if (m_is_buildpath) { //If the current drawing needs to re-establish the connected path cache
                            GraphicsPath gp = this.CreateBezierPath(op.DotLeft + op.DotSize, op.DotTop + op.DotSize / 2,
                                v.DotLeft - 1, v.DotTop + v.DotSize / 2, this._Curvature);
                            m_dic_gp_info.Add(gp, new ConnectionInfo() { Output = op, Input = v });
                        }
                    }
                }
            }
            m_p_line_hover.Color = this._HighLineColor;
            if (m_gp_hover != null) { //If there is currently a hovered connection road, it will be highlighted and drawn
                g.DrawPath(m_p_line_hover, m_gp_hover);
            }
            m_is_buildpath = false; //The reset flag will not re-build the path cache next time you draw
        }
        /// <summary>
        /// Occurs when drawing Mark details
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        protected virtual void OnDrawMark(DrawingTools dt) {
            Graphics g = dt.Graphics;
            SizeF sz = g.MeasureString(m_find.Mark, this.Font); //Confirm the required size of the text
            Rectangle rect = new Rectangle(m_pt_in_control.X + 15,
                m_pt_in_control.Y + 10,
                (int)sz.Width + 6,
                4 + (this.Font.Height + 4) * m_find.MarkLines.Length); //sz.Height does not consider the line spacing of the text, so the height is calculated by itself

            if (rect.Right > this.Width) rect.X = this.Width - rect.Width;
            if (rect.Bottom > this.Height) rect.Y = this.Height - rect.Height;
            if (rect.X < 0) rect.X = 0;
            if (rect.Y < 0) rect.Y = 0;

            dt.SolidBrush.Color = this._MarkBackColor;
            g.SmoothingMode = SmoothingMode.None;
            g.FillRectangle(dt.SolidBrush, rect); //Draw the background area
            rect.Width--; rect.Height--;
            dt.Pen.Color = Color.FromArgb(255, this._MarkBackColor);
            g.DrawRectangle(dt.Pen, rect);
            dt.SolidBrush.Color = this._MarkForeColor;

            m_sf.LineAlignment = StringAlignment.Center;
            //g.SmoothingMode = SmoothingMode.HighQuality;
            rect.X += 2; rect.Width -= 3;
            rect.Height = this.Font.Height + 4;
            int nY = rect.Y + 2;
            for (int i = 0; i < m_find.MarkLines.Length; i++) { //draw text
                rect.Y = nY + i * (this.Font.Height + 4);
                g.DrawString(m_find.MarkLines[i], this.Font, dt.SolidBrush, rect, m_sf);
            }
        }
        /// <summary>
        /// Occurs when the alignment guide needs to be displayed when moving the Node
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        /// <param name="mi">Matching magnet information</param>
        protected virtual void OnDrawMagnet(DrawingTools dt, MagnetInfo mi) {
            if (this._ActiveNode == null) return;
            Graphics g = dt.Graphics;
            Pen pen = m_drawing_tools.Pen;
            SolidBrush brush = dt.SolidBrush;
            pen.Color = this._MagnetColor;
            brush.Color = Color.FromArgb(this._MagnetColor.A / 3, this._MagnetColor);
            g.SmoothingMode = SmoothingMode.None;
            int nL = this._ActiveNode.Left, nMX = this._ActiveNode.Left + this._ActiveNode.Width / 2, nR = this._ActiveNode.Right;
            int nT = this._ActiveNode.Top, nMY = this._ActiveNode.Top + this._ActiveNode.Height / 2, nB = this._ActiveNode.Bottom;
            if (mi.XMatched) g.DrawLine(pen, this.CanvasToControl(mi.X, true), 0, this.CanvasToControl(mi.X, true), this.Height);
            if (mi.YMatched) g.DrawLine(pen, 0, this.CanvasToControl(mi.Y, false), this.Width, this.CanvasToControl(mi.Y, false));
            g.TranslateTransform(this._CanvasOffsetX, this._CanvasOffsetY); //Move the coordinate system
            g.ScaleTransform(this._CanvasScale, this._CanvasScale); //Scale the drawing surface
            if (mi.XMatched) {
                //g.DrawLine(pen, this.CanvasToControl(mi.X, true), 0, this.CanvasToControl(mi.X, true), this.Height);
                foreach (STNode n in this._Nodes) {
                    if (n.Left == mi.X || n.Right == mi.X || n.Left + n.Width / 2 == mi.X) {
                        //g.DrawRectangle(pen, n.Left, n.Top, n.Width - 1, n.Height - 1);
                        g.FillRectangle(brush, n.Rectangle);
                    }
                }
            }
            if (mi.YMatched) {
                //g.DrawLine(pen, 0, this.CanvasToControl(mi.Y, false), this.Width, this.CanvasToControl(mi.Y, false));
                foreach (STNode n in this._Nodes) {
                    if (n.Top == mi.Y || n.Bottom == mi.Y || n.Top + n.Height / 2 == mi.Y) {
                        //g.DrawRectangle(pen, n.Left, n.Top, n.Width - 1, n.Height - 1);
                        g.FillRectangle(brush, n.Rectangle);
                    }
                }
            }
            g.ResetTransform();
        }
        /// <summary>
        /// Draw the selected rectangular area
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        /// <param name="rectf">Rectangular area on the control</param>
        protected virtual void OnDrawSelectedRectangle(DrawingTools dt, RectangleF rectf) {
            Graphics g = dt.Graphics;
            SolidBrush brush = dt.SolidBrush;
            dt.Pen.Color = this._SelectedRectangleColor;
            g.DrawRectangle(dt.Pen, rectf.Left, rectf.Y, rectf.Width, rectf.Height);
            brush.Color = Color.FromArgb(this._SelectedRectangleColor.A / 3, this._SelectedRectangleColor);
            g.FillRectangle(brush, this.CanvasToControl(m_rect_select));
        }
        /// <summary>
        /// Draw the Node position prompt information beyond the visual area
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        /// <param name="sz">Prompt box margin</param>
        /// <param name="lstPts">Node position information beyond the visual area</param>
        protected virtual void OnDrawNodeOutLocation(DrawingTools dt, Size sz, List<Point> lstPts) {
            Graphics g = dt.Graphics;
            SolidBrush brush = dt.SolidBrush;
            brush.Color = this._LocationBackColor;
            g.SmoothingMode = SmoothingMode.None;
            if (lstPts.Count == this._Nodes.Count && this._Nodes.Count != 0) { //If the number of excesses is the same as the number of sets, all of them will exceed the drawing circumscribed rectangle
                g.FillRectangle(brush, this.CanvasToControl(this._CanvasValidBounds));
            }
            g.FillRectangle(brush, 0, 0, 4, sz.Height); //Draw the four-sided background
            g.FillRectangle(brush, sz.Width - 4, 0, 4, sz.Height);
            g.FillRectangle(brush, 4, 0, sz.Width - 8, 4);
            g.FillRectangle(brush, 4, sz.Height - 4, sz.Width - 8, 4);
            brush.Color = this._LocationForeColor;
            foreach (var v in lstPts) { // draw points
                var pt = this.CanvasToControl(v);
                if (pt.X < 0) pt.X = 0;
                if (pt.Y < 0) pt.Y = 0;
                if (pt.X > sz.Width) pt.X = sz.Width - 4;
                if (pt.Y > sz.Height) pt.Y = sz.Height - 4;
                g.FillRectangle(brush, pt.X, pt.Y, 4, 4);
            }
        }
        /// <summary>
        /// Drawing prompt information
        /// </summary>
        /// <param name="dt">Drawing tool</param>
        /// <param name="rect">Area to be drawn</param>
        /// <param name="strText">Need to draw text</param>
        /// <param name="foreColor">Information foreground color</param>
        /// <param name="backColor">Information background color</param>
        /// <param name="al">Information Location</param>
        protected virtual void OnDrawAlert(DrawingTools dt, Rectangle rect, string strText, Color foreColor, Color backColor, AlertLocation al) {
            if (m_alpha_alert == 0) return;
            Graphics g = dt.Graphics;
            SolidBrush brush = dt.SolidBrush;

            g.SmoothingMode = SmoothingMode.None;
            brush.Color = backColor;
            dt.Pen.Color = brush.Color;
            g.FillRectangle(brush, rect);
            g.DrawRectangle(dt.Pen, rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);

            brush.Color = foreColor;
            m_sf.Alignment = StringAlignment.Center;
            m_sf.LineAlignment = StringAlignment.Center;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawString(strText, this.Font, brush, rect, m_sf);
        }
        /// <summary>
        /// Get the rectangular area that needs to be drawn for the prompt information
        /// </summary>
        /// <param name="g">Drawing surface</param>
        /// <param name="strText">Need to draw text</param>
        /// <param name="al">Information Location</param>
        /// <returns>Rectangular area</returns>
        protected virtual Rectangle GetAlertRectangle(Graphics g, string strText, AlertLocation al) {
            SizeF szf = g.MeasureString(m_str_alert, this.Font);
            Size sz = new Size((int)Math.Round(szf.Width + 10), (int)Math.Round(szf.Height + 4));
            Rectangle rect = new Rectangle(4, this.Height - sz.Height - 4, sz.Width, sz.Height);

            switch (al) {
                case AlertLocation.Left:
                    rect.Y = (this.Height - sz.Height) >> 1;
                    break;
                case AlertLocation.Top:
                    rect.Y = 4;
                    rect.X = (this.Width - sz.Width) >> 1;
                    break;
                case AlertLocation.Right:
                    rect.X = this.Width - sz.Width - 4;
                    rect.Y = (this.Height - sz.Height) >> 1;
                    break;
                case AlertLocation.Bottom:
                    rect.X = (this.Width - sz.Width) >> 1;
                    break;
                case AlertLocation.Center:
                    rect.X = (this.Width - sz.Width) >> 1;
                    rect.Y = (this.Height - sz.Height) >> 1;
                    break;
                case AlertLocation.LeftTop:
                    rect.X = rect.Y = 4;
                    break;
                case AlertLocation.RightTop:
                    rect.Y = 4;
                    rect.X = this.Width - sz.Width - 4;
                    break;
                case AlertLocation.RightBottom:
                    rect.X = this.Width - sz.Width - 4;
                    break;
            }
            return rect;
        }

        #endregion protected

        #region internal

        internal void BuildLinePath() {
            foreach (var v in m_dic_gp_info) v.Key.Dispose();
            m_dic_gp_info.Clear();
            m_is_buildpath = true;
            this.Invalidate();
        }

        internal void OnDrawAlert(Graphics g) {
            m_rect_alert = this.GetAlertRectangle(g, m_str_alert, m_al);
            Color clr_fore = Color.FromArgb((int)((float)m_alpha_alert / 255 * m_forecolor_alert.A), m_forecolor_alert);
            Color clr_back = Color.FromArgb((int)((float)m_alpha_alert / 255 * m_backcolor_alert.A), m_backcolor_alert);
            this.OnDrawAlert(m_drawing_tools, m_rect_alert, m_str_alert, clr_fore, clr_back, m_al);
        }

        internal void InternalAddSelectedNode(STNode node) {
            node.IsSelected = true;
            lock (m_hs_node_selected) m_hs_node_selected.Add(node);
        }

        internal void InternalRemoveSelectedNode(STNode node) {
            node.IsSelected = false;
            lock (m_hs_node_selected) m_hs_node_selected.Remove(node);
        }

        #endregion internal

        #region private -----------------------------------------------------------------------------------------------------

        private void MoveCanvasThread() {
            bool bRedraw;
            while (true) {
                bRedraw = false;
                if (m_real_canvas_x != this._CanvasOffsetX) {
                    float nx = m_real_canvas_x - this._CanvasOffsetX;
                    float n = Math.Abs(nx) / 10;
                    float nTemp = Math.Abs(nx);
                    if (nTemp <= 4) n = 1;
                    else if (nTemp <= 12) n = 2;
                    else if (nTemp <= 30) n = 3;
                    if (nTemp < 1) this._CanvasOffsetX = m_real_canvas_x;
                    else
                        this._CanvasOffsetX += nx > 0 ? n : -n;
                    bRedraw = true;
                }
                if (m_real_canvas_y != this._CanvasOffsetY) {
                    float ny = m_real_canvas_y - this._CanvasOffsetY;
                    float n = Math.Abs(ny) / 10;
                    float nTemp = Math.Abs(ny);
                    if (nTemp <= 4) n = 1;
                    else if (nTemp <= 12) n = 2;
                    else if (nTemp <= 30) n = 3;
                    if (nTemp < 1)
                        this._CanvasOffsetY = m_real_canvas_y;
                    else
                        this._CanvasOffsetY += ny > 0 ? n : -n;
                    bRedraw = true;
                }
                if (bRedraw) {
                    m_pt_canvas_old.X = this._CanvasOffsetX;
                    m_pt_canvas_old.Y = this._CanvasOffsetY;
                    this.Invalidate();
                    Thread.Sleep(30);
                } else {
                    Thread.Sleep(100);
                }
            }
        }

        private void ShowAlertThread() {
            while (true) {
                int nTime = m_time_alert - (int)DateTime.Now.Subtract(m_dt_alert).TotalMilliseconds;
                if (nTime > 0) {
                    Thread.Sleep(nTime);
                    continue;
                }
                if (nTime < -1000) {
                    if (m_alpha_alert != 0) {
                        m_alpha_alert = 0;
                        this.Invalidate();
                    }
                    Thread.Sleep(100);
                } else {
                    m_alpha_alert = (int)(255 - (-nTime / 1000F) * 255);
                    this.Invalidate(m_rect_alert);
                    Thread.Sleep(50);
                }
            }
        }

        private Image CreateBorderImage(Color clr) {
            Image img = new Bitmap(12, 12);
            using (Graphics g = Graphics.FromImage(img)) {
                g.SmoothingMode = SmoothingMode.HighQuality;
                using (GraphicsPath gp = new GraphicsPath()) {
                    gp.AddEllipse(new Rectangle(0, 0, 11, 11));
                    using (PathGradientBrush b = new PathGradientBrush(gp)) {
                        b.CenterColor = Color.FromArgb(200, clr);
                        b.SurroundColors = new Color[] { Color.FromArgb(10, clr) };
                        g.FillPath(b, gp);
                    }
                }
            }
            return img;
        }

        private ConnectionStatus DisConnectionHover() {
            if (!m_dic_gp_info.ContainsKey(m_gp_hover)) return ConnectionStatus.DisConnected;
            ConnectionInfo ci = m_dic_gp_info[m_gp_hover];
            var ret = ci.Output.DisConnectOption(ci.Input);
            //this.OnOptionDisConnected(new STNodeOptionEventArgs(ci.Output, ci.Input, ret));
            if (ret == ConnectionStatus.DisConnected) {
                m_dic_gp_info.Remove(m_gp_hover);
                m_gp_hover.Dispose();
                m_gp_hover = null;
                this.Invalidate();
            }
            return ret;
        }

        private void StartConnect(STNodeOption op) {
            if (op.IsInput) {
                m_pt_dot_down.X = op.DotLeft;
                m_pt_dot_down.Y = op.DotTop + 5;
            } else {
                m_pt_dot_down.X = op.DotLeft + op.DotSize;
                m_pt_dot_down.Y = op.DotTop + 5;
            }
            m_ca = CanvasAction.ConnectOption;
            m_option_down = op;
        }

        private void MoveNode(Point pt) {
            int nX = (int)((pt.X - m_pt_down_in_control.X) / this._CanvasScale);
            int nY = (int)((pt.Y - m_pt_down_in_control.Y) / this._CanvasScale);
            lock (m_hs_node_selected) {
                foreach (STNode v in m_hs_node_selected) {
                    v.Left = m_dic_pt_selected[v].X + nX;
                    v.Top = m_dic_pt_selected[v].Y + nY;
                }
                if (this._ShowMagnet) {
                    MagnetInfo mi = this.CheckMagnet(this._ActiveNode);
                    if (mi.XMatched) {
                        foreach (STNode v in m_hs_node_selected) v.Left -= mi.OffsetX;
                    }
                    if (mi.YMatched) {
                        foreach (STNode v in m_hs_node_selected) v.Top -= mi.OffsetY;
                    }
                }
            }
            this.Invalidate();
        }

        protected internal virtual void BuildBounds() {
            if (this._Nodes.Count == 0) {
                this._CanvasValidBounds = this.ControlToCanvas(this.DisplayRectangle);
                return;
            }
            int x = int.MaxValue;
            int y = int.MaxValue;
            int r = int.MinValue;
            int b = int.MinValue;
            foreach (STNode n in this._Nodes) {
                if (x > n.Left) x = n.Left;
                if (y > n.Top) y = n.Top;
                if (r < n.Right) r = n.Right;
                if (b < n.Bottom) b = n.Bottom;
            }
            this._CanvasValidBounds.X = x - 60;
            this._CanvasValidBounds.Y = y - 60;
            this._CanvasValidBounds.Width = r - x + 120;
            this._CanvasValidBounds.Height = b - y + 120;
        }

        private bool PointInRectangle(Rectangle rect, float x, float y) {
            if (x < rect.Left) return false;
            if (x > rect.Right) return false;
            if (y < rect.Top) return false;
            if (y > rect.Bottom) return false;
            return true;
        }

        private void BuildMagnetLocation() {
            m_lst_magnet_x.Clear();
            m_lst_magnet_y.Clear();
            foreach (STNode v in this._Nodes) {
                if (v.IsSelected) continue;
                m_lst_magnet_x.Add(v.Left);
                m_lst_magnet_x.Add(v.Left + v.Width / 2);
                m_lst_magnet_x.Add(v.Left + v.Width);
                m_lst_magnet_y.Add(v.Top);
                m_lst_magnet_y.Add(v.Top + v.Height / 2);
                m_lst_magnet_y.Add(v.Top + v.Height);
            }
        }

        private MagnetInfo CheckMagnet(STNode node) {
            m_mi.XMatched = m_mi.YMatched = false;
            m_lst_magnet_mx.Clear();
            m_lst_magnet_my.Clear();
            m_lst_magnet_mx.Add(node.Left + node.Width / 2);
            m_lst_magnet_mx.Add(node.Left);
            m_lst_magnet_mx.Add(node.Left + node.Width);
            m_lst_magnet_my.Add(node.Top + node.Height / 2);
            m_lst_magnet_my.Add(node.Top);
            m_lst_magnet_my.Add(node.Top + node.Height);

            bool bFlag = false;
            foreach (var mx in m_lst_magnet_mx) {
                foreach (var x in m_lst_magnet_x) {
                    if (Math.Abs(mx - x) <= 5) {
                        bFlag = true;
                        m_mi.X = x;
                        m_mi.OffsetX = mx - x;
                        m_mi.XMatched = true;
                        break;
                    }
                }
                if (bFlag) break;
            }
            bFlag = false;
            foreach (var my in m_lst_magnet_my) {
                foreach (var y in m_lst_magnet_y) {
                    if (Math.Abs(my - y) <= 5) {
                        bFlag = true;
                        m_mi.Y = y;
                        m_mi.OffsetY = my - y;
                        m_mi.YMatched = true;
                        break;
                    }
                }
                if (bFlag) break;
            }
            return m_mi;
        }

        private void DrawBezier(Graphics g, Pen p, PointF ptStart, PointF ptEnd, float f) {
            this.DrawBezier(g, p, ptStart.X, ptStart.Y, ptEnd.X, ptEnd.Y, f);
        }

        private void DrawBezier(Graphics g, Pen p, float x1, float y1, float x2, float y2, float f) {
            float n = (Math.Abs(x1 - x2) * f);
            if (this._Curvature != 0 && n < 30) n = 30;
            g.DrawBezier(p,
                x1, y1,
                x1 + n, y1,
                x2 - n, y2,
                x2, y2);
        }

        private GraphicsPath CreateBezierPath(float x1, float y1, float x2, float y2, float f) {
            GraphicsPath gp = new GraphicsPath();
            float n = (Math.Abs(x1 - x2) * f);
            if (this._Curvature != 0 && n < 30) n = 30;
            gp.AddBezier(
                x1, y1,
                x1 + n, y1,
                x2 - n, y2,
                x2, y2
                );
            return gp;
        }

        private void RenderBorder(Graphics g, Rectangle rect, Image img) {
            //fill the four corners
            g.DrawImage(img, new Rectangle(rect.X - 5, rect.Y - 5, 5, 5),
                new Rectangle(0, 0, 5, 5), GraphicsUnit.Pixel);
            g.DrawImage(img, new Rectangle(rect.Right, rect.Y - 5, 5, 5),
                new Rectangle(img.Width - 5, 0, 5, 5), GraphicsUnit.Pixel);
            g.DrawImage(img, new Rectangle(rect.X - 5, rect.Bottom, 5, 5),
                new Rectangle(0, img.Height - 5, 5, 5), GraphicsUnit.Pixel);
            g.DrawImage(img, new Rectangle(rect.Right, rect.Bottom, 5, 5),
                new Rectangle(img.Width - 5, img.Height - 5, 5, 5), GraphicsUnit.Pixel);
            //four sides
            g.DrawImage(img, new Rectangle(rect.X - 5, rect.Y, 5, rect.Height),
                new Rectangle(0, 5, 5, img.Height - 10), GraphicsUnit.Pixel);
            g.DrawImage(img, new Rectangle(rect.X, rect.Y - 5, rect.Width, 5),
                new Rectangle(5, 0, img.Width - 10, 5), GraphicsUnit.Pixel);
            g.DrawImage(img, new Rectangle(rect.Right, rect.Y, 5, rect.Height),
                new Rectangle(img.Width - 5, 5, 5, img.Height - 10), GraphicsUnit.Pixel);
            g.DrawImage(img, new Rectangle(rect.X, rect.Bottom, rect.Width, 5),
                new Rectangle(5, img.Height - 5, img.Width - 10, 5), GraphicsUnit.Pixel);
        }

        #endregion private

        #region public --------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Find by canvas coordinates
        /// </summary>
        /// <param name="pt">Coordinates in canvas</param>
        /// <returns>Data found</returns>
        public NodeFindInfo FindNodeFromPoint(PointF pt) {
            m_find.Node = null; m_find.NodeOption = null; m_find.Mark = null;
            for (int i = this._Nodes.Count - 1; i >= 0; i--) {
                if (!string.IsNullOrEmpty(this._Nodes[i].Mark) && this.PointInRectangle(this._Nodes[i].MarkRectangle, pt.X, pt.Y)) {
                    m_find.Mark = this._Nodes[i].Mark;
                    m_find.MarkLines = this._Nodes[i].MarkLines;
                    return m_find;
                }
                foreach (STNodeOption v in this._Nodes[i].InputOptions) {
                    if (v == STNodeOption.Empty) continue;
                    if (this.PointInRectangle(v.DotRectangle, pt.X, pt.Y)) m_find.NodeOption = v;
                }
                foreach (STNodeOption v in this._Nodes[i].OutputOptions) {
                    if (v == STNodeOption.Empty) continue;
                    if (this.PointInRectangle(v.DotRectangle, pt.X, pt.Y)) m_find.NodeOption = v;
                }
                if (this.PointInRectangle(this._Nodes[i].Rectangle, pt.X, pt.Y)) {
                    m_find.Node = this._Nodes[i];
                }
                if (m_find.NodeOption != null || m_find.Node != null) return m_find;
            }
            return m_find;
        }
        /// <summary>
        /// Get the Node collection that has been selected
        /// </summary>
        /// <returns>Node array</returns>
        public STNode[] GetSelectedNode() {
            return m_hs_node_selected.ToArray();
        }
        /// <summary>
        /// Convert canvas coordinates to control coordinates
        /// </summary>
        /// <param name="number">parameter</param>
        /// <param name="isX">Whether it is the X coordinate</param>
        /// <returns>Converted coordinates</returns>
        public float CanvasToControl(float number, bool isX) {
            return (number * this._CanvasScale) + (isX ? this._CanvasOffsetX : this._CanvasOffsetY);
        }
        /// <summary>
        /// Convert canvas coordinates to control coordinates
        /// </summary>
        /// <param name="pt">coordinates</param>
        /// <returns>Converted coordinates</returns>
        public PointF CanvasToControl(PointF pt) {
            pt.X = (pt.X * this._CanvasScale) + this._CanvasOffsetX;
            pt.Y = (pt.Y * this._CanvasScale) + this._CanvasOffsetY;
            //pt.X += this._CanvasOffsetX;
            //pt.Y += this._CanvasOffsetY;
            return pt;
        }
        /// <summary>
        /// Convert canvas coordinates to control coordinates
        /// </summary>
        /// <param name="pt">coordinates</param>
        /// <returns>Converted coordinates</returns>
        public Point CanvasToControl(Point pt) {
            pt.X = (int)(pt.X * this._CanvasScale + this._CanvasOffsetX);
            pt.Y = (int)(pt.Y * this._CanvasScale + this._CanvasOffsetY);
            //pt.X += (int)this._CanvasOffsetX;
            //pt.Y += (int)this._CanvasOffsetY;
            return pt;
        }
        /// <summary>
        /// Convert canvas coordinates to control coordinates
        /// </summary>
        /// <param name="rect">rectangular area</param>
        /// <returns>Transformed rectangular area</returns>
        public Rectangle CanvasToControl(Rectangle rect) {
            rect.X = (int)((rect.X * this._CanvasScale) + this._CanvasOffsetX);
            rect.Y = (int)((rect.Y * this._CanvasScale) + this._CanvasOffsetY);
            rect.Width = (int)(rect.Width * this._CanvasScale);
            rect.Height = (int)(rect.Height * this._CanvasScale);
            //rect.X += (int)this._CanvasOffsetX;
            //rect.Y += (int)this._CanvasOffsetY;
            return rect;
        }
        /// <summary>
        /// Convert canvas coordinates to control coordinates
        /// </summary>
        /// <param name="rect">rectangular area</param>
        /// <returns>Transformed rectangular area</returns>
        public RectangleF CanvasToControl(RectangleF rect) {
            rect.X = (rect.X * this._CanvasScale) + this._CanvasOffsetX;
            rect.Y = (rect.Y * this._CanvasScale) + this._CanvasOffsetY;
            rect.Width = (rect.Width * this._CanvasScale);
            rect.Height = (rect.Height * this._CanvasScale);
            //rect.X += this._CanvasOffsetX;
            //rect.Y += this._CanvasOffsetY;
            return rect;
        }
        /// <summary>
        /// Convert control coordinates to canvas coordinates
        /// </summary>
        /// <param name="number">parameter</param>
        /// <param name="isX">Whether it is the X coordinate</param>
        /// <returns>Converted coordinates</returns>
        public float ControlToCanvas(float number, bool isX) {
            return (number - (isX ? this._CanvasOffsetX : this._CanvasOffsetY)) / this._CanvasScale;
        }
        /// <summary>
        /// Convert control coordinates to canvas coordinates
        /// </summary>
        /// <param name="pt">coordinates</param>
        /// <returns>Converted coordinates</returns>
        public Point ControlToCanvas(Point pt) {
            pt.X = (int)((pt.X - this._CanvasOffsetX) / this._CanvasScale);
            pt.Y = (int)((pt.Y - this._CanvasOffsetY) / this._CanvasScale);
            return pt;
        }
        /// <summary>
        /// Convert control coordinates to canvas coordinates
        /// </summary>
        /// <param name="pt">coordinates</param>
        /// <returns>Converted coordinates</returns>
        public PointF ControlToCanvas(PointF pt) {
            pt.X = ((pt.X - this._CanvasOffsetX) / this._CanvasScale);
            pt.Y = ((pt.Y - this._CanvasOffsetY) / this._CanvasScale);
            return pt;
        }
        /// <summary>
        /// Convert control coordinates to canvas coordinates
        /// </summary>
        /// <param name="rect">rectangular area</param>
        /// <returns>Transformed area</returns>
        public Rectangle ControlToCanvas(Rectangle rect) {
            rect.X = (int)((rect.X - this._CanvasOffsetX) / this._CanvasScale);
            rect.Y = (int)((rect.Y - this._CanvasOffsetY) / this._CanvasScale);
            rect.Width = (int)(rect.Width / this._CanvasScale);
            rect.Height = (int)(rect.Height / this._CanvasScale);
            return rect;
        }
        /// <summary>
        /// Convert control coordinates to canvas coordinates
        /// </summary>
        /// <param name="rect">rectangular area</param>
        /// <returns>Transformed area</returns>
        public RectangleF ControlToCanvas(RectangleF rect) {
            rect.X = ((rect.X - this._CanvasOffsetX) / this._CanvasScale);
            rect.Y = ((rect.Y - this._CanvasOffsetY) / this._CanvasScale);
            rect.Width = (rect.Width / this._CanvasScale);
            rect.Height = (rect.Height / this._CanvasScale);
            return rect;
        }
        /// <summary>
        /// Move the origin coordinates of the canvas to the specified control coordinates
        /// Cannot move when Node does not exist
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="bAnimation">Whether to start the animation effect during the movement process</param>
        /// <param name="ma">Specify the coordinate parameters to be modified</param>
        public void MoveCanvas(float x, float y, bool bAnimation, CanvasMoveArgs ma) {
            if (this._Nodes.Count == 0) {
                m_real_canvas_x = m_real_canvas_y = 10;
                return;
            }
            int l = (int)((this._CanvasValidBounds.Left + 50) * this._CanvasScale);
            int t = (int)((this._CanvasValidBounds.Top + 50) * this._CanvasScale);
            int r = (int)((this._CanvasValidBounds.Right - 50) * this._CanvasScale);
            int b = (int)((this._CanvasValidBounds.Bottom - 50) * this._CanvasScale);
            if (r + x < 0) x = -r;
            if (this.Width - l < x) x = this.Width - l;
            if (b + y < 0) y = -b;
            if (this.Height - t < y) y = this.Height - t;
            if (bAnimation) {
                if ((ma & CanvasMoveArgs.Left) == CanvasMoveArgs.Left)
                    m_real_canvas_x = x;
                if ((ma & CanvasMoveArgs.Top) == CanvasMoveArgs.Top)
                    m_real_canvas_y = y;
            } else {
                m_real_canvas_x = this._CanvasOffsetX = x;
                m_real_canvas_y = this._CanvasOffsetY = y;
            }
            this.OnCanvasMoved(EventArgs.Empty);
        }
        /// <summary>
        /// Scale the canvas
        /// Cannot zoom when no Node exists
        /// </summary>
        /// <param name="f">Scale ratio</param>
        /// <param name="x">Zoom center X coordinates on the control</param>
        /// <param name="y">The coordinate of the zoom center Y on the control</param>
        public void ScaleCanvas(float f, float x, float y) {
            if (this._Nodes.Count == 0) {
                this._CanvasScale = 1F;
                return;
            }
            if (this._CanvasScale == f) return;
            if (f < 0.5) f = 0.5f; else if (f > 3) f = 3;
            float x_c = this.ControlToCanvas(x, true);
            float y_c = this.ControlToCanvas(y, false);
            this._CanvasScale = f;
            this._CanvasOffsetX = m_real_canvas_x -= this.CanvasToControl(x_c, true) - x;
            this._CanvasOffsetY = m_real_canvas_y -= this.CanvasToControl(y_c, false) - y;
            this.OnCanvasScaled(EventArgs.Empty);
            this.Invalidate();
        }
        /// <summary>
        /// Get the corresponding relationship of the currently connected Option
        /// </summary>
        /// <returns>Connection information collection</returns>
        public ConnectionInfo[] GetConnectionInfo() {
            return m_dic_gp_info.Values.ToArray();
        }
        /// <summary>
        /// Determine if there is a connection path between two Nodes
        /// </summary>
        /// <param name="nodeStart">Start Node</param>
        /// <param name="nodeFind">Target Node</param>
        /// <returns>true if the path exists, otherwise false</returns>
        public static bool CanFindNodePath(STNode nodeStart, STNode nodeFind) {
            HashSet<STNode> hs = new HashSet<STNode>();
            return STNodeEditor.CanFindNodePath(nodeStart, nodeFind, hs);
        }
        private static bool CanFindNodePath(STNode nodeStart, STNode nodeFind, HashSet<STNode> hs) {
            foreach (STNodeOption op_1 in nodeStart.OutputOptions) {
                foreach(STNodeOption op_2 in op_1.ConnectedOption) {
                    if (op_2.Owner == nodeFind) return true;
                    if (hs.Add(op_2.Owner)) {
                        if (STNodeEditor.CanFindNodePath(op_2.Owner, nodeFind)) return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Get the image of the specified rectangular area in the canvas
        /// </summary>
        /// <param name="rect">The rectangular area specified in the canvas</param>
        /// <returns>image</returns>
        public Image GetCanvasImage(Rectangle rect) { return this.GetCanvasImage(rect, 1f); }
        /// <summary>
        /// Get the image of the specified rectangular area in the canvas
        /// </summary>
        /// <param name="rect">The rectangular area specified in the canvas</param>
        /// <param name="fScale">Scale</param>
        /// <returns>image</returns>
        public Image GetCanvasImage(Rectangle rect, float fScale) {
            if (fScale < 0.5) fScale = 0.5f; else if (fScale > 3) fScale = 3;
            Image img = new Bitmap((int)(rect.Width * fScale), (int)(rect.Height * fScale));
            using (Graphics g = Graphics.FromImage(img)) {
                g.Clear(this.BackColor);
                g.ScaleTransform (fScale, fScale);
                m_drawing_tools.Graphics = g;

                if (this._ShowGrid) this.OnDrawGrid(m_drawing_tools, rect.Width, rect.Height);
                g.TranslateTransform(-rect.X, -rect.Y); //Move the coordinate system
                this.OnDrawNode(m_drawing_tools, rect);
                this.OnDrawConnectedLine(m_drawing_tools);

                g.ResetTransform();

                if (this._ShowLocation) this.OnDrawNodeOutLocation(m_drawing_tools, img.Size, m_lst_node_out);
            }
            return img;
        }
        /// <summary>
        /// Save the class content in the canvas to the file
        /// </summary>
        /// <param name="strFileName">File path</param>
        public void SaveCanvas(string strFileName) {
            using (FileStream fs = new FileStream(strFileName, FileMode.Create, FileAccess.Write)) {
                this.SaveCanvas(fs);
            }
        }
        /// <summary>
        /// Save the class content in the canvas to the data stream
        /// </summary>
        /// <param name="s">Data stream object</param>
        public void SaveCanvas(Stream s) {
            Dictionary<STNodeOption, long> dic = new Dictionary<STNodeOption, long>();
            s.Write(new byte[] { (byte)'S', (byte)'T', (byte)'N', (byte)'D' }, 0, 4); //file head
            s.WriteByte(1);                                                           //ver
            using (GZipStream gs = new GZipStream(s, CompressionMode.Compress)) {
                gs.Write(BitConverter.GetBytes(this._CanvasOffsetX), 0, 4);
                gs.Write(BitConverter.GetBytes(this._CanvasOffsetY), 0, 4);
                gs.Write(BitConverter.GetBytes(this._CanvasScale), 0, 4);
                gs.Write(BitConverter.GetBytes(this._Nodes.Count), 0, 4);
                foreach (STNode node in this._Nodes) {
                    try {
                        byte[] byNode = node.GetSaveData();
                        gs.Write(BitConverter.GetBytes(byNode.Length), 0, 4);
                        gs.Write(byNode, 0, byNode.Length);
                        foreach (STNodeOption op in node.InputOptions) if (!dic.ContainsKey(op)) dic.Add(op, dic.Count);
                        foreach (STNodeOption op in node.OutputOptions) if (!dic.ContainsKey(op)) dic.Add(op, dic.Count);
                    } catch (Exception ex) {
                        throw new Exception("Error getting node data-" + node.Title, ex);
                    }
                }
                gs.Write(BitConverter.GetBytes(m_dic_gp_info.Count), 0, 4);
                foreach (var v in m_dic_gp_info.Values)
                    gs.Write(BitConverter.GetBytes(((dic[v.Output] << 32) | dic[v.Input])), 0, 8);
            }
        }
        /// <summary>
        /// Get the binary data of the content in the canvas
        /// </summary>
        /// <returns>Binary data</returns>
        public byte[] GetCanvasData() {
            using (MemoryStream ms = new MemoryStream()) {
                this.SaveCanvas(ms);
                return ms.ToArray();
            }
        }
        /// <summary>
        /// Load the assembly
        /// </summary>
        /// <param name="strFiles">Assembly Collection</param>
        /// <returns>Number of files of STNode type</returns>
        public int LoadAssembly(string[] strFiles) {
            int nCount = 0;
            foreach (var v in strFiles) {
                try {
                    if (this.LoadAssembly(v)) nCount++;
                } catch { }
            }
            return nCount;
        }
        /// <summary>
        /// Load the assembly
        /// </summary>
        /// <param name="strFile">Specify the file to be loaded</param>
        /// <returns>Whether the load is successful</returns>
        public bool LoadAssembly(string strFile) {
            bool bFound = false;
            Assembly asm = Assembly.LoadFrom(strFile);
            if (asm == null) return false;
            foreach (var t in asm.GetTypes()) {
                if (t.IsAbstract) continue;
                if (t == m_type_node || t.IsSubclassOf(m_type_node)) {
                    if (m_dic_type.ContainsKey(t.GUID.ToString())) continue;
                    m_dic_type.Add(t.GUID.ToString(), t);
                    bFound = true;
                }
            }
            return bFound;
        }
        /// <summary>
        /// Get the Node type loaded in the current editor
        /// </summary>
        /// <returns>Type Collection</returns>
        public Type[] GetTypes() {
            return m_dic_type.Values.ToArray();
        }
        /// <summary>
        /// Load data from file
        /// Note: This method does not clear the data in the canvas but superimposes the data
        /// </summary>
        /// <param name="strFileName">File path</param>
        public void LoadCanvas(string strFileName) {
            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(strFileName)))
                this.LoadCanvas(ms);
        }
        /// <summary>
        /// Load data from binary
        /// Note: This method does not clear the data in the canvas but superimposes the data
        /// </summary>
        /// <param name="byData">Binary data</param>
        public void LoadCanvas(byte[] byData) {
            using (MemoryStream ms = new MemoryStream(byData))
                this.LoadCanvas(ms);
        }
        /// <summary>
        /// Load data from the data stream
        /// Note: This method does not clear the data in the canvas but superimposes the data
        /// </summary>
        /// <param name="s">Data stream object</param>
        public void LoadCanvas(Stream s) {
            int nLen = 0;
            byte[] byLen = new byte[4];
            s.Read(byLen, 0, 4);
            if (BitConverter.ToInt32(byLen, 0) != BitConverter.ToInt32(new byte[] { (byte)'S', (byte)'T', (byte)'N', (byte)'D' }, 0))
                throw new InvalidDataException("Unrecognized file type");
            if (s.ReadByte() != 1) throw new InvalidDataException("Unrecognized file version number");
            using (GZipStream gs = new GZipStream(s, CompressionMode.Decompress)) {
                gs.Read(byLen, 0, 4);
                float x = BitConverter.ToSingle(byLen, 0);
                gs.Read(byLen, 0, 4);
                float y = BitConverter.ToSingle(byLen, 0);
                gs.Read(byLen, 0, 4);
                float scale = BitConverter.ToSingle(byLen, 0);
                gs.Read(byLen, 0, 4);
                int nCount = BitConverter.ToInt32(byLen, 0);
                Dictionary<long, STNodeOption> dic = new Dictionary<long, STNodeOption>();
                HashSet<STNodeOption> hs = new HashSet<STNodeOption>();
                byte[] byData = null;
                for (int i = 0; i < nCount; i++) {
                    gs.Read(byLen, 0, byLen.Length);
                    nLen = BitConverter.ToInt32(byLen, 0);
                    byData = new byte[nLen];
                    gs.Read(byData, 0, byData.Length);
                    STNode node = null;
                    try { node = this.GetNodeFromData(byData); } catch (Exception ex) {
                        throw new Exception("An error occurred while loading the node, the data may be corrupted\r\n" + ex.Message, ex);
                    }
                    try { this._Nodes.Add(node); } catch (Exception ex) {
                        throw new Exception("Error loading node-" + node.Title, ex);
                    }
                    foreach (STNodeOption op in node.InputOptions) if (hs.Add(op)) dic.Add(dic.Count, op);
                    foreach (STNodeOption op in node.OutputOptions) if (hs.Add(op)) dic.Add(dic.Count, op);
                }
                gs.Read(byLen, 0, 4);
                nCount = BitConverter.ToInt32(byLen, 0);
                byData = new byte[8];
                for (int i = 0; i < nCount; i++) {
                    gs.Read(byData, 0, byData.Length);
                    long id = BitConverter.ToInt64(byData, 0);
                    long op_out = id >> 32;
                    long op_in = (int)id;
                    dic[op_out].ConnectOption(dic[op_in]);
                }
                this.ScaleCanvas(scale, 0, 0);
                this.MoveCanvas(x, y, false, CanvasMoveArgs.All);
            }
            this.BuildBounds();
            foreach (STNode node in this._Nodes) node.OnEditorLoadCompleted();
        }

        private STNode GetNodeFromData(byte[] byData) {
            int nIndex = 0;
            string strModel = Encoding.UTF8.GetString(byData, nIndex + 1, byData[nIndex]);
            nIndex += byData[nIndex] + 1;
            string strGUID = Encoding.UTF8.GetString(byData, nIndex + 1, byData[nIndex]);
            nIndex += byData[nIndex] + 1;

            int nLen = 0;

            Dictionary<string, byte[]> dic = new Dictionary<string, byte[]>();
            while (nIndex < byData.Length) {
                nLen = BitConverter.ToInt32(byData, nIndex);
                nIndex += 4;
                string strKey = Encoding.UTF8.GetString(byData, nIndex, nLen);
                nIndex += nLen;
                nLen = BitConverter.ToInt32(byData, nIndex);
                nIndex += 4;
                byte[] byValue = new byte[nLen];
                Array.Copy(byData, nIndex, byValue, 0, nLen);
                nIndex += nLen;
                dic.Add(strKey, byValue);
            }
            if (!m_dic_type.ContainsKey(strGUID)) throw new TypeLoadException("Cannot find the assembly where the type {" + strModel.Split('|')[1] + "} is located to ensure that the assembly {" + strModel.Split('|')[0] + "} has been loaded correctly by the editor. The assembly can be loaded by calling LoadAssembly()");
            Type t = m_dic_type[strGUID]; ;
            STNode node = (STNode)Activator.CreateInstance(t);
            node.OnLoadNode(dic);
            return node;
        }
        /// <summary>
        /// Display the prompt information in the canvas
        /// </summary>
        /// <param name="strText">Information to display</param>
        /// <param name="foreColor">Information foreground color</param>
        /// <param name="backColor">Information background color</param>
        public void ShowAlert(string strText, Color foreColor, Color backColor) {
            this.ShowAlert(strText, foreColor, backColor, 1000, AlertLocation.RightBottom, true);
        }
        /// <summary>
        /// Display the prompt information in the canvas
        /// </summary>
        /// <param name="strText">Information to display</param>
        /// <param name="foreColor">Information foreground color</param>
        /// <param name="backColor">Information background color</param>
        /// <param name="al">The location where the information is to be displayed</param>
        public void ShowAlert(string strText, Color foreColor, Color backColor, AlertLocation al) {
            this.ShowAlert(strText, foreColor, backColor, 1000, al, true);
        }
        /// <summary>
        /// Display the prompt information in the canvas
        /// </summary>
        /// <param name="strText">Information to display</param>
        /// <param name="foreColor">Information foreground color</param>
        /// <param name="backColor">Information background color</param>
        /// <param name="nTime">message duration</param>
        /// <param name="al">The location where the information is to be displayed</param>
        /// <param name="bRedraw">Whether to redraw immediately</param>
        public void ShowAlert(string strText, Color foreColor, Color backColor, int nTime, AlertLocation al, bool bRedraw) {
            m_str_alert = strText;
            m_forecolor_alert = foreColor;
            m_backcolor_alert = backColor;
            m_time_alert = nTime;
            m_dt_alert = DateTime.Now;
            m_alpha_alert = 255;
            m_al = al;
            if (bRedraw) this.Invalidate();
        }
        /// <summary>
        /// Set the active node in the canvas
        /// </summary>
        /// <param name="node">The node that needs to be set as active</param>
        /// <returns>The active node before setting</returns>
        public STNode SetActiveNode(STNode node) {
            if (node != null && !this._Nodes.Contains(node)) return this._ActiveNode;
            STNode ret = this._ActiveNode;
            if (this._ActiveNode != node) { //Reset active selection node
                if (node != null) {
                    this._Nodes.MoveToEnd(node);
                    node.IsActive = true;
                    node.SetSelected(true, false);
                    node.OnGotFocus(EventArgs.Empty);
                }
                if (this._ActiveNode != null) {
                    this._ActiveNode.IsActive /*= this._ActiveNode.IsSelected*/ = false;
                    this._ActiveNode.OnLostFocus(EventArgs.Empty);
                }
                this._ActiveNode = node;
                this.Invalidate();
                this.OnActiveChanged(EventArgs.Empty);
                //this.OnSelectedChanged(EventArgs.Empty);
            }
            return ret;
        }
        /// <summary>
        /// Add a selected node to the canvas
        /// </summary>
        /// <param name="node">The node that needs to be selected</param>
        /// <returns>Whether the addition is successful</returns>
        public bool AddSelectedNode(STNode node) {
            if (!this._Nodes.Contains(node)) return false;
            bool b = !node.IsSelected;
            node.IsSelected = true;
            lock (m_hs_node_selected) return m_hs_node_selected.Add(node) || b;
        }
        /// <summary>
        /// Remove a selected node from the canvas
        /// </summary>
        /// <param name="node">Node to be removed</param>
        /// <returns>Is the removal successful or not</returns>
        public bool RemoveSelectedNode(STNode node) {
            if (!this._Nodes.Contains(node)) return false;
            bool b = node.IsSelected;
            node.IsSelected = false;
            lock (m_hs_node_selected) return m_hs_node_selected.Remove(node) || b;
        }
        /// <summary>
        /// Adds a default data type color to the editor
        /// </summary>
        /// <param name="t">Data Type</param>
        /// <param name="clr">Corresponding color</param>
        /// <returns>The set color</returns>
        public Color SetTypeColor(Type t, Color clr) {
            return this.SetTypeColor(t, clr, false);
        }
        /// <summary>
        /// Adds a default data type color to the editor
        /// </summary>
        /// <param name="t">Data Type</param>
        /// <param name="clr">Corresponding color</param>
        /// <param name="bReplace">Replace the color if it already exists</param>
        /// <returns>The set color</returns>
        public Color SetTypeColor(Type t, Color clr, bool bReplace) {
            if (this._TypeColor.ContainsKey(t)) {
                if (bReplace) this._TypeColor[t] = clr;
            } else {
                this._TypeColor.Add(t, clr);
            }
            return this._TypeColor[t];
        }

        #endregion public
    }
}