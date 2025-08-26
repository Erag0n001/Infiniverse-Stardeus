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
using Multiplayer.Helpers;
using Multiplayer.Misc;
using Multiplayer.Systems;
using UnityEngine;

namespace Multiplayer.Data.Biomes;

/// <summary>
/// This biome basically mimics vanilla, and should not be used! It's only as a last resort.
/// </summary>
public class StandardBiome : ChunkBiome
{
    public override string Id => "Standard";
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    // private static void Register()
    // {
    //     ChunkBiome.Register(new StandardBiome());
    // }

    public override Chunk Generate(Vector2Int position)
    {
        var pos = ChunkHelper.GetNearestChunkPositionFloored(position);
        Chunk chunk = new Chunk(pos);
        chunk.Biome = this;

        var rng = ChunkHelper.GetRngFromPosition(position);
        int regionCount = rng.Range(MinRegionPerChunk, MaxRegionPerChunk);
        for (int i = 0; i < regionCount; i++)
        {
            var regionPos = RegionHelper.GetValidRegionLocation(chunk, rng);
            var region = RegionHelper.CreateRegionEmpty(regionPos, rng.Fork());
            
            RegionHelper.GenerateRegionBiome(region, rng.Fork());
            BiomeHelper.BiomeGen.RemapSectorDifficulty(region, rng.Fork());
            SectorHelper.GenerateBiomesForSector(region, rng.Fork());

            chunk.Regions.Add(region);
        }

        foreach (var region in chunk.Regions)
        {
            RegionHelper.SetupLinks(region, chunk.Rng.Fork());
        }
        
        return chunk;
    }
    
    
}