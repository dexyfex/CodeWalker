using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using CodeWalker.Core.Utils;
using CodeWalker.GameFiles;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace CodeWalker.Benchmarks
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        public const string markup = @"<CVehicleModelInfo__InitDataList>
  <residentTxd>vehshare</residentTxd>
  <residentAnims />
  <InitDatas>
    <Item>
      <modelName>brabusgt600</modelName><txdName>brabusgt600</txdName>
      <handlingId>brabusgt600</handlingId>
      <gameName>GT 600</gameName>
      <vehicleMakeName>BRABUS</vehicleMakeName>
      <expressionDictName>null</expressionDictName>
      <expressionName>null</expressionName>
      <animConvRoofDictName>null</animConvRoofDictName>
      <animConvRoofName>null</animConvRoofName>
      <animConvRoofWindowsAffected />
      <ptfxAssetName>null</ptfxAssetName>
      <audioNameHash>ta176m177</audioNameHash>
      <layout>LAYOUT_LOW</layout>
      <coverBoundOffsets>BULLET_COVER_OFFSET_INFO</coverBoundOffsets>
      <explosionInfo>EXPLOSION_INFO_DEFAULT</explosionInfo>
      <scenarioLayout />
      <cameraName>DEFAULT_FOLLOW_VEHICLE_CAMERA</cameraName>
      <aimCameraName>MID_BOX_VEHICLE_AIM_CAMERA</aimCameraName>
      <bonnetCameraName>VEHICLE_BONNET_CAMERA_NEAR_EXTRA_HIGH</bonnetCameraName>
      <povCameraName>DEFAULT_POV_CAMERA</povCameraName>
      <FirstPersonDriveByIKOffset x=""0.000000"" y=""-0.060000"" z=""-0.005000"" />
      <FirstPersonDriveByUnarmedIKOffset x=""0.000000"" y=""-0.150000"" z=""0.000000"" />
	  <FirstPersonProjectileDriveByIKOffset x=""0.025000"" y=""-0.100000"" z=""0.050000"" />
	  <FirstPersonProjectileDriveByPassengerIKOffset x=""-0.025000"" y=""-0.100000"" z=""0.050000"" />
	  <FirstPersonDriveByLeftPassengerIKOffset x=""0.000000"" y=""0.000000"" z=""0.000000"" />
	  <FirstPersonDriveByRightPassengerIKOffset x=""0.000000"" y=""-0.060000"" z=""-0.005000"" />
	  <FirstPersonDriveByLeftPassengerUnarmedIKOffset x=""0.000000"" y=""0.000000"" z=""0.000000"" />
	  <FirstPersonDriveByRightPassengerUnarmedIKOffset x=""0.000000"" y=""-0.150000"" z=""0.000000"" />
	  <FirstPersonMobilePhoneOffset x=""0.125000"" y=""0.175000"" z=""0.513000"" />
      <FirstPersonPassengerMobilePhoneOffset x=""0.136000"" y=""0.123000"" z=""0.425000"" />
      <PovCameraOffset x=""0.000000"" y=""-0.200000"" z=""0.615000"" />
      <PovCameraVerticalAdjustmentForRollCage value=""0.000000"" />
      <PovPassengerCameraOffset x=""0.000000"" y=""0.000000"" z=""0.000000"" />
      <vfxInfoName>VFXVEHICLEINFO_CAR_BULLET</vfxInfoName>
      <shouldUseCinematicViewMode value=""true"" />
      <shouldCameraTransitionOnClimbUpDown value=""false"" />
      <shouldCameraIgnoreExiting value=""false"" />
      <AllowPretendOccupants value=""true"" />
      <AllowJoyriding value=""true"" />
      <AllowSundayDriving value=""true"" />
      <AllowBodyColorMapping value=""true"" />
      <wheelScale value=""0.293500"" />
      <wheelScaleRear value=""0.293500"" />
      <dirtLevelMin value=""0.000000"" />
      <dirtLevelMax value=""0.450000"" />
      <envEffScaleMin value=""0.000000"" />
      <envEffScaleMax value=""1.000000"" />
      <envEffScaleMin2 value=""0.000000"" />
      <envEffScaleMax2 value=""1.000000"" />
      <damageMapScale value=""0.600000"" />
      <damageOffsetScale value=""0.400000"" />
      <diffuseTint value=""0xAA0A0A0A"" />
      <steerWheelMult value=""1.000000"" />
      <HDTextureDist value=""5.000000"" />
      <lodDistances content=""float_array"">
        60.000000
        80.000000
        100.000000
        120.000000
        500.000000
        500.000000
      </lodDistances>
      <minSeatHeight value=""0.813"" />
      <identicalModelSpawnDistance value=""80"" />
      <maxNumOfSameColor value=""1"" />
      <defaultBodyHealth value=""1000.000000"" />
      <pretendOccupantsScale value=""1.000000"" />
      <visibleSpawnDistScale value=""1.000000"" />
      <trackerPathWidth value=""2.000000"" />
      <weaponForceMult value=""1.000000"" />
      <frequency value=""10"" />
      <swankness>SWANKNESS_3</swankness>
      <maxNum value=""1"" />
      <flags>FLAG_SPORTS FLAG_RICH_CAR FLAG_NO_BROKEN_DOWN_SCENARIO FLAG_RECESSED_TAILLIGHT_CORONAS FLAG_NO_HEAVY_BRAKE_ANIMATION</flags>
      <type>VEHICLE_TYPE_CAR</type>
      <plateType>VPT_FRONT_AND_BACK_PLATES</plateType>
      <dashboardType>VDT_BANSHEE</dashboardType>
      <vehicleClass>VC_SPORT</vehicleClass>
      <wheelType>VWT_HIEND</wheelType>
      <trailers>
                <Item>docktrailer</Item>
        <Item>trailers</Item>
        <Item>trailers2</Item>
        <Item>trailers3</Item>
        <Item>trailers4</Item>
        <Item>tanker</Item>
        <Item>trailerlogs</Item>
        <Item>tr2</Item>
        <Item>trflat</Item>
      </trailers>
      <additionalTrailers />
      <drivers>
        <Item>
		  <driverName>S_M_Y_Cop_01</driverName>
		  <npcName/>
		</Item>
      </drivers>
      <extraIncludes />
      <doorsWithCollisionWhenClosed />
      <driveableDoors />
      <bumpersNeedToCollideWithMap value=""false"" />
      <needsRopeTexture value=""false"" />
      <requiredExtras />
      <rewards />
      <cinematicPartCamera>
        <Item>WHEEL_FRONT_RIGHT_CAMERA</Item>
        <Item>WHEEL_FRONT_LEFT_CAMERA</Item>
        <Item>WHEEL_REAR_RIGHT_CAMERA</Item>
        <Item>WHEEL_REAR_LEFT_CAMERA</Item>
      </cinematicPartCamera>
      <NmBraceOverrideSet />
      <buoyancySphereOffset x=""0.000000"" y=""0.000000"" z=""0.000000"" />
      <buoyancySphereSizeScale value=""1.000000"" />
      <pOverrideRagdollThreshold type=""CVehicleModelInfo__CVehicleOverrideRagdollThreshold"">
        <MinComponent value=""22"" />
        <MaxComponent value=""22"" />
        <ThresholdMult value=""1.500000"" />
      </pOverrideRagdollThreshold>
      <firstPersonDrivebyData>
        <Item>LOW_BULLET_FRONT_LEFT</Item>
        <Item>LOW_BULLET_FRONT_RIGHT</Item>
      </firstPersonDrivebyData>
    </Item>
	










  </InitDatas>
  <txdRelationships>
       <Item>
      <parent>vehicles_banshee_interior</parent>
      <child>brabusgt600</child>
    </Item>



	
  </txdRelationships>
</CVehicleModelInfo__InitDataList>";

        private byte[] data;
        private RpfFileEntry fileEntry;

        [GlobalSetup]
        public void Setup()
        {
            data = new byte[2048];
            var random = new Random(42);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)random.Next(byte.MinValue, byte.MaxValue);
            }
            GTA5Keys.LoadFromPath("C:\\Program Files\\Rockstar Games\\Grand Theft Auto V", "");
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

        [Benchmark]
        public void DecryptNGSpan()
        {
            GTACrypto.DecryptNG(data.AsSpan(), "kaas", 2048);
        }

        [Benchmark]
        public void DecryptNG()
        {
            GTACrypto.DecryptNG(data, "kaas", 2048);
        }
    }
}
