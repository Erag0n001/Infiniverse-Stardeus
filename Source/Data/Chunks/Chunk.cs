using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using Game;
using Game.Systems.Space;
using KL.Randomness;
using Multiplayer.Data.Biomes;
using Multiplayer.Helpers;
using Multiplayer.Systems;
using UnityEngine;

namespace Multiplayer.Data.Chunks;

public class Chunk(Vector2Int position)
{
    public const int ChunkSize = 5;
    public List<SpaceRegion> Regions = new List<SpaceRegion>();
    public ChunkBiome Biome;
    private Rng Random;

    public Rng Rng
    {
        get
        {
            if (Random == null)
            {
                Random = ChunkHelper.GetRngFromPosition(position);
            }
            return Random;
        }
    }
    
    public Vector2Int Position = position;

    public Vector2 Middle => Position + new Vector2(ChunkSize / 2f, ChunkSize / 2f);
    
    public SpaceRegion GetRegion(Direction xDirection, Direction yDirection)
    {
        return Regions
            .OrderBy(region => xDirection == Direction.Left ? -region.Position.x : region.Position.x)
            .ThenBy(region => yDirection == Direction.Down ? -region.Position.y : region.Position.y)
            .FirstOrDefault();
    }
    
    public override string ToString()
    {
        return $"Chunk|Pos:{position}|Biome:{Biome}|Regions:{Regions.Count}";
    }
}