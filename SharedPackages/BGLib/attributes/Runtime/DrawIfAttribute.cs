using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfAttribute : PropertyAttribute {

    public readonly string propertyName;
    public readonly object value;
    public readonly object? orValue;
    public readonly DisablingType disablingType;

    public enum DisablingType {
        ReadOnly,
        DontDraw
    }

    public DrawIfAttribute(string propertyName, object value, DisablingType disablingType = DisablingType.DontDraw) {

        this.propertyName = propertyName;
        this.value = value;
        this.disablingType = disablingType;
    }
    
    public DrawIfAttribute(string propertyName, object value, object orValue, DisablingType disablingType = DisablingType.DontDraw) {

        this.propertyName = propertyName;
        this.value = value;
        this.orValue = orValue;
        this.disablingType = disablingType;
    }    
}