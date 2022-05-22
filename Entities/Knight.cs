
using static SmolKnight.HKMP;

namespace SmolKnight
{
    static class Knight{
       
      public static DateTime lastRemotePlayerCheck;
      public static float lastScale = 1f;  
      public static void CheckRemotePlayers(bool forceUpdate = false)
        {   
            var currentTime = DateTime.Now;
            var isTimeToUpdate = (currentTime - lastRemotePlayerCheck).TotalMilliseconds > 1000;
            lastRemotePlayerCheck = currentTime;
            if (( isTimeToUpdate || forceUpdate) && HKMP.isEnabledWithUserName()) {           
               DebugLog("GetRemotePlayerObjects");
               GetRemotePlayerObjects().ForEach((remotePlayer) => {
                  var playerTransform = remotePlayer.getPlayerTransform();
                  var nameTransform = remotePlayer.getNameTransform();
                  var name = remotePlayer.getName();
                  if(name.Contains("SMOL") && !isPlayerSmol(playerTransform))
                  {
                     Smol(playerTransform);
                     AdjustPlayerName(playerTransform,nameTransform,Size.SMOL);
                  }
                  else if(name.Contains("BEEG") && !isPlayerBeeg(playerTransform))
                  {
                     Beeg(playerTransform);
                     AdjustPlayerName(playerTransform,nameTransform,Size.BEEG);
                  }
                  else if((!name.Contains("SMOL") && !name.Contains("BEEG")) && !isPlayerNormal(playerTransform))
                  {
                     Normal(playerTransform);
                     AdjustPlayerName(playerTransform,nameTransform,Size.NORMAL);
                  }
               });
            }
        }
   
      public static void UpdateLocalPlayer()
      {
         if(HeroController.instance == null) { return; }
         if (GameManager.instance.isPaused) { return; } 
         DebugLog("UpdateLocalPlayer");
         var localPlayer = HKMP.GetLocalPlayer();
         if(localPlayer == null) { return; }
         var playerTransform = localPlayer.getPlayerTransform();

         if(HKMP.isEnabledWithUserName()){
            var nameTransform = localPlayer.getNameTransform();
            var name = localPlayer.getName();
            var localScale = playerTransform.localScale;
         
            if(!(name.Contains("SMOL") || name.Contains("BEEG")) && !isPlayerNormal(playerTransform))
               {
                  SmolKnight.currentScale = Size.NORMAL;
                  Normal(playerTransform);
                  SFX.ChangePitch();
                  AdjustPlayerName(playerTransform,nameTransform,SmolKnight.currentScale);
               } 
               else if((name.Contains("SMOL")) && !isPlayerSmol(playerTransform))
               {
                  SmolKnight.currentScale = Size.SMOL;
                  Smol(playerTransform);
                  SFX.ChangePitch();
                  AdjustPlayerName(playerTransform,nameTransform,SmolKnight.currentScale);

               } 
               else if((name.Contains("BEEG") && !isPlayerBeeg(playerTransform)))
               {
                  SmolKnight.currentScale = Size.BEEG;
                  Beeg(playerTransform);
                  SFX.ChangePitch();
                  AdjustPlayerName(playerTransform,nameTransform,SmolKnight.currentScale);
               }    
         } else {
               InteractiveScale(playerTransform,SmolKnight.currentScale);
         }
         lastScale = SmolKnight.currentScale;
      }
      
      public static void PlayTransformEffects(){
         HeroController.instance.GetComponent<SpriteFlash>().flashFocusHeal();
         SFX.PlayTransformSound();
      }
   }
}