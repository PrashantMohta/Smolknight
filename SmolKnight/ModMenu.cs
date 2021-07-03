using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;

namespace SmolKnight
{
    public class ModMenu
    {
        private static MenuScreen Screen;

        public static MenuScreen CreatemenuScreen(MenuScreen modListMenu)
        { 
            MappableKey TransformKey = null;
            
            Screen =  new MenuBuilder(UIManager.instance.UICanvas.gameObject, "TKMenu")
                .CreateTitle("SmolKnight Settings", MenuTitleStyle.vanillaStyle)
                .CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 903f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ))
                .CreateControlPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 259f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -502f)
                    )
                ))
                .SetDefaultNavGraph(new ChainedNavGraph())
                .AddContent(
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c =>
                    {
                        c.AddKeybind(
                            "TransformBind",
                            SmolKnight.settings.keybinds.Transform,
                            new KeybindConfig
                            {
                                Label = "Transform",
                                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu)
                            },out TransformKey
                        );
                    }
                )
                .AddControls(
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
                            CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            SubmitAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            Style = MenuButtonStyle.VanillaStyle,
                            Proceed = true
                        }
                    )
                )
                .Build();

            return Screen;
        }
    }
}