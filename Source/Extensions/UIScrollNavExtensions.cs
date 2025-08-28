using System.Reflection;
using Game.UI;
using HarmonyLib;
using UnityEngine;

namespace Infiniverse.Extensions;

public static class UIScrollNavExtensions
{
    private static readonly FieldInfo Target = AccessTools.Field(typeof(UIScrollNav), "target");
    public static RectTransform GetTarget(this UIScrollNav scrollNav)
    {
        return (RectTransform)Target.GetValue(scrollNav);
    }
}