using System.Collections.Generic;
using Game.UI;
using HarmonyLib;
using Infiniverse.Helpers;
using Infiniverse.Misc;
using UnityEngine;

namespace Infiniverse.Patches;

public static class UIScrollNavPatches
{
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
            Printer.Warn(target.localPosition);
            var x = Mathf.Clamp(target.localPosition.x, Common.UniverseMinimum.x, Common.UniverseMaximum.x);
            var y = Mathf.Clamp(target.localPosition.y, Common.UniverseMinimum.y, Common.UniverseMaximum.y);
            target.localPosition = new Vector2(x, y);
        }
    }
}