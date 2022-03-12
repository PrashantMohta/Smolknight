namespace SmolKnight
{
    public class KnightController : MonoBehaviour{

        public DateTime lastCheckTime = DateTime.Now.AddMilliseconds(-5000);    
        private static void nextScale() {
            if(!SmolKnight.saveSettings.enableSwitching || HKMP.isEnabledWithUserName()) { 
                return;
            }
            var i = Size.scales.FindIndex((item) => item == SmolKnight.currentScale);
            do{
                if(i < Size.scales.Count - 1 ){
                    i++;
                } else {
                    i = 0;
                }
                if(PlayerDataPatcher.hasScale(Size.scales[i])){
                    SmolKnight.currentScale = Size.scales[i];
                    break;
                }
            } while(Size.scales[i] != SmolKnight.currentScale);
        }
        
        public void applyTransformation(){
            if(HeroController.instance == null || GameManager.instance.isPaused) { 
                lastCheckTime = DateTime.Now.AddMilliseconds(-5000);
                return; 
            } 
            Knight.UpdateLocalPlayer();
            Knight.PlayTransformEffects();
            SmolKnight.setSaveSettings();
            lastCheckTime = DateTime.Now;
        }
        public void Update(){
            if (!GameManager.instance.isPaused && (SmolKnight.settings.keybinds.Transform.WasPressed || SmolKnight.settings.buttonbinds.Transform.WasPressed))
            {
                nextScale();
                applyTransformation();
                BetterMenu.UpdateMenu();
            }
            Knight.CheckRemotePlayers(false);
            var currentTime = DateTime.Now;
            if ((currentTime - lastCheckTime).TotalMilliseconds > 5000) {
                Knight.UpdateLocalPlayer();
                lastCheckTime = currentTime;
            }
        }
    }
}