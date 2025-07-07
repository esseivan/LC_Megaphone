using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using Megaphone.Items;
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

    protected static AudioClip sfx,
        siren;

    public static AudioClip SFX => sfx;
    public static AudioClip Siren => siren;

    public static void LoadAssets()
    {
        sfx = Plugin.Assets.LoadAsset<AudioClip>(Plugin.ASSET_PATH_MEGAPHONE_SFX);
        siren = Plugin.Assets.LoadAsset<AudioClip>(Plugin.ASSET_PATH_MEGAPHONE_SIREN);
    }

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
            MyLog.LogError("Unable to continue, player is null... Owner must re-equip the item");
            return false;
        }

        if (setupPlayersID.Contains(player.playerClientId))
        {
            //MyLog.LogDebug($"Components already set for {player.playerUsername}");
            return true;
        }
        setupPlayersID.Add(player.actualClientId);

        MyLog.LogDebug($"Setting gameobjects for player {player.name} - {player.playerClientId}");

        MyLog.LogInfo(
            $"Settings up audio components for player {player.playerUsername} ; ID({player.playerClientId})"
        );

        AudioSource src = player.currentVoiceChatAudioSource;
        if (src == null)
        {
            MyLog.LogInfo($"No AudioSource found...");
            return false;
        }

        AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
        if (echo == null)
        {
            MyLog.LogDebug($"AudioEchoFilter missing");
            src.gameObject.AddComponent<AudioEchoFilter>();
            echo = src.GetComponent<AudioEchoFilter>();
        }
        echo.enabled = false;
        MyLog.LogDebug($"Echo ready");

        AudioHighPassFilter hp = src.GetComponent<AudioHighPassFilter>();
        if (hp == null)
        {
            MyLog.LogDebug($"AudioHighPassFilter missing");
            src.gameObject.AddComponent<AudioHighPassFilter>();
            hp = src.GetComponent<AudioHighPassFilter>();
        }
        hp.enabled = false;
        MyLog.LogDebug($"HighPass ready");

        AudioDistortionFilter dist = src.GetComponent<AudioDistortionFilter>();
        if (dist == null)
        {
            MyLog.LogDebug($"AudioDistortionFilter missing");
            src.gameObject.AddComponent<AudioDistortionFilter>();
            dist = src.GetComponent<AudioDistortionFilter>();
        }
        dist.enabled = false;
        MyLog.LogDebug($"Distortion ready");

        AudioChorusFilter chorus = src.GetComponent<AudioChorusFilter>();
        if (chorus == null)
        {
            MyLog.LogDebug($"AudioHighPassFilter missing");
            src.gameObject.AddComponent<AudioChorusFilter>();
            chorus = src.GetComponent<AudioChorusFilter>();
        }
        chorus.enabled = false;
        MyLog.LogDebug($"Chorus ready");

        MyLog.LogInfo($"Player {player.playerUsername} ; ID({player.playerClientId}) ready !");

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
            MyLog.LogError($"AudioEchoFilter missing");
            return false;
        }
        if (lp == null)
        {
            MyLog.LogError($"OccludeAudio missing");
            return false;
        }
        if (hp == null)
        {
            MyLog.LogError($"AudioHighPassFilter missing");
            return false;
        }
        if (chorus == null)
        {
            MyLog.LogError($"AudioChorusFilter missing");
            return false;
        }
        if (dist == null)
        {
            MyLog.LogError($"AudioDistortionFilter missing");
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
        MyLog.LogInfo(
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

        AudioPatch.playersPitchTargets[player.playerClientId] = on ? MyConfig.RobotVoicePitch : -1; // -1 : skip override

        return true;
    }

    internal static bool EnableLoudVoice(PlayerControllerB player, bool on)
    {
        MyLog.LogInfo(
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

            dist.distortionLevel = 0.9f; // High distortion increases volume. Decrease a bit
        }
        src.volume = on ? MyConfig.LoudVoiceVolume : 1;
        player.voiceMuffledByEnemy = true; // Necessary to prevent the game from disabling the low pass override
        lp.overridingLowPass = on;
        lp.lowPassOverride = on ? 3500f : 20000f; // Always enabled
        hp.enabled = dist.enabled = on;
        AudioPatch.EnableHighpass(player.actualClientId, on);

        src.maxDistance = on ? (MyConfig.HearDistance * 50) : 50f; // Default is 50 ; double the distance. Ennemies too :)

        return true;
    }

    internal static void EnableHighPitch(PlayerControllerB player, bool on)
    {
        MyLog.LogInfo(
            $"{(on ? "Enabling" : "Disabling")} high pitch voice for player {player.playerUsername}"
        );

        AudioPatch.playersPitchTargets[player.playerClientId] = on ? 2 : -1; // -1 : skip override
    }

    internal static void EnableLowPitch(PlayerControllerB player, bool on)
    {
        MyLog.LogInfo(
            $"{(on ? "Enabling" : "Disabling")} low pitch voice for player {player.playerUsername}"
        );

        AudioPatch.playersPitchTargets[player.playerClientId] = on ? 0.5f : -1; // -1 : skip override
    }

    internal static bool PlaySFX(MegaphoneItem item, AudioClip sound, int counter = 0)
    {
        if (sound == null)
        {
            MyLog.LogError("SFX is null");
            return false;
        }
        MyLog.LogInfo("Playing SFX...");

        AudioSource src = item.GetComponent<AudioSource>();
        if (src == null)
        {
            MyLog.LogError("No AudioSource to play SFX...");
            return false;
        }

        src.maxDistance = MyConfig.SFXHearDistance * 50;
        src.volume = MyConfig.SFXVolume;
        src.PlayOneShot(sound);
        RoundManager.Instance.PlayAudibleNoise(
            item.transform.position,
            noiseRange: src.maxDistance * MyConfig.SFXEnemyHearDistance,
            noiseLoudness: 1.0f,
            timesPlayedInSameSpot: counter,
            noiseIsInsideClosedShip: item.isInElevator && StartOfRound.Instance.hangarDoorsClosed
        );

        return true;
    }

    internal static bool StopSFX(MegaphoneItem item)
    {
        if (sfx == null)
        {
            MyLog.LogError("SFX is null");
            return false;
        }
        MyLog.LogInfo("Stopping SFX...");

        AudioSource src = item.GetComponent<AudioSource>();
        if (src == null)
        {
            MyLog.LogError("No AudioSource to play SFX...");
            return false;
        }

        src.maxDistance = 50;
        src.volume = 1.0f;
        src.Stop(true);

        return true;
    }
}
