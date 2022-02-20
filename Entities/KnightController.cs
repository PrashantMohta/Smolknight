using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;


namespace SmolKnight{
    public class KnightController : MonoBehaviour{

        public DateTime lastCheckTime = DateTime.Now.AddMilliseconds(-5000);    

        private static void nextScale() {
            if(!SmolKnight.saveSettings.enableSwitching || HKMP.isEnabledWithUserName()) { 
                return;
            }

            if(SmolKnight.currentScale == Size.SMOL){
                SmolKnight.currentScale = Size.NORMAL;
            } else if(SmolKnight.currentScale == Size.NORMAL){
                SmolKnight.currentScale = Size.BEEG;
            } else if(SmolKnight.currentScale == Size.BEEG){
                SmolKnight.currentScale = Size.SMOL;
            }
        }
        
        public void applyTransformation(){
            if(HeroController.instance == null || GameManager.instance.isPaused) { 
                lastCheckTime = DateTime.Now.AddMilliseconds(-5000);
                return; 
            } 
            Knight.UpdateLocalPlayer();
            Knight.PlayTransformEffects();
            SmolKnight.setSaveSettings();
            lastCheckTime = DateTime.Now;
        }
        public void Update(){
            if(!SmolKnight.saveSettings.startupSelection && GameManager.instance.IsGameplayScene() && HeroController.instance.cState.onGround && Input.anyKey){
                SmolKnight.startUpScreen();
            }

            if (!GameManager.instance.isPaused && (SmolKnight.settings.keybinds.Transform.WasPressed || SmolKnight.settings.buttonbinds.Transform.WasPressed))
            {
                nextScale();
                applyTransformation();
                BetterMenu.MenuRef.Update();
            }
            Knight.CheckRemotePlayers(false);
            var currentTime = DateTime.Now;
            if ((currentTime - lastCheckTime).TotalMilliseconds > 5000) {
                Knight.UpdateLocalPlayer();
                lastCheckTime = currentTime;
            }
        }
    }
}