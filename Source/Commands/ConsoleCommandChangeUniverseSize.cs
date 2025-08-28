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
        Common.UniverseMinimum = new Vector2(args.GetFloat(1), args.GetFloat(2));
        Common.UniverseMaximum = new Vector2(args.GetFloat(3), args.GetFloat(4));
        FogOfWarHelper.Toggle(!FogOfWarHelper.Toggled);
        return OK();
    }

    public override void Initialize()
    {
        Name = "test";
        HelpLine = "Temp_SpawnFog";
    }
}
