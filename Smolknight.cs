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
    public class SmolKnight:Mod,ICustomMenuMod
    {
        internal static SmolKnight Instance;

        private static float currentScale = 0.5f;

        private static int currentTransformation = 0;

        private bool isHKMP = false;

        private bool currentPlayerIsTransformed = false;

        private AudioClip BeegKnightWalk;
        private GameObject BeegKnightWalkSource;

        public override string GetVersion()
        {
            return "v1.5";
        }

        private static readonly List<float> SmolKnightValues = new List<float>()
        {
            0.5f,
            1f,
            2.5f,
        };
        
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
        };
        
        
        public bool ToggleButtonInsideMenu => false;

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            return ModMenu.CreatemenuScreen(modListMenu);
        }

        public override void Initialize()
        {
            Instance = this;

            foreach (string res in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (!res.Contains("BeegKnightWalk")) continue;
                
                Stream audioStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
                if (audioStream != null)
                {
                    byte[] buffer = new byte[audioStream.Length];
                    audioStream.Read(buffer, 0, buffer.Length);
                    audioStream.Dispose();
                    BeegKnightWalk = WavUtility.ToAudioClip(buffer);
                }
            }
            
            BeegKnightWalkSource = new GameObject("BeegKnightWalkSource", typeof(AudioSource));
            GameObject.DontDestroyOnLoad(BeegKnightWalkSource);

            ModHooks.HeroUpdateHook += HeroUpdate;
            On.HeroController.FaceLeft += FaceLeft;
            On.HeroController.FaceRight += FaceRight;
            IL.HeroController.Update10 += DONT_CHANGE_MY_LOCALSCALE;
            On.GameManager.EnterHero += StopInfiniteTransitions;
            On.HutongGames.PlayMaker.Actions.SetScale.DoSetScale += DoSetScale;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += FixSceneSpecificThings;
        }

        private void StopInfiniteTransitions(On.GameManager.orig_EnterHero orig, GameManager self, bool additivegatesearch)
        {
            //This is needed because beeg knight can go into infinite loading scene loop because its beeg
            float AdditionalMovex, AdditionalMovey;
            if (currentTransformation % 3 == 2) // dont wanna mess with stuff unless knight is beeg
            {
                if (self.entryGateName.Contains("left")) AdditionalMovex = 2f;
                else if (self.entryGateName.Contains("right")) AdditionalMovex = -2f;
                else AdditionalMovex = 0;

                if (self.entryGateName.Contains("bot")) AdditionalMovey = 2f;
                else AdditionalMovey = 0;
            }
            else AdditionalMovex = AdditionalMovey = 0;

            
            // The normal function from GameManager
            if (self.entryGateName == "door_dreamReturn" &&
                !string.IsNullOrEmpty(self.playerData.GetString("bossReturnEntryGate")))
            {
                if (self.GetCurrentMapZone() == MapZone.GODS_GLORY.ToString())
                    self.entryGateName = self.playerData.GetString("bossReturnEntryGate");
                PlayerData.instance.SetString("bossReturnEntryGate", string.Empty);
            }

            bool needFirstFadeIn = ReflectionHelper.GetField<GameManager, bool>(self, "needFirstFadeIn");
            bool hazardRespawningHero = ReflectionHelper.GetField<GameManager, bool>(self, "hazardRespawningHero");
            bool verboseMode = ReflectionHelper.GetField<GameManager, bool>(self, "verboseMode");
            float entryDelay = ReflectionHelper.GetField<GameManager, float>(self, "entryDelay");
            
            if (self.RespawningHero)
            {
                if (needFirstFadeIn)
                {
                    self.StartCoroutine(self.FadeSceneInWithDelay(0.3f));
                    ReflectionHelper.SetField<GameManager, bool>(self, "needFirstFadeIn", false);
                }

                self.StartCoroutine(self.hero_ctrl.Respawn());
                self.FinishedEnteringScene();
                self.RespawningHero = false;
            }
            else if (hazardRespawningHero)
            {
                self.StartCoroutine(self.hero_ctrl.HazardRespawn());
                self.FinishedEnteringScene();
                ReflectionHelper.SetField<GameManager, bool>(self, "hazardRespawningHero", false);
            }
            else if (self.entryGateName == "dreamGate")
                self.hero_ctrl.EnterSceneDreamGate();
            else if (!self.startedOnThisScene)
            {
                self.SetState(GameState.ENTERING_LEVEL);
                if (!string.IsNullOrEmpty(self.entryGateName))
                {
                    if (additivegatesearch)
                    {
                        if (verboseMode) Debug.Log((object) ("Searching for entry gate " + self.entryGateName + " in the next scene: " + self.nextSceneName));
                        
                        foreach (GameObject rootGameObject in UnityEngine.SceneManagement.SceneManager.GetSceneByName(self.nextSceneName).GetRootGameObjects())
                        {
                            TransitionPoint component = rootGameObject.GetComponent<TransitionPoint>();
                            if ((UnityEngine.Object) component != (UnityEngine.Object) null &&
                                component.name == self.entryGateName)
                            {
                                if (verboseMode) Debug.Log((object) "SUCCESS - Found as root object");
                                component.entryOffset.x += AdditionalMovex;
                                component.entryOffset.y += AdditionalMovey;
                                self.StartCoroutine(self.hero_ctrl.EnterScene(component, entryDelay));
                                return;
                            }

                            if (rootGameObject.name == "_Transition Gates")
                            {
                                TransitionPoint[] componentsInChildren =
                                    rootGameObject.GetComponentsInChildren<TransitionPoint>();
                                for (int index = 0; index < componentsInChildren.Length; ++index)
                                {
                                    if (componentsInChildren[index].name == self.entryGateName)
                                    {
                                        if (verboseMode)  Debug.Log((object) "SUCCESS - Found in _Transition Gates folder");
                                        componentsInChildren[index].entryOffset.x += AdditionalMovex;
                                        componentsInChildren[index].entryOffset.y += AdditionalMovey;
                                        self.StartCoroutine(self.hero_ctrl.EnterScene(componentsInChildren[index],
                                            entryDelay));
                                        return;
                                    }
                                }
                            }

                            TransitionPoint[] componentsInChildren1 =
                                rootGameObject.GetComponentsInChildren<TransitionPoint>();
                            for (int index = 0; index < componentsInChildren1.Length; ++index)
                            {
                                if (componentsInChildren1[index].name == self.entryGateName)
                                {
                                    if (verboseMode) Debug.Log("SUCCESS - Found as a child of a random scene object, can't win em all");
                                    componentsInChildren1[index].entryOffset.x += AdditionalMovex;
                                    componentsInChildren1[index].entryOffset.y += AdditionalMovey;
                                    self.StartCoroutine(self.hero_ctrl.EnterScene(componentsInChildren1[index],
                                        entryDelay));
                                    return;
                                }
                            }
                        }

                        Debug.LogError((object) "Searching in next scene for TransitionGate failed.");
                    }
                    else
                    {
                        GameObject gameObject = GameObject.Find(self.entryGateName);
                        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
                        {
                            TransitionPoint EntryGate = gameObject.GetComponent<TransitionPoint>();
                            EntryGate.entryOffset.x += AdditionalMovex;
                            EntryGate.entryOffset.y += AdditionalMovey;
                            self.StartCoroutine(self.hero_ctrl.EnterScene(EntryGate, entryDelay));
                        }
                        else
                        {
                            Debug.LogError((object) ("No entry point found with the name \"" + self.entryGateName + "\" in self scene (" + self.sceneName + "). Unable to move hero into position, trying alternative gates..."));
                            TransitionPoint[] objectsOfType = UnityEngine.Object.FindObjectsOfType<TransitionPoint>();
                            if (objectsOfType != null)
                            {
                                objectsOfType[0].entryOffset.x += AdditionalMovex;
                                objectsOfType[0].entryOffset.y += AdditionalMovey;
                                self.StartCoroutine(self.hero_ctrl.EnterScene(objectsOfType[0], entryDelay));
                            }
                            else
                            {
                                Debug.LogError((object) "Could not find any gates in self scene. Trying last ditch spawn...");
                                self.hero_ctrl.transform.SetPosition2D((float) (self.tilemap.width / 2f) + AdditionalMovex,
                                    (self.tilemap.height / 2f)+AdditionalMovey);
                                self.hero_ctrl.EnterSceneDreamGate();
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("No entry gate has been defined in the Game Manager, unable to move hero into position.");
                    self.FinishedEnteringScene();
                }
            }
            else
            {
                if (!self.IsGameplayScene())
                    return;
                self.FinishedEnteringScene();
                self.FadeSceneIn();
            }
        }

        // Changing the value that the update function in HeroController uses to "fix" wrong local scales
        private void DONT_CHANGE_MY_LOCALSCALE(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-1f) || instr.MatchLdcR4(1f) ))  
            {
                cursor.EmitDelegate<Func<float>>(GiveTimeScale);
                cursor.Emit(OpCodes.Mul);
            }
        }
        //ngl idk why this works i thought i needed to check fro + and - but i apparently dont. if it works i dont question
        private static float GiveTimeScale() => currentScale;
        
        
        
        private void FixSceneSpecificThings(Scene scene,LoadSceneMode mode)
        {
            GameManager.instance.StartCoroutine(FixShinyItemStand(scene));
        }
        
        private IEnumerator FixShinyItemStand(Scene scene)
        {
            yield return null;
            float ShineyPos;
            if (ShineyItems.TryGetValue(scene.name, out ShineyPos))
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
        }

        private static void Transform(Transform transform)
        {
            var localScale = transform.localScale;
            var x = currentScale;
            var y = currentScale;
            bool isNextBigger = Math.Abs(localScale.y) < currentScale;
            
            //checks for looking left or right
            if (localScale.x < 0)
            {
                x = -currentScale;
            }
            if (localScale.y < 0)
            {
                y = -currentScale;
            }

            float AdditionalMove = (currentTransformation % 3) switch
            {
                1 => 1f,
                2 => 2f,
                _ => 0f,
            };
            
            //makes sure people dont get stuck into the ground
            if (isNextBigger) transform.position = new Vector3(transform.position.x, transform.position.y + AdditionalMove, transform.position.z);
            
            if (Math.Abs(localScale.x - x) > Mathf.Epsilon || Math.Abs(localScale.y - y) > Mathf.Epsilon) 
            { 
                transform.localScale = new Vector3(x, y, 1f);
            }

            //if we need to increase the light 
            //var LightControl = transform.Find("HeroLight").gameObject.LocateMyFSM("HeroLight Control");
            //LightControl.FsmVariables.FindFsmVector3("Damage Scale").Value = new Vector2(1.5f * 1.5f,1.5f * 1.5f);
            //LightControl.FsmVariables.FindFsmVector3("Idle Scale").Value = new Vector2(3f * 1.5f,3f * 1.5f);

            if(transform.gameObject == HeroController.instance.gameObject)
            {
                setVignette(2f);
            }
        }

        private static void normal(Transform transform)
        {
            var localScale = transform.localScale;
            var x = 1f;
            var y = 1f;
            if (localScale.x < 0)
            {
                x = -1f;
            }
            if (localScale.y < 0)
            {
                y = -1f;
            }
            if (Math.Abs(localScale.x - x) > Mathf.Epsilon || Math.Abs(localScale.y - y) > Mathf.Epsilon) { 
                transform.localScale = new Vector3(x, y, 1f);
            }
            if(transform.gameObject == HeroController.instance.gameObject)
            {
                setVignette(1f);
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
                    if(tmp.text.Contains("SMOL") || tmp.text.Contains("BEEG"))
                    {
                        Transform(gameObj.transform);
                    } 
                    else 
                    {
                        normal(gameObj.transform);
                    }
                }
            }
        }

        private void UpdatePlayer()
        {
            var playerTransform = HeroController.instance.gameObject.transform;
            var hkmpUsername = playerTransform.Find("Username");
            var localScale = playerTransform.localScale;
            currentPlayerIsTransformed = (localScale.x * localScale.x < 0.5f) && (localScale.y * localScale.y < 0.5f);
            if( hkmpUsername != null)
            {
                isHKMP = true;
                var tmp = hkmpUsername.gameObject.GetComponent<TextMeshPro>();
                if(!(tmp.text.Contains("SMOL") || tmp.text.Contains("BEEG")) && currentPlayerIsTransformed)
                {
                    playerTransform.position = playerTransform.position + new Vector3(0,1f,0);
                    normal(playerTransform);
                } 
                else if((tmp.text.Contains("SMOL")|| tmp.text.Contains("BEEG")) && !currentPlayerIsTransformed)
                {
                    
                    Transform(playerTransform);
                }
            }
            else 
            {
                Transform(playerTransform);
            }
        }

        private DateTime lastHKMPCheckTime = DateTime.Now;
        
        private void FaceRight(On.HeroController.orig_FaceRight orig, HeroController self)
        {
            self.cState.facingRight = true;
            Vector3 localScale = self.transform.localScale;
            localScale.x = -currentScale;
            self.transform.localScale = localScale;
            UpdatePlayer();
        }

        private bool ShouldPlayWalkingSound()
        {
            return HeroController.instance.hero_state == ActorStates.running &&
                   !HeroController.instance.cState.dashing &&
                   (!HeroController.instance.cState.backDashing &&
                    !HeroController.instance.controlReqlinquished);
        }

        private void HeroUpdate()
        {
            if (settings.keybinds.Transform.WasPressed)
            {
                currentTransformation++;
                currentScale = SmolKnightValues[currentTransformation % 3];
            }

            if (currentTransformation % 3 == 2)
            {
                if (ShouldPlayWalkingSound())
                {
                    AudioSource audios = BeegKnightWalkSource.GetComponent<AudioSource>();
                    audios.clip = BeegKnightWalk;
                    HeroAudioController audioCtrl = ReflectionHelper.GetField<HeroController, HeroAudioController>(
                        HeroController.instance,
                        "audioCtrl");
                    audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
                    audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
                    if (!audios.isPlaying)
                    {
                        audios.PlayOneShot(audios.clip, GameManager.instance.GetImplicitCinematicVolume());
                    }
                }
            }

            var currentTime = DateTime.Now;
            if (isHKMP && (currentTime - lastHKMPCheckTime).TotalMilliseconds > 500) 
            {
                UpdateHKMPPlayers();
                UpdatePlayer();
                lastHKMPCheckTime = currentTime;
            }
            else
            {
                UpdatePlayer();
            }
        }
        private void FaceLeft(On.HeroController.orig_FaceLeft orig, HeroController self)
        {
            self.cState.facingRight = false;
            Vector3 localScale = self.transform.localScale;
            localScale.x = currentScale;
            self.transform.localScale = localScale;
            UpdatePlayer();
        }

        private void DoSetScale(On.HutongGames.PlayMaker.Actions.SetScale.orig_DoSetScale orig, HutongGames.PlayMaker.Actions.SetScale self)
        {
            if (self.gameObject != null && self.gameObject.GameObject != null &&
                self.gameObject.GameObject.Value != null &&
                self.gameObject.GameObject.Value == HeroController.instance.gameObject)
            {
                GameObject ownerDefaultTarget = self.Fsm.GetOwnerDefaultTarget(self.gameObject);
                if (ownerDefaultTarget == null)
                {
                    return;
                }

                Vector3 localScale =
                    (!self.vector.IsNone) ? self.vector.Value : ownerDefaultTarget.transform.localScale;
                if (!self.x.IsNone)
                {
                    localScale.x = self.x.Value * currentScale;
                }

                if (!self.y.IsNone)
                {
                    localScale.y = self.y.Value * currentScale;
                }

                if (!self.z.IsNone)
                {
                    localScale.z = self.z.Value;
                }

                ownerDefaultTarget.transform.localScale = localScale;
                UpdatePlayer();
            }
            else
            {
                orig(self);
            }
        }
    }
}
