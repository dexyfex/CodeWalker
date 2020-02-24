using CodeWalker.GameFiles;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Rendering
{
    public static class VertexTypeGTAV
    {
        public static string[] Semantics = new string[16]
        {
            "POSITION",
            "BLENDWEIGHTS",
            "BLENDINDICES",
            "NORMAL",
            "COLOR",
            "COLOR",
            "TEXCOORD",
            "TEXCOORD",
            "TEXCOORD",
            "TEXCOORD",
            "TEXCOORD",
            "TEXCOORD",
            "TEXCOORD",
            "TEXCOORD",
            "TANGENT",
            "BINORMAL",
        };

        public static Format GetDXGIFormat(VertexComponentType type)
        {
            switch (type)
            {
                case VertexComponentType.Nothing: return Format.Unknown;
                case VertexComponentType.Half2: return Format.R16G16_Float;
                case VertexComponentType.Float: return Format.R32_Float;
                case VertexComponentType.Half4: return Format.R16G16B16A16_Float;
                case VertexComponentType.FloatUnk: return Format.Unknown;
                case VertexComponentType.Float2: return Format.R32G32_Float;
                case VertexComponentType.Float3: return Format.R32G32B32_Float;
                case VertexComponentType.Float4: return Format.R32G32B32A32_Float;
                case VertexComponentType.UByte4: return Format.R8G8B8A8_UInt;
                case VertexComponentType.Colour: return Format.R8G8B8A8_UNorm;
                case VertexComponentType.Dec3N: return Format.R10G10B10A2_UNorm;
                default: return Format.Unknown;
            }
        }

        public static InputElement[] GetLayout(VertexType componentsFlags, VertexDeclarationTypes componentsTypes = VertexDeclarationTypes.GTAV1)
        {
            List<InputElement> inputElements = new List<InputElement>();

            var types = (ulong)componentsTypes;
            var flags = (uint)componentsFlags;

            var offset = 0;

            for (int k = 0; k < 16; k++)
            {
                if (((flags >> k) & 0x1) == 1)
                {
                    var componentType = (VertexComponentType)((types >> k * 4) & 0x0000000F);

                    if (componentType == VertexComponentType.Nothing) continue; // should never hit this

                    var componentTypeSize = VertexComponentTypes.GetSizeInBytes(componentType);
                    var format = GetDXGIFormat(componentType);

                    if (componentTypeSize == 0 || format == Format.Unknown) continue;

                    int index = inputElements.Where(e => e.SemanticName.Equals(Semantics[k])).Count();
                    inputElements.Add(new InputElement(Semantics[k], index, format, offset, 0));

                    offset += componentTypeSize;
                }
            }

            return inputElements.ToArray();
        }
    }


    public struct VertexTypeDefault //id: 84500486, stride: 36, flags: 89, refs: 76099
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public Vector2 Texcoord;
    }

    public struct VertexTypeDefaultEx //id: 168386566, stride: 52, flags: 16473, refs: 32337
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public Vector2 Texcoord;
        public Vector4 Tangent;
    }

    public struct VertexTypePNCCT //id: 93937670, stride: 40, flags: 121, refs: 31413
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord;
    }

    public struct VertexTypePNCCTTTT //id: 1436115100, stride: 64, flags: 1017, refs: 28673
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
        public Vector2 Texcoord3;
    }

    public struct VertexTypePBBNCCTTX //id: 1520003478, stride: 72, flags: 16639, refs: 11178
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector4 Tangents;
    }

    public struct VertexTypePBBNCCT //id: 93940118, stride: 48, flags: 127, refs: 10396
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord;
    }

    public struct VertexTypePNCTTTX //id: 1510563852, stride: 68, flags: 16857, refs: 3688
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
        public Vector4 Tangents;
    }

    public struct VertexTypePNCTTTX_2 //id: 168413446, stride: 68, flags: 19545, refs: 72
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
        public Vector4 Tangents;
    }

    public struct VertexTypePNCTTTX_3 //id: 1510563990, stride: 68, flags: 17113, refs: 43
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
        public Vector4 Tangents;
    }

    public struct VertexTypePNCTTX //id: 1510563846, stride: 60, flags: 16601, refs: 2712
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector4 Tangents;
    }

    public struct VertexTypePNCCTTX //id: 1520001030, stride: 64, flags: 16633, refs: 2635
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector4 Tangents;
    }

    public struct VertexTypePNCCTTTX //id: 1520001174, stride: 72, flags: 17145, refs: 2238
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
        public Vector4 Tangents;
    }

    public struct VertexTypePBBNCCTX //id: 177826198, stride: 64, flags: 16511, refs: 1990
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord;
        public Vector4 Tangent;
    }

    public struct VertexTypePNCCTTX_2 //id: 177823894, stride: 64, flags: 17017, refs: 1800
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector4 Tangent;
    }

    public struct VertexTypePBBNCTX //id: 168389014, stride: 60, flags: 16479, refs: 1736
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
        public Vector2 Texcoord;
        public Vector4 Tangent;
    }

    public struct VertexTypePNCCTT //id: 1436114950, stride: 48, flags: 249, refs: 1704
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
    }

    public struct VertexTypePNCCTX //id: 177823750, stride: 56, flags: 16505, refs: 1338
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord;
        public Vector4 Tangent;
    }

    public struct VertexTypePCT //id: 84475910, stride: 24, flags: 81, refs: 102
    {
        public Vector3 Position;
        public uint Colour;
        public Vector2 Texcoord;
    }

    public struct VertexTypePT //id: 83886086, stride: 20, flags: 65, refs: 159 //water pools seem to use this
    {
        public Vector3 Position;
        public Vector2 Texcoord;
    }

    public struct VertexTypePTT //id: 1426063366, stride: 28, flags: 193, refs: 1 (skydome)
    {
        public Vector3 Position;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
    }

    public struct VertexTypePNC //id: 614406, stride: 28, flags: 25, refs: 380 // tunnel shadow casters seem use this
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
    }

    public struct VertexTypePBBNCT //id: 84502934, stride: 44, flags: 95, refs: 806
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
        public Vector2 Texcoord;
    }

    public struct VertexTypePC //id: 589830, stride: 16, flags: 17, refs: 405
    {
        public Vector3 Position;
        public uint Colour;
    }

    public struct VertexTypePCC //id: 2454, stride: 20, flags: 7, refs: 242
    {
        public Vector3 Position;
        public uint Colour0;
        public uint Colour1;
    }

    public struct VertexTypePNCTTTT //id: 1426677916, stride: 60, flags: 985, refs: 150
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
        public Vector2 Texcoord3;
    }

    public struct VertexTypePBBNCCTT //id: 1436117398, stride: 56, flags: 255, refs: 99
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour2;
        public uint Colour3;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
    }

    public struct VertexTypePCTT //id: 1426653190, stride: 32, flags: 209, refs: 79
    {
        public Vector3 Position;
        public uint Colour;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
    }

    public struct VertexTypePNCTTTTX //id: 1510590726, stride: 76, flags: 19673, refs: 3   //beach graffiti trees -  normal_spec_detail_dpm_vertdecal_tnt.sps
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
        public Vector2 Texcoord3;
        public Vector4 Tangent;
    }

    public struct VertexTypePBBCCT //id: 93915542, stride: 36, flags: 119, refs: 2
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public uint Colour0;
        public uint Colour1;
        public Vector2 Texcoord;
    }

    public struct VertexTypePBBNC //id: 616854, stride: 36, flags: 31, refs: 1
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
    }




    public struct VertexTypePCCH2H4 //id: 34185222, stride: 32, flags: 16473, types: 216172782140628998, refs: 2191 (yft only)  - frag cloth normalmapped
    {
        public Vector3 Position;
        public uint NormalPacked;// Vector3 Normal;
        public uint Colour;
        public ushort TexcoordX;// Vector2 Texcoord;
        public ushort TexcoordY;
        public ushort TangentX; // Vector4 Tangent;
        public ushort TangentY;
        public ushort TangentZ;
        public ushort TangentW;
    }

    public struct VertexTypePNCH2 //id: 17391622, stride: 32, flags: 89, types: 216172782140612614, refs: 1174 (yft only) - frag cloth default
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public ushort TexcoordX;// Vector2 Texcoord;
        public ushort TexcoordY;
    }



    public struct VertexTypePBBNCTT //id: 1426680214, stride: 52, flags: 223, types: 8598872888530528662, refs: 1470
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
    }

    public struct VertexTypePBBNCTTX //id: 1510566294, stride: 68, flags: 16607, types: 8598872888530528662, refs: 1478 (+9)
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector4 Tangent;
    }

    public struct VertexTypePBBNCTTT //id: 1426680220, stride: 60, flags: 479, types: 8598872888530528662, refs: 1290
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
    }

    public struct VertexTypePNCTT //id: 1426677766, stride: 44, flags: 217, types: 8598872888530528662, refs: 4434
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
    }

    public struct VertexTypePNCTTT //id: 1426677772, stride: 52, flags: 473, types: 8598872888530528662, refs: 65
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Colour;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
    }



    public struct VertexTypePBBNCTTTX //id: 1510566300, stride: 76, flags: 16863, types: 8598872888530528662, refs: 38
    {
        public Vector3 Position;
        public uint BlendWeights;
        public uint BlendIndices;
        public Vector3 Normal;
        public uint Colour0;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
        public Vector4 Tangent;
    }












    /*

    //full types output from all ydr/ydd files


    public struct VertexType89 //id: 84500486, stride: 36, flags: 89, types: 8598872888530528662, refs: 76099
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
    }

    public struct VertexType121 //id: 93937670, stride: 40, flags: 121, types: 8598872888530528662, refs: 31413
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public uint Component4;
       public Vector2 Component5;
    }

    public struct VertexType193 //id: 1426063366, stride: 28, flags: 193, types: 8598872888530528662, refs: 1
    {
       public Vector3 Component1;
       public Vector2 Component2;
       public Vector2 Component3;
    }

    public struct VertexType16473 //id: 168386566, stride: 52, flags: 16473, types: 8598872888530528662, refs: 32337
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector4 Component5;
    }

    public struct VertexType209 //id: 1426653190, stride: 32, flags: 209, types: 8598872888530528662, refs: 79
    {
       public Vector3 Component1;
       public uint Component2;
       public Vector2 Component3;
       public Vector2 Component4;
    }

    public struct VertexType95 //id: 84502934, stride: 44, flags: 95, types: 8598872888530528662, refs: 806
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public Vector2 Component6;
    }

    public struct VertexType17145 //id: 1520001174, stride: 72, flags: 17145, types: 8598872888530528662, refs: 2238
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public uint Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector2 Component7;
       public Vector4 Component8;
    }

    public struct VertexType16601 //id: 1510563846, stride: 60, flags: 16601, types: 8598872888530528662, refs: 2712
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector2 Component5;
       public Vector4 Component6;
    }

    public struct VertexType249 //id: 1436114950, stride: 48, flags: 249, types: 8598872888530528662, refs: 1704
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public uint Component4;
       public Vector2 Component5;
       public Vector2 Component6;
    }

    public struct VertexType16857 //id: 1510563852, stride: 68, flags: 16857, types: 8598872888530528662, refs: 3688
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector4 Component7;
    }

    public struct VertexType17017 //id: 177823894, stride: 64, flags: 17017, types: 8598872888530528662, refs: 1800
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public uint Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector4 Component7;
    }

    public struct VertexType16505 //id: 177823750, stride: 56, flags: 16505, types: 8598872888530528662, refs: 1338
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public uint Component4;
       public Vector2 Component5;
       public Vector4 Component6;
    }

    public struct VertexType16633 //id: 1520001030, stride: 64, flags: 16633, types: 8598872888530528662, refs: 2635
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public uint Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector4 Component7;
    }

    public struct VertexType16479 //id: 168389014, stride: 60, flags: 16479, types: 8598872888530528662, refs: 1736
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public Vector2 Component6;
       public Vector4 Component7;
    }

    public struct VertexType16511 //id: 177826198, stride: 64, flags: 16511, types: 8598872888530528662, refs: 1990
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public uint Component6;
       public Vector2 Component7;
       public Vector4 Component8;
    }

    public struct VertexType119 //id: 93915542, stride: 36, flags: 119, types: 8598872888530528662, refs: 2
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public uint Component4;
       public uint Component5;
       public Vector2 Component6;
    }

    public struct VertexType16607 //id: 1510566294, stride: 68, flags: 16607, types: 8598872888530528662, refs: 9
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public Vector2 Component6;
       public Vector2 Component7;
       public Vector4 Component8;
    }

    public struct VertexType25 //id: 614406, stride: 28, flags: 25, types: 8598872888530528662, refs: 380
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
    }

    public struct VertexType81 //id: 84475910, stride: 24, flags: 81, types: 8598872888530528662, refs: 102
    {
       public Vector3 Component1;
       public uint Component2;
       public Vector2 Component3;
    }

    public struct VertexType19545 //id: 168413446, stride: 68, flags: 19545, types: 8598872888530528662, refs: 72
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector4 Component7;
    }

    public struct VertexType19673 //id: 1510590726, stride: 76, flags: 19673, types: 8598872888530528662, refs: 3
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector2 Component7;
       public Vector4 Component8;
    }

    public struct VertexType17 //id: 589830, stride: 16, flags: 17, types: 8598872888530528662, refs: 405
    {
       public Vector3 Component1;
       public uint Component2;
    }

    public struct VertexType16639 //id: 1520003478, stride: 72, flags: 16639, types: 8598872888530528662, refs: 11178
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public uint Component6;
       public Vector2 Component7;
       public Vector2 Component8;
       public Vector4 Component9;
    }

    public struct VertexType127 //id: 93940118, stride: 48, flags: 127, types: 8598872888530528662, refs: 10396
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public uint Component6;
       public Vector2 Component7;
    }

    public struct VertexType65 //id: 83886086, stride: 20, flags: 65, types: 8598872888530528662, refs: 159
    {
       public Vector3 Component1;
       public Vector2 Component2;
    }

    public struct VertexType1017 //id: 1436115100, stride: 64, flags: 1017, types: 8598872888530528662, refs: 28673
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public uint Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector2 Component7;
       public Vector2 Component8;
    }

    public struct VertexType985 //id: 1426677916, stride: 60, flags: 985, types: 8598872888530528662, refs: 150
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector2 Component7;
    }

    public struct VertexType17113 //id: 1510563990, stride: 68, flags: 17113, types: 8598872888530528662, refs: 43
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector4 Component7;
    }

    public struct VertexType255 //id: 1436117398, stride: 56, flags: 255, types: 8598872888530528662, refs: 99
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public uint Component6;
       public Vector2 Component7;
       public Vector2 Component8;
    }

    public struct VertexType7 //id: 2454, stride: 20, flags: 7, types: 8598872888530528662, refs: 242
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
    }

    public struct VertexType31 //id: 616854, stride: 36, flags: 31, types: 8598872888530528662, refs: 1
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
    }




    */








    /*


    //full types output from all yft files
    
    public struct VertexType16639 //id: 1520003478, stride: 72, flags: 16639, types: 8598872888530528662, refs: 6
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public uint Component6;
       public Vector2 Component7;
       public Vector2 Component8;
       public Vector4 Component9;
    }

    public struct VertexType89 //id: 84500486, stride: 36, flags: 89, types: 8598872888530528662, refs: 2705
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
    }

    public struct VertexType95 //id: 84502934, stride: 44, flags: 95, types: 8598872888530528662, refs: 2662
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public Vector2 Component6;
    }

    public struct VertexType16473 //id: 34185222, stride: 32, flags: 16473, types: 216172782140628998, refs: 2191
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public ushort2 Component4;
       public ushort4 Component5;
    }

    public struct VertexType16473 //id: 168386566, stride: 52, flags: 16473, types: 8598872888530528662, refs: 2540
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector4 Component5;
    }

    public struct VertexType16601 //id: 1510563846, stride: 60, flags: 16601, types: 8598872888530528662, refs: 6422
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector2 Component5;
       public Vector4 Component6;
    }

    public struct VertexType16479 //id: 168389014, stride: 60, flags: 16479, types: 8598872888530528662, refs: 1749
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public Vector2 Component6;
       public Vector4 Component7;
    }

    public struct VertexType89 //id: 17391622, stride: 32, flags: 89, types: 216172782140612614, refs: 1174
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public ushort2 Component4;
    }

    public struct VertexType127 //id: 93940118, stride: 48, flags: 127, types: 8598872888530528662, refs: 1
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public uint Component6;
       public Vector2 Component7;
    }

    public struct VertexType121 //id: 93937670, stride: 40, flags: 121, types: 8598872888530528662, refs: 14
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public uint Component4;
       public Vector2 Component5;
    }

    public struct VertexType16633 //id: 1520001030, stride: 64, flags: 16633, types: 8598872888530528662, refs: 1
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public uint Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector4 Component7;
    }

    public struct VertexType16505 //id: 177823750, stride: 56, flags: 16505, types: 8598872888530528662, refs: 26
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public uint Component4;
       public Vector2 Component5;
       public Vector4 Component6;
    }

    public struct VertexType223 //id: 1426680214, stride: 52, flags: 223, types: 8598872888530528662, refs: 1470
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public Vector2 Component6;
       public Vector2 Component7;
    }

    public struct VertexType16607 //id: 1510566294, stride: 68, flags: 16607, types: 8598872888530528662, refs: 1478
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public Vector2 Component6;
       public Vector2 Component7;
       public Vector4 Component8;
    }

    public struct VertexType479 //id: 1426680220, stride: 60, flags: 479, types: 8598872888530528662, refs: 1290
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public Vector2 Component6;
       public Vector2 Component7;
       public Vector2 Component8;
    }

    public struct VertexType16511 //id: 177826198, stride: 64, flags: 16511, types: 8598872888530528662, refs: 1
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public uint Component6;
       public Vector2 Component7;
       public Vector4 Component8;
    }

    public struct VertexType25 //id: 614406, stride: 28, flags: 25, types: 8598872888530528662, refs: 8
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
    }

    public struct VertexType16857 //id: 1510563852, stride: 68, flags: 16857, types: 8598872888530528662, refs: 5
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector2 Component5;
       public Vector2 Component6;
       public Vector4 Component7;
    }

    public struct VertexType217 //id: 1426677766, stride: 44, flags: 217, types: 8598872888530528662, refs: 4434
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector2 Component5;
    }

    public struct VertexType473 //id: 1426677772, stride: 52, flags: 473, types: 8598872888530528662, refs: 65
    {
       public Vector3 Component1;
       public Vector3 Component2;
       public uint Component3;
       public Vector2 Component4;
       public Vector2 Component5;
       public Vector2 Component6;
    }



    //new (xmas 2017?)
    
    public struct VertexType16863 //id: 1510566300, stride: 76, flags: 16863, types: 8598872888530528662, refs: 38
    {
       public Vector3 Component1;
       public uint Component2;
       public uint Component3;
       public Vector3 Component4;
       public uint Component5;
       public Vector2 Component6;
       public Vector2 Component7;
       public Vector2 Component8;
       public Vector4 Component9;
    }





     */


}
