using CodeWalker.GameFiles;
using CodeWalker.World;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Project.Panels
{
    public partial class ProjectExplorerPanel : ProjectPanel
    {
        public ProjectForm ProjectForm { get; set; }
        public ProjectFile CurrentProjectFile { get; set; }

        private bool inDoubleClick = false; //used in disabling double-click to expand tree nodes

        public ProjectExplorerPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }


        public void LoadProjectTree(ProjectFile projectFile)
        {
            ProjectTreeView.Nodes.Clear();

            CurrentProjectFile = projectFile;
            if (CurrentProjectFile == null) return;

            var pcstr = CurrentProjectFile.HasChanged ? "*" : "";

            var projnode = ProjectTreeView.Nodes.Add(pcstr + CurrentProjectFile.Name);
            projnode.Tag = CurrentProjectFile;


            if (CurrentProjectFile.YmapFiles.Count > 0)
            {
                var ymapsnode = projnode.Nodes.Add("Ymap Files");
                ymapsnode.Name = "Ymap";

                foreach (var ymapfile in CurrentProjectFile.YmapFiles)
                {
                    var ycstr = ymapfile.HasChanged ? "*" : "";
                    string name = ymapfile.Name;
                    if (ymapfile.RpfFileEntry != null)
                    {
                        name = ymapfile.RpfFileEntry.Name;
                    }
                    var ymapnode = ymapsnode.Nodes.Add(ycstr + name);
                    ymapnode.Tag = ymapfile;

                    LoadYmapTreeNodes(ymapfile, ymapnode);

                    JenkIndex.Ensure(name);
                    JenkIndex.Ensure(Path.GetFileNameWithoutExtension(name));
                }
                ymapsnode.Expand();
            }

            if (CurrentProjectFile.YtypFiles.Count > 0)
            {
                var ytypsnode = projnode.Nodes.Add("Ytyp Files");
                ytypsnode.Name = "Ytyp";

                foreach (var ytypfile in CurrentProjectFile.YtypFiles)
                {
                    var ycstr = ytypfile.HasChanged ? "*" : "";
                    string name = ytypfile.Name;
                    if (ytypfile.RpfFileEntry != null)
                    {
                        name = ytypfile.RpfFileEntry.Name;
                    }
                    var ytypnode = ytypsnode.Nodes.Add(ycstr + name);
                    ytypnode.Tag = ytypfile;

                    LoadYtypTreeNodes(ytypfile, ytypnode);

                    JenkIndex.Ensure(name);
                    JenkIndex.Ensure(Path.GetFileNameWithoutExtension(name));
                }
                ytypsnode.Expand();
            }

            if (CurrentProjectFile.YndFiles.Count > 0)
            {
                var yndsnode = projnode.Nodes.Add("Ynd Files");
                yndsnode.Name = "Ynd";

                foreach (var yndfile in CurrentProjectFile.YndFiles)
                {
                    var ycstr = yndfile.HasChanged ? "*" : "";
                    string name = yndfile.Name;
                    if (yndfile.RpfFileEntry != null)
                    {
                        name = yndfile.RpfFileEntry.Name;
                    }
                    var yndnode = yndsnode.Nodes.Add(ycstr + name);
                    yndnode.Tag = yndfile;

                    LoadYndTreeNodes(yndfile, yndnode);
                }
                yndsnode.Expand();
            }

            if (CurrentProjectFile.YnvFiles.Count > 0)
            {
                var ynvsnode = projnode.Nodes.Add("Ynv Files");
                ynvsnode.Name = "Ynv";

                foreach (var ynvfile in CurrentProjectFile.YnvFiles)
                {
                    var ycstr = ynvfile.HasChanged ? "*" : "";
                    string name = ynvfile.Name;
                    if (ynvfile.RpfFileEntry != null)
                    {
                        name = ynvfile.RpfFileEntry.Name;
                    }
                    var ynvnode = ynvsnode.Nodes.Add(ycstr + name);
                    ynvnode.Tag = ynvfile;

                    LoadYnvTreeNodes(ynvfile, ynvnode);
                }
                ynvsnode.Expand();
            }

            if (CurrentProjectFile.TrainsFiles.Count > 0)
            {
                var trainsnode = projnode.Nodes.Add("Trains Files");
                trainsnode.Name = "Trains";

                foreach (var trainfile in CurrentProjectFile.TrainsFiles)
                {
                    var tcstr = trainfile.HasChanged ? "*" : "";
                    string name = trainfile.Name;
                    if (trainfile.RpfFileEntry != null)
                    {
                        name = trainfile.RpfFileEntry.Name;
                    }
                    var trainnode = trainsnode.Nodes.Add(tcstr + name);
                    trainnode.Tag = trainfile;

                    LoadTrainTrackTreeNodes(trainfile, trainnode);
                }
                trainsnode.Expand();
            }

            if (CurrentProjectFile.ScenarioFiles.Count > 0)
            {
                var scenariosnode = projnode.Nodes.Add("Scenario Files");
                scenariosnode.Name = "Scenarios";

                foreach (var scenariofile in CurrentProjectFile.ScenarioFiles)
                {
                    var scstr = scenariofile.HasChanged ? "*" : "";
                    string name = scenariofile.Name;
                    if (scenariofile.RpfFileEntry != null)
                    {
                        name = scenariofile.RpfFileEntry.Name;
                    }
                    var scenarionode = scenariosnode.Nodes.Add(scstr + name);
                    scenarionode.Tag = scenariofile;

                    LoadScenarioTreeNodes(scenariofile, scenarionode);
                }
                scenariosnode.Expand();
            }

            if (CurrentProjectFile.AudioRelFiles.Count > 0)
            {
                var audiorelsnode = projnode.Nodes.Add("Audio Rel Files");
                audiorelsnode.Name = "AudioRels";

                foreach (var audiorelfile in CurrentProjectFile.AudioRelFiles)
                {
                    var acstr = audiorelfile.HasChanged ? "*" : "";
                    string name = audiorelfile.Name;
                    if (audiorelfile.RpfFileEntry != null)
                    {
                        name = audiorelfile.RpfFileEntry.Name;
                    }
                    var audiorelnode = audiorelsnode.Nodes.Add(acstr + name);
                    audiorelnode.Tag = audiorelfile;

                    LoadAudioRelTreeNodes(audiorelfile, audiorelnode);
                }
                audiorelsnode.Expand();
            }

            projnode.Expand();

        }

        private void LoadYmapTreeNodes(YmapFile ymap, TreeNode node)
        {
            if (ymap == null) return;

            if (!string.IsNullOrEmpty(node.Name)) return; //named nodes are eg Entities and CarGens

            node.Nodes.Clear();

            if ((ymap.AllEntities != null) && (ymap.AllEntities.Length > 0))
            {
                var entsnode = node.Nodes.Add("Entities (" + ymap.AllEntities.Length.ToString() + ")");
                entsnode.Name = "Entities";
                entsnode.Tag = ymap;
                var ents = ymap.AllEntities;
                for (int i = 0; i < ents.Length; i++)
                {
                    var ent = ents[i];
                    var edef = ent.CEntityDef;
                    var enode = entsnode.Nodes.Add(edef.archetypeName.ToString());
                    enode.Tag = ent;
                }
            }
            if ((ymap.CarGenerators != null) && (ymap.CarGenerators.Length > 0))
            {
                var cargensnode = node.Nodes.Add("Car Generators (" + ymap.CarGenerators.Length.ToString() + ")");
                cargensnode.Name = "CarGens";
                cargensnode.Tag = ymap;
                var cargens = ymap.CarGenerators;
                for (int i = 0; i < cargens.Length; i++)
                {
                    var cargen = cargens[i];
                    var ccgnode = cargensnode.Nodes.Add(cargen.ToString());
                    ccgnode.Tag = cargen;
                }
            }
            if ((ymap.GrassInstanceBatches != null) && (ymap.GrassInstanceBatches.Length > 0))
            {
                var grassbatchesnodes = node.Nodes.Add("Grass Batches (" + ymap.GrassInstanceBatches.Length.ToString() + ")");
                grassbatchesnodes.Name = "GrassBatches";
                grassbatchesnodes.Tag = ymap;
                var grassbatches = ymap.GrassInstanceBatches;
                for (int i = 0; i < grassbatches.Length; i++)
                {
                    var batch = grassbatches[i];
                    var gbnode = grassbatchesnodes.Nodes.Add(batch.ToString());
                    gbnode.Tag = batch;
                }
            }

        }
        private void LoadYtypTreeNodes(YtypFile ytyp, TreeNode node)
        {
            if (ytyp == null) return;

            if (!string.IsNullOrEmpty(node.Name)) return;

            node.Nodes.Clear();

            if ((ytyp.AllArchetypes != null) && (ytyp.AllArchetypes.Length > 0))
            {
                var archetypesnode = node.Nodes.Add("Archetypes (" + ytyp.AllArchetypes.Length.ToString() + ")");
                archetypesnode.Name = "Archetypes";
                archetypesnode.Tag = ytyp;
                var archetypes = ytyp.AllArchetypes;
                for (int i = 0; i < archetypes.Length; i++)
                {
                    var yarch = archetypes[i];
                    var tarch = archetypesnode.Nodes.Add(yarch.Name);
                    tarch.Tag = yarch;

                    if (yarch is MloArchetype mlo)
                    {
                        if ((mlo.entities.Length) > 0 && (mlo.rooms.Length > 0))
                        {
                            MCEntityDef[] entities = mlo.entities;
                            var roomsnode = tarch.Nodes.Add("Rooms (" + mlo.rooms.Length.ToString() + ")");
                            roomsnode.Name = "Rooms";
                            for (int j = 0; j < mlo.rooms.Length; j++)
                            {
                                MCMloRoomDef room = mlo.rooms[j];
                                var roomnode = roomsnode.Nodes.Add(room.RoomName);
                                roomnode.Tag = room;
                                var entitiesnode = roomnode.Nodes.Add("Attached Objects (" + room.AttachedObjects.Length + ")");
                                entitiesnode.Name = "Attached Objects";

                                for (int k = 0; k < room.AttachedObjects.Length; k++)
                                {
                                    uint attachedObject = room.AttachedObjects[k];
                                    MCEntityDef ent = entities[attachedObject];
                                    TreeNode entnode = entitiesnode.Nodes.Add(ent.ToString());
                                    entnode.Tag = ent;
                                }
                            }
                        }
                    }
                }
            }
        }
        private void LoadYndTreeNodes(YndFile ynd, TreeNode node)
        {
            if (ynd == null) return;

            if (!string.IsNullOrEmpty(node.Name)) return; //named nodes are eg Nodes

            node.Nodes.Clear();



            if ((ynd.Nodes != null) && (ynd.Nodes.Length > 0))
            {
                var nodesnode = node.Nodes.Add("Nodes (" + ynd.Nodes.Length.ToString() + ")");
                nodesnode.Name = "Nodes";
                nodesnode.Tag = ynd;
                var nodes = ynd.Nodes;
                for (int i = 0; i < nodes.Length; i++)
                {
                    var ynode = nodes[i];
                    var nnode = ynode.RawData;
                    var tnode = nodesnode.Nodes.Add(nnode.ToString());
                    tnode.Tag = ynode;
                }
            }

        }
        private void LoadYnvTreeNodes(YnvFile ynv, TreeNode node)//TODO!
        {
            if (ynv == null) return;

            if (!string.IsNullOrEmpty(node.Name)) return; //named nodes are eg Polygons

            node.Nodes.Clear();


            TreeNode n;
            n = node.Nodes.Add("Edit Polygon");
            n.Name = "EditPoly";
            n.Tag = ynv; //this tag should get updated with the selected poly!

            n = node.Nodes.Add("Edit Portal");
            n.Name = "EditPortal";
            n.Tag = ynv; //this tag should get updated with the selected portal!

            n = node.Nodes.Add("Edit Point");
            n.Name = "EditPoint";
            n.Tag = ynv; //this tag should get updated with the selected point!


        }
        private void LoadTrainTrackTreeNodes(TrainTrack track, TreeNode node)
        {
            if (track == null) return;

            if (!string.IsNullOrEmpty(node.Name)) return; //named nodes are eg Nodes

            node.Nodes.Clear();



            if ((track.Nodes != null) && (track.Nodes.Count > 0))
            {
                var nodesnode = node.Nodes.Add("Nodes (" + track.Nodes.Count.ToString() + ")");
                nodesnode.Name = "Nodes";
                nodesnode.Tag = track;
                var nodes = track.Nodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var ynode = nodes[i];
                    var tnode = nodesnode.Nodes.Add(ynode.ToString());
                    tnode.Tag = ynode;
                }
            }

        }
        private void LoadScenarioTreeNodes(YmtFile ymt, TreeNode node)
        {
            if (!string.IsNullOrEmpty(node.Name)) return; //named nodes are eg Points

            node.Nodes.Clear();

            var region = ymt?.ScenarioRegion;

            if (region == null) return;

            var nodes = region.Nodes;
            if ((nodes == null) || (nodes.Count == 0)) return;

            var pointsnode = node.Nodes.Add("Points (" + nodes.Count.ToString() + ")");
            pointsnode.Name = "Points";
            pointsnode.Tag = ymt;
            for (int i = 0; i < nodes.Count; i++)
            {
                var snode = nodes[i];
                var tnode = pointsnode.Nodes.Add(snode.MedTypeName + ": " + snode.StringText);
                tnode.Tag = snode;
            }

            //var sr = region.Region;
            //if (sr == null) return;
            //int pointCount = (sr.Points?.LoadSavePoints?.Length ?? 0) + (sr.Points?.MyPoints?.Length ?? 0);
            //int entityOverrideCount = (sr.EntityOverrides?.Length ?? 0);
            //int chainCount = (sr.Paths?.Chains?.Length ?? 0);
            //int clusterCount = (sr.Clusters?.Length ?? 0);
            //TreeNode pointsNode = null;
            //TreeNode entityOverridesNode = null;
            //TreeNode chainsNode = null;
            //TreeNode clustersNode = null;
            //if (pointCount > 0)
            //{
            //    pointsNode = node.Nodes.Add("Points (" + pointCount.ToString() + ")");
            //}
            //if (entityOverrideCount > 0)
            //{
            //    entityOverridesNode = node.Nodes.Add("Entity Overrides (" + entityOverrideCount.ToString() + ")");
            //}
            //if (chainCount > 0)
            //{
            //    chainsNode = node.Nodes.Add("Chains (" + chainsNode.ToString() + ")");
            //}
            //if (clusterCount > 0)
            //{
            //    clustersNode = node.Nodes.Add("Clusters (" + clusterCount.ToString() + ")");
            //}
            //for (int i = 0; i < nodes.Count; i++)
            //{
            //    var snode = nodes[i];
            //    if (snode == null) continue;
            //    if ((pointsNode != null) && ((snode.LoadSavePoint != null) || (snode.MyPoint != null)))
            //    {
            //        pointsNode.Nodes.Add(snode.ToString()).Tag = snode;
            //    }
            //    if ((entityOverridesNode != null) && ((snode.EntityOverride != null) || (snode.EntityPoint != null)))
            //    {
            //        entityOverridesNode.Nodes.Add(snode.ToString()).Tag = snode;
            //    }
            //    if ((chainsNode != null) && (snode.ChainingNode != null))
            //    {
            //        chainsNode.Nodes.Add(snode.ToString()).Tag = snode;
            //    }
            //    if ((clustersNode != null) && ((snode.Cluster != null) || (snode.ClusterLoadSavePoint != null) || (snode.ClusterMyPoint != null)))
            //    {
            //        clustersNode.Nodes.Add(snode.ToString()).Tag = snode;
            //    }
            //}

        }
        private void LoadAudioRelTreeNodes(RelFile rel, TreeNode node)
        {
            if (!string.IsNullOrEmpty(node.Name)) return; //named nodes are eg Zones, Emitters

            node.Nodes.Clear();


            if (rel.RelDatasSorted == null) return; //nothing to see here


            var zones = new List<Dat151AmbientZone>();
            var emitters = new List<Dat151AmbientEmitter>();
            var zonelists = new List<Dat151AmbientZoneList>();
            var emitterlists = new List<Dat151AmbientEmitterList>();

            foreach (var reldata in rel.RelDatasSorted)
            {
                if (reldata is Dat151AmbientZone)
                {
                    zones.Add(reldata as Dat151AmbientZone);
                }
                if (reldata is Dat151AmbientEmitter)
                {
                    emitters.Add(reldata as Dat151AmbientEmitter);
                }
                if (reldata is Dat151AmbientZoneList)
                {
                    zonelists.Add(reldata as Dat151AmbientZoneList);
                }
                if (reldata is Dat151AmbientEmitterList)
                {
                    emitterlists.Add(reldata as Dat151AmbientEmitterList);
                }
            }



            if (zones.Count > 0)
            {
                var n = node.Nodes.Add("Ambient Zones (" + zones.Count.ToString() + ")");
                n.Name = "AmbientZones";
                n.Tag = rel;

                for (int i = 0; i < zones.Count; i++)
                {
                    var zone = zones[i];
                    var tnode = n.Nodes.Add(zone.NameHash.ToString());
                    tnode.Tag = zone;
                }
            }


            if (emitters.Count > 0)
            {
                var n = node.Nodes.Add("Ambient Emitters (" + emitters.Count.ToString() + ")");
                n.Name = "AmbientEmitters";
                n.Tag = rel;

                for (int i = 0; i < emitters.Count; i++)
                {
                    var emitter = emitters[i];
                    var tnode = n.Nodes.Add(emitter.NameHash.ToString());
                    tnode.Tag = emitter;
                }
            }



            if (zonelists.Count > 0)
            {
                var zonelistsnode = node.Nodes.Add("Ambient Zone Lists (" + zonelists.Count.ToString() + ")");
                zonelistsnode.Name = "AmbientZoneLists";
                zonelistsnode.Tag = rel;
                for (int i = 0; i < zonelists.Count; i++)
                {
                    var zonelist = zonelists[i];
                    var tnode = zonelistsnode.Nodes.Add(zonelist.NameHash.ToString());
                    tnode.Tag = zonelist;
                }
            }

            if (emitterlists.Count > 0)
            {
                var emitterlistsnode = node.Nodes.Add("Ambient Emitter Lists (" + emitterlists.Count.ToString() + ")");
                emitterlistsnode.Name = "AmbientEmitterLists";
                emitterlistsnode.Tag = rel;
                for (int i = 0; i < emitterlists.Count; i++)
                {
                    var emitterlist = emitterlists[i];
                    var tnode = emitterlistsnode.Nodes.Add(emitterlist.NameHash.ToString());
                    tnode.Tag = emitterlist;
                }
            }


        }



        public void SetProjectHasChanged(bool changed)
        {
            if ((ProjectTreeView.Nodes.Count > 0) && (CurrentProjectFile != null))
            {
                //first node is the project...
                string changestr = changed ? "*" : "";
                ProjectTreeView.Nodes[0].Text = changestr + CurrentProjectFile.Name;
            }
        }
        public void SetYmapHasChanged(YmapFile ymap, bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var ymnode = GetChildTreeNode(pnode, "Ymap");
                if (ymnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < ymnode.Nodes.Count; i++)
                {
                    var ynode = ymnode.Nodes[i];
                    if (ynode.Tag == ymap)
                    {
                        string name = ymap.Name;
                        if (ymap.RpfFileEntry != null)
                        {
                            name = ymap.RpfFileEntry.Name;
                        }
                        ynode.Text = changestr + name;
                        break;
                    }
                }
            }
        }
        public void SetYtypHasChanged(YtypFile ytyp, bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var ytnode = GetChildTreeNode(pnode, "Ytyp");
                if (ytnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < ytnode.Nodes.Count; i++)
                {
                    var ynode = ytnode.Nodes[i];
                    if (ynode.Tag == ytyp)
                    {
                        string name = ytyp.Name;
                        if (ytyp.RpfFileEntry != null)
                        {
                            name = ytyp.RpfFileEntry.Name;
                        }
                        ynode.Text = changestr + name;
                        break;
                    }
                }
            }
        }
        public void SetYndHasChanged(YndFile ynd, bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var ynnode = GetChildTreeNode(pnode, "Ynd");
                if (ynnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < ynnode.Nodes.Count; i++)
                {
                    var ynode = ynnode.Nodes[i];
                    if (ynode.Tag == ynd)
                    {
                        string name = ynd.Name;
                        if (ynd.RpfFileEntry != null)
                        {
                            name = ynd.RpfFileEntry.Name;
                        }
                        ynode.Text = changestr + name;
                        break;
                    }
                }
            }
        }
        public void SetYnvHasChanged(YnvFile ynv, bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var ynnode = GetChildTreeNode(pnode, "Ynv");
                if (ynnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < ynnode.Nodes.Count; i++)
                {
                    var ynode = ynnode.Nodes[i];
                    if (ynode.Tag == ynv)
                    {
                        string name = ynv.Name;
                        if (ynv.RpfFileEntry != null)
                        {
                            name = ynv.RpfFileEntry.Name;
                        }
                        ynode.Text = changestr + name;
                        break;
                    }
                }
            }
        }
        public void SetTrainTrackHasChanged(TrainTrack track, bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var trnode = GetChildTreeNode(pnode, "Trains");
                if (trnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < trnode.Nodes.Count; i++)
                {
                    var tnode = trnode.Nodes[i];
                    if (tnode.Tag == track)
                    {
                        string name = track.Name;
                        if (track.RpfFileEntry != null)
                        {
                            name = track.RpfFileEntry.Name;
                        }
                        tnode.Text = changestr + name;
                        break;
                    }
                }
            }
        }
        public void SetScenarioHasChanged(YmtFile scenario, bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var scnode = GetChildTreeNode(pnode, "Scenarios");
                if (scnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < scnode.Nodes.Count; i++)
                {
                    var snode = scnode.Nodes[i];
                    if (snode.Tag == scenario)
                    {
                        string name = scenario.Name;
                        if (scenario.RpfFileEntry != null)
                        {
                            name = scenario.RpfFileEntry.Name;
                        }
                        snode.Text = changestr + name;
                        break;
                    }
                }
            }
        }
        public void SetAudioRelHasChanged(RelFile rel, bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var pnode = ProjectTreeView.Nodes[0];
                var acnode = GetChildTreeNode(pnode, "AudioRels");
                if (acnode == null) return;
                string changestr = changed ? "*" : "";
                for (int i = 0; i < acnode.Nodes.Count; i++)
                {
                    var anode = acnode.Nodes[i];
                    if (anode.Tag == rel)
                    {
                        string name = rel.Name;
                        if (rel.RpfFileEntry != null)
                        {
                            name = rel.RpfFileEntry.Name;
                        }
                        anode.Text = changestr + name;
                        break;
                    }
                }
            }
        }
        public void SetGrassBatchHasChanged(YmapGrassInstanceBatch batch, bool changed)
        {
            if (ProjectTreeView.Nodes.Count > 0)
            {
                var gbnode = FindGrassTreeNode(batch);
                if (gbnode == null) return;
                string changestr = changed ? "*" : "";
                if (gbnode.Tag == batch)
                {
                    string name = batch.ToString();
                    gbnode.Text = changestr + name;
                }
            }
        }









        private TreeNode GetChildTreeNode(TreeNode node, string name)
        {
            if (node == null) return null;
            var nodes = node.Nodes.Find(name, false);
            if ((nodes == null) || (nodes.Length != 1)) return null;
            return nodes[0];
        }
        public TreeNode FindYmapTreeNode(YmapFile ymap)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var ymapsnode = GetChildTreeNode(projnode, "Ymap");
            if (ymapsnode == null) return null;
            for (int i = 0; i < ymapsnode.Nodes.Count; i++)
            {
                var ymapnode = ymapsnode.Nodes[i];
                if (ymapnode.Tag == ymap) return ymapnode;
            }
            return null;
        }
        public TreeNode FindEntityTreeNode(YmapEntityDef ent)
        {
            if (ent == null) return null;
            TreeNode ymapnode = FindYmapTreeNode(ent.Ymap);
            if (ymapnode == null) return null;
            var entsnode = GetChildTreeNode(ymapnode, "Entities");
            if (entsnode == null) return null;
            for (int i = 0; i < entsnode.Nodes.Count; i++)
            {
                TreeNode entnode = entsnode.Nodes[i];
                if (entnode.Tag == ent) return entnode;
            }
            return null;
        }
        public TreeNode FindCarGenTreeNode(YmapCarGen cargen)
        {
            if (cargen == null) return null;
            TreeNode ymapnode = FindYmapTreeNode(cargen.Ymap);
            if (ymapnode == null) return null;
            var cargensnode = GetChildTreeNode(ymapnode, "CarGens");
            if (cargensnode == null) return null;
            for (int i = 0; i < cargensnode.Nodes.Count; i++)
            {
                TreeNode cargennode = cargensnode.Nodes[i];
                if (cargennode.Tag == cargen) return cargennode;
            }
            return null;
        }
        public TreeNode FindGrassTreeNode(YmapGrassInstanceBatch batch)
        {
            if (batch == null) return null;
            TreeNode ymapnode = FindYmapTreeNode(batch.Ymap);
            if (ymapnode == null) return null;
            var batchnode = GetChildTreeNode(ymapnode, "GrassBatches");
            if (batchnode == null) return null;
            for (int i = 0; i < batchnode.Nodes.Count; i++)
            {
                TreeNode grassnode = batchnode.Nodes[i];
                if (grassnode.Tag == batch) return grassnode;
            }
            return null;
        }
        public TreeNode FindYtypTreeNode(YtypFile ytyp)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var ytypsnode = GetChildTreeNode(projnode, "Ytyp");
            if (ytypsnode == null) return null;
            for (int i = 0; i < ytypsnode.Nodes.Count; i++)
            {
                var ytypnode = ytypsnode.Nodes[i];
                if (ytypnode.Tag == ytyp) return ytypnode;
            }
            return null;
        }
        public TreeNode FindArchetypeTreeNode(Archetype archetype)
        {
            if (archetype == null) return null;
            TreeNode ytypnode = FindYtypTreeNode(archetype.Ytyp);
            if (ytypnode == null) return null;
            var archetypenode = GetChildTreeNode(ytypnode, "Archetypes");
            if (archetypenode == null) return null;
            for (int i = 0; i < archetypenode.Nodes.Count; i++)
            {
                TreeNode archnode = archetypenode.Nodes[i];
                if (archnode.Tag == archetype) return archnode;
            }
            return null;
        }
        public TreeNode FindMloRoomTreeNode(MCMloRoomDef room)
        {
            if (room == null) return null;

            TreeNode ytypnode = FindYtypTreeNode(room.Archetype.Ytyp);
            if (ytypnode == null) return null;

            TreeNode archetypesnode = GetChildTreeNode(ytypnode, "Archetypes");
            if (archetypesnode == null) return null;

            for (int i = 0; i < archetypesnode.Nodes.Count; i++)
            {
                TreeNode mloarchetypenode = archetypesnode.Nodes[i];
                if (mloarchetypenode.Tag == room.Archetype)
                {
                    TreeNode roomsnode = GetChildTreeNode(mloarchetypenode, "Rooms");
                    if (roomsnode == null) return null;

                    for (int j = 0; j < roomsnode.Nodes.Count; j++)
                    {
                        TreeNode roomnode = roomsnode.Nodes[j];
                        if (roomnode.Tag == room) return roomnode;
                    }
                    break;
                }
            }
            return null;
        }
        public TreeNode FindMloEntityTreeNode(MCEntityDef ent)
        {
            MCMloRoomDef entityroom = ent?.Archetype?.GetEntityRoom(ent);
            if (entityroom == null) return null;

            TreeNode ytypnode = FindYtypTreeNode(ent.Archetype.Ytyp);
            if (ytypnode == null) return null;

            var archetypesnode = GetChildTreeNode(ytypnode, "Archetypes");
            if (archetypesnode == null) return null;

            for (int i = 0; i < archetypesnode.Nodes.Count; i++)
            {
                TreeNode mloarchetypenode = archetypesnode.Nodes[i];
                if (mloarchetypenode.Tag == ent.Archetype)
                {
                    TreeNode roomsnode = GetChildTreeNode(mloarchetypenode, "Rooms");
                    if (roomsnode == null) return null;

                    for (int j = 0; j < roomsnode.Nodes.Count; j++)
                    {
                        TreeNode roomnode = roomsnode.Nodes[j];
                        if (roomnode.Tag == entityroom)
                        {
                            TreeNode entitiesnode = GetChildTreeNode(roomnode, "Attached Objects");
                            if (entitiesnode == null) return null;

                            for (var k = 0; k < entitiesnode.Nodes.Count; k++)
                            {
                                TreeNode entitynode = entitiesnode.Nodes[k];
                                if (entitynode.Tag == ent) return entitynode;
                            }
                            break;
                        }
                    }
                    break;
                }
            }
            return null;
        }
        public TreeNode FindYndTreeNode(YndFile ynd)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var yndsnode = GetChildTreeNode(projnode, "Ynd");
            if (yndsnode == null) return null;
            for (int i = 0; i < yndsnode.Nodes.Count; i++)
            {
                var yndnode = yndsnode.Nodes[i];
                if (yndnode.Tag == ynd) return yndnode;
            }
            return null;
        }
        public TreeNode FindPathNodeTreeNode(YndNode n)
        {
            if (n == null) return null;
            TreeNode yndnode = FindYndTreeNode(n.Ynd);
            var nodesnode = GetChildTreeNode(yndnode, "Nodes");
            if (nodesnode == null) return null;
            for (int i = 0; i < nodesnode.Nodes.Count; i++)
            {
                TreeNode nnode = nodesnode.Nodes[i];
                if (nnode.Tag == n) return nnode;
            }
            return null;
        }
        public TreeNode FindYnvTreeNode(YnvFile ynv)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var ynvsnode = GetChildTreeNode(projnode, "Ynv");
            if (ynvsnode == null) return null;
            for (int i = 0; i < ynvsnode.Nodes.Count; i++)
            {
                var yndnode = ynvsnode.Nodes[i];
                if (yndnode.Tag == ynv) return yndnode;
            }
            return null;
        }
        public TreeNode FindNavPolyTreeNode(YnvPoly p)
        {
            if (p == null) return null;
            TreeNode ynvnode = FindYnvTreeNode(p.Ynv);
            var polynode = GetChildTreeNode(ynvnode, "EditPoly");
            if (polynode == null) return null;
            polynode.Tag = p;
            return polynode;
            //for (int i = 0; i < polysnode.Nodes.Count; i++)
            //{
            //    TreeNode pnode = polysnode.Nodes[i];
            //    if (pnode.Tag == p) return pnode;
            //}
            //return null;
        }
        public TreeNode FindNavPointTreeNode(YnvPoint p)
        {
            if (p == null) return null;
            TreeNode ynvnode = FindYnvTreeNode(p.Ynv);
            var pointnode = GetChildTreeNode(ynvnode, "EditPoint");
            if (pointnode == null) return null;
            pointnode.Tag = p;
            return pointnode;
            //for (int i = 0; i < pointsnode.Nodes.Count; i++)
            //{
            //    TreeNode pnode = pointsnode.Nodes[i];
            //    if (pnode.Tag == p) return pnode;
            //}
            //return null;
        }
        public TreeNode FindNavPortalTreeNode(YnvPortal p)
        {
            if (p == null) return null;
            TreeNode ynvnode = FindYnvTreeNode(p.Ynv);
            var portalnode = GetChildTreeNode(ynvnode, "EditPortal");
            if (portalnode == null) return null;
            portalnode.Tag = p;
            return portalnode;
            //for (int i = 0; i < portalsnode.Nodes.Count; i++)
            //{
            //    TreeNode pnode = portalsnode.Nodes[i];
            //    if (pnode.Tag == p) return pnode;
            //}
            //return null;
        }
        public TreeNode FindTrainTrackTreeNode(TrainTrack track)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var trainsnode = GetChildTreeNode(projnode, "Trains");
            if (trainsnode == null) return null;
            for (int i = 0; i < trainsnode.Nodes.Count; i++)
            {
                var trainnode = trainsnode.Nodes[i];
                if (trainnode.Tag == track) return trainnode;
            }
            return null;
        }
        public TreeNode FindTrainNodeTreeNode(TrainTrackNode n)
        {
            if (n == null) return null;
            TreeNode tracknode = FindTrainTrackTreeNode(n.Track);
            var nodesnode = GetChildTreeNode(tracknode, "Nodes");
            if (nodesnode == null) return null;
            for (int i = 0; i < nodesnode.Nodes.Count; i++)
            {
                TreeNode nnode = nodesnode.Nodes[i];
                if (nnode.Tag == n) return nnode;
            }
            return null;
        }
        public TreeNode FindScenarioTreeNode(YmtFile ymt)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var scenariosnode = GetChildTreeNode(projnode, "Scenarios");
            if (scenariosnode == null) return null;
            for (int i = 0; i < scenariosnode.Nodes.Count; i++)
            {
                var ymtnode = scenariosnode.Nodes[i];
                if (ymtnode.Tag == ymt) return ymtnode;
            }
            return null;
        }
        public TreeNode FindScenarioNodeTreeNode(ScenarioNode p)
        {
            if (p == null) return null;
            TreeNode ymtnode = FindScenarioTreeNode(p.Ymt);
            var pointsnode = GetChildTreeNode(ymtnode, "Points");
            if (pointsnode == null) return null;
            for (int i = 0; i < pointsnode.Nodes.Count; i++)
            {
                TreeNode pnode = pointsnode.Nodes[i];
                if (pnode.Tag == p) return pnode;
            }
            return null;
        }
        public TreeNode FindAudioRelTreeNode(RelFile rel)
        {
            if (ProjectTreeView.Nodes.Count <= 0) return null;
            var projnode = ProjectTreeView.Nodes[0];
            var scenariosnode = GetChildTreeNode(projnode, "AudioRels");
            if (scenariosnode == null) return null;
            for (int i = 0; i < scenariosnode.Nodes.Count; i++)
            {
                var ymtnode = scenariosnode.Nodes[i];
                if (ymtnode.Tag == rel) return ymtnode;
            }
            return null;
        }
        public TreeNode FindAudioZoneTreeNode(AudioPlacement zone)
        {
            if (zone == null) return null;
            TreeNode relnode = FindAudioRelTreeNode(zone.RelFile);
            var zonenode = GetChildTreeNode(relnode, "AmbientZones");
            if (zonenode == null) return null;
            //zonenode.Tag = zone;
            for (int i = 0; i < zonenode.Nodes.Count; i++)
            {
                TreeNode znode = zonenode.Nodes[i];
                if (znode.Tag == zone.AudioZone) return znode;
            }
            return zonenode;
        }
        public TreeNode FindAudioEmitterTreeNode(AudioPlacement emitter)
        {
            if (emitter == null) return null;
            TreeNode relnode = FindAudioRelTreeNode(emitter.RelFile);
            var zonenode = GetChildTreeNode(relnode, "AmbientEmitters");
            if (zonenode == null) return null;
            //zonenode.Tag = emitter;
            for (int i = 0; i < zonenode.Nodes.Count; i++)
            {
                TreeNode znode = zonenode.Nodes[i];
                if (znode.Tag == emitter.AudioEmitter) return znode;
            }
            return zonenode;
        }
        public TreeNode FindAudioZoneListTreeNode(Dat151AmbientZoneList list)
        {
            if (list == null) return null;
            TreeNode relnode = FindAudioRelTreeNode(list.Rel);
            var zonelistsnode = GetChildTreeNode(relnode, "ZoneLists");
            if (zonelistsnode == null) return null;
            for (int i = 0; i < zonelistsnode.Nodes.Count; i++)
            {
                TreeNode lnode = zonelistsnode.Nodes[i];
                if (lnode.Tag == list) return lnode;
            }
            return null;
        }
        public TreeNode FindAudioEmitterListTreeNode(Dat151AmbientEmitterList list)
        {
            if (list == null) return null;
            TreeNode relnode = FindAudioRelTreeNode(list.Rel);
            var emitterlistsnode = GetChildTreeNode(relnode, "AmbientEmitterLists");
            if (emitterlistsnode == null) return null;
            for (int i = 0; i < emitterlistsnode.Nodes.Count; i++)
            {
                TreeNode enode = emitterlistsnode.Nodes[i];
                if (enode.Tag == list) return enode;
            }
            return null;
        }





        public void DeselectNode()
        {
            ProjectTreeView.SelectedNode = null;
        }
        public void TrySelectEntityTreeNode(YmapEntityDef ent)
        {
            TreeNode entnode = FindEntityTreeNode(ent);
            if (entnode != null)
            {
                if (ProjectTreeView.SelectedNode == entnode)
                {
                    OnItemSelected?.Invoke(ent);
                }
                else
                {
                    ProjectTreeView.SelectedNode = entnode;
                }
            }
        }
        public void TrySelectCarGenTreeNode(YmapCarGen cargen)
        {
            TreeNode cargennode = FindCarGenTreeNode(cargen);
            if (cargennode != null)
            {
                if (ProjectTreeView.SelectedNode == cargennode)
                {
                    OnItemSelected?.Invoke(cargen);
                }
                else
                {
                    ProjectTreeView.SelectedNode = cargennode;
                }
            }
        }
        public void TrySelectGrassBatchTreeNode(YmapGrassInstanceBatch grassBatch)
        {
            TreeNode grassNode = FindGrassTreeNode(grassBatch);
            if (grassNode != null)
            {
                if (ProjectTreeView.SelectedNode == grassNode)
                {
                    OnItemSelected?.Invoke(grassNode);
                }
                else
                {
                    ProjectTreeView.SelectedNode = grassNode;
                }
            }
        }
        public void TrySelectMloEntityTreeNode(MCEntityDef ent)
        {
            TreeNode entnode = FindMloEntityTreeNode(ent);
            if (entnode != null)
            {
                if (ProjectTreeView.SelectedNode == entnode)
                {
                    OnItemSelected?.Invoke(ent);
                }
                else
                {
                    ProjectTreeView.SelectedNode = entnode;
                }
            }
        }
        public void TrySelectMloRoomTreeNode(MCMloRoomDef room)
        {
            TreeNode roomnode = FindMloRoomTreeNode(room);
            if (roomnode != null)
            {
                if (ProjectTreeView.SelectedNode == roomnode)
                {
                    OnItemSelected?.Invoke(room);
                }
                else
                {
                    ProjectTreeView.SelectedNode = roomnode;
                }
            }
        }
        public void TrySelectArchetypeTreeNode(Archetype archetype)
        {
            TreeNode archetypenode = FindArchetypeTreeNode(archetype);
            if (archetypenode != null)
            {
                if (ProjectTreeView.SelectedNode == archetypenode)
                {
                    OnItemSelected?.Invoke(archetype);
                }
                else
                {
                    ProjectTreeView.SelectedNode = archetypenode;
                }
            }
        }
        public void TrySelectPathNodeTreeNode(YndNode node)
        {
            TreeNode tnode = FindPathNodeTreeNode(node);
            if (tnode == null)
            {
                tnode = FindYndTreeNode(node?.Ynd);
            }
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(node);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectNavPolyTreeNode(YnvPoly poly)
        {
            TreeNode tnode = FindNavPolyTreeNode(poly);
            if (tnode == null)
            {
                tnode = FindYnvTreeNode(poly?.Ynv);
            }
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(poly);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectNavPointTreeNode(YnvPoint point)
        {
            TreeNode tnode = FindNavPointTreeNode(point);
            if (tnode == null)
            {
                tnode = FindYnvTreeNode(point?.Ynv);
            }
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(point);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectNavPortalTreeNode(YnvPortal portal)
        {
            TreeNode tnode = FindNavPortalTreeNode(portal);
            if (tnode == null)
            {
                tnode = FindYnvTreeNode(portal?.Ynv);
            }
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(portal);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectTrainNodeTreeNode(TrainTrackNode node)
        {
            TreeNode tnode = FindTrainNodeTreeNode(node);
            if (tnode == null)
            {
                tnode = FindTrainTrackTreeNode(node?.Track);
            }
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(node);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectScenarioTreeNode(YmtFile scenario)
        {
            TreeNode tnode = FindScenarioTreeNode(scenario);
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(scenario);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectScenarioNodeTreeNode(ScenarioNode node)
        {
            TreeNode tnode = FindScenarioNodeTreeNode(node);
            if (tnode == null)
            {
                tnode = FindScenarioTreeNode(node?.Ymt);
            }
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(node);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectAudioRelTreeNode(RelFile rel)
        {
            TreeNode tnode = FindAudioRelTreeNode(rel);
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(rel);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectAudioZoneTreeNode(AudioPlacement zone)
        {
            TreeNode tnode = FindAudioZoneTreeNode(zone);
            if (tnode == null)
            {
                tnode = FindAudioRelTreeNode(zone?.RelFile);
            }
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(zone);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectAudioEmitterTreeNode(AudioPlacement emitter)
        {
            TreeNode tnode = FindAudioEmitterTreeNode(emitter);
            if (tnode == null)
            {
                tnode = FindAudioRelTreeNode(emitter?.RelFile);
            }
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(emitter);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectAudioZoneListTreeNode(Dat151AmbientZoneList list)
        {
            TreeNode tnode = FindAudioZoneListTreeNode(list);
            if (tnode == null)
            {
                tnode = FindAudioRelTreeNode(list?.Rel);
            }
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(list);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }
        public void TrySelectAudioEmitterListTreeNode(Dat151AmbientEmitterList list)
        {
            TreeNode tnode = FindAudioEmitterListTreeNode(list);
            if (tnode == null)
            {
                tnode = FindAudioRelTreeNode(list?.Rel);
            }
            if (tnode != null)
            {
                if (ProjectTreeView.SelectedNode == tnode)
                {
                    OnItemSelected?.Invoke(list);
                }
                else
                {
                    ProjectTreeView.SelectedNode = tnode;
                }
            }
        }



        public void UpdateArchetypeTreeNode(Archetype archetype)
        {
            var tn = FindArchetypeTreeNode(archetype);
            if (tn != null)
            {
                tn.Text = archetype._BaseArchetypeDef.ToString();
            }
        }
        public void UpdateCarGenTreeNode(YmapCarGen cargen)
        {
            var tn = FindCarGenTreeNode(cargen);
            if (tn != null)
            {
                tn.Text = cargen.ToString();
            }
        }
        public void UpdatePathNodeTreeNode(YndNode node)
        {
            var tn = FindPathNodeTreeNode(node);
            if (tn != null)
            {
                tn.Text = node._RawData.ToString();
            }
        }
        public void UpdateNavPolyTreeNode(YnvPoly poly)
        {
            var tn = FindNavPolyTreeNode(poly);
            if (tn != null)
            {
            }
        }
        public void UpdateTrainNodeTreeNode(TrainTrackNode node)
        {
            var tn = FindTrainNodeTreeNode(node);
            if (tn != null)
            {
                tn.Text = node.ToString();
            }
        }
        public void UpdateScenarioNodeTreeNode(ScenarioNode node)
        {
            var tn = FindScenarioNodeTreeNode(node);
            if (tn != null)
            {
                tn.Text = node.MedTypeName + ": " + node.StringText;
            }
        }
        public void UpdateAudioZoneTreeNode(AudioPlacement zone)
        {
            var tn = FindAudioZoneTreeNode(zone);
            if (tn != null)
            {
            }
        }
        public void UpdateAudioEmitterTreeNode(AudioPlacement emitter)
        {
            var tn = FindAudioEmitterTreeNode(emitter);
            if (tn != null)
            {
            }
        }
        public void UpdateAudioZoneListTreeNode(Dat151AmbientZoneList list)
        {
            var tn = FindAudioZoneListTreeNode(list);
            if (tn != null)
            {
                tn.Text = list.NameHash.ToString();
            }
        }
        public void UpdateAudioEmitterListTreeNode(Dat151AmbientEmitterList list)
        {
            var tn = FindAudioEmitterListTreeNode(list);
            if (tn != null)
            {
                tn.Text = list.NameHash.ToString();
            }
        }



        public void RemoveEntityTreeNode(YmapEntityDef ent)
        {
            var tn = FindEntityTreeNode(ent);
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Entities (" + ent.Ymap.AllEntities.Length.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }

        public void RemoveCarGenTreeNode(YmapCarGen cargen)
        {
            var tn = FindCarGenTreeNode(cargen);
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Car Generators (" + cargen.Ymap.CarGenerators.Length.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }

        public void RemoveGrassBatchTreeNode(YmapGrassInstanceBatch batch)
        {
            var tn = FindGrassTreeNode(batch);
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Grass Batches (" + batch.Ymap.GrassInstanceBatches.Length.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }
        public void RemoveArchetypeTreeNode(Archetype archetype)
        {
            var tn = FindArchetypeTreeNode(archetype);
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Archetypes (" + archetype.Ytyp.AllArchetypes.Length.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }
        public void RemoveMloEntityTreeNode(MCEntityDef ent)
        {
            var tn = FindMloEntityTreeNode(ent);
            if ((tn != null) && (tn.Parent != null))
            {
                var tnp = tn.Parent.Parent;
                MCMloRoomDef room = null;
                if (tnp != null) room = tnp.Tag as MCMloRoomDef;

                tn.Parent.Text = "Attached Objects (" + (room?.AttachedObjects.Length - 1 ?? 0) + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }
        public void RemovePathNodeTreeNode(YndNode node)
        {
            var tn = FindPathNodeTreeNode(node);
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Nodes (" + node.Ynd.Nodes.Length.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }
        public void RemoveTrainNodeTreeNode(TrainTrackNode node)
        {
            var tn = FindTrainNodeTreeNode(node);
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Nodes (" + node.Track.Nodes.Count.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }
        public void RemoveScenarioNodeTreeNode(ScenarioNode node)
        {
            var tn = FindScenarioNodeTreeNode(node);
            if ((tn != null) && (tn.Parent != null))
            {
                tn.Parent.Text = "Points (" + (node.Ymt?.ScenarioRegion?.Nodes?.Count ?? 0).ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }
        public void RemoveAudioZoneTreeNode(AudioPlacement zone)
        {
            var tn = FindAudioZoneTreeNode(zone);
            if ((tn != null) && (tn.Parent != null))
            {
                var zones = new List<Dat151AmbientZone>();
                foreach (var reldata in zone.RelFile.RelDatas)
                {
                    if (reldata is Dat151AmbientZone)
                    {
                        zones.Add(reldata as Dat151AmbientZone);
                    }
                }

                tn.Parent.Text = "Ambient Zones (" + zones.Count.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }
        public void RemoveAudioEmitterTreeNode(AudioPlacement emitter)
        {
            var tn = FindAudioEmitterTreeNode(emitter);
            if ((tn != null) && (tn.Parent != null))
            {
                var emitters = new List<Dat151AmbientEmitter>();
                foreach (var reldata in emitter.RelFile.RelDatas)
                {
                    if (reldata is Dat151AmbientEmitter)
                    {
                        emitters.Add(reldata as Dat151AmbientEmitter);
                    }
                }

                tn.Parent.Text = "Ambient Emitters (" + emitters.Count.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }
        public void RemoveAudioZoneListTreeNode(Dat151AmbientZoneList list)
        {
            var tn = FindAudioZoneListTreeNode(list);
            if ((tn != null) && (tn.Parent != null))
            {
                var zonelists = new List<Dat151AmbientZoneList>();
                foreach (var reldata in list.Rel.RelDatas)
                {
                    if (reldata is Dat151AmbientZoneList)
                    {
                        zonelists.Add(reldata as Dat151AmbientZoneList);
                    }
                }

                tn.Parent.Text = "Ambient Zone Lists (" + zonelists.Count.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }
        public void RemoveAudioEmitterListTreeNode(Dat151AmbientEmitterList list)
        {
            var tn = FindAudioEmitterListTreeNode(list);
            if ((tn != null) && (tn.Parent != null))
            {
                var emitterlists = new List<Dat151AmbientEmitterList>();
                foreach (var reldata in list.Rel.RelDatas)
                {
                    if (reldata is Dat151AmbientEmitterList)
                    {
                        emitterlists.Add(reldata as Dat151AmbientEmitterList);
                    }
                }

                tn.Parent.Text = "Ambient Emitter Lists (" + emitterlists.Count.ToString() + ")";
                tn.Parent.Nodes.Remove(tn);
            }
        }





        public event ProjectExplorerItemSelectHandler OnItemSelected;
        public event ProjectExplorerItemActivateHandler OnItemActivated;



        private void ProjectTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OnItemSelected?.Invoke(ProjectTreeView.SelectedNode?.Tag);
        }
        private void ProjectTreeView_DoubleClick(object sender, EventArgs e)
        {
            if (ProjectTreeView.SelectedNode != null)
            {
                OnItemActivated?.Invoke(ProjectTreeView.SelectedNode.Tag);
            }
        }

        private void ProjectTreeView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            //if (e.Node.Tag != CurrentProjectFile) return; //disabling doubleclick expand/collapse only for project node
            if (inDoubleClick == true && e.Action == TreeViewAction.Collapse) e.Cancel = true;
        }
        private void ProjectTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            //if (e.Node.Tag != CurrentProjectFile) return; //disabling doubleclick expand/collapse only for project node
            if (inDoubleClick == true && e.Action == TreeViewAction.Expand) e.Cancel = true;
        }
        private void ProjectTreeView_MouseDown(object sender, MouseEventArgs e)
        {
            inDoubleClick = (e.Clicks > 1); //disabling doubleclick expand/collapse
        }
    }
    public delegate void ProjectExplorerItemSelectHandler(object item);
    public delegate void ProjectExplorerItemActivateHandler(object item);
}
