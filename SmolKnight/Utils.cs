using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalEnums;

namespace SmolKnight
{
   static class Utils{

       public static void SetScale(this Transform transform,float scale){
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
                    VignettePatcher.Patch(1f/scale);
                }
            }

            if (Math.Abs(localScale.x - x) > Mathf.Epsilon || Math.Abs(localScale.y - y) > Mathf.Epsilon) 
            { 
                transform.localScale = new Vector3(x, y, transform.localScale.z);
            }
        }
        public static void scaleGO(this GameObject go,float scale){
            var localScale = go.transform.localScale;
            localScale.x = localScale.x > 0 ? scale : -scale;
            localScale.y = scale;
            go.transform.localScale = localScale;
        }
       
   }
}