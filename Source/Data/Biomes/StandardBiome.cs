using Infiniverse.Data.Chunks;
using Infiniverse.Extensions;
using Infiniverse.Helpers;
using UnityEngine;

namespace Infiniverse.Data.Biomes;

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