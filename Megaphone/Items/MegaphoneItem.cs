using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Megaphone.Items
{
    public class MegaphoneItem : PhysicsProp
    {
        public override void Start()
        {
            base.Start();
            itemProperties.itemIsTrigger = false;
            itemProperties.batteryUsage = 25;
        }

        /// <summary>
        /// Used when the user activates the object
        /// </summary>
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            // Make it detectable by ennemies, do not actually play a sound
            AudioSource audio = this.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
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
