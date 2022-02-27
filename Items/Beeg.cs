namespace SmolKnight
{
    public class Beeg{
        public bool ShouldSpawn(){
            return !PlayerDataPatcher.GetBool(PlayerDataPatcher.SMOLKNIGHT_HAS_BEEG);
        }

        public void onPickUp(){
           SmolKnight.saveSettings.hasBeeg = true;
           CustomBigItemGet.ShowDialog(
                "Beeg power",
                "Acquired",
                "Press",
                "To change size and become Beeg",
                "Beeger things hit harder",
                AssemblyUtils.GetSpriteFromResources("beeg_get.png"),
                () => {
                    if(GameManager.instance.inputHandler.lastActiveController == BindingSourceType.DeviceBindingSource){
                        return SmolKnight.settings.buttonbinds.Transform;
                    }
                    return SmolKnight.settings.keybinds.Transform;
                },
                ()=>{});
        }
        public Beeg(CustomShinyManager csm){
            csm.AddShiny("Beeg Power","Tutorial_01",new Vector3(32f,11f,0f),false,false, onPickUp , ShouldSpawn);
        }
    }
}