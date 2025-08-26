using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.CodeGen;
using Game.Constants;
using Game.Data;
using Game.Data.Factions;
using Game.Data.Space;
using Game.Procgen.Space;
using Game.Systems.Naming;
using Game.Systems.Space;
using KL.Randomness;
using KL.Utils;
using Multiplayer.Data.Chunks;
using Multiplayer.Extensions;
using Multiplayer.Misc;
using Multiplayer.Systems;
using UnityEngine;

namespace Multiplayer.Helpers;

public static class RegionHelper
{
    public static readonly SpaceObjectType RegType = SpaceObjectType.Find(SpaceObjectTypeIdH.Region);
    public static readonly GenSpaceObject RegGen = GenUniverse.FindGenerator(RegType.Id);
    public static readonly List<RegionTemplate> Templates = RegionTemplate.All
        .Where(x => !Arrays.IsEmpty(x.Value.DifficultyChance))
        .Select(x => x.Value)
        .ToList();
    
    public static Vector2 GetValidRegionLocation(Chunk chunk, Rng rng)
    {
        const float min = 0.5f;
        const float max = Chunk.ChunkSize - 0.5f;
        int tries = 0;
        while (tries < 100)
        {
            tries++;
            var regPosition = new Vector2(chunk.Position.x + rng.Range(min, max),
                chunk.Position.y + rng.Range(min, max));
            if(IsTooClose(regPosition))
                continue;
            return regPosition;
        }

        Printer.Error($"Failed to generate a valid region position in the chunk, fallback to center of chunk...");
        return new Vector2(chunk.Position.x - Chunk.ChunkSize / 2, chunk.Position.y - Chunk.ChunkSize / 2);
        
        bool IsTooClose(Vector2 pos) => chunk.Regions.Any(x=> Vectors.Distance(x.Position, pos) < 1);
    }
    
    public static SpaceRegion CreateRegionEmpty(Vector2 position, Rng rng, bool withSectors = true)
    {
        BiomeHelper.BiomeGen.SetRng(rng.Fork());
        
        var region = GenerateRegion(position, rng.Fork());
        GenerateRegionSO(region);
        if (withSectors)
        {
            SectorHelper.GenerateSectorForRegion(region);
        }
        
        return region;
    }
    
    public static void GenerateFactionsForRegion(SpaceRegion region, Rng rng)
    {
        if (region.Biome.Template.ForceFaction != null)
        {
            FactionDef factionDef = FactionDef.Get(region.Biome.Template.ForceFaction);
            if (factionDef == null)
            {
                D.Err("ForceFaction not available for SpaceRegionTemplate: {0} | {1}", region.Biome.Template.Id, region.Biome.Template.ForceFaction);
            }
            else
            {
                region.Biome.AddFaction(factionDef, 1f);
            }
            return;
        }
        int num = rng.IntCurve(region.Biome.Template.NumFactionsCurve);
        if (num < 1)
        {
            return;
        }
        Dictionary<FactionDef, float> weightedFactions = Caches.WeightedFactions;
        weightedFactions.Clear();
        foreach (FactionDef item in Caches.FactionList)
        {
            float num2 = item.RegionDifficultyCurve.Evaluate(region.Biome.Difficulty);
            if (num2 > 0f)
            {
                weightedFactions.Add(item, num2);
            }
        }
        for (int i = 0; i < num; i++)
        {
            if (!rng.TryWeightedFrom(weightedFactions, out var res))
            {
                break;
            }
            float dominance = weightedFactions[res];
            region.Biome.AddFaction(res, dominance);
            weightedFactions.Remove(res);
        }
    }
    
    public static SpaceRegion GenerateRegion(Vector2 position, Rng rng)
    {
        SpaceRegion region = new SpaceRegion();
        region.Id = ChunkSys.NextSpaceObjectId;
        region.Seed = (int)A.S.Rng.Uint;
        NameResult nameResult = A.S.Query.GenerateNameExcept.Ask(NameRequest.ForSpaceMap(SpaceMapObjectType.Region, rng), ChunkSys.UsedRegionNames);
        region.Name = nameResult.Name;
        region.Position = position;
        region.Map = A.S.Query.GetSpaceMap.Ask();
        region.Map.Regions.Add(region);
        region.Biome = new SpaceBiome();
        return region;
    }

    public static void GenerateRegionBiome(SpaceRegion region, Rng rng)
    {
        float lerpRes = rng.Range(7f, 13f);
        float gradRes = rng.Range(0.2f, 0.35f);
        float cellRes = rng.Range(0.1f, 0.25f);
        int @int = rng.Int;
        int int2 = rng.Int;
        int genOpt = 3;
        region.Biome = new SpaceBiome();
        region.Biome.Type = SpaceBiomeType.Region;
        var difficulty = 
            BiomeHelper.BiomeGen.GetNoiseAt(region.Position.x, region.Position.y, genOpt, lerpRes, cellRes, gradRes, @int, int2);
        difficulty = (difficulty + 1f) * 0.5f;
        region.Biome.Difficulty = difficulty;
        
        if (!BiomeHelper.BiomeGen.FindTemplateForRegion(region, RegionHelper.Templates, rng))
        {
            Printer.Warn($"Failed to find template for region {region}, assigning fallback...");
            region.Biome.SetTemplate(RegionTemplate.Get("UnclaimedStarSystem"));
        }
        
        GenerateFactionsForRegion(region, A.S.Rng);
    }
    public static void GenerateRegionSO(SpaceRegion region)
    {
        var spaceObject = RegionHelper.RegGen.Generate(A.S.Universe, UniverseLayer.Region, RegionHelper.RegType, region.Map.SO);
        spaceObject.Region = region;
        spaceObject.Location = region.Position;
        spaceObject.Size = GenUniverse.RegionSize;
        spaceObject.Name = region.Name;
        region.SO = spaceObject;
        region.Map.SO.AddChild(spaceObject);
    }
    
    public static SpaceRegion[] GetNearestRegions(Vector2 position, int amountOfRegionsToGet, List<SpaceRegion> allRegions)
    {
        var distances = new List<(float distance, SpaceRegion region)>(allRegions.Count);
        foreach (var region in allRegions)
            distances.Add((Vectors.SqrDistance(region.Position, position), region));
            
        distances.Sort((a, b) => a.distance.CompareTo(b.distance));
            
        var nearest = new SpaceRegion[amountOfRegionsToGet];
        for (int i = 0; i < amountOfRegionsToGet && i < distances.Count; i++)
            nearest[i] = distances[i].region;
        return nearest;
    }

    public static void SetupLinks(SpaceRegion region, Rng rng)
    {
        SpaceMap spaceMap = A.S.Query.GetSpaceMap.Ask();
        List<SpaceRegion> allRegions = spaceMap.Regions;
        allRegions.Remove(region);
        
        var linkCount = rng.Range(1, 3);
        var links = GetNearestRegions(region.Position, linkCount, allRegions);
        for (int i = 0; i < linkCount; i++)
        {
            var link = new SpaceLink(region, links[i]);
            if(links[i].Links.Any(x => Equals(x, link)))
                continue;
            region.AddLink(link);
            links[i].AddLink(link);
        }
        
        allRegions.Add(region);
    }
}