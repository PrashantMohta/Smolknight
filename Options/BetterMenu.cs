
using Satchel.BetterMenus;

namespace SmolKnight
{
    public partial class BetterMenu
    {
        public static Menu MenuRef;
        public static Menu PrepareMenu(){
            return new Menu("SmolKnight",new Element[]{
                new TextPanel("Experience hallownest through a new perspective"),
                new HorizontalOption(
                    "Current size", "",
                    getSizeOptionsArr(),
                    (setting) => { setSizeOption(setting); },
                    () => getSizeOption(),
                    Id:"SizeOptions"),
                new TextPanel("Set your preferred key for transformation"),
                Blueprints.KeyAndButtonBind("Transform",
                    SmolKnight.settings.keybinds.Transform,
                    SmolKnight.settings.buttonbinds.Transform),
                new TextPanel("The Transform key has no Effect when connected to HKMP. To use the Mod with HKMP add the words \"SMOL\" or \"BEEG\" into your name."),                    
            });
        }
        public static MenuScreen GetMenu(MenuScreen lastMenu){
            if(MenuRef == null){
                MenuRef = PrepareMenu();
            }
            return MenuRef.GetMenuScreen(lastMenu);
        }
    }
}