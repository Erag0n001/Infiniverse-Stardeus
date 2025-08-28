using Game.UI;
using HarmonyLib;
using Infiniverse.Misc;
using Infiniverse.Systems;

namespace Infiniverse.Patches;

public static class DetailBlockStarmapWidgetPatch
{
    [HarmonyPatch(typeof(DetailBlockStarmapWidget), nameof(DetailBlockStarmapWidget.OnOpen))]
    public static class OnOpenPatch
    {
        [HarmonyPrefix]
        public static void Patch(DetailBlockStarmapWidget __instance)
        {
            FogOfWarHelper.Toggle(true, __instance.Layer, __instance);
        }
    }
    [HarmonyPatch(typeof(DetailBlockStarmapWidget), nameof(DetailBlockStarmapWidget.OnClose))]
    public static class OnClosePatch
    {
        [HarmonyPrefix]
        public static void Patch(DetailBlockStarmapWidget __instance)
        {
            Printer.Warn("Onclose");
            FogOfWarHelper.Toggle(false, __instance.Layer, __instance);
        }
    }

    [HarmonyPatch(typeof(DetailBlockStarmapWidget), nameof(DetailBlockStarmapWidget.OnBlockUpdate))]
    public static class OnBlockUpdatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(DetailBlockStarmapWidget __instance)
        {
            FogOfWarHelper.Toggle(true, __instance.Layer, __instance);
        }
    }
}