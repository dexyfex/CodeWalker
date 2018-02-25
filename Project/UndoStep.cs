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



    public class MultiPositionUndoStep : UndoStep
    {
        private MapSelection Selection;
        private MapSelection[] Items;
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public MultiPositionUndoStep(MapSelection multiSel, MapSelection[] items, Vector3 startpos, WorldForm wf)
        {
            Selection = multiSel;
            Items = items;
            StartPosition = startpos;
            EndPosition = multiSel.WidgetPosition;

            UpdateGraphics(wf);
        }

        private void Update(WorldForm wf, ref MapSelection sel, Vector3 p, Vector3 o)
        {
            //update selection items positions for new widget position p

            Vector3 dp = p - o;
            for (int i = 0; i < Items.Length; i++)
            {
                var refpos = Items[i].WidgetPosition;
                Items[i].SetPosition(refpos + dp, false);
            }
            sel.MultipleSelectionCenter = p; //center used for widget pos...

            wf.SelectMulti(Items);
            wf.SetWidgetPosition(p);

            UpdateGraphics(wf);
        }


        private void UpdateGraphics(WorldForm wf)
        {

            Dictionary<YndFile, int> pathYnds = new Dictionary<YndFile, int>();
            Dictionary<TrainTrack, int> trainTracks = new Dictionary<TrainTrack, int>();
            Dictionary<YmtFile, int> scenarioYmts = new Dictionary<YmtFile, int>();

            if (Items != null)
            {
                foreach (var item in Items)
                {
                    if (item.PathNode != null)
                    {
                        pathYnds[item.PathNode.Ynd] = 1;
                    }
                    if (item.TrainTrackNode != null)
                    {
                        trainTracks[item.TrainTrackNode.Track] = 1;
                    }
                    if (item.ScenarioNode != null)
                    {
                        scenarioYmts[item.ScenarioNode.Ymt] = 1;
                    }
                }
            }

            foreach (var kvp in pathYnds)
            {
                wf.UpdatePathYndGraphics(kvp.Key, true);
            }
            foreach (var kvp in trainTracks)
            {
                wf.UpdateTrainTrackGraphics(kvp.Key, false);
            }
            foreach (var kvp in scenarioYmts)
            {
                wf.UpdateScenarioGraphics(kvp.Key, false);
            }

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
            return (Items?.Length ?? 0).ToString() + " items: Position";
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

            if (Entity != sel.EntityDef) wf.SelectEntity(Entity);
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

            if (Entity != sel.EntityDef) wf.SelectEntity(Entity);
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

            if (Entity != sel.EntityDef) wf.SelectEntity(Entity);
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

            if (Entity != sel.EntityDef) wf.SelectEntity(Entity);
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

            if (Entity != sel.EntityDef) wf.SelectEntity(Entity);
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

            if (CarGen != sel.CarGenerator) wf.SelectCarGen(CarGen);
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

            if (CarGen != sel.CarGenerator) wf.SelectCarGen(CarGen);
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

            if (CarGen != sel.CarGenerator) wf.SelectCarGen(CarGen);
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
                    wf.SelectPathLink(sel.PathLink);
                }
                else
                {
                    wf.SelectPathNode(PathNode);
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
                wf.SelectTrainTrackNode(Node);
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

            if (ScenarioNode != sel.ScenarioNode) wf.SelectScenarioNode(ScenarioNode);
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

            if (ScenarioNode != sel.ScenarioNode) wf.SelectScenarioNode(ScenarioNode);
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

            if (Audio != sel.Audio) wf.SelectAudio(Audio);
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

            if (Audio != sel.Audio) wf.SelectAudio(Audio);
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
