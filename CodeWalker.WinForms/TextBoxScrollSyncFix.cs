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
    public partial class TextBoxScrollSyncFix : TextBoxFix
    {
        public TextBoxScrollSyncFix()
        {
            InitializeComponent();
        }

        public const int WM_VSCROLL = 0x115;

        List<TextBoxScrollSyncFix> peers = new List<TextBoxScrollSyncFix>();

        public void AddPeer(TextBoxScrollSyncFix peer)
        {
            peers.Add(peer);
        }

        private void DirectWndProc(ref Message m)
        {
            base.WndProc(ref m);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_VSCROLL)
            {
                foreach (var peer in this.peers)
                {
                    var peerMessage = Message.Create(peer.Handle, m.Msg, m.WParam, m.LParam);
                    peer.DirectWndProc(ref peerMessage);
                }
            }

            base.WndProc(ref m);
        }
    }
}
