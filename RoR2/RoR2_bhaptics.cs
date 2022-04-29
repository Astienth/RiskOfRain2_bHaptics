using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using TactsuitVRbHaptics;
using RoR2;

namespace RoR2_bhaptics
{
    [BepInPlugin("org.bepinex.plugins.RoR2_bhaptics", "RoR2 bhaptics integration", "1.0")]
    public class RoR2_bhaptics_Plugin : BaseUnityPlugin
    {
#pragma warning disable CS0109 // Remove unnecessary warning
        internal static new ManualLogSource Log;
#pragma warning restore CS0109
        public static TactsuitVR tactsuitVr;
        public static Vector3 playerPosition;


        private void Awake()
        {
            // Make my own logger so it can be accessed from the Tactsuit class
            Log = base.Logger;
            // Plugin startup logic
            Logger.LogMessage("Plugin RoR2_bhaptics is loaded!");
            tactsuitVr = new TactsuitVR();
            // one startup heartbeat so you know the vest works correctly
            tactsuitVr.PlaybackHaptics("HeartBeat");
            // patch all functions
            var harmony = new Harmony("bhaptics.patch.RoR2");
            harmony.PatchAll();
        }

/*
        private static KeyValuePair<float, float> getAngleAndShift(FistVR.FVRPlayerBody player, Vector3 hit)
        {
            // bhaptics starts in the front, then rotates to the left. 0° is front, 90° is left, 270° is right.
            // y is "up", z is "forward" in local coordinates
            Vector3 patternOrigin = new Vector3(0f, 0f, 1f);
            Vector3 hitPosition = hit - player.TorsoTransform.position;
            Quaternion PlayerRotation = player.TorsoTransform.rotation;
            Vector3 playerDir = PlayerRotation.eulerAngles;
            // get rid of the up/down component to analyze xz-rotation
            Vector3 flattenedHit = new Vector3(hitPosition.x, 0f, hitPosition.z);

            // get angle. .Net < 4.0 does not have a "SignedAngle" function...
            float earlyhitAngle = Vector3.Angle(flattenedHit, patternOrigin);
            // check if cross product points up or down, to make signed angle myself
            Vector3 earlycrossProduct = Vector3.Cross(flattenedHit, patternOrigin);
            if (earlycrossProduct.y > 0f) { earlyhitAngle *= -1f; }
            // relative to player direction
            float myRotation = earlyhitAngle - playerDir.y;
            // switch directions (bhaptics angles are in mathematically negative direction)
            myRotation *= -1f;
            // convert signed angle into [0, 360] rotation
            if (myRotation < 0f) { myRotation = 360f + myRotation; }

            // up/down shift is in y-direction
            float hitShift = hitPosition.y;
            // in H3VR, the TorsoTransform has y=0 at the neck,
            // and the torso ends at roughly -0.5 (that's in meters)
            // so cap the shift to [-0.5, 0]...
            if (hitShift > 0.0f) { hitShift = 0.5f; }
            else if (hitShift < -0.5f) { hitShift = -0.5f; }
            // ...and then spread/shift it to [-0.5, 0.5]
            else { hitShift = (hitShift + 0.5f) * 2.0f - 0.5f; }

            // No tuple returns available in .NET < 4.0, so this is the easiest quickfix
            return new KeyValuePair<float, float>(myRotation, hitShift);
        }*/

        [HarmonyPatch(typeof(EntityStates.GenericCharacterMain), "ApplyJumpVelocity")]
        public class bhaptics_Jump
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (tactsuitVr.suitDisabled) return;
                tactsuitVr.PlaybackHaptics("HeartBeat");
            }
        }

    }
}

