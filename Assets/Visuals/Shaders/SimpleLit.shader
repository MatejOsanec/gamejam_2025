Shader "Custom/SimpleLit" {

    Properties {

        // implemented
        _Color ("Color", Color) = (1,1,1,1)
        [ToggleHeader(AVATAR_COMPUTE_SKINNING)] _AvatarComputeSkinning ("Avatar Compute Skinning", Int) = 0
        [EnumHeader(None, Import, External Scale, Object Space, Additive Offset)] _Secondary_UVs ("Secondary UVs", Int) = 0
        [ShowIfAny(_SECONDARY_UVS_EXTERNAL_SCALE, _SECONDARY_UVS_OBJECT_SPACE)] _UVScale ("UV Scale", Vector) = (1,1,1,1)
        [ShowIfAny(_SECONDARY_UVS_ADDITIVE_OFFSET)] _AdditiveUVOffset ("UV Offset", Vector) = (0,0,0,0)

        [VectorShowIfAny(2)] _InputUvMultiplier ("UV Multiplier", Vector) = (1,1,0,0)

        [BigHeader(BASE PROPERTIES)]
        [Space(12)]

        [ToggleHeader(METAL_SMOOTHNESS_TEXTURE)] _EnableMetalSmoothnessTex ("Multi Purpose Map", Int) = 0 // TODO: Rename and transfer
        [ShowIfAny(METAL_SMOOTHNESS_TEXTURE)] _MetalSmoothnessTex ("MPM Texture", 2D) = "white" {}
        [ToggleShowIfAny(SECONDARY_UVS_MPM, 2, 0_SECONDARY_UVS_NONE, METAL_SMOOTHNESS_TEXTURE)] _SecondaryUVsMPM ("MPM Secondary UVs", Int) = 0
        [ToggleShowIfAny(MPM_CUSTOM_MIP, METAL_SMOOTHNESS_TEXTURE)] _EnableCustomMPMMip ("Mip map bias", Int) = 0
        [ShowIfAny(2, METAL_SMOOTHNESS_TEXTURE, MPM_CUSTOM_MIP)] _MpmMipBias ("MPM mipmap bias", Range(-3,0)) = 0.0

        [Space(12)]
        [KeywordEnum(None, MPM R, MPM_A, MPM Avatar B)] _Metallic_Texture ("Metallic Source", Int) = 0
        _Metallic ("Metallic", Range(0,1)) = 1.0


        [Space(12)]
        [InfoBox(ERROR MPM Texture must be used to use its channels, 2, 0METAL_SMOOTHNESS_TEXTURE, 0_SMOOTHNESS_TEXTURE_NONE)]
        [KeywordEnum(None, MPM A, MPM G Roughness)] _Smoothness_Texture("Smoothness Source", Int) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        [Space(12)]
        [ToggleHeader(SPECULAR_ANTIFLICKER)] _SpecularAntiflicker ("Smoothness Anti-Flicker", Int) = 0
        [ShowIfAny(SPECULAR_ANTIFLICKER)] _AntiflickerStrength ("Antiflicker Strength", Range(0,1)) = 0.7
        [ShowIfAny(SPECULAR_ANTIFLICKER)] _AntiflickerDistanceScale ("Antiflicker Distance Scale", Float) = 0.1
        [ShowIfAny(SPECULAR_ANTIFLICKER)] _AntiflickerDistanceOffset ("Antiflicker Distance Offset", Float) = 21

        [Space(12)]
        [Toggle(PRECISE_NORMAL)] _PreciseNormal ("Precise Normal", Int) = 0
        [Space(18)]

        [EnumHeader(None, Color, Emission, MetalSmoothness, Special, Displacement, Emissive Mult Add)] _VertexMode ("Vertex Color Mode", Int) = 0
        [Space(14)]
        [SpaceShowIfAny(12, _VERTEXMODE_EMISSION, _VERTEXMODE_METALSMOOTHNESS, _VERTEXMODE_SPECIAL, _VERTEXMODE_DISPLACEMENT, _VERTEXMODE_EMISSIVE_MULT_ADD)]
        [InfoBox(Emission uses green channel and alpha gets multiplied by alpha channel, _VERTEXMODE_EMISSION)]
        [InfoBox(Displacement maps blue channel to displace along vertex normal by default. Toggle RGB Direction to map RGB values to vertex XYZ displacement, _VERTEXMODE_DISPLACEMENT)]
        [InfoBox(Red for Metallic _ Green for Emission _ Alpha for Smoothness, _VERTEXMODE_SPECIAL)]
        [InfoBox(Red for Metallic _ Alpha for Smoothness, _VERTEXMODE_METALSMOOTHNESS)]
        [InfoBox(Green for emission _ Alpha for texture emission fading, _VERTEXMODE_EMISSIVE_MULT_ADD)]
        [ShowIfAny(_VERTEXMODE_EMISSION, _VERTEXMODE_SPECIAL, _VERTEXMODE_EMISSIVE_MULT_ADD)] _EmissionThreshold ("Emission Threshold", Range(0,1)) = 0
        [ShowIfAny(_VERTEXMODE_EMISSION, _VERTEXMODE_SPECIAL, _VERTEXMODE_EMISSIVE_MULT_ADD)] _EmissionColor ("Emission Color", Color) = (1, 1, 1, 0)
        [ShowIfAny(_VERTEXMODE_EMISSION, _VERTEXMODE_SPECIAL, _VERTEXMODE_EMISSIVE_MULT_ADD)] _EmissionStrength ("Emission Strength", Float) = 1.0
        [ShowIfAny(_VERTEXMODE_EMISSION, _VERTEXMODE_SPECIAL, _VERTEXMODE_EMISSIVE_MULT_ADD)] _EmissionBloomIntensity ("Bloom Intensity", Float) = 1.0
        [EnumShowIf(3, None, MainEffect, Always, _VERTEXMODE_EMISSION, _VERTEXMODE_SPECIAL, _VERTEXMODE_EMISSIVE_MULT_ADD)] _Vertex_WhiteBoostType ("Vertex Color Treatment", Int) = 0
        [ShowIfAny(_VERTEXMODE_EMISSION, _VERTEXMODE_SPECIAL, _VERTEXMODE_EMISSIVE_MULT_ADD)] _QuestWhiteboostMultiplier ("Whiteboost Multiplier", Float) = 1.0

        [ToggleShowIfAny(DISPLACEMENT_SPATIAL, _VERTEXMODE_DISPLACEMENT)] _DisplacementSpatial ("RGB Direction", Int) = 0
        [ToggleShowIfAny(DISPLACEMENT_BIDIRECTIONAL, 2, DISPLACEMENT_SPATIAL, _VERTEXMODE_DISPLACEMENT)] _DisplacementBidirectional ("RGB Bidirectional", Int) = 0
        [InfoBox(Flat Spectrogram relies on a Spectrogram Row component, 2, _VERTEXMODE_DISPLACEMENT, _SPECTROGRAM_FLAT)]
        [InfoBox(Full Spectrogram uses uv3 coords to map 64 spectrogram channels, 2, _VERTEXMODE_DISPLACEMENT, _SPECTROGRAM_FULL)]
        [EnumShowIf(3, None, Flat, Full, _VERTEXMODE_DISPLACEMENT)] _Spectrogram ("Spectrogram", Int) = 0
        [ShowIfAny(_VERTEXMODE_DISPLACEMENT)] _DisplacementStrength ("Displacement Strength", Float) = 0.1
        [ShowIfAny(_VERTEXMODE_DISPLACEMENT)] _DisplacementAxisMultiplier ("Axis Multiplier", Vector) = (1.0, 1.0, 1.0, 1.0)

        [ToggleShowIfAny(VERTEXDISPLACEMENT_MASK, _VERTEXMODE_DISPLACEMENT)] _EnableVertexDisplacementMask ("Vertex Displacement Mask", Int) = 0
        [EnumHeader(Texture, _EMISSION_TEXTURE, _EMISSION_MASK, _SECONDARY_EMISSION_MASK)] _VertexDisplacement_Mask_Source ("Mask Source", Int) = 0
        [ShowIfAny(1, VERTEXDISPLACEMENT_MASK)] _VertexDisplacementMask ("Mask Texture", 2D) = "white" {}
        [ShowIfAny(1, VERTEXDISPLACEMENT_MASK)] _VertexDisplacementMaskMixer ("Mask Texture Mixer", Vector) = (1,1,1,1)
        [ShowIfAny(1, VERTEXDISPLACEMENT_MASK)] _VertexDisplacementMaskSpeed ("Mask Texture Speed", Vector) = (0,1,0,0)
        [EnumHeader(Multiply, Add, AddRGB)] _VertexDisplacementMaskMode ("Mask Mode", Int) = 0
        [ShowIfAny(1, VERTEXDISPLACEMENT_MASK)] _VertexDisplacementMaskMultiplier ("Mask Multiplier", Float) = 1.0
        [ShowIfAny(1, VERTEXDISPLACEMENT_MASK)] _VertexDisplacementMaskOffset ("Mask Offset", Float) = 0.0

        [BigHeader(EMISSIONS)]

        [Space(18)]
        [EnumHeader(None, Simple, Pulse, Flipbook)] _EmissionTexture ("Texture Emission", Int) = 0

        [SpaceShowIfAny(18, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)]

        [EnumShowIf(3, Texture, Fill, MPM G, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_FLIPBOOK)] _Emission_Texture_Source ("Emission Source", Int) = 0 //MPM G as an option for _EMISSIONTEXTURE_FLIPBOOK needs to be removed
        [ShowIfAny(1, _EMISSION_TEXTURE_SOURCE_TEXTURE, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_FLIPBOOK)] _EmissionTex ("Emission Texture", 2D) = "white" {}
        [VectorShowIfAny(2, 2, _EMISSIONTEXTURE_SIMPLE, _EMISSION_TEXTURE_SOURCE_TEXTURE)] _EmissionTexSpeed ("Texture Speed", Vector) = (0,0,0,0)
        [ToggleShowIfAny(SECONDARY_UVS_EMISSION, 2, 0_SECONDARY_UVS_NONE, _EMISSION_TEXTURE_SOURCE_TEXTURE, _EMISSIONTEXTURE_SIMPLE)] _SecondaryUVsEmissionTex ("Use Secondary UVs", Int) = 0

        [SpaceShowIfAny(12, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)]
        [EnumShowIf(3, Emission G, Copy Emission, MPM R, _EMISSIONTEXTURE_SIMPLE)] _Emission_Alpha_Source ("Alpha Source", Int) = 0

        _EmissionBrightness ("Brightness", Float) = 1.0
        [ToggleShowIfAny(ENABLE_EMISSION_ANGLE_DISAPPEAR, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)] _AngleDisappear ("Angle Disappear", Int) = 0
        [ShowIfAny(1, ENABLE_EMISSION_ANGLE_DISAPPEAR, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)] _ThresholdAngle ("Threshold Angle", Float) = 0.0

        [InfoBox(This will only Whiteboost on Quest, 1, _EMISSIONCOLORTYPE_MAINEFFECT, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)][Space(6)]
        [EnumShowIf(4, Flat, Whiteboost, Gradient, MainEffect, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)] _EmissionColorType ("Emission Color Treatment", Int) = 0
        [SpaceShowIfAny(6, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)]

        [ShowIfAny(1, 0_EMISSIONCOLORTYPE_GRADIENT, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)] _EmissionTexColor ("Emission Color", Color) = (1, 1, 1, 1)
        [ShowIfAny(1, _EMISSIONCOLORTYPE_GRADIENT, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)] _EmissionGradientTex ("Gradient LUT", 2D) = "white" {}

        _EmissionTexBloomIntensity ("Bloom Intensity", Float) = 1.0
        _EmissionTexWhiteboostMultiplier ("Whiteboost multiplier", Float) = 1.0

        [ShowIfAny(_EMISSIONTEXTURE_PULSE)] _PulseMask ("Pulse Mask", 2D) = "white" {}
        [ToggleShowIfAny(SECONDARY_UVS_PULSE, 2, 0_SECONDARY_UVS_NONE, _EMISSIONTEXTURE_PULSE)] _SecondaryUVsPulseTex ("Pulse Texture Secondary UVs", Int) = 0
        [ToggleShowIfAny(INVERT_PULSE, _EMISSIONTEXTURE_PULSE)] _InvertPulseTexture ("Invert Texture", Int) = 0
        [ToggleShowIfAny(PULSE_MULTIPLY_TEXTURE, _EMISSIONTEXTURE_PULSE)] _PulseMultiplyByTexture ("Brightness from Texture", Int) = 0
        [ShowIfAny(_EMISSIONTEXTURE_PULSE)] _PulseWidth ("Pulse Width", Float) = 0.1
        [ShowIfAny(_EMISSIONTEXTURE_PULSE)] _PulseSpeed ("Pulse Speed", Float) = 0.2
        [ShowIfAny(_EMISSIONTEXTURE_PULSE)] _PulseSmooth ("Pulse Smooth", Range(0.0, 0.2)) = 0.02

        [Space(12)]
        [InfoBox(Keep in sRGB or alpha will have different intensity, _EMISSIONTEXTURE_FLIPBOOK)]
        [InfoBox(Frame 1 contains frames 1234 in RGBA channels Frame 2 contains 4567 (5678 if blending is disabled), _EMISSIONTEXTURE_FLIPBOOK)]
        [ShowIfAny(_EMISSIONTEXTURE_FLIPBOOK)] _FlipbookColumns ("Flipbook Columns", Int) = 8
        [ShowIfAny(_EMISSIONTEXTURE_FLIPBOOK)] _FlipbookRows ("Flipbook Rows", Int) = 8
        [ShowIfAny(_EMISSIONTEXTURE_FLIPBOOK)] _FlipbookNonloopableFrames ("Full Non-loopable frames", Int) = 0
        [ShowIfAny(_EMISSIONTEXTURE_FLIPBOOK)] _FlipbookSpeed("Flipbook Speed", Float) = 1.0
        [ToggleShowIfAny(FLIPBOOK_BLENDING_OFF, _EMISSIONTEXTURE_FLIPBOOK)] _FlipbookBlendingOff ("No Frame Blending", Int) = 0

        [SpaceShowIfAny(18, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)]

        [ToggleShowIfAny(EMISSION_MASK, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE)] _EnableEmissionMask ("Layer 2", Int) = 0
        [InfoBox(Green channel is used for whiteboost and alpha bloom purposes, EMISSION_MASK)]
        [EnumShowIf(Multiply, Add, Masked Add, EMISSION_MASK)] _MaskBlend ("Layer Blend", Int) = 0
        [ShowIfAny(1, EMISSION_MASK, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_SIMPLE)] _EmissionMask ("Layer Texture", 2D) = "white" {}
        [ToggleShowIfAny(SECONDARY_UVS_EMISSION_MASK, 2, 0_SECONDARY_UVS_NONE, EMISSION_MASK)] _SecondaryUVsMask ("Use Secondary UVs", Int) = 0
        [VectorShowIfAny(2, 1, EMISSION_MASK, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_SIMPLE)] _EmissionMaskSpeed ("Layer Texture Speed", Vector) = (0,1,0,0)
        [ShowIfAny(1, EMISSION_MASK, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_SIMPLE)] _EmissionMaskIntensity ("Layer Intensity", Float) = 1.0

        [SpaceShowIfAny(18, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK)]
        [ToggleShowIfAny(SECONDARY_EMISSION_MASK, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE)] _EnableSecondaryEmissionMask ("Layer 3", Int) = 0
        [EnumShowIf(Multiply, Add, Masked Add, SECONDARY_EMISSION_MASK)] _Secondary_Mask_Blend ("Layer Blend", Int) = 0
        [ShowIfAny(1, SECONDARY_EMISSION_MASK, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_SIMPLE)] _SecondaryEmissionMask ("Layer Texture", 2D) = "white" {}
        [ToggleShowIfAny(SECONDARY_UVS_EMISSION_MASK2, 2, 0_SECONDARY_UVS_NONE, SECONDARY_EMISSION_MASK)] _SecondaryUVsMask2 ("Use Secondary UVs", Int) = 0
        [VectorShowIfAny(2, 1, SECONDARY_EMISSION_MASK, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_SIMPLE)] _SecondaryEmissionMaskSpeed ("Texture Speed", Vector) = (0,1,0,0)
        [ShowIfAny(1, SECONDARY_EMISSION_MASK, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_SIMPLE)] _SecondaryEmissionMaskIntensity ("Layer Intensity", Float) = 1.0

        [Space(12)]
        [EnumShowIf(4 , None, Mask, Secondary Mask, Emission Texture, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_SIMPLE)] _Emission_Step ("Step Emission", Int) = 0
        [ShowIfAny(_EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_SIMPLE)] _EmissionMaskStepValue ("Step Value", Range(0.0, 1.0)) = 0.5
        [ShowIfAny(_EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_SIMPLE)] _EmissionMaskStepWidth ("Step Width", Range(0.0, 0.5)) = 0.1
        [SpaceShowIfAny(12, 0_EMISSIONTEXTURE_NONE)]

        [Space(12)]
        [InfoBox(ERROR RGB parallax approach is not implemented yet, _PARALLAX_RGB)]
        [InfoBox(Parallax is an expensive feature so try to limit its screen space usage, 0_PARALLAX_NONE)]
        [EnumHeader(None, Flexible, RGB)] _Parallax ("Parallax Emission", Int) = 0
        [SpaceShowIfAny(12, 0_PARALLAX_NONE)]
        [ToggleShowIfAny(_PARALLAX_FLEXIBLE_REFLECTED, 0_PARALLAX_NONE)] _EnableReflectedDir ("Reflected Direction", Int) = 0
        [EnumShowIf(2, Planar, Warped, 0_PARALLAX_NONE)] _Parallax_Projection ("Parallax Projection", Int) = 0
        [ShowIfAny(0_PARALLAX_NONE)] _ParallaxColor ("Parallax Color", Color) = (1, 1, 1, 1)
        [ShowIfAny(0_PARALLAX_NONE)] _ParallaxMap ("Parallax Map", 2D) = "black" {} // TODO Rename to _ParallaxTexture when possible
        [ToggleShowIfAny(SECONDARY_UVS_PARALLAX, 2, 0_SECONDARY_UVS_NONE, 0_PARALLAX_NONE)] _SecondaryUVsParallax ("Parallax Texture Secondary UVs", Int) = 0
        [VectorShowIfAny(2, 0_PARALLAX_NONE)] _ParallaxTexSpeed ("Parallax Speed", Vector) = (0,0,0,0)
        [ShowIfAny(0_PARALLAX_NONE)] _ParallaxIntensity ("Parallax Intensity", Float) = 1.0
        [ShowIfAny(0_PARALLAX_NONE)] _ParallaxIntensity_Step ("Parallax Intensity Step", Float) = -0.25
        [SpaceShowIfAny(12, 0_PARALLAX_NONE)]
        [ShowIfAny(_PARALLAX_FLEXIBLE)] _Layers ("Layers", Range(2.0, 6.0)) = 3.0
        [ShowIfAny(0_PARALLAX_NONE)] _StartOffset ("Start Offset", Float) = 1.0
        [ShowIfAny(0_PARALLAX_NONE)] _OffsetStep ("Offset Step", Float) = 1.0
        [SpaceShowIfAny(12, 0_PARALLAX_NONE)]

        [ToggleShowIfAny(PARALLAX_IRIDESCENCE, 0_PARALLAX_NONE)] _Parallax_Iridescence ("Iridescence", Int) = 0
        [ShowIfAny(2, 0_PARALLAX_NONE, PARALLAX_IRIDESCENCE)] _IridescenceAxesMultiplier ("Axes Multiplier", Vector) = (1,2,3,0)
        [ShowIfAny(2, 0_PARALLAX_NONE, PARALLAX_IRIDESCENCE)] _IridescenceTiling ("Iridescence Tiling", Float) = 0.25
        [ShowIfAny(2, 0_PARALLAX_NONE, PARALLAX_IRIDESCENCE)] _IridescenceColorInfluence ("Color Influence", Range(0.0, 1.0)) = 0.0

        [InfoBox(Masked by Green channel of vertex color, 2, 0_PARALLAX_NONE, _PARALLAX_MASKING_VERTEX_COLOR)]
        [InfoBox(WARNING Try not to mask off significant areas to limit performance costs, 2, 0_PARALLAX_NONE, 0_PARALLAX_MASKING_NONE)]
        [EnumShowIf(3, None, Texture, Vertex Color, 0_PARALLAX_NONE)] _Parallax_Masking ("Mask by", Int) = 0
        [ShowIfAny(2, 0_PARALLAX_NONE, _PARALLAX_MASKING_TEXTURE)] _ParallaxMaskingMap ("Parallax Mask", 2D) = "white" {}
        [VectorShowIfAny(2, 2, 0_PARALLAX_NONE, _PARALLAX_MASKING_TEXTURE)] _ParallaxMaskSpeed ("Mask Speed", Vector) = (0,0,0,0)
        [ShowIfAny(2, 0_PARALLAX_NONE, _PARALLAX_MASKING_TEXTURE)] _ParallaxMaskIntensity ("Mask Intensity", Range(0.0, 1.0)) = 1.0

        [SpaceShowIfAny(18, 0_EMISSIONTEXTURE_NONE)]

        [Space(12)]
        [EnumHeader(None, Lerp, Additive)] _RimLight ("Rim Light Type", Int) = 0
        [ToggleShowIfAny(RIMLIGHT_INVERT, _RIMLIGHT_LERP, _RIMLIGHT_ADDITIVE)] _InvertRimlight ("Invert Rimlight", Int) = 0
        [ToggleShowIfAny(DIRECTIONAL_RIM, _RIMLIGHT_LERP, _RIMLIGHT_ADDITIVE)] _EnableDirectionalRim ("Make Rim Directional", Int) = 0
        [VectorShowIfAny(3, 1, DIRECTIONAL_RIM, _RIMLIGHT_LERP, _RIMLIGHT_ADDITIVE)] _RimPerpendicularAxis ("Rim Perpendicular Axis", Vector) = (0,1,0,0) // In Object Space
        [ShowIfAny(_RIMLIGHT_LERP, _RIMLIGHT_ADDITIVE)] _RimLightEdgeStart ("Rim Light Edge Start", Float) = 0.5
        [ShowIfAny(_RIMLIGHT_LERP, _RIMLIGHT_ADDITIVE)] _RimLightColor ("Rim Light Color", Color) = (1, 1, 1, 0)
        [ShowIfAny(_RIMLIGHT_LERP, _RIMLIGHT_ADDITIVE)] _RimLightIntensity ("Rim Light Intensity", Float) = 1.0
        [ShowIfAny(_RIMLIGHT_LERP, _RIMLIGHT_ADDITIVE)] _RimLightBloomIntensity ("Rim Light Bloom Intensity", Float) = 1.0
        [EnumShowIf(3, None, MainEffect, Always, _RIMLIGHT_LERP, _RIMLIGHT_ADDITIVE)] _Rim_WhiteBoostType ("Rimlight Color Treatment", Int) = 0
        [ShowIfAny(_RIMLIGHT_LERP, _RIMLIGHT_ADDITIVE)] _RimLightWhiteboostMultiplier ("Rim Light Whiteboost Multiplier", Float) = 1.0



        // to test



        // not implemented

        [BigHeader(LIGHTNING)]

        [Header(Ambient)]
        [Space(8)]
        _AmbientMinimalValue ("Ambient Minimum", Range(0.0, 1.0)) = 0.0
        _NominalDiffuseLevel ("Ambient Color", Color) = (0, 0, 0, 0)
        _AmbientMultiplier ("Ambient Color Multiplier", Float) = 1.0

        [Space(18)]
        [ToggleHeader(DIFFUSE)] _EnableDiffuse ("Diffuse", Int) = 1

        [InfoBox(Warning Light Falloff is VERY EXPENSIVE, 1, LIGHT_FALLOFF, DIFFUSE, SPECULAR)]
        [ToggleShowIfAny(LIGHT_FALLOFF, DIFFUSE, SPECULAR)] _EnableLightFalloff ("Light Falloff", Int) = 0

        [SpaceShowIfAny(12, DIFFUSE)]
        [ToggleShowIfAny(INVERT_DIFFUSE_NORMAL, DIFFUSE)] _InvertDiffuseNormal ("Invert Diffuse Normal", Int) = 0
        [ToggleShowIfAny(BOTH_SIDES_DIFFUSE, DIFFUSE)] _EnableBothSidesDiffuse ("Both Sides Diffuse", Int) = 0
        [ShowIfAny(2, BOTH_SIDES_DIFFUSE, DIFFUSE)] _BothSidesDiffuseMultiplier ("Far Side Multiplier", Float) = 1.0


        [Space(12)]
        [ToggleHeader(PRIVATE_POINT_LIGHT)] _PrivatePointLight ("Private Point Light", Int) = 0
        [ToggleShowIfAny(INSTANCED_PRIVATE_POINT_LIGHT, PRIVATE_POINT_LIGHT)] _InstancedPrivatePointLightColor ("Instance Color", Int) = 0
        [ShowIfAny(PRIVATE_POINT_LIGHT)] [HDR] _PrivatePointLightColor ("Color", Color) = (1, 0, 0, 0)
        [ToggleShowIfAny(POINT_LIGHT_IS_LOCAL, PRIVATE_POINT_LIGHT)] _PointLightPositionLocal ("Make Position Local", Int) = 0
        [ShowIfAny(PRIVATE_POINT_LIGHT)] _PrivatePointLightIntensity ("Intensity Multiplier", Float) = 1.0
        [ShowIfAny(PRIVATE_POINT_LIGHT)] _PrivatePointLightPosition ("Light World Position", Vector) = (0, 0, 0)

        [Space(12)]
        [ToggleHeader(DIFFUSE_TEXTURE)] _EnableDiffuseTexture ("Albedo Texture", Int) = 0
        [InfoBox(ERROR MPM Texture must be used to use its channels, 3, DIFFUSE_TEXTURE, 0METAL_SMOOTHNESS_TEXTURE, 0_DIFFUSE_TEXTURE_SOURCE_TEXTURE)]
        [EnumShowIf(3, Texture, MPM R, MPM A Smoothness, DIFFUSE_TEXTURE)] _Diffuse_Texture_Source ("Diffuse Texture Source", Int) = 0
        [ShowIfAny(2, DIFFUSE_TEXTURE, _DIFFUSE_TEXTURE_SOURCE_TEXTURE)] _DiffuseTexture ("Diffuse Texture", 2D) = "white" {}
        [ToggleShowIfAny(SECONDARY_UVS_DIFFUSE, 2, 0_SECONDARY_UVS_NONE, DIFFUSE_TEXTURE)] _SecondaryUVsDiffuse ("Diffuse Texture Secondary UVs", Int) = 0
        [InfoBox(Albedo from Smoothness uses inverted smoothness value, 3, DIFFUSE_TEXTURE, _DIFFUSE_TEXTURE_SOURCE_MPM_A_SMOOTHNESS)]
        [ShowIfAny(2, DIFFUSE_TEXTURE, _DIFFUSE_TEXTURE_SOURCE_MPM_A_SMOOTHNESS)] _AlbedoMultiplier ("Albedo multiplier", Float) = 1.0

        [Space(12)]
        [ToggleHeader(SPECULAR)] _EnableSpecular ("Specular", Int) = 1
        [ShowIfAny(SPECULAR)] _SpecularIntensity ("Specular Intensity", Float) = 1.0

        [Space(12)]
        [ToggleHeader(LIGHTMAP)] _EnableLightmap ("Lightmap", Int) = 0

        [Space(12)]
        [ToggleHeader(NORMAL_MAP)] _EnableNormalMap ("Normal Map", Int) = 0
        [ShowIfAny(NORMAL_MAP)] _NormalTexture ("Normal Texture", 2D) = "bump" {}
        [ToggleShowIfAny(SECONDARY_UVS_NORMAL, 2, 0_SECONDARY_UVS_NONE, NORMAL_MAP)] _SecondaryUVsNormal ("Normal Map Secondary UVs", Int) = 0
        [ShowIfAny(NORMAL_MAP)] _NormalScale ("Normal Scale", Float) = 1.0

        [Space(12)]
        [ToggleHeader(USE_SPHERICAL_NORMAL_OFFSET)] _UseSphericalNormalOffset ("Spherical Normal Offset", Int) = 0
        [ShowIfAny(USE_SPHERICAL_NORMAL_OFFSET)] _SphericalNormalOffsetIntensity ("Spherical Normal Offset Intensity", Float) = 0.5
        [ShowIfAny(USE_SPHERICAL_NORMAL_OFFSET)] _SphericalNormalOffsetCenter ("Spherical Normal Offset Center", Vector) = (0.0, 0.0, 0.0)

        [BigHeader(REFLECTIONS)]

        [Space(12)]
        [ToggleHeader(REFLECTION_TEXTURE)] _EnableReflectionTexture ("Reflection Texture", Int) = 0
        [ShowIfAny(REFLECTION_TEXTURE)] _ReflectionTexIntensity ("Texture Intensity", Float) = 1.0
        [ShowIfAny(REFLECTION_TEXTURE)] _EnvironmentReflectionCube ("Environment Reflection", CUBE) = "" {}

        [Space(12)]
        [ToggleHeader(REFLECTION_PROBE)] _EnableReflectionProbe ("Reflection Probe", Int) = 0

        [SpaceShowIfAny(12, REFLECTION_PROBE)]
        [EnumShowIf(2, Fast, Precise, REFLECTION_PROBE)] _Probe_Calculation ("Probe Calculations", Int) = 0
        [ToggleShowIfAny(REFLECTION_PROBE_DISABLED_WHITEBOOST, REFLECTION_PROBE)]  _ReflectionProbeDisabledWhiteboost ("Disable Probe Whiteboost", Int) = 0 // Future Tooltip: Also makes probe cheaper to calculate
        [ShowIfAny(2, REFLECTION_PROBE, _PROBE_CALCULATION_PRECISE)] _ReflectionProbeGrayscale ("Probe Grayscale Factor", Range(0,1)) = 0.2
        [ShowIfAny(2, REFLECTION_PROBE, _PROBE_CALCULATION_PRECISE)] _ColoredMetalMultiplier ("Colored Metal Multiplier", Range(0.0, 15.0)) = 3.5
        [ShowIfAny(2, REFLECTION_PROBE, _PROBE_CALCULATION_PRECISE)] _WhiteOffset ("White Offset", Float) = 2

        [SpaceShowIfAny(12, REFLECTION_PROBE)]
        [ShowIfAny(REFLECTION_PROBE)] _ReflectionProbeIntensity ("Probe Intensity", Float) = 1.0
        [ToggleShowIfAny(REFLECTION_PROBE_BOX_PROJECTION, REFLECTION_PROBE)] _ReflectionProbeBoxProjection ("Box Projection", Int) = 1
        [ToggleShowIfAny(REFLECTION_PROBE_BOX_PROJECTION_OFFSET, 2, REFLECTION_PROBE, REFLECTION_PROBE_BOX_PROJECTION)] _EnableBoxProjectionOffset ("Box Projection Offset", Int) = 0
        [ShowIfAny(3, REFLECTION_PROBE, REFLECTION_PROBE_BOX_PROJECTION, REFLECTION_PROBE_BOX_PROJECTION_OFFSET)] _ReflectionProbeBoxProjectionSizeOffset ("Box Projection Size Offset", Vector) = (0, 0, 0, 0)
        [ShowIfAny(3, REFLECTION_PROBE, REFLECTION_PROBE_BOX_PROJECTION, REFLECTION_PROBE_BOX_PROJECTION_OFFSET)] _ReflectionProbeBoxProjectionPositionOffset ("Box Projection Position Offset", Vector) = (0, 0, 0, 0)
        [ToggleShowIfAny(REFLECTION_STATIC, REFLECTION_PROBE)] _ReflectionStatic ("Static Reflection", Int) = 0
        [ToggleShowIfAny(MULTIPLY_REFLECTIONS, 2, REFLECTION_PROBE, REFLECTION_TEXTURE)] _MultiplyReflections ("Multiply Reflection Texture", Int) = 1

//        [SpaceShowIfAny(12, REFLECTION_PROBE, REFLECTION_TEXTURE)]
//        [ToggleHeader(OVERRIDE_REFLECTION_SMOOTHNESS, REFLECTION_PROBE, REFLECTION_TEXTURE)] _OverrideProbeSmoothness ("Override Reflection Smoothness", Float) = 0
//        [ShowIfAny(2, REFLECTION_PROBE, OVERRIDE_REFLECTION_SMOOTHNESS)] _ReflectionSmoothness ("Reflection Smoothness", Range(0,1)) = 0.5

        [SpaceShowIfAny(12, REFLECTION_PROBE, REFLECTION_TEXTURE)]
        [ToggleHeader(ENABLE_RIM_DIM, REFLECTION_PROBE, REFLECTION_TEXTURE)] _EnableRimDim ("Reflection Rim Dim", Float) = 0
        [ShowIfAny(ENABLE_RIM_DIM)] _RimScale ("Rim Scale", Float) = 1.0
        [ShowIfAny(ENABLE_RIM_DIM)] _RimOffset ("Rim Offset", Float) = 1.0
        [ShowIfAny(ENABLE_RIM_DIM)] _RimCameraDistanceOffset ("Rim Camera Distance Offset", Float) = 2.0
        [ShowIfAny(ENABLE_RIM_DIM)] _RimCameraDistanceScale ("Rim Camera Distance Scale", Float) = 0.3
        [ShowIfAny(ENABLE_RIM_DIM)] _RimSmoothness ("Rim Smoothness", Float) = 1.0
        [ShowIfAny(ENABLE_RIM_DIM)] _RimDarkening ("Rim Darkening", Float) = 0.0
        [ToggleShowIfAny(INVERT_RIM_DIM, ENABLE_RIM_DIM)] _InvertRimDim ("Invert Rim Dim", Float) = 0

        [BigHeader(OCCLUSION AND GROUND FADE)]

        [Space(12)]
        [ToggleHeader(GROUND_FADE)] _EnableGroundFade ("Height Occlusion", Int) = 0
        [ShowIfAny(GROUND_FADE)] _GroundFadeScale ("Height Occlusion Scale", Float) = 0.5
        [ShowIfAny(GROUND_FADE)] _GroundFadeOffset ("Height Occlusion Offset", Float) = 1.0

        [Space(12)]
        [ToggleHeader(OCCLUSION)] _EnableOcclusion ("Texture Occlusion", Int) = 0
        [InfoBox(ERROR MPM Texture must be used to use its channels, 2, OCCLUSION, 0METAL_SMOOTHNESS_TEXTURE, _OCCLUSION_SOURCE_MPM_B, _OCCLUSION_SOURCE_AVATAR_MPM_R)]
        [EnumShowIf(3, Texture, MPM B, Avatar MPM R, OCCLUSION)] _Occlusion_Source ("Occlusion Source", Int) = 0
        [ShowIfAny(2, OCCLUSION, _OCCLUSION_SOURCE_TEXTURE)] _DirtTex ("Occlusion Texture", 2D) = "white" {} // TODO Rename once we're using Unity 2021.3.26+ (cannot be automated on earlier versions due to Unity bug)
        [ToggleShowIfAny(SECONDARY_UVS_OCCLUSION, 2, 0_SECONDARY_UVS_NONE, _OCCLUSION_SOURCE_TEXTURE, OCCLUSION)] _SecondaryUVsOcclusion ("Occlusion Map Secondary UVs", Int) = 0
        [ShowIfAny(OCCLUSION)] _OcclusionIntensity ("Occlusion Intensity", Range(0.0, 1.0)) = 1.0

        [Space(12)]
        [ToggleHeader(OCCLUSION_DETAIL)] _EnableOcclusionDetail ("Texture Occlusion Detail", Int) = 0
        [ShowIfAny(OCCLUSION_DETAIL)] _DirtDetailTex ("Occlusion Detail Texture", 2D) = "white" {} // TODO Rename once we're using Unity 2021.3.26+ (cannot be automated on earlier versions due to Unity bug)
        [ToggleShowIfAny(SECONDARY_UVS_OCCLUSION_DETAIL, 2, 0_SECONDARY_UVS_NONE, OCCLUSION_DETAIL)] _SecondaryUVsOcclusionDetail ("Occlusion Detail Secondary UVs", Int) = 0
        [ShowIfAny(OCCLUSION_DETAIL)] _OcclusionDetailIntensity ("Occlusion Detail Intensity", Range(0.0, 1.0)) = 1.0

        [SpaceShowIfAny(12, OCCLUSION_DETAIL, OCCLUSION)]
        [InfoBox(WARNING Paired with ACES approach of Before Emissive (currently true) this will exempt occlusion from ACES color grading, 2, _ACES_APPROACH_BEFORE_EMISSIVE, OCCLUSION_BEFORE_EMISSION, OCCLUSION_DETAIL, OCCLUSION)]
        [ToggleShowIfAny(OCCLUSION_BEFORE_EMISSION, OCCLUSION, OCCLUSION_DETAIL)] _OcclusionBeforeEmission ("Texture Occlusion Before Emission", Int) = 0

        [BigHeader(OTHER)]

        [Space(16)]
        _EnableRotateUV ("Rotate UVs 90", Int) = 0
        _Rotate_UV ("Rotation Angle", Float) = 0.0

        [Space(12)]
        [Toggle(UV_COLOR_SEGMENTS)] _UVColorSegments ("UV Color Segments", Int) = 0
        [ToggleShowIfAny(UV_SEGMENTS_IGNORE_RIM, UV_COLOR_SEGMENTS)] _UvSegmentsIgnoreRim ("Don't override Rim color", Int) = 0

        [Space(12)]
        [ToggleHeader(HIGHLIGHT_SELECTION)] _HighlightSelection ("Highlight Selection", Int) = 0
        [ShowIfAny(HIGHLIGHT_SELECTION)] _SegmentToHighlight ("Segment To Highlight", Int) = -1

        [Space(12)]
        [ToggleHeader(FOG)] _EnableFog ("Fog", Int) = 1
        [ShowIfAny(FOG)] _FogStartOffset ("Fog Start Offset", Float) = 0.0
        [ShowIfAny(FOG)] _FogScale ("Fog Scale", Float) = 1.0
        [ToggleShowIfAny(HEIGHT_FOG, FOG)] _EnableHeightFog ("Height Fog", Int) = 0
        [ShowIfAny(2, FOG, HEIGHT_FOG)] _FogHeightScale ("Fog Height Scale", Float) = 1.0
        [ShowIfAny(2, FOG, HEIGHT_FOG)] _FogHeightOffset ("Fog Height Offset", Float) = 0.0
        [ToggleShowIfAny(HEIGHT_FOG_DEPTH_SOFTEN, 2, FOG, HEIGHT_FOG)] _EnableHeightFogSoften ("Soften with Distance", Int) = 0
        [ShowIfAny(3, FOG, HEIGHT_FOG, HEIGHT_FOG_DEPTH_SOFTEN)] _FogSoften ("Soften Scale", Float) = 1.0
        [ShowIfAny(3, FOG, HEIGHT_FOG, HEIGHT_FOG_DEPTH_SOFTEN)] _FogSoftenOffset ("Soften Offset", Float) = 1.0

        [ShowIfAny(1, FOG, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK, _VERTEXMODE_EMISSION, _VERTEXMODE_SPECIAL)] _EmissionFogSuppression ("Quest Fog Supression", Range(0.0, 1.0)) = 0.0
        [ShowIfAny(1, FOG, _EMISSIONTEXTURE_SIMPLE, _EMISSIONTEXTURE_PULSE, _EMISSIONTEXTURE_FLIPBOOK, _VERTEXMODE_EMISSION, _VERTEXMODE_SPECIAL)] _MainEffectFogSuppression ("MainEffect Fog Supression", Range(0.0, 1.0)) = 0.0

        [Space(18)]
        [ToggleHeader(COLOR_BY_FOG)] _ColorFog ("Color by Fog", Int) = 0
        [ShowIfAny(COLOR_BY_FOG)] _ColorFogMultiplier ("Fog Multiplier", Float) = 1
        [ShowIfAny(COLOR_BY_FOG)] _ColorFogMax ("Fog Max Brightness", Float) = 1.0
        [ShowIfAny(COLOR_BY_FOG)] _ColorFogInfluence ("Color Influence", Range(0,1)) = 0.5
        [ToggleShowIfAny(FOG_COLOR_HIGHLIGHT, COLOR_BY_FOG)] _FogColorHighlight ("Fog Highlight", Int) = 0
        [ShowIfAny(2, COLOR_BY_FOG, FOG_COLOR_HIGHLIGHT)] _ColorFogHighlightMultiplier ("Fog Highlight Multiplier", Float) = 30000

        [Space(12)]
        [ToggleHeader(DISTANCE_DARKENING)] _EnableDistanceDarkening ("Worldspace Occlusion", Int) = 0
        [ShowIfAny(DISTANCE_DARKENING)] _DarkeningScale ("Scale", Float) = 0.35
        [ShowIfAny(DISTANCE_DARKENING)] _DarkeningIntensity ("Intensity", Float) = 1.0
        [VectorShowIfAny(3, DISTANCE_DARKENING)] _DarkeningCenter ("Center", Vector) = (0,0,0,0)
        [VectorShowIfAny(3, DISTANCE_DARKENING)] _DarkeningDirection ("Axes", Vector) = (1,1,1,1)

        [Space(12)]
        [EnumHeader(None, Grid, Scanline, Legacy)] _Hologram ("Hologram Effect", Int) = 0
        [ToggleShowIfAny(HOLOGRAM_MATERIALIZATION, _HOLOGRAM_GRID)] _UseHologramMaterialization ("Materialization", Int) = 0
        [ShowIfAny(_HOLOGRAM_GRID, _HOLOGRAM_SCANLINE, _HOLOGRAM_LEGACY)] _HologramColor ("Hologram Color", Color) = (1,1,1,1)
        [ShowIfAny(_HOLOGRAM_GRID, _HOLOGRAM_LEGACY)] _HologramGridSize ("Hologram Grid Size", Float) = 3.0

        [ShowIfAny(_HOLOGRAM_GRID)] _HologramFill ("Hologram Fill", Float) = -0.6
        [ShowIfAny(_HOLOGRAM_GRID, _HOLOGRAM_SCANLINE)] _HologramStripeSpeed ("Hologram Stripe Speed", Float) = 1.43
        [ShowIfAny(_HOLOGRAM_GRID, _HOLOGRAM_SCANLINE)] _HologramScanDistance ("Hologram Scan Distance", Float) = 2.0
        [ShowIfAny(_HOLOGRAM_GRID, _HOLOGRAM_SCANLINE)] _HologramPhaseOffset ("Hologram Phase Offset", Range(-1.0, 1.0)) = 0.0
        [ShowIfAny(2, _HOLOGRAM_GRID, HOLOGRAM_MATERIALIZATION)] _HoloMaterialize ("Hologram Materialize", Range(0.0, 1.0)) = 1.0
        [ShowIfAny(_HOLOGRAM_GRID, _HOLOGRAM_SCANLINE)] _HoloIntensity ("Hologram Intensity", Float) = 1.0
        [ShowIfAny(_HOLOGRAM_GRID, _HOLOGRAM_SCANLINE)] _HaltScan ("Halt scanning", Int) = 0

        [Space(12)]
        [ToggleHeader(FAKE_MIRROR_TRANSPARENCY)] _FakeMirrorTransparencyEnabled ("Fake Mirror Transparency", Int) = 0
        [ShowIfAny(FAKE_MIRROR_TRANSPARENCY)] _FakeMirrorTransparencyMultiplier ("Mirror Transparency Multiplier", Float) = 1.0
        [ToggleShowIfAny(MIRROR_VERTEX_DISTORTION, FAKE_MIRROR_TRANSPARENCY)] _NoteVertexDistortion ("Mirror Vertex Distortion", Float) = 0

        [Space(12)]
        [EnumHeader(None, HD dissolve, LW snap)] Note_Plane_Cut ("Note Plane Cut", Int) = 0
        [SpaceShowIfAny(8, NOTE_PLANE_CUT_HD_DISSOLVE NOTE_PLANE_CUT_LW_SNAP)]
        [ShowIfAny(NOTE_PLANE_CUT_HD_DISSOLVE NOTE_PLANE_CUT_LW_SNAP)] _CutPlaneEdgeGlowWidth ("Plane Edge Glow Width", Float) = 0.01
        [ShowIfAny(NOTE_PLANE_CUT_HD_DISSOLVE NOTE_PLANE_CUT_LW_SNAP)] _NoteSize ("Note Size", Float) = 0.25
        [ShowIfAny(NOTE_PLANE_CUT_HD_DISSOLVE NOTE_PLANE_CUT_LW_SNAP)] _CutPlane ("Cut Plane", Vector) = (1.0, 0.0, 0.0, 0.0) // x, y, z defines the plane normal and w is D from Ax + By + Cz + D = 0

        [Space(12)]
        [EnumHeader(None, HD dissolve, LW scale)] Cutout_Type ("Cutout", Int) = 0
        [SpaceShowIfAny(8, CUTOUT_TYPE_HD_DISSOLVE, CUTOUT_TYPE_LW_SCALE)]
        [ShowIfAny(CUTOUT_TYPE_HD_DISSOLVE, CUTOUT_TYPE_LW_SCALE)] _Cutout ("Cutout Threshold", Range(0,1)) = 0
        [ShowIfAny(CUTOUT_TYPE_HD_DISSOLVE)] _CutoutTexScale ("Cutout Texture Scale", Float) = 1.0
        [ToggleShowIfAny(CLOSE_TO_CAMERA_CUTOUT, CUTOUT_TYPE_HD_DISSOLVE, CUTOUT_TYPE_LW_SCALE)] _EnableCloseToCameraCutout ("Close to Camera Cutout", Float) = 0
        [ShowIfAny(1, CLOSE_TO_CAMERA_CUTOUT, CUTOUT_TYPE_HD_DISSOLVE, CUTOUT_TYPE_LW_SCALE)] _CloseToCameraCutoutOffset ("Close to Camera Cutout Offset", Float) = 0.5
        [ShowIfAny(1, CLOSE_TO_CAMERA_CUTOUT, CUTOUT_TYPE_HD_DISSOLVE, CUTOUT_TYPE_LW_SCALE)] _CloseToCameraCutoutScale ("Close to Camera Cutout Scale", Float) = 0.5

        [SpaceShowIfAny(24, CUTOUT_TYPE_HD_DISSOLVE, CUTOUT_TYPE_LW_SCALE, NOTE_PLANE_CUT_HD_DISSOLVE NOTE_PLANE_CUT_LW_SNAP)]
        [InfoBox(Color is only visible with MainEffect ON as on Quest it is fully whiteboosted, CUTOUT_TYPE_HD_DISSOLVE, CUTOUT_TYPE_LW_SCALE, NOTE_PLANE_CUT_HD_DISSOLVE NOTE_PLANE_CUT_LW_SNAP)]
        [ShowIfAny(CUTOUT_TYPE_HD_DISSOLVE, CUTOUT_TYPE_LW_SCALE, NOTE_PLANE_CUT_HD_DISSOLVE NOTE_PLANE_CUT_LW_SNAP)] _GlowCutoutColor ("Cut/Cutout Glow Color", Color) = (1,1,1,1) // Feels better for generic use if we keep it in and set extra
        [SpaceShowIfAny(12, CUTOUT_TYPE_HD_DISSOLVE, CUTOUT_TYPE_LW_SCALE, NOTE_PLANE_CUT_HD_DISSOLVE NOTE_PLANE_CUT_LW_SNAP)]

        [Space(12)]
        [ToggleHeader(DISSOLVE)] _EnableDissolve ("Dissolve", Int) = 0

        [InfoBox(Warning this can easily create weird situation with backfaces and self transparency, 1, DISSOLVE, _DISSOLVEALPHA_FADE, _DISSOLVEALPHA_BOTH)]
        [InfoBox(Use Color Blending of One OneMinusSrcAlpha and Alpha Blending of Zero OneMinusSrcAlpha for best results, 1, DISSOLVE, _DISSOLVEALPHA_FADE, _DISSOLVEALPHA_BOTH)]
        [EnumShowIf(Clip, Fade, Both, DISSOLVE)] _DissolveAlpha ("Alpha Aproach", Int) = 0

        [ShowIfAny(1, DISSOLVE, _DISSOLVEALPHA_FADE, _DISSOLVEALPHA_BOTH)] _AlphaMultiplier ("Alpha Multiplier", Float) = 1.0
        [ShowIfAny(1, DISSOLVE, _DISSOLVEALPHA_FADE, _DISSOLVEALPHA_BOTH)] _DissolveGradientWidth ("Fade Falloff Scale", Float) = 5.0

        [Space(16)]
        [InfoBox(To change direction it is better to use negative Axis Vector instead of Dissolve invert, 1, DISSOLVE)]
        [FloatToggleShowIfAny(DISSOLVE)] _InvertDissolve ("Invert Dissolve", Float) = 0.0
        [EnumShowIf(Local, World, World Centered, Uv, Avatar, DISSOLVE)] _Dissolve_Space ("Dissolve Space", Int) = 0

        [SpaceShowIfAny(12, 2, DISSOLVE, _DISSOLVEAXIS_AVATAR)]
        [ShowIfAny(2, DISSOLVE, _DISSOLVEAXIS_AVATAR)] _FadeStartY ("Body Fade Fully Opaque Y", Float) = 1.25
        [ShowIfAny(2, DISSOLVE, _DISSOLVEAXIS_AVATAR)] _FadeEndY ("Body Fade Fully Transparent Y", Float) = 1.0
        [ShowIfAny(2, DISSOLVE, _DISSOLVEAXIS_AVATAR)] _FadeZoneInterceptX ("Fade Zone Intercept X", Float) = 0.2
        [ShowIfAny(2, DISSOLVE, _DISSOLVEAXIS_AVATAR)] _FadeZoneSlope ("Fade Zone Slope", Float) = 0.76
        [ShowIfAny(2, DISSOLVE, _DISSOLVEAXIS_AVATAR)] _BodyFadeGamma ("Fadeout Factor", Float) = 2.0

        [ShowIfAny(2, DISSOLVE, 0_DISSOLVEAXIS_AVATAR)] _DissolveAxisVector ("Dissolve Axis", Vector) = (0,1,0,0)

        [ToggleShowIfAny(DISSOLVE_PROGRESS, DISSOLVE)] _UseDissolveProgress ("Dissolve Progress", Int) = 0
        [ShowIfAny(3, DISSOLVE, 0_DISSOLVEAXIS_AVATAR, 0DISSOLVE_PROGRESS)] _DissolveOffset ("Dissolve Offset", Float) = 0.0
        [ShowIfAny(3, DISSOLVE, DISSOLVE_PROGRESS, 0_DISSOLVEAXIS_AVATAR)] _DissolveStartValue ("Dissolve Start Value", Float) = 0.0
        [ShowIfAny(3, DISSOLVE, DISSOLVE_PROGRESS, 0_DISSOLVEAXIS_AVATAR)] _DissolveEndValue ("Dissolve End Value", Float) = 10.0
        [ShowIfAny(3, DISSOLVE, DISSOLVE_PROGRESS, 0_DISSOLVEAXIS_AVATAR)] _DissolveProgress ("Dissolve Progress", Range(-1.0, 1.0)) = 0.0

        [SpaceShowIfAny(24, 2, DISSOLVE, DISSOLVE_PROGRESS)]
        [ToggleShowIfAny(DISSOLVE_COLOR, 1, DISSOLVE)] _UseDissolveColor ("Dissolve Color", Int) = 0
        [ShowIfAny(2, DISSOLVE, DISSOLVE_COLOR)] _DissolveColor ("Dissolve Color", Color) = (0, 1, 1, 0)
        [ShowIfAny(2, DISSOLVE, DISSOLVE_COLOR)] _DissolveColorIntensity ("Color Intensity", Float) = 1.0
        [ShowIfAny(2, DISSOLVE, DISSOLVE_COLOR)] _CutColorFalloff ("Cut Falloff Scale", Float) = 4
        [ShowIfAny(2, DISSOLVE, DISSOLVE_COLOR)] _CutColorBacksideFalloff ("Backface Falloff Multiplier", Float) = 0.07

        [SpaceShowIfAny(24, 2, DISSOLVE, DISSOLVE_COLOR)]
        [EnumShowIf(4, None, Local, World, Uv, DISSOLVE, DISSOLVE_COLOR)] _Dissolve_Grid ("Dissolve Grid", Int) = 0
        [ShowIfAny(3, DISSOLVE, 0_DISSOLVE_GRID_NONE, DISSOLVE_COLOR)] _GridThickness ("Grid Thickness", Float) = 1.5
        [ShowIfAny(3, DISSOLVE, 0_DISSOLVE_GRID_NONE, DISSOLVE_COLOR)] _GridSize ("Grid Size", Float) = 10
        [ShowIfAny(3, DISSOLVE, 0_DISSOLVE_GRID_NONE, DISSOLVE_COLOR)] _GridFalloff ("Grid Falloff Scale", Float) = 4
        [ShowIfAny(3, DISSOLVE, 0_DISSOLVE_GRID_NONE, DISSOLVE_COLOR)] _GridSpeed ("Grid Speed", Float) = 0.1

        [SpaceShowIfAny(24, 3, DISSOLVE, 0_DISSOLVE_GRID_NONE, DISSOLVE_COLOR)]
        [ToggleShowIfAny(DISSOLVE_TEXTURE, 1, DISSOLVE)] _UseDissolveTexture ("Dissolve Texture", Int) = 0
        [ShowIfAny(2, DISSOLVE, DISSOLVE_TEXTURE)] _DissolveTexture ("Dissolve Texture", 2D) = "black" {}
        [VectorShowIfAny(2, 2, DISSOLVE, DISSOLVE_TEXTURE)] _DissolveTextureSpeed ("Texture Speed", Vector) = (0,0,0,0)
        [ShowIfAny(2, DISSOLVE, DISSOLVE_TEXTURE)] _DissolveTextureInfluence ("Texture Influence", Float) = 0.2
        [SpaceShowIfAny(24, 2, DISSOLVE, DISSOLVE_TEXTURE)]

        // Feature currently not supported as it wasn't used for all the time it was available.
        // If needed again, its implementation needs to update according to the new DISSOLVE
        //[ToggleShowIfAny(DISSOLVE_DISPLACEMENT, 1, DISSOLVE)] _EnableDissolveDisplacement ("Enable Dissolve Displacement", Int) = 0
        //[ShowIfAny(2, DISSOLVE, DISSOLVE_DISPLACEMENT)] _DissolveDisplacementTex ("Dissolve Displacement Texture", 2D) = "black" { }
        //[ShowIfAny(2, DISSOLVE, DISSOLVE_DISPLACEMENT)] _DissolveDisplacementStrength ("Strength", Float) = 0.1
        //[VectorShowIfAny(3, 2, DISSOLVE, DISSOLVE_DISPLACEMENT)] _DissolveDisplacementAxes ("Per Axis Strength", Vector) = (1, 1, 1, 0)
        //[VectorShowIfAny(2, 2, DISSOLVE, DISSOLVE_DISPLACEMENT)] _DissolveDisplacementPanning ("Panning", Vector) = (0, 0, 0, 0)
        //[ShowIfAny(2, DISSOLVE, DISSOLVE_DISPLACEMENT)] _DissolveDisplacementOverallSpeed ("Overall Speed", Float) = 1.0

        [Space(12)]
        [EnumHeader(None, Simple)] Distortion ("Distortion", Int) = 0
        [SpaceShowIfAny(12, DISTORTION_SIMPLE)]
        [EnumShowIf(10, MPM, EmissionTex, Emission Mask, Secondary Emission Mask, Pulse, Parralax, Diffuse, Normal, Occlusion, Occlusion Detail, DISTORTION_SIMPLE)] _Distortion_Target ("Distortion Target", Int) = 0

        [Space(12)]
        [InfoBox(WARNING Emission Mask must be enabled to use with Distortion Target Emission Mask, 2, 0EMISSION_MASK, _DISTORTION_TARGET_EMISSION_MASK)]
        [InfoBox(WARNING Use MPM Texture must be enabled to use with Distortion Target MPM, 2, 0METAL_SMOOTHNESS_TEXTURE, _DISTORTION_TARGET_MPM)]
        [InfoBox(WARNING Distortion Target Emission Texture will not work when Emission Source is not set to Texture, 2, _DISTORTION_TARGET_EMISSIONTEX, 0_EMISSION_TEXTURE_SOURCE_TEXTURE)]
        [ShowIfAny(DISTORTION_SIMPLE)] _DistortionTex ("Distortion Texture", 2D) = "black" {}
        [ToggleShowIfAny(SECONDARY_UVS_DISTORTION, 2, 0_SECONDARY_UVS_NONE, DISTORTION_SIMPLE)] _SecondaryUVsDistortion ("Distortion Secondary UVs", Int) = 0
        [ShowIfAny(DISTORTION_SIMPLE)] _DistortionStrength ("Distortion Strength", Float) = 0.2
        [ShowIfAny(DISTORTION_SIMPLE)] _DistortionAxes ("Distortion Axes", Vector) = (1,1,0,0)
        [ShowIfAny(DISTORTION_SIMPLE)] _DistortionPanning ("Distortion Panning", Vector) = (0,0,0,0)
        [SpaceShowIfAny(12, DISTORTION_SIMPLE)]

        [Space(12)]
        [Header(Other)][Space]

        [Toggle(NOISE_DITHERING)] _EnableNoiseDithering ("Noise Dithering", Int) = 1
        [Toggle(LINEAR_TO_GAMMA)] _LinearToGamma ("LinearToGamma", Int) = 0
        [KeywordEnum(Standard, Song Time, Freeze)] _Custom_Time ("Time Behavior", Int) = 0
        [KeywordEnum(None, Around_X, Around_Y, Around_Z)] _Curve_Vertices ("Curve Vertices", Int) = 0
        [InfoBox(WARNING Any emission may appear unexpectedly toned down unless Before Emissive approach is used, _ACES_APPROACH_AFTER_EMISSIVE)]
        [KeywordEnum(After Emissive, Before Emissive)] _Aces_Approach ("ACES approach", Int) = 0 // We'd ideally only use Before Emissive approach, but keeping the option and default here until we confirm this doesn't cause issues with old content

        [BigHeader(SETTINGS)]

        [Space(12)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Toggle] _ZWrite ("Z Write", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
        _StencilRefValue ("Stencil Ref Value", int) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comp Func", Int) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass("Stencill Pass Op", Int) = 0

        [Space(12)]
        [Header(Color Blending)][Space]
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrcFactor ("Foreground Factor", Int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDstFactor ("Background Factor", Int) = 0
        [Header(Bloom Blending)][Space]
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrcFactorA ("Foreground Factor", Int) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDstFactorA ("Background Factor", Int) = 0

        [Space(12)]
        [Toggle(MESH_PACKING)] _MeshPacking ("Mesh Packed Instancing", Int) = 0
        [InfoBox(Id below is for debug only and needs to be set via Material Property Blocks, MESH_PACKING)]
        [ShowIfAny(MESH_PACKING)] _MeshPackingId ("Mesh Packing Id", Float) = 1.0
        [InfoBox(ERROR Color Array needs Texture or Vertex Emission enabled to take effect, 4, COLOR_ARRAY, 0_EMISSION_TEXTURE_SOURCE_NONE, 0_VERTEXMODE_SPECIAL, 0_VERTEXMODE_EMISSION)]
        [InfoBox(WARNING Color Array overrides emission colors, 1, COLOR_ARRAY, _EMISSION_TEXTURE_SOURCE_NONE, _VERTEXMODE_SPECIAL, _VERTEXMODE_EMISSION )]
        [InfoBox(Color Array is limited to 150 IDs, COLOR_ARRAY)]
        [Toggle(COLOR_ARRAY)] _UseColorArray ("Color Array", Int) = 0

        _BlendingPreset ("Dummy Custom Shader Inspector Property", Float) = 0.0
        _StencilPreset ("Dummy Custom Shader Inspector Property", Float) = 1.0
        _LightingPreset ("Dummy Custom Shader Inspector Property", Float) = 0.0
        _BloomPreset ("Dummy Custom Shader Inspector Property", Float) = 0.0
        _BlendSrcAlphaPreset ("Dummy Custom Shader Inspector Property", Float) = 0.0
    }

    CustomEditor "BGLib.ShaderInspector.SimpleLitShaderInspector"

    SubShader {

        Tags {
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Stencil {
            Ref [_StencilRefValue]
            Comp [_StencilComp]
            Pass [_StencilPass]
        }

        Pass {
            Cull [_Cull]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Blend [_BlendSrcFactor] [_BlendDstFactor], [_BlendSrcFactorA] [_BlendDstFactorA]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            //#pragma multi_compile_fwdbase
            // We can potentially use multi_compile_fwdbase instead of these two multicompiles, but it probably produces more unneeded variants.
            // If something breaks in the build which uses mesh normals and lightmapping, this should be the first suspect.
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ UNITY_ASSUME_UNIFORM_SCALING

            #pragma multi_compile _ ENABLE_BLOOM_FOG
            #pragma multi_compile _ ACES_TONE_MAPPING
            #pragma multi_compile _ MAIN_EFFECT_ENABLED

            // We plan to remove this feature for launch but leaving it until then
            #if MIRROR_VERTEX_DISTORTION && FAKE_MIRROR_TRANSPARENCY
                #pragma multi_compile _ NOTE_VERTEX_DISTORTION
            #endif
            #pragma multi_compile _ OVERDRAW_VIEW

            #pragma shader_feature FOG
            #pragma shader_feature HEIGHT_FOG
            #pragma shader_feature PRECISE_NORMAL
            #pragma shader_feature DISSOLVE
            //#pragma shader_feature DISSOLVE_DISPLACEMENT
            #pragma shader_feature DISTANCE_DARKENING
            #pragma shader_feature HEIGHT_FOG_DEPTH_SOFTEN
            #pragma shader_feature MESH_PACKING
            #pragma shader_feature FLIPBOOK_BLENDING_OFF
            #pragma shader_feature _ _CUSTOM_TIME_SONG_TIME _CUSTOM_TIME_FREEZE
            #pragma shader_feature _ACES_APPROACH_BEFORE_EMISSIVE
            #pragma shader_feature COLOR_ARRAY

            #pragma shader_feature_local _ _DISSOLVE_SPACE_WORLD _DISSOLVE_SPACE_WORLD_CENTERED _DISSOLVE_SPACE_UV _DISSOLVE_SPACE_AVATAR
            #pragma shader_feature_local DISSOLVE_PROGRESS
            #pragma shader_feature_local DIRECTIONAL_RIM
            #pragma shader_feature_local INVERT_RIM
            #pragma shader_feature_local DIFFUSE
            #pragma shader_feature_local SPECULAR
            #pragma shader_feature_local METAL_SMOOTHNESS_TEXTURE
            #pragma shader_feature_local LIGHTMAP
            #pragma shader_feature_local DIFFUSE_TEXTURE
            #pragma shader_feature_local LIGHT_FALLOFF
            #pragma shader_feature_local PRIVATE_POINT_LIGHT
            #pragma shader_feature_local POINT_LIGHT_IS_LOCAL
            #pragma shader_feature_local OCCLUSION
            #pragma shader_feature_local OCCLUSION_DETAIL
            #pragma shader_feature_local GROUND_FADE
            #pragma shader_feature_local USE_SPHERICAL_NORMAL_OFFSET
            #pragma shader_feature_local UV_COLOR_SEGMENTS
            #pragma shader_feature_local UV_SEGMENTS_IGNORE_RIM
            #pragma shader_feature_local _ _HOLOGRAM_GRID _HOLOGRAM_SCANLINE _HOLOGRAM_LEGACY
            #pragma shader_feature_local HIGHLIGHT_SELECTION
            #pragma shader_feature_local _ _VERTEXMODE_COLOR _VERTEXMODE_EMISSIVE_MULT_ADD _VERTEXMODE_EMISSION _VERTEXMODE_METALSMOOTHNESS _VERTEXMODE_DISPLACEMENT _VERTEXMODE_SPECIAL
            #pragma shader_feature_local NOISE_DITHERING
            #pragma shader_feature_local INVERT_DIFFUSE_NORMAL
            #pragma shader_feature_local BOTH_SIDES_DIFFUSE
            #pragma shader_feature_local LINEAR_TO_GAMMA
            #pragma shader_feature_local _ _RIM_WHITEBOOSTTYPE_MAINEFFECT _RIM_WHITEBOOSTTYPE_ALWAYS
            #pragma shader_feature_local _ _VERTEX_WHITEBOOSTTYPE_MAINEFFECT _VERTEX_WHITEBOOSTTYPE_ALWAYS
            #pragma shader_feature_local _ _RIMLIGHT_LERP _RIMLIGHT_ADDITIVE
            #pragma shader_feature_local RIMLIGHT_INVERT
            #pragma shader_feature_local REFLECTION_TEXTURE
            #pragma shader_feature_local REFLECTION_PROBE
            #pragma shader_feature_local REFLECTION_PROBE_BOX_PROJECTION
            #pragma shader_feature_local REFLECTION_PROBE_BOX_PROJECTION_OFFSET
            #pragma shader_feature_local REFLECTION_STATIC
            #pragma shader_feature_local MULTIPLY_REFLECTIONS
            // #pragma shader_feature_local OVERRIDE_REFLECTION_SMOOTHNESS
            #pragma shader_feature_local NORMAL_MAP
            #pragma shader_feature_local ENABLE_RIM_DIM
            #pragma shader_feature_local INVERT_RIM_DIM
            #pragma shader_feature_local SPECULAR_ANTIFLICKER
            #pragma shader_feature_local EMISSION_MASK
            #pragma shader_feature_local SECONDARY_EMISSION_MASK
            #pragma shader_feature_local _ _MASKBLEND_ADD _MASKBLEND_MASKED_ADD
            #pragma shader_feature_local _ _SECONDARY_MASK_BLEND_ADD _SECONDARY_MASK_BLEND_MASKED_ADD
            #pragma shader_feature_local _ _EMISSION_STEP_MASK _EMISSION_STEP_SECONDARY_MASK _EMISSION_STEP_EMISSION_TEXTURE
            #pragma shader_feature_local _ _SECONDARY_UVS_IMPORT _SECONDARY_UVS_EXTERNAL_SCALE _SECONDARY_UVS_OBJECT_SPACE _SECONDARY_UVS_ADDITIVE_OFFSET
            #pragma shader_feature_local SECONDARY_UVS_EMISSION
            #pragma shader_feature_local SECONDARY_UVS_MPM
            #pragma shader_feature_local SECONDARY_UVS_PULSE
            #pragma shader_feature_local SECONDARY_UVS_EMISSION_MASK
            #pragma shader_feature_local SECONDARY_UVS_EMISSION_MASK2
            #pragma shader_feature_local SECONDARY_UVS_DIFFUSE
            #pragma shader_feature_local SECONDARY_UVS_PARALLAX
            #pragma shader_feature_local SECONDARY_UVS_NORMAL
            #pragma shader_feature_local SECONDARY_UVS_OCCLUSION
            #pragma shader_feature_local SECONDARY_UVS_OCCLUSION_DETAIL
            #pragma shader_feature_local PULSE_MULTIPLY_TEXTURE
            #pragma shader_feature_local INVERT_PULSE
            #pragma shader_feature_local _ _EMISSIONTEXTURE_SIMPLE _EMISSIONTEXTURE_PULSE _EMISSIONTEXTURE_FLIPBOOK
            #pragma shader_feature_local _ _EMISSIONCOLORTYPE_WHITEBOOST _EMISSIONCOLORTYPE_GRADIENT _EMISSIONCOLORTYPE_MAINEFFECT
            #pragma shader_feature_local ENABLE_EMISSION_ANGLE_DISAPPEAR
            #pragma shader_feature_local ROTATE_UV
            #pragma shader_feature_local INSTANCED_PRIVATE_POINT_LIGHT
            #pragma shader_feature_local DISPLACEMENT_SPATIAL
            #pragma shader_feature_local DISPLACEMENT_BIDIRECTIONAL
            #pragma shader_feature_local VERTEXDISPLACEMENT_MASK
            #pragma shader_feature_local _ _VERTEXDISPLACEMENT_MASK_SOURCE_EMISSION_TEXTURE _VERTEXDISPLACEMENT_MASK_SOURCE_EMISSION_MASK _VERTEXDISPLACEMENT_MASK_SOURCE_SECONDARY_EMISSION_MASK
            #pragma shader_feature_local _ _VERTEXDISPLACEMENT_MASK_ADDITIVE _VERTEXDISPLACEMENT_MASK_ADDITIVERGB //default is Multiplicative behaviour
            #pragma shader_feature_local _ _SPECTROGRAM_FLAT _SPECTROGRAM_FULL
            #pragma shader_feature_local _ _CURVE_VERTICES_AROUND_X _CURVE_VERTICES_AROUND_Y _CURVE_VERTICES_AROUND_Z
            #pragma shader_feature_local _ _METALLIC_TEXTURE_MPM_R _METALLIC_TEXTURE_MPM_A _METALLIC_TEXTURE_MPM_AVATAR_B
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_MPM_A _SMOOTHNESS_TEXTURE_MPM_G_ROUGHNESS
            #pragma shader_feature_local _ _EMISSION_TEXTURE_SOURCE_FILL _EMISSION_TEXTURE_SOURCE_MPM_G
            #pragma shader_feature_local _ _EMISSION_ALPHA_SOURCE_COPY_EMISSION _EMISSION_ALPHA_SOURCE_MPM_R
            #pragma shader_feature_local _ _OCCLUSION_SOURCE_MPM_B _OCCLUSION_SOURCE_AVATAR_MPM_R
            #pragma shader_feature_local _ _DIFFUSE_TEXTURE_SOURCE_MPM_R _DIFFUSE_TEXTURE_SOURCE_MPM_A_SMOOTHNESS
            #pragma shader_feature_local _PROBE_CALCULATION_PRECISE
            #pragma shader_feature_local REFLECTION_PROBE_DISABLED_WHITEBOOST
            #pragma shader_feature_local MPM_CUSTOM_MIP
            #pragma shader_feature_local AVATAR_COMPUTE_SKINNING
            #pragma shader_feature_local _ _DISSOLVEALPHA_FADE _DISSOLVEALPHA_BOTH
            #pragma shader_feature_local DISSOLVE_COLOR
            #pragma shader_feature_local DISSOLVE_TEXTURE
            #pragma shader_feature_local HOLOGRAM_MATERIALIZATION
            #pragma shader_feature_local FAKE_MIRROR_TRANSPARENCY
            #pragma shader_feature_local _ CUTOUT_TYPE_HD_DISSOLVE CUTOUT_TYPE_LW_SCALE
            #pragma shader_feature_local CLOSE_TO_CAMERA_CUTOUT
            #pragma shader_feature_local _ NOTE_PLANE_CUT_HD_DISSOLVE NOTE_PLANE_CUT_LW_SNAP
            #pragma shader_feature_local MIRROR_VERTEX_DISTORTION
            #pragma shader_feature_local OCCLUSION_BEFORE_EMISSION
            #pragma shader_feature_local _ _PARALLAX_FLEXIBLE _PARALLAX_RGB
            #pragma shader_feature_local _PARALLAX_FLEXIBLE_REFLECTED
            #pragma shader_feature_local _ _PARALLAX_MASKING_TEXTURE _PARALLAX_MASKING_VERTEX_COLOR
            #pragma shader_feature_local PARALLAX_IRIDESCENCE
            #pragma shader_feature_local _PARALLAX_PROJECTION_WARPED
            #pragma shader_feature_local COLOR_BY_FOG
            #pragma shader_feature_local FOG_COLOR_HIGHLIGHT
            #pragma shader_feature_local _ DISTORTION_SIMPLE
            #pragma shader_feature_local SECONDARY_UVS_DISTORTION
            #pragma shader_feature_local _ _DISSOLVE_GRID_LOCAL _DISSOLVE_GRID_WORLD _DISSOLVE_GRID_UV
            #pragma shader_feature_local _ _DISTORTION_TARGET_NORMAL _DISTORTION_TARGET_DIFFUSE _DISTORTION_TARGET_OCCLUSION _DISTORTION_TARGET_OCCLUSION_DETAIL _DISTORTION_TARGET_EMISSIONTEX _DISTORTION_TARGET_PULSE _DISTORTION_TARGET_EMISSION_MASK _DISTORTION_TARGET_SECONDARY_EMISSION_MASK _DISTORTION_TARGET_PARRALAX

            // Needed for Avatar Compute Skinning
            #pragma target 3.5 // necessary for use of SV_VertexID

            #if !_DIFFUSE_TEXTURE_SOURCE_MPM_R && !_DIFFUSE_TEXTURE_SOURCE_MPM_A_SMOOTHNESS
                #define _DIFFUSE_TEXTURE_SOURCE_TEXTURE
            #endif

            #if !_EMISSION_TEXTURE_SOURCE_FILL && !_EMISSION_TEXTURE_SOURCE_MPM_G
                #define _EMISSION_TEXTURE_SOURCE_TEXTURE
            #endif

            // CONDITION DEFINITIONS
            #if OCCLUSION || OCCLUSION_DETAIL || UV_COLOR_SEGMENTS || defined(_DIFFUSE_TEXTURE_SOURCE_TEXTURE) || METAL_SMOOTHNESS_TEXTURE || NORMAL_MAP || DISSOLVE || DISSOLVE_DISPLACEMENT || defined(_EMISSION_TEXTURE_SOURCE_TEXTURE) || _EMISSIONTEXTURE_PULSE || METAL_SMOOTHNESS_TEXTURE || _PARALLAX_FLEXIBLE || _PARALLAX_RGB || (_VERTEXMODE_DISPLACEMENT && VERTEXDISPLACEMENT_MASK)
                #define USES_UV
            #endif

            #if _EMISSIONTEXTURE_SIMPLE || _EMISSIONTEXTURE_FLIPBOOK || _EMISSIONTEXTURE_PULSE
                #define EMISSION_DEFINED
            #endif

            #if _SECONDARY_UVS_IMPORT || _SECONDARY_UVS_OBJECT_SPACE || _SECONDARY_UVS_EXTERNAL_SCALE || _SECONDARY_UVS_ADDITIVE_OFFSET
                #define SECONDARY_UV_DEFINED
            #endif

            #if DIFFUSE || SPECULAR || _RIMLIGHT_LERP || _RIMLIGHT_ADDITIVE || REFLECTION_PROBE || NORMAL_MAP || SPECULAR_ANTIFLICKER || PRIVATE_POINT_LIGHT
                #define USES_FRAG_NORMAL
            #endif

            #if _EMISSION_STEP_MASK || _EMISSION_STEP_SECONDARY_MASK || _EMISSION_STEP_EMISSION_TEXTURE
                #define EMISSION_STEP_ON
            #endif

            #include "UnityCG.cginc"
            #include "Assets/Visuals/Shaders/Rendering/CustomFog.cginc"
            #include "Assets/Visuals/Shaders/Rendering/PhongLighting.cginc"
            #include "Assets/Visuals/Shaders/Rendering/OverdrawView.cginc"
            #include "Assets/Visuals/Shaders/Rendering/MainEffectHelpers.cginc"
            #include "Assets/Visuals/Shaders/Rendering/FakeMirrorTransparency.cginc"

            #if ACES_TONE_MAPPING
                #include "Assets/Visuals/Shaders/Rendering/Tonemapping.cginc"
            #endif

            #if NOISE_DITHERING
                #include "Assets/Visuals/Shaders/Rendering/BlueNoise.cginc"
            #endif

            #if CUTOUT_TYPE_HD_DISSOLVE
                #include "Assets/Visuals/Shaders/Rendering/Cutout3D.cginc"
            #endif

            #if _HOLOGRAM_GRID || _HOLOGRAM_SCANLINE || _HOLOGRAM_LEGACY
                #include "Assets/Visuals/Shaders/Rendering/Hologram.cginc"
            #endif

            #if NORMAL_MAP
                #include "UnityStandardUtils.cginc"
            #endif

            #if REFLECTION_PROBE || LIGHTMAP || REFLECTION_TEXTURE
                #include "Assets/Visuals/Shaders/Rendering/Lightmapping.cginc"
            #endif

            #if NOTE_VERTEX_DISTORTION && MIRROR_VERTEX_DISTORTION && FAKE_MIRROR_TRANSPARENCY
                #include "Assets/Visuals/Shaders/Rendering/FakeMirrorVertexDistortion.cginc"
            #endif

            #if DISSOLVE && (_DISSOLVE_GRID_LOCAL || (!_DISSOLVE_SPACE_WORLD && !_DISSOLVE_SPACE_WORLD_CENTERED && !_DISSOLVE_SPACE_UV))
                #define DISSOLVE_LOCAL
            #endif

            #if DISSOLVE && (_DISSOLVE_GRID_LOCAL || _DISSOLVE_GRID_WORLD || _DISSOLVE_GRID_UV)
                #define DISSOLVE_GRID
            #endif

            #if !_DISTORTION_TARGET_EMISSIONTEX && !_DISTORTION_TARGET_EMISSION_MASK && !_DISTORTION_TARGET_SECONDARY_EMISSION_MASK && !_DISTORTION_TARGET_PULSE && !_DISTORTION_TARGET_PARRALAX && !_DISTORTION_TARGET_DIFFUSE && !_DISTORTION_TARGET_NORMAL && !_DISTORTION_TARGET_OCCLUSION && !_DISTORTION_TARGET_OCCLUSION_DETAIL
                #define _DISTORTION_TARGET_MPM
            #endif

            #if !_OCCLUSION_SOURCE_MPM_B && !_OCCLUSION_SOURCE_AVATAR_MPM_R
                #define _OCCLUSION_SOURCE_TEXTURE
            #endif

            half _Smoothness;
            half _Metallic;
            half _EmissionFogSuppression;
            half _MainEffectFogSuppression;
            half2 _InputUvMultiplier;
            half _AmbientMinimalValue;
            half _AmbientMultiplier;

            float4 _TimeHelperOffset;

            #if METAL_SMOOTHNESS_TEXTURE
                sampler2D _MetalSmoothnessTex;
                float4 _MetalSmoothnessTex_ST;
                #if MPM_CUSTOM_MIP
                    int _MpmMipBias;
                #endif
            #endif

            #if SPECULAR_ANTIFLICKER
                half _AntiflickerStrength;
                half _AntiflickerDistanceScale;
                half _AntiflickerDistanceOffset;
            #endif

            #if _RIMLIGHT_LERP || _RIMLIGHT_ADDITIVE
                half _RimLightEdgeStart;
                half _RimLightIntensity;
                half _RimLightBloomIntensity;
                half _RimLightWhiteboostMultiplier;
                #if DIRECTIONAL_RIM
                    half3 _RimPerpendicularAxis;
                #endif
            #endif

            #if OCCLUSION
                half _OcclusionIntensity;
                #if defined(_OCCLUSION_SOURCE_TEXTURE)
                    sampler2D _DirtTex;
                    float4 _DirtTex_ST;
                #endif
            #endif

            #if OCCLUSION_DETAIL
                sampler2D _DirtDetailTex;
                float4 _DirtDetailTex_ST;
            #endif

            #if GROUND_FADE
                half _GroundFadeScale;
                half _GroundFadeOffset;
            #endif

            #if DISTANCE_DARKENING
                half3 _DarkeningCenter;
                half _DarkeningScale;
                half _DarkeningIntensity;
                half3 _DarkeningDirection;
            #endif

            #if _VERTEXMODE_EMISSION || _VERTEXMODE_SPECIAL || _VERTEXMODE_EMISSIVE_MULT_ADD
                half _EmissionThreshold;
                half _EmissionStrength;
                half _QuestWhiteboostMultiplier;
                half _EmissionBloomIntensity;
            #endif

            #if _VERTEXMODE_DISPLACEMENT && VERTEXDISPLACEMENT_MASK
                sampler2D _VertexDisplacementMask;
                float4 _VertexDisplacementMask_ST;
                half2 _VertexDisplacementMaskSpeed;
                half4 _VertexDisplacementMaskMixer;
                float _VertexDisplacementMaskMultiplier;
                float _VertexDisplacementMaskOffset;
            #endif

            #ifdef EMISSION_DEFINED
                half _EmissionBrightness;
                half _EmissionTexBloomIntensity;
                half _EmissionTexWhiteboostMultiplier;
                #ifdef EMISSION_STEP_ON
                    half _EmissionMaskStepValue;
                    half _EmissionMaskStepWidth;
                #endif
            #endif

            #if ENABLE_EMISSION_ANGLE_DISAPPEAR
                half _ThresholdAngle;
            #endif

            #if _EMISSIONCOLORTYPE_GRADIENT
                sampler2D _EmissionGradientTex;
                float4 _EmissionGradientTex_ST;
            #endif

            #if (defined(_EMISSION_TEXTURE_SOURCE_TEXTURE) && (_EMISSIONTEXTURE_SIMPLE || _EMISSIONTEXTURE_FLIPBOOK)) || (_VERTEXMODE_DISPLACEMENT && VERTEXDISPLACEMENT_MASK && _VERTEXDISPLACEMENT_MASK_SOURCE_EMISSION_TEXTURE)
                sampler2D _EmissionTex;
                float4 _EmissionTex_ST;
            #endif

            #if _EMISSIONTEXTURE_SIMPLE
                half2 _EmissionTexSpeed;
            #endif

            #if EMISSION_MASK || (_VERTEXMODE_DISPLACEMENT && VERTEXDISPLACEMENT_MASK && _VERTEXDISPLACEMENT_MASK_SOURCE_EMISSION_MASK)
                sampler2D _EmissionMask;
                float4 _EmissionMask_ST;
                half2 _EmissionMaskSpeed;
            #endif

            #if SECONDARY_EMISSION_MASK || (_VERTEXMODE_DISPLACEMENT && VERTEXDISPLACEMENT_MASK && _VERTEXDISPLACEMENT_MASK_SOURCE_SECONDARY_EMISSION_MASK)
                sampler2D _SecondaryEmissionMask;
                float4 _SecondaryEmissionMask_ST;
                half2 _SecondaryEmissionMaskSpeed;
                half _SecondaryEmissionMaskIntensity;
            #endif

            #if _EMISSIONTEXTURE_PULSE
                sampler2D _PulseMask;
                float4 _PulseMask_ST;
                half _PulseWidth;
                half _PulseSpeed;
                half _PulseSmooth;
            #endif

            #if FOG
                half _FogStartOffset;
                half _FogScale;
            #endif

            #if COLOR_BY_FOG
                half _ColorFogMultiplier;
                half _ColorFogHighlightMultiplier;
                half _ColorFogInfluence;
                half _ColorFogMax;
            #endif

            #if HEIGHT_FOG
                half _FogHeightScale;
                half _FogHeightOffset;
                #if HEIGHT_FOG_DEPTH_SOFTEN
                    half _FogSoften;
                    half _FogSoftenOffset;
                #endif
            #endif

            #if DIFFUSE_TEXTURE && defined(_DIFFUSE_TEXTURE_SOURCE_TEXTURE)
                sampler2D _DiffuseTexture;
                float4 _DiffuseTexture_ST;
            #endif

            #if DIFFUSE || SPECULAR || LIGHTMAP || PRIVATE_POINT_LIGHT
                half _BothSidesDiffuseMultiplier;
                half _SpecularIntensity;
            #endif

            #if _DIFFUSE_TEXTURE_SOURCE_MPM_A_SMOOTHNESS && DIFFUSE_TEXTURE
                half _AlbedoMultiplier;
            #endif

            #if REFLECTION_TEXTURE
                samplerCUBE _EnvironmentReflectionCube;
                half _ReflectionTexIntensity;
            #endif

            #if REFLECTION_PROBE || REFLECTION_TEXTURE
                half _ReflectionProbeIntensity;
                #include "Assets/Visuals/Shaders/Rendering/CubeMapping.cginc"
            #endif

            #if REFLECTION_PROBE && _PROBE_CALCULATION_PRECISE
                half _ReflectionProbeGrayscale;
                half _ColoredMetalMultiplier;
                half _WhiteOffset;
            #endif

            // #if OVERRIDE_REFLECTION_SMOOTHNESS
            //     float _ReflectionSmoothness;
            // #endif

            #if NORMAL_MAP
                sampler2D _NormalTexture;
                float4 _NormalTexture_ST;
                half _NormalScale;
            #endif

            #if CUTOUT_TYPE_HD_DISSOLVE
                half _CutoutTexScale;
            #endif

            #if _PARALLAX_FLEXIBLE || _PARALLAX_RGB
                half _Layers;
                half _StartOffset;
                half _OffsetStep;
                sampler2D _ParallaxMap;
                float4 _ParallaxMap_ST;
                half _ParallaxIntensity_Step;
                half _ParallaxIntensity;
                half2 _ParallaxTexSpeed;
                #if _PARALLAX_MASKING_TEXTURE
                    sampler2D _ParallaxMaskingMap;
                    float4 _ParallaxMaskingMap_ST;
                    half2 _ParallaxMaskSpeed;
                    half _ParallaxMaskIntensity;
                #endif
                #if PARALLAX_IRIDESCENCE
                    half3 _IridescenceAxesMultiplier;
                    half _IridescenceTiling;
                #endif
            #endif

            #if DISSOLVE
                half3 _DissolveAxisVector;
                half _DissolveOffset;
                #if DISSOLVE_PROGRESS
                    half _DissolveStartValue;
                    half _DissolveEndValue;
                    half _DissolveProgress;
                #endif
                #if DISSOLVE_COLOR
                    fixed4 _DissolveColor;
                    half _DissolveColorIntensity;
                    half _CutColorFalloff;
                    half _CutColorBacksideFalloff;
                    #if defined(DISSOLVE_GRID)
                        half _GridThickness;
                        half _GridSize;
                        half _GridFalloff;
                        half _GridSpeed;
                    #endif
                #endif
                #if DISSOLVE_TEXTURE
                    sampler2D _DissolveTexture;
                    float4 _DissolveTexture_ST;
                    half2 _DissolveTextureSpeed;
                #endif
                half _DissolveTextureInfluence;
                half _DissolveGradientWidth;
                half _InvertDissolve;
                #if _DISSOLVEALPHA_FADE || _DISSOLVEALPHA_BOTH
                    half _AlphaMultiplier;
                #endif

                // #if DISSOLVE_DISPLACEMENT
                //     sampler2D _DissolveDisplacementTex;
                //     half4 _DissolveDisplacementTex_ST;
                //     half2 _DissolveDisplacementPanning;
                //     half _DissolveDisplacementStrength;
                //     half3 _DissolveDisplacementAxes;
                //     half _DissolveDisplacementOverallSpeed;
                // #endif
                #if _DISSOLVEAXIS_AVATAR
                    half _FadeZoneInterceptX;
                    half _FadeZoneSlope;
                    half _FadeEndY;
                    half _FadeStartY;
                #endif
            #endif





            #if UV_COLOR_SEGMENTS
                fixed4 _UVColors[10];
                fixed4 _UVRimColors[10];
            #endif

            #if HIGHLIGHT_SELECTION
                int _SegmentToHighlight;
            #endif

            #if PRIVATE_POINT_LIGHT
                float3 _PrivatePointLightPosition;
                half _PrivatePointLightIntensity;
                #if !INSTANCED_PRIVATE_POINT_LIGHT
                    half3 _PrivatePointLightColor;
                #endif
            #endif

            #if USE_SPHERICAL_NORMAL_OFFSET
                half _SphericalNormalOffsetIntensity;
                half3 _SphericalNormalOffsetCenter;
            #endif

            #if _HOLOGRAM_GRID || _HOLOGRAM_LEGACY
                half _HologramGridSize;
            #endif

            #if _HOLOGRAM_GRID || _HOLOGRAM_SCANLINE
                half _HologramStripeWidth;
                half _HologramScanDistance;
                half _HoloIntensity;
            #endif

            #if ENABLE_RIM_DIM
                half _RimScale;
                half _RimOffset;
                half _RimCameraDistanceOffset;
                half _RimCameraDistanceScale;
                half _RimDarkening;
                half _RimSmoothness;
            #endif

            #if _CUSTOM_TIME_SONG_TIME
                float4 _SongTime;
            #endif

            #if _SPECTROGRAM_FULL
                #define SPECTROGRAM_SIZE 64
                float _SpectrogramData[SPECTROGRAM_SIZE];
            #endif

            #if COLOR_ARRAY
                fixed4 _ColorsArray[200];
            #endif

            #if _EMISSIONTEXTURE_FLIPBOOK
                half _FlipbookColumns;
                half _FlipbookRows;
                half _FlipbookNonloopableFrames;
                half _FlipbookSpeed;
            #endif

            #if CLOSE_TO_CAMERA_CUTOUT && (CUTOUT_TYPE_HD_DISSOLVE || CUTOUT_TYPE_LW_SCALE)
                float _CloseToCameraCutoutOffset;
                float _CloseToCameraCutoutScale;
            #endif

            #if NOTE_PLANE_CUT_LW_SNAP || NOTE_PLANE_CUT_HD_DISSOLVE
                half _CutPlaneEdgeGlowWidth;
                half _NoteSize;
            #endif

            #if DISTORTION_SIMPLE
                sampler2D _DistortionTex;
                float4 _DistortionTex_ST;
                half2 _DistortionAxes;
                half2 _DistortionPanning;
            #endif

            #if ROTATE_UV
                half _Rotate_UV;
            #endif

            #if AVATAR_COMPUTE_SKINNING
                #define OVR_VERTEX_HAS_VERTEX_ID
                #define OVR_VERTEX_POSITION_FIELD_NAME vertex
                #define OVR_VERTEX_NORMAL_FIELD_NAME normal
                #define OVR_VERTEX_TANGENT_FIELD_NAME tangent
                #define OVR_VERTEX_VERT_ID_FIELD_NAME v_Id
                #define OVR_VERTEX_TEXCOORD_FIELD_NAME uv
                #define OVR_VERTEX_COLOR_FIELD_NAME color
                #include "Packages/com.meta.xr.sdk.avatars/Scripts/ShaderUtils/AvatarCustomTypes.cginc"
                #include "Packages/com.meta.xr.sdk.avatars/Scripts/ShaderUtils/AvatarCustom.cginc"
            #else
                struct OvrDefaultAppdata {

                  #ifdef AVATAR_COMPUTE_SKINNING
                      OVR_DEFAULT_VERTEX_FIELDS
                  #else

                    float4 vertex : POSITION;
                    half3 normal : NORMAL;

                    #if _VERTEXMODE_COLOR || _VERTEXMODE_EMISSION || _VERTEXMODE_METALSMOOTHNESS || _VERTEXMODE_SPECIAL || _VERTEXMODE_DISPLACEMENT
                        fixed4 color : COLOR;
                    #endif

                    #ifdef USES_UV
                        float2 uv : TEXCOORD0;
                    #endif

                    #if LIGHTMAP
                        float2 lightmapUv : TEXCOORD1;
                    #elif MESH_PACKING
                        float2 uv2 : TEXCOORD1;
                    #endif

                    #if NORMAL_MAP || DISSOLVE_DISPLACEMENT
                        float4 tangent : TANGENT;
                    #endif

                    UNITY_VERTEX_INPUT_INSTANCE_ID

                  #endif
                };
            #endif

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float, _TimeOffset)
                UNITY_DEFINE_INSTANCED_PROP(fixed3, _NominalDiffuseLevel)

                #if _SECONDARY_UVS_EXTERNAL_SCALE || _SECONDARY_UVS_OBJECT_SPACE
                   UNITY_DEFINE_INSTANCED_PROP(float3, _UVScale)
                #endif

                #if _SECONDARY_UVS_ADDITIVE_OFFSET
                    UNITY_DEFINE_INSTANCED_PROP(float2, _AdditiveUVOffset)
                #endif

                #if _VERTEXMODE_EMISSION || _VERTEXMODE_SPECIAL || _VERTEXMODE_EMISSIVE_MULT_ADD
                    UNITY_DEFINE_INSTANCED_PROP(fixed4, _EmissionColor)
                #endif

                #if _PARALLAX_FLEXIBLE || _PARALLAX_RGB
                    UNITY_DEFINE_INSTANCED_PROP(fixed4, _ParallaxColor)
                    #if PARALLAX_IRIDESCENCE
                        UNITY_DEFINE_INSTANCED_PROP(half, _IridescenceColorInfluence)
                    #endif
                #endif

                #ifdef EMISSION_DEFINED
                    UNITY_DEFINE_INSTANCED_PROP(fixed4, _EmissionTexColor)
                #endif

                #if _RIMLIGHT_LERP || _RIMLIGHT_ADDITIVE
                    UNITY_DEFINE_INSTANCED_PROP(fixed4, _RimLightColor)
                #endif

                #if CUTOUT_TYPE_HD_DISSOLVE || CUTOUT_TYPE_LW_SCALE
                    UNITY_DEFINE_INSTANCED_PROP(half, _Cutout)
                    #if CUTOUT_TYPE_HD_DISSOLVE
                        UNITY_DEFINE_INSTANCED_PROP(half4, _CutoutTexOffset)
                    #endif
                #endif

                #if PRIVATE_POINT_LIGHT && INSTANCED_PRIVATE_POINT_LIGHT
                    UNITY_DEFINE_INSTANCED_PROP(half3, _PrivatePointLightColor)
                #endif

                #if _SPECTROGRAM_FLAT
                    UNITY_DEFINE_INSTANCED_PROP(half, _SpectrogramData)
                #endif

                #if MESH_PACKING
                    UNITY_DEFINE_INSTANCED_PROP(half, _MeshPackingId)
                #endif

                #if _EMISSIONTEXTURE_FLIPBOOK
                    UNITY_DEFINE_INSTANCED_PROP(float, _StartTime)
                #endif

                #if _VERTEXMODE_DISPLACEMENT
                    UNITY_DEFINE_INSTANCED_PROP(half4, _DisplacementAxisMultiplier)
                #endif

                #if CUTOUT_TYPE_HD_DISSOLVE || NOTE_PLANE_CUT_LW_SNAP || NOTE_PLANE_CUT_HD_DISSOLVE
                    UNITY_DEFINE_INSTANCED_PROP(fixed4, _GlowCutoutColor)
                #endif

                #if NOTE_PLANE_CUT_LW_SNAP || NOTE_PLANE_CUT_HD_DISSOLVE
                    UNITY_DEFINE_INSTANCED_PROP(half4, _CutPlane)
                #endif

                #if _HOLOGRAM_GRID
                    UNITY_DEFINE_INSTANCED_PROP(half, _HologramFill)
                    #if HOLOGRAM_MATERIALIZATION
                        UNITY_DEFINE_INSTANCED_PROP(half, _HoloMaterialize)
                    #endif
                #endif

                #if _HOLOGRAM_GRID || _HOLOGRAM_SCANLINE || _HOLOGRAM_LEGACY
                    UNITY_DEFINE_INSTANCED_PROP(half4, _HologramColor)
                    #if _HOLOGRAM_GRID || _HOLOGRAM_SCANLINE
                        UNITY_DEFINE_INSTANCED_PROP(half, _HologramPhaseOffset)
                        UNITY_DEFINE_INSTANCED_PROP(half, _HologramStripeSpeed)
                        UNITY_DEFINE_INSTANCED_PROP(half, _HaltScan)
                    #endif
                #endif

                #if COLOR_ARRAY
                    UNITY_DEFINE_INSTANCED_PROP(float, _ColorsArrayOffset)
                #endif

                #if EMISSION_MASK
                    UNITY_DEFINE_INSTANCED_PROP(half, _EmissionMaskIntensity)
                #endif

                #if _VERTEXMODE_DISPLACEMENT
                    UNITY_DEFINE_INSTANCED_PROP(half, _DisplacementStrength)
                #endif
                #if DISTORTION_SIMPLE
                    UNITY_DEFINE_INSTANCED_PROP(half, _DistortionStrength)
                #endif
                #if OCCLUSION_DETAIL
                    UNITY_DEFINE_INSTANCED_PROP(half, _OcclusionDetailIntensity)
                #endif

            UNITY_INSTANCING_BUFFER_END(Props)

            // Cheaper than smoothstep, TODO: let's try to use it where applicable
            float linearSmoothstep(float edge0, float edge1, float x) {
              return clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
            }

            float map(float s, float a1, float a2, float b1, float b2) {
                return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
            }

            static const float4 timeVector = float4(0.05, 1.0, 2.0, 3.0);

            struct appdata {

                float4 vertex : POSITION;
                half3 normal : NORMAL;

                #if _VERTEXMODE_COLOR || _VERTEXMODE_EMISSIVE_MULT_ADD || _VERTEXMODE_EMISSION || _VERTEXMODE_METALSMOOTHNESS || _VERTEXMODE_SPECIAL || _VERTEXMODE_DISPLACEMENT || _PARALLAX_MASKING_VERTEX_COLOR
                    fixed4 color : COLOR;
                #endif

                #ifdef USES_UV
                    float2 uv : TEXCOORD0;
                #endif

                #if LIGHTMAP
                    float2 lightmapUv : TEXCOORD1;
                #elif MESH_PACKING || COLOR_ARRAY || (_VERTEXMODE_DISPLACEMENT && VERTEXDISPLACEMENT_MASK)
                    float2 uv2 : TEXCOORD1;
                #endif

                #if _SECONDARY_UVS_IMPORT
                    float2 texcoords2 : TEXCOORD1;
                #endif

                #if _SPECTROGRAM_FULL
                    float2 uv3 : TEXCOORD2;
                #endif

                #if NORMAL_MAP || DISSOLVE_DISPLACEMENT || (!_PARALLAX_FLEXIBLE_REFLECTED && (_PARALLAX_FLEXIBLE || _PARALLAX_RGB)) || _SECONDARY_UVS_EXTERNAL_SCALE
                    float4 tangent : TANGENT;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {

                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;

                #if defined(USES_FRAG_NORMAL) || REFLECTION_TEXTURE || ENABLE_EMISSION_ANGLE_DISAPPEAR
                    float3 worldSpaceNormal : TEXCOORD3;
                    #if SPECULAR_ANTIFLICKER
                        centroid float3 centroidWorldSpaceNormal: TEXCOORD15;
                    #endif
                #endif

                #if NOISE_DITHERING
                    half4 noiseScreenPos : TEXCOORD4;
                #endif

                #if FOG || COLOR_BY_FOG
                    half4 fogScreenPos : TEXCOORD2;
                #endif

                #if _VERTEXMODE_COLOR || _VERTEXMODE_EMISSIVE_MULT_ADD || _VERTEXMODE_EMISSION || _VERTEXMODE_METALSMOOTHNESS || _VERTEXMODE_SPECIAL || _PARALLAX_MASKING_VERTEX_COLOR
                    fixed4 color: TEXCOORD5;
                #elif UV_COLOR_SEGMENTS
                    half4 uvColor : TEXCOORD5;
                    half4 uvRimColor : TEXCOORD8;
                #endif

                #if _SECONDARY_UVS_EXTERNAL_SCALE
                    float2 uvScaleMultiplier : TEXCOORD21;
                #elif _SECONDARY_UVS_OBJECT_SPACE
                    float2 worldspaceUVs : TEXCOORD21;
                #elif _SECONDARY_UVS_IMPORT
                    float2 texcoords2 : TEXCOORD21;
                #endif

                #if HIGHLIGHT_SELECTION
                    half segmentHighlight : TEXCOORD9;
                #endif

                #ifdef USES_UV
                    float2 uv : TEXCOORD0;
                #endif

                #if REFLECTION_TEXTURE && !NORMAL_MAP && !REFLECTION_PROBE
                    half3 worldRefl : TEXCOORD10;
                #endif

                #if LIGHTMAP
                    float2 lightmapUv : TEXCOORD11;
                #endif

                #if NORMAL_MAP
                    float3 tangent : TEXCOORD12;
                    float3 binormal : TEXCOORD13;
                #endif

                #if ENABLE_RIM_DIM
                    half rim : TEXCOORD14;
                #endif

                #if DIRECTIONAL_RIM
                    float3 normalOnPlane : TEXCOORD20;
                #endif

                #if _EMISSIONTEXTURE_FLIPBOOK
                    half4 flipbookChannelMix : TEXCOORD16;
                    float2 flipbookUV : TEXCOORD22;
                #endif

                #if COLOR_ARRAY
                    float2 uv2 : TEXCOORD17;
                #endif

                #if !_PARALLAX_FLEXIBLE_REFLECTED && (_PARALLAX_FLEXIBLE || _PARALLAX_RGB)
                    float3 viewDirTangent : TEXCOORD31;
                #endif

                #if ENABLE_EMISSION_ANGLE_DISAPPEAR
                    half viewAngle : TEXCOORD18;
                #endif

                #if CLOSE_TO_CAMERA_CUTOUT && CUTOUT_TYPE_HD_DISSOLVE
                    half camDistance : TEXCOORD32;
                #endif

                #if NOTE_PLANE_CUT_HD_DISSOLVE || NOTE_PLANE_CUT_LW_SNAP
                    half4 localVertex : TEXCOORD19;
                #endif

                #if AVATAR_COMPUTE_SKINNING && (_DISSOLVE_SPACE_AVATAR|| _HOLOGRAM_GRID)
                    float3 originalPos : TEXCOORD23;
                #endif
                #if ((_HOLOGRAM_GRID || _HOLOGRAM_SCANLINE) && !AVATAR_COMPUTE_SKINNING) || defined(DISSOLVE_LOCAL) // TODO: Will need more granular conditions
                    float3 localPos : TEXCOORD30;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            inline float4 CurveVertex(float4 vertex) {

                float sine, cosine;
                #if _CURVE_VERTICES_AROUND_X
                    sincos(vertex.z / vertex.y, sine, cosine);
                    return float4(vertex.x, cosine * vertex.y, sine * vertex.y, vertex.w);
                #elif _CURVE_VERTICES_AROUND_Y
                    sincos(vertex.x / vertex.z, sine, cosine);
                    return float4(sine * vertex.z, vertex.y, cosine * vertex.z, vertex.w);
                #elif _CURVE_VERTICES_AROUND_Z
                    sincos(vertex.y / vertex.x, sine, cosine);
                    return float4(cosine * vertex.x, sine * vertex.x, vertex.z, vertex.w);
                #else
                    return vertex;
                #endif
            }

            #if AVATAR_COMPUTE_SKINNING
                    v2f vert(OvrDefaultAppdata v) {
                        OVR_INITIALIZE_VERTEX_FIELDS(v);
            #else
                    v2f vert(appdata v) {
            #endif

                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                #if AVATAR_COMPUTE_SKINNING
                    #if _DISSOLVE_SPACE_AVATAR|| _HOLOGRAM_GRID
                        o.originalPos = v.vertex;
                    #endif
                    const OvrVertexData avatarVertexData = OVR_CREATE_VERTEX_DATA(v); // Creates POS, NORMAL, TANGENT, VERT_ID fields
                    #define INPUT_POSITION avatarVertexData.position
                    #define INPUT_NORMAL avatarVertexData.normal
                    #define INPUT_TANGENT avatarVertexData.tangent
                #else
                    #define INPUT_POSITION v.vertex
                    #define INPUT_NORMAL v.normal
                    #define INPUT_TANGENT v.tangent
                #endif

                #if _CUSTOM_TIME_SONG_TIME
                    _Time = _SongTime;
                #elif _CUSTOM_TIME_FREEZE
                    _Time = 0;
                #else
                    _Time += _TimeHelperOffset;
                #endif

                _Time += UNITY_ACCESS_INSTANCED_PROP(Props, _TimeOffset) * timeVector;

                #if CUTOUT_TYPE_LW_SCALE
                    half cutout = UNITY_ACCESS_INSTANCED_PROP(Props, _Cutout);
                #endif

                #if NOTE_PLANE_CUT_LW_SNAP
                    // p' = p - (n ⋅ p + d) * n
                    half4 cutPlane = UNITY_ACCESS_INSTANCED_PROP(Props, _CutPlane);
                    half d = dot(cutPlane.xyz, v.vertex.xyz) + cutPlane.w;
                    #if CUTOUT_TYPE_LW_SCALE
                        d -= cutout * (_NoteSize + cutPlane.w);
                    #endif
                    v.vertex.xyz = v.vertex.xyz - min(0, d) * cutPlane.xyz;
                #endif

                #if NOTE_PLANE_CUT_HD_DISSOLVE || NOTE_PLANE_CUT_LW_SNAP
                    o.localVertex = v.vertex;
                #endif

                #if _VERTEXMODE_DISPLACEMENT
                    float4 displacementVector = UNITY_ACCESS_INSTANCED_PROP(Props, _DisplacementAxisMultiplier);
                    float displacementStrength = UNITY_ACCESS_INSTANCED_PROP(Props, _DisplacementStrength);

                    #if VERTEXDISPLACEMENT_MASK
                        sampler2D _SourceTex;
                        float4 _SourceTex_ST = float4(1.0, 1.0, 0.0, 0.0);
                        #if _VERTEXDISPLACEMENT_MASK_SOURCE_EMISSION_TEXTURE
                            _SourceTex = _EmissionTex;
                            _SourceTex_ST = _EmissionTex_ST;
                        #elif _VERTEXDISPLACEMENT_MASK_SOURCE_EMISSION_MASK
                            _SourceTex = _EmissionMask;
                            _SourceTex_ST = _EmissionMask_ST;
                        #elif _VERTEXDISPLACEMENT_MASK_SOURCE_SECONDARY_EMISSION_MASK
                            _SourceTex = _SecondaryEmissionMask;
                            _SourceTex_ST = _SecondaryEmissionMask_ST;
                        #else
                            _SourceTex = _VertexDisplacementMask;
                            _SourceTex_ST = _VertexDisplacementMask_ST;
                        #endif

                        float2 vertexDisplacementUV= TRANSFORM_TEX(v.uv2.xy, _SourceTex) + _VertexDisplacementMaskSpeed * _VertexDisplacementMask_ST.xy * _Time.x;

                        #if _VERTEXDISPLACEMENT_MASK_ADDITIVERGB
                            float3 displacementMaskOperand = _VertexDisplacementMaskOffset.xxx + _VertexDisplacementMaskMultiplier * 2.0 * (tex2Dlod(_SourceTex, float4(vertexDisplacementUV.xy, 0.0, 0.0)).rgb - float3(0.5, 0.5, 0.5));
                            displacementMaskOperand = _VertexDisplacementMaskMixer.x * displacementMaskOperand.r + _VertexDisplacementMaskMixer.y * displacementMaskOperand.g + _VertexDisplacementMaskMixer.z * displacementMaskOperand.b;
                        #else
                            float3 sampledMask = 2.0 * tex2Dlod(_SourceTex, float4(vertexDisplacementUV.xy, 0.0, 0.0)).rgb - float3(1.0,1.0,1.0);
                            sampledMask = float3(1.0, 1.0, 1.0) * (_VertexDisplacementMaskMixer.x * sampledMask.r + _VertexDisplacementMaskMixer.y * sampledMask.g + _VertexDisplacementMaskMixer.z * sampledMask.b);
                            float3 displacementMaskOperand = _VertexDisplacementMaskOffset.xxx + _VertexDisplacementMaskMultiplier * sampledMask;
                        #endif
                    #endif

                    #if _SPECTROGRAM_FLAT
                        displacementStrength *= UNITY_ACCESS_INSTANCED_PROP(Props, _SpectrogramData);
                    #elif _SPECTROGRAM_FULL
                        displacementStrength *= _SpectrogramData[v.uv3.x * (SPECTROGRAM_SIZE - 1)];
                    #endif

                    #if DISPLACEMENT_SPATIAL
                        float3 baseDisplacement;
                        #if DISPLACEMENT_BIDIRECTIONAL
                            baseDisplacement = (2.0 * v.color.rgb - float3 (1.0, 1.0, 1.0)) * displacementVector.rgb;
                        #else
                            baseDisplacement = displacementStrength.xxx * v.color.rgb * displacementVector.rgb;
                        #endif

                        #if VERTEXDISPLACEMENT_MASK
                            #if _VERTEXDISPLACEMENT_MASK_ADDITIVE
                                INPUT_POSITION.xyz += baseDisplacement * (displacementStrength.xxx + displacementMaskOperand.r);
                            #elif _VERTEXDISPLACEMENT_MASK_ADDITIVERGB
                                INPUT_POSITION.xyz += displacementMaskOperand + displacementStrength.xxx * baseDisplacement;
                            #else
                                INPUT_POSITION.xyz += displacementMaskOperand * displacementStrength.xxx * baseDisplacement;
                            #endif
                        #else
                            INPUT_POSITION.xyz += displacementStrength.xxx * baseDisplacement;
                        #endif
                    #else
                        float3 baseDisplacement = displacementStrength.xxx * v.color.b;
                        #if VERTEXDISPLACEMENT_MASK
                            #if _VERTEXDISPLACEMENT_MASK_ADDITIVE || _VERTEXDISPLACEMENT_MASK_ADDITIVERGB
                                baseDisplacement += displacementMaskOperand;
                            #else
                                baseDisplacement *= displacementMaskOperand;
                            #endif
                        #endif
                        INPUT_POSITION.xyz += baseDisplacement * displacementVector.rgb * v.normal;
                    #endif
                #endif

                #if _CURVE_VERTICES_AROUND_X || _CURVE_VERTICES_AROUND_Y || _CURVE_VERTICES_AROUND_Z
                    v.vertex = CurveVertex(INPUT_POSITION);
                #endif

                #if MESH_PACKING && !LIGHTMAP
                    if (abs(v.uv2.y - UNITY_ACCESS_INSTANCED_PROP(Props, _MeshPackingId)) > 0.1) {
                        INPUT_POSITION = float4(0.0, 0.0, 0.0, 0.0);
                    }
                #endif

                #if ((_HOLOGRAM_GRID || _HOLOGRAM_SCANLINE) && !AVATAR_COMPUTE_SKINNING) || defined(DISSOLVE_LOCAL)
                    o.localPos = INPUT_POSITION;
                #endif

                o.worldPos = mul(unity_ObjectToWorld, INPUT_POSITION);

                #if CLOSE_TO_CAMERA_CUTOUT && (CUTOUT_TYPE_HD_DISSOLVE || CUTOUT_TYPE_LW_SCALE)
                    float camDistance = length(o.worldPos - _WorldSpaceCameraPos);
                    #if CUTOUT_TYPE_HD_DISSOLVE
                        o.camDistance = camDistance;
                    #endif
                #endif

                #if CUTOUT_TYPE_LW_SCALE
                    #if CLOSE_TO_CAMERA_CUTOUT
                        cutout += saturate(1 + _CloseToCameraCutoutOffset - camDistance * _CloseToCameraCutoutScale);
                    #endif
                    #if NOTE_PLANE_CUT_LW_SNAP
                        half t = smoothstep(0.5, 1, cutout);
                        INPUT_POSITION.xyz *= 1.0 - t;
                        INPUT_POSITION.xyz += cutPlane.xyz * t * _NoteSize;
                    #else
                        INPUT_POSITION.xyz *= 1.0 - cutout;
                    #endif
                #endif

                #if NOTE_VERTEX_DISTORTION && MIRROR_VERTEX_DISTORTION && FAKE_MIRROR_TRANSPARENCY
                    float3 vertexOffset = GetFakeMirrorDistortionVertexOffset(o.worldPos);
                    INPUT_POSITION.xyz += vertexOffset;
                #endif

                o.vertex = UnityObjectToClipPos(INPUT_POSITION);

                #if FOG || NOISE_DITHERING || COLOR_BY_FOG
                    half4 nonStereoScreenPos = ComputeOffsetNonStereoScreenPos(o.vertex);
                #endif

                #if FOG || COLOR_BY_FOG
                    o.fogScreenPos = ComputeFogScreenPos(nonStereoScreenPos);
                #endif

                #if NOISE_DITHERING
                    o.noiseScreenPos = ComputeNoiseScreenPos(nonStereoScreenPos);
                #endif

                #if defined(USES_FRAG_NORMAL) || REFLECTION_TEXTURE || ENABLE_EMISSION_ANGLE_DISAPPEAR
                    float3 worldSpaceNormal;
                    #if USE_SPHERICAL_NORMAL_OFFSET
                        worldSpaceNormal = UnityObjectToWorldNormal((INPUT_POSITION.xyz + _SphericalNormalOffsetCenter) * _SphericalNormalOffsetIntensity + INPUT_NORMAL.xyz * (1.0 - _SphericalNormalOffsetIntensity));
                    #else
                        worldSpaceNormal = UnityObjectToWorldNormal(INPUT_NORMAL);
                    #endif
                #endif

                #if defined(USES_FRAG_NORMAL) || ENABLE_RIM_DIM || ENABLE_EMISSION_ANGLE_DISAPPEAR
                    o.worldSpaceNormal = worldSpaceNormal;
                    #if SPECULAR_ANTIFLICKER
                        o.centroidWorldSpaceNormal = o.worldSpaceNormal;
                    #endif
                #endif

                #if _VERTEXMODE_COLOR || _VERTEXMODE_EMISSIVE_MULT_ADD || _VERTEXMODE_EMISSION || _VERTEXMODE_METALSMOOTHNESS || _VERTEXMODE_SPECIAL || _PARALLAX_MASKING_VERTEX_COLOR
                    o.color = v.color;
                #endif

                #if UV_COLOR_SEGMENTS
                    o.uvColor = _UVColors[v.uv.x * 10];

                    #if _RIMLIGHT_LERP || _RIMLIGHT_ADDITIVE
                        o.uvRimColor = _UVRimColors[v.uv.x * 10];
                    #endif
                #endif

                #if HIGHLIGHT_SELECTION
                    #if UV_COLOR_SEGMENTS
                        o.segmentHighlight = _SegmentToHighlight == floor(v.uv.x * 10) ? 1 : 0;
                    #else
                        o.segmentHighlight = _SegmentToHighlight == 0 ? 1 : 0;
                    #endif
                #endif

                #if _SECONDARY_UVS_IMPORT
                    o.texcoords2 = v.texcoords2;
                #endif

                #ifdef USES_UV
                    #if ROTATE_UV // 0 CW / 1 CCW / 2 180
                        if (_Rotate_UV > 1.5) {
                            o.uv = 1 - v.uv;
                        } else if (_Rotate_UV > 0.5) {
                            o.uv = float2(v.uv.y, 1 - v.uv.x);
                        } else {
                            o.uv = float2(1 - v.uv.y, v.uv.x);
                        }

                    #elif _EMISSIONTEXTURE_FLIPBOOK
                        int frames = _FlipbookColumns * _FlipbookRows;
                        int loopableFrames = frames - _FlipbookNonloopableFrames;

                        float flipbookTime = _FlipbookSpeed * (_Time.y - UNITY_ACCESS_INSTANCED_PROP(Props, _StartTime));
                        float currentFrame = flipbookTime < frames ? flipbookTime : ((flipbookTime - _FlipbookNonloopableFrames) % loopableFrames) + _FlipbookNonloopableFrames;

                        float2 flipSize = 1.0 / float2(_FlipbookColumns, _FlipbookRows);
                        float subFrame = frac(currentFrame);
                        float x = fmod(floor(currentFrame), _FlipbookColumns);
                        float y = _FlipbookRows - 1.0 - floor(currentFrame / _FlipbookColumns);

                        #if !FLIPBOOK_BLENDING_OFF
                            // Version for R-G, G-B, B-A encoding (allows 300% of standard frame count while making frame blending free instead of costing additional texture sample)
                            float r = saturate(1 - subFrame * 3.0);
                            float g = saturate(1.0 - abs(subFrame * 3.0 - 1.0));
                            float b = saturate(1.0 - abs(subFrame * 3.0 - 2.0));
                            float a = saturate(subFrame * 3.0 - 2.0);
                            o.flipbookChannelMix = half4(r,g,b,a);
                        #else
                            if (subFrame < 0.25) {
                                o.flipbookChannelMix = half4(1.0, 0.0, 0.0, 0.0);
                            } else if (subFrame < 0.5) {
                                o.flipbookChannelMix = half4(0.0, 1.0, 0.0, 0.0);
                            } else if (subFrame < 0.75) {
                                o.flipbookChannelMix = half4(0.0, 0.0, 1.0, 0.0);
                            } else {
                                o.flipbookChannelMix = half4(0.0, 0.0, 0.0, 1.0);
                            }
                        #endif

                        if (v.uv.x + v.uv.y <= 0.01) { o.flipbookChannelMix *= 0.0; } //Temporary fix for Rock Screens, TODO: Make sure bottom left pixel of all frames is black

                        o.flipbookUV = float2(v.uv.xy + float2(x,y)) * flipSize;
                        o.uv = v.uv;

                    #else // No rotation or flipbook
                        o.uv = v.uv;
                    #endif
                #endif

                #if COLOR_ARRAY
                    o.uv2 = v.uv2 + half2(0.0, UNITY_ACCESS_INSTANCED_PROP(Props, _ColorsArrayOffset));
                #endif

                //#if DISSOLVE && DISSOLVE_DISPLACEMENT
                    // FIX ALL THIS
                    // half dissolveInvert = _DissolveProgress > 0.0 ? _InvertDissolve > 0.0 ? 1.0 : -1.0 : _InvertDissolve > 0.0 ? -1.0 : 1.0;
                    // #if _DISSOLVEAXIS_LOCALX
                    //     half pos = dissolveInvert * INPUT_POSITION.x;
                    // #elif _DISSOLVEAXIS_LOCALY
                    //     half pos = dissolveInvert * INPUT_POSITION.y;
                    // #elif _DISSOLVEAXIS_LOCALZ
                    //     half pos = dissolveInvert * INPUT_POSITION.z;
                    // #elif _DISSOLVEAXIS_WORLDX
                    //     half pos = dissolveInvert * o.worldPos.x;
                    // #elif _DISSOLVEAXIS_WORLDY
                    //     half pos = dissolveInvert * o.worldPos.y;
                    // #elif _DISSOLVEAXIS_WORLDZ
                    //     half pos = dissolveInvert * o.worldPos.z;
                    // #elif _DISSOLVEAXIS_UVX
                    //     half pos = dissolveInvert * o.uv.x;
                    // #else // _DISSOLVEAXIS_UVY
                    //     half pos = dissolveInvert * o.uv.y;
                    // #endif
                    //
                    // half displacementMask = saturate(pos + _DissolveGradientWidth - lerp(_DissolveStartValue, _DissolveEndValue, abs(_DissolveProgress)));
                    //
                    // float2 displacementUV = TRANSFORM_TEX(v.uv.xy, _DissolveDisplacementTex);
                    // displacementUV += _Time.y * _DissolveDisplacementPanning * _DissolveDisplacementTex_ST.xy * _DissolveDisplacementOverallSpeed;
                    // half3 binormal = cross(INPUT_TANGENT, INPUT_NORMAL);
                    // half3 displacementTex = tex2Dlod(_DissolveDisplacementTex, half4(displacementUV.xy, 0.0, 0.0));
                    // displacementTex = displacementTex * 2 - 1;
                    // half3 displacement = normalize(displacementTex.x * INPUT_TANGENT.xyz + displacementTex.y * binormal.xyz + displacementTex.z * INPUT_NORMAL.xyz);
                    // displacement *= _DissolveDisplacementStrength * _DissolveDisplacementAxes;
                    // o.vertex.xyz += displacement * displacementMask;
                //#endif

                #if ENABLE_RIM_DIM || ENABLE_EMISSION_ANGLE_DISAPPEAR || (REFLECTION_TEXTURE && !NORMAL_MAP)
                    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                #endif

                #if REFLECTION_TEXTURE && !NORMAL_MAP && !REFLECTION_PROBE
                    o.worldRefl = reflect(-worldViewDir, worldSpaceNormal);
                #endif

                #if LIGHTMAP
                    o.lightmapUv = v.lightmapUv.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                #endif

                #if NORMAL_MAP
                    o.tangent = float4(UnityObjectToWorldDir(INPUT_TANGENT.xyz), INPUT_TANGENT.w);
                    o.binormal = cross(o.worldSpaceNormal, o.tangent.xyz) * (INPUT_TANGENT.w * unity_WorldTransformParams.w);
                #endif

                #if ENABLE_RIM_DIM
                    o.rim = dot(worldViewDir.xyz, o.worldSpaceNormal.xyz);

                    #if INVERT_RIM_DIM
                        o.rim = 1 - o.rim;
                    #endif

                    o.rim = (1.0 + _RimOffset - o.rim);
                    o.rim = saturate(o.rim);
                #endif

                #if _RIMLIGHT_ADDITIVE || _RIMLIGHT_LERP
                    #if DIRECTIONAL_RIM
                        float3 objectPos = float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]);
                        float3 pn = UnityObjectToWorldNormal(_RimPerpendicularAxis);
                        float d = -dot(pn, objectPos);
                        float dist = dot(pn, _WorldSpaceCameraPos) + d;
                        o.normalOnPlane = normalize(_WorldSpaceCameraPos - dist * pn - objectPos);
                    #endif
                #endif

                #if !_PARALLAX_FLEXIBLE_REFLECTED && (_PARALLAX_FLEXIBLE || _PARALLAX_RGB)

                    #if _PARALLAX_PROJECTION_WARPED
                        float3x3 objectToTangent = float3x3(
                            v.tangent.xyz,
                            cross(v.normal, v.tangent.xyz) * v.tangent.w,
                            v.normal);
                        o.viewDirTangent = mul(objectToTangent, ObjSpaceViewDir(v.vertex));
                    #else // default Planar projection
                        float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
                        float3 viewDir = v.vertex.xyz - objCam.xyz;
                        float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                        float3 bitangent = cross(v.normal.xyz, v.tangent.xyz) * tangentSign;
                        o.viewDirTangent = float3(dot(viewDir, v.tangent.xyz), dot(viewDir, bitangent.xyz), dot(viewDir, v.normal.xyz));
                    #endif
                #endif

                #if ENABLE_EMISSION_ANGLE_DISAPPEAR
                    o.viewAngle = smoothstep(0.05, _ThresholdAngle, abs(dot(normalize(worldViewDir.xyz), normalize(o.worldSpaceNormal.xyz))));
                #endif

                #if _SECONDARY_UVS_EXTERNAL_SCALE
                    float3 uvScale = UNITY_ACCESS_INSTANCED_PROP(Props, _UVScale);
                    float3 tangentV = cross(v.tangent.xyz, v.normal.xyz);
                    o.uvScaleMultiplier = abs(float2(dot(uvScale, v.tangent.xyz), dot(uvScale, tangentV)));
                #endif

                #if _SECONDARY_UVS_OBJECT_SPACE
                    float2 uvScale = UNITY_ACCESS_INSTANCED_PROP(Props, _UVScale);
                    float3 objectSpacePivot = float3(unity_WorldToObject[0][3], unity_WorldToObject[1][3], unity_WorldToObject[2][3]);
                    o.worldspaceUVs = (v.vertex.xy - objectSpacePivot) * uvScale.xy;
                #endif

                return o;
            }

            fixed4 frag(v2f i, bool facing : SV_IsFrontFace) : SV_Target {

                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                UNITY_SETUP_INSTANCE_ID(i);

                #if OVERDRAW_VIEW
                    return OverdrawViewOpaque();
                #endif

                i.uv *= _InputUvMultiplier;

                #if _CUSTOM_TIME_SONG_TIME
                    _Time = _SongTime;
                #elif _CUSTOM_TIME_FREEZE
                    _Time = 0;
                #else
                    _Time += _TimeHelperOffset;
                #endif

                _Time += UNITY_ACCESS_INSTANCED_PROP(Props, _TimeOffset) * timeVector;

                #ifdef SECONDARY_UV_DEFINED
                    float2 secondaryUV;
                    #if _SECONDARY_UVS_EXTERNAL_SCALE
                        secondaryUV = i.uv;
                        #if SECONDARY_UVS_EMISSION && defined(_EMISSION_TEXTURE_SOURCE_TEXTURE) && _EMISSIONTEXTURE_SIMPLE
                            _EmissionTex_ST.xy *= i.uvScaleMultiplier;
                        #endif
                        #if SECONDARY_UVS_PULSE && _EMISSIONTEXTURE_PULSE
                            _PulseMask_ST.xy *= i.uvScaleMultiplier;
                        #endif
                        #if SECONDARY_UVS_EMISSION_MASK && EMISSION_MASK
                            _EmissionMask_ST.xy *= i.uvScaleMultiplier;
                        #endif
                        #if SECONDARY_UVS_EMISSION_MASK2 && SECONDARY_EMISSION_MASK
                            _SecondaryEmissionMask_ST.xy *= i.uvScaleMultiplier;
                        #endif
                        #if SECONDARY_UVS_DIFFUSE && DIFFUSE_TEXTURE && defined(_DIFFUSE_TEXTURE_SOURCE_TEXTURE)
                            _DiffuseTexture_ST.xy *= i.uvScaleMultiplier;
                        #endif
                        #if SECONDARY_UVS_PARALLAX && (_PARALLAX_FLEXIBLE || _PARALLAX_RGB)
                            _ParallaxMap_ST.xy *= i.uvScaleMultiplier;
                        #endif
                        #if SECONDARY_UVS_NORMAL && NORMAL_MAP
                            _NormalTexture_ST.xy *= i.uvScaleMultiplier;
                        #endif
                        #if SECONDARY_UVS_OCCLUSION && OCCLUSION && defined(_OCCLUSION_SOURCE_TEXTURE)
                            _DirtTex_ST.xy *= i.uvScaleMultiplier;
                        #endif
                        #if SECONDARY_UVS_OCCLUSION_DETAIL && OCCLUSION_DETAIL
                            _DirtDetailTex_ST.xy *= i.uvScaleMultiplier;
                        #endif
                        #if SECONDARY_UVS_MPM && METAL_SMOOTHNESS_TEXTURE && !_EMISSIONTEXTURE_FLIPBOOK
                            _MetalSmoothnessTex_ST.xy *= i.uvScaleMultiplier;
                        #endif
                        #if SECONDARY_UVS_DISTORTION && DISTORTION_SIMPLE
                            _DistortionTex_ST.xy *= i.uvScaleMultiplier;
                        #endif
                    #endif
                    #if _SECONDARY_UVS_IMPORT
                        secondaryUV = i.texcoords2.xy;
                    #elif _SECONDARY_UVS_OBJECT_SPACE
                        secondaryUV = i.worldspaceUVs;
                    #elif _SECONDARY_UVS_ADDITIVE_OFFSET
                        secondaryUV = i.uv + UNITY_ACCESS_INSTANCED_PROP(Props, _AdditiveUVOffset);
                    #endif
                #endif

                #if METAL_SMOOTHNESS_TEXTURE
                    #if SECONDARY_UVS_MPM && defined(SECONDARY_UV_DEFINED)
                        float2 mpmUV = secondaryUV;
                    #else
                        float2 mpmUV = i.uv;
                    #endif
                #endif

                #if DIFFUSE_TEXTURE && defined(_DIFFUSE_TEXTURE_SOURCE_TEXTURE)
                    #if SECONDARY_UVS_DIFFUSE && defined(SECONDARY_UV_DEFINED)
                        float2 diffuseUV = secondaryUV;
                    #else
                        float2 diffuseUV = i.uv;
                    #endif
                #endif

                #if NORMAL_MAP
                    #if SECONDARY_UVS_NORMAL && defined(SECONDARY_UV_DEFINED)
                        float2 normalUV = secondaryUV;
                    #else
                        float2 normalUV = i.uv;
                    #endif
                #endif

                #if OCCLUSION
                    #if SECONDARY_UVS_OCCLUSION && defined(SECONDARY_UV_DEFINED)
                        float2 occlusionUV = secondaryUV;
                    #else
                        float2 occlusionUV = i.uv;
                    #endif
                #endif

                #if OCCLUSION_DETAIL
                    #if SECONDARY_UVS_OCCLUSION_DETAIL && defined(SECONDARY_UV_DEFINED)
                        float2 occlusionDetailUV = secondaryUV;
                    #else
                        float2 occlusionDetailUV = i.uv;
                    #endif
                #endif

                #if defined(_EMISSION_TEXTURE_SOURCE_TEXTURE) && _EMISSIONTEXTURE_SIMPLE
                    #if SECONDARY_UVS_EMISSION && defined(SECONDARY_UV_DEFINED)
                        float2 emissionTexUV = secondaryUV;
                    #else
                        float2 emissionTexUV = i.uv;
                    #endif
                #endif

                #if _EMISSIONTEXTURE_PULSE
                    #if SECONDARY_UVS_PULSE && defined(SECONDARY_UV_DEFINED)
                        float2 pulseMaskUV = secondaryUV;
                    #else
                        float2 pulseMaskUV = i.uv;
                    #endif
                #endif

                #if EMISSION_MASK
                    #if SECONDARY_UVS_EMISSION_MASK && defined(SECONDARY_UV_DEFINED)
                        float2 emissionMaskUV = secondaryUV;
                    #else
                        float2 emissionMaskUV = i.uv;
                    #endif
                #endif

                #if SECONDARY_EMISSION_MASK
                    #if SECONDARY_UVS_EMISSION_MASK2 && defined(SECONDARY_UV_DEFINED)
                        float2 secondaryEmissionMaskUV = secondaryUV;
                    #else
                        float2 secondaryEmissionMaskUV = i.uv;
                    #endif
                #endif

                #if _PARALLAX_FLEXIBLE || _PARALLAX_RGB
                    #if SECONDARY_UVS_PARALLAX && defined(SECONDARY_UV_DEFINED)
                        float2 parralaxUV = secondaryUV;
                    #else
                        float2 parralaxUV = i.uv;
                    #endif
                #endif


                #if DISTORTION_SIMPLE
                    float distortionStrength = UNITY_ACCESS_INSTANCED_PROP(Props, _DistortionStrength);
                    #if SECONDARY_UVS_DISTORTION && defined(SECONDARY_UV_DEFINED)
                        float2 distortionUVs = secondaryUV;
                    #else
                        float2 distortionUVs = i.uv;
                    #endif
                    float2 distortionUV = TRANSFORM_TEX(distortionUVs.xy, _DistortionTex) + _Time.y * _DistortionPanning * _DistortionTex_ST.xy * 0.1;
                    float2 distortion = distortionStrength * 0.1 * tex2D(_DistortionTex, distortionUV).xy * _DistortionAxes * 2.0 - 1.0;
                    #if defined(_DISTORTION_TARGET_MPM) && METAL_SMOOTHNESS_TEXTURE
                        mpmUV += distortion;
                    #endif
                    #if _DISTORTION_TARGET_DIFFUSE && DIFFUSE_TEXTURE && defined(_DIFFUSE_TEXTURE_SOURCE_TEXTURE)
                        diffuseUV += distortion;
                    #endif
                    #if _DISTORTION_TARGET_NORMAL && NORMAL_MAP
                        normalUV += distortion;
                    #endif
                    #if _DISTORTION_TARGET_OCCLUSION && OCCLUSION && defined(_OCCLUSION_SOURCE_TEXTURE)
                        occlusionUV += distortion;
                    #endif
                    #if _DISTORTION_TARGET_OCCLUSION_DETAIL && OCCLUSION_DETAIL
                        occlusionDetailUV += distortion;
                    #endif
                    #if _DISTORTION_TARGET_EMISSIONTEX && defined(_EMISSION_TEXTURE_SOURCE_TEXTURE) && _EMISSIONTEXTURE_SIMPLE
                        emissionTexUV += distortion;
                    #endif
                    #if _DISTORTION_TARGET_PULSE && _EMISSIONTEXTURE_PULSE
                        pulseMaskUV += distortion;
                    #endif
                    #if _DISTORTION_TARGET_EMISSION_MASK && EMISSION_MASK
                        emissionMaskUV += distortion;
                    #endif
                    #if _DISTORTION_TARGET_SECONDARY_EMISSION_MASK && SECONDARY_EMISSION_MASK
                        secondaryEmissionMaskUV += distortion;
                    #endif
                    #if _DISTORTION_TARGET_PARRALAX && (_PARALLAX_FLEXIBLE || _PARALLAX_RGB)
                        parralaxUV += distortion;
                    #endif
                #endif

                #if METAL_SMOOTHNESS_TEXTURE
                    #if MPM_CUSTOM_MIP
                        fixed4 mpmTexture = tex2Dbias(_MetalSmoothnessTex, float4(TRANSFORM_TEX(mpmUV, _MetalSmoothnessTex).xy, 0.0, _MpmMipBias));
                    #else
                        fixed4 mpmTexture = tex2D(_MetalSmoothnessTex, TRANSFORM_TEX(mpmUV, _MetalSmoothnessTex));
                    #endif
                #endif

                #if SPECULAR_ANTIFLICKER
                    if (dot( i.worldSpaceNormal.xyz, i.worldSpaceNormal.xyz) >= 1.01 ) {
                        i.worldSpaceNormal.xyz = i.centroidWorldSpaceNormal.xyz;
                    }
                #endif

                #if SPECULAR || _RIMLIGHT_LERP || _RIMLIGHT_ADDITIVE || FOG ||  REFLECTION_PROBE || DIFFUSE || ENABLE_RIM_DIM || SPECULAR_ANTIFLICKER || ENABLE_EMISSION_ANGLE_DISAPPEAR || (REFLECTION_TEXTURE && NORMAL_MAP)  || (_HOLOGRAM_GRID || _HOLOGRAM_SCANLINE) || (_PARALLAX_FLEXIBLE_REFLECTED && (_PARALLAX_FLEXIBLE || _PARALLAX_RGB))
                    #define VIEW_DIR_DEFINED
                    float3 viewDir = i.worldPos - _WorldSpaceCameraPos;
                    float dist = length(viewDir);
                    viewDir /= dist;
                #endif

                float rimDim = 1.0;
                #if ENABLE_RIM_DIM
                    i.rim *= _RimScale + max(dist - _RimCameraDistanceOffset, 0.0) * _RimCameraDistanceScale;
                    rimDim = 1.0 - i.rim * _RimDarkening;
                #endif

                #if UV_COLOR_SEGMENTS
                    fixed4 color = i.uvColor;
                #else
                    fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                #endif
                color.a = 0.0; // Set to zero because it's unintuitive to control PC Bloom with Base Color alpha (and can be completely missed by the artist)

                #if _VERTEXMODE_COLOR
                    color.rgb *= i.color.rgb;
                #endif

                #if DIFFUSE_TEXTURE
                    #if defined(_DIFFUSE_TEXTURE_SOURCE_TEXTURE)
                        color *= tex2D(_DiffuseTexture, TRANSFORM_TEX(diffuseUV, _DiffuseTexture));
                    #elif _DIFFUSE_TEXTURE_SOURCE_MPM_R
                        color *= mpmTexture.r;
                    #elif _DIFFUSE_TEXTURE_SOURCE_MPM_A_SMOOTHNESS
                        color *= (1.0 - mpmTexture.a * _AlbedoMultiplier);
                    #endif
                #endif

                #if METAL_SMOOTHNESS_TEXTURE
                    #if _METALLIC_TEXTURE_MPM_R
                        _Metallic *= mpmTexture.r;
                    #elif _METALLIC_TEXTURE_MPM_A
                        _Metallic *= mpmTexture.a;
                    #elif _METALLIC_TEXTURE_MPM_AVATAR_B
                        _Metallic *= mpmTexture.b;
                    #endif

                    #if _SMOOTHNESS_TEXTURE_MPM_A
                    _Smoothness *= mpmTexture.a;
                    #elif _SMOOTHNESS_TEXTURE_MPM_G_ROUGHNESS
                        _Smoothness *= 1.0 - mpmTexture.g;
                    #endif
                #endif

                #if _VERTEXMODE_SPECIAL || _VERTEXMODE_METALSMOOTHNESS
                    _Smoothness *= i.color.a;
                    _Metallic *= i.color.r;
                #endif

                #if defined(USES_FRAG_NORMAL)
                    #if PRECISE_NORMAL
                        half3 normal = normalize(i.worldSpaceNormal);
                    #else
                        half3 normal = i.worldSpaceNormal;
                    #endif
                #endif

                // To make sure we can control normal scale in all cases, a scale of 1.0 is used for UnpackScaleNormal and scale is applied consequently
                #if NORMAL_MAP
                    float3 normalMap = UnpackScaleNormal(tex2D(_NormalTexture, TRANSFORM_TEX(normalUV, _NormalTexture)), 1.0);
                    normalMap.xy *= _NormalScale;
                    normal = normalize(normalMap.x * i.tangent + normalMap.y * i.binormal + normalMap.z * normal);
                #endif

                #if SPECULAR_ANTIFLICKER
                    float3 vNormalWsDdx = ddx(i.worldSpaceNormal.xyz);
                    float3 vNormalWsDdy = ddy(i.worldSpaceNormal.xyz);
                    //float flGeometricRoughnessFactor = pow( saturate( max( dot( vNormalWsDdx.xyz, vNormalWsDdx.xyz ), dot( vNormalWsDdy.xyz, vNormalWsDdy.xyz ) ) ), 0.333 );
                    float flGeometricRoughnessFactor = saturate(max(dot(vNormalWsDdx.xyz, vNormalWsDdx.xyz), dot(vNormalWsDdy.xyz, vNormalWsDdy.xyz)));
                    //flGeometricRoughnessFactor = sqrt(flGeometricRoughnessFactor * 0.1);
                    flGeometricRoughnessFactor = pow(flGeometricRoughnessFactor, 0.333);
                    float targetSmoothness = min(_Smoothness, 1.0 - flGeometricRoughnessFactor);

                    float distanceStrength = saturate(_AntiflickerDistanceScale * (_AntiflickerDistanceOffset - dist));
                    _Smoothness = lerp(_Smoothness, targetSmoothness, distanceStrength * _AntiflickerStrength);
                #endif

                float groundFade = 1.0;

                #if GROUND_FADE
                    groundFade = 1 - saturate(_GroundFadeOffset - i.worldPos.y * _GroundFadeScale);
                #endif

                #if OCCLUSION
                    half occlusion = 1.0;
                    #if defined(_OCCLUSION_SOURCE_TEXTURE)
                        occlusion = _OcclusionIntensity * tex2D(_DirtTex, TRANSFORM_TEX(occlusionUV, _DirtTex)).r + 1.0 - _OcclusionIntensity;
                    #elif _OCCLUSION_SOURCE_MPM_B
                        occlusion = _OcclusionIntensity * mpmTexture.b + 1.0 - _OcclusionIntensity;
                    #elif _OCCLUSION_SOURCE_AVATAR_MPM_R
                        occlusion = _OcclusionIntensity * mpmTexture.r + 1.0 - _OcclusionIntensity;
                    #endif

                    #if SPECULAR
                        _SpecularIntensity *= occlusion;
                    #endif
                #endif

                #if OCCLUSION_DETAIL
                    half occlusionDetail = UNITY_ACCESS_INSTANCED_PROP(Props, _OcclusionDetailIntensity) * tex2D(_DirtDetailTex, TRANSFORM_TEX(occlusionDetailUV, _DirtDetailTex)).r + (1.0 - UNITY_ACCESS_INSTANCED_PROP(Props, _OcclusionDetailIntensity));
                    #if SPECULAR
                        _SpecularIntensity *= occlusionDetail;
                    #endif
                #endif

                #if LIGHTMAP
                    half3 lightmap = GetLightmap(i.lightmapUv);
                #endif

                color.rgb *= groundFade;
                fixed3 albedo = color.rgb;

                half3 ambientColor = max(UNITY_ACCESS_INSTANCED_PROP(Props, _NominalDiffuseLevel) * _AmbientMultiplier, _AmbientMinimalValue.xxx);
                color.rgb *= ambientColor;

                #if DIFFUSE || SPECULAR || LIGHTMAP || PRIVATE_POINT_LIGHT

                    #if DIFFUSE || SPECULAR || PRIVATE_POINT_LIGHT
                        half3 diffuseNormal = normal;
                    #else
                        half3 diffuseNormal = half3(0,1,0);
                    #endif

                    #ifndef VIEW_DIR_DEFINED
                        half3 viewDir = half3(0,1,0);
                    #endif

                    #if INVERTDIFFUSENORMAL
                        diffuseNormal *= -1.0;
                    #endif


                    // Unlike Ambient Color, starting diffuse gets affected by Metalness
                    half3 startingDiffuse = half3(0.0, 0.0, 0.0);
                    half3 startingSpecular = half3(0.0, 0.0, 0.0);

                    #if PRIVATE_POINT_LIGHT
                        #if INSTANCED_PRIVATE_POINT_LIGHT
                            startingDiffuse += PhongPointLightDiffuse(i.worldPos, _PrivatePointLightPosition, diffuseNormal, UNITY_ACCESS_INSTANCED_PROP(Props, _PrivatePointLightColor) * _PrivatePointLightIntensity);
                        #else
                            startingDiffuse += PhongPointLightDiffuse(i.worldPos, _PrivatePointLightPosition, diffuseNormal, _PrivatePointLightColor * _PrivatePointLightIntensity);
                        #endif
                    #endif

                    #if LIGHTMAP
                        startingDiffuse += lightmap;
                    #endif

                    #if SPECULAR
                        _SpecularIntensity *= groundFade;
                    #endif

                    color.rgb += PhongLighting(i.worldPos, diffuseNormal, viewDir, albedo, _Smoothness, _Metallic, _SpecularIntensity, startingDiffuse, startingSpecular, _BothSidesDiffuseMultiplier);
                #endif


                #if REFLECTION_TEXTURE || REFLECTION_PROBE
                    #if ENABLE_RIM_DIM
                        _Smoothness = saturate(_Smoothness - i.rim * _RimSmoothness);
                    #endif

                    #if REFLECTION_STATIC && !REFLECTION_TEXTURE
                        float3 reflectedDir = i.worldPos + normal;
                    #elif REFLECTION_PROBE || NORMAL_MAP
                        float3 reflectedDir = reflect(viewDir, normal); // needs frag precision
                    #else
                        half3 reflectedDir = i.worldRefl;
                    #endif

                    half3 reflection = half3(0.0,0.0,0.0);
                    half mip = GetMipmapLevel(_Smoothness);

                    #if REFLECTION_TEXTURE
                        reflection += GlossyEnvironmentMip(_EnvironmentReflectionCube, mip, reflectedDir) * _ReflectionTexIntensity;
                    #endif

                    #if REFLECTION_PROBE
                        #if REFLECTION_STATIC && REFLECTION_TEXTURE
                            reflectedDir = i.worldPos + normal;
                        #endif

                        half3 probe = GetReflectionProbe(i.worldPos, reflectedDir, mip) * _ReflectionProbeIntensity;
                        #if _PROBE_CALCULATION_PRECISE
                            half maxBaseChannel = max(albedo.r, max(albedo.g, albedo.b));
                            half baseColorSaturation = (maxBaseChannel - min(albedo.r, min(albedo.g, albedo.b))) / maxBaseChannel;

                            color *= (1.0 - _Metallic) * (1.0 + baseColorSaturation);

                            half grayscaleProbe = dot(float3(0.33, 0.33, 0.33), probe) * (_Metallic * _Metallic * 2.5 + 1.0);
                            half reflectionColorization = saturate(_Metallic - 0.1 * grayscaleProbe * grayscaleProbe);
                            probe = lerp(probe, grayscaleProbe, max(_ReflectionProbeGrayscale, _Metallic * baseColorSaturation));
                        #endif
                        #if MULTIPLY_REFLECTIONS && REFLECTION_TEXTURE
                            reflection = (reflection * 2.0 + 1.0) * probe;
                        #else
                            reflection += probe;
                        #endif
                    #endif

                    #if REFLECTION_PROBE && _PROBE_CALCULATION_PRECISE
                        fixed3 colorTarget = albedo * (1.0 + _Metallic * baseColorSaturation * _ColoredMetalMultiplier); // This could be done in texture
                        reflection *= lerp(half3(1.0, 1.0, 1.0), colorTarget, reflectionColorization * max(baseColorSaturation,0.95)); // Colorizes the reflection by base color

                        //bringing white back up as it wasn't brightened up with previous code
                        half grayscaleFactor = 1.0 - baseColorSaturation;
                        reflection *= max(1.0, grayscaleFactor * _WhiteOffset * max(1.0, _ColoredMetalMultiplier) * albedo.rgb * _Metallic);
                    #else
                        reflection *= lerp(half3(1.0 ,1.0 ,1.0), albedo, _Metallic);
                        reflection *= (_Metallic * 0.8 + 0.2) * 2.0;
                    #endif

                    color.rgb += reflection * rimDim * _Smoothness * groundFade;
                #endif

                #if DISTANCE_DARKENING
                    float3 distanceSqr = _DarkeningCenter.xyz - i.worldPos.xyz;
                    distanceSqr = abs(distanceSqr * distanceSqr);
                    distanceSqr *= _DarkeningDirection * 0.0001;
                    color.rgb *= 1.0 - saturate((distanceSqr.x + distanceSqr.y + distanceSqr.z) * _DarkeningScale) * _DarkeningIntensity;
                #endif

                #if OCCLUSION && OCCLUSION_BEFORE_EMISSION
                    color *= occlusion;
                #endif

                #if OCCLUSION_DETAIL && OCCLUSION_BEFORE_EMISSION
                    color *= occlusionDetail;
                #endif

                #if ACES_TONE_MAPPING && _ACES_APPROACH_BEFORE_EMISSIVE
                    color.rgb = ACESFilmTonemapping(color.rgb);
                #endif

                #if _HOLOGRAM_GRID || _HOLOGRAM_SCANLINE
                    #if _HOLOGRAM_GRID
                        float normDist = 1.0 - saturate(dist / 7.5);
                        normDist = 1.0 - normDist * normDist;
                    #endif
                    half holoTime = UNITY_ACCESS_INSTANCED_PROP(Props, _HaltScan) > 0.5 ? 0.0 : _Time.w;

                    #if _HOLOGRAM_GRID
                        #if AVATAR_COMPUTE_SKINNING
                            half3 holoPos = i.originalPos;
                        #else
                            half3 holoPos = i.localPos;
                        #endif

                        half stripeSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _HologramStripeSpeed);
                        half phaseOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _HologramPhaseOffset);

                        float gridHolo = HologramDigiGridOnly(i.worldPos, holoPos, _HologramGridSize - 10.0 * normDist,
                            stripeSpeed, _HologramScanDistance, UNITY_ACCESS_INSTANCED_PROP(Props, _HologramFill),
                            phaseOffset, holoTime);

                        // Todo: pack gridHolo to not calculate multiple times
                        #if HOLOGRAM_MATERIALIZATION
                            half holoMaterialize = UNITY_ACCESS_INSTANCED_PROP(Props, _HoloMaterialize);
                            float holoScanline = HologramDigiScan(i.worldPos, stripeSpeed, _HologramScanDistance, phaseOffset, holoTime - 0.2);
                            float scanlineReveal = smoothstep(0.0, 0.01, holoScanline);
                            float scanlineTransparentBand = smoothstep(1.01, 0.9, holoScanline);
                            scanlineReveal *= scanlineTransparentBand;
                        #endif

                    #endif
                #endif

                #if _RIMLIGHT_LERP || _RIMLIGHT_ADDITIVE
                    fixed4 rimColor;

                    #if UV_COLOR_SEGMENTS && !UV_SEGMENTS_IGNORE_RIM
                        rimColor = i.uvRimColor;
                    #else
                        rimColor = UNITY_ACCESS_INSTANCED_PROP(Props, _RimLightColor);
                    #endif

                    #if DIRECTIONAL_RIM
                        half fresnel = abs(dot(i.worldSpaceNormal, i.normalOnPlane));
                    #else
                        half fresnel = abs(dot(normal, viewDir));
                    #endif

                    #if !RIMLIGHT_INVERT
                        fresnel = 1.0 - fresnel;
                    #endif

                    half rimLight = smoothstep(_RimLightEdgeStart, 1.0, fresnel);

                    rimColor.a *= _RimLightIntensity;
                    rimColor.rgb *= rimColor.a;
                    color.a += rimLight * rimColor.a * _RimLightBloomIntensity;

                    #if _RIMLIGHT_ADDITIVE
                        #if _RIM_WHITEBOOSTTYPE_ALWAYS || (_RIM_WHITEBOOSTTYPE_MAINEFFECT && !MAIN_EFFECT_ENABLED)
                            color.rgb += WhiteBoost(rimLight * rimColor.rgb, rimLight * rimColor.a * _RimLightWhiteboostMultiplier);
                        #else
                            color.rgb += rimLight * rimColor.rgb * rimColor.a;
                        #endif

                    #elif _RIMLIGHT_LERP
                        #if _RIM_WHITEBOOSTTYPE_ALWAYS || (_RIM_WHITEBOOSTTYPE_MAINEFFECT && !MAIN_EFFECT_ENABLED)
                            color.rgb = lerp(color.rgb, WhiteBoost(rimColor.rgb * rimLight, rimLight * rimColor.a * _RimLightWhiteboostMultiplier), rimLight);
                        #else
                            color.rgb = lerp(color.rgb, rimColor.rgb, rimLight);
                        #endif
                    #endif
                #endif

                #ifdef EMISSION_DEFINED

                    #if defined(_EMISSION_TEXTURE_SOURCE_TEXTURE) && _EMISSIONTEXTURE_FLIPBOOK
                        float4 texEmission = tex2D(_EmissionTex, TRANSFORM_TEX(i.flipbookUV, _EmissionTex));
                    #elif defined(_EMISSION_TEXTURE_SOURCE_TEXTURE) && _EMISSIONTEXTURE_SIMPLE
                        float2 texEmission = tex2D(_EmissionTex, TRANSFORM_TEX(emissionTexUV, _EmissionTex) + _Time.x * _EmissionTexSpeed * _EmissionTex_ST.xy).rg;
                    #elif _EMISSIONTEXTURE_PULSE
                        float2 texEmission = tex2D(_PulseMask, TRANSFORM_TEX(pulseMaskUV, _PulseMask)).rg;
                    #elif (_EMISSIONTEXTURE_FLIPBOOK || _EMISSIONTEXTURE_SIMPLE) && _EMISSION_TEXTURE_SOURCE_MPM_G
                        float4 texEmission = mpmTexture.g;
                    #endif
                    #if _EMISSION_STEP_EMISSION_TEXTURE && defined(_EMISSION_TEXTURE_SOURCE_TEXTURE)
                        texEmission *= smoothstep(_EmissionMaskStepValue, _EmissionMaskStepValue + _EmissionMaskStepWidth, texEmission);
                    #endif

                    #if _EMISSIONTEXTURE_SIMPLE

                        #if defined(_EMISSION_TEXTURE_SOURCE_TEXTURE)
                            float emissionCoverage = texEmission.r;
                        #elif _EMISSION_TEXTURE_SOURCE_FILL
                            float emissionCoverage = 1.0;
                        #elif _EMISSION_TEXTURE_SOURCE_MPM_G
                            float emissionCoverage = mpmTexture.g;
                        #endif

                        #if (!_EMISSION_ALPHA_SOURCE_COPY_EMISSION && !_EMISSION_ALPHA_SOURCE_MPM_R) && defined(_EMISSION_TEXTURE_SOURCE_TEXTURE)
                            float emissionAlpha = texEmission.g;
                        #elif _EMISSION_ALPHA_SOURCE_MPM_R
                            float emissionAlpha = mpmTexture.r;
                        #else // _EMISSION_ALPHA_SOURCE_COPY_EMISSION
                            float emissionAlpha = emissionCoverage;
                        #endif

                    #elif _EMISSIONTEXTURE_PULSE

                        float threshold = frac(_Time.x * _PulseSpeed);

                        float texRemapped = (1.0 - 2.0 *_PulseSmooth - 2.0 * _PulseWidth) * texEmission.r + _PulseSmooth + _PulseWidth;

                        float pulse = clamp((texRemapped - threshold + _PulseWidth + _PulseSmooth) / (2 * _PulseSmooth), 0.0, 1.0);
                        pulse -= clamp((texRemapped - threshold - _PulseWidth + _PulseSmooth) / (2 * _PulseSmooth), 0.0, 1.0);

                        #if INVERT_PULSE
                            pulse = 1.0 - pulse;
                        #endif

                        //Brightness from texture enabled - using green channel as that is the whiteboost mask, if we just want a regular texture mask swap to red
                        #if PULSE_MULTIPLY_TEXTURE
                            pulse *= texEmission.g;
                        #endif

                        float emissionCoverage = pulse;
                        float emissionAlpha = pulse;

                    #elif _EMISSIONTEXTURE_FLIPBOOK
                        float emissionCoverage = float2(texEmission.r * i.flipbookChannelMix.r + texEmission.g * i.flipbookChannelMix.g + texEmission.b * i.flipbookChannelMix.b + texEmission.a * i.flipbookChannelMix.a, 1.0);
                        float emissionAlpha = emissionCoverage;
                    #endif

                    #if EMISSION_MASK
                        half emissionMaskIntensity = UNITY_ACCESS_INSTANCED_PROP(Props, _EmissionMaskIntensity);
                        float2 emissionMask = tex2D(_EmissionMask, TRANSFORM_TEX(emissionMaskUV, _EmissionMask) + _Time.x * _EmissionMaskSpeed * _EmissionMask_ST.xy).rg;
                        #if _EMISSION_STEP_MASK
                            emissionMask *= smoothstep(_EmissionMaskStepValue, _EmissionMaskStepValue + _EmissionMaskStepWidth, emissionMask);
                        #endif

                        #if _MASKBLEND_MASKED_ADD
                            emissionCoverage *= 1.0 + emissionMask.r * emissionMaskIntensity;
                            emissionAlpha *= 1.0 + emissionMask.g * emissionMaskIntensity;
                        #elif _MASKBLEND_ADD
                            emissionCoverage += emissionMaskIntensity * emissionMask.r;
                            emissionAlpha += emissionMaskIntensity * emissionMask.g;
                        #else // default Blend is Multiply
                            //matches the Lerp setup from CustomParticles but may be more performant
                            emissionMask = emissionMask * emissionMaskIntensity + (1.0 - emissionMaskIntensity);
                            emissionCoverage *= emissionMask.r;
                            emissionAlpha *= emissionMask.g;
                        #endif
                    #endif

                    #if SECONDARY_EMISSION_MASK
                        float3 secondaryEmissionMaskSample =  tex2D(_SecondaryEmissionMask, TRANSFORM_TEX(secondaryEmissionMaskUV, _SecondaryEmissionMask) + _Time.x * _SecondaryEmissionMaskSpeed * _SecondaryEmissionMask_ST.xy);
                        #if VERTEXDISPLACEMENT_MASK
                            float2 secondaryEmissionMask = _VertexDisplacementMaskMixer.x * secondaryEmissionMaskSample.r + _VertexDisplacementMaskMixer.y * secondaryEmissionMaskSample.g + _VertexDisplacementMaskMixer.z * secondaryEmissionMaskSample.b;
                        #else
                            float2 secondaryEmissionMask = secondaryEmissionMaskSample.rg;
                        #endif
                        #if _EMISSION_STEP_SECONDARY_MASK
                            secondaryEmissionMask *= smoothstep(_EmissionMaskStepValue, _EmissionMaskStepValue + _EmissionMaskStepWidth, secondaryEmissionMask);
                        #endif

                        #if _SECONDARY_MASK_BLEND_MASKED_ADD
                            emissionCoverage *= 1.0 + secondaryEmissionMask.r * _SecondaryEmissionMaskIntensity;
                            emissionAlpha *= 1.0 + secondaryEmissionMask.g * _SecondaryEmissionMaskIntensity;
                        #elif _SECONDARY_MASK_BLEND_ADD
                            emissionCoverage += _SecondaryEmissionMaskIntensity * secondaryEmissionMask.r;
                            emissionAlpha += _SecondaryEmissionMaskIntensity * secondaryEmissionMask.g;
                        #else // default Blend is Multiply
                            //matches the Lerp setup from CustomParticles but may be more performant
                            secondaryEmissionMask = secondaryEmissionMask * _SecondaryEmissionMaskIntensity + (1.0 - _SecondaryEmissionMaskIntensity);
                            emissionCoverage *= secondaryEmissionMask.r;
                            emissionAlpha *= secondaryEmissionMask.g;
                        #endif
                    #endif

                    #if COLOR_ARRAY
                        fixed4 texEmissionColor = _ColorsArray[round(i.uv2.x * 10 + i.uv2.y)];
                    #else
                        fixed4 texEmissionColor = UNITY_ACCESS_INSTANCED_PROP(Props, _EmissionTexColor);
                    #endif

                    emissionCoverage *= _EmissionBrightness;
                    emissionAlpha *= _EmissionBrightness;
                    #if ENABLE_EMISSION_ANGLE_DISAPPEAR
                        emissionCoverage *= i.viewAngle;
                        emissionAlpha *= i.viewAngle;
                    #endif

                    #if _VERTEXMODE_EMISSIVE_MULT_ADD // multiplication part of MultAdd blend mode
                        emissionCoverage *= i.color.a;
                        emissionAlpha *= i.color.a;
                    #endif

                    color.a += emissionAlpha * emissionAlpha * 3.5 * texEmissionColor.a * _EmissionTexBloomIntensity; // magic number that keeps bloomed PC and white boosted Quest visuals appear more similar


                    #if _EMISSIONCOLORTYPE_WHITEBOOST || (_EMISSIONCOLORTYPE_MAINEFFECT && !MAIN_EFFECT_ENABLED)
                        color.rgb += WhiteBoost(texEmissionColor.rgb * emissionCoverage * texEmissionColor.a, emissionAlpha * emissionAlpha * texEmissionColor.a * _EmissionTexWhiteboostMultiplier);
                    #elif _EMISSIONCOLORTYPE_GRADIENT
                        color.rgb += saturate(tex2D(_EmissionGradientTex, half2(emissionAlpha, 0.5)) * emissionCoverage);
                    #else
                        color.rgb += texEmissionColor.rgb * emissionCoverage * texEmissionColor.a;
                    #endif

                #endif // End of Texture Emission

                #if _VERTEXMODE_EMISSION || _VERTEXMODE_SPECIAL || _VERTEXMODE_EMISSIVE_MULT_ADD
                    #if COLOR_ARRAY
                        fixed4 instancedColor = _ColorsArray[round(i.uv2.x * 10 + i.uv2.y)];
                    #else
                        fixed4 instancedColor = UNITY_ACCESS_INSTANCED_PROP(Props, _EmissionColor);
                    #endif

                    float emission = smoothstep(_EmissionThreshold, 1.0, i.color.g) * _EmissionStrength;
                    float4 emissionColor = instancedColor * instancedColor.a * emission;

                    #if (!MAIN_EFFECT_ENABLED && _VERTEX_WHITEBOOSTTYPE_MAINEFFECT) || _VERTEX_WHITEBOOSTTYPE_ALWAYS
                        emissionColor.rgb = WhiteBoost(emissionColor.rgb, emissionColor.a * i.color.a) * _QuestWhiteboostMultiplier;
                    #endif

                    color.rgb += emissionColor;

                    #if _VERTEXMODE_EMISSION || _VERTEXMODE_SPECIAL
                        color.a += i.color.a * i.color.a * instancedColor.a * _EmissionBloomIntensity;
                    #elif _VERTEXMODE_EMISSIVE_MULT_ADD // add part of MultAdd blend mode
                        color.a += i.color.g * i.color.g * instancedColor.a * _EmissionBloomIntensity; //TODO: unify to use alpha here and green for mult, will need adjustment of saber blade meshes
                    #endif
                #endif

                #if _PARALLAX_FLEXIBLE

                    #if _PARALLAX_FLEXIBLE_REFLECTED
                        float3 dirType = reflect(viewDir, normal); //TODO test if i.worldRefl wouldn't be enough
                    #else
                        float3 dirType = normalize(i.viewDirTangent);
                    #endif

                    #if PARALLAX_IRIDESCENCE
                        fixed3 hsb = fixed3(frac(_IridescenceTiling * (dirType.x * _IridescenceAxesMultiplier.x + dirType.y * _IridescenceAxesMultiplier.y + dirType.z * _IridescenceAxesMultiplier.z)), 1.0, 1.0);
                        fixed3 rgb = clamp(abs(fmod(hsb.x * 6.0 + fixed3(0.0, 4.0, 2.0),6.0) - 3.0) -1.0, 0.0, 1.0);
                        rgb = rgb * rgb * (3.0 - 2.0 * rgb);
                        fixed3 iridescentColor = hsb.z * lerp(fixed3(1.0, 1.0, 1.0), rgb, hsb.y);
                        float3 parallax = 0.0;
                    #else
                        float parallax = 0.0;
                    #endif

                    #if _PARALLAX_PROJECTION_WARPED
                        _ParallaxMap_ST.xy *= 2;
                    #endif

                    for (int j = 0; j < _Layers; j ++) {
                        float layer = tex2D(_ParallaxMap, TRANSFORM_TEX(parralaxUV.xy, _ParallaxMap) + _Time.x * _ParallaxTexSpeed * _ParallaxMap_ST.xy + (_StartOffset + _OffsetStep * j) * dirType);

                        #if PARALLAX_IRIDESCENCE
                            fixed3 layerColor;
                            if (j <= 0.1) {
                                layerColor = iridescentColor.rgb;
                            } else if (j <= 1.1) {
                                layerColor = iridescentColor.gbr;
                            } else if (j <= 2.1) {
                                layerColor = iridescentColor.bgr;
                            } else if (j <= 3.1) {
                                layerColor = iridescentColor.rbg;
                            } else if (j <= 4.1) {
                                layerColor = iridescentColor.grb;
                            } else {
                                layerColor = iridescentColor.brg;
                            }
                            parallax += layer * (_ParallaxIntensity + _ParallaxIntensity_Step * j) * layerColor;
                        #else
                            parallax += layer * (_ParallaxIntensity + _ParallaxIntensity_Step * j);
                        #endif
                    }

                    #if _PARALLAX_MASKING_TEXTURE
                        parallax *= tex2D(_ParallaxMaskingMap, TRANSFORM_TEX(i.uv.xy, _ParallaxMaskingMap) + _Time.x * _ParallaxMaskSpeed * _ParallaxMaskingMap_ST.xy) * _ParallaxMaskIntensity + (1.0 - _ParallaxMaskIntensity);
                    #elif _PARALLAX_MASKING_VERTEX_COLOR
                        parallax *= i.color.g;
                    #endif

                    #if PARALLAX_IRIDESCENCE
                        fixed4 parallaxColor = UNITY_ACCESS_INSTANCED_PROP(Props, _ParallaxColor);
                        color.rgb += lerp(parallax.rgb, (parallax.r + parallax.g + parallax.b) * 0.5 * parallaxColor.rgb, UNITY_ACCESS_INSTANCED_PROP(Props, _IridescenceColorInfluence)) * parallaxColor.a;
                    #else
                        color.rgb += parallax * UNITY_ACCESS_INSTANCED_PROP(Props, _ParallaxColor);
                    #endif
                #endif

                #if OCCLUSION && !OCCLUSION_BEFORE_EMISSION
                    color *= occlusion;
                #endif

                #if OCCLUSION_DETAIL && !OCCLUSION_BEFORE_EMISSION
                    color *= occlusionDetail;
                #endif

                #if HIGHLIGHT_SELECTION
                    float gradientSweep = smoothstep(1.0, 0.0, saturate(frac(1.0 * i.worldPos.y + 0.2 * i.worldPos.x + _Time.w * 0.15)) * 5.0);
                    gradientSweep *= saturate(lerp(100.0, 0.0, gradientSweep)); // smooth the sharp edge
                    color.rgb += saturate(i.segmentHighlight * gradientSweep * gradientSweep * 0.4);
                #endif

                #if _HOLOGRAM_GRID || _HOLOGRAM_SCANLINE || _HOLOGRAM_LEGACY
                    #if _HOLOGRAM_GRID // TODO Discuss/check if the normdist is needed
                        half3 holoOverlay = (1.0 - 0.6 * normDist) * UNITY_ACCESS_INSTANCED_PROP(Props, _HologramColor) * gridHolo;

                        #if HOLOGRAM_MATERIALIZATION
                            half avatarAlpha = max(holoMaterialize, max(scanlineReveal, gridHolo));
                            color.rgb = color.rgb * avatarAlpha + holoOverlay * _HoloIntensity;
                            color.a = lerp(avatarAlpha, color.a, holoMaterialize);
                        #else
                            color.rgb += holoOverlay * _HoloIntensity;
                        #endif

                    #elif _HOLOGRAM_SCANLINE
                        color.rgb += UNITY_ACCESS_INSTANCED_PROP(Props, _HologramColor) * _HoloIntensity * HologramDigiScan(i.worldPos,
                        UNITY_ACCESS_INSTANCED_PROP(Props, _HologramStripeSpeed), _HologramScanDistance,
                        UNITY_ACCESS_INSTANCED_PROP(Props, _HologramPhaseOffset), holoTime);
                    #elif _HOLOGRAM_LEGACY
                        color.rgb += HologramOverlayValue(i.worldPos, _HologramGridSize) * UNITY_ACCESS_INSTANCED_PROP(Props, _HologramColor);
                    #endif
                #endif

                // Cutout and Note Plane Cut
                #if CUTOUT_TYPE_HD_DISSOLVE || CUTOUT_TYPE_LW_SCALE || NOTE_PLANE_CUT_HD_DISSOLVE || NOTE_PLANE_CUT_LW_SNAP
                    float cutout = 0.0;
                    half edgeBoost = 0.0;

                    #if CUTOUT_TYPE_HD_DISSOLVE || CUTOUT_TYPE_LW_SCALE
                        cutout += UNITY_ACCESS_INSTANCED_PROP(Props, _Cutout);
                    #endif

                    #if CUTOUT_TYPE_HD_DISSOLVE
                        #if CLOSE_TO_CAMERA_CUTOUT
                            cutout += saturate(1 + _CloseToCameraCutoutOffset - i.camDistance * _CloseToCameraCutoutScale);
                        #endif
                        float cutoutTexOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _CutoutTexOffset);
                        edgeBoost += CutoutWithEdgeBoost(cutout, i.worldPos, cutoutTexOffset, _CutoutTexScale);
                    #endif

                    #if NOTE_PLANE_CUT_LW_SNAP || NOTE_PLANE_CUT_HD_DISSOLVE
                        const half4 cutPlane = UNITY_ACCESS_INSTANCED_PROP(Props, _CutPlane);
                        half d = dot(i.localVertex.xyz, cutPlane.xyz) + cutPlane.w; // Signed distance from plane
                        #if CUTOUT_TYPE_LW_SCALE
                            d -= cutout * (_NoteSize + cutPlane.w);
                        #endif

                        #if NOTE_PLANE_CUT_HD_DISSOLVE
                            clip(d);
                            edgeBoost += smoothstep(_CutPlaneEdgeGlowWidth + 0.005, _CutPlaneEdgeGlowWidth, d);
                        #else
                            edgeBoost += smoothstep(_CutPlaneEdgeGlowWidth + 0.005, _CutPlaneEdgeGlowWidth, d + step(0.0, 0.001 - d));
                            color.rgb *= max(0.1, step(0.001, d)); // Darken the cut face
                        #endif
                    #endif

                    // Glow
                    #if CUTOUT_TYPE_HD_DISSOLVE || NOTE_PLANE_CUT_HD_DISSOLVE || NOTE_PLANE_CUT_LW_SNAP
                        #if !MAIN_EFFECT_ENABLED // Without more in-depth refactor there's no need for whiteboost as there is no gradient in the "alpha"
                            fixed3 glowCutoutColor = fixed3(1.0, 1.0, 1.0);
                        #else
                            fixed3 glowCutoutColor = UNITY_ACCESS_INSTANCED_PROP(Props, _GlowCutoutColor);
                        #endif
                        edgeBoost = saturate(edgeBoost);
                        color.a += edgeBoost;
                        color.rgb = lerp(color.rgb, glowCutoutColor.rgb, edgeBoost);
                    #endif
                #endif

                #if ACES_TONE_MAPPING && !_ACES_APPROACH_BEFORE_EMISSIVE
                    color.rgb = ACESFilmTonemapping(color.rgb);
                #endif

                #if COLOR_BY_FOG || FOG
                    float4 fogColor = FogColor(i.fogScreenPos);
                #endif

                #if COLOR_BY_FOG
                    #if FOG_COLOR_HIGHLIGHT
                        float highlightFog = max(fogColor.r, max(fogColor.g, fogColor.b));
                        highlightFog *= highlightFog * highlightFog * highlightFog * _ColorFogHighlightMultiplier;
                        highlightFog = min(highlightFog, _ColorFogMax);
                    #endif

                    fogColor *= _ColorFogMultiplier;

                    #if FOG_COLOR_HIGHLIGHT
                        fogColor += fogColor * highlightFog;
                    #endif

                    fogColor = min(fogColor, _ColorFogMax);
                    float colorContribution = _ColorFogInfluence;

                    color.rgb = fogColor.rgb + color.rgb * colorContribution;
                #endif

                #if FOG
                    // Fog suppression for emissive effects
                    #if _VERTEXMODE_EMISSION || _VERTEXMODE_SPECIAL || _EMISSIONTEXTURE_SIMPLE || _EMISSIONTEXTURE_PULSE || _EMISSIONTEXTURE_FLIPBOOK
                        half fogSuppression = 0;

                        #ifdef EMISSION_DEFINED
                            #if !MAIN_EFFECT_ENABLED
                                fogSuppression += _EmissionFogSuppression * emissionCoverage * texEmissionColor.a;
                            #else
                                fogSuppression += _MainEffectFogSuppression * emissionCoverage * texEmissionColor.a;
                            #endif
                        #endif

                        #if (_VERTEXMODE_EMISSION || _VERTEXMODE_SPECIAL)
                            #if !MAIN_EFFECT_ENABLED
                                fogSuppression += _EmissionFogSuppression * emissionColor.a;
                            #else
                                fogSuppression += _MainEffectFogSuppression * emissionColor.a;
                            #endif
                        #endif

                        _FogScale *= 1.0 - fogSuppression;
                    #endif

                    #if HEIGHT_FOG
                        #if HEIGHT_FOG_DEPTH_SOFTEN
                            _FogHeightScale = _FogHeightScale / (_FogSoften * dist * 0.01);
                            _FogHeightOffset -= _FogSoftenOffset * dist * 0.001;
                        #endif
                        half fogStrength = FogStrength(dist * dist, i.worldPos.y * _FogHeightScale + _FogHeightOffset, _FogStartOffset, _FogScale);
                    #else
                        half fogStrength = FogStrength(dist * dist, _FogStartOffset, _FogScale);
                    #endif
                    color = lerp(color, fogColor, fogStrength);
                #endif

                #if LINEAR_TO_GAMMA
                    color.rgb = LinearToGammaSpace(color.rgb);
                #endif

                // Noise
                #if NOISE_DITHERING
                    color.rgb += BlueNoise(i.noiseScreenPos).rrr;
                #endif

                #if DISSOLVE
                    half dissolveInvert = _InvertDissolve > 0.5 ? -1.0 : 1.0;

                    #if _DISSOLVE_SPACE_AVATAR && AVATAR_COMPUTE_SKINNING
                        half pos = 1.0f - (1.0f - step(_FadeStartY, i.originalPos.y)) * (1.0f - step(_FadeZoneInterceptX + _FadeZoneSlope * (_FadeStartY - i.originalPos.y), abs(i.originalPos.x))); // will need to be updated if we implement Avatar GPU skinning
                        // area inside the fade zone fades based on y
                        pos += saturate(map(i.originalPos.y, _FadeStartY, _FadeEndY, 1.0f, 0.0f));
                        pos = saturate(pos);
                    #else
                        _DissolveAxisVector = normalize(_DissolveAxisVector);

                        #if DISSOLVE_PROGRESS
                            _DissolveOffset = lerp(_DissolveStartValue, _DissolveEndValue, abs(_DissolveProgress));
                            _DissolveAxisVector *= _DissolveProgress < -0.001 ? -1.0 : 1.0;
                        #endif

                        #if _DISSOLVE_SPACE_WORLD
                            half pos = dissolveInvert * (dot(i.worldPos.xyz, _DissolveAxisVector.xyz) - _DissolveOffset);
                        #elif _DISSOLVE_SPACE_WORLD_CENTERED
                            half pos = dissolveInvert * (dot(i.worldPos.xyz - mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz, _DissolveAxisVector.xyz) - _DissolveOffset);
                        #elif _DISSOLVE_SPACE_UV
                            half pos = dissolveInvert * (dot(i.uv.xy, _DissolveAxisVector.xy) - _DissolveOffset);
                        #else // _DISSOLVE_SPACE_LOCAL
                            half pos = dissolveInvert * (dot(i.localPos.xyz, _DissolveAxisVector.xyz) - _DissolveOffset);
                        #endif
                    #endif

                    #if DISSOLVE_TEXTURE
                        half gradientTextureInfluence = saturate((0.5 - pos) * (0.5 + pos) * _DissolveTextureInfluence);
                        half dissolve_texture_influence = tex2D(_DissolveTexture, TRANSFORM_TEX(i.uv, _DissolveTexture) + _Time.y * _DissolveTextureSpeed).x;
                        dissolve_texture_influence = dissolve_texture_influence * 2.0 - 1.0;
                        half posWithTexture = pos + dissolve_texture_influence * _DissolveTextureInfluence * gradientTextureInfluence;

                        // pos for color calculations needs to be moved to the start of the gradient for color to work properly
                        pos -= _DissolveTextureInfluence * gradientTextureInfluence;
                    #endif

                    #if DISSOLVE_COLOR
                        _CutColorFalloff *= facing ? 1.0 : _CutColorBacksideFalloff;
                        half colorGradient = saturate(1.0- pos * _CutColorFalloff);
                        colorGradient *= colorGradient * colorGradient;
                        half dissolveEmission = colorGradient;


                        #if defined(DISSOLVE_GRID)
                            #if _DISSOLVE_GRID_LOCAL
                                float3 grid = sin(frac(i.localPos * _GridSize - _Time.y * _GridSpeed) * UNITY_PI) * _GridThickness;
                                float gridResult = grid.y * grid.x * grid.z;
                            #elif _DISSOLVE_GRID_WORLD
                                float3 grid = sin(frac(i.worldPos * _GridSize - _Time.y * _GridSpeed) * UNITY_PI) * _GridThickness;
                                float gridResult = grid.y * grid.x * grid.z;
                            #elif _DISSOLVE_GRID_UV
                                float2 grid = sin(frac(i.uv * _GridSize - _Time.y * _GridSpeed) * UNITY_PI) * _GridThickness;
                                float gridResult = grid.y * grid.x;
                            #endif

                            gridResult = saturate(1.0 - gridResult);
                            half gridGradient = saturate(1.0 - pos * _GridFalloff);
                            gridGradient *= facing ? 1.0 : 0.00;
                            gridResult *= gridGradient;
                            gridResult *= gridResult;
                            dissolveEmission = max(colorGradient, gridResult);
                        #endif

                        color.rgb = lerp(color.rgb, _DissolveColorIntensity * _DissolveColor.rgb, dissolveEmission * _DissolveColor.a);
                        #if MAIN_EFFECT_ENABLED
                            color.a += dissolveEmission * _DissolveColor.a;
                        #endif
                    #endif

                    #if DISSOLVE_TEXTURE
                        pos = posWithTexture;
                    #endif

                    #if _DISSOLVEALPHA_FADE || _DISSOLVEALPHA_BOTH
                        float alphaValue = saturate(pos * _DissolveGradientWidth + 0.5);
                        color.a = _AlphaMultiplier * alphaValue;
                        #if HOLOGRAM_MATERIALIZATION && AVATAR_COMPUTE_SKINNING
                            color.a *= lerp(avatarAlpha, 1.0, holoMaterialize);
                        #endif
                        color.rgb *= color.a * color.a;
                    #endif

                    #if _DISSOLVEALPHA_BOTH
                        clip(alphaValue - 0.0001);
                    #elif !_DISSOLVEALPHA_FADE // stands for implicit _DISSOLVEALPHA_CLIP
                        clip(pos);
                    #endif
                #endif

                APPLY_FAKE_MIRROR_TRANSPARENCY_SQUARED(color)

                return color;
            }
            ENDCG
        }

        // This pass is used for Depth Texture mostly used for Soft particle fading
        // Anything that moves vertex positions should be copied to this pass
        Pass {

            Tags {
                "LightMode"="ShadowCaster"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragShadow
            #pragma multi_compile_instancing
            #pragma shader_feature _ CUTOUT_TYPE_HD_DISSOLVE CUTOUT_TYPE_LW_SCALE
            #pragma shader_feature MESH_PACKING
            #pragma shader_feature_local _ _SPECTROGRAM_FLAT _SPECTROGRAM_FULL
            #pragma shader_feature_local DISPLACEMENT_SPATIAL
            #pragma shader_feature_local DISPLACEMENT_BIDIRECTIONAL
            #pragma shader_feature_local _ _VERTEXMODE_COLOR _VERTEXMODE_EMISSION _VERTEXMODE_METALSMOOTHNESS _VERTEXMODE_DISPLACEMENT _VERTEXMODE_SPECIAL

            #include "UnityCG.cginc"
            #if CUTOUT_TYPE_HD_DISSOLVE
                #include "Assets/Visuals/Shaders/Rendering/Cutout3D.cginc"
                half _CutoutTexScale;
            #endif

            #if _SPECTROGRAM_FULL
                #define SPECTROGRAM_SIZE 64
                float _SpectrogramData[SPECTROGRAM_SIZE];
            #endif

            UNITY_INSTANCING_BUFFER_START(Props)
                #if CUTOUT_TYPE_HD_DISSOLVE || CUTOUT_TYPE_LW_SCALE
                    UNITY_DEFINE_INSTANCED_PROP(half4, _CutoutTexOffset)
                    UNITY_DEFINE_INSTANCED_PROP(half, _Cutout)
                #endif
                #if MESH_PACKING
                    UNITY_DEFINE_INSTANCED_PROP(half, _MeshPackingId)
                #endif
                #if _VERTEXMODE_DISPLACEMENT
                    UNITY_DEFINE_INSTANCED_PROP(half4, _DisplacementAxisMultiplier)
                #endif
                #if _SPECTROGRAM_FLAT
                    UNITY_DEFINE_INSTANCED_PROP(half, _SpectrogramData)
                #endif
                #if _VERTEXMODE_DISPLACEMENT
                    UNITY_DEFINE_INSTANCED_PROP(half, _DisplacementStrength)
                #endif
            UNITY_INSTANCING_BUFFER_END(Props)

            struct appdata {

                float4 vertex : POSITION;

                #if _VERTEXMODE_DISPLACEMENT && !DISPLACEMENT_SPATIAL
                    half3 normal : NORMAL;
                #endif

                #if MESH_PACKING
                    float2 uv2 : TEXCOORD1;
                #endif
                #if _VERTEXMODE_DISPLACEMENT
                    fixed4 color : COLOR;
                #endif
                #if _SPECTROGRAM_FULL
                    float2 uv3 : TEXCOORD2;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {

                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;

                #if ENABLE_PLANE_CUT
                    float4 localVertex : TEXCOORD2;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v) {

                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                #if _VERTEXMODE_DISPLACEMENT
                    float4 displacementVector = UNITY_ACCESS_INSTANCED_PROP(Props, _DisplacementAxisMultiplier);
                    float displacementStrength = UNITY_ACCESS_INSTANCED_PROP(Props, _DisplacementStrength);
                    #if _SPECTROGRAM_FLAT
                        displacementStrength *= UNITY_ACCESS_INSTANCED_PROP(Props, _SpectrogramData);
                    #elif _SPECTROGRAM_FULL
                        displacementStrength *= _SpectrogramData[v.uv3.x * (SPECTROGRAM_SIZE - 1)];
                    #endif

                    #if DISPLACEMENT_SPATIAL
                        #if DISPLACEMENT_BIDIRECTIONAL
                            v.vertex.xyz += displacementStrength.xxx * (2.0 * v.color.rgb - float3 (1.0, 1.0, 1.0)) * displacementVector.rgb;
                        #else
                            v.vertex.xyz += displacementStrength.xxx * v.color.rgb * displacementVector.rgb;
                        #endif
                    #else
                        v.vertex.xyz += displacementStrength.xxx * v.color.b * displacementVector.rgb * v.normal;
                    #endif
                #endif

                #if CUTOUT_TYPE_LW_SCALE
                    half cutout = UNITY_ACCESS_INSTANCED_PROP(Props, _Cutout);
                #endif

                #if NOTE_PLANE_CUT_LW_SNAP
                    // p' = p - (n ⋅ p + d) * n
                    half4 cutPlane = UNITY_ACCESS_INSTANCED_PROP(Props, _CutPlane);
                    half d = dot(cutPlane.xyz, v.vertex.xyz) + cutPlane.w;
                    #if CUTOUT_TYPE_LW_SCALE
                        d -= cutout * (_NoteSize + cutPlane.w);
                    #endif
                    v.vertex.xyz = v.vertex.xyz - min(0, d) * cutPlane.xyz;
                #endif

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                #if CUTOUT_TYPE_LW_SCALE
                    #if CLOSE_TO_CAMERA_CUTOUT
                        float camDistance = length(o.worldPos - _WorldSpaceCameraPos);
                        cutout += saturate(1 + _CloseToCameraCutoutOffset - camDistance * _CloseToCameraCutoutScale);
                    #endif
                    #if NOTE_PLANE_CUT_LW_SNAP
                        half t = smoothstep(0.5, 1, cutout);
                        v.vertex.xyz *= 1.0 - t;
                        v.vertex.xyz += cutPlane.xyz * t * _NoteSize;
                    #else
                        v.vertex.xyz *= 1.0 - cutout;
                    #endif
                #endif

                #if NOTE_VERTEX_DISTORTION && MIRROR_VERTEX_DISTORTION && FAKE_MIRROR_TRANSPARENCY
                    float3 vertexOffset = GetFakeMirrorDistortionVertexOffset(o.worldPos);
                    v.vertex.xyz += vertexOffset;
                #endif

                #if MESH_PACKING
                    if (abs(v.uv2.y - UNITY_ACCESS_INSTANCED_PROP(Props, _MeshPackingId)) > 0.1) {
                        v.vertex = float4(0.0, 0.0, 0.0, 0.0);
                    }
                #endif

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 fragShadow(v2f i) : SV_Target {

                UNITY_SETUP_INSTANCE_ID(i);

                #if NOTE_PLANE_CUT_HD_DISSOLVE
                    half4 cutPlane = UNITY_ACCESS_INSTANCED_PROP(Props, _CutPlane);
                    half d = dot(i.localVertex.xyz, cutPlane.xyz) + cutPlane.w; // Signed distance from plane.
                    clip(d);
                #endif

                #if CUTOUT_TYPE_HD_DISSOLVE
                    float3 objectPos = float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]); // World pos of object
                    half cutout = UNITY_ACCESS_INSTANCED_PROP(Props, _Cutout);
                    half cutoutSample = tex3D(_CutoutTex, (i.worldPos.xyz - objectPos + UNITY_ACCESS_INSTANCED_PROP(Props, _CutoutTexOffset)) * _CutoutTexScale).a;
                    clip(cutoutSample - cutout * 1.1 + 0.1);
                #endif

                return fixed4(0.0, 0.0, 0.0, 1.0);
            }
            ENDCG
        }

        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass is not used during regular rendering.
        Pass
        {
            Name "META"
            Tags {"LightMode"="Meta"}
            Cull Off

            CGPROGRAM

                #include"UnityStandardMeta.cginc"

                float4 frag_meta2 (v2f_meta i): SV_Target {

                    // We're interested in diffuse & specular colors
                    // and surface roughness to produce final albedo.

                    FragmentCommonData data = UNITY_SETUP_BRDF_INPUT (i.uv);
                    UnityMetaInput o;

                    UNITY_INITIALIZE_OUTPUT(UnityMetaInput, o);

                    o.Albedo = _Color;
                    o.Emission = 0;

                    return UnityMetaFragment(o);
                }

                #pragma vertex vert_meta
                #pragma fragment frag_meta2
                #pragma shader_feature ___ _DETAIL_MULX2

            ENDCG
        }
    }
}
