using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX.Direct3D11;
using System.IO;
using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;

namespace CodeWalker.Rendering
{

    public struct PostProcessorReduceCSVars
    {
        public uint dimx;
        public uint dimy;
        public uint Width;
        public uint Height;
    }
    public struct PostProcessorLumBlendCSVars
    {
        public Vector4 blend;
    }
    public struct PostProcessorFilterSampleWeights
    {
        public Vector4 avSampleWeights00; //[15];
        public Vector4 avSampleWeights01;
        public Vector4 avSampleWeights02;
        public Vector4 avSampleWeights03;
        public Vector4 avSampleWeights04;
        public Vector4 avSampleWeights05;
        public Vector4 avSampleWeights06;
        public Vector4 avSampleWeights07;
        public Vector4 avSampleWeights08;
        public Vector4 avSampleWeights09;
        public Vector4 avSampleWeights10;
        public Vector4 avSampleWeights11;
        public Vector4 avSampleWeights12;
        public Vector4 avSampleWeights13;
        public Vector4 avSampleWeights14;

        public readonly Vector4 Get(int i)
        {
            switch (i)
            {
                default:
                case 0: return avSampleWeights00;
                case 1: return avSampleWeights01;
                case 2: return avSampleWeights02;
                case 3: return avSampleWeights03;
                case 4: return avSampleWeights04;
                case 5: return avSampleWeights05;
                case 6: return avSampleWeights06;
                case 7: return avSampleWeights07;
                case 8: return avSampleWeights08;
                case 9: return avSampleWeights09;
                case 10: return avSampleWeights10;
                case 11: return avSampleWeights11;
                case 12: return avSampleWeights12;
                case 13: return avSampleWeights13;
                case 14: return avSampleWeights14;
            }
        }
        public void Set(int i, Vector4 v)
        {
            switch (i)
            {
                case 0: avSampleWeights00 = v; break;
                case 1: avSampleWeights01 = v; break;
                case 2: avSampleWeights02 = v; break;
                case 3: avSampleWeights03 = v; break;
                case 4: avSampleWeights04 = v; break;
                case 5: avSampleWeights05 = v; break;
                case 6: avSampleWeights06 = v; break;
                case 7: avSampleWeights07 = v; break;
                case 8: avSampleWeights08 = v; break;
                case 9: avSampleWeights09 = v; break;
                case 10: avSampleWeights10 = v; break;
                case 11: avSampleWeights11 = v; break;
                case 12: avSampleWeights12 = v; break;
                case 13: avSampleWeights13 = v; break;
                case 14: avSampleWeights14 = v; break;
            }
        }
    }
    public struct PostProcessorFilterBPHCSVars
    {
        public PostProcessorFilterSampleWeights avSampleWeights;
        public uint outputwidth;
        public float finverse;
        public int inputsize0;
        public int inputsize1;
    }
    public struct PostProcessorFilterVCSVars
    {
        public PostProcessorFilterSampleWeights avSampleWeights;
        public int outputsize0;
        public int outputsize1;
        public int inputsize0;
        public int inputsize1;
    }
    public struct PostProcessorFinalPSVars
    {
        public Vector4 invPixelCount;
    }


    public class PostProcessor : IDisposable
    {
        ComputeShader? ReduceTo1DCS;
        ComputeShader? ReduceTo0DCS;
        ComputeShader? LumBlendCS;
        ComputeShader? BloomFilterBPHCS;
        ComputeShader? BloomFilterVCS;
        PixelShader? CopyPixelsPS;
        VertexShader? FinalPassVS;
        PixelShader? FinalPassPS;
        UnitQuad? FinalPassQuad;
        InputLayout? FinalPassLayout;
        GpuVarsBuffer<PostProcessorReduceCSVars>? ReduceCSVars;
        GpuVarsBuffer<PostProcessorLumBlendCSVars>? LumBlendCSVars;
        GpuVarsBuffer<PostProcessorFilterBPHCSVars>? FilterBPHCSVars;
        GpuVarsBuffer<PostProcessorFilterVCSVars>? FilterVCSVars;
        GpuVarsBuffer<PostProcessorFinalPSVars>? FinalPSVars;

