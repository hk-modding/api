using System;
using System.Collections.Generic;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;
using static Modding.ModLoader;
using Patch = Modding.Patches;
using Lang = Language.Language;

namespace Modding
{
    internal class ModListMenu
    {
        private MenuScreen screen;

        private Dictionary<ModInstance, bool> changedMods = new Dictionary<ModInstance, bool>();

        public static Dictionary<IMod, MenuScreen> ModScreens = new Dictionary<IMod, MenuScreen>();

        // Due to the lifecycle of the UIManager object, The `EditMenus` event has to be used to create custom menus.
        // This event is called every time a UIManager is created,
        // and will also call the added action if the UIManager has already started.
        internal void InitMenuCreation()
        {
            Patch.UIManager.BeforeHideDynamicMenu += ToggleMods;
            Patch.UIManager.EditMenus += () =>
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
                                },
                                SelectionPadding = _ => (-120, 120)
                            },
                            new RelLength(0f),
                            RegularGridLayout.CreateVerticalLayout(105f),
                            c =>
                            {
                                foreach (var modInst in ModLoader.ModInstances)
                                {
                                    if (modInst.Error != null) continue;
                                    ModToggleDelegates? toggleDels = null;
                                    if (modInst.Mod is ITogglableMod itmod)
                                    {
                                        try 
                                        {    
                                            if (
                                                modInst.Mod is not (
                                                    IMenuMod { ToggleButtonInsideMenu: true } or
                                                    ICustomMenuMod { ToggleButtonInsideMenu: true }
                                                )
                                            )
                                            {
                                                var rt = c.ContentObject.GetComponent<RectTransform>();
                                                rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                                c.AddHorizontalOption(
                                                    modInst.Name,
                                                    new HorizontalOptionConfig
                                                    {
                                                        ApplySetting = (self, ind) =>
                                                        {
                                                            changedMods[modInst] = ind == 1;
                                                        },
                                                        CancelAction = _ => this.ApplyChanges(),
                                                        Label = modInst.Name,
                                                        Options = new string[] {
                                                            Lang.Get("MOH_OFF", "MainMenu"),
                                                            Lang.Get("MOH_ON", "MainMenu")
                                                        },
                                                        RefreshSetting = (self, apply) => self.optionList.SetOptionTo(
                                                            modInst.Enabled ? 1 : 0
                                                        ),
                                                        Style = HorizontalOptionStyle.VanillaStyle,
                                                        Description = new DescriptionInfo
                                                        {
                                                            Text = $"v{modInst.Mod.GetVersion()}"
                                                        }
                                                    },
                                                    out var opt
                                                );
                                                opt.menuSetting.RefreshValueFromGameSettings();
                                            }
                                            else
                                            {
                                                toggleDels = new ModToggleDelegates
                                                {
                                                    SetModEnabled = enabled =>
                                                    {
                                                        changedMods[modInst] = enabled;
                                                    },
                                                    GetModEnabled = () => modInst.Enabled,
                                                    #pragma warning disable CS0618
                                                    // Kept for backwards compatability.
                                                    ApplyChange = () => {  }
                                                    #pragma warning restore CS0618
                                                };
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.APILogger.LogError(e);
                                        }
                                    }
                                    if (modInst.Mod is IMenuMod)
                                    {
                                        try 
                                        {
                                            var menu = CreateModMenu(modInst, toggleDels);
                                            var rt = c.ContentObject.GetComponent<RectTransform>();
                                            rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                            c.AddMenuButton(
                                                $"{modInst.Name}_Settings",
                                                new MenuButtonConfig
                                                {
                                                    Style = MenuButtonStyle.VanillaStyle,
                                                    CancelAction = _ => this.ApplyChanges(),
                                                    Label = toggleDels == null ? $"{modInst.Name} {Lang.Get("MAIN_OPTIONS", "MainMenu")}" : modInst.Name,
                                                    SubmitAction = _ => ((Patch.UIManager)UIManager.instance)
                                                        .UIGoToDynamicMenu(menu),
                                                    Proceed = true,
                                                    Description = new DescriptionInfo
                                                    {
                                                        Text = $"v{modInst.Mod.GetVersion()}"
                                                    }
                                                }
                                            );
                                            ModScreens[modInst.Mod] = menu;
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.APILogger.LogError(e);
                                        }
                                    }
                                    else if (modInst.Mod is ICustomMenuMod icmmod)
                                    {
                                        try {
                                            var menu = icmmod.GetMenuScreen(this.screen, toggleDels);
                                            var rt = c.ContentObject.GetComponent<RectTransform>();
                                            rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                            c.AddMenuButton(
                                                $"{modInst.Name}_Settings",
                                                new MenuButtonConfig
                                                {
                                                    Style = MenuButtonStyle.VanillaStyle,
                                                    CancelAction = _ => this.ApplyChanges(),
                                                    Label = toggleDels == null ? $"{modInst.Name} {Lang.Get("MAIN_OPTIONS", "MainMenu")}" : modInst.Name,
                                                    SubmitAction = _ => ((Patch.UIManager)UIManager.instance)
                                                        .UIGoToDynamicMenu(menu),
                                                    Proceed = true,
                                                    Description = new DescriptionInfo
                                                    {
                                                        Text = $"v{modInst.Mod.GetVersion()}"
                                                    }
                                                }
                                            );
                                            ModScreens[modInst.Mod] = menu;
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.APILogger.LogError(e);
                                        }
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
                                Label = Lang.Get("NAV_BACK", "MainMenu"),
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
                                    CancelAction = self => UIManager.instance.UILeaveOptionsMenu(),
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
        }

        private void ToggleMods()
        {
            foreach (var (mod, enabled) in changedMods)
            {
                var name = mod.Name;
                if (enabled)
                {
                    ModLoader.LoadMod(mod, true);
                }
                else
                {
                    ModLoader.UnloadMod(mod);
                }
            } 
            changedMods.Clear();
        }

        private void ApplyChanges()
        {
            ToggleMods();
            ((Patch.UIManager)UIManager.instance).UILeaveDynamicMenu(
                UIManager.instance.optionsMenuScreen,
                Patch.MainMenuState.OPTIONS_MENU
            );
        }

        private MenuScreen CreateModMenu(ModInstance modInst, ModToggleDelegates? toggleDelegates)
        {
            var mod = modInst.Mod as IMenuMod;
            IMenuMod.MenuEntry? toggleEntry = toggleDelegates is ModToggleDelegates dels ? new IMenuMod.MenuEntry
            {
                Name = modInst.Name,
                Values = new string[] {
                    Lang.Get("MOH_OFF", "MainMenu"),
                    Lang.Get("MOH_ON", "MainMenu")
                },
                Saver = v => dels.SetModEnabled(v == 1),
                Loader = () => dels.GetModEnabled() ? 1 : 0,
            } : null;
            
            var name = modInst.Name;
            var entries = mod.GetMenuData(toggleEntry);
            return MenuUtils.CreateMenuScreen(name, entries, this.screen);
        }

        private void GoToModListMenu(object _) => GoToModListMenu();
        private void GoToModListMenu() => ((Patch.UIManager)UIManager.instance).UIGoToDynamicMenu(this.screen);

    }
}
