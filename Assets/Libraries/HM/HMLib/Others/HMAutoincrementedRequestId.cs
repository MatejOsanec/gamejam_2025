using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HMAutoincrementedRequestId : IEquatable<HMAutoincrementedRequestId> {

    public ulong RequestId => _requestId;

    private static ulong _nextRequestId = 0;
    private readonly ulong _requestId = 0;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected static void NoDomainReloadInit() {

        _nextRequestId = 0;
    }
#endif

    public HMAutoincrementedRequestId() {

        _requestId = _nextRequestId;
        _nextRequestId++;
    }

    public bool Equals(HMAutoincrementedRequestId obj) {

        if (obj == null) {
            return false;
        }

        return obj.RequestId == _requestId;
    }

    public override bool Equals(object obj) {

        if (obj == null && obj is HMAutoincrementedRequestId) {
            return false;
        }

        return ((HMAutoincrementedRequestId)obj).RequestId == _requestId;
    }

    public override int GetHashCode() {

        return (int)(_requestId % Int32.MaxValue);
    }
}
