using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Rendering
{
    public class RenderTargetSwitch
    {

        RenderTargetView[] OrigRenderTargetViewArr;
        DepthStencilView OrigDepthStencilView;
        RasterizerState OrigRasterizerState;
        RawViewportF[] OrigViewports;
        bool IsReset = true;
        //bool ResetOnDestroy = false;
        int RenderTargetCount = 1;


        public void Dispose()
        {
            if (OrigRenderTargetViewArr != null)
            {
                for (int i = 0; i < RenderTargetCount; i++)
                {
                    if (OrigRenderTargetViewArr[i] != null)
                    {
                        OrigRenderTargetViewArr[i].Dispose();
                    }
                }
                OrigRenderTargetViewArr = null;
            }
            if (OrigDepthStencilView != null)
            {
                OrigDepthStencilView.Dispose();
                OrigDepthStencilView = null;
            }
            if (OrigRasterizerState != null)
            {
                OrigRasterizerState.Dispose();
                OrigRasterizerState = null;
            }
        }

        public void Set(DeviceContext context)
        {
            Dispose();

            OrigRenderTargetViewArr = context.OutputMerger.GetRenderTargets(RenderTargetCount, out OrigDepthStencilView);
            OrigViewports = context.Rasterizer.GetViewports<RawViewportF>();
            OrigRasterizerState = context.Rasterizer.State;

            //OrigRenderTargetViewArr = new RenderTargetView[RenderTargetCount];
            //uint origNumViewports = 1;
            //dc->OMGetRenderTargets(RenderTargetCount, OrigRenderTargetViewArr, &OrigDepthStencilView);
            //dc->RSGetViewports(&origNumViewports, &OrigViewport);
            //dc->RSGetState(&OrigRasterizerState);

            IsReset = false;
        }
        public void Reset(DeviceContext context)
        {
            if (IsReset) return;

            context.OutputMerger.SetRenderTargets(OrigDepthStencilView, OrigRenderTargetViewArr);
            context.Rasterizer.State = OrigRasterizerState;
            context.Rasterizer.SetViewports(OrigViewports);

            //auto dc = DXManager::GetDeviceContext();
            //dc->OMSetRenderTargets(RenderTargetCount, OrigRenderTargetViewArr, OrigDepthStencilView);
            //dc->RSSetState(OrigRasterizerState);
            //dc->RSSetViewports(1, &OrigViewport);

            IsReset = true;

            Dispose();
        }



    };
}
