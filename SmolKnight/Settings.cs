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
        public KeyBinds keybinds = new KeyBinds();
    }

    public class KeyBinds : PlayerActionSet
    {
        public PlayerAction Transform;

        public KeyBinds()
        {
            Transform = CreatePlayerAction("Transform");
            DefaultBinds();
        }

        private void DefaultBinds()
        {
            Transform.AddDefaultBinding(Key.Backspace);
        }
    }
}