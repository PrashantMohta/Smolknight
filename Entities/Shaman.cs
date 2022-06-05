using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;
using Satchel;
using UnityEngine.SceneManagement;

namespace SmolKnight{
    public class Shaman {

        AssetBundle bundle;
        GameObject prefab,shamanInstance;
        Animator anim;
        AudioSource shamanSource;

        AudioClip shamanTalk;

        public Shaman(){
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += activeSceneChanged;
        }
        public void AddCustomDialogue(CustomDialogueManager cdm){
            cdm.AddConversation("ShamanText1","I am a snail<page>I am also a shaman<page>this is lorem ipsum");  
            cdm.OnEndConversation((string conversation)=>{
                if("ShamanText1" == conversation) { 
                    anim.Play("Shaman Idle loop");
                    shamanInstance.transform.localScale = new Vector3(1f,1f,1f);
                }
            });          
        }
        public void activeSceneChanged(Scene from, Scene to){
            if(bundle == null){
                bundle = AssemblyUtils.GetAssetBundleFromResources("shaman");
            }
            if(to.name.Contains("Town")){ // spawning in dirtmouth for now
                if(!prefab){
                    prefab = bundle.LoadAsset<GameObject>("Assets/Shaman NPC.prefab");
                }
                if(!shamanTalk){
                    shamanTalk = bundle.LoadAsset<AudioClip>("Assets/bruh_test.wav");
                }
                shamanInstance = GameObject.Instantiate(prefab);
                shamanInstance.transform.position = new Vector3(138f, 11f,0f);
                anim = shamanInstance.GetComponent<Animator>();
                shamanSource = shamanInstance.GetComponent<AudioSource>();
                shamanInstance.GetAddCustomArrowPrompt(()=>{
                    SmolKnight.Instance.customDialogueManager.ShowDialogue("ShamanText1");
                    /*foreach(AnimationClip ac in anim.runtimeAnimatorController.animationClips)
                    {
                       SmolKnight.Instance.Log(ac.name);
                    }*/
                    anim.Play("Shaman Talk");
                    if(shamanSource == null){
                        SmolKnight.Instance.Log("shamanSource");
                    }
                    if(shamanTalk == null){
                        SmolKnight.Instance.Log("shamanTalk");
                    }
                    shamanSource.PlayOneShot(shamanTalk);
                    if(shamanInstance.transform.position.x < HeroController.instance.transform.position.x){ // face the player
                        shamanInstance.transform.localScale = new Vector3(-1f,1f,1f);
                    }
                });
            }
        }

    }
}