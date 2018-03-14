using SharpDX.D3DCompiler;
using System;

namespace CodeWalker.GameFiles
{

    public static class FxcParser
    {

        public static bool ParseShader(FxcShader shader)
        {
            ShaderBytecode ByteCodeObj;
            ShaderProfile ShaderProfile;

            try
            {
                ByteCodeObj = new ShaderBytecode(shader.ByteCode);

                ShaderProfile = ByteCodeObj.GetVersion();


                switch (ShaderProfile.Version)
                {
                    case ShaderVersion.VertexShader:
                    case ShaderVersion.PixelShader:
                    case ShaderVersion.GeometryShader:
                        //VersionMajor = br.ReadByte();//4,5 //appears to be shader model version
                        //VersionMinor = br.ReadByte(); //perhaps shader minor version
                        break;
                    default:
                        shader.VersionMajor = (byte)ShaderProfile.Major;
                        shader.VersionMinor = (byte)ShaderProfile.Minor;
                        break;
                }

                shader.Disassembly = ByteCodeObj.Disassemble();

            }
            catch (Exception ex)
            {
                shader.LastError += ex.ToString() + "\r\n";
                return false;
            }

            return true;
        }

    }

}