using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;

namespace Megaphone;

public class MyLog
{
    // Enable debug when not in Release mode

    public static void LogInfo(string message)
    {
        Plugin.Logger.LogInfo(message);
    }

    public static void LogWarning(string message)
    {
        Plugin.Logger.LogWarning(message);
    }

    public static void LogError(string message)
    {
        Plugin.Logger.LogError(message);
    }

    public static void LogDebug(string message)
    {
#if DEBUG
        Plugin.Logger.LogDebug(message);
#else
        // In Release mode, do not log debug messages
        // Uncomment the line below if you want to log debug messages in Release mode
        //Plugin.Logger.LogDebug(message);
#endif
    }
}
