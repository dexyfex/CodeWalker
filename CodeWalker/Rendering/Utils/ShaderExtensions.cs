using CodeWalker.Core.Utils;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Rendering.Utils
{
    public static class ShaderExtensions
    {
        /// <summary>
        /// Attempts to find the shaders folder in the directory tree
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static string GetShaderFolder()
        {
            // Directory we're looking for.
            var dirToFind = Path.Combine("Shaders");


            if (!FileUtils.TryFindFolder(dirToFind, out var folder))
            {
                throw new FileNotFoundException($"Could not find '{dirToFind}' directory.");
            }
            else
            {
                Console.WriteLine(folder);
                return folder;
            }
        }

        public static PixelShader CreatePixelShader(this Device device, string filename)
        {
            var file = Path.Combine(GetShaderFolder(), filename);

            byte[] shaderBytes = File.ReadAllBytes(file);

            var shader = new PixelShader(device, shaderBytes);
            shader.DebugName = filename;
            return shader;
        }

        public static VertexShader CreateVertexShader(this Device device, string filename)
        {
            var file = Path.Combine(GetShaderFolder(), filename);

            byte[] shaderBytes = File.ReadAllBytes(file);
            var shader = new VertexShader(device, shaderBytes);
            shader.DebugName = filename;
            return shader;
        }
    }
}
