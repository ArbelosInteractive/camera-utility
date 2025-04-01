using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arbelos.CameraUtility.Runtime
{
    public enum CinemachineStateType
    {
        Original,
        DollyPath,
        StaticZoom,
        FollowZoom,
        Shake,
        AutoPan
    }
    public enum ZoomDirection
    {
        North,
        South,
        East,
        West
    }
}