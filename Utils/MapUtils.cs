using System;
using System.IO;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using CodeWalker.Utils;

namespace CodeWalker
{


    public class MapIcon
    {
        public string Name { get; set; }
        public string Filepath { get; set; }
        public Texture2D Tex { get; set; }
        public ShaderResourceView TexView { get; set; }
        public Vector3 Center { get; set; } //in image pixels
        public float Scale { get; set; } //screen pixels per icon pixel
        public int TexWidth { get; set; }
        public int TexHeight { get; set; }

        public MapIcon(string name, string filepath, int texw, int texh, float centerx, float centery, float scale)
        {
            Name = name;
            Filepath = filepath;
            TexWidth = texw;
            TexHeight = texh;
            Center = new Vector3(centerx, centery, 0.0f);
            Scale = scale;

            if (!File.Exists(filepath))
            {
                throw new Exception("File not found.");
            }
        }

        public void LoadTexture(Device device, Action<string> errorAction)
        {
            try
            {
                if (device != null)
                {
                    Tex = TextureLoader.CreateTexture2DFromBitmap(device, TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), Filepath));
                    TexView = new ShaderResourceView(device, Tex);
                }
            }
            catch (Exception ex)
            {
                errorAction("Could not load map icon " + Filepath + " for " + Name + "!\n\n" + ex.ToString());
            }
        }

        public void UnloadTexture()
        {
            if (TexView != null)
            {
                TexView.Dispose();
                TexView = null;
            }
            if (Tex != null)
            {
                Tex.Dispose();
                Tex = null;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MapMarker
    {
        public MapIcon Icon { get; set; }
        public Vector3 WorldPos { get; set; } //actual world pos
        public Vector3 CamRelPos { get; set; } //updated per frame
        public Vector3 ScreenPos { get; set; } //position on screen (updated per frame)
        public string Name { get; set; }
        public List<string> Properties { get; set; } //additional data
        public bool IsMovable { get; set; }
        public float Distance { get; set; } //length of CamRelPos, updated per frame

        public void Parse(string s)
        {
            Vector3 p = new Vector3(0.0f);
            string[] ss = s.Split(',');
            if (ss.Length > 1)
            {
                FloatUtil.TryParse(ss[0].Trim(), out p.X);
                FloatUtil.TryParse(ss[1].Trim(), out p.Y);
            }
            if (ss.Length > 2)
            {
                FloatUtil.TryParse(ss[2].Trim(), out p.Z);
            }
            if (ss.Length > 3)
            {
                Name = ss[3].Trim();
            }
            else
            {
                Name = string.Empty;
            }
            for (int i = 4; i < ss.Length; i++)
            {
                if (Properties == null) Properties = new List<string>();
                Properties.Add(ss[i].Trim());
            }
            WorldPos = p;
        }

        public override string ToString()
        {
            string cstr = Get3DWorldPosString();
            if (!string.IsNullOrEmpty(Name))
            {
                cstr += ", " + Name;
                if (Properties != null)
                {
                    foreach (string prop in Properties)
                    {
                        cstr += ", " + prop;
                    }
                }
            }
            return cstr;
        }

        public string Get2DWorldPosString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", WorldPos.X, WorldPos.Y);
        }
        public string Get3DWorldPosString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", WorldPos.X, WorldPos.Y, WorldPos.Z);
        }


    }



    public struct MapSphere
    {
        public Vector3 CamRelPos { get; set; }
        public float Radius { get; set; }
    }

    public struct MapBox
    {
        public Vector3 CamRelPos { get; set; }
        public Vector3 BBMin { get; set; }
        public Vector3 BBMax { get; set; }
        public Quaternion Orientation { get; set; }
        public Vector3 Scale { get; set; }
    }



}