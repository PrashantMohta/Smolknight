namespace SmolKnight
{
    public static class PlayerDataPatcher
    {
        public static readonly string DREAMGATE_Y = "dreamGateY";
        public static readonly string SMOLKNIGHT_DREAMGATE_Y = $"SmolKnight.{DREAMGATE_Y}";
        public static readonly string SMOLKNIGHT_HAS_SMOL = $"SmolKnight.hasSmol";
        public static readonly string SMOLKNIGHT_HAS_NORMAL = $"SmolKnight.hasNormal";
        public static readonly string SMOLKNIGHT_HAS_BEEG = $"SmolKnight.hasBeeg";

        public static bool GetBool(string name){
            if(!HeroController.instance){
                return false;
            }
            return HeroController.instance.playerData.GetBool(name);
        }
        public static bool hasScale(float scale){
            
            if(scale == Size.SMOL){
                return GetBool(SMOLKNIGHT_HAS_SMOL);
            } else if(scale == Size.NORMAL){
                return GetBool(SMOLKNIGHT_HAS_NORMAL);
            } else if(scale == Size.BEEG){
                return GetBool(SMOLKNIGHT_HAS_BEEG);
            }
            return false;
        }

        public static bool GetPlayerBool(string name,bool orig){
            DebugLog("GetPlayerBool");
            if(name == SMOLKNIGHT_HAS_SMOL){
                return SmolKnight.saveSettings.hasSmol;
            } else if(name == SMOLKNIGHT_HAS_NORMAL){
                return SmolKnight.saveSettings.hasNormal;
            } else if(name == SMOLKNIGHT_HAS_BEEG){
                return SmolKnight.saveSettings.hasBeeg;
            }
            return orig;
        }
        
        public static float SetPlayerFloat(string name,float orig){
            DebugLog("SetPlayerFloat");
            //sets dreamGate as if normal knight set it
            float res = orig;
            if(name == DREAMGATE_Y){
                if(SmolKnight.GetCurrentScale() == Size.SMOL){
                    res += 0.6f;
                }
                if(SmolKnight.GetCurrentScale() == Size.BEEG){
                    res -= 2f;
                }
            }                        
            return res;
        }

        public static float GetPlayerFloat(string name, float orig){
            DebugLog("GetPlayerFloat");
            //gets dreamGate location based on current knight size
            if( name == DREAMGATE_Y) { 
                if(SmolKnight.GetCurrentScale() == Size.SMOL){
                    return orig - 0.6f;
                }
                if(SmolKnight.GetCurrentScale() == Size.BEEG){
                    return orig + 2f;
                }
            } 
            //if dreamgate needs to spawn spawn it at normal position
            if(name == SMOLKNIGHT_DREAMGATE_Y){ 
                return PlayerData.instance.GetFloatInternal(DREAMGATE_Y);
            } 
            return orig;
        }
    }
}