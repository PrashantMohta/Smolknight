using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalEnums;

namespace SmolKnight
{
   static class Utils{

        public static void DebugLog(string s){
            if(true){ //make it false when not debugging
                SmolKnight.Instance.Log(s);
            }
        }
       public static void SetScale(this Transform transform,float scale){
            DebugLog("SetScale");
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
                            if(Math.Abs(localScale.y) == Size.BEEG){
                                AdditionalMove = -1.5f;
                            } else {
                                AdditionalMove = -0.3f;
                            }
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
                    VignettePatcher.Patch(1f/scale);
                }
            }

            if (Math.Abs(localScale.x - x) > Mathf.Epsilon || Math.Abs(localScale.y - y) > Mathf.Epsilon) 
            { 
                transform.localScale = new Vector3(x, y, transform.localScale.z);
            }
        }
       public static void scaleGO(this GameObject go,float scale){
            DebugLog("scaleGO");
            var localScale = go.transform.localScale;
            localScale.x = localScale.x > 0 ? scale : -scale;
            localScale.y = scale;
            go.transform.localScale = localScale;
        }
       
       public static void AdjustPlayerName(Transform Player , Transform Username,float currentPlayerScale){
            DebugLog("AdjustPlayerName");
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


        public static void Smol(Transform transform)
        {
            transform.SetScale(Size.SMOL);  
        }
        public static void Normal(Transform transform)
        {
            transform.SetScale(Size.NORMAL);
        }
        public static void Beeg(Transform transform)
        {
            transform.SetScale(Size.BEEG);
        }

        public static void InteractiveScale(Transform transform,float currentScale){
            if(currentScale == Size.SMOL && !isPlayerSmol(transform)){
                Smol(transform);
                SFX.ChangePitch();
            } else if(currentScale == Size.NORMAL && !isPlayerNormal(transform)){
                Normal(transform);
                SFX.ChangePitch();
            } else if(currentScale == Size.BEEG && !isPlayerBeeg(transform)){
                Beeg(transform);
                SFX.ChangePitch();
            }
        }

        public static bool isPlayerBeeg(Transform transform){
           return Math.Abs(transform.localScale.x) == Size.BEEG && Math.Abs(transform.localScale.y) == Size.BEEG;
        }
        public static bool isPlayerNormal(Transform transform){
           return Math.Abs(transform.localScale.x) == Size.NORMAL && Math.Abs(transform.localScale.y) == Size.NORMAL;
        }
        public static bool isPlayerSmol(Transform transform){
            return Math.Abs(transform.localScale.x) == Size.SMOL && Math.Abs(transform.localScale.y) == Size.SMOL;
        }
   }
}