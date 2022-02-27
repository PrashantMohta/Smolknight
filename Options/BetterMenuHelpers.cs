using Satchel.BetterMenus;
namespace SmolKnight
{
    public partial class BetterMenu
    {
        private static void setSizeOption(int i){
            if(i == 0){
                SmolKnight.currentScale = Size.NORMAL;
            } else if(i == 1){
                SmolKnight.currentScale = Size.SMOL;
            } else if(i == 2){
                SmolKnight.currentScale = Size.BEEG;
            }
            SmolKnight.knightController?.applyTransformation();
        }
        private static int getSizeOption(){
            if(SmolKnight.currentScale == Size.SMOL) { return 1;};
            if(SmolKnight.currentScale == Size.BEEG) { return 2;};
            return 0;
        }

        private static void setAdhocSwitching(int i){
            SmolKnight.saveSettings.enableSwitching = (i == 0);
        }

        private static int getAdhocSwitching(){
            return SmolKnight.saveSettings.enableSwitching ? 0 : 1;
        }


    }
}