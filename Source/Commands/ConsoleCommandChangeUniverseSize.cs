using System.Linq;
using Game;
using Game.Systems.Space;
using Infiniverse.Data.Chunks;
using Infiniverse.Helpers;
using Infiniverse.Systems;
using KL.Console;
using UnityEngine;

namespace Infiniverse.Commands;

public class ConsoleCommandChangeUniverseSize : ConsoleCommand
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        Register(new ConsoleCommandChangeUniverseSize());
    }

    public override ConsoleCommandResult Execute(ConsoleCommandArguments args)
    {
        FogOfWarHelper.UpdatePlayerNode(A.S.Universe.ObjectsById.FirstOrDefault(x => x.Value.UI != null && x.Value.SpaceEntity is SpaceRegion).Value.SpaceEntity as SpaceRegion);
        return OK();
    }

    public override void Initialize()
    {
        Name = "test";
        HelpLine = "Temp_SpawnFog";
    }
}
