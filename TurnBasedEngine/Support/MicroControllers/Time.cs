using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeUtil {
    public static ulong UTCNow() {
        return (ulong)(System.DateTime.Now - System.DateTime.UnixEpoch).TotalSeconds;
    }
}