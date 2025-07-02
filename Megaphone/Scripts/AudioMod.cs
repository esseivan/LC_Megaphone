using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
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
                    return false;

                active = true;
                switch (_mode)
                {
                    case AudioFilteringMode.Robot:
                        EnableRobotVoice(player);
                        break;
                    case AudioFilteringMode.Loud:
                        EnableLoudVoice(player);
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
                    return false;

                switch (_mode)
                {
                    case AudioFilteringMode.Robot:
                        DisableRobotVoice(player);
                        break;
                    case AudioFilteringMode.Loud:
                        DisableLoudVoice(player);
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

        public static string RobotVoice(bool state, PlayerControllerB player)
        {
            return state ? EnableRobotVoice(player) : DisableRobotVoice(player);
        }

        public static string DisableRobotVoice(PlayerControllerB player)
        {
            MyLog.Logger.LogInfo($"Disabling robot voice for player {player.playerUsername}");

            AudioSource src = player.currentVoiceChatAudioSource;
            if (src == null)
                return "failed";

            AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
            AudioHighPassFilter hp = src.GetComponent<AudioHighPassFilter>();
            AudioChorusFilter chorus = src.GetComponent<AudioChorusFilter>();

            if (echo == null)
            {
                MyLog.Logger.LogInfo($"AudioEchoFilter missing");
                return "failed";
            }
            if (hp == null)
            {
                MyLog.Logger.LogInfo($"AudioHighPassFilter missing");
                return "failed";
            }
            if (chorus == null)
            {
                MyLog.Logger.LogInfo($"AudioChorusFilter missing");
                return "failed";
            }

            echo.enabled = false;
            hp.enabled = false;
            chorus.enabled = false;

            SoundManager.Instance.playerVoicePitchTargets[player.playerClientId] = 1f;

            return "success";
        }

        public static string EnableRobotVoice(PlayerControllerB player)
        {
            MyLog.Logger.LogInfo($"Enabling robot voice for player {player.playerUsername}");

            AudioSource src = player.currentVoiceChatAudioSource;
            if (src == null)
                return "failed";

            AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
            AudioHighPassFilter hp = src.GetComponent<AudioHighPassFilter>();
            AudioChorusFilter chorus = src.GetComponent<AudioChorusFilter>();

            if (echo == null)
            {
                MyLog.Logger.LogInfo($"AudioEchoFilter missing");
                return "failed";
            }
            if (hp == null)
            {
                MyLog.Logger.LogInfo($"AudioHighPassFilter missing");
                return "failed";
            }
            if (chorus == null)
            {
                MyLog.Logger.LogInfo($"AudioChorusFilter missing");
                return "failed";
            }

            echo.delay = 10f;
            echo.decayRatio = 0.75f;
            echo.enabled = true;

            hp.cutoffFrequency = 500;
            hp.enabled = true;

            chorus.dryMix = 0.75f;
            chorus.wetMix1 = chorus.wetMix2 = 0.75f;
            chorus.delay = 40f;
            chorus.depth = 0.7f;
            chorus.rate = 1.2f;
            chorus.enabled = true;

            SoundManager.Instance.playerVoicePitchTargets[player.playerClientId] = 1.2f;
            //SoundManager.Instance.SetPlayerPitch(1.2f, (int)player.playerClientId);
            //}

            return "success";
        }

        public static bool EnableLoudVoice(PlayerControllerB player)
        {
            MyLog.Logger.LogInfo($"Enabling loud voice for player {player.playerUsername}");

            AudioSource src = player.currentVoiceChatAudioSource;
            if (src == null)
                return false;

            AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
            AudioLowPassFilter lp = src.GetComponent<AudioLowPassFilter>();
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
                MyLog.Logger.LogError($"AudioLowPassFilter missing");
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

            lp.cutoffFrequency = 3000f;

            hp.cutoffFrequency = 300;
            hp.enabled = true;

            dist.distortionLevel = 0.5f;
            dist.enabled = true;

            return true;
        }

        public static bool DisableLoudVoice(PlayerControllerB player)
        {
            MyLog.Logger.LogInfo($"Disabling loud voice for player {player.playerUsername}");

            AudioSource src = player.currentVoiceChatAudioSource;
            if (src == null)
                return false;

            AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
            AudioLowPassFilter lp = src.GetComponent<AudioLowPassFilter>();
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
                MyLog.Logger.LogError($"AudioLowPassFilter missing");
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

            lp.cutoffFrequency = 20000f;

            hp.cutoffFrequency = 300;
            hp.enabled = false;

            dist.distortionLevel = 0.5f;
            dist.enabled = false;

            return true;
        }
    }
}
