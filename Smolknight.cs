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
        

        public bool ToggleButtonInsideMenu => false;


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
            
            IL.HeroController.Update10 += ILHooks.BypassCheckForKnightScaleRange;
            ILHooks.InitCustomHooks();

            ModHooks.HeroUpdateHook += HeroUpdate;
            ModHooks.AfterSavegameLoadHook += LoadSaveGame;
            ModHooks.SetPlayerFloatHook += PlayerDataPatcher.SetPlayerFloat;
            ModHooks.GetPlayerFloatHook += PlayerDataPatcher.GetPlayerFloat;

            On.HeroController.FaceLeft += FaceLeft;
            On.HeroController.FaceRight += FaceRight;
            On.HeroController.EnterScene += EnterScene;
            On.HeroController.FindGroundPointY += FindGroundPointY;
            On.HeroController.FindGroundPoint += FindGroundPoint;
            
            On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.OnEnter += OnSpellSpawn;
            On.HutongGames.PlayMaker.Actions.SetScale.DoSetScale += DoSetScale;
            On.HutongGames.PlayMaker.Actions.RayCast2d.OnEnter += OnRayCast2d;

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += ShinyItemStandPatcher.StartPatchCoro;
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
            if(go.name.StartsWith("dream_gate_object")){
                //visually move the dreamgate when spawned 
                var pos = go.transform.position;
                if(currentScale == Size.SMOL){
                    pos.y += Size.SMOL_OFFSET;
                }
                if(currentScale == Size.BEEG){
                    pos.y -= Size.BEEG_OFFSET;
                }
                go.transform.position = pos;
            }
            if(go.name.StartsWith("Fireball")) {
                scaleGO(go,currentScale);
                GameManager.instance.StartCoroutine(scaleFireballCoro(go));
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
        private Vector3 FindGroundPoint(On.HeroController.orig_FindGroundPoint orig,HeroController self,Vector2 startPoint,bool useExtended){
           //This is needed to get smol knight on the floor
           var pos = orig(self,startPoint,useExtended);
           if (currentScale == Size.SMOL) 
            {
                pos.y -= 0.3f;
            }
            return pos;
        }

        //warpToDreamGate
        //GameManager.BeginScene
        //PositionHeroAtSceneEntrance
        
        private IEnumerator EnterScene(On.HeroController.orig_EnterScene orig,HeroController self, TransitionPoint enterGate, float delayBeforeEnter){

            float AdditionalMovex = 0, AdditionalMovey = 0;
            var gateposition = enterGate.GetGatePosition();
            //This is needed because beeg knight can go into infinite loading scene loop because its beeg    
            if (currentScale == Size.BEEG) 
            {
                if (gateposition == GatePosition.left) {
                    AdditionalMovex = Size.BEEG_OFFSET;
                } else if (gateposition == GatePosition.right) {
                    AdditionalMovex = -Size.BEEG_OFFSET;
                }

                if (gateposition == GatePosition.bottom) {
                    AdditionalMovey = Size.BEEG_OFFSET;
                }
                enterGate.transform.position = enterGate.transform.position + new Vector3(AdditionalMovex, AdditionalMovey,0f);
            }
            
            var wait = orig(self,enterGate,delayBeforeEnter);
            UpdatePlayer();
            UpdateHKMPPlayers();
            yield return wait;

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
                VignettePatcher.Patch(1f/scale);
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
            UpdatePlayer();
            PlayTransformEffects();
            SFX.ChangePitch();
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
            
            if (isHKMP == true && (currentTime - lastHKMPCheckTime).TotalMilliseconds > 1000) {
                UpdateHKMPPlayers();
                lastHKMPCheckTime = currentTime;
            }

            if ((currentTime - lastCheckTime).TotalMilliseconds > 5000) {
                UpdatePlayer();
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
