using CodeWalker.GameFiles;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using CodeWalker.World;

namespace CodeWalker.Rendering
{
    public abstract class Shader
    {

        public abstract void SetShader(DeviceContext context);
        public abstract bool SetInputLayout(DeviceContext context, VertexType type);
        public abstract void SetSceneVars(DeviceContext context, Camera camera, Shadowmap shadowmap, ShaderGlobalLights lights);
        public abstract void SetEntityVars(DeviceContext context, ref RenderableInst rend);
        public abstract void SetModelVars(DeviceContext context, RenderableModel model);
        public abstract void SetGeomVars(DeviceContext context, RenderableGeometry geom);
        public abstract void UnbindResources(DeviceContext context);

        //public abstract void Dispose();

    }
}
