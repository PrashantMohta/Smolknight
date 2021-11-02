using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TMPro;
using MonoMod.RuntimeDetour;
using Satchel;
using static Satchel.FsmUtil;
using static Satchel.GameObjectUtils;
using static Satchel.WavUtils;

namespace SmolKnight
{
    public class SmolKnight:Mod,ICustomMenuMod,IGlobalSettings<GlobalModSettings>, ILocalSettings<SaveModSettings>
    {
        internal static SmolKnight Instance;

        public static float currentScale = Size.SMOL;
        public static float GetCurrentScale() => currentScale;
        private bool isHKMP = false;

        private bool playerIsSmol = false;
        private bool playerIsBeeg = false;

        public DateTime lastHKMPCheckTime = DateTime.Now;
        public DateTime lastCheckTime = DateTime.Now;    

        private string getVersionSafely(){
            return Satchel.AssemblyUtils.GetAssemblyVersionHash();
        }
        public override string GetVersion()
        {
            var version = "Satchel not found";
            try{
                version = getVersionSafely();
            } catch(Exception e){

            }
            return version;
        }
        
        public static GlobalModSettings settings { get; set; } = new GlobalModSettings();
        public void OnLoadGlobal(GlobalModSettings s) => settings = s;
        public GlobalModSettings OnSaveGlobal() => settings;

        public static SaveModSettings saveSettings { get; set; } = new SaveModSettings();
        public void OnLoadLocal(SaveModSettings s) => saveSettings = s;
        public SaveModSettings OnSaveLocal() => saveSettings;

        public void setSaveSettings(){
            if(currentScale == Size.SMOL){
                saveSettings.currentScale = "SMOL";
            } else if(currentScale == Size.BEEG){
                saveSettings.currentScale = "BEEG";
            } else {
                saveSettings.currentScale = "NORMAL";
            }
        }
        public void LoadSaveGame(SaveGameData data){
            if(saveSettings.currentScale == "SMOL"){
                currentScale = Size.SMOL;
            } else if(saveSettings.currentScale == "BEEG"){
                currentScale = Size.BEEG;
            } else {
                currentScale = Size.NORMAL;
            }
            ModMenu.RefreshOptions();
        }
        

        public bool ToggleButtonInsideMenu => false;

        public static void startUpScreen(){            
                ModMenu.skipPauseMenu = true;
                GameManager.instance.StartCoroutine(GameManager.instance.PauseToggleDynamicMenu(ModMenu.Screen));
        }

