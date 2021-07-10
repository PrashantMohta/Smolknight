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

    public class SmolKnight:Mod,ICustomMenuMod
    {
        internal static SmolKnight Instance;

        private static float currentScale = Size.SMOL;
        private static bool enableInteractivity = true;
        private bool isHKMP = false;

        private bool playerIsSmol = false;
        private bool playerIsBeeg = false;

        public DateTime lastHKMPCheckTime = DateTime.Now;
        public DateTime lastCheckTime = DateTime.Now;    

        public override string GetVersion()
        {
            return "v1.5";
        }
        
        public static Settings settings { get; set; } = new Settings();
        public void OnLoadGlobal(Settings s) => settings = s;
        public Settings OnSaveGlobal() => settings;

        private readonly Dictionary<string, float> ShineyItems = new Dictionary<string, float>()
        {
           {"Fungus2_14",19.3f}, // Mantis Claw
           {"Ruins1_30",49.3f}, // Spell Twister
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

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            return ModMenu.CreatemenuScreen(modListMenu);
        }

        public override void Initialize()
        {
            Instance = this;

            ModHooks.HeroUpdateHook += HeroUpdate;
            On.HeroController.FaceLeft += FaceLeft;
            On.HeroController.FaceRight += FaceRight;
            IL.HeroController.Update10 += DONT_CHANGE_MY_LOCALSCALE;
            On.HeroController.EnterScene += EnterScene;
            On.HeroController.FindGroundPointY += FindGroundPointY;

            On.HutongGames.PlayMaker.Actions.SetScale.DoSetScale += DoSetScale;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += FixSceneSpecificThings;
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
            if (ShineyItems.TryGetValue(scene.name, out float ShineyPos))
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
        private static void InteractiveScale(Transform transform){
            //add sound effects here + flash white here
            if(currentScale == Size.SMOL){
                Smol(transform);
            } else if(currentScale == Size.NORMAL){
                Normal(transform);
            } else if(currentScale == Size.BEEG){
                Beeg(transform);
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
                if(scale == Size.NORMAL){
                    AdditionalMove = 0.7f;
                } else if(scale == Size.BEEG){
                    AdditionalMove = 2f;
                } else if(scale == Size.SMOL){
                    AdditionalMove = -3f;
                } 
                
                //try to make sure player stays on the ground when rescaling
                //todo : sometimes it's possible to fall through into the floor , possible fix -> only scale when benched
                if(Math.Abs(localScale.y) != scale){
                    transform.position = new Vector3(transform.position.x, transform.position.y + AdditionalMove, transform.position.z);
                }
                setVignette(1f/scale);
            }

            if (Math.Abs(localScale.x - x) > Mathf.Epsilon || Math.Abs(localScale.y - y) > Mathf.Epsilon) 
            { 
                transform.localScale = new Vector3(x, y, 1f);
            }
        }

        private static void nextScale(){
            if(!enableInteractivity || Instance.isHKMP) { 
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
                } 
                else if((tmp.text.Contains("SMOL")) && !playerIsSmol)
                {
                    currentScale = Size.SMOL;
                    Smol(playerTransform);
                } 
                else if((tmp.text.Contains("BEEG") && !playerIsBeeg))
                {
                    currentScale = Size.BEEG;
                    Beeg(playerTransform);
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

        
        private void manageReverb(AudioSource source){
            if(currentScale == Size.BEEG){
                if(!reverb.TryGetValue(source.name,out var rev)){
                    var r = source.gameObject.AddComponent<AudioReverbFilter>();
                    r.reverbLevel = 200f;
                    r.decayTime = 3f;
                    r.decayHFRatio = 2f;
                    reverb.Add(source.name,r);
                } else {
                    rev.enabled = true;
                }
                
            } else {
                if(reverb.TryGetValue(source.name,out var rev)){
                    rev.enabled = false;
                }
            }
        }
        private Dictionary<string,AudioReverbFilter> reverb = new Dictionary<string,AudioReverbFilter>();
        private void SoundMagic(){
            AudioSource[] audioSources = HeroController.instance.GetComponentsInChildren<AudioSource>();
            foreach(AudioSource source in audioSources){
                Log(source.name + ":" +source.volume);
                if(currentScale == Size.SMOL){
                    source.pitch = 1.5f;
                    HeroController.instance.checkEnvironment();
                } else if(currentScale == Size.NORMAL){
                    source.pitch = 1f;
                    HeroController.instance.checkEnvironment();
                } else if(currentScale == Size.BEEG){
                    source.pitch = 0.9f;
                    if(source.name == "FootstepsWalk"){
                        source.pitch = 0.8f;
                    }
                }
                if(source.name == "FootstepsWalk" || source.name == "FootstepsRun"){
                        manageReverb(source);
                }
            }
        }
        private void HeroUpdate()
        {
            var currentTime = DateTime.Now;

            if (settings.keybinds.Transform.WasPressed)
            {
                nextScale();
                UpdatePlayer();
                SoundMagic();
                this.lastCheckTime = currentTime;
            }

            if (isHKMP == true && (currentTime - this.lastHKMPCheckTime).TotalMilliseconds > 1000) {
                UpdateHKMPPlayers();
                this.lastHKMPCheckTime = currentTime;
            }

            if ((currentTime - this.lastCheckTime).TotalMilliseconds > 5000) {
                UpdatePlayer();
                this.lastCheckTime = currentTime;
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
            if (self.gameObject != null && 
                self.gameObject.GameObject != null &&
                self.gameObject.GameObject.Value != null &&
                self.gameObject.GameObject.Value == HeroController.instance.gameObject)
            {
                UpdatePlayer();
            }
        }
    }
}
