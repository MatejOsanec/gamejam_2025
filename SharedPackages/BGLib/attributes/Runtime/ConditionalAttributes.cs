using UnityEngine;

#if BS_TOURS || UNITY_EDITOR

// Originally ToursOnlyAttribute
public class FutureFieldAttribute : PropertyAttribute {

    public FutureFieldAttribute() { }
}

#endif

#if !BS_TOURS || UNITY_EDITOR

public class WillNotBeUsedAttribute : PropertyAttribute {

    public WillNotBeUsedAttribute() { }
}

#endif
