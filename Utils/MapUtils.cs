using System;
using System.IO;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using CodeWalker.Utils;
using CodeWalker.World;
using CodeWalker.GameFiles;

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








    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct MapSelection
    {
        public YmapEntityDef EntityDef { get; set; }
        public Archetype Archetype { get; set; }
        public DrawableBase Drawable { get; set; }
        public DrawableGeometry Geometry { get; set; }
        public MetaWrapper EntityExtension { get; set; }
        public MetaWrapper ArchetypeExtension { get; set; }
        public YmapTimeCycleModifier TimeCycleModifier { get; set; }
        public YmapCarGen CarGenerator { get; set; }
        public YmapGrassInstanceBatch GrassBatch { get; set; }
        public YmapDistantLODLights DistantLodLights { get; set; }
        public YmapEntityDef MloEntityDef { get; set; }
        public WaterQuad WaterQuad { get; set; }
        public Bounds CollisionBounds { get; set; }
        public YnvPoly NavPoly { get; set; }
        public YndNode PathNode { get; set; }
        public YndLink PathLink { get; set; }
        public TrainTrackNode TrainTrackNode { get; set; }
        public ScenarioNode ScenarioNode { get; set; }
        public MCScenarioChainingEdge ScenarioEdge { get; set; }
        public AudioPlacement Audio { get; set; }

        public bool MultipleSelection { get; set; }
        public Vector3 MultipleSelectionCenter { get; set; }

        public BoundingBox AABB { get; set; }
        public BoundingSphere BSphere { get; set; }
        public int GeometryIndex { get; set; }
        public Vector3 CamRel { get; set; }
        public float HitDist { get; set; }


        public bool HasValue
        {
            get
            {
                return (EntityDef != null) ||
                    (Archetype != null) ||
                    (Drawable != null) ||
                    (Geometry != null) ||
                    (EntityExtension != null) ||
                    (ArchetypeExtension != null) ||
                    (TimeCycleModifier != null) ||
                    (CarGenerator != null) ||
                    (GrassBatch != null) ||
                    (WaterQuad != null) ||
                    (CollisionBounds != null) ||
                    (NavPoly != null) ||
                    (PathNode != null) ||
                    (TrainTrackNode != null) ||
                    (DistantLodLights != null) ||
                    (MloEntityDef != null) ||
                    (ScenarioNode != null) ||
                    (Audio != null);
            }
        }

        public bool HasHit
        {
            get { return (HitDist != float.MaxValue); }
        }


        public bool CheckForChanges(MapSelection mhit)
        {
            return (EntityDef != mhit.EntityDef)
                || (Archetype != mhit.Archetype)
                || (Drawable != mhit.Drawable)
                || (TimeCycleModifier != mhit.TimeCycleModifier)
                || (ArchetypeExtension != mhit.ArchetypeExtension)
                || (EntityExtension != mhit.EntityExtension)
                || (CarGenerator != mhit.CarGenerator)
                || (MloEntityDef != mhit.MloEntityDef)
                || (DistantLodLights != mhit.DistantLodLights)
                || (GrassBatch != mhit.GrassBatch)
                || (WaterQuad != mhit.WaterQuad)
                || (CollisionBounds != mhit.CollisionBounds)
                || (NavPoly != mhit.NavPoly)
                || (PathNode != mhit.PathNode)
                || (TrainTrackNode != mhit.TrainTrackNode)
                || (ScenarioNode != mhit.ScenarioNode)
                || (Audio != mhit.Audio);
        }
        public bool CheckForChanges()
        {
            return (EntityDef != null)
                || (Archetype != null)
                || (Drawable != null)
                || (TimeCycleModifier != null)
                || (ArchetypeExtension != null)
                || (EntityExtension != null)
                || (CarGenerator != null)
                || (MloEntityDef != null)
                || (DistantLodLights != null)
                || (GrassBatch != null)
                || (WaterQuad != null)
                || (CollisionBounds != null)
                || (NavPoly != null)
                || (PathNode != null)
                || (PathLink != null)
                || (TrainTrackNode != null)
                || (ScenarioNode != null)
                || (Audio != null);
        }


        public void Clear()
        {
            EntityDef = null;
            Archetype = null;
            Drawable = null;
            Geometry = null;
            EntityExtension = null;
            ArchetypeExtension = null;
            TimeCycleModifier = null;
            CarGenerator = null;
            GrassBatch = null;
            WaterQuad = null;
            CollisionBounds = null;
            NavPoly = null;
            PathNode = null;
            PathLink = null;
            TrainTrackNode = null;
            DistantLodLights = null;
            MloEntityDef = null;
            ScenarioNode = null;
            ScenarioEdge = null;
            Audio = null;
            MultipleSelection = false;
            AABB = new BoundingBox();
            GeometryIndex = 0;
            CamRel = new Vector3();
            HitDist = float.MaxValue;
        }

        public string GetNameString(string defval)
        {
            string name = defval;
            if (MultipleSelection)
            {
                name = "Multiple items";
            }
            else if (EntityDef != null)
            {
                name = EntityDef.CEntityDef.archetypeName.ToString();
            }
            else if (Archetype != null)
            {
                name = Archetype.Hash.ToString();
            }
            else if (TimeCycleModifier != null)
            {
                name = TimeCycleModifier.CTimeCycleModifier.name.ToString();
            }
            else if (CarGenerator != null)
            {
                name = CarGenerator.CCarGen.carModel.ToString();
            }
            else if (DistantLodLights != null)
            {
                name = DistantLodLights.Ymap?.Name ?? "";
            }
            else if (CollisionBounds != null)
            {
                name = CollisionBounds.GetName();
            }
            if (EntityExtension != null)
            {
                name = EntityExtension.Name;
            }
            if (ArchetypeExtension != null)
            {
                name = ArchetypeExtension.Name;
            }
            if (WaterQuad != null)
            {
                name = "WaterQuad " + WaterQuad.ToString();
            }
            if (NavPoly != null)
            {
                name = "NavPoly " + NavPoly.ToString();
            }
            if (PathNode != null)
            {
                name = "PathNode " + PathNode.AreaID.ToString() + "." + PathNode.NodeID.ToString(); //+ FloatUtil.GetVector3String(PathNode.Position);
            }
            if (TrainTrackNode != null)
            {
                name = "TrainTrackNode " + FloatUtil.GetVector3String(TrainTrackNode.Position);
            }
            if (ScenarioNode != null)
            {
                name = ScenarioNode.ToString();
            }
            if (Audio != null)
            {
                name = Audio.ShortTypeName + " " + Audio.GetNameString();// FloatUtil.GetVector3String(Audio.InnerPos);
            }
            return name;
        }

        public string GetFullNameString(string defval)
        {
            string name = defval;
            if (MultipleSelection)
            {
                name = "Multiple items";
            }
            else if (EntityDef != null)
            {
                name = EntityDef.CEntityDef.archetypeName.ToString();
            }
            else if (Archetype != null)
            {
                name = Archetype.Hash.ToString();
            }
            else if (CollisionBounds != null)
            {
                name = CollisionBounds.GetName();
            }
            if (Geometry != null)
            {
                name += " (" + GeometryIndex.ToString() + ")";
            }
            if (TimeCycleModifier != null)
            {
                name = TimeCycleModifier.CTimeCycleModifier.name.ToString();
            }
            if (CarGenerator != null)
            {
                name = CarGenerator.NameString();
            }
            if (EntityExtension != null)
            {
                name += ": " + EntityExtension.Name;
            }
            if (ArchetypeExtension != null)
            {
                name += ": " + ArchetypeExtension.Name;
            }
            if (WaterQuad != null)
            {
                name = "WaterQuad " + WaterQuad.ToString();
            }
            if (NavPoly != null)
            {
                name = "NavPoly " + NavPoly.ToString();
            }
            if (PathNode != null)
            {
                name = "PathNode " + PathNode.AreaID.ToString() + "." + PathNode.NodeID.ToString();// + FloatUtil.GetVector3String(PathNode.Position);
            }
            if (TrainTrackNode != null)
            {
                name = "TrainTrackNode " + FloatUtil.GetVector3String(TrainTrackNode.Position);
            }
            if (ScenarioNode != null)
            {
                name = ScenarioNode.ToString();
            }
            if (Audio != null)
            {
                name = Audio.ShortTypeName + " " + Audio.GetNameString();//  + FloatUtil.GetVector3String(Audio.InnerPos);
            }
            return name;
        }



        public bool CanShowWidget
        {
            get
            {
                bool res = false;

                if (MultipleSelection)
                {
                    res = true;
                }
                else if (EntityDef != null)
                {
                    res = true;
                }
                else if (CarGenerator != null)
                {
                    res = true;
                }
                else if (NavPoly != null)
                {
                    res = true;
                }
                else if (PathNode != null)
                {
                    res = true;
                }
                else if (TrainTrackNode != null)
                {
                    res = true;
                }
                else if (ScenarioNode != null)
                {
                    res = true;
                }
                else if (Audio != null)
                {
                    res = true;
                }
                return res;
            }
        }
        public Vector3 WidgetPosition
        {
            get
            {
                if (MultipleSelection)
                {
                    return MultipleSelectionCenter;
                }
                else if (EntityDef != null)
                {
                    return EntityDef.WidgetPosition;
                }
                else if (CarGenerator != null)
                {
                    return CarGenerator.Position;
                }
                else if (NavPoly != null)
                {
                    return NavPoly.Position;
                }
                else if (PathNode != null)
                {
                    return PathNode.Position;
                }
                else if (TrainTrackNode != null)
                {
                    return TrainTrackNode.Position;
                }
                else if (ScenarioNode != null)
                {
                    return ScenarioNode.Position;
                }
                else if (Audio != null)
                {
                    return Audio.InnerPos;
                }
                return Vector3.Zero;
            }
        }
        public Quaternion WidgetRotation
        {
            get
            {
                if (MultipleSelection)
                {
                    return Quaternion.Identity;
                }
                else if (EntityDef != null)
                {
                    return EntityDef.WidgetOrientation;
                }
                else if (CarGenerator != null)
                {
                    return CarGenerator.Orientation;
                }
                else if (NavPoly != null)
                {
                    return Quaternion.Identity;
                }
                else if (PathNode != null)
                {
                    return Quaternion.Identity;
                }
                else if (TrainTrackNode != null)
                {
                    return Quaternion.Identity;
                }
                else if (ScenarioNode != null)
                {
                    return ScenarioNode.Orientation;
                }
                else if (Audio != null)
                {
                    return Audio.Orientation;
                }
                return Quaternion.Identity;
            }
        }
        public WidgetAxis WidgetRotationAxes
        {
            get
            {
                if (MultipleSelection)
                {
                    return WidgetAxis.XYZ;
                }
                else if (EntityDef != null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (CarGenerator != null)
                {
                    return WidgetAxis.Z;
                }
                else if (NavPoly != null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (PathNode != null)
                {
                    return WidgetAxis.None;
                }
                else if (TrainTrackNode != null)
                {
                    return WidgetAxis.None;
                }
                else if (ScenarioNode != null)
                {
                    return WidgetAxis.Z;
                }
                else if (Audio != null)
                {
                    return WidgetAxis.XYZ;
                }
                return WidgetAxis.None;
            }
        }
        public Vector3 WidgetScale
        {
            get
            {
                if (MultipleSelection)
                {
                    return Vector3.One;
                }
                else if (EntityDef != null)
                {
                    return EntityDef.Scale;
                }
                else if (CarGenerator != null)
                {
                    return new Vector3(CarGenerator.CCarGen.perpendicularLength);
                }
                else if (NavPoly != null)
                {
                    return Vector3.One;
                }
                else if (PathNode != null)
                {
                    return Vector3.One;
                }
                else if (TrainTrackNode != null)
                {
                    return Vector3.One;
                }
                else if (ScenarioNode != null)
                {
                    return Vector3.One;
                }
                else if (Audio != null)
                {
                    return Vector3.One;
                }
                return Vector3.One;
            }
        }




        public void SetPosition(Vector3 newpos, bool editPivot)
        {
            if (MultipleSelection)
            {
                //don't do anything here for multiselection
            }
            else if (EntityDef != null)
            {
                if (editPivot)
                {
                    EntityDef.SetPivotPositionFromWidget(newpos);
                }
                else
                {
                    EntityDef.SetPositionFromWidget(newpos);
                }
            }
            else if (CarGenerator != null)
            {
                CarGenerator.SetPosition(newpos);
            }
            else if (PathNode != null)
            {
                PathNode.SetPosition(newpos);
            }
            else if (NavPoly != null)
            {
                //NavPoly.SetPosition(newpos);

                //if (projectForm != null)
                //{
                //    projectForm.OnWorldNavPolyModified(NavPoly);
                //}
            }
            else if (TrainTrackNode != null)
            {
                TrainTrackNode.SetPosition(newpos);
            }
            else if (ScenarioNode != null)
            {
                ScenarioNode.SetPosition(newpos);
            }
            else if (Audio != null)
            {
                Audio.SetPosition(newpos);
            }

        }
        public void SetRotation(Quaternion newrot, Quaternion oldrot, bool editPivot)
        {
            if (EntityDef != null)
            {
                if (editPivot)
                {
                    EntityDef.SetPivotOrientationFromWidget(newrot);
                }
                else
                {
                    EntityDef.SetOrientationFromWidget(newrot);
                }
            }
            else if (CarGenerator != null)
            {
                CarGenerator.SetOrientation(newrot);
            }
            else if (ScenarioNode != null)
            {
                ScenarioNode.SetOrientation(newrot);
            }
            else if (Audio != null)
            {
                Audio.SetOrientation(newrot);
            }
        }
        public void SetScale(Vector3 newscale, Vector3 oldscale, bool editPivot)
        {
            if (EntityDef != null)
            {
                EntityDef.SetScale(newscale);
            }
            else if (CarGenerator != null)
            {
                CarGenerator.SetScale(newscale);
                AABB = new BoundingBox(CarGenerator.BBMin, CarGenerator.BBMax);
            }
        }


        public override string ToString()
        {
            return GetFullNameString("[Empty]");
        }
    }

    public enum MapSelectionMode
    {
        None = 0,
        Entity = 1,
        EntityExtension = 2,
        ArchetypeExtension = 3,
        TimeCycleModifier = 4,
        CarGenerator = 5,
        Grass = 6,
        WaterQuad = 7,
        Collision = 8,
        NavMesh = 9,
        Path = 10,
        TrainTrack = 11,
        DistantLodLights = 12,
        MloInstance = 13,
        Scenario = 14,
        PopZone = 15,
        Audio = 16,
    }







}