using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;

namespace SmolKnight
{
    public class ModMenu
    {
        public static MenuScreen Screen;
        public static MenuScreen modListMenu; 
        private static MappableKey TransformKey = null;

        public static bool skipPauseMenu = false;
        private static float currentScale;
        private static bool adhocSwitching;
        
        private static void setSizeOption(int i){
            if(i == 0){
                currentScale = Size.NORMAL;
                setAdhocSwitching(1);
            } else if(i == 1){
                currentScale = Size.SMOL;
                setAdhocSwitching(1);
            } else if(i == 2){
                currentScale = Size.BEEG;
                setAdhocSwitching(0);
            }
            
            if(AdhocSwitchingSelector != null){
                AdhocSwitchingSelector.menuSetting.RefreshValueFromGameSettings();
            }
        }
        private static int getSizeOption(){
            currentScale = SmolKnight.currentScale;
            if(currentScale == Size.SMOL) { return 1;};
            if(currentScale == Size.BEEG) { return 2;};
            return 0;
        }

        private static void setAdhocSwitching(int i){
            adhocSwitching = (i == 0);
        }

        private static int getAdhocSwitching(){
            return adhocSwitching ? 0 : 1;
        }

        public static void startPlaying(){
                UIManager.instance.TogglePauseGame();
                UIManager.instance.UIClearPauseMenu();
                UIManager.instance.UIClosePauseMenu();
        }
        public static void ApplySetting(){
            if(!SmolKnight.saveSettings.startupSelection && skipPauseMenu){
                startPlaying();
            } else {
                UIManager.instance.UIGoToDynamicMenu(modListMenu); 
            }
            SmolKnight.currentScale = currentScale;
            SmolKnight.saveSettings.enableSwitching = adhocSwitching;
            SmolKnight.Instance.applyTransformation();
            SmolKnight.saveSettings.startupSelection = true;
            skipPauseMenu = false;
        }

        public static void BackSetting(){
            if(!SmolKnight.saveSettings.startupSelection && skipPauseMenu){
                startPlaying();
            } else {
                UIManager.instance.UIGoToDynamicMenu(modListMenu); 
            }
            SmolKnight.saveSettings.startupSelection = true;
            skipPauseMenu = false;
        }

        public static void GoToModListMenu(object _) {
            GoToModListMenu();
        }
        public static void GoToModListMenu() {
            if(!SmolKnight.saveSettings.startupSelection && skipPauseMenu){
                startPlaying();
            } else {
                UIManager.instance.UIGoToDynamicMenu(modListMenu); 
            }
            SmolKnight.saveSettings.startupSelection = true;
            skipPauseMenu = false;
        }

        public static void RefreshOptions(){
            currentScale = SmolKnight.currentScale;
            if(SizeOptionsSelector != null){
                SizeOptionsSelector.menuSetting.RefreshValueFromGameSettings();
            }
            if(AdhocSwitchingSelector != null){
                AdhocSwitchingSelector.menuSetting.RefreshValueFromGameSettings();
            }
        }
        public static MenuOptionHorizontal SizeOptionsSelector,AdhocSwitchingSelector;

