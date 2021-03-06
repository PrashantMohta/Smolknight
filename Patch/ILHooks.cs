
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace SmolKnight
{
    public static class ILHooks
    {
        private static bool AreCustomHooksEnabled = false;
        private static List<ILHook> Hooks = new List<ILHook>();

        // Changing the value that the update function in HeroController uses to "fix" wrong local scales
        public static void BypassCheckForKnightScaleRange(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-1f) || instr.MatchLdcR4(1f) ))  
            {
                cursor.EmitDelegate<Func<float>>(() => {
                    return SmolKnight.GetCurrentScale();
                    });
                cursor.Emit(OpCodes.Mul);
            }
        }
        
        
        private static void ChangeDreamGateYPositionName(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);;
            while (cursor.TryGotoNext(instr => instr.MatchLdstr(PlayerDataPatcher.DREAMGATE_Y)))  
            {
                cursor.Remove();
                cursor.Emit(OpCodes.Ldstr, PlayerDataPatcher.SMOLKNIGHT_DREAMGATE_Y);
            }
        }

        public static void InitCustomHooks(){
            if(AreCustomHooksEnabled) return;
            AreCustomHooksEnabled = true;

            //add hook modify the name of the player float so that we can over ride it
            Hooks.Add(new ILHook(
                typeof(SceneManager).GetMethod("orig_Start", BindingFlags.NonPublic | BindingFlags.Instance),
                ChangeDreamGateYPositionName
            ));
        }

        public static void Unhook()
        {
            foreach(var hook in Hooks){
                hook?.Dispose();
            }
            AreCustomHooksEnabled = false;
        }


    }
}