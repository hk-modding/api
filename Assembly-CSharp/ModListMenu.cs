using System;
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

        private Dictionary<string, bool> modEnabledSettings = ModHooks.GlobalSettings.ModEnabledSettings;

        public static Dictionary<IMod, MenuScreen> ModScreens = new Dictionary<IMod, MenuScreen>();

        // Due to the lifecycle of the UIManager object, The `EditMenus` event has to be used to create custom menus.
        // This event is called every time a UIManager is created,
        // and will also call the added action if the UIManager has already started.
        internal void InitMenuCreation() => Patch.UIManager.EditMenus += () =>
        {
            ModScreens = new Dictionary<IMod, MenuScreen>();
            var builder = new MenuBuilder("ModListMenu");
            this.screen = builder.Screen;
            builder.CreateTitle("Mods", MenuTitleStyle.vanillaStyle)
                .SetDefaultNavGraph(new ChainedNavGraph())
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
                    new NullContentLayout(),
                    c => c.AddScrollPaneContent(
                        new ScrollbarConfig
                        {
                            CancelAction = _ => this.ApplyChanges(),
                            Navigation = new Navigation { mode = Navigation.Mode.Explicit },
                            Position = new AnchoredPosition
                            {
                                ChildAnchor = new Vector2(0f, 1f),
                                ParentAnchor = new Vector2(1f, 1f),
                                Offset = new Vector2(-310f, 0f)
                            }
                        },
                        new RelLength(0f),
                        RegularGridLayout.CreateVerticalLayout(105f),
                        c =>
                        {
                            foreach (var mod in ModLoader.LoadedMods)
                            {
                                ModToggleDelegates? toggleDels = null;
                                if (mod is ITogglableMod itmod)
                                {
                                    if (
                                        mod is not (
                                            IMenuMod { ToggleButtonInsideMenu: true } or
                                            ICustomMenuMod { ToggleButtonInsideMenu: true }
                                        )
                                    )
                                    {
                                        var rt = c.ContentObject.GetComponent<RectTransform>();
                                        rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                        c.AddHorizontalOption(
                                            itmod.GetName(),
                                            new HorizontalOptionConfig
                                            {
                                                ApplySetting = (self, ind) =>
                                                {
                                                    changedMods[itmod] = ind == 1;
                                                },
                                                CancelAction = _ => this.ApplyChanges(),
                                                Label = itmod.GetName(),
                                                Options = new string[] { "Off", "On" },
                                                RefreshSetting = (self, apply) => self.optionList.SetOptionTo(
                                                    this.modEnabledSettings[itmod.GetName()] ? 1 : 0
                                                ),
                                                Style = HorizontalOptionStyle.VanillaStyle,
                                                Description = new DescriptionInfo
                                                {
                                                    Text = $"Version {mod.GetVersion()}"
                                                }
                                            },
                                            out var opt
                                        );
                                        opt.menuSetting.RefreshValueFromGameSettings();
                                    }
                                    else
                                    {
                                        bool? change = null;
                                        string name = itmod.GetName();
                                        toggleDels = new ModToggleDelegates
                                        {
                                            SetModEnabled = enabled =>
                                            {
                                                change = enabled;
                                            },
                                            GetModEnabled = () => this.modEnabledSettings[name],
                                            ApplyChange = () =>
                                            {
                                                if (change is bool enabled)
                                                {
                                                    if (this.modEnabledSettings[name] != enabled)
                                                    {
                                                        this.modEnabledSettings[name] = enabled;
                                                    }
                                                    if (enabled)
                                                    {
                                                        ModLoader.LoadMod(itmod, true);
                                                    }
                                                    else
                                                    {
                                                        ModLoader.UnloadMod(itmod);
                                                    }
                                                }
                                                change = null;
                                            }
                                        };
                                    }
                                }
                                if (mod is IMenuMod immod)
                                {
                                    var menu = CreateModMenu(immod, toggleDels);
                                    var rt = c.ContentObject.GetComponent<RectTransform>();
                                    rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                    c.AddMenuButton(
                                        $"{immod.GetName()}_Settings",
                                        new MenuButtonConfig
                                        {
                                            Style = MenuButtonStyle.VanillaStyle,
                                            CancelAction = _ => this.ApplyChanges(),
                                            Label = $"{immod.GetName()} Settings",
                                            SubmitAction = _ => ((Patch.UIManager)UIManager.instance)
                                                .UIGoToDynamicMenu(menu),
                                            Proceed = true,
                                            Description = new DescriptionInfo
                                            {
                                                Text = $"Version {mod.GetVersion()}"
                                            }
                                        }
                                    );
                                    ModScreens[mod] = menu;
                                }
                                else if (mod is ICustomMenuMod icmmod)
                                {
                                    var menu = icmmod.GetMenuScreen(this.screen, toggleDels);
                                    var rt = c.ContentObject.GetComponent<RectTransform>();
                                    rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                    c.AddMenuButton(
                                        $"{icmmod.GetName()}_Settings",
                                        new MenuButtonConfig
                                        {
                                            Style = MenuButtonStyle.VanillaStyle,
                                            CancelAction = _ => this.ApplyChanges(),
                                            Label = $"{icmmod.GetName()} Settings",
                                            SubmitAction = _ => ((Patch.UIManager)UIManager.instance)
                                                .UIGoToDynamicMenu(menu),
                                            Proceed = true,
                                            Description = new DescriptionInfo
                                            {
                                                Text = $"Version {mod.GetVersion()}"
                                            }
                                        }
                                    );
                                    ModScreens[mod] = menu;
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
                            Label = "Back",
                            CancelAction = _ => this.ApplyChanges(),
                            SubmitAction = _ => this.ApplyChanges(),
                            Proceed = true,
                            Style = MenuButtonStyle.VanillaStyle
                        }
                    )
                )
                .Build();

            var optScreen = UIManager.instance.optionsMenuScreen;
            var mbl = (Modding.Patches.MenuButtonList)optScreen.gameObject.GetComponent<MenuButtonList>();
            new ContentArea(optScreen.content.gameObject, new SingleContentLayout(new Vector2(0.5f, 0.5f)))
                .AddWrappedItem(
                    "ModMenuButtonWrapper",
                    c =>
                    {
                        c.AddMenuButton(
                            "ModMenuButton",
                            new MenuButtonConfig
                            {
                                CancelAction = self => UIManager.instance.UIGoToMainMenu(),
                                Label = "Mods",
                                SubmitAction = GoToModListMenu,
                                Proceed = true,
                                Style = MenuButtonStyle.VanillaStyle
                            },
                            out var modButton
                        );
                        mbl.AddSelectableEnd(modButton, 1);
                    }
                );
            mbl.RecalculateNavigation();
        };

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

        private MenuScreen CreateModMenu(IMenuMod mod, ModToggleDelegates? toggleDelegates)
        {
            IMenuMod.MenuEntry? toggleEntry = toggleDelegates is ModToggleDelegates dels ? new IMenuMod.MenuEntry
            {
                Name = mod.GetName(),
                Values = new string[] { "Off", "On" },
                Saver = v => dels.SetModEnabled(v == 1),
                Loader = () => dels.GetModEnabled() ? 1 : 0,
            } : null;
            Action<MenuSelectable> returnDelegate = toggleDelegates is ModToggleDelegates
            {
                ApplyChange: var applyChange
            } ? _ =>
            {
                applyChange();
                this.GoToModListMenu();
            }
            : this.GoToModListMenu;

            var name = mod.GetName();
            var entries = mod.GetMenuData(toggleEntry);
            MenuButton backButton = null;
            var builder = new MenuBuilder(name)
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
                .SetDefaultNavGraph(new ChainedNavGraph());
            if (entries.Count > 5)
            {
                builder.AddContent(new NullContentLayout(), c => c.AddScrollPaneContent(
                    new ScrollbarConfig
                    {
                        CancelAction = _ => ((Patch.UIManager)UIManager.instance).UIGoToDynamicMenu(this.screen),
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
                    new RelLength(entries.Count * 105f),
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c => AddModMenuContent(entries, c, returnDelegate)
                ));
            }
            else
            {
                builder.AddContent(
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c => AddModMenuContent(entries, c, returnDelegate)
                );
            }
            builder.AddControls(
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
                        CancelAction = returnDelegate,
                        SubmitAction = this.GoToModListMenu,
                        Proceed = true,
                        Style = MenuButtonStyle.VanillaStyle
                    },
                    out backButton
                )
            );
            return builder.Build();
        }

        private void GoToModListMenu(object _) => GoToModListMenu();
        private void GoToModListMenu() => ((Patch.UIManager)UIManager.instance).UIGoToDynamicMenu(this.screen);

        private void AddModMenuContent(
            List<IMenuMod.MenuEntry> entries,
            ContentArea c,
            Action<MenuSelectable> returnDelegate
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
                        CancelAction = returnDelegate,
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
    }
}