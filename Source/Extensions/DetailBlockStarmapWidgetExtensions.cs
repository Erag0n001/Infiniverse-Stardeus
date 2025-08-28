using System.Reflection;
using Game.Data.Space;
using Game.UI;
using HarmonyLib;
using Infiniverse.Misc;
using KL.Pool;
using UnityEngine;

namespace Infiniverse.Extensions;

public static class DetailBlockStarmapWidgetExtensions
{
    private static readonly FieldInfo CenterPos = AccessTools.Field(typeof(DetailBlockStarmapWidget), "centerPos");
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

    public static Vector2 GetCenterPos(this DetailBlockStarmapWidget widget)
    {
        return (Vector2)CenterPos.GetValue(widget);
    }
}