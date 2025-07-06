using System;
using GameNetcodeStuff;

namespace Megaphone.Scripts;

/// <summary>
/// Audio Filter for an unique player
/// </summary>
public class AudioFiltering
{
    public PlayerControllerB player;
    private AudioFilteringMode _mode = AudioFilteringMode.Robot;
    public AudioFilteringMode Mode => _mode;
    public bool active = false;

    public AudioFiltering() { }

    public AudioFiltering(PlayerControllerB player)
    {
        this.player = player;
    }

    /// <summary>
    /// Go to the next audio filter mode
    /// </summary>
    /// <param name="enable">Enable the filter after the change</param>
    /// <returns></returns>
    public bool NextFilterMode(bool enable)
    {
        AudioFilteringMode newMode =
            _mode == AudioFilteringMode.Loud ? AudioFilteringMode.Robot : (_mode + 1);
        bool res = SetFilterMode(newMode, enable);
        if (res)
        {
            MyLog.Logger.LogDebug(
                $"Switched to mode {Enum.GetName(typeof(AudioFilteringMode), _mode)}"
            );
        }
        else
        {
            MyLog.Logger.LogError(
                $"Unable to switch to mode {Enum.GetName(typeof(AudioFilteringMode), _mode)}"
            );
        }
        return res;
    }

    /// <summary>
    /// Switch to the specified filtering mode
    /// </summary>
    /// <param name="newMode">New filter mode</param>
    /// <param name="enable">Enable the filter after the change</param>
    /// <returns></returns>
    public bool SetFilterMode(AudioFilteringMode newMode, bool enable)
    {
        if (active)
        {
            if (!Disable())
                return false;
        }
        _mode = newMode;
        if (enable)
            return Enable();
        return true;
    }

    /// <summary>
    /// Enable the filter
    /// </summary>
    /// <returns></returns>
    public bool Enable()
    {
        MyLog.Logger.LogDebug($"Enabling...");

        if (active)
        {
            MyLog.Logger.LogDebug($"Already enabled...");
            return true;
        }

        active = true;
        switch (_mode)
        {
            case AudioFilteringMode.Robot:
                AudioMod.EnableRobotVoice(player, true);
                break;
            case AudioFilteringMode.Loud:
                AudioMod.EnableLoudVoice(player, true);
                break;
            case AudioFilteringMode.HighPitch:
                return false;
            case AudioFilteringMode.LowPitch:
                return false;
            default:
                return false;
        }

        return true;
    }

    /// <summary>
    /// Disable the filter
    /// </summary>
    /// <returns></returns>
    public bool Disable()
    {
        MyLog.Logger.LogDebug($"Disabling...");

        if (!active)
        {
            MyLog.Logger.LogDebug($"Already disabled...");
            return true;
        }

        switch (_mode)
        {
            case AudioFilteringMode.Robot:
                AudioMod.EnableRobotVoice(player, false);
                break;
            case AudioFilteringMode.Loud:
                AudioMod.EnableLoudVoice(player, false);
                break;
            case AudioFilteringMode.HighPitch:
                return false;
            case AudioFilteringMode.LowPitch:
                return false;
            default:
                return false;
        }

        active = false;
        return true;
    }
}
