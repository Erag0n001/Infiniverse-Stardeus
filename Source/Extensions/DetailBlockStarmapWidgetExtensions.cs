using Game;
using Game.Data.Space;
using Game.UI;
using HarmonyLib;
using KL.Pool;
using Multiplayer.Misc;

namespace Multiplayer.Extensions;

public static class DetailBlockStarmapWidgetExtensions
{
    public static void RemoveObject(this DetailBlockStarmapWidget widget, SpaceObject obj)
    {
        if (widget.Instances.TryGetValue(obj, out var uiElement))
        {
            widget.Instances.Remove(obj);
            PrefabPool.Despawn(uiElement.gameObject);
            AccessTools.Method(typeof(DetailBlockStarmapWidget), "RebuildStarmap")
                .Invoke(widget, new object[] { false, false, $"{Printer.Prefix}Removing object: {obj}"});
        }
    }
}