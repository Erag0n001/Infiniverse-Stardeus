using Game;
using Game.CodeGen;
using Game.Constants;
using Game.Rendering;
using Game.UI;
using Game.Utils;
using Infiniverse.Data.Chunks;
using Infiniverse.Extensions;
using Infiniverse.Helpers;
using Infiniverse.Misc;
using KL.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Infiniverse.Systems;

public static class FogOfWarHelper
{
    public static bool Toggled => FogOfWarObject?.activeInHierarchy ?? false;
    private static GameObject FogOfWarObject;
    
    private static RenderTexture Mask;

    public static void Toggle(bool value, string layer = "", DetailBlockStarmapWidget widget = null)
    {
        if (string.IsNullOrWhiteSpace(layer))
            layer = A.Starmap.Layer;
        if (widget is null)
        {
            widget = A.Starmap;
        }
        Printer.Warn(layer);
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
            
            Printer.Warn(A.Starmap);
            var scrollView = widget.transform.Find("Scroll View");
            var viewPort = scrollView.transform.Find("Viewport");
            var navBallObj = viewPort.transform.Find("Content");
            var navBall = navBallObj.GetComponent<UIScrollNav>();
            navBall.SetLimits(new Vector2(0.0001f, 0.1f));
            
            FogOfWarObject.transform.SetParent(navBallObj.transform);
            
            var image = FogOfWarObject.AddComponent<RawImage>();
            image.material = Common.FogOfWarMaterial;

            var texture = new Texture2D(2000, 2000, TextureFormat.RGBA32, false);
            texture.Apply();
            image.texture = texture;
            
            var transform = FogOfWarObject.GetComponent<RectTransform>();
            transform.position = navBallObj.transform.position;
        }
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