using System.Collections.Generic;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;
using Patch = Modding.Patches;

namespace Modding
{
    internal class ModListMenu
    {
        private MenuScreen screen;

        private Dictionary<ITogglableMod, bool> changedMods = new Dictionary<ITogglableMod, bool>();

        private Dictionary<string, bool> modEnabledSettings = ModHooks.Instance.GlobalSettings.ModEnabledSettings;

        public void InitMenu()
        {
            var builder = new MenuBuilder(UIManager.instance.UICanvas.gameObject, "ModListMenu");
            this.screen = builder.screen;
            builder.CreateAutoMenuNav()
                .CreateTitle("Mods", MenuTitleStyle.vanillaStyle)
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
                .AddContent(
                    null,
                    c => c.AddScrollPaneContent(
                        new ScrollbarConfig
                        {
                            cancelAction = _ => this.ApplyChanges(),
                            navigation = new Navigation { mode = Navigation.Mode.Explicit },
                            position = new AnchoredPosition
                            {
                                childAnchor = new Vector2(0f, 1f),
                                parentAnchor = new Vector2(1f, 1f),
                                offset = new Vector2(-310f, 0f)
                            }
                        },
                        new RelLength(0f),
                        RegularGridLayout.CreateVerticalLayout(105f),
                        c =>
                        {
                            foreach (var mod in ModLoader.LoadedMods)
                            {
                                if (mod is ITogglableMod itmod)
                                {
                                    var rt = c.contentObject.GetComponent<RectTransform>();
                                    rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                    GameObject obj;
                                    c.AddHorizontalOption(
                                        itmod.GetName(),
                                        new HorizontalOptionConfig
                                        {
                                            applySetting = (self, ind) =>
                                            {
                                                changedMods[itmod] = ind == 1;
                                            },
                                            cancelAction = _ => this.ApplyChanges(),
                                            label = itmod.GetName(),
                                            options = new string[] { "Off", "On" },
                                            refreshSetting = (self, apply) => self.optionList.SetOptionTo(
                                                this.modEnabledSettings[itmod.GetName()] ? 1 : 0
                                            ),
                                            style = HorizontalOptionStyle.vanillaStyle,
                                            description = new DescriptionInfo
                                            {
                                                text = $"Version {mod.GetVersion()}",
                                                style = DescriptionStyle.singleLineVanillaStyle
                                            }
                                        },
                                        out obj
                                    );
                                    obj.GetComponent<MenuSetting>().RefreshValueFromGameSettings();
                                }
                                if (mod is IMenuMod immod)
                                {
                                    var menu = CreateModMenu(immod);
                                    var rt = c.contentObject.GetComponent<RectTransform>();
                                    rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                    c.AddMenuButton(
                                        $"{immod.GetName()}_Settings",
                                        new MenuButtonConfig
                                        {
                                            style = MenuButtonStyle.vanillaStyle,
                                            cancelAction = _ => this.ApplyChanges(),
                                            label = $"{immod.GetName()} Settings",
                                            submitAction = _ => ((Patch.UIManager)UIManager.instance)
                                                .UIGoToDynamicMenu(menu),
                                            proceed = true
                                        }
                                    );
                                }
                                else if (mod is ICustomMenuMod icmmod)
                                {
                                    var menu = icmmod.GetMenuScreen(this.screen);
                                    var rt = c.contentObject.GetComponent<RectTransform>();
                                    rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                    c.AddMenuButton(
                                        $"{icmmod.GetName()}_Settings",
                                        new MenuButtonConfig
                                        {
                                            style = MenuButtonStyle.vanillaStyle,
                                            cancelAction = _ => this.ApplyChanges(),
                                            label = $"{icmmod.GetName()} Settings",
                                            submitAction = _ => ((Patch.UIManager)UIManager.instance)
                                                .UIGoToDynamicMenu(menu),
                                            proceed = true
                                        }
                                    );
                                }
                            }
                        }
                    )
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
                            label = "Back",
                            cancelAction = _ => this.ApplyChanges(),
                            submitAction = _ => this.ApplyChanges(),
                            proceed = true,
                            style = MenuButtonStyle.vanillaStyle
                        }
                    )
                )
                .Build();

