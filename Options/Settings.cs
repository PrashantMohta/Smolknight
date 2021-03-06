using Modding.Converters;
using Newtonsoft.Json;

namespace SmolKnight
{

    public class SaveModSettings  {
        public string currentScale {get; set;} = "Normal";
        public float shadeScale {get; set;} = Size.NORMAL;
        public bool enableSwitching {get; set;} = true;
        public bool startupSelection {get; set;} = false;
        public bool hasSmol {get; set;} = false;
        public bool hasNormal {get; set;} = true;
        public bool hasBeeg {get; set;} = false;

    }

    public class GlobalModSettings 
    {
        public string Version {get; set;} = "";
        
        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds keybinds = new KeyBinds();

        [JsonConverter(typeof(PlayerActionSetConverter))]
        public ButtonBinds buttonbinds = new ButtonBinds();
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

    public class ButtonBinds : PlayerActionSet
    {
        public PlayerAction Transform;
        public ButtonBinds()
        {
            Transform = CreatePlayerAction("TransformController");
            DefaultBinds();
        }

        private void DefaultBinds()
        {
            Transform.AddDefaultBinding(InputControlType.Action2);
        }
    }
}