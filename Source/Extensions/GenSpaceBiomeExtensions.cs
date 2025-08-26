using System.Collections.Generic;
using System.Reflection;
using Game.Systems.Space;
using HarmonyLib;
using KL.Randomness;

namespace Multiplayer.Extensions;

public static class GenSpaceBiomeExtensions
{
    private static readonly MethodInfo AssignForcedSectorTemplatesForMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "AssignForcedSectorTemplatesFor");
    private static readonly MethodInfo AssignSectorDepositsForMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "AssignSectorDepositsFor");
    private static readonly MethodInfo AssignSectorPOIsForMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "AssignSectorPOIsFor");
    private static readonly MethodInfo CleanupTemplateMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "CleanupTemplate");
    private static readonly MethodInfo AssignSectorTemplatesForMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "AssignSectorTemplatesFor");
    private static readonly MethodInfo AssignSectorEffectsForMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "AssignSectorEffectsFor");
    private static readonly MethodInfo AssignUnassignedSectorTemplatesMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "AssignUnassignedSectorTemplates");
    private static readonly MethodInfo CleanupUnavailableTemplatesMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "CleanupUnavailableTemplates");
    private static readonly MethodInfo ValidateSectorsForMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "ValidateSectorsFor");
    private static readonly MethodInfo RemapSectorDifficultyMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "RemapSectorDifficulty");
    private static readonly MethodInfo GetNoiseAtMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "GetNoiseAt");
    private static readonly MethodInfo FindTemplateForRegionMethod = 
        AccessTools.Method(typeof(GenSpaceBiome), "FindTemplateFor").MakeGenericMethod(typeof(RegionTemplate));
    private static readonly FieldInfo Rng = AccessTools.Field(typeof(GenSpaceBiome), "rng");
    public static void AssignForcedSectorTemplatesFor(this GenSpaceBiome generator, SpaceRegion region, Rng rng)
    {
        AssignForcedSectorTemplatesForMethod.Invoke(generator, [region, rng]);
    }
    public static void AssignSectorDepositsFor(this GenSpaceBiome generator, SpaceRegion region, Rng rng)
    {
        AssignSectorDepositsForMethod.Invoke(generator, [region, rng]);
    }
    public static void AssignSectorPOIsFor(this GenSpaceBiome generator, SpaceRegion region, Rng rng)
    {
        AssignSectorPOIsForMethod.Invoke(generator, [region, rng]);
    }
    public static void CleanupTemplate(this GenSpaceBiome generator, SpaceRegion region)
    {
        CleanupTemplateMethod.Invoke(generator, [region]);
    }
    public static void AssignSectorTemplatesFor(this GenSpaceBiome generator, SpaceRegion region, Rng rng)
    {
        AssignSectorTemplatesForMethod.Invoke(generator, [region, rng]);
    }
    public static void AssignSectorEffectsFor(this GenSpaceBiome generator, SpaceRegion region, Rng rng)
    {
        AssignSectorEffectsForMethod.Invoke(generator, [region, rng]);
    }
    public static void AssignUnassignedSectorTemplates(this GenSpaceBiome generator, List<SpaceSector> sectors, Rng rng, bool final)
    {
        AssignUnassignedSectorTemplatesMethod.Invoke(generator, [sectors, rng, final]);
    }
    public static void CleanupUnavailableTemplates(this GenSpaceBiome generator, SpaceRegion region)
    {
        CleanupUnavailableTemplatesMethod.Invoke(generator, [region]);
    }
    public static void ValidateSectorsFor(this GenSpaceBiome generator, SpaceRegion region)
    {
        ValidateSectorsForMethod.Invoke(generator, [region]);
    }
    
    public static void RemapSectorDifficulty(this GenSpaceBiome generator, SpaceRegion region, Rng rng)
    {
        RemapSectorDifficultyMethod.Invoke(generator, [region, rng]);
    }

    public static void SetRng(this GenSpaceBiome generator, Rng rng)
    {
        Rng.SetValue(generator, rng);
    }

    public static float GetNoiseAt(this GenSpaceBiome generator, float x, float y, int genOpt, float lerpRes, float cellRes,
        float gradRes, int cellSeed, int gradSeed)
    {
        return (float)GetNoiseAtMethod.Invoke(generator, [x, y, genOpt, lerpRes, cellRes, gradRes, cellSeed, gradSeed]);
    }

    public static bool FindTemplateForRegion(this GenSpaceBiome generator, 
        SpaceRegion region, List<RegionTemplate> templates, Rng rng)
    {
        return (bool)FindTemplateForRegionMethod.Invoke(generator, [region, templates, rng]);
    }
}