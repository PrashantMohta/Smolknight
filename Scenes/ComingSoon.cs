using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Satchel;
using static Satchel.SceneUtils;
using GlobalEnums;

namespace SmolKnight.Scenes{
public static class ComingSoon{
    private static AssetBundle sceneBundle;
    public static AssetBundle getAssetBundle(){
        if(sceneBundle == null){
            sceneBundle = AssemblyUtils.GetAssetBundleFromResources("comingsoon");
        }
        return sceneBundle;
    }

    public static void CreateScene(){
        var preloads = SmolKnight.Instance.preloads;
        var customScene = SmolKnight.satchel.GetCustomScene(
            "comingsoon",
            preloads["Fungus1_03"]["TileMap"],
            preloads["Fungus1_03"]["_SceneManager"]
            );
        var settings = new CustomSceneManagerSettings(SceneUtils.getSceneManagerFromPrefab(customScene.SceneManager)){
            mapZone = MapZone.GREEN_PATH,
            overrideParticlesWith = MapZone.NONE//,
            //backgroundMusicGet = () => WavUtils.ToAudioClip(AssemblyUtils.GetBytesFromResources("mystic.wav"), 0)
        };
        customScene.Config(64,32,settings);
        customScene.AddGateway(
            new GatewayParams{
                    gateName = "comingsoon entry left",
                    pos = new Vector2(12.5f, 12.5f),
                    size = new Vector2(1, 4),
                    fromScene = "Town",
                    toScene = "comingsoon",
                    entryGate = "comingsoon gate right",
                    respawnPoint = new Vector2(3, 0),
                    onlyOut = false,
                    vis = GameManager.SceneLoadVisualizations.GrimmDream
                }
        );
        customScene.AddGateway(
            new GatewayParams{
                    gateName = "comingsoon gate right",
                    pos = new Vector2(62f, 6),
                    size = new Vector2(1, 4),
                    fromScene = "comingsoon",
                    toScene = "Town",
                    entryGate = "comingsoon entry left",
                    respawnPoint = new Vector2(-3, 0),
                    onlyOut = false,
                    vis = GameManager.SceneLoadVisualizations.Default
                }
        );
    }

}

}