namespace SmolKnight
{
    public static class SFX{
        private static AudioSource transformSource;
        public static void PlayTransformSound(){
            if(transformSource == null){
                var soundGO = HeroController.instance.gameObject.FindGameObjectInChildren("Dash");
                transformSource = soundGO.GetComponent<AudioSource>();
            }
            transformSource.Play();
        }
        public static void ChangePitch(){
            DebugLog("ChangePitch");
            //pitches up or down the sounds of the hero based on size
            AudioSource[] audioSources = HeroController.instance.GetComponentsInChildren<AudioSource>();
            foreach(AudioSource source in audioSources){          
                if(SmolKnight.GetCurrentScale() == Size.SMOL){
                    source.pitch = 1.5f;
                    HeroController.instance.checkEnvironment();
                    if(source.name == "FootstepsWalk" || source.name == "FootstepsRun"){
                        source.volume = 1f;
                    }
                } else if(SmolKnight.GetCurrentScale() == Size.NORMAL){
                    source.pitch = 1f;
                    HeroController.instance.checkEnvironment();
                    if(source.name == "FootstepsWalk" || source.name == "FootstepsRun"){
                        source.volume = 1f;
                    }
                } else if(SmolKnight.GetCurrentScale() == Size.BEEG){
                    source.pitch = 0.8f;
                    if(source.name == "FootstepsWalk"){
                        source.pitch = 0.7f;
                        source.volume = 1.5f;
                    }
                    if(source.name == "FootstepsRun"){
                        source.pitch = 0.8f;
                        source.volume = 1.5f;

                    }
                }
            }
        }
    }
}