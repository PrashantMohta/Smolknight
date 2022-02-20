using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TMPro;
using InControl;
using MonoMod.RuntimeDetour;
using Satchel;
using static Satchel.FsmUtil;
using static Satchel.Futils.FsmVariables;
using static Satchel.GameObjectUtils;
using static Satchel.WavUtils;
using static SmolKnight.Utils;

namespace SmolKnight
{
    public class SmolKnight:Mod,ICustomMenuMod,IGlobalSettings<GlobalModSettings>, ILocalSettings<SaveModSettings>
    {
        public static SmolKnight Instance;
        public static GameObject KnightControllerGo;
        public static KnightController knightController;
        public static float currentScale = Size.SMOL;
        public Dictionary<string, Dictionary<string, GameObject>> preloaded;
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
            BetterMenu.MenuRef.Update();
        }
        
        public bool ToggleButtonInsideMenu => false;
        public static void prepareItemDialog(){
           var item = Instance.preloaded["Fungus2_14"]["Shiny Item Stand"];
           CustomBigItemGet.Prepare(Instance.preloaded["Fungus2_14"]["Shiny Item Stand"]);
        }
        public static bool isSmol = false;
        public static void startUpScreen(){  
            if(isSmol){
            CustomBigItemGet.ShowDialog(
                "Smol power",
                "Acquired",
                "Press",
                "To change size and become Smol",
                "smoller things can enter smoller pathways",
                AssemblyUtils.GetSpriteFromResources("smol_get.png"),
                () => {
                    if(GameManager.instance.inputHandler.lastActiveController == BindingSourceType.DeviceBindingSource){
                        return SmolKnight.settings.buttonbinds.Transform;
                    }
                    return SmolKnight.settings.keybinds.Transform;
                },                ()=>{
                    Instance.Log("Got Smol Power dialog down");
                });
            } else {
            CustomBigItemGet.ShowDialog(
                "Beeg power",
                "Acquired",
                "Press",
                "To change size and become Beeg",
                "Beeger things hit harder",
                AssemblyUtils.GetSpriteFromResources("beeg_get.png"),
                () => {
                    if(GameManager.instance.inputHandler.lastActiveController == BindingSourceType.DeviceBindingSource){
                        return SmolKnight.settings.buttonbinds.Transform;
                    }
                    return SmolKnight.settings.keybinds.Transform;
                },
                ()=>{
                    Instance.Log("Got Beeg Power dialog down");
                });
            }
            isSmol = !isSmol;
            //show startup screen
            // on accept 
            // SmolKnight.saveSettings.startupSelection = true
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            return BetterMenu.GetMenu(modListMenu);
        }
        
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Fungus2_14", "Shiny Item Stand"),
            };   
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;
            preloaded = preloadedObjects;
            prepareItemDialog();//
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

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += ShinyItemStandPatcher.StartPatchCoro;
        }        

        private void HeroUpdate()
        {
            if(KnightControllerGo == null){
                KnightControllerGo = new GameObject();
                knightController = KnightControllerGo.AddComponent<KnightController>();
            }
            if(Input.GetKeyDown(KeyCode.P)){
               startUpScreen();
            }
        }
        
    }
}
