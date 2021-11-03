using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalEnums;
using TMPro;

namespace SmolKnight
{
   class HKMPPlayer{
      public GameObject gameObject;
      public HKMPPlayer(GameObject playerGo){
         gameObject = playerGo;
       }
       public Transform getPlayerTransform(){
         return gameObject.transform;
       }
       private Transform nameTransform;
       public Transform getNameTransform(){
         if(nameTransform == null || nameTransform.gameObject == null){
            nameTransform = getPlayerTransform().Find("Username");
         }
         return nameTransform;
       }
       private TextMeshPro nameTextMeshPro;
       public string getName(){
          if(nameTextMeshPro == null){
               var nameTransform = getNameTransform();
               if( nameTransform == null){ return ""; }
               nameTextMeshPro = nameTransform.gameObject.GetComponent<TextMeshPro>();
               if( nameTextMeshPro == null) { return ""; }
           } 

           return nameTextMeshPro.text;
       }
   }
   static class HKMP{
       private static List<HKMPPlayer> RemotePlayers;
       private static bool hasUsernames = false;
       
       private static HKMPPlayer localPlayer;
       public static HKMPPlayer GetLocalPlayer(){
         if(localPlayer == null){
            localPlayer = new HKMPPlayer(HeroController.instance.gameObject);
         }
         return localPlayer;
       }
       public static bool isEnabledWithUserName(){
         if(HeroController.instance == null) { return false;} 
         var nameTransform = GetLocalPlayer().getNameTransform();
         hasUsernames = (nameTransform != null && nameTransform.gameObject.activeSelf);
         return hasUsernames;
       }

       public static List<HKMPPlayer> GetRemotePlayerObjects(){
         RemotePlayers = new List<HKMPPlayer>();
         foreach(GameObject gameObj in GameObject.FindObjectsOfType<GameObject>())
         {
            if(gameObj.name.StartsWith("Player Container"))
            {
               RemotePlayers.Add(new HKMPPlayer(gameObj));
            }
         }
         return RemotePlayers;
       }

   }
}