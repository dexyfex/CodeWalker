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
        DistantLodLights = 12,
        MloInstance = 13,
        Scenario = 14,
        PopZone = 15,
        Audio = 16,
        Occlusion = 17,
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
        public YmapBoxOccluder BoxOccluder { get; set; }
        public YmapOccludeModel OccludeModel { get; set; }
        public YmapEntityDef MloEntityDef { get; set; }
        public MCMloRoomDef MloRoomDef { get; set; }
        public WaterQuad WaterQuad { get; set; }
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
                    (DistantLodLights != null) ||
                    (BoxOccluder != null) ||
                    (OccludeModel != null) ||
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
                || (DistantLodLights != mhit.DistantLodLights)
                || (GrassBatch != mhit.GrassBatch)
                || (BoxOccluder != mhit.BoxOccluder)
                || (OccludeModel != mhit.OccludeModel)
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
                || (DistantLodLights != null)
                || (GrassBatch != null)
                || (BoxOccluder != null)
                || (OccludeModel != null)
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
            OccludeModel = null;
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
            DistantLodLights = null;
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
            if (MultipleSelectionItems != null)
            {
                name = "Multiple items";
            }
            else if (EntityDef != null)
            {
                name = EntityDef._CEntityDef.archetypeName.ToString();
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
            else if (DistantLodLights != null)
            {
                name = DistantLodLights.Ymap?.Name ?? "";
            }
            else if (BoxOccluder != null)
            {
                name = "BoxOccluder " + (BoxOccluder.Ymap?.Name ?? "") + ": " + BoxOccluder.Index.ToString();
            }
            else if (OccludeModel != null)
            {
                name = "OccludeModel " + (OccludeModel.Ymap?.Name ?? "") + ": " + OccludeModel.Index.ToString();
            }
            else if (CollisionVertex != null)
            {
                name = "Vertex " + CollisionVertex.Index.ToString() + ((CollisionBounds != null) ? (": " + CollisionBounds.GetName()) : string.Empty);
            }
            else if (CollisionPoly != null)
            {
                name = "Poly " + CollisionPoly.Index.ToString() + ((CollisionBounds != null) ? (": " + CollisionBounds.GetName()) : string.Empty);
            }
            else if (CollisionBounds != null)
            {
                name = CollisionBounds.GetName();
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
            if (CollisionBounds != null) return true;
            if (CollisionPoly != null) return true;
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
            else if (CollisionVertex != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CollisionVertexPositionUndoStep(CollisionVertex, startPos, wf);
                }
            }
            else if (CollisionPoly != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CollisionPolyPositionUndoStep(CollisionPoly, startPos, wf);
                    case WidgetMode.Rotation: return new CollisionPolyRotationUndoStep(CollisionPoly, startRot, wf);
                    case WidgetMode.Scale: return new CollisionPolyScaleUndoStep(CollisionPoly, startScale, wf);
                }
            }
            else if (CollisionBounds != null)
            {
                switch (mode)
                {
                    case WidgetMode.Position: return new CollisionPositionUndoStep(CollisionBounds, startPos, wf);
                    case WidgetMode.Rotation: return new CollisionRotationUndoStep(CollisionBounds, startRot, wf);
                    case WidgetMode.Scale: return new CollisionScaleUndoStep(CollisionBounds, startScale, wf);
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
                else if (EntityDef != null)
                {
                    return EntityDef.WidgetPosition;
                }
                else if (CarGenerator != null)
                {
                    return CarGenerator.Position;
                }
                else if (CollisionVertex != null)
                {
                    return CollisionVertex.Position;
                }
                else if (CollisionPoly != null)
                {
                    return CollisionPoly.Position;
                }
                else if (CollisionBounds != null)
                {
                    return CollisionBounds.Position;
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
                else if (EntityDef != null)
                {
                    return EntityDef.WidgetOrientation;
                }
                else if (CarGenerator != null)
                {
                    return CarGenerator.Orientation;
                }
                else if (CollisionVertex != null)
                {
                    return Quaternion.Identity;
                }
                else if (CollisionPoly != null)
                {
                    return CollisionPoly.Orientation;
                }
                else if (CollisionBounds != null)
                {
                    return CollisionBounds.Orientation;
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
                else if (EntityDef != null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (CarGenerator != null)
                {
                    return WidgetAxis.Z;
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
                else if (EntityDef != null)
                {
                    return EntityDef.Scale;
                }
                else if (CarGenerator != null)
                {
                    return new Vector3(CarGenerator.CCarGen.perpendicularLength);
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
                        if (MultipleSelectionItems[i].EntityDef != null) return true;
                    }
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
                else if (EntityDef != null) return true;
                else if (CarGenerator != null) return true;
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
                    var collPoly = items[i].CollisionPoly;
                    if (collPoly != null)
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
                    for (int i = 0; i < MultipleSelectionItems.Length; i++)
                    {
                        if (MultipleSelectionItems[i].CollisionPoly == null)//skip polys, they use gathered verts
                        {
                            var refpos = MultipleSelectionItems[i].WidgetPosition;
                            MultipleSelectionItems[i].SetPosition(refpos + dpos, false);
                        }
                    }
                    if (GatheredCollisionVerts != null)
                    {
                        for (int i = 0; i < GatheredCollisionVerts.Length; i++)
                        {
                            var refpos = GatheredCollisionVerts[i].Position;
                            GatheredCollisionVerts[i].Position = refpos + dpos;
                        }
                    }
                    MultipleSelectionCenter = newpos;
                }
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
            else if (CollisionVertex != null)
            {
                CollisionVertex.Position = newpos;
            }
            else if (CollisionPoly != null)
            {
                CollisionPoly.Position = newpos;
            }
            else if (CollisionBounds != null)
            {
                CollisionBounds.Position = newpos;
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
                    for (int i = 0; i < MultipleSelectionItems.Length; i++)
                    {
                        if (MultipleSelectionItems[i].CollisionPoly == null)//skip polys, they use gathered verts
                        {
                            var refpos = MultipleSelectionItems[i].WidgetPosition;
                            var relpos = refpos - cen;
                            var newpos = trans.Multiply(relpos) + cen;
                            var refori = MultipleSelectionItems[i].WidgetRotation;
                            var newori = trans * refori;
                            MultipleSelectionItems[i].SetPosition(newpos, false);
                            MultipleSelectionItems[i].SetRotation(newori, false);
                        }
                    }
                    if (GatheredCollisionVerts != null)
                    {
                        for (int i = 0; i < GatheredCollisionVerts.Length; i++)
                        {
                            var refpos = GatheredCollisionVerts[i].Position;
                            var relpos = refpos - cen;
                            var newpos = trans.Multiply(relpos) + cen;
                            GatheredCollisionVerts[i].Position = newpos;
                        }
                    }
                    MultipleSelectionRotation = newrot;
                }
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
            else if (CollisionVertex != null)
            {
                //do nothing, but stop any poly from being rotated also
            }
            else if (CollisionPoly != null)
            {
                CollisionPoly.Orientation = newrot;
            }
            else if (CollisionBounds != null)
            {
                CollisionBounds.Orientation = newrot;
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
                    for (int i = 0; i < MultipleSelectionItems.Length; i++)
                    {
                        if (MultipleSelectionItems[i].CollisionPoly == null)//skip polys, they use gathered verts
                        {
                            var refpos = MultipleSelectionItems[i].WidgetPosition;
                            var relpos = refpos - cen;
                            var newpos = ori.Multiply(orinv.Multiply(relpos) * rsca) + cen;
                            MultipleSelectionItems[i].SetPosition(newpos, false);
                            MultipleSelectionItems[i].SetScale(newscale, false);
                        }
                    }
                    if (GatheredCollisionVerts != null)
                    {
                        for (int i = 0; i < GatheredCollisionVerts.Length; i++)
                        {
                            var refpos = GatheredCollisionVerts[i].Position;
                            var relpos = refpos - cen;
                            var newpos = ori.Multiply(orinv.Multiply(relpos) * rsca) + cen;
                            GatheredCollisionVerts[i].Position = newpos;
                        }
                    }
                    MultipleSelectionScale = newscale;
                }
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



        public override string ToString()
        {
            return GetFullNameString("[Empty]");
        }
    }



}
