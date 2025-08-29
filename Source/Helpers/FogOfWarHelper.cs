using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Systems.Space;
using Game.UI;
using Infiniverse.Data.Chunks;
using Infiniverse.Misc;
using UnityEngine;
using UnityEngine.UI;

namespace Infiniverse.Helpers;

public static class FogOfWarHelper
{
    public static bool Toggled => FogOfWarObject?.activeInHierarchy ?? false;
    private static GameObject FogOfWarObject;
    private static RectTransform FogOfWarRect;
    private static RenderTexture Mask;

    public static void Toggle(bool value, string layer = "", DetailBlockStarmapWidget widget = null)
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
        }
    }

    private static void UpdateTexture(List<Vector4> points)
    {
        if (points.Count > 128)
        {
            Printer.Warn($"Trying to discover too many circles at once, trimming down to 128!");
            while (points.Count > 128)
            {
                points.Remove(points.Last());
            }
        }
        Common.CircleMakerMaterial.SetVectorArray("_Circles", points.ToArray());
        Common.CircleMakerMaterial.SetInt("_CircleCount", points.Count);
        
        Graphics.Blit(null, Mask, Common.CircleMakerMaterial);
    }
    
    public static void UpdatePlayerNode(SpaceRegion region)
    {
        List<Vector4> circles = new List<Vector4>();
        if (TryGetUvCoordinatesFromRegionOntoFog(region.SO.UI, out var uv))
        {
            circles.Add(new Vector4(uv.x, uv.y, 0.01f, 0));
        }

        foreach (var link in region.Links)
        {
            var child = link.To;
            if (TryGetUvCoordinatesFromRegionOntoFog(child.SO.UI, out var childUv))
            {
                circles.Add(new Vector4(childUv.x, childUv.y, 0.005f, 0));
            }
        }
        
        UpdateTexture(circles);
    }

    private static bool TryGetUvCoordinatesFromRegionOntoFog(UISpaceObject region, out Vector2 result)
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
    
    public static void OnChunkAdded(Chunk _)
    {
        // var child = A.Starmap.transform.Find("Scroll View");
        // child = child.transform.Find("Viewport");
        // child = child.transform.Find("Content");
        // var navBall = child.GetComponent<UIScrollNav>();
        // navBall.SetLimits(new Vector2(0.0001f, 0.1f));
        // FogOfWarObject.transform.SetParent(child);
        // FogOfWarObject.transform.SetAsLastSibling();
        // var transform = FogOfWarObject.GetComponent<RectTransform>();
        // var num = Tunables.StarmapPositionMultiplier_Region;
        // var center = A.Starmap.GetCenterPos();
        // float x = 5000 * num + center.x;
        // float y = 5000 * num + center.y;
        // transform.RepositionTopRight(x, y);
    }
}