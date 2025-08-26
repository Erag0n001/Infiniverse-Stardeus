using Game;
using Game.Systems.Space;
using KL.Console;
using Multiplayer.Extensions;
using UnityEngine;

namespace Multiplayer.Commands;

public class ConsoleCommandRemoveRegion : ConsoleCommand
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        Register(new ConsoleCommandRemoveRegion());
    }
        
    public override ConsoleCommandResult Execute(ConsoleCommandArguments args)
    {
        SpaceMap spaceMap = A.S.Query.GetSpaceMap.Ask();
        var index = Random.Range(0, spaceMap.Regions.Count);
        var region = spaceMap.Regions[index];
        region.ClearReferences();
        return OK();
    }

    public override void Initialize()
    {
        Name = "RemoveRandomRegion";
        HelpLine = "test";
        LongUsage = "test";
    }
}