using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;

using static Satchel.GameObjectUtils;

namespace SmolKnight{
    public static class PreloadManager{
        
        private static Dictionary<string,GameObject> Objects = new Dictionary<string, GameObject>();

        public static GameObject GetGameObject(string name){
            if(Objects.TryGetValue(name,out var go)){
                return go;
            }
            return null;
        }
        public static void AddPreload(string name,GameObject preload){
            Objects[name] = preload;
        }
        public static void AddDeathLoodle(GameObject holder){
            var go = holder.LocateMyFSM("Spawn").Fsm.GetFsmGameObject("Enemy Type").Value;
            Objects["deathloodle"] = go;
        }

    }
}