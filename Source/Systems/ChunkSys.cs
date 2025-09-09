using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Commands;
using Game.Data;
using Game.Systems;
using Game.Systems.Space;
using Infiniverse.Data.Biomes;
using Infiniverse.Data.Chunks;
using Infiniverse.Data.Saveable;
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
    private const string ChunksSaveLabel = "AllChunksData";
    private const string CurrentIdSaveLabel = "CurrentIdData";
    
    public static ChunkSys Instance;
    
    public override string Id => "ChunkSys";
    
    public GenSpace GenSpace { get; private set; }
    public Dictionary<Vector2Int, Chunk> AllChunks = new Dictionary<Vector2Int, Chunk>();
    public HashSet<int> UsedRegionNames = new HashSet<int>();
    public Signal1<Chunk> OnChunkGeneratedSignal = new Signal1<Chunk>("OnChunkGenerated", isSystem: true);
    
    public int NextSpaceObjectId => ++ CurrentId;
    private int CurrentId;
    
    protected override void OnInitialize()
    {
        Printer.Warn($"Initializing ChunkSys");
        Instance = this;
        OnChunkGeneratedSignal.AddListener(OnChunkGenerated);
        GenSpace = new GenSpace(S, S.Seed);
    }
    
    public override void Unload()
    {
        AllChunks.Clear();
        Instance = null;
    }

    public void SaveSpecial(SystemsDataSpecial sd)
    {
        Printer.Warn($"Saving {Id}");
        byte[] allChunksData = MessagePackSerializer.Serialize(AllChunks.Values.Select(ChunkData.Serialize), CmdSaveGame.MsgPackOptions);
        sd.ModData.Add(ChunksSaveLabel, allChunksData);
        byte[] currentIdData = BitConverter.GetBytes(CurrentId);
        sd.ModData.Add(CurrentIdSaveLabel, currentIdData);
    }

    public void LoadSpecial(SystemsDataSpecial sd)
    {
        Printer.Warn($"Loading {Id}");
        foreach (var str in sd.ModData.Keys)
        {
            Printer.Warn(str);
            Printer.Warn(sd.ModData[str]);
        }
        if (!sd.ModData.TryGetValue(ChunksSaveLabel, out var rawChunks))
        {
            Printer.Warn($"Failed to fetch save data for the current universe, attempting to convert universe...");
            ConvertCurrentUniverseToChunks();
            return;
        }
        
        var allChunks = MessagePackSerializer.Deserialize<ChunkData[]>(rawChunks, CmdSaveGame.MsgPackOptions).Select(x => x.Deserialize());
        foreach (var chunk in allChunks)
        {
            AllChunks.Add(chunk.Position, chunk);
        }
        
        CurrentId = BitConverter.ToInt32(sd.ModData[CurrentIdSaveLabel]);
    }

    public Chunk GenerateChunkAt(Vector2Int targetPos)
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
                chunk.Biome = ChunkBiome.AllBiomes["Standard"];
                AllChunks.Add(chunkPos, chunk);
            }       
            chunk.Regions.Add(region);
        }
        
        Printer.Warn($"All done converting universe. Chunk count:{AllChunks.Count}");
    }

    internal void UpdateId(int id)
    {
        CurrentId = id;
    }
}