        private static void addMenuOptions(ContentArea area){

            area.AddTextPanel("HelpText",
                    new RelVector2(new Vector2(950f, 125f)),
                    new TextPanelConfig{
                        Text = "Experience hallownest through a new perspective",
                        Size = 30,
                        Font = TextPanelConfig.TextFont.TrajanRegular,
                        Anchor = TextAnchor.MiddleCenter
                    });  

            area.AddHorizontalOption(
                    "SizeOptions",
                    new HorizontalOptionConfig
                    {
                        Options = new string[] { "Normal" , "Smoll", "Beeg" },
                        ApplySetting = (_, i) => setSizeOption(i),
                        RefreshSetting = (s, _) => s.optionList.SetOptionTo(getSizeOption()),
                               
                        CancelAction = _ => { BackSetting(); },
                        Description = new DescriptionInfo
                        {
                            Text = "",
                            Style = DescriptionStyle.HorizOptionSingleLineVanillaStyle
                        },
                        Label = "Current size",
                        Style = HorizontalOptionStyle.VanillaStyle
                    },
                    out SizeOptionsSelector
                ); 

            area.AddHorizontalOption(
                    "AdhocSwitching",
                    new HorizontalOptionConfig
                    {
                        Options = new string[] { "Yes" , "No"},
                        ApplySetting = (_, i) => setAdhocSwitching(i),
                        RefreshSetting = (s, _) => s.optionList.SetOptionTo(getAdhocSwitching()),
                               
                        CancelAction = _ => { BackSetting(); },
                        Description = new DescriptionInfo
                        {
                            Text = "Recommended for BEEG knight",
                            Style = DescriptionStyle.HorizOptionSingleLineVanillaStyle
                        },
                        Label = "Allow switching mid-game?",
                        Style = HorizontalOptionStyle.VanillaStyle
                    },
                    out AdhocSwitchingSelector
                ); 

            area.AddTextPanel("HelpText2",
                    new RelVector2(new Vector2(850f, 105f)),
                    new TextPanelConfig{
                        Text = "Set your preferred key for transformation",
                        Size = 30,
                        Font = TextPanelConfig.TextFont.TrajanRegular,
                        Anchor = TextAnchor.MiddleCenter
                    });  
                    
            area.AddKeybind(
                    "TransformBind",
                    SmolKnight.settings.keybinds.Transform,
                    new KeybindConfig
                    {
                        Label = "Transform",
                        CancelAction = _ => { BackSetting(); },
                    },out TransformKey
                );
                
            area.AddTextPanel("HelpText3",
                    new RelVector2(new Vector2(850f, 125f)),
                    new TextPanelConfig{
                        Text = "The Transform key has no Effect when connected to HKMP. To use the Mod with HKMP add the words \"SMOL\" or \"BEEG\" into your name.",
                        Size = 35,
                        Font = TextPanelConfig.TextFont.Perpetua,
                        Anchor = TextAnchor.MiddleLeft
                    }); 
        }

        public static void saveModsMenuScreen(MenuScreen screen){
            modListMenu = screen;
        }

        public static MenuScreen CreatemenuScreen()
        { 
            var builder =  new MenuBuilder(UIManager.instance.UICanvas.gameObject, "SKMenu")
                .CreateTitle("SmolKnight", MenuTitleStyle.vanillaStyle)
                .CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 903f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ))
                .CreateControlPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 250f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -502f)
                    )
                ))
                .SetDefaultNavGraph(new ChainedNavGraph())
                
                .AddControls(
                    new SingleContentLayout(new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, 32f)
                    )),
                    c => c.AddMenuButton(
                        "ApplyButton",
                            new MenuButtonConfig
                            {
                                Label = "Apply",
                                CancelAction = _ => { BackSetting(); },
                                SubmitAction = _ => { ApplySetting();} ,
                                Style = MenuButtonStyle.VanillaStyle,
                                Proceed = true
                            }
                        )
                ).AddControls(
                    new SingleContentLayout(new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -64f)
                    )),
                    c => c.AddMenuButton(
                        "BackButton",
                            new MenuButtonConfig
                            {
                                Label = "Back",
                                CancelAction = _ => { BackSetting(); },
                                SubmitAction = _ => { BackSetting(); } ,
                                Style = MenuButtonStyle.VanillaStyle,
                                Proceed = true
                            }
                        )
                );
                builder.AddContent(new NullContentLayout(), c => c.AddScrollPaneContent(
                new ScrollbarConfig
                {
                    CancelAction = _ => { BackSetting(); },
                    Navigation = new Navigation
                    {
                        mode = Navigation.Mode.Explicit,
                    },
                    Position = new AnchoredPosition
                    {
                        ChildAnchor = new Vector2(0f, 1f),
                        ParentAnchor = new Vector2(1f, 1f),
                        Offset = new Vector2(-310f, 0f)
                    }
                },
                    new RelLength(1600f), 
                    RegularGridLayout.CreateVerticalLayout(125f),
                    contentArea => addMenuOptions(contentArea)
                ));
                Screen = builder.Build();

            return Screen;
        }
    }
}