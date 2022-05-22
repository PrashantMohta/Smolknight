using Satchel.BetterMenus;
namespace SmolKnight
{
    public partial class BetterMenu
    {
        public static void UpdateMenu(){
            BetterMenu.MenuRef.updateAfter((_)=>{
                ((HorizontalOption)BetterMenu.MenuRef.Find("SizeOptions")).Values = getSizeOptionsArr();
            });
        }
        public static string[] getSizeOptionsArr(){
            List<string> options = new();
            if(PlayerDataPatcher.GetBool(PlayerDataPatcher.SMOLKNIGHT_HAS_NORMAL)){
                options.Add("Normal");
            }
            if(PlayerDataPatcher.GetBool(PlayerDataPatcher.SMOLKNIGHT_HAS_SMOL)){
                options.Add("Smol");
            }
            if(PlayerDataPatcher.GetBool(PlayerDataPatcher.SMOLKNIGHT_HAS_BEEG)){
                options.Add("Beeg");
            }
            return options.ToArray();
        }
        private static void setSizeOption(int i){
            var sizeOps = getSizeOptionsArr();
            var scaleStr =  i < sizeOps.Length ? sizeOps[i] : sizeOps[0];
            if(scaleStr == "Normal" && PlayerDataPatcher.hasScale(Size.NORMAL)){
                SmolKnight.currentScale = Size.NORMAL;
            } else if(scaleStr == "Smol" && PlayerDataPatcher.hasScale(Size.SMOL)){
                SmolKnight.currentScale = Size.SMOL;
            } else if(scaleStr == "Beeg" && PlayerDataPatcher.hasScale(Size.BEEG)){
                SmolKnight.currentScale = Size.BEEG;
            }
            SmolKnight.knightController?.applyTransformation();
            BetterMenu.UpdateMenu();
        }
        private static int getSizeOption(){
            var sizeOps = getSizeOptionsArr();
            var scaleStr = "Normal";
            if(SmolKnight.currentScale == Size.NORMAL) { scaleStr = "Normal"; };
            if(SmolKnight.currentScale == Size.BEEG) { scaleStr = "Beeg"; };
            if(SmolKnight.currentScale == Size.SMOL) { scaleStr = "Smol"; };
            return Array.FindIndex(sizeOps,(item)=> item == scaleStr);

        }

    }
}