using Megaphone.Scripts;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Megaphone.Items
{
    public class MegaphoneItem : GrabbableObject
    {
        public int audioMode;

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
            audioMode = 0;
        }

        /// <summary>
        /// Q and E presses
        /// </summary>
        /// <param name="right">True if 'E', false if 'Q'</param>
        public override void ItemInteractLeftRight(bool right)
        {
            MyLog.Logger.LogDebug($"ItemInteractLeftRight({right})");
            base.ItemInteractLeftRight(right);
            // Switch mode
            MyLog.Logger.LogDebug($"Device used : {isBeingUsed}");
            MyLog.Logger.LogDebug($"right/left : {(right ? "1" : "0")}");
            if (right)
                return;
            audioMode++;
            audioMode = (audioMode < 3) ? audioMode : 0;
            MyLog.Logger.LogInfo($"Switched to mode {audioMode}");
            if (this.isBeingUsed)
            {
                // Replace audio ; Disable previous and enable current (if being used)
                // BUT need to sync with the clients !!
                // Therefore, required to use serverRPC and clientRPC
                // BUT : the base is : 
                /*
                 *    public void ItemInteractLeftRightOnClient(bool right)
                      {
                        if (!this.IsOwner)
                        {
                          Debug.Log((object) "InteractLeftRight was called but player was not the owner.");
                        }
                        else
                        {
                          if (this.RequireCooldown() || !this.UseItemBatteries(true))
                            return;
                          this.ItemInteractLeftRight(right);
                          if (!this.itemProperties.syncInteractLRFunction)
                            return;
                          ++this.isSendingItemRPC;
                          this.InteractLeftRightServerRpc(right);
                        }
                      }
                 */
            }
        }

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
        public override void EquipItem()
        {
            MyLog.Logger.LogDebug($"EquipItem()");
            base.EquipItem();
            this.playerHeldBy.equippedUsableItemQE = true;
        }

        public override void DiscardItem()
        {
            MyLog.Logger.LogDebug($"DiscardItem()");
            // Play sound on death
            //if (this.playerHeldBy.isPlayerDead && this.clientIsHoldingAndSpeakingIntoThis)
            //    this.BroadcastSFXFromWalkieTalkie(this.playerDieOnWalkieTalkieSFX, (int)this.playerHeldBy.playerClientId);
            this.playerHeldBy.equippedUsableItemQE = false;
            base.DiscardItem();
        }

        /// <summary>
        /// Used when the user activates the object
        /// </summary>
        /// <param name="used">State of the 'isBeingUsed' variable</param>
        /// <param name="buttonDown">Is the button currently pressed</param>
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            MyLog.Logger.LogDebug($"ItemActivate({used}, {buttonDown})");
            SwitchOnOff(buttonDown); // Hold to enable

            if (!this.IsOwner)
            {
                MyLog.Logger.LogInfo($"Flashlight click. playerheldby: {this.playerHeldBy}");
                if (this.playerHeldBy != null)
                {
                    if (used)
                    {
                        AudioMod.EnableRobotVoice(this.playerHeldBy);
                    }
                    else
                    {
                        AudioMod.DisableRobotVoice(this.playerHeldBy);
                    }
                }
            }
            else
            {
                MyLog.Logger.LogInfo($"Flashlight click by owner");
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
            isBeingUsed = state;
            // Do stuff here
        }
    }
}
