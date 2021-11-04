using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalEnums;
using static SmolKnight.Utils;

namespace SmolKnight
{
   static class Shade{
        
        public static void OnHeroDeath() {
            DebugLog("OnHeroDeath");
            SmolKnight.saveSettings.shadeScale = SmolKnight.currentScale;
            UpdateShade();
        }
        public static void UpdateShade(){
            DebugLog("UpdateShade");
            SceneManager sm = GameManager.instance.GetSceneManager().GetComponent<SceneManager>();
            sm.hollowShadeObject.scaleGO(SmolKnight.saveSettings.shadeScale);
        }
       
   }
}