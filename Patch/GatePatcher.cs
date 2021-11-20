using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalEnums;
using static SmolKnight.Utils;

namespace SmolKnight
{
   static class GatePatcher{
       
        private static TransitionPoint originalGate;
        private static Vector3 originalGatePosition;
        public static IEnumerator EnterScene(On.HeroController.orig_EnterScene orig,HeroController self, TransitionPoint enterGate, float delayBeforeEnter){
            DebugLog("EnterScene");
            float AdditionalMovex = 0, AdditionalMovey = 0;
            var gateposition = enterGate.GetGatePosition();
            originalGate = enterGate;
            originalGatePosition = enterGate.transform.position;
            //This is needed because beeg knight can go into infinite loading scene loop because its beeg    
            if (SmolKnight.currentScale == Size.BEEG) 
            {
                if (gateposition == GatePosition.left) {
                    AdditionalMovex = Size.BEEG_OFFSET;
                } else if (gateposition == GatePosition.right) {
                    AdditionalMovex = -Size.BEEG_OFFSET;
                }

                if (gateposition == GatePosition.bottom) {
                    AdditionalMovey = Size.BEEG_OFFSET;
                }
                enterGate.transform.position = enterGate.transform.position + new Vector3(AdditionalMovex, AdditionalMovey,0f);
            }
            
            var wait = orig(self,enterGate,delayBeforeEnter);
            Knight.UpdateLocalPlayer();
            Knight.CheckRemotePlayers(true);
            Shade.UpdateShade();
            yield return wait;
        }

        public static void FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig,HeroController self,bool setHazardMarker, bool preventRunBool){
            DebugLog("FinishedEnteringScene");
            if(originalGate != null){
                originalGate.transform.position = originalGatePosition;
            }
            originalGate = null; // do not keep this gate anymore
            orig(self,setHazardMarker,preventRunBool);
        }
   }
}