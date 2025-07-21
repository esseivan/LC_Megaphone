using System;
using System.Collections.Generic;
using System.Text;
using Megaphone.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace Megaphone.Items
{
    public class MegaphoneItem : GrabbableObject
    {
        public AudioFiltering audioFiltering;
        protected bool isSynced;
        protected int TimesPlayedWithoutTurnedOff;

        public override void OnNetworkSpawn()
        {
            MyLog.LogDebug("OnNetworkSpawn() called");

            base.OnNetworkSpawn();
        }

        public override void Start()
        {
            MyLog.LogDebug("MegaphoneItem item created !");
            base.Start();
            itemProperties.itemIsTrigger = false;
            itemProperties.batteryUsage = 120; // Number of seconds it can stay on
            itemProperties.syncUseFunction = true;
            itemProperties.syncInteractLRFunction = true;
            itemProperties.syncDiscardFunction = true;
            itemProperties.holdButtonUse = true;
            insertedBattery.charge = 1;
            itemProperties.positionOffset = new Vector3(0.08f, 0.2f, -0.1f);
            itemProperties.rotationOffset = new Vector3(-90, 180, 38);
            TimesPlayedWithoutTurnedOff = 0;

            this.audioFiltering = new AudioFiltering(this);

            // Server is always in sync
            this.isSynced = (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer);
        }

        /// <summary>
        /// Q and E presses
        /// </summary>
        /// <param name="right">True if 'E', false if 'Q'</param>
        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            if (right)
                return;

            audioFiltering.NextFilterMode(this.isBeingUsed);
            UpdateTooltips();
        }

        /// <summary>
        /// Called when the item is in the hotbar and becomes unselected
        /// </summary>
        public override void PocketItem()
        {
            MyLog.LogDebug($"PocketItem()");
            if (this.playerHeldBy != null)
            {
                this.playerHeldBy.equippedUsableItemQE = false;
                this.isBeingUsed = false;
                audioFiltering.Disable();
            }
            base.PocketItem();
        }

        /// <summary>
        /// Called when the item is equiped (held in hand)
        /// This function is called on all clients
        /// </summary>
        public override void EquipItem()
        {
            MyLog.LogDebug($"Equipped");

            base.EquipItem();

            audioFiltering.player = this.playerHeldBy;
            if (this.playerHeldBy == null)
            {
                MyLog.LogWarning(
                    $"Unable to continue, player is null... Owner must re-equip the item"
                );
                return;
            }

            this.playerHeldBy.equippedUsableItemQE = true;

            AudioMod.SetupGameobjects(this.playerHeldBy);

            UpdateTooltips();

            if (!isSynced)
            {
                MyLog.LogDebug($"Syncing mode...");
                SyncAudioModeToClientsServerRpc();
            }
        }

        /// <summary>
        /// Sync the audio filtering mode to all the clients
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SyncAudioModeToClientsServerRpc()
        {
            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                return;

            MyLog.LogInfo($"Sending RPC... mode='{audioFiltering.Mode}'");
            SyncAudioModeClientRpc(audioFiltering.Mode);
        }

        /// <summary>
        /// Sync the audio filtering mode called from server
        /// </summary>
        /// <param name="mode"></param>
        [ClientRpc]
        public void SyncAudioModeClientRpc(AudioFilteringMode mode)
        {
            MyLog.LogDebug("EventClientRpc() called");
            MyLog.LogDebug($"isSynced={isSynced}");

            if (!isSynced)
            {
                this.isSynced = true;
                audioFiltering.SetFilterMode(mode, false);
                UpdateTooltips();
                MyLog.LogInfo($"Synced to mode '{mode}'");
            }
        }

        protected void UpdateTooltips()
        {
            if (!IsOwner)
                return;

            AudioFilteringMode mode = audioFiltering.Mode;
            string name;
            switch (mode)
            {
                case AudioFilteringMode.Robot:
                    name = "Robot";
                    break;
                case AudioFilteringMode.Siren:
                    name = "Siren";
                    break;
                case AudioFilteringMode.Loud:
                    name = "Loud";
                    break;
                case AudioFilteringMode.HighPitch:
                    name = "High Pitch";
                    break;
                case AudioFilteringMode.LowPitch:
                    name = "Low Pitch";
                    break;
                default:
                    name = "Unknown";
                    break;
            }

            string[] tooltip = { "Change mode : [Q]", "Mode : " + name };

            this.itemProperties.toolTips = tooltip;
            HUDManager.Instance.ChangeControlTipMultiple(
                this.itemProperties.toolTips,
                true,
                this.itemProperties
            );
        }

        /// <summary>
        /// Called when the item is dropped
        /// </summary>
        public override void DiscardItem()
        {
            MyLog.LogDebug($"DiscardItem()");
            // Play sound on death
            //if (this.playerHeldBy.isPlayerDead && this.clientIsHoldingAndSpeakingIntoThis)
            //    this.BroadcastSFXFromWalkieTalkie(this.playerDieOnWalkieTalkieSFX, (int)this.playerHeldBy.playerClientId);
            this.playerHeldBy.equippedUsableItemQE = false;

            audioFiltering.Disable();
            if (this.playerHeldBy.isPlayerDead && this.isBeingUsed)
            {
                AudioMod.PlaySFX(this, AudioMod.SFX, TimesPlayedWithoutTurnedOff);
            }

            base.DiscardItem();
        }

        public override void Update()
        {
            base.Update();

            if (this.isBeingUsed)
            {
                TimesPlayedWithoutTurnedOff++;
                audioFiltering.PlaySirenAudibleNoiseIfApplicable(this, TimesPlayedWithoutTurnedOff);
            }
            else
            {
                TimesPlayedWithoutTurnedOff = 0;
            }

            // PlayAudibleNoise every second to alert ennemies around

            //// From boombox :
            //if (!this.isPlayingMusic)
            //    return;
            //if ((double)this.noiseInterval <= 0.0)
            //{
            //    this.noiseInterval = 1f;
            //    ++this.timesPlayedWithoutTurningOff;
            //    this.roundManager.PlayAudibleNoise(this.transform.position, 16f, 0.9f, this.timesPlayedWithoutTurningOff, noiseID: 5);
            //}
            //else
            //    this.noiseInterval -= Time.deltaTime;

            // From ship horn
            //if (this.hornBlaring)
            //{
            //    this.hornFar.volume = Mathf.Min(this.hornFar.volume + Time.deltaTime * 0.45f, 1f);
            //    this.hornFar.pitch = Mathf.Lerp(this.hornFar.pitch, 0.97f, Time.deltaTime * 0.8f);
            //    this.hornClose.volume = Mathf.Min(this.hornClose.volume + Time.deltaTime * 0.45f, 1f);
            //    this.hornClose.pitch = Mathf.Lerp(this.hornClose.pitch, 0.97f, Time.deltaTime * 0.8f);
            //    if ((double)this.hornClose.volume > 0.60000002384185791 && (double)this.playAudibleNoiseInterval <= 0.0)
            //    {
            //        this.playAudibleNoiseInterval = 1f;
            //        RoundManager.Instance.PlayAudibleNoise(this.hornClose.transform.position, 30f, 0.8f, this.timesPlayingAtOnce, noiseID: 14155);
            //        ++this.timesPlayingAtOnce;
            //    }
            //    else
            //        this.playAudibleNoiseInterval -= Time.deltaTime;
            //}
        }

        /// <summary>
        /// Called when the user activates the object
        /// </summary>
        /// <param name="used">State of the 'isBeingUsed' variable</param>
        /// <param name="buttonDown">Is the button currently pressed</param>
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            SwitchOnOff(buttonDown); // Hold to enable

            // Make it detectable by ennemies, do not actually play a sound
            //AudioSource audio = GetComponent<AudioSource>();
            //MyLog.LogDebug($"Found audio : {audio}");
            //if (audio != null)
            //{
            //    MyLog.LogDebug($"\t{audio.isActiveAndEnabled}");
            //    MyLog.LogDebug($"\t{audio.clip}");
            //    MyLog.LogDebug($"\t{audio.clip.name}");
            //    audio.Play();

            //    MyLog.LogDebug($"Found RoundManager {RoundManager.Instance}");
            //    MyLog.LogDebug($"Found isInElevator {isInElevator}");
            //    MyLog.LogDebug($"Found StartOfRound.Instance {StartOfRound.Instance}");
            //    MyLog.LogDebug(
            //        $"Found StartOfRound.Instance.hangarDoorsClosed {StartOfRound.Instance.hangarDoorsClosed}"
            //    );
            //    RoundManager.Instance.PlayAudibleNoise(
            //        transform.position,
            //        10f,
            //        0.7f,
            //        noiseIsInsideClosedShip: isInElevator && StartOfRound.Instance.hangarDoorsClosed
            //    );
            //}
        }

        /// <summary>
        /// Called when battery runs out
        /// </summary>
        public override void UseUpBatteries()
        {
            MyLog.LogDebug($"UseUpBatteries()");
            SwitchOnOff(false);
            base.UseUpBatteries();
            //this.SwitchFlashlight(false);
            //this.flashlightAudio.PlayOneShot(this.outOfBatteriesClip, 1f);
            //RoundManager.Instance.PlayAudibleNoise(
            //    this.transform.position,
            //    13f,
            //    0.65f,
            //    noiseIsInsideClosedShip: this.isInElevator && StartOfRound.Instance.hangarDoorsClosed
            //);
        }

        public void SwitchOnOff(bool on)
        {
            MyLog.LogDebug($"SwitchOnOff({on})");
            AudioMod.SetupGameobjects(this.playerHeldBy);

            isBeingUsed = on;

            if (!this.IsOwner)
            {
                if (this.playerHeldBy != null)
                {
                    bool res = on ? audioFiltering.Enable() : audioFiltering.Disable();
                    if (!res)
                    {
                        MyLog.LogError($"Unable to {(on ? "enable" : "disable")}");
                    }
                }
                else
                {
                    MyLog.LogWarning($"playerHeldBy: null");
                }
            }
            else
            {
                // Owner
                MyLog.LogInfo($"Megaphone click by owner");
                if (audioFiltering.Mode == AudioFilteringMode.Siren)
                {
                    bool res = on ? audioFiltering.Enable() : audioFiltering.Disable();
                    if (!res)
                    {
                        MyLog.LogError($"Unable to {(on ? "enable" : "disable")}");
                    }
                }
            }
        }
    }
}
