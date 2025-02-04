namespace BGLib.ShaderInspector {

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEngine;

    [UsedImplicitly]
    public class CustomParticlesShaderInspector : ShaderInspector {

        protected override IReadOnlyList<Element> GetRootElements() {

            return new List<Element>() {
                BaseProperties(),
                GlobalFeatures(),
                TextureFeatures(),
                GeometryFeatures(),
                UVControl(),
                ColorControl(),
                AlphaControl(),
                Fog(),
                SpecialContexts(),
                Settings(),
            };
        }


        // Functions for repeated UI
        private static Element SecondaryUVSelection() {

            return new KeywordDropdown("_Secondary_UVs", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                new KeywordDropdown.Option(string.Empty, "None"),
                new KeywordDropdown.Option("_SECONDARY_UVS_IMPORT", "Import"),
                new KeywordDropdown.Option("_SECONDARY_UVS_EXTERNAL_SCALE", "External Scale"),
                new KeywordDropdown.Option("_SECONDARY_UVS_TRAILS", "Trails"),
                new KeywordDropdown.Option("_SECONDARY_UVS_OBJECT_SPACE", "Object Space", childElements: new List<Element>() {
                    new InfoBox("Avoid nested scale and use with Material Property Block Local Scale Animator for best results", MessageType.Warning)
                }),
            }, childElements: new List<Element>() {
                new VectorProperty("_UVScale", 3, displayFilter: properties => properties.HasAnyOfKeywords("_SECONDARY_UVS_EXTERNAL_SCALE", "_SECONDARY_UVS_OBJECT_SPACE")),
                new VectorProperty("_UVManualOffset", 2, displayFilter: properties => properties.HasAnyOfKeywords("_SECONDARY_UVS_EXTERNAL_SCALE", "_SECONDARY_UVS_OBJECT_SPACE"),
                    description: "Use this offset manually updated via C# instead of texture panning if scale changes during gameplay to avoid artifacts")
            },
                tooltip: "Import: Simply use to access uv2 of the mesh \n\n" +
                        "External Scale: Used usually with component to set '_UVScale' when dynamically changing mesh scale at runtime to access both 0-1 and 0-X tiling \n\n" +
                        "Trails: Use to access distance-based uvs generated for trails \n\n" +
                        "Object Space: Use to access object-space XY position multiplied by '_UVScale' property \n");
        }

        private static Element ParticleVertexStream() {

            return new SubFeature("PARTICLE_VERTEX_STREAM", "_UsesParticleVertexStream",
                tooltip: "Useful for noise based particles, Requires Stable Random Stream added",
                description: "Useful for noise based particles, Requires Stable Random Stream added",
                documentationUrl: "https://beatgames.atlassian.net/wiki/spaces/BS/pages/261324806/VFX+Particle+UV+Randomization"
            );
        }

        private static Element WorldspacePanning() {

            return new SubFeature("WORLDSPACE_PANNING", "_WorldspacePanning",
                tooltip: "Adds worldspace movement as UV offset based on tangents. Designed for cubes and may behave weird on complex geometry",
                childElements: new List<Element>() {
                    new FloatProperty("_WorldspacePanningSpeed")
            });
        }

        // Categories

        private Element GlobalFeatures() {

            return new Category("Global Features", childElements: new List<Element>() {
                SecondaryUVSelection(),
                ParticleVertexStream(),
                WorldspacePanning(),
            }, description: "Collection of features that enable plenty of smaller sub features throughout the shader.");
        }

        private static Element Settings() {

            return new Category("Settings", childElements: new List<Element>() {

                new PresetDropdown("_BlendingPreset", "Blending Presets", options: new List<PresetDropdown.Option>() {
                        new PresetDropdown.Option(
                            displayName: "Custom",
                            description: "Manually set color and bloom blending in the section below"
                        ),
                        new PresetDropdown.Option(
                            displayName: "Default Additive",
                            presetValues: new Collection<PresetDropdown.PresetValue>() {
                                new PresetDropdown.FloatPropertyPreset( "_BlendSrcFactor", 1.0f), // One
                                new PresetDropdown.FloatPropertyPreset( "_BlendDstFactor", 1.0f), // One
                                new PresetDropdown.FloatPropertyPreset( "_BlendOp", 0.0f), // Add
                                new PresetDropdown.FloatPropertyPreset( "_BlendDstFactorA", 1.0f), // One
                            }),
                        new PresetDropdown.Option(
                            displayName: "Alpha Blended",
                            description: "It's recommended to avoid Bloom when using this option",
                            presetValues: new Collection<PresetDropdown.PresetValue>() {
                                new PresetDropdown.FloatPropertyPreset( "_BlendSrcFactor", 1.0f), // One
                                new PresetDropdown.FloatPropertyPreset( "_BlendDstFactor", 11.0f), // OneMinusSrcAlpha
                                new PresetDropdown.FloatPropertyPreset( "_BlendOp", 0.0f), // Add
                                new PresetDropdown.FloatPropertyPreset( "_BlendDstFactorA", 11.0f), // OneMinusSrcAlpha
                        }),
                }),
                new InfoBox("Alpha Blending doesn't work well with Bloom", MessageType.Warning,
                    displayFilter: properties => properties.FloatPropertyComparison("_BlendingPreset", DisplayFilter.ComparisonType.Equal, 2.0f)),

                new EnumPropertyDropdown<UnityEngine.Rendering.CullMode>("_CullMode", displayName: "Triangle Face Culling"),
                new NoKeywordFeature("_CustomZWrite", Feature.Style.SubFeature),

                new Space(12.0f),
                new Group("Color Blend Factors", hasFoldout: true,
                    enabledFilter: properties => properties.FloatPropertyComparison("_BlendingPreset", DisplayFilter.ComparisonType.Equal, 0.0f),
                    childElements: new List<Element>() {
                        new EnumPropertyDropdown<UnityEngine.Rendering.BlendMode>("_BlendSrcFactor"),
                        new EnumPropertyDropdown<UnityEngine.Rendering.BlendMode>("_BlendDstFactor"),
                        new EnumPropertyDropdown<UnityEngine.Rendering.BlendMode>("_BlendDstFactorA"),
                        new EnumPropertyDropdown<UnityEngine.Rendering.BlendOp>("_BlendOp"),
                        new InfoBox("Only first 5 Blend Operations (Add to Max) are guaranteed to work on all platforms ", MessageType.Warning,
                            displayFilter: properties => properties.FloatPropertyComparison("_BlendOp", DisplayFilter.ComparisonType.Higher, 5.0f)),
                }),

                new Group("Bloom Options", hasFoldout: true,
                    enabledFilter: properties => properties.FloatPropertyComparison("_BloomPreset", DisplayFilter.ComparisonType.Equal, 0.0f),
                    childElements: new List<Element>() {
                        new KeywordDropdown("_WhiteBoostType", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                            new KeywordDropdown.Option(string.Empty, "None"),
                            new KeywordDropdown.Option("_WHITEBOOSTTYPE_MAINEFFECT", "MainEffect"),
                            new KeywordDropdown.Option("_WHITEBOOSTTYPE_ALWAYS", "Always"),
                        }),
                        new EnumPropertyDropdown<UnityEngine.Rendering.BlendMode>("_BlendSrcFactorA"),

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
                        new Group(enabledFilter: properties => properties.FloatPropertyComparison("_StencilPreset", DisplayFilter.ComparisonType.NotEqual, 0.0f), childElements: new List<Element>() {
                            new FloatProperty("_StencilRefValue"),
                            new EnumPropertyDropdown<UnityEngine.Rendering.CompareFunction>("_StencilComp"),
                            new EnumPropertyDropdown<UnityEngine.Rendering.StencilOp>("_StencilPass"),
                        }),
                }),

                new Group("Z/Depth Test", hasFoldout: true, childElements: new List<Element>() {
                    new EnumPropertyDropdown<UnityEngine.Rendering.CompareFunction>("_ZTest"),
                    new FloatProperty("_OffsetFactor"),
                    new FloatProperty("_OffsetUnits"),
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

        private static Element SpecialContexts() {
            return new Category("Special Contexts", childElements: new List<Element>() {

                new SubFeature("NOISE_DITHERING", "_EnableNoiseDithering"),

                new KeywordDropdown("_Custom_Time", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(String.Empty, "Standard"),
                    new KeywordDropdown.Option("_CUSTOM_TIME_SONG_TIME", "Song Time"),
                    new KeywordDropdown.Option("_CUSTOM_TIME_FREEZE", "Freeze", description: "Useful when"),
                }, tooltip: "Standard: Time always updates, everywhere and even during pause \n\n" +
                            "Song Time: Time is updated only when song plays, using its speed. Doesn't work elsewhere \n\n" +
                            "Freeze: Time doesn't move on its own but can be controlled externally using _TimeOffset \n"),

                ParticleVertexStream(),

                new SubFeature("SOFT_PARTICLES", "_EnableSoftParticles", childElements: new List<Element>() {
                    new RangeProperty("_SoftFactor")
                }, tooltip: "Makes particles fade out when close to solid surfaces. Only some platforms though, so don't rely on it."),

                new SubFeature("HOLOGRAM", "_EnableHologram", childElements: new List<Element>() {
                    new ColorProperty("_HologramColor")
                }),

                new SubFeature("FAKE_MIRROR_TRANSPARENCY", "_FakeMirrorTransparencyEnabled", childElements: new List<Element>() {
                    new FloatProperty("_FakeMirrorTransparencyMultiplier")
                }),

                new SubFeature("NOTE_VERTEX_DISTORTION", "_EnableVertexDistortion", childElements: new List<Element>() {
                    new InfoBox("Mirror Vertex Distortion will be deprecated with Tours", MessageType.Warning)
                })

            });
        }

        private static Element Fog() {

            return new Category("Fog", childElements: new List<Element>() {
                new KeywordDropdown("_FogType", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("_FOGTYPE_ALPHA", "Alpha"),
                    new KeywordDropdown.Option("_FOGTYPE_LERP", "Color"),
                    new KeywordDropdown.Option("_FOGTYPE_COLOR", "Lerp"),
                }, childElements: new List<Element>() {
                    new Group(enabledFilter: properties => properties.HasNonZeroIndex("_FogType"), childElements: new List<Element>() {
                        new FloatProperty("_FogScale"),
                        new FloatProperty("_FogStartOffset"),
                        new SubFeature("PRECISE_FOG", "_PreciseFog"),
                        new SubFeature("HEIGHT_FOG", "_EnableHeightFog", childElements: new List<Element>() {
                            new FloatProperty("_FogHeightScale"),
                            new FloatProperty("_FogHeightOffset")
                        })
                    })
                }, tooltip: "Alpha: Gradually fades alpha out \n\n" +
                            "Color: Colors the particle by the fog color and gradually fades alpha out  \n\n" +
                            "Lerp: Gradually blends color into the fog color \n")

            });
        }

        private static Element AlphaControl() {

            return new Category("Alpha Control", childElements: new List<Element>() {

                new FloatProperty("_AlphaMultiplier"),
                new SubFeature("SQUARE_ALPHA", "_SquareAlpha"),

                new Feature("FILL_ALPHA", "_EnableFillAlpha", childElements: new List<Element>() {
                    new FloatProperty("_FillAlpha"),
                    new KeywordDropdown("_FillMask", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                     new KeywordDropdown.Option(string.Empty, "None"),
                     new KeywordDropdown.Option(string.Empty, "MainTex Blue"),
                    }),
                    new KeywordDropdown("_FillColor", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "Base Color"),
                        new KeywordDropdown.Option(string.Empty, "Material"),
                        new KeywordDropdown.Option(string.Empty, "Black"),
                        new KeywordDropdown.Option(string.Empty, "White"),
                    }),
                }),

                new Feature("CLOSE_TO_CAMERA_DISAPPEAR", "_EnableCloseToCameraDisappear", childElements: new List<Element>() {
                    new FloatProperty("_CloseToCameraOffset"),
                    new FloatProperty("_CloseToCameraFactor")
                }),

                new Feature("VIEW_ALIGN_DISAPPEAR", "_EnableViewAlignDisappear", childElements: new List<Element>() {
                    new NoKeywordFeature("_SquareAngleForViewAlignDisappear", Feature.Style.SubFeature),
                    new FloatProperty("_ViewAlignFactor", tooltip: "Shortens or lengthens the gradient between the fade point and max brightness. Lower values will lengthen the gradient whereas a Higher value will shorten the gradient"),
                    new FloatProperty("_ViewAlignOffset")
                }, description: "Squares the angle value to make the gradient step smoother between 1 and 0"),

                new KeywordDropdown("_Override_Final_Alpha", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("_OVERRIDE_FINAL_ALPHA_FLAT", "Flat", description: "Main use case is when using DST Color blend of One minus Src Alpha"),
                    new KeywordDropdown.Option("_OVERRIDE_FINAL_ALPHA_COLOR_BASED", "Color Based", description: "Main use case is when using DST Color blend of One minus Src Alpha \nOverride value lerps to 0 as base color value approaches 1"),

                }, childElements: new List<Element>() {
                    new FloatProperty("_FinalAlphaOverride", enabledFilter: properties => properties.HasNonZeroIndex("_Override_Final_Alpha"))
                }),

                new Feature("WORLD_NOISE", "_EnableWorldNoise", childElements: new List<Element>() {
                    new FloatProperty("_WorldNoiseScale"),
                    new FloatProperty("_WorldNoiseIntensityOffset"),
                    new FloatProperty("_WorldNoiseIntensityScale"),
                    new VectorProperty("_WorldNoiseScrolling", 3),
                }, description: "The game needs to enter play mode at least once for the noise to be initiated"),

                new KeywordDropdown("_CutoutType", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("_CUTOUTTYPE_ALPHA_CLIP", "Alpha Clip"),
                    new KeywordDropdown.Option("_CUTOUTTYPE_WORLDSPACE_NOISE", "Worldspace Noise", childElements: new List<Element>() {
                        new FloatProperty("_CutoutTexScale"),
                        new RangeProperty("_CutoutGradientWidth"),
                        new VectorProperty("_CutoutTexOffset", 3)
                    }),
                    new KeywordDropdown.Option("_CUTOUTTYPE_SCALE", "Scale"),

                    }, childElements: new List<Element>() {
                    new RangeProperty("_Cutout", enabledFilter: properties => properties.HasNonZeroIndex("_CutoutType"))

                    }, tooltip: "Alpha Clip: Pixels get clipped based on their transparency \n\n" +
                             "Worldspace Noise: 3D Noise is used to clip pixels of the object  \n\n" +
                             "Scale: Performant. Vertices get scaled down toward the object pivot \n"
                ),

                new Feature("DISSOLVE", "_EnableDissolve", childElements: new List<Element>() {

                    new FloatProperty("_DissolveGradientWidth"),
                    new NoKeywordFeature("_InvertDissolve", Feature.Style.SubFeature, childElements: new List<Element>() {
                        new InfoBox("To change direction it is better to use negative Axis Vector instead of Dissolve invert")
                    }),

                    new KeywordDropdown("_Dissolve_Space", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "Local"),
                        new KeywordDropdown.Option("_DISSOLVE_SPACE_WORLD", "World"),
                        new KeywordDropdown.Option("_DISSOLVE_SPACE_WORLD_CENTERED", "World Centered", description: "World space but centered on object's pivot"),
                        new KeywordDropdown.Option("_DISSOLVE_SPACE_UV", "UV"),
                        }, tooltip: "Alpha Clip: Pixels get clipped based on their transparency \n\n" +
                                "Worldspace Noise: 3D Noise is used to clip pixels of the object  \n\n" +
                                "Scale: Performant. Vertices get scaled down toward the object pivot \n"
                    ),

                    new VectorProperty("_DissolveAxisVector", 3),
                    new SubFeature("DISSOLVE_PROGRESS", "_UseDissolveProgress",
                        childElements: new List<Element>() {
                            new FloatProperty("_DissolveStartValue"),
                            new FloatProperty("_DissolveEndValue"),
                            new SubFeature("DISSOLVE_PROGRESS_FROM_VERTEX_ALPHA", "_DissolveProgressFromVertexAlpha"),
                            new RangeProperty("_DissolveProgress", displayFilter: properties => properties.HasNoneOfKeywords("DISSOLVE_PROGRESS_FROM_VERTEX_ALPHA")),
                    }),
                    new FloatProperty("_DissolveOffset", displayFilter: properties => properties.HasNoneOfKeywords("DISSOLVE_PROGRESS")),

                    new Feature("DISSOLVE_COLOR", "_UseDissolveColor", childElements: new List<Element>() {
                        new ColorProperty("_DissolveColor"),
                        new FloatProperty("_DissolveColorIntensity"),
                        new FloatProperty("_CutColorFalloff"),
                        new NoKeywordFeature("_MultiplyDissolveGridByAlpha", Feature.Style.SubFeature)
                    }),

                    new KeywordDropdown("_Dissolve_Grid", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "None"),
                        new KeywordDropdown.Option("_DISSOLVE_GRID_LOCAL", "Local"),
                        new KeywordDropdown.Option("_DISSOLVE_GRID_WORLD", "World", description: "World space but centered on object's pivot"),
                        new KeywordDropdown.Option("_DISSOLVE_GRID_UV", "Uv"),
                    }, childElements: new List<Element>() {
                        new Group(enabledFilter: properties => properties.HasNonZeroIndex("_Dissolve_Grid"), childElements: new List<Element>() {
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

                new Feature("PLANE_CLIPPING", "_PlaneClipping", childElements: new List<Element>() {
                    new VectorProperty("_ClippingPlanePosition", 3, tooltip: "World position of plane origin"),
                    new VectorProperty("_ClippingPlaneNormal", 3, tooltip: "World normal of the plane")
                }),

                new Feature("REVEAL", "_EnableReveal", description: "Old feature designed to turn sabers on consistently across multiple materials and shaders")

            });
        }

        private static Element ColorControl() {

            return new Category("Color Control", childElements: new List<Element>() {
                new Feature("VERTEX_COLOR", "_EnableVertexColor", childElements: new List<Element>() {
                    new SubFeature("VERTEX_SQUARE_ALPHA", "_SquareVertexAlpha"),
                    new SubFeature("VERTEX_RED_IS_ALPHA", "_RedIsVertexAlpha"),
                    new SubFeature("LIFETIME", "_EnableLifetime"),
                    new KeywordDropdown("_VertexChannels", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "Full RGBA"),
                        new KeywordDropdown.Option("_VERTEXCHANNELS_A", "Alpha only"),
                        new KeywordDropdown.Option("_VERTEXCHANNELS_RGB", "RBG only"),
                    })
                }),

                new Feature("CHROMATIC_ABERRATION", "_UseChromaticAberration", childElements: new List<Element>() {
                    new InfoBox("Experimental feature. Works best on white and does less as color approaches the RBG channels below.\n" +
                                "High values can cause aliasing and appear pixelated in the headset (depending on head tilt among other things)."),
                    new VectorProperty("_ChromaticAberration", 3, tooltip: "RBG channels affected by Chromatic Aberration"),
                }),

                new Feature("COLOR_BY_FOG", "_EnableObstacle", childElements: new List<Element>() {
                    new KeywordDropdown("_Fog_Mask_Source", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "None"),
                        new KeywordDropdown.Option("_FOG_MASK_SOURCE_TEXTURE", "Texture", childElements: new List<Element>() {
                            new InfoBox("Color by Fog > Fog Mask Source > Texture mask option is planned but not implemented yet", MessageType.Error)
                        }),
                        new KeywordDropdown.Option("_FOG_MASK_SOURCE_PRIMARY_MASK", "Primary Mask", childElements: new List<Element>() {
                            new InfoBox("Color by Fog > Fog Mask Source > Primary Mask option requires Mask to be enabled", MessageType.Error,
                                displayFilter: properties => properties.HasNoneOfKeywords("MASK"))
                        }),
                    }),
                    new FloatProperty("_ObstacleFogMultiplier"),
                    new FloatProperty("_ObstacleFogMax"),
                    new RangeProperty("_ObstacleColorInfluence"),
                    new SubFeature("FOG_COLOR_HIGHLIGHT", "_FogColorHighlight", childElements: new List<Element>() {
                        new FloatProperty("_ObstacleFogHighlightMultiplier"),
                    }),
                }),

                new Feature("COLOR_GRADIENT", "_UseColor_Gradient", childElements: new List<Element>() {
                    new TexturePropertyMiniThumbnail("_ColorGradient"),
                    new SubFeature("GRADIENT_ALPHA", "_GradientUseAlpha"),
                    new RangeProperty("_GradientPosition"),
                    new FloatProperty("_GradientPanningSpeed")
                }),

                new Feature("SPECTROGRAM_COLOR", "_UseSpectrogram",
                    description: "Spectrogram Color relies on uv3 where x defines column and y height",
                    childElements: new List<Element>() {
                        new InfoBox("Color by Spectrogram feature is not compatible with Flat Spectrogram Displacement",
                            MessageType.Error, displayFilter: properties => properties.HasAnyOfKeywords("_SPECTROGRAM_FLAT")),
                        new RangeProperty("_SpectrogramBaseValue"),
                        new RangeProperty("_SpectrogramRange")
                }),

                new Feature("COLOR_ARRAY", "_UseColorArray",
                    description: "A way to color one mesh using many light ids - useful in cases where many different meshes would be needed otherwise. See documentation for details",
                    documentationUrl: "https://beatgames.atlassian.net/wiki/spaces/BS/pages/109346823/Environment+Optimization+Performance+Testing#Color-Light-Array"
                ),

                new Feature("SECONDARY_COLOR", "_EnableSecondaryColor",
                    description: "Please avoid using this technique unless you need to control two independent dynamic colors for one renderer based on texture",
                    childElements: new List<Element>() {
                        new ColorProperty("_SecondaryColor"),
                        new TextureProperty("_SecondaryColorTex"),
                        new VectorProperty("_SecondaryColorPanning", 2)
                })

            });
        }

        private static Element UVControl() {

            return new Category("UV Control", childElements: new List<Element>() {

                SecondaryUVSelection(),

                new SubFeature("ROTATE_UV", "_EnableRotateUV", childElements: new List<Element>() {
                    new NoKeywordDropdown("_Rotate_UV", KeywordDropdown.Style.SubFeature, new List<NoKeywordDropdown.NoKeywordOption>() {
                        new NoKeywordDropdown.NoKeywordOption("90\u00b0 clockwise"),
                        new NoKeywordDropdown.NoKeywordOption("90\u00b0 counter-clockwise"),
                        new NoKeywordDropdown.NoKeywordOption("180\u00b0"),
                    }),
                    new SubFeature("ROTATE_MAIN_ONLY", "_RotateMainUVOnly")
                }),

                WorldspacePanning(),

                new SubFeature("PIXELATE", "_Pixelate", childElements: new List<Element>() {
                    new VectorProperty("_PixelateResolution", 2, uiToMaterialDelegate: InOutValueModification.Round),
                }),

                new SubFeature("MIPMAP_BIAS", "_UseMipmapBias", tooltip: "Offset what mipmap level is used for Main Texture", childElements: new List<Element>() {
                    new FloatProperty("_MipmapBias", tooltip: "Negative numbers to make things appear sharper at cost of performance. Positive values for an opposite effect. Only applies to Main Texture")
                }),

                new SubFeature("CUSTOM_WRAPPING", "_EnableCustomPadding", childElements: new List<Element>() {
                    new InfoBox("Texture must have Clamp wrapping mode for Custom Wrapping to work", MessageType.Error,
                        displayFilter: properties => properties.TexturePropertyCondition("_MainTex", tex => tex?.wrapMode != TextureWrapMode.Clamp)),
                    new VectorProperty("_CustomPadding", 2)
                }),

            });
        }

        private static Element GeometryFeatures() {

            return new Category("Geometry Features", childElements: new List<Element>() {

                new KeywordDropdown("_RimLight", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("RIMLIGHT_ADD", "Add"),
                    new KeywordDropdown.Option("RIMLIGHT_MASKED_ADD", "Masked Add"),
                    new KeywordDropdown.Option("RIMLIGHT_MULTIPLY", "Multiply"),
                }, childElements: new List<Element>() {
                    new Group(enabledFilter: properties => properties.HasNonZeroIndex("_RimLight"), childElements: new List<Element>() {
                        new NoKeywordFeature("_RimlightInvert", Feature.Style.SubFeature),
                        new FloatProperty("_RimLightEdgeStart"),
                        new FloatProperty("_RimLightIntensity")
                    })
                }),

                new KeywordDropdown("_Billboard", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("_BILLBOARD_FULL", "Full"),
                    new KeywordDropdown.Option("_BILLBOARD_Y_AXIS", "Y Axis", childElements: new List<Element>() {
                        new InfoBox("Scale should be equal on X and Z axis for this to work", MessageType.Info)
                    }),
                    new KeywordDropdown.Option("_BILLBOARD_CAMERA_FACING", "Camera Facing"),

                    }, childElements: new List<Element>() {
                        new FloatProperty("_BillboardScale", displayFilter: properties => properties.HasNonZeroIndex("_Billboard")),
                }),


                new KeywordDropdown("_Curve_Vertices", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("_CURVE_VERTICES_AROUND_X", "Around X"),
                    new KeywordDropdown.Option("_CURVE_VERTICES_AROUND_Y", "Around Y"),
                    new KeywordDropdown.Option("_CURVE_VERTICES_AROUND_Z", "Around Z"),
                }),

                new Feature("VERTEX_START_END", "_EnableStartEnd",
                    description: "Allows small tweaks to vertices of a rectangular mesh alongside its vertical UV",
                    childElements: new List<Element>() {
                        new FloatProperty("_AlphaStart"),
                        new FloatProperty("_AlphaEnd"),
                        new FloatProperty("_WidthStart"),
                        new FloatProperty("_WidthEnd"),
                }),

                new Feature("VERTEX_DISPLACEMENT", "_VertexDisplacement", childElements: new List<Element>() {
                    new TextureProperty("_DisplacementTex"),
                    new FloatProperty("_DisplacementPanningSpeed"),
                    new VectorProperty("_DisplacementPanning", 2),
                    new SubFeature("SPATIAL_DISPLACEMENT", "_3DDisplacement"),
                    new FloatProperty("_DisplacementStrength"),
                    new VectorProperty("_DisplacementAxes", 3, displayFilter: properties => properties.HasAnyOfKeywords("SPATIAL_DISPLACEMENT")),

                    new SubFeatureSecondaryUV("SECONDARY_UVS_DISPLACEMENT", "_DisplacementSecondaryUVs"),
                    new SubFeaturePerParticleRandom("DISPLACEMENT_PER_PARTICLE_RANDOM", "_DisplacementPerParticleRandomization"),

                    new KeywordDropdown("_Spectrogram", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "None"),
                        new KeywordDropdown.Option("_SPECTROGRAM_FLAT", "Flat", description: "Use Spectrogram Row component to set the _SpectrogramData property"),
                        new KeywordDropdown.Option("_SPECTROGRAM_FULL", "Full", description: "Use uv3 coords to map vertices to one of 64 spectrogram channels",
                            childElements: new List<Element>() {
                                new FloatProperty("_UV3Offset"),
                                new FloatProperty("_UV3Scale"),
                        })
                    })
                }, description: "Allows using a texture to displace the vertices either along its normal (by default) or in the " +
                                "XYZ directions from the RGB texture when using 3D displacement"),

                new Feature("VERTEX_FLIPBOOK", "_EnableVertexFlipbook",
                    description: "Allows going through a sequence of meshes within one renderer\nR channel = Frame # \nG channel = Offset to randomize",
                    childElements: new List<Element>() {
                        new FloatProperty("_VertexFlipbookCount", uiToMaterialDelegate: InOutValueModification.Round),
                        new FloatProperty("_VertexFlipbookSpeed"),
                        new SubFeature("VERTEX_FLIPBOOK_FADE", "_EnableVertexFlipbookFade"),
                }),

            });
        }

        private static Element TextureFeatures() {

            return new Category("Texture Based Effects", childElements: new List<Element> {

                new KeywordDropdown("Distortion", KeywordDropdown.Style.Feature, new List<KeywordDropdown.Option>() {
                    new KeywordDropdown.Option(string.Empty, "None"),
                    new KeywordDropdown.Option("DISTORTION_SIMPLE", "Simple"),
                    new KeywordDropdown.Option("DISTORTION_FLOWMAP", "Flowmap"),

                }, childElements: new List<Element>() {

                    new KeywordDropdown("Distortion_Target", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option("DISTORTION_TARGET_MAIN", "Main"),
                        new KeywordDropdown.Option("DISTORTION_TARGET_MASK", "Mask"),
                        new KeywordDropdown.Option("DISTORTION_TARGET_MASK2", "Mask2"),
                    }, enabledFilter: properties => properties.HasAnyOfKeywords("DISTORTION_FLOWMAP", "DISTORTION_SIMPLE")),


                    new Group(childElements: new List<Element>() {
                        new TextureProperty("_DistortionTex"),
                        new FloatProperty("_DistortionStrength"),
                        new VectorProperty("_DistortionAxes", 2),
                        new VectorProperty("_DistortionPanning", 2),
                        new SubFeatureSecondaryUV("SECONDARY_UVS_DISTORTION", "_DistortionSecondaryUVs"),
                        new SubFeaturePerParticleRandom("DISTORTION_PER_PARTICLE_RANDOM", "_DistortionParticleRandomization"),
                        new SubFeatureWorldspacePanning("WORLDSPACE_PANNING_DISTORTION", "_DistortionTexWorldspacePanning"),
                        }, displayFilter: properties => properties.HasAnyOfKeywords("DISTORTION_SIMPLE")
                    ),

                    new Group(childElements: new List<Element>() {
                        new TextureProperty("_FlowTex"),
                        new VectorProperty("_FlowPanning", 2),
                        new RangeProperty("_FlowSpeed"),
                        new RangeProperty("_FlowStrength"),
                        new VectorProperty("_FlowAdd", 2),
                        new SubFeatureSecondaryUV("SECONDARY_UVS_FLOWMAP", "_FlowmapSecondaryUVs"),
                        new SubFeaturePerParticleRandom("FLOWMAP_PER_PARTICLE_RANDOM", "_FlowmapParticleRandomization"),
                        new SubFeatureWorldspacePanning("WORLDSPACE_PANNING_FLOWMAP", "_FlowmapTexWorldspacePanning"),
                        }, displayFilter: properties => properties.HasAnyOfKeywords("DISTORTION_FLOWMAP")
                    ),
                }, tooltip: "Simple: Cheaper. Uses RG texture to offset target UVs \n\n " +
                            "Flowmap: More expensive. Uses RG texture to define direction along which a texture movement is created \n"),

                new Feature("EROSION", "_Erosion", childElements: new List<Element>() {
                    new KeywordDropdown("_Erosion_Source", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "Erosion"),
                        new KeywordDropdown.Option("_EROSION_SOURCE_MAINTEX", "MainTex"),
                        new KeywordDropdown.Option("_EROSION_SOURCE_MASK", "Mask"),
                        new KeywordDropdown.Option("_EROSION_SOURCE_SECONDARY_MASK", "Secondary Mask"),
                        //new KeywordDropdown.Option("_EROSION_SOURCE_VERTEX_ALPHA", "Vertex Alpha"), // Not implemented yet
                        new KeywordDropdown.Option("_EROSION_SOURCE_COMBINED", "Combined"),
                    }),

                    new SubFeatureSecondaryUV("SECONDARY_UVS_EROSION", "_ErosionSecondaryUVs"),
                    new SubFeatureWorldspacePanning("WORLDSPACE_PANNING_EROSION", "_ErosionTexWorldspacePanning"),
                    new SubFeaturePerParticleRandom("EROSION_PER_PARTICLE_RANDOM", "_ErosionPerParticleRandomization"),

                    new Group(displayFilter: properties => properties.FloatPropertyComparison("_Erosion_Source", DisplayFilter.ComparisonType.Equal, 0), childElements: new List<Element>() {
                        new TextureProperty("_ErosionTex"),
                        new VectorProperty("_ErosionPanning", 2),
                    }),

                    new SubFeature("EROSION_VERTEX_ALPHA_THRESHOLD", "_ErosionVertexThreshold"),
                    new RangeProperty("_ErosionThreshold", displayFilter: properties => properties.HasNoneOfKeywords("EROSION_VERTEX_ALPHA_THRESHOLD")),
                    new RangeProperty("_ErosionSmoothness")

                }),

            });
        }
        private static Element BaseProperties() {

            return new Category("Main Texture & Masks", childElements: new List<Element> {

                new ColorProperty("_Color"),
                new FloatProperty("_Intensity"),
                new InfoBox("Please avoid using Color intensity over 3 for production-ready assets using any color scheme", MessageType.Warning,
                    displayFilter: properties => properties.FloatPropertyComparison("_Intensity", DisplayFilter.ComparisonType.Higher, 3.0f)),

                new PresetDropdown("_BloomPreset", "Bloom Preset",
                    tooltip: "Bloom: Maximizes visual quality on all platforms at the cost of slight visual mismatch between the platforms \n\n" +
                             "Whiteboost only: No Bloom is applied on high end platforms. Useful to ensure the same looks \n\n" +
                             "No Bloom: Straightforward flat color, no surprises there \n\n",
                    options: new List<PresetDropdown.Option>() {
                        new PresetDropdown.Option("Custom", presetValues: new List<PresetDropdown.PresetValue>() {
                        }),
                        new PresetDropdown.Option("Bloom (improves PC fidelity)", presetValues: new List<PresetDropdown.PresetValue>() {
                            new PresetDropdown.FloatPropertyPreset( "_BlendSrcFactorA", 5.0f), // Src Alpha
                            new PresetDropdown.KeywordPreset("_WhiteBoostType", "_WHITEBOOSTTYPE_MAINEFFECT", true), // Whiteboost MainEffect
                        }),
                        new PresetDropdown.Option("Whiteboost (visual parity)", presetValues: new List<PresetDropdown.PresetValue>() {
                            new PresetDropdown.FloatPropertyPreset( "_BlendSrcFactorA", 0.0f), // Zero
                            new PresetDropdown.KeywordPreset("_WhiteBoostType", "_WHITEBOOSTTYPE_ALWAYS", true), // Whiteboost Always
                        }),
                        new PresetDropdown.Option("No Bloom", presetValues: new List<PresetDropdown.PresetValue>() {
                            new PresetDropdown.FloatPropertyPreset( "_BlendSrcFactorA", 0.0f), // Zero
                            new PresetDropdown.KeywordPreset("_WhiteBoostType", "_WHITEBOOSTTYPE_MAINEFFECT", false), // Whiteboost None
                        }),
                }),

                new Group("Bloom Adjusting", indentationCount: 1, hasFoldout: true, enabledFilter: properties => properties.HasNonZeroIndex("_WhiteBoostType"),
                    childElements: new List<Element>() {
                        new FloatProperty("_QuestWhiteboostMultiplier"),
                        new FloatProperty("_BloomMultiplier"),
                        new SubFeature("GREEN_CHANNEL_WHITEBOOST", "_GreenChannelWhiteboost", childElements: new List<Element>() {
                            new InfoBox("Make sure Main Texture contains green channel created for this purpose"),
                            new InfoBox("Green channel whiteboost feature is not compatible with Texture color",
                                displayFilter: properties => properties.HasAnyOfKeywords("TEXTURE_COLOR")),
                            new InfoBox("Green channel whiteboost feature is not compatible with Flipbook feature",
                                displayFilter: properties => properties.HasAnyOfKeywords("TEXTURE_FLIPBOOK")),
                            new InfoBox("Green channel whiteboost feature is not compatible with Flowmap distortion",
                                displayFilter: properties => properties.HasAnyOfKeywords("DISTORTION_FLOWMAP")),
                        }),
                        new SubFeature("REMAP_WHITEBOOST_START", "_RemapWhiteboostStart", childElements: new List<Element>() {
                            new RangeProperty("_WhiteboostRemapStart")
                        }),
                }),

                new Feature("MAIN_TEXTURE", "_UseMainTex", childElements: new List<Element>() {
                    new TextureProperty("_MainTex"),
                    new VectorProperty("_UvPanning", 2),
                    new SubFeature("TEXTURE_COLOR", "_EnableTextureColor", tooltip: "Use color channels of a texture instead of assuming it grayscale"),
                    new KeywordDropdown("_AlphaChannel", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "Alpha", childElements: new List<Element>() {
                            new InfoBox("Make sure you use true alpha channel and not the alpha from grayscale as that can mess sRGB treatment", MessageType.Info)
                        }),
                        new KeywordDropdown.Option("_ALPHACHANNEL_RED", "Red"),
                        new KeywordDropdown.Option("_ALPHACHANNEL_TEXTURE_BLEND", "Texture Blend",
                            description: "Blends between textures based on view angle. Diagonals are optional. \nRed Channel = Top & Bottom \nBlue channel = Left & Right \nGreen Channel = Diagonal Top \nAlpha Channel = Diagonal Bottom",
                            childElements: new List<Element>() {
                                new InfoBox("Distortion and Texture Flipbooks will not work with Alpha channel > Texture Blend option", MessageType.Warning,
                                    displayFilter: properties => properties.HasAnyOfKeywords("TEXTURE_FLIPBOOK", "DISTORTION_FLOWMAP", "DISTORTION_SIMPLE")),
                                new InfoBox("Billboard Y Axis should be enabled for Alpha channel > Texture Blend option to work properly", MessageType.Warning,
                                    displayFilter: properties => properties.HasNoneOfKeywords("_BILLBOARD_Y_AXIS")),
                                new NoKeywordFeature("_SquareAngleForTextureFade", Feature.Style.SubFeature),
                                new RangeProperty("_TopBotFadeAngle"),
                                new RangeProperty("_LeftRightFadeAngle"),
                                new NoKeywordFeature("_ApplyTopBotMaskToEnds", Feature.Style.SubFeature),
                                new SubFeature("_DIAGONAL_CHANNEL", "_Diagonal_Channel", childElements: new List<Element>() {
                                    new RangeProperty("_DiagonalBottomFadeAngle"),
                                    new RangeProperty("_DiagonalTopFadeAngle")
                                })

                        })
                    }, enabledFilter: properties => properties.HasNoneOfKeywords("TEXTURE_COLOR")),

                    new Space(12.0f),
                    new SubFeatureSecondaryUV("SECONDARY_UVS_MAIN", "_MainTexSecondaryUVs"),
                    new SubFeaturePerParticleRandom("MAIN_PER_PARTICLE_RANDOM", "_MainPerParticleRandomization"),
                    new SubFeatureWorldspacePanning("WORLDSPACE_PANNING_MAIN", "_MainTexWorldspacePanning"),


                    new SubFeature("TEXTURE_FLIPBOOK", "_UseTextureFlipbook", childElements: new List<Element>() {
                        new InfoBox("Keep the texture in sRGB or alpha will have different intensity"),
                        new InfoBox("Frame 1 in RGBA texture contains grayscale frames 1234 \nFrame 2 in RGBA texture contains grayscale frames 4567",
                            displayFilter: properties => properties.HasNoneOfKeywords("FLIPBOOK_BLENDING_OFF")),
                        new InfoBox("Frame 1 in RGBA texture contains grayscale frames 1234 \nFrame 2 in RGBA texture contains grayscale frames 5678",
                            displayFilter: properties => properties.HasAllOfKeywords("FLIPBOOK_BLENDING_OFF")),
                        new FloatProperty("_FlipbookColumns", uiToMaterialDelegate: InOutValueModification.Round),
                        new FloatProperty("_FlipbookRows", uiToMaterialDelegate: InOutValueModification.Round),
                        new FloatProperty("_FlipbookNonloopableFrames", uiToMaterialDelegate: InOutValueModification.Round),
                        new FloatProperty("_FlipbookSpeed"),
                        new SubFeature("FLIPBOOK_BLENDING_OFF", "_FlipbookBlendingOff"),

                        new Feature("MOTION_VECTORS", "_UseMotionVectors", childElements: new List<Element>() {
                            new TextureProperty("_MotionVectorTex"),
                            new FloatProperty("_MotionVectorColumns", uiToMaterialDelegate: InOutValueModification.Round),
                            new FloatProperty("_MotionVectorRows", uiToMaterialDelegate: InOutValueModification.Round),
                            new FloatProperty("_MotionVectorSpeed"),
                            new FloatProperty("_MotionVectorIntensity")
                        }),
                    }),
                }),

                new NoKeywordDropdown("_BaseLayer", displayName: "Base", style: KeywordDropdown.Style.SubFeature,
                    displayFilter: properties => properties.HasNoneOfKeywords("MAIN_TEXTURE"),
                    options: new List<NoKeywordDropdown.NoKeywordOption>(){
                        new NoKeywordDropdown.NoKeywordOption("Transparent"),
                        new NoKeywordDropdown.NoKeywordOption("White"),
                }),

                new Space(12.0f),
                new Feature("MASK", "_EnableMask", childElements: new List<Element>() {
                    new TextureProperty("_MaskTex"),
                    new VectorProperty("_MaskPanning", 2),
                    new SubFeature("MASK_RED_IS_ALPHA", "_MaskRedIsAlpha"),
                    new FloatProperty("_MaskStrength"),
                    new KeywordDropdown("_MaskBlend", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "Multiply"),
                        new KeywordDropdown.Option("_MASKBLEND_ADD", "Add"),
                        new KeywordDropdown.Option("_MASKBLEND_MASKED_ADD", "Masked Add"),
                    }),

                    new SubFeatureSecondaryUV("SECONDARY_UVS_MASK", "_MaskSecondaryUVs"),
                    new SubFeaturePerParticleRandom("MASK_PER_PARTICLE_RANDOM", "_MaskPerParticleRandomization"),
                    new SubFeatureWorldspacePanning("WORLDSPACE_PANNING_MASK", "_MaskTexWorldspacePanning"),
                    new SubFeature("MASK_DISSOLVE", "_MaskDissolve", childElements: new List<Element>() {
                        new FloatProperty("_MaskDissolveTiling"),
                        new FloatProperty("_MaskDissolveOffset"),
                    }),
                }),

                new Feature("MASK2", "_EnableMask2", childElements: new List<Element>() {
                    new TextureProperty("_Mask2Tex"),
                    new VectorProperty("_Mask2Panning", 2),
                    new SubFeature("MASK2_RED_IS_ALPHA", "_Mask2RedIsAlpha"),
                    new FloatProperty("_Mask2Strength"),
                    new KeywordDropdown("_Mask2Blend", KeywordDropdown.Style.SubFeature, new List<KeywordDropdown.Option>() {
                        new KeywordDropdown.Option(string.Empty, "Multiply"),
                        new KeywordDropdown.Option("_MASK2BLEND_ADD", "Add"),
                        new KeywordDropdown.Option("_MASK2BLEND_MASKED_ADD", "Masked Add"),
                    }),

                    new SubFeatureSecondaryUV("SECONDARY_UVS_MASK2", "_Mask2SecondaryUVs"),
                    new SubFeaturePerParticleRandom("MASK2_PER_PARTICLE_RANDOM", "_Mask2ParticleRandomization"),
                    new SubFeatureWorldspacePanning("WORLDSPACE_PANNING_MASK2", "_Mask2TexWorldspacePanning"),
                }),

            });
        }



    }

    // SubFeature Shortcuts
    public class SubFeatureSecondaryUV : Feature {

        public SubFeatureSecondaryUV(
            string keyword,
            string propertyName,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            List<Element> childElements = null,
            Color? backgroundColor = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(
            keyword,
            propertyName,
            Style.SubFeature,
            displayName,
            tooltip + "Requires Secondary UV option other than None",
            description,
            documentationUrl,
            documentationButtonLabel,
            childElements,
            backgroundColor,
            displayFilter,
            properties => properties.HasSecondaryUVsEnabled()
        ) { }
    }

    public class SubFeatureWorldspacePanning : Feature {

        public SubFeatureWorldspacePanning(
            string keyword,
            string propertyName,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            List<Element> childElements = null,
            Color? backgroundColor = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(
            keyword,
            propertyName,
            Style.SubFeature,
            displayName,
            tooltip + "Requires Worldspace Panning feature enabled",
            description,
            documentationUrl,
            documentationButtonLabel,
            childElements,
            backgroundColor,
            displayFilter,
            properties => properties.HasAnyOfKeywords("WORLDSPACE_PANNING")
        ) { }
    }

    public class SubFeaturePerParticleRandom : Feature {

        public SubFeaturePerParticleRandom(
            string keyword,
            string propertyName,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            List<Element> childElements = null,
            Color? backgroundColor = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(
            keyword,
            propertyName,
            Style.SubFeature,
            displayName,
            tooltip + "Requires Particle Vertex Stream feature enabled",
            description,
            documentationUrl,
            documentationButtonLabel,
            childElements,
            backgroundColor,
            displayFilter,
            properties => properties.HasAnyOfKeywords("PARTICLE_VERTEX_STREAM")
        ) { }
    }
}
