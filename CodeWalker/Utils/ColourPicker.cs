using CodeWalker.Properties;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace CodeWalker.Utils
{
    public partial class ColourPicker : UserControl
    {
        public Color SelectedColour
        {
            get => Colour;
            set => SetColour(value, true, true);
        }
        private Color OldColour = Color.Black;
        private Color Colour = Color.Black;
        private ColourComponent ColourMode = ColourComponent.H;
        private Bitmap MainGradient;
        private Bitmap SideGradient;
        private int HVal = 0;
        private int SVal = 0;
        private int VVal = 0;
        private int RVal = 0;
        private int GVal = 0;
        private int BVal = 0;
        private int AVal = 255;
        private const float Deg255 = 360.0f / 255.0f;
        private const float Hun255 = 100.0f / 255.0f;
        private const float One255 = 1.0f / 255.0f;
        private const float One100 = 1.0f / 100.0f;
        private bool UpdatingTextboxes = false;
        private float CircleThickness = 1.0f;
        private float CircleRadius = 5.8f;
        private int CircleBox = 13;
        private int CircleOffset = 6;
        private const float CircleH = 255.0f / 360.0f;
        private const float CircleSV = 255.0f / 100.0f;
        private Bitmap CircleBlack;
        private Bitmap CircleWhite;
        private int SliderBox = 13;
        private int SliderOffset = 6;
        private Bitmap SliderL;
        private Bitmap SliderR;
        private bool MainDrag;
        private bool SideDrag;
        private Color[] CustomColours;
        private Color[] RecentColours;
        private static Color[] DefaultColours =
        {
            NewColour(255,0,0,255),NewColour(255,255,0,255),NewColour(0,255,0,255),
            NewColour(0,255,255,255),NewColour(0,0,255,255),NewColour(255,0,255,255),
            NewColour(255,255,255,255),NewColour(191,191,191,255),NewColour(127,127,127,255),
            NewColour(63,63,63,255),NewColour(31,31,31,255),NewColour(0,0,0,255),
            NewColour(255,255,255,255),NewColour(238,238,238,255),NewColour(221,221,221,255),
            NewColour(204,204,204,255),NewColour(187,187,187,255),NewColour(170,170,170,255),
            NewColour(153,153,153,255),NewColour(136,136,136,255),NewColour(119,119,119,255),
            NewColour(102,102,102,255),NewColour(85,85,85,255),NewColour(68,68,68,255),
        };
        private static Color NewColour(int r, int g, int b, int a)
        {
            return Color.FromArgb(a, r, g, b);
        }
        private static Color NewColour(Vector4 v)
        {
            return Color.FromArgb(ToByte(v.W), ToByte(v.X), ToByte(v.Y), ToByte(v.Z));
        }
        private static Color ColourFromHexString(string hex)
        {
            uint.TryParse(hex, NumberStyles.AllowHexSpecifier, null, out var u);
            return NewColour((byte)(u >> 24), (byte)(u >> 16), (byte)(u >> 8), (byte)u);
        }
        private static Color ColourFromHexRGBString(string str)
        {
            var rstr = str.Length > 1 ? str.Substring(0, 2) : "00";
            var gstr = str.Length > 3 ? str.Substring(2, 2) : "00";
            var bstr = str.Length > 5 ? str.Substring(4, 2) : "00";
            int.TryParse(rstr, NumberStyles.AllowHexSpecifier, null, out var r);
            int.TryParse(gstr, NumberStyles.AllowHexSpecifier, null, out var g);
            int.TryParse(bstr, NumberStyles.AllowHexSpecifier, null, out var b);
            var a = 255;
            var c = NewColour(r, g, b, a);
            return c;
        }
        private static string ColourToHexString(Color c)
        {
            var u = (uint)(c.A | (c.B << 8) | (c.G << 16) | (c.R << 24));
            var s = u.ToString("X").PadLeft(8, '0');
            return s;
        }
        private static string ColourToHexRGBString(Color c)
        {
            var u = (uint)(c.B | (c.G << 8) | (c.R << 16));
            var s = u.ToString("X").PadLeft(6, '0');
            return s;
        }
        private static Vector4 ColourFFromHSB(float hue, float saturation, float brightness, float alpha = 1.0f)
        {
            var h = Math.Max(0, Math.Min(360.0f, hue));
            var s = Math.Max(0, Math.Min(1.0f, saturation));
            var b = Math.Max(0, Math.Min(1.0f, brightness));
            var a = Math.Max(0, Math.Min(1.0f, alpha));
            if (Math.Abs(s) < 1e-6f) return new Vector4(b, b, b, a);
            var sectorPos = h / 60.0f; // the argb wheel consists of 6 sectors. Figure out which sector you're in.
            var sectorNum = (int)Math.Floor(sectorPos);
            var sectorFrac = sectorPos - sectorNum;
            var p = b * (1.0f - s); // calculate values for the three axes of the argb.
            var q = b * (1.0f - (s * sectorFrac));
            var t = b * (1.0f - (s * (1.0f - sectorFrac)));
            switch (sectorNum) // assign the fractional colors to r, g, and b based on the sector the angle is in.
            {
                case 0: return new Vector4(b, t, p, a);
                case 1: return new Vector4(q, b, p, a);
                case 2: return new Vector4(p, b, t, a);
                case 3: return new Vector4(p, q, b, a);
                case 4: return new Vector4(t, p, b, a);
                case 5: return new Vector4(b, p, q, a);
            }
            return new Vector4(b, t, p, a);
        }
        private static Color ColourFromHSB(float hue, float saturation, float brightness, int alpha = 255)
        {
            var f = ColourFFromHSB(hue, saturation, brightness);
            var c = NewColour(ToByte(f.X), ToByte(f.Y), ToByte(f.Z), alpha);
            return c;
        }
        private static Vector3 ColourToHSB(Color c)
        {
            var r = c.R / 255.0f;
            var g = c.G / 255.0f;
            var b = c.B / 255.0f;
            var max = Math.Max(Math.Max(r, g), b);
            var min = Math.Min(Math.Min(r, g), b);
            var rng = max - min;
            var h = (Math.Abs(rng) < 1e-6f) ? 0 :
                    (Math.Abs(max - r) < 1e-6f) ?
                        ((60 * (g - b)) / rng) + ((g >= b) ? 0 : 360) :
                    (Math.Abs(max - g) < 1e-6f) ?
                        ((60 * (b - r)) / rng) + 120 :
                    (Math.Abs(max - b) < 1e-6f) ?
                        ((60 * (r - g)) / rng) + 240 : 0;
            var s = (Math.Abs(max) < 1e-6f) ? 0 : 1 - (min / max);
            return new Vector3(FloatUtil.Clamp(h, 0, 360), FloatUtil.Saturate(s), FloatUtil.Saturate(max));
        }
        private static byte ToByte(float component)
        {
            var value = (int)(component * 255.0f);
            return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
        }

        private enum ColourComponent
        {
            H, S, V, R, G, B, A, Hex, None
        }




        public ColourPicker()
        {
            InitializeComponent();
            InitPaletteColours();
            InitBitmaps();
            UpdateGradients();
            Disposed += ColourPicker_Disposed;
        }

        private void InitBitmaps()
        {
            MainGradient = new Bitmap(256, 256);
            SideGradient = new Bitmap(20, 256);
            CircleBlack = CreateCircleBitmap(Color.Black);
            CircleWhite = CreateCircleBitmap(Color.White);
            SliderL = CreateSliderBitmap(true);
            SliderR = CreateSliderBitmap(false);
        }
        private void DisposeBitmaps()
        {
            MainGradient?.Dispose();
            MainGradient = null;
            SideGradient?.Dispose();
            SideGradient = null;
            CircleBlack?.Dispose();
            CircleBlack = null;
            CircleWhite?.Dispose();
            CircleWhite = null;
            SliderL?.Dispose();
            SliderL = null;
            SliderR?.Dispose();
            SliderR = null;
        }
        private Bitmap CreateCircleBitmap(Color colour)
        {
            var c = colour;
            var b = new Bitmap(CircleBox, CircleBox);
            var cen = new Vector2(CircleOffset);
            for (int y = 0; y < CircleBox; y++)
            {
                for (int x = 0; x < CircleBox; x++)
                {
                    var p = new Vector2(x, y) - cen;
                    var r = p.Length();
                    var t = Math.Abs(r - CircleRadius) / CircleThickness;
                    var a = FloatUtil.Saturate(1 - t);
                    b.SetPixel(x, y, Color.FromArgb(ToByte(a), c.R, c.G, c.B));
                }
            }
            return b;
        }
        private Bitmap CreateSliderBitmap(bool dirL)
        {
            var outlineThickInv = 0.9f;
            var p1 = new Vector2(1.5f, 1.5f);
            var p2 = new Vector2(1.5f, SliderBox - 2.5f);
            var p3 = new Vector2(SliderBox - 1.0f, SliderBox * 0.5f - 0.5f);
            var b = new Bitmap(SliderBox, SliderBox);
            for (int y = 0; y < SliderBox; y++)
            {
                for (int x = 0; x < SliderBox; x++)
                {
                    var v = new Vector2(x, y);
                    var d1 = LineMath.DistanceFieldTest(v, p1, p2, out var w1);
                    var d2 = LineMath.DistanceFieldTest(v, p2, p3, out var w2);
                    var d3 = LineMath.DistanceFieldTest(v, p3, p1, out var w3);
                    var d = Math.Min(Math.Min(d1, d2), d3);
                    var w = w1 + w2 + w3;
                    var dw = (w != 0) ? d : -d;
                    var c1 = (dw > 0) ? Vector4.One : Vector4.Zero;
                    var a2 = FloatUtil.Saturate(1.0f - (d * outlineThickInv));
                    var c2 = new Vector4(0, 0, 0, a2);
                    var c = NewColour(Vector4.Lerp(c1, c2, a2));
                    var px = dirL ? x : SliderBox - x - 1;
                    b.SetPixel(px, y, c);
                }
            }
            return b;
        }
        private Color[] GetColoursSetting(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            var strs = str.Split(' ');
            var colours = new List<Color>();
            foreach (var item in strs)
            {
                var itemt = item.Trim();
                if (string.IsNullOrEmpty(itemt)) continue;
                var c = ColourFromHexString(itemt);
                if (c != default)
                {
                    colours.Add(c);
                }
            }
            if (colours.Count == 0) return null;
            return colours.ToArray();
        }
        private string SetColoursSetting(Color[] colours)
        {
            if ((colours == null) || (colours.Length == 0))
            {
                return "";
            }
            var sb = new StringBuilder();
            foreach (var c in colours)
            {
                sb.Append(ColourToHexString(c));
                sb.Append(" ");
            }
            var val = sb.ToString();
            return val;
        }
        private void InitPaletteColours()
        {
            var s = Settings.Default;
            var c = GetColoursSetting(s.ColourPickerCustomColours);
            var r = GetColoursSetting(s.ColourPickerRecentColours);
            CustomColours = new Color[12];
            RecentColours = new Color[12];
            for (int i = 0; i < 12; i++)
            {
                CustomColours[i] = ((c != null) && (i < c.Length)) ? c[i] : DefaultColours[i];
                RecentColours[i] = ((r != null) && (i < r.Length)) ? r[i] : DefaultColours[i + 12];
            }
        }
        private void UpdateCustomColour(int i)
        {
            if (i < 0) return;
            if (i > 11) return;
            CustomColours[i] = Colour;
            Settings.Default.ColourPickerCustomColours = SetColoursSetting(CustomColours);
            Invalidate();
        }
        private void UpdateRecentColour(int i)
        {
            if (i < 0) return;
            if (i > 11) return;
            RecentColours[i] = Colour;
            Settings.Default.ColourPickerRecentColours = SetColoursSetting(RecentColours);
            Invalidate();
        }
        public void SaveRecentColour()
        {
            //push the current colour to the front of the recent colours list, if it's not in there already
            if (RecentColours == null) return;
            if (Colour == OldColour) return;//don't try save the old colour
            foreach (var rc in RecentColours)
            {
                if (rc == Colour) return;//it's already in the list, abort
            }
            for (int i = RecentColours.Length - 1; i > 0; i--)
            {
                RecentColours[i] = RecentColours[i - 1];
            }
            RecentColours[0] = Colour;
            Settings.Default.ColourPickerRecentColours = SetColoursSetting(RecentColours);
        }

        private void SetColourMode(ColourComponent m)
        {
            ColourMode = m;
            UpdateGradients();
        }

        private void SetColourComponentEvent(ColourComponent c, string str)
        {
            if (UpdatingTextboxes) return;
            SetColourComponent(c, str, false);
        }
        private void SetColourComponent(ColourComponent c, string str, bool updateTextbox)
        {
            int.TryParse(str, out var v);
            SetColourComponent(c, v, updateTextbox);
        }
        private void SetColourComponent(ColourComponent c, int v, bool updateTextbox)
        {
            switch (c)
            {
                case ColourComponent.H: HVal = v; break;
                case ColourComponent.S: SVal = v; break;
                case ColourComponent.V: VVal = v; break;
                case ColourComponent.R: RVal = v; break;
                case ColourComponent.G: GVal = v; break;
                case ColourComponent.B: BVal = v; break;
                case ColourComponent.A: AVal = v; break;
            }
            switch (c)
            {
                case ColourComponent.H:
                case ColourComponent.S:
                case ColourComponent.V:
                    Colour = ColourFromHSB(HVal, SVal * One100, VVal * One100, AVal);
                    RVal = Colour.R;
                    GVal = Colour.G;
                    BVal = Colour.B;
                    break;
                default:
                    Colour = NewColour(RVal, GVal, BVal, AVal);
                    var hsb = ColourToHSB(Colour);
                    HVal = (int)(hsb.X);
                    SVal = (int)(hsb.Y * 100);
                    VVal = (int)(hsb.Z * 100);
                    break;
            }
            UpdateGradients();
            UpdateTextboxes(updateTextbox ? ColourComponent.None : c);
        }
        private void SetColour(Color c, bool updateTextbox = true, bool origVal = false)
        {
            Colour = c;
            if (origVal) OldColour = c;
            var hsb = ColourToHSB(Colour);
            HVal = (int)(hsb.X);
            SVal = (int)(hsb.Y * 100);
            VVal = (int)(hsb.Z * 100);
            RVal = c.R;
            GVal = c.G;
            BVal = c.B;
            AVal = c.A;
            UpdateGradients();
            if (updateTextbox)
            {
                UpdateTextboxes(ColourComponent.None);
            }
        }
        private void SetMainColour(int x, int y)
        {
            x -= 16;
            y -= 16;
            y = 255 - y;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x > 255) x = 255;
            if (y > 255) y = 255;
            var c = GetMainColour(x, y);
            var hsb = ColourToHSB(c);
            Colour = c;
            switch (ColourMode)
            {
                case ColourComponent.H:
                    SVal = (int)Math.Round(x * Hun255);
                    VVal = (int)Math.Round(y * Hun255);
                    break;
                case ColourComponent.S:
                    HVal = (int)Math.Round(x * Deg255);
                    VVal = (int)Math.Round(y * Hun255);
                    break;
                case ColourComponent.V:
                    HVal = (int)Math.Round(x * Deg255);
                    SVal = (int)Math.Round(y * Hun255);
                    break;
                default:
                    HVal = (int)(hsb.X);
                    SVal = (int)(hsb.Y * 100);
                    VVal = (int)(hsb.Z * 100);
                    break;
            }
            RVal = c.R;
            GVal = c.G;
            BVal = c.B;
            AVal = c.A;
            UpdateGradients();
            UpdateTextboxes(ColourComponent.None);
        }
        private void SetSideColour(int x, int y)
        {
            x -= 16;
            y -= 16;
            y = 255 - y;
            if (y < 0) y = 0;
            if (y > 255) y = 255;
            var c = GetSideColour(x, y, false);
            var hsb = ColourToHSB(c);
            Colour = c;
            switch (ColourMode)
            {
                case ColourComponent.H:
                    HVal = (int)Math.Round(y * Deg255);
                    break;
                case ColourComponent.S:
                    SVal = (int)Math.Round(y * Hun255);
                    break;
                case ColourComponent.V:
                    VVal = (int)Math.Round(y * Hun255);
                    break;
                default:
                    HVal = (int)(hsb.X);
                    SVal = (int)(hsb.Y * 100);
                    VVal = (int)(hsb.Z * 100);
                    break;
            }
            RVal = c.R;
            GVal = c.G;
            BVal = c.B;
            AVal = c.A;
            UpdateGradients();
            UpdateTextboxes(ColourComponent.None);
        }

        private void UpdateTextboxes(ColourComponent ignore)
        {
            UpdatingTextboxes = true;
            if (ignore != ColourComponent.H) HTextBox.Text = HVal.ToString();
            if (ignore != ColourComponent.S) STextBox.Text = SVal.ToString();
            if (ignore != ColourComponent.V) VTextBox.Text = VVal.ToString();
            if (ignore != ColourComponent.R) RTextBox.Text = RVal.ToString();
            if (ignore != ColourComponent.G) GTextBox.Text = GVal.ToString();
            if (ignore != ColourComponent.B) BTextBox.Text = BVal.ToString();
            if (ignore != ColourComponent.A) ATextBox.Text = AVal.ToString();
            if (ignore != ColourComponent.Hex) HexTextBox.Text = ColourToHexRGBString(Colour);
            UpdatingTextboxes = false;
        }

        private void UpdateGradients()
        {
            if (MainDrag || SideDrag)
            {
                UpdateTimer.Enabled = true;
                Invalidate();
            }
            else
            {
                UpdateGradientsProc();
            }
        }
        private void UpdateGradientsProc()
        {

            var mg = new byte[256 * 256 * 4];
            var sg = new byte[256 * 20 * 4];
            //Parallel.For(0, 256, y =>
            for (int y = 0; y < 256; y++)
            {
                var iy = 255 - y;
                for (int x = 0; x < 256; x++)
                {
                    var c = GetMainColour(x, iy);
                    //MainGradient.SetPixel(x, y, Color.FromArgb(c.A, c.R, c.G, c.B));
                    var i = ((y * 256) + x) * 4;
                    mg[i + 0] = c.B;
                    mg[i + 1] = c.G;
                    mg[i + 2] = c.R;
                    mg[i + 3] = c.A;
                }
                for (int x = 0; x < 20; x++)
                {
                    var c = GetSideColour(x, iy, true);
                    //SideGradient.SetPixel(x, y, Color.FromArgb(c.A, c.R, c.G, c.B));
                    var i = ((y * 20) + x) * 4;
                    sg[i + 0] = c.B;
                    sg[i + 1] = c.G;
                    sg[i + 2] = c.R;
                    sg[i + 3] = c.A;
                }
            }//);

            var mbmp = MainGradient.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var mptr = mbmp.Scan0;
            Marshal.Copy(mg, 0, mptr, mg.Length);
            MainGradient.UnlockBits(mbmp);

            var sbmp = SideGradient.LockBits(new Rectangle(0, 0, 20, 256), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var sptr = sbmp.Scan0;
            Marshal.Copy(sg, 0, sptr, sg.Length);
            SideGradient.UnlockBits(sbmp);

            Invalidate();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Color GetMainColour(int x, int y)
        {
            switch (ColourMode)
            {
                case ColourComponent.H: return ColourFromHSB(HVal, x * One255, y * One255, AVal);
                case ColourComponent.S: return ColourFromHSB(x * Deg255, SVal * One100, y * One255, AVal);
                case ColourComponent.V: return ColourFromHSB(x * Deg255, y * One255, VVal * One100, AVal);
                case ColourComponent.R: return NewColour(RVal, x, y, AVal);
                case ColourComponent.G: return NewColour(x, GVal, y, AVal);
                case ColourComponent.B: return NewColour(x, y, BVal, AVal);
                case ColourComponent.A: return NewColour(RVal, GVal, BVal, y);
                default: return NewColour(x, y, 0, AVal);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Color GetSideColour(int x, int y, bool preview)
        {
            switch (ColourMode)
            {
                case ColourComponent.H: return preview ? ColourFromHSB(y * Deg255, 1, 1, AVal) : ColourFromHSB(y * Deg255, SVal * One100, VVal * One100, AVal);
                case ColourComponent.S: return ColourFromHSB(HVal, y * One255, VVal * One100, AVal);
                case ColourComponent.V: return ColourFromHSB(HVal, SVal * One100, y * One255, AVal);
                case ColourComponent.R: return NewColour(y, GVal, BVal, AVal);
                case ColourComponent.G: return NewColour(RVal, y, BVal, AVal);
                case ColourComponent.B: return NewColour(RVal, GVal, y, AVal);
                case ColourComponent.A: return NewColour(RVal, GVal, BVal, y);
                default: return NewColour(x, y, 0, AVal);
            }
        }

        private int GetCircleX()
        {
            switch (ColourMode)
            {
                case ColourComponent.H: return (int)Math.Round(SVal * CircleSV);
                case ColourComponent.S: return (int)Math.Round(HVal * CircleH);
                case ColourComponent.V: return (int)Math.Round(HVal * CircleH);
                case ColourComponent.R: return GVal;
                case ColourComponent.G: return RVal;
                case ColourComponent.B: return RVal;
                case ColourComponent.A: return 0;
                default: return RVal;
            }
        }
        private int GetCircleY()
        {
            switch (ColourMode)
            {
                case ColourComponent.H: return (int)Math.Round(VVal * CircleSV);
                case ColourComponent.S: return (int)Math.Round(VVal * CircleSV);
                case ColourComponent.V: return (int)Math.Round(SVal * CircleSV);
                case ColourComponent.R: return BVal;
                case ColourComponent.G: return BVal;
                case ColourComponent.B: return GVal;
                case ColourComponent.A: return AVal;
                default: return GVal;
            }
        }
        private int GetSliderY()
        {
            switch (ColourMode)
            {
                case ColourComponent.H: return (int)Math.Round(HVal * CircleH);
                case ColourComponent.S: return (int)Math.Round(SVal * CircleSV);
                case ColourComponent.V: return (int)Math.Round(VVal * CircleSV);
                case ColourComponent.R: return RVal;
                case ColourComponent.G: return GVal;
                case ColourComponent.B: return BVal;
                case ColourComponent.A: return AVal;
                default: return RVal;
            }
        }

        private bool InMainImage(int x, int y)
        {
            if (x < 16) return false;
            if (y < 16) return false;
            if (x > 271) return false;
            if (y > 271) return false;
            return true;
        }
        private bool InSideImage(int x, int y)
        {
            if (x < 280) return false;
            if (y < 16) return false;
            if (x > 320) return false;
            if (y > 271) return false;
            return true;
        }

        private int GetCustomColourIndex(int x, int y)
        {
            if (y < 296) return -1;
            if (y > (296 + 23)) return -1;
            var xr = x - 130;
            if (xr < 0) return -1;
            var xi = xr / 26;
            if (xi > 11) return -1;
            var t = xr - (xi * 26);
            if (t > 23) return -1;
            return xi;
        }
        private int GetRecentColourIndex(int x, int y)
        {
            if (y < (296 + 26)) return -1;
            if (y > (296 + 26 + 23)) return -1;
            var xr = x - 130;
            if (xr < 0) return -1;
            var xi = xr / 26;
            if (xi > 11) return -1;
            var t = xr - (xi * 26);
            if (t > 23) return -1;
            return xi;
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (MainGradient == null) return;

            var g = e.Graphics;

            g.DrawImage(MainGradient, 16, 16);
            g.DrawImage(SideGradient, 290, 16);

            var circImg = (VVal >= 80) ? CircleBlack : CircleWhite;
            var cx = 16 + GetCircleX() - CircleOffset;
            var cy = 16 - GetCircleY() - CircleOffset + 255;
            g.SetClip(new Rectangle(16, 16, 256, 256));
            g.DrawImage(circImg, cx, cy);
            g.ResetClip();


            var sy = 16 - GetSliderY() - SliderOffset + 255;
            var sxL = 290 - SliderBox;
            var sxR = 290 + 20;
            g.DrawImage(SliderL, sxL, sy);
            g.DrawImage(SliderR, sxR, sy);


            using (var ob = new SolidBrush(Color.FromArgb(OldColour.A, OldColour.R, OldColour.G, OldColour.B)))
            {
                using (var nb = new SolidBrush(Color.FromArgb(Colour.A, Colour.R, Colour.G, Colour.B)))
                {
                    g.FillRectangle(nb, 16, 296, 50, 50);
                    g.FillRectangle(ob, 66, 296, 50, 50);

                    for (int y = 0; y < 2; y++)
                    {
                        var carr = (y == 0) ? CustomColours : RecentColours;
                        for (int x = 0; x < 12; x++)
                        {
                            var c = ((carr != null) && (x < carr.Length)) ? carr[x] : NewColour(0, 0, 0, 255);
                            using (var b = new SolidBrush(Color.FromArgb(c.A, c.R, c.G, c.B)))
                            {
                                g.FillRectangle(b, 130 + (x * 26), 296 + (y * 26), 24, 24);
                            }
                        }
                    }
                }
            }

        }



        private void HRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (HRadio.Checked) SetColourMode(ColourComponent.H);
        }

        private void SRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (SRadio.Checked) SetColourMode(ColourComponent.S);
        }

        private void VRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (VRadio.Checked) SetColourMode(ColourComponent.V);
        }

        private void RRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (RRadio.Checked) SetColourMode(ColourComponent.R);
        }

        private void GRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (GRadio.Checked) SetColourMode(ColourComponent.G);
        }

        private void BRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (BRadio.Checked) SetColourMode(ColourComponent.B);
        }

        private void ARadio_CheckedChanged(object sender, EventArgs e)
        {
            if (ARadio.Checked) SetColourMode(ColourComponent.A);
        }

        private void HTextBox_TextChanged(object sender, EventArgs e)
        {
            SetColourComponentEvent(ColourComponent.H, HTextBox.Text);
        }

        private void STextBox_TextChanged(object sender, EventArgs e)
        {
            SetColourComponentEvent(ColourComponent.S, STextBox.Text);
        }

        private void VTextBox_TextChanged(object sender, EventArgs e)
        {
            SetColourComponentEvent(ColourComponent.V, VTextBox.Text);
        }

        private void RTextBox_TextChanged(object sender, EventArgs e)
        {
            SetColourComponentEvent(ColourComponent.R, RTextBox.Text);
        }

        private void GTextBox_TextChanged(object sender, EventArgs e)
        {
            SetColourComponentEvent(ColourComponent.G, GTextBox.Text);
        }

        private void BTextBox_TextChanged(object sender, EventArgs e)
        {
            SetColourComponentEvent(ColourComponent.B, BTextBox.Text);
        }

        private void ATextBox_TextChanged(object sender, EventArgs e)
        {
            SetColourComponentEvent(ColourComponent.A, ATextBox.Text);
        }

        private void HexTextBox_TextChanged(object sender, EventArgs e)
        {
            if (UpdatingTextboxes) return;
            var c = ColourFromHexRGBString(HexTextBox.Text);
            SetColour(c, false);
            UpdateTextboxes(ColourComponent.Hex);
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimer.Enabled = false;
            UpdateGradientsProc();
        }

        private void ColourPicker_MouseDown(object sender, MouseEventArgs e)
        {
            if (InMainImage(e.X, e.Y))
            {
                SetMainColour(e.X, e.Y);
                MainDrag = true;
            }
            else if (InSideImage(e.X, e.Y))
            {
                SetSideColour(e.X, e.Y);
                SideDrag = true;
            }
            else
            {
                var ci = GetCustomColourIndex(e.X, e.Y);
                var ri = GetRecentColourIndex(e.X, e.Y);
                if (ci != -1)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        UpdateCustomColour(ci);
                    }
                    else
                    {
                        SetColour(CustomColours[ci]);
                    }
                }
                else if (ri != -1)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        UpdateRecentColour(ri);
                    }
                    else
                    {
                        SetColour(RecentColours[ri]);
                    }
                }
            }
        }

        private void ColourPicker_MouseUp(object sender, MouseEventArgs e)
        {
            if (MainDrag)
            {
                SetMainColour(e.X, e.Y);
            }
            else if (SideDrag)
            {
                SetSideColour(e.X, e.Y);
            }
            MainDrag = false;
            SideDrag = false;
        }

        private void ColourPicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (MainDrag)
            {
                SetMainColour(e.X, e.Y);
            }
            else if (SideDrag)
            {
                SetSideColour(e.X, e.Y);
            }
        }

        private void ColourPicker_Disposed(object sender, EventArgs e)
        {
            DisposeBitmaps();
        }
    }
}
