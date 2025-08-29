using System;
using System.Collections.Generic;
using Game.UI;
using HarmonyLib;
using Infiniverse.Helpers;
using Infiniverse.Misc;
using UnityEngine;

namespace Infiniverse.Patches;

public static class UIScrollNavPatches
{
    public static RectTransform TopLeft;
    public static RectTransform BottomRight;
    [HarmonyPatch(typeof(UIScrollNav), "OnNav")]
    public static class OnNavPatch
    {
        [HarmonyPostfix]
        public static void Postfix(UIScrollNav __instance, RectTransform ___target)
        {
            SnapBackToUniverse(__instance, ___target);
        }
    }

    [HarmonyPatch(typeof(UIScrollNav), "OnDrag")]
    public static class OnDragPatch
    {
        [HarmonyPostfix]
        public static void Postfix(UIScrollNav __instance, RectTransform ___target)
        {
            SnapBackToUniverse(__instance, ___target);
        }
    }

    private static void SnapBackToUniverse(UIScrollNav uiScrollNav, RectTransform target)
    {
        // We only want to snap if this is the Universe
        if (uiScrollNav.gameObject.transform.parent.parent.parent.gameObject.GetComponent<DetailBlockStarmapWidget>() is not null)
        {
            Vector3 offset = new Vector3();
            if (TopLeft.position.x > 0)
            {
                offset.x += TopLeft.position.x * -1;
            }
            if (TopLeft.position.y < 0)
            {
                offset.y += TopLeft.position.y * -1;
            }

            if (BottomRight.position.x < 0)
            {
                offset.x += BottomRight.position.x * -1;
            }

            if (BottomRight.position.y > 0)
            {
                offset.y += BottomRight.position.y * -1;
            }
            
            target.position += offset;
        }
    }
}