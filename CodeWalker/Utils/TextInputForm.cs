using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Utils
{
    public partial class TextInputForm : Form
    {
        public TextInputForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }


        private string _MainText = string.Empty;
        public string MainText
        {
            get
            {
                return _MainText;
            }
            set
            {
                _MainText = value;
                MainTextBox.Text = _MainText;
            }
        }

        public string PromptText
        {
            get
            {
                return PromptLabel.Text;
            }
            set
            {
                PromptLabel.Text = value;
            }
        }

        private string _TitleText = string.Empty;
        public string TitleText
        {
            get
            {
                return _TitleText;
            }
            set
            {
                _TitleText = value;
                var str = "Text Input - CodeWalker by dexyfex";
                if (!string.IsNullOrEmpty(_TitleText))
                {
                    Text = _TitleText + " - " + str;
                }
                else
                {
                    Text = str;
                }
            }
        }


        private void OkButton_Click(object sender, EventArgs e)
        {
            _MainText = MainTextBox.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelThisButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            _MainText = MainTextBox.Text;
        }

    }
}
