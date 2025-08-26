using System;
using System.Runtime.CompilerServices;
using Game;
using KL.Randomness;
using Multiplayer.Data.Biomes;
using Multiplayer.Data.Chunks;
using Multiplayer.Misc;
using Multiplayer.Systems;
using UnityEngine;

namespace Multiplayer.Helpers;

public static class ChunkHelper
{
    public static Rng GetRngFromPosition(Vector2Int position)
    {
        return new Rng(position.GetHashCode() + A.S.Seed);
    }

    public static Vector2Int GetNearestChunkPositionFloored(Vector2 position)
    {
        int x = (int)Math.Floor(position.x / (double)Chunk.ChunkSize);
        int y = (int)Math.Floor(position.y / (double)Chunk.ChunkSize);
        
        return new Vector2Int(x * Chunk.ChunkSize, y * Chunk.ChunkSize);
    }

    public static Vector2Int GetNearestChunkPositionFloored(Vector2Int position)
    {
        int x = (int)Math.Floor(position.x / (double)Chunk.ChunkSize);
        int y = (int)Math.Floor(position.y / (double)Chunk.ChunkSize);
        
        return new Vector2Int(x * Chunk.ChunkSize, y * Chunk.ChunkSize);
    }
    
    public static bool EnsureIsChunkLocation(Vector2Int position)
    {
        return position.x % Chunk.ChunkSize == 0 && position.y % Chunk.ChunkSize == 0;
    }
    
    /// <summary>
    /// Takes in a chunk position on grid
    /// </summary>
    public static bool ChunkFitAt(Vector2Int position, ChunkBiome biome)
    {
        if (!ChunkHelper.EnsureIsChunkLocation(position))
        {
            return false;
        }

        for (int x = 0; x < biome.BiomeSizeInChunks.x; x++)
        {
            for (int y = 0; y < biome.BiomeSizeInChunks.y; y++)
            {
                var chunkPos = position + new Vector2Int(x * Chunk.ChunkSize, y * Chunk.ChunkSize);
                if (ChunkSys.AllChunks.ContainsKey(chunkPos))
                    return false;
            }
        }
        
        return true;
    }
}