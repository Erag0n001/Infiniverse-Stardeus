using System.Reflection;
using Game.Systems.Space;
using HarmonyLib;

namespace Infiniverse.Helpers;

public static class BiomeHelper
{
    public static GenSpaceBiome BiomeGen = new GenSpaceBiome();
    public static readonly FieldInfo UnassignedSectorTemplates = AccessTools.Field(typeof(GenSpaceBiome), "unassignedSectorTemplates");
}