namespace SmolKnight
{
    public class Smol{
        private CustomBigItemGetManager igm;
        public bool ShouldSpawn(){
            return !PlayerDataPatcher.GetBool(PlayerDataPatcher.SMOLKNIGHT_HAS_SMOL);
        }

        public void onPickUp(){
            SmolKnight.saveSettings.hasSmol = true;
            igm.ShowDialog(
                "Smol power",
                "Acquired",
                "Press",
                "To change size and become Smol",
                "smoller things can enter smoller pathways",
                AssemblyUtils.GetSpriteFromResources("smol_get.png"),
                () => {
                    if(GameManager.instance.inputHandler.lastActiveController == BindingSourceType.DeviceBindingSource){
                        return SmolKnight.settings.buttonbinds.Transform;
                    }
                    return SmolKnight.settings.keybinds.Transform;
                },()=>{});
        }
        public Smol(CustomShinyManager csm, CustomBigItemGetManager igm){
            this.igm = igm;
            csm.AddShiny("Smol Power","Tutorial_01",new Vector3(38f,11f,0f),false,false, onPickUp , ShouldSpawn);
        }
    }
}