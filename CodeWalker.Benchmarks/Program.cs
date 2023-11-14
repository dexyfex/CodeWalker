using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace CodeWalker.Benchmarks
{
    internal class Program
    {


        static void Main(string[] args)
        {
            //var benchmarks = new Benchmarks();

            //benchmarks.Setup();
            //benchmarks.ToUInt64();
            //benchmarks.ReadUInt64LittleEndian();
            //benchmarks.ReadUInt64BigEndian();
            //benchmarks.ToUIntBigEndian();
            //benchmarks.HtmlEncode();

            //benchmarks.GlobalCleanup();

            //ParseBuffer();

            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
