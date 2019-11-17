using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.Rendering;
using CodeWalker.World;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = SharpDX.Color;

namespace CodeWalker.Peds
{
    public partial class PedsForm : Form, DXForm
    {
        public Form Form { get { return this; } } //for DXForm/DXManager use

        public Renderer Renderer = null;
        public object RenderSyncRoot { get { return Renderer.RenderSyncRoot; } }

        volatile bool formopen = false;
        volatile bool running = false;
        volatile bool pauserendering = false;
        //volatile bool initialised = false;

        Stopwatch frametimer = new Stopwatch();
        Camera camera;
        Timecycle timecycle;
        Weather weather;
        Clouds clouds;

        Entity camEntity = new Entity();


        bool MouseLButtonDown = false;
        bool MouseRButtonDown = false;
        int MouseX;
        int MouseY;
        System.Drawing.Point MouseDownPoint;
        System.Drawing.Point MouseLastPoint;


        public GameFileCache GameFileCache { get; } = GameFileCacheFactory.Create();


        InputManager Input = new InputManager();


        bool initedOk = false;



        bool toolsPanelResizing = false;
        int toolsPanelResizeStartX = 0;
        int toolsPanelResizeStartLeft = 0;
        int toolsPanelResizeStartRight = 0;

        Dictionary<DrawableBase, bool> DrawableDrawFlags = new Dictionary<DrawableBase, bool>();

        bool enableGrid = false;
        float gridSize = 1.0f;
        int gridCount = 40;
        List<VertexTypePC> gridVerts = new List<VertexTypePC>();
        object gridSyncRoot = new object();




        [TypeConverter(typeof(ExpandableObjectConverter))] public class PedSelection
        {
            public string Name { get; set; } = string.Empty;
            public MetaHash NameHash { get; set; } = 0;//ped name hash
            public CPedModelInfo__InitData InitData { get; set; } = null; //ped init data
            public YddFile Ydd { get; set; } = null; //ped drawables
            public YtdFile Ytd { get; set; } = null; //ped textures
            public YcdFile Ycd { get; set; } = null; //ped animations
            public YftFile Yft { get; set; } = null; //ped skeleton YFT
            public PedFile Ymt { get; set; } = null; //ped variation info
            public Dictionary<MetaHash, RpfFileEntry> DrawableFilesDict { get; set; } = null;
            public Dictionary<MetaHash, RpfFileEntry> TextureFilesDict { get; set; } = null;
            public RpfFileEntry[] DrawableFiles { get; set; } = null;
            public RpfFileEntry[] TextureFiles { get; set; } = null;
            public ClipMapEntry AnimClip { get; set; } = null;
            public string[] DrawableNames { get; set; } = new string[12];
            public Drawable[] Drawables { get; set; } = new Drawable[12];
            public Texture[] Textures { get; set; } = new Texture[12];
            public bool EnableRootMotion { get; set; } = false; //used to toggle whether or not to include root motion when playing animations
        }

        PedSelection SelectedPed = new PedSelection();


        ComboBox[] ComponentComboBoxes = null;
        public class ComponentComboItem
        {
            public MUnk_1535046754 DrawableData { get; set; }
            public int AlternativeIndex { get; set; }
            public int TextureIndex { get; set; }
            public ComponentComboItem(MUnk_1535046754 drawableData, int altIndex = 0, int textureIndex = -1)
            {
                DrawableData = drawableData;
                AlternativeIndex = altIndex;
                TextureIndex = textureIndex;
            }
            public override string ToString()
            {
                if (DrawableData == null) return TextureIndex.ToString();
                var itemname = DrawableData.GetDrawableName(AlternativeIndex);
                if (DrawableData.TexData?.Length > 0) return itemname + " + " + DrawableData.GetTextureSuffix(TextureIndex);
                return itemname;
            }
            public string DrawableName
            {
                get
                {
                    return DrawableData?.GetDrawableName(AlternativeIndex) ?? "error";
                }
            }
            public string TextureName
            {
                get
                {
                    return DrawableData?.GetTextureName(TextureIndex);
                }
            }
        }



        public PedsForm()
        {
            InitializeComponent();

            ComponentComboBoxes = new[]
            {
                CompHeadComboBox,
                CompBerdComboBox,
                CompHairComboBox,
                CompUpprComboBox,
                CompLowrComboBox,
                CompHandComboBox,
                CompFeetComboBox,
                CompTeefComboBox,
                CompAccsComboBox,
                CompTaskComboBox,
                CompDeclComboBox,
                CompJbibComboBox
            };


            Renderer = new Renderer(this, GameFileCache);
            camera = Renderer.camera;
            timecycle = Renderer.timecycle;
            weather = Renderer.weather;
            clouds = Renderer.clouds;

            initedOk = Renderer.Init();

            Renderer.controllightdir = !Settings.Default.Skydome;
            Renderer.rendercollisionmeshes = false;
            Renderer.renderclouds = false;
            //Renderer.renderclouds = true;
            //Renderer.individualcloudfrag = "Contrails";
            Renderer.rendermoon = false;
            Renderer.renderskeletons = false;
            Renderer.SelectionFlagsTestAll = true;
            Renderer.swaphemisphere = true;
        }

