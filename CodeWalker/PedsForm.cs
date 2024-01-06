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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Color = SharpDX.Color;

namespace CodeWalker
{
    public partial class PedsForm : Form, DXForm
    {
        public Form Form { get { return this; } } //for DXForm/DXManager use

        public Renderer Renderer { get; set; }

        public bool Pauserendering { get; set; }
        volatile bool formopen = false;
        volatile bool running = false;

        //volatile bool initialised = false;

        Stopwatch frametimer = new Stopwatch();
        Camera camera;
        Timecycle timecycle;
        Weather weather;
        Clouds clouds;

        Entity camEntity = new Entity();

        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();


        bool MouseLButtonDown = false;
        bool MouseRButtonDown = false;
        int MouseX;
        int MouseY;
        System.Drawing.Point MouseDownPoint;
        System.Drawing.Point MouseLastPoint;


        public GameFileCache GameFileCache => GameFileCacheFactory.Instance;


        InputManager Input = new InputManager();


        bool initedOk = false;



        bool toolsPanelResizing = false;
        int toolsPanelResizeStartX = 0;
        int toolsPanelResizeStartLeft = 0;
        int toolsPanelResizeStartRight = 0;


        bool enableGrid = false;
        float gridSize = 1.0f;
        int gridCount = 40;
        List<VertexTypePC> gridVerts = new List<VertexTypePC>();
        object gridSyncRoot = new object();





        Ped SelectedPed = new Ped();

        public class ComponentComboBoxesContainer
        {
            public int Index = 0;
            public required ComboBox DrawableBox;
            public required ComboBox TextureBox;

            [SetsRequiredMembers]
            public ComponentComboBoxesContainer(ComboBox drawableBox, ComboBox textureBox)
            {
                DrawableBox = drawableBox;
                TextureBox = textureBox;
            }
        }

        ComboBox[] ComponentComboBoxesOld = null;

        ComponentComboBoxesContainer[] ComponentComboBoxes = null;

        public class ComponentComboItem
        {
            public MCPVDrawblData DrawableData { get; set; }
            public int AlternativeIndex { get; set; }
            public MetaHash DlcName => DrawableData?.Owner.Owner.Data.dlcName ?? default;
            public int Index { get; set; }

            public string DrawableName => DrawableData?.GetDrawableName(AlternativeIndex) ?? "error";

            public ComponentComboItem(MCPVDrawblData drawableData, int altIndex = 0, int index = 0)
            {
                DrawableData = drawableData;
                AlternativeIndex = altIndex;
                Index = index;
            }

            public override string ToString()
            {
                if (DrawableData == null)
                    return $"[{Index}] {AlternativeIndex}";
                var itemname = DrawableData.GetDrawableName(AlternativeIndex);
                if (DrawableData.TexData?.Length > 0)
                    return $"[{Index}] {DlcName}^{itemname}";
                return $"[{Index}] {itemname}{DlcName}";
            }
        }

        public class ComponentTextureItem : ComponentComboItem
        {
            public int TextureIndex { get; set; }

            public string TextureName => DrawableData?.GetTextureName(TextureIndex);

            public ComponentTextureItem(MCPVDrawblData drawableData, int altIndex = 0, int textureIndex = -1) : base(drawableData, altIndex)
            {
                TextureIndex = textureIndex;
            }

            public override string ToString()
            {
                if (DrawableData == null)
                    return TextureIndex.ToString();

                if (DrawableData.TexData?.Length > 0)
                    return TextureName;

                return TextureIndex.ToString();
            }
        }


