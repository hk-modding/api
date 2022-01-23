using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;
using Patch = Modding.Patches;
using Lang = Language.Language;


namespace Modding.Menu
{
    /// <summary>
    /// Class containing some utilities for creating Menu Screens in the default style.
    /// </summary>
    public static class MenuUtils
    {
        /// <summary>
        /// Create a MenuBuilder with the default size and position data, but no content or controls.
        /// </summary>
        /// <param name="title">The title to give the menu screen.</param>
        /// <returns>The MenuBuilder object.</returns>
        public static MenuBuilder CreateMenuBuilder(string title)
        {
            return new MenuBuilder(title)
                .CreateTitle(title, MenuTitleStyle.vanillaStyle)
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
                .SetDefaultNavGraph(new ChainedNavGraph());
        }

        /// <summary>
        /// Create a MenuBuilder with the default size and position data and a back button, but no content.
        /// </summary>
        /// <param name="title">The title to give the menu screen.</param>
        /// <param name="returnScreen">The screen to return to when the user hits back.</param>
        /// <param name="backButton">The back button.</param>
        /// <returns>The MenuBuilder object.</returns>
        public static MenuBuilder CreateMenuBuilderWithBackButton(string title, MenuScreen returnScreen, out MenuButton backButton)
        {
            MenuButton _backButton = null;
            MenuBuilder builder = CreateMenuBuilder(title)
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
                        Label = Lang.Get("NAV_BACK", "MainMenu"),
                        CancelAction = _ => ((Patch.UIManager)UIManager.instance).UIGoToDynamicMenu(returnScreen),
                        SubmitAction = _ => ((Patch.UIManager)UIManager.instance).UIGoToDynamicMenu(returnScreen),
                        Proceed = true,
                        Style = MenuButtonStyle.VanillaStyle
                    },
                    out _backButton
                )
            );
            backButton = _backButton;
            return builder;
        }

        /// <summary>
        /// Add Horizontal Options to the content area.
        /// </summary>
        /// <param name="entries">The menu data.</param>
        /// <param name="c">The content area to add the entries to.</param>
        /// <param name="returnScreen">The screen to return to when the user hits cancel.</param>
        public static void AddModMenuContent(
            List<IMenuMod.MenuEntry> entries,
            ContentArea c,
            MenuScreen returnScreen
        )
        {
            foreach (var entry in entries)
            {
                c.AddHorizontalOption(
                    entry.Name,
                    new HorizontalOptionConfig
                    {
                        ApplySetting = (_, i) => entry.Saver(i),
                        RefreshSetting = (s, _) => s.optionList.SetOptionTo(entry.Loader()),
                        CancelAction = _ => ((Patch.UIManager)UIManager.instance).GoToDynamicMenu(returnScreen),
                        Description = string.IsNullOrEmpty(entry.Description) ? null : new DescriptionInfo
                        {
                            Text = entry.Description
                        },
                        Label = entry.Name,
                        Options = entry.Values,
                        Style = HorizontalOptionStyle.VanillaStyle
                    },
                    out var option
                );
                option.menuSetting.RefreshValueFromGameSettings();
            }
        }

        /// <summary>
        /// Create a menu screen in the default style.
        /// </summary>
        /// <param name="title">The title to give the menu screen.</param>
        /// <param name="menuData">The data for the horizontal options.</param>
        /// <param name="returnScreen">The screen to return to when the user hits back.</param>
        /// <returns>A built menu screen in the default style.</returns>
        public static MenuScreen CreateMenuScreen(string title, List<IMenuMod.MenuEntry> menuData, MenuScreen returnScreen)
        {
            MenuBuilder builder = CreateMenuBuilderWithBackButton(title, returnScreen, out MenuButton backButton);

            if (menuData.Count > 5)
            {
                builder.AddContent(new NullContentLayout(), c => c.AddScrollPaneContent(
                    new ScrollbarConfig
                    {
                        CancelAction = _ => ((Patch.UIManager)UIManager.instance).UIGoToDynamicMenu(returnScreen),
                        Navigation = new Navigation
                        {
                            mode = Navigation.Mode.Explicit,
                            selectOnUp = backButton,
                            selectOnDown = backButton
                        },
                        Position = new AnchoredPosition
                        {
                            ChildAnchor = new Vector2(0f, 1f),
                            ParentAnchor = new Vector2(1f, 1f),
                            Offset = new Vector2(-310f, 0f)
                        }
                    },
                    new RelLength(menuData.Count * 105f),
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c => AddModMenuContent(menuData, c, returnScreen)
                ));
            }
            else
            {
                builder.AddContent(
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c => AddModMenuContent(menuData, c, returnScreen)
                );
            }

            return builder.Build();
        }
    }
}
