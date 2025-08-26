using System;
using System.Linq;
using Game.Systems.Space;
using KL.Randomness;
using Multiplayer.Data.Chunks;
using Multiplayer.Extensions;
using Multiplayer.Helpers;
using Multiplayer.Misc;
using Multiplayer.Systems;
using UnityEngine;

namespace Multiplayer.Data.Biomes;

public class OasisBiome : ChunkBiome
{
    public override string Id => "OasisBiome";
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Register()
    {
        ChunkBiome.Register(new OasisBiome());
    }
    
    public override int MinRegionPerChunk => 6;

    public override Chunk Generate(Vector2Int position)
    {
        var pos = ChunkHelper.GetNearestChunkPositionFloored(position);

        Chunk chunk = new Chunk(pos);
        var oasisRegion = RegionHelper.CreateRegionEmpty(chunk.Middle, chunk.Rng.Fork(), false);
        oasisRegion.Biome.Difficulty = chunk.Rng.Range(0, 0.10f);
        oasisRegion.Biome.SetTemplate(RegionTemplate.Get("UnclaimedStarSystem"));
        SectorHelper.GenerateSectorForRegion(oasisRegion);
        BiomeHelper.BiomeGen.RemapSectorDifficulty(oasisRegion, chunk.Rng.Fork());
        SectorHelper.GenerateBiomesForSector(oasisRegion, chunk.Rng.Fork());
        CreateRings(chunk, oasisRegion);
        chunk.Regions.Add(oasisRegion);
        return chunk;
    }

    private void CreateRings(Chunk chunk, SpaceRegion oasisRegion)
    {
        var amount = chunk.Rng.Range(MinRegionPerChunk, MinRegionPerChunk);
        for (int i = 0; i < amount; i++)
        {
            float angle = i * 2 * MathF.PI / amount; 
            float x = chunk.Middle.x + 1 * MathF.Cos(angle);
            float y = chunk.Middle.y  + 1 * MathF.Sin(angle);
            var regionPos = new Vector2(x, y);
            var region = RegionHelper.CreateRegionEmpty(regionPos, chunk.Rng.Fork(), false);
            region.Biome.Difficulty = chunk.Rng.Range(0.90f, 1f);
            region.Biome.Type = SpaceBiomeType.Region;
            region.Biome.SetTemplate(RegionTemplate.Get("VirusBase"));
            SectorHelper.GenerateSectorsForRegionSpecific(region, chunk.Rng.Range(4, 6), chunk.Rng.Fork());
            BiomeHelper.BiomeGen.RemapSectorDifficulty(region, chunk.Rng.Fork());
            SectorHelper.GenerateBiomesForSector(region, chunk.Rng.Fork());
            
            chunk.Regions.Add(region);
        }

        bool generatedLink = false;
        for (int i = 0; i < amount; i++)
        {
            if (i + 1 < amount)
            {
                SpaceLink link = new SpaceLink(chunk.Regions[i], chunk.Regions[i + 1]);
                chunk.Regions[i].AddLink(link);
                chunk.Regions[i + 1].AddLink(link);
            }

            if (chunk.Rng.Chance(0.5f))
            {
                SpaceLink linkToOasis = new SpaceLink(oasisRegion, chunk.Regions[i]);
                chunk.Regions[i].AddLink(linkToOasis);
                oasisRegion.AddLink(linkToOasis);
                generatedLink = true;
            }
        }

        if (!generatedLink)
        {
            var index = chunk.Rng.Range(0, amount + 1);
            SpaceLink linkToOasis = new SpaceLink(oasisRegion, chunk.Regions[index]);
            chunk.Regions[index].AddLink(linkToOasis);
            oasisRegion.AddLink(linkToOasis);
        }
        
        SpaceLink lastLink = new  SpaceLink(chunk.Regions.First(), chunk.Regions.Last());
        chunk.Regions.First().AddLink(lastLink);
        chunk.Regions.Last().AddLink(lastLink);
    }
}