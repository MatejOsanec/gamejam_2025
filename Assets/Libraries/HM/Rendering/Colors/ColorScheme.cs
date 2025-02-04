using UnityEngine;
using System;

[Serializable]
public class ColorScheme {

    [SerializeField] string _colorSchemeId;
    [SerializeField][LocalizationKey] string _colorSchemeNameLocalizationKey;
    [SerializeField] bool _useNonLocalizedName;
    [SerializeField] string _nonLocalizedName;
    [SerializeField] bool _isEditable;

    [Space]
    [SerializeField] bool _overrideNotes;

    [Space]
    [SerializeField] Color _saberAColor;
    [SerializeField] Color _saberBColor;

    [Space]
    [SerializeField] Color _obstaclesColor;
#if BS_TOURS || UNITY_EDITOR
#pragma warning disable CS0414
    [FutureField]
    [SerializeField] Color _bombsEmissionColor = Color.red;
#pragma warning restore CS0414
#endif

    [Space]
    [SerializeField] bool _overrideLights;

    [Space]
    [SerializeField] Color _environmentColor0;
    [SerializeField] Color _environmentColor1;
    [SerializeField] Color _environmentColorW = Color.white;
    [SerializeField] bool _supportsEnvironmentColorBoost;
    [SerializeField] Color _environmentColor0Boost;
    [SerializeField] Color _environmentColor1Boost;
    [SerializeField] Color _environmentColorWBoost = Color.white;

    public string colorSchemeId => _colorSchemeId;
    public string colorSchemeNameLocalizationKey => _colorSchemeNameLocalizationKey;
    public string nonLocalizedName => _nonLocalizedName;
    public bool useNonLocalizedName => _useNonLocalizedName;
    public bool isEditable => _isEditable;
    public bool overrideNotes => _overrideNotes;
    public Color saberAColor => _saberAColor;
    public Color saberBColor => _saberBColor;
    public bool overrideLights => _overrideLights;
    public Color environmentColor0 => _environmentColor0;
    public Color environmentColor1 => _environmentColor1;
    public Color environmentColorW => _environmentColorW;
    public bool supportsEnvironmentColorBoost => _supportsEnvironmentColorBoost;
    public Color environmentColor0Boost => _environmentColor0Boost;
    public Color environmentColor1Boost => _environmentColor1Boost;
    public Color environmentColorWBoost => _environmentColorWBoost;
    public Color obstaclesColor => _obstaclesColor;
#if BS_TOURS
    public Color bombsEmissionColor => _bombsEmissionColor;
#endif

    // Default constructor is needed so default values are assigned for the class fields
    public ColorScheme() { }

    public ColorScheme(
        string colorSchemeId,
        string colorSchemeNameLocalizationKey,
        bool useNonLocalizedName,
        string nonLocalizedName,
        bool isEditable,
        bool overrideNotes,
        Color saberAColor,
        Color saberBColor,
        bool overrideLights,
        Color environmentColor0,
        Color environmentColor1,
        Color environmentColorW,
        bool supportsEnvironmentColorBoost,
        Color environmentColor0Boost,
        Color environmentColor1Boost,
        Color environmentColorWBoost,
        Color obstaclesColor
#if BS_TOURS
        , Color bombsEmissionColor
#endif
    ) {

        _colorSchemeId = colorSchemeId;
        _colorSchemeNameLocalizationKey = colorSchemeNameLocalizationKey;
        _isEditable = isEditable;
        _overrideNotes = overrideNotes;
        _saberAColor = saberAColor;
        _saberBColor = saberBColor;
        _overrideLights = overrideLights;
        _environmentColor0 = environmentColor0;
        _environmentColor1 = environmentColor1;
        _environmentColorW = environmentColorW;
        _supportsEnvironmentColorBoost = supportsEnvironmentColorBoost;
        _environmentColor0Boost = environmentColor0Boost;
        _environmentColor1Boost = environmentColor1Boost;
        _environmentColorWBoost = environmentColorWBoost;
        _obstaclesColor = obstaclesColor;
#if BS_TOURS
        _bombsEmissionColor = bombsEmissionColor;
#endif
        _nonLocalizedName = nonLocalizedName;
        _useNonLocalizedName = useNonLocalizedName;
    }

    public ColorScheme(
        ColorScheme colorScheme,
        bool overrideNotes,
        Color saberAColor,
        Color saberBColor,
        bool overrideLights,
        Color environmentColor0,
        Color environmentColor1,
        Color environmentColorW,
        bool supportsEnvironmentColorBoost,
        Color environmentColor0Boost,
        Color environmentColor1Boost,
        Color environmentColorWBoost,
        Color obstaclesColor
#if BS_TOURS
        , Color bombsEmissionColor
#endif
    ) : this(
        colorScheme.colorSchemeId,
        colorScheme.colorSchemeNameLocalizationKey,
        colorScheme.useNonLocalizedName,
        colorScheme.nonLocalizedName,
        colorScheme.isEditable,
        overrideNotes,
        saberAColor,
        saberBColor,
        overrideLights,
        environmentColor0,
        environmentColor1,
        environmentColorW,
        supportsEnvironmentColorBoost,
        environmentColor0Boost,
        environmentColor1Boost,
        environmentColorWBoost,
        obstaclesColor
#if BS_TOURS
        , bombsEmissionColor
#endif
    ) { }

    public ColorScheme(ColorScheme colorScheme, ColorSchemeSO environmentColorScheme)
        : this(
            colorScheme,
            environmentColorScheme.colorScheme.environmentColorW,
            environmentColorScheme.colorScheme.environmentColorWBoost
        ) { }

    public ColorScheme(ColorSchemeSO colorScheme)
        : this(
            colorScheme.colorScheme,
            colorScheme.colorScheme.environmentColorW,
            colorScheme.colorScheme.environmentColorWBoost
        ) { }

    private ColorScheme(ColorScheme colorScheme, Color environmentColorW, Color environmentColorWBoost)
        : this(
            colorScheme.colorSchemeId,
            colorScheme.colorSchemeNameLocalizationKey,
            colorScheme.useNonLocalizedName,
            colorScheme.nonLocalizedName,
            colorScheme.isEditable,
            colorScheme.overrideNotes,
            colorScheme.saberAColor,
            colorScheme.saberBColor,
            colorScheme.overrideLights,
            colorScheme.environmentColor0,
            colorScheme.environmentColor1,
            environmentColorW,
            colorScheme.supportsEnvironmentColorBoost,
            colorScheme.environmentColor0Boost,
            colorScheme.environmentColor1Boost,
            environmentColorWBoost,
            colorScheme.obstaclesColor
#if BS_TOURS
            , colorScheme.bombsEmissionColor
#endif
        ) { }
}
