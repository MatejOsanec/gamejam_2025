using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HMAsyncRequest : HMAutoincrementedRequestId {

    public delegate void CancelHander(HMAsyncRequest request);

    public CancelHander CancelHandler {
        get {
            return _cancelHander;
        }
        set {
            _cancelHander = value;
        }
    }


    public bool cancelled { get { return _cancelled; } }


    private bool _cancelled = false;
    private CancelHander _cancelHander;
    
    public virtual void Cancel() {

        _cancelled = true;
        if (_cancelHander != null) {
            _cancelHander(this);
        }
    }
}