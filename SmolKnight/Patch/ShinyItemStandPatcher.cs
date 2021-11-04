using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using static Modding.Logger;

namespace SmolKnight{
    public static class ShinyItemStandPatcher{
        private static readonly Dictionary<string, float> ShineyItemStandList = new Dictionary<string, float>()
        {
           {"Fungus2_14",19.3f}, // Mantis Claw
           {"Ruins1_30",49.3f},  // Spell Twister
           {"Deepnest_32",2.2f}, // Pale Ore
           {"Hive_05",12.0f},  // Hive Blood
           {"Room_Wyrm",6.9f}, // Kings Brand
           {"RestingGrounds_10",17.1f}, // Soul Eater
           {"Mines_11",39.5f}, // ShopKeeper's Key
           {"Fungus3_39",35f}, // Love Key
           {"Abyss_17",14.2f},  // Pale Ore 
           {"Mines_34",53.0f},  // Pale Ore
           {"Mines_36",8.3f},   // Deep Focus
           {"Fungus2_20",6.9f}, // Spore Shroom
           {"Abyss_20",7.2f},   // Simple Key
           {"Cliffs_05",5.5f},    //Joni's Blessing
           {"Mines_30",15.8f},    // kings idol
           {"Waterways_15",3f}  //kings idol
           //do not move these, they only work without moving
           //{"Deepnest_44",5.4f},// sharp shadow
           //{"Fungus2_23",3.0f},// dashmaster 
        };
        
        private static IEnumerator Patch(Scene scene)
        {
            yield return null;
            if (ShineyItemStandList.TryGetValue(scene.name, out float ShineyPos))
            {
                var Shiney = GameObject.Find("Shiny Item Stand");
                if(Shiney == null)
                {
                    Log($"Shiny not found in scene : {scene.name}");
                } 
                else 
                {
                    var pos = Shiney.transform.position;
                    pos.y = ShineyPos;
                    Shiney.transform.position = pos;
                }
            } 
        }


        public static void StartPatchCoro(Scene scene,LoadSceneMode mode)
        {
            GameManager.instance.StartCoroutine(Patch(scene));
        }

    }

}