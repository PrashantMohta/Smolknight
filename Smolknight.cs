using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;

using Modding;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;


namespace SmolKnight
{
    public static class Utils {

        public static FsmStateAction _GetAction(PlayMakerFSM fsm, string stateName, int index)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);

                return actions[index];
            }

            return null;
        }

        public static T _GetAction<T>(PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction
        {
            return GetAction(fsm, stateName, index) as T;
        }
        public static FsmStateAction GetAction(this PlayMakerFSM fsm, string stateName, int index) =>
            Utils._GetAction(fsm, stateName, index);

        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction =>
            Utils._GetAction<T>(fsm, stateName, index); 

        public static GameObject FindGameObjectInChildren( this GameObject gameObject, string name )
        {
            if( gameObject == null )
                return null;

            foreach( var t in gameObject.GetComponentsInChildren<Transform>( true ) )
            {
                if( t.name == name )
                    return t.gameObject;
            }
            return null;
        }
    }   
    public class SmolKnight : Mod
    {

        internal static SmolKnight Instance;

        public float currentScale = 0.5f;

        public bool isHKMP = false;

        public bool currentPlayerIsSmol = false;
        public override string GetVersion()
        {
            return "v1.4H";
        }
        
        public override void Initialize()
        {
            Instance = this;

            ShineyItems.Add("Fungus2_14",19.3f); // Mantis Claw
            ShineyItems.Add("Ruins1_30",49.3f); // Spell Twister
            ShineyItems.Add("Deepnest_32",2.2f); // Pale Ore
            ShineyItems.Add("Hive_05",12.0f);  // Hive Blood
            ShineyItems.Add("Room_Wyrm",6.9f); // Kings Brand
            ShineyItems.Add("RestingGrounds_10",17.1f); // Soul Eater
            ShineyItems.Add("Mines_11",39.5f); // ShopKeeper's Key
            ShineyItems.Add("Fungus3_39",35f); // Love Key
            ShineyItems.Add("Abyss_17",14.2f);  // Pale Ore 

            ModHooks.HeroUpdateHook += heroUpdate;

            On.HeroController.FaceLeft += FaceLeft;
            On.HeroController.FaceRight += FaceRight;
            On.HutongGames.PlayMaker.Actions.SetScale.DoSetScale += DoSetScale;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += FixSceneSpecificThings;
        }

        private Dictionary<string,float> ShineyItems = new Dictionary<string,float>();

        private IEnumerator FixShinyItemStand(Scene scene){
            yield return null;
            float ShineyPos;
            if (ShineyItems.TryGetValue(scene.name, out ShineyPos))
            {
                var Shiney = GameObject.Find("Shiny Item Stand");
                if(Shiney == null){
                    this.Log("Shiny not found in scene : " + scene.name);
                } else {
                    var pos = Shiney.transform.position;
                        pos.y = ShineyPos;
                        Shiney.transform.position  = pos;
                }
            } 
        }
        public void FixSceneSpecificThings(Scene scene,LoadSceneMode mode){
            GameManager.instance.StartCoroutine(FixShinyItemStand(scene));
        }

        public void setVignette(float factor){
            
            var fsm = HeroController.instance.transform.Find("Vignette").gameObject.LocateMyFSM("Darkness Control");
            if(fsm == null){
                this.Log("no fsm");
            }

            if(false && fsm.ActiveStateName == "Normal" || fsm.ActiveStateName == "Normal 2"){
                fsm.FsmVariables.FindFsmVector3("Damage Scale").Value = new Vector2(4f * factor,4f * factor);
                fsm.FsmVariables.FindFsmVector3("Idle Scale").Value = new Vector2(5.5f * factor,5.5f * factor);
            }
            
            fsm.GetAction<SetVector3XYZ>("Dark -1", 1).x.Value = 10.5f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark -1", 1).y.Value = 10.5f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark -1", 1).z.Value = 10.5f * factor;

            fsm.GetAction<SetVector3XYZ>("Dark -1", 2).x.Value = 5f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark -1", 2).y.Value = 5f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark -1", 2).z.Value = 5f * factor;

            fsm.GetAction<SetVector3XYZ>("Dark -1 2", 0).x.Value = 10.5f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark -1 2", 0).y.Value = 10.5f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark -1 2", 0).z.Value = 10.5f * factor;

            fsm.GetAction<SetVector3XYZ>("Dark -1 2", 1).x.Value = 5f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark -1 2", 1).y.Value = 5f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark -1 2", 1).z.Value = 5f * factor;

            fsm.GetAction<SetVector3XYZ>("Normal", 1).x.Value = 5f * factor;
            fsm.GetAction<SetVector3XYZ>("Normal", 1).y.Value = 5f * factor;
            fsm.GetAction<SetVector3XYZ>("Normal", 1).z.Value = 5f * factor;

            fsm.GetAction<SetVector3XYZ>("Normal", 3).x.Value = 4f * factor;
            fsm.GetAction<SetVector3XYZ>("Normal", 3).y.Value = 4f * factor;
            fsm.GetAction<SetVector3XYZ>("Normal", 3).z.Value = 4f * factor;

            fsm.GetAction<SetVector3XYZ>("Normal 2", 1).x.Value = 5f * factor;
            fsm.GetAction<SetVector3XYZ>("Normal 2", 1).y.Value = 5f * factor;
            fsm.GetAction<SetVector3XYZ>("Normal 2", 1).z.Value = 5f * factor;

            fsm.GetAction<SetVector3XYZ>("Normal 2", 2).x.Value = 4f * factor;
            fsm.GetAction<SetVector3XYZ>("Normal 2", 2).y.Value = 4f * factor;
            fsm.GetAction<SetVector3XYZ>("Normal 2", 2).z.Value = 4f * factor;

            fsm.GetAction<SetVector3XYZ>("Dark 1", 1).x.Value = 2.2f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 1", 1).y.Value = 2.2f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 1", 1).z.Value = 2.2f * factor;

            fsm.GetAction<SetVector3XYZ>("Dark 1", 2).x.Value = 1.9f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 1", 2).y.Value = 1.9f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 1", 2).z.Value = 1.9f * factor;


            fsm.GetAction<SetVector3XYZ>("Dark 1 2", 1).x.Value = 2.2f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 1 2", 1).y.Value = 2.2f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 1 2", 1).z.Value = 2.2f * factor;

            fsm.GetAction<SetVector3XYZ>("Dark 1 2", 2).x.Value = 1.9f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 1 2", 2).y.Value = 1.9f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 1 2", 2).z.Value = 1.9f * factor;

            fsm.GetAction<SetVector3XYZ>("Dark 2", 1).x.Value = 0.8f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 2", 1).y.Value = 0.8f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 2", 1).z.Value = 0.8f * factor;

            fsm.GetAction<SetVector3XYZ>("Dark 2", 2).x.Value = 0.8f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 2", 2).y.Value = 0.8f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 2", 2).z.Value = 0.8f * factor;


            fsm.GetAction<SetVector3XYZ>("Dark 2 2", 1).x.Value = 0.8f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 2 2", 1).y.Value = 0.8f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 2 2", 1).z.Value = 0.8f * factor;

            fsm.GetAction<SetVector3XYZ>("Dark 2 2", 2).x.Value = 0.8f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 2 2", 2).y.Value = 0.8f * factor;
            fsm.GetAction<SetVector3XYZ>("Dark 2 2", 2).z.Value = 0.8f * factor;

            fsm.GetAction<SetVector3XYZ>("Lantern", 0).x.Value = 3f * factor;
            fsm.GetAction<SetVector3XYZ>("Lantern", 0).y.Value = 3f * factor;
            fsm.GetAction<SetVector3XYZ>("Lantern", 0).z.Value = 3f * factor;

            fsm.GetAction<SetVector3XYZ>("Lantern", 2).x.Value = 2.2f * factor;
            fsm.GetAction<SetVector3XYZ>("Lantern", 2).y.Value = 2.2f * factor;
            fsm.GetAction<SetVector3XYZ>("Lantern", 2).z.Value = 2.2f * factor;

            fsm.GetAction<SetVector3XYZ>("Lantern 2", 0).x.Value = 3f * factor;
            fsm.GetAction<SetVector3XYZ>("Lantern 2", 0).y.Value = 3f * factor;
            fsm.GetAction<SetVector3XYZ>("Lantern 2", 0).z.Value = 3f * factor;

            fsm.GetAction<SetVector3XYZ>("Lantern 2", 2).x.Value = 2.2f * factor;
            fsm.GetAction<SetVector3XYZ>("Lantern 2", 2).y.Value = 2.2f * factor;
            fsm.GetAction<SetVector3XYZ>("Lantern 2", 2).z.Value = 2.2f * factor;

        }

        public void smol(Transform transform)
        {
            var localScale = transform.localScale;
            var x = currentScale;
            var y = currentScale;
            if (localScale.x < 0)
            {
                x = -currentScale;
            }
            if (localScale.y < 0)
            {
                y = -currentScale;
            }
            if (localScale.x != x || localScale.y != y) { 
                transform.localScale = new Vector3(x, y, 1f);
            }

            //if we need to increase the light 
            //var LightControl = transform.Find("HeroLight").gameObject.LocateMyFSM("HeroLight Control");
            //LightControl.FsmVariables.FindFsmVector3("Damage Scale").Value = new Vector2(1.5f * 1.5f,1.5f * 1.5f);
            //LightControl.FsmVariables.FindFsmVector3("Idle Scale").Value = new Vector2(3f * 1.5f,3f * 1.5f);

            if(!transform.gameObject.name.StartsWith("Player Container")){
                setVignette(2f);
            }      

        }
        public void normal(Transform transform)
        {
            var localScale = transform.localScale;
            var x = 1f;
            var y = 1f;
            if (localScale.x < 0)
            {
                x = -1f;
            }
            if (localScale.y < 0)
            {
                y = -1f;
            }
            if (localScale.x != x || localScale.y != y) { 
                transform.localScale = new Vector3(x, y, 1f);
            }

            if(!transform.gameObject.name.StartsWith("Player Container")){
                setVignette(1f);
            }
        }
        
        public void UpdateHKMPPlayers(){
            foreach(GameObject gameObj in GameObject.FindObjectsOfType<GameObject>())
            {
                if(gameObj.name.StartsWith("Player Container"))
                {
                   var name = gameObj.transform.Find("Username");
                   if( name == null){continue;}
                   var tmp = name.gameObject.GetComponent<TextMeshPro>();
                   if(tmp.text.Contains("SMOL")){
                        smol(gameObj.transform);
                   } else {
                       normal(gameObj.transform);
                   }
                }
            }
        }
        public void UpdatePlayer(){
            var playerTransform = HeroController.instance.gameObject.transform;
            var hkmpUsername = playerTransform.Find("Username");
            currentPlayerIsSmol = ( (playerTransform.localScale.x*playerTransform.localScale.x) < 0.5f ) && ((playerTransform.localScale.y*playerTransform.localScale.y) < 0.5f );
            if( hkmpUsername != null){
                isHKMP = true;
                var tmp = hkmpUsername.gameObject.GetComponent<TextMeshPro>();
                if(!tmp.text.Contains("SMOL") && currentPlayerIsSmol ){
                    playerTransform.position = playerTransform.position + new Vector3(0,1f,0);
                    normal(playerTransform);
                } else if(tmp.text.Contains("SMOL") && !currentPlayerIsSmol ){
                    smol(playerTransform);
                }
            } else {
                smol(playerTransform);
            }
        }
        public DateTime lastHKMPCheckTime = DateTime.Now;
        public DateTime lastCheckTime = DateTime.Now;

        public void heroUpdate()
        {
            var currentTime = DateTime.Now;
            if (isHKMP == true && (currentTime - this.lastHKMPCheckTime).TotalMilliseconds > 1000) {
                UpdateHKMPPlayers();
                this.lastHKMPCheckTime = currentTime;
            }
            if ((currentTime - this.lastCheckTime).TotalMilliseconds > 5000) {
                UpdatePlayer();
                this.lastCheckTime = currentTime;
            }     
        }

        

        private void FaceRight(On.HeroController.orig_FaceRight orig, HeroController self)
        {
            orig(self); 
            UpdatePlayer();
        }
        private void FaceLeft(On.HeroController.orig_FaceLeft orig, HeroController self)
        {
            orig(self); 
            UpdatePlayer();
        }
        public void DoSetScale(On.HutongGames.PlayMaker.Actions.SetScale.orig_DoSetScale orig, HutongGames.PlayMaker.Actions.SetScale self){
            orig(self);
            if(self.gameObject != null && self.gameObject.GameObject != null && self.gameObject.GameObject.Value != null && self.gameObject.GameObject.Value == HeroController.instance.gameObject){
                UpdatePlayer();
            }
        }

    }

}
