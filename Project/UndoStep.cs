using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Project
{
    public abstract class UndoStep
    {

        //revert the object to the state marked at the start of this step
        public abstract void Undo(WorldForm wf, ref MapSelection sel);

        //revert the object to the state marked at the end of this step
        public abstract void Redo(WorldForm wf, ref MapSelection sel);

    }


    public abstract class MultiItemUndoStep : UndoStep
    {
        protected MapSelection Selection;

        protected void UpdateGraphics(WorldForm wf)
        {
            Selection.UpdateGraphics(wf);
        }
    }
    public class MultiPositionUndoStep : MultiItemUndoStep
    {
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public MultiPositionUndoStep(MapSelection multiSel, Vector3 startpos, WorldForm wf)
        {
            Selection = multiSel;
            StartPosition = startpos;
            EndPosition = multiSel.WidgetPosition;

            UpdateGraphics(wf);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p, Vector3 o)
        {
            //update selection items positions for new widget position p

            Selection.MultipleSelectionCenter = o;
            Selection.SetPosition(p, false);

            sel.MultipleSelectionCenter = p; //center used for widget pos...

            wf.SelectMulti(Selection.MultipleSelectionItems);
            wf.SetWidgetPosition(p);

            UpdateGraphics(wf);
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition, EndPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition, StartPosition);
        }

        public override string ToString()
        {
            return (Selection.MultipleSelectionItems?.Length ?? 0).ToString() + " items: Position";
        }
    }
    public class MultiRotationUndoStep : MultiItemUndoStep
    {
        public Quaternion StartRotation { get; set; }
        public Quaternion EndRotation { get; set; }

        public MultiRotationUndoStep(MapSelection multiSel, Quaternion startrot, WorldForm wf)
        {
            Selection = multiSel;
            StartRotation = startrot;
            EndRotation = multiSel.WidgetRotation;

            UpdateGraphics(wf);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Quaternion r, Quaternion o)
        {
            //update selection items positions+rotations for new widget rotation r

            Selection.MultipleSelectionRotation = o;
            Selection.SetRotation(r, false);

            sel.MultipleSelectionRotation = r; //used for widget rot...

            wf.SelectMulti(Selection.MultipleSelectionItems);
            wf.SetWidgetRotation(r);

            UpdateGraphics(wf);
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartRotation, EndRotation);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndRotation, StartRotation);
        }

        public override string ToString()
        {
            return (Selection.MultipleSelectionItems?.Length ?? 0).ToString() + " items: Rotation";
        }
    }
    public class MultiScaleUndoStep : MultiItemUndoStep
    {
        public Vector3 StartScale { get; set; }
        public Vector3 EndScale { get; set; }

        public MultiScaleUndoStep(MapSelection multiSel, Vector3 startpos, WorldForm wf)
        {
            Selection = multiSel;
            StartScale = startpos;
            EndScale = multiSel.WidgetScale;

            UpdateGraphics(wf);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 s, Vector3 o)
        {
            //update selection items positions for new widget position p

            Selection.MultipleSelectionScale = o;
            Selection.SetScale(s, false);

            sel.MultipleSelectionScale = s; // used for widget scale...

            wf.SelectMulti(Selection.MultipleSelectionItems);
            wf.SetWidgetScale(s);

            UpdateGraphics(wf);
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartScale, EndScale);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndScale, StartScale);
        }

        public override string ToString()
        {
            return (Selection.MultipleSelectionItems?.Length ?? 0).ToString() + " items: Scale";
        }
    }



    public class EntityPositionUndoStep : UndoStep
    {
        public YmapEntityDef Entity { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public EntityPositionUndoStep(YmapEntityDef ent, Vector3 startpos)
        {
            Entity = ent;
            StartPosition = startpos;
            EndPosition = ent?.WidgetPosition ?? Vector3.Zero;
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            Entity?.SetPositionFromWidget(p);

            if (Entity != sel.EntityDef) wf.SelectObject(Entity);
            wf.SetWidgetPosition(Entity.WidgetPosition);
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return (Entity?._CEntityDef.archetypeName.ToString() ?? "") + ": Position";
        }
    }
    public class EntityRotationUndoStep : UndoStep
    {
        public YmapEntityDef Entity { get; set; }
        public Quaternion StartRotation { get; set; }
        public Quaternion EndRotation { get; set; }

        public EntityRotationUndoStep(YmapEntityDef ent, Quaternion startrot)
        {
            Entity = ent;
            StartRotation = startrot;
            EndRotation = ent?.WidgetOrientation ?? Quaternion.Identity;
        }


        private void Update(WorldForm wf, ref MapSelection sel, Quaternion q)
        {
            Entity?.SetOrientationFromWidget(q);

            if (Entity != sel.EntityDef) wf.SelectObject(Entity);
            wf.SetWidgetRotation(q);
        }


        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartRotation);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndRotation);
        }

        public override string ToString()
        {
            return (Entity?._CEntityDef.archetypeName.ToString() ?? "") + ": Rotation";
        }
    }
    public class EntityScaleUndoStep : UndoStep
    {
        public YmapEntityDef Entity { get; set; }
        public Vector3 StartScale { get; set; }
        public Vector3 EndScale { get; set; }

        public EntityScaleUndoStep(YmapEntityDef ent, Vector3 startscale)
        {
            Entity = ent;
            StartScale = startscale;
            EndScale = ent?.Scale ?? Vector3.One;
        }


        private void Update(WorldForm wf, ref MapSelection sel, Vector3 s)
        {
            Entity?.SetScale(s);

            if (Entity != sel.EntityDef) wf.SelectObject(Entity);
            wf.SetWidgetScale(s);
        }


        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartScale);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndScale);
        }

        public override string ToString()
        {
            return (Entity?._CEntityDef.archetypeName.ToString() ?? "") + ": Scale";
        }
    }



    public class EntityPivotPositionUndoStep : UndoStep
    {
        public YmapEntityDef Entity { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public EntityPivotPositionUndoStep(YmapEntityDef ent, Vector3 startpos)
        {
            Entity = ent;
            StartPosition = startpos;
            EndPosition = ent?.WidgetPosition ?? Vector3.Zero;
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            Entity?.SetPivotPositionFromWidget(p);

            if (Entity != sel.EntityDef) wf.SelectObject(Entity);
            wf.SetWidgetPosition(p);
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return (Entity?._CEntityDef.archetypeName.ToString() ?? "") + ": Pivot Position";
        }
    }
    public class EntityPivotRotationUndoStep : UndoStep
    {
        public YmapEntityDef Entity { get; set; }
        public Quaternion StartRotation { get; set; }
        public Quaternion EndRotation { get; set; }

        public EntityPivotRotationUndoStep(YmapEntityDef ent, Quaternion startrot)
        {
            Entity = ent;
            StartRotation = startrot;
            EndRotation = ent?.WidgetOrientation ?? Quaternion.Identity;
        }


        private void Update(WorldForm wf, ref MapSelection sel, Quaternion q)
        {
            Entity?.SetPivotOrientationFromWidget(q);

            if (Entity != sel.EntityDef) wf.SelectObject(Entity);
            wf.SetWidgetRotation(q);
        }


        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartRotation);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndRotation);
        }

        public override string ToString()
        {
            return (Entity?._CEntityDef.archetypeName.ToString() ?? "") + ": Pivot Rotation";
        }
    }



    public class CarGenPositionUndoStep : UndoStep
    {
        public YmapCarGen CarGen { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public CarGenPositionUndoStep(YmapCarGen cargen, Vector3 startpos)
        {
            CarGen = cargen;
            StartPosition = startpos;
            EndPosition = cargen?.Position ?? Vector3.Zero;
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            CarGen?.SetPosition(p);

            if (CarGen != sel.CarGenerator) wf.SelectObject(CarGen);
            wf.SetWidgetPosition(p);
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return "CarGen " + (CarGen?._CCarGen.carModel.ToString() ?? "") + ": Position";
        }
    }
    public class CarGenRotationUndoStep : UndoStep
    {
        public YmapCarGen CarGen { get; set; }
        public Quaternion StartRotation { get; set; }
        public Quaternion EndRotation { get; set; }

        public CarGenRotationUndoStep(YmapCarGen cargen, Quaternion startrot)
        {
            CarGen = cargen;
            StartRotation = startrot;
            EndRotation = cargen?.Orientation ?? Quaternion.Identity;
        }


        private void Update(WorldForm wf, ref MapSelection sel, Quaternion q)
        {
            CarGen?.SetOrientation(q);

            if (CarGen != sel.CarGenerator) wf.SelectObject(CarGen);
            wf.SetWidgetRotation(q);
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartRotation);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndRotation);
        }

        public override string ToString()
        {
            return "CarGen " + (CarGen?._CCarGen.carModel.ToString() ?? "") + ": Rotation";
        }
    }
    public class CarGenScaleUndoStep : UndoStep
    {
        public YmapCarGen CarGen { get; set; }
        public Vector3 StartScale { get; set; }
        public Vector3 EndScale { get; set; }

        public CarGenScaleUndoStep(YmapCarGen cargen, Vector3 startscale)
        {
            CarGen = cargen;
            StartScale = startscale;
            EndScale = new Vector3(cargen?._CCarGen.perpendicularLength ?? 1.0f);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 s)
        {
            CarGen?.SetScale(s);

            if (CarGen != sel.CarGenerator) wf.SelectObject(CarGen);
            wf.SetWidgetScale(s);
        }


        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartScale);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndScale);
        }

        public override string ToString()
        {
            return "CarGen " + (CarGen?._CCarGen.carModel.ToString() ?? "") + ": Scale";
        }
    }



    public class CollisionPositionUndoStep : UndoStep
    {
        public Bounds Bounds { get; set; }
        public YmapEntityDef Entity { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public CollisionPositionUndoStep(Bounds bounds, YmapEntityDef ent, Vector3 startpos, WorldForm wf)
        {
            Bounds = bounds;
            Entity = ent;
            StartPosition = startpos;
            EndPosition = bounds?.Position ?? Vector3.Zero;

            UpdateGraphics(wf);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            if (Bounds != null)
            {
                if (Entity != null)
                {
                    Bounds.Position = Quaternion.Invert(Entity.Orientation).Multiply(p - Entity.Position);
                }
                else
                {
                    Bounds.Position = p;
                }
            }


            if (Bounds != sel.CollisionBounds) wf.SelectObject(Bounds);
            wf.SetWidgetPosition(p);

            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (Bounds != null)
            {
                wf.UpdateCollisionBoundsGraphics(Bounds);
            }
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return "Collision " + (Bounds?.GetName() ?? "") + ": Position";
        }
    }
    public class CollisionRotationUndoStep : UndoStep
    {
        public Bounds Bounds { get; set; }
        public YmapEntityDef Entity { get; set; }
        public Quaternion StartRotation { get; set; }
        public Quaternion EndRotation { get; set; }

        public CollisionRotationUndoStep(Bounds bounds, YmapEntityDef ent, Quaternion startrot, WorldForm wf)
        {
            Bounds = bounds;
            Entity = ent;
            StartRotation = startrot;
            EndRotation = bounds?.Orientation ?? Quaternion.Identity;
            if (ent != null)
            {
                EndRotation = EndRotation * ent.Orientation;
            }

            UpdateGraphics(wf);
        }


        private void Update(WorldForm wf, ref MapSelection sel, Quaternion q)
        {
            if (Bounds != null)
            {
                if (Entity != null)
                {
                    Bounds.Orientation = Quaternion.Invert(Entity.Orientation) * q;
                }
                else
                {
                    Bounds.Orientation = q;
                }
            }

            if (Bounds != sel.CollisionBounds) wf.SelectObject(Bounds);
            wf.SetWidgetRotation(q);

            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (Bounds != null)
            {
                wf.UpdateCollisionBoundsGraphics(Bounds);
            }
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartRotation);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndRotation);
        }

        public override string ToString()
        {
            return "Collision " + (Bounds?.GetName() ?? "") + ": Rotation";
        }
    }
    public class CollisionScaleUndoStep : UndoStep
    {
        public Bounds Bounds { get; set; }
        public Vector3 StartScale { get; set; }
        public Vector3 EndScale { get; set; }

        public CollisionScaleUndoStep(Bounds bounds, Vector3 startsca, WorldForm wf)
        {
            Bounds = bounds;
            StartScale = startsca;
            EndScale = bounds?.Scale ?? Vector3.One;

            UpdateGraphics(wf);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 s)
        {
            if (Bounds != null)
            {
                Bounds.Scale = s;
            }

            if (Bounds != sel.CollisionBounds) wf.SelectObject(Bounds);
            wf.SetWidgetScale(s);

            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (Bounds != null)
            {
                wf.UpdateCollisionBoundsGraphics(Bounds);
            }
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartScale);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndScale);
        }

        public override string ToString()
        {
            return "Collision " + (Bounds?.GetName() ?? "") + ": Scale";
        }
    }

    public class CollisionPolyPositionUndoStep : UndoStep
    {
        public BoundPolygon Polygon { get; set; }
        public YmapEntityDef Entity { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public CollisionPolyPositionUndoStep(BoundPolygon poly, YmapEntityDef ent, Vector3 startpos, WorldForm wf)
        {
            Polygon = poly;
            Entity = ent;
            StartPosition = startpos;
            EndPosition = poly?.Position ?? Vector3.Zero;

            UpdateGraphics(wf);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            if (Polygon != null)
            {
                if (Entity != null)
                {
                    Polygon.Position = Quaternion.Invert(Entity.Orientation).Multiply(p - Entity.Position);
                }
                else
                {
                    Polygon.Position = p;
                }
            }

            if (Polygon != sel.CollisionPoly) wf.SelectObject(Polygon);
            wf.SetWidgetPosition(p);

            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (Polygon?.Owner != null)
            {
                wf.UpdateCollisionBoundsGraphics(Polygon.Owner);
            }
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return "Collision Poly " + (Polygon?.Index.ToString() ?? "") + ": Position";
        }
    }
    public class CollisionPolyRotationUndoStep : UndoStep
    {
        public BoundPolygon Polygon { get; set; }
        public YmapEntityDef Entity { get; set; }
        public Quaternion StartRotation { get; set; }
        public Quaternion EndRotation { get; set; }

        public CollisionPolyRotationUndoStep(BoundPolygon poly, YmapEntityDef ent, Quaternion startrot, WorldForm wf)
        {
            Polygon = poly;
            Entity = ent;
            StartRotation = startrot;
            EndRotation = poly?.Orientation ?? Quaternion.Identity;
            if (ent != null)
            {
                EndRotation = EndRotation * ent.Orientation;
            }

            UpdateGraphics(wf);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Quaternion q)
        {
            if (Polygon != null)
            {
                if (Entity != null)
                {
                    Polygon.Orientation = Quaternion.Invert(Entity.Orientation) * q;
                }
                else
                {
                    Polygon.Orientation = q;
                }
            }

            if (Polygon != sel.CollisionPoly) wf.SelectObject(Polygon);
            wf.SetWidgetRotation(q);

            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (Polygon?.Owner != null)
            {
                wf.UpdateCollisionBoundsGraphics(Polygon.Owner);
            }
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartRotation);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndRotation);
        }

        public override string ToString()
        {
            return "Collision Poly " + (Polygon?.Index.ToString() ?? "") + ": Rotation";
        }
    }
    public class CollisionPolyScaleUndoStep : UndoStep
    {
        public BoundPolygon Polygon { get; set; }
        public Vector3 StartScale { get; set; }
        public Vector3 EndScale { get; set; }

        public CollisionPolyScaleUndoStep(BoundPolygon poly, Vector3 startsca, WorldForm wf)
        {
            Polygon = poly;
            StartScale = startsca;
            EndScale = poly?.Scale ?? Vector3.One;

            UpdateGraphics(wf);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 s)
        {
            if (Polygon != null)
            {
                Polygon.Scale = s;
            }

            if (Polygon != sel.CollisionPoly) wf.SelectObject(Polygon);
            wf.SetWidgetScale(s);

            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (Polygon?.Owner != null)
            {
                wf.UpdateCollisionBoundsGraphics(Polygon.Owner);
            }
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartScale);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndScale);
        }

        public override string ToString()
        {
            return "Collision Poly " + (Polygon?.Index.ToString() ?? "") + ": Scale";
        }
    }

    public class CollisionVertexPositionUndoStep : UndoStep
    {
        public BoundVertex Vertex { get; set; }
        public YmapEntityDef Entity { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public CollisionVertexPositionUndoStep(BoundVertex vertex, YmapEntityDef ent, Vector3 startpos, WorldForm wf)
        {
            Vertex = vertex;
            Entity = ent;
            StartPosition = startpos;
            EndPosition = vertex?.Position ?? Vector3.Zero;

            UpdateGraphics(wf);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            if (Vertex != null)
            {
                if (Entity != null)
                {
                    Vertex.Position = Quaternion.Invert(Entity.Orientation).Multiply(p - Entity.Position);
                }
                else
                {
                    Vertex.Position = p;
                }
            }

            if (Vertex != sel.CollisionVertex) wf.SelectObject(Vertex);
            wf.SetWidgetPosition(p);

            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (Vertex?.Owner != null)
            {
                wf.UpdateCollisionBoundsGraphics(Vertex.Owner);
            }
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return "Collision Vertex " + (Vertex?.Index.ToString() ?? "") + ": Position";
        }
    }



    public class PathNodePositionUndoStep : UndoStep
    {
        public YndNode PathNode { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public PathNodePositionUndoStep(YndNode pathnode, Vector3 startpos, WorldForm wf)
        {
            PathNode = pathnode;
            StartPosition = startpos;
            EndPosition = pathnode?.Position ?? Vector3.Zero;

            UpdateGraphics(wf); //forces the update of the path graphics when it's moved...
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            PathNode?.SetPosition(p);

            if (PathNode != sel.PathNode)
            {
                if (sel.PathLink != null)
                {
                    wf.SelectObject(sel.PathLink);
                }
                else
                {
                    wf.SelectObject(PathNode);
                }
            }
            wf.SetWidgetPosition(p);


            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (PathNode != null)
            {
                //Ynd graphics needs to be updated.....
                wf.UpdatePathNodeGraphics(PathNode, false);
            }
        }


        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return "PathNode " + (PathNode?._RawData.ToString() ?? "") + ": Position";
        }
    }



    public class NavPointPositionUndoStep : UndoStep
    {
        public YnvPoint Point { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public NavPointPositionUndoStep(YnvPoint point, Vector3 startpos, WorldForm wf)
        {
            Point = point;
            StartPosition = startpos;
            EndPosition = point?.Position ?? Vector3.Zero;

            UpdateGraphics(wf); //forces the update of the path graphics when it's moved...
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            Point?.SetPosition(p);

            if (Point != sel.NavPoint)
            {
                wf.SelectObject(Point);
            }
            wf.SetWidgetPosition(p);


            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (Point != null)
            {
                //Ynv graphics needs to be updated.....
                wf.UpdateNavYnvGraphics(Point.Ynv, false);
            }
        }


        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return "NavPoint " + (Point?.ToString() ?? "") + ": Position";
        }
    }
    public class NavPointRotationUndoStep : UndoStep
    {
        public YnvPoint Point { get; set; }
        public Quaternion StartRotation { get; set; }
        public Quaternion EndRotation { get; set; }

        public NavPointRotationUndoStep(YnvPoint point, Quaternion startrot, WorldForm wf)
        {
            Point = point;
            StartRotation = startrot;
            EndRotation = point?.Orientation ?? Quaternion.Identity;

            //UpdateGraphics(wf);
        }


        private void Update(WorldForm wf, ref MapSelection sel, Quaternion q)
        {
            Point?.SetOrientation(q);

            if (Point != sel.NavPoint) wf.SelectObject(Point);
            wf.SetWidgetRotation(q);

            //UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            ////this function shouldn't actually be needed for rotating...
            //if (Point != null)
            //{
            //    //Ynv graphics needs to be updated.....
            //    wf.UpdateNavYnvGraphics(Point.Ynv, false);
            //}
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartRotation);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndRotation);
        }

        public override string ToString()
        {
            return "NavPoint " + (Point?.ToString() ?? "") + ": Rotation";
        }
    }

    public class NavPortalPositionUndoStep : UndoStep
    {
        public YnvPortal Portal { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public NavPortalPositionUndoStep(YnvPortal portal, Vector3 startpos, WorldForm wf)
        {
            Portal = portal;
            StartPosition = startpos;
            EndPosition = portal?.Position ?? Vector3.Zero;

            UpdateGraphics(wf); //forces the update of the path graphics when it's moved...
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            Portal?.SetPosition(p);

            if (Portal != sel.NavPortal)
            {
                wf.SelectObject(Portal);
            }
            wf.SetWidgetPosition(p);


            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (Portal != null)
            {
                //Ynv graphics needs to be updated.....
                wf.UpdateNavYnvGraphics(Portal.Ynv, false);
            }
        }


        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return "NavPortal " + (Portal?.ToString() ?? "") + ": Position";
        }
    }
    public class NavPortalRotationUndoStep : UndoStep
    {
        public YnvPortal Portal { get; set; }
        public Quaternion StartRotation { get; set; }
        public Quaternion EndRotation { get; set; }

        public NavPortalRotationUndoStep(YnvPortal portal, Quaternion startrot, WorldForm wf)
        {
            Portal = portal;
            StartRotation = startrot;
            EndRotation = portal?.Orientation ?? Quaternion.Identity;

            //UpdateGraphics(wf);
        }


        private void Update(WorldForm wf, ref MapSelection sel, Quaternion q)
        {
            Portal?.SetOrientation(q);

            if (Portal != sel.NavPortal) wf.SelectObject(Portal);
            wf.SetWidgetRotation(q);

            //UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            ////this function shouldn't actually be needed for rotating...
            //if (Point != null)
            //{
            //    //Ynv graphics needs to be updated.....
            //    wf.UpdateNavYnvGraphics(Point.Ynv, false);
            //}
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartRotation);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndRotation);
        }

        public override string ToString()
        {
            return "NavPortal " + (Portal?.ToString() ?? "") + ": Rotation";
        }
    }



    public class TrainTrackNodePositionUndoStep : UndoStep
    {
        public TrainTrackNode Node { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public TrainTrackNodePositionUndoStep(TrainTrackNode node, Vector3 startpos, WorldForm wf)
        {
            Node = node;
            StartPosition = startpos;
            EndPosition = node?.Position ?? Vector3.Zero;

            UpdateGraphics(wf); //forces the update of the path graphics when it's moved...
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            Node?.SetPosition(p);

            if (Node != sel.TrainTrackNode)
            {
                wf.SelectObject(Node);
            }
            wf.SetWidgetPosition(p);


            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (Node != null)
            {
                //Ynd graphics needs to be updated.....
                wf.UpdateTrainTrackNodeGraphics(Node, false);
            }
        }


        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return "TrainTrackNode " + (Node?.ToString() ?? "") + ": Position";
        }
    }



    public class ScenarioNodePositionUndoStep : UndoStep
    {
        public ScenarioNode ScenarioNode { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public ScenarioNodePositionUndoStep(ScenarioNode node, Vector3 startpos, WorldForm wf)
        {
            ScenarioNode = node;
            StartPosition = startpos;
            EndPosition = node?.Position ?? Vector3.Zero;

            UpdateGraphics(wf); //forces the update of the path graphics when it's moved...
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            ScenarioNode?.SetPosition(p);

            if (ScenarioNode != sel.ScenarioNode) wf.SelectObject(ScenarioNode);
            wf.SetWidgetPosition(p);

            UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            if (ScenarioNode != null)
            {
                //Ymt graphics needs to be updated.....
                wf.UpdateScenarioGraphics(ScenarioNode.Ymt, false);
            }
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return ScenarioNode.ToString() + ": Position";
        }
    }
    public class ScenarioNodeRotationUndoStep : UndoStep
    {
        public ScenarioNode ScenarioNode { get; set; }
        public Quaternion StartRotation { get; set; }
        public Quaternion EndRotation { get; set; }

        public ScenarioNodeRotationUndoStep(ScenarioNode node, Quaternion startrot, WorldForm wf)
        {
            ScenarioNode = node;
            StartRotation = startrot;
            EndRotation = node?.Orientation ?? Quaternion.Identity;

            //UpdateGraphics(wf);
        }


        private void Update(WorldForm wf, ref MapSelection sel, Quaternion q)
        {
            ScenarioNode?.SetOrientation(q);

            if (ScenarioNode != sel.ScenarioNode) wf.SelectObject(ScenarioNode);
            wf.SetWidgetRotation(q);

            //UpdateGraphics(wf);
        }

        private void UpdateGraphics(WorldForm wf)
        {
            ////this function shouldn't actually be needed for rotating...
            //if (ScenarioNode != null)
            //{
            //    //Ymt graphics needs to be updated.....
            //    wf.UpdateScenarioGraphics(ScenarioNode.Ymt, false);
            //}
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartRotation);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndRotation);
        }

        public override string ToString()
        {
            return ScenarioNode.ToString() + ": Rotation";
        }
    }



    public class AudioPositionUndoStep : UndoStep
    {
        public AudioPlacement Audio { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public AudioPositionUndoStep(AudioPlacement audio, Vector3 startpos)
        {
            Audio = audio;
            StartPosition = startpos;
            EndPosition = audio?.Position ?? Vector3.Zero;
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p)
        {
            Audio?.SetPosition(p);

            if (Audio != sel.Audio) wf.SelectObject(Audio);
            wf.SetWidgetPosition(p);
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartPosition);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndPosition);
        }

        public override string ToString()
        {
            return "Audio " + (Audio?.GetNameString() ?? "") + ": Position";
        }
    }
    public class AudioRotationUndoStep : UndoStep
    {
        public AudioPlacement Audio { get; set; }
        public Quaternion StartRotation { get; set; }
        public Quaternion EndRotation { get; set; }

        public AudioRotationUndoStep(AudioPlacement audio, Quaternion startrot)
        {
            Audio = audio;
            StartRotation = startrot;
            EndRotation = audio?.Orientation ?? Quaternion.Identity;
        }


        private void Update(WorldForm wf, ref MapSelection sel, Quaternion q)
        {
            Audio?.SetOrientation(q);

            if (Audio != sel.Audio) wf.SelectObject(Audio);
            wf.SetWidgetRotation(q);
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, StartRotation);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            Update(wf, ref sel, EndRotation);
        }

        public override string ToString()
        {
            return "Audio " + (Audio?.GetNameString() ?? "") + ": Rotation";
        }
    }


}
