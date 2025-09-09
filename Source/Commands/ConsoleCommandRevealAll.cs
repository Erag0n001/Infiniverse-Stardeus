using Infiniverse.Systems;
using KL.Console;
using UnityEngine;

namespace Infiniverse.Commands;

public class ConsoleCommandRevealAll : ConsoleCommand
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        Register(new ConsoleCommandRevealAll());
    }
    public override ConsoleCommandResult Execute(ConsoleCommandArguments args)
    {
        if (FogOfWarSys.Instance == null)
            return Error($"This command doesn't work outside of a loaded save!");
        FogOfWarSys.Instance.DebugRevealed = !FogOfWarSys.Instance.DebugRevealed;
        FogOfWarSys.Instance.UpdateTexture();
        return OK();
    }

    public override void Initialize()
    {
        Name = "revealall";
        HelpLine = "Reveals the entire map, or returns it to normal if already revealed";
    }
}