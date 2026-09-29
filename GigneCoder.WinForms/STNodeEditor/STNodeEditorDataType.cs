using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace ST.Library.UI.NodeEditor
{
    public enum ConnectionStatus
    {
        /// <summary>
        /// No owner exists
        /// </summary>
        [Description("No owner exists")]
        NoOwner,
        /// <summary>
        /// same owner
        /// </summary>
        [Description("same owner")]
        SameOwner,
        /// <summary>
        /// Both are input or output options
        /// </summary>
        [Description("both input or output options")]
        SameInputOrOutput,
        /// <summary>
        /// Different data types
        /// </summary>
        [Description("Different data types")]
        ErrorType,
        /// <summary>
        /// Single connection node
        /// </summary>
        [Description("Single connection node")]
        SingleOption,
        /// <summary>
        /// A circular path appears
        /// </summary>
        [Description("A circular path appears")]
        Loop,
        /// <summary>
        /// Existing connection
        /// </summary>
        [Description("existing connection")]
        Exists,
        /// <summary>
        /// blank options
        /// </summary>
        [Description("Blank option")]
        EmptyOption,
        /// <summary>
        /// already connected
        /// </summary>
        [Description("Connected")]
        Connected,
        /// <summary>
        /// The connection is disconnected
        /// </summary>
        [Description("The connection was disconnected")]
        DisConnected,
        /// <summary>
        /// Node is locked
        /// </summary>
        [Description("Node is locked")]
        Locked,
        /// <summary>
        /// Operation rejected
        /// </summary>
        [Description("Operation denied")]
        Reject,
        /// <summary>
        /// is being connected
        /// </summary>
        [Description("being connected")]
        Connecting,
        /// <summary>
        /// Disconnecting
        /// </summary>
        [Description("Disconnecting")]
        DisConnecting
    }

    public enum AlertLocation
    {
        Left,
        Top,
        Right,
        Bottom,
        Center,
        LeftTop,
        RightTop,
        RightBottom,
        LeftBottom,
    }

    public struct DrawingTools
    {
        public Graphics Graphics;
        public Pen Pen;
        public SolidBrush SolidBrush;
    }

    public enum CanvasMoveArgs //View the parameters needed when moving the canvas ->MoveCanvas()
    {
        Left = 1, //indicates that only the X coordinate is moved
        Top = 2, //Indicates that only the Y coordinate is moved
        All = 4 //Indicates that XY move at the same time
    }

    public struct NodeFindInfo
    {
        public STNode Node;
        public STNodeOption NodeOption;
        public string Mark;
        public string[] MarkLines;
    }

    public struct ConnectionInfo
    {
        public STNodeOption Input;
        public STNodeOption Output;
    }

    public delegate void STNodeOptionEventHandler(object sender, STNodeOptionEventArgs e);

    public class STNodeOptionEventArgs : EventArgs
    {
        private STNodeOption _TargetOption;
        /// <summary>
        /// The corresponding Option that triggers this event
        /// </summary>
        public STNodeOption TargetOption {
            get { return _TargetOption; }
        }

        private ConnectionStatus _Status;
        /// <summary>
        /// Connection status between Option
        /// </summary>
        public ConnectionStatus Status {
            get { return _Status; }
            internal set { _Status = value; }
        }

        private bool _IsSponsor;
        /// <summary>
        /// Whether it is the initiator of this behavior
        /// </summary>
        public bool IsSponsor {
            get { return _IsSponsor; }
        }

        public STNodeOptionEventArgs(bool isSponsor, STNodeOption opTarget, ConnectionStatus cr) {
            this._IsSponsor = isSponsor;
            this._TargetOption = opTarget;
            this._Status = cr;
        }
    }

    public delegate void STNodeEditorEventHandler(object sender, STNodeEditorEventArgs e);
    public delegate void STNodeEditorOptionEventHandler(object sender, STNodeEditorOptionEventArgs e);


    public class STNodeEditorEventArgs : EventArgs
    {
        private STNode _Node;

        public STNode Node {
            get { return _Node; }
        }

        public STNodeEditorEventArgs(STNode node) {
            this._Node = node;
        }
    }

    public class STNodeEditorOptionEventArgs : STNodeOptionEventArgs
    {

        private STNodeOption _CurrentOption;
        /// <summary>
        /// Option to actively trigger events
        /// </summary>
        public STNodeOption CurrentOption {
            get { return _CurrentOption; }
        }

        private bool _Continue = true;
        /// <summary>
        /// Whether to continue the downward operation for Begin(Connecting/DisConnecting) whether to continue the backward operation
        /// </summary>
        public bool Continue {
            get { return _Continue; }
            set { _Continue = value; }
        }

        public STNodeEditorOptionEventArgs(STNodeOption opTarget, STNodeOption opCurrent, ConnectionStatus cr)
            : base(false, opTarget, cr) {
            this._CurrentOption = opCurrent;
        }
    }
}