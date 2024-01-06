using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace CodeWalker.Test
{
    public class XmlTests
    {
        private readonly ITestOutputHelper output;
        public XmlTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        public static string markup = @"<CVehicleModelInfo__InitDataList>
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

        public static string pedsList = @"﻿<?xml version=""1.0"" encoding=""UTF-8""?>
<CPedModelInfo__InitDataList>
  <residentTxd>comp_peds_generic</residentTxd>
  <residentAnims />
  <InitDatas>
    <Item>
      <Name>CS_LesterCrest_2</Name>
      <PropsName>CS_LesterCrest_2_p</PropsName>
      <ClipDictionaryName>move_m@generic</ClipDictionaryName>
      <BlendShapeFileName>null</BlendShapeFileName>
      <ExpressionSetName />
      <ExpressionDictionaryName>CS_LesterCrest</ExpressionDictionaryName>
      <ExpressionName>CS_LesterCrest</ExpressionName>
      <Pedtype>CIVMALE</Pedtype>
      <MovementClipSet>move_m@generic</MovementClipSet>
      <StrafeClipSet>move_ped_strafing</StrafeClipSet>
      <MovementToStrafeClipSet>move_ped_to_strafe</MovementToStrafeClipSet>
      <InjuredStrafeClipSet>move_strafe_injured</InjuredStrafeClipSet>
      <FullBodyDamageClipSet>dam_ko</FullBodyDamageClipSet>
      <AdditiveDamageClipSet>dam_ad</AdditiveDamageClipSet>
      <DefaultGestureClipSet>ANIM_GROUP_GESTURE_M_GENERIC</DefaultGestureClipSet>
      <FacialClipsetGroupName>facial_clipset_group_gen_male</FacialClipsetGroupName>
      <DefaultVisemeClipSet>ANIM_GROUP_VISEMES_M_HI</DefaultVisemeClipSet>
      <SidestepClipSet>CLIP_SET_ID_INVALID</SidestepClipSet>
      <PoseMatcherName>Male</PoseMatcherName>
      <PoseMatcherProneName>Male_prone</PoseMatcherProneName>
      <GetupSetHash>NMBS_SLOW_GETUPS</GetupSetHash>
      <CreatureMetadataName>3Lateral_Facial</CreatureMetadataName>
      <DecisionMakerName>DEFAULT</DecisionMakerName>
      <MotionTaskDataSetName>STANDARD_PED</MotionTaskDataSetName>
      <DefaultTaskDataSetName>STANDARD_PED</DefaultTaskDataSetName>
      <PedCapsuleName>STANDARD_MALE</PedCapsuleName>
      <PedLayoutName />
      <PedComponentSetName />
      <PedComponentClothName />
      <PedIKSettingsName>NO_IK</PedIKSettingsName>
      <TaskDataName />
      <IsStreamedGfx value=""true"" />
      <AmbulanceShouldRespondTo value=""true"" />
      <CanRideBikeWithNoHelmet value=""false"" />
      <CanSpawnInCar value=""true"" />
      <IsHeadBlendPed value=""false"" />
      <bOnlyBulkyItemVariations value=""false"" />
      <RelationshipGroup>CIVMALE</RelationshipGroup>
      <NavCapabilitiesName>STANDARD_PED</NavCapabilitiesName>
      <PerceptionInfo>DEFAULT_PERCEPTION</PerceptionInfo>
      <DefaultBrawlingStyle>BS_AI</DefaultBrawlingStyle>
      <DefaultUnarmedWeapon>WEAPON_UNARMED</DefaultUnarmedWeapon>
      <Personality>Streamed_Male</Personality>
      <CombatInfo>DEFAULT</CombatInfo>
      <VfxInfoName>VFXPEDINFO_HUMAN_GENERIC</VfxInfoName>
      <AmbientClipsForFlee>FLEE</AmbientClipsForFlee>
      <Radio1>RADIO_GENRE_OFF</Radio1>
      <Radio2>RADIO_GENRE_OFF</Radio2>
      <FUpOffset value=""0.000000"" />
      <RUpOffset value=""0.000000"" />
      <FFrontOffset value=""0.000000"" />
      <RFrontOffset value=""0.147000"" />
      <MinActivationImpulse value=""20.000000"" />
      <Stubble value=""0.000000"" />
      <HDDist value=""0.000000"" />
      <TargetingThreatModifier value=""1.000000"" />
      <KilledPerceptionRangeModifer value=""-1.000000"" />
      <Sexiness />
      <Age value=""0"" />
      <MaxPassengersInCar value=""0"" />
      <ExternallyDrivenDOFs />
      <PedVoiceGroup>SILENT_CUTSCENE_PVG</PedVoiceGroup>
      <AnimalAudioObject />
      <AbilityType>SAT_NONE</AbilityType>
      <ThermalBehaviour>TB_WARM</ThermalBehaviour>
      <SuperlodType>SLOD_HUMAN</SuperlodType>
      <ScenarioPopStreamingSlot>SCENARIO_POP_STREAMING_NORMAL</ScenarioPopStreamingSlot>
      <DefaultSpawningPreference>DSP_NORMAL</DefaultSpawningPreference>
      <DefaultRemoveRangeMultiplier value=""1.000000"" />
      <AllowCloseSpawning value=""false"" />
    </Item>
    <Item>
      <Name>IG_LesterCrest_2</Name>
      <PropsName>IG_LesterCrest_2_p</PropsName>
      <ClipDictionaryName>move_characters@lester@STD_CaneUp</ClipDictionaryName>
      <BlendShapeFileName>null</BlendShapeFileName>
      <ExpressionSetName>expr_set_ambient_male</ExpressionSetName>
      <ExpressionDictionaryName>null</ExpressionDictionaryName>
      <ExpressionName>null</ExpressionName>
      <Pedtype>CIVMALE</Pedtype>
      <MovementClipSet>move_lester_CaneUp</MovementClipSet>
      <StrafeClipSet>move_ped_strafing</StrafeClipSet>
      <MovementToStrafeClipSet>move_ped_to_strafe</MovementToStrafeClipSet>
      <InjuredStrafeClipSet>move_strafe_injured</InjuredStrafeClipSet>
      <FullBodyDamageClipSet>dam_ko</FullBodyDamageClipSet>
      <AdditiveDamageClipSet>dam_ad</AdditiveDamageClipSet>
      <DefaultGestureClipSet>ANIM_GROUP_GESTURE_M_GENERIC</DefaultGestureClipSet>
      <FacialClipsetGroupName>facial_clipset_group_gen_male</FacialClipsetGroupName>
      <DefaultVisemeClipSet>ANIM_GROUP_VISEMES_M_LO</DefaultVisemeClipSet>
      <SidestepClipSet>CLIP_SET_ID_INVALID</SidestepClipSet>
      <PoseMatcherName>Male</PoseMatcherName>
      <PoseMatcherProneName>Male_prone</PoseMatcherProneName>
      <GetupSetHash>NMBS_SLOW_GETUPS</GetupSetHash>
      <CreatureMetadataName>ambientPed_upperWrinkles</CreatureMetadataName>
      <DecisionMakerName>DEFAULT</DecisionMakerName>
      <MotionTaskDataSetName>STANDARD_PED</MotionTaskDataSetName>
      <DefaultTaskDataSetName>STANDARD_PED</DefaultTaskDataSetName>
      <PedCapsuleName>STANDARD_MALE</PedCapsuleName>
      <PedLayoutName />
      <PedComponentSetName />
      <PedComponentClothName />
      <PedIKSettingsName />
      <TaskDataName />
      <IsStreamedGfx value=""false"" />
      <AmbulanceShouldRespondTo value=""true"" />
      <CanRideBikeWithNoHelmet value=""false"" />
      <CanSpawnInCar value=""true"" />
      <IsHeadBlendPed value=""false"" />
      <bOnlyBulkyItemVariations value=""false"" />
      <RelationshipGroup>CIVMALE</RelationshipGroup>
      <NavCapabilitiesName>STANDARD_PED</NavCapabilitiesName>
      <PerceptionInfo>DEFAULT_PERCEPTION</PerceptionInfo>
      <DefaultBrawlingStyle>BS_AI</DefaultBrawlingStyle>
      <DefaultUnarmedWeapon>WEAPON_UNARMED</DefaultUnarmedWeapon>
      <Personality>Streamed_Male</Personality>
      <CombatInfo>DEFAULT</CombatInfo>
      <VfxInfoName>VFXPEDINFO_HUMAN_GENERIC</VfxInfoName>
      <AmbientClipsForFlee>FLEE</AmbientClipsForFlee>
      <Radio1>RADIO_GENRE_REGGAE</Radio1>
      <Radio2>RADIO_GENRE_SURF</Radio2>
      <FUpOffset value=""0.000000"" />
      <RUpOffset value=""0.000000"" />
      <FFrontOffset value=""0.000000"" />
      <RFrontOffset value=""0.147000"" />
      <MinActivationImpulse value=""20.000000"" />
      <Stubble value=""0.000000"" />
      <HDDist value=""0.000000"" />
      <TargetingThreatModifier value=""1.000000"" />
      <KilledPerceptionRangeModifer value=""-1.000000"" />
      <Sexiness />
      <Age value=""0"" />
      <MaxPassengersInCar value=""0"" />
      <ExternallyDrivenDOFs />
      <PedVoiceGroup>LESTER_PVG</PedVoiceGroup>
      <AnimalAudioObject />
      <AbilityType>SAT_NONE</AbilityType>
      <ThermalBehaviour>TB_WARM</ThermalBehaviour>
      <SuperlodType>SLOD_HUMAN</SuperlodType>
      <ScenarioPopStreamingSlot>SCENARIO_POP_STREAMING_NORMAL</ScenarioPopStreamingSlot>
      <DefaultSpawningPreference>DSP_NORMAL</DefaultSpawningPreference>
      <DefaultRemoveRangeMultiplier value=""1.000000"" />
      <AllowCloseSpawning value=""false"" />
    </Item>
    <Item>
      <Name>CSB_Mrs_R</Name>
      <PropsName>null</PropsName>
      <ClipDictionaryName>move_f@generic</ClipDictionaryName>
      <BlendShapeFileName>null</BlendShapeFileName>
      <ExpressionSetName />
      <ExpressionDictionaryName>CSB_Denise_friend</ExpressionDictionaryName>
      <ExpressionName>CSB_Denise_friend</ExpressionName>
      <Pedtype>CIVFEMALE</Pedtype>
      <MovementClipSet>move_characters@patricia</MovementClipSet>
      <StrafeClipSet>move_ped_strafing</StrafeClipSet>
      <MovementToStrafeClipSet>move_ped_to_strafe</MovementToStrafeClipSet>
      <InjuredStrafeClipSet>move_strafe_injured</InjuredStrafeClipSet>
      <FullBodyDamageClipSet>dam_ko</FullBodyDamageClipSet>
      <AdditiveDamageClipSet>dam_ad</AdditiveDamageClipSet>
      <DefaultGestureClipSet>ANIM_GROUP_GESTURE_F_GENERIC</DefaultGestureClipSet>
      <FacialClipsetGroupName>facial_clipset_group_gen_female</FacialClipsetGroupName>
      <DefaultVisemeClipSet>ANIM_GROUP_VISEMES_F_LO</DefaultVisemeClipSet>
      <SidestepClipSet>CLIP_SET_ID_INVALID</SidestepClipSet>
      <PoseMatcherName>Male</PoseMatcherName>
      <PoseMatcherProneName>Male_prone</PoseMatcherProneName>
      <GetupSetHash>NMBS_SLOW_GETUPS</GetupSetHash>
      <CreatureMetadataName>null</CreatureMetadataName>
      <DecisionMakerName>DEFAULT</DecisionMakerName>
      <MotionTaskDataSetName>STANDARD_PED</MotionTaskDataSetName>
      <DefaultTaskDataSetName>STANDARD_PED</DefaultTaskDataSetName>
      <PedCapsuleName>STANDARD_FEMALE</PedCapsuleName>
      <PedLayoutName />
      <PedComponentSetName />
      <PedComponentClothName />
      <PedIKSettingsName />
      <TaskDataName />
      <IsStreamedGfx value=""false"" />
      <AmbulanceShouldRespondTo value=""true"" />
      <CanRideBikeWithNoHelmet value=""false"" />
      <CanSpawnInCar value=""true"" />
      <IsHeadBlendPed value=""false"" />
      <bOnlyBulkyItemVariations value=""false"" />
      <RelationshipGroup>CIVFEMALE</RelationshipGroup>
      <NavCapabilitiesName>STANDARD_PED</NavCapabilitiesName>
      <PerceptionInfo>DEFAULT_PERCEPTION</PerceptionInfo>
      <DefaultBrawlingStyle>BS_AI</DefaultBrawlingStyle>
      <DefaultUnarmedWeapon>WEAPON_UNARMED</DefaultUnarmedWeapon>
      <Personality>Streamed_Female</Personality>
      <CombatInfo>DEFAULT</CombatInfo>
      <VfxInfoName>VFXPEDINFO_HUMAN_GENERIC</VfxInfoName>
      <AmbientClipsForFlee>FLEE</AmbientClipsForFlee>
      <Radio1>RADIO_GENRE_POP</Radio1>
      <Radio2>RADIO_GENRE_DANCE</Radio2>
      <FUpOffset value=""0.000000"" />
      <RUpOffset value=""0.000000"" />
      <FFrontOffset value=""0.000000"" />
      <RFrontOffset value=""0.147000"" />
      <MinActivationImpulse value=""20.000000"" />
      <Stubble value=""0.000000"" />
      <HDDist value=""3.000000"" />
      <TargetingThreatModifier value=""1.000000"" />
      <KilledPerceptionRangeModifer value=""-1.000000"" />
      <Sexiness />
      <Age value=""0"" />
      <MaxPassengersInCar value=""0"" />
      <ExternallyDrivenDOFs />
      <PedVoiceGroup>SILENT_CUTSCENE_PVG</PedVoiceGroup>
      <AnimalAudioObject />
      <AbilityType>SAT_NONE</AbilityType>
      <ThermalBehaviour>TB_WARM</ThermalBehaviour>
      <SuperlodType>SLOD_HUMAN</SuperlodType>
      <ScenarioPopStreamingSlot>SCENARIO_POP_STREAMING_NORMAL</ScenarioPopStreamingSlot>
      <DefaultSpawningPreference>DSP_NORMAL</DefaultSpawningPreference>
      <DefaultRemoveRangeMultiplier value=""1.000000"" />
      <AllowCloseSpawning value=""false"" />
    </Item>
    <Item>
      <Name>CSB_Avon</Name>
      <PropsName>CSB_Avon_p</PropsName>
      <ClipDictionaryName>move_m@generic</ClipDictionaryName>
      <BlendShapeFileName>null</BlendShapeFileName>
      <ExpressionSetName />
      <ExpressionDictionaryName>CSB_Avon</ExpressionDictionaryName>
      <ExpressionName>CSB_Avon</ExpressionName>
      <Pedtype>CIVMALE</Pedtype>
      <MovementClipSet>move_m@generic</MovementClipSet>
      <StrafeClipSet>move_ped_strafing</StrafeClipSet>
      <MovementToStrafeClipSet>move_ped_to_strafe</MovementToStrafeClipSet>
      <InjuredStrafeClipSet>move_strafe_injured</InjuredStrafeClipSet>
      <FullBodyDamageClipSet>dam_ko</FullBodyDamageClipSet>
      <AdditiveDamageClipSet>dam_ad</AdditiveDamageClipSet>
      <DefaultGestureClipSet>ANIM_GROUP_GESTURE_M_GENERIC</DefaultGestureClipSet>
      <FacialClipsetGroupName>facial_clipset_group_gen_male</FacialClipsetGroupName>
      <DefaultVisemeClipSet>ANIM_GROUP_VISEMES_M_HI</DefaultVisemeClipSet>
      <SidestepClipSet>CLIP_SET_ID_INVALID</SidestepClipSet>
      <PoseMatcherName>Male</PoseMatcherName>
      <PoseMatcherProneName>Male_prone</PoseMatcherProneName>
      <GetupSetHash>NMBS_SLOW_GETUPS</GetupSetHash>
      <CreatureMetadataName>null</CreatureMetadataName>
      <DecisionMakerName>DEFAULT</DecisionMakerName>
      <MotionTaskDataSetName>STANDARD_PED</MotionTaskDataSetName>
      <DefaultTaskDataSetName>STANDARD_PED</DefaultTaskDataSetName>
      <PedCapsuleName>STANDARD_MALE</PedCapsuleName>
      <PedLayoutName />
      <PedComponentSetName />
      <PedComponentClothName />
      <PedIKSettingsName />
      <TaskDataName />
      <IsStreamedGfx value=""false"" />
      <AmbulanceShouldRespondTo value=""true"" />
      <CanRideBikeWithNoHelmet value=""false"" />
      <CanSpawnInCar value=""true"" />
      <IsHeadBlendPed value=""false"" />
      <bOnlyBulkyItemVariations value=""false"" />
      <RelationshipGroup>CIVMALE</RelationshipGroup>
      <NavCapabilitiesName>STANDARD_PED</NavCapabilitiesName>
      <PerceptionInfo>DEFAULT_PERCEPTION</PerceptionInfo>
      <DefaultBrawlingStyle>BS_AI</DefaultBrawlingStyle>
      <DefaultUnarmedWeapon>WEAPON_UNARMED</DefaultUnarmedWeapon>
      <Personality>Streamed_Male</Personality>
      <CombatInfo>DEFAULT</CombatInfo>
      <VfxInfoName>VFXPEDINFO_HUMAN_GENERIC</VfxInfoName>
      <AmbientClipsForFlee>FLEE</AmbientClipsForFlee>
      <Radio1>RADIO_GENRE_OFF</Radio1>
      <Radio2>RADIO_GENRE_OFF</Radio2>
      <FUpOffset value=""0.000000"" />
      <RUpOffset value=""0.000000"" />
      <FFrontOffset value=""0.000000"" />
      <RFrontOffset value=""0.147000"" />
      <MinActivationImpulse value=""20.000000"" />
      <Stubble value=""0.000000"" />
      <HDDist value=""3.000000"" />
      <TargetingThreatModifier value=""1.000000"" />
      <KilledPerceptionRangeModifer value=""-1.000000"" />
      <Sexiness />
      <Age value=""0"" />
      <MaxPassengersInCar value=""0"" />
      <ExternallyDrivenDOFs />
      <PedVoiceGroup>SILENT_CUTSCENE_PVG</PedVoiceGroup>
      <AnimalAudioObject />
      <AbilityType>SAT_NONE</AbilityType>
      <ThermalBehaviour>TB_WARM</ThermalBehaviour>
      <SuperlodType>SLOD_HUMAN</SuperlodType>
      <ScenarioPopStreamingSlot>SCENARIO_POP_STREAMING_NORMAL</ScenarioPopStreamingSlot>
      <DefaultSpawningPreference>DSP_NORMAL</DefaultSpawningPreference>
      <DefaultRemoveRangeMultiplier value=""1.000000"" />
      <AllowCloseSpawning value=""false"" />
    </Item>
    <Item>
      <Name>IG_Avon</Name>
      <PropsName>Ig_Avon_p</PropsName>
      <ClipDictionaryName>move_m@generic</ClipDictionaryName>
      <BlendShapeFileName>null</BlendShapeFileName>
      <ExpressionSetName>expr_set_ambient_male_skirt</ExpressionSetName>
      <ExpressionDictionaryName>null</ExpressionDictionaryName>
      <ExpressionName>null</ExpressionName>
      <Pedtype>CIVMALE</Pedtype>
      <MovementClipSet>move_chubby</MovementClipSet>
      <StrafeClipSet>move_ped_strafing</StrafeClipSet>
      <MovementToStrafeClipSet>move_ped_to_strafe</MovementToStrafeClipSet>
      <InjuredStrafeClipSet>move_strafe_injured</InjuredStrafeClipSet>
      <FullBodyDamageClipSet>dam_ko</FullBodyDamageClipSet>
      <AdditiveDamageClipSet>dam_ad</AdditiveDamageClipSet>
      <DefaultGestureClipSet>ANIM_GROUP_GESTURE_M_GENERIC</DefaultGestureClipSet>
      <FacialClipsetGroupName>facial_clipset_group_gen_male</FacialClipsetGroupName>
      <DefaultVisemeClipSet>ANIM_GROUP_VISEMES_M_LO</DefaultVisemeClipSet>
      <SidestepClipSet>CLIP_SET_ID_INVALID</SidestepClipSet>
      <PoseMatcherName>Male</PoseMatcherName>
      <PoseMatcherProneName>Male_prone</PoseMatcherProneName>
      <GetupSetHash>NMBS_SLOW_GETUPS</GetupSetHash>
      <CreatureMetadataName>null</CreatureMetadataName>
      <DecisionMakerName>DEFAULT</DecisionMakerName>
      <MotionTaskDataSetName>STANDARD_PED</MotionTaskDataSetName>
      <DefaultTaskDataSetName>STANDARD_PED</DefaultTaskDataSetName>
      <PedCapsuleName>STANDARD_MALE</PedCapsuleName>
      <PedLayoutName />
      <PedComponentSetName />
      <PedComponentClothName />
      <PedIKSettingsName />
      <TaskDataName />
      <IsStreamedGfx value=""false"" />
      <AmbulanceShouldRespondTo value=""true"" />
      <CanRideBikeWithNoHelmet value=""false"" />
      <CanSpawnInCar value=""false"" />
      <IsHeadBlendPed value=""false"" />
      <bOnlyBulkyItemVariations value=""false"" />
      <RelationshipGroup>CIVMALE</RelationshipGroup>
      <NavCapabilitiesName>STANDARD_PED</NavCapabilitiesName>
      <PerceptionInfo>DEFAULT_PERCEPTION</PerceptionInfo>
      <DefaultBrawlingStyle>BS_AI</DefaultBrawlingStyle>
      <DefaultUnarmedWeapon>WEAPON_UNARMED</DefaultUnarmedWeapon>
      <Personality>CONSTRUCTION</Personality>
      <CombatInfo>DEFAULT</CombatInfo>
      <VfxInfoName>VFXPEDINFO_HUMAN_GENERIC</VfxInfoName>
      <AmbientClipsForFlee>FLEE</AmbientClipsForFlee>
      <Radio1>RADIO_GENRE_MODERN_ROCK</Radio1>
      <Radio2>RADIO_GENRE_MOTOWN</Radio2>
      <FUpOffset value=""0.000000"" />
      <RUpOffset value=""0.000000"" />
      <FFrontOffset value=""0.000000"" />
      <RFrontOffset value=""0.147000"" />
      <MinActivationImpulse value=""20.000000"" />
      <Stubble value=""0.000000"" />
      <HDDist value=""3.000000"" />
      <TargetingThreatModifier value=""1.000000"" />
      <KilledPerceptionRangeModifer value=""-1.000000"" />
      <Sexiness>SF_JEER_AT_HOT_PED</Sexiness>
      <Age value=""0"" />
      <MaxPassengersInCar value=""0"" />
      <ExternallyDrivenDOFs />
      <PedVoiceGroup>H2AVON_PVG</PedVoiceGroup>
      <AnimalAudioObject />
      <AbilityType>SAT_NONE</AbilityType>
      <ThermalBehaviour>TB_WARM</ThermalBehaviour>
      <SuperlodType>SLOD_HUMAN</SuperlodType>
      <ScenarioPopStreamingSlot>SCENARIO_POP_STREAMING_NORMAL</ScenarioPopStreamingSlot>
      <DefaultSpawningPreference>DSP_NORMAL</DefaultSpawningPreference>
      <DefaultRemoveRangeMultiplier value=""1.000000"" />
      <AllowCloseSpawning value=""false"" />
    </Item>
    <Item>
      <Name>CSB_Bogdan</Name>
      <PropsName>CSB_Bogdan_p</PropsName>
      <ClipDictionaryName>move_m@generic</ClipDictionaryName>
      <BlendShapeFileName>null</BlendShapeFileName>
      <ExpressionSetName />
      <ExpressionDictionaryName>CSB_Bogdan</ExpressionDictionaryName>
      <ExpressionName>CSB_Bogdan</ExpressionName>
      <Pedtype>CIVMALE</Pedtype>
      <MovementClipSet>move_m@generic</MovementClipSet>
      <StrafeClipSet>move_ped_strafing</StrafeClipSet>
      <MovementToStrafeClipSet>move_ped_to_strafe</MovementToStrafeClipSet>
      <InjuredStrafeClipSet>move_strafe_injured</InjuredStrafeClipSet>
      <FullBodyDamageClipSet>dam_ko</FullBodyDamageClipSet>
      <AdditiveDamageClipSet>dam_ad</AdditiveDamageClipSet>
      <DefaultGestureClipSet>ANIM_GROUP_GESTURE_M_GENERIC</DefaultGestureClipSet>
      <FacialClipsetGroupName>facial_clipset_group_gen_male</FacialClipsetGroupName>
      <DefaultVisemeClipSet>ANIM_GROUP_VISEMES_M_HI</DefaultVisemeClipSet>
      <SidestepClipSet>CLIP_SET_ID_INVALID</SidestepClipSet>
      <PoseMatcherName>Male</PoseMatcherName>
      <PoseMatcherProneName>Male_prone</PoseMatcherProneName>
      <GetupSetHash>NMBS_SLOW_GETUPS</GetupSetHash>
      <CreatureMetadataName>null</CreatureMetadataName>
      <DecisionMakerName>DEFAULT</DecisionMakerName>
      <MotionTaskDataSetName>STANDARD_PED</MotionTaskDataSetName>
      <DefaultTaskDataSetName>STANDARD_PED</DefaultTaskDataSetName>
      <PedCapsuleName>STANDARD_MALE</PedCapsuleName>
      <PedLayoutName />
      <PedComponentSetName />
      <PedComponentClothName />
      <PedIKSettingsName />
      <TaskDataName />
      <IsStreamedGfx value=""false"" />
      <AmbulanceShouldRespondTo value=""true"" />
      <CanRideBikeWithNoHelmet value=""false"" />
      <CanSpawnInCar value=""true"" />
      <IsHeadBlendPed value=""false"" />
      <bOnlyBulkyItemVariations value=""false"" />
      <RelationshipGroup>CIVMALE</RelationshipGroup>
      <NavCapabilitiesName>STANDARD_PED</NavCapabilitiesName>
      <PerceptionInfo>DEFAULT_PERCEPTION</PerceptionInfo>
      <DefaultBrawlingStyle>BS_AI</DefaultBrawlingStyle>
      <DefaultUnarmedWeapon>WEAPON_UNARMED</DefaultUnarmedWeapon>
      <Personality>Streamed_Male</Personality>
      <CombatInfo>DEFAULT</CombatInfo>
      <VfxInfoName>VFXPEDINFO_HUMAN_GENERIC</VfxInfoName>
      <AmbientClipsForFlee>FLEE</AmbientClipsForFlee>
      <Radio1>RADIO_GENRE_OFF</Radio1>
      <Radio2>RADIO_GENRE_OFF</Radio2>
      <FUpOffset value=""0.000000"" />
      <RUpOffset value=""0.000000"" />
      <FFrontOffset value=""0.000000"" />
      <RFrontOffset value=""0.147000"" />
      <MinActivationImpulse value=""20.000000"" />
      <Stubble value=""0.000000"" />
      <HDDist value=""3.000000"" />
      <TargetingThreatModifier value=""1.000000"" />
      <KilledPerceptionRangeModifer value=""-1.000000"" />
      <Sexiness />
      <Age value=""0"" />
      <MaxPassengersInCar value=""0"" />
      <ExternallyDrivenDOFs />
      <PedVoiceGroup>H2_BOGDAN_PVG</PedVoiceGroup>
      <AnimalAudioObject />
      <AbilityType>SAT_NONE</AbilityType>
      <ThermalBehaviour>TB_WARM</ThermalBehaviour>
      <SuperlodType>SLOD_HUMAN</SuperlodType>
      <ScenarioPopStreamingSlot>SCENARIO_POP_STREAMING_NORMAL</ScenarioPopStreamingSlot>
      <DefaultSpawningPreference>DSP_NORMAL</DefaultSpawningPreference>
      <DefaultRemoveRangeMultiplier value=""1.000000"" />
      <AllowCloseSpawning value=""false"" />
    </Item>
    <Item>
      <Name>U_M_Y_Juggernaut_01</Name>
      <PropsName>U_M_Y_Juggernaut_01_p</PropsName>
      <ClipDictionaryName>move_m@multiplayer</ClipDictionaryName>
      <BlendShapeFileName>null</BlendShapeFileName>
      <ExpressionSetName />
      <ExpressionDictionaryName>HC_Gunman</ExpressionDictionaryName>
      <ExpressionName>HC_Gunman</ExpressionName>
      <Pedtype>CIVMALE</Pedtype>
      <MovementClipSet>move_m@multiplayer</MovementClipSet>
      <StrafeClipSet>move_ped_strafing</StrafeClipSet>
      <MovementToStrafeClipSet>move_ped_to_strafe</MovementToStrafeClipSet>
      <InjuredStrafeClipSet>move_strafe_injured</InjuredStrafeClipSet>
      <FullBodyDamageClipSet>dam_ko</FullBodyDamageClipSet>
      <AdditiveDamageClipSet>dam_ad</AdditiveDamageClipSet>
      <DefaultGestureClipSet>ANIM_GROUP_GESTURE_M_GENERIC</DefaultGestureClipSet>
      <FacialClipsetGroupName>facial_clipset_group_gen_male</FacialClipsetGroupName>
      <DefaultVisemeClipSet>ANIM_GROUP_VISEMES_M_LO</DefaultVisemeClipSet>
      <SidestepClipSet>CLIP_SET_ID_INVALID</SidestepClipSet>
      <PoseMatcherName>Male</PoseMatcherName>
      <PoseMatcherProneName>Male_prone</PoseMatcherProneName>
      <GetupSetHash>NMBS_SLOW_GETUPS</GetupSetHash>
      <CreatureMetadataName>fx_fire_torch</CreatureMetadataName>
      <DecisionMakerName>DEFAULT</DecisionMakerName>
      <MotionTaskDataSetName>STANDARD_PED</MotionTaskDataSetName>
      <DefaultTaskDataSetName>STANDARD_PED</DefaultTaskDataSetName>
      <PedCapsuleName>STANDARD_MALE</PedCapsuleName>
      <PedLayoutName />
      <PedComponentSetName />
      <PedComponentClothName />
      <PedIKSettingsName />
      <TaskDataName />
      <IsStreamedGfx value=""false"" />
      <AmbulanceShouldRespondTo value=""true"" />
      <CanRideBikeWithNoHelmet value=""false"" />
      <CanSpawnInCar value=""true"" />
      <IsHeadBlendPed value=""false"" />
      <bOnlyBulkyItemVariations value=""false"" />
      <RelationshipGroup>CIVMALE</RelationshipGroup>
      <NavCapabilitiesName>STANDARD_PED</NavCapabilitiesName>
      <PerceptionInfo>DEFAULT_PERCEPTION</PerceptionInfo>
      <DefaultBrawlingStyle>BS_AI</DefaultBrawlingStyle>
      <DefaultUnarmedWeapon>WEAPON_UNARMED</DefaultUnarmedWeapon>
      <Personality>Streamed_Male</Personality>
      <CombatInfo>DEFAULT</CombatInfo>
      <VfxInfoName>VFXPEDINFO_HUMAN_GENERIC</VfxInfoName>
      <AmbientClipsForFlee>FLEE</AmbientClipsForFlee>
      <Radio1>RADIO_GENRE_PUNK</Radio1>
      <Radio2>RADIO_GENRE_MOTOWN</Radio2>
      <FUpOffset value=""0.000000"" />
      <RUpOffset value=""0.000000"" />
      <FFrontOffset value=""0.000000"" />
      <RFrontOffset value=""0.147000"" />
      <MinActivationImpulse value=""20.000000"" />
      <Stubble value=""0.000000"" />
      <HDDist value=""0.000000"" />
      <TargetingThreatModifier value=""1.000000"" />
      <KilledPerceptionRangeModifer value=""-1.000000"" />
      <Sexiness />
      <Age value=""0"" />
      <MaxPassengersInCar value=""0"" />
      <ExternallyDrivenDOFs>COLLAR</ExternallyDrivenDOFs>
      <PedVoiceGroup>SILENT_PVG</PedVoiceGroup>
      <AnimalAudioObject />
      <AbilityType>SAT_NONE</AbilityType>
      <ThermalBehaviour>TB_WARM</ThermalBehaviour>
      <SuperlodType>SLOD_HUMAN</SuperlodType>
      <ScenarioPopStreamingSlot>SCENARIO_POP_STREAMING_NORMAL</ScenarioPopStreamingSlot>
      <DefaultSpawningPreference>DSP_NORMAL</DefaultSpawningPreference>
      <DefaultRemoveRangeMultiplier value=""1.000000"" />
      <AllowCloseSpawning value=""false"" />
    </Item>
    <Item>
      <Name>MP_M_AvonGoon</Name>
      <PropsName>MP_M_AvonGoon_p</PropsName>
      <ClipDictionaryName>move_m@generic</ClipDictionaryName>
      <BlendShapeFileName>null</BlendShapeFileName>
      <ExpressionSetName>expr_set_ambient_male_skirt</ExpressionSetName>
      <ExpressionDictionaryName>null</ExpressionDictionaryName>
      <ExpressionName>null</ExpressionName>
      <Pedtype>CIVMALE</Pedtype>
      <MovementClipSet>move_chubby</MovementClipSet>
      <StrafeClipSet>move_ped_strafing</StrafeClipSet>
      <MovementToStrafeClipSet>move_ped_to_strafe</MovementToStrafeClipSet>
      <InjuredStrafeClipSet>move_strafe_injured</InjuredStrafeClipSet>
      <FullBodyDamageClipSet>dam_ko</FullBodyDamageClipSet>
      <AdditiveDamageClipSet>dam_ad</AdditiveDamageClipSet>
      <DefaultGestureClipSet>ANIM_GROUP_GESTURE_M_GENERIC</DefaultGestureClipSet>
      <FacialClipsetGroupName>facial_clipset_group_gen_male</FacialClipsetGroupName>
      <DefaultVisemeClipSet>ANIM_GROUP_VISEMES_M_LO</DefaultVisemeClipSet>
      <SidestepClipSet>CLIP_SET_ID_INVALID</SidestepClipSet>
      <PoseMatcherName>Male</PoseMatcherName>
      <PoseMatcherProneName>Male_prone</PoseMatcherProneName>
      <GetupSetHash>NMBS_SLOW_GETUPS</GetupSetHash>
      <CreatureMetadataName>null</CreatureMetadataName>
      <DecisionMakerName>DEFAULT</DecisionMakerName>
      <MotionTaskDataSetName>STANDARD_PED</MotionTaskDataSetName>
      <DefaultTaskDataSetName>STANDARD_PED</DefaultTaskDataSetName>
      <PedCapsuleName>STANDARD_MALE</PedCapsuleName>
      <PedLayoutName />
      <PedComponentSetName />
      <PedComponentClothName />
      <PedIKSettingsName />
      <TaskDataName />
      <IsStreamedGfx value=""false"" />
      <AmbulanceShouldRespondTo value=""true"" />
      <CanRideBikeWithNoHelmet value=""false"" />
      <CanSpawnInCar value=""false"" />
      <IsHeadBlendPed value=""false"" />
      <bOnlyBulkyItemVariations value=""false"" />
      <RelationshipGroup>CIVMALE</RelationshipGroup>
      <NavCapabilitiesName>STANDARD_PED</NavCapabilitiesName>
      <PerceptionInfo>DEFAULT_PERCEPTION</PerceptionInfo>
      <DefaultBrawlingStyle>BS_AI</DefaultBrawlingStyle>
      <DefaultUnarmedWeapon>WEAPON_UNARMED</DefaultUnarmedWeapon>
      <Personality>CONSTRUCTION</Personality>
      <CombatInfo>DEFAULT</CombatInfo>
      <VfxInfoName>VFXPEDINFO_HUMAN_GENERIC</VfxInfoName>
      <AmbientClipsForFlee>FLEE</AmbientClipsForFlee>
      <Radio1>RADIO_GENRE_MODERN_ROCK</Radio1>
      <Radio2>RADIO_GENRE_MOTOWN</Radio2>
      <FUpOffset value=""0.000000"" />
      <RUpOffset value=""0.000000"" />
      <FFrontOffset value=""0.000000"" />
      <RFrontOffset value=""0.147000"" />
      <MinActivationImpulse value=""20.000000"" />
      <Stubble value=""0.000000"" />
      <HDDist value=""3.000000"" />
      <TargetingThreatModifier value=""1.000000"" />
      <KilledPerceptionRangeModifer value=""-1.000000"" />
      <Sexiness>SF_JEER_AT_HOT_PED</Sexiness>
      <Age value=""0"" />
      <MaxPassengersInCar value=""0"" />
      <ExternallyDrivenDOFs />
      <PedVoiceGroup>G_M_Y_X17_AGuard_PVG</PedVoiceGroup>
      <AnimalAudioObject />
      <AbilityType>SAT_NONE</AbilityType>
      <ThermalBehaviour>TB_WARM</ThermalBehaviour>
      <SuperlodType>SLOD_HUMAN</SuperlodType>
      <ScenarioPopStreamingSlot>SCENARIO_POP_STREAMING_NORMAL</ScenarioPopStreamingSlot>
      <DefaultSpawningPreference>DSP_NORMAL</DefaultSpawningPreference>
      <DefaultRemoveRangeMultiplier value=""1.000000"" />
      <AllowCloseSpawning value=""false"" />
    </Item>
    <Item>
      <Name>MP_M_BogdanGoon</Name>
      <PropsName>MP_M_BogdanGoon_p</PropsName>
      <ClipDictionaryName>move_m@generic</ClipDictionaryName>
      <BlendShapeFileName>null</BlendShapeFileName>
      <ExpressionSetName>expr_set_ambient_male_skirt</ExpressionSetName>
      <ExpressionDictionaryName>null</ExpressionDictionaryName>
      <ExpressionName>null</ExpressionName>
      <Pedtype>CIVMALE</Pedtype>
      <MovementClipSet>move_chubby</MovementClipSet>
      <StrafeClipSet>move_ped_strafing</StrafeClipSet>
      <MovementToStrafeClipSet>move_ped_to_strafe</MovementToStrafeClipSet>
      <InjuredStrafeClipSet>move_strafe_injured</InjuredStrafeClipSet>
      <FullBodyDamageClipSet>dam_ko</FullBodyDamageClipSet>
      <AdditiveDamageClipSet>dam_ad</AdditiveDamageClipSet>
      <DefaultGestureClipSet>ANIM_GROUP_GESTURE_M_GENERIC</DefaultGestureClipSet>
      <FacialClipsetGroupName>facial_clipset_group_gen_male</FacialClipsetGroupName>
      <DefaultVisemeClipSet>ANIM_GROUP_VISEMES_M_LO</DefaultVisemeClipSet>
      <SidestepClipSet>CLIP_SET_ID_INVALID</SidestepClipSet>
      <PoseMatcherName>Male</PoseMatcherName>
      <PoseMatcherProneName>Male_prone</PoseMatcherProneName>
      <GetupSetHash>NMBS_SLOW_GETUPS</GetupSetHash>
      <CreatureMetadataName>null</CreatureMetadataName>
      <DecisionMakerName>DEFAULT</DecisionMakerName>
      <MotionTaskDataSetName>STANDARD_PED</MotionTaskDataSetName>
      <DefaultTaskDataSetName>STANDARD_PED</DefaultTaskDataSetName>
      <PedCapsuleName>STANDARD_MALE</PedCapsuleName>
      <PedLayoutName />
      <PedComponentSetName />
      <PedComponentClothName />
      <PedIKSettingsName />
      <TaskDataName />
      <IsStreamedGfx value=""false"" />
      <AmbulanceShouldRespondTo value=""true"" />
      <CanRideBikeWithNoHelmet value=""false"" />
      <CanSpawnInCar value=""false"" />
      <IsHeadBlendPed value=""false"" />
      <bOnlyBulkyItemVariations value=""false"" />
      <RelationshipGroup>CIVMALE</RelationshipGroup>
      <NavCapabilitiesName>STANDARD_PED</NavCapabilitiesName>
      <PerceptionInfo>DEFAULT_PERCEPTION</PerceptionInfo>
      <DefaultBrawlingStyle>BS_AI</DefaultBrawlingStyle>
      <DefaultUnarmedWeapon>WEAPON_UNARMED</DefaultUnarmedWeapon>
      <Personality>CONSTRUCTION</Personality>
      <CombatInfo>DEFAULT</CombatInfo>
      <VfxInfoName>VFXPEDINFO_HUMAN_GENERIC</VfxInfoName>
      <AmbientClipsForFlee>FLEE</AmbientClipsForFlee>
      <Radio1>RADIO_GENRE_MODERN_ROCK</Radio1>
      <Radio2>RADIO_GENRE_MOTOWN</Radio2>
      <FUpOffset value=""0.000000"" />
      <RUpOffset value=""0.000000"" />
      <FFrontOffset value=""0.000000"" />
      <RFrontOffset value=""0.147000"" />
      <MinActivationImpulse value=""20.000000"" />
      <Stubble value=""0.000000"" />
      <HDDist value=""3.000000"" />
      <TargetingThreatModifier value=""1.000000"" />
      <KilledPerceptionRangeModifer value=""-1.000000"" />
      <Sexiness>SF_JEER_AT_HOT_PED</Sexiness>
      <Age value=""0"" />
      <MaxPassengersInCar value=""0"" />
      <ExternallyDrivenDOFs />
      <PedVoiceGroup>G_M_M_X17_RSO_PVG</PedVoiceGroup>
      <AnimalAudioObject />
      <AbilityType>SAT_NONE</AbilityType>
      <ThermalBehaviour>TB_WARM</ThermalBehaviour>
      <SuperlodType>SLOD_HUMAN</SuperlodType>
      <ScenarioPopStreamingSlot>SCENARIO_POP_STREAMING_NORMAL</ScenarioPopStreamingSlot>
      <DefaultSpawningPreference>DSP_NORMAL</DefaultSpawningPreference>
      <DefaultRemoveRangeMultiplier value=""1.000000"" />
      <AllowCloseSpawning value=""false"" />
    </Item>
    <Item>
      <Name>U_M_Y_Corpse_01</Name>
      <PropsName>null</PropsName>
      <ClipDictionaryName>move_m@generic</ClipDictionaryName>
      <BlendShapeFileName>null</BlendShapeFileName>
      <ExpressionSetName>expr_set_ambient_male_skirt</ExpressionSetName>
      <ExpressionDictionaryName>null</ExpressionDictionaryName>
      <ExpressionName>null</ExpressionName>
      <Pedtype>CIVMALE</Pedtype>
      <MovementClipSet>move_chubby</MovementClipSet>
      <StrafeClipSet>move_ped_strafing</StrafeClipSet>
      <MovementToStrafeClipSet>move_ped_to_strafe</MovementToStrafeClipSet>
      <InjuredStrafeClipSet>move_strafe_injured</InjuredStrafeClipSet>
      <FullBodyDamageClipSet>dam_ko_@gangops@morgue@table@</FullBodyDamageClipSet>
      <AdditiveDamageClipSet>dam_ad</AdditiveDamageClipSet>
      <DefaultGestureClipSet>ANIM_GROUP_GESTURE_M_GENERIC</DefaultGestureClipSet>
      <FacialClipsetGroupName>facial_clipset_group_gen_male</FacialClipsetGroupName>
      <DefaultVisemeClipSet>ANIM_GROUP_VISEMES_M_LO</DefaultVisemeClipSet>
      <SidestepClipSet>CLIP_SET_ID_INVALID</SidestepClipSet>
      <PoseMatcherName>Male</PoseMatcherName>
      <PoseMatcherProneName>Male_prone</PoseMatcherProneName>
      <GetupSetHash>NMBS_SLOW_GETUPS</GetupSetHash>
      <CreatureMetadataName>null</CreatureMetadataName>
      <DecisionMakerName>DEFAULT</DecisionMakerName>
      <MotionTaskDataSetName>STANDARD_PED</MotionTaskDataSetName>
      <DefaultTaskDataSetName>STANDARD_PED</DefaultTaskDataSetName>
      <PedCapsuleName>STANDARD_MALE</PedCapsuleName>
      <PedLayoutName />
      <PedComponentSetName />
      <PedComponentClothName />
      <PedIKSettingsName />
      <TaskDataName />
      <IsStreamedGfx value=""false"" />
      <AmbulanceShouldRespondTo value=""true"" />
      <CanRideBikeWithNoHelmet value=""false"" />
      <CanSpawnInCar value=""false"" />
      <IsHeadBlendPed value=""false"" />
      <bOnlyBulkyItemVariations value=""false"" />
      <RelationshipGroup>CIVMALE</RelationshipGroup>
      <NavCapabilitiesName>STANDARD_PED</NavCapabilitiesName>
      <PerceptionInfo>DEFAULT_PERCEPTION</PerceptionInfo>
      <DefaultBrawlingStyle>BS_AI</DefaultBrawlingStyle>
      <DefaultUnarmedWeapon>WEAPON_UNARMED</DefaultUnarmedWeapon>
      <Personality>CONSTRUCTION</Personality>
      <CombatInfo>DEFAULT</CombatInfo>
      <VfxInfoName>VFXPEDINFO_HUMAN_GENERIC</VfxInfoName>
      <AmbientClipsForFlee>FLEE</AmbientClipsForFlee>
      <Radio1>RADIO_GENRE_MODERN_ROCK</Radio1>
      <Radio2>RADIO_GENRE_MOTOWN</Radio2>
      <FUpOffset value=""0.000000"" />
      <RUpOffset value=""0.000000"" />
      <FFrontOffset value=""0.000000"" />
      <RFrontOffset value=""0.147000"" />
      <MinActivationImpulse value=""20.000000"" />
      <Stubble value=""0.000000"" />
      <HDDist value=""3.000000"" />
      <TargetingThreatModifier value=""1.000000"" />
      <KilledPerceptionRangeModifer value=""-1.000000"" />
      <Sexiness>SF_JEER_AT_HOT_PED</Sexiness>
      <Age value=""0"" />
      <MaxPassengersInCar value=""0"" />
      <ExternallyDrivenDOFs />
      <PedVoiceGroup>SILENT_PVG</PedVoiceGroup>
      <AnimalAudioObject />
      <AbilityType>SAT_NONE</AbilityType>
      <ThermalBehaviour>TB_WARM</ThermalBehaviour>
      <SuperlodType>SLOD_HUMAN</SuperlodType>
      <ScenarioPopStreamingSlot>SCENARIO_POP_STREAMING_NORMAL</ScenarioPopStreamingSlot>
      <DefaultSpawningPreference>DSP_NORMAL</DefaultSpawningPreference>
      <DefaultRemoveRangeMultiplier value=""1.000000"" />
      <AllowCloseSpawning value=""false"" />
    </Item>
</InitDatas>

  <multiTxdRelationships>
    <Item>
      <parent>comp_peds_helmets_moped</parent>
      <children>
       
      </children>
    </Item>
    <Item>
      <parent>comp_peds_helmets_motox</parent>
      <children>
      
      </children>
    </Item>
    <Item>
      <parent>comp_peds_helmets_sports</parent>
      <children>
      
      </children>
    </Item>
    <Item>
      <parent>comp_peds_helmets_shorty</parent>
      <children>
     
      </children>
    </Item>
    <Item>
      <parent>strm_peds_mpTattRTs</parent>
      <children>
     
      </children>
    </Item>
    <Item>
      <parent>strm_peds_mpShare</parent>
      <children>
      
      </children>
    </Item>
    <Item>
      <parent>comp_peds_marine</parent>
      <children>
       
      </children>
    </Item>
    <Item>
      <parent>comp_peds_marine</parent>
      <children>
        <Item>S_M_M_Marine_01</Item>
        <Item>S_M_Y_Marine_01</Item>
        <Item>S_M_Y_Marine_02</Item>
        <Item>S_M_Y_Marine_03</Item>
      </children>
    </Item>
  </multiTxdRelationships>
</CPedModelInfo__InitDataList>
";


        [Fact]
        public void GetChildInnerTextShouldReturnInnerText()
        {
            var xdoc = new XmlDocument();
            xdoc.LoadXml(markup);

            XmlNodeList? items = xdoc.SelectNodes("CVehicleModelInfo__InitDataList/InitDatas/Item | CVehicleModelInfo__InitDataList/InitDatas/item");
            VehicleInitData? initDataExpected = null;
            VehicleInitData? initData = null;

            string? modelName;
            string? gameName;
            for (int i = 0; i < items.Count; i++)
            {
                var node = items[i];

                Assert.NotNull(node);

                initDataExpected = new VehicleInitData();
                initDataExpected.Load(node);

                modelName = Xml.GetChildInnerText(node, "modelName");
                Assert.Equal("brabusgt600", modelName);

                gameName = Xml.GetChildInnerText(node, "gameName");
                Assert.Equal("GT 600", gameName);
            }

            Assert.NotNull(initDataExpected);

            Assert.Equal("brabusgt600", initDataExpected.modelName);
            Assert.Equal("GT 600", initDataExpected.gameName);
            Assert.Equal("brabusgt600", initDataExpected.txdName);

            //Assert.Null(initDataExpected.trailers);
            Assert.Equal(9, initDataExpected.trailers.Length);
            Assert.Single(initDataExpected.drivers);
            Assert.Equal("S_M_Y_Cop_01", initDataExpected.drivers[0].DriverName);
            Assert.Equal("trailers4", initDataExpected.trailers[4]);

            using XmlReader xmlReader = XmlReader.Create(new StringReader(markup));

            gameName = null;
            modelName = null;
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    xmlReader.ReadToFollowing("InitDatas");
                    if (xmlReader.Name == "InitDatas")
                    {
                        xmlReader.Read();
                        xmlReader.MoveToContent();
                        
                        initData = new VehicleInitData();
                        initData.Load(xmlReader);
                    }
                }
            }

            Assert.NotNull(initData);

            Assert.NotNull(initData.pOverrideRagdollThreshold);

            Assert.Equal(initDataExpected.pOverrideRagdollThreshold, initData.pOverrideRagdollThreshold);
            Assert.Equal(initDataExpected.requiredExtras, initData.requiredExtras);

            Assert.Equivalent(initDataExpected, initData);

            var bytes = Encoding.UTF8.GetBytes(markup);

            var vehiclesFileExpected = new VehiclesFile();
            var fileEntry = RpfFile.CreateFileEntry("kaas.meta", "saak.meta", ref bytes);
            vehiclesFileExpected.LoadOld(bytes, fileEntry);

            var vehiclesFile = new VehiclesFile();
            vehiclesFile.Load(bytes, fileEntry);

            Assert.Equivalent(vehiclesFileExpected, vehiclesFile);
        }

        [Fact]
        public void IterateOverItemsShouldReturnOnlyItemsAndAdvanceXmlReaderPastEndElement()
        {
            string xml = @"      <cinematicPartCamera>
        <Item>WHEEL_FRONT_RIGHT_CAMERA</Item>
        <Item>WHEEL_FRONT_LEFT_CAMERA</Item>
        <Item>WHEEL_REAR_RIGHT_CAMERA</Item>
        <Item>WHEEL_REAR_LEFT_CAMERA</Item>
      </cinematicPartCamera>";

            using var xmlReader = XmlReader.Create(new StringReader(xml));

            foreach(var item in Xml.IterateItems(xmlReader, "cinematicPartCamera"))
            {
                Assert.NotNull(item);
                output.WriteLine(item.ToString());
            }
        }

        [Fact]
        public void PedFileShouldBeSame()
        {
            var xmlText = TextUtil.GetUTF8Text(System.Text.Encoding.UTF8.GetBytes(pedsList));
            using XmlReader xmlReader = XmlReader.Create(new StringReader(xmlText));

            var xDocument = new XmlDocument();
            xDocument.LoadXml(xmlText);

            var pedsFile = new CPedModelInfo__InitDataList(xmlReader);
            Assert.NotNull(xDocument?.DocumentElement);
            var pedsFileExpected = new CPedModelInfo__InitDataList(xDocument.DocumentElement);

            for (int i = 0; i < pedsFileExpected.InitDatas.Length; i++)
            {
                Assert.Equivalent(pedsFileExpected.InitDatas[i], pedsFile.InitDatas[i]);
            }

            for (int i = 0; i < pedsFileExpected.multiTxdRelationships.Length; i++)
            {
                Assert.Equivalent(pedsFileExpected.multiTxdRelationships[i], pedsFile.multiTxdRelationships[i]);
            }

            Assert.Equivalent(pedsFileExpected, pedsFile);
        }

        [Fact]
        public void ContentFileShouldBeTheSame()
        {
            var fileStream = File.OpenRead(TestFiles.GetFilePath("content.xml"));

            var xmlReader = XmlReader.Create(fileStream);

            var contentFile = new DlcContentFile();
            contentFile.Load(xmlReader);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(TextUtil.GetUTF8Text(File.ReadAllBytes(TestFiles.GetFilePath("content.xml"))));

            var contentFileExpected = new DlcContentFile();
            contentFileExpected.Load(xmlDocument);

            for (int i = 0; i < contentFileExpected.contentChangeSets.Count; i++)
            {
                Assert.Equivalent(contentFileExpected.contentChangeSets[i], contentFile.contentChangeSets[i]);
            }

            Assert.Equivalent(contentFileExpected, contentFile);
        }
    }
}