        public static IEnumerator HideCurrentMenu(On.UIManager.orig_HideCurrentMenu orig,UIManager self){
            Modding.Logger.Log(self.menuState);
            if(self.menuState == MainMenuState.DYNAMIC_MENU &&
             self.currentDynamicMenu == ModMenu.Screen && 
             !SmolKnight.saveSettings.startupSelection && ModMenu.skipPauseMenu){
                ModMenu.startPlaying();
                yield return self.HideMenu(ModMenu.Screen);
                yield return null;
            } else {
                yield return orig(self);
            }
        }
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            ModMenu.saveModsMenuScreen(modListMenu);
            return ModMenu.CreatemenuScreen();
        }

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 10/Colosseum Cage Small (5)"),
                ("Cliffs_01","Cornifer Card")
            };   
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;
            PreloadManager
            .AddDeathLoodle(preloadedObjects["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Wave 10/Colosseum Cage Small (5)"]);
            PreloadManager
            .AddPreload("card",preloadedObjects["Cliffs_01"]["Cornifer Card"]);
            CustomArrowPrompt.Prepare(PreloadManager.GetGameObject("card"));

            IL.HeroController.Update10 += ILHooks.BypassCheckForKnightScaleRange;
            ILHooks.InitCustomHooks();

            ModHooks.HeroUpdateHook += HeroUpdate;
            ModHooks.BeforePlayerDeadHook += Shade.OnHeroDeath;
            ModHooks.AfterSavegameLoadHook += LoadSaveGame;

            ModHooks.SetPlayerFloatHook += PlayerDataPatcher.SetPlayerFloat;
            ModHooks.GetPlayerFloatHook += PlayerDataPatcher.GetPlayerFloat;

            On.HeroController.FaceLeft += HeroControllerPatcher.FaceLeft;
            On.HeroController.FaceRight += HeroControllerPatcher.FaceRight;
            On.HeroController.FindGroundPointY += HeroControllerPatcher.FindGroundPointY;
            On.HeroController.FindGroundPoint += HeroControllerPatcher.FindGroundPoint;
            
            On.HeroController.EnterScene += GatePatcher.EnterScene;
            On.HeroController.FinishedEnteringScene += GatePatcher.FinishedEnteringScene;
            
            On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.OnEnter += ActionPatcher.OnSpellSpawn;
            On.HutongGames.PlayMaker.Actions.SetScale.DoSetScale += ActionPatcher.DoSetScale;
            On.HutongGames.PlayMaker.Actions.RayCast2d.OnEnter += ActionPatcher.OnRayCast2d;


            On.UIManager.HideCurrentMenu += HideCurrentMenu;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += ShinyItemStandPatcher.StartPatchCoro;
        }        

        //warpToDreamGate
        //GameManager.BeginScene
        //PositionHeroAtSceneEntrance
                
        private static void Smol(Transform transform)
        {
            transform.SetScale(Size.SMOL);  
        }
        private static void Normal(Transform transform)
        {
            transform.SetScale(Size.NORMAL);
        }
        private static void Beeg(Transform transform)
        {
            transform.SetScale(Size.BEEG);
        }
        
        private static void InteractiveScale(Transform transform){
            if(currentScale == Size.SMOL){
                Smol(transform);
                Instance.scalePrefabs();
            } else if(currentScale == Size.NORMAL){
                Normal(transform);
                Instance.scalePrefabs();
            } else if(currentScale == Size.BEEG){
                Beeg(transform);
                Instance.scalePrefabs();
            }
        }

        private void PlayTransformEffects(){
            HeroController.instance.GetComponent<SpriteFlash>().flashFocusHeal();
            SFX.PlayTransformSound();
        }

        private static void nextScale(){
            if(!saveSettings.enableSwitching || Instance.isHKMP) { 
                return;
            }

            if(currentScale == Size.SMOL){
                currentScale = Size.NORMAL;
            } else if(currentScale == Size.NORMAL){
                currentScale = Size.BEEG;
            } else if(currentScale == Size.BEEG){
                currentScale = Size.SMOL;
            }
        }

        private void fixPlayerName(Transform Player , Transform Username,float currentPlayerScale){

            if(currentPlayerScale == Size.NORMAL){
                Username.position = Player.position + new Vector3(0, 1.25f, 0);
            } else if(currentPlayerScale == Size.SMOL){
                Username.position = Player.position + new Vector3(0, 0.75f, 0);
            } else if(currentPlayerScale == Size.BEEG){
                Username.position = Player.position + new Vector3(0, 2f, 0);
            }

            if(currentPlayerScale != Size.SMOL){ // because it looks absurd on smolknight
                var ulocalScale = new Vector3(0.25f, 0.25f, Username.localScale.z);
                ulocalScale.x = ulocalScale.x * 1/currentPlayerScale;
                ulocalScale.y = ulocalScale.y * 1/currentPlayerScale;
                Username.localScale = ulocalScale;
            }

        }

        private void scalePrefabs(){
            return;
           var h = HeroController.instance;
           GameObject[] prefabs = {h.spell1Prefab,
                                   h.grubberFlyBeamPrefabL,
                                   h.grubberFlyBeamPrefabR,
                                   h.grubberFlyBeamPrefabU,
                                   h.grubberFlyBeamPrefabD,
                                   h.grubberFlyBeamPrefabL_fury,
                                   h.grubberFlyBeamPrefabR_fury,
                                   h.grubberFlyBeamPrefabU_fury,
                                   h.grubberFlyBeamPrefabD_fury,
                                   h.corpsePrefab};
            for(var i=0;i < prefabs.Length;i++){
               var localScale = prefabs[i].transform.localScale;
               localScale.x = localScale.x * currentScale;
               localScale.y = localScale.y * currentScale;
               prefabs[i].transform.localScale = localScale;
            }
        }

        public void UpdateHKMPPlayers()
        {
            foreach(GameObject gameObj in GameObject.FindObjectsOfType<GameObject>())
            {
                if(gameObj.name.StartsWith("Player Container"))
                {
                    var name = gameObj.transform.Find("Username");
                    if( name == null){continue;}
                    
                    var tmp = name.gameObject.GetComponent<TextMeshPro>();
                    if(tmp.text.Contains("SMOL"))
                    {
                        Smol(gameObj.transform);
                        fixPlayerName(gameObj.transform,name,Size.SMOL);
                    } 
                    else  if(tmp.text.Contains("BEEG"))
                    {   
                        Beeg(gameObj.transform);
                        fixPlayerName(gameObj.transform,name,Size.BEEG);
                    } 
                    else 
                    {
                        Normal(gameObj.transform);
                        fixPlayerName(gameObj.transform,name,Size.NORMAL);
                    }

                }
            }
        }

        public void UpdatePlayer()
        {
            if(HeroController.instance == null) { return;} 

            var playerTransform = HeroController.instance.gameObject.transform;
            var hkmpUsername = playerTransform.Find("Username");
            var localScale = playerTransform.localScale;
            
            playerIsSmol = Math.Abs(localScale.x) == Size.SMOL && Math.Abs(localScale.y) == Size.SMOL;
            playerIsBeeg = Math.Abs(localScale.x) == Size.BEEG && Math.Abs(localScale.y) == Size.BEEG;

            if( hkmpUsername != null && hkmpUsername.gameObject.activeSelf)
            {
                isHKMP = true;
                var tmp = hkmpUsername.gameObject.GetComponent<TextMeshPro>();
                if(!(tmp.text.Contains("SMOL") || tmp.text.Contains("BEEG")) && (playerIsSmol || playerIsBeeg))
                {
                    currentScale = Size.NORMAL;
                    Normal(playerTransform);
                    Instance.scalePrefabs();
                } 
                else if((tmp.text.Contains("SMOL")) && !playerIsSmol)
                {
                    currentScale = Size.SMOL;
                    Smol(playerTransform);
                    Instance.scalePrefabs();

                } 
                else if((tmp.text.Contains("BEEG") && !playerIsBeeg))
                {
                    currentScale = Size.BEEG;
                    Beeg(playerTransform);
                    Instance.scalePrefabs();
                }    
                fixPlayerName(playerTransform,hkmpUsername,currentScale);
                SFX.ChangePitch();
            }
            else 
            {
                isHKMP = false;
                InteractiveScale(playerTransform);
                SFX.ChangePitch();
            }
        }
        public void applyTransformation(){
            if(HeroController.instance == null) { return; } 
            UpdatePlayer();
            PlayTransformEffects();
            SFX.ChangePitch();
            setSaveSettings();
            lastCheckTime = DateTime.Now;
        }
        private void HeroUpdate()
        {
            if(!saveSettings.startupSelection && GameManager.instance.IsGameplayScene() && HeroController.instance.cState.onGround && Input.anyKey){
                startUpScreen();
            }

            var currentTime = DateTime.Now;
            if (settings.keybinds.Transform.WasPressed || settings.buttonbinds.Transform.WasPressed)
            {
                nextScale();
                ModMenu.RefreshOptions();
                applyTransformation();
            }
            
            if (isHKMP == true && (currentTime - lastHKMPCheckTime).TotalMilliseconds > 1000) {
                UpdateHKMPPlayers();
                lastHKMPCheckTime = currentTime;
            }

            if ((currentTime - lastCheckTime).TotalMilliseconds > 5000) {
                UpdatePlayer();
                lastCheckTime = currentTime;
            }
        }
        
    }
}
