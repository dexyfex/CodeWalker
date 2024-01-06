using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Color = SharpDX.Color;
using Device = SharpDX.Direct3D11.Device;
using DriverType = SharpDX.Direct3D.DriverType;
using System.Threading;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D;
using System.Runtime;
using CodeWalker.Core.Utils;
using CodeWalker.Properties;
using CodeWalker.GameFiles;
using System.Diagnostics.CodeAnalysis;

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
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);


        public int multisamplecount { get; private set; } = Settings.Default.AntiAliasing;
        public int multisamplequality { get; private set; } = 0; //should be a setting...
        public Color clearcolour { get; private set; } = new Color(0.2f, 0.4f, 0.6f, 1.0f); //gross
        private System.Drawing.Size beginSize;
        private ViewportF Viewport;
        private bool autoStartLoop = false;

        private void ThrowInitFailedException(Exception exc)
        {
            var msg = "CodeWalker was unable to initialise the graphics device. Please ensure your system meets the minimum requirements and that your graphics drivers and DirectX are up to date.";
            if (exc != null)
            {
                msg += "\n\nException info: " + exc.ToString();
            }
            throw new Exception(msg);
        }

        public bool Init(DXForm form, bool autostart = true)
        {
            dxform = form;
            autoStartLoop = autostart;

            try
            {
                //SharpDX.Configuration.EnableObjectTracking = true;

                var scd = new SwapChainDescription()
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
                Device? dev = null;
                SwapChain? sc = null;
                Exception? exc = null;

                bool success = false;
                try
                {
                    if (device is not null)
                    {
                        dev = device;
                    }
                    else
                    {
                        dev = new Device(DriverType.Hardware, flags, levels);
                        device = dev;
                    }
                    
                    using Factory1 scFactory = new Factory1();
                    sc = new SwapChain(scFactory, dev, scd);
                    Device.CreateWithSwapChain(DriverType.Hardware, flags, levels, scd, out dev, out sc);
                    success = true;
                }
                catch(Exception ex)
                {
                    exc = ex;
                }

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
                    catch (Exception ex)
                    {
                        exc = ex;
                    }
                }

                if (!success)
                {
                    ThrowInitFailedException(exc);
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


        private async ValueTask Cleanup()
        {
            try
            {
                using var _ = new DisposableTimer("DXManager Cleanup");
                Running = false;
                int count = 0;

                if (!dxform.CancellationTokenSource.IsCancellationRequested)
                {
                    dxform.CancellationTokenSource.Cancel();
                }

                while (Rendering && (count < 1000))
                {
                    await Task.Delay(1);
                    count++;
                }

                await dxform.CleanupScene();

                context?.ClearState();

                //dipose of all objects
                depthview?.Dispose();
                depthbuffer?.Dispose();
                targetview?.Dispose();
                backbuffer?.Dispose();
                swapchain?.Dispose();
                context?.Dispose();

                //var objs = SharpDX.Diagnostics.ObjectTracker.FindActiveObjects();

                device?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        [MemberNotNull(nameof(targetview), nameof(backbuffer), nameof(depthview), nameof(depthbuffer))]
        private void CreateRenderBuffers()
        {
            targetview?.Dispose();
            backbuffer?.Dispose();
            depthview?.Dispose();
            depthbuffer?.Dispose();


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
        private async void Resize()
        {
            Console.WriteLine($"Resizing {Resizing}");
            if (Resizing) return;
            await semaphore.WaitAsync().ConfigureAwait(false);
            int width = dxform.Form.ClientSize.Width;
            int height = dxform.Form.ClientSize.Height;

            if (targetview != null) targetview.Dispose();
            if (backbuffer != null) backbuffer.Dispose();



            swapchain.ResizeBuffers(1, width, height, Format.Unknown, SwapChainFlags.AllowModeSwitch);

            CreateRenderBuffers();

            semaphore.Release();

            dxform.BuffersResized(width, height);
        }

        private void Dxform_Load(object sender, EventArgs e)
        {
            if (autoStartLoop)
            {
                StartRenderLoop();
            }
        }
        private async void Dxform_FormClosing(object sender, FormClosingEventArgs e)
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
                if (!dxform.CancellationTokenSource.IsCancellationRequested)
                {
                    dxform.CancellationTokenSource.Cancel();
                }
                await Cleanup();
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
            if (Running)
            {
                return;
            }

            Running = true;

            Task.Run(RenderLoop);
            //new Thread(new ThreadStart(RenderLoop)).Start();
        }


        bool isPointVisibleOnAScreen(Point p)
        {
            foreach (Screen s in Screen.AllScreens)
            {
                if (p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom)
                    return true;
            }
            return false;
        }

        bool isFormFullyVisible(Form f)
        {
            return isPointVisibleOnAScreen(new Point(f.Left, f.Top))
                && isPointVisibleOnAScreen(new Point(f.Right, f.Top))
                && isPointVisibleOnAScreen(new Point(f.Left, f.Bottom))
                && isPointVisibleOnAScreen(new Point(f.Right, f.Bottom));
        }

        private int inactiveCount = 0;
        private async Task RenderLoop()
        {
            //SharpDX.Configuration.EnableObjectTracking = true;
            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        Console.WriteLine(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
            //        await Task.Delay(5000);
            //    }
            //});

            try
            {
                while (Running)
                {
                    while (Resizing)
                    {
                        var result = swapchain.Present(1, PresentFlags.None); //just flip buffers when resizing; don't draw
                        if (result != Result.Ok)
                        {
                            if (result == SharpDX.DXGI.ResultCode.DeviceRemoved)
                            {
                                Console.WriteLine($"Device was removed {device.DeviceRemovedReason}");
                            }
                        }
                    }
                    if (dxform.Form.WindowState == FormWindowState.Minimized)
                    {
                        dxform.Pauserendering = true;

                        Console.WriteLine("Window is minimized");
                        await dxform.RenderScene(context);
                        
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                        while (dxform.Form.WindowState == FormWindowState.Minimized)
                        {
                            await Task.Delay(100, dxform.CancellationToken); //don't hog CPU when minimised
                            if (dxform.Form.IsDisposed || dxform.CancellationToken.IsCancellationRequested)
                                return; //if closed while minimised
                        }
                        dxform.Pauserendering = false;
                        Console.WriteLine("Window is maximized");
                    }


                    if (Form.ActiveForm is null)
                    {
                        inactiveCount++;
                        if (inactiveCount > 100)
                        {
                            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                            GC.Collect();
                            while (Form.ActiveForm is null)
                            {
                                await Task.Delay(100, dxform.CancellationToken); //reduce the FPS when the app isn't active (maybe this should be configurable?)
                                if (context.IsDisposed || dxform.Form.IsDisposed || dxform.CancellationToken.IsCancellationRequested)
                                    return; //if form closed while sleeping (eg from rightclick on taskbar)
                            }
                        }
                        else
                        {
                            await Task.Delay(100, dxform.CancellationToken);
                        }
                    }
                    else
                    {
                        inactiveCount = 0;
                        if (Form.ActiveForm != dxform.Form)
                        {
                            await Task.Delay(25, dxform.CancellationToken);
                        }
                    }

                    if (context.IsDisposed || dxform.Form.IsDisposed || dxform.CancellationToken.IsCancellationRequested)
                        return;

                    Rendering = true;

                    if (!await semaphore.WaitAsync(50, dxform.CancellationToken).ConfigureAwait(false))
                    {
                        Console.WriteLine("Failed to get lock for syncroot");
                        await Task.Delay(10, dxform.CancellationToken).ConfigureAwait(false); //don't hog CPU when not able to render...
                        continue;
                    }

                    if (dxform.Form.IsDisposed || dxform.CancellationToken.IsCancellationRequested)
                    {
                        Rendering = false;
                        return;
                    }

                    try
                    {
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
                            if (dxform.Form.IsDisposed || dxform.CancellationToken.IsCancellationRequested)
                            {

                                Rendering = false;
                                return; //the form was closed... stop!!
                            }

                            await dxform.RenderScene(context);

                            try
                            {
                                var result = swapchain.Present(1, PresentFlags.None);
                                if (result != Result.Ok)
                                {
                                    if (result == SharpDX.DXGI.ResultCode.DeviceRemoved)
                                    {
                                        Console.WriteLine($"Device was removed {device.DeviceRemovedReason}");
                                    } else
                                    {
                                        Console.WriteLine(result.ToString());
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error presenting swap chain!\n" + ex.ToString());
                            }
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                        Rendering = false;
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                Running = false;
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
