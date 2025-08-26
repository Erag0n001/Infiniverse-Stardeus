using HarmonyLib;
using Infiniverse.Config;
using Infiniverse.Misc;
using ModConfig;
using UnityEngine;

namespace Infiniverse;

public static class Main
{
    public static Harmony harmony;
    public static ConfigDataMultiplayer Configs;
    [RuntimeInitializeOnLoadMethod]
    static void StaticConstructorOnStartup() 
    {
        Configs = (ConfigDataMultiplayer)ConfigData.LoadConfig("Eragon.MMOMultiplayer");
        Printer.Warn("Multiplayer Loaded!");
        LoadHarmony();
        CreateUnityDispatcher();
    }

    static void LoadHarmony() 
    {
        harmony = new Harmony("Eragon.Multiplayer");
        harmony.PatchAll();
    }

    private static void CreateUnityDispatcher()
    {
        GameObject go = new GameObject("Dispatcher");
        go.AddComponent(typeof(MainThread));
    }
}