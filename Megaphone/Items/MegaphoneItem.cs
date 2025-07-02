using System;
using System.Collections.Generic;
using System.Text;
using Megaphone.Scripts;
using UnityEngine;

namespace Megaphone.Items
{
    public class MegaphoneItem : GrabbableObject
    {
        public override void Start()
        {
            MyLog.Logger.LogDebug("MegaphoneItem item created !");
            base.Start();
            itemProperties.itemIsTrigger = false;
            itemProperties.batteryUsage = 120; // Number of seconds it can stay on
            itemProperties.syncUseFunction = true;
            itemProperties.syncInteractLRFunction = true;
            itemProperties.syncDiscardFunction = true;
            itemProperties.holdButtonUse = true;
            insertedBattery.charge = 1;
        }

        /// <summary>
        /// Q and E presses
        /// </summary>
        /// <param name="right">True if 'E', false if 'Q'</param>
        public override void ItemInteractLeftRight(bool right)
        {
            MyLog.Logger.LogDebug($"ItemInteractLeftRight({right})");
            base.ItemInteractLeftRight(right);
            if (right)
                return;
            AudioMod.SwitchFilterNextMode(this.playerHeldBy, this.isBeingUsed);
        }

        /// <summary>
        /// Called when the item is in the hotbar and becomes unselected
        /// </summary>
        public override void PocketItem()
        {
            MyLog.Logger.LogDebug($"PocketItem()");
            if (this.IsOwner && this.playerHeldBy != null)
            {
                this.playerHeldBy.equippedUsableItemQE = false;
                this.isBeingUsed = false;
            }
            base.PocketItem();
        }

        /// <summary>
        /// Called when the item is equiped (held in hand)
        /// This function is called on all clients
        /// </summary>
        public override void EquipItem()
        {
            MyLog.Logger.LogDebug($"EquipItem()");
            base.EquipItem();
            if (this.playerHeldBy == null)
                return;
            this.playerHeldBy.equippedUsableItemQE = true;
            AudioMod.SetupGameobjects(this.playerHeldBy);
        }

        /// <summary>
        /// Called when the item is dropped
        /// </summary>
        public override void DiscardItem()
        {
            MyLog.Logger.LogDebug($"DiscardItem()");
            // Play sound on death
            //if (this.playerHeldBy.isPlayerDead && this.clientIsHoldingAndSpeakingIntoThis)
            //    this.BroadcastSFXFromWalkieTalkie(this.playerDieOnWalkieTalkieSFX, (int)this.playerHeldBy.playerClientId);
            this.playerHeldBy.equippedUsableItemQE = false;
            base.DiscardItem();
        }

        public override void Update()
        {
            base.Update();

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
            MyLog.Logger.LogDebug($"ItemActivate({used}, {buttonDown})");
            SwitchOnOff(buttonDown); // Hold to enable

            if (!this.IsOwner)
            {
                if (this.playerHeldBy != null)
                {
                    MyLog.Logger.LogInfo(
                        $"Megaphone click. playerheldby: {this.playerHeldBy.name}"
                    );
                    AudioMod.SwitchFilterOnOff(this.playerHeldBy, buttonDown);
                }
                else
                {
                    MyLog.Logger.LogInfo($"Megaphone click. playerheldby: null");
                }
            }
            else
            {
                MyLog.Logger.LogDebug($"Megaphone click by owner");
            }

            // Make it detectable by ennemies, do not actually play a sound
            //AudioSource audio = GetComponent<AudioSource>();
            //MyLog.Logger.LogDebug($"Found audio : {audio}");
            //if (audio != null)
            //{
            //    MyLog.Logger.LogDebug($"\t{audio.isActiveAndEnabled}");
            //    MyLog.Logger.LogDebug($"\t{audio.clip}");
            //    MyLog.Logger.LogDebug($"\t{audio.clip.name}");
            //    audio.Play();

            //    MyLog.Logger.LogDebug($"Found RoundManager {RoundManager.Instance}");
            //    MyLog.Logger.LogDebug($"Found isInElevator {isInElevator}");
            //    MyLog.Logger.LogDebug($"Found StartOfRound.Instance {StartOfRound.Instance}");
            //    MyLog.Logger.LogDebug(
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
            MyLog.Logger.LogDebug($"UseUpBatteries()");
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

        public void SwitchOnOff(bool state)
        {
            MyLog.Logger.LogDebug($"SwitchOnOff({state})");
            AudioMod.SetupGameobjects(this.playerHeldBy);

            isBeingUsed = state;
            // Do stuff here
        }
    }
}