        public PedsForm()
        {
            InitializeComponent();

            ComponentComboBoxes = new[]
            {
                new ComponentComboBoxesContainer(CompHeadComboBox, CompHeadTexture),
                new ComponentComboBoxesContainer(CompBerdComboBox, CompBerdTexture),
                new ComponentComboBoxesContainer(CompHairComboBox, CompHairTexture),
                new ComponentComboBoxesContainer(CompUpprComboBox, CompUpprTexture),
                new ComponentComboBoxesContainer(CompLowrComboBox, CompLowrTexture),
                new ComponentComboBoxesContainer(CompHandComboBox, CompHandTexture),
                new ComponentComboBoxesContainer(CompFeetComboBox, CompFeetTexture),
                new ComponentComboBoxesContainer(CompTeefComboBox, CompTeefTexture),
                new ComponentComboBoxesContainer(CompAccsComboBox, CompAccsTexture),
                new ComponentComboBoxesContainer(CompTaskComboBox, CompTaskTexture),
                new ComponentComboBoxesContainer(CompDeclComboBox, CompDeclTexture),
                new ComponentComboBoxesContainer(CompJbibComboBox, CompJbibTexture),
            };

            ComponentComboBoxesOld = new[]
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

            Renderer.Shaders.deferred = false; //no point using this here yet


            LoadSettings();


            formopen = true;

            Task.Run(ContentThread);

            frametimer.Start();

        }
        public async ValueTask CleanupScene()
        {
            formopen = false;

            Renderer.DeviceDestroyed();

            int count = 0;
            while (running && (count < 5000)) //wait for the content thread to exit gracefully
            {
                await Task.Delay(1);
                count++;
            }
        }
        public async ValueTask RenderScene(DeviceContext context)
        {
            float elapsed = (float)frametimer.Elapsed.TotalSeconds;
            frametimer.Restart();

            if (elapsed < 0.016666)
            {
                await Task.Delay((int)(0.016666 * elapsed) * 1000);
            }

            if (Pauserendering) return;

            GameFileCache.BeginFrame();

            if (!await Renderer.RenderSyncRoot.WaitAsync(50))
            {
                return;
            } //couldn't get a lock, try again next time

            try
            {
                UpdateControlInputs(elapsed);
                //space.Update(elapsed);

                Renderer.Update(elapsed, MouseLastPoint.X, MouseLastPoint.Y);



                //UpdateWidgets();
                //BeginMouseHitTest();




                Renderer.BeginRender(context);

                Renderer.RenderSkyAndClouds();

                Renderer.SelectedDrawable = null;// SelectedItem.Drawable;


                Renderer.RenderPed(SelectedPed);

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
            }
            finally
            {
                Renderer.RenderSyncRoot.Release();
            }

            //UpdateMarkerSelectionPanelInvoke();
        }
        public void BuffersResized(int w, int h)
        {
            Renderer.BuffersResized(w, h);
        }
        public bool ConfirmQuit()
        {
            return true;
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


        private async ValueTask ContentThread()
        {
            try
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

                if (!GameFileCache.IsInited)
                {
                    GameFileCache.EnableDlc = true;
                    GameFileCache.EnableMods = true;
                    GameFileCache.LoadPeds = true;
                    GameFileCache.LoadVehicles = false;
                    GameFileCache.LoadArchetypes = false;//to speed things up a little
                    GameFileCache.BuildExtendedJenkIndex = false;//to speed things up a little
                    GameFileCache.DoFullStringIndex = true;//to get all global text from DLC...
                    await GameFileCache.InitAsync(UpdateStatus, LogError, force: false);
                }

                //UpdateDlcListComboBox(gameFileCache.DlcNameList);

                //EnableCacheDependentUI();

                UpdateGlobalPedsUI();


                LoadWorld();



                //initialised = true;

                //EnableDLCModsUI();

                //UpdateStatus("Ready");


                Task.Run(async () =>
                {
                    while (formopen && !IsDisposed) //renderer content loop
                    {
                        bool rcItemsPending = Renderer.ContentThreadProc();

                        if (!rcItemsPending)
                        {
                            await Task.Delay(ActiveForm == null ? 50 : 2).ConfigureAwait(false);
                        }
                    }
                });

                Task.Run(async () =>
                {
                    while (formopen && !IsDisposed) //main asset loop
                    {
                        bool fcItemsPending = GameFileCache.ContentThreadProc();

                        if (!fcItemsPending)
                        {
                            await Task.Delay(ActiveForm == null ? 50 : 2).ConfigureAwait(false);
                        }
                    }

                    running = false;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured in PedsForm::ContentThread.\n{ex}");
            }
            finally
            {
                running = false;
            }
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
                    BeginInvoke(UpdateStatus, text);
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
            }
        }
        private void LogError(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(LogError, text);
                }
                else
                {
                    //TODO: error logging..
                    ConsoleTextBox.AppendText(text + "\r\n");
                    Console.WriteLine(text);
                    //StatusLabel.Text = text;
                    //MessageBox.Show(text);
                }
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
            }
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

