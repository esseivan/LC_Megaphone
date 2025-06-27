using System;
using System.Collections.Generic;
using System.Text;
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
            itemProperties.batteryUsage = 25;
        }

        /// <summary>
        /// Used when the user activates the object
        /// </summary>
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            MyLog.Logger.LogDebug("ItemActivate() called !");

            // Make it detectable by ennemies, do not actually play a sound
            AudioSource audio = GetComponent<AudioSource>();
            MyLog.Logger.LogDebug($"Found audio : {audio}");
            if (audio != null)
            {
                MyLog.Logger.LogDebug($"\t{audio.isActiveAndEnabled}");
                MyLog.Logger.LogDebug($"\t{audio.clip}");
                MyLog.Logger.LogDebug($"\t{audio.clip.name}");
                audio.Play();

                MyLog.Logger.LogDebug($"Found RoundManager {RoundManager.Instance}");
                MyLog.Logger.LogDebug($"Found isInElevator {isInElevator}");
                MyLog.Logger.LogDebug($"Found StartOfRound.Instance {StartOfRound.Instance}");
                MyLog.Logger.LogDebug(
                    $"Found StartOfRound.Instance.hangarDoorsClosed {StartOfRound.Instance.hangarDoorsClosed}"
                );
                RoundManager.Instance.PlayAudibleNoise(
                    transform.position,
                    10f,
                    0.7f,
                    noiseIsInsideClosedShip: isInElevator && StartOfRound.Instance.hangarDoorsClosed
                );
            }
        }

        /// <summary>
        /// Called when battery runs out
        /// </summary>
        public override void UseUpBatteries()
        {
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
            isBeingUsed = state;
        }
    }
}