        GpuTexture? Primary;

        GpuBuffer<float>? Reduction0;
        GpuBuffer<float>? Reduction1;

        GpuBuffer<float>? LumBlendResult;

        GpuBuffer<Vector4>? Bloom0;
        GpuBuffer<Vector4>? Bloom1;
        GpuTexture? Bloom;

        SamplerState? SampleStatePoint;
        SamplerState? SampleStateLinear;
        BlendState? BlendState;
        long WindowSizeVramUsage = 0;
        int Width = 0;
        int Height = 0;
        ViewportF Viewport;
        bool Multisampled = false;
        bool EnableBloom = true;
        float ElapsedTime = 0.0f;
        public float LumBlendSpeed = 1.0f;

        bool CS_FULL_PIXEL_REDUCTION = false;

        public long VramUsage
        {
            get
            {
                return WindowSizeVramUsage;
            }
        }

        RawViewportF[] vpOld = new RawViewportF[15];

        DeferredScene? DefScene;
        bool UsePrimary = true;

        ShaderResourceView SceneColourSRV
        {
            get
            {
                var srv = DefScene?.SceneColour?.SRV;
                if (UsePrimary || (srv == null))
                {
                    srv = Primary.SRV;
                }
                return srv;
            }
        }


        public PostProcessor(DXManager dxman)
        {
            var device = dxman.device;

            string folder = ShaderManager.GetShaderFolder();
            byte[] bReduceTo1DCS = File.ReadAllBytes(Path.Combine(folder, "PPReduceTo1DCS.cso"));
            byte[] bReduceTo0DCS = File.ReadAllBytes(Path.Combine(folder, "PPReduceTo0DCS.cso"));
            byte[] bLumBlendCS = File.ReadAllBytes(Path.Combine(folder, "PPLumBlendCS.cso"));
            byte[] bBloomFilterBPHCS = File.ReadAllBytes(Path.Combine(folder, "PPBloomFilterBPHCS.cso"));
            byte[] bBloomFilterVCS = File.ReadAllBytes(Path.Combine(folder, "PPBloomFilterVCS.cso"));
            byte[] bCopyPixelsPS = File.ReadAllBytes(Path.Combine(folder, "PPCopyPixelsPS.cso"));
            byte[] bFinalPassVS = File.ReadAllBytes(Path.Combine(folder, "PPFinalPassVS.cso"));
            byte[] bFinalPassPS = File.ReadAllBytes(Path.Combine(folder, "PPFinalPassPS.cso"));

            ReduceTo1DCS = new ComputeShader(device, bReduceTo1DCS);
            ReduceTo0DCS = new ComputeShader(device, bReduceTo0DCS);
            LumBlendCS = new ComputeShader(device, bLumBlendCS);
            BloomFilterBPHCS = new ComputeShader(device, bBloomFilterBPHCS);
            BloomFilterVCS = new ComputeShader(device, bBloomFilterVCS);
            CopyPixelsPS = new PixelShader(device, bCopyPixelsPS);
            FinalPassVS = new VertexShader(device, bFinalPassVS);
            FinalPassPS = new PixelShader(device, bFinalPassPS);
            FinalPassQuad = new UnitQuad(device, true);
            FinalPassLayout = new InputLayout(device, bFinalPassVS, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),
            });


            ReduceCSVars = new GpuVarsBuffer<PostProcessorReduceCSVars>(device);
            LumBlendCSVars = new GpuVarsBuffer<PostProcessorLumBlendCSVars>(device);
            FilterBPHCSVars = new GpuVarsBuffer<PostProcessorFilterBPHCSVars>(device);
            FilterVCSVars = new GpuVarsBuffer<PostProcessorFilterVCSVars>(device);
            FinalPSVars = new GpuVarsBuffer<PostProcessorFinalPSVars>(device);

            TextureAddressMode a = TextureAddressMode.Clamp;
            Color4 b = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            Comparison c = Comparison.Always;
            SampleStatePoint = DXUtility.CreateSamplerState(device, a, b, c, Filter.MinMagMipPoint, 0, 1.0f, 1.0f, 0.0f);
            SampleStateLinear = DXUtility.CreateSamplerState(device, a, b, c, Filter.MinMagMipLinear, 0, 1.0f, 1.0f, 0.0f);

