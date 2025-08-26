using Game;
using Game.Systems.Space;
using KL.Console;
using Multiplayer.Misc;
using UnityEngine;

namespace Multiplayer.Commands;

public class ConsoleCommandTest : ConsoleCommand
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        Register(new ConsoleCommandTest());
    }
    public override ConsoleCommandResult Execute(ConsoleCommandArguments args)
    {
        SpaceMap spaceMap = A.S.Query.GetSpaceMap.Ask();
        var index = Random.Range(0, spaceMap.Regions.Count);
        var region = spaceMap.Regions[index];
        Printer.Warn(region);
        Printer.Warn(region.SO);
        return OK();
    }

    public override void Initialize()
    {
        Name = "test";
        HelpLine = "test";
        LongUsage = "test";
    }
}