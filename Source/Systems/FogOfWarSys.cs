using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Commands;
using Game.Data;
using Game.Data.Space;
using Game.Systems;
using Game.Systems.Space;
using Game.UI;
using Infiniverse.Extensions;
using Infiniverse.Helpers;
using Infiniverse.Misc;
using MessagePack;
using UnityEngine;
using UnityEngine.UI;

namespace Infiniverse.Systems;

public class FogOfWarSys : GameSystem, ISaveableSpecial
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Register()
    {
        GameSystems.Register(ID, () => new FogOfWarSys());
    }

    
    private static readonly int CircleCountProp = Shader.PropertyToID("_CircleCount");
    private static readonly int CirclesProp = Shader.PropertyToID("_Circles");

    public enum DiscoverState : byte
    {
        None = 0,
        BarelyDiscovered = 1,
        SomewhatDiscovered = 2,
        FullyDiscovered = 3,
        StartingArea = 4,
        Debug = 255
    } 
    private const string VisitedRegionKey = "VisitedRegionsKey";
    private const string VisitedRegionValue = "VisitedRegionsValue";
    private const string ID = "Infiniverse.Systems.FogOfWarSystem";
    public override string Id => ID;
    public bool DebugRevealed = false;
    public bool Toggled => Instance.FogOfWarObject?.activeInHierarchy ?? false;
    
    public static FogOfWarSys Instance { get; private set; }
    
    private GameObject FogOfWarObject;
    private RectTransform FogOfWarRect;
    private RenderTexture Mask;
    
    public Dictionary<SpaceObject, DiscoverState> DiscoveredRegions = new Dictionary<SpaceObject, DiscoverState>();
    
    protected override void OnInitialize()
    {
        Printer.Warn($"Fog of war initializing!");
        Instance = this;
        S.Sig.HyperspaceLocationChanged.AddListener(UpdateTextureFromHyperSpaceMovement);
    }

    public override void Unload()
    {
        GameObject.Destroy(FogOfWarObject);
        Instance = null;
        S.Sig.HyperspaceLocationChanged.RemoveListener(UpdateTextureFromHyperSpaceMovement);
        DiscoveredRegions.Clear();
    }
    
    public void SaveSpecial(SystemsDataSpecial sd)
    {
        Printer.Warn($"Saving {Id}");
        var rawKeys = new byte[DiscoveredRegions.Keys.Count * sizeof(int)];
        Buffer.BlockCopy(DiscoveredRegions.Keys.Select(x => x.Id).ToArray(), 0,  rawKeys, 0,  rawKeys.Length);
        
        sd.ModData.Add(VisitedRegionKey, rawKeys);
        sd.ModData.Add(VisitedRegionValue, DiscoveredRegions.Values.Select(x => (byte)x).ToArray());
    }

    public void LoadSpecial(SystemsDataSpecial sd)
    {
        Printer.Warn($"Loading {Id}");
        if (!sd.ModData.ContainsKey(VisitedRegionKey))
        {
            UpdateTextureFromPlayerMovement();
            return;
        }
        DiscoveredRegions = new Dictionary<SpaceObject, DiscoverState>();
        ReadOnlySpan<byte> keys = sd.ModData[VisitedRegionKey];
        ReadOnlySpan<byte> values = sd.ModData[VisitedRegionValue];
        var length = keys.Length / sizeof(int);
        for (int i = 0; i < length; i++)
        {
            var id = BinaryPrimitives.ReadInt32LittleEndian(keys.Slice(i * sizeof(int), sizeof(int)));
            DiscoverState state = (DiscoverState)values[i];
            if (S.Universe.ObjectsById.TryGetValue(id, out var so))
            {
                DiscoveredRegions.Add(so, state);
            }
            else
            {
                Printer.Error($"Failed to fetch space object with id {id} in the universe post load!");
            }
        }
    }
    
    public void Toggle(bool value, string layer = "", DetailBlockStarmapWidget widget = null)
    {
        // Todo try to look into centerPos from the Starmap to position the fog of war
        if (string.IsNullOrWhiteSpace(layer))
            layer = A.Starmap.Layer;
        if (widget is null)
        {
            widget = A.Starmap;
        }
        if (value && layer != "Universe")
        {
            return;
        }
        if (FogOfWarObject is not null && FogOfWarObject)
        {
            FogOfWarObject.SetActive(value && !DebugRevealed);
            return;
        }
        if (value)
        {
            CreateGameObject(widget);
            UpdateTexture();
        }
    }

    private void CreateGameObject(DetailBlockStarmapWidget widget)
    {
        FogOfWarObject = new GameObject();
        FogOfWarObject.name = "FogOfWar";
            
        var scrollView = widget.transform.Find("Scroll View");
        var viewPort = scrollView.transform.Find("Viewport");
        var navBallObj = viewPort.transform.Find("Content");
            
        FogOfWarObject.transform.SetParent(navBallObj);
            
        var image = FogOfWarObject.AddComponent<RawImage>();
        image.material = Common.FogOfWarMaterial;

        Mask = new RenderTexture(4000, 4000, 0, RenderTextureFormat.ARGB32);
            
        image.texture = Mask;
            
        var transform = FogOfWarObject.GetComponent<RectTransform>();
        transform.position = Common.FogOfWarPos;
        transform.sizeDelta = Common.FogOfWarSize;
        FogOfWarRect = transform;
    }
    
    public UISpaceObject[] GetAllRegionsWithUIObjects()
    {
        return A.S.Query.GetSpaceMap.Ask().Regions.Where(x => x.SO.UI is not null).Select(x => x.SO.UI).ToArray();
    }
    
    public void UpdateTexture()
    {
        Vector4[] points = new  Vector4[128];
        int i = 0;
        foreach (var region in DiscoveredRegions)
        {
            try
            {
                if (TryGetUvCoordinatesFromRegionOntoFog(region.Key.UI, out var uv))
                {
                    points[i] = new Vector4(uv.x, uv.y, GetCircleSizeForDiscoverState(region.Value), 0);
                    i++;
                }
                else
                {
                    Printer.Error($"Failed to get texture coordinate for {region.Key} with uv pos {uv}");
                }
            }
            catch (Exception ex)
            {
                Printer.Error(ex.Message);
            }
        }
        
        var regions = GetAllRegionsWithUIObjects();
        
        foreach (var region in regions)
        {
            region.GetCanvasGroup().gameObject.SetActive(DebugRevealed);
        }
        
        foreach (var region in regions.Where(x => DiscoveredRegions.ContainsKey(x.SpaceObject)))
        {
            region.GetCanvasGroup().gameObject.SetActive(true);
        }
        
        RenderTexture.active = Mask;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;
        
        Common.CircleMakerMaterial.SetVectorArray(CirclesProp, points.ToArray());
        Common.CircleMakerMaterial.SetInt(CircleCountProp, i);
        
        Graphics.Blit(null, Mask, Common.CircleMakerMaterial);

        FogOfWarObject.SetActive(!DebugRevealed);
    }

    public void UpdateTextureFromHyperSpaceMovement(SpaceObject _, SpaceObject to)
    {
        UpdateTextureFromPlayerMovement(to);
    }
    
    public void UpdateTextureFromPlayerMovement(SpaceObject playerObj = null)
    {
        if(playerObj is null)
        {
            playerObj = A.Player;
        }
        
        if (S.Universe.Hyperspace == playerObj.Parent)
        {
            Printer.Warn($"Played tried updating his location while in hyperspace, trying to fallback...");
            playerObj = A.HyperspaceLocation;
        }

        SpaceRegion playerRegion;
        if (playerObj.Parent.SpaceEntity is SpaceRegion r)
        {
            playerRegion = r;
        }
        else
        {
            playerRegion = (SpaceRegion)playerObj.Parent.Parent.SpaceEntity;
        }

        if (DiscoveredRegions.TryGetValue(playerRegion.SO, out var playerState))
        {
            DiscoveredRegions[playerRegion.SO] = DiscoverState.FullyDiscovered;
        }
        else
        {
            DiscoveredRegions.Add(playerRegion.SO, DiscoverState.FullyDiscovered);
        }

        foreach (var link in playerRegion.Links)
        {
            SpaceObject child;
            if (link.To == playerRegion)
            {
                child = link.From.SO;
            }
            else
            {
                child = link.To.SO;
            }
            if (DiscoveredRegions.TryGetValue(child, out var childState))
            {
                if (childState < DiscoverState.SomewhatDiscovered)
                {
                    DiscoveredRegions[child] = childState;
                }
            }
            else
            {
                DiscoveredRegions.Add(child, DiscoverState.SomewhatDiscovered);
            }
        }
        
        if(FogOfWarObject is not null)
            UpdateTexture();
    }

    private bool TryGetUvCoordinatesFromRegionOntoFog(UISpaceObject region, out Vector2 result)
    {
        if (region is null)
        {
            Printer.Warn($"Passing a null region, cannot find uv!");
            result = Vector2.zero;
            return false;
        }

        Vector3 worldPos = region.gameObject.transform.position;
        Vector3 posInFog = FogOfWarRect.InverseTransformPoint(worldPos);
        Vector2 size = FogOfWarRect.rect.size;
        Vector2 pivot = FogOfWarRect.pivot;
        Vector2 bottomLeft = (Vector2)posInFog + Vector2.Scale(size, pivot);
        result = new Vector2(bottomLeft.x / size.x, bottomLeft.y / size.y);
        return true;
    }

    private static float GetCircleSizeForDiscoverState(DiscoverState state)
    {
        switch (state)
        {
            case DiscoverState.None : return 0f;
            case DiscoverState.BarelyDiscovered : return 0.002f;
            case DiscoverState.SomewhatDiscovered : return 0.005f;
            case DiscoverState.FullyDiscovered : return 0.01f;
            case DiscoverState.StartingArea : return 0.025f;
            case DiscoverState.Debug : return 1f;
        }

        throw new ArgumentOutOfRangeException();
    }

    public void Reset()
    {
        Toggle(false);
        DiscoveredRegions.Clear();
        Mask = new RenderTexture(4000, 4000, 0, RenderTextureFormat.ARGB32);
        var image = FogOfWarObject.GetComponent<RawImage>();
        image.texture = Mask;
        UpdateTextureFromPlayerMovement();
    }
}