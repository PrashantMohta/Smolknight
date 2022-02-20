using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalEnums;
using static Satchel.GameObjectUtils;
using static SmolKnight.Utils;

namespace SmolKnight
{
   static class ActionPatcher{

       public static void OnRayCast2d(On.HutongGames.PlayMaker.Actions.RayCast2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.RayCast2d self){
            DebugLog("OnRayCast2d");
            GameObject fromObj = self.Fsm.GetOwnerDefaultTarget(self.fromGameObject);
            if(fromObj.name == "Knight"){
                self.distance.Value = 2f * SmolKnight.currentScale;
            }
            orig(self);
        }

        private static IEnumerator scaleFireballCoro(GameObject go){
            DebugLog("scaleFireballCoro");
            yield return null;
            go.scaleGO(SmolKnight.currentScale);
            var blast = go.FindGameObjectInChildren("Fireball Blast");
            var blastpos = blast.transform.position;
            if(SmolKnight.currentScale == Size.SMOL){
                blast.scaleGO(SmolKnight.currentScale * 2f);
            }
            var hgo = HeroController.instance.gameObject;
            var heroCollider = hgo.GetComponent<BoxCollider2D>();
            blastpos.y = heroCollider.bounds.center.y ;
            blastpos.x = heroCollider.bounds.center.x - (hgo.transform.localScale.x > 0 ? SmolKnight.currentScale : -SmolKnight.currentScale);
            blast.transform.position = blastpos;

        }
        public static void OnSpellSpawn(On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool self){
            DebugLog("OnSpellSpawn");
            if(self.gameObject.Value){
                var g = self.gameObject.Value;
                if(g.name.StartsWith("Fireball")) {
                    if(SmolKnight.currentScale == Size.SMOL){
                        var p = self.position.Value;
                        p.y = -0.3f;
                        self.position.Value = p;
                    }
                }
            }
            orig(self);
            var go = self.storeObject.Value;
            if(go.name.StartsWith("dream_gate_object")){
                //visually move the dreamgate when spawned 
                var pos = go.transform.position;
                if(SmolKnight.currentScale == Size.SMOL){
                    pos.y += Size.SMOL_OFFSET;
                }
                if(SmolKnight.currentScale == Size.BEEG){
                    pos.y -= Size.BEEG_OFFSET;
                }
                go.transform.position = pos;
            }
            if(go.name.StartsWith("Fireball")) {
                go.scaleGO(SmolKnight.currentScale);
                GameManager.instance.StartCoroutine(scaleFireballCoro(go));
            }
        }

        public static void DoSetScale(On.HutongGames.PlayMaker.Actions.SetScale.orig_DoSetScale orig, HutongGames.PlayMaker.Actions.SetScale self)
        {
            DebugLog("DoSetScale");

            orig(self);
            var go = self.Fsm.GetOwnerDefaultTarget(self.gameObject);
            if(go == null){
                return;
            }
            DebugLog(go.name);
            if (go == HeroController.instance.gameObject)
            {
                Knight.UpdateLocalPlayer();
            }
            if (go.name.StartsWith("Knight Spike Death")) // fix for spike squish
            {
                go.scaleGO(SmolKnight.currentScale); 
            } else if (go.name.StartsWith("SD Crystal")) // fix for Cdash?
            {
                go.scaleGO(SmolKnight.currentScale); 
            } else if (go.name.StartsWith("Knight Dream Arrival")) // fix for Entering dreams
            {
                go.scaleGO(SmolKnight.currentScale); 
            }
        }

        public static void CreateObject(On.HutongGames.PlayMaker.Actions.CreateObject.orig_OnEnter orig,HutongGames.PlayMaker.Actions.CreateObject self){
            DebugLog("CreateObject");
            orig(self);
            if(self.storeObject.Value.name.StartsWith("Shadow Ball")){ //shade fireball
                self.storeObject.Value.scaleGO(SmolKnight.saveSettings.shadeScale);
            }
        }
       
   }
}