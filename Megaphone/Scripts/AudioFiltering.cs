using System;
using GameNetcodeStuff;
using Megaphone.Items;

namespace Megaphone.Scripts;

/// <summary>
/// Audio Filter for an unique player
/// </summary>
public class AudioFiltering
{
    public MegaphoneItem parent;
    public PlayerControllerB player;

    const AudioFilteringMode endNode = AudioFilteringMode.Siren;

    private AudioFilteringMode _mode = AudioFilteringMode.Robot;
    public AudioFilteringMode Mode => _mode;
    public bool active = false;

    private AudioFiltering() { }

    public AudioFiltering(MegaphoneItem parent)
    {
        this.parent = parent;
    }

    /// <summary>
    /// Go to the next audio filter mode
    /// </summary>
    /// <param name="enable">Enable the filter after the change</param>
    /// <returns></returns>
    public bool NextFilterMode(bool enable)
    {
        AudioFilteringMode newMode = _mode == endNode ? AudioFilteringMode.Robot : (_mode + 1);
        bool res = SetFilterMode(newMode, enable);
        if (res)
        {
            MyLog.LogInfo($"Switched to mode {Enum.GetName(typeof(AudioFilteringMode), _mode)}");
        }
        else
        {
            MyLog.LogError(
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
        MyLog.LogDebug($"Enabling...");

        if (active)
        {
            MyLog.LogDebug($"Already enabled...");
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
                AudioMod.EnableHighPitch(player, true);
                break;
            case AudioFilteringMode.LowPitch:
                AudioMod.EnableLowPitch(player, true);
                break;
            case AudioFilteringMode.Siren:
                AudioMod.PlaySFX(parent, AudioMod.Siren, noiseID: MyConfig.SIREN_NOISE_ID);
                break;
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
        MyLog.LogDebug($"Disabling...");

        if (!active)
        {
            MyLog.LogDebug($"Already disabled...");
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
                AudioMod.EnableHighPitch(player, false);
                break;
            case AudioFilteringMode.LowPitch:
                AudioMod.EnableLowPitch(player, false);
                break;
            case AudioFilteringMode.Siren:
                AudioMod.StopSFX(parent);
                break;
            default:
                return false;
        }

        active = false;
        return true;
    }

    /// <summary>
    /// Keep playing 'AudibleNoise' to alert ennemies. Call this every update
    /// </summary>
    /// <param name="item"></param>
    /// <param name="counter"></param>
    public void PlaySirenAudibleNoiseIfApplicable(MegaphoneItem item, int counter)
    {
        if (!active)
            return;
        if (_mode != AudioFilteringMode.Siren)
            return;

        RoundManager.Instance.PlayAudibleNoise(
            item.transform.position,
            noiseRange: MyConfig.SFXHearDistance * MyConfig.SFXEnemyHearDistance * 50,
            noiseLoudness: 1.0f,
            timesPlayedInSameSpot: counter,
            noiseIsInsideClosedShip: item.isInElevator && StartOfRound.Instance.hangarDoorsClosed,
            noiseID: MyConfig.SIREN_NOISE_ID
        );
    }
}
