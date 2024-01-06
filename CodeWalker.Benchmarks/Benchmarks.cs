using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Disassemblers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using CodeWalker.Core.Utils;
using CodeWalker.GameFiles;
using Collections.Pooled;
using CommunityToolkit.HighPerformance;
using SharpDX;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CodeWalker.Benchmarks
{
    //[InProcess]
    [ShortRunJob]
    [MemoryDiagnoser]
    public class Benchmarks
    {
        //private class Config : ManualConfig
        //{
        //    public Config()
        //    {
        //        AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig(true)));
        //        AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig()));

                
        //    }
        //}
        private static string markup = "<?xml version=\"1.0\"?>\r\n<vectors><Item x=\"\"></Item></vectors>";
        private List<SimpleType3> listClass;
        private List<BigStruct> listStruct;
        private PooledList<SimpleType3> pooledListClass;
        private PooledList<BigStruct> pooledListStruct;

        public class SimpleType
        {
            public int Value1;
        }

        public class SimpleType2 : SimpleType
        {
            public int Value2;
        }

        public class SimpleType3
        {
            public long Value1;
            public long Value2;
            public long Value3;

            public SimpleType3()
            {
                Value1 = random.Next();
                Value2 = random.Next();
                Value3 = random.Next();
            }
        }

        private SimpleType[] intArr;
        private List<SimpleType> intList;


        private static Random random = new Random(42);

        public struct BigStruct
        {
            public long Value1;
            public long Value2;
            public long Value3;
            public long Value4;
            public long Value5;
            public long Value6;
            public long Value7;
            public long Value8;
            public BigStruct()
            {
                Value1 = random.Next();
                Value2 = random.Next();
                Value3 = random.Next();
                Value4 = random.Next();
                Value5 = random.Next();
                Value6 = random.Next();
                Value7 = random.Next();
                Value8 = random.Next();
            }

            public BigStruct(long value1, long value2, long value3, long value4, long value5, long value6, long value7, long value8)
            {
                Value1 = value1;
                Value2 = value2;
                Value3 = value3;
                Value4 = value4;
                Value5 = value5;
                Value6 = value6;
                Value7 = value7;
                Value8 = value8;
            }

            public static BigStruct operator +(in BigStruct left, in BigStruct right)
            {
                return new BigStruct(
                    left.Value1 + right.Value1,
                    left.Value2 + right.Value2,
                    left.Value3 + right.Value3,
                    left.Value4 + right.Value4,
                    left.Value5 + right.Value5,
                    left.Value6 + right.Value6,
                    left.Value7 + right.Value7,
                    left.Value8 + right.Value8
                );
            }
        }

        [Params(10, 100, 1000)]
        public int Length { get; set; } = 10000;

        [Params(100)]
        public int Chance { get; set; } = 25;

        public ArrayOfChars64 chars64;

        private BigStruct[] vectors = new BigStruct[0];
        private BigStruct vector;

        private byte valueByte;
        private uint valueUint;

        private uint[] data;

        private MetaName[] Values;

        private int randomValue;

        private uint[] ushorts = new uint[0];

        private string Str = "iakslgbhfibnrihbderpiugaehigoI BIHGVUIVDSOUFVBOUADGBOIUYfgiuywetrg872q13rh9872`134tgyihsbaopuJGUIYODGBFIOUFgvbouailksdbhnfp";

        [GlobalSetup]
        public void Setup()
        {
            random = new Random(42);

            valueByte = 0;
            valueUint = 0;

            ushorts = new uint[Length];

            for (int i = 0; i < Length; i++)
            {
                ushorts[i] = (uint)random.Next(0, int.MaxValue);
            }

            var hashes = MemoryMarshal.Cast<uint, MetaHash>(ushorts);

            for (int i = 0; i < Length; i++)
            {
                Console.WriteLine($"{ushorts[i]} -> {hashes[i]}");
            }

            //Console.WriteLine("Setup done");

            //XElement? result = null;
            //var _doc = new XmlDocument();
            //_doc.LoadXml(markup);

            //var doc = XDocument.Load(new XmlNodeReader(_doc));
            //Console.WriteLine(doc.Root);

            //data = new byte[2048];
            //var random = new Random(42);
            //for (int i = 0; i < data.Length; i++)
            //{
            //    data[i] = (byte)random.Next(byte.MinValue, byte.MaxValue);
            //}
            //GTA5Keys.LoadFromPath("C:\\Program Files\\Rockstar Games\\Grand Theft Auto V", "");

            //rotation = new Quaternion(random.NextFloat(-1.0f, 1.0f), random.NextFloat(-1.0f, 1.0f), random.NextFloat(-1.0f, 1.0f), random.NextFloat(-1.0f, 1.0f));
            //translation = new Vector3(random.NextFloat(-1.0f, 1.0f), random.NextFloat(-1.0f, 1.0f), random.NextFloat(-1.0f, 1.0f));
            //scale = random.NextFloat(-1.0f, 1.0f);
            //matrix = Matrix.AffineTransformation(scale, rotation, translation);
        }

        //[Benchmark(Baseline = true)]
        //public void RunLoad()
        //{
        //    var vehiclesFileExpected = new VehiclesFile();
        //    vehiclesFileExpected.LoadOld(data, fileEntry);
        //}

        //[Benchmark]
        //public void RunLoadNew()
        //{
        //    var vehiclesFile = new VehiclesFile();
        //    vehiclesFile.Load(data, fileEntry);
        //}

        //[Benchmark(Baseline = true)]
        //public uint SwapBytes()
        //{
        //    var result = test;
        //    for (int i = 0; i < 1000; i++)
        //    {
        //        result = MetaTypes.SwapBytes(result);
        //    }
        //    return result;
        //}

        //[Benchmark]
        //public uint ReverseEndianness()
        //{
        //    var result = test;
        //    for (int i = 0; i < 1000; i++)
        //    {
        //        result = BinaryPrimitives.ReverseEndianness(result);
        //    }

        //    return result;
        //}

        //[Benchmark]
        //public int IndexOf()
        //{
        //    return Str.IndexOf('\0');
        //}

        //[Benchmark]
        //public int IndexOfAsSpan()
        //{
        //    return Str.AsSpan().IndexOf('\0');
        //}

        private PooledList<SimpleType3> getPooledListClass()
        {
            var list = new PooledList<SimpleType3>();
            for (int i = 0; i < Length; i++)
            {
                list.Add(new SimpleType3());
            }

            return list;
        }

        private PooledList<BigStruct> getPooledListStruct()
        {
            var list = new PooledList<BigStruct>();
            for (int i = 0; i < Length; i++)
            {
                list.Add(new BigStruct());
            }

            return list;
        }

        private List<SimpleType3> getListClass()
        {
            var list = new List<SimpleType3>();
            for (int i = 0; i < Length; i++)
            {
                list.Add(new SimpleType3());
            }

            return list;
        }

        private List<BigStruct> getListStruct()
        {
            var list = new List<BigStruct>();
            for (int i = 0; i < Length; i++)
            {
                list.Add(new BigStruct());
            }

            return list;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void inVectorOperator()
        {
            var vect = Vector256.Create<uint>(data.AsSpan());
            var shiftVect =  Vector256.Create<uint>(10);
            var leftShiftVect = Vector256.Create<uint>(6);


        }

        private uint joaatLower(Span<char> span)
        {
            uint h = 0;
            for (int i = 0; i < span.Length; i++)
            {
                h += toLower(span[i]);
                h += h << 10;
                h ^= h >> 6;
            }

            return h;
        }

        private uint joaat(Span<char> span)
        {
            uint h = 0;
            for (int i = 0; i < span.Length; i++)
            {
                h += span[i];
                h += h << 10;
                h ^= h >> 6;
            }

            return h;
        }

        private void toLowerVectorized(Span<char> span)
        {
            Ascii.ToLowerInPlace(span, out _);
        }

        private char toLower(char c)
        {
            return ('A' <= c && c <= 'Z') ? (char)(c | 0x20) : c;
        }

        private void toLowerPleb(Span<char> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = toLower(span[i]);
            }
        }



        [Benchmark]
        public void ReverseEndianness()
        {
            //BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<float, uint>(ushorts), MemoryMarshal.Cast<float, uint>(ushorts));
        }

        [Benchmark]
        public void SwapBytes()
        {
            var _ushorts = ushorts;
            for (int i = 0; i < _ushorts.Length; i++)
            {
                _ushorts[i] = MetaTypes.SwapBytes(_ushorts[i]);
            }
        }
    }
}
