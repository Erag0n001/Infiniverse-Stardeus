using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Systems.Space;
using Infiniverse.Data.Biomes;
using Infiniverse.Data.Chunks;
using Infiniverse.Misc;
using MessagePack;
using UnityEngine;

namespace Infiniverse.Data.Saveable;

[MessagePackObject]
public class ChunkData
{
    [Key(0)] public List<int> RegionIds = new List<int>();
    [Key(1)] public string BiomeId;
    [Key(2)] public Vector2Int Position;
    public static ChunkData Serialize(Chunk chunk)
    {
        return new ChunkData
        {
            RegionIds = chunk.Regions.Select(x => x.SO.Id).ToList(),
            BiomeId = chunk.Biome.Id,
            Position = chunk.Position
        };
    }

    public Chunk Deserialize()
    {
        List<SpaceRegion> regions = new List<SpaceRegion>();
        foreach (var regionId in RegionIds)
        {
            if(A.S.Universe.ObjectsById.TryGetValue(regionId, out var so))
            {
                var region = so.SpaceEntity as SpaceRegion;
                if (region == null)
                {
                    Printer.Error($"Found a space object with id {regionId} at chunk pos {Position}, but it was not a region. It was instead an {so.SpaceEntity?.GetType().FullName}");
                    continue;
                }
                regions.Add(region);
            }
            else
            {
                Printer.Error($"Failed to find a space object for chunk at {Position} with id {regionId}");
            }
        }

        if (!ChunkBiome.TryGetBiome(BiomeId, out var biome))
        {
            Printer.Error($"Failed to find a biome with {BiomeId} for chunk at {Position}. Defaulting to standard");
            biome = ChunkBiome.AllBiomes["Standard"];
        }

        return new Chunk(Position)
        {
            Regions = regions,
            Biome = biome
        };
    }
}