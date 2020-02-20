﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker
{


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



        public static string GetUTF8Text(byte[] bytes)
        {
            if (bytes == null)
            { return string.Empty; } //file not found..
            if ((bytes.Length > 3) && (bytes[0] == 0xEF) && (bytes[1] == 0xBB) && (bytes[2] == 0xBF))
            {
                byte[] newb = new byte[bytes.Length - 3];
                for (int i = 3; i < bytes.Length; i++)
                {
                    newb[i - 3] = bytes[i];
                }
                bytes = newb; //trim starting byte order mark
            }
            return Encoding.UTF8.GetString(bytes);
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
        public static string GetVector2XmlString(Vector2 v)
        {
            var c = CultureInfo.InvariantCulture;
            return string.Format("x=\"{0}\" y=\"{1}\"", v.X.ToString(c), v.Y.ToString(c));
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
        public static string GetVector3XmlString(Vector3 v)
        {
            var c = CultureInfo.InvariantCulture;
            return string.Format("x=\"{0}\" y=\"{1}\" z=\"{2}\"", v.X.ToString(c), v.Y.ToString(c), v.Z.ToString(c));
        }
        public static string GetVector4XmlString(Vector4 v)
        {
            var c = CultureInfo.InvariantCulture;
            return string.Format("x=\"{0}\" y=\"{1}\" z=\"{2}\" w=\"{3}\"", v.X.ToString(c), v.Y.ToString(c), v.Z.ToString(c), v.W.ToString(c));
        }
        public static string GetQuaternionXmlString(Quaternion q)
        {
            var c = CultureInfo.InvariantCulture;
            return string.Format("x=\"{0}\" y=\"{1}\" z=\"{2}\" w=\"{3}\"", q.X.ToString(c), q.Y.ToString(c), q.Z.ToString(c), q.W.ToString(c));
        }


        public static Vector2 ParseVector2String(string s)
        {
            Vector2 p = new Vector2(0.0f);
            string[] ss = s.Split(',');
            if (ss.Length > 0)
            {
                TryParse(ss[0].Trim(), out p.X);
            }
            if (ss.Length > 1)
            {
                TryParse(ss[1].Trim(), out p.Y);
            }
            return p;
        }

        public static Vector3 ParseVector3String(string s)
        {
            Vector3 p = new Vector3(0.0f);
            string[] ss = s.Split(',');
            if (ss.Length > 0)
            {
                TryParse(ss[0].Trim(), out p.X);
            }
            if (ss.Length > 1)
            {
                TryParse(ss[1].Trim(), out p.Y);
            }
            if (ss.Length > 2)
            {
                TryParse(ss[2].Trim(), out p.Z);
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
                TryParse(ss[0].Trim(), out p.X);
            }
            if (ss.Length > 1)
            {
                TryParse(ss[1].Trim(), out p.Y);
            }
            if (ss.Length > 2)
            {
                TryParse(ss[2].Trim(), out p.Z);
            }
            if (ss.Length > 3)
            {
                TryParse(ss[3].Trim(), out p.W);
            }
            return p;
        }


    }





    public static class BitUtil
    {
        public static bool IsBitSet(uint value, int bit)
        {
            return (((value >> bit) & 1) > 0);
        }
        public static uint SetBit(uint value, int bit)
        {
            return (value | (1u << bit));
        }
        public static uint ClearBit(uint value, int bit)
        {
            return (value & (~(1u << bit)));
        }
        public static uint UpdateBit(uint value, int bit, bool flag)
        {
            if (flag) return SetBit(value, bit);
            else return ClearBit(value, bit);
        }
    }


}
