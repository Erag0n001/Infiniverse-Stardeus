using System.Collections.Generic;
using System.Reflection;
using Game;
using Game.Systems.Space;
using HarmonyLib;
using Multiplayer.Commands;
using Multiplayer.Extensions;

namespace Multiplayer.Helpers;

public static class BiomeHelper
{
    public static GenSpaceBiome BiomeGen = new GenSpaceBiome();
    public static readonly FieldInfo UnassignedSectorTemplates = AccessTools.Field(typeof(GenSpaceBiome), "unassignedSectorTemplates");
}