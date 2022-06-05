using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Satchel.SceneUtils;

namespace SmolKnight.Scenes
{
    internal class Boss1
    {
        private static AssetBundle sceneBundle;
        public static AssetBundle getAssetBundle()
        {
            if (sceneBundle == null)
            {
                sceneBundle = AssemblyUtils.GetAssetBundleFromResources("boss1");
            }
            return sceneBundle;
        }
        public static void EnemyManager()
        {
            var EnemyManager = SmolKnight.satchel.GetCustomEnemyManager();
            var DreamNailManager = SmolKnight.satchel.GetCustomDreamNailManager();

            EnemyManager.Setup(
                SmolKnight.Instance.preloads["GG_Broken_Vessel"]["Infected Knight"],
                SmolKnight.Instance.preloads["GG_Hornet_2"]["Boss Holder/Hornet Boss 2"]
                );

            EnemyManager.AddCallbackForNewEnemies((List<GameObject> enemies) => {
                if (enemies == null) { return; }

                foreach (var enemy in enemies)
                {
                    if (enemy.name.StartsWith("BossMango"))
                    {
                        //do stuff for your enemy 
                        var conv = new Convo(enemy.name);
                        DreamNailManager.SetText(conv, "This is a test from : " + enemy.name);
                        DreamNailManager.SetText(conv, "This is a test 2 from : " + enemy.name);
                        DreamNailManager.SetText(conv, "This is a test 3 from : " + enemy.name);
                        enemy.addDreamNailDialogue(conv.Key, conv.Amount);

                        // add a you logic here
                        //enemy.AddComponent<FollowBall>();
                        var hm = enemy.GetComponent<HealthManager>();
                        if (hm != null)
                        {
                            hm.OnDeath += () => {
                                updateZoteStatue(null, true);
                            };
                        }

                    }
                }
            });
        }
        public static void updateZoteStatue(GameObject interactiveGo, bool bossDefeated = false)
        {
            if(interactiveGo == null)
            {
                interactiveGo = GameObject.Find("interactive");
            }
            if (!bossDefeated)
            {
                interactiveGo.Find("BossUndefeated").SetActive(true);
                interactiveGo.Find("BossDefeated").SetActive(false);
            }
            else
            {
                interactiveGo.Find("BossUndefeated").SetActive(false);
                interactiveGo.Find("BossDefeated").SetActive(true);
            }
        }
        public static void OnSceneLoad() {
            var interactiveGo = GameObject.Find("interactive");
            updateZoteStatue(interactiveGo);
            var bC = interactiveGo.Find("BlockerController");
            bC.AddComponent<BlockerController>();

        }
        public static void CreateScene()
        {
            var sceneName = "Boss1";
            var preloads = SmolKnight.Instance.preloads;
            var customScene = SmolKnight.satchel.GetCustomScene(
                sceneName,
                preloads["Fungus1_03"]["TileMap"],
                preloads["Fungus1_03"]["_SceneManager"]
                );
            var settings = new CustomSceneManagerSettings(SceneUtils.getSceneManagerFromPrefab(customScene.SceneManager))
            {
                mapZone = MapZone.OVERGROWN_MOUND,
                overrideParticlesWith = MapZone.NONE//,
                //backgroundMusicGet = () => WavUtils.ToAudioClip(AssemblyUtils.GetBytesFromResources("mystic.wav"), 0)
            };
            customScene.Config(32, 16, settings);
            customScene.OnLoaded += (_,e) => { OnSceneLoad(); };
        }
    }
}
