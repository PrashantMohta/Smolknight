using System;
using Modding;
using UnityEngine;
using TMPro;

namespace SmolKnight
{
    
    public class SmolKnight : Mod
    {

        internal static SmolKnight Instance;

        public float currentScale = 0.5f;

        public bool isHKMP = false;

        public bool currentPlayerIsSmol = false;
        public override string GetVersion()
        {
            return "v1.0H";
        }
        
        public override void Initialize()
        {
            Instance = this;
            ModHooks.HeroUpdateHook += heroUpdate;
            On.HeroController.FaceLeft += FaceLeft;
            On.HeroController.FaceRight += FaceRight;
            On.HutongGames.PlayMaker.Actions.SetScale.DoSetScale += DoSetScale;
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
