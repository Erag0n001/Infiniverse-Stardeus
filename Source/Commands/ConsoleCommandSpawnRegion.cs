using Infiniverse.Systems;
using KL.Console;
using UnityEngine;

namespace Infiniverse.Commands;

public class ConsoleCommandSpawnRegion : ConsoleCommand
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        Register(new ConsoleCommandSpawnRegion());
    }

    public override ConsoleCommandResult Execute(ConsoleCommandArguments args)
    {
        if (!args.HasArgument(2))
        {
            return Error($"Pleased provide an x and y position");
        }

        var x = args.GetInt(1);
        var y = args.GetInt(2);
        ChunkSys.Instance.GenerateChunkAt(new Vector2Int(x, y));
        return OK();
    }

    public override void Initialize()
    {
        Name = "SpawnChunkAt";
        HelpLine = "Creates a new chunk at the target position";
        Args =
        [
            new Argument
            {
                Name = "X coordinate"
            },
            new Argument
            {
                Name = "Y coordinate"
            }
        ];
    }
}