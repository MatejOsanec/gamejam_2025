using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IValue<T> {
    
    T value { get; set; }
}