using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalEnums;

namespace SmolKnight
{
   static class Shade{
        
        public static void OnHeroDeath() {
            SmolKnight.saveSettings.shadeScale = SmolKnight.currentScale;
            UpdateShade();
        }
        public static void UpdateShade(){
            SceneManager sm = GameManager.instance.GetSceneManager().GetComponent<SceneManager>();
            sm.hollowShadeObject.scaleGO(SmolKnight.saveSettings.shadeScale);
        }
       
   }
}