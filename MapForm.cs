using SlimDX;
using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker
{
    public partial class MapForm : Form
    {

        Vector3 TargetViewCenter = new Vector3(0.0f);
        Vector3 CurrentViewCenter = new Vector3(0.0f);
        float TargetZoom = 1.0f;
        float CurrentZoom = 1.0f;


        volatile bool LoadingImages = false;
        List<MapImage> Images;
        MapImage CurrentMap = null;

        List<MapIcon> Icons;
        MapIcon MarkerIcon = null;
        MapIcon LocatorIcon = null;
        MapMarker LocatorMarker = null;
        MapMarker GrabbedMarker = null;
        MapMarker SelectedMarker = null;
        MapMarker MousedMarker = null;
        List<MapMarker> Markers = new List<MapMarker>();
        List<MapMarker> RenderMarkers = new List<MapMarker>();

        bool Resizing = false;
        bool MouseButtonDown = false;
        Point MouseDownPoint;
        Point MouseLastPoint;

        volatile bool NeedsUpdate = false;

        Direct3D d3d = null;
        Device device = null;
        Sprite sprite = null;

        Stopwatch FrameTimer = new Stopwatch();

        Size OldSize;

        public MapForm()
        {
            InitializeComponent();


            Icons = new List<MapIcon>();
            AddIcon("Google Marker", "icon_google_marker_64x64.png", 64, 64, 11.0f, 40.0f, 1.0f);
            AddIcon("Glokon Marker", "icon_glokon_normal_32x32.png", 32, 32, 11.0f, 32.0f, 1.0f);
            AddIcon("Glokon Debug", "icon_glokon_debug_32x32.png", 32, 32, 11.5f, 32.0f, 1.0f);
            MarkerIcon = Icons[1];
            LocatorIcon = Icons[2];
            foreach (MapIcon icon in Icons)
            {
                MarkerStyleComboBox.Items.Add(icon);
                LocatorStyleComboBox.Items.Add(icon);
            }
            MarkerStyleComboBox.SelectedItem = MarkerIcon;
            LocatorStyleComboBox.SelectedItem = LocatorIcon;
            LocatorMarker = new MapMarker();
            LocatorMarker.Icon = LocatorIcon;
            LocatorMarker.IsMovable = true;


            ////for google map calibration...
            //var game_1_x = 1972.606; //var map_1_lng = -60.8258056640625;
            //var game_1_y = 3817.044; //var map_1_lat = 72.06379257078102;
            //var game_2_x = -1154.11; //var map_2_lng = -72.1417236328125;
            //var game_2_y = -2715.203; //var map_2_lat = 48.41572128171852;
           
            ////reference point:
            //501.4398, 5603.9600, 795.9738, 0x4CC3BAFC, cs1_10_redeye

            //origin in 8192x8192 textures is at approx:
            //3755.2, 5525.5

            float ox = 3755.2f;
            float oy = 5525.5f;
            float uptx = 1.517952f; //this seems pretty close...
            float upty = -1.517952f;

            Images = new List<MapImage>();
            AddImage("Satellite", "gtav_satellite_8192x8192.jpg", 8192, 8192, ox, oy, uptx, upty);
            AddImage("Roads", "gtav_roadmap_8192x8192.jpg", 8192, 8192, ox, oy, uptx, upty);
            AddImage("Atlas", "gtav_atlas_8192x8192.jpg", 8192, 8192, ox, oy, uptx, upty);
            foreach (MapImage mi in Images)
            {
                MapComboBox.Items.Add(mi);
            }

            InitializeDevice();

            if (Images.Count > 0)
            {
                MapImage mi = Images[0];
                MapComboBox.SelectedItem = mi;
                TargetViewCenter = mi.Origin;
                CurrentViewCenter = mi.Origin;
            }

            this.MouseWheel += MapForm_MouseMove;

            FrameTimer.Restart();
            SlimDX.Windows.MessagePump.Run(this, new SlimDX.Windows.MainLoop(SlimDX_Render));
        }

        private void MapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeDevice();

            if (d3d != null)
            {
                d3d.Dispose();
                d3d = null;
            }
        }



        public void InitializeDevice()
        {
            if (WindowState == FormWindowState.Minimized) return;

            PresentParameters presentParams = new PresentParameters
            {
                BackBufferWidth = ClientSize.Width,
                BackBufferHeight = ClientSize.Height,
                DeviceWindowHandle = Handle,
                PresentFlags = PresentFlags.None,
                Multisample = MultisampleType.None,
                BackBufferCount = 0,
                PresentationInterval = PresentInterval.One, // VSYNC ON
                SwapEffect = SwapEffect.Discard,
                BackBufferFormat = Format.X8R8G8B8,
                Windowed = true,
                EnableAutoDepthStencil = false,
            };

            if (d3d == null)
            {
                d3d = new Direct3D();
            }

            device = new Device(d3d, 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing, presentParams);
            device.Viewport = new Viewport(0, 0, ClientSize.Width, ClientSize.Height);

            sprite = new Sprite(device);
            
            if (Icons != null)
            {
                foreach (MapIcon icon in Icons)
                {
                    LoadIconTexture(icon);
                }
            }

            if ((CurrentMap != null) && (CurrentMap.Tex == null))
            {
                LoadImageTexture(CurrentMap);
            }
        }

        public void DisposeDevice()
        {
            if (device == null) return;

            if (Icons != null)
            {
                foreach (MapIcon icon in Icons)
                {
                    if (icon.Tex != null)
                    {
                        icon.Tex.Dispose();
                        icon.Tex = null;
                    }
                }
            }

            foreach (MapImage img in Images)
            {
                if (img.Tex != null)
                {
                    img.Tex.Dispose();
                    img.Tex = null;
                }
            }

            if ((CurrentMap != null) && (CurrentMap.Tex != null))
            {
                CurrentMap.Tex.Dispose();
                CurrentMap.Tex = null;
            }

            sprite.Dispose();
            sprite = null;

            device.Dispose();
            device = null;
        }

        private void HandleWindowResize()
        {
            DisposeDevice();
            InitializeDevice(); //this really shouldn't be necessary!


            //sprite.Dispose();
            //sprite = null;

            //device.Reset();

            //sprite = new Sprite(device);
        }


        private void MapForm_ResizeBegin(object sender, EventArgs e)
        {
            OldSize = Size;
            Resizing = true;
        }
        private void MapForm_ResizeEnd(object sender, EventArgs e)
        {
            Resizing = false;
            if ((OldSize != Size) && (!LoadingImages))
            {
                HandleWindowResize();
            }
        }
        private void MapForm_SizeChanged(object sender, EventArgs e)
        {

        }
        private void MapForm_ClientSizeChanged(object sender, EventArgs e)
        {
            if ((!Resizing) && (!LoadingImages))
            {
                HandleWindowResize();
            }
        }


        private void MapForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MouseButtonDown = true;
            }
            MouseDownPoint = e.Location;
            MouseLastPoint = MouseDownPoint;

            if (MouseButtonDown)
            {
                if (MousedMarker != null)
                {
                    if (MousedMarker.IsMovable)
                    {
                        GrabbedMarker = MousedMarker;
                    }
                    else
                    {
                        SelectedMarker = MousedMarker;
                        SelectionNameTextBox.Text = SelectedMarker.Name;
                        SelectionPositionTextBox.Text = SelectedMarker.Get3DWorldPosString();
                        UpdateSelectionPanel();
                        SelectionPanel.Visible = true;
                    }
                }
                else
                {
                    GrabbedMarker = null;
                    //SelectedMarker = null;
                    //SelectionPanel.Visible = false;
                }
            }
        }
        private void MapForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MouseButtonDown = false;

                GrabbedMarker = null;

                if ((e.Location == MouseDownPoint) && (MousedMarker == null))
                {
                    //was clicked. but not on a marker... deselect and hide the panel

                    SelectedMarker = null;
                    SelectionPanel.Visible = false;
                }
            }
        }
        private void MapForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                float zoomfac = 1.0f + 0.1f*((float)ZoomSpeedUpDown.Value);
                float oldzoom = TargetZoom;
                float newzoom = oldzoom * (e.Delta > 0 ? zoomfac : 1.0f / zoomfac);

                if (newzoom < 1e-2f) newzoom = 1e-2f;
                if (newzoom > 1e4f) newzoom = 1e4f;

                //figure out the new target center based on current mouse pos
                float w = ClientSize.Width;
                float h = ClientSize.Height;
                float mx = MouseLastPoint.X - w * 0.5f;
                float my = MouseLastPoint.Y - h * 0.5f;

                Vector3 mpold = new Vector3(mx, my, 0.0f) / oldzoom;
                Vector3 mpnew = new Vector3(mx, my, 0.0f) / newzoom;
                Vector3 mpdelt = mpnew - mpold;

                TargetViewCenter -= mpdelt;
                TargetZoom = newzoom;


                NeedsUpdate = true;
            }
            if (MouseButtonDown)
            {
                float dx = (e.X - MouseLastPoint.X) / CurrentZoom;
                float dy = (e.Y - MouseLastPoint.Y) / CurrentZoom;

                if (GrabbedMarker == null)
                {
                    //pan the view.
                    TargetViewCenter.X -= dx;
                    TargetViewCenter.Y -= dy;
                    NeedsUpdate = true;
                }
                else
                {
                    //move the grabbed marker...
                    float uptx = (CurrentMap != null) ? CurrentMap.UnitsPerTexelX : 1.0f;
                    float upty = (CurrentMap != null) ? CurrentMap.UnitsPerTexelY : 1.0f;
                    Vector3 wpos = GrabbedMarker.WorldPos;
                    wpos.X += dx*uptx;
                    wpos.Y += dy*upty;
                    GrabbedMarker.WorldPos = wpos;
                    UpdateMarkerTexturePos(GrabbedMarker);

                    if (GrabbedMarker == LocatorMarker)
                    {
                        LocateTextBox.Text = LocatorMarker.ToString();
                        WorldCoordTextBox.Text = LocatorMarker.Get2DWorldPosString();
                        TextureCoordTextBox.Text = LocatorMarker.Get2DTexturePosString();
                    }

                }
            }


            if (NeedsUpdate)
            {
                //limit to the texture.
                float tvcmaxx = 8192.0f;
                float tvcminx = 0.0f;
                float tvcmaxy = 8192.0f;
                float tvcminy = 0.0f;
                if (!((TargetViewCenter.X > tvcmaxx) && (TargetViewCenter.X < tvcminx)))
                {
                    if (TargetViewCenter.X > tvcmaxx) TargetViewCenter.X = tvcmaxx;
                    if (TargetViewCenter.X < tvcminx) TargetViewCenter.X = tvcminx;
                }
                if (!((TargetViewCenter.Y > tvcmaxy) && (TargetViewCenter.Y < tvcminy)))
                {
                    if (TargetViewCenter.Y > tvcmaxy) TargetViewCenter.Y = tvcmaxy;
                    if (TargetViewCenter.Y < tvcminy) TargetViewCenter.Y = tvcminy;
                }
            }


            MouseLastPoint = e.Location;

            MousedMarker = FindMousedMarker();

            if (MousedMarker != null)
            {
                if (MousedMarker.IsMovable)
                {
                    Cursor = Cursors.SizeAll;
                }
                else
                {
                    Cursor = Cursors.Hand;
                }
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }





        private void SlimDX_Render()
        {
            if (device == null)
            {
                return;
            }
            if (LoadingImages)
            {
                return;
            }


            device.Clear(ClearFlags.Target, new Color4(BackColor), 1.0f, 0);
            device.BeginScene();
            device.SetRenderState(RenderState.AlphaBlendEnable, true);
            device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            if (NeedsUpdate)
            {
            }

            if ((CurrentMap != null) && (CurrentMap.Tex != null))
            {
                float smooth = 1.0f - (((float)SmoothingUpDown.Value)*0.9f);
                float elapsed = (float)Math.Min(FrameTimer.Elapsed.TotalSeconds, 0.1);
                CurrentZoom = CurrentZoom + (TargetZoom - CurrentZoom) * smooth;
                CurrentViewCenter = CurrentViewCenter + (TargetViewCenter - CurrentViewCenter) * smooth;

                float w = ClientSize.Width;
                float h = ClientSize.Height;


                Matrix scale = Matrix.Scaling(CurrentZoom, CurrentZoom, 0.0f);
                Matrix trans = Matrix.Translation(-CurrentViewCenter);
                Matrix offset = Matrix.Translation(w * 0.5f, h * 0.5f, 0.0f);
                Matrix matrix = Matrix.Multiply(Matrix.Multiply(trans, scale), offset);

                sprite.Begin(SpriteFlags.None);
                sprite.Transform = matrix;
                sprite.Draw(CurrentMap.Tex, Color.White);
                sprite.End();


                sprite.Begin(SpriteFlags.None);
                sprite.Transform = Matrix.Identity;



                //sort by Y to make markers look correct
                RenderMarkers.Clear();
                RenderMarkers.AddRange(Markers);
                RenderMarkers.Sort((m1, m2) => m1.TexturePos.Y.CompareTo(m2.TexturePos.Y));



                //draw all the markers
                foreach (MapMarker m in RenderMarkers)
                {
                    if ((m.Icon != null) && (m.Icon.Tex != null))
                    {
                        UpdateMarkerScreenPos(m);

                        sprite.Transform = GetMarkerRenderMatrix(m);
                        sprite.Draw(m.Icon.Tex, Color.White);
                    }
                }

                //draw the locator marker
                MapIcon locic = LocatorMarker.Icon;
                if ((ShowLocatorCheckBox.Checked) && (locic != null) && (locic.Tex != null))
                {
                    UpdateMarkerScreenPos(LocatorMarker);

                    sprite.Transform = GetMarkerRenderMatrix(LocatorMarker);
                    sprite.Draw(locic.Tex, Color.White);
                }


                sprite.End();

            }

            device.EndScene();
            device.Present();



            if (SelectedMarker != null)
            {
                UpdateSelectionPanel();
            }


            FrameTimer.Restart();
        }

        private void UpdateSelectionPanel()
        {


            int ox = -90;
            int oy = -76;

            int px = (int)Math.Round(SelectedMarker.ScreenPos.X, MidpointRounding.AwayFromZero) + ox;
            int py = (int)Math.Round(SelectedMarker.ScreenPos.Y, MidpointRounding.AwayFromZero) + oy;

            int sx = SelectionPanel.Width;
            int sy = SelectionPanel.Height;

            SelectionPanel.SetBounds(px, py, sx, sy);

        }



        private void MapComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapImage mi = MapComboBox.SelectedItem as MapImage;
            if ((mi != null) && (mi != CurrentMap) && (device != null))
            {
                SetIsLoading(true);
                if ((CurrentMap != null) && (CurrentMap.Tex != null))
                {
                    CurrentMap.Tex.Dispose();
                    CurrentMap.Tex = null;
                }

                if (mi.Tex == null)
                {
                    LoadImageTexture(mi);
                }
                else
                {
                    SetIsLoading(false);
                }

                CurrentMap = mi;
                NeedsUpdate = true;

                if (CurrentMap != null)
                {
                    TextureNameLabel.Text = CurrentMap.Name;
                    TextureFileLabel.Text = CurrentMap.Filepath;
                    TextureOriginTextBox.Text = string.Format("{0}, {1}", CurrentMap.Origin.X, CurrentMap.Origin.Y);
                    UnitsPerTexelXTextBox.Text = CurrentMap.UnitsPerTexelX.ToString();
                    UnitsPerTexelYTextBox.Text = CurrentMap.UnitsPerTexelY.ToString();
                    if (LocatorMarker != null)
                    {
                        UpdateMarkerTexturePos(LocatorMarker);
                        WorldCoordTextBox.Text = LocatorMarker.Get2DWorldPosString();
                        TextureCoordTextBox.Text = LocatorMarker.Get2DTexturePosString();
                    }
                }
                else
                {
                    TextureNameLabel.Text = "(No texture)";
                    TextureFileLabel.Text = "(No texture)";
                }
            }
        }


        private void MainPanelShowButton_Click(object sender, EventArgs e)
        {
            MainPanel.Visible = true;
            MainPanelShowButton.Visible = false;
        }
        private void MainPanelHideButton_Click(object sender, EventArgs e)
        {
            MainPanel.Visible = false;
            MainPanelShowButton.Visible = true;
        }

        private void MarkerStyleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapIcon icon = MarkerStyleComboBox.SelectedItem as MapIcon;
            if (icon != MarkerIcon)
            {
                MarkerIcon = icon;

                foreach (MapMarker m in Markers)
                {
                    m.Icon = icon;
                }
            }
        }
        private void LocatorStyleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapIcon icon = LocatorStyleComboBox.SelectedItem as MapIcon;
            if (icon != LocatorIcon)
            {
                LocatorIcon = icon;
                LocatorMarker.Icon = icon;
            }
        }



        private void LocateTextBox_TextChanged(object sender, EventArgs e)
        {
            if (GrabbedMarker == LocatorMarker) return; //don't try to update the marker if it's being dragged
            if (LocatorMarker == null) return; //this shouldn't happen, but anyway

            LocatorMarker.Parse(LocateTextBox.Text);

            UpdateMarkerTexturePos(LocatorMarker);
        }
        private void GoToButton_Click(object sender, EventArgs e)
        {
            GoToMarker(LocatorMarker);
        }


        private void AddMarkersButton_Click(object sender, EventArgs e)
        {
            string[] lines = MultiFindTextBox.Text.Split('\n');
            foreach (string line in lines)
            {
                AddMarker(line);
            }
        }

        private void ClearMarkersButton_Click(object sender, EventArgs e)
        {
            Markers.Clear();
            MarkersListView.Items.Clear();
        }
        private void ResetMarkersButton_Click(object sender, EventArgs e)
        {
            Markers.Clear();
            MarkersListView.Items.Clear();
            AddDefaultMarkers();
        }



        private void SetOriginButton_Click(object sender, EventArgs e)
        {
            if (CurrentMap == null) return;

            TextureOriginTextBox.Text = TextureCoordTextBox.Text;

            string[] comps = TextureOriginTextBox.Text.Split(',');
            if (comps.Length > 1)
            {
                Vector3 origin = new Vector3(0.0f);
                float.TryParse(comps[0].Trim(), out origin.X);
                float.TryParse(comps[1].Trim(), out origin.Y);
                CurrentMap.Origin = origin;

                RecalculateAllMarkers();

                LocatorMarker.WorldPos = new Vector3(0.0f);//force to the world origin
                LocatorMarker.TexturePos = origin;
                GrabbedMarker = LocatorMarker; //don't force the marker update when updating textbox..
                LocateTextBox.Text = "0, 0, 0";
                GrabbedMarker = null;
                WorldCoordTextBox.Text = LocatorMarker.Get2DWorldPosString();
                TextureCoordTextBox.Text = LocatorMarker.Get2DTexturePosString();
            }

        }
        private void SetCoordButton_Click(object sender, EventArgs e)
        {
            if (CurrentMap == null) return;

            string[] comps = WorldCoordTextBox.Text.Split(',');
            if (comps.Length > 1)
            {
                Vector3 coord = new Vector3(0.0f);
                float.TryParse(comps[0].Trim(), out coord.X);
                float.TryParse(comps[1].Trim(), out coord.Y);

                //assume the entered coord is the world coord for the locator's tex coord.
                //find the appropriate scaling factor to make it so.

                float tdx = LocatorMarker.TexturePos.X - CurrentMap.Origin.X; //texel dist from origin
                float tdy = LocatorMarker.TexturePos.Y - CurrentMap.Origin.Y;
                float tdl = (float)Math.Sqrt(tdx * tdx + tdy * tdy);

                float wcl = (float)Math.Sqrt(coord.X * coord.X + coord.Y * coord.Y);

                float upt = wcl / tdl;
                float uptx = upt * Math.Sign(tdx * coord.X);
                float upty = upt * Math.Sign(tdy * coord.Y);

                CurrentMap.UnitsPerTexelX = uptx;
                CurrentMap.UnitsPerTexelY = upty;

                RecalculateAllMarkers();

                LocatorMarker.WorldPos = coord;
                GrabbedMarker = LocatorMarker; //don't force the marker update when updating textbox..
                LocateTextBox.Text = LocatorMarker.ToString();
                GrabbedMarker = null;
                WorldCoordTextBox.Text = LocatorMarker.Get2DWorldPosString();
                TextureCoordTextBox.Text = LocatorMarker.Get2DTexturePosString();
                UnitsPerTexelXTextBox.Text = uptx.ToString();
                UnitsPerTexelYTextBox.Text = upty.ToString();
            }

        }

        private void CalibrateButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not yet implemented...");
        }

        private void CopyMarkersButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Still need to build this function.");
        }



        private void RecalculateAllMarkers()
        {
            //updates marker world coords based off current tex coord

            if (LocatorMarker != null)
            {
                LocatorMarker.WorldPos = GetWorldCoordFromTextureCoord(LocatorMarker.TexturePos);
            }

            foreach (MapMarker marker in Markers)
            {
                marker.WorldPos = GetWorldCoordFromTextureCoord(marker.TexturePos);
            }

        }


        private Vector3 GetTextureCoordFromWorldCoord(Vector3 world)
        {
            if (CurrentMap != null)
            {
                float tx = world.X / CurrentMap.UnitsPerTexelX;
                float ty = world.Y / CurrentMap.UnitsPerTexelY;
                return CurrentMap.Origin + new Vector3(tx, ty, 1.0f);
            }
            else
            {
                return new Vector3(0.0f);
            }
        }
        private Vector3 GetWorldCoordFromTextureCoord(Vector3 coord)
        {
            if (CurrentMap != null)
            {
                Vector3 tv = (coord - CurrentMap.Origin);
                tv.X *= CurrentMap.UnitsPerTexelX;
                tv.Y *= CurrentMap.UnitsPerTexelY;
                return tv;
            }
            else
            {
                return new Vector3(0.0f);
            }
        }

        private Vector3 GetTextureCoordFromScreenCoord(Vector3 screen)
        {
            return new Vector3(0.0f);
        }
        private Vector3 GetScreenCoordFromTextureCoord(Vector3 coord)
        {
            float w = ClientSize.Width;
            float h = ClientSize.Height;

            Vector3 centerdisttx = coord - CurrentViewCenter;
            Vector3 centerdistpx = centerdisttx * CurrentZoom;
            Vector3 screenpos = centerdistpx + new Vector3(w*0.5f, h*0.5f, 0.0f);
            screenpos.Z = 0.0f;

            return screenpos;
        }

        private Vector3 GetWorldCoordFromScreenCoord(Vector3 screen)
        {
            return GetWorldCoordFromTextureCoord(GetTextureCoordFromScreenCoord(screen));
        }
        private Vector3 GetScreenCoordFromWorldCoord(Vector3 world)
        {
            return GetScreenCoordFromTextureCoord(GetTextureCoordFromWorldCoord(world));
        }

        private void UpdateMarkerTexturePos(MapMarker marker)
        {
            marker.TexturePos = GetTextureCoordFromWorldCoord(marker.WorldPos);
        }
        private void UpdateMarkerScreenPos(MapMarker marker)
        {
            marker.ScreenPos = GetScreenCoordFromTextureCoord(marker.TexturePos);
        }
        private void UpdateMarkerTexAndScreenPos(MapMarker marker)
        {
            marker.TexturePos = GetTextureCoordFromWorldCoord(marker.WorldPos);
            marker.ScreenPos = GetScreenCoordFromTextureCoord(marker.TexturePos);
        }

        private Matrix GetMarkerRenderMatrix(MapMarker marker)
        {
            float sx = 1.0f, sy = 1.0f;
            MapIcon ic = marker.Icon;
            int icw = ic.TexWidth;
            int ich = ic.TexHeight;
            if (icw > ich) //shrink square vertically
            {
                sy = ((float)ich) / ((float)icw);
            }
            else //shrink square horizontally
            {
                sx = ((float)icw) / ((float)ich);
            }

            sx *= ic.Scale;
            sy *= ic.Scale;


            float px = marker.ScreenPos.X - ic.Center.X * ic.Scale;
            float py = marker.ScreenPos.Y - ic.Center.Y*ic.Scale;

            px = (float)Math.Round(px, MidpointRounding.AwayFromZero); //snap to pixels...
            py = (float)Math.Round(py, MidpointRounding.AwayFromZero);


            Matrix scale = Matrix.Scaling(sx, sy, 0.0f);
            Matrix trans = Matrix.Translation(px, py, 0.0f);

            return Matrix.Multiply(scale, trans);
        }

        private MapMarker FindMousedMarker()
        {

            float mx = MouseLastPoint.X;
            float my = MouseLastPoint.Y;

            if (ShowLocatorCheckBox.Checked)
            {
                if (IsMapMarkerUnderPoint(LocatorMarker, mx, my))
                {
                    return LocatorMarker;
                }
            }

            //search backwards through the render markers (front to back)
            for (int i = RenderMarkers.Count - 1; i >= 0; i--)
            {
                MapMarker m = RenderMarkers[i];
                if (IsMapMarkerUnderPoint(m, mx, my))
                {
                    return m;
                }
            }

            return null;
        }
        private bool IsMapMarkerUnderPoint(MapMarker marker, float x, float y)
        {
            float dx = x - marker.ScreenPos.X;
            float dy = y - marker.ScreenPos.Y;
            float mcx = marker.Icon.Center.X;
            float mcy = marker.Icon.Center.Y;
            bool bx = ((dx >= -mcx) && (dx <=  mcx));
            bool by = ((dy <= 0.0f) && (dy >=  -mcy));
            return (bx && by);
        }


        private void GoToMarker(MapMarker m)
        {
            //adjust the target to account for the main panel...
            Vector3 view = m.TexturePos;
            view.X += ((float)(MainPanel.Width + 4) * 0.5f) / CurrentZoom;

            TargetViewCenter = view;
        }


        private void AddMarker(string markerstr)
        {
            MapMarker m = new MapMarker();
            m.Parse(markerstr.Trim());
            m.Icon = MarkerIcon;

            UpdateMarkerTexturePos(m);

            Markers.Add(m);

            //////sort by Y
            ////Markers.Sort((m1, m2) => m1.TexturePos.Y.CompareTo(m2.TexturePos.Y));

            ListViewItem lvi = new ListViewItem(new string[] { m.Name, m.WorldPos.X.ToString(), m.WorldPos.Y.ToString(), m.WorldPos.Z.ToString() });
            lvi.Tag = m;
            MarkersListView.Items.Add(lvi);
        }
        private void AddDefaultMarkers()
        {

            AddMarker("1972.606, 3817.044, 0.0, Trevor Bed");
            AddMarker("94.5723, -1290.082, 0.0, Strip Club Bed");
            AddMarker("-1151.746, -1518.136, 0.0, Trevor City Bed"); 
            AddMarker("-2052.0, 3237.0, 0.0, Zancudo UFO"); 
            AddMarker("2490.0, 3777.0, 0.0, Hippy UFO"); 
            AddMarker("760.4618, 7392.8032, -126.0774, Sea UFO"); 
            AddMarker("501.4398, 5603.96, 0.0, RedEye"); 
            AddMarker("-1154.11, -2715.203, 0.0, Flight School"); 
            AddMarker("-1370.625, 56.1227, 0.0, Golf"); 
            AddMarker("-1109.213, 4914.744, 0.0, Altruist Cult"); 
            AddMarker("-1633.087, 4736.784, 0.0, Deal Gone Wrong"); 
            AddMarker("51.3909, 5957.7568, 209.614, cs1_10_clue_moon02"); 
            AddMarker("400.7087, 5714.5645, 605.0978, cs1_10_clue_rain01"); 
            AddMarker("703.442, 6329.8936, 76.4973, cs1_10_clue_rain02"); 
            AddMarker("228.7844, 5370.585, 577.2613, cs1_10_clue_moon01"); 
            AddMarker("366.4871, 5518.0742, 704.3185, cs1_10_clue_mountain01"); 


        }


        private MapIcon AddIcon(string name, string filename, int texw, int texh, float centerx, float centery, float scale)
        {
            string filepath = "maps\\" + filename;
            try
            {
                MapIcon mi = new MapIcon(name, filepath, texw, texh, centerx, centery, scale);
                Icons.Add(mi);
                return mi;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load map icon " + filepath + " for " + name + "!\n\n" + ex.ToString());
            }
            return null;
        }
        private MapImage AddImage(string name, string filename, int texw, int texh, float origx, float origy, float uppx, float uppy)
        {
            string filepath = "maps\\" + filename;
            try
            {
                MapImage mi = new MapImage(name, filepath, texw, texh, origx, origy, uppx, uppy);
                Images.Add(mi);
                return mi;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load map image " + filepath + " for " + name + " map!\n\n" + ex.ToString());
            }
            return null;
        }

        private void LoadImageTexture(MapImage mi)
        {
            SetIsLoading(true);

            Task.Run(() =>
            {
                try
                {
                    if (device != null)
                    {
                        mi.Tex = Texture.FromFile(device, mi.Filepath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not load map image " + mi.Filepath + " for " + mi.Name + " map!\n\n" + ex.ToString());
                }

                LoadCurrentImageComplete();
            });
        }
        private void LoadIconTexture(MapIcon mi)
        {
            try
            {
                if (device != null)
                {
                    mi.Tex = Texture.FromFile(device, mi.Filepath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load map icon " + mi.Filepath + " for " + mi.Name + "!\n\n" + ex.ToString());
            }
        }

        private void LoadCurrentImageComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { LoadCurrentImageComplete(); }));
                }
                else
                {
                    SetIsLoading(false);
                }
            }
            catch { }
        }
        private void LoadImagesComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { LoadImagesComplete(); }));
                }
                else
                {
                    if (Images.Count > 0)
                    {
                        foreach (MapImage mi in Images)
                        {
                            MapComboBox.Items.Add(mi);
                        }
                        CurrentMap = Images[0];
                        MapComboBox.SelectedItem = CurrentMap;
                    }
                    SetIsLoading(false);
                }
            }
            catch { }
        }
        private void SetIsLoading(bool loading)
        {
            LoadingImages = loading;
            LoadingLabel.Visible = loading;
            MapComboBox.Enabled = !loading;
        }

        private void GoToSelectedMarkerButton_Click(object sender, EventArgs e)
        {
            if (MarkersListView.SelectedItems.Count == 1)
            {
                MapMarker m = MarkersListView.SelectedItems[0].Tag as MapMarker;
                if (m != null)
                {
                    GoToMarker(m);
                }
            }
        }

        private void MarkersListView_DoubleClick(object sender, EventArgs e)
        {
            //go to the marker..
            if (MarkersListView.SelectedItems.Count == 1)
            {
                MapMarker m = MarkersListView.SelectedItems[0].Tag as MapMarker;
                if (m != null)
                {
                    GoToMarker(m);
                }
            }
        }
    }


    public class MapImage
    {
        public string Name { get; set; }
        public string Filepath { get; set; }
        public Texture Tex { get; set; }
        public Vector3 Origin { get; set; } //in image pixels
        public float UnitsPerTexelX { get; set; } //world units per image pixel
        public float UnitsPerTexelY { get; set; } //world units per image pixel
        public int TexWidth { get; set; }
        public int TexHeight { get; set; }

        public MapImage(string name, string filepath, int texw, int texh, float origx, float origy, float uppx, float uppy)
        {
            Name = name;
            Filepath = filepath;
            TexWidth = texw;
            TexHeight = texh;
            Origin = new Vector3(origx, origy, 0.0f);
            UnitsPerTexelX = uppx;
            UnitsPerTexelY = uppy;

            if (!File.Exists(filepath))
            {
                throw new Exception("File not found.");
            }
        }

        public override string ToString()
        {
            return Name;
        }

    }

    public class MapIcon
    {
        public string Name { get; set; }
        public string Filepath { get; set; }
        public Texture Tex { get; set; }
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

        public override string ToString()
        {
            return Name;
        }
    }

    public class MapMarker
    {
        public MapIcon Icon { get; set; }
        public Vector3 WorldPos { get; set; } //actual world pos
        public Vector3 TexturePos { get; set; } //position in the current texture (temp)
        public Vector3 ScreenPos { get; set; } //position on screen (updated per frame if needed)
        public string Name { get; set; }
        public List<string> Properties { get; set; } //additional data
        public bool IsMovable { get; set; }

        public void Parse(string s)
        {
            Vector3 p = new Vector3(0.0f);
            string[] ss = s.Split(',');
            if (ss.Length > 1)
            {
                float.TryParse(ss[0].Trim(), out p.X);
                float.TryParse(ss[1].Trim(), out p.Y);
            }
            if (ss.Length > 2)
            {
                float.TryParse(ss[2].Trim(), out p.Z);
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
            string cstr = string.Format("{0}, {1}, {2}", WorldPos.X, WorldPos.Y, WorldPos.Z);
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
            return string.Format("{0}, {1}", WorldPos.X, WorldPos.Y);
        }
        public string Get3DWorldPosString()
        {
            return string.Format("{0}, {1}, {2}", WorldPos.X, WorldPos.Y, WorldPos.Z);
        }

        public string Get2DTexturePosString()
        {
            return string.Format("{0}, {1}", TexturePos.X, TexturePos.Y);
        }

    }

}
