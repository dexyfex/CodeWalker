using System;
using System.IO;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using CodeWalker.Utils;
using CodeWalker.Core.Utils;

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
                errorAction($"Could not load map icon {Filepath} for {Name}!\n\n{ex}");
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
        public MapIcon? Icon { get; set; }
        public Vector3 WorldPos { get; set; } //actual world pos
        public Vector3 CamRelPos { get; set; } //updated per frame
        public Vector3 ScreenPos { get; set; } //position on screen (updated per frame)
        public string Name { get; set; } = string.Empty;
        public List<string> Properties { get; set; } //additional data
        public bool IsMovable { get; set; }
        public float Distance { get; set; } //length of CamRelPos, updated per frame

        public void Parse(ReadOnlySpan<char> s)
        {
            Vector3 p = new Vector3(0.0f);

            var enumerator = s.EnumerateSplit(',');

            if (!enumerator.MoveNext())
                return;

            FloatUtil.TryParse(enumerator.Current.Trim(), out p.X);

            if (!enumerator.MoveNext())
                return;
            
            FloatUtil.TryParse(enumerator.Current.Trim(), out p.Y);

            if (enumerator.MoveNext())
            {
                FloatUtil.TryParse(enumerator.Current.Trim(), out p.Z);
            }

            WorldPos = p;

            if (enumerator.MoveNext())
            {
                Name = enumerator.Current.Trim().ToString();
            }

            while(enumerator.MoveNext())
            {
                Properties ??= new List<string>();
                Properties.Add(enumerator.Current.Trim().ToString());
            }
        }

        public override string ToString()
        {
            string cstr = Get3DWorldPosString();
            if (!string.IsNullOrEmpty(Name))
            {
                cstr += $", {Name}";
                if (Properties is not null)
                {
                    foreach (string prop in Properties)
                    {
                        cstr += $", {prop}";
                    }
                }
            }
            return cstr;
        }

        public string Get2DWorldPosString()
        {
            return string.Create(CultureInfo.InvariantCulture, $"{WorldPos.X}, {WorldPos.Y}");
        }
        public string Get3DWorldPosString()
        {
            return string.Create(CultureInfo.InvariantCulture, $"{WorldPos.X}, {WorldPos.Y}, {WorldPos.Z}");
        }


    }



    public struct MapSphere
    {
        public MapSphere()
        { }

        public MapSphere(Vector3 camRelPos, float radius)
        {
            CamRelPos = camRelPos;
            Radius = radius;
        }

        public Vector3 CamRelPos;
        public float Radius;
    }

    public struct MapBox
    {
        public MapBox()
        { }

        public MapBox(Vector3 camRelPos, Vector3 bbMin, Vector3 bbMax, Quaternion orientation, Vector3 scale)
        {
            CamRelPos = camRelPos;
            BBMin = bbMin;
            BBMax = bbMax;
            Orientation = orientation;
            Scale = scale;
        }

        public Vector3 CamRelPos;
        public Vector3 BBMin;
        public Vector3 BBMax;
        public Quaternion Orientation;
        public Vector3 Scale;
    }



}