        public void InitScene(Device device)
        {
            int width = ClientSize.Width;
            int height = ClientSize.Height;

            try
            {
                Renderer.DeviceCreated(device, width, height);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading shaders!\n" + ex.ToString());
                return;
            }


            camera.FollowEntity = camEntity;
            camera.FollowEntity.Position = Vector3.Zero;// prevworldpos;
            camera.FollowEntity.Orientation = Quaternion.LookAtLH(Vector3.Zero, Vector3.Up, Vector3.ForwardLH);
            camera.TargetDistance = 2.0f;
            camera.CurrentDistance = 2.0f;
            camera.TargetRotation.Y = 0.2f;
            camera.CurrentRotation.Y = 0.2f;
            camera.TargetRotation.X = 1.0f * (float)Math.PI;
            camera.CurrentRotation.X = 1.0f * (float)Math.PI;


            LoadSettings();


            formopen = true;
            new Thread(new ThreadStart(ContentThread)).Start();

            frametimer.Start();

        }
        public void CleanupScene()
        {
            formopen = false;

            Renderer.DeviceDestroyed();

            int count = 0;
            while (running && (count < 5000)) //wait for the content thread to exit gracefully
            {
                Thread.Sleep(1);
                count++;
            }
        }
        public void RenderScene(DeviceContext context)
        {
            float elapsed = (float)frametimer.Elapsed.TotalSeconds;
            frametimer.Restart();

            if (pauserendering) return;

            if (!Monitor.TryEnter(Renderer.RenderSyncRoot, 50))
            { return; } //couldn't get a lock, try again next time

            UpdateControlInputs(elapsed);
            //space.Update(elapsed);


            Renderer.Update(elapsed, MouseLastPoint.X, MouseLastPoint.Y);



            //UpdateWidgets();
            //BeginMouseHitTest();




            Renderer.BeginRender(context);

            Renderer.RenderSkyAndClouds();

            Renderer.SelectedDrawable = null;// SelectedItem.Drawable;


            RenderPed();

            //UpdateMouseHitsFromRenderer();
            //RenderSelection();


            RenderGrid(context);


            Renderer.RenderQueued();

            //Renderer.RenderBounds(MapSelectionMode.Entity);

            //Renderer.RenderSelectionGeometry(MapSelectionMode.Entity);

            //RenderMoused();

            Renderer.RenderFinalPass();

            //RenderMarkers();
            //RenderWidgets();

            Renderer.EndRender();

            Monitor.Exit(Renderer.RenderSyncRoot);

            //UpdateMarkerSelectionPanelInvoke();
        }
        public void BuffersResized(int w, int h)
        {
            Renderer.BuffersResized(w, h);
        }





        private void Init()
        {
            //called from PedForm_Load

            if (!initedOk)
            {
                Close();
                return;
            }


            MouseWheel += PedsForm_MouseWheel;

            if (!GTAFolder.UpdateGTAFolder(true))
            {
                Close();
                return;
            }



            ShaderParamNames[] texsamplers = RenderableGeometry.GetTextureSamplerList();
            foreach (var texsampler in texsamplers)
            {
                TextureSamplerComboBox.Items.Add(texsampler);
            }
            //TextureSamplerComboBox.SelectedIndex = 0;//LoadSettings will do this..


            UpdateGridVerts();
            GridSizeComboBox.SelectedIndex = 1;
            GridCountComboBox.SelectedIndex = 1;



            Input.Init();


            Renderer.Start();
        }


        private void ContentThread()
        {
            //main content loading thread.
            running = true;

            UpdateStatus("Scanning...");

            try
            {
                GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
            }
            catch
            {
                MessageBox.Show("Keys not found! This shouldn't happen.");
                Close();
                return;
            }

            GameFileCache.EnableDlc = true;
            GameFileCache.EnableMods = true;
            GameFileCache.LoadPeds = true;
            GameFileCache.LoadArchetypes = false;//to speed things up a little
            GameFileCache.BuildExtendedJenkIndex = false;//to speed things up a little
            GameFileCache.DoFullStringIndex = true;//to get all global text from DLC...
            GameFileCache.Init(UpdateStatus, LogError);

            //UpdateDlcListComboBox(gameFileCache.DlcNameList);

            //EnableCacheDependentUI();

            UpdateGlobalPedsUI();


            LoadWorld();



            //initialised = true;

            //EnableDLCModsUI();

            //UpdateStatus("Ready");


            Task.Run(() => {
                while (formopen && !IsDisposed) //renderer content loop
                {
                    bool rcItemsPending = Renderer.ContentThreadProc();

                    if (!rcItemsPending)
                    {
                        Thread.Sleep(1); //sleep if there's nothing to do
                    }
                }
            });

            while (formopen && !IsDisposed) //main asset loop
            {
                bool fcItemsPending = GameFileCache.ContentThreadProc();

                if (!fcItemsPending)
                {
                    Thread.Sleep(1); //sleep if there's nothing to do
                }
            }

            GameFileCache.Clear();

            running = false;
        }




        private void LoadSettings()
        {
            var s = Settings.Default;
            //WindowState = s.WindowMaximized ? FormWindowState.Maximized : WindowState;
            //FullScreenCheckBox.Checked = s.FullScreen;
            WireframeCheckBox.Checked = s.Wireframe;
            HDRRenderingCheckBox.Checked = s.HDR;
            ShadowsCheckBox.Checked = s.Shadows;
            SkydomeCheckBox.Checked = s.Skydome;
            RenderModeComboBox.SelectedIndex = Math.Max(RenderModeComboBox.FindString(s.RenderMode), 0);
            TextureSamplerComboBox.SelectedIndex = Math.Max(TextureSamplerComboBox.FindString(s.RenderTextureSampler), 0);
            TextureCoordsComboBox.SelectedIndex = Math.Max(TextureCoordsComboBox.FindString(s.RenderTextureSamplerCoord), 0);
            AnisotropicFilteringCheckBox.Checked = s.AnisotropicFiltering;
            //ErrorConsoleCheckBox.Checked = s.ShowErrorConsole;
            //StatusBarCheckBox.Checked = s.ShowStatusBar;
        }



        private void LoadWorld()
        {
            UpdateStatus("Loading timecycles...");
            timecycle.Init(GameFileCache, UpdateStatus);
            timecycle.SetTime(Renderer.timeofday);

            UpdateStatus("Loading materials...");
            BoundsMaterialTypes.Init(GameFileCache);

            UpdateStatus("Loading weather...");
            weather.Init(GameFileCache, UpdateStatus, timecycle);
            //UpdateWeatherTypesComboBox(weather);

            UpdateStatus("Loading clouds...");
            clouds.Init(GameFileCache, UpdateStatus, weather);
            //UpdateCloudTypesComboBox(clouds);

        }






