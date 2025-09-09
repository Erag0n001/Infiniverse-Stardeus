using Infiniverse.Systems;
using KL.Console;
using UnityEngine;

namespace Infiniverse.Commands;

public class ConsoleCommandResetFog : ConsoleCommand
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        Register(new ConsoleCommandResetFog());
    }
    public override ConsoleCommandResult Execute(ConsoleCommandArguments args)
    {
        if (FogOfWarSys.Instance == null)
            return Error($"This command doesn't work outside of a loaded save!");
        FogOfWarSys.Instance.Reset();
        return OK();
    }

    public override void Initialize()
    {
        Name = "resetfog";
        HelpLine = "Reset all fog of war memory, fogging the entire map again";
    }
}