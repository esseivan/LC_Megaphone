using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using UnityEngine;

namespace Megaphone.Scripts
{
    public class AudioMod
    {
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

        public static bool SetupGameobjects(PlayerControllerB player)
        {
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

        public static string RobotVoice(bool state, PlayerControllerB player)
        {
            return state ? EnableRobotVoice(player) : DisableRobotVoice(player);
        }

        public static string DisableRobotVoice(PlayerControllerB player)
        {
            //foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            //{
            MyLog.Logger.LogInfo($"Disabling robot voice for player {player.playerUsername}");

            AudioSource src = player.currentVoiceChatAudioSource;
            if (src == null)
                return "failed";
            //continue;

            AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
            AudioHighPassFilter hp = src.GetComponent<AudioHighPassFilter>();
            AudioChorusFilter chorus = src.GetComponent<AudioChorusFilter>();

            if (echo == null)
            {
                MyLog.Logger.LogInfo($"AudioEchoFilter missing");
                return "failed";
                //continue;
            }
            if (hp == null)
            {
                MyLog.Logger.LogInfo($"AudioHighPassFilter missing");
                return "failed";
                //continue;
            }
            if (chorus == null)
            {
                MyLog.Logger.LogInfo($"AudioChorusFilter missing");
                return "failed";
                //continue;
            }

            echo.enabled = false;

            hp.enabled = false;

            chorus.enabled = false;

            SoundManager.Instance.playerVoicePitchTargets[player.playerClientId] = 1f;
            //SoundManager.Instance.SetPlayerPitch(1.2f, (int)player.playerClientId);
            //}

            return "success";
        }

        public static string EnableRobotVoice(PlayerControllerB player)
        {
            //foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            //{
            MyLog.Logger.LogInfo($"Enabling robot voice for player {player.playerUsername}");

            AudioSource src = player.currentVoiceChatAudioSource;
            if (src == null)
                return "failed";
            //continue;

            AudioEchoFilter echo = src.GetComponent<AudioEchoFilter>();
            AudioHighPassFilter hp = src.GetComponent<AudioHighPassFilter>();
            AudioChorusFilter chorus = src.GetComponent<AudioChorusFilter>();

            if (echo == null)
            {
                MyLog.Logger.LogInfo($"AudioEchoFilter missing");
                src.gameObject.AddComponent<AudioEchoFilter>();
                echo = src.GetComponent<AudioEchoFilter>();
            }
            if (hp == null)
            {
                MyLog.Logger.LogInfo($"AudioHighPassFilter missing");
                src.gameObject.AddComponent<AudioHighPassFilter>();
                hp = src.GetComponent<AudioHighPassFilter>();
            }
            if (chorus == null)
            {
                MyLog.Logger.LogInfo($"AudioChorusFilter missing");
                return "failed";
                //continue;
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
            chorus.enabled = false;

            SoundManager.Instance.playerVoicePitchTargets[player.playerClientId] = 1.2f;
            //SoundManager.Instance.SetPlayerPitch(1.2f, (int)player.playerClientId);
            //}

            return "success";
        }
    }
}
