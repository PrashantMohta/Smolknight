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
using static SmolKnight.Utils;

namespace SmolKnight
{
    public class SmolKnight:Mod,ICustomMenuMod,IGlobalSettings<GlobalModSettings>, ILocalSettings<SaveModSettings>
    {
        internal static SmolKnight Instance;

        public static GameObject KnightControllerGo;
        public static KnightController knightController;
        public static float currentScale = Size.SMOL;
        public static float GetCurrentScale(){
            return currentScale;
        }

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

        public static void setSaveSettings(){
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
            DebugLog("HideCurrentMenu");
            DebugLog(self.menuState.ToString());
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

        public override void Initialize()
        {
            Instance = this;

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
            On.HutongGames.PlayMaker.Actions.CreateObject.OnEnter += ActionPatcher.CreateObject;


            On.UIManager.HideCurrentMenu += HideCurrentMenu;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += ShinyItemStandPatcher.StartPatchCoro;
        }        

        //warpToDreamGate
        //GameManager.BeginScene
        //PositionHeroAtSceneEntrance
        
        private void HeroUpdate()
        {
            if(KnightControllerGo == null){
                KnightControllerGo = new GameObject();
                knightController = KnightControllerGo.AddComponent<KnightController>();
            }
           
        }
        
    }
}
