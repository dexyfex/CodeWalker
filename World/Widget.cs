using CodeWalker.Rendering;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.World
{
    public abstract class Widget
    {
        public bool Visible { get; set; } = true;

        public abstract void Update(Camera cam);

        public abstract void Render(DeviceContext context, Camera cam, WidgetShader shader);

        protected bool GetAxisRayHit(Vector3 ax1, Vector3 ax2, Vector3 camrel, Ray ray, out Vector3 pos)
        {
            //helper method for double sided ray/plane intersection
            Vector3 pn = Vector3.Cross(ax1, ax2);
            Plane p = new Plane(camrel, pn);
            if (ray.Intersects(ref p, out pos))
            {
                return true;
            }
            else
            {
                p = new Plane(camrel, -pn);
                if (ray.Intersects(ref p, out pos))
                {
                    return true;
                }
            }
            pos = Vector3.Zero;
            return false;
        }

        protected int GetAxisIndex(WidgetAxis axis)
        {
            switch (axis)
            {
                default:
                case WidgetAxis.X: return 0;
                case WidgetAxis.Y: return 1;
                case WidgetAxis.Z: return 2;
            }
        }

        protected float GetWorldSize(float pxsize, float dist, Camera cam)
        {
            float sssize = pxsize / cam.Height;
            float size = sssize * dist;
            if (cam.IsMapView || cam.IsOrthographic)
            {
                size = sssize * cam.OrthographicSize;
            }
            return size;
        }
    }

    public class TransformWidget : Widget
    {
        public DefaultWidget DefaultWidget { get; set; } = new DefaultWidget();
        public PositionWidget PositionWidget { get; set; } = new PositionWidget();
        public RotationWidget RotationWidget { get; set; } = new RotationWidget();
        public ScaleWidget ScaleWidget { get; set; } = new ScaleWidget();

        public bool ObjectSpace
        {
            get { return PositionWidget.ObjectSpace; }
            set
            {
                DefaultWidget.ObjectSpace = value;
                PositionWidget.ObjectSpace = value;
                RotationWidget.ObjectSpace = value;
            }
        }
        public float SnapAngleDegrees
        {
            get { return RotationWidget.SnapAngleDegrees; }
            set { RotationWidget.SnapAngleDegrees = value; }
        }
        public Vector3 Position
        {
            get { return PositionWidget.Position; }
            set
            {
                PositionWidget.Position = value;
                RotationWidget.Position = value;
                ScaleWidget.Position = value;
                DefaultWidget.Position = value;
            }
        }
        public Quaternion Rotation
        {
            get { return RotationWidget.Rotation; }
            set
            {
                PositionWidget.Rotation = value;
                RotationWidget.Rotation = value;
                ScaleWidget.Rotation = value;
                DefaultWidget.Rotation = value;
            }
        }
        public Vector3 Scale
        {
            get { return ScaleWidget.Scale; }
            set { ScaleWidget.Scale = value; }
        }
        public WidgetMode Mode { get; set; } = WidgetMode.Default;


        public event WidgetPositionChangeHandler OnPositionChange;
        public event WidgetRotationChangeHandler OnRotationChange;
        public event WidgetScaleChangeHandler OnScaleChange;

        public bool IsUnderMouse
        {
            get
            {
                switch (Mode)
                {
                    default:
                    case WidgetMode.Default: return false;
                    case WidgetMode.Position: return (PositionWidget.MousedAxis != WidgetAxis.None);
                    case WidgetMode.Rotation: return (RotationWidget.MousedAxis != WidgetAxis.None);
                    case WidgetMode.Scale: return (ScaleWidget.MousedAxis != WidgetAxis.None);
                }
            }
        }
        public bool IsDragging
        {
            get
            {
                switch (Mode)
                {
                    default:
                    case WidgetMode.Default: return false;
                    case WidgetMode.Position: return PositionWidget.IsDragging;
                    case WidgetMode.Rotation: return RotationWidget.IsDragging;
                    case WidgetMode.Scale: return ScaleWidget.IsDragging;
                }
            }
            set
            {
                switch (Mode)
                {
                    case WidgetMode.Position: PositionWidget.IsDragging = value; break;
                    case WidgetMode.Rotation: RotationWidget.IsDragging = value; break;
                    case WidgetMode.Scale: ScaleWidget.IsDragging = value; break;
                }
            }
        }


        public TransformWidget()
        {
            PositionWidget.OnPositionChange += PositionWidget_OnPositionChange;
            RotationWidget.OnRotationChange += RotationWidget_OnRotationChange;
            ScaleWidget.OnScaleChange += ScaleWidget_OnScaleChange;
        }

        private void PositionWidget_OnPositionChange(Vector3 newpos, Vector3 oldpos)
        {
            DefaultWidget.Position = newpos;
            RotationWidget.Position = newpos;
            ScaleWidget.Position = newpos;
            OnPositionChange?.Invoke(newpos, oldpos);
        }

        private void RotationWidget_OnRotationChange(Quaternion newrot, Quaternion oldrot)
        {
            DefaultWidget.Rotation = newrot;
            PositionWidget.Rotation = newrot;
            ScaleWidget.Rotation = newrot;
            OnRotationChange?.Invoke(newrot, oldrot);
        }

        private void ScaleWidget_OnScaleChange(Vector3 newscale, Vector3 oldscale)
        {
            OnScaleChange?.Invoke(newscale, oldscale);
        }

        public override void Update(Camera cam)
        {
            if (!Visible) return;
            switch (Mode)
            {
                case WidgetMode.Position:
                    PositionWidget.Update(cam);
                    break;
                case WidgetMode.Rotation:
                    RotationWidget.Update(cam);
                    break;
                case WidgetMode.Scale:
                    ScaleWidget.Update(cam);
                    break;
                case WidgetMode.Default:
                    DefaultWidget.Update(cam);
                    break;
            }
        }

        public override void Render(DeviceContext context, Camera cam, WidgetShader shader)
        {
            if (!Visible) return;
            switch (Mode)
            {
                case WidgetMode.Position:
                    PositionWidget.Render(context, cam, shader);
                    break;
                case WidgetMode.Rotation:
                    RotationWidget.Render(context, cam, shader);
                    break;
                case WidgetMode.Scale:
                    ScaleWidget.Render(context, cam, shader);
                    break;
                case WidgetMode.Default:
                    DefaultWidget.Render(context, cam, shader);
                    break;
            }
        }


    }

    public class DefaultWidget : Widget
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.Identity;

        public float Size { get; set; } = 70.0f;

        public bool ObjectSpace { get; set; } = true;


        public override void Render(DeviceContext context, Camera cam, WidgetShader shader)
        {
            if (!Visible) return;

            Vector3 camrel = Position - cam.Position;
            float dist = camrel.Length();
            float size = GetWorldSize(Size, dist, cam);

            var ori = ObjectSpace ? Rotation : Quaternion.Identity;

            shader.DrawDefaultWidget(context, cam, camrel, ori, size);
        }

        public override void Update(Camera cam)
        {
            //nothing to update here...
        }
    }

    public class PositionWidget : Widget
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public event WidgetPositionChangeHandler OnPositionChange;

        public WidgetAxis MousedAxis { get; set; } = WidgetAxis.None;
        public WidgetAxis DraggedAxis { get; set; } = WidgetAxis.None;
        public Vector3 DraggedAxisDir { get; set; }
        public Vector3 DraggedAxisSideDir { get; set; }
        public Vector3 DragStartPosition { get; set; }
        public bool IsDragging { get; set; }
        public bool WasDragging = false;
        public Vector3 DragStartVec; //projected onto the plane/axis being dragged

        public float Size { get; set; } = 90.0f;

        public bool ObjectSpace { get; set; } = true;


        public PositionWidget()
        {
        }

        public override void Render(DeviceContext context, Camera cam, WidgetShader shader)
        {
            if (!Visible) return;

            Vector3 camrel = Position - cam.Position;
            float dist = camrel.Length();
            float size = GetWorldSize(Size, dist, cam);

            WidgetAxis ax = IsDragging ? DraggedAxis : MousedAxis;

            var ori = ObjectSpace ? Rotation : Quaternion.Identity;

            shader.DrawPositionWidget(context, cam, camrel, ori, size, ax);

        }

        public override void Update(Camera cam)
        {
            if (!Visible) return;

            var ori = ObjectSpace ? Rotation : Quaternion.Identity;

            Vector3 xdir = Vector3.UnitX;
            Vector3 ydir = Vector3.UnitY;
            Vector3 zdir = Vector3.UnitZ;
            Vector3[] axes = { xdir, ydir, zdir };
            Vector3[] sides1 = { ydir, zdir, xdir };
            Vector3[] sides2 = { zdir, xdir, ydir };
            WidgetAxis[] sideax1 = { WidgetAxis.Y, WidgetAxis.Z, WidgetAxis.X };
            WidgetAxis[] sideax2 = { WidgetAxis.Z, WidgetAxis.X, WidgetAxis.Y };



            Quaternion iori = Quaternion.Invert(ori);
            Vector3 camrel = iori.Multiply(Position - cam.Position);
            Vector3 cdir = Vector3.Normalize(camrel);
            Ray ray = cam.MouseRay;
            ray.Position = iori.Multiply(ray.Position);
            ray.Direction = iori.Multiply(ray.Direction);


            float dist = camrel.Length();
            float size = GetWorldSize(Size, dist, cam);
            float linestart = 0.2f * size;
            float lineend = 1.0f * size;
            float sideval = 0.4f * size;
            float arrowstart = 1.0f * size;
            float arrowend = 1.33f * size;
            float arrowrad = 0.06f * size;
            float axhitrad = 0.07f * size;
            float axhitstart = 0.2f * size;
            float axhitend = 1.33f * size;
            float sidehitend = 0.5f * size;
            float sidehitstart = 0.25f * size;
            float allhitrad = 0.07f * size;

            //test for single and double axes hits
            BoundingBox bb = new BoundingBox();
            BoundingBox bb2 = new BoundingBox();
            float hitd = float.MaxValue;
            float d, d2;
            WidgetAxis hitax = WidgetAxis.None;
            for (int i = 0; i < 3; i++)
            {
                WidgetAxis ax = (WidgetAxis)(1 << i);
                Vector3 s = sides1[i] * axhitrad + sides2[i] * axhitrad;
                bb.Minimum = camrel - s + axes[i] * axhitstart;
                bb.Maximum = camrel + s + axes[i] * axhitend;
                if (ray.Intersects(ref bb, out d)) //single axis
                {
                    if (d < hitd)
                    {
                        hitd = d;
                        hitax = ax;
                    }
                }
                for (int n = i + 1; n < 3; n++)
                {
                    //double axis hit test - don't hit if in the central area (L shape hit area)
                    WidgetAxis ax2 = (WidgetAxis)(1 << n);
                    bb.Minimum = camrel;
                    bb.Maximum = camrel + axes[i] * sidehitend + axes[n] * sidehitend;
                    bb2.Minimum = camrel;
                    bb2.Maximum = camrel + axes[i] * sidehitstart + axes[n] * sidehitstart;
                    if (ray.Intersects(ref bb, out d) && !ray.Intersects(ref bb2, out d2))
                    {
                        if (d < hitd)
                        {
                            hitd = d;
                            hitax = ax | ax2;
                        }
                    }
                }
            }

            //small box at the center for all axes hit.
            Vector3 ss = (axes[0] + axes[1] + axes[2]) * allhitrad;
            bb.Minimum = camrel - ss;
            bb.Maximum = camrel + ss;
            if (ray.Intersects(ref bb, out d))
            {
                if (d < hitd)
                {
                    hitd = d;
                    hitax = WidgetAxis.XYZ;
                }
            }

            MousedAxis = hitax;


            if (IsDragging && !WasDragging)
            {
                //drag start. mark the start vector and axes
                DraggedAxis = MousedAxis;
                DragStartPosition = Position;
                DraggedAxisDir = axes[0];
                DraggedAxisSideDir = axes[1];

                switch (DraggedAxis)
                {
                    case WidgetAxis.XZ: DraggedAxisSideDir = axes[2]; break;
                    case WidgetAxis.YZ: DraggedAxisDir = axes[1]; DraggedAxisSideDir = axes[2]; break;
                    case WidgetAxis.Y: DraggedAxisDir = axes[1]; break;
                    case WidgetAxis.Z: DraggedAxisDir = axes[2]; break;
                }
                switch (DraggedAxis) //find the best second axis to use, for single axis motion only.
                {
                    case WidgetAxis.X:
                    case WidgetAxis.Y:
                    case WidgetAxis.Z:
                        int curax = GetAxisIndex(DraggedAxis);
                        int minax = 0;
                        float mindp = float.MaxValue;
                        for (int i = 0; i < 3; i++)
                        {
                            if (i != curax)
                            {
                                float dp = Math.Abs(Vector3.Dot(cdir, axes[i]));
                                if (dp < mindp)
                                {
                                    mindp = dp;
                                    minax = i;
                                }
                            }
                        }
                        DraggedAxisSideDir = axes[minax];
                        break;
                }
                if (DraggedAxis == WidgetAxis.XYZ)
                {
                    //all axes, move in the screen plane
                    float ad1 = Math.Abs(Vector3.Dot(cdir, Vector3.UnitY));
                    float ad2 = Math.Abs(Vector3.Dot(cdir, Vector3.UnitZ));
                    DraggedAxisDir = Vector3.Normalize(Vector3.Cross(cdir, (ad1 > ad2) ? Vector3.UnitY : Vector3.UnitZ));
                    DraggedAxisSideDir = Vector3.Normalize(Vector3.Cross(cdir, DraggedAxisDir));
                }


                bool hit = GetAxisRayHit(DraggedAxisDir, DraggedAxisSideDir, camrel, ray, out DragStartVec);
                if ((MousedAxis == WidgetAxis.None) || !hit)
                {
                    IsDragging = false;
                }
            }
            else if (IsDragging)
            {
                //continue drag.
                Vector3 newvec;
                bool hit = GetAxisRayHit(DraggedAxisDir, DraggedAxisSideDir, camrel, ray, out newvec);
                if (hit)
                {
                    Vector3 diff = newvec - DragStartVec;
                    switch (DraggedAxis)
                    {
                        case WidgetAxis.X: diff.Y = 0; diff.Z = 0; break;
                        case WidgetAxis.Y: diff.X = 0; diff.Z = 0; break;
                        case WidgetAxis.Z: diff.X = 0; diff.Y = 0; break;
                    }

                    if (ObjectSpace)
                    {
                        diff = ori.Multiply(diff);
                    }

                    if (diff.Length() < 10000.0f) //limit movement in one go, to avoid losing the widget...
                    {
                        Vector3 oldpos = Position;
                        Position = DragStartPosition + diff;
                        if (Position != oldpos)
                        {
                            OnPositionChange?.Invoke(Position, oldpos);
                        }
                    }
                }
            }



            WasDragging = IsDragging;
        }


    }

    public class RotationWidget : Widget
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public event WidgetRotationChangeHandler OnRotationChange;

        public WidgetAxis MousedAxis { get; set; } = WidgetAxis.None;
        public WidgetAxis DraggedAxis { get; set; } = WidgetAxis.None;
        public Vector3 DraggedAxisDir { get; set; }
        public Vector3 DraggedAxisSideDir1 { get; set; }
        public Vector3 DraggedAxisSideDir2 { get; set; }
        public Quaternion DragStartRotation { get; set; }
        public bool IsDragging { get; set; }
        private bool WasDragging = false;
        private Vector3 DragStartVec; //projected onto the plane/axis being dragged

        public float Size { get; set; } = 100.0f;

        public bool ObjectSpace { get; set; } = true;
        public WidgetAxis EnableAxes { get; set; } = WidgetAxis.XYZ;
        public float SnapAngleDegrees { get; set; } = 5.0f;

        public RotationWidget()
        {
        }

        public override void Render(DeviceContext context, Camera cam, WidgetShader shader)
        {
            if (!Visible) return;

            Vector3 camrel = Position - cam.Position;
            float dist = camrel.Length();
            float size = GetWorldSize(Size, dist, cam);

            WidgetAxis ax = IsDragging ? DraggedAxis : MousedAxis;

            var ori = ObjectSpace ? Rotation : Quaternion.Identity;

            shader.DrawRotationWidget(context, cam, camrel, ori, size, ax, EnableAxes);
        }

        public override void Update(Camera cam)
        {
            if (!Visible) return;

            var ori = ObjectSpace ? Rotation : Quaternion.Identity;
            Vector3 xdir = ori.Multiply(Vector3.UnitX);
            Vector3 ydir = ori.Multiply(Vector3.UnitY);
            Vector3 zdir = ori.Multiply(Vector3.UnitZ);
            Vector3[] axes = { xdir, ydir, zdir };
            Vector3[] sides1 = { ydir, zdir, xdir };
            Vector3[] sides2 = { zdir, xdir, ydir };

            Ray ray = cam.MouseRay;
            Vector3 camrel = Position - cam.Position;
            float dist = camrel.Length();
            float size = GetWorldSize(Size, dist, cam);
            float ocircsize = 1.0f * size; //outer ring radius
            float icircsize = 0.75f * size; //inner ring radius
            float icircthick = 0.2f * size; //inner ring hit width
            float ocircthick = 0.13f * size;//outer ring hit width
            float icirchiti = icircsize - icircthick;
            float icirchito = icircsize + icircthick;
            float ocirchiti = ocircsize - ocircthick;
            float ocirchito = ocircsize + ocircthick;


            //test for the main axes hits
            float cullvalue = -0.18f;
            float hitd = float.MaxValue;
            Vector3 hitp = camrel;
            Vector3 hitrel = Vector3.Zero;
            WidgetAxis hitax = WidgetAxis.None;
            Vector3 hitaxd = Vector3.UnitX;
            Vector3 hitax1 = Vector3.UnitY;
            Vector3 hitax2 = Vector3.UnitZ;
            for (int i = 0; i < 3; i++)
            {
                WidgetAxis ax = (WidgetAxis)(1 << i);
                if ((ax & EnableAxes) == 0) continue;
                Vector3 s1 = sides1[i];
                Vector3 s2 = sides2[i];
                if (GetAxisRayHit(s1, s2, camrel, ray, out hitp))
                {
                    float hitdist = hitp.Length();
                    float hitreld = (camrel.Length() - hitdist) / size;
                    if (hitreld < cullvalue) continue; //this hit was at the backside of the widget; ignore
                    Vector3 thitrel = hitp - camrel;
                    float hitrad = thitrel.Length();
                    if ((hitrad > icirchiti) && (hitrad < icirchito) && (hitdist < hitd))
                    {
                        hitd = hitdist;
                        hitax = ax;
                        hitax1 = s1;
                        hitax2 = s2;
                        hitrel = thitrel;
                        hitaxd = axes[i];
                    }
                }
            }

            //test for the outer ring hit
            if ((hitax == WidgetAxis.None) && (EnableAxes == WidgetAxis.XYZ))
            {
                Vector3 sdir = Vector3.Normalize(camrel);
                //if (cam.IsMapView || cam.IsOrthographic)
                //{
                //    sdir = cam.ViewDirection;
                //}
                float ad1 = Math.Abs(Vector3.Dot(sdir, Vector3.UnitY));
                float ad2 = Math.Abs(Vector3.Dot(sdir, Vector3.UnitZ));
                Vector3 ax1 = Vector3.Normalize(Vector3.Cross(sdir, (ad1 > ad2) ? Vector3.UnitY : Vector3.UnitZ));
                Vector3 ax2 = Vector3.Normalize(Vector3.Cross(sdir, ax1));
                if (GetAxisRayHit(ax1, ax2, camrel, ray, out hitp))
                {
                    Vector3 thitrel = hitp - camrel;
                    float hitrad = thitrel.Length();
                    if ((hitrad > ocirchiti) && (hitrad < ocirchito))
                    {
                        hitax = WidgetAxis.XYZ;
                        hitax1 = ax1;
                        hitax2 = ax2;
                        hitrel = thitrel;
                        hitaxd = sdir;
                    }
                }
            }


            MousedAxis = hitax;


            if (IsDragging && !WasDragging)
            {
                //drag start. mark the start vector and axes
                DraggedAxis = MousedAxis;
                DragStartRotation = Rotation;
                DraggedAxisDir = hitaxd;
                DraggedAxisSideDir1 = hitax1;
                DraggedAxisSideDir2 = hitax2;

                bool hit = GetAxisRayHit(DraggedAxisSideDir1, DraggedAxisSideDir2, camrel, ray, out DragStartVec);
                if ((MousedAxis == WidgetAxis.None) || !hit)
                {
                    IsDragging = false;
                }
            }
            else if (IsDragging)
            {
                //continue drag.
                Vector3 newvec;
                bool hit = GetAxisRayHit(DraggedAxisSideDir1, DraggedAxisSideDir2, camrel, ray, out newvec);
                if (hit)
                {
                    Vector3 diff = newvec - DragStartVec;
                    if (diff.Length() < 10000.0f) //put some limit to the plane intersection...
                    {
                        Vector3 nv = Vector3.Normalize(newvec - camrel);
                        Vector3 ov = Vector3.Normalize(DragStartVec - camrel);
                        float na = AngleOnAxes(nv, DraggedAxisSideDir1, DraggedAxisSideDir2);
                        float oa = AngleOnAxes(ov, DraggedAxisSideDir1, DraggedAxisSideDir2);
                        float a = SnapAngle(na - oa);
                        Quaternion rot = Quaternion.RotationAxis(DraggedAxisDir, a);
                        Quaternion oldrot = Rotation;
                        Rotation = Quaternion.Normalize(Quaternion.Multiply(rot, DragStartRotation));
                        if (Rotation != oldrot)
                        {
                            OnRotationChange?.Invoke(Rotation, oldrot);
                        }
                    }
                }
            }


            WasDragging = IsDragging;
        }

        private float AngleOnAxes(Vector3 v, Vector3 ax1, Vector3 ax2)
        {
            float d1 = Vector3.Dot(v, ax1);
            float d2 = Vector3.Dot(v, ax2);
            return (float)Math.Atan2(d2, d1);
        }

        private float SnapAngle(float a)
        {
            if (SnapAngleDegrees <= 0.0f) return a;
            float snaprad = SnapAngleDegrees * 0.0174532925f;
            float ahalf = a + (snaprad * 0.5f);
            float mod = ahalf % snaprad;
            float snapped = ahalf - mod;
            return snapped;
        }
    }

    public class ScaleWidget : Widget
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;
        public event WidgetScaleChangeHandler OnScaleChange;

        public WidgetAxis MousedAxis { get; set; } = WidgetAxis.None;
        public WidgetAxis DraggedAxis { get; set; } = WidgetAxis.None;
        public Vector3 DraggedAxisDir { get; set; }
        public Vector3 DraggedAxisSideDir { get; set; }
        public Vector3 DragStartScale { get; set; }
        public bool IsDragging { get; set; }
        private bool WasDragging = false;
        private Vector3 DragStartVec; //projected onto the plane/axis being dragged

        public bool LockXY { get; set; } = true;

        public float Size { get; set; } = 90.0f;

        public ScaleWidget()
        {
        }

        public override void Render(DeviceContext context, Camera cam, WidgetShader shader)
        {
            if (!Visible) return;

            Vector3 camrel = Position - cam.Position;
            float dist = camrel.Length();
            float size = GetWorldSize(Size, dist, cam);

            WidgetAxis ax = IsDragging ? DraggedAxis : MousedAxis;

            var ori = Rotation; //scale is always in object space.

            shader.DrawScaleWidget(context, cam, camrel, ori, size, ax);
        }

        public override void Update(Camera cam)
        {
            if (!Visible) return;

            var ori = Rotation;// : Quaternion.Identity;

            Vector3 xdir = Vector3.UnitX;
            Vector3 ydir = Vector3.UnitY;
            Vector3 zdir = Vector3.UnitZ;
            Vector3[] axes = { xdir, ydir, zdir };
            Vector3[] sides1 = { ydir, zdir, xdir };
            Vector3[] sides2 = { zdir, xdir, ydir };
            WidgetAxis[] sideax1 = { WidgetAxis.Y, WidgetAxis.Z, WidgetAxis.X };
            WidgetAxis[] sideax2 = { WidgetAxis.Z, WidgetAxis.X, WidgetAxis.Y };



            Quaternion iori = Quaternion.Invert(ori);
            Vector3 camrel = iori.Multiply(Position - cam.Position);
            Vector3 cdir = Vector3.Normalize(camrel);
            Ray ray = cam.MouseRay;
            ray.Position = iori.Multiply(ray.Position);
            ray.Direction = iori.Multiply(ray.Direction);


            float dist = camrel.Length();
            float size = GetWorldSize(Size, dist, cam);

            float axhitrad = 0.09f * size;
            float axhitstart = 0.4f * size;
            float axhitend = 1.33f * size;
            float innertri = 0.7f * size;
            float outertri = 1.0f * size;

            //test for single and double axes hits
            BoundingBox bb = new BoundingBox();
            float hitd = float.MaxValue;
            float d;
            Vector3 hitp;
            WidgetAxis hitax = WidgetAxis.None;
            for (int i = 0; i < 3; i++)
            {
                WidgetAxis ax = (WidgetAxis)(1 << i);
                Vector3 s = sides1[i] * axhitrad + sides2[i] * axhitrad;
                bb.Minimum = camrel - s + axes[i] * axhitstart;
                bb.Maximum = camrel + s + axes[i] * axhitend;
                if (ray.Intersects(ref bb, out d)) //single axis
                {
                    if (d < hitd)
                    {
                        hitd = d;
                        hitax = ax;
                    }
                }

                Vector3 s1 = axes[i];
                Vector3 s2 = sides1[i];
                if (GetAxisRayHit(s1, s2, camrel, ray, out hitp))
                {
                    //test if hitp is within the inner triangle - uniform scale
                    //test if hitp is within the outer triangle - 2 axes scale
                    float hitpl = hitp.Length();
                    if (hitpl > hitd) continue;
                    Vector3 hitrel = hitp - camrel;
                    float d1 = Vector3.Dot(hitrel, s1);
                    float d2 = Vector3.Dot(hitrel, s2);

                    if ((d1 > 0) && (d2 > 0))
                    {
                        if ((d1 < innertri) && (d2 < innertri) && ((d1 + d2) < innertri))
                        {
                            hitd = hitpl;
                            hitax = WidgetAxis.XYZ;
                        }
                        else if ((d1 < outertri) && (d2 < outertri) && ((d1 + d2) < outertri))
                        {
                            hitd = hitpl;
                            hitax = ax | sideax1[i];
                        }
                    }
                }
            }
            if (LockXY)
            {
                switch (hitax)
                {
                    case WidgetAxis.X:
                    case WidgetAxis.Y:
                        hitax = WidgetAxis.XY;
                        break;
                    case WidgetAxis.XZ:
                    case WidgetAxis.YZ:
                        hitax = WidgetAxis.XYZ;
                        break;
                }
            }


            MousedAxis = hitax;


            if (IsDragging && !WasDragging)
            {
                //drag start. mark the start vector and axes
                DraggedAxis = MousedAxis;
                DragStartScale = Scale;
                DraggedAxisDir = axes[0];
                DraggedAxisSideDir = axes[1];

                switch (DraggedAxis)
                {
                    case WidgetAxis.XZ: DraggedAxisSideDir = axes[2]; break;
                    case WidgetAxis.YZ: DraggedAxisDir = axes[1]; DraggedAxisSideDir = axes[2]; break;
                    case WidgetAxis.Y: DraggedAxisDir = axes[1]; break;
                    case WidgetAxis.Z: DraggedAxisDir = axes[2]; break;
                }
                switch (DraggedAxis) //find the best second axis to use, for single axis motion only.
                {
                    case WidgetAxis.X:
                    case WidgetAxis.Y:
                    case WidgetAxis.Z:
                        int curax = GetAxisIndex(DraggedAxis);
                        int minax = 0;
                        float mindp = float.MaxValue;
                        for (int i = 0; i < 3; i++)
                        {
                            if (i != curax)
                            {
                                float dp = Math.Abs(Vector3.Dot(cdir, axes[i]));
                                if (dp < mindp)
                                {
                                    mindp = dp;
                                    minax = i;
                                }
                            }
                        }
                        DraggedAxisSideDir = axes[minax];
                        break;
                }
                if (DraggedAxis == WidgetAxis.XYZ)
                {
                    //all axes, move in the screen plane
                    float ad1 = Math.Abs(Vector3.Dot(cdir, Vector3.UnitY));
                    float ad2 = Math.Abs(Vector3.Dot(cdir, Vector3.UnitZ));
                    DraggedAxisDir = Vector3.Normalize(Vector3.Cross(cdir, (ad1 > ad2) ? Vector3.UnitY : Vector3.UnitZ));
                    DraggedAxisSideDir = Vector3.Normalize(Vector3.Cross(cdir, DraggedAxisDir));
                }


                bool hit = GetAxisRayHit(DraggedAxisDir, DraggedAxisSideDir, camrel, ray, out DragStartVec);
                if ((MousedAxis == WidgetAxis.None) || !hit)
                {
                    IsDragging = false;
                }
            }
            else if (IsDragging)
            {
                //continue drag.
                Vector3 newvec;
                bool hit = GetAxisRayHit(DraggedAxisDir, DraggedAxisSideDir, camrel, ray, out newvec);
                if (hit)
                {
                    Vector3 diff = newvec - DragStartVec;
                    switch (DraggedAxis)
                    {
                        case WidgetAxis.X: diff.Y = 0; diff.Z = 0; break;
                        case WidgetAxis.Y: diff.X = 0; diff.Z = 0; break;
                        case WidgetAxis.Z: diff.X = 0; diff.Y = 0; break;
                    }

                    //diff = ori.Multiply(diff);
                    Vector3 ods = DragStartVec - camrel;// ori.Multiply(DragStartVec);
                    float odl = Math.Max(ods.Length(), 0.0001f); //don't divide by 0
                    float ndl = Math.Max((ods + diff).Length(), 0.001f); //don't scale to 0 size
                    float dl = ndl / odl;

                    if (diff.Length() < 10000.0f) //limit movement in one go, to avoid crazy values...
                    {
                        Vector3 oldscale = Scale;
                        Vector3 sv = Vector3.One;
                        switch (DraggedAxis)
                        {
                            case WidgetAxis.X: sv = new Vector3(dl, 1, 1); break;
                            case WidgetAxis.Y: sv = new Vector3(1, dl, 1); break;
                            case WidgetAxis.Z: sv = new Vector3(1, 1, dl); break;
                            case WidgetAxis.XY: sv = new Vector3(dl, dl, 1); break;
                            case WidgetAxis.YZ: sv = new Vector3(1, dl, dl); break;
                            case WidgetAxis.XZ: sv = new Vector3(dl, 1, dl); break;
                            case WidgetAxis.XYZ: sv = new Vector3(dl); break;
                        }
                        Scale = DragStartScale * sv;

                        if (Scale != oldscale)
                        {
                            OnScaleChange?.Invoke(Scale, oldscale);
                        }
                    }
                }
            }

            WasDragging = IsDragging;
        }

    }


    public delegate void WidgetPositionChangeHandler(Vector3 newpos, Vector3 oldpos);
    public delegate void WidgetRotationChangeHandler(Quaternion newrot, Quaternion oldrot);
    public delegate void WidgetScaleChangeHandler(Vector3 newscale, Vector3 oldscale);

    public enum WidgetMode
    {
        Default = 0,
        Position = 1,
        Rotation = 2,
        Scale = 3,
    }
    public enum WidgetAxis
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
        XY = 3,
        XZ = 5,
        YZ = 6,
        XYZ = 7,
    }






}
