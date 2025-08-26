using System;
using System.IO;
using Game;
using KL.Utils;

namespace Infiniverse.Misc;

public static class Printer
{
    public const string Prefix = "[MP]> ";
    private static object printLock = new object();
    private static string LogFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "..",
        "LocalLow",
        "Kodo Linija",
        "Stardeus",
        "MpLogs"
    );
    private static readonly string extension = ".log";
    private static readonly string logPath;
    static Printer() 
    {
        if(!Directory.Exists(LogFolder)) 
        {
            Directory.CreateDirectory(LogFolder);
        }
        var files = Directory.GetFiles(LogFolder);
        logPath = Path.Combine(LogFolder, $"{The.Platform.PlayerName ?? "Test"}{extension}");
        if(File.Exists(logPath))
            File.Delete(logPath);
        using (File.Create(logPath));
    }
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