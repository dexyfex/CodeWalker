using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Rendering
{
    public interface DXForm
    {
        //unfortunately this can't be made a base class for the main render forms, because the
        //form designer causes an error when inheriting from a form in the same project, if
        //the architecture is set to x64. really annoying!
        //So, i've used an interface instead, since really just some of the form properties
        //and a couple of extra methods (these callbacks) are needed by DXManager.

        Form Form { get; }


        void InitScene(Device device);
        void CleanupScene();
        void RenderScene(DeviceContext context);
        void BuffersResized(int w, int h);
        bool ConfirmQuit();
    }
}