            BlendState = DXUtility.CreateBlendState(device, false, BlendOperation.Add, BlendOption.One, BlendOption.Zero, BlendOperation.Add, BlendOption.One, BlendOption.Zero, ColorWriteMaskFlags.All);

            GetSampleWeights(ref FilterVCSVars.Vars.avSampleWeights, 3.0f, 1.25f); //init sample weights
            FilterBPHCSVars.Vars.avSampleWeights = FilterVCSVars.Vars.avSampleWeights;
        }
        private bool isDisposed = false;
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            DisposeBuffers();

            BlendState?.Dispose();
            BlendState = null;

            SampleStateLinear?.Dispose();
            SampleStateLinear = null;

            SampleStatePoint?.Dispose();
            SampleStatePoint = null;


            FinalPSVars?.Dispose();
            FinalPSVars = null;


            FilterVCSVars?.Dispose();
            FilterVCSVars = null;

            FilterBPHCSVars?.Dispose();
            FilterBPHCSVars = null;

            LumBlendCSVars?.Dispose();
            LumBlendCSVars = null;

            ReduceCSVars?.Dispose();
            ReduceCSVars = null;

            FinalPassLayout?.Dispose();
            FinalPassLayout = null;

            FinalPassQuad?.Dispose();
            FinalPassQuad = null;

            FinalPassPS?.Dispose();
            FinalPassPS = null;

            FinalPassVS?.Dispose();
            FinalPassVS = null;

            CopyPixelsPS?.Dispose();
            CopyPixelsPS = null;

            BloomFilterVCS?.Dispose();
            BloomFilterVCS = null;

            BloomFilterBPHCS?.Dispose();
            BloomFilterBPHCS = null;

            LumBlendCS?.Dispose();
            LumBlendCS = null;

            ReduceTo0DCS?.Dispose();
            ReduceTo0DCS = null;

            ReduceTo1DCS?.Dispose();
            ReduceTo1DCS = null;

            isDisposed = true;
            GC.SuppressFinalize(this);
        }

        public void OnWindowResize(DXManager dxman)
        {
            DisposeBuffers();

            var device = dxman.device;


            int sc = dxman.multisamplecount;
            int sq = dxman.multisamplequality;
            Multisampled = (sc > 1);

            int uw = Width = dxman.backbuffer.Description.Width;
            int uh = Height = dxman.backbuffer.Description.Height;
            Viewport = new ViewportF();
            Viewport.Width = (float)uw;
            Viewport.Height = (float)uh;
            Viewport.MinDepth = 0.0f;
            Viewport.MaxDepth = 1.0f;
            Viewport.X = 0.0f;
            Viewport.Y = 0.0f;


            Format f = Format.R32G32B32A32_Float;
            Format df = Format.D32_Float;


            Primary = new GpuTexture(device, uw, uh, f, sc, sq, true, df);
            WindowSizeVramUsage += Primary.VramUsage;


            int rc = (int)(Math.Ceiling(uw / 8.0f) * Math.Ceiling(uh / 8.0f));

            Reduction0 = new GpuBuffer<float>(device, 1, rc);
            Reduction1 = new GpuBuffer<float>(device, 1, rc);
            WindowSizeVramUsage += sizeof(float) * rc * 2;

            LumBlendResult = new GpuBuffer<float>(device, 1, 1);
            WindowSizeVramUsage += sizeof(float); //because 4 bytes matter

            int tw = uw / 8;
            int th = uh / 8;
            rc = tw * th;
            f = Format.R8G8B8A8_UNorm;

            Bloom0 = new GpuBuffer<Vector4>(device, 1, rc);
            Bloom1 = new GpuBuffer<Vector4>(device, 1, rc);
            WindowSizeVramUsage += /*sizeof(V4F)*/ 16 * rc * 2;

            Bloom = new GpuTexture(device, tw, th, f, 1, 0, false, df);
            WindowSizeVramUsage += Bloom.VramUsage;


        }
        public void DisposeBuffers()
        {
            Bloom?.Dispose();
            Bloom = null;

            Bloom0?.Dispose();
            Bloom0 = null;

            Bloom1?.Dispose();
            Bloom1 = null;

            LumBlendResult?.Dispose();
            LumBlendResult = null;

            Reduction0?.Dispose();
            Reduction0 = null;

            Reduction1?.Dispose();
            Reduction1 = null;

            Primary?.Dispose();
            Primary = null;

            WindowSizeVramUsage = 0;
        }

        public void Clear(DeviceContext context)
        {
            Color4 clearColour = new Color4(0.2f, 0.4f, 0.6f, 0.0f);
            //Color4 clearColour = new Color4(0.0f, 0.0f, 0.0f, 0.0f);

            Primary?.Clear(context, clearColour);
        }
        public void ClearDepth(DeviceContext context)
        {
            Primary?.ClearDepth(context);
        }
        public void SetPrimary(DeviceContext context)
        {
            Primary?.SetRenderTarget(context);
            context.Rasterizer.SetViewport(Viewport);
        }

        public void Render(DXManager dxman, float elapsed, DeferredScene? defScene)
        {
            ElapsedTime = elapsed;
            DefScene = defScene;
            UsePrimary = ((defScene?.SSAASampleCount ?? 2) > 1) || (defScene?.SceneColour == null);

            var context = dxman.context;

            if (Multisampled && UsePrimary)
            {
                int sr = 0;// D3D11CalcSubresource(0, 0, 1);
                context.ResolveSubresource(Primary.TextureMS, sr, Primary.Texture, sr, Format.R32G32B32A32_Float);
            }

            context.OutputMerger.SetRenderTargets((RenderTargetView)null);

            ProcessLuminance(context);
            ProcessBloom(context);

            dxman.SetDefaultRenderTarget(context);
            context.OutputMerger.SetBlendState(BlendState, null, 0xFFFFFFFF);
            FinalPass(context);
        }


        private void ProcessLuminance(DeviceContext context)
        {
            var srv = SceneColourSRV;

            uint dimx, dimy;
            if (CS_FULL_PIXEL_REDUCTION)
            {
                dimx = (uint)(Math.Ceiling(Width / 8.0f));
                dimx = (uint)(Math.Ceiling(dimx / 2.0f));
                dimy = (uint)(Math.Ceiling(Height / 8.0f));
                dimy = (uint)(Math.Ceiling(dimy / 2.0f));
            }
            else
            {
                dimx = (uint)(Math.Ceiling(81.0f / 8.0f)); //ToneMappingTexSize = (int)pow(3.0f, NUM_TONEMAP_TEXTURES-1);
                dimy = dimx;
            }

            ReduceCSVars.Vars.dimx = dimx;
            ReduceCSVars.Vars.dimy = dimy;
            ReduceCSVars.Vars.Width = (uint)Width;
            ReduceCSVars.Vars.Height = (uint)Height;
            ReduceCSVars.Update(context);


            Compute(context, ReduceTo1DCS, ReduceCSVars.Buffer, Reduction0.UAV, (int)dimx, (int)dimy, 1, srv);

            uint dim = dimx * dimy;
            uint nNumToReduce = dim;
            dim = (uint)(Math.Ceiling(dim / 128.0f));
            if (nNumToReduce > 1)
            {
                for (;;)
                {
                    ReduceCSVars.Vars.dimx = nNumToReduce;
                    ReduceCSVars.Vars.dimy = dim;
                    ReduceCSVars.Vars.Width = 0;
                    ReduceCSVars.Vars.Height = 0;
                    ReduceCSVars.Update(context);

                    Compute(context, ReduceTo0DCS, ReduceCSVars.Buffer, Reduction1.UAV, (int)dim, 1, 1, Reduction0.SRV);

                    nNumToReduce = dim;
                    dim = (uint)(Math.Ceiling(dim / 128.0f));

                    if (nNumToReduce == 1) break;

                    var r0 = Reduction0;
                    Reduction0 = Reduction1;
                    Reduction1 = r0;
                }
            }
            else
            {
                var r0 = Reduction0;
                Reduction0 = Reduction1;
                Reduction1 = r0;
            }

            LumBlendCSVars.Vars.blend = new Vector4(Math.Min(ElapsedTime * LumBlendSpeed, 1.0f));
            LumBlendCSVars.Update(context);

            Compute(context, LumBlendCS, LumBlendCSVars.Buffer, LumBlendResult.UAV, 1, 1, 1, Reduction1.SRV);

        }
        private void ProcessBloom(DeviceContext context)
        {
            if (EnableBloom)
            {
                var srv = SceneColourSRV;

                // Bright pass and horizontal blur

                //GetSampleWeights(cbFilter.avSampleWeights, 3.0f, 1.25f);
                FilterBPHCSVars.Vars.outputwidth = (uint)(Width / 8);
                if (CS_FULL_PIXEL_REDUCTION)
                {
                    FilterBPHCSVars.Vars.finverse = 1.0f / (Width * Height);
                }
                else
                {
                    FilterBPHCSVars.Vars.finverse = 1.0f / (81 * 81); //(ToneMappingTexSize*ToneMappingTexSize);
                }
                FilterBPHCSVars.Vars.inputsize0 = (int)Width;
                FilterBPHCSVars.Vars.inputsize1 = (int)Height;
                FilterBPHCSVars.Update(context);

                int x = (int)(Math.Ceiling((float)FilterBPHCSVars.Vars.outputwidth / (128 - 7 * 2)));
                int y = (Height / 8);
                Compute(context, BloomFilterBPHCS, FilterBPHCSVars.Buffer, Bloom1.UAV, x, y, 1, srv, LumBlendResult.SRV);

                // Vertical blur
                FilterVCSVars.Vars.outputsize0 = (int)(Width / 8);
                FilterVCSVars.Vars.outputsize1 = (int)(Height / 8);
                FilterVCSVars.Vars.inputsize0 = (int)(Width / 8);
                FilterVCSVars.Vars.inputsize1 = (int)(Height / 8);
                FilterVCSVars.Update(context);
                x = Width / 8;
                y = (int)(Math.Ceiling((float)FilterVCSVars.Vars.outputsize1 / (128 - 7 * 2)));

                Compute(context, BloomFilterVCS, FilterVCSVars.Buffer, Bloom0.UAV, x, y, 1, Bloom1.SRV, LumBlendResult.SRV);
            }

            CopyPixels(context, Width / 8, Height / 8, Bloom0.SRV, Bloom.RTV);

        }
        private void FinalPass(DeviceContext context)
        {
            context.Rasterizer.SetViewport(Viewport);
            context.VertexShader.Set(FinalPassVS);
            context.PixelShader.Set(FinalPassPS);

            var srv = SceneColourSRV;

            context.PixelShader.SetShaderResources(0, srv, LumBlendResult.SRV, EnableBloom ? Bloom.SRV : null);

            if (CS_FULL_PIXEL_REDUCTION)
            {
                FinalPSVars.Vars.invPixelCount = new Vector4(1.0f / (Width * Height));
            }
            else
            {
                FinalPSVars.Vars.invPixelCount = new Vector4(1.0f / (81 * 81));
            }
            FinalPSVars.Update(context);
            FinalPSVars.SetPSCBuffer(context, 0);

            context.PixelShader.SetSamplers(0, SampleStatePoint, SampleStateLinear);

            context.InputAssembler.InputLayout = FinalPassLayout;
            FinalPassQuad.Draw(context);

            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
            context.PixelShader.SetShaderResources(0, null, null, null);
            context.PixelShader.SetSamplers(0, null, null);
        }




        private float GaussianDistribution(float x, float y, float rho)
        {
            float g = 1.0f / (float)Math.Sqrt(2.0f * 3.14159265f * rho * rho);
            g *= (float)Math.Exp(-(x * x + y * y) / (2 * rho * rho));
            return g;
        }
        private void GetSampleWeights(ref PostProcessorFilterSampleWeights w, float fDeviation, float fMultiplier)
        {
            // Fill the center texel
            float weight = 1.0f * GaussianDistribution(0, 0, fDeviation);
            w.Set(7, new Vector4(weight, weight, weight, 1.0f));

            // Fill the right side
            for (int i = 1; i < 8; i++)
            {
                weight = fMultiplier * GaussianDistribution((float)i, 0, fDeviation);
                w.Set(7 - i, new Vector4(weight, weight, weight, 1.0f));
            }

            // Copy to the left side
            for (int i = 8; i < 15; i++)
            {
                w.Set(i, w.Get(14 - i));
            }

            // Debug convolution kernel which doesn't transform input data
            /*ZeroMemory( avColorWeight, sizeof(D3DXVECTOR4)*15 );
            w.Set(7, new Vector4( 1, 1, 1, 1 ));*/
        }


        private void Compute(DeviceContext context, ComputeShader cs, Buffer constantBuffer, UnorderedAccessView unorderedAccessView, int X, int Y, int Z, params ShaderResourceView[] resourceViews)
        {
            context.ComputeShader.Set(cs);
            context.ComputeShader.SetShaderResources(0, resourceViews);
            context.ComputeShader.SetUnorderedAccessView(0, unorderedAccessView);

            if (constantBuffer != null)
            {
                //make sure buffer is updated first...
                //D3D11_MAPPED_SUBRESOURCE mappedResource;
                //dc->Map(constantBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
                //memcpy(mappedResource.pData, constantData, constantDataSize);
                //dc->Unmap(constantBuffer, 0);
                context.ComputeShader.SetConstantBuffer(0, constantBuffer); //dc->CSSetConstantBuffers(0, 1, &constantBuffer);
            }

            context.Dispatch(X, Y, Z);

            ShaderResourceView[] ppSRVNULL = { null, null, null };
            context.ComputeShader.SetUnorderedAccessView(0, null);
            context.ComputeShader.SetShaderResources(0, 3, ppSRVNULL);
            context.ComputeShader.SetConstantBuffer(0, null);
        }

        private void CopyPixels(DeviceContext context, int w, int h, ShaderResourceView fromSRV, RenderTargetView toRTV)
        {
            context.VertexShader.Set(FinalPassVS);
            context.PixelShader.Set(CopyPixelsPS);

            ShaderResourceView[] aRViews = { fromSRV };
            context.PixelShader.SetShaderResource(0, fromSRV);

            RenderTargetView[] aRTViews = { toRTV };
            context.OutputMerger.SetRenderTargets(toRTV);

            //D3D11_MAPPED_SUBRESOURCE mappedResource;
            //dc->Map(ReduceCSVars.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
            //UINT* p = (UINT*)mappedResource.pData;
            //p[0] = w;
            //p[1] = h;
            //dc->Unmap(ReduceCSVars.Get(), 0);
            //ID3D11Buffer* ppCB[1] = { ReduceCSVars.Get() };
            //dc->PSSetConstantBuffers(0, 1, ppCB);
            ReduceCSVars.Vars.dimx = (uint)w;
            ReduceCSVars.Vars.dimy = (uint)h;
            ReduceCSVars.Update(context);
            ReduceCSVars.SetPSCBuffer(context, 0);

            //DrawFullScreenQuad11( pd3dImmediateContext, g_pDumpBufferPS, dwWidth, dwHeight );
            //ViewportF[] vpOld = new ViewportF[15];// [D3D11_VIEWPORT_AND_SCISSORRECT_MAX_INDEX];
            //UINT nViewPorts = 1;
            //dc->RSGetViewports(&nViewPorts, vpOld);
            context.Rasterizer.GetViewports(vpOld);

            // Setup the viewport to match the backbuffer
            ViewportF vp;
            vp.Width = (float)Width;
            vp.Height = (float)Height;
            vp.MinDepth = 0.0f;
            vp.MaxDepth = 1.0f;
            vp.X = 0;
            vp.Y = 0;
            context.Rasterizer.SetViewport(vp);

            context.InputAssembler.InputLayout = FinalPassLayout;
            FinalPassQuad.Draw(context);


            context.Rasterizer.SetViewports(vpOld); //reverting viewports maybe not necessary...
        }


    }
}
