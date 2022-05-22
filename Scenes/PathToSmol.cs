using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Satchel;
using static Satchel.SceneUtils;
using GlobalEnums;

namespace SmolKnight.Scenes{
public static class PathToSmol{
    private static AssetBundle sceneBundle;
    public static AssetBundle getAssetBundle(){
        if(sceneBundle == null){
            sceneBundle = AssemblyUtils.GetAssetBundleFromResources("pathtosmol");
        }
        return sceneBundle;
    }

    public static void registerSaveSlotArt(CustomSaveSlotsManager customSaveSlotsManager)
    {

         customSaveSlotsManager.Add(new CustomSaveSlotParams
        {
            sceneName = "PathToSmol",
            sceneTitle = "Old Pathway",
            background = AssemblyUtils.GetSpriteFromResources("old_pathway.png")
        });
    }

    public static void CreateScene(){
            var sceneName = "PathToSmol";
            var preloads = SmolKnight.Instance.preloads;
            var customScene = SmolKnight.satchel.GetCustomScene(
                sceneName,
                preloads["Fungus1_03"]["TileMap"],
                preloads["Fungus1_03"]["_SceneManager"]
                );
            var settings = new CustomSceneManagerSettings(SceneUtils.getSceneManagerFromPrefab(customScene.SceneManager)){
                mapZone = MapZone.GREEN_PATH,
                overrideParticlesWith = MapZone.NONE//,
                //backgroundMusicGet = () => WavUtils.ToAudioClip(AssemblyUtils.GetBytesFromResources("mystic.wav"), 0)
            };
            customScene.Config(32,32,settings);

            // right entrance from crossroads 
            customScene.AddGateway(
                new GatewayParams{
                        gateName = "Path_to_smol entry left",
                        pos = new Vector2(2.32f, 44.5f),
                        size = new Vector2(1, 4),
                        fromScene = "Crossroads_35",
                        toScene = sceneName,
                        entryGate = "Path_to_smol gate right",
                        respawnPoint = new Vector2(3, 0),
                        onlyOut = false,
                        vis = GameManager.SceneLoadVisualizations.GrimmDream
                    }
            );
            customScene.AddGateway(
                new GatewayParams{
                        gateName = "Path_to_smol gate right",
                        pos = new Vector2(33f, 7f),
                        size = new Vector2(1, 4),
                        fromScene = sceneName,
                        toScene = "Crossroads_35",
                        entryGate = "Path_to_smol entry left",
                        respawnPoint = new Vector2(-3, 0),
                        onlyOut = false,
                        vis = GameManager.SceneLoadVisualizations.Default
                    }
            );
            // top entrance from greenpath
            customScene.AddGateway(
                new GatewayParams
                {
                    gateName = "Path_to_smol entry bottom",
                    pos = new Vector2(4f, 5f),
                    size = new Vector2(2, 1),
                    fromScene = "Crossroads_11_alt",
                    toScene = sceneName,
                    entryGate = "Path_to_smol gate top",
                    respawnPoint = new Vector2(3, 0),
                    onlyOut = false,
                    vis = GameManager.SceneLoadVisualizations.GrimmDream
                }
            );
            customScene.AddGateway(
                new GatewayParams
                {
                    gateName = "Path_to_smol gate top",
                    pos = new Vector2(7f, 32f),
                    size = new Vector2(8, 1),
                    fromScene = sceneName,
                    toScene = "Crossroads_11_alt",
                    entryGate = "Path_to_smol entry bottom",
                    respawnPoint = new Vector2(-3, 0),
                    onlyOut = false,
                    vis = GameManager.SceneLoadVisualizations.Default
                }
            );
            // left exit to next scene
            customScene.AddGateway(
                new GatewayParams
                {
                    gateName = "Path_to_smol entry right",
                    pos = new Vector2(31.5f, 4f),
                    size = new Vector2(1, 8),
                    fromScene = "Boss1",
                    toScene = sceneName,
                    entryGate = "Path_to_smol gate left",
                    respawnPoint = new Vector2(3, 0),
                    onlyOut = false,
                    vis = GameManager.SceneLoadVisualizations.GrimmDream
                }
            );
            customScene.AddGateway(
                new GatewayParams
                {
                    gateName = "Path_to_smol gate left",
                    pos = new Vector2(1f, 16f),
                    size = new Vector2(1, 6f),
                    fromScene = sceneName,
                    toScene = "Boss1",
                    entryGate = "Path_to_smol entry right",
                    respawnPoint = new Vector2(3, 0),
                    onlyOut = false,
                    vis = GameManager.SceneLoadVisualizations.Default
                }
            );

            customScene.AddBenchFromPrefab(
                preloads["Fungus1_37"]["RestBench"],
                "Main Bench",
                new Vector3(17f, 12.8f, 0.02f)
            );
        }

}

}