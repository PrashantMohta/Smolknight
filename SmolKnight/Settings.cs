using System;
using InControl;
using Modding.Converters;
using Newtonsoft.Json;

namespace SmolKnight
{
    public class Settings
    {
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