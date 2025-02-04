using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class NullAllowed : PropertyAttribute {

    public enum Context {
        Everywhere,
        Prefab
    }

    private readonly Context _context;

    public NullAllowed(Context context = Context.Everywhere) {

        _context = context;
    }

    public bool IsNullAllowedFor(Context context) {

        return _context == Context.Everywhere || _context == context;
    }
}
