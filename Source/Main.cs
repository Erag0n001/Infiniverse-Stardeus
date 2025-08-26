using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Multiplayer.Misc;
using Multiplayer.Config;
using ModConfig;
using UnityEngine.SceneManagement;

namespace Multiplayer;

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