using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace ST.Library.UI.NodeEditor
{
    public class STNodeOption
    {
        #region Properties

        public static readonly STNodeOption Empty = new STNodeOption();

        private STNode _Owner;
        /// <summary>
        /// Get the Node to which the current Option belongs
        /// </summary>
        public STNode Owner {
            get { return _Owner; }
            internal set {
                if (value == _Owner) return;
                if (_Owner != null) this.DisConnectionAll(); //Disconnect all current connections when the owner changes
                _Owner = value;
            }
        }

        private bool _IsSingle;
        /// <summary>
        /// Get whether the current Option can only be connected once
        /// </summary>
        public bool IsSingle {
            get { return _IsSingle; }
        }

        private bool _IsInput;
        /// <summary>
        /// Get whether the current Option is an input option
        /// </summary>
        public bool IsInput {
            get { return _IsInput; }
            internal set { _IsInput = value; }
        }

        private Color _TextColor = Color.White;
        /// <summary>
        /// Gets or sets the current Option text color
        /// </summary>
        public Color TextColor {
            get { return _TextColor; }
            internal set {
                if (value == _TextColor) return;
                _TextColor = value;
                this.Invalidate();
            }
        }

        private Color _DotColor = Color.Transparent;
        /// <summary>
        /// Gets or sets the color of the current Option connection point
        /// </summary>
        public Color DotColor {
            get { return _DotColor; }
            internal set {
                if (value == _DotColor) return;
                _DotColor = value;
                this.Invalidate();
            }
        }

        private string _Text;
        /// <summary>
        /// Gets or sets the current Option display text
        /// This property cannot be modified when AutoSize is set
        /// </summary>
        public string Text {
            get { return _Text; }
            internal set {
                if (value == _Text) return;
                _Text = value;
                if (this._Owner == null) return;
                this._Owner.BuildSize(true, true, true);
            }
        }

        private int _DotLeft;
        /// <summary>
        /// Get the left coordinate of the current Option connection point
        /// </summary>
        public int DotLeft {
            get { return _DotLeft; }
            internal set { _DotLeft = value; }
        }
        private int _DotTop;
        /// <summary>
        /// Get the upper coordinate of the current Option connection point
        /// </summary>
        public int DotTop {
            get { return _DotTop; }
            internal set { _DotTop = value; }
        }

        private int _DotSize;
        /// <summary>
        /// Get the width of the current Option connection point
        /// </summary>
        public int DotSize {
            get { return _DotSize; }
            protected set { _DotSize = value; }
        }

        private Rectangle _TextRectangle;
        /// <summary>
        /// Get the current Option text area
        /// </summary>
        public Rectangle TextRectangle {
            get { return _TextRectangle; }
            internal set { _TextRectangle = value; }
        }

        private object _Data;
        /// <summary>
        /// Get or set the data contained in the current Option
        /// </summary>
        public object Data {
            get { return _Data; }
            set {
                if (value != null) {
                    if (this._DataType == null) return;
                    var t = value.GetType();
                    if (t != this._DataType && !t.IsSubclassOf(this._DataType)) {
                        throw new ArgumentException("Invalid data type The data type must be the specified data type or its subclass");
                    }
                }
                _Data = value;
            }
        }

        private Type _DataType;
        /// <summary>
        /// Get the current Option data type
        /// </summary>
        public Type DataType {
            get { return _DataType; }
            internal set { _DataType = value; }
        }

        //private Rectangle _DotRectangle;
        /// <summary>
        /// Get the area of ​​the current Option connection point
        /// </summary>
        public Rectangle DotRectangle {
            get {
                return new Rectangle(this._DotLeft, this._DotTop, this._DotSize, this._DotSize);
            }
        }
        /// <summary>
        /// Get the current number of Option connected
        /// </summary>
        public int ConnectionCount {
            get { return m_hs_connected.Count; }
        }
        /// <summary>
        /// Get the Option collection that the current Option is connected to
        /// </summary>
        internal HashSet<STNodeOption> ConnectedOption {
            get { return m_hs_connected; }
        }

        #endregion Properties
        /// <summary>
        /// Save the points that have been connected
        /// </summary>
        protected HashSet<STNodeOption> m_hs_connected;

        #region Constructor

        private STNodeOption() { }

        /// <summary>
        /// Constructs an Option
        /// </summary>
        /// <param name="strText">Display text</param>
        /// <param name="dataType">Data Type</param>
        /// <param name="bSingle">Whether it is a single connection</param>
        public STNodeOption(string strText, Type dataType, bool bSingle) {
            if (dataType == null) throw new ArgumentNullException("The specified data type cannot be null");
            this._DotSize = 10;
            m_hs_connected = new HashSet<STNodeOption>();
            this._DataType = dataType;
            this._Text = strText;
            this._IsSingle = bSingle;
        }

        #endregion Builder

        #region Event

        /// <summary>
        /// Occurs when connected
        /// </summary>
        public event STNodeOptionEventHandler Connected;
        /// <summary>
        /// Occurs when a connection starts happening
        /// </summary>
        public event STNodeOptionEventHandler Connecting;
        /// <summary>
        /// Occurs when the connection is disconnected
        /// </summary>
        public event STNodeOptionEventHandler DisConnected;
        /// <summary>
        /// Occurs when the connection starts to drop
        /// </summary>
        public event STNodeOptionEventHandler DisConnecting;
        /// <summary>
        /// Occurs when data is passed
        /// </summary>
        public event STNodeOptionEventHandler DataTransfer;

        #endregion Event

        #region protected
        /// <summary>
        /// Redraw the entire control
        /// </summary>
        protected void Invalidate() {
            if (this._Owner == null) return;
            this._Owner.Invalidate();
        }
        /*
         * At first I thought that only input type options should have events because input is passive and output is active
         * But later found that events are used for output nodes in STNodeHub, for example
         * Just in case, it is not very problematic to comment the code here. The output option does not register the event, and the same effect
         */
        protected internal virtual void OnConnected(STNodeOptionEventArgs e) {
            if (this.Connected != null/* && this._IsInput*/) this.Connected(this, e);
        }
        protected internal virtual void OnConnecting(STNodeOptionEventArgs e) {
            if (this.Connecting != null) this.Connecting(this, e);
        }
        protected internal virtual void OnDisConnected(STNodeOptionEventArgs e) {
            if (this.DisConnected != null/* && this._IsInput*/) this.DisConnected(this, e);
        }
        protected internal virtual void OnDisConnecting(STNodeOptionEventArgs e) {
            if (this.DisConnecting != null) this.DisConnecting(this, e);
        }
        protected internal virtual void OnDataTransfer(STNodeOptionEventArgs e) {
            if (this.DataTransfer != null/* && this._IsInput*/) this.DataTransfer(this, e);
        }
        protected void STNodeEidtorConnected(STNodeEditorOptionEventArgs e) {
            if (this._Owner == null) return;
            if (this._Owner.Owner == null) return;
            this._Owner.Owner.OnOptionConnected(e);
        }
        protected void STNodeEidtorDisConnected(STNodeEditorOptionEventArgs e) {
            if (this._Owner == null) return;
            if (this._Owner.Owner == null) return;
            this._Owner.Owner.OnOptionDisConnected(e);
        }
        /// <summary>
        /// The current Option starts to connect to the target Option
        /// </summary>
        /// <param name="op">Option to connect</param>
        /// <returns>Are you allowed to continue the operation?</returns>
        protected virtual bool ConnectingOption(STNodeOption op) {
            if (this._Owner == null) return false;
            if (this._Owner.Owner == null) return false;
            STNodeEditorOptionEventArgs e = new STNodeEditorOptionEventArgs(op, this, ConnectionStatus.Connecting);
            this._Owner.Owner.OnOptionConnecting(e);
            this.OnConnecting(new STNodeOptionEventArgs(true, op, ConnectionStatus.Connecting));
            op.OnConnecting(new STNodeOptionEventArgs(false, this, ConnectionStatus.Connecting));
            return e.Continue;
        }
        /// <summary>
        /// The current Option starts to disconnect the target Option
        /// </summary>
        /// <param name="op">Option to be disconnected</param>
        /// <returns>Are you allowed to continue the operation?</returns>
        protected virtual bool DisConnectingOption(STNodeOption op) {
            if (this._Owner == null) return false;
            if (this._Owner.Owner == null) return false;
            STNodeEditorOptionEventArgs e = new STNodeEditorOptionEventArgs(op, this, ConnectionStatus.DisConnecting);
            this._Owner.Owner.OnOptionDisConnecting(e);
            this.OnDisConnecting(new STNodeOptionEventArgs(true, op, ConnectionStatus.DisConnecting));
            op.OnDisConnecting(new STNodeOptionEventArgs(false, this, ConnectionStatus.DisConnecting));
            return e.Continue;
        }

        #endregion protected

        #region public
        /// <summary>
        /// The current Option is connected to the target Option
        /// </summary>
        /// <param name="op">Option to connect</param>
        /// <returns>Connection result</returns>
        public virtual ConnectionStatus ConnectOption(STNodeOption op) {
            if (!this.ConnectingOption(op)) {
                this.STNodeEidtorConnected(new STNodeEditorOptionEventArgs(op, this, ConnectionStatus.Reject));
                return ConnectionStatus.Reject;
            }

            var v = this.CanConnect(op);
            if (v != ConnectionStatus.Connected) {
                this.STNodeEidtorConnected(new STNodeEditorOptionEventArgs(op, this, v));
                return v;
            }
            v = op.CanConnect(this);
            if (v != ConnectionStatus.Connected) {
                this.STNodeEidtorConnected(new STNodeEditorOptionEventArgs(op, this, v));
                return v;
            }
            op.AddConnection(this, false);
            this.AddConnection(op, true);
            this.ControlBuildLinePath();

            this.STNodeEidtorConnected(new STNodeEditorOptionEventArgs(op, this, v));
            return v;
        }
        /// <summary>
        /// Check whether the current Option can connect to the target Option
        /// </summary>
        /// <param name="op">Option to connect</param>
        /// <returns>Test results</returns>
        public virtual ConnectionStatus CanConnect(STNodeOption op) {
            if (this == STNodeOption.Empty || op == STNodeOption.Empty) return ConnectionStatus.EmptyOption;
            if (this._IsInput == op.IsInput) return ConnectionStatus.SameInputOrOutput;
            if (op.Owner == null || this._Owner == null) return ConnectionStatus.NoOwner;
            if (op.Owner == this._Owner) return ConnectionStatus.SameOwner;
            if (this._Owner.LockOption || op._Owner.LockOption) return ConnectionStatus.Locked;
            if (this._IsSingle && m_hs_connected.Count == 1) return ConnectionStatus.SingleOption;
            if (op.IsInput && STNodeEditor.CanFindNodePath(op.Owner, this._Owner)) return ConnectionStatus.Loop;
            if (m_hs_connected.Contains(op)) return ConnectionStatus.Exists;
            if (this._IsInput && op._DataType != this._DataType && !op._DataType.IsSubclassOf(this._DataType)) return ConnectionStatus.ErrorType;
            return ConnectionStatus.Connected;
        }
        /// <summary>
        /// The current Option disconnects the target Option
        /// </summary>
        /// <param name="op">Option to be disconnected</param>
        /// <returns></returns>
        public virtual ConnectionStatus DisConnectOption(STNodeOption op) {
            if (!this.DisConnectingOption(op)) {
                this.STNodeEidtorDisConnected(new STNodeEditorOptionEventArgs(op, this, ConnectionStatus.Reject));
                return ConnectionStatus.Reject;
            }

            if (op.Owner == null) return ConnectionStatus.NoOwner;
            if (this._Owner == null) return ConnectionStatus.NoOwner;
            if (op.Owner.LockOption && this._Owner.LockOption) {
                this.STNodeEidtorDisConnected(new STNodeEditorOptionEventArgs(op, this, ConnectionStatus.Locked));
                return ConnectionStatus.Locked;
            }
            op.RemoveConnection(this, false);
            this.RemoveConnection(op, true);
            this.ControlBuildLinePath();

            this.STNodeEidtorDisConnected(new STNodeEditorOptionEventArgs(op, this, ConnectionStatus.DisConnected));
            return ConnectionStatus.DisConnected;
        }
        /// <summary>
        /// Disconnect all connections of the current Option
        /// </summary>
        public void DisConnectionAll() {
            if (this._DataType == null) return;
            var arr = m_hs_connected.ToArray();
            foreach (var v in arr) {
                this.DisConnectOption(v);
            }
        }
        /// <summary>
        /// Get the Option collection that the current Option is connected to
        /// </summary>
        /// <returns>If it is null, it means that there is no owner, otherwise it returns the collection</returns>
        public List<STNodeOption> GetConnectedOption() {
            if (this._DataType == null) return null;
            if (!this._IsInput)
                return m_hs_connected.ToList();
            List<STNodeOption> lst = new List<STNodeOption>();
            if (this._Owner == null) return null;
            if (this._Owner.Owner == null) return null;
            foreach (var v in this._Owner.Owner.GetConnectionInfo()) {
                if (v.Output == this) lst.Add(v.Input);
            }
            return lst;
        }
        /// <summary>
        /// Post data to all Option connected to the current Option
        /// </summary>
        public void TransferData() {
            if (this._DataType == null) return;
            foreach (var v in m_hs_connected) {
                v.OnDataTransfer(new STNodeOptionEventArgs(true, this, ConnectionStatus.Connected));
            }
        }
        /// <summary>
        /// Post data to all Option connected to the current Option
        /// </summary>
        /// <param name="data">Data to be delivered</param>
        public void TransferData(object data) {
            if (this._DataType == null) return;
            this.Data = data; //not this._Data
            foreach (var v in m_hs_connected) {
                v.OnDataTransfer(new STNodeOptionEventArgs(true, this, ConnectionStatus.Connected));
            }
        }
        /// <summary>
        /// Post data to all Option connected to the current Option
        /// </summary>
        /// <param name="data">Data to be delivered</param>
        /// <param name="bDisposeOld">Whether to release old data</param>
        public void TransferData(object data, bool bDisposeOld) {
            if (bDisposeOld && this._Data != null) {
                if (this._Data is IDisposable) ((IDisposable)this._Data).Dispose();
                this._Data = null;
            }
            this.TransferData(data);
        }

        #endregion public

        #region internal

        private bool AddConnection(STNodeOption op, bool bSponsor) {
            if (this._DataType == null) return false;
            bool b = m_hs_connected.Add(op);
            this.OnConnected(new STNodeOptionEventArgs(bSponsor, op, ConnectionStatus.Connected));
            if (this._IsInput) this.OnDataTransfer(new STNodeOptionEventArgs(bSponsor, op, ConnectionStatus.Connected));
            return b;
        }

        private bool RemoveConnection(STNodeOption op, bool bSponsor) {
            if (this._DataType == null) return false;
            bool b = false;
            if (m_hs_connected.Contains(op)) {
                b = m_hs_connected.Remove(op);
                if (this._IsInput) this.OnDataTransfer(new STNodeOptionEventArgs(bSponsor, op, ConnectionStatus.DisConnected));
                this.OnDisConnected(new STNodeOptionEventArgs(bSponsor, op, ConnectionStatus.Connected));
            }
            return b;
        }

        #endregion internal

        #region private

        private void ControlBuildLinePath() {
            if (this.Owner == null) return;
            if (this.Owner.Owner == null) return;
            this.Owner.Owner.BuildLinePath();
        }

        #endregion
    }
}