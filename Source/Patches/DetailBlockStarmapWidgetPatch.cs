using Game.Data.Space;
using Game.Rendering;
using Game.UI;
using HarmonyLib;
using Infiniverse.Helpers;
using Infiniverse.Misc;
using Infiniverse.Systems;
using KL.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Infiniverse.Patches;

public static class DetailBlockStarmapWidgetPatch
{
    [HarmonyPatch(typeof(DetailBlockStarmapWidget), nameof(DetailBlockStarmapWidget.OnOpen))]
    public static class OnOpenPatch
    {
        [HarmonyPrefix]
        public static void Patch(DetailBlockStarmapWidget __instance, Vector2 ___centerPos)
        {
            FogOfWarSys.Instance.Toggle(true, __instance.Layer, __instance);
            if (UIScrollNavPatches.TopLeft is null)
            {
                var scrollView = __instance.transform.Find("Scroll View");
                var viewPort = scrollView.transform.Find("Viewport");
                var navBallObj = viewPort.transform.Find("Content");
                
                GameObject topLeft = new GameObject();
                UIScrollNavPatches.TopLeft = topLeft.AddComponent<RectTransform>();
                topLeft.AddComponent<RawImage>().texture = Texture2D.whiteTexture;
                topLeft.transform.SetParent(navBallObj, worldPositionStays: true);
                UIScrollNavPatches.TopLeft.sizeDelta = new Vector2(2f, 2f);
                UIScrollNavPatches.TopLeft.position = new Vector3(Common.TopLeft.x, Common.TopLeft.y);
                topLeft.name = "UniverseViewTopLeft";
                
                GameObject bottomRight = new GameObject();
                UIScrollNavPatches.BottomRight = bottomRight.AddComponent<RectTransform>();
                bottomRight.AddComponent<RawImage>().texture = Texture2D.whiteTexture;
                bottomRight.transform.SetParent(navBallObj, worldPositionStays: true);
                UIScrollNavPatches.BottomRight.sizeDelta = new Vector2(2f, 2f);
                UIScrollNavPatches.BottomRight.position = new Vector3(Common.BottomRight.x, Common.BottomRight.y);
                bottomRight.name = "UniverseViewBottomRight";
            }
        }
    }
    [HarmonyPatch(typeof(DetailBlockStarmapWidget), nameof(DetailBlockStarmapWidget.OnClose))]
    public static class OnClosePatch
    {
        [HarmonyPrefix]
        public static void Patch(DetailBlockStarmapWidget __instance)
        {
            Printer.Warn("Onclose");
            FogOfWarSys.Instance.Toggle(false, __instance.Layer, __instance);
        }
    }
}