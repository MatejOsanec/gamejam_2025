using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObservableChange {

    event System.Action didChangeEvent;
}
