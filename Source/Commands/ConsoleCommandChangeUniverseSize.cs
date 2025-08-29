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
        FogOfWarSys.Instance.UpdateTextureFromPlayerMovement();
        return OK();
    }

    public override void Initialize()
    {
        Name = "test";
        HelpLine = "Temp_SpawnFog";
    }
}
