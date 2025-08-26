using System;
using System.IO;
using Game;
using KL.Utils;

namespace Infiniverse.Misc;

public static class Printer
{
    public const string Prefix = "[IU]> ";
    private static object printLock = new object();
    private static readonly string extension = ".log";
    private static readonly string logPath;
    public static void Log(object toLog)
    {
        lock (printLock)
        {
            D.Log(Prefix + (toLog?.ToString() ?? "null"));
        }
    }
    public static void Warn(object toLog)
    {
        lock (printLock)
        {
            D.Warn(Prefix + (toLog?.ToString() ?? "null"));
        }
    }
    public static void Error(object toLog)
    {
        lock (printLock)
        {
            D.Err(Prefix + (toLog?.ToString() ?? "null"));
        }
    }
}