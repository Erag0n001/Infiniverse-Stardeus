using System;
using KL.Utils;
using UnityEngine;

namespace Infiniverse.Helpers;

public static class Common
{
    public static Material FogOfWarMaterial;
    public static Material CircleMakerMaterial;
    public static Vector2 TopLeft = new Vector2(-128, 64);
    public static Vector2 BottomRight = new Vector2(128, -64);
    public static Vector2 FogOfWarPos => Vector2.zero;
    public static Vector2 FogOfWarSize => GetFogOfWarSize();

    private static Vector2 GetFogOfWarSize()
    {
        var size = Mathf.Abs(TopLeft.x) + MathF.Abs(BottomRight.x);
        return new Vector2(size, size);
    }
}