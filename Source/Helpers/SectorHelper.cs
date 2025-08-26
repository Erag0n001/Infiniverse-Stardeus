using System;
using System.Collections.Generic;
using System.Reflection;
using Game;
using Game.CodeGen;
using Game.Constants;
using Game.Data.Space;
using Game.Procgen.Space;
using Game.Systems.Naming;
using Game.Systems.Space;
using HarmonyLib;
using Kl.Algos;
using KL.Randomness;
using Multiplayer.Extensions;
using Multiplayer.Misc;
using Multiplayer.Systems;
using UnityEngine;

namespace Multiplayer.Helpers;

public static class SectorHelper
{
    private static SpaceObjectType SecType = SpaceObjectType.Find(SpaceObjectTypeIdH.Sector);
    private static GenSpaceObject SecGen = GenUniverse.FindGenerator(SecType.Id);
    private static readonly MethodInfo CreateSectorFor = AccessTools.Method(typeof(GenSpace), "CreateSectorsFor");
    private static GenSpace Gen = new GenSpace(A.S, A.S.Seed);
    private static readonly Vector2 RegionSize = new Vector2(15f, 15f);
    public static void GenerateSectorForRegion(SpaceRegion region)
    {
        region.Sectors = new List<SpaceSector>();
            
        int currentId = (int)CreateSectorFor.Invoke(Gen, new object[] { ChunkSys.NextSpaceObjectId, region, Rng.Unseeded, new HashSet<int>() });
        ChunkSys.UpdateId(currentId);
        
        foreach (var sector in region.Sectors)
        {
            sector.SO = SecGen.Generate(A.S.Universe, UniverseLayer.Sector, SecType, region.SO);
            sector.SO.Size = GenUniverse.SectorSize;
            sector.SO.Name = sector.Name;
            sector.SO.Sector = sector;
            sector.SO.Location = sector.Position;
            region.SO.AddChild(sector.SO);
        }
    }

    public static void GenerateSectorsForRegionSpecific(SpaceRegion region, int amount, Rng rng)
    {
        region.Sectors = new List<SpaceSector>();
        float minRadius = rng.Range(1f, 2f);
        float maxRadius = rng.Range(5f, 8f);
        List<Vector2> list = new List<Vector2>();
        var attemps = 0;
        while (list.Count < amount && attemps < 200)
        {
            list = PoissonSpajusDiscSampling.GeneratePoints(minRadius, maxRadius, RegionSize, rng, amount);
            attemps++;
        }

        if (attemps == 200)
        {
            Printer.Warn($"Failed to generate {amount} sectors in a single region, consider lowering the amount! Fallback to {list.Count}");
            amount = list.Count;
        }
        for (int i = 0; i < amount; i++)
        {
            SpaceSector spaceSector = new SpaceSector();
            spaceSector.Seed = rng.Int;
            spaceSector.Id = ChunkSys.NextSpaceObjectId;
            spaceSector.Region = region;
            spaceSector.Position = list[i] - RegionSize * 0.5f;
            spaceSector.Biome = new SpaceBiome();
            spaceSector.Biome.Type = SpaceBiomeType.Sector;
            spaceSector.Biome.Difficulty = -1f;
            NameResult nameResult = A.S.Query.GenerateNameExcept.Ask(NameRequest.ForSpaceMap(SpaceMapObjectType.Sector, rng), null);
            spaceSector.Name = nameResult.Name;
            region.Sectors.Add(spaceSector);  
            
            spaceSector.SO = SecGen.Generate(A.S.Universe, UniverseLayer.Sector, SecType, region.SO);
            spaceSector.SO.Size = GenUniverse.SectorSize;
            spaceSector.SO.Name = spaceSector.Name;
            spaceSector.SO.Sector = spaceSector;
            spaceSector.SO.Location = spaceSector.Position;
            region.SO.AddChild(spaceSector.SO);
        }
    }
    
    public static void GenerateBiomesForSector(SpaceRegion region, Rng rng)
    {
        BiomeHelper.BiomeGen.AssignForcedSectorTemplatesFor(region, rng.Fork());
        BiomeHelper.BiomeGen.AssignSectorDepositsFor(region, rng.Fork());
        BiomeHelper.BiomeGen.AssignSectorPOIsFor(region, rng.Fork());
        BiomeHelper.BiomeGen.CleanupTemplate(region);
        BiomeHelper.BiomeGen.AssignSectorTemplatesFor(region, rng.Fork());
        BiomeHelper.BiomeGen.AssignSectorEffectsFor(region, rng.Fork());
        BiomeHelper.BiomeGen.CleanupTemplate(region);
        BiomeHelper.BiomeGen.AssignUnassignedSectorTemplates(region.Sectors, rng.Fork(), false);
        BiomeHelper.BiomeGen.CleanupUnavailableTemplates(region);
        if (((List<SectorTemplate>)BiomeHelper.UnassignedSectorTemplates.GetValue(BiomeHelper.BiomeGen)).Count > 0)
        {
            BiomeHelper.BiomeGen.AssignUnassignedSectorTemplates(region.Sectors, rng.Fork(), true);
        }
        BiomeHelper.BiomeGen.ValidateSectorsFor(region);
    }
}