using Infiniverse.Data.Chunks;
using Infiniverse.Helpers;
using Infiniverse.Systems;
using KL.Console;
using UnityEngine;

namespace Infiniverse.Commands;

public class ConsoleCommandToggleFog : ConsoleCommand
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        Register(new ConsoleCommandToggleFog());
    }

    public override ConsoleCommandResult Execute(ConsoleCommandArguments args)
    {
        FogOfWarSys.Instance.Toggle(!FogOfWarSys.Toggled);
        return OK();
    }

    public override void Initialize()
    {
        Name = "togglefog";
        HelpLine = "Temp_SpawnFog";
    }
}
