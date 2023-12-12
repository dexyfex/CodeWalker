using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX.Direct3D11;
using SharpDX;
using System.IO;
using System.Diagnostics;

namespace CodeWalker.Rendering
{
    public struct WidgetShaderSceneVars
    {
        public Matrix ViewProj;
        public uint Mode; //0=Vertices, 1=Arc
        public float Size; //world units
        public float SegScale; //arc angle / number of segments
        public float SegOffset; //angle offset of arc
        public Vector3 CamRel; //center position
        public uint CullBack; //culls pixels behind 0,0,0
        public Color4 Colour; //colour for arc
        public Vector3 Axis1; //axis 1 of arc
        public float WidgetPad2;
        public Vector3 Axis2; //axis 2 of arc
        public float WidgetPad3;
    }


    public class WidgetShader : Shader
    {
        VertexShader vs;
        PixelShader ps;

        GpuVarsBuffer<WidgetShaderSceneVars> SceneVars;
        GpuCBuffer<WidgetShaderVertex> Vertices;
        

        public WidgetShader(Device device)
        {
            string folder = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Shaders");
            byte[] vsbytes = File.ReadAllBytes(Path.Combine(folder, "WidgetVS.cso"));
            byte[] psbytes = File.ReadAllBytes(Path.Combine(folder, "WidgetPS.cso"));

            vs = new VertexShader(device, vsbytes);
            ps = new PixelShader(device, psbytes);

            SceneVars = new GpuVarsBuffer<WidgetShaderSceneVars>(device);

            Vertices = new GpuCBuffer<WidgetShaderVertex>(device, 150); //should be more than needed....

        }
        public void Dispose()
        {
            Vertices.Dispose();
            SceneVars.Dispose();
            ps.Dispose();
            vs.Dispose();
        }

