using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.WinForms
{
    public partial class QuaternionBox : UserControl
    {
        public QuaternionBox()
        {
            InitializeComponent();
        }


        public Quaternion Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                UpdateFromValue();
            }
        }
        public Vector3 EulerDeg
        {
            get
            {
                return new Vector3((float)EulerXUpDown.Value, (float)EulerYUpDown.Value, (float)EulerZUpDown.Value);
            }
        }

        private Quaternion _Value = Quaternion.Identity;
        private bool suppressEvents = false;

        public event EventHandler ValueChanged;
        private void RaiseValueChanged()
        {
            if (suppressEvents) return;
            if (ValueChanged == null) return;
            ValueChanged(this, null);
        }


        private void UpdateFromValue()
        {
            suppressEvents = true;
            UpdateTextBox();
            UpdateEulerUpDowns();
            suppressEvents = false;
        }


        private void UpdateFromTextBox()
        {
            if (suppressEvents) return;
            suppressEvents = true;
            _Value = ParseQuaternionString(QuaternionTextBox.Text);
            UpdateEulerUpDowns();
            suppressEvents = false;
            RaiseValueChanged();
        }

        private void UpdateFromEuler()
        {
            if (suppressEvents) return;
            suppressEvents = true;
            _Value = GetQuaternion(EulerXUpDown.Value, EulerYUpDown.Value, EulerZUpDown.Value);
            UpdateTextBox();
            suppressEvents = false;
            RaiseValueChanged();
        }

        private void UpdateTextBox()
        {
            QuaternionTextBox.Text = GetQuaternionString(_Value);
        }

        private void UpdateEulerUpDowns()
        {
            var e = GetEulerAngles(_Value);
            EulerXUpDown.Value = (decimal)e.X;
            EulerYUpDown.Value = (decimal)e.Y;
            EulerZUpDown.Value = (decimal)e.Z;
        }

        private void Normalize()
        {
            _Value.Normalize();
            UpdateFromValue();
            RaiseValueChanged();
        }







        private static Quaternion ParseQuaternionString(string s)
        {
            bool tryParseFloat(string str, out float f)
            {
                if (float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out f))
                {
                    return true;
                }
                return false;
            }

            Quaternion q = Quaternion.Identity;
            string[] ss = s.Split(',');
            if (ss.Length > 0)
            {
                tryParseFloat(ss[0].Trim(), out q.X);
            }
            if (ss.Length > 1)
            {
                tryParseFloat(ss[1].Trim(), out q.Y);
            }
            if (ss.Length > 2)
            {
                tryParseFloat(ss[2].Trim(), out q.Z);
            }
            if (ss.Length > 3)
            {
                tryParseFloat(ss[3].Trim(), out q.W);
            }
            return q;
        }
        private static string GetQuaternionString(Quaternion q, string d = ", ")
        {
            var c = CultureInfo.InvariantCulture;
            return q.X.ToString(c) + d + q.Y.ToString(c) + d + q.Z.ToString(c) + d + q.W.ToString(c);
        }
        private static Vector3 GetEulerAngles(Quaternion q)
        {
            var x = q.X;
            var y = q.Y;
            var z = q.Z;
            var w = q.W;
            var xx = x * x;
            var yy = y * y;
            var zz = z * z;
            var ww = w * w;
            var ls = xx + yy + zz + ww;
            var st = x * w - y * z;
            var sv = ls * 0.499f;
            var rd = 180.0f / (float)Math.PI;
            if (st > sv)
            {
                return new Vector3(90, (float)Math.Atan2(y, x) * 2.0f * rd, 0);
            }
            else if (st < -sv)
            {
                return new Vector3(-90, (float)Math.Atan2(y, x) * -2.0f * rd, 0);
            }
            else
            {
                return new Vector3(
                    (float)Math.Asin(2.0f * st) * rd,
                    (float)Math.Atan2(2.0f * (y * w + x * z), 1.0f - 2.0f * (xx + yy)) * rd,
                    (float)Math.Atan2(2.0f * (x * y + z * w), 1.0f - 2.0f * (xx + zz)) * rd
                    );
            }
        }
        private static Quaternion GetQuaternion(decimal x, decimal y, decimal z)
        {
            var deg = new Vector3((float)x, (float)y, (float)z);
            var rads = deg * (float)(Math.PI / 180.0);
            return Quaternion.RotationYawPitchRoll(rads.Y, rads.X, rads.Z);
        }



        private void QuaternionTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateFromTextBox();
        }

        private void EulerXUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateFromEuler();
        }

        private void EulerYUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateFromEuler();
        }

        private void EulerZUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateFromEuler();
        }

        private void NormalizeButton_Click(object sender, EventArgs e)
        {
            Normalize();
        }
    }

}
