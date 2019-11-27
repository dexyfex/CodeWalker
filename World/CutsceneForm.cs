using CodeWalker.GameFiles;
using CodeWalker.Rendering;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.World
{
    public partial class CutsceneForm : Form
    {
        private WorldForm WorldForm;
        private GameFileCache GameFileCache;

        private Cutscene Cutscene = null;

        private bool AnimateCamera = true;
        private bool Playing = false;


        class CutsceneDropdownItem
        {
            public RpfEntry RpfEntry { get; set; }

            public override string ToString()
            {
                return RpfEntry?.Path ?? "";
            }
        }

        public CutsceneForm(WorldForm worldForm)
        {
            WorldForm = worldForm;
            GameFileCache = WorldForm.GameFileCache;
            InitializeComponent();
        }



        public void UpdateAnimation(float elapsed)
        {
            if (Cutscene != null)
            {
                if (Playing)
                {
                    var newt = Cutscene.PlaybackTime + elapsed;
                    Cutscene.Update(newt);
                }

                if (AnimateCamera && (Cutscene.CameraObject != null))
                {
                    var cori = Quaternion.RotationAxis(Vector3.UnitX, -1.57079632679f) * Quaternion.RotationAxis(Vector3.UnitZ, 3.141592653f);

                    var pos = Cutscene.Position;
                    var rot = Cutscene.Rotation;
                    pos = pos + rot.Multiply(Cutscene.CameraObject.Position);
                    rot = rot * Cutscene.CameraObject.Rotation * cori;

                    WorldForm.SetCameraTransform(pos, rot);

                }

            }
        }

        public void GetVisibleYmaps(Camera camera, Dictionary<MetaHash, YmapFile> ymaps)
        {
            //use a temporary ymap for entities?

            var renderer = WorldForm?.Renderer;
            if (renderer == null) return;

            if (Cutscene == null) return;
            Cutscene.Render(renderer);

        }





        private void SelectCutscene(CutsceneDropdownItem dditem)
        {
            Cursor = Cursors.WaitCursor;
            Task.Run(() =>
            {
                CutFile cutFile = null;
                Cutscene cutscene = null;

                if (GameFileCache.IsInited)
                {
                    var entry = dditem?.RpfEntry as RpfFileEntry;
                    if (entry != null)
                    {

                        cutFile = new CutFile(entry);
                        GameFileCache.RpfMan.LoadFile(cutFile, entry);

                        cutscene = new Cutscene();
                        cutscene.Init(cutFile, GameFileCache, WorldForm);

                    }
                }

                CutsceneLoaded(cutscene);

            });
        }
        private void CutsceneLoaded(Cutscene cs)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action(() => { CutsceneLoaded(cs); }));
                }
                catch
                { }
                return;
            }

            Cutscene = cs;

            LoadTreeView(cs);

            TimeTrackBar.Maximum = (int)(cs.Duration * 10.0f);
            TimeTrackBar.Value = 0;
            UpdateTimeLabel();

            Cursor = Cursors.Default;
        }


        private void LoadTreeView(Cutscene cs)
        {
            CutsceneTreeView.Nodes.Clear();

            var cutFile = cs?.CutFile;
            var cf = cutFile?.CutsceneFile2;
            if (cf != null)
            {
                var csnode = CutsceneTreeView.Nodes.Add(cutFile.FileEntry?.Name);
                csnode.Tag = cs;

                if (cs.SceneObjects != null)
                {
                    var objsnode = csnode.Nodes.Add("Objects");
                    objsnode.Name = "Objects";

                    foreach (var obj in cs.SceneObjects.Values)
                    {
                        var objnode = objsnode.Nodes.Add(obj.ToString());
                        objnode.Tag = obj;
                    }
                }
                if (cf.pCutsceneEventList != null)
                {
                    var evtsnode = csnode.Nodes.Add("Events");
                    evtsnode.Name = "Events";

                    foreach (var evt in cf.pCutsceneEventList)
                    {
                        var evtnode = evtsnode.Nodes.Add(evt.ToString());
                        evtnode.Tag = evt;
                    }
                }
                if (cf.pCutsceneLoadEventList != null)
                {
                    var ldesnode = csnode.Nodes.Add("Load Events");
                    ldesnode.Name = "Load Events";

                    foreach (var lev in cf.pCutsceneLoadEventList)
                    {
                        var ldenode = ldesnode.Nodes.Add(lev.ToString());
                        ldenode.Tag = lev;
                    }
                }



                csnode.Expand();
                CutsceneTreeView.SelectedNode = csnode;
            }

        }



        private void UpdateTimeTrackBar()
        {
            var tim = Cutscene?.PlaybackTime ?? 0.0f;
            var itim = (int)(tim * 10.0f);
            TimeTrackBar.Value = itim;
        }
        private void UpdateTimeLabel()
        {
            var tim = Cutscene?.PlaybackTime ?? 0.0f;
            var dur = Cutscene?.Duration ?? 0.0f;
            TimeLabel.Text = tim.ToString("0.00") + " / " + dur.ToString("0.00");
        }







        private void CutsceneForm_Load(object sender, EventArgs e)
        {
            if (!GameFileCache.IsInited) return;//what to do here?

            var rpfman = GameFileCache.RpfMan;
            var rpflist = rpfman.AllRpfs; //loadedOnly ? gfc.ActiveMapRpfFiles.Values.ToList() :

            var dditems = new List<CutsceneDropdownItem>();
            foreach (var rpf in rpflist)
            {
                foreach (var entry in rpf.AllEntries)
                {
                    if (entry.NameLower.EndsWith(".cut"))
                    {
                        var dditem = new CutsceneDropdownItem();
                        dditem.RpfEntry = entry;
                        dditems.Add(dditem);
                    }
                }
            }

            CutsceneComboBox.Items.Clear();
            CutsceneComboBox.Items.AddRange(dditems.ToArray());


        }

        private void CutsceneForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            WorldForm?.OnCutsceneFormClosed();
        }

        private void CutsceneComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = CutsceneComboBox.SelectedItem as CutsceneDropdownItem;
            SelectCutscene(item);
        }

        private void CutsceneTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            InfoPropertyGrid.SelectedObject = CutsceneTreeView.SelectedNode?.Tag;
        }

        private void AnimateCameraCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            AnimateCamera = AnimateCameraCheckBox.Checked;
        }

        private void PlayStopButton_Click(object sender, EventArgs e)
        {
            if (Playing)
            {
                Playing = false;
                PlayStopButton.Text = "Play";
                PlaybackTimer.Enabled = false;
            }
            else
            {
                Playing = true;
                PlayStopButton.Text = "Stop";
                PlaybackTimer.Enabled = true;
            }
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (Playing)
            {
                UpdateTimeTrackBar();
                UpdateTimeLabel();
            }
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class Cutscene
    {
        public CutFile CutFile { get; set; } = null;
        private GameFileCache GameFileCache = null;
        private WorldForm WorldForm = null;

        public float[] CameraCutList { get; set; } = null;
        public YcdFile[] Ycds { get; set; } = null;


        public float Duration { get; set; } = 0.0f;
        public float PlaybackTime { get; set; } = 0.0f;


        public Dictionary<int, CutObject> Objects { get; set; } = null;
        public Dictionary<int, CutsceneObject> SceneObjects { get; set; } = null;
        public CutEvent[] LoadEvents { get; set; } = null;
        public CutEvent[] PlayEvents { get; set; } = null;
        public CutConcatData[] ConcatDatas { get; set; } = null;

        public int NextLoadEvent { get; set; } = 0;
        public int NextPlayEvent { get; set; } = 0;
        public int NextCameraCut { get; set; } = 0;
        public int NextConcatData { get; set; } = 0;

        public Gxt2File Gxt2File { get; set; } = null;

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }


        public CutsceneObject CameraObject = null;




        public void Init(CutFile cutFile, GameFileCache gfc, WorldForm wf)
        {
            CutFile = cutFile;
            GameFileCache = gfc;
            WorldForm = wf;

            var csf = cutFile?.CutsceneFile2;
            if (csf == null) return;
            if (gfc == null) return;


            Duration = csf.fTotalDuration;
            CameraCutList = csf.cameraCutList;
            Position = csf.vOffset;
            Rotation = Quaternion.RotationAxis(Vector3.UnitZ, csf.fRotation);
            Objects = csf.ObjectsDict;
            LoadEvents = RecastArray<CutEvent>(csf.pCutsceneLoadEventList);
            PlayEvents = RecastArray<CutEvent>(csf.pCutsceneEventList);
            ConcatDatas = csf.concatDataList;


            LoadYcds();
            CreateSceneObjects();
            RaiseEvents(0.0f);
        }

        private void LoadYcds()
        {
            int cutListCount = (CameraCutList?.Length ?? 0) + 1;
            var shortName = CutFile.FileEntry?.GetShortNameLower() ?? "";
            Ycds = new YcdFile[cutListCount];
            if (!string.IsNullOrEmpty(shortName))
            {
                for (int i = 0; i < cutListCount; i++)
                {
                    var ycdname = shortName + "-" + i.ToString();
                    var ycdhash = JenkHash.GenHash(ycdname);
                    var ycd = GameFileCache.GetYcd(ycdhash);
                    while ((ycd != null) && (!ycd.Loaded))
                    {
                        Thread.Sleep(20);//bite me
                        ycd = GameFileCache.GetYcd(ycdhash);
                    }
                    if (ycd != null)
                    {
                        ycd.BuildCutsceneMap(i);
                    }
                    Ycds[i] = ycd;
                }
            }
        }


        public void Update(float newTime)
        {
            if (newTime > Duration)
            {
                newTime = 0.0f; //stop or loop?
            }

            if (newTime >= PlaybackTime)
            {
                RaiseEvents(newTime);
            }
            else
            {
                //reset playback to beginning, and seek to newTime
                RaiseEvents(Duration);//raise all events up to the end first
                PlaybackTime = 0.0f;
                NextLoadEvent = 0;
                NextPlayEvent = 0;
                NextCameraCut = 0;
                NextConcatData = 0;
                RaiseEvents(newTime);
            }

            PlaybackTime = newTime;

            int cutIndex = 0;
            float cutStart = 0.0f;
            for (cutIndex = 0; cutIndex < CameraCutList?.Length; cutIndex++)
            {
                var cutTime = CameraCutList[cutIndex];
                if (cutTime > newTime) break;
                cutStart = cutTime;
            }

            float cutOffset = newTime - cutStart;//offset into the current cut


            void updateObjectTransform(CutsceneObject obj, ClipMapEntry cme, ushort boneTag, byte posTrack, byte rotTrack)
            {
                if (cme != null)
                {
                    if (cme.Clip is ClipAnimation canim)
                    {
                        if (canim.Animation != null)
                        {
                            var t = canim.GetPlaybackTime(cutOffset);
                            var f = canim.Animation.GetFramePosition(t);
                            var p = canim.Animation.FindBoneIndex(boneTag, posTrack);
                            var r = canim.Animation.FindBoneIndex(boneTag, rotTrack);
                            if (p >= 0) obj.Position = canim.Animation.EvaluateVector4(f, p, true).XYZ();
                            if (r >= 0) obj.Rotation = canim.Animation.EvaluateQuaternion(f, r, true);
                        }
                    }
                    else if (cme.Clip is ClipAnimationList alist)
                    {
                        if (alist.Animations?.Data != null)
                        {
                            foreach (var anim in alist.Animations.Data)
                            {
                                var t = anim.GetPlaybackTime(cutOffset);
                                var f = anim.Animation.GetFramePosition(t);
                                var p = anim.Animation.FindBoneIndex(boneTag, posTrack);
                                var r = anim.Animation.FindBoneIndex(boneTag, rotTrack);
                                if (p >= 0) obj.Position = anim.Animation.EvaluateVector4(f, p, true).XYZ();
                                if (r >= 0) obj.Rotation = anim.Animation.EvaluateQuaternion(f, r, true);
                            }
                        }
                    }
                }
            }




            var ycd = (cutIndex < (Ycds?.Length ?? 0)) ? Ycds[cutIndex] : null;
            if (ycd?.CutsceneMap != null)
            {
                ClipMapEntry cme = null;

                if (CameraObject != null)
                {
                    ycd.CutsceneMap.TryGetValue(CameraObject.Name, out cme);

                    updateObjectTransform(CameraObject, cme, 0, 7, 8);
                }

                if (SceneObjects != null)
                {
                    foreach (var obj in SceneObjects.Values)
                    {
                        if (obj.Enabled == false) continue;

                        var pos = Position;
                        var rot = Rotation;
                        var animate = (obj.Ped != null) || (obj.Prop != null) || (obj.Vehicle != null) || (obj.Weapon != null);
                        if (animate)
                        {
                            ycd.CutsceneMap.TryGetValue(obj.AnimHash, out cme);
                            if (cme != null)
                            {
                                cme.OverridePlayTime = true;
                                cme.PlayTime = cutOffset;
                                updateObjectTransform(obj, cme, 0, 5, 6); //using root animation bone ids
                                pos = pos + rot.Multiply(obj.Position);
                                rot = rot * obj.Rotation;
                            }
                            obj.AnimClip = cme;
                        }
                        if (obj.Ped != null)
                        {
                            obj.Ped.Position = pos;
                            obj.Ped.Rotation = rot;
                            obj.Ped.UpdateEntity();
                            obj.Ped.AnimClip = cme;
                        }
                        if (obj.Prop != null)
                        {
                            obj.Prop.Position = pos;
                            obj.Prop.Orientation = rot;
                        }
                        if (obj.Vehicle != null)
                        {
                            obj.Vehicle.Position = pos;
                            obj.Vehicle.Rotation = rot;
                            obj.Vehicle.UpdateEntity();
                        }
                        if (obj.Weapon != null)
                        {
                            obj.Weapon.Position = pos;
                            obj.Weapon.Rotation = rot;
                            obj.Weapon.UpdateEntity();
                        }
                    }
                }




            }


        }


        public void Render(Renderer renderer)
        {

            if (SceneObjects != null)
            {
                foreach (var obj in SceneObjects.Values)
                {
                    if (obj.Enabled == false) continue;

                    if (obj.Ped != null)
                    {
                        renderer.RenderPed(obj.Ped);
                    }
                    if (obj.Prop != null)
                    {
                        renderer.RenderArchetype(obj.Prop.Archetype, obj.Prop, null, true, obj.AnimClip);
                    }
                    if (obj.Vehicle != null)
                    {
                        renderer.RenderVehicle(obj.Vehicle, obj.AnimClip);
                    }
                    if (obj.Weapon != null)
                    {
                        renderer.RenderWeapon(obj.Weapon, obj.AnimClip);
                    }
                }
            }

        }





        private void RaiseEvents(float upToTime)
        {

            int i;
            for (i = NextLoadEvent; i < LoadEvents?.Length; i++)
            {
                var e = LoadEvents[i];
                if (e != null)
                {
                    if (e.fTime > upToTime) break;
                    RaiseEvent(e);
                }
            }
            NextLoadEvent = i;

            for (i = NextPlayEvent; i < PlayEvents?.Length; i++)
            {
                var e = PlayEvents[i];
                if (e != null)
                {
                    if (e.fTime > upToTime) break;
                    RaiseEvent(e);
                }
            }
            NextPlayEvent = i;

            for (i = NextCameraCut; i < CameraCutList?.Length; i++)
            {
                var c = CameraCutList[i];
                if (c > upToTime) break;
            }
            NextCameraCut = i;

            for (i = NextConcatData; i < ConcatDatas?.Length; i++)
            {
                var c = ConcatDatas[i];
                if (c.fStartTime > upToTime) break;
                if (c.cSceneName == 0) break;

                Position = c.vOffset;
                Rotation = Quaternion.RotationAxis(Vector3.UnitZ, c.fRotation * 0.0174532925f); //is this right?
            }
            NextConcatData = i;


        }
        private void RaiseEvent(CutEvent e)
        {

            switch (e.iEventId)
            {
                case CutEventType.LoadScene: LoadScene(e); break;
                case CutEventType.LoadAnimation: LoadAnimation(e); break;
                case CutEventType.LoadAudio: LoadAudio(e); break;
                case CutEventType.LoadModels: LoadModels(e); break;
                case CutEventType.LoadRayfireDes: LoadRayfireDes(e); break;
                case CutEventType.LoadParticles: LoadParticles(e); break;
                case CutEventType.LoadOverlays: LoadOverlays(e); break;
                case CutEventType.LoadGxt2: LoadGxt2(e); break;
                case CutEventType.UnloadModels: UnloadModels(e); break;
                case CutEventType.UnloadRayfireDes: UnloadRayfireDes(e); break;
                case CutEventType.EnableScreenFade: EnableScreenFade(e); break;
                case CutEventType.EnableHideObject: EnableHideObject(e); break;
                case CutEventType.EnableFixupModel: EnableFixupModel(e); break;
                case CutEventType.EnableBlockBounds: EnableBlockBounds(e); break;
                case CutEventType.EnableAnimation: EnableAnimation(e); break;
                case CutEventType.EnableParticleEffect: EnableParticleEffect(e); break;
                case CutEventType.EnableOverlay: EnableOverlay(e); break;
                case CutEventType.EnableAudio: EnableAudio(e); break;
                case CutEventType.EnableCamera: EnableCamera(e); break;
                case CutEventType.EnableLight: EnableLight(e); break;
                case CutEventType.DisableScreenFade: DisableScreenFade(e); break;
                case CutEventType.DisableHideObject: DisableHideObject(e); break;
                case CutEventType.DisableBlockBounds: DisableBlockBounds(e); break;
                case CutEventType.DisableAnimation: DisableAnimation(e); break;
                case CutEventType.DisableParticleEffect: DisableParticleEffect(e); break;
                case CutEventType.DisableOverlay: DisableOverlay(e); break;
                case CutEventType.DisableAudio: DisableAudio(e); break;
                case CutEventType.DisableCamera: DisableCamera(e); break;
                case CutEventType.DisableLight: DisableLight(e); break;
                case CutEventType.Subtitle: Subtitle(e); break;
                case CutEventType.PedVariation: PedVariation(e); break;
                case CutEventType.CameraCut: CameraCut(e); break;
                case CutEventType.CameraShadowCascade: CameraShadowCascade(e); break;
                case CutEventType.CameraUnk1: CameraUnk1(e); break;
                case CutEventType.CameraUnk2: CameraUnk2(e); break;
                case CutEventType.CameraUnk3: CameraUnk3(e); break;
                case CutEventType.CameraUnk4: CameraUnk4(e); break;
                case CutEventType.CameraUnk5: CameraUnk5(e); break;
                case CutEventType.CameraUnk6: CameraUnk6(e); break;
                case CutEventType.CameraUnk7: CameraUnk7(e); break;
                case CutEventType.CameraUnk8: CameraUnk8(e); break;
                case CutEventType.DecalUnk1: DecalUnk1(e); break;
                case CutEventType.DecalUnk2: DecalUnk2(e); break;
                case CutEventType.PropUnk1: PropUnk1(e); break;
                case CutEventType.Unk1: Unk1(e); break;
                case CutEventType.Unk2: Unk2(e); break;
                case CutEventType.VehicleUnk1: VehicleUnk1(e); break;
                case CutEventType.PedUnk1: PedUnk1(e); break;
                default: break;
            }
            
        }

        private void LoadScene(CutEvent e)
        {
            var args = e.EventArgs as CutLoadSceneEventArgs;
            if (args == null)
            { return; }


            Position = args.vOffset;
            Rotation = Quaternion.RotationAxis(Vector3.UnitZ, args.fRotation * 0.0174532925f);//is this right?

        }
        private void LoadAnimation(CutEvent e)
        {
            var args = e.EventArgs as CutNameEventArgs;
            if (args == null)
            { return; }

        }
        private void LoadAudio(CutEvent e)
        {
            var args = e.EventArgs as CutNameEventArgs;
            if (args == null)
            { return; }

        }
        private void LoadModels(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdListEventArgs;
            if (args == null)
            { return; }

            if (args.iObjectIdList == null) return;

            foreach (var objid in args.iObjectIdList)
            {
                CutsceneObject obj = null;
                SceneObjects.TryGetValue(objid, out obj);
                if (obj != null)
                {
                    obj.Enabled = true;
                }
            }
        }
        private void LoadRayfireDes(CutEvent e)
        {
        }
        private void LoadParticles(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdListEventArgs;
            if (args == null)
            { return; }

        }
        private void LoadOverlays(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdListEventArgs;
            if (args == null)
            { return; }

        }
        private void LoadGxt2(CutEvent e)
        {
            if (GameFileCache == null)
            { return; }
            if (Gxt2File != null)
            { }

            var args = e.EventArgs as CutFinalNameEventArgs;
            if (args == null)
            { return; }

            var namel = args.cName?.ToLowerInvariant();
            var namehash = JenkHash.GenHash(namel);

            RpfFileEntry gxt2entry = null;
            GameFileCache.Gxt2Dict.TryGetValue(namehash, out gxt2entry);

            if (gxt2entry != null) //probably should do this load async
            {
                Gxt2File = GameFileCache.RpfMan.GetFile<Gxt2File>(gxt2entry);

                if (Gxt2File != null)
                {
                    for (int i = 0; i < Gxt2File.TextEntries.Length; i++)
                    {
                        var te = Gxt2File.TextEntries[i];
                        GlobalText.Ensure(te.Text, te.Hash);
                    }
                }
            }

        }
        private void UnloadModels(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdListEventArgs;
            if (args == null)
            { return; }

            if (args.iObjectIdList == null) return;

            foreach (var objid in args.iObjectIdList)
            {
                CutsceneObject obj = null;
                SceneObjects.TryGetValue(objid, out obj);
                if (obj != null)
                {
                    obj.Enabled = false;
                }
            }
        }
        private void UnloadRayfireDes(CutEvent e)
        {
        }
        private void EnableHideObject(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdEventArgs;
            if (args == null)
            { return; }

            var obj = args.Object;

        }
        private void EnableFixupModel(CutEvent e)
        {
        }
        private void EnableBlockBounds(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdEventArgs;
            if (args == null)
            { return; }

            var obj = args.Object;

        }
        private void EnableScreenFade(CutEvent e)
        {
        }
        private void EnableAnimation(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdEventArgs;
            if (args == null)
            { return; }

            var obj = args.Object;

        }
        private void EnableParticleEffect(CutEvent e)
        {
        }
        private void EnableOverlay(CutEvent e)
        {
        }
        private void EnableAudio(CutEvent e)
        {
            var args = e.EventArgs as CutNameEventArgs;
            if (args == null)
            { return; }

        }
        private void EnableCamera(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdEventArgs;
            if (args == null)
            { return; }

            var obj = args.Object;

        }
        private void EnableLight(CutEvent e)
        {
        }
        private void DisableHideObject(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdEventArgs;
            if (args == null)
            { return; }

            var obj = args.Object;

        }
        private void DisableBlockBounds(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdEventArgs;
            if (args == null)
            { return; }

            var obj = args.Object;

        }
        private void DisableScreenFade(CutEvent e)
        {
        }
        private void DisableAnimation(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdEventArgs;
            if (args == null)
            { return; }

            var obj = args.Object;

        }
        private void DisableParticleEffect(CutEvent e)
        {
        }
        private void DisableOverlay(CutEvent e)
        {
        }
        private void DisableAudio(CutEvent e)
        {
            var args = e.EventArgs as CutNameEventArgs;
            if (args == null)
            { return; }

        }
        private void DisableCamera(CutEvent e)
        {
            var args = e.EventArgs as CutObjectIdEventArgs;
            if (args == null)
            { return; }

            var obj = args.Object;

        }
        private void DisableLight(CutEvent e)
        {
        }
        private void Subtitle(CutEvent e)
        {
            var args = e.EventArgs as CutSubtitleEventArgs;
            if (args == null)
            { return; }

            if (WorldForm != null)
            {
                var txt = args.cName.ToString();
                var dur = args.fSubtitleDuration;

                txt = txt.Replace("~z~", "");
                txt = txt.Replace("~c~~n~", "\n - ");
                txt = txt.Replace("~n~", "\n");
                txt = txt.Replace("~c~", " - ");
                txt = txt.Replace("~t~", " - ");

                WorldForm.ShowSubtitle(txt, dur);
            }
        }
        private void PedVariation(CutEvent e)
        {
            var args = e.EventArgs as CutObjectVariationEventArgs;
            if (args == null)
            { return; }

            var oe = e as CutObjectIdEvent;
            if (oe == null)
            { return; }


            CutsceneObject cso = null;
            SceneObjects.TryGetValue(oe.iObjectId, out cso);

            if (cso?.Ped != null)
            {
                int comp = args.iComponent;
                int drbl = args.iDrawable;
                int texx = args.iTexture;

                Task.Run(() =>
                {
                    cso.Ped.SetComponentDrawable(comp, drbl, 0, texx, GameFileCache);
                });
            }


        }
        private void CameraCut(CutEvent e)
        {
            var args = e.EventArgs as CutCameraCutEventArgs;
            if (args == null)
            { return; }

            var oe = e as CutObjectIdEvent;
            if (oe == null)
            { return; }


            CutsceneObject obj = null;
            SceneObjects.TryGetValue(oe.iObjectId, out obj);
            if (obj == null)
            { return; }


            obj.Position = args.vPosition;
            obj.Rotation = args.vRotationQuaternion;

            CameraObject = obj;
        }
        private void CameraShadowCascade(CutEvent e)
        {
        }
        private void CameraUnk1(CutEvent e)
        {
        }
        private void CameraUnk2(CutEvent e)
        {
        }
        private void CameraUnk3(CutEvent e)
        {
        }
        private void CameraUnk4(CutEvent e)
        {
        }
        private void CameraUnk5(CutEvent e)
        {
        }
        private void CameraUnk6(CutEvent e)
        {
        }
        private void CameraUnk7(CutEvent e)
        {
        }
        private void CameraUnk8(CutEvent e)
        {
        }
        private void DecalUnk1(CutEvent e)
        {
        }
        private void DecalUnk2(CutEvent e)
        {
        }
        private void PropUnk1(CutEvent e)
        {
        }
        private void Unk1(CutEvent e)
        {
        }
        private void Unk2(CutEvent e)
        {
        }
        private void VehicleUnk1(CutEvent e)
        {
        }
        private void PedUnk1(CutEvent e)
        {
        }



        private T[] RecastArray<T>(object[] arr) where T : class
        {
            if (arr == null) return null;
            var r = new T[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                r[i] = arr[i] as T;
            }
            return r;
        }


        private void CreateSceneObjects()
        {
            SceneObjects = new Dictionary<int, CutsceneObject>();

            if (Objects == null) return;


            var refCounts = new Dictionary<MetaHash, int>();

            foreach (var obj in Objects.Values)
            {
                var sobj = new CutsceneObject();
                sobj.Init(obj, GameFileCache);
                SceneObjects[sobj.ObjectID] = sobj;

                if (sobj.AnimHash != 0)
                {
                    int refcount = 0;
                    var hash = sobj.AnimHash;
                    refCounts.TryGetValue(hash, out refcount);
                    if (refcount > 0)
                    {
                        var newstr = hash.ToString() + "^" + refcount.ToString();
                        sobj.AnimHash = JenkHash.GenHash(newstr);
                    }
                    refcount++;
                    refCounts[hash] = refcount;
                }

            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class CutsceneObject
    {
        public int ObjectID { get; set; }
        public CutObject CutObject { get; set; }
        public MetaHash Name { get; set; }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public Ped Ped { get; set; }
        public YmapEntityDef Prop { get; set; }
        public Vehicle Vehicle { get; set; }
        public Weapon Weapon { get; set; }

        public MetaHash AnimHash { get; set; }
        public ClipMapEntry AnimClip { get; set; }

        public bool Enabled { get; set; } = false;


        public void Init(CutObject obj, GameFileCache gfc)
        {
            CutObject = obj;
            ObjectID = obj?.iObjectId ?? -1;

            if (obj is CutNamedObject nobj)
            {
                Name = nobj.cName;
            }

            if (obj is CutAnimationManagerObject anim)
            {
            }
            else if (obj is CutAssetManagerObject ass)
            {
            }
            else if (obj is CutCameraObject cam)
            {
            }
            else if (obj is CutPedModelObject ped)
            {
                InitPed(ped, gfc);
            }
            else if (obj is CutPropModelObject prop)
            {
                InitProp(prop, gfc);
            }
            else if (obj is CutVehicleModelObject veh)
            {
                InitVehicle(veh, gfc);
            }
            else if (obj is CutWeaponModelObject weap)
            {
                InitWeapon(weap, gfc);
            }
            else if (obj is CutHiddenModelObject hid)
            {
                InitHiddenModel(hid, gfc);
            }
            else if (obj is CutFixupModelObject fix)
            {
            }
            else if (obj is CutRayfireObject rayf)
            {
            }
            else if (obj is CutParticleEffectObject eff)
            {
            }
            else if (obj is CutAnimatedParticleEffectObject aeff)
            {
            }
            else if (obj is CutLightObject light)
            {
            }
            else if (obj is CutAnimatedLightObject alight)
            {
            }
            else if (obj is CutDecalObject dec)
            {
            }
            else if (obj is CutOverlayObject ovr)
            {
            }
            else if (obj is CutAudioObject aud)
            {
            }
            else if (obj is CutSubtitleObject sub)
            {
            }
            else if (obj is CutBlockingBoundsObject blk)
            {
            }
            else if (obj is CutScreenFadeObject fad)
            {
            }
            else
            { }
        }

        private void InitPed(CutPedModelObject ped, GameFileCache gfc)
        {

            Ped = new Ped();
            Ped.Init(ped.StreamingName, gfc);
            Ped.LoadDefaultComponents(gfc);

            if (ped.StreamingName == JenkHash.GenHash("player_zero"))
            {
                //for michael, switch his outfit so it's not glitching everywhere (until it's fixed?)
                Ped.SetComponentDrawable(3, 27, 0, 0, gfc);
                Ped.SetComponentDrawable(4, 19, 0, 0, gfc);
                Ped.SetComponentDrawable(6, null, null, gfc);
            }

            AnimHash = ped.StreamingName;
        }

        private void InitProp(CutPropModelObject prop, GameFileCache gfc)
        {

            Prop = new YmapEntityDef();
            Prop.SetArchetype(gfc.GetArchetype(prop.StreamingName));

            AnimHash = prop.StreamingName;
        }

        private void InitVehicle(CutVehicleModelObject veh, GameFileCache gfc)
        {
            var name = veh.StreamingName.ToString();

            Vehicle = new Vehicle();
            Vehicle.Init(name, gfc);

            AnimHash = veh.StreamingName;
        }

        private void InitWeapon(CutWeaponModelObject weap, GameFileCache gfc)
        {
            var name = weap.StreamingName.ToString();

            Weapon = new Weapon();
            Weapon.Init(name, gfc);

            AnimHash = weap.StreamingName;
        }

        private void InitHiddenModel(CutHiddenModelObject hid, GameFileCache gfc)
        {

        }


        public override string ToString()
        {
            return CutObject?.ToString() ?? (ObjectID.ToString() + ": " + Name.ToString());
        }
    }




}
