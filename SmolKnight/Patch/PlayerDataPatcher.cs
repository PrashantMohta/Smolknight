using System;
using System.Collections;
using Modding;


namespace SmolKnight
{
    public static class PlayerDataPatcher
    {
        
        public static float SetPlayerFloat(string name,float orig){
            //sets dreamGate as if normal knight set it
            float res = orig;
            if(name == "dreamGateY"){
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
            //gets dreamGate location based on current knight size
            if( name == "dreamGateY") { 
                if(SmolKnight.GetCurrentScale() == Size.SMOL){
                    return orig - 0.6f;
                }
                if(SmolKnight.GetCurrentScale() == Size.BEEG){
                    return orig + 2f;
                }
            } 
            //if dreamgate needs to spawn spawn it at normal position
            if(name == "SmolKnight.dreamGateY"){ 
                return PlayerData.instance.GetFloatInternal("dreamGateY");
            } 
            return orig;
        }
    }
}