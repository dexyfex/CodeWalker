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
        public WorldForm? WorldForm { get; set; }
        public YmapEntityDef? EntityDef { get; set; }
        public Archetype? Archetype { get; set; }
        public DrawableBase? Drawable { get; set; }
        public DrawableGeometry? Geometry { get; set; }
        public MetaWrapper? EntityExtension { get; set; }
        public MetaWrapper? ArchetypeExtension { get; set; }
        public YmapTimeCycleModifier? TimeCycleModifier { get; set; }
        public YmapCarGen? CarGenerator { get; set; }
        public YmapGrassInstanceBatch? GrassBatch { get; set; }
        public YmapLODLight? LodLight { get; set; }
        public YmapBoxOccluder? BoxOccluder { get; set; }
        public YmapOccludeModelTriangle? OccludeModelTri { get; set; }
        public YmapEntityDef? MloEntityDef { get; set; }
        public MCMloRoomDef? MloRoomDef { get; set; }
        public WaterQuad? WaterQuad { get; set; }
        public WaterCalmingQuad? CalmingQuad { get; set; }
        public WaterWaveQuad? WaveQuad { get; set; }
        public Bounds? CollisionBounds { get; set; }
        public BoundPolygon? CollisionPoly { get; set; }
        public BoundVertex? CollisionVertex { get; set; }
        public YnvPoly? NavPoly { get; set; }
        public YnvPoint? NavPoint { get; set; }
        public YnvPortal? NavPortal { get; set; }
        public YndNode? PathNode { get; set; }
        public YndLink? PathLink { get; set; }
        public TrainTrackNode? TrainTrackNode { get; set; }
        public ScenarioNode? ScenarioNode { get; set; }
        public MCScenarioChainingEdge? ScenarioEdge { get; set; }
        public AudioPlacement? Audio { get; set; }

        public MapSelection[]? MultipleSelectionItems { get; private set; }
        public Vector3 MultipleSelectionCenter { get; set; }
        public Quaternion MultipleSelectionRotation { get; set; }
        public Vector3 MultipleSelectionScale { get; set; }
        public BoundVertex[]? GatheredCollisionVerts { get; private set; } //for collision polys, need to move all the individual vertices instead

        public Vector3 BBOffset { get; set; }
        public Quaternion BBOrientation { get; set; }
        public BoundingBox AABB { get; set; }
        public BoundingSphere BSphere { get; set; }
        public int GeometryIndex { get; set; }
        public Vector3 CamRel { get; set; }
        public float HitDist { get; set; }


        public readonly bool HasValue
        {
            get
            {
                return (EntityDef is not null) ||
                    (Archetype is not null) ||
                    (Drawable is not null) ||
                    (Geometry is not null) ||
                    (EntityExtension is not null) ||
                    (ArchetypeExtension is not null) ||
                    (TimeCycleModifier is not null) ||
                    (CarGenerator is not null) ||
                    (GrassBatch is not null) ||
                    (WaterQuad is not null) ||
                    (CalmingQuad is not null) ||
                    (WaveQuad is not null) ||
                    (CollisionBounds is not null) ||
                    (CollisionPoly is not null) ||
                    (CollisionVertex is not null) ||
                    (NavPoly is not null) ||
                    (NavPoint is not null) ||
                    (NavPortal is not null) ||
                    (PathNode is not null) ||
                    (TrainTrackNode is not null) ||
                    (LodLight is not null) ||
                    (BoxOccluder is not null) ||
                    (OccludeModelTri is not null) ||
                    (MloEntityDef is not null) ||
                    (ScenarioNode is not null) ||
                    (Audio is not null) ||
                    (MloRoomDef is not null);
            }
        }

        public readonly bool HasHit => HitDist != float.MaxValue;

        public readonly bool CheckForChanges(in MapSelection mhit)
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
                || (CalmingQuad != mhit.CalmingQuad)
                || (WaveQuad != mhit.WaveQuad)
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
        public readonly bool CheckForChanges()
        {
            return (EntityDef is not null)
                || (Archetype is not null)
                || (Drawable is not null)
                || (TimeCycleModifier is not null)
                || (ArchetypeExtension is not null)
                || (EntityExtension is not null)
                || (CarGenerator is not null)
                || (MloEntityDef is not null)
                || (LodLight is not null)
                || (GrassBatch is not null)
                || (BoxOccluder is not null)
                || (OccludeModelTri is not null)
                || (WaterQuad is not null)
                || (CalmingQuad is not null)
                || (WaveQuad is not null)
                || (CollisionBounds is not null)
                || (CollisionPoly is not null)
                || (CollisionVertex is not null)
                || (NavPoly is not null)
                || (NavPoint is not null)
                || (NavPortal is not null)
                || (PathNode is not null)
                || (PathLink is not null)
                || (TrainTrackNode is not null)
                || (ScenarioNode is not null)
                || (Audio is not null)
                || (MloRoomDef is not null);
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
            CalmingQuad = null;
            WaveQuad = null;
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

        public readonly string GetNameString(string defval)
        {
            string name = defval;
            var ename = (EntityDef is not null) ? EntityDef._CEntityDef.archetypeName.ToString() : string.Empty;
            var enamec = ename + ((!string.IsNullOrEmpty(ename)) ? ": " : string.Empty);
            if (MultipleSelectionItems is not null)
            {
                name = "Multiple items";
            }
            else if (CollisionVertex is not null)
            {
                name = $"{enamec}Vertex {CollisionVertex.Index}{((CollisionBounds is not null) ? ($": {CollisionBounds.GetName()}") : string.Empty)}";
            }
            else if (CollisionPoly is not null)
            {
                name = $"{enamec}Poly {CollisionPoly.Index}{((CollisionBounds != null) ? ($": {CollisionBounds.GetName()}") : string.Empty)}";
            }
            else if (CollisionBounds is not null)
            {
                name = $"{enamec}{CollisionBounds.GetName()}";
            }
            else if (EntityDef is not null)
            {
                name = ename;
            }
            else if (Archetype is not null)
            {
                name = Archetype.Hash.ToString();
            }
            else if (TimeCycleModifier is not null)
            {
                name = TimeCycleModifier.CTimeCycleModifier.name.ToString();
            }
            else if (CarGenerator is not null)
            {
                name = $"{(CarGenerator.Ymap?.Name ?? string.Empty)}: {CarGenerator.NameString()}";
            }
            else if (GrassBatch is not null)
            {
                name = $"{(GrassBatch.Ymap?.Name ?? string.Empty)}: {GrassBatch.Archetype?.Name ?? string.Empty}";
            }
            else if (LodLight is not null)
            {
                name = $"{(LodLight.Ymap?.Name ?? string.Empty)}: {LodLight.Index}";
            }
            else if (BoxOccluder is not null)
            {
                name = "BoxOccluder " + (BoxOccluder.Ymap?.Name ?? string.Empty) + ": " + BoxOccluder.Index.ToString();
            }
            else if (OccludeModelTri is not null)
            {
                name = $"OccludeModel {OccludeModelTri.Ymap?.Name ?? string.Empty}: {OccludeModelTri.Model?.Index ?? 0}:{OccludeModelTri.Index}";
            }
            else if (WaterQuad is not null)
            {
                name = $"WaterQuad {WaterQuad}";
            }
            else if (CalmingQuad is not null)
            {
                name = $"WaterCalmingQuad {CalmingQuad}";
            }
            else if (WaveQuad is not null)
            {
                name = $"WaterWaveQuad {WaveQuad}";
            }
            else if (NavPoly is not null)
            {
                name = $"NavPoly {NavPoly}";
            }
            else if (NavPoint is not null)
            {
                name = $"NavPoint {NavPoint}";
            }
            else if (NavPortal is not null)
            {
                name = $"NavPortal {NavPortal}";
            }
            else if (PathNode is not null)
            {
                name = $"PathNode {PathNode.AreaID}.{PathNode.NodeID}"; //+ FloatUtil.GetVector3String(PathNode.Position);
            }
            else if (TrainTrackNode is not null)
            {
                name = $"TrainTrackNode {FloatUtil.GetVector3String(TrainTrackNode.Position)}";
            }
            else if (ScenarioNode is not null)
            {
                name = ScenarioNode.ToString();
            }
            else if (Audio is not null)
            {
                name = $"{Audio.ShortTypeName} {Audio.GetNameString()}";// FloatUtil.GetVector3String(Audio.InnerPos);
            }
            if (MloRoomDef is not null)
            {
                name = $"MloRoomDef {MloRoomDef.RoomName}";
            }
            if (EntityExtension is not null)
            {
                name += $": {EntityExtension.Name}";
            }
            if (ArchetypeExtension is not null)
            {
                name += $": {ArchetypeExtension.Name}";
            }
            return name;
        }

        public readonly string GetFullNameString(string defval)
        {
            string name = GetNameString(defval);
            if (Geometry is not null)
            {
                name += $" ({GeometryIndex})";
            }
            return name;
        }


        public readonly bool CanMarkUndo()
        {
            if (MultipleSelectionItems is not null) return true;
            if (EntityDef is not null) return true;
            if (CarGenerator is not null) return true;
            if (LodLight is not null) return true;
            if (BoxOccluder is not null) return true;
            if (OccludeModelTri is not null) return true;
            if (CollisionBounds is not null) return true;
            if (CollisionPoly is not null) return true;
            if (CollisionVertex is not null) return true;
            if (PathNode is not null) return true;
            //if (NavPoly != null) return true;
            if (NavPoint is not null) return true;
            if (NavPortal is not null) return true;
            if (TrainTrackNode is not null) return true;
            if (ScenarioNode is not null) return true;
            if (Audio is not null) return true;
            return false;
        }
        public readonly UndoStep? CreateUndoStep(WidgetMode mode, Vector3 startPos, Quaternion startRot, Vector3 startScale, WorldForm wf, bool editPivot)
        {
            if (MultipleSelectionItems is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new MultiPositionUndoStep(this, startPos, wf);
                    case WidgetMode.Rotation: return new MultiRotationUndoStep(this, startRot, wf);
                    case WidgetMode.Scale: return new MultiScaleUndoStep(this, startScale, wf);
                }
            }
            else if (CollisionVertex is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CollisionVertexPositionUndoStep(CollisionVertex, EntityDef, startPos, wf);
                }
            }
            else if (CollisionPoly is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CollisionPolyPositionUndoStep(CollisionPoly, EntityDef, startPos, wf);
                    case WidgetMode.Rotation: return new CollisionPolyRotationUndoStep(CollisionPoly, EntityDef, startRot, wf);
                    case WidgetMode.Scale: return new CollisionPolyScaleUndoStep(CollisionPoly, startScale, wf);
                }
            }
            else if (CollisionBounds is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CollisionPositionUndoStep(CollisionBounds, EntityDef, startPos, wf);
                    case WidgetMode.Rotation: return new CollisionRotationUndoStep(CollisionBounds, EntityDef, startRot, wf);
                    case WidgetMode.Scale: return new CollisionScaleUndoStep(CollisionBounds, startScale, wf);
                }
            }
            else if (EntityDef is not null)
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
            else if (CarGenerator is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CarGenPositionUndoStep(CarGenerator, startPos);
                    case WidgetMode.Rotation: return new CarGenRotationUndoStep(CarGenerator, startRot);
                    case WidgetMode.Scale: return new CarGenScaleUndoStep(CarGenerator, startScale);
                }
            }
            else if (LodLight is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new LodLightPositionUndoStep(LodLight, startPos);
                    case WidgetMode.Rotation: return new LodLightRotationUndoStep(LodLight, startRot);
                    case WidgetMode.Scale: return new LodLightScaleUndoStep(LodLight, startScale);
                }
            }
            else if (BoxOccluder is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new BoxOccluderPositionUndoStep(BoxOccluder, startPos);
                    case WidgetMode.Rotation: return new BoxOccluderRotationUndoStep(BoxOccluder, startRot);
                    case WidgetMode.Scale: return new BoxOccluderScaleUndoStep(BoxOccluder, startScale);
                }
            }
            else if (OccludeModelTri is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new OccludeModelTriPositionUndoStep(OccludeModelTri, startPos);
                    case WidgetMode.Rotation: return new OccludeModelTriRotationUndoStep(OccludeModelTri, startRot);
                    case WidgetMode.Scale: return new OccludeModelTriScaleUndoStep(OccludeModelTri, startScale);
                }
            }
            else if (PathNode is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new PathNodePositionUndoStep(PathNode, startPos, wf);
                }
            }
            else if (NavPoly is not null)
            {
                //todo...
            }
            else if (NavPoint is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new NavPointPositionUndoStep(NavPoint, startPos, wf);
                    case WidgetMode.Rotation: return new NavPointRotationUndoStep(NavPoint, startRot, wf);
                }
            }
            else if (NavPortal is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new NavPortalPositionUndoStep(NavPortal, startPos, wf);
                    case WidgetMode.Rotation: return new NavPortalRotationUndoStep(NavPortal, startRot, wf);
                }
            }
            else if (TrainTrackNode is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new TrainTrackNodePositionUndoStep(TrainTrackNode, startPos, wf);
                }
            }
            else if (ScenarioNode is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new ScenarioNodePositionUndoStep(ScenarioNode, startPos, wf);
                    case WidgetMode.Rotation: return new ScenarioNodeRotationUndoStep(ScenarioNode, startRot, wf);
                }
            }
            else if (Audio is not null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new AudioPositionUndoStep(Audio, startPos);
                    case WidgetMode.Rotation: return new AudioRotationUndoStep(Audio, startRot);
                }
            }
            return null;
        }

        public readonly bool CanShowWidget
        {
            get
            {
                bool res = false;

                if (MultipleSelectionItems is not null)
                {
                    res = true;
                }
                else if (EntityDef is not null)
                {
                    res = true;
                }
                else if (CarGenerator is not null)
                {
                    res = true;
                }
                else if (LodLight is not null)
                {
                    res = true;
                }
                else if (BoxOccluder is not null)
                {
                    res = true;
                }
                else if (OccludeModelTri is not null)
                {
                    res = true;
                }
                else if (NavPoly is not null)
                {
                    res = true;
                }
                else if (CollisionVertex is not null)
                {
                    res = true;
                }
                else if (CollisionPoly is not null)
                {
                    res = true;
                }
                else if (CollisionBounds is not null)
                {
                    res = true;
                }
                else if (NavPoint is not null)
                {
                    res = true;
                }
                else if (NavPortal is not null)
                {
                    res = true;
                }
                else if (PathNode is not null)
                {
                    res = true;
                }
                else if (TrainTrackNode is not null)
                {
                    res = true;
                }
                else if (ScenarioNode is not null)
                {
                    res = true;
                }
                else if (Audio is not null)
                {
                    res = true;
                }
                return res;
            }
        }
        public readonly Vector3 WidgetPosition
        {
            get
            {
                if (MultipleSelectionItems is not null)
                {
                    return MultipleSelectionCenter;
                }
                else if (CollisionVertex is not null)
                {
                    if (EntityDef is not null)
                        return EntityDef.Position + EntityDef.Orientation.Multiply(CollisionVertex.Position);
                    return CollisionVertex.Position;
                }
                else if (CollisionPoly is not null)
                {
                    if (EntityDef is not null) return EntityDef.Position + EntityDef.Orientation.Multiply(CollisionPoly.Position);
                    return CollisionPoly.Position;
                }
                else if (CollisionBounds is not null)
                {
                    if (EntityDef is not null) return EntityDef.Position + EntityDef.Orientation.Multiply(CollisionBounds.Position);
                    return CollisionBounds.Position;
                }
                else if (EntityDef is not null)
                {
                    return EntityDef.WidgetPosition;
                }
                else if (CarGenerator is not null)
                {
                    return CarGenerator.Position;
                }
                else if (LodLight is not null)
                {
                    return LodLight.Position;
                }
                else if (BoxOccluder is not null)
                {
                    return BoxOccluder.Position;
                }
                else if (OccludeModelTri is not null)
                {
                    return OccludeModelTri.Center;
                }
                else if (NavPoly is not null)
                {
                    return NavPoly.Position;
                }
                else if (NavPoint is not null)
                {
                    return NavPoint.Position;
                }
                else if (NavPortal is not null)
                {
                    return NavPortal.Position;
                }
                else if (PathNode is not null)
                {
                    return PathNode.Position;
                }
                else if (TrainTrackNode is not null)
                {
                    return TrainTrackNode.Position;
                }
                else if (ScenarioNode is not null)
                {
                    return ScenarioNode.Position;
                }
                else if (Audio is not null)
                {
                    return Audio.InnerPos;
                }
                return Vector3.Zero;
            }
        }
        public readonly Quaternion WidgetRotation
        {
            get
            {
                if (MultipleSelectionItems is not null)
                {
                    return MultipleSelectionRotation;
                }
                else if (CollisionVertex is not null)
                {
                    if (EntityDef is not null) return EntityDef.Orientation;
                    return Quaternion.Identity;
                }
                else if (CollisionPoly is not null)
                {
                    if (EntityDef is not null) return CollisionPoly.Orientation * EntityDef.Orientation;
                    return CollisionPoly.Orientation;
                }
                else if (CollisionBounds is not null)
                {
                    if (EntityDef is not null) return CollisionBounds.Orientation * EntityDef.Orientation;
                    return CollisionBounds.Orientation;
                }
                else if (EntityDef is not null)
                {
                    return EntityDef.WidgetOrientation;
                }
                else if (CarGenerator is not null)
                {
                    return CarGenerator.Orientation;
                }
                else if (LodLight is not null)
                {
                    return LodLight.Orientation;
                }
                else if (BoxOccluder is not null)
                {
                    return BoxOccluder.Orientation;
                }
                else if (OccludeModelTri is not null)
                {
                    return OccludeModelTri.Orientation;
                }
                else if (NavPoly is not null)
                {
                    return Quaternion.Identity;
                }
                else if (NavPoint is not null)
                {
                    return NavPoint.Orientation;
                }
                else if (NavPortal is not null)
                {
                    return NavPortal.Orientation;
                }
                else if (PathNode is not null)
                {
                    return Quaternion.Identity;
                }
                else if (TrainTrackNode is not null)
                {
                    return Quaternion.Identity;
                }
                else if (ScenarioNode is not null)
                {
                    return ScenarioNode.Orientation;
                }
                else if (Audio is not null)
                {
                    return Audio.Orientation;
                }
                return Quaternion.Identity;
            }
        }
        public readonly WidgetAxis WidgetRotationAxes
        {
            get
            {
                if (MultipleSelectionItems is not null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (CollisionVertex is not null)
                {
                    return WidgetAxis.None;
                }
                else if (CollisionPoly is not null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (CollisionBounds is not null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (EntityDef is not null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (CarGenerator is not null)
                {
                    return WidgetAxis.Z;
                }
                else if (LodLight is not null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (BoxOccluder is not null)
                {
                    return WidgetAxis.Z;
                }
                else if (OccludeModelTri is not null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (NavPoly is not null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (NavPoint is not null)
                {
                    return WidgetAxis.Z;
                }
                else if (NavPortal is not null)
                {
                    return WidgetAxis.Z;
                }
                else if (PathNode is not null)
                {
                    return WidgetAxis.None;
                }
                else if (TrainTrackNode is not null)
                {
                    return WidgetAxis.None;
                }
                else if (ScenarioNode is not null)
                {
                    return WidgetAxis.Z;
                }
                else if (Audio is not null)
                {
                    return WidgetAxis.Z;
                }
                return WidgetAxis.None;
            }
        }
        public readonly Vector3 WidgetScale
        {
            get
            {
                if (MultipleSelectionItems is not null)
                {
                    return MultipleSelectionScale;
                }
                else if (CollisionVertex is not null)
                {
                    return Vector3.One;
                }
                else if (CollisionPoly is not null)
                {
                    return CollisionPoly.Scale;
                }
                else if (CollisionBounds is not null)
                {
                    return CollisionBounds.Scale;
                }
                else if (EntityDef is not null)
                {
                    return EntityDef.Scale;
                }
                else if (CarGenerator is not null)
                {
                    return new Vector3(CarGenerator.CCarGen.perpendicularLength);
                }
                else if (LodLight is not null)
                {
                    return LodLight.Scale;
                }
                else if (BoxOccluder is not null)
                {
                    return BoxOccluder.Size;
                }
                else if (OccludeModelTri is not null)
                {
                    return OccludeModelTri.Scale;
                }
                else if (NavPoly is not null)
                {
                    return Vector3.One;
                }
                else if (NavPoint is not null)
                {
                    return Vector3.One;
                }
                else if (NavPortal is not null)
                {
                    return Vector3.One;
                }
                else if (PathNode is not null)
                {
                    return Vector3.One;
                }
                else if (TrainTrackNode is not null)
                {
                    return Vector3.One;
                }
                else if (ScenarioNode is not null)
                {
                    return Vector3.One;
                }
                else if (Audio is not null)
                {
                    return Vector3.One;
                }
                return Vector3.One;
            }
        }
        public readonly bool WidgetScaleLockXY
        {
            get
            {
                if (MultipleSelectionItems is not null)
                {
                    for (int i = 0; i < MultipleSelectionItems.Length; i++)
                    {
                        if ((MultipleSelectionItems[i].EntityDef is not null) && (MultipleSelectionItems[i].CollisionBounds is null)) return true;
                    }
                    return false;
                }
                if (BoxOccluder is not null)
                {
                    return false;
                }
                if (OccludeModelTri is not null)
                {
                    return false;
                }
                if (CollisionBounds is not null)
                {
                    return false;
                }
                if (CollisionPoly is not null)
                {
                    return false;
                }
                return true;
            }
        }

        public readonly bool CanCopyPaste
        {
            get
            {
                if (MultipleSelectionItems is not null)
                {
                    for (int i = 0; i < MultipleSelectionItems.Length; i++)
                    {
                        if (MultipleSelectionItems[i].CanCopyPaste)
                            return true;
                    }
                    return false;
                }
                else if (CollisionVertex is not null) return false;//can't copy just a vertex..
                else if (CollisionPoly is not null) return true;
                else if (CollisionBounds is not null) return true;
                else if (EntityDef is not null) return true;
                else if (CarGenerator is not null) return true;
                else if (LodLight is not null) return true;
                else if (BoxOccluder is not null) return true;
                else if (OccludeModelTri is not null) return true;
                else if (PathNode is not null) return true;
                else if (NavPoly is not null) return true;
                else if (NavPoint is not null) return true;
                else if (NavPortal is not null) return true;
                else if (TrainTrackNode is not null) return true;
                else if (ScenarioNode is not null) return true;
                else if (Audio?.AudioZone is not null) return true;
                else if (Audio?.AudioEmitter is not null) return true;
                return false;
            }
        }


        public void SetMultipleSelectionItems(MapSelection[]? items)
        {
            if (items is not null && items.Length == 0)
                items = null;

            MultipleSelectionItems = items;
            GatheredCollisionVerts = null;
            var center = Vector3.Zero;
            if (items is not null)
            {
                Dictionary<BoundVertex, int> collVerts = null;
                for (int i = 0; i < items.Length; i++)
                {
                    center += items[i].WidgetPosition;
                    var collVert = items[i].CollisionVertex;
                    var collPoly = items[i].CollisionPoly;
                    if (collVert is not null)
                    {
                        if (collVerts == null) collVerts = new Dictionary<BoundVertex, int>();
                        collVerts[collVert] = collVert.Index;
                    }
                    else if (collPoly is not null)
                    {
                        if (collVerts == null) collVerts = new Dictionary<BoundVertex, int>();
                        collPoly.GatherVertices(collVerts);
                    }
                }
                if (collVerts is not null)
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
            if (MultipleSelectionItems is not null)
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
                    if (ent is not null) dpos = Quaternion.Invert(ent.Orientation).Multiply(dpos);
                    if (GatheredCollisionVerts is not null)
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
            else if (CollisionVertex is not null)
            {
                if (EntityDef is not null) newpos = Quaternion.Invert(EntityDef.Orientation).Multiply(newpos - EntityDef.Position);
                CollisionVertex.Position = newpos;
            }
            else if (CollisionPoly is not null)
            {
                if (EntityDef is not null) newpos = Quaternion.Invert(EntityDef.Orientation).Multiply(newpos - EntityDef.Position);
                CollisionPoly.Position = newpos;
            }
            else if (CollisionBounds is not null)
            {
                if (EntityDef is not null) newpos = Quaternion.Invert(EntityDef.Orientation).Multiply(newpos - EntityDef.Position);
                CollisionBounds.Position = newpos;
            }
            else if (EntityDef is not null)
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
            else if (CarGenerator is not null)
            {
                CarGenerator.SetPosition(newpos);
            }
            else if (LodLight is not null)
            {
                LodLight.SetPosition(newpos);
            }
            else if (BoxOccluder is not null)
            {
                BoxOccluder.Position = newpos;
            }
            else if (OccludeModelTri is not null)
            {
                OccludeModelTri.Center = newpos;
            }
            else if (PathNode is not null)
            {
                PathNode.SetYndNodePosition(WorldForm.Space, newpos, out var affectedFiles);
                foreach (var affectedFile in affectedFiles)
                {
                    WorldForm.UpdatePathYndGraphics(affectedFile, false);
                }
            }
            else if (NavPoly is not null)
            {
                NavPoly.SetPosition(newpos);
            }
            else if (NavPoint is not null)
            {
                NavPoint.SetPosition(newpos);
            }
            else if (NavPortal is not null)
            {
                NavPortal.SetPosition(newpos);
            }
            else if (TrainTrackNode is not null)
            {
                TrainTrackNode.SetPosition(newpos);
            }
            else if (ScenarioNode is not null)
            {
                ScenarioNode.SetPosition(newpos);
            }
            else if (Audio is not null)
            {
                Audio.SetPosition(newpos);
            }

        }
        public void SetRotation(Quaternion newrot, bool editPivot)
        {
            if (MultipleSelectionItems is not null)
            {
                if (editPivot)
                {
                }
                else
                {
                    var cen = MultipleSelectionCenter;
                    var orinv = Quaternion.Invert(MultipleSelectionRotation);
                    var trans = newrot * orinv;
                    YmapEntityDef? ent = null;//hack to use an entity for multple selections... buggy if entities mismatch!!!
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
                    var eorinv = (ent is not null) ? Quaternion.Invert(ent.Orientation) : Quaternion.Identity;
                    if (GatheredCollisionVerts is not null)
                    {
                        for (int i = 0; i < GatheredCollisionVerts.Length; i++)
                        {
                            var refpos = GatheredCollisionVerts[i].Position;
                            var relpos = refpos - cen;
                            var newpos = trans.Multiply(relpos) + cen;
                            if (ent is not null)
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
            else if (CollisionVertex is not null)
            {
                //do nothing, but stop any poly from being rotated also
            }
            else if (CollisionPoly is not null)
            {
                if (EntityDef is not null) newrot = Quaternion.Normalize(Quaternion.Invert(EntityDef.Orientation) * newrot);
                CollisionPoly.Orientation = newrot;
            }
            else if (CollisionBounds is not null)
            {
                if (EntityDef is not null) newrot = Quaternion.Normalize(Quaternion.Invert(EntityDef.Orientation) * newrot);
                CollisionBounds.Orientation = newrot;
            }
            else if (EntityDef is not null)
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
            else if (CarGenerator is not null)
            {
                CarGenerator.SetOrientation(newrot);
            }
            else if (LodLight is not null)
            {
                LodLight.SetOrientation(newrot);
            }
            else if (BoxOccluder is not null)
            {
                BoxOccluder.Orientation = newrot;
            }
            else if (OccludeModelTri is not null)
            {
                OccludeModelTri.Orientation = newrot;
            }
            else if (ScenarioNode is not null)
            {
                ScenarioNode.SetOrientation(newrot);
            }
            else if (NavPoint is not null)
            {
                NavPoint.SetOrientation(newrot);
            }
            else if (NavPortal is not null)
            {
                NavPortal.SetOrientation(newrot);
            }
            else if (Audio is not null)
            {
                Audio.SetOrientation(newrot);
            }
        }
        public void SetScale(Vector3 newscale, bool editPivot)
        {
            if (MultipleSelectionItems is not null)
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
                    YmapEntityDef? ent = null;//hack to use an entity for multple selections... buggy if entities mismatch!!!
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
                    if (GatheredCollisionVerts is not null)
                    {
                        for (int i = 0; i < GatheredCollisionVerts.Length; i++)
                        {
                            var refpos = GatheredCollisionVerts[i].Position;
                            var relpos = refpos - cen;
                            var newpos = ori.Multiply(orinv.Multiply(relpos) * rsca) + cen;
                            if (ent is not null)
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
            else if (CollisionVertex is not null)
            {
                //do nothing, but stop any poly from being scaled also
            }
            else if (CollisionPoly is not null)
            {
                CollisionPoly.Scale = newscale;
            }
            else if (CollisionBounds is not null)
            {
                CollisionBounds.Scale = newscale;
            }
            else if (EntityDef is not null)
            {
                EntityDef.SetScale(newscale);
            }
            else if (CarGenerator is not null)
            {
                CarGenerator.SetScale(newscale);
                AABB = new BoundingBox(CarGenerator.BBMin, CarGenerator.BBMax);
            }
            else if (LodLight is not null)
            {
                LodLight.SetScale(newscale);
            }
            else if (BoxOccluder is not null)
            {
                BoxOccluder.SetSize(newscale);
            }
            else if (OccludeModelTri is not null)
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




        public readonly void UpdateGraphics(WorldForm wf)
        {
            if (MultipleSelectionItems is not null)
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
                    if (item.PathNode is not null)
                    {
                        pathYnds[item.PathNode.Ynd] = 1;
                    }
                    if (item.NavPoly is not null)
                    {
                        navYnvs[item.NavPoly.Ynv] = 1;
                    }
                    if (item.NavPoint is not null)
                    {
                        navYnvs[item.NavPoint.Ynv] = 1;
                    }
                    if (item.NavPortal is not null)
                    {
                        navYnvs[item.NavPortal.Ynv] = 1;
                    }
                    if (item.TrainTrackNode is not null)
                    {
                        trainTracks[item.TrainTrackNode.Track] = 1;
                    }
                    if (item.ScenarioNode is not null)
                    {
                        scenarioYmts[item.ScenarioNode.Ymt] = 1;
                    }
                    if (item.CollisionBounds is not null)
                    {
                        bounds[item.CollisionBounds] = 1;
                    }
                    if (item.CollisionPoly?.Owner is not null)
                    {
                        bounds[item.CollisionPoly.Owner] = 1;
                    }
                    if (item.CollisionVertex?.Owner is not null)
                    {
                        bounds[item.CollisionVertex.Owner] = 1;
                    }
                    if (item.LodLight?.LodLights is not null)
                    {
                        lodlights[item.LodLight.LodLights] = 1;
                    }
                    if (item.BoxOccluder is not null)
                    {
                        boxoccls[item.BoxOccluder] = 1;
                    }
                    if (item.OccludeModelTri?.Model is not null)
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
                    wf.UpdatePathYndGraphics(kvp.Key, false);
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
                if (PathNode is not null)
                {
                    wf.UpdatePathYndGraphics(PathNode.Ynd, false);
                }
                if (NavPoly is not null)
                {
                    wf.UpdateNavYnvGraphics(NavPoly.Ynv, true);
                }
                if (NavPoint is not null)
                {
                    wf.UpdateNavYnvGraphics(NavPoint.Ynv, true);
                }
                if (NavPortal is not null)
                {
                    wf.UpdateNavYnvGraphics(NavPortal.Ynv, true);
                }
                if (TrainTrackNode is not null)
                {
                    wf.UpdateTrainTrackGraphics(TrainTrackNode.Track, false);
                }
                if (ScenarioNode is not null)
                {
                    wf.UpdateScenarioGraphics(ScenarioNode.Ymt, false);
                }
                if (CollisionVertex?.Owner is not null)
                {
                    wf.UpdateCollisionBoundsGraphics(CollisionVertex.Owner);
                }
                else if (CollisionPoly?.Owner is not null)
                {
                    wf.UpdateCollisionBoundsGraphics(CollisionPoly.Owner);
                }
                else if (CollisionBounds is not null)
                {
                    wf.UpdateCollisionBoundsGraphics(CollisionBounds);
                }
                else if (LodLight is not null)
                {
                    wf.UpdateLodLightGraphics(LodLight);
                }
                else if (BoxOccluder is not null)
                {
                    wf.UpdateBoxOccluderGraphics(BoxOccluder);
                }
                else if (OccludeModelTri?.Model is not null)
                {
                    wf.UpdateOccludeModelGraphics(OccludeModelTri?.Model);
                }
            }
        }


        public readonly object GetProjectObject()
        {
            if (MultipleSelectionItems is not null) return MultipleSelectionItems;
            else if (CollisionVertex is not null) return CollisionVertex;
            else if (CollisionPoly is not null) return CollisionPoly;
            else if (CollisionBounds is not null) return CollisionBounds;
            else if (EntityDef is not null) return EntityDef;
            else if (CarGenerator is not null) return CarGenerator;
            else if (LodLight is not null) return LodLight;
            else if (BoxOccluder is not null) return BoxOccluder;
            else if (OccludeModelTri is not null) return OccludeModelTri;
            else if (PathNode is not null) return PathNode;
            else if (NavPoly is not null) return NavPoly;
            else if (NavPoint is not null) return NavPoint;
            else if (NavPortal is not null) return NavPortal;
            else if (TrainTrackNode is not null) return TrainTrackNode;
            else if (ScenarioNode is not null) return ScenarioNode;
            else if (Audio?.AudioZone is not null) return Audio;
            else if (Audio?.AudioEmitter is not null) return Audio;
            return null;
        }

        public static MapSelection FromProjectObject(WorldForm worldForm, object o, object? parent = null)
        {
            const float nrad = 0.5f;
            var ms = new MapSelection();
            ms.WorldForm = worldForm;
            if (o is object[] arr)
            {
                var multi = new MapSelection[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    multi[i] = FromProjectObject(worldForm, arr[i]);
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



        public override readonly string ToString() => GetFullNameString("[Empty]");
    }



}
