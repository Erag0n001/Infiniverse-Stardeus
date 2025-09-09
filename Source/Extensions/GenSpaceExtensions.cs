using System.Collections.Generic;
using System.Reflection;
using Game.Constants;
using Game.Systems.Space;
using HarmonyLib;
using KL.Randomness;

namespace Infiniverse.Extensions;

public static class GenSpaceExtensions
{
    private static readonly MethodInfo GenerateSectorsMethod = AccessTools.Method(typeof(GenSpace), "GenerateLinks").MakeGenericMethod(typeof(SpaceSector));

    public static void GenerateLinksForSector(this GenSpace genSpace, string ctx, Rng rng, List<SpaceSector> linkables, HashSet<SpaceLink> links,
        int connectFrom, int connectTo)
    {
        GenerateSectorsMethod.Invoke(genSpace, [ctx, rng, linkables, links, connectFrom, connectTo]);
    }
}