        private void UpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateStatus(text); }));
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch { }
        }
        private void LogError(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { LogError(text); }));
                }
                else
                {
                    //TODO: error logging..
                    ConsoleTextBox.AppendText(text + "\r\n");
                    //StatusLabel.Text = text;
                    //MessageBox.Show(text);
                }
            }
            catch { }
        }




        private void UpdateMousePosition(MouseEventArgs e)
        {
            MouseX = e.X;
            MouseY = e.Y;
            MouseLastPoint = e.Location;
        }

        private void RotateCam(int dx, int dy)
        {
            camera.MouseRotate(dx, dy);
        }

        private void MoveCameraToView(Vector3 pos, float rad)
        {
            //move the camera to a default place where the given sphere is fully visible.

            rad = Math.Max(0.01f, rad*0.1f);

            camera.FollowEntity.Position = pos;
            camera.TargetDistance = rad * 1.2f;
            camera.CurrentDistance = rad * 1.2f;

            camera.ZFar = Math.Min(rad * 200.0f, 12000.0f);
            camera.ZNear = Math.Min(camera.ZFar * 5e-5f, 0.5f);
            camera.UpdateProj = true;

        }







        private void AddDrawableTreeNode(DrawableBase drawable, string name, bool check)
        {
            var tnode = TexturesTreeView.Nodes.Add(name);
            var dnode = ModelsTreeView.Nodes.Add(name);
            dnode.Tag = drawable;
            dnode.Checked = check;

            AddDrawableModelsTreeNodes(drawable.DrawableModelsHigh?.data_items, "High Detail", true, dnode, tnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModelsMedium?.data_items, "Medium Detail", false, dnode, tnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModelsLow?.data_items, "Low Detail", false, dnode, tnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModelsVeryLow?.data_items, "Very Low Detail", false, dnode, tnode);
            //AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModelsX, "X Detail", false, dnode, tnode);

        }
        private void AddDrawableModelsTreeNodes(DrawableModel[] models, string prefix, bool check, TreeNode parentDrawableNode = null, TreeNode parentTextureNode = null)
        {
            if (models == null) return;

            for (int mi = 0; mi < models.Length; mi++)
            {
                var tnc = (parentDrawableNode != null) ? parentDrawableNode.Nodes : ModelsTreeView.Nodes;

                var model = models[mi];
                string mprefix = prefix + " " + (mi + 1).ToString();
                var mnode = tnc.Add(mprefix + " " + model.ToString());
                mnode.Tag = model;
                mnode.Checked = check;

                var ttnc = (parentTextureNode != null) ? parentTextureNode.Nodes : TexturesTreeView.Nodes;
                var tmnode = ttnc.Add(mprefix + " " + model.ToString());
                tmnode.Tag = model;

                if (!check)
                {
                    Renderer.SelectionModelDrawFlags[model] = false;
                }

                if ((model.Geometries == null) || (model.Geometries.data_items == null)) continue;

                foreach (var geom in model.Geometries.data_items)
                {
                    var gname = geom.ToString();
                    var gnode = mnode.Nodes.Add(gname);
                    gnode.Tag = geom;
                    gnode.Checked = true;// check;

                    var tgnode = tmnode.Nodes.Add(gname);
                    tgnode.Tag = geom;

                    if ((geom.Shader != null) && (geom.Shader.ParametersList != null) && (geom.Shader.ParametersList.Hashes != null))
                    {
                        var pl = geom.Shader.ParametersList;
                        var h = pl.Hashes;
                        var p = pl.Parameters;
                        for (int ip = 0; ip < h.Length; ip++)
                        {
                            var hash = pl.Hashes[ip];
                            var parm = pl.Parameters[ip];
                            var tex = parm.Data as TextureBase;
                            if (tex != null)
                            {
                                var t = tex as Texture;
                                var tstr = tex.Name.Trim();
                                if (t != null)
                                {
                                    tstr = string.Format("{0} ({1}x{2}, embedded)", tex.Name, t.Width, t.Height);
                                }
                                var tnode = tgnode.Nodes.Add(hash.ToString().Trim() + ": " + tstr);
                                tnode.Tag = tex;
                            }
                        }
                        tgnode.Expand();
                    }

                }

                mnode.Expand();
                tmnode.Expand();
            }
        }
        private void UpdateSelectionDrawFlags(TreeNode node)
        {
            //update the selection draw flags depending on tag and checked/unchecked
            var drwbl = node.Tag as DrawableBase;
            var model = node.Tag as DrawableModel;
            var geom = node.Tag as DrawableGeometry;
            bool rem = node.Checked;
            lock (Renderer.RenderSyncRoot)
            {
                if (drwbl != null)
                {
                    if (rem)
                    {
                        if (DrawableDrawFlags.ContainsKey(drwbl))
                        {
                            DrawableDrawFlags.Remove(drwbl);
                        }
                    }
                    else
                    {
                        DrawableDrawFlags[drwbl] = false;
                    }
                }
                if (model != null)
                {
                    if (rem)
                    {
                        if (Renderer.SelectionModelDrawFlags.ContainsKey(model))
                        {
                            Renderer.SelectionModelDrawFlags.Remove(model);
                        }
                    }
                    else
                    {
                        Renderer.SelectionModelDrawFlags[model] = false;
                    }
                }
                if (geom != null)
                {
                    if (rem)
                    {
                        if (Renderer.SelectionGeometryDrawFlags.ContainsKey(geom))
                        {
                            Renderer.SelectionGeometryDrawFlags.Remove(geom);
                        }
                    }
                    else
                    {
                        Renderer.SelectionGeometryDrawFlags[geom] = false;
                    }
                }
                //updateArchetypeStatus = true;
            }
        }


        private void UpdateGlobalPedsUI()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { UpdateGlobalPedsUI(); }));
            }
            else
            {

                ClipComboBox.Items.Clear();
                ClipDictComboBox.Items.Clear();
                var ycds = GameFileCache.YcdDict.Values.ToList();
                ycds.Sort((a, b) => { return a.Name.CompareTo(b.Name); });
                ClipDictComboBox.AutoCompleteCustomSource.Clear();
                List<string> ycdlist = new List<string>();
                foreach (var ycde in ycds)
                {
                    ycdlist.Add(ycde.GetShortName());
                }
                ClipDictComboBox.AutoCompleteCustomSource.AddRange(ycdlist.ToArray());
                ClipDictComboBox.Text = "";



                PedNameComboBox.Items.Clear();
                var peds = GameFileCache.PedsInitDict.Values.ToList();
                peds.Sort((a, b) => { return a.Name.CompareTo(b.Name); });
                foreach (var ped in peds)
                {
                    PedNameComboBox.Items.Add(ped.Name);
                }
                if (peds.Count > 0)
                {
                    var ind = PedNameComboBox.FindString("A_F_M_Beach_01"); // //A_C_Pug
                    PedNameComboBox.SelectedIndex = Math.Max(ind, 0);
                    //PedNameComboBox.SelectedIndex = 0;
                }

            }

        }





        private void UpdateModelsUI()
        {
            //TODO: change to go through each component and add/update/remove treeview item accordingly?

            DrawableDrawFlags.Clear();
            Renderer.SelectionModelDrawFlags.Clear();
            Renderer.SelectionGeometryDrawFlags.Clear();
            ModelsTreeView.Nodes.Clear();
            ModelsTreeView.ShowRootLines = true;
            TexturesTreeView.Nodes.Clear();
            TexturesTreeView.ShowRootLines = true;

            if (SelectedPed == null) return;


            for (int i = 0; i < 12; i++)
            {
                var drawable = SelectedPed.Drawables[i];
                var drawablename = SelectedPed.DrawableNames[i];

                if (drawable != null)
                {
                    AddDrawableTreeNode(drawable, drawablename, true);
                }
            }

        }




        public void LoadPed()
        {
            var pedname = PedNameComboBox.Text;
            var pednamel = pedname.ToLowerInvariant();
            MetaHash pedhash = JenkHash.GenHash(pednamel);

            SelectedPed.Name = string.Empty;
            SelectedPed.NameHash = 0;
            SelectedPed.InitData = null;
            SelectedPed.Ydd = null;
            SelectedPed.Ytd = null;
            SelectedPed.Ycd = null;
            SelectedPed.Yft = null;
            SelectedPed.Ymt = null;
            SelectedPed.AnimClip = null;
            for (int i = 0; i < 12; i++)
            {
                ClearCombo(ComponentComboBoxes[i]);
                SelectedPed.Drawables[i] = null;
                SelectedPed.Textures[i] = null;
            }

            DetailsPropertyGrid.SelectedObject = null;


            CPedModelInfo__InitData initdata = null;
            if (!GameFileCache.PedsInitDict.TryGetValue(pedhash, out initdata)) return;

            var ycdhash = JenkHash.GenHash(initdata.ClipDictionaryName.ToLowerInvariant());

            bool pedchange = SelectedPed.NameHash != pedhash;
            SelectedPed.Name = pedname;
            SelectedPed.NameHash = pedhash;
            SelectedPed.InitData = initdata;
            SelectedPed.Ydd = GameFileCache.GetYdd(pedhash);
            SelectedPed.Ytd = GameFileCache.GetYtd(pedhash);
            SelectedPed.Ycd = GameFileCache.GetYcd(ycdhash);
            SelectedPed.Yft = GameFileCache.GetYft(pedhash);

            PedFile pedFile = null;
            GameFileCache.PedVariationsDict?.TryGetValue(pedhash, out pedFile);
            SelectedPed.Ymt = pedFile;

            Dictionary<MetaHash, RpfFileEntry> peddict = null;
            GameFileCache.PedDrawableDicts.TryGetValue(SelectedPed.NameHash, out peddict);
            SelectedPed.DrawableFilesDict = peddict;
            SelectedPed.DrawableFiles = SelectedPed.DrawableFilesDict?.Values.ToArray();
            GameFileCache.PedTextureDicts.TryGetValue(SelectedPed.NameHash, out peddict);
            SelectedPed.TextureFilesDict = peddict;
            SelectedPed.TextureFiles = SelectedPed.TextureFilesDict?.Values.ToArray();


            while ((SelectedPed.Ydd != null) && (!SelectedPed.Ydd.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                SelectedPed.Ydd = GameFileCache.GetYdd(pedhash);
            }
            while ((SelectedPed.Ytd != null) && (!SelectedPed.Ytd.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                SelectedPed.Ytd = GameFileCache.GetYtd(pedhash);
            }
            while ((SelectedPed.Ycd != null) && (!SelectedPed.Ycd.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                SelectedPed.Ycd = GameFileCache.GetYcd(ycdhash);
            }
            while ((SelectedPed.Yft != null) && (!SelectedPed.Yft.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                SelectedPed.Yft = GameFileCache.GetYft(pedhash);
            }

            LoadModel(SelectedPed.Yft, pedchange);


            var vi = SelectedPed.Ymt?.VariationInfo;
            if (vi != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    PopulateCompCombo(ComponentComboBoxes[i], vi.GetComponentData(i));
                }
            }



            ClipDictComboBox.Text = SelectedPed.InitData?.ClipDictionaryName ?? "";
            ClipComboBox.Text = "idle";
            MetaHash cliphash = JenkHash.GenHash("idle");
            ClipMapEntry cme = null;
            SelectedPed.Ycd?.ClipMap?.TryGetValue(cliphash, out cme);
            SelectedPed.AnimClip = cme;


            DetailsPropertyGrid.SelectedObject = SelectedPed;

            UpdateModelsUI();

        }

        public void LoadModel(YftFile yft, bool movecamera = true)
        {
            if (yft == null) return;

            //FileName = yft.Name;
            //Yft = yft;

            var dr = yft.Fragment?.Drawable;
            if (movecamera && (dr != null))
            {
                MoveCameraToView(dr.BoundingCenter, dr.BoundingSphereRadius);
            }

            //UpdateModelsUI(yft.Fragment.Drawable);
        }



        private void ClearCombo(ComboBox c)
        {
            c.Items.Clear();
            c.Items.Add("");
            c.Text = string.Empty;
        }
        private void PopulateCompCombo(ComboBox c, MUnk_3538495220 compData)
        {
            if (compData?.DrawblData3 == null) return;
            foreach (var item in compData.DrawblData3)
            {
                for (int alt = 0; alt <= item.NumAlternatives; alt++)
                {
                    if (item.TexData?.Length > 0)
                    {
                        for (int tex = 0; tex < item.TexData.Length; tex++)
                        {
                            c.Items.Add(new ComponentComboItem(item, alt, tex));
                        }
                    }
                    else
                    {
                        c.Items.Add(new ComponentComboItem(item));
                    }
                }
            }
            if (compData.DrawblData3.Length > 0)
            {
                c.SelectedIndex = 1;
            }
        }

        private void SetComponentDrawable(int index, object comboObj)
        {

            var comboItem = comboObj as ComponentComboItem;
            var name = comboItem?.DrawableName;
            if (string.IsNullOrEmpty(name))
            {
                SelectedPed.DrawableNames[index] = null;
                SelectedPed.Drawables[index] = null;
                SelectedPed.Textures[index] = null;
                UpdateModelsUI();
                return;
            }

            MetaHash namehash = JenkHash.GenHash(name.ToLowerInvariant());
            Drawable d = null;
            if (SelectedPed.Ydd?.Dict != null)
            {
                SelectedPed.Ydd.Dict.TryGetValue(namehash, out d);
            }
            if ((d == null) && (SelectedPed.DrawableFilesDict != null))
            {
                RpfFileEntry file = null;
                if (SelectedPed.DrawableFilesDict.TryGetValue(namehash, out file))
                {
                    var ydd = GameFileCache.GetFileUncached<YddFile>(file);
                    while ((ydd != null) && (!ydd.Loaded))
                    {
                        Thread.Sleep(20);//kinda hacky
                        GameFileCache.TryLoadEnqueue(ydd);
                    }
                    if (ydd?.Drawables?.Length > 0)
                    {
                        d = ydd.Drawables[0];//should only be one in this dict
                    }
                }
            }


            var tex = comboItem.TextureName;
            MetaHash texhash = JenkHash.GenHash(tex.ToLowerInvariant());
            Texture t = null;
            if (SelectedPed.Ytd?.TextureDict?.Dict != null)
            {
                SelectedPed.Ytd.TextureDict.Dict.TryGetValue(texhash, out t);
            }
            if ((t == null) && (SelectedPed.TextureFilesDict != null))
            {
                RpfFileEntry file = null;
                if (SelectedPed.TextureFilesDict.TryGetValue(texhash, out file))
                {
                    var ytd = GameFileCache.GetFileUncached<YtdFile>(file);
                    while ((ytd != null) && (!ytd.Loaded))
                    {
                        Thread.Sleep(20);//kinda hacky
                        GameFileCache.TryLoadEnqueue(ytd);
                    }
                    if (ytd?.TextureDict?.Textures?.data_items.Length > 0)
                    {
                        t = ytd.TextureDict.Textures.data_items[0];//should only be one in this dict
                    }
                }
            }


            if (d != null) SelectedPed.Drawables[index] = d;
            if (t != null) SelectedPed.Textures[index] = t;

            SelectedPed.DrawableNames[index] = name;

            UpdateModelsUI();
        }






        private void LoadClipDict(string name)
        {
            var ycdhash = JenkHash.GenHash(name.ToLowerInvariant());
            var ycd = GameFileCache.GetYcd(ycdhash);
            while ((ycd != null) && (!ycd.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                ycd = GameFileCache.GetYcd(ycdhash);
            }



            //if (ycd != null)
            //{
            //    ////// TESTING XML CONVERSIONS
            //    //var data = ycd.Save();
            //    var xml = YcdXml.GetXml(ycd);
            //    var ycd2 = XmlYcd.GetYcd(xml);
            //    var data = ycd2.Save();
            //    var ycd3 = new YcdFile();
            //    RpfFile.LoadResourceFile(ycd3, data, 46);
            //    //var xml2 = YcdXml.GetXml(ycd3);
            //    //if (xml != xml2)
            //    //{ }
            //    ycd = ycd3;
            //}



            SelectedPed.Ycd = ycd;

            ClipComboBox.Items.Clear();
            ClipComboBox.Items.Add("");

            if (ycd?.ClipMapEntries == null)
            {
                ClipComboBox.SelectedIndex = 0;
                SelectedPed.AnimClip = null;
                return;
            }

            List<string> items = new List<string>();

            foreach (var cme in ycd.ClipMapEntries)
            {
                var animclip = cme.Clip as ClipAnimation;
                if (animclip != null)
                {
                    items.Add(animclip.ShortName);
                    continue;
                }
                var animcliplist = cme.Clip as ClipAnimationList;
                if (animcliplist?.Animations?.Data != null)
                {
                    items.Add(animcliplist.ShortName);
                    continue;
                }
            }


            items.Sort();
            foreach (var item in items)
            {
                ClipComboBox.Items.Add(item);
            }
        }

        private void SelectClip(string name)
        {
            MetaHash cliphash = JenkHash.GenHash(name);
            ClipMapEntry cme = null;
            SelectedPed.Ycd?.ClipMap?.TryGetValue(cliphash, out cme);
            SelectedPed.AnimClip = cme;
        }





        private void UpdateTimeOfDayLabel()
        {
            int v = TimeOfDayTrackBar.Value;
            float fh = v / 60.0f;
            int ih = (int)fh;
            int im = v - (ih * 60);
            if (ih == 24) ih = 0;
            TimeOfDayLabel.Text = string.Format("{0:00}:{1:00}", ih, im);
        }


        private void UpdateControlInputs(float elapsed)
        {
            if (elapsed > 0.1f) elapsed = 0.1f;

            var s = Settings.Default;

            float moveSpeed = 2.0f;


            Input.Update(elapsed);

            if (Input.xbenable)
            {
                //if (ControllerButtonJustPressed(GamepadButtonFlags.Start))
                //{
                //    SetControlMode(ControlMode == WorldControlMode.Free ? WorldControlMode.Ped : WorldControlMode.Free);
                //}
            }



            if (Input.ShiftPressed)
            {
                moveSpeed *= 5.0f;
            }
            if (Input.CtrlPressed)
            {
                moveSpeed *= 0.2f;
            }

            Vector3 movevec = Input.KeyboardMoveVec(false);


            //if (MapViewEnabled == true)
            //{
            //    movevec *= elapsed * 100.0f * Math.Min(camera.OrthographicTargetSize * 0.01f, 30.0f);
            //    float mapviewscale = 1.0f / camera.Height;
            //    float fdx = MapViewDragX * mapviewscale;
            //    float fdy = MapViewDragY * mapviewscale;
            //    movevec.X -= fdx * camera.OrthographicSize;
            //    movevec.Y += fdy * camera.OrthographicSize;
            //}
            //else
            {
                //normal movement
                movevec *= elapsed * moveSpeed * Math.Min(camera.TargetDistance, 50.0f);
            }


            Vector3 movewvec = camera.ViewInvQuaternion.Multiply(movevec);
            camEntity.Position += movewvec;

            //MapViewDragX = 0;
            //MapViewDragY = 0;




            if (Input.xbenable)
            {
                camera.ControllerRotate(Input.xblx + Input.xbrx, Input.xbly + Input.xbry);

                float zoom = 0.0f;
                float zoomspd = s.XInputZoomSpeed;
                float zoomamt = zoomspd * elapsed;
                if (Input.ControllerButtonPressed(GamepadButtonFlags.DPadUp)) zoom += zoomamt;
                if (Input.ControllerButtonPressed(GamepadButtonFlags.DPadDown)) zoom -= zoomamt;

                camera.ControllerZoom(zoom);

                float acc = 0.0f;
                float accspd = s.XInputMoveSpeed;//actually accel speed...
                acc += Input.xbrt * accspd;
                acc -= Input.xblt * accspd;

                Vector3 newdir = camera.ViewDirection; //maybe use the "vehicle" direction...?
                Input.xbcontrolvelocity += (acc * elapsed);

                if (Input.ControllerButtonPressed(GamepadButtonFlags.A | GamepadButtonFlags.RightShoulder)) //handbrake...
                {
                    Input.xbcontrolvelocity *= Math.Max(0.75f - elapsed, 0);//not ideal for low fps...
                                                                            //xbcontrolvelocity = 0.0f;
                    if (Math.Abs(Input.xbcontrolvelocity) < 0.001f) Input.xbcontrolvelocity = 0.0f;
                }

                camEntity.Velocity = newdir * Input.xbcontrolvelocity;
                camEntity.Position += camEntity.Velocity * elapsed;


                //fire!
                //if (ControllerButtonJustPressed(GamepadButtonFlags.LeftShoulder))
                //{
                //    SpawnTestEntity(true);
                //}

            }



        }



        private void UpdateGridVerts()
        {
            lock (gridSyncRoot)
            {
                gridVerts.Clear();

                float s = gridSize * gridCount * 0.5f;
                uint cblack = (uint)Color.Black.ToRgba();
                uint cgray = (uint)Color.DimGray.ToRgba();
                uint cred = (uint)Color.DarkRed.ToRgba();
                uint cgrn = (uint)Color.DarkGreen.ToRgba();
                int interval = 10;

                for (int i = 0; i <= gridCount; i++)
                {
                    float o = (gridSize * i) - s;
                    if ((i % interval) != 0)
                    {
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, -s, 0), Colour = cgray });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, s, 0), Colour = cgray });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(-s, o, 0), Colour = cgray });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(s, o, 0), Colour = cgray });
                    }
                }
                for (int i = 0; i <= gridCount; i++) //draw main lines last, so they are on top
                {
                    float o = (gridSize * i) - s;
                    if ((i % interval) == 0)
                    {
                        var cx = (o == 0) ? cred : cblack;
                        var cy = (o == 0) ? cgrn : cblack;
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, -s, 0), Colour = cy });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, s, 0), Colour = cy });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(-s, o, 0), Colour = cx });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(s, o, 0), Colour = cx });
                    }
                }

            }
        }

        private void RenderGrid(DeviceContext context)
        {
            if (!enableGrid) return;

            lock (gridSyncRoot)
            {
                if (gridVerts.Count > 0)
                {
                    Renderer.RenderLines(gridVerts);
                }
            }
        }







        private void RenderPed()
        {

            YftFile yft = SelectedPed.Yft;// GameFileCache.GetYft(SelectedModelHash);
            if (yft != null)
            {
                if (yft.Loaded)
                {
                    if (yft.Fragment != null)
                    {
                        //var f = yft.Fragment;
                        //var txdhash = 0u;// SelectedVehicleHash;// yft.RpfFileEntry?.ShortNameHash ?? 0;
                        //var namelower = yft.RpfFileEntry?.GetShortNameLower();
                        //Archetype arch = null;// TryGetArchetype(hash);
                        //Renderer.RenderFragment(arch, null, f, txdhash);
                        //seldrwbl = f.Drawable;
                    }
                }


                var vi = SelectedPed.Ymt?.VariationInfo;
                if (vi != null)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        RenderPedComponent(i);
                    }
                }

            }
            
        }

        private void RenderPedComponent(int i)
        {
            //var compData = SelectedPed.Ymt?.VariationInfo?.GetComponentData(i);
            var drawable = SelectedPed.Drawables[i];
            var texture = SelectedPed.Textures[i];

            //if (compData == null) return;
            if (drawable == null) return;

            var td = SelectedPed.Ytd?.TextureDict;
            var ac = SelectedPed.AnimClip;
            if (ac != null)
            {
                ac.EnableRootMotion = SelectedPed.EnableRootMotion;
            }

            var skel = SelectedPed.Yft?.Fragment?.Drawable?.Skeleton;
            if (skel != null)
            {
                if (drawable.Skeleton == null)
                {
                    drawable.Skeleton = skel;//force the drawable to use this skeleton.
                }
                else if (drawable.Skeleton != skel)
                {
                    var dskel = drawable.Skeleton; //put the bones of the fragment into the drawable. drawable's bones in this case seem messed up!
                    for (int b = 0; b < skel.Bones.Count; b++)
                    {
                        var srcbone = skel.Bones[b];
                        var dstbone = srcbone;
                        if (dskel.BonesMap.TryGetValue(srcbone.Tag, out dstbone))
                        {
                            if (srcbone == dstbone) break; //bone reassignment already done!
                            dskel.Bones[dstbone.Index] = srcbone;
                            dskel.BonesMap[srcbone.Tag] = srcbone;
                        }
                    }
                }
            }


            bool drawFlag = true;
            if (!DrawableDrawFlags.TryGetValue(drawable, out drawFlag))
            { drawFlag = true; }

            if (drawFlag)
            {
                Renderer.RenderDrawable(drawable, null, null, 0, td, texture, ac);
            }


        }








        private void PedsForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void PedsForm_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = true; break;
                case MouseButtons.Right: MouseRButtonDown = true; break;
            }

            if (!ToolsPanelShowButton.Focused)
            {
                ToolsPanelShowButton.Focus(); //make sure no textboxes etc are focused!
            }

            MouseDownPoint = e.Location;
            MouseLastPoint = MouseDownPoint;

            if (MouseLButtonDown)
            {
            }

            if (MouseRButtonDown)
            {
                //SelectMousedItem();
            }

            MouseX = e.X; //to stop jumps happening on mousedown, sometimes the last MouseMove event was somewhere else... (eg after clicked a menu)
            MouseY = e.Y;
        }

        private void PedsForm_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = false; break;
                case MouseButtons.Right: MouseRButtonDown = false; break;
            }



            if (e.Button == MouseButtons.Left)
            {
            }
        }

        private void PedsForm_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - MouseX;
            int dy = e.Y - MouseY;

            //if (MouseInvert)
            //{
            //    dy = -dy;
            //}

            //if (ControlMode == WorldControlMode.Free && !ControlBrushEnabled)
            {
                if (MouseLButtonDown)
                {
                    RotateCam(dx, dy);
                }
                if (MouseRButtonDown)
                {
                    if (Renderer.controllightdir)
                    {
                        Renderer.lightdirx += (dx * camera.Sensitivity);
                        Renderer.lightdiry += (dy * camera.Sensitivity);
                    }
                    else if (Renderer.controltimeofday)
                    {
                        float tod = Renderer.timeofday;
                        tod += (dx - dy) / 30.0f;
                        while (tod >= 24.0f) tod -= 24.0f;
                        while (tod < 0.0f) tod += 24.0f;
                        timecycle.SetTime(tod);
                        Renderer.timeofday = tod;

                        float fv = tod * 60.0f;
                        TimeOfDayTrackBar.Value = (int)fv;
                        UpdateTimeOfDayLabel();
                    }
                }

                UpdateMousePosition(e);

            }



        }

        private void PedsForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                camera.MouseZoom(e.Delta);
            }
        }

        private void PedsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (ActiveControl is TextBox)
            {
                var tb = ActiveControl as TextBox;
                if (!tb.ReadOnly) return; //don't move the camera when typing!
            }
            if (ActiveControl is ComboBox)
            {
                var cb = ActiveControl as ComboBox;
                if (cb.DropDownStyle != ComboBoxStyle.DropDownList) return; //nontypable combobox
            }

            bool enablemove = true;// (!iseditmode) || (MouseLButtonDown && (GrabbedMarker == null) && (GrabbedWidget == null));

            Input.KeyDown(e, enablemove);

            var k = e.KeyCode;
            var kb = Input.keyBindings;
            bool ctrl = Input.CtrlPressed;
            bool shift = Input.ShiftPressed;


            if (!ctrl)
            {
                if (k == kb.MoveSlowerZoomIn)
                {
                    camera.MouseZoom(1);
                }
                if (k == kb.MoveFasterZoomOut)
                {
                    camera.MouseZoom(-1);
                }
            }


            if (!Input.kbmoving) //don't trigger further actions if moving.
            {
                if (!ctrl)
                {

                }
                else
                {
                    //switch (k)
                    //{
                    //    //case Keys.N:
                    //    //    New();
                    //    //    break;
                    //    //case Keys.O:
                    //    //    Open();
                    //    //    break;
                    //    //case Keys.S:
                    //    //    if (shift) SaveAll();
                    //    //    else Save();
                    //    //    break;
                    //    //case Keys.Z:
                    //    //    Undo();
                    //    //    break;
                    //    //case Keys.Y:
                    //    //    Redo();
                    //    //    break;
                    //    //case Keys.C:
                    //    //    CopyItem();
                    //    //    break;
                    //    //case Keys.V:
                    //    //    PasteItem();
                    //    //    break;
                    //    //case Keys.U:
                    //    //    ToolsPanelShowButton.Visible = !ToolsPanelShowButton.Visible;
                    //    //    break;
                    //}
                }
            }

            //if (ControlMode != WorldControlMode.Free || ControlBrushEnabled)
            //{
            //    e.Handled = true;
            //}
        }

        private void PedsForm_KeyUp(object sender, KeyEventArgs e)
        {
            Input.KeyUp(e);

            if (ActiveControl is TextBox)
            {
                var tb = ActiveControl as TextBox;
                if (!tb.ReadOnly) return; //don't move the camera when typing!
            }
            if (ActiveControl is ComboBox)
            {
                var cb = ActiveControl as ComboBox;
                if (cb.DropDownStyle != ComboBoxStyle.DropDownList) return; //non-typable combobox
            }

            //if (ControlMode != WorldControlMode.Free)
            //{
            //    e.Handled = true;
            //}
        }

        private void PedsForm_Deactivate(object sender, EventArgs e)
        {
            //try not to lock keyboard movement if the form loses focus.
            Input.KeyboardStop();
        }

        private void StatsUpdateTimer_Tick(object sender, EventArgs e)
        {
            StatsLabel.Text = Renderer.GetStatusText();

            if (Renderer.timerunning)
            {
                float fv = Renderer.timeofday * 60.0f;
                //TimeOfDayTrackBar.Value = (int)fv;
                UpdateTimeOfDayLabel();
            }

            //CameraPositionTextBox.Text = FloatUtil.GetVector3String(camera.Position, "0.##");
        }

        private void ToolsPanelShowButton_Click(object sender, EventArgs e)
        {
            ToolsPanel.Visible = true;
        }

        private void ToolsPanelHideButton_Click(object sender, EventArgs e)
        {
            ToolsPanel.Visible = false;
        }

        private void ToolsDragPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                toolsPanelResizing = true;
                toolsPanelResizeStartX = e.X + ToolsPanel.Left + ToolsDragPanel.Left;
                toolsPanelResizeStartLeft = ToolsPanel.Left;
                toolsPanelResizeStartRight = ToolsPanel.Right;
            }
        }

        private void ToolsDragPanel_MouseUp(object sender, MouseEventArgs e)
        {
            toolsPanelResizing = false;
        }

        private void ToolsDragPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (toolsPanelResizing)
            {
                int rx = e.X + ToolsPanel.Left + ToolsDragPanel.Left;
                int dx = rx - toolsPanelResizeStartX;
                ToolsPanel.Width = toolsPanelResizeStartRight - toolsPanelResizeStartLeft + dx;
            }
        }

        private void ModelsTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                UpdateSelectionDrawFlags(e.Node);
            }
        }

        private void ModelsTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null)
            {
                e.Node.Checked = !e.Node.Checked;
                //UpdateSelectionDrawFlags(e.Node);
            }
        }

        private void ModelsTreeView_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; //stops annoying ding sound...
        }

        private void HDRRenderingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.shaders.hdr = HDRRenderingCheckBox.Checked;
            }
        }

        private void ShadowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.shaders.shadows = ShadowsCheckBox.Checked;
            }
        }

        private void SkydomeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderskydome = SkydomeCheckBox.Checked;
            //Renderer.controllightdir = !Renderer.renderskydome;
        }

        private void ControlLightDirCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.controllightdir = ControlLightDirCheckBox.Checked;
        }

        private void TimeOfDayTrackBar_Scroll(object sender, EventArgs e)
        {
            int v = TimeOfDayTrackBar.Value;
            float fh = v / 60.0f;
            UpdateTimeOfDayLabel();
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.timeofday = fh;
                timecycle.SetTime(Renderer.timeofday);
            }
        }

        private void ShowCollisionMeshesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.rendercollisionmeshes = ShowCollisionMeshesCheckBox.Checked;
            Renderer.rendercollisionmeshlayerdrawable = ShowCollisionMeshesCheckBox.Checked;
        }

        private void WireframeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.shaders.wireframe = WireframeCheckBox.Checked;
        }

        private void AnisotropicFilteringCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.shaders.AnisotropicFiltering = AnisotropicFilteringCheckBox.Checked;
        }

        private void HDTexturesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderhdtextures = HDTexturesCheckBox.Checked;
        }

        private void RenderModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextureSamplerComboBox.Enabled = false;
            TextureCoordsComboBox.Enabled = false;
            switch (RenderModeComboBox.Text)
            {
                default:
                case "Default":
                    Renderer.shaders.RenderMode = WorldRenderMode.Default;
                    break;
                case "Single texture":
                    Renderer.shaders.RenderMode = WorldRenderMode.SingleTexture;
                    TextureSamplerComboBox.Enabled = true;
                    TextureCoordsComboBox.Enabled = true;
                    break;
                case "Vertex normals":
                    Renderer.shaders.RenderMode = WorldRenderMode.VertexNormals;
                    break;
                case "Vertex tangents":
                    Renderer.shaders.RenderMode = WorldRenderMode.VertexTangents;
                    break;
                case "Vertex colour 1":
                    Renderer.shaders.RenderMode = WorldRenderMode.VertexColour;
                    Renderer.shaders.RenderVertexColourIndex = 1;
                    break;
                case "Vertex colour 2":
                    Renderer.shaders.RenderMode = WorldRenderMode.VertexColour;
                    Renderer.shaders.RenderVertexColourIndex = 2;
                    break;
                case "Vertex colour 3":
                    Renderer.shaders.RenderMode = WorldRenderMode.VertexColour;
                    Renderer.shaders.RenderVertexColourIndex = 3;
                    break;
                case "Texture coord 1":
                    Renderer.shaders.RenderMode = WorldRenderMode.TextureCoord;
                    Renderer.shaders.RenderTextureCoordIndex = 1;
                    break;
                case "Texture coord 2":
                    Renderer.shaders.RenderMode = WorldRenderMode.TextureCoord;
                    Renderer.shaders.RenderTextureCoordIndex = 2;
                    break;
                case "Texture coord 3":
                    Renderer.shaders.RenderMode = WorldRenderMode.TextureCoord;
                    Renderer.shaders.RenderTextureCoordIndex = 3;
                    break;
            }
        }

        private void TextureSamplerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TextureSamplerComboBox.SelectedItem is ShaderParamNames)
            {
                Renderer.shaders.RenderTextureSampler = (ShaderParamNames)TextureSamplerComboBox.SelectedItem;
            }
        }

        private void TextureCoordsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (TextureCoordsComboBox.Text)
            {
                default:
                case "Texture coord 1":
                    Renderer.shaders.RenderTextureSamplerCoord = 1;
                    break;
                case "Texture coord 2":
                    Renderer.shaders.RenderTextureSamplerCoord = 2;
                    break;
                case "Texture coord 3":
                    Renderer.shaders.RenderTextureSamplerCoord = 3;
                    break;
            }
        }

        private void GridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            enableGrid = GridCheckBox.Checked;
        }

        private void GridSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            float newgs;
            float.TryParse(GridSizeComboBox.Text, out newgs);
            if (newgs != gridSize)
            {
                gridSize = newgs;
                UpdateGridVerts();
            }
        }

        private void GridCountComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int newgc;
            int.TryParse(GridCountComboBox.Text, out newgc);
            if (newgc != gridCount)
            {
                gridCount = newgc;
                UpdateGridVerts();
            }
        }

        private void SkeletonsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderskeletons = SkeletonsCheckBox.Checked;
        }

        private void StatusBarCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            StatusStrip.Visible = StatusBarCheckBox.Checked;
        }

        private void ErrorConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConsolePanel.Visible = ErrorConsoleCheckBox.Checked;
        }

        private void TextureViewerButton_Click(object sender, EventArgs e)
        {
            //TextureDictionary td = null;

            //if ((Ydr != null) && (Ydr.Loaded))
            //{
            //    td = Ydr.Drawable?.ShaderGroup?.TextureDictionary;
            //}
            //else if ((Yft != null) && (Yft.Loaded))
            //{
            //    td = Yft.Fragment?.Drawable?.ShaderGroup?.TextureDictionary;
            //}

            //if (td != null)
            //{
            //    YtdForm f = new YtdForm();
            //    f.Show();
            //    f.LoadTexDict(td, fileName);
            //    //f.LoadYtd(ytd);
            //}
            //else
            //{
            //    MessageBox.Show("Couldn't find embedded texture dict.");
            //}
        }





        private void PedNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!GameFileCache.IsInited) return;

            LoadPed();
        }

        private void CompHeadComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(0, CompHeadComboBox.SelectedItem);
        }

        private void CompBerdComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(1, CompBerdComboBox.SelectedItem);
        }

        private void CompHairComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(2, CompHairComboBox.SelectedItem);
        }

        private void CompUpprComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(3, CompUpprComboBox.SelectedItem);
        }

        private void CompLowrComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(4, CompLowrComboBox.SelectedItem);
        }

        private void CompHandComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(5, CompHandComboBox.SelectedItem);
        }

        private void CompFeetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(6, CompFeetComboBox.SelectedItem);
        }

        private void CompTeefComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(7, CompTeefComboBox.SelectedItem);
        }

        private void CompAccsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(8, CompAccsComboBox.SelectedItem);
        }

        private void CompTaskComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(9, CompTaskComboBox.SelectedItem);
        }

        private void CompDeclComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(10, CompDeclComboBox.SelectedItem);
        }

        private void CompJbibComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetComponentDrawable(11, CompJbibComboBox.SelectedItem);
        }

        private void ClipDictComboBox_TextChanged(object sender, EventArgs e)
        {
            LoadClipDict(ClipDictComboBox.Text);
        }

        private void ClipComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectClip(ClipComboBox.Text);
        }

        private void ClipComboBox_TextChanged(object sender, EventArgs e)
        {
            SelectClip(ClipComboBox.Text);
        }

        private void EnableRootMotionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SelectedPed.EnableRootMotion = EnableRootMotionCheckBox.Checked;
        }
    }
}
