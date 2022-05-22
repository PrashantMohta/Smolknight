namespace SmolKnight
{
    public class SmolKnight:Mod,ICustomMenuMod,IGlobalSettings<GlobalModSettings>, ILocalSettings<SaveModSettings>
    {
        public static Satchel.Core satchel = new Satchel.Core(); 
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
                Modding.Logger.Log(e.ToString());
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
            BetterMenu.UpdateMenu();
        }
        
        public bool ToggleButtonInsideMenu => false;
        public static void prepareItems(){
           var customShinyManager = satchel.GetCustomShinyManager();
           var customBigItemGetManager = satchel.GetCustomBigItemGetManager();

           customShinyManager.standPrefab = Instance.preloaded["Fungus2_14"]["Shiny Item Stand"];
           customShinyManager.prefab = Instance.preloaded["Mines_29"]["Shiny Item"]; 
           
           customBigItemGetManager.Prepare(Instance.preloaded["Mines_29"]["Shiny Item"]);
           // create all items that need to be added
           new Smol(customShinyManager,customBigItemGetManager);
           new Beeg(customShinyManager,customBigItemGetManager);
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
                ("Mines_29", "Shiny Item"),
            };   
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;
            preloaded = preloadedObjects;
            prepareItems();
            IL.HeroController.Update10 += ILHooks.BypassCheckForKnightScaleRange;
            ILHooks.InitCustomHooks();

            ModHooks.HeroUpdateHook += HeroUpdate;
            ModHooks.BeforePlayerDeadHook += Shade.OnHeroDeath;
            ModHooks.AfterSavegameLoadHook += LoadSaveGame;

            ModHooks.SetPlayerFloatHook += PlayerDataPatcher.SetPlayerFloat;
            ModHooks.GetPlayerFloatHook += PlayerDataPatcher.GetPlayerFloat;
            ModHooks.GetPlayerBoolHook += PlayerDataPatcher.GetPlayerBool;

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
            if(KnightControllerGo == null && GameManager.instance.IsGameplayScene() && HeroController.instance.cState.onGround){
                KnightControllerGo = new GameObject();
                knightController = KnightControllerGo.AddComponent<KnightController>();
            }
        }
        
    }
}
