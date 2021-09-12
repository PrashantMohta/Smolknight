using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using static Satchel.FsmUtil;
using static Modding.Logger;
namespace SmolKnight
{
   static class VignettePatcher{

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
        public static void Patch(float factor)
        {
            var fsm = HeroController.instance.transform.Find("Vignette").gameObject.LocateMyFSM("Darkness Control");
            
            if(fsm == null) Log("no fsm");
            
            foreach (var lightAffectors in LightAffectors)
            {
                fsm.GetAction<SetVector3XYZ>(lightAffectors.Item1, lightAffectors.Item2).x.Value = lightAffectors.Item3 * factor;
                fsm.GetAction<SetVector3XYZ>(lightAffectors.Item1, lightAffectors.Item2).y.Value = lightAffectors.Item3 * factor;
                fsm.GetAction<SetVector3XYZ>(lightAffectors.Item1, lightAffectors.Item2).z.Value = lightAffectors.Item3 * factor;
            }

            // re-evaluate instead of waiting for screen transition
            fsm.SetState("Dark Lev Check");
        }

    }
}