            rad = Math.Max(0.01f, rad * 0.1f);

            camera.FollowEntity.Position = pos;
            camera.TargetDistance = rad * 1.2f;
            camera.CurrentDistance = rad * 1.2f;

            camera.UpdateProj = true;

        }







        private void AddDrawableTreeNode(DrawableBase drawable, string name, bool check)
        {
            var tnode = TexturesTreeView.Nodes.Add(name);
            var dnode = ModelsTreeView.Nodes.Add(name);
            dnode.Tag = drawable;
            dnode.Checked = check;
            UpdateDrawFlags(drawable, check);

            AddDrawableModelsTreeNodes(drawable.DrawableModels?.High, "High Detail", true, dnode, tnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModels?.Med, "Medium Detail", false, dnode, tnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModels?.Low, "Low Detail", false, dnode, tnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModels?.VLow, "Very Low Detail", false, dnode, tnode);
            //AddDrawableModelsTreeNodes(drawable.DrawableModels?.Extra, "X Detail", false, dnode, tnode);

        }
        private void AddDrawableModelsTreeNodes(DrawableModel[] models, string prefix, bool check, TreeNode parentDrawableNode = null, TreeNode parentTextureNode = null)
        {
            if (models is null || models.Length == 0)
                return;

            for (int mi = 0; mi < models.Length; mi++)
            {
                var tnc = (parentDrawableNode != null) ? parentDrawableNode.Nodes : ModelsTreeView.Nodes;

                var model = models[mi];
                string mprefix = prefix + " " + (mi + 1).ToString();
                var mnode = tnc.Add(mprefix + " " + model.ToString());
                mnode.Tag = model;
                mnode.Checked = check;
                UpdateDrawFlags(model, check);

                var ttnc = (parentTextureNode != null) ? parentTextureNode.Nodes : TexturesTreeView.Nodes;
                var tmnode = ttnc.Add(mprefix + " " + model.ToString());
                tmnode.Tag = model;

                if (model.Geometries is null || model.Geometries.Length == 0)
                    continue;

                foreach (var geom in model.Geometries)
                {
                    var gname = geom.ToString();
                    var gnode = mnode.Nodes.Add(gname);
                    gnode.Tag = geom;
                    gnode.Checked = true;// check;
                    UpdateDrawFlags(geom, true);

                    var tgnode = tmnode.Nodes.Add(gname);
                    tgnode.Tag = geom;

                    if (geom.Shader?.ParametersList?.Hashes is not null)
                    {
                        var pl = geom.Shader.ParametersList;
                        var h = pl.Hashes;
                        var p = pl.Parameters;
                        for (int ip = 0; ip < h.Length; ip++)
                        {
                            var hash = pl.Hashes[ip];
                            var parm = pl.Parameters[ip];
                            if (parm.Data is TextureBase tex)
                            {
                                var tstr = tex.Name.Trim();
                                if (tex is Texture t)
                                {
                                    tstr = $"{tex.Name} ({t.Width}x{t.Height}, embedded)";
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

        private void UpdateDrawFlags(DrawableModel model, bool rem)
        {
            using (Renderer.RenderSyncRoot.WaitDisposable())
            {
                if (rem)
                {
                    Renderer.SelectionModelDrawFlags.Remove(model);
                }
                else
                {
                    Renderer.SelectionModelDrawFlags[model] = false;
                }
            }
        }

        private void UpdateDrawFlags(DrawableGeometry geom, bool rem)
        {
            using (Renderer.RenderSyncRoot.WaitDisposable())
            {
                if (rem)
                {
                    Renderer.SelectionGeometryDrawFlags.Remove(geom);
                }
                else
                {
                    Renderer.SelectionGeometryDrawFlags[geom] = false;
                }
            }
        }

        private void UpdateDrawFlags(DrawableBase drwbl, bool rem)
        {
            using (Renderer.RenderSyncRoot.WaitDisposable())
            {
                if (rem)
                {
                    Renderer.SelectionDrawableDrawFlags.Remove(drwbl);
                }
                else
                {
                    Renderer.SelectionDrawableDrawFlags[drwbl] = false;
                }
            }
        }

        private void UpdateSelectionDrawFlags(TreeNode node)
        {
            if (node.Tag is DrawableBase drwbl)
            {
                UpdateDrawFlags(drwbl, node.Checked);
            }
            else if (node.Tag is DrawableModel model)
            {
                UpdateDrawFlags(model, node.Checked);
            }
            else if (node.Tag is DrawableGeometry geom)
            {
                UpdateDrawFlags(geom, node.Checked);
            }
        }


        private async void UpdateGlobalPedsUI()
        {
            if (InvokeRequired)
            {
                BeginInvoke(UpdateGlobalPedsUI);
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
                    ycdlist.Add(ycde.ShortName.ToString());
                }
                ClipDictComboBox.AutoCompleteCustomSource.AddRange(ycdlist.ToArray());
                ClipDictComboBox.Text = "";



                PedNameComboBox.Items.Clear();
                if (GameFileCache.PedsInitDict is null)
                {
                    await GameFileCache.InitPeds(true);
                }
                var peds = GameFileCache.PedsInitDict.Values.ToList();
                peds.Sort((a, b) => { return a.Name.CompareTo(b.Name); });
                foreach (var ped in peds)
                {
                    PedNameComboBox.Items.Add(ped.Name);
                }
                if (peds.Count > 0)
                {
                    var ind = PedNameComboBox.FindString("MP_M_Freemode_01"); // //A_C_Pug
                    PedNameComboBox.SelectedIndex = Math.Max(ind, 0);
                    //PedNameComboBox.SelectedIndex = 0;
                }

            }

        }





        private void UpdateModelsUI()
        {
            //TODO: change to go through each component and add/update/remove treeview item accordingly?

            Renderer.SelectionDrawableDrawFlags.Clear();
            Renderer.SelectionModelDrawFlags.Clear();
            Renderer.SelectionGeometryDrawFlags.Clear();
            ModelsTreeView.Nodes.Clear();
            ModelsTreeView.ShowRootLines = true;
            TexturesTreeView.Nodes.Clear();
            TexturesTreeView.ShowRootLines = true;

            if (SelectedPed is null)
                return;


            for (int i = 0; i < 12; i++)
            {
                var drawable = SelectedPed.Drawables[i];
                var drawablename = SelectedPed.DrawableNames[i];

                if (drawable is not null)
                {
                    AddDrawableTreeNode(drawable, drawablename, true);

                    PopulateCompComboTextures(ComponentComboBoxes[i]);
                }
            }

        }




        public async ValueTask LoadPedAsync()
        {
            SuspendLayout();
            var pedname = PedNameComboBox.Text;
            var pedhash = JenkHash.GenHashLower(pedname);
            var pedchange = SelectedPed.NameHash != pedhash;

            for (int i = 0; i < 12; i++)
            {
                ClearCombo(ComponentComboBoxesOld[i]);
            }

            DetailsPropertyGrid.SelectedObject = null;

            Console.WriteLine($"{YmtNameComboBox.SelectedItem}: {YmtNameComboBox.SelectedItem?.GetType()}");
            MetaHash? selectedDlc = null;
            if (YmtNameComboBox.SelectedItem is MetaHash metaHash)
            {
                selectedDlc = metaHash;
            }
            PedFile? pedFile = YmtNameComboBox.SelectedItem as PedFile;

            await SelectedPed.InitAsync(pedname, GameFileCache, selectedDlc);

            YmtNameComboBox.Items.Clear();
            YmtNameComboBox.Items.AddRange(SelectedPed.Dlcs.Select(p => (object)p).ToArray());

            YmtNameComboBox.SelectedItem = SelectedPed.Ymt;

            LoadModel(SelectedPed.Yft, pedchange);

            if (SelectedPed.Ymts?.Count > 0)
            {
                foreach (var comboBox in ComponentComboBoxes)
                {
                    comboBox.DrawableBox.BeginUpdate();
                    comboBox.TextureBox.BeginUpdate();
                    comboBox.Index = 0;
                }
                try
                {
                    foreach (var ymt in SelectedPed.Ymts)
                    {
                        var vi = ymt.VariationInfo;
                        if (vi is not null)
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                PopulateCompCombo(ComponentComboBoxes[i], vi.GetComponentData(i));
                            }
                        }
                    }
                }
                finally
                {
                    foreach (var comboBox in ComponentComboBoxes)
                    {
                        if (comboBox.DrawableBox.Items.Count > 1)
                        {
                            comboBox.DrawableBox.SelectedIndex = 1;
                        }
                        if (comboBox.TextureBox.Items.Count > 1)
                        {
                            comboBox.TextureBox.SelectedIndex = 0;
                        }
                    }
                    foreach (var comboBox in ComponentComboBoxes)
                    {
                        comboBox.DrawableBox.EndUpdate();
                        comboBox.TextureBox.EndUpdate();
                    }
                }
            }

            //var vi = SelectedPed.Ymt?.VariationInfo;
            //if (vi != null)
            //{
            //    for (int i = 0; i < 12; i++)
            //    {
            //        PopulateCompCombo(ComponentComboBoxes[i], vi.GetComponentData(i));
            //    }
            //}

            ClipDictComboBox.Text = SelectedPed.InitData?.ClipDictionaryName ?? "";
            ClipComboBox.Text = "idle";

            UpdateModelsUI();
            ResumeLayout(true);

            DetailsPropertyGrid.SelectedObject = SelectedPed;
            DetailsPropertyGrid.Invalidate();
            DetailsPropertyGrid.Update();
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

        private void PopulateCompComboTextures(ComponentComboBoxesContainer c)
        {
            if (c.DrawableBox.SelectedItem is not ComponentComboItem selectedDrawable || selectedDrawable.DrawableData is null)
                return;

            if (c.TextureBox.SelectedItem is ComponentTextureItem selectedTexture)
            {
                if (selectedTexture.DrawableData == selectedDrawable.DrawableData)
                {
                    return;
                }
            }

            c.TextureBox.BeginUpdate();

            c.TextureBox.Items.Clear();

            var item = selectedDrawable.DrawableData;

            if (item.TexData?.Length > 0)
            {
                for (int tex = 0; tex < item.TexData.Length; tex++)
                {
                    c.TextureBox.Items.Add(new ComponentTextureItem(item, selectedDrawable.AlternativeIndex, tex));
                }
            }

            c.TextureBox.SelectedIndex = 0;

            c.TextureBox.EndUpdate();
        }

        private void PopulateCompCombo(ComponentComboBoxesContainer c, MCPVComponentData compData)
        {
            if (compData?.DrawblData3 == null) return;
            foreach (var item in compData.DrawblData3)
            {
                for (int alt = 0; alt <= item.NumAlternatives; alt++)
                {
                    c.DrawableBox.Items.Add(new ComponentComboItem(item, alt, c.Index));
                    //if (item.TexData?.Length > 0)
                    //{
                    //    for (int tex = 0; tex < item.TexData.Length; tex++)
                    //    {
                    //        c.TextureBox.Items.Add(new ComponentTextureItem(item, alt, tex));
                    //    }
                    //}
                }
                c.Index++;
            }
        }

        private async ValueTask SetComponentDrawableAsync(int index)
        {
            var comboBoxes = ComponentComboBoxes[index];
            var comboItem = comboBoxes.DrawableBox.SelectedItem as ComponentComboItem;
            var textureItem = comboBoxes.TextureBox.SelectedItem as ComponentTextureItem;
            var name = comboItem?.DrawableName;
            var tex = textureItem?.TextureName;
            var dlc = comboItem?.DlcName ?? 0;

            await SelectedPed.SetComponentDrawableAsync(index, name, tex, dlc, GameFileCache);

            UpdateModelsUI();
        }






        private void LoadClipDict(string name)
        {
            var ycdhash = JenkHash.GenHash(name.ToLowerInvariant());
            var ycd = GameFileCache.GetYcd(ycdhash);
            while ((ycd != null) && (!ycd.Loaded))
            {
                Thread.Sleep(1);//kinda hacky
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
                if (cme.Clip != null)
                {
                    items.Add(cme.Clip.ShortName);
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
            if (SelectedPed.Ycd?.ClipMap?.TryGetValue(cliphash, out var cme) ?? false)
            {
                SelectedPed.AnimClip = cme;
            }
            else
            {
                SelectedPed.AnimClip = null;
            }
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


            Input.Update();

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

            if (Input.xbenable)
            {
                movevec.X += Input.xblx;
                movevec.Z -= Input.xbly;
                moveSpeed *= (1.0f + (Math.Min(Math.Max(Input.xblt, 0.0f), 1.0f) * 15.0f)); //boost with left trigger
                if (Input.ControllerButtonPressed(GamepadButtonFlags.A | GamepadButtonFlags.RightShoulder | GamepadButtonFlags.LeftShoulder))
                {
                    moveSpeed *= 5.0f;
                }
            }


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
                camera.ControllerRotate(Input.xbrx, Input.xbry, elapsed);

                float zoom = 0.0f;
                float zoomspd = s.XInputZoomSpeed;
                float zoomamt = zoomspd * elapsed;
                if (Input.ControllerButtonPressed(GamepadButtonFlags.DPadUp)) zoom += zoomamt;
                if (Input.ControllerButtonPressed(GamepadButtonFlags.DPadDown)) zoom -= zoomamt;

                camera.ControllerZoom(zoom);

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
            var kb = Input.KeyBindings;
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
            if (e.Node is not null)
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
            if (Renderer.Shaders is null)
                return;

            Renderer.RenderSyncRoot.Wait();
            try
            {
                Renderer.Shaders.hdr = HDRRenderingCheckBox.Checked;
            }
            finally
            {
                Renderer.RenderSyncRoot.Release();
            }
        }

        private void ShadowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Renderer.Shaders is null)
                return;

            Renderer.RenderSyncRoot.Wait();
            try
            {
                Renderer.Shaders.shadows = ShadowsCheckBox.Checked;
            }
            finally
            {
                Renderer.RenderSyncRoot.Release();
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
            using (Renderer.RenderSyncRoot.WaitDisposable())
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
            Renderer.Shaders.wireframe = WireframeCheckBox.Checked;
        }

        private void AnisotropicFilteringCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.Shaders.AnisotropicFiltering = AnisotropicFilteringCheckBox.Checked;
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
                    Renderer.Shaders.RenderMode = WorldRenderMode.Default;
                    break;
                case "Single texture":
                    Renderer.Shaders.RenderMode = WorldRenderMode.SingleTexture;
                    TextureSamplerComboBox.Enabled = true;
                    TextureCoordsComboBox.Enabled = true;
                    break;
                case "Vertex normals":
                    Renderer.Shaders.RenderMode = WorldRenderMode.VertexNormals;
                    break;
                case "Vertex tangents":
                    Renderer.Shaders.RenderMode = WorldRenderMode.VertexTangents;
                    break;
                case "Vertex colour 1":
                    Renderer.Shaders.RenderMode = WorldRenderMode.VertexColour;
                    Renderer.Shaders.RenderVertexColourIndex = 1;
                    break;
                case "Vertex colour 2":
                    Renderer.Shaders.RenderMode = WorldRenderMode.VertexColour;
                    Renderer.Shaders.RenderVertexColourIndex = 2;
                    break;
                case "Vertex colour 3":
                    Renderer.Shaders.RenderMode = WorldRenderMode.VertexColour;
                    Renderer.Shaders.RenderVertexColourIndex = 3;
                    break;
                case "Texture coord 1":
                    Renderer.Shaders.RenderMode = WorldRenderMode.TextureCoord;
                    Renderer.Shaders.RenderTextureCoordIndex = 1;
                    break;
                case "Texture coord 2":
                    Renderer.Shaders.RenderMode = WorldRenderMode.TextureCoord;
                    Renderer.Shaders.RenderTextureCoordIndex = 2;
                    break;
                case "Texture coord 3":
                    Renderer.Shaders.RenderMode = WorldRenderMode.TextureCoord;
                    Renderer.Shaders.RenderTextureCoordIndex = 3;
                    break;
            }
        }

        private void TextureSamplerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TextureSamplerComboBox.SelectedItem is ShaderParamNames)
            {
                Renderer.Shaders.RenderTextureSampler = (ShaderParamNames)TextureSamplerComboBox.SelectedItem;
            }
        }

        private void TextureCoordsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (TextureCoordsComboBox.Text)
            {
                default:
                case "Texture coord 1":
                    Renderer.Shaders.RenderTextureSamplerCoord = 1;
                    break;
                case "Texture coord 2":
                    Renderer.Shaders.RenderTextureSamplerCoord = 2;
                    break;
                case "Texture coord 3":
                    Renderer.Shaders.RenderTextureSamplerCoord = 3;
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





        private async void PedNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!GameFileCache.IsInited)
            {
                return;
            }

            await LoadPedAsync();
        }

        private async void YmtNameCombaBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!GameFileCache.IsInited)
            {
                return;
            }

