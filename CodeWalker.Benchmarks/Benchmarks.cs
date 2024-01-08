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
using Iced.Intel;
using SharpDX;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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

        [Params(1000)]
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

        private byte[] bytes = new byte[16];

        private string Str = "iakslgbhfibnrihbderpiugaehigoI BIHGVUIVDSOUFVBOUADGBOIUYfgiuywetrg872q13rh9872`134tgyihsbaopuJGUIYODGBFIOUFgvbouailksdbhnfp";

        private char[] chars1 = new char[0];

        private Vector3 WorldPos;

        [GlobalSetup]
        public void Setup()
        {
            random = new Random(42);

            valueByte = 0;
            valueUint = 0;

            Str = "";
            for (int i = 0; i < Length; i++)
            {
                if (random.Next(0, 10) <= 1)
                {
                    Str += random.Next('A', 'Z');
                } else
                {
                    Str += random.Next('a', 'z');
                }
            }

            chars1 = Str.ToCharArray();


                //float f0 = Flags0.Value / 255.0f;
                //float f1 = Flags1.Value / 255.0f;
                //float f2 = Flags2.Value / 255.0f;
                //var c = new Color4(f0, f1, f2, 1.0f);

            var c = new Color4(0.0f, 0.0f, 0.0f, 0.5f);

            // Shortcut
            Console.WriteLine(GetBytesReadable(1234));
            Console.WriteLine(GetBytesReadableNew(1234));

            Console.WriteLine(GetBytesReadable(100234));
            Console.WriteLine(GetBytesReadableNew(100234));

            Console.WriteLine(GetBytesReadable(long.MaxValue));
            Console.WriteLine(GetBytesReadableNew(long.MaxValue));

            WorldPos = new Vector3(random.NextFloat(float.MinValue, float.MaxValue), random.NextFloat(float.MinValue, float.MaxValue), random.NextFloat(float.MinValue, float.MaxValue));

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

        public static string GetBytesReadable(long i)
        {
            //shamelessly stolen from stackoverflow, and a bit mangled

            // Returns the human-readable file size for an arbitrary, 64-bit file size 
            // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
            // Get absolute value
            long absolute_i = Math.Abs(i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 bytes"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);

            string fmt = "0.### ";
            if (readable > 1000)
            {
                fmt = "0";
            }
            else if (readable > 100)
            {
                fmt = "0.#";
            }
            else if (readable > 10)
            {
                fmt = "0.##";
            }

            // Return formatted number with suffix
            return $"{readable.ToString(fmt)}{suffix}";
        }

        static string[] sizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

        public static string GetBytesReadableNew(long size)
        {
            //shamelessly stolen from stackoverflow, and a bit mangled

            // Returns the human-readable file size for an arbitrary, 64-bit file size 
            // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
            // Get absolute value
            Debug.Assert(sizeSuffixes.Length > 0);

            if (size == 0)
            {
                return $"{0:0.#} {sizeSuffixes[0]}";
            }

            var absSize = Math.Abs(size);
            var fpPower = Math.Log(absSize, 1024);
            var intPower = (int)fpPower;
            var normSize = absSize / Math.Pow(1024, intPower);

            return $"{normSize:G4} {sizeSuffixes[intPower]}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static byte ToLower(char c)
        {
            return ToLower((byte)c);
            //return (c >= 'A' && c <= 'Z') ? (byte)(c - 'A' + 'a') : (byte)c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static byte ToLower(byte c)
        {
            return ('A' <= c && c <= 'Z') ? (byte)(c | 0x20) : c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static byte ToLowerDirect(char c)
        {
            return (byte)(('A' <= c && c <= 'Z') ? (byte)(c | 0x20) : (byte)c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static uint GenHashLower(ReadOnlySpan<char> data)
        {
            uint h = 0;
            foreach (var b in data)
            {
                h += ToLower(b);
                h += h << 10;
                h ^= h >> 6;
            }
            h += h << 3;
            h ^= h >> 11;
            h += h << 15;

            return h;
        }

        [SkipLocalsInit]
        public static void WriteDataOneGo(Span<byte> data)
        {
            Unsafe.WriteUnaligned(ref data[0], new Int128(ulong.MaxValue, ulong.MaxValue));
        }

        [SkipLocalsInit]
        public static void WriteData(Span<byte> data)
        {
            Unsafe.WriteUnaligned<ulong>(ref data[0], ulong.MaxValue);
            Unsafe.WriteUnaligned<ulong>(ref data[8], ulong.MaxValue);
        }

        [SkipLocalsInit]
        public static void WriteDataOld(Span<byte> data)
        {
            Unsafe.WriteUnaligned<ulong>(ref MemoryMarshal.GetReference(data[..8]), ulong.MaxValue);
            Unsafe.WriteUnaligned<ulong>(ref MemoryMarshal.GetReference(data.Slice(8, 8)), ulong.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static uint GenHashLowerDirect(ReadOnlySpan<char> data)
        {
            uint h = 0;
            foreach (var b in data)
            {
                h += ToLowerDirect(b);
                h += h << 10;
                h ^= h >> 6;
            }
            h += h << 3;
            h ^= h >> 11;
            h += h << 15;

            return h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static uint GenHashLowerVectorized(Span<char> data)
        {
            Ascii.ToLowerInPlace(data, out _);

            uint h = 0;
            for (int i = 0; i < data.Length; i++)
            {
                h += (byte)data[i];
                h += h << 10;
                h ^= h >> 6;
            }
            h += h << 3;
            h ^= h >> 11;
            h += h << 15;

            return h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static uint GenHashLowerVectorized(ReadOnlySpan<char> data)
        {
            Span<char> chars = stackalloc char[data.Length];

            Ascii.ToLower(data, chars, out _);

            uint h = 0;
            foreach (var b in chars)
            {
                h += (byte)b;
                h += h << 10;
                h ^= h >> 6;
            }
            h += h << 3;
            h ^= h >> 11;
            h += h << 15;

            return h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static uint GenHashLower(string data)
        {
            uint h = 0;
            foreach (var b in data)
            {
                h += ToLower(b);
                h += h << 10;
                h ^= h >> 6;
            }
            h += h << 3;
            h ^= h >> 11;
            h += h << 15;

            return h;
        }

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
        [SkipLocalsInit]
        public void WriteData()
        {
            WriteData(bytes);
        }

        [Benchmark]
        [SkipLocalsInit]
        public void WriteDataOld()
        {
            WriteDataOld(bytes);
        }

        [Benchmark]
        [SkipLocalsInit]
        public void WriteDataOneGo()
        {
            WriteDataOneGo(bytes);
        }
    }
}
