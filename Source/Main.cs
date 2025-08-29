using System.Linq;
using Game;
using Game.Rendering;
using Game.Systems;
using HarmonyLib;
using Infiniverse.Config;
using Infiniverse.Helpers;
using Infiniverse.Misc;
using ModdingOverhauled.AssetBundleModule;
using ModdingOverhauled.ConfigModule;
using UnityEngine;

namespace Infiniverse;

public static class Main
{
    public static Harmony harmony;
    public static ConfigDataInfiniverse Configs;
    [RuntimeInitializeOnLoadMethod]
    static void StaticConstructorOnStartup() 
    {
        Configs = (ConfigDataInfiniverse)ConfigData.LoadConfig("Eragon.Infiniverse");
        Printer.Warn("Infiniverse Loaded!");
        LoadHarmony();
        GetShader();
    }

    static void LoadHarmony() 
    {
        harmony = new Harmony("Eragon.Infiniverse");
        harmony.PatchAll();
    }

    static void GetShader()
    {
        const string fogShaderName = "Infiniverse/FogShader";
        const string fogCircleName = "Infiniverse/FogCircle";
        const string assetBundleName = "infiniverse_shaders";
        Common.FogOfWarMaterial = new Material(RenderingService.Shaders.shaders[fogShaderName]);
        Common.CircleMakerMaterial = new  Material(RenderingService.Shaders.shaders[fogCircleName]);
    }
}