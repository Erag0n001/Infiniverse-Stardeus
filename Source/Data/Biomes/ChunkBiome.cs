using System.Collections.Generic;
using System.Linq;
using Infiniverse.Data.Chunks;
using Infiniverse.Helpers;
using Infiniverse.Systems;
using UnityEngine;

namespace Infiniverse.Data.Biomes;

public abstract class ChunkBiome
{
    public static readonly Dictionary<string, ChunkBiome> AllBiomes = new Dictionary<string, ChunkBiome>();
    public abstract string Id { get; }

    public virtual int MaxRegionPerChunk => 8;

    public virtual int MinRegionPerChunk => 3;

    public virtual float BiomeMinDifficulty => 0;

    public virtual float BiomeMaxDifficulty => 1;
    
    public Vector2Int BiomeSizeInChunks = Vector2Int.one;

    public abstract Chunk Generate(Vector2Int position);

    protected void RegisterChunk(Chunk chunk)
    {
        ChunkSys.OnChunkGeneratedSignal.Send(chunk);
    }

    public static Chunk GenerateAt(Vector2Int position)
    {
        var chunkPos = ChunkHelper.GetNearestChunkPositionFloored(position);
        var rng = ChunkHelper.GetRngFromPosition(position);
        var index = rng.Range(0, AllBiomes.Count);
        var biome = AllBiomes.Values.ToArray()[index];
        return biome.Generate(chunkPos);
    }
    
    public static bool TryGetBiome(string id, out ChunkBiome biome)
    {
        return AllBiomes.TryGetValue(id, out biome);
    }
    
    protected static void Register(ChunkBiome biome)
    {
        AllBiomes[biome.Id] = biome;
    }

    public override string ToString()
    {
        return Id;
    }
}