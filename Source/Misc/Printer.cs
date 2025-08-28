using System;
using System.IO;
using Game;
using KL.Utils;

namespace Infiniverse.Misc;

public static class Printer
{
    public const string Prefix = "[IU]> ";
    // ReSharper disable once ChangeFieldTypeToSystemThreadingLock
    private static readonly object PrintLock = new object();
    public static void Log(object toLog)
    {
        lock (PrintLock)
        {
            D.Log(Prefix + (toLog?.ToString() ?? "null"));
        }
    }
    public static void Warn(object toLog)
    {
        lock (PrintLock)
        {
            D.Warn(Prefix + (toLog?.ToString() ?? "null"));
        }
    }
    public static void Error(object toLog)
    {
        lock (PrintLock)
        {
            D.Err(Prefix + (toLog?.ToString() ?? "null"));
        }
    }
}