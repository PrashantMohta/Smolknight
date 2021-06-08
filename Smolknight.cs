using System;
using Modding;
using UnityEngine;

namespace SmolKnight
{
    
    public class SmolKnight : Mod
    {

        internal static SmolKnight Instance;

        public float currentScale = 0.5f;

        public override string GetVersion()
        {
            return "v1.0";
        }

        public override void Initialize()
        {
            Instance = this;

            ModHooks.HeroUpdateHook += smol;
            ModHooks.AfterTakeDamageHook += updateSize;
            ModHooks.AfterPlayerDeadHook += AfterPlayerDied;
            ModHooks.SceneChanged += SceneLoaded;

        }
        public void SceneLoaded(string targetScene){
            resize();

        }
       
        public void resize() {
            this.currentScale = 0.5f;
        }
        public void AfterPlayerDied(){
            resize();
        }
        public int updateSize(int hazardType, int damageAmount) {
            resize();
            return damageAmount;
        }
        public void smol()
        {
        
            var localScale = HeroController.instance.gameObject.transform.localScale;
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
                HeroController.instance.gameObject.transform.localScale = new Vector3(x, y, 1f);
            }
            
        }

    }

}