            await LoadPedAsync();
        }

        private async void CompHeadComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(0);
        }

        private async void CompBerdComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(1);
        }

        private async void CompHairComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(2);
        }

        private async void CompUpprComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(3);
        }

        private async void CompLowrComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(4);
        }

        private async void CompHandComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(5);
        }

        private async void CompFeetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(6);
        }

        private async void CompTeefComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(7);
        }

        private async void CompAccsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(8);
        }

        private async void CompTaskComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(9);
        }

        private async void CompDeclComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(10);
        }

        private async void CompJbibComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(11);
        }

        private void ClipDictComboBox_TextChanged(object sender, EventArgs e)
        {
            LoadClipDict(ClipDictComboBox.Text);
        }

        private void ClipComboBox_TextChanged(object sender, EventArgs e)
        {
            SelectClip(ClipComboBox.Text);
        }

        private void EnableRootMotionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SelectedPed.EnableRootMotion = EnableRootMotionCheckBox.Checked;
        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private async void CompHeadTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(0);
        }

        private async void CompBerdTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(1);
        }

        private async void CompHairTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(2);
        }

        private async void CompUpprTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(3);
        }

        private async void CompLowrTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(4);
        }

        private async void CompHandTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(5);
        }

        private async void CompFeetTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(6);
        }

        private async void CompTeefTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(7);
        }

        private async void CompAccsTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(8);
        }

        private async void CompTaskTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(9);
        }

        private async void CompDeclTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(10);
        }

        private async void CompJbibTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SetComponentDrawableAsync(11);
        }
    }
}
