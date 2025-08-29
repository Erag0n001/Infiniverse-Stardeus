using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Data;
using Game.Data.Space;
using Game.Systems;
using Game.Systems.Space;
using Game.UI;
using Infiniverse.Extensions;
using Infiniverse.Helpers;
using Infiniverse.Misc;
using UnityEngine;
using UnityEngine.UI;

namespace Infiniverse.Systems;

public class FogOfWarSys : GameSystem, ISaveableSpecial
{
    private const string VisitedRegionKey = "VisitedRegions";
    public override string Id => "Infiniverse.Systems.FogOfWarSystem";
    
    public static bool Toggled => Instance.FogOfWarObject?.activeInHierarchy ?? false;
    
    public static FogOfWarSys Instance { get; private set; }
    
    private GameObject FogOfWarObject;
    private RectTransform FogOfWarRect;
    private RenderTexture Mask;
    
    private HashSet<SpaceObject> RevealedObjects = new HashSet<SpaceObject>();
    
    protected override void OnInitialize()
    {
        Instance = this;
        S.Sig.HyperspaceLocationChanged.AddListener(UpdateTextureFromHyperSpaceMovement);
    }

    public override void Unload()
    {
        GameObject.Destroy(FogOfWarObject);
        Instance = null;
        S.Sig.HyperspaceLocationChanged.RemoveListener(UpdateTextureFromHyperSpaceMovement);
    }
    
    public void SaveSpecial(SystemsDataSpecial sd)
    {
        sd.ModData = new Dictionary<string, byte[]>();
        List<byte> data = new List<byte>();
        foreach (var region in RevealedObjects)
        {
            data.AddRange(BitConverter.GetBytes(region.Id));
        }
        sd.ModData.Add(VisitedRegionKey, data.ToArray());
    }

    public void LoadSpecial(SystemsDataSpecial sd)
    {
        if (sd.ModData == null)
        {
            // Added to save maybe?
            // todo get the player object and such
            return;
        }
        Span<byte> data = sd.ModData[VisitedRegionKey];
        var length = data.Length - 4;
        for (int i = 0; i < length; i += 4)
        {
            var id = BitConverter.ToInt32(data.Slice(i, 4));
            
            if (S.Universe.ObjectsById.TryGetValue(id, out var obj))
            {
                RevealedObjects.Add(obj);
            }
            else
            {
                Printer.Error($"Tried to load a SpaceObject with id {id}, but it did not exist.");
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
            FogOfWarObject.SetActive(value);
            return;
        }
        if (value)
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
            
            Common.CircleMakerMaterial.SetVectorArray("_Circles", new Vector4[128]);
            Common.CircleMakerMaterial.SetInt("_CircleCount", 128);
            
        }
    }
    
    private static UISpaceObject[] GetAllRegionsWithUIObjects()
    {
        return A.S.Query.GetSpaceMap.Ask().Regions.Where(x => x.SO.UI is not null).Select(x => x.SO.UI).ToArray();
    }
    
    private void UpdateTexture(List<Vector4> points)
    {
        if (points.Count > 128)
        {
            Printer.Warn($"Trying to discover too many circles at once, trimming down to 128!");
            while (points.Count > 128)
            {
                points.Remove(points.Last());
            }
        }
        
        RenderTexture.active = Mask;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;
        
        Common.CircleMakerMaterial.SetVectorArray("_Circles", points.ToArray());
        Common.CircleMakerMaterial.SetInt("_CircleCount", points.Count);
        
        Graphics.Blit(null, Mask, Common.CircleMakerMaterial);
    }

    public void UpdateTextureFromHyperSpaceMovement(SpaceObject _, SpaceObject to)
    {
        UpdateTextureFromPlayerMovement(to);
    }
    
    public void UpdateTextureFromPlayerMovement(SpaceObject playerObj = null)
    {
        if(playerObj is null) 
            playerObj = A.Player;
        Printer.Warn($"Played moved to {playerObj.Parent}, recalculating FogOfWar");
        if (A.HyperspaceLocation == playerObj.Parent)
        {
            return;
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
        
        List<Vector4> circles = new List<Vector4>();
        if (TryGetUvCoordinatesFromRegionOntoFog(playerRegion.SO.UI, out var uv))
        {
            circles.Add(new Vector4(uv.x, uv.y, 0.01f, 0));
            playerRegion.SO.UI.GetCanvasGroup().alpha = 1f;
        }
        
        RevealedObjects.Add(playerRegion.SO);
        
        
        foreach (var link in playerRegion.Links)
        {
            Printer.Warn(link);

            UISpaceObject child;
            if (link.To == playerRegion)
            {
                child = link.From.SO.UI;
            }
            else
            {
                child = link.To.SO.UI;
            }
            if (TryGetUvCoordinatesFromRegionOntoFog(child, out var childUv))
            {
                RevealedObjects.Add(child.SpaceObject);
                circles.Add(new Vector4(childUv.x, childUv.y, 0.005f, 0));
                child.GetCanvasGroup().alpha = 1f;
            }
            else
            {
                Printer.Error($"Failed to get texture coordinate for {child} with uv pos {childUv}");
            }
        }

        var regions = GetAllRegionsWithUIObjects();
        
        foreach (var region in regions.Where(x => !RevealedObjects.Contains(x.SpaceObject)))
        {
            region.GetCanvasGroup().alpha = 0f;
        }
        
        UpdateTexture(circles);
    }

    private bool TryGetUvCoordinatesFromRegionOntoFog(UISpaceObject region, out Vector2 result)
    {
        if (region is null)
        {
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
}