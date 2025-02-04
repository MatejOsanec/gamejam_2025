using UnityEngine;

public static class ColorSchemeExtensions {

    public static ColorScheme ResolveColorScheme(
        ColorScheme playerOverrideColorScheme,
        bool playerOverrideLightshowColors,
        ColorScheme beatmapOverrideColorScheme,
        ColorScheme environmentColorScheme
    ) {

        var supportsBoost = playerOverrideColorScheme != null ?
            playerOverrideColorScheme.supportsEnvironmentColorBoost :
            beatmapOverrideColorScheme != null && beatmapOverrideColorScheme.overrideLights ?
                beatmapOverrideColorScheme.supportsEnvironmentColorBoost :
                environmentColorScheme.supportsEnvironmentColorBoost;

        return new ColorScheme(
            environmentColorScheme,
            overrideNotes: true,
            saberAColor: ResolveColor(
                playerOverrideColorScheme?.saberAColor,
                usePlayerOverride: true,
                beatmapOverrideColorScheme?.overrideNotes,
                beatmapOverrideColorScheme?.saberAColor,
                environmentColorScheme.saberAColor
            ),
            saberBColor: ResolveColor(
                playerOverrideColorScheme?.saberBColor,
                usePlayerOverride: true,
                beatmapOverrideColorScheme?.overrideNotes,
                beatmapOverrideColorScheme?.saberBColor,
                environmentColorScheme.saberBColor
            ),
            obstaclesColor: ResolveColor(
                playerOverrideColorScheme?.obstaclesColor,
                usePlayerOverride: true,
                beatmapOverrideColorScheme?.overrideNotes,
                beatmapOverrideColorScheme?.obstaclesColor,
                environmentColorScheme.obstaclesColor
            ),
#if BS_TOURS
            bombsEmissionColor: ResolveColor(
                playerOverrideColorScheme?.bombsEmissionColor,
                usePlayerOverride: true,
                beatmapOverrideColorScheme?.overrideNotes,
                beatmapOverrideColorScheme?.bombsEmissionColor,
                environmentColorScheme.bombsEmissionColor
            ),
#endif
            overrideLights: true,
            environmentColor0: ResolveColor(
                playerOverrideColorScheme?.environmentColor0,
                playerOverrideLightshowColors,
                beatmapOverrideColorScheme?.overrideLights,
                beatmapOverrideColorScheme?.environmentColor0,
                environmentColorScheme.environmentColor0
            ),
            environmentColor1: ResolveColor(
                playerOverrideColorScheme?.environmentColor1,
                playerOverrideLightshowColors,
                beatmapOverrideColorScheme?.overrideLights,
                beatmapOverrideColorScheme?.environmentColor1,
                environmentColorScheme.environmentColor1
            ),
            environmentColorW: environmentColorScheme.environmentColorW,
            supportsEnvironmentColorBoost: supportsBoost,
            environmentColor0Boost: ResolveColor(
                playerOverrideColorScheme?.environmentColor0Boost,
                playerOverrideLightshowColors,
                beatmapOverrideColorScheme?.overrideLights,
                beatmapOverrideColorScheme?.environmentColor0Boost,
                environmentColorScheme.environmentColor0Boost
            ),
            environmentColor1Boost: ResolveColor(
                playerOverrideColorScheme?.environmentColor1Boost,
                playerOverrideLightshowColors,
                beatmapOverrideColorScheme?.overrideLights,
                beatmapOverrideColorScheme?.environmentColor1Boost,
                environmentColorScheme.environmentColor1Boost
            ),
            environmentColorWBoost: environmentColorScheme.environmentColorWBoost
        );
    }

    public static Color ResolveColor(
        Color? playerOverrideColor,
        bool usePlayerOverride,
        bool? useBeatmapOverride,
        Color? beatmapOverrideColor,
        Color environmentColor
    ) {

        if (playerOverrideColor != null && usePlayerOverride) {
            return playerOverrideColor.Value;
        }

        var useOverride = useBeatmapOverride ?? false;
        if (useOverride && beatmapOverrideColor != null) {
            return beatmapOverrideColor.Value;
        }

        return environmentColor;
    }
}
