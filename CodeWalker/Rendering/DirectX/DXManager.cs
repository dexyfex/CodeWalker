using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Color = SharpDX.Color;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using DriverType = SharpDX.Direct3D.DriverType;
using System.Threading;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D;

namespace CodeWalker.Rendering
{
    public class DXManager
    {
        private DXForm dxform;

        public Device device { get; private set; }
        public DeviceContext context { get; private set; }
        public SwapChain swapchain { get; private set; }
        public Texture2D backbuffer { get; private set; }
        public Texture2D depthbuffer { get; private set; }
        public RenderTargetView targetview { get; private set; }
        public DepthStencilView depthview { get; private set; }

        private volatile bool Running = false;
        private volatile bool Rendering = false;
        private volatile bool Resizing = false;
        private object syncroot = new object(); //for thread safety
        public int multisamplecount { get; private set; } = 4; //should be a setting..
        public int multisamplequality { get; private set; } = 0; //should be a setting...
        public Color clearcolour { get; private set; } = new Color(0.2f, 0.4f, 0.6f, 1.0f); //gross
        private System.Drawing.Size beginSize;
        private ViewportF Viewport;
        private bool autoStartLoop = false;

        public bool Init(DXForm form, bool autostart = true)
        {
            dxform = form;
            autoStartLoop = autostart;

            try
            {
                //SharpDX.Configuration.EnableObjectTracking = true;

                SwapChainDescription scd = new SwapChainDescription()
                {
                    BufferCount = 2,
                    Flags = SwapChainFlags.None,
                    IsWindowed = true,
                    ModeDescription = new ModeDescription(
                        form.Form.ClientSize.Width,
                        form.Form.ClientSize.Height,
                        new Rational(0, 0),
                        Format.R8G8B8A8_UNorm),
                    OutputHandle = form.Form.Handle,
                    SampleDescription = new SampleDescription(multisamplecount, multisamplequality),
                    SwapEffect = SwapEffect.Discard,
                    Usage = Usage.RenderTargetOutput
                };

                FeatureLevel[] levels = new FeatureLevel[] { FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 };

                DeviceCreationFlags flags = DeviceCreationFlags.None;
                //#if DEBUG
                //    flags = DeviceCreationFlags.Debug;
                //#endif
                Device dev = null;
                SwapChain sc = null;
                Exception exc = null;

                bool success = false;
                try
                {
                    Device.CreateWithSwapChain(DriverType.Hardware, flags, levels, scd, out dev, out sc);
                    success = true;
                }
                catch(Exception ex) { exc = ex; }

                if (!success)
                {
                    multisamplecount = 1;
                    multisamplequality = 0;
                    scd.SampleDescription = new SampleDescription(1, 0); //try no AA
                    try
                    {
                        Device.CreateWithSwapChain(DriverType.Hardware, flags, levels, scd, out dev, out sc);
                        success = true;
                    }
                    catch (Exception ex) { exc = ex; }
                }

                if (!success)
                {
                    var msg = "CodeWalker was unable to initialise the graphics device. Please ensure your system meets the minimum requirements and that your graphics drivers and DirectX are up to date.";
                    if (exc != null)
                    {
                        msg += "\n\nException info: " + exc.ToString();
                    }
                    throw new Exception(msg);
                }

                device = dev;
                swapchain = sc;


                var factory = swapchain.GetParent<Factory>(); //ignore windows events...
                factory.MakeWindowAssociation(form.Form.Handle, WindowAssociationFlags.IgnoreAll);



                context = device.ImmediateContext;



                CreateRenderBuffers();



                dxform.Form.Load += Dxform_Load;
                dxform.Form.FormClosing += Dxform_FormClosing;
                dxform.Form.ClientSizeChanged += Dxform_ClientSizeChanged;
                dxform.Form.ResizeBegin += DxForm_ResizeBegin;
                dxform.Form.ResizeEnd += DxForm_ResizeEnd;

                if (autostart)
                {
                    dxform.InitScene(device);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to initialise DirectX11.\n" + ex.Message, "CodeWalker - Error!");
                return false;
            }
        }


        private void Cleanup()
        {
            Running = false;
            int count = 0;
            while (Rendering && (count < 1000))
            {
                Thread.Sleep(1); //try to gracefully exit...
                count++;
            }

            dxform.CleanupScene();

            if (context != null) context.ClearState();

            //dipose of all objects
            if (depthview != null) depthview.Dispose();
            if (depthbuffer != null) depthbuffer.Dispose();
            if (targetview != null) targetview.Dispose();
            if (backbuffer != null) backbuffer.Dispose();
            if (swapchain != null) swapchain.Dispose();
            if (context != null) context.Dispose();

            //var objs = SharpDX.Diagnostics.ObjectTracker.FindActiveObjects();

            if (device != null) device.Dispose();

            GC.Collect();
        }
        private void CreateRenderBuffers()
        {
            if (targetview != null) targetview.Dispose();
            if (backbuffer != null) backbuffer.Dispose();
            if (depthview != null) depthview.Dispose();
            if (depthbuffer != null) depthbuffer.Dispose();


            backbuffer = Texture2D.FromSwapChain<Texture2D>(swapchain, 0);
            targetview = new RenderTargetView(device, backbuffer);

            depthbuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float,
                ArraySize = 1,
                MipLevels = 1,
                Width = backbuffer.Description.Width,
                Height = backbuffer.Description.Height,
                SampleDescription = new SampleDescription(multisamplecount, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            depthview = new DepthStencilView(device, depthbuffer);

            Viewport.Width = (float)backbuffer.Description.Width;
            Viewport.Height = (float)backbuffer.Description.Height;
            Viewport.MinDepth = 0.0f;
            Viewport.MaxDepth = 1.0f;
            Viewport.X = 0;
            Viewport.Y = 0;
        }
        private void Resize()
        {
            if (Resizing) return;
            Monitor.Enter(syncroot);

            int width = dxform.Form.ClientSize.Width;
            int height = dxform.Form.ClientSize.Height;

            if (targetview != null) targetview.Dispose();
            if (backbuffer != null) backbuffer.Dispose();

            swapchain.ResizeBuffers(1, width, height, Format.Unknown, SwapChainFlags.AllowModeSwitch);

            CreateRenderBuffers();

            Monitor.Exit(syncroot);

            dxform.BuffersResized(width, height);
        }

        private void Dxform_Load(object sender, EventArgs e)
        {
            if (autoStartLoop)
            {
                StartRenderLoop();
            }
        }
        private void Dxform_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!e.Cancel)
            {
                if (!dxform.ConfirmQuit())
                {
                    e.Cancel = true;
                }
            }
            if (!e.Cancel)
            {
                Cleanup();
            }
        }
        private void Dxform_ClientSizeChanged(object sender, EventArgs e)
        {
            Resize();
        }
        private void DxForm_ResizeBegin(object sender, EventArgs e)
        {
            beginSize = dxform.Form.ClientSize;
            Resizing = true;
        }
        private void DxForm_ResizeEnd(object sender, EventArgs e)
        {
            Resizing = false;
            if (dxform.Form.ClientSize != beginSize)
            {
                Resize();
            }
        }


