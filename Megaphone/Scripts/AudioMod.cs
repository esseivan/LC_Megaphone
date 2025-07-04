using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using Megaphone.Patches;
using UnityEngine;
using static ES3Spreadsheet;
using static Megaphone.Scripts.AudioMod;

namespace Megaphone.Scripts
{
    public class AudioMod
    {
        protected static List<ulong> setupPlayersID = new List<ulong>();

        public static Dictionary<ulong, AudioFiltering> audioFilterings =
            new Dictionary<ulong, AudioFiltering>();

        public enum AudioFilteringMode
        {
            Robot,
            Loud,
            HighPitch,
            LowPitch,
        }

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

            public bool IncrementMode(bool enable = false)
            {
                AudioFilteringMode newMode =
                    _mode == AudioFilteringMode.Loud ? AudioFilteringMode.Robot : (_mode + 1);
                MyLog.Logger.LogDebug(
                    $"Switched to mode {Enum.GetName(typeof(AudioFilteringMode), _mode)}"
                );
                return SwitchTo(newMode, enable);
            }

            public bool SwitchTo(AudioFilteringMode newMode, bool enable = true)
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

            public bool Enable()
            {
                if (active)
                    return true;

                active = true;
                switch (_mode)
                {
                    case AudioFilteringMode.Robot:
                        EnableRobotVoice(player, true);
                        break;
                    case AudioFilteringMode.Loud:
                        EnableLoudVoice(player, true);
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

            public bool Disable()
            {
                if (!active)
                    return true;

                switch (_mode)
                {
                    case AudioFilteringMode.Robot:
                        EnableRobotVoice(player, false);
                        break;
                    case AudioFilteringMode.Loud:
                        EnableLoudVoice(player, false);
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
                audioFilterings.Remove(player.playerClientId);
            }
        }

        public static bool SetupGameobjects(PlayerControllerB player)
        {
            if (setupPlayersID.Contains(player.playerClientId))
            {
                MyLog.Logger.LogDebug($"Components already set for {player.playerUsername}");
                return true;
            }
            setupPlayersID.Add(player.actualClientId);
            audioFilterings[player.actualClientId] = new AudioFiltering(player);

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

        public static bool SwitchFilterOnOff(PlayerControllerB player, bool state)
        {
            AudioFiltering filter = audioFilterings[player.actualClientId];
            if (state)
                return filter.Enable();
            else
                return filter.Disable();
        }

        public static bool SwitchFilterNextMode(PlayerControllerB player, bool state)
        {
            AudioFiltering filter = audioFilterings[player.actualClientId];
            if (!filter.Disable())
                return false;
            return filter.IncrementMode(state);
        }

        public static bool SwitchFilterMode(
            PlayerControllerB player,
            bool state,
            AudioFilteringMode mode
        )
        {
            AudioFiltering filter = audioFilterings[player.actualClientId];
            return filter.SwitchTo(mode, state);
        }

        protected static bool EnableRobotVoice(PlayerControllerB player, bool on)
        {
            MyLog.Logger.LogInfo(
                $"{(on ? "Enabling" : "Disabling")} robot voice for player {player.playerUsername}"
            );

            AudioSource src = player.currentVoiceChatAudioSource;
            if (src == null)
                return false;

            AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
            AudioHighPassFilter hp = src.GetComponent<AudioHighPassFilter>();
            AudioChorusFilter chorus = src.GetComponent<AudioChorusFilter>();

            if (echo == null)
            {
                MyLog.Logger.LogInfo($"AudioEchoFilter missing");
                return false;
            }
            if (hp == null)
            {
                MyLog.Logger.LogInfo($"AudioHighPassFilter missing");
                return false;
            }
            if (chorus == null)
            {
                MyLog.Logger.LogInfo($"AudioChorusFilter missing");
                return false;
            }

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

        protected static bool EnableLoudVoice(PlayerControllerB player, bool on)
        {
            MyLog.Logger.LogInfo(
                $"{(on ? "Enabling" : "Disabling")} loud voice for player {player.playerUsername}"
            );

            AudioSource src = player.currentVoiceChatAudioSource;
            if (src == null)
                return false;

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

        // UpdatePlayerVoiceEffects
    }
}
