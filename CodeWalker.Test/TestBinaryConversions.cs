using CodeWalker.GameFiles;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace CodeWalker.Test
{
    public static class TestData
    {
        public const string ReferenceDataBytes = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAwAAAAAAAADWA9YDAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAATABMAAAAAAAAAAAAAAAAACgAAAAAAAADZAdkBAAAAAAYAAAAAAAAAogGiAQAAAAABAAAAAAAAADsAOwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP3///8mAAAANgAAAEkAAAAAAAAAAAAAAAAAAAAAAAAAAACAQgAAgEIAAAAAAAAAAAAAAAAIQDQAAAAAAEgDSAMAAAAABwAAAAAAAAAGAAYAAAAAAAgAAAAAAAAANwA3AAAAAAAIwA0AAAAAAB4AHgAAAAAACEAVAAAAAAAOAA4AAAAAAAjAGAAAAAAACQAJAAAAAAAIABsAAAAAAAIAAgAAAAAACIAbAAAAAAABAAEAAAAAAA==";
        public static CScenarioPointRegion ReferenceData => new CScenarioPointRegion
        {
            AccelGrid = new rage__spdGrid2D
            {
                CellDimX = 64,
                CellDimY = 64,
                MaxCellX = 38,
                MaxCellY = 73,
                MinCellX = -3,
                MinCellY = 54,
            },
            ChainingGraph = new CodeWalker.GameFiles.CScenarioChainingGraph
            {
                Chains = new CodeWalker.GameFiles.Array_Structure
                {
                    Count1 = 59,
                    Count2 = 59,
                    Pointer = 1,
                },
                Edges = new CodeWalker.GameFiles.Array_Structure
                {
                    Count1 = 418,
                    Count2 = 418,
                    Pointer = 6,
                },
                Nodes = new CodeWalker.GameFiles.Array_Structure
                {
                    Count1 = 473,
                    Count2 = 473,
                    Pointer = 10,
                }
            },
            Clusters = new CodeWalker.GameFiles.Array_Structure
            {
                Count1 = 6,
                Count2 = 6,
                Pointer = 7,
            },
            EntityOverrides = new CodeWalker.GameFiles.Array_Structure
            {
                Count1 = 19,
                Count2 = 19,
                Pointer = 2,
            },
            LookUps = new CodeWalker.GameFiles.CScenarioPointLookUps
            {
                GroupNames = new CodeWalker.GameFiles.Array_uint
                {
                    Count1 = 9,
                    Count2 = 9,
                    Pointer = 1622024,
                },
                InteriorNames = new CodeWalker.GameFiles.Array_uint
                {
                    Count1 = 2,
                    Count2 = 2,
                    Pointer = 1769480,
                },
                PedModelSetNames = new CodeWalker.GameFiles.Array_uint
                {
                    Count1 = 30,
                    Count2 = 30,
                    Pointer = 901128,
                },
                RequiredIMapNames = new CodeWalker.GameFiles.Array_uint
                {
                    Count1 = 1,
                    Count2 = 1,
                    Pointer = 1802248,
                },
                TypeNames = new CodeWalker.GameFiles.Array_uint
                {
                    Count1 = 55,
                    Count2 = 55,
                    Pointer = 8,
                },
                VehicleModelSetNames = new CodeWalker.GameFiles.Array_uint
                {
                    Count1 = 14,
                    Count2 = 14,
                    Pointer = 1392648,
                },
            },
            Points = new CodeWalker.GameFiles.CScenarioPointContainer
            {
                LoadSavePoints = new CodeWalker.GameFiles.Array_Structure
                {
                    PointerDataIndex = 4294967295,
                },
                MyPoints = new CodeWalker.GameFiles.Array_Structure
                {
                    Count1 = 982,
                    Count2 = 982,
                    Pointer = 3,
                }
            },
            Unk_3844724227 = new CodeWalker.GameFiles.Array_ushort
            {
                Count1 = 840,
                Count2 = 840,
                Pointer = 3424264,
            }
        };
    }

    public class TestBinaryConversions
    {
        private readonly ITestOutputHelper _output;
        public TestBinaryConversions(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
        }
        [Fact]
        public void TestConvertData()
        {
            var data = BitConverter.GetBytes((uint)1234);
            var number = MetaTypes.ConvertData<uint>(data);
            if (number != (uint)1234)
            {
                throw new Exception("number 1234 converted back to itself doesn't equal 1234");
            }
        }

        public bool Equals(Array_Structure a, Array_Structure b)
        {
            return a.Count1 == b.Count1 && a.Count2 == b.Count2 && a.Pointer == b.Pointer;
        }

        public bool Equals(Array_uint a, Array_uint b)
        {
            return a.Count1 == b.Count1 && a.Count2 == b.Count2 && a.Pointer == b.Pointer;
        }

        public bool Equals(rage__spdGrid2D a, rage__spdGrid2D b)
        {
            return a.Dimensions == b.Dimensions && a.Max == b.Max && a.Min == b.Min;
        }

        [Fact]
        public void Test_Convert_Data_From_Bytes()
        {
            var bytes = Convert.FromBase64String(TestData.ReferenceDataBytes);

            var newData = MetaTypes.ConvertData<CScenarioPointRegion>(bytes);

            Assert.Equal(TestData.ReferenceData, newData);
        }

        [Fact]
        public void Test_Convert_Data_From_Bytes_Offset()
        {
            var bytes = Convert.FromBase64String(TestData.ReferenceDataBytes);
            bytes = new byte[4].Concat(bytes).ToArray();

            var newData = MetaTypes.ConvertData<CScenarioPointRegion>(bytes, 4);

            Assert.Equal(TestData.ReferenceData, newData);
        }

        [Fact]
        public void Test_Convert_To_Bytes()
        {
            var bytes = MetaTypes.ConvertToBytes(TestData.ReferenceData);

            var bytesBase64 = Convert.ToBase64String(bytes);

            Assert.Equal(TestData.ReferenceDataBytes, bytesBase64);
        }

        [Fact]
        public void Test_Convert_Data_From_And_To_Bytes()
        {
            var bytes = MetaTypes.ConvertToBytes(TestData.ReferenceData);

            var newData = MetaTypes.ConvertData<CScenarioPointRegion>(bytes);

            Assert.Equal(TestData.ReferenceData, newData);
        }

        private readonly SharpDX.Vector3[] referenceArray = new SharpDX.Vector3[] { new SharpDX.Vector3(12.5f, 23.0f, 65.0f), new SharpDX.Vector3(1234.234f, 17896.22f, 9845.12f), new SharpDX.Vector3(-12.3f, -82349.123f, 1234.9f), new SharpDX.Vector3(-23412.4f, 1289.2f, 948.0f) };
        private readonly string referenceArrayBytes = "AABIQQAAuEEAAIJCfUeaRHHQi0Z71BlGzcxEwZDWoMfNXJpEzei2xmYmoUQAAG1E";
        
        [Fact]
        public void Test_Convert_Array_To_Bytes()
        {
            var bytes = MetaTypes.ConvertArrayToBytes(referenceArray);

            var base64 = Convert.ToBase64String(bytes);

            Assert.Equal(referenceArrayBytes, base64);
        }

        [Fact]
        public void Test_Convert_Bytes_To_Array()
        {
            var bytes = Convert.FromBase64String(referenceArrayBytes);
            var arr = MetaTypes.ConvertDataArray<SharpDX.Vector3>(bytes, 0, referenceArray.Length).ToArray();
            Assert.Equal(referenceArray, arr);

            var newBytes = MetaTypes.ConvertArrayToBytes(arr);

            var base64 = Convert.ToBase64String(newBytes);

            Assert.Equal(referenceArrayBytes, base64);
        }
    }

    public class TestResourceData
    {
        [Fact]
        public void Test_Write_Struct()
        {
            using var systemStream = new MemoryStream();
            using var graphicsStream = new MemoryStream();

            using var dataWriter = new ResourceDataWriter(systemStream, graphicsStream);
            dataWriter.Position = 0x50000000;

            dataWriter.WriteStruct(TestData.ReferenceData);

            var buffer = systemStream.ToArray();

            var base64 = Convert.ToBase64String(buffer);

            Assert.Equal(TestData.ReferenceDataBytes, base64);
        }

        [Fact]
        public void Test_Read_Struct()
        {
            var buffer = Convert.FromBase64String(TestData.ReferenceDataBytes);
            using var systemStream = new MemoryStream(buffer);
            using var graphicsStream = new MemoryStream();

            using var dataReader = new ResourceDataReader(systemStream, graphicsStream);

            var region = dataReader.ReadStruct<CScenarioPointRegion>();

            Assert.Equal(TestData.ReferenceData, region);
        }

        [Fact]
        public void Test_Write_Single()
        {
            using var systemStream = new MemoryStream();
            using var graphicsStream = new MemoryStream();

            using var dataWriter = new ResourceDataWriter(systemStream, graphicsStream);
            dataWriter.Position = 0x50000000;

            dataWriter.Write(20.0f);

            var buffer = systemStream.ToArray();
            var result = BitConverter.ToSingle(buffer, 0);

            Assert.Equal(20.0f, result);
        }

        [Fact]
        public void Test_Write_Single_Big_Endian()
        {
            using var systemStream = new MemoryStream();
            using var graphicsStream = new MemoryStream();

            using var dataWriter = new ResourceDataWriter(systemStream, graphicsStream, Endianess.BigEndian);
            dataWriter.Position = 0x50000000;

            dataWriter.Write(20.0f);

            var buffer = systemStream.ToArray();

            var valueInt = BinaryPrimitives.ReadInt32BigEndian(buffer);
            var valueFloat = Unsafe.ReadUnaligned<float>(ref Unsafe.As<int, byte>(ref valueInt));
            //BinaryPrimitives.ReverseEndianness(20.0f)

            Assert.Equal(20.0f, valueFloat);

            dataWriter.Position = 0x50000000;

            dataWriter.Write(double.MaxValue);

            buffer = systemStream.ToArray();

            var valueLong = BinaryPrimitives.ReadInt64BigEndian(buffer);
            var valueDouble = Unsafe.ReadUnaligned<double>(ref Unsafe.As<long, byte>(ref valueLong));

            Assert.Equal(double.MaxValue, valueDouble);
        }

        [Fact]
        public void Test_Write_Multiple_Structs()
        {
            using var systemStream = new MemoryStream();
            using var graphicsStream = new MemoryStream();

            using var dataWriter = new ResourceDataWriter(systemStream, graphicsStream);
            dataWriter.Position = 0x50000000;

            dataWriter.WriteStructs(new[]
            {
                TestData.ReferenceData,
                TestData.ReferenceData,
                TestData.ReferenceData,
                TestData.ReferenceData
            });

            var buffer = systemStream.ToArray();

            var structs = MemoryMarshal.Cast<byte, CScenarioPointRegion>(buffer);

            Assert.Equal(4, structs.Length);

            foreach (var scenarioRegion in structs)
            {
                Assert.Equal(TestData.ReferenceData, scenarioRegion);
            }
        }

        [Fact]
        public void Test_Read_Multiple_Structs()
        {
            using var systemStream = new MemoryStream();
            using var graphicsStream = new MemoryStream();

            using var dataWriter = new ResourceDataWriter(systemStream, graphicsStream);
            dataWriter.Position = 0x50000000;

            dataWriter.WriteStructs(new[]
            {
                TestData.ReferenceData,
                TestData.ReferenceData,
                TestData.ReferenceData,
                TestData.ReferenceData
            });

            var buffer = systemStream.ToArray();
            systemStream.Position = 0;
            var dataReader = new ResourceDataReader(systemStream, graphicsStream);
            

            var structs = dataReader.ReadStructs<CScenarioPointRegion>(4);

            Assert.Equal(4, structs.Length);

            foreach (var scenarioRegion in structs)
            {
                Assert.Equal(TestData.ReferenceData, scenarioRegion);
            }

            structs = dataReader.ReadStructsAt<CScenarioPointRegion>(0 | 0x50000000, 4);

            foreach (var scenarioRegion in structs)
            {
                Assert.Equal(TestData.ReferenceData, scenarioRegion);
            }

            systemStream.Position = 0;
            dataWriter.Position = 0x50000000;

            dataWriter.Write(new byte[] { 0, 0, 0, 0 });

            dataWriter.WriteStructs(new[]
{
                TestData.ReferenceData,
                TestData.ReferenceData,
                TestData.ReferenceData,
                TestData.ReferenceData
            });

            structs = dataReader.ReadStructsAt<CScenarioPointRegion>(4 | 0x50000000, 4);

            foreach (var scenarioRegion in structs)
            {
                Assert.Equal(TestData.ReferenceData, scenarioRegion);
            }
        }

        //[Fact]
        //public void Test()
        //{
        //    var attachedObjects = MetaTypes.GetUintArray(meta, _Data.attachedObjects);
        //}

        private const string ArchetypeData = "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";

        private readonly CBaseArchetypeDef referenceArchetype = new CBaseArchetypeDef
        {
            assetName = new MetaHash((uint)0x1d0b5555),
            assetType = rage__fwArchetypeDef__eAssetType.ASSET_TYPE_DRAWABLE,
            bbMax = new SharpDX.Vector3(3.617807f, 4.9E-05f, 0.447415f),
            bbMin = new SharpDX.Vector3(-3.617834f, -5.1E-05f, -0.447416f),
            bsCentre = new SharpDX.Vector3(-1.3589859E-05f, -1.00000034E-06f, -5.066395E-07f),
            bsRadius = 3.645381f,
            hdTextureDist = 5,
            lodDist = 100,
            Unused05 = float.NaN,
            Unused06 = float.NaN,
            Unused07 = float.NaN,
            name = new MetaHash((uint)0x1d0b5555),
            textureDictionary = new MetaHash((uint)0x5e0cce25),
            flags = 0x00002000,
        };

        [Fact]
        public void TestConvertDataRaw()
        {
            var data = Convert.FromBase64String(ArchetypeData);
            var basearch = PsoTypes.ConvertDataRawOld<CBaseArchetypeDef>(data, 0);

            Assert.Equivalent(referenceArchetype, basearch);

            var basearch2 = PsoTypes.ConvertDataRaw<CBaseArchetypeDef>(data, 0);

            Assert.Equivalent(basearch, basearch2);
            
            var random = new Random();
            var length = random.Next(100);

            var newData = Enumerable.Empty<byte>().Concat(new byte[length]).Concat(data).ToArray();

            basearch = PsoTypes.ConvertDataRawOld<CBaseArchetypeDef>(newData, length);
            Assert.Equivalent(basearch, referenceArchetype);

            basearch2 = PsoTypes.ConvertDataRaw<CBaseArchetypeDef>(newData, length);

            Assert.Equivalent(basearch, basearch2);

            //a.Extensions = MetaTypes.GetExtensions(Meta, basearch.extensions);


        }
    }

    public class TestConvertDataArray
    {
        private readonly ITestOutputHelper _output;
        public TestConvertDataArray(ITestOutputHelper output)
        {
            _output = output;
        }
        private string GetFilePath(string filename)
        {
            // Directory we're looking for.
            var dirToFind = Path.Combine(@"CodeWalker.Test", "Files");

            // Search up directory tree starting at assembly path looking for 'Images' dir.
            var searchPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            while (true)
            {
                var testPath = Path.Combine(searchPath, dirToFind);
                if (Directory.Exists(testPath))
                {
                    // Found it!
                    return Path.Combine(testPath, filename);
                }

                // Move up one directory.
                var newSearchPath = Path.GetFullPath(Path.Combine(searchPath, ".."));
                if (newSearchPath == searchPath)
                {
                    // Didn't move up, so we're at the root.
                    throw new FileNotFoundException($"Could not find '{dirToFind}' directory.");
                }
                searchPath = newSearchPath;
            }
        }

        public string TestData = "AAAAAAAAAAAFAAAAAAAAAAUABQAAAAAAAAAAAAAAAAAAAKDAAACgwAAAAMAAAAAAAACgQAAAoEAAAABAAAAAAAAAgD9kViJCAAAAAGAAAAABAAAAAAAAAP////8AAAAABgAAAAAAAAABAAEAAAAAAAAAAAAAAAAABWAAAAAAAAAKAAoAAAAAAAAAAAAAAAAAgPEnwiB/QcIAcFzBAAAAAADgDsL9ke/BHKgswQAAAAAAAIA/yyTUkwAAAABgAAAAAQAAAAEAAAD/////AAAAAAYAAQAAAAAALgAuAAAAAAA=";
        [Fact]
        public void ConvertDataArrayShouldReturnSameAsConvertData()
        {
            var data = Convert.FromBase64String(TestData);

            var referenceData = MetaTypes.ConvertData<CMloRoomDef>(data);

            var arrayData = MetaTypes.ConvertDataArray<CMloRoomDef>(data, 0, 2);

            var path = GetFilePath("anwblokaal.ytyp");
            var bytes = File.ReadAllBytes(path);
            var entry = RpfFile.CreateFileEntry("anwblokaal.ytyp", path, ref bytes);
            var ytypFile = RpfFile.GetFile<YtypFile>(entry, bytes);

            var rooms = ytypFile.AllArchetypes
                .Where(p => p is MloArchetype)
                .Select(p => p as MloArchetype)
                .SelectMany(p => p.rooms)
                .Select(p => p.Data);

            var secondRoom = rooms.Last();

            var arr = new byte[Marshal.SizeOf<CMloRoomDef>() * rooms.Count()];
            //MemoryMarshal.Write(arr.AsSpan(), ref rooms);

            _output.WriteLine(Convert.ToBase64String(MemoryMarshal.AsBytes(rooms.ToArray().AsSpan())));

            // I know the bbMax values don't really make sense here, seems to be a weird ytyp, but it's mostly about testing the serialization, not about using a 100% correct ytyp
            // And didn't want to include a ytyp from Rockstar Games since that could cause some legal issues down the road
            // Another possible solution would be to do abest attempt to find the GTA V installed on the user's computer running the tests and use files from there, but this would add complexity and links which could break tests
            // However this would be more in the scope of implementation testing as it tests the whole chain including encryption which is not wanted here
            Assert.Equivalent(new SharpDX.Vector3(-41.98584f, -48.3741455f, -13.7773438f), secondRoom.bbMin);
            Assert.Equivalent(new SharpDX.Vector3(-35.71875f, -29.9462833f, -10.7910423f), secondRoom.bbMax);
            Assert.Equivalent(new Array_uint(65542, 46), secondRoom.attachedObjects);
            Assert.Equal(96u, secondRoom.flags);
            Assert.Equal(1, secondRoom.floorId);
            Assert.Equivalent(new CharPointer() { Count1 = 10, Count2 = 10, Pointer = 24581 }, secondRoom.name);
            Assert.Equal(1u, secondRoom.portalCount);
            Assert.Equal(2480153803, secondRoom.timecycleName.Hash);

            // First room is limbo, so this check isn't that useful, but just make sure, and the following check will compare the entire array anyways
            Assert.Equivalent(referenceData, rooms.First());

            Assert.Equivalent(arrayData.ToArray(), rooms);
        }
    }
}