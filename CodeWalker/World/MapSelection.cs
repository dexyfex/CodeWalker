using CodeWalker.GameFiles;
using CodeWalker.Project;
using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker
{



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
        LodLights = 12,
        MloInstance = 13,
        Scenario = 14,
        PopZone = 15,
        Heightmap = 16,
        Watermap = 17,
        Audio = 18,
        Occlusion = 19,
        CalmingQuad = 20,
        WaveQuad = 21,
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
        public YmapLODLight LodLight { get; set; }
        public YmapBoxOccluder BoxOccluder { get; set; }
        public YmapOccludeModelTriangle OccludeModelTri { get; set; }
        public YmapEntityDef MloEntityDef { get; set; }
        public MCMloRoomDef MloRoomDef { get; set; }
        public WaterQuad WaterQuad { get; set; }
        public WaterCalmingQuad CalmingQuad { get; set; }
        public WaterWaveQuad WaveQuad { get; set; }
        public Bounds CollisionBounds { get; set; }
        public BoundPolygon CollisionPoly { get; set; }
        public BoundVertex CollisionVertex { get; set; }
        public YnvPoly NavPoly { get; set; }
        public YnvPoint NavPoint { get; set; }
        public YnvPortal NavPortal { get; set; }
        public YndNode PathNode { get; set; }
        public YndLink PathLink { get; set; }
        public TrainTrackNode TrainTrackNode { get; set; }
        public ScenarioNode ScenarioNode { get; set; }
        public MCScenarioChainingEdge ScenarioEdge { get; set; }
        public AudioPlacement Audio { get; set; }

        public MapSelection[] MultipleSelectionItems { get; private set; }
        public Vector3 MultipleSelectionCenter { get; set; }
        public Quaternion MultipleSelectionRotation { get; set; }
        public Vector3 MultipleSelectionScale { get; set; }
        public BoundVertex[] GatheredCollisionVerts { get; private set; } //for collision polys, need to move all the individual vertices instead

        public Vector3 BBOffset { get; set; }
        public Quaternion BBOrientation { get; set; }
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
                    (CollisionPoly != null) ||
                    (CollisionVertex != null) ||
                    (NavPoly != null) ||
                    (NavPoint != null) ||
                    (NavPortal != null) ||
                    (PathNode != null) ||
                    (TrainTrackNode != null) ||
                    (LodLight != null) ||
                    (BoxOccluder != null) ||
                    (OccludeModelTri != null) ||
                    (MloEntityDef != null) ||
                    (ScenarioNode != null) ||
                    (Audio != null) ||
                    (MloRoomDef != null);
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
                || (LodLight != mhit.LodLight)
                || (GrassBatch != mhit.GrassBatch)
                || (BoxOccluder != mhit.BoxOccluder)
                || (OccludeModelTri != mhit.OccludeModelTri)
                || (WaterQuad != mhit.WaterQuad)
                || (CollisionBounds != mhit.CollisionBounds)
                || (CollisionPoly != mhit.CollisionPoly)
                || (CollisionVertex != mhit.CollisionVertex)
                || (NavPoly != mhit.NavPoly)
                || (NavPoint != mhit.NavPoint)
                || (NavPortal != mhit.NavPortal)
                || (PathNode != mhit.PathNode)
                || (TrainTrackNode != mhit.TrainTrackNode)
                || (ScenarioNode != mhit.ScenarioNode)
                || (Audio != mhit.Audio)
                || (MloRoomDef != mhit.MloRoomDef);
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
                || (LodLight != null)
                || (GrassBatch != null)
                || (BoxOccluder != null)
                || (OccludeModelTri != null)
                || (WaterQuad != null)
                || (CollisionBounds != null)
                || (CollisionPoly != null)
                || (CollisionVertex != null)
                || (NavPoly != null)
                || (NavPoint != null)
                || (NavPortal != null)
                || (PathNode != null)
                || (PathLink != null)
                || (TrainTrackNode != null)
                || (ScenarioNode != null)
                || (Audio != null)
                || (MloRoomDef != null);
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
            BoxOccluder = null;
            OccludeModelTri = null;
            WaterQuad = null;
            CollisionBounds = null;
            CollisionPoly = null;
            CollisionVertex = null;
            NavPoly = null;
            NavPoint = null;
            NavPortal = null;
            PathNode = null;
            PathLink = null;
            TrainTrackNode = null;
            LodLight = null;
            MloEntityDef = null;
            ScenarioNode = null;
            ScenarioEdge = null;
            Audio = null;
            MultipleSelectionItems = null;
            MultipleSelectionCenter = Vector3.Zero;
            MultipleSelectionRotation = Quaternion.Identity;
            MultipleSelectionScale = Vector3.One;
            GatheredCollisionVerts = null;
            AABB = new BoundingBox();
            GeometryIndex = 0;
            CamRel = Vector3.Zero;
            HitDist = float.MaxValue;
        }

        public string GetNameString(string defval)
        {
            string name = defval;
            var ename = (EntityDef != null) ? EntityDef._CEntityDef.archetypeName.ToString() : string.Empty;
            var enamec = ename + ((!string.IsNullOrEmpty(ename)) ? ": " : "");
            if (MultipleSelectionItems != null)
            {
                name = "Multiple items";
            }
            else if (CollisionVertex != null)
            {
                name = enamec + "Vertex " + CollisionVertex.Index.ToString() + ((CollisionBounds != null) ? (": " + CollisionBounds.GetName()) : string.Empty);
            }
            else if (CollisionPoly != null)
            {
                name = enamec + "Poly " + CollisionPoly.Index.ToString() + ((CollisionBounds != null) ? (": " + CollisionBounds.GetName()) : string.Empty);
            }
            else if (CollisionBounds != null)
            {
                name = enamec + CollisionBounds.GetName();
            }
            else if (EntityDef != null)
            {
                name = ename;
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
                name = (CarGenerator.Ymap?.Name ?? "") + ": " + CarGenerator.NameString();
            }
            else if (GrassBatch != null)
            {
                name = (GrassBatch.Ymap?.Name ?? "") + ": " + GrassBatch.Archetype?.Name ?? "";
            }
            else if (LodLight != null)
            {
                name = (LodLight.Ymap?.Name ?? "") + ": " + LodLight.Index.ToString();
            }
            else if (BoxOccluder != null)
            {
                name = "BoxOccluder " + (BoxOccluder.Ymap?.Name ?? "") + ": " + BoxOccluder.Index.ToString();
            }
            else if (OccludeModelTri != null)
            {
                name = "OccludeModel " + (OccludeModelTri.Ymap?.Name ?? "") + ": " + (OccludeModelTri.Model?.Index??0).ToString() + ":" + OccludeModelTri.Index.ToString();
            }
            else if (WaterQuad != null)
            {
                name = "WaterQuad " + WaterQuad.ToString();
            }
            else if (NavPoly != null)
            {
                name = "NavPoly " + NavPoly.ToString();
            }
            else if (NavPoint != null)
            {
                name = "NavPoint " + NavPoint.ToString();
            }
            else if (NavPortal != null)
            {
                name = "NavPortal " + NavPortal.ToString();
            }
            else if (PathNode != null)
            {
                name = "PathNode " + PathNode.AreaID.ToString() + "." + PathNode.NodeID.ToString(); //+ FloatUtil.GetVector3String(PathNode.Position);
            }
            else if (TrainTrackNode != null)
            {
                name = "TrainTrackNode " + FloatUtil.GetVector3String(TrainTrackNode.Position);
            }
            else if (ScenarioNode != null)
            {
                name = ScenarioNode.ToString();
            }
            else if (Audio != null)
            {
                name = Audio.ShortTypeName + " " + Audio.GetNameString();// FloatUtil.GetVector3String(Audio.InnerPos);
            }
            if (MloRoomDef != null)
            {
                name = "MloRoomDef " + MloRoomDef.RoomName;
            }
            if (EntityExtension != null)
            {
                name += ": " + EntityExtension.Name;
            }
            if (ArchetypeExtension != null)
            {
                name += ": " + ArchetypeExtension.Name;
            }
            return name;
        }

        public string GetFullNameString(string defval)
        {
            string name = GetNameString(defval);
            if (Geometry != null)
            {
                name += " (" + GeometryIndex.ToString() + ")";
            }
            return name;
        }


        public bool CanMarkUndo()
        {
            if (MultipleSelectionItems != null) return true;
            if (EntityDef != null) return true;
            if (CarGenerator != null) return true;
            if (LodLight != null) return true;
            if (BoxOccluder != null) return true;
            if (OccludeModelTri != null) return true;
            if (CollisionBounds != null) return true;
            if (CollisionPoly != null) return true;
            if (CollisionVertex != null) return true;
            if (PathNode != null) return true;
            //if (NavPoly != null) return true;
            if (NavPoint != null) return true;
            if (NavPortal != null) return true;
            if (TrainTrackNode != null) return true;
            if (ScenarioNode != null) return true;
            if (Audio != null) return true;
            return false;
        }
        public UndoStep CreateUndoStep(WidgetMode mode, Vector3 startPos, Quaternion startRot, Vector3 startScale, WorldForm wf, bool editPivot)
        {
            if (MultipleSelectionItems != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new MultiPositionUndoStep(this, startPos, wf);
                    case WidgetMode.Rotation: return new MultiRotationUndoStep(this, startRot, wf);
                    case WidgetMode.Scale: return new MultiScaleUndoStep(this, startScale, wf);
                }
            }
            else if (CollisionVertex != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CollisionVertexPositionUndoStep(CollisionVertex, EntityDef, startPos, wf);
                }
            }
            else if (CollisionPoly != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CollisionPolyPositionUndoStep(CollisionPoly, EntityDef, startPos, wf);
                    case WidgetMode.Rotation: return new CollisionPolyRotationUndoStep(CollisionPoly, EntityDef, startRot, wf);
                    case WidgetMode.Scale: return new CollisionPolyScaleUndoStep(CollisionPoly, startScale, wf);
                }
            }
            else if (CollisionBounds != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CollisionPositionUndoStep(CollisionBounds, EntityDef, startPos, wf);
                    case WidgetMode.Rotation: return new CollisionRotationUndoStep(CollisionBounds, EntityDef, startRot, wf);
                    case WidgetMode.Scale: return new CollisionScaleUndoStep(CollisionBounds, startScale, wf);
                }
            }
            else if (EntityDef != null)
            {
                if (editPivot)
                {
                    switch (mode)
                    {
                        case WidgetMode.Position: return new EntityPivotPositionUndoStep(EntityDef, startPos);
                        case WidgetMode.Rotation: return new EntityPivotRotationUndoStep(EntityDef, startRot);
                    }
                }
                else
                {
                    switch (mode)
                    {
                        case WidgetMode.Position: return new EntityPositionUndoStep(EntityDef, startPos);
                        case WidgetMode.Rotation: return new EntityRotationUndoStep(EntityDef, startRot);
                        case WidgetMode.Scale: return new EntityScaleUndoStep(EntityDef, startScale);
                    }
                }
            }
            else if (CarGenerator != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CarGenPositionUndoStep(CarGenerator, startPos);
                    case WidgetMode.Rotation: return new CarGenRotationUndoStep(CarGenerator, startRot);
                    case WidgetMode.Scale: return new CarGenScaleUndoStep(CarGenerator, startScale);
                }
            }
            else if (LodLight != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new LodLightPositionUndoStep(LodLight, startPos);
                    case WidgetMode.Rotation: return new LodLightRotationUndoStep(LodLight, startRot);
                    case WidgetMode.Scale: return new LodLightScaleUndoStep(LodLight, startScale);
                }
            }
            else if (BoxOccluder != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new BoxOccluderPositionUndoStep(BoxOccluder, startPos);
                    case WidgetMode.Rotation: return new BoxOccluderRotationUndoStep(BoxOccluder, startRot);
                    case WidgetMode.Scale: return new BoxOccluderScaleUndoStep(BoxOccluder, startScale);
                }
            }
            else if (OccludeModelTri != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new OccludeModelTriPositionUndoStep(OccludeModelTri, startPos);
                    case WidgetMode.Rotation: return new OccludeModelTriRotationUndoStep(OccludeModelTri, startRot);
                    case WidgetMode.Scale: return new OccludeModelTriScaleUndoStep(OccludeModelTri, startScale);
                }
            }
            else if (PathNode != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new PathNodePositionUndoStep(PathNode, startPos, wf);
                }
            }
            else if (NavPoly != null)
            {
                //todo...
            }
            else if (NavPoint != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new NavPointPositionUndoStep(NavPoint, startPos, wf);
                    case WidgetMode.Rotation: return new NavPointRotationUndoStep(NavPoint, startRot, wf);
                }
            }
            else if (NavPortal != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new NavPortalPositionUndoStep(NavPortal, startPos, wf);
                    case WidgetMode.Rotation: return new NavPortalRotationUndoStep(NavPortal, startRot, wf);
                }
            }
            else if (TrainTrackNode != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new TrainTrackNodePositionUndoStep(TrainTrackNode, startPos, wf);
                }
            }
            else if (ScenarioNode != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new ScenarioNodePositionUndoStep(ScenarioNode, startPos, wf);
                    case WidgetMode.Rotation: return new ScenarioNodeRotationUndoStep(ScenarioNode, startRot, wf);
                }
            }
            else if (Audio != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new AudioPositionUndoStep(Audio, startPos);
                    case WidgetMode.Rotation: return new AudioRotationUndoStep(Audio, startRot);
                }
            }
            return null;
        }

        public bool CanShowWidget
        {
            get
            {
                bool res = false;

                if (MultipleSelectionItems != null)
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
                else if (LodLight != null)
                {
                    res = true;
                }
                else if (BoxOccluder != null)
                {
                    res = true;
                }
                else if (OccludeModelTri != null)
                {
                    res = true;
                }
                else if (NavPoly != null)
                {
                    res = true;
                }
                else if (CollisionVertex != null)
                {
                    res = true;
                }
                else if (CollisionPoly != null)
                {
                    res = true;
                }
                else if (CollisionBounds != null)
                {
                    res = true;
                }
                else if (NavPoint != null)
                {
                    res = true;
                }
                else if (NavPortal != null)
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
                if (MultipleSelectionItems != null)
                {
                    return MultipleSelectionCenter;
                }
                else if (CollisionVertex != null)
                {
                    if (EntityDef != null) return EntityDef.Position + EntityDef.Orientation.Multiply(CollisionVertex.Position);
                    return CollisionVertex.Position;
                }
                else if (CollisionPoly != null)
                {
                    if (EntityDef != null) return EntityDef.Position + EntityDef.Orientation.Multiply(CollisionPoly.Position);
                    return CollisionPoly.Position;
                }
                else if (CollisionBounds != null)
                {
                    if (EntityDef != null) return EntityDef.Position + EntityDef.Orientation.Multiply(CollisionBounds.Position);
                    return CollisionBounds.Position;
                }
                else if (EntityDef != null)
                {
                    return EntityDef.WidgetPosition;
                }
                else if (CarGenerator != null)
                {
                    return CarGenerator.Position;
                }
                else if (LodLight != null)
                {
                    return LodLight.Position;
                }
                else if (BoxOccluder != null)
                {
                    return BoxOccluder.Position;
                }
                else if (OccludeModelTri != null)
                {
                    return OccludeModelTri.Center;
                }
                else if (NavPoly != null)
                {
                    return NavPoly.Position;
                }
                else if (NavPoint != null)
                {
                    return NavPoint.Position;
                }
                else if (NavPortal != null)
                {
                    return NavPortal.Position;
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
                if (MultipleSelectionItems != null)
                {
                    return MultipleSelectionRotation;
                }
                else if (CollisionVertex != null)
                {
                    if (EntityDef != null) return EntityDef.Orientation;
                    return Quaternion.Identity;
                }
                else if (CollisionPoly != null)
                {
                    if (EntityDef != null) return CollisionPoly.Orientation * EntityDef.Orientation;
                    return CollisionPoly.Orientation;
                }
                else if (CollisionBounds != null)
                {
                    if (EntityDef != null) return CollisionBounds.Orientation * EntityDef.Orientation;
                    return CollisionBounds.Orientation;
                }
                else if (EntityDef != null)
                {
                    return EntityDef.WidgetOrientation;
                }
                else if (CarGenerator != null)
                {
                    return CarGenerator.Orientation;
                }
                else if (LodLight != null)
                {
                    return LodLight.Orientation;
                }
                else if (BoxOccluder != null)
                {
                    return BoxOccluder.Orientation;
                }
                else if (OccludeModelTri != null)
                {
                    return OccludeModelTri.Orientation;
                }
                else if (NavPoly != null)
                {
                    return Quaternion.Identity;
                }
                else if (NavPoint != null)
                {
                    return NavPoint.Orientation;
                }
                else if (NavPortal != null)
                {
                    return NavPortal.Orientation;
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
                if (MultipleSelectionItems != null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (CollisionVertex != null)
                {
                    return WidgetAxis.None;
                }
                else if (CollisionPoly != null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (CollisionBounds != null)
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
                else if (LodLight != null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (BoxOccluder != null)
                {
                    return WidgetAxis.Z;
                }
                else if (OccludeModelTri != null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (NavPoly != null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (NavPoint != null)
                {
                    return WidgetAxis.Z;
                }
                else if (NavPortal != null)
                {
                    return WidgetAxis.Z;
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
                    return WidgetAxis.Z;
                }
                return WidgetAxis.None;
            }
        }
        public Vector3 WidgetScale
        {
            get
            {
                if (MultipleSelectionItems != null)
                {
                    return MultipleSelectionScale;
                }
                else if (CollisionVertex != null)
                {
                    return Vector3.One;
                }
                else if (CollisionPoly != null)
                {
                    return CollisionPoly.Scale;
                }
                else if (CollisionBounds != null)
                {
                    return CollisionBounds.Scale;
                }
                else if (EntityDef != null)
                {
                    return EntityDef.Scale;
                }
                else if (CarGenerator != null)
                {
                    return new Vector3(CarGenerator.CCarGen.perpendicularLength);
                }
                else if (LodLight != null)
                {
                    return LodLight.Scale;
                }
                else if (BoxOccluder != null)
                {
                    return BoxOccluder.Size;
                }
                else if (OccludeModelTri != null)
                {
                    return OccludeModelTri.Scale;
                }
                else if (NavPoly != null)
                {
                    return Vector3.One;
                }
                else if (NavPoint != null)
                {
                    return Vector3.One;
                }
                else if (NavPortal != null)
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
        public bool WidgetScaleLockXY
        {
            get
            {
                if (MultipleSelectionItems != null)
                {
                    for (int i = 0; i < MultipleSelectionItems.Length; i++)
                    {
                        if ((MultipleSelectionItems[i].EntityDef != null) && (MultipleSelectionItems[i].CollisionBounds == null)) return true;
                    }
                    return false;
                }
                if (BoxOccluder != null)
                {
                    return false;
                }
                if (OccludeModelTri != null)
                {
                    return false;
                }
                if (CollisionBounds != null)
                {
                    return false;
                }
                if (CollisionPoly != null)
                {
                    return false;
                }
                return true;
            }
        }

        public bool CanCopyPaste
        {
            get
            {
                if (MultipleSelectionItems != null)
                {
                    for (int i = 0; i < MultipleSelectionItems.Length; i++)
                    {
                        if (MultipleSelectionItems[i].CanCopyPaste) return true;
                    }
                    return false;
                }
                else if (CollisionVertex != null) return false;//can't copy just a vertex..
                else if (CollisionPoly != null) return true;
                else if (CollisionBounds != null) return true;
                else if (EntityDef != null) return true;
                else if (CarGenerator != null) return true;
                else if (LodLight != null) return true;
                else if (BoxOccluder != null) return true;
                else if (OccludeModelTri != null) return true;
                else if (PathNode != null) return true;
                else if (NavPoly != null) return true;
                else if (NavPoint != null) return true;
                else if (NavPortal != null) return true;
                else if (TrainTrackNode != null) return true;
                else if (ScenarioNode != null) return true;
                else if (Audio?.AudioZone != null) return true;
                else if (Audio?.AudioEmitter != null) return true;
                return false;
            }
        }


        public void SetMultipleSelectionItems(MapSelection[] items)
        {
            if ((items != null) && (items.Length == 0)) items = null;
            MultipleSelectionItems = items;
            GatheredCollisionVerts = null;
            var center = Vector3.Zero;
            if (items != null)
            {
                Dictionary<BoundVertex, int> collVerts = null;
                for (int i = 0; i < items.Length; i++)
                {
                    center += items[i].WidgetPosition;
                    var collVert = items[i].CollisionVertex;
                    var collPoly = items[i].CollisionPoly;
                    if (collVert != null)
                    {
                        if (collVerts == null) collVerts = new Dictionary<BoundVertex, int>();
                        collVerts[collVert] = collVert.Index;
                    }
                    else if (collPoly != null)
                    {
                        if (collVerts == null) collVerts = new Dictionary<BoundVertex, int>();
                        collPoly.GatherVertices(collVerts);
                    }
                }
                if (collVerts != null)
                {
                    GatheredCollisionVerts = collVerts.Keys.ToArray();
                }
                if (items.Length > 0)
                {
                    center *= (1.0f / items.Length);
                }
            }
            MultipleSelectionCenter = center;
        }


        public void SetPosition(Vector3 newpos, bool editPivot)
        {
            if (MultipleSelectionItems != null)
            {
                if (editPivot)
                {
                }
                else
                {
                    var dpos = newpos - MultipleSelectionCenter;// oldpos;
                    if (dpos == Vector3.Zero) return; //nothing moved.. (probably due to snap)
                    YmapEntityDef ent = null;//hack to use an entity for multple selections... buggy if entities mismatch!!!
                    for (int i = 0; i < MultipleSelectionItems.Length; i++)
                    {
                        var collVert = MultipleSelectionItems[i].CollisionVertex;
                        var collPoly = MultipleSelectionItems[i].CollisionPoly;
                        if ((collVert == null) && (collPoly == null))//skip polys, they use gathered verts
                        {
                            var refpos = MultipleSelectionItems[i].WidgetPosition;
                            MultipleSelectionItems[i].SetPosition(refpos + dpos, false);
                        }
                        ent = MultipleSelectionItems[i].EntityDef ?? ent;
                    }
                    if (ent != null) dpos = Quaternion.Invert(ent.Orientation).Multiply(dpos);
                    if (GatheredCollisionVerts != null)
                    {
                        for (int i = 0; i < GatheredCollisionVerts.Length; i++)
                        {
                            var refpos = GatheredCollisionVerts[i].Position;
                            var vnewpos = refpos + dpos;
                            GatheredCollisionVerts[i].Position = vnewpos;
                        }
                    }
                    MultipleSelectionCenter = newpos;
                }
            }
            else if (CollisionVertex != null)
            {
                if (EntityDef != null) newpos = Quaternion.Invert(EntityDef.Orientation).Multiply(newpos - EntityDef.Position);
                CollisionVertex.Position = newpos;
            }
            else if (CollisionPoly != null)
            {
                if (EntityDef != null) newpos = Quaternion.Invert(EntityDef.Orientation).Multiply(newpos - EntityDef.Position);
                CollisionPoly.Position = newpos;
            }
            else if (CollisionBounds != null)
            {
                if (EntityDef != null) newpos = Quaternion.Invert(EntityDef.Orientation).Multiply(newpos - EntityDef.Position);
                CollisionBounds.Position = newpos;
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
            else if (LodLight != null)
            {
                LodLight.SetPosition(newpos);
            }
            else if (BoxOccluder != null)
            {
                BoxOccluder.Position = newpos;
            }
            else if (OccludeModelTri != null)
            {
                OccludeModelTri.Center = newpos;
            }
            else if (PathNode != null)
            {
                PathNode.SetPosition(newpos);
            }
            else if (NavPoly != null)
            {
                NavPoly.SetPosition(newpos);
            }
            else if (NavPoint != null)
            {
                NavPoint.SetPosition(newpos);
            }
            else if (NavPortal != null)
            {
                NavPortal.SetPosition(newpos);
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
        public void SetRotation(Quaternion newrot, bool editPivot)
        {
            if (MultipleSelectionItems != null)
            {
                if (editPivot)
                {
                }
                else
                {
                    var cen = MultipleSelectionCenter;
                    var orinv = Quaternion.Invert(MultipleSelectionRotation);
                    var trans = newrot * orinv;
                    YmapEntityDef ent = null;//hack to use an entity for multple selections... buggy if entities mismatch!!!
                    for (int i = 0; i < MultipleSelectionItems.Length; i++)
                    {
                        var collVert = MultipleSelectionItems[i].CollisionVertex;
                        var collPoly = MultipleSelectionItems[i].CollisionPoly;
                        if ((collVert == null) && (collPoly == null))//skip polys, they use gathered verts
                        {
                            var refpos = MultipleSelectionItems[i].WidgetPosition;
                            var relpos = refpos - cen;
                            var newpos = trans.Multiply(relpos) + cen;
                            var refori = MultipleSelectionItems[i].WidgetRotation;
                            var newori = trans * refori;
                            MultipleSelectionItems[i].SetPosition(newpos, false);
                            MultipleSelectionItems[i].SetRotation(newori, false);
                        }
                        ent = MultipleSelectionItems[i].EntityDef ?? ent;
                    }
                    var eorinv = (ent != null) ? Quaternion.Invert(ent.Orientation) : Quaternion.Identity;
                    if (GatheredCollisionVerts != null)
                    {
                        for (int i = 0; i < GatheredCollisionVerts.Length; i++)
                        {
                            var refpos = GatheredCollisionVerts[i].Position;
                            var relpos = refpos - cen;
                            var newpos = trans.Multiply(relpos) + cen;
                            if (ent != null)
                            {
                                refpos = ent.Position + ent.Orientation.Multiply(refpos);
                                relpos = refpos - cen;
                                newpos = eorinv.Multiply(trans.Multiply(relpos) + cen - ent.Position);
                            }
                            GatheredCollisionVerts[i].Position = newpos;
                        }
                    }
                    MultipleSelectionRotation = newrot;
                }
            }
            else if (CollisionVertex != null)
            {
                //do nothing, but stop any poly from being rotated also
            }
            else if (CollisionPoly != null)
            {
                if (EntityDef != null) newrot = Quaternion.Normalize(Quaternion.Invert(EntityDef.Orientation) * newrot);
                CollisionPoly.Orientation = newrot;
            }
            else if (CollisionBounds != null)
            {
                if (EntityDef != null) newrot = Quaternion.Normalize(Quaternion.Invert(EntityDef.Orientation) * newrot);
                CollisionBounds.Orientation = newrot;
            }
            else if (EntityDef != null)
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
            else if (LodLight != null)
            {
                LodLight.SetOrientation(newrot);
            }
            else if (BoxOccluder != null)
            {
                BoxOccluder.Orientation = newrot;
            }
            else if (OccludeModelTri != null)
            {
                OccludeModelTri.Orientation = newrot;
            }
            else if (ScenarioNode != null)
            {
                ScenarioNode.SetOrientation(newrot);
            }
            else if (NavPoint != null)
            {
                NavPoint.SetOrientation(newrot);
            }
            else if (NavPortal != null)
            {
                NavPortal.SetOrientation(newrot);
            }
            else if (Audio != null)
            {
                Audio.SetOrientation(newrot);
            }
        }
        public void SetScale(Vector3 newscale, bool editPivot)
        {
            if (MultipleSelectionItems != null)
            {
                if (editPivot)
                {//editing pivot scale is sort of meaningless..
                }
                else
                {
                    var cen = MultipleSelectionCenter;
                    var ori = MultipleSelectionRotation;
                    var orinv = Quaternion.Invert(ori);
                    var rsca = newscale / MultipleSelectionScale;
                    YmapEntityDef ent = null;//hack to use an entity for multple selections... buggy if entities mismatch!!!
                    for (int i = 0; i < MultipleSelectionItems.Length; i++)
                    {
                        var collVert = MultipleSelectionItems[i].CollisionVertex;
                        var collPoly = MultipleSelectionItems[i].CollisionPoly;
                        if ((collVert == null) && (collPoly == null))//skip polys, they use gathered verts
                        {
                            var refpos = MultipleSelectionItems[i].WidgetPosition;
                            var relpos = refpos - cen;
                            var newpos = ori.Multiply(orinv.Multiply(relpos) * rsca) + cen;
                            MultipleSelectionItems[i].SetPosition(newpos, false);
                            MultipleSelectionItems[i].SetScale(newscale, false);
                        }
                        ent = MultipleSelectionItems[i].EntityDef ?? ent;
                    }
                    var eorinv = (ent != null) ? Quaternion.Invert(ent.Orientation) : Quaternion.Identity;
                    if (GatheredCollisionVerts != null)
                    {
                        for (int i = 0; i < GatheredCollisionVerts.Length; i++)
                        {
                            var refpos = GatheredCollisionVerts[i].Position;
                            var relpos = refpos - cen;
                            var newpos = ori.Multiply(orinv.Multiply(relpos) * rsca) + cen;
                            if (ent != null)
                            {
                                refpos = ent.Position + ent.Orientation.Multiply(refpos);
                                relpos = refpos - cen;
                                newpos = ori.Multiply(orinv.Multiply(relpos) * rsca) + cen;
                                newpos = eorinv.Multiply(newpos - ent.Position);
                            }
                            GatheredCollisionVerts[i].Position = newpos;
                        }
                    }
                    MultipleSelectionScale = newscale;
                }
            }
            else if (CollisionVertex != null)
            {
                //do nothing, but stop any poly from being scaled also
            }
            else if (CollisionPoly != null)
            {
                CollisionPoly.Scale = newscale;
            }
            else if (CollisionBounds != null)
            {
                CollisionBounds.Scale = newscale;
            }
            else if (EntityDef != null)
            {
                EntityDef.SetScale(newscale);
            }
            else if (CarGenerator != null)
            {
                CarGenerator.SetScale(newscale);
                AABB = new BoundingBox(CarGenerator.BBMin, CarGenerator.BBMax);
            }
            else if (LodLight != null)
            {
                LodLight.SetScale(newscale);
            }
            else if (BoxOccluder != null)
            {
                BoxOccluder.SetSize(newscale);
            }
            else if (OccludeModelTri != null)
            {
                OccludeModelTri.Scale = newscale;
            }
        }



        public void UpdateCollisionFromRayHit(ref SpaceRayIntersectResult hit, Camera camera)
        {
            var position = hit.HitEntity?.Position ?? Vector3.Zero;
            var orientation = hit.HitEntity?.Orientation ?? Quaternion.Identity;
            var scale = hit.HitEntity?.Scale ?? Vector3.One;
            var camrel = position - camera.Position;
            var trans = hit.HitBounds?.Transform.TranslationVector ?? Vector3.Zero;

            CollisionPoly = hit.HitPolygon;
            CollisionBounds = hit.HitBounds;
            EntityDef = hit.HitEntity;
            Archetype = hit.HitEntity?.Archetype;
            HitDist = hit.HitDist;
            CamRel = camrel + orientation.Multiply(trans);
            BBOffset = trans;
            BBOrientation = hit.HitBounds?.Transform.ToQuaternion() ?? Quaternion.Identity;
            AABB = new BoundingBox(hit.HitBounds?.BoxMin ?? Vector3.Zero, hit.HitBounds?.BoxMax ?? Vector3.Zero);

            float vertexDist = 0.1f;
            if ((hit.HitVertex.Distance < vertexDist) && (hit.HitBounds is BoundGeometry bgeom))
            {
                CollisionVertex = bgeom.GetVertexObject(hit.HitVertex.Index);
            }
            else
            {
                CollisionVertex = null;
            }
        }




        public void UpdateGraphics(WorldForm wf)
        {
            if (MultipleSelectionItems != null)
            {
                var pathYnds = new Dictionary<YndFile, int>();
                var navYnvs = new Dictionary<YnvFile, int>();
                var trainTracks = new Dictionary<TrainTrack, int>();
                var scenarioYmts = new Dictionary<YmtFile, int>();
                var bounds = new Dictionary<Bounds, int>();
                var lodlights = new Dictionary<YmapLODLights, int>();
                var boxoccls = new Dictionary<YmapBoxOccluder, int>();
                var occlmods = new Dictionary<YmapOccludeModel, int>();

                foreach (var item in MultipleSelectionItems)
                {
                    if (item.PathNode != null)
                    {
                        pathYnds[item.PathNode.Ynd] = 1;
                    }
                    if (item.NavPoly != null)
                    {
                        navYnvs[item.NavPoly.Ynv] = 1;
                    }
                    if (item.NavPoint != null)
                    {
                        navYnvs[item.NavPoint.Ynv] = 1;
                    }
                    if (item.NavPortal != null)
                    {
                        navYnvs[item.NavPortal.Ynv] = 1;
                    }
                    if (item.TrainTrackNode != null)
                    {
                        trainTracks[item.TrainTrackNode.Track] = 1;
                    }
                    if (item.ScenarioNode != null)
                    {
                        scenarioYmts[item.ScenarioNode.Ymt] = 1;
                    }
                    if (item.CollisionBounds != null)
                    {
                        bounds[item.CollisionBounds] = 1;
                    }
                    if (item.CollisionPoly?.Owner != null)
                    {
                        bounds[item.CollisionPoly.Owner] = 1;
                    }
                    if (item.CollisionVertex?.Owner != null)
                    {
                        bounds[item.CollisionVertex.Owner] = 1;
                    }
                    if (item.LodLight?.LodLights != null)
                    {
                        lodlights[item.LodLight.LodLights] = 1;
                    }
                    if (item.BoxOccluder != null)
                    {
                        boxoccls[item.BoxOccluder] = 1;
                    }
                    if (item.OccludeModelTri?.Model != null)
                    {
                        occlmods[item.OccludeModelTri.Model] = 1;
                    }
                }
                foreach (var kvp in bounds)
                {
                    wf.UpdateCollisionBoundsGraphics(kvp.Key);
                }
                foreach (var kvp in pathYnds)
                {
                    wf.UpdatePathYndGraphics(kvp.Key, true);
                }
                foreach (var kvp in navYnvs)
                {
                    wf.UpdateNavYnvGraphics(kvp.Key, true);
                }
                foreach (var kvp in trainTracks)
                {
                    wf.UpdateTrainTrackGraphics(kvp.Key, false);
                }
                foreach (var kvp in scenarioYmts)
                {
                    wf.UpdateScenarioGraphics(kvp.Key, false);
                }
                foreach (var kvp in lodlights)
                {
                    if ((kvp.Key.LodLights != null) && (kvp.Key.LodLights.Length > 0))
                    {
                        wf.UpdateLodLightGraphics(kvp.Key.LodLights[0]);
                    }
                }
                foreach (var kvp in boxoccls)
                {
                    wf.UpdateBoxOccluderGraphics(kvp.Key);
                }
                foreach (var kvp in occlmods)
                {
                    wf.UpdateOccludeModelGraphics(kvp.Key);
                }
            }
            else
            {
                if (PathNode != null)
                {
                    wf.UpdatePathYndGraphics(PathNode.Ynd, true);
                }
                if (NavPoly != null)
                {
                    wf.UpdateNavYnvGraphics(NavPoly.Ynv, true);
                }
                if (NavPoint != null)
                {
                    wf.UpdateNavYnvGraphics(NavPoint.Ynv, true);
                }
                if (NavPortal != null)
                {
                    wf.UpdateNavYnvGraphics(NavPortal.Ynv, true);
                }
                if (TrainTrackNode != null)
                {
                    wf.UpdateTrainTrackGraphics(TrainTrackNode.Track, false);
                }
                if (ScenarioNode != null)
                {
                    wf.UpdateScenarioGraphics(ScenarioNode.Ymt, false);
                }
                if (CollisionVertex?.Owner != null)
                {
                    wf.UpdateCollisionBoundsGraphics(CollisionVertex.Owner);
                }
                else if (CollisionPoly?.Owner != null)
                {
                    wf.UpdateCollisionBoundsGraphics(CollisionPoly.Owner);
                }
                else if (CollisionBounds != null)
                {
                    wf.UpdateCollisionBoundsGraphics(CollisionBounds);
                }
                else if (LodLight != null)
                {
                    wf.UpdateLodLightGraphics(LodLight);
                }
                else if (BoxOccluder != null)
                {
                    wf.UpdateBoxOccluderGraphics(BoxOccluder);
                }
                else if (OccludeModelTri?.Model != null)
                {
                    wf.UpdateOccludeModelGraphics(OccludeModelTri?.Model);
                }
            }
        }


        public object GetProjectObject()
        {
            if (MultipleSelectionItems != null) return MultipleSelectionItems;
            else if (CollisionVertex != null) return CollisionVertex;
            else if (CollisionPoly != null) return CollisionPoly;
            else if (CollisionBounds != null) return CollisionBounds;
            else if (EntityDef != null) return EntityDef;
            else if (CarGenerator != null) return CarGenerator;
            else if (LodLight != null) return LodLight;
            else if (BoxOccluder != null) return BoxOccluder;
            else if (OccludeModelTri != null) return OccludeModelTri;
            else if (PathNode != null) return PathNode;
            else if (NavPoly != null) return NavPoly;
            else if (NavPoint != null) return NavPoint;
            else if (NavPortal != null) return NavPortal;
            else if (TrainTrackNode != null) return TrainTrackNode;
            else if (ScenarioNode != null) return ScenarioNode;
            else if (Audio?.AudioZone != null) return Audio;
            else if (Audio?.AudioEmitter != null) return Audio;
            return null;
        }

        public static MapSelection FromProjectObject(object o, object parent = null)
        {
            const float nrad = 0.5f;
            var ms = new MapSelection();
            if (o is object[] arr)
            {
                var multi = new MapSelection[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    multi[i] = FromProjectObject(arr[i]);
                }
                ms.SetMultipleSelectionItems(multi);
            }
            else if (o is YmapEntityDef entity)
            {
                ms.EntityDef = entity;
                ms.Archetype = entity?.Archetype;
                ms.AABB = new BoundingBox(entity.BBMin, entity.BBMax);
                if (entity.MloInstance != null)
                {
                    ms.MloEntityDef = entity;
                }
            }
            else if (o is YmapCarGen cargen)
            {
                ms.CarGenerator = cargen;
                ms.AABB = new BoundingBox(cargen.BBMin, cargen.BBMax);
            }
            else if (o is YmapLODLight lodlight)
            {
                ms.LodLight = lodlight;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
            }
            else if (o is YmapBoxOccluder boxoccl)
            {
                ms.BoxOccluder = boxoccl;
                ms.AABB = new BoundingBox(boxoccl.BBMin, boxoccl.BBMax);
            }
            else if (o is YmapOccludeModelTriangle occltri)
            {
                ms.OccludeModelTri = occltri;
                ms.AABB = new BoundingBox(occltri.Box.Minimum, occltri.Box.Maximum);
            }
            else if (o is YmapGrassInstanceBatch batch)
            {
                ms.GrassBatch = batch;
                ms.AABB = new BoundingBox(batch.AABBMin, batch.AABBMax);
            }
            else if (o is MCMloRoomDef room)
            {
                if (parent is MloInstanceData instance)
                {
                    ms.MloRoomDef = room;
                    ms.AABB = new BoundingBox(room.BBMin_CW, room.BBMax_CW);
                    ms.BBOffset = instance.Owner.Position;
                    ms.BBOrientation = instance.Owner.Orientation;
                }
            }
            else if (o is Bounds b)
            {
                ms.CollisionBounds = b;
                ms.AABB = new BoundingBox(b.BoxMin, b.BoxMax);
            }
            else if (o is BoundPolygon p)
            {
                ms.CollisionPoly = p;
                //ms.AABB = new BoundingBox(p.BoundingBoxMin, p.BoundingBoxMax);
            }
            else if (o is BoundVertex v)
            {
                ms.CollisionVertex = v;
                //ms.AABB = new BoundingBox(p.BoundingBoxMin, p.BoundingBoxMax);
            }
            else if (o is YnvPoly poly)
            {
                var cellaabb = poly._RawData.CellAABB;
                var sect = poly.Ynv?.Nav?.SectorTree;
                ms.NavPoly = poly;
                ms.AABB = new BoundingBox(cellaabb.Min, cellaabb.Max);
                //if (sect != null)
                //{
                //    ms.AABB = new BoundingBox(sect.AABBMin.XYZ(), sect.AABBMax.XYZ());
                //}
            }
            else if (o is YnvPoint point)
            {
                ms.NavPoint = point;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
            }
            else if (o is YnvPortal portal)
            {
                ms.NavPortal = portal;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
            }
            else if (o is YndNode node)
            {
                ms.PathNode = node;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
            }
            else if (o is YndLink link)
            {
                ms.PathNode = link.Node1;
                ms.PathLink = link;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
            }
            else if (o is TrainTrackNode tnode)
            {
                ms.TrainTrackNode = tnode;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
            }
            else if (o is ScenarioNode snode)
            {
                ms.ScenarioNode = snode;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
            }
            else if (o is MCScenarioChainingEdge sedge)
            {
                if (parent is ScenarioNode enode)
                {
                    ms.ScenarioNode = enode;
                    ms.ScenarioEdge = sedge;
                    ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
                }
            }
            else if (o is AudioPlacement audio)
            {
                ms.Audio = audio;
                ms.AABB = new BoundingBox(audio.HitboxMin, audio.HitboxMax);
                ms.BSphere = new BoundingSphere(audio.Position, audio.HitSphereRad);
            }
            return ms;
        }



        public override string ToString()
        {
            return GetFullNameString("[Empty]");
        }
    }



}
