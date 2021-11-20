using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalEnums;
using static SmolKnight.Utils;

namespace SmolKnight
{
   static class HeroControllerPatcher{
        public static void FaceLeft(On.HeroController.orig_FaceLeft orig, HeroController self)
        {
            DebugLog("FaceLeft");
            orig(self);
            Knight.UpdateLocalPlayer();
        }
        public static void FaceRight(On.HeroController.orig_FaceRight orig, HeroController self)
        {
            DebugLog("FaceRight");
            orig(self);
            Knight.UpdateLocalPlayer();
        }

        public static float FindGroundPointY(On.HeroController.orig_FindGroundPointY orig,HeroController self,float x, float y,bool useExtended){
            DebugLog("FindGroundPointY");
           //This is needed to get smol knight on the floor
           var posY = orig(self,x, y,useExtended);
           if (SmolKnight.currentScale == Size.SMOL) 
            {
                posY -= 0.3f;
            }
            return posY;
        }
        public static Vector3 FindGroundPoint(On.HeroController.orig_FindGroundPoint orig,HeroController self,Vector2 startPoint,bool useExtended){
           DebugLog("FindGroundPoint");
           //This is needed to get smol knight on the floor
           var pos = orig(self,startPoint,useExtended);
           if (SmolKnight.currentScale == Size.SMOL) 
            {
                pos.y -= 0.3f;
            }
            return pos;
        }
   }
}