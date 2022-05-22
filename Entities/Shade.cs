namespace SmolKnight
{
    static class Shade{
        public static List<string> particleObjectNames = new List<string>{
            "Shade Particles",
            "Reform Particles",
            "Retreat Particles",
            "Charge Particles",
            "Depart Particles",
            "Quake Particles"
        };

        public static float MAX_PARTICLE_SIZE = 0.5f;
        public static void OnHeroDeath() {
            DebugLog("OnHeroDeath");
            SmolKnight.saveSettings.shadeScale = SmolKnight.currentScale;
            UpdateShade();
        }

        public static void ScaleShadeParticles(GameObject shade,string name){
            var emitter = shade.FindGameObjectInChildren(name);
            if(emitter != null){
               var myParticleSystem = emitter.GetComponent<ParticleSystem>();
               var emissionModule = myParticleSystem.sizeOverLifetime;
               if(SmolKnight.saveSettings.shadeScale == Size.SMOL){
                   emissionModule.size = new ParticleSystem.MinMaxCurve(0.0f, 0.5f);
               } else {
                    emissionModule.size = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
               }
            }
        }

        public static void UpdateShade(){
            DebugLog("UpdateShade");
            SceneManager sm = GameManager.instance.GetSceneManager().GetComponent<SceneManager>();
            sm.hollowShadeObject.scaleGO(SmolKnight.saveSettings.shadeScale);
            foreach(var name in particleObjectNames){
                ScaleShadeParticles(sm.hollowShadeObject,name);
            }
        }
       
   }
}