        public override void SetShader(DeviceContext context)
        {
            context.VertexShader.Set(vs);
            context.PixelShader.Set(ps);
        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            context.InputAssembler.InputLayout = null;
            context.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.Unknown, 0);
            return true;
        }
        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap shadowmap, ShaderGlobalLights lights)
        {
            SceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            SceneVars.Update(context);
            SceneVars.SetVSCBuffer(context, 0);

        }
        public override void SetEntityVars(DeviceContext context, ref RenderableInst rend)
        {
        }
        public override void SetModelVars(DeviceContext context, RenderableModel model)
        {
        }
        public override void SetGeomVars(DeviceContext context, RenderableGeometry geom)
        {
        }

        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.VertexShader.SetShaderResource(0, null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
        }


        public void DrawDefaultWidget(DeviceContext context, Camera cam, Vector3 camrel, Quaternion ori, float size)
        {
            SetShader(context);
            SetInputLayout(context, VertexType.Default);

            SceneVars.Vars.Mode = 0; //vertices mode
            SceneVars.Vars.CamRel = camrel;
            SetSceneVars(context, cam, null, null);

            Vector3 xdir = ori.Multiply(Vector3.UnitX);
            Vector3 ydir = ori.Multiply(Vector3.UnitY);
            Vector3 zdir = ori.Multiply(Vector3.UnitZ);
            Color4 xcolour = new Color4(0.8f, 0.0f, 0.0f, 1.0f);
            Color4 ycolour = new Color4(0.8f, 0.0f, 0.0f, 1.0f);
            Color4 zcolour = new Color4(0.5f, 0.5f, 0.5f, 1.0f);

            Vector3[] axes = { xdir, ydir, zdir };
            Vector3[] sides = { ydir, xdir, xdir };
            Color4[] colours = { xcolour, ycolour, zcolour };

            float linestart = 0.0f * size;
            float lineend = 1.0f * size;
            float arrowstart = 0.9f * size;
            float arrowend = 1.0f * size;
            float arrowrad = 0.05f * size;

            //draw lines...
            Vertices.Clear();

            for (int i = 0; i < 3; i++)
            {
                WidgetAxis sa = (WidgetAxis)(1 << i);
                Color4 axcol = colours[i];

                //main axis lines
                Vertices.Add(new WidgetShaderVertex(axes[i] * linestart, axcol));
                Vertices.Add(new WidgetShaderVertex(axes[i] * lineend, axcol));

                //arrow heads
                Vector3 astart = axes[i] * arrowstart;
                Vector3 aend = axes[i] * arrowend;
                Vector3 aside = sides[i] * arrowrad;
                Vertices.Add(new WidgetShaderVertex(aend, axcol));
                Vertices.Add(new WidgetShaderVertex(astart + aside, axcol));
                Vertices.Add(new WidgetShaderVertex(aend, axcol));
                Vertices.Add(new WidgetShaderVertex(astart - aside, axcol));
            }

            Vertices.Update(context);
            Vertices.SetVSResource(context, 0);

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            context.Draw(Vertices.CurrentCount, 0);

            UnbindResources(context);
        }

        public void DrawPositionWidget(DeviceContext context, Camera cam, Vector3 camrel, Quaternion ori, float size, WidgetAxis selax)
        {
            SetShader(context);
            SetInputLayout(context, VertexType.Default);

            SceneVars.Vars.Mode = 0; //vertices mode
            SceneVars.Vars.CamRel = camrel;
            SetSceneVars(context, cam, null, null);

            Vector3 xdir = ori.Multiply(Vector3.UnitX);
            Vector3 ydir = ori.Multiply(Vector3.UnitY);
            Vector3 zdir = ori.Multiply(Vector3.UnitZ);
            Color4 xcolour = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
            Color4 ycolour = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
            Color4 zcolour = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
            Color4 selaxcol = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
            Color4 selplcol = new Color4(1.0f, 1.0f, 0.0f, 0.5f);

            Vector3[] axes = { xdir, ydir, zdir };
            Vector3[] sides1 = { ydir, zdir, xdir };
            Vector3[] sides2 = { zdir, xdir, ydir };
            WidgetAxis[] sideax1 = { WidgetAxis.Y, WidgetAxis.Z, WidgetAxis.X };
            WidgetAxis[] sideax2 = { WidgetAxis.Z, WidgetAxis.X, WidgetAxis.Y };
            Color4[] colours = { xcolour, ycolour, zcolour };
            Color4[] coloursdark = { xcolour * 0.5f, ycolour * 0.5f, zcolour * 0.5f };
            for (int i = 0; i < 3; i++) coloursdark[i].Alpha = 1.0f;

            float linestart = 0.2f * size;
            float lineend = 1.0f * size;
            float sideval = 0.4f * size;
            float arrowstart = 1.0f * size;
            float arrowend = 1.33f * size;
            float arrowrad = 0.06f * size;

            float hexx = 0.5f;
            float hexy = 0.86602540378443864676372317075294f; //sqrt(0.75)
            Vector2[] arrowv =
            {
                new Vector2(-1, 0) * arrowrad,
                new Vector2(-hexx, hexy) * arrowrad,
                new Vector2(hexx, hexy) * arrowrad,
                new Vector2(1, 0) * arrowrad,
                new Vector2(hexx, -hexy) * arrowrad,
                new Vector2(-hexx, -hexy) * arrowrad,
                new Vector2(-1, 0) * arrowrad
            };


            

            //draw lines...
            Vertices.Clear();

            for (int i = 0; i < 3; i++)
            {
                WidgetAxis sa = (WidgetAxis)(1 << i);
                bool axsel = ((selax & sa) > 0);
                Color4 axcol = axsel ? selaxcol : colours[i];

                //axis side square lines
                Vector3 ax = axes[i] * sideval;
                Vector3 s1 = sides1[i] * sideval;
                Vector3 s2 = sides2[i] * sideval;
                Color4 sc1 = (axsel && ((selax & sideax1[i]) > 0)) ? selaxcol : colours[i];
                Color4 sc2 = (axsel && ((selax & sideax2[i]) > 0)) ? selaxcol : colours[i];
                Vertices.Add(new WidgetShaderVertex(ax, sc1));
                Vertices.Add(new WidgetShaderVertex(ax + s1, sc1));
                Vertices.Add(new WidgetShaderVertex(ax, sc2));
                Vertices.Add(new WidgetShaderVertex(ax + s2, sc2));

                //main axis lines - draw after side lines to be on top
                Vertices.Add(new WidgetShaderVertex(axes[i] * linestart, axcol));
                Vertices.Add(new WidgetShaderVertex(axes[i] * lineend, axcol));
            }

            Vertices.Update(context);
            Vertices.SetVSResource(context, 0);

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            context.Draw(Vertices.CurrentCount, 0);



            //draw triangles...
            Vertices.Clear();

            for (int i = 0; i < 3; i++)
            {
                //axis arrows - kind of inefficient, but meh
                Vector3 aend = axes[i] * arrowend;
                Vector3 astart = axes[i] * arrowstart;
                for (int n = 0; n < 6; n++)
                {
                    Vector2 a1 = arrowv[n];
                    Vector2 a2 = arrowv[n + 1];
                    Vector3 ap1 = astart + sides1[i] * a1.Y + sides2[i] * a1.X;
                    Vector3 ap2 = astart + sides1[i] * a2.Y + sides2[i] * a2.X;
                    Vertices.Add(new WidgetShaderVertex(aend, colours[i]));
                    Vertices.Add(new WidgetShaderVertex(ap1, colours[i]));
                    Vertices.Add(new WidgetShaderVertex(ap2, colours[i]));
                    Vertices.Add(new WidgetShaderVertex(astart, coloursdark[i]));
                    Vertices.Add(new WidgetShaderVertex(ap2, coloursdark[i]));
                    Vertices.Add(new WidgetShaderVertex(ap1, coloursdark[i]));
                }

                //selection planes
                WidgetAxis sa = (WidgetAxis)(1 << i);
                if (((selax & sa) > 0))
                {
                    Vector3 ax = axes[i] * sideval;
                    for (int n = i + 1; n < 3; n++)
                    {
                        WidgetAxis tsa = (WidgetAxis)(1 << n);
                        if (((selax & tsa) > 0))
                        {
                            Vector3 tax = axes[n] * sideval;
                            Vertices.Add(new WidgetShaderVertex(Vector3.Zero, selplcol));
                            Vertices.Add(new WidgetShaderVertex(ax, selplcol));
                            Vertices.Add(new WidgetShaderVertex(tax, selplcol));
                            Vertices.Add(new WidgetShaderVertex(tax + ax, selplcol));
                            Vertices.Add(new WidgetShaderVertex(tax, selplcol));
                            Vertices.Add(new WidgetShaderVertex(ax, selplcol));
                        }
                    }
                }
            }

            Vertices.Update(context);
            Vertices.SetVSResource(context, 0);

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            context.Draw(Vertices.CurrentCount, 0);

            UnbindResources(context);
        }

        public void DrawRotationWidget(DeviceContext context, Camera cam, Vector3 camrel, Quaternion ori, float size, WidgetAxis selax, WidgetAxis drawax)
        {
            SetShader(context);
            SetInputLayout(context, VertexType.Default);


            SceneVars.Vars.Mode = 0; //vertices mode
            SceneVars.Vars.CamRel = camrel;
            SetSceneVars(context, cam, null, null);

            Vector3 xdir = ori.Multiply(Vector3.UnitX);
            Vector3 ydir = ori.Multiply(Vector3.UnitY);
            Vector3 zdir = ori.Multiply(Vector3.UnitZ);
            Color4 xcolour = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
            Color4 ycolour = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
            Color4 zcolour = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
            Color4 icolour = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            Color4 ocolour = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            Color4 scolour = new Color4(1.0f, 1.0f, 0.0f, 1.0f);

            Vector3[] axes = { xdir, ydir, zdir };
            Vector3[] sides = { ydir, xdir, xdir };
            Color4[] colours = { xcolour, ycolour, zcolour };

            float linestart = 0.0f * size;
            float lineend = 0.3f * size;
            float ocircsize = 1.0f * size;
            float icircsize = 0.75f * size;

            //draw lines...
            Vertices.Clear();

            for (int i = 0; i < 3; i++)
            {
                WidgetAxis sa = (WidgetAxis)(1 << i);
                bool axsel = ((selax & sa) > 0);
                Color4 axcol = axsel ? colours[i] : icolour;

                //main axis lines
                Vertices.Add(new WidgetShaderVertex(axes[i] * linestart, axcol));
                Vertices.Add(new WidgetShaderVertex(axes[i] * lineend, axcol));
            }

            Vertices.Update(context);
            Vertices.SetVSResource(context, 0);

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            context.Draw(Vertices.CurrentCount, 0);



            //linestrip for arcs and circles
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineStrip;

            Vector3 sdir = Vector3.Normalize(camrel);
            //if (cam.IsMapView || cam.IsOrthographic)
            //{
            //    sdir = cam.ViewDirection;
            //}
            float ad1 = Math.Abs(Vector3.Dot(sdir, Vector3.UnitY));
            float ad2 = Math.Abs(Vector3.Dot(sdir, Vector3.UnitZ));
            Vector3 ax1 = Vector3.Normalize(Vector3.Cross(sdir, (ad1 > ad2) ? Vector3.UnitY : Vector3.UnitZ));
            Vector3 ax2 = Vector3.Normalize(Vector3.Cross(sdir, ax1));

            //drawing circles
            int segcount = 40;
            int vertcount = segcount + 1;
            SceneVars.Vars.Mode = 1; //arc mode
            SceneVars.Vars.SegScale = ((float)Math.PI) * 2.0f / segcount; //arc angle / number of segments
            SceneVars.Vars.SegOffset = 0.0f; //angle offset of arc
            SceneVars.Vars.Axis1 = ax1; //axis 1 of arc
            SceneVars.Vars.Axis2 = ax2; //axis 2 of arc
            SceneVars.Vars.CullBack = 0; //culls pixels behind 0,0,0

            //outer circle
            if (drawax == WidgetAxis.XYZ)
            {
                SceneVars.Vars.Size = ocircsize; //world units
                SceneVars.Vars.Colour = (selax == WidgetAxis.XYZ) ? scolour : ocolour; //colour for arc
                SetSceneVars(context, cam, null, null);
                context.Draw(vertcount, 0);
            }

            //inner circle
            SceneVars.Vars.Size = icircsize; //world units
            SceneVars.Vars.Colour = icolour; //colour for arc
            SetSceneVars(context, cam, null, null);
            context.Draw(vertcount, 0);


            //drawing arcs - culling done in PS
            SceneVars.Vars.Size = icircsize; //world units
            SceneVars.Vars.CullBack = 1; //culls pixels behind 0,0,0

            if ((drawax & WidgetAxis.X) != 0)
            {
                SceneVars.Vars.SegOffset = 0.0f; //angle offset of arc
                SceneVars.Vars.Axis1 = ydir; //axis 1 of arc
                SceneVars.Vars.Axis2 = zdir; //axis 2 of arc
                SceneVars.Vars.Colour = (selax == WidgetAxis.X) ? scolour : xcolour; //colour for arc
                SetSceneVars(context, cam, null, null);
                context.Draw(vertcount, 0);
            }

            if ((drawax & WidgetAxis.Y) != 0)
            {
                SceneVars.Vars.SegOffset = 0.0f; //angle offset of arc
                SceneVars.Vars.Axis1 = xdir; //axis 1 of arc
                SceneVars.Vars.Axis2 = zdir; //axis 2 of arc
                SceneVars.Vars.Colour = (selax == WidgetAxis.Y) ? scolour : ycolour; //colour for arc
                SetSceneVars(context, cam, null, null);
                context.Draw(vertcount, 0);
            }

            if ((drawax & WidgetAxis.Z) != 0)
            {
                SceneVars.Vars.SegOffset = 0.0f; //angle offset of arc
                SceneVars.Vars.Axis1 = xdir; //axis 1 of arc
                SceneVars.Vars.Axis2 = ydir; //axis 2 of arc
                SceneVars.Vars.Colour = (selax == WidgetAxis.Z) ? scolour : zcolour; //colour for arc
                SetSceneVars(context, cam, null, null);
                context.Draw(vertcount, 0);
            }




            UnbindResources(context);
        }

        public void DrawScaleWidget(DeviceContext context, Camera cam, Vector3 camrel, Quaternion ori, float size, WidgetAxis selax)
        {
            SetShader(context);
            SetInputLayout(context, VertexType.Default);

            SceneVars.Vars.Mode = 0; //vertices mode
            SceneVars.Vars.CamRel = camrel;
            SetSceneVars(context, cam, null, null);

            Vector3 xdir = ori.Multiply(Vector3.UnitX);
            Vector3 ydir = ori.Multiply(Vector3.UnitY);
            Vector3 zdir = ori.Multiply(Vector3.UnitZ);
            Color4 xcolour = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
            Color4 ycolour = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
            Color4 zcolour = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
            Color4 selaxcol = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
            Color4 selplcol = new Color4(1.0f, 1.0f, 0.0f, 0.5f);

            Vector3[] axes = { xdir, ydir, zdir };
            Vector3[] sides1 = { ydir, zdir, xdir };
            Vector3[] sides2 = { zdir, xdir, ydir };
            WidgetAxis[] sideax1 = { WidgetAxis.Y, WidgetAxis.Z, WidgetAxis.X };
            WidgetAxis[] sideax2 = { WidgetAxis.Z, WidgetAxis.X, WidgetAxis.Y };
            Color4[] colours = { xcolour, ycolour, zcolour };
            Color4[] coloursn = { ycolour, zcolour, xcolour };
            Color4[] coloursdark = { xcolour * 0.5f, ycolour * 0.5f, zcolour * 0.5f };
            for (int i = 0; i < 3; i++) coloursdark[i].Alpha = 1.0f;

            float linestart = 0.0f * size;
            float lineend = 1.33f * size;
            float innertri = 0.7f * size;
            float outertri = 1.0f * size;
            float cubestart = 1.28f * size;
            float cubeend = 1.33f * size;
            float cubesize = 0.025f * size;

            //draw lines...
            Vertices.Clear();

            for (int i = 0; i < 3; i++)
            {
                WidgetAxis sa = (WidgetAxis)(1 << i);
                bool axsel = ((selax & sa) > 0);
                Color4 axcol = axsel ? selaxcol : colours[i];

                WidgetAxis triaxn = sideax1[i];
                bool trisel = axsel && ((selax & triaxn) > 0);
                Color4 tricol = trisel ? selaxcol : colours[i];
                Color4 trincol = trisel ? selaxcol : coloursn[i];

                Vector3 inner1 = axes[i] * innertri;
                Vector3 inner2 = sides1[i] * innertri;
                Vector3 innera = (inner1 + inner2) * 0.5f;
                Vector3 outer1 = axes[i] * outertri;
                Vector3 outer2 = sides1[i] * outertri;
                Vector3 outera = (outer1 + outer2) * 0.5f;

                //triangle axis lines
                Vertices.Add(new WidgetShaderVertex(inner1, tricol));
                Vertices.Add(new WidgetShaderVertex(innera, tricol));
                Vertices.Add(new WidgetShaderVertex(innera, trincol));
                Vertices.Add(new WidgetShaderVertex(inner2, trincol));
                Vertices.Add(new WidgetShaderVertex(outer1, tricol));
                Vertices.Add(new WidgetShaderVertex(outera, tricol));
                Vertices.Add(new WidgetShaderVertex(outera, trincol));
                Vertices.Add(new WidgetShaderVertex(outer2, trincol));

                //main axis lines - draw after side lines to be on top
                Vertices.Add(new WidgetShaderVertex(axes[i] * linestart, axcol));
                Vertices.Add(new WidgetShaderVertex(axes[i] * lineend, axcol));
            }

            Vertices.Update(context);
            Vertices.SetVSResource(context, 0);


            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            context.Draw(Vertices.CurrentCount, 0);





            //draw triangles...
            Vertices.Clear();

            for (int i = 0; i < 3; i++)
            {
                //axis end cubes - kind of inefficient, but meh
                Vector3 cend = axes[i] * cubeend;
                Vector3 cstart = axes[i] * cubestart;
                Vector3 cside1 = sides1[i] * cubesize;
                Vector3 cside2 = sides2[i] * cubesize;
                Vector3 cv1 = cstart + cside1 - cside2;
                Vector3 cv2 = cstart - cside1 - cside2;
                Vector3 cv3 = cend + cside1 - cside2;
                Vector3 cv4 = cend - cside1 - cside2;
                Vector3 cv5 = cstart + cside1 + cside2;
                Vector3 cv6 = cstart - cside1 + cside2;
                Vector3 cv7 = cend + cside1 + cside2;
                Vector3 cv8 = cend - cside1 + cside2;
                Color4 col = colours[i];
                Color4 cold = coloursdark[i];
                Vertices.Add(new WidgetShaderVertex(cv1, cold));
                Vertices.Add(new WidgetShaderVertex(cv2, cold));
                Vertices.Add(new WidgetShaderVertex(cv5, cold));
                Vertices.Add(new WidgetShaderVertex(cv5, cold));
                Vertices.Add(new WidgetShaderVertex(cv2, cold));
                Vertices.Add(new WidgetShaderVertex(cv6, cold));
                Vertices.Add(new WidgetShaderVertex(cv3, col));
                Vertices.Add(new WidgetShaderVertex(cv4, col));
                Vertices.Add(new WidgetShaderVertex(cv7, col));
                Vertices.Add(new WidgetShaderVertex(cv7, col));
                Vertices.Add(new WidgetShaderVertex(cv4, col));
                Vertices.Add(new WidgetShaderVertex(cv8, col));
                Vertices.Add(new WidgetShaderVertex(cv1, col));
                Vertices.Add(new WidgetShaderVertex(cv2, col));
                Vertices.Add(new WidgetShaderVertex(cv3, col));
                Vertices.Add(new WidgetShaderVertex(cv3, col));
                Vertices.Add(new WidgetShaderVertex(cv2, col));
                Vertices.Add(new WidgetShaderVertex(cv4, col));
                Vertices.Add(new WidgetShaderVertex(cv5, col));
                Vertices.Add(new WidgetShaderVertex(cv6, col));
                Vertices.Add(new WidgetShaderVertex(cv7, col));
                Vertices.Add(new WidgetShaderVertex(cv7, col));
                Vertices.Add(new WidgetShaderVertex(cv6, col));
                Vertices.Add(new WidgetShaderVertex(cv8, col));
                Vertices.Add(new WidgetShaderVertex(cv1, col));
                Vertices.Add(new WidgetShaderVertex(cv5, col));
                Vertices.Add(new WidgetShaderVertex(cv3, col));
                Vertices.Add(new WidgetShaderVertex(cv3, col));
                Vertices.Add(new WidgetShaderVertex(cv5, col));
                Vertices.Add(new WidgetShaderVertex(cv7, col));
                Vertices.Add(new WidgetShaderVertex(cv2, col));
                Vertices.Add(new WidgetShaderVertex(cv6, col));
                Vertices.Add(new WidgetShaderVertex(cv4, col));
                Vertices.Add(new WidgetShaderVertex(cv4, col));
                Vertices.Add(new WidgetShaderVertex(cv6, col));
                Vertices.Add(new WidgetShaderVertex(cv8, col));


                //selection triangles
                if (selax == WidgetAxis.XYZ)
                {
                    //all axes - just draw inner triangle
                    Vertices.Add(new WidgetShaderVertex(Vector3.Zero, selplcol));
                    Vertices.Add(new WidgetShaderVertex(axes[i] * innertri, selplcol));
                    Vertices.Add(new WidgetShaderVertex(sides1[i] * innertri, selplcol));
                }
                else
                {
                    WidgetAxis sa = (WidgetAxis)(1 << i);
                    WidgetAxis na = sideax1[i];
                    if (((selax & sa) > 0) && ((selax & na) > 0))
                    {
                        Vertices.Add(new WidgetShaderVertex(axes[i] * innertri, selplcol));
                        Vertices.Add(new WidgetShaderVertex(sides1[i] * innertri, selplcol));
                        Vertices.Add(new WidgetShaderVertex(axes[i] * outertri, selplcol));
                        Vertices.Add(new WidgetShaderVertex(axes[i] * outertri, selplcol));
                        Vertices.Add(new WidgetShaderVertex(sides1[i] * innertri, selplcol));
                        Vertices.Add(new WidgetShaderVertex(sides1[i] * outertri, selplcol));
                    }
                }
            }

            Vertices.Update(context);
            Vertices.SetVSResource(context, 0);

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            context.Draw(Vertices.CurrentCount, 0);





            UnbindResources(context);
        }

    }


    public struct WidgetShaderVertex
    {
        public Vector4 Position;
        public Color4 Colour;

        public WidgetShaderVertex(Vector3 p, Color4 c)
        {
            Position = new Vector4(p, 0.0f);
            Colour = c;
        }
    }



}
