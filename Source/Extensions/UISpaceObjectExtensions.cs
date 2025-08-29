using System.Reflection;
using Game.UI;
using HarmonyLib;
using UnityEngine;

namespace Infiniverse.Extensions;

public static class UISpaceObjectExtensions
{
    private static readonly FieldInfo GetCanvasField = AccessTools.Field(typeof(UISpaceObject), "tooltipCg");
    public static CanvasGroup GetCanvasGroup(this UISpaceObject obj)
    {
        return (CanvasGroup)GetCanvasField.GetValue(obj);
    }
}