using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public enum VertexComponentType : byte
    {
        Nothing = 0,
        Half2 = 1,
        Float = 2,
        Half4 = 3,
        FloatUnk = 4,
        Float2 = 5,
        Float3 = 6,
        Float4 = 7,
        UByte4 = 8,
        Colour = 9,
        Dec3N = 10,
        Unk1 = 11,
        Unk2 = 12,
        Unk3 = 13,
        Unk4 = 14,
        Unk5 = 15,
    }

    public static class VertexComponentTypes
    {
        public static int GetSizeInBytes(VertexComponentType type)
        {
            switch (type)
            {
                case VertexComponentType.Nothing: return 0;
                case VertexComponentType.Half2: return 4;
                case VertexComponentType.Float: return 4;
                case VertexComponentType.Half4: return 8;
                case VertexComponentType.FloatUnk: return 0;
                case VertexComponentType.Float2: return 8;
                case VertexComponentType.Float3: return 12;
                case VertexComponentType.Float4: return 16;
                case VertexComponentType.UByte4: return 4;
                case VertexComponentType.Colour: return 4;
                case VertexComponentType.Dec3N: return 4;
                default: return 0;
            }
        }

        public static int GetComponentCount(VertexComponentType type)
        {
            switch (type)
            {
                case VertexComponentType.Nothing: return 0;
                case VertexComponentType.Half2: return 2;
                case VertexComponentType.Float: return 1;
                case VertexComponentType.Half4: return 4;
                case VertexComponentType.FloatUnk: return 0;
                case VertexComponentType.Float2: return 2;
                case VertexComponentType.Float3: return 3;
                case VertexComponentType.Float4: return 4;
                case VertexComponentType.UByte4: return 4;
                case VertexComponentType.Colour: return 4;
                case VertexComponentType.Dec3N: return 3;
                default: return 0;
            }
        }

    }

    public enum VertexDeclarationTypes : ulong
    {
        GTAV1 = 0x7755555555996996, // GTAV - used by most drawables
        GTAV2 = 0x030000000199A006, // GTAV - used on cloth?
        GTAV3 = 0x0300000001996006, // GTAV - used on cloth?
        GTAV4 = 0x7655555555996996, // GTAV - used by FragGlassWindow

        //Types4 = 0x0000000007097007, // Max Payne 3
        //Types5 = 0x0700000007097977, // Max Payne 3
        //Types6 = 0x0700000007997977, // Max Payne 3
        //Types7 = 0x0700007777097977, // Max Payne 3
        //Types8 = 0x0700007777997977, // Max Payne 3
    }

    public enum VertexSemantics : int
    {
        Position = 0,
        BlendWeights = 1,
        BlendIndices = 2,
        Normal = 3,
        Colour0 = 4,
        Colour1 = 5,
        TexCoord0 = 6,
        TexCoord1 = 7,
        TexCoord2 = 8,
        TexCoord3 = 9,
        TexCoord4 = 10,
        TexCoord5 = 11,
        TexCoord6 = 12,
        TexCoord7 = 13,
        Tangent = 14,
        Binormal = 15,
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

    public struct VertexTypeGTAV1    //0x7755555555996996
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
        public Vector2 Texcoord3;
        public Vector2 Texcoord4;
        public Vector2 Texcoord5;
        public Vector2 Texcoord6;
        public Vector2 Texcoord7;
        public Vector4 Tangent;
        public Vector4 Binormal;
    }

    public struct VertexTypeGTAV2    //0x030000000199A006
    {
        public Vector3 Position;
        public uint Normal; // Packed as Dec3N 
        public uint Colour0;
        public uint Colour1;
        public Half2 Texcoord0;
        public Half4 Tangent;
    }

    public struct VertexTypeGTAV3    //0x0300000001996006
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Half2 Texcoord0;
        public Half4 Tangent;
    }

    public struct EditorVertex    //vertex data to be used by the editor. TODO: maybe move somewhere else.
    {
        public Vector3 Position;
        public uint Colour;
    }


}
