using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Commands;
using Game.Data;
using Game.Systems;
using Infiniverse.Data.Biomes;
using Infiniverse.Data.Chunks;
using Infiniverse.Helpers;
using Infiniverse.Misc;
using KL.Signals;
using KL.Utils;
using MessagePack;
using UnityEngine;

namespace Infiniverse.Systems;

public class ChunkSys : GameSystem, ISaveableSpecial
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Register()
    {
        GameSystems.Register("ChunkSys", () => new ChunkSys());
    }
    
    public override string Id => "ChunkSys";
    
    private const string ChunksSaveLabel = "AllChunksData";
    public static Dictionary<Vector2Int, Chunk> AllChunks = new Dictionary<Vector2Int, Chunk>();
    public static HashSet<int> UsedRegionNames = new HashSet<int>();
    public static ChunkSys Instance;
    public static Signal1<Chunk> OnChunkGeneratedSignal = new Signal1<Chunk>("OnChunkGenerated", isSystem: true);
    public static int NextSpaceObjectId => ++ CurrentId;

    private const string CurrentIdSaveLabel = "CurrentIdData";
    private static int CurrentId;
    
    protected override void OnInitialize()
    {
        Printer.Warn($"Initializing ChunkSys");
        Instance = this;
        OnChunkGeneratedSignal.AddListener(OnChunkGenerated);
    }
    
    public override void Unload()
    {
        AllChunks.Clear();
        Instance = null;
    }

    public void SaveSpecial(SystemsDataSpecial sd)
    {
        sd.ModData = new Dictionary<string, byte[]>();
        byte[] allChunksData = MessagePackSerializer.Serialize(AllChunks.Values, CmdSaveGame.MsgPackOptions);
        sd.ModData.Add(ChunksSaveLabel, allChunksData);
        byte[] currentIdData = BitConverter.GetBytes(CurrentId);
        sd.ModData.Add(CurrentIdSaveLabel, currentIdData);
    }

    public void LoadSpecial(SystemsDataSpecial sd)
    {
        Printer.Warn("Loading ChunkSys");
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (sd.ModData == null || !sd.ModData.ContainsKey(ChunksSaveLabel))
        {
            Printer.Error($"Failed to fetch save data for the current universe, attempting to convert universe...");
            ConvertCurrentUniverseToChunks();
            return;
        }
        
        AllChunks = MessagePackSerializer.Deserialize<Dictionary<Vector2Int, Chunk>>(sd.ModData[ChunksSaveLabel]);
        CurrentId = BitConverter.ToInt32(sd.ModData[CurrentIdSaveLabel]);
    }

    public static Chunk GenerateChunkAt(Vector2Int targetPos)
    {
        var chunkPos = targetPos;
        if (!ChunkHelper.EnsureIsChunkLocation(targetPos))
        {
            chunkPos = ChunkHelper.GetNearestChunkPositionFloored(targetPos);
            Printer.Warn($"Tried to generate a chunk at {targetPos}, but it's not aligned to grid! Fixed to {chunkPos}");
        }

        if (AllChunks.TryGetValue(chunkPos, out var at))
        {
            Printer.Error($"Tried to generate a chunk at {chunkPos}, but a chunk already exists there!");
            return at;
        }
        
        return ChunkBiome.GenerateAt(chunkPos);
    }

    private void OnChunkGenerated(Chunk chunk)
    {
        Printer.Warn($"Generated chunk {chunk}");
        if (A.Starmap != null)
        {
            A.Starmap.PrepareForRebuild("Added chunk");
            S.Sig.RebuildStarmap.Send(parameter: true, parameter2: true, "Added chunk");
        }
    }
    
    private void ConvertCurrentUniverseToChunks()
    {
        if (AllChunks.Any())
        {
            Printer.Error($"Tried to convert the universe to chunks, but some chunks already exist");
            return;
        }

        var currentMap = S.Query.GetSpaceMap.Ask();
        CurrentId = currentMap.Regions.Count;
        foreach (var region in currentMap.Regions)
        {
            CurrentId = Math.Max(CurrentId, region.Id);
            UsedRegionNames.Add(Hashes.S(region.Name));
            foreach (var sector in region.Sectors)
            {
                CurrentId = Math.Max(CurrentId, sector.Id);
            }
        }

        Printer.Warn($"Found id {CurrentId} on existing universe");
        
        foreach (var region in currentMap.Regions)
        {
            var chunkPos = ChunkHelper.GetNearestChunkPositionFloored(region.Position);
            if (!AllChunks.TryGetValue(chunkPos, out var chunk))
            {
                chunk = new Chunk(chunkPos);
                AllChunks.Add(chunkPos, chunk);
            }       
            chunk.Regions.Add(region);
        }
        
        Printer.Warn($"All done converting universe. Chunk count:{AllChunks.Count}");
    }

    internal static void UpdateId(int id)
    {
        CurrentId = id;
    }
}