        public void Start()
        {
            dxform.InitScene(device);
            StartRenderLoop();
        }
        private void StartRenderLoop()
        {
            Running = true;
            new Thread(new ThreadStart(RenderLoop)).Start();
        }
        private void RenderLoop()
        {
            while (Running)
            {
                while (Resizing)
                {
                    swapchain.Present(1, PresentFlags.None); //just flip buffers when resizing; don't draw
                }
                while (dxform.Form.WindowState == FormWindowState.Minimized)
                {
                    Thread.Sleep(10); //don't hog CPU when minimised
                    if (dxform.Form.IsDisposed) return; //if closed while minimised
                }
                if (Form.ActiveForm == null)
                {
                    Thread.Sleep(100); //reduce the FPS when the app isn't active (maybe this should be configurable?)
                    if (context.IsDisposed) return; //if form closed while sleeping (eg from rightclick on taskbar)
                }

                Rendering = true;
                if(!Monitor.TryEnter(syncroot, 50))
                {
                    Thread.Sleep(10); //don't hog CPU when not able to render...
                    continue;
                }

                bool ok = true;
                try
                {
                    context.OutputMerger.SetRenderTargets(depthview, targetview);
                    context.Rasterizer.SetViewport(0, 0, dxform.Form.ClientSize.Width, dxform.Form.ClientSize.Height);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error setting main render target!\n" + ex.ToString());
                    ok = false;
                }

                if (ok)
                {
                    if (dxform.Form.IsDisposed)
                    {
                        Monitor.Exit(syncroot);
                        Rendering = false;
                        return; //the form was closed... stop!!
                    }

                    dxform.RenderScene(context);

                    try
                    {
                        swapchain.Present(1, PresentFlags.None);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error presenting swap chain!\n" + ex.ToString());
                    }
                }

                Monitor.Exit(syncroot);
                Rendering = false;

            }
        }


        public void ClearRenderTarget(DeviceContext ctx)
        {
            ctx.ClearRenderTargetView(targetview, clearcolour);
            ctx.ClearDepthStencilView(depthview, DepthStencilClearFlags.Depth, 0.0f, 0);
        }
        public void ClearDepth(DeviceContext ctx)
        {
            ctx.ClearDepthStencilView(depthview, DepthStencilClearFlags.Depth, 0.0f, 0);
        }
        public void SetDefaultRenderTarget(DeviceContext ctx)
        {
            ctx.OutputMerger.SetRenderTargets(depthview, targetview);
            ctx.Rasterizer.SetViewport(Viewport);
            //ctx.Rasterizer.State = RasterizerStateSolid;
        }





    }
}
