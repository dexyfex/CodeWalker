using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{

    public enum VertexDecl : ulong
    {
        Type1 = 0x7755555555996996,
        Type2 = 0x030000000199A006,
        Type3 = 0x0300000001996006,
    }

    public enum VertexType : uint
    {
        Default = 89, //PNCT
        DefaultEx = 16473, //PNCTX
        PNCCT = 121,
        PNCCTTTT = 1017,
        PBBNCCTTX = 16639,
        PBBNCCT = 127,
        PNCTTTX = 16857,
        PNCTTX = 16601,
        PNCTTTX_2 = 19545,
        PNCTTTX_3 = 17113,
        PNCCTTX = 16633,
        PNCCTTX_2 = 17017,
        PNCCTTTX = 17145,
        PBBNCCTX = 16511,
        PBBNCTX = 16479,
        PBBNCT = 95,
        PNCCTT = 249,
        PNCCTX = 16505,
        PCT = 81,
        PT = 65,
        PTT = 193,
        PNC = 25,
        PC = 17,
        PCC = 7,
        PCCH2H4 = 2147500121, //0x80004059  (16473 + 0x80000000) DefaultEx Cloth?
        PNCH2 =   2147483737, //0x80000059  (89 + 0x80000000) Default Cloth?
        PNCTTTTX = 19673,  //normal_spec_detail_dpm_vertdecal_tnt
        PNCTTTT = 985,
        PBBNCCTT = 255,
        PCTT = 209,
        PBBCCT = 119,
        PBBNC = 31,
        PBBNCTT = 223,
        PBBNCTTX = 16607,
        PBBNCTTT = 479,
        PNCTT = 217,
        PNCTTT = 473,
        PBBNCTTTX = 16863,
    }

    //0x7755555555996996
    public struct VertexTypeGTAV1
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normals;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoords0;
        public Vector2 Texcoords1;
        public Vector2 Texcoords2;
        public Vector2 Texcoords3;
        public Vector2 Texcoords4;
        public Vector2 Texcoords5;
        public Vector2 Texcoords6;
        public Vector2 Texcoords7;
        public Vector4 Tangents;
        public Vector4 Binormals;
    }

    //0x030000000199A006
    public struct VertexTypeGTAV2
    {
        public Vector3 Position;
        public uint Normals; // Packed as Dec3N 
        public uint Colour0;
        public uint Colour1;
        public Half2 Texcoords0;
        public Half4 Tangents;
    }

    //0x0300000001996006
    public struct VertexTypeGTAV3
    {
        public Vector3 Position;
        public Vector3 Normals;
        public uint Colour0;
        public uint Colour1;
        public Half2 Texcoords0;
        public Half4 Tangents;
    }

    //vertex data to be used by the editor. TODO: maybe move somewhere else.
    public struct EditorVertex
    {
        public Vector3 Position;
        public uint Colour;

        // TODO: Find a way to add this without adding dependency to SharpDX.Direct3D11 or CW main assembly
        /*public static InputElement[] GetLayout()
        {
            return new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 12, 0),
            };
        }*/
    }


}
