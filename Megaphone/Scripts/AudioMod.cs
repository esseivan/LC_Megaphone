using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using Megaphone.Patches;
using UnityEngine;

namespace Megaphone.Scripts;

/// <summary>
/// Audio filter manager
/// </summary>
public partial class AudioMod
{
    /// <summary>
    /// List of players ID whose setup is done
    /// </summary>
    protected static List<ulong> setupPlayersID = new List<ulong>();

    /// <summary>
    /// Indicate that a new player joined.
    /// This information is used to clear the list
    /// of players ID where the setup is applied
    /// </summary>
    /// <param name="player"></param>
    public static void RegisterNewPlayer(PlayerControllerB player)
    {
        if (setupPlayersID.Contains(player.playerClientId))
        {
            setupPlayersID.Remove(player.playerClientId);
        }
    }

    /// <summary>
    /// Setup the player audio components for future filters
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool SetupGameobjects(PlayerControllerB player)
    {
        if (player == null)
        {
            MyLog.Logger.LogError(
                "Unable to continue, player is null... Owner must re-equip the item"
            );
            return false;
        }

        MyLog.Logger.LogDebug(
            $"Setting gameobjects for player {player.name} - {player.playerClientId} - {player.actualClientId} - {player.OwnerClientId}"
        );
        if (setupPlayersID.Contains(player.playerClientId))
        {
            MyLog.Logger.LogDebug($"Components already set for {player.playerUsername}");
            return true;
        }
        setupPlayersID.Add(player.actualClientId);

        MyLog.Logger.LogInfo(
            $"Settings up audio components for player {player.playerUsername} ; ID({player.playerClientId})"
        );

        AudioSource src = player.currentVoiceChatAudioSource;
        if (src == null)
        {
            MyLog.Logger.LogInfo($"No AudioSource found...");
            return false;
        }

        AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
        if (echo == null)
        {
            MyLog.Logger.LogDebug($"AudioEchoFilter missing");
            src.gameObject.AddComponent<AudioEchoFilter>();
            echo = src.GetComponent<AudioEchoFilter>();
        }
        echo.enabled = false;
        MyLog.Logger.LogDebug($"Echo ready");

        AudioHighPassFilter hp = src.GetComponent<AudioHighPassFilter>();
        if (hp == null)
        {
            MyLog.Logger.LogDebug($"AudioHighPassFilter missing");
            src.gameObject.AddComponent<AudioHighPassFilter>();
            hp = src.GetComponent<AudioHighPassFilter>();
        }
        hp.enabled = false;
        MyLog.Logger.LogDebug($"HighPass ready");

        AudioDistortionFilter dist = src.GetComponent<AudioDistortionFilter>();
        if (dist == null)
        {
            MyLog.Logger.LogDebug($"AudioDistortionFilter missing");
            src.gameObject.AddComponent<AudioDistortionFilter>();
            dist = src.GetComponent<AudioDistortionFilter>();
        }
        dist.enabled = false;
        MyLog.Logger.LogDebug($"Distortion ready");

        AudioChorusFilter chorus = src.GetComponent<AudioChorusFilter>();
        if (chorus == null)
        {
            MyLog.Logger.LogDebug($"AudioHighPassFilter missing");
            src.gameObject.AddComponent<AudioChorusFilter>();
            chorus = src.GetComponent<AudioChorusFilter>();
        }
        chorus.enabled = false;
        MyLog.Logger.LogDebug($"Chorus ready");

        MyLog.Logger.LogDebug(
            $"Player {player.playerUsername} ; ID({player.playerClientId}) ready !"
        );

        return true;
    }

    protected static bool AreAllComponentsAdded(AudioSource src)
    {
        AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
        OccludeAudio lp = src.GetComponent<OccludeAudio>();
        AudioHighPassFilter hp = src.GetComponent<AudioHighPassFilter>();
        AudioChorusFilter chorus = src.GetComponent<AudioChorusFilter>();
        AudioDistortionFilter dist = src.GetComponent<AudioDistortionFilter>();

        if (echo == null)
        {
            MyLog.Logger.LogError($"AudioEchoFilter missing");
            return false;
        }
        if (lp == null)
        {
            MyLog.Logger.LogError($"OccludeAudio missing");
            return false;
        }
        if (hp == null)
        {
            MyLog.Logger.LogError($"AudioHighPassFilter missing");
            return false;
        }
        if (chorus == null)
        {
            MyLog.Logger.LogError($"AudioChorusFilter missing");
            return false;
        }
        if (dist == null)
        {
            MyLog.Logger.LogError($"AudioDistortionFilter missing");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check that all components for the user are availble
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    protected static bool CheckComponents(PlayerControllerB player)
    {
        //audioFilterings

        AudioSource src = player.currentVoiceChatAudioSource;
        if (src == null)
            return false;

        if (!AreAllComponentsAdded(src))
        {
            // Missing components, clear flag
            RegisterNewPlayer(player);

            return SetupGameobjects(player);
        }
        return true;
    }

    internal static bool EnableRobotVoice(PlayerControllerB player, bool on)
    {
        MyLog.Logger.LogInfo(
            $"{(on ? "Enabling" : "Disabling")} robot voice for player {player.playerUsername}"
        );

        if (!CheckComponents(player))
            return false;

        AudioSource src = player.currentVoiceChatAudioSource;
        AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
        AudioHighPassFilter hp = src.GetComponent<AudioHighPassFilter>();
        AudioChorusFilter chorus = src.GetComponent<AudioChorusFilter>();

        if (on)
        {
            echo.delay = 10f;
            echo.decayRatio = 0.75f;

            hp.cutoffFrequency = 500;

            chorus.dryMix = 0.75f;
            chorus.wetMix1 = chorus.wetMix2 = 0.75f;
            chorus.delay = 40f;
            chorus.depth = 0.7f;
            chorus.rate = 1.2f;
        }

        echo.enabled = hp.enabled = chorus.enabled = on;

        SoundManager.Instance.playerVoicePitchTargets[player.playerClientId] = on ? 1.2f : 1f;

        return true;
    }

    internal static bool EnableLoudVoice(PlayerControllerB player, bool on)
    {
        MyLog.Logger.LogInfo(
            $"{(on ? "Enabling" : "Disabling")} loud voice for player {player.playerUsername}"
        );

        if (!CheckComponents(player))
            return false;

        AudioSource src = player.currentVoiceChatAudioSource;
        AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
        OccludeAudio lp = src.GetComponent<OccludeAudio>();
        AudioHighPassFilter hp = src.GetComponent<AudioHighPassFilter>();
        AudioChorusFilter chorus = src.GetComponent<AudioChorusFilter>();
        AudioDistortionFilter dist = src.GetComponent<AudioDistortionFilter>();

        echo.enabled = false;
        chorus.enabled = false;

        if (on)
        {
            hp.cutoffFrequency = 800;

            dist.distortionLevel = 0.9f;
        }
        player.voiceMuffledByEnemy = true; // Necessary to prevent the game from disabling the low pass override
        lp.overridingLowPass = on;
        lp.lowPassOverride = on ? 3500f : 20000f; // Always enabled
        hp.enabled = dist.enabled = on;
        AudioPatch.EnableHighpass(player.actualClientId, on);

        float modifier = Plugin.configHearDistance.Value;
        if (modifier <= 0)
            modifier = 1.0f;
        src.maxDistance = on ? (modifier * 50) : 50f; // Default is 50 ; double the distance. Ennemies too :)

        return true;
    }
}
