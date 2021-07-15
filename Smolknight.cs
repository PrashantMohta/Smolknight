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

namespace SmolKnight
{

    static class Size{
        public static readonly float SMOL = 0.5f;
        public static readonly float NORMAL = 1f;
        public static readonly float BEEG = 2.5f;
    }

    public class SmolKnight:Mod,ICustomMenuMod,IGlobalSettings<GlobalModSettings>, ILocalSettings<SaveModSettings>
    {
        internal static SmolKnight Instance;

        public static float currentScale = Size.SMOL;
        private bool isHKMP = false;

        private bool playerIsSmol = false;
        private bool playerIsBeeg = false;

        public DateTime lastHKMPCheckTime = DateTime.Now;
        public DateTime lastCheckTime = DateTime.Now;    

        public override string GetVersion()
        {
            return "v1.5-01";
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
        

        private readonly Dictionary<string, float> ShineyItemStandList = new Dictionary<string, float>()
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
        
        public bool ToggleButtonInsideMenu => false;

        private AudioSource transformSource;

        public static void startUpScreen(){
            ModMenu.skipPauseMenu = true;
            GameManager.instance.StartCoroutine(GameManager.instance.PauseToggleDynamicMenu(ModMenu.Screen));
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            ModMenu.saveModsMenuScreen(modListMenu);
            return ModMenu.CreatemenuScreen();
        }

        public override void Initialize()
        {
            Instance = this;

            ModHooks.HeroUpdateHook += HeroUpdate;
            ModHooks.AfterSavegameLoadHook += LoadSaveGame;

            On.HeroController.FaceLeft += FaceLeft;
            On.HeroController.FaceRight += FaceRight;
            IL.HeroController.Update10 += DONT_CHANGE_MY_LOCALSCALE;
            On.HeroController.EnterScene += EnterScene;
            On.HeroController.FindGroundPointY += FindGroundPointY;
            On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.OnEnter += OnSpellSpawn;

            On.HutongGames.PlayMaker.Actions.SetScale.DoSetScale += DoSetScale;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += FixSceneSpecificThings;
            On.HutongGames.PlayMaker.Actions.RayCast2d.OnEnter += OnRayCast2d;
        }
        private void OnRayCast2d(On.HutongGames.PlayMaker.Actions.RayCast2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.RayCast2d self){
            GameObject fromObj = self.Fsm.GetOwnerDefaultTarget(self.fromGameObject);
            self.debug.Value = true;
            if(fromObj.name == "Knight"){
                self.distance.Value = 2f * currentScale;
            }
            orig(self);
        }

        private void scaleGO(GameObject go,float scale){
            var localScale = go.transform.localScale;
            localScale.x = localScale.x > 0 ? scale : -scale;
            localScale.y = scale;
            go.transform.localScale = localScale;
        }

        private IEnumerator scaleFireballCoro(GameObject go){
            yield return null;
            scaleGO(go,currentScale);
            var blast = go.FindGameObjectInChildren("Fireball Blast");
            var blastpos = blast.transform.position;
            if(currentScale == Size.SMOL){
                scaleGO(blast,currentScale * 2f);
            }
            var hgo = HeroController.instance.gameObject;
            var heroCollider = hgo.GetComponent<BoxCollider2D>();
            blastpos.y = heroCollider.bounds.center.y ;
            blastpos.x = heroCollider.bounds.center.x - (hgo.transform.localScale.x > 0 ? currentScale : -currentScale);
            blast.transform.position = blastpos;

        }

        private IEnumerator scaleDG(GameObject go){
            yield return null;
            scaleGO(go,currentScale);
        }
        
        private void OnSpellSpawn(On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool self){
            if(self.gameObject.Value){
                var g = self.gameObject.Value;
                if(g.name.StartsWith("Fireball")) {
                    if(currentScale == Size.SMOL){
                        var p = self.position.Value;
                        p.y = -0.3f;
                        self.position.Value = p;
                    }
                }
            }
            orig(self);
            var go = self.storeObject.Value;
            var localScale = go.transform.localScale;
            if(go.name.StartsWith("Fireball")) {
                scaleGO(go,currentScale);
                GameManager.instance.StartCoroutine(scaleFireballCoro(go));

            } else if (go.name.StartsWith("dream_gate_object")){
                /*foreach(var c in go.GetComponents<Component>()){
                    Log(c.GetType());
                    if(c.GetType().ToString() == "PlayMakerFSM"){
                        Log(((PlayMakerFSM)c).FsmName);
                    }
                }
                PlayMakerFSM DGFSM = go.LocateMyFSM("Cancel");
                var act = DGFSM.GetAction<ActivateGameObject>("ActivateGameObject",1);
                var go1 = act.gameObject.GameObject.Value;
                scaleGO(go1,currentScale);
                GameManager.instance.StartCoroutine(scaleDG(go1));
                */
            }

        }

        private float FindGroundPointY(On.HeroController.orig_FindGroundPointY orig,HeroController self,float x, float y,bool useExtended){
           //This is needed to get smol knight on the floor
           var posY = orig(self,x, y,useExtended);
           if (currentScale == Size.SMOL) 
            {
                posY -= 0.3f;
            }
            return posY;
        }
        private IEnumerator EnterScene(On.HeroController.orig_EnterScene orig,HeroController self, TransitionPoint enterGate, float delayBeforeEnter){

            float AdditionalMovex = 0, AdditionalMovey = 0;
            var gateposition = enterGate.GetGatePosition();
            //This is needed because beeg knight can go into infinite loading scene loop because its beeg    
            if (currentScale == Size.BEEG) 
            {
                if (gateposition == GatePosition.left) {
                    AdditionalMovex = 2f;
                } else if (gateposition == GatePosition.right) {
                    AdditionalMovex = -2f;
                }

                if (gateposition == GatePosition.bottom) {
                    AdditionalMovey = 2f;
                }
                enterGate.transform.position = enterGate.transform.position + new Vector3(AdditionalMovex, AdditionalMovey,0f);
            }
            
            var wait = orig(self,enterGate,delayBeforeEnter);
            UpdatePlayer();
            UpdateHKMPPlayers();
            yield return wait;

        }

        // Changing the value that the update function in HeroController uses to "fix" wrong local scales
        private void DONT_CHANGE_MY_LOCALSCALE(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-1f) || instr.MatchLdcR4(1f) ))  
            {
                cursor.EmitDelegate<Func<float>>(GetCurrentScale);
                cursor.Emit(OpCodes.Mul);
            }
        }
        //ngl idk why this works i thought i needed to check fro + and - but i apparently dont. if it works i dont question
        private static float GetCurrentScale() => currentScale;
        
        private void FixSceneSpecificThings(Scene scene,LoadSceneMode mode)
        {
            GameManager.instance.StartCoroutine(FixShinyItemStand(scene));
        }
        
        private IEnumerator FixShinyItemStand(Scene scene)
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

        private static List<(string,int,float)> LightAffectors = new List<(string, int,float)>()
        {
            ("Dark -1",1,10.5f),
            ("Dark -1",2,5f),
            ("Dark -1 2",0,10.5f),
            ("Dark -1 2",1,5f),
            ("Normal",1,5f),
            ("Normal",3,4f),
            ("Normal 2",1,5f),
            ("Normal 2",2,4f),
            ("Dark 1",1,2.2f),
            ("Dark 1",2,1.9f),
            ("Dark 1 2",1,2.2f),
            ("Dark 1 2",2,1.9f),
            ("Dark 2",1,0.8f),
            ("Dark 2",2,0.8f),
            ("Dark 2 2",1,0.8f),
            ("Dark 2 2",2,0.8f),
            ("Lantern",0,3f),
            ("Lantern",2,2.2f),
            ("Lantern 2",0,3f),
            ("Lantern 2",2,2.2f),
        };
        private static void setVignette(float factor)
        {
            var fsm = HeroController.instance.transform.Find("Vignette").gameObject.LocateMyFSM("Darkness Control");
            
            if(fsm == null) Instance.Log("no fsm");
            
            foreach (var lightAffectors in LightAffectors)
            {
                fsm.GetAction<SetVector3XYZ>(lightAffectors.Item1, lightAffectors.Item2).x.Value = lightAffectors.Item3 * factor;
                fsm.GetAction<SetVector3XYZ>(lightAffectors.Item1, lightAffectors.Item2).y.Value = lightAffectors.Item3 * factor;
                fsm.GetAction<SetVector3XYZ>(lightAffectors.Item1, lightAffectors.Item2).z.Value = lightAffectors.Item3 * factor;
            }

            // re-evaluate instead of waiting for screen transition
            fsm.SetState("Dark Lev Check");
        }

        private static void Smol(Transform transform)
        {
            SetScale(transform,Size.SMOL);  
        }
        private static void Normal(Transform transform)
        {
            SetScale(transform,Size.NORMAL);
        }
        private static void Beeg(Transform transform)
        {
            SetScale(transform,Size.BEEG);
        }
        
        private void PlayTransformEffects(){
            HeroController.instance.GetComponent<SpriteFlash>().flashFocusHeal();
            if(transformSource == null){
                var soundGO = HeroController.instance.gameObject.FindGameObjectInChildren("Dash");
                transformSource = soundGO.GetComponent<AudioSource>();
            }
            transformSource.Play();
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
        private static void SetScale(Transform transform,float scale){
            var localScale = transform.localScale;
            var x = scale;
            var y = scale;
            
            //checks for looking left or right
            if (localScale.x < 0)
            {
                x = -scale;
            }
            if (localScale.y < 0)
            {
                y = -scale;
            }

            //if we need to increase the light 
            //var LightControl = transform.Find("HeroLight").gameObject.LocateMyFSM("HeroLight Control");
            //LightControl.FsmVariables.FindFsmVector3("Damage Scale").Value = new Vector2(1.5f * 1.5f,1.5f * 1.5f);
            //LightControl.FsmVariables.FindFsmVector3("Idle Scale").Value = new Vector2(3f * 1.5f,3f * 1.5f);

            if(transform.gameObject == HeroController.instance.gameObject)
            {
                float AdditionalMove = 0f;
                //try to make sure player stays above the ground when rescaling
                if(Math.Abs(localScale.y) != scale){
                    if(HeroController.instance.cState.onGround){
                        if(scale == Size.NORMAL){
                            AdditionalMove = 0f;
                        } else if(scale == Size.BEEG){
                            AdditionalMove = 1f;
                        } else if(scale == Size.SMOL){
                            AdditionalMove = -1.5f;
                        } 
                        transform.position = HeroController.instance.FindGroundPoint(transform.position) + new Vector3(0f,AdditionalMove,0f);
                    } else {
                         if(scale == Size.NORMAL){
                            AdditionalMove = 0.7f;
                        } else if(scale == Size.BEEG){
                            AdditionalMove = 2f;
                        } else if(scale == Size.SMOL){
                            AdditionalMove = -3f;
                        } 
                        transform.position = new Vector3(transform.position.x, transform.position.y + AdditionalMove, transform.position.z);
                    }
                }
                setVignette(1f/scale);
            }

            if (Math.Abs(localScale.x - x) > Mathf.Epsilon || Math.Abs(localScale.y - y) > Mathf.Epsilon) 
            { 
                transform.localScale = new Vector3(x, y, 1f);
            }
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

        private void UpdateHKMPPlayers()
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

        private void UpdatePlayer()
        {
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
                SoundMagic();            
            }
            else 
            {
                isHKMP = false;
                InteractiveScale(playerTransform);
                SoundMagic();
            }
        }

        
        private Dictionary<string,AudioEchoFilter> echo = new Dictionary<string,AudioEchoFilter>();
        private void SoundMagic(){
            //pitches up or down the sounds of the hero based on size
            AudioSource[] audioSources = HeroController.instance.GetComponentsInChildren<AudioSource>();
            foreach(AudioSource source in audioSources){          
                if(currentScale == Size.SMOL){
                    source.pitch = 1.5f;
                    HeroController.instance.checkEnvironment();
                    if(source.name == "FootstepsWalk" || source.name == "FootstepsRun"){
                        source.volume = 1f;
                    }
                } else if(currentScale == Size.NORMAL){
                    source.pitch = 1f;
                    HeroController.instance.checkEnvironment();
                    if(source.name == "FootstepsWalk" || source.name == "FootstepsRun"){
                        source.volume = 1f;
                    }
                } else if(currentScale == Size.BEEG){
                    source.pitch = 0.8f;
                    if(source.name == "FootstepsWalk"){
                        source.pitch = 0.7f;
                        source.volume = 1.5f;
                    }
                    if(source.name == "FootstepsRun"){
                        source.pitch = 0.8f;
                        source.volume = 1.5f;

                    }
                }
            }
        }

        public void applyTransformation(){
            UpdatePlayer();
            PlayTransformEffects();
            SoundMagic();
            setSaveSettings();
            lastCheckTime = DateTime.Now;
        }
        private void HeroUpdate()
        {
            if (!saveSettings.startupSelection && Input.anyKey)
            {
                startUpScreen();
            }

            var currentTime = DateTime.Now;
            if (settings.keybinds.Transform.WasPressed)
            {
                nextScale();
                ModMenu.RefreshOptions();
                applyTransformation();
            }
            

            var dg = GameObject.Find("door_dreamReturn");
            if(dg != null){
                scaleGO(dg,currentScale);
            }
            if (isHKMP == true && (currentTime - lastHKMPCheckTime).TotalMilliseconds > 1000) {
                UpdateHKMPPlayers();
                lastHKMPCheckTime = currentTime;
            }

            if ((currentTime - lastCheckTime).TotalMilliseconds > 5000) {
                UpdatePlayer();

                /*
                var g = (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) ;
                foreach(var go in g){
                    if(go.name.ToLower().Contains("dream")){
                        if(go.name.StartsWith("Dream_Gate")){
                            Log(go.name);
                            if(go.name.Contains("Other"))
                            scaleGO(go,currentScale);
                        }
                    }
                }
                */
                lastCheckTime = currentTime;
            }
        }
        private void FaceLeft(On.HeroController.orig_FaceLeft orig, HeroController self)
        {
            orig(self);
            UpdatePlayer();
        }
        private void FaceRight(On.HeroController.orig_FaceRight orig, HeroController self)
        {
            orig(self);
            UpdatePlayer();
        }
        private void DoSetScale(On.HutongGames.PlayMaker.Actions.SetScale.orig_DoSetScale orig, HutongGames.PlayMaker.Actions.SetScale self)
        {
            orig(self);

            if(self.gameObject == null || self.gameObject.GameObject == null || self.gameObject.GameObject.Value == null){
                return;
            }
            if (self.gameObject.GameObject.Value == HeroController.instance.gameObject)
            {
                UpdatePlayer();
            }
            
        }
    }
}
