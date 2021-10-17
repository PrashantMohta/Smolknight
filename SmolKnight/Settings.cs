using System;
using InControl;
using Modding.Converters;
using Newtonsoft.Json;

namespace SmolKnight
{

    public class SaveModSettings  {
        public string currentScale {get; set;} = "SMOL";
        public float shadeScale {get; set;} = Size.SMOL;
        public bool enableSwitching {get; set;} = false;
        public bool startupSelection {get; set;} = false;

    }

    public class GlobalModSettings 
    {
        public string Version {get; set;} = "";
        
        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds keybinds = new KeyBinds(false);

        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds buttonbinds = new KeyBinds(true);
    }

    public class KeyBinds : PlayerActionSet
    {
        public PlayerAction Transform;

        public KeyBinds()
        {
            Transform = CreatePlayerAction("Transform");
            DefaultBinds(false);        }
        public KeyBinds(bool isButton)
        {
            Transform = CreatePlayerAction("Transform");
            DefaultBinds(isButton);
        }

        private void DefaultBinds(bool isButton)
        {
            if(isButton){
                Transform.AddDefaultBinding(InputControlType.None);
            } else {
                Transform.AddDefaultBinding(Key.Backspace);
            }
        }
    }
}