using Modding;
using Modding.Menu;
using Modding.Menu.Config;

using Satchel.BetterMenus;

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

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
                    new string[] { "Normal" , "Smoll", "Beeg" },
                    (setting) => { setSizeOption(setting); },
                    () => getSizeOption(),
                    Id:"SizeOptions"),
                new HorizontalOption(
                    "Allow switching mid-game?", "Recommended for BEEG knight",
                    new string[] { "Yes" , "No"},
                    (setting) => { setAdhocSwitching(setting);},
                    () => getAdhocSwitching(),
                    Id:"AdhocSwitching"),
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