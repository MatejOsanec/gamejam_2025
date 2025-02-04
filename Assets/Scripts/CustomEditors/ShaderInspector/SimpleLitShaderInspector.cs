namespace BGLib.ShaderInspector {

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEngine;

    [UsedImplicitly]
    public class SimpleLitShaderInspector : ShaderInspector {

        protected override IReadOnlyList<Element> GetRootElements() {

            return new List<Element>() {
                OverarchingFeatures(),
                BaseProperties(),
                Emissions(),
                Occlusions(),
                Lighting(),
                Fog(),
                SpecialContexts(),
                Other(),
                Settings(),
            };
        }

        private static Element LightingPreset() {

            return new PresetDropdown("_LightingPreset", "Lighting Preset", options: new List<PresetDropdown.Option>() {
                new PresetDropdown.Option("Custom"),
                new PresetDropdown.Option("Diffuse and Specular", presetValues: new Collection<PresetDropdown.PresetValue>() {
                    new PresetDropdown.KeywordPreset("_EnableDiffuse", "DIFFUSE", true),
                    new PresetDropdown.KeywordPreset("_EnableSpecular", "SPECULAR", true),
                    new PresetDropdown.KeywordPreset("_EnableReflectionTexture", "REFLECTION_TEXTURE", false),
                    new PresetDropdown.KeywordPreset("_EnableReflectionProbe", "REFLECTION_PROBE", false),
                }),
                new PresetDropdown.Option("Diffuse and Probe", presetValues: new Collection<PresetDropdown.PresetValue>() {
                    new PresetDropdown.KeywordPreset("_EnableDiffuse", "DIFFUSE", true),
                    new PresetDropdown.KeywordPreset("_EnableSpecular", "SPECULAR", false),
                    new PresetDropdown.KeywordPreset("_EnableReflectionTexture", "REFLECTION_TEXTURE", false),
                    new PresetDropdown.KeywordPreset("_EnableReflectionProbe", "REFLECTION_PROBE", true),
                }),
                new PresetDropdown.Option("Reflection Probe", presetValues: new Collection<PresetDropdown.PresetValue>() {
                    new PresetDropdown.KeywordPreset("_EnableDiffuse", "DIFFUSE", false),
                    new PresetDropdown.KeywordPreset("_EnableSpecular", "SPECULAR", false),
                    new PresetDropdown.KeywordPreset("_EnableReflectionTexture", "REFLECTION_TEXTURE", false),
                    new PresetDropdown.KeywordPreset("_EnableReflectionProbe", "REFLECTION_PROBE", true),
                }),
            });
        }

        private static Element BloomSettingsGroup() {

            return new Group(childElements: new List<Element>() {
                EmissionColorType(),
                RimLightBloom(),
                VertexModeBloom(),
                BlendSrcAlphaPreset(),

                new InfoBox("At least one of the Bloom options uses Bloom. Make sure to enable it", MessageType.Warning,
                    displayFilter: properties => properties.HasAnyOfKeywords("_VERTEX_WHITEBOOSTTYPE_MAINEFFECT",
                        "_RIM_WHITEBOOSTTYPE_MAINEFFECT", "_EMISSIONCOLORTYPE_MAINEFFECT") &&
                        properties.FloatPropertyComparison("_BlendSrcFactorA", DisplayFilter.ComparisonType.NotEqual, 5.0f) &&
                        properties.FloatPropertyComparison("_BlendSrcFactorA", DisplayFilter.ComparisonType.NotEqual, 1.0f)),

                new InfoBox("None of the Bloom options uses Bloom. It's recommended to disable it", MessageType.Info,
                    displayFilter: properties => properties.HasNoneOfKeywords("_VERTEX_WHITEBOOSTTYPE_MAINEFFECT",
                        "_RIM_WHITEBOOSTTYPE_MAINEFFECT", "_EMISSIONCOLORTYPE_MAINEFFECT") &&
                        properties.FloatPropertyComparison("_BlendSrcFactorA", DisplayFilter.ComparisonType.NotEqual, 0.0f)),
            });
        }
        private static Element BloomPreset() {

            return new PresetDropdown("_BloomPreset", "Bloom Preset",
                tooltip: "Custom: Specify a treatment for each emissive layer individually \n\n " +
                         "No Bloom: Straightforward color, no surprises there \n\n" +
                         "Bloom: Maximizes visual quality on high end platforms at the cost of slight visual mismatch between the platforms \n\n " +
                         "Whiteboost: High intensity blends into white. No Bloom is applied on high end platforms. Useful to ensure the same looks \n\n",
                options: new List<PresetDropdown.Option>() {
                    new PresetDropdown.Option("Fully Custom", presetValues: new List<PresetDropdown.PresetValue>() {
                    }),
                    new PresetDropdown.Option("All Bloom (improves PC fidelity)", presetValues: new Collection<PresetDropdown.PresetValue>() {
                        new PresetDropdown.KeywordPreset("_Vertex_WhiteBoostType", "_VERTEX_WHITEBOOSTTYPE_MAINEFFECT", true),
                        new PresetDropdown.KeywordPreset("_Rim_WhiteBoostType", "_RIM_WHITEBOOSTTYPE_MAINEFFECT", true),
                        new PresetDropdown.KeywordPreset("_EmissionColorType", "_EMISSIONCOLORTYPE_MAINEFFECT", true),
                        new PresetDropdown.PresetPropertyPreset("_BlendSrcAlphaPreset", 1), // Bloom ON
                    }),
                    new PresetDropdown.Option("All Whiteboost (visual parity)", presetValues: new Collection<PresetDropdown.PresetValue>() {
                        new PresetDropdown.KeywordPreset("_Vertex_WhiteBoostType", "_VERTEX_WHITEBOOSTTYPE_ALWAYS", true),
                        new PresetDropdown.KeywordPreset("_Rim_WhiteBoostType", "_RIM_WHITEBOOSTTYPE_ALWAYS", true),
                        new PresetDropdown.KeywordPreset("_EmissionColorType", "_EMISSIONCOLORTYPE_WHITEBOOST", true),
                        new PresetDropdown.PresetPropertyPreset("_BlendSrcAlphaPreset", 2), // Bloom OFF
                    }),
                    new PresetDropdown.Option("All Without Bloom", presetValues: new Collection<PresetDropdown.PresetValue>() {
                        new PresetDropdown.KeywordPreset("_Vertex_WhiteBoostType", "_VERTEX_WHITEBOOSTTYPE_MAINEFFECT", false),
                        new PresetDropdown.KeywordPreset("_Rim_WhiteBoostType", "_RIM_WHITEBOOSTTYPE_MAINEFFECT", false),
                        new PresetDropdown.KeywordPreset("_EmissionColorType", "_EMISSIONCOLORTYPE_MAINEFFECT", false),
                        new PresetDropdown.PresetPropertyPreset("_BlendSrcAlphaPreset", 2), // Bloom OFF
                    }),
            });
        }

        private static Element BlendSrcAlphaPreset() {
            return new PresetDropdown("_BlendSrcAlphaPreset", "Bloom State",
                options: new List<PresetDropdown.Option>() {
                    new PresetDropdown.Option("Custom"),
                    new PresetDropdown.Option("Bloom Enabled", presetValues: new Collection<PresetDropdown.PresetValue>() {
                        new PresetDropdown.FloatPropertyPreset("_BlendSrcFactorA", 5.0f), // Zero
                    }),
                    new PresetDropdown.Option("Bloom disabled", presetValues: new Collection<PresetDropdown.PresetValue>() {
                        new PresetDropdown.FloatPropertyPreset("_BlendSrcFactorA", 0.0f), // Zero
                    }),
            });
        }

        private static Element VertexModeBloom() {
            return new KeywordDropdown("_Vertex_WhiteBoostType", KeywordDropdown.Style.SubFeature,
                tooltip: "No Bloom: Straightforward color, no surprises there \n\n " +
                "Bloom: Maximizes visual quality on high end platforms at the cost of slight visual mismatch between the platforms \n\n" +
                "Whiteboost: High intensity blends into white. No Bloom is applied on high end platforms. Useful to ensure the same looks \n\n",
                options: new List<KeywordDropdown.Option> {
                    new KeywordDropdown.Option(string.Empty, "No Bloom"),
                    new KeywordDropdown.Option("_VERTEX_WHITEBOOSTTYPE_MAINEFFECT", "Bloom (improves PC fidelity)"),
                    new KeywordDropdown.Option("_VERTEX_WHITEBOOSTTYPE_ALWAYS", "Whiteboost (visual parity)")
            });
        }

        private static Element EmissionColorType() {
            return new KeywordDropdown(propertyName: "_EmissionColorType", style: KeywordDropdown.Style.SubFeature,
                tooltip: "Flat Color: Straightforward color, no surprises there \n\n " +
                         "Whiteboost: High intensity blends into white. No Bloom is applied on high end platforms. Useful to ensure the same looks \n\n" +
                         "Gradient: Uses a texture (LUT) to define a color gradient applied to emission" +
                         "Bloom: Maximizes visual quality on high end platforms at the cost of slight visual mismatch between the platforms \n\n",
                options: new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "Flat Color, no Bloom"),
                    new KeywordDropdown.Option("_EMISSIONCOLORTYPE_WHITEBOOST", "Whiteboost (visual parity)",
                        displayFilter: properties => properties.HasNoneOfKeywords("_EMISSION_TEXTURE_SOURCE_FILL"),
                        displayFilterErrorMessage: "Emission Color Type > Whiteboost option doesn't support Emission Source: Fill"),
                    new KeywordDropdown.Option("_EMISSIONCOLORTYPE_GRADIENT", "Gradient (visual parity)",
                        displayFilter: properties => properties.HasNoneOfKeywords("_EMISSION_TEXTURE_SOURCE_FILL"),
                        displayFilterErrorMessage: "Emission Color Type > Gradient option doesn't support Emission Source: Fill"),
                    new KeywordDropdown.Option("_EMISSIONCOLORTYPE_MAINEFFECT", "Bloom (improves PC fidelity)",
                        displayFilter: properties => properties.HasNoneOfKeywords("_EMISSION_TEXTURE_SOURCE_FILL"),
                        displayFilterErrorMessage: "Emission Color Type > MainEffect option doesn't support Emission Source: Fill")
            });
        }

        private static Element RimLightBloom() {
            return new KeywordDropdown("_Rim_WhiteBoostType", KeywordDropdown.Style.SubFeature,
                tooltip: "No Bloom: Straightforward color, no surprises there \n\n " +
                "Bloom: Maximizes visual quality on high end platforms at the cost of slight visual mismatch between the platforms \n\n" +
                "Whiteboost: High intensity blends into white. No Bloom is applied on high end platforms. Useful to ensure the same looks \n\n",
                options: new List<KeywordDropdown.Option>() {
                new KeywordDropdown.Option(string.Empty, "No Bloom"),
                new KeywordDropdown.Option("_RIM_WHITEBOOSTTYPE_MAINEFFECT", "Bloom (improves PC fidelity)"),
                new KeywordDropdown.Option("_RIM_WHITEBOOSTTYPE_ALWAYS", "Whiteboost (visual parity)"),
            });
        }

        private static Element Settings() {
            return new Category("Settings", childElements: new List<Element>() {

                new EnumPropertyDropdown<UnityEngine.Rendering.CullMode>("_Cull", displayName: "Triangle Face Culling"),
                new NoKeywordFeature("_ZWrite", Feature.Style.SubFeature),
                new EnumPropertyDropdown<UnityEngine.Rendering.CompareFunction>("_ZTest", displayName: "ZTest"),

                new PresetDropdown("_BlendingPreset", "Blending Presets", options: new List<PresetDropdown.Option>() {
                        new PresetDropdown.Option(
                            displayName: "Custom",
                            description: "Manually set color and bloom blending in the section below"
                        ),
                        new PresetDropdown.Option(
                            displayName: "Solid without bloom",
                            presetValues: new Collection<PresetDropdown.PresetValue>() {
                                new PresetDropdown.FloatPropertyPreset( "_BlendSrcFactor", 1.0f), // One
                                new PresetDropdown.FloatPropertyPreset( "_BlendDstFactor", 0.0f), // Zero
                                new PresetDropdown.FloatPropertyPreset( "_BlendDstFactorA", 0.0f), // Zero
                            }),
                        new PresetDropdown.Option(
                            displayName: "Transparent Alpha Faded",
                            presetValues: new Collection<PresetDropdown.PresetValue>() {
                                new PresetDropdown.FloatPropertyPreset( "_BlendSrcFactor", 1.0f), // One
                                new PresetDropdown.FloatPropertyPreset( "_BlendDstFactor", 11.0f), // OneMinusSrcAlpha
                                new PresetDropdown.FloatPropertyPreset( "_BlendDstFactorA", 11.0f), // OneMinusSrcAlpha
                        }),
                        new PresetDropdown.Option(
                            displayName: "Transparent Additive",
                            presetValues: new Collection<PresetDropdown.PresetValue>() {
                                new PresetDropdown.FloatPropertyPreset( "_BlendSrcFactor", 1.0f), // One
                                new PresetDropdown.FloatPropertyPreset( "_BlendDstFactor", 1.0f), // One
                                new PresetDropdown.FloatPropertyPreset( "_BlendDstFactorA", 1.0f), // One
                        }),
                }),

                new Group("Stencil Setting", hasFoldout: true,
                    childElements: new List<Element>() {
                        new PresetDropdown("_StencilPreset", "Stencil Presets", options: new List<PresetDropdown.Option>() {
                            new PresetDropdown.Option(
                                displayName: "Default - None",
                                presetValues: new Collection<PresetDropdown.PresetValue>() {
                                    new PresetDropdown.FloatPropertyPreset( "_StencilRefValue", 0.0f),
                                    new PresetDropdown.FloatPropertyPreset( "_StencilComp", 8.0f), // Always
                                    new PresetDropdown.FloatPropertyPreset( "_StencilPass", 0.0f), // Keep
                                }),
                            new PresetDropdown.Option("Custom", description: "When using custom values, please add your use-case to the linked confluence page",
                                documentationUrl: "https://beatgames.atlassian.net/wiki/spaces/BS/pages/5046356/Usage+of+Stencil")
                        }),
                        new Group(enabledFilter: properties => properties.FloatPropertyComparison("_StencilPreset", DisplayFilter.ComparisonType.NotEqual, 0.0f),
                            childElements: new List<Element>() {
                                new FloatProperty("_StencilRefValue"),
                                new EnumPropertyDropdown<UnityEngine.Rendering.CompareFunction>("_StencilComp"),
                                new EnumPropertyDropdown<UnityEngine.Rendering.StencilOp>("_StencilPass"),
                        }),
                }),

                new Group("Blending Setting", hasFoldout: true,
                    childElements: new List<Element>() {
                        new Space(12.0f),
                        new Group("Color Blending", enabledFilter: properties => properties.FloatPropertyComparison("_BlendingPreset", DisplayFilter.ComparisonType.Equal, 0.0f),
                            childElements: new List<Element>() {
                                new EnumPropertyDropdown<UnityEngine.Rendering.BlendMode>("_BlendSrcFactor"),
                                new EnumPropertyDropdown<UnityEngine.Rendering.BlendMode>("_BlendDstFactor"),
                                new EnumPropertyDropdown<UnityEngine.Rendering.BlendMode>("_BlendDstFactorA"),
                        }),

                        new Group("Bloom Settings", childElements: new List<Element>() {
                            BloomPreset(),
                            BloomSettingsGroup(),
                            new EnumPropertyDropdown<UnityEngine.Rendering.BlendMode>("_BlendSrcFactorA"),
                        }),
                }),

                new MiscShaderProperties(),
                new SubFeature("MESH_PACKING", "_MeshPacking",
                    description: "Collapses any geo whose uv2.y value doesn't match the ID" +
                                 "\nID below is for debug and needs to be set via Material Property Blocks",
                    documentationButtonLabel: "docs",
                    documentationUrl: "https://beatgames.atlassian.net/wiki/spaces/BS/pages/109346823/Environment+Optimization+Performance+Testing#Mesh-Packing.1",
                    childElements: new List<Element>() {
                        new FloatProperty("_MeshPackingId"),
                    }),

            });
        }

        private static Element Dissolve() {
            return new Category("Dissolve", childElements: new List<Element>() {

            });
        }

        private static Element SpecialContexts() {
            return new Category("Special Contexts", childElements: new List<Element>() {

                new SubFeature("LIGHT_FALLOFF", "_EnableLightFalloff", description: "VERY Expensive feature applying diffuse and specular falloff",
                    displayFilter: properties => properties.HasAnyOfKeywords("DIFFUSE", "SPECULAR")),

                new KeywordDropdown("_Hologram", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("_HOLOGRAM_GRID", "Grid"),
                    new KeywordDropdown.Option("_HOLOGRAM_SCANLINE", "Scanline"),
                    new KeywordDropdown.Option("_HOLOGRAM_LEGACY", "Legacy"),
                }, childElements: new List<Element>() {

                    new SubFeature("HOLOGRAM_MATERIALIZATION", "_UseHologramMaterialization", displayFilter: properties => properties.HasAnyOfKeywords("_HOLOGRAM_GRID")),
                    new ColorProperty("_HologramColor", displayFilter: properties =>
                        properties.HasAnyOfKeywords("_HOLOGRAM_GRID", "_HOLOGRAM_SCANLINE", "_HOLOGRAM_LEGACY")),
                    new FloatProperty("_HologramGridSize", displayFilter: properties =>
                        properties.HasAnyOfKeywords("_HOLOGRAM_GRID", "_HOLOGRAM_LEGACY")),
                    new FloatProperty("_HologramFill", displayFilter: properties => properties.HasAnyOfKeywords("_HOLOGRAM_GRID")),
                    new FloatProperty("_HoloMaterialize", displayFilter: properties =>
                        properties.HasAllOfKeywords("_HOLOGRAM_GRID", "HOLOGRAM_MATERIALIZATION")),

                    new Group(displayFilter: properties => properties.HasAnyOfKeywords("_HOLOGRAM_GRID", "_HOLOGRAM_SCANLINE"), childElements: new List<Element>() {
                        new FloatProperty("_HologramStripeSpeed"),
                        new FloatProperty("_HologramScanDistance"),
                        new RangeProperty("_HologramPhaseOffset"),
                        new FloatProperty("_HoloIntensity"),
                        new FloatProperty("_HaltScan"),
                    })
                }),

                new Space(16.0f),
                new Group(title: "Beat Avatar Features", childElements: new List<Element>() {
                    new SubFeature("UV_COLOR_SEGMENTS", "_UVColorSegments", childElements: new List<Element>() {
                        new SubFeature("UV_SEGMENTS_IGNORE_RIM", "_UvSegmentsIgnoreRim")
                    }),
                    new SubFeature("HIGHLIGHT_SELECTION", "_HighlightSelection", childElements: new List<Element>() {
                        new FloatProperty("_SegmentToHighlight")
                    }),
                }),
                new Space(16.0f),

                new Group(title: "Gameplay element features", childElements: new List<Element>() {
                    new Feature("FAKE_MIRROR_TRANSPARENCY", "_FakeMirrorTransparencyEnabled", childElements: new List<Element>() {
                        new FloatProperty("_FakeMirrorTransparencyMultiplier"),
                        new SubFeature("MIRROR_VERTEX_DISTORTION", "_NoteVertexDistortion", childElements: new List<Element>() {
                            new InfoBox("Mirror Vertex Distortion will be deprecated with Tours", MessageType.Warning)
                        })
                    }),

                    new KeywordDropdown("Note_Plane_Cut", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "None"),
                        new KeywordDropdown.Option("NOTE_PLANE_CUT_HD_DISSOLVE", "HD dissolve"),
                        new KeywordDropdown.Option("NOTE_PLANE_CUT_LW_SNAP", "LW snap"),
                    }, childElements: new List<Element>() {
                        new Group(enabledFilter: properties => properties.HasAnyOfKeywords("NOTE_PLANE_CUT_HD_DISSOLVE", "NOTE_PLANE_CUT_LW_SNAP"),
                            childElements: new List<Element>() {
                                new FloatProperty("_CutPlaneEdgeGlowWidth"),
                                new FloatProperty("_NoteSize"),
                                new VectorProperty("_CutPlane", 4, uiToMaterialDelegate: InOutValueModification.Normalize),
                            })
                    }),

                    new KeywordDropdown("Cutout_Type", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "None"),
                        new KeywordDropdown.Option("CUTOUT_TYPE_HD_DISSOLVE", "HD dissolve"),
                        new KeywordDropdown.Option("CUTOUT_TYPE_LW_SCALE", "LW scale"),
                    }, childElements: new List<Element>() {
                        new Group(enabledFilter: properties => properties.HasAnyOfKeywords("CUTOUT_TYPE_HD_DISSOLVE", "CUTOUT_TYPE_LW_SCALE"),
                            childElements: new List<Element>() {
                                new RangeProperty("_Cutout"),
                                new FloatProperty("_CutoutTexScale"),
                                new SubFeature("CLOSE_TO_CAMERA_CUTOUT", "_EnableCloseToCameraCutout", childElements: new List<Element>() {
                                    new FloatProperty("_CloseToCameraCutoutOffset"),
                                    new FloatProperty("_CloseToCameraCutoutScale"),
                                }),
                            })
                    }),

                    new Group(displayFilter: properties => properties.HasAnyOfKeywords("NOTE_PLANE_CUT_HD_DISSOLVE",
                        "NOTE_PLANE_CUT_LW_SNAP", "CUTOUT_TYPE_HD_DISSOLVE", "CUTOUT_TYPE_LW_SCALE"), childElements: new List<Element>() {

                        new ColorProperty("_GlowCutoutColor"),
                        new InfoBox("Color is only visible with MainEffect ON as on Quest it is fully whiteboosted")
                    })
                }),
            });
        }

        private static Element Fog() {
            return new Category("Fog", childElements: new List<Element>() {

                new Feature("FOG", "_EnableFog", childElements: new List<Element>() {
                    new FloatProperty("_FogStartOffset"),
                    new FloatProperty("_FogScale"),
                    new SubFeature("HEIGHT_FOG", "_EnableHeightFog", childElements: new List<Element>() {
                        new FloatProperty("_FogHeightScale"),
                        new FloatProperty("_FogHeightOffset"),
                        new SubFeature("HEIGHT_FOG_DEPTH_SOFTEN", "_EnableHeightFogSoften", childElements: new List<Element>() {
                            new FloatProperty("_FogSoften"),
                            new FloatProperty("_FogSoftenOffset"),
                        }),
                    }),
                    new Group(displayFilter: properties => properties.HasAnyOfKeywords(
                        "_EMISSIONTEXTURE_SIMPLE", "_EMISSIONTEXTURE_PULSE", "_EMISSIONTEXTURE_FLIPBOOK",
                        "_VERTEXMODE_EMISSION", "_VERTEXMODE_SPECIAL"), childElements: new List<Element>() {
                            new RangeProperty("_EmissionFogSuppression"),
                            new RangeProperty("_MainEffectFogSuppression"),
                    }),
                }),

                new Feature("COLOR_BY_FOG", "_ColorFog", childElements: new List<Element>() {
                    new FloatProperty("_ColorFogMultiplier"),
                    new FloatProperty("_ColorFogMax"),
                    new RangeProperty("_ColorFogInfluence"),
                    new SubFeature("FOG_COLOR_HIGHLIGHT", "_FogColorHighlight", childElements: new List<Element>() {
                        new FloatProperty("_ColorFogHighlightMultiplier")
                    })
                }),

            });
        }

        private static Element Occlusions() {
            return new Category("Occlusions", childElements: new List<Element>() {

                new Feature("OCCLUSION", "_EnableOcclusion", childElements: new List<Element>() {
                    new KeywordDropdown("_Occlusion_Source", KeywordDropdown.Style.SubFeature, options: new Collection<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option("_OCCLUSION_SOURCE_TEXTURE", "Texture"),

                        new KeywordDropdown.Option("_OCCLUSION_SOURCE_MPM_B", "MPM Blue",
                            displayFilter: properties => properties.HasAnyOfKeywords("METAL_SMOOTHNESS_TEXTURE"),
                            displayFilterErrorMessage: "Occlusion Source > MPM Blue option requires MPM"),

                        new KeywordDropdown.Option("_OCCLUSION_SOURCE_AVATAR_MPM_R", "MPM Red (Avatars)",
                            displayFilter: properties => properties.HasAllOfKeywords("METAL_SMOOTHNESS_TEXTURE", "AVATAR_COMPUTE_SKINNING"),
                            displayFilterErrorMessage: "Occlusion Source > MPM Red (Avatars) option requires MPM and Avatar"),
                    }),

                    new TextureProperty("_DirtTex"),
                    new SubFeature("SECONDARY_UVS_OCCLUSION", "_SecondaryUVsOcclusion", displayFilter: properties => properties.HasSecondaryUVsEnabled()),
                    new RangeProperty("_OcclusionIntensity")
                }),

                new Feature("OCCLUSION_DETAIL", "_EnableOcclusionDetail", childElements: new List<Element>() {

                    new TextureProperty("_DirtDetailTex"),
                    new SubFeature("SECONDARY_UVS_OCCLUSION_DETAIL", "_SecondaryUVsOcclusionDetail", displayFilter: properties => properties.HasSecondaryUVsEnabled()),
                    new RangeProperty("_OcclusionDetailIntensity")
                }),

                new Feature("OCCLUSION_BEFORE_EMISSION", "_OcclusionBeforeEmission", enabledFilter: properties => properties.HasAnyOfKeywords("OCCLUSION", "OCCLUSION_DETAIL")),
                new InfoBox("Paired with ACES approach: Before Emissive (currently true) this will exempt occlusion from ACES color grading. " +
                            "It's okay, just something to keep in mind.",
                    displayFilter: properties => properties.HasAllOfKeywords("_ACES_APPROACH_BEFORE_EMISSIVE", "OCCLUSION_BEFORE_EMISSION")),

                new Feature("GROUND_FADE", "_EnableGroundFade", childElements: new List<Element>() {
                    new FloatProperty("_GroundFadeScale"),
                    new FloatProperty("_GroundFadeOffset")
                }),

                new Feature("DISTANCE_DARKENING", "_EnableDistanceDarkening", childElements: new List<Element>() {
                    new FloatProperty("_DarkeningScale"),
                    new FloatProperty("_DarkeningIntensity"),
                    new VectorProperty("_DarkeningCenter", 3),
                    new VectorProperty("_DarkeningDirection", 3),
                }),

            });
        }

        private static Element Lighting() {

            return new Category(title: "Lighting Options", childElements: new List<Element>() {

                LightingPreset(),
                new Space(8.0f),
                new Feature("DIFFUSE", "_EnableDiffuse", childElements: new List<Element>() {
                    new SubFeature("BOTH_SIDES_DIFFUSE", "_EnableBothSidesDiffuse", childElements: new List<Element>() {
                        new FloatProperty("_BothSidesDiffuseMultiplier"),
                    }),
                    new SubFeature("INVERT_DIFFUSE_NORMAL", "_InvertDiffuseNormal"),
                }),

                new Feature("SPECULAR", "_EnableSpecular", childElements: new List<Element>() {
                    new FloatProperty("_SpecularIntensity")
                }),

                new Feature("REFLECTION_TEXTURE", "_EnableReflectionTexture", childElements: new List<Element>() {
                    new TexturePropertyMiniThumbnail("_EnvironmentReflectionCube"),
                    new FloatProperty("_ReflectionTexIntensity"),
                }),

                new Feature("REFLECTION_PROBE", "_EnableReflectionProbe", childElements: new List<Element>() {

                    new SubFeature("REFLECTION_PROBE_DISABLED_WHITEBOOST", "_ReflectionProbeDisabledWhiteboost"),

                    new KeywordDropdown("_Probe_Calculation", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "Fast"),
                        new KeywordDropdown.Option("_PROBE_CALCULATION_PRECISE", "Precise", childElements: new List<Element>() {
                            new RangeProperty("_ReflectionProbeGrayscale"),
                            new RangeProperty("_ColoredMetalMultiplier"),
                            new FloatProperty("_WhiteOffset"),
                        }),
                    }),

                    new FloatProperty("_ReflectionProbeIntensity"),
                    new SubFeature("REFLECTION_PROBE_BOX_PROJECTION", "_ReflectionProbeBoxProjection", childElements: new List<Element>() {
                        new SubFeature("REFLECTION_PROBE_BOX_PROJECTION_OFFSET", "_EnableBoxProjectionOffset", childElements: new List<Element>() {
                            new VectorProperty("_ReflectionProbeBoxProjectionSizeOffset", 3),
                            new VectorProperty("_ReflectionProbeBoxProjectionPositionOffset", 3),
                        }),
                    }),

                    new SubFeature("REFLECTION_STATIC", "_ReflectionStatic"),
                    new SubFeature("MULTIPLY_REFLECTIONS", "_MultiplyReflections", displayFilter: properties => properties.HasAllOfKeywords("REFLECTION_TEXTURE")),
                }),

                new Feature("ENABLE_RIM_DIM", "_EnableRimDim",
                    enabledFilter: properties => properties.HasAnyOfKeywords("REFLECTION_PROBE", "REFLECTION_TEXTURE"),
                    tooltip: "Dims reflections (if any) based on view angle similar to fresnel or rimlight",
                    childElements: new List<Element>() {
                        new FloatProperty("_RimScale"),
                        new FloatProperty("_RimOffset"),
                        new FloatProperty("_RimCameraDistanceOffset"),
                        new FloatProperty("_RimCameraDistanceScale"),
                        new FloatProperty("_RimSmoothness"),
                        new FloatProperty("_RimDarkening"),
                        new SubFeature("INVERT_RIM_DIM", "_InvertRimDim")
                }),

                new Feature("PRIVATE_POINT_LIGHT", "_PrivatePointLight", childElements: new List<Element>() {
                    new SubFeature("INSTANCED_PRIVATE_POINT_LIGHT", "_InstancedPrivatePointLightColor"),
                    new ColorProperty("_PrivatePointLightColor", hdr: true),
                    new VectorProperty("_PrivatePointLightPosition", 3),
                    new SubFeature("POINT_LIGHT_IS_LOCAL", "_PointLightPositionLocal"),
                    new FloatProperty("_PrivatePointLightIntensity"),
                }),

                new Feature("LIGHTMAP", "_EnableLightmap",
                    description: "Used very rarely in a few older environments like Billie, Linkin Park",
                    documentationUrl: "https://beatgames.atlassian.net/wiki/spaces/BS/pages/3574857/Lightmap+Refl.+Probe+Baking+and+Fake+Reflections+-+Usage"),

            });
        }

        private static Element Other() {
            return new Category("Other Features", childElements: new List<Element>() {

                new SubFeature("NOISE_DITHERING", "_EnableNoiseDithering"),
                new SubFeature("LINEAR_TO_GAMMA", "_LinearToGamma"),
                new SubFeature("SPECULAR_ANTIFLICKER", "_SpecularAntiflicker", childElements: new List<Element> {
                    new RangeProperty("_AntiflickerStrength"),
                    new FloatProperty("_AntiflickerDistanceScale"),
                    new FloatProperty("_AntiflickerDistanceOffset"),
                }),

                new SubFeature("ROTATE_UV", "_EnableRotateUV", childElements: new List<Element>() {
                    new NoKeywordDropdown("_Rotate_UV", KeywordDropdown.Style.SubFeature, new List<NoKeywordDropdown.NoKeywordOption>() {
                        new NoKeywordDropdown.NoKeywordOption("90\u00b0 clockwise"),
                        new NoKeywordDropdown.NoKeywordOption("90\u00b0 counter-clockwise"),
                        new NoKeywordDropdown.NoKeywordOption("180\u00b0"),
                    })
                }),

                new KeywordDropdown("_Custom_Time", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "Standard"),
                    new KeywordDropdown.Option("_CUSTOM_TIME_SONG_TIME", "Song Time"),
                    new KeywordDropdown.Option("_CUSTOM_TIME_FREEZE", "Freeze"),
                }),

                new KeywordDropdown("_Aces_Approach", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "After Emissive", childElements: new List<Element>() {
                        new InfoBox("Any emission may appear unexpectedly toned down due to ACES after Emissive", MessageType.Warning,
                            displayFilter: properties => properties.HasTextureEmission() || properties.HasAnyOfKeywords(
                                "_VERTEXMODE_SPECIAL", "_VERTEXMODE_EMISSIVE_MULT_ADD", "_VERTEXMODE_EMISSION"))
                    }),
                    new KeywordDropdown.Option("_ACES_APPROACH_BEFORE_EMISSIVE", "Before Emissive"),
                }),

                new KeywordDropdown("_Curve_Vertices", KeywordDropdown.Style.SubFeature,
                    description: "curves in object space around pivot",
                    options: new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "None"),
                        new KeywordDropdown.Option("_CURVE_VERTICES_AROUND_X", "Around X"),
                        new KeywordDropdown.Option("_CURVE_VERTICES_AROUND_Y", "Around Y"),
                        new KeywordDropdown.Option("_CURVE_VERTICES_AROUND_Z", "Around Z"),
                }),

                new KeywordDropdown("Distortion", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("DISTORTION_SIMPLE", "Simple"),
                }, childElements: new List<Element>() {

                    new Group(enabledFilter: properties => properties.HasAnyOfKeywords("DISTORTION_SIMPLE"), childElements: new List<Element>() {
                        new KeywordDropdown("_Distortion_Target", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {

                            new KeywordDropdown.Option(string.Empty, "MPM",
                                displayFilter: properties => properties.HasAnyOfKeywords("METAL_SMOOTHNESS_TEXTURE")),

                            new KeywordDropdown.Option("_DISTORTION_TARGET_EMISSIONTEX", "Emission Tex",
                                displayFilter: properties => properties.HasTextureEmission() && properties.HasEmissionSourceTexture()),

                            new KeywordDropdown.Option("_DISTORTION_TARGET_EMISSION_MASK", "Emission Mask",
                                displayFilter: properties => properties.HasAnyOfKeywords("EMISSION_MASK") && properties.HasTextureEmission()),

                            new KeywordDropdown.Option("_DISTORTION_TARGET_SECONDARY_EMISSION_MASK", "Secondary Emission Mask",
                                displayFilter: properties => properties.HasAnyOfKeywords("SECONDARY_EMISSION_MASK") && properties.HasTextureEmission()),

                            new KeywordDropdown.Option("_DISTORTION_TARGET_PULSE", "Pulse",
                                displayFilter: properties => properties.HasAnyOfKeywords("_EMISSIONTEXTURE_PULSE")),

                            new KeywordDropdown.Option("_DISTORTION_TARGET_PARRALAX", "ParalLax",
                                displayFilter: properties => properties.HasAnyOfKeywords("_PARALLAX_FLEXIBLE", "_PARALLAX_RGB")),

                            new KeywordDropdown.Option("_DISTORTION_TARGET_DIFFUSE", "Diffuse",
                                displayFilter: properties => properties.HasAnyOfKeywords("DIFFUSE")),

                            new KeywordDropdown.Option("_DISTORTION_TARGET_NORMAL", "Normal",
                                displayFilter: properties => properties.HasAnyOfKeywords("NORMAL_MAP")),

                            new KeywordDropdown.Option("_DISTORTION_TARGET_OCCLUSION", "Occlusion",
                                displayFilter: properties => properties.HasAnyOfKeywords("OCCLUSION")),

                            new KeywordDropdown.Option("_DISTORTION_TARGET_OCCLUSION_DETAIL", "Occlusion Detail",
                                displayFilter: properties => properties.HasAnyOfKeywords("OCCLUSION_DETAIL")),
                        }),

                        new TextureProperty("_DistortionTex"),
                        new SubFeature("SECONDARY_UVS_DISTORTION", "_SecondaryUVsDistortion", displayFilter: properties => properties.HasSecondaryUVsEnabled()),
                        new FloatProperty("_DistortionStrength"),
                        new VectorProperty("_DistortionAxes", 2),
                        new VectorProperty("_DistortionPanning", 2),
                    }),
                }),

                new Feature("COLOR_ARRAY", "_UseColorArray",
                    description: "See the linked documentation",
                    documentationButtonLabel: "docs",
                    documentationUrl: "https://beatgames.atlassian.net/wiki/spaces/BS/pages/109346823/Environment+Optimization+Performance+Testing#Color-Light-Array",
                    childElements: new List<Element>() {
                        new InfoBox("Color Array overrides emission colors and is limited to 150 IDs"),
                        new InfoBox("Color Array needs Texture or Vertex Emission enabled to take effect",
                            displayFilter: properties => properties.HasNoneOfKeywords("_VERTEXMODE_SPECIAL",
                                "_VERTEXMODE_EMISSION", "_VERTEXMODE_EMISSIVE_MULT_ADD", "_EMISSIONTEXTURE_SIMPLE",
                                "_EMISSIONTEXTURE_FLIPBOOK", "_EMISSIONTEXTURE_PULSE")),
                }),

                new Feature("DISSOLVE", "_EnableDissolve", childElements: new List<Element>() {

                    new InfoBox("You probably want to set Culling to Off so the backside of materials show when partially dissolved", MessageType.Info,
                        displayFilter: properties => properties.FloatPropertyComparison("_Cull", DisplayFilter.ComparisonType.NotEqual, 0.0f)),
                    new KeywordDropdown("_DissolveAlpha", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "Clip"),
                        new KeywordDropdown.Option("_DISSOLVEALPHA_FADE", "Fade"),
                        new KeywordDropdown.Option("_DISSOLVEALPHA_BOTH", "Both"),
                    }),

                    new Group(displayFilter: properties => properties.HasAnyOfKeywords("_DISSOLVEALPHA_FADE", "_DISSOLVEALPHA_BOTH"), childElements: new List<Element>() {
                        new InfoBox("Non-Clip transparent dissolve can easily create weird situation with backfaces and self transparency"),
                        new FloatProperty("_AlphaMultiplier"),
                        new FloatProperty("_DissolveGradientWidth")
                    }),

                    new NoKeywordFeature("_InvertDissolve", Feature.Style.SubFeature, childElements: new List<Element>() {
                        new InfoBox("To change direction it is better to use negative Axis Vector instead of Dissolve invert")
                    }),

                    new KeywordDropdown("_Dissolve_Space", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "Local"),
                        new KeywordDropdown.Option("_DISSOLVE_SPACE_WORLD", "World"),
                        new KeywordDropdown.Option("_DISSOLVE_SPACE_WORLD_CENTERED", "World Centered", description: "World space but centered on object's pivot"),
                        new KeywordDropdown.Option("_DISSOLVE_SPACE_UV", "UV"),
                        new KeywordDropdown.Option("_DISSOLVE_SPACE_AVATAR", "Avatar", description: "Not updated, will produce errors"),
                    }),

                    new Group(title: "Avatar Related", displayFilter: properties => properties.HasAnyOfKeywords("_DISSOLVE_SPACE_AVATAR"), childElements: new List<Element>() {
                        new FloatProperty("_FadeStartY"),
                        new FloatProperty("_FadeEndY"),
                        new FloatProperty("_FadeZoneInterceptX"),
                        new FloatProperty("_FadeZoneSlope"),
                        new FloatProperty("_BodyFadeGamma"),
                    }),

                    new VectorProperty("_DissolveAxisVector", 3, displayFilter: properties => properties.HasNoneOfKeywords("_DISSOLVE_SPACE_AVATAR")),
                    new SubFeature("DISSOLVE_PROGRESS", "_UseDissolveProgress", displayFilter: properties => properties.HasNoneOfKeywords("_DISSOLVE_SPACE_AVATAR"),
                        childElements: new List<Element>() {
                            new FloatProperty("_DissolveStartValue"),
                            new FloatProperty("_DissolveEndValue"),
                            new RangeProperty("_DissolveProgress"),
                    }),
                    new FloatProperty("_DissolveOffset", displayFilter: properties => properties.HasNoneOfKeywords("DISSOLVE_PROGRESS")),

                    new Feature("DISSOLVE_COLOR", "_UseDissolveColor", childElements: new List<Element>() {
                        new ColorProperty("_DissolveColor"),
                        new FloatProperty("_DissolveColorIntensity"),
                        new FloatProperty("_CutColorFalloff"),
                        new FloatProperty("_CutColorBacksideFalloff"),
                    }),

                    new KeywordDropdown("_Dissolve_Grid", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "None"),
                        new KeywordDropdown.Option("_DISSOLVE_GRID_LOCAL", "Local"),
                        new KeywordDropdown.Option("_DISSOLVE_GRID_WORLD", "World", description: "World space but centered on object's pivot"),
                        new KeywordDropdown.Option("_DISSOLVE_GRID_UV", "Uv"),
                    }, childElements: new List<Element>() {
                        new Group(enabledFilter: properties => properties.HasAnyOfKeywords("_DISSOLVE_GRID_LOCAL",
                            "_DISSOLVE_GRID_WORLD", "_DISSOLVE_GRID_UV"), childElements: new List<Element>() {
                            new FloatProperty("_GridThickness"),
                            new FloatProperty("_GridSize"),
                            new FloatProperty("_GridFalloff"),
                            new FloatProperty("_GridSpeed"),
                        })
                    }),

                    new Feature("DISSOLVE_TEXTURE", "_UseDissolveTexture", childElements: new List<Element>() {
                        new TextureProperty("_DissolveTexture"),
                        new VectorProperty("_DissolveTextureSpeed", 2),
                        new FloatProperty("_DissolveTextureInfluence"),
                    }),
                }),

            });
        }

        private static Element OverarchingFeatures() {
            return new Category("Global Features", childElements: new List<Element>() {

                new VectorProperty("_InputUvMultiplier", 2),

                new SubFeature("AVATAR_COMPUTE_SKINNING", "_AvatarComputeSkinning", childElements: new List<Element> {
                    new InfoBox(message: "Avatar Features weren't updated in a while and will produce errors", MessageType.Warning,
                        displayFilter: properties => properties.HasAllOfKeywords("AVATAR_COMPUTE_SKINNING"))
                }),

                new KeywordDropdown("_Secondary_UVs", KeywordDropdown.Style.SubFeature, options: new List<KeywordDropdown.Option> {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("_SECONDARY_UVS_IMPORT", "Import"),
                    new KeywordDropdown.Option("_SECONDARY_UVS_EXTERNAL_SCALE", "External Scale",
                        description: "Based on 3D Scale, this component translates it to UV scales for each face of a cube (or other objects)" +
                                     " Useful when a script changes scale of the object and we want the UVs to scale too instead of stretching."),
                    new KeywordDropdown.Option("_SECONDARY_UVS_OBJECT_SPACE", "Object Space",
                        description: "Avoid nested scale and use with Material Property Block Local Scale Animator for best results"),
                    new KeywordDropdown.Option("_SECONDARY_UVS_ADDITIVE_OFFSET", "Additive Offset",
                        description: "Adds float2 _AdditiveUVOffset to the standard uvs without overriding them. Useful to set random texture offsets")
                }),

                new VectorProperty("_UVScale", 3, displayFilter: properties => properties.HasAnyOfKeywords("_SECONDARY_UVS_EXTERNAL_SCALE", "_SECONDARY_UVS_OBJECT_SPACE")),
                new VectorProperty("_AdditiveUVOffset", 2, displayFilter: properties => properties.HasAnyOfKeywords("_SECONDARY_UVS_ADDITIVE_OFFSET")),
            });
        }

        private static Element BaseProperties() {

            return new Category("Base Properties", childElements: new List<Element> {

                new ColorProperty("_Color", displayName: "Base Color"),
                new InfoBox(
                    message: "Metalness and/or Smoothness are affected by the MPM in Overarching Features",
                    displayFilter: properties =>
                        properties.HasAllOfKeywords("METAL_SMOOTHNESS_TEXTURE") &&
                        properties.HasAnyOfKeywords(
                            "_METALLIC_TEXTURE_MPM_R",
                            "_METALLIC_TEXTURE_MPM_A",
                            "_METALLIC_TEXTURE_MPM_AVATAR_B",
                            "_SMOOTHNESS_TEXTURE_MPM_A",
                            "_SMOOTHNESS_TEXTURE_MPM_G_ROUGHNESS")
                ),
                new RangeProperty("_Metallic"),
                new RangeProperty("_Smoothness"),

                LightingPreset(),
                BloomPreset(),

                new Space(8.0f),
                new Group("Separate Bloom options", hasFoldout: true, childElements: new List<Element>() {
                    BloomSettingsGroup(),
                }),

                new Group("Ambient", hasFoldout: true, childElements: new List<Element>() {
                    new ColorProperty("_NominalDiffuseLevel"),
                    new RangeProperty("_AmbientMinimalValue", tooltip: "0 behaves normally, low values represent guaranteed minimal ambient, 1 results in 'unlit' behavior"),
                    new FloatProperty("_AmbientMultiplier")
                }),

                new Group("Normals", hasFoldout: true, childElements: new List<Element>() {

                    new SubFeature("USE_SPHERICAL_NORMAL_OFFSET", "_UseSphericalNormalOffset", childElements: new List<Element>() {
                        new FloatProperty("_SphericalNormalOffsetIntensity"),
                        new VectorProperty("_SphericalNormalOffsetCenter", 3)
                    }),

                    new SubFeature("PRECISE_NORMAL", "_PreciseNormal",
                        tooltip: "By default some features like Rim Light can use cheaper normals. This forces per-pixel precision."),

                    new Feature("NORMAL_MAP", "_EnableNormalMap", childElements: new List<Element>() {
                        new TextureProperty("_NormalTexture"),
                        new SubFeature("SECONDARY_UVS_NORMAL", "_SecondaryUVsNormal", displayFilter: properties => properties.HasSecondaryUVsEnabled()),
                        new FloatProperty("_NormalScale")
                    }),
                }),

                new Feature("METAL_SMOOTHNESS_TEXTURE", "_EnableMetalSmoothnessTex",
                    displayName: "Multi Purpose Map (MPM)",
                    tooltip: "Multi Purpose Map uses multiple channels to store data used by various features",
                    description: "Multi Purpose Map uses multiple channels to store data used by various features",
                    childElements: new List<Element> {
                        new TextureProperty("_MetalSmoothnessTex"),
                        new SubFeature("SECONDARY_UVS_MPM", "_SecondaryUVsMPM", displayFilter: properties => properties.HasSecondaryUVsEnabled()),
                        new KeywordDropdown("_Metallic_Texture", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option> {
                            new KeywordDropdown.Option(string.Empty, "None"),
                            new KeywordDropdown.Option("_METALLIC_TEXTURE_MPM_R", "MPM Red"),
                            new KeywordDropdown.Option("_METALLIC_TEXTURE_MPM_A", "MPM Alpha"),
                            new KeywordDropdown.Option("_METALLIC_TEXTURE_MPM_AVATAR_B", "MPM Blue (Avatar)")
                        }),
                        new KeywordDropdown("_Smoothness_Texture", style: KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option> {
                            new KeywordDropdown.Option(string.Empty, "None"),
                            new KeywordDropdown.Option("_SMOOTHNESS_TEXTURE_MPM_A", "MPM A"),
                            new KeywordDropdown.Option("_SMOOTHNESS_TEXTURE_MPM_G_ROUGHNESS", "MPM G Roughness")
                        }),

                        new SubFeature("MPM_CUSTOM_MIP", "_EnableCustomMPMMip", childElements: new List<Element> {
                            new RangeProperty("_MpmMipBias")
                        }),
                }),

                new Feature("DIFFUSE_TEXTURE", "_EnableDiffuseTexture", displayName: "Base Texture", childElements: new List<Element>() {
                    new KeywordDropdown("_Diffuse_Texture_Source", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {

                        new KeywordDropdown.Option(string.Empty, "Texture"),

                        new KeywordDropdown.Option("_DIFFUSE_TEXTURE_SOURCE_MPM_R", "MPM Red",
                            displayFilter: properties => properties.HasAnyOfKeywords("METAL_SMOOTHNESS_TEXTURE"),
                            displayFilterErrorMessage: "Diffuse Texture Source > MPM Red option requires MPM"),

                        new KeywordDropdown.Option("_DIFFUSE_TEXTURE_SOURCE_MPM_A_SMOOTHNESS", "MPM Alpha Smoothness",
                            displayFilter: properties => properties.HasAnyOfKeywords("METAL_SMOOTHNESS_TEXTURE"),
                            displayFilterErrorMessage: "Diffuse Texture Source > MPM Alpha Smoothness option requires MPM",
                            description: "This uses inverted smoothness value as a grayscale texture"),
                    }),
                    new Group(displayFilter: properties => properties.FloatPropertyComparison("_Diffuse_Texture_Source", DisplayFilter.ComparisonType.Equal, 0), childElements: new List<Element>() {
                        new TextureProperty("_DiffuseTexture"),
                        new SubFeature("SECONDARY_UVS_DIFFUSE", "_SecondaryUVsDiffuse", displayFilter: properties => properties.HasSecondaryUVsEnabled()),
                        new FloatProperty("_AlbedoMultiplier")
                    }),
                }),

                new KeywordDropdown(
                    propertyName: "_VertexMode",
                    style: KeywordDropdown.Style.Feature,
                    options: new List<KeywordDropdown.Option> {
                        new KeywordDropdown.Option(string.Empty, "None"),

                        new KeywordDropdown.Option("_VERTEXMODE_COLOR", "Color", childElements: new List<Element> {
                            new InfoBox("Multiplies Base Color by RGB channels of vertex colors")
                        }),

                        new KeywordDropdown.Option("_VERTEXMODE_EMISSION", "Emission", childElements: new List<Element> {
                            new InfoBox("Green channel is used for coverage, \nAlpha channel for Bloom and Whiteboost")
                        }),

                        new KeywordDropdown.Option("_VERTEXMODE_METALSMOOTHNESS", "MetalSmoothness", childElements: new List<Element> {
                            new InfoBox("Metallic is multiplied by Red channel, \nSmoothness is multiplied by Alpha channel")
                        }),

                        new KeywordDropdown.Option("_VERTEXMODE_SPECIAL", "Special", childElements: new List<Element> {
                            new InfoBox("Metallic is multiplied by Red channel, \nSmoothness is multiplied by Alpha channel, \nGreen is used for Emission coverage")
                        }),

                        new KeywordDropdown.Option("_VERTEXMODE_DISPLACEMENT", "Displacement", childElements: new List<Element> {
                            new InfoBox("Vertices displaced along vertex normals using blue channel value",
                                displayFilter: properties => properties.HasNoneOfKeywords("DISPLACEMENT_SPATIAL")),
                            new InfoBox("Vertices displaced in positive direction along objectspace axes using RGB values ",
                                displayFilter: properties => properties.HasAllOfKeywords("DISPLACEMENT_SPATIAL") && properties.HasNoneOfKeywords("DISPLACEMENT_BIDIRECTIONAL")),
                            new InfoBox("Vertices displaced bidirectionally along objectspace axes using zero-centered RGB values - 50 percent gray vertex will not be displaced",
                                displayFilter: properties => properties.HasAllOfKeywords("DISPLACEMENT_SPATIAL","DISPLACEMENT_BIDIRECTIONAL")),
                            new InfoBox("Flat Spectrogram relies on a Spectrogram Row component",
                                displayFilter: properties => properties.HasAllOfKeywords("_SPECTROGRAM_FLAT")),
                            new InfoBox("Full Spectrogram uses uv3 coords to map 64 spectrogram channels",
                            displayFilter: properties => properties.HasAllOfKeywords("_SPECTROGRAM_FULL"))
                        }),

                        new KeywordDropdown.Option("_VERTEXMODE_EMISSIVE_MULT_ADD", "Mult Add", childElements: new List<Element>() {
                            new InfoBox("Green for emission _ Alpha for texture emission fading")
                        })


                    }, childElements: new List<Element>() {
                        new Group(
                            indentationCount: 1,
                            displayFilter: properties => properties.HasAnyOfKeywords("_VERTEXMODE_EMISSION", "_VERTEXMODE_SPECIAL"),
                            childElements: new List<Element> {
                                new RangeProperty("_EmissionThreshold"),
                                new ColorProperty("_EmissionColor"),
                                new FloatProperty("_EmissionStrength"),
                                new FloatProperty("_EmissionBloomIntensity"),
                                VertexModeBloom(),
                                new FloatProperty(propertyName: "_QuestWhiteboostMultiplier")
                            }),

                        new Group(
                            indentationCount: 1,
                            displayFilter: properties => properties.HasAnyOfKeywords("_VERTEXMODE_DISPLACEMENT"),
                            childElements: new List<Element> {
                                new SubFeature("DISPLACEMENT_SPATIAL", "_DisplacementSpatial"),
                                new SubFeature("DISPLACEMENT_BIDIRECTIONAL", "_DisplacementBidirectional",
                                    displayFilter: properties => properties.HasAllOfKeywords("DISPLACEMENT_SPATIAL")
                                ),

                                new KeywordDropdown("_Spectrogram", KeywordDropdown.Style.SubFeature, options: new List<KeywordDropdown.Option> {
                                    new KeywordDropdown.Option(string.Empty, "None"),
                                    new KeywordDropdown.Option("_SPECTROGRAM_FLAT", "Flat"),
                                    new KeywordDropdown.Option("_SPECTROGRAM_FULL", "Full")
                                }),
                                new FloatProperty("_DisplacementStrength"),
                                new VectorProperty("_DisplacementAxisMultiplier"),
                                new VectorProperty("_VertexDisplacementMaskMixer"),
                            }
                        ),
                    }),

                new Feature( "VERTEXDISPLACEMENT_MASK", "_EnableVertexDisplacementMask",
                    displayFilter: properties => properties.HasAnyOfKeywords("_VERTEXMODE_DISPLACEMENT"),
                    childElements: new List<Element>() {
                        new KeywordDropdown(
                            "_VertexDisplacement_Mask_Source",
                            KeywordDropdown.Style.SubFeature,
                            options: new List<KeywordDropdown.Option> {
                                new KeywordDropdown.Option(string.Empty, "Texture"),
                                new KeywordDropdown.Option("_VERTEXDISPLACEMENT_MASK_SOURCE_EMISSION_TEXTURE", "Emission Texture",
                                    childElements: new List<Element> {
                                        new InfoBox("As the emission texture needs to be sampled again at vertex stage, for convenience and more flexibility please use the tiling and offset input below.")
                                    }
                                ),
                                new KeywordDropdown.Option("_VERTEXDISPLACEMENT_MASK_SOURCE_EMISSION_MASK", "Emission Mask",
                                    childElements: new List<Element> {
                                        new InfoBox("The emission texture needs to be sampled again at vertex stage, for convenience and more flexibility please use the tiling and offset input below.")
                                    }
                                ),
                                new KeywordDropdown.Option("_VERTEXDISPLACEMENT_MASK_SOURCE_SECONDARY_EMISSION_MASK", "Secondary Emission Mask",
                                    childElements: new List<Element> {
                                        new InfoBox("The emission texture needs to be sampled again at vertex stage, for convenience and more flexibility please use the tiling and offset input below.")
                                    }
                                )
                            }
                        ),

                    new TextureProperty("_VertexDisplacementMask"),
                    new VectorProperty("_VertexDisplacementMaskSpeed", 2),
                    new KeywordDropdown("_VertexDisplacementMaskMode", KeywordDropdown.Style.SubFeature, options: new List<KeywordDropdown.Option> {
                        new KeywordDropdown.Option(string.Empty, "Multiply", description: "Mask texture RGB values fetched at vertex are scaled and offset with below values, then multiplied individually per each axis with existing displacement (R*X, G*Y, B*Z)"),
                        new KeywordDropdown.Option("_VERTEXDISPLACEMENT_MASK_ADDITIVE", "Add (along direction)", description: "Mask texture R value fetched at vertex is scaled and offset with below values, then added along existing displacement direction"),
                        new KeywordDropdown.Option("_VERTEXDISPLACEMENT_MASK_ADDITIVERGB", "Add RGB", description: "Mask texture RGB values fetched at vertex are mapped to [-1,1] range, then scaled and offset with below values, finally added to existing displacement along respective axes")
                        }),
                    new FloatProperty("_VertexDisplacementMaskMultiplier"),
                    new FloatProperty("_VertexDisplacementMaskOffset")
                })
                }
            );
        }

        private static Element Emissions() {

            return new Category("Emission", childElements: new List<Element>() {

                new KeywordDropdown("_EmissionTexture", KeywordDropdown.Style.Feature, options: new List<KeywordDropdown.Option> {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("_EMISSIONTEXTURE_SIMPLE", "Simple"),
                    new KeywordDropdown.Option("_EMISSIONTEXTURE_PULSE", "Pulse",
                        childElements: new List<Element> {
                            new InfoBox(message: "WARNING Pulse feature is not fully functional and may not work as expected")
                        }),
                    new KeywordDropdown.Option("_EMISSIONTEXTURE_FLIPBOOK", "Flipbook",
                        documentationUrl: "https://beatgames.atlassian.net/wiki/spaces/BS/pages/3573904/Flipbook+Tools")
                    }, childElements: new List<Element> {

                        new Group(enabledFilter: properties => properties.HasTextureEmission(), childElements: new List<Element> {
                            new KeywordDropdown("_Emission_Texture_Source", KeywordDropdown.Style.SubFeature, options: new List<KeywordDropdown.Option> {
                                    new KeywordDropdown.Option(string.Empty, "Texture"),

                                    new KeywordDropdown.Option("_EMISSION_TEXTURE_SOURCE_FILL", "Fill",
                                        displayFilter: properties => properties.HasNoneOfKeywords("_EMISSIONTEXTURE_FLIPBOOK, _EMISSIONTEXTURE_PULSE"),
                                        displayFilterErrorMessage: "Emission Texture Source > Fill option only supports Simple Emission"),

                                    new KeywordDropdown.Option("_EMISSION_TEXTURE_SOURCE_MPM_G", "MPM G",
                                        displayFilter: properties => properties.HasAnyOfKeywords("METAL_SMOOTHNESS_TEXTURE") &&
                                                                     properties.HasNoneOfKeywords("_EMISSIONTEXTURE_FLIPBOOK"),
                                        displayFilterErrorMessage: "Emission Texture Source > MPP G option requires MPM and doesn't support Flipbooks"
                                    )
                                }
                            ),

                            new TextureProperty("_EmissionTex", displayFilter: properties => properties.HasAnyOfKeywords("_EMISSIONTEXTURE_SIMPLE", "_EMISSIONTEXTURE_FLIPBOOK")
                                                             && properties.HasEmissionSourceTexture()),
                            new SubFeature("SECONDARY_UVS_EMISSION", "_SecondaryUVsEmissionTex", displayFilter: properties => properties.HasSecondaryUVsEnabled()),
                            new VectorProperty("_EmissionTexSpeed", 2, displayFilter: properties =>
                                    properties.HasAllOfKeywords("_EMISSIONTEXTURE_SIMPLE") && properties.HasEmissionSourceTexture()),

                            new Space(12.0f),
                            new KeywordDropdown("_Emission_Alpha_Source", KeywordDropdown.Style.SubFeature,
                                displayFilter: properties => properties.HasAnyOfKeywords("_EMISSIONTEXTURE_SIMPLE"),
                                options: new List<KeywordDropdown.Option> {
                                    new KeywordDropdown.Option(string.Empty, "Emission G",
                                        displayFilter: properties => properties.HasEmissionSourceTexture(),
                                        displayFilterErrorMessage: "Emission Alpha Source > Emission G option requires Emission source to be a Texture"
                                    ),
                                    new KeywordDropdown.Option("_EMISSION_ALPHA_SOURCE_COPY_EMISSION", "Copy Emission"),
                                    new KeywordDropdown.Option("_EMISSION_ALPHA_SOURCE_MPM_R", "MPM R",
                                        displayFilter: properties => properties.HasAnyOfKeywords("METAL_SMOOTHNESS_TEXTURE"),
                                        displayFilterErrorMessage: "Emission Alpha Source > Copy Emission option requires MPM"
                                    )
                                }
                            ),

                            new FloatProperty("_EmissionBrightness"),

                            new SubFeature("ENABLE_EMISSION_ANGLE_DISAPPEAR", "_AngleDisappear", childElements: new List<Element> {
                                    new FloatProperty("_ThresholdAngle")
                            }),

                            new Space(12.0f),
                            EmissionColorType(),

                            new TextureProperty("_EmissionGradientTex",
                                displayFilter: properties => properties.HasAllOfKeywords("_EMISSIONCOLORTYPE_GRADIENT")),
                            new ColorProperty("_EmissionTexColor",
                                displayFilter: properties => properties.HasNoneOfKeywords("_EMISSIONCOLORTYPE_GRADIENT")),

                            new Group(
                                indentationCount: 1,
                                displayFilter: properties => properties.HasAnyOfKeywords(
                                "_EMISSIONCOLORTYPE_WHITEBOOST",
                                "_EMISSIONCOLORTYPE_MAINEFFECT"
                                ), childElements: new List<Element>() {
                                    new FloatProperty("_EmissionTexBloomIntensity"),
                                    new FloatProperty("_EmissionTexWhiteboostMultiplier")
                            }),

                            new Group(
                                indentationCount: 1,
                                displayFilter: properties => properties.HasAnyOfKeywords("_EMISSIONTEXTURE_PULSE"),
                                childElements: new List<Element>() {
                                    new TextureProperty("_PulseMask"),
                                    new SubFeature("SECONDARY_UVS_PULSE", "_SecondaryUVsPulseTex", displayFilter:
                                        properties => properties.HasAnyOfKeywords("_SECONDARY_UVS_EXTERNAL_SCALE", "_SECONDARY_UVS_IMPORT", "_SECONDARY_UVS_OBJECT_SPACE")),
                                    new SubFeature("INVERT_PULSE", "_InvertPulseTexture"),
                                    new SubFeature("PULSE_MULTIPLY_TEXTURE", "_PulseMultiplyByTexture"),
                                    new FloatProperty("_PulseWidth"),
                                    new FloatProperty("_PulseSpeed"),
                                    new RangeProperty("_PulseSmooth")
                                }),

                            new Group(
                                indentationCount: 1,
                                displayFilter: properties => properties.HasAnyOfKeywords("_EMISSIONTEXTURE_FLIPBOOK"),
                                childElements: new List<Element>() {
                                    new InfoBox("Keep the texture sRGB or alpha will have different intensity"),
                                    new InfoBox("Frame 1 in RGBA texture contains grayscale frames 1234 \nFrame 2 in RGBA texture contains grayscale frames 4567",
                                        displayFilter: properties => properties.HasNoneOfKeywords("FLIPBOOK_BLENDING_OFF")),
                                    new InfoBox("Frame 1 in RGBA texture contains grayscale frames 1234 \nFrame 2 in RGBA texture contains grayscale frames 5678",
                                        displayFilter: properties => properties.HasAllOfKeywords("FLIPBOOK_BLENDING_OFF")),
                                    new FloatProperty("_FlipbookColumns", uiToMaterialDelegate: InOutValueModification.Round),
                                    new FloatProperty("_FlipbookRows", uiToMaterialDelegate: InOutValueModification.Round),
                                    new FloatProperty("_FlipbookNonloopableFrames", uiToMaterialDelegate: InOutValueModification.Round),
                                    new FloatProperty("_FlipbookSpeed"),
                                    new SubFeature("FLIPBOOK_BLENDING_OFF", "_FlipbookBlendingOff"),

                                })
                            })
                    }
                ),

                new Group(
                    indentationCount: 0,
                    displayFilter: properties => properties.HasNoneOfKeywords("_EMISSIONTEXTURE_FLIPBOOK"),
                    childElements: new List<Element>() {

                        new Feature("EMISSION_MASK", "_EnableEmissionMask",
                            displayFilter: properties => properties.HasAnyOfKeywords("_EMISSIONTEXTURE_SIMPLE",
                                "_EMISSIONTEXTURE_PULSE", "_EMISSIONTEXTURE_FLIPBOOK"),
                            childElements: new List<Element>() {
                                new InfoBox("Green channel is used for whiteboost and alpha bloom purposes"),
                                new KeywordDropdown("_MaskBlend", style: KeywordDropdown.Style.SubFeature, options: new List<KeywordDropdown.Option>() {
                                        new KeywordDropdown.Option(string.Empty, "Multiply"),
                                        new KeywordDropdown.Option("_MASKBLEND_ADD", "Add"),
                                        new KeywordDropdown.Option("_MASKBLEND_MASKED_ADD", "Masked Add"),
                                    }),
                                new TextureProperty("_EmissionMask"),
                                new SubFeature("SECONDARY_UVS_EMISSION_MASK", "_SecondaryUVsMask", displayFilter: properties => properties.HasSecondaryUVsEnabled()),
                                new VectorProperty("_EmissionMaskSpeed", 2),
                                new FloatProperty("_EmissionMaskIntensity")
                        }),

                        new Feature("SECONDARY_EMISSION_MASK", "_EnableSecondaryEmissionMask",
                            displayFilter: properties => properties.HasAnyOfKeywords("_EMISSIONTEXTURE_SIMPLE",
                                "_EMISSIONTEXTURE_PULSE", "_EMISSIONTEXTURE_FLIPBOOK"),
                            childElements: new List<Element>() {
                                new KeywordDropdown("_Secondary_Mask_Blend", style: KeywordDropdown.Style.SubFeature, options: new List<KeywordDropdown.Option>() {
                                    new KeywordDropdown.Option(string.Empty, "Multiply"),
                                    new KeywordDropdown.Option("_SECONDARY_MASK_BLEND_ADD", "Add"),
                                    new KeywordDropdown.Option("_SECONDARY_MASK_BLEND_MASKED_ADD", "Masked Add"),
                                }),
                                new TextureProperty("_SecondaryEmissionMask"),
                                new SubFeature("SECONDARY_UVS_EMISSION_MASK2", "_SecondaryUVsMask2", displayFilter: properties => properties.HasSecondaryUVsEnabled()),
                                new VectorProperty("_SecondaryEmissionMaskSpeed", 2),
                                new FloatProperty("_SecondaryEmissionMaskIntensity")
                            }),

                        new KeywordDropdown("_Emission_Step",
                            style: KeywordDropdown.Style.Feature,
                            displayFilter: properties => properties.HasEmissionSourceTexture()
                            && properties.HasAnyOfKeywords("_EMISSIONTEXTURE_SIMPLE", "_EMISSIONTEXTURE_PULSE", "_EMISSIONTEXTURE_FLIPBOOK"),

                            options: new List<KeywordDropdown.Option>() {
                            new KeywordDropdown.Option(string.Empty, "None"),
                            new KeywordDropdown.Option("_EMISSION_STEP_MASK", "Mask",
                                displayFilter: properties => properties.HasAnyOfKeywords("EMISSION_MASK"),
                                displayFilterErrorMessage: "Emission Step > Mask option requires Emission Mask to be on"),
                            new KeywordDropdown.Option("_EMISSION_STEP_SECONDARY_MASK", "Secondary Mask",
                                displayFilter: properties => properties.HasAnyOfKeywords("SECONDARY_EMISSION_MASK"),
                                displayFilterErrorMessage: "Emission Step > Secondary Mask option requires Secondary Emission Mask to be on"),
                            new KeywordDropdown.Option("_EMISSION_STEP_EMISSION_TEXTURE", "Emission Texture",
                                displayFilter: properties => properties.HasEmissionSourceTexture(),
                                displayFilterErrorMessage: "Emission Step > Emission Texture option requires Emission source to be a Texture"),
                            }, childElements: new List<Element>() {
                                new Group(
                                    enabledFilter: properties => properties.HasAnyOfKeywords(
                                        "_EMISSION_STEP_MASK", "_EMISSION_STEP_SECONDARY_MASK", "_EMISSION_STEP_EMISSION_TEXTURE"),
                                    childElements: new List<Element>() {
                                        new RangeProperty("_EmissionMaskStepValue"),
                                        new RangeProperty("_EmissionMaskStepWidth")
                                    }),
                            }),

                        // RIM LIGHT SECTION
                        new KeywordDropdown("_RimLight", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                            new KeywordDropdown.Option(string.Empty, "None"),
                            new KeywordDropdown.Option("_RIMLIGHT_LERP", "Lerp"),
                            new KeywordDropdown.Option("_RIMLIGHT_ADDITIVE", "Additive"),
                        }, childElements: new List<Element>() {
                            new Group(enabledFilter: properties => properties.HasAnyOfKeywords("_RIMLIGHT_LERP", "_RIMLIGHT_ADDITIVE"), childElements: new List<Element>() {
                                new SubFeature("RIMLIGHT_INVERT", "_InvertRimlight"),
                                new SubFeature("DIRECTIONAL_RIM", "_EnableDirectionalRim"),
                                new VectorProperty("_RimPerpendicularAxis", 3, uiToMaterialDelegate: InOutValueModification.Normalize,
                                    displayFilter: properties => properties.HasAnyOfKeywords("DIRECTIONAL_RIM")),
                                new FloatProperty("_RimLightEdgeStart"),
                                new ColorProperty("_RimLightColor"),
                                new FloatProperty("_RimLightIntensity"),
                                new FloatProperty("_RimLightBloomIntensity"),
                                RimLightBloom(),
                                new FloatProperty("_RimLightWhiteboostMultiplier", displayFilter: properties => properties.HasAnyOfKeywords())
                            })

                        }),

                        // PARALLAX SECTION
                        new KeywordDropdown("_Parallax", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                            new KeywordDropdown.Option(string.Empty, "None"),
                            new KeywordDropdown.Option("_PARALLAX_FLEXIBLE", "Flexible"),
                            new KeywordDropdown.Option("_PARALLAX_RGB", "RGB"),
                        }, childElements: new List<Element>() {

                           new Group(enabledFilter: properties => properties.HasAnyOfKeywords("_PARALLAX_FLEXIBLE", "_PARALLAX_RGB"),
                            childElements: new List<Element>() {
                                new SubFeature("_PARALLAX_FLEXIBLE_REFLECTED", "_EnableReflectedDir"),
                                new KeywordDropdown("_Parallax_Projection", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                                    new KeywordDropdown.Option(string.Empty, "Planar"),
                                    new KeywordDropdown.Option("_PARALLAX_PROJECTION_WARPED", "Warped"),
                                }),
                                new ColorProperty("_ParallaxColor"),
                                new TextureProperty("_ParallaxMap"),
                                new SubFeature("SECONDARY_UVS_PARALLAX", "_SecondaryUVsParallax", displayFilter: properties => properties.HasSecondaryUVsEnabled()),
                                new VectorProperty("_ParallaxTexSpeed", 2),
                                new FloatProperty("_ParallaxIntensity"),
                                new FloatProperty("_ParallaxIntensity_Step"),
                                new RangeProperty("_Layers", uiToMaterialDelegate: InOutValueModification.Round),
                                new FloatProperty("_StartOffset"),
                                new FloatProperty("_OffsetStep"),

                                new Feature("PARALLAX_IRIDESCENCE", "_Parallax_Iridescence", childElements: new List<Element>() {
                                    new VectorProperty("_IridescenceAxesMultiplier", 3),
                                    new FloatProperty("_IridescenceTiling"),
                                    new RangeProperty("_IridescenceColorInfluence")
                                }),

                                new KeywordDropdown("_Parallax_Masking", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                                    new KeywordDropdown.Option(string.Empty, "None"),
                                    new KeywordDropdown.Option("_PARALLAX_MASKING_TEXTURE", "Texture"),
                                    new KeywordDropdown.Option("_PARALLAX_MASKING_VERTEX_COLOR", "Vertex Color",
                                        description: "Masked by Green channel of vertex color"),
                                }),
                                new InfoBox("Do not mask off significant areas to limit performance costs of Parallax",
                                    MessageType.Warning, displayFilter: properties => properties.HasAnyOfKeywords(
                                        "_PARALLAX_MASKING_TEXTURE", "_PARALLAX_MASKING_VERTEX_COLOR")),
                                new Group(displayFilter: properties => properties.HasAnyOfKeywords("_PARALLAX_MASKING_TEXTURE"), childElements: new List<Element>() {
                                    new TextureProperty("_ParallaxMaskingMap"),
                                    new VectorProperty("_ParallaxMaskSpeed", 2),
                                    new RangeProperty("_ParallaxMaskIntensity")
                                }),


                            })
                        }),


                    })
                }
            );
        }

    }
}
