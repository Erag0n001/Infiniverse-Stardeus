using Infiniverse.Systems;
using KL.Console;
using UnityEngine;

namespace Infiniverse.Commands;

public class ConsoleCommandDebufFillChunks: ConsoleCommand
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        Register(new ConsoleCommandDebufFillChunks());
    }

    public override ConsoleCommandResult Execute(ConsoleCommandArguments args)
    {
        const int size = 100;
        for (int i = 0; i < size; i += 5)
        {
            for (int j = 0; j < size; j += 5)
            {
                ChunkSys.GenerateChunkAt(new Vector2Int(i, j));
            }
        }

        return OK();
    }

    public override void Initialize()
    {
        Name = "SpawnChunks";
        HelpLine = "Creates a very large amount of chunks in the universe";
    }
}