            var optScreen = UIManager.instance.optionsMenuScreen;
            GameObject modButton = null;
            new ContentArea(optScreen.content.gameObject, new SingleContentLayout(new Vector2(0.5f, 0.5f)))
                .AddWrappedItem(
                    "ModMenuButtonWrapper",
                    c => c.AddMenuButton(
                        "ModMenuButton",
                        new MenuButtonConfig
                        {
                            cancelAction = self => UIManager.instance.UIGoToMainMenu(),
                            label = "Mods",
                            submitAction = GoToModListMenu,
                            proceed = true,
                            style = MenuButtonStyle.vanillaStyle
                        },
                        out modButton
                    )
                );
            var mbl = (Modding.Patches.MenuButtonList)optScreen.gameObject.GetComponent<MenuButtonList>();
            mbl.AddSelectableEnd(modButton.GetComponent<MenuButton>(), 1);
            mbl.RecalculateNavigation();
        }

        private void ApplyChanges()
        {
            foreach (var (mod, enabled) in changedMods)
            {
                var name = mod.GetName();
                if (this.modEnabledSettings[name] != enabled)
                {
                    this.modEnabledSettings[name] = enabled;
                    if (enabled)
                    {
                        ModLoader.LoadMod(mod, true);
                    }
                    else
                    {
                        ModLoader.UnloadMod(mod);
                    }
                }
            }
            changedMods.Clear();
            ((Patch.UIManager)UIManager.instance).UILeaveDynamicMenu(
                UIManager.instance.optionsMenuScreen,
                Patch.MainMenuState.OPTIONS_MENU
            );
        }

        private MenuScreen CreateModMenu(IMenuMod mod)
        {
            var name = mod.GetName();
            var entries = mod.GetMenuData();
            GameObject backButton = null;
            var builder = new MenuBuilder(UIManager.instance.UICanvas.gameObject, name)
                .CreateTitle(name, MenuTitleStyle.vanillaStyle)
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
                .CreateAutoMenuNav()
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
                            label = "Back",
                            cancelAction = GoToModListMenu,
                            submitAction = GoToModListMenu,
                            proceed = true,
                            style = MenuButtonStyle.vanillaStyle
                        },
                        out backButton
                    )
                );
            if (entries.Count > 5)
            {
                builder.AddContent(null, c => c.AddScrollPaneContent(
                    new ScrollbarConfig
                    {
                        cancelAction = _ => ((Patch.UIManager)UIManager.instance).UIGoToDynamicMenu(this.screen),
                        navigation = new Navigation
                        {
                            mode = Navigation.Mode.Explicit,
                            selectOnUp = backButton.GetComponent<MenuButton>(),
                            selectOnDown = backButton.GetComponent<MenuButton>()
                        },
                        position = new AnchoredPosition
                        {
                            childAnchor = new Vector2(0f, 1f),
                            parentAnchor = new Vector2(1f, 1f),
                            offset = new Vector2(-310f, 0f)
                        }
                    },
                    new RelLength(entries.Count * 105f),
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c => AddModMenuContent(entries, c)
                ));
            }
            else
            {
                builder.AddContent(
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c => AddModMenuContent(entries, c)
                );
            }
            return builder.Build();
        }

        private void GoToModListMenu(object _) => GoToModListMenu();
        private void GoToModListMenu() => ((Patch.UIManager)UIManager.instance).UIGoToDynamicMenu(this.screen);

        private void AddModMenuContent(List<IMenuMod.MenuEntry> entries, ContentArea c)
        {
            foreach (var entry in entries)
            {
                c.AddHorizontalOption(
                    entry.name,
                    new HorizontalOptionConfig
                    {
                        applySetting = (_, i) => entry.saver(i),
                        refreshSetting = (s, _) => s.optionList.SetOptionTo(entry.loader()),
                        cancelAction = GoToModListMenu,
                        description = string.IsNullOrEmpty(entry.description) ? null : new DescriptionInfo
                        {
                            text = entry.description,
                            style = DescriptionStyle.singleLineVanillaStyle
                        },
                        label = entry.name,
                        options = entry.values,
                        style = HorizontalOptionStyle.vanillaStyle
                    }
                );
            }
        }
    }
}