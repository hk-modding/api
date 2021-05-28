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

        public void InitMenu()
        {
            var builder = new MenuBuilder(UIManager.instance.UICanvas.gameObject, "ModListMenu");
            this.screen = builder.Screen;
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
                                if (mod is ITogglableMod itmod)
                                {
                                    var rt = c.contentObject.GetComponent<RectTransform>();
                                    rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y + 105f);
                                    MenuOptionHorizontal opt;
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
                                                Text = $"Version {mod.GetVersion()}",
                                                Style = DescriptionStyle.SingleLineVanillaStyle
                                            }
                                        },
                                        out opt
                                    );
                                    opt.menuSetting.RefreshValueFromGameSettings();
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
                                            Style = MenuButtonStyle.VanillaStyle,
                                            CancelAction = _ => this.ApplyChanges(),
                                            Label = $"{immod.GetName()} Settings",
                                            SubmitAction = _ => ((Patch.UIManager)UIManager.instance)
                                                .UIGoToDynamicMenu(menu),
                                            Proceed = true
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
                                            Style = MenuButtonStyle.VanillaStyle,
                                            CancelAction = _ => this.ApplyChanges(),
                                            Label = $"{icmmod.GetName()} Settings",
                                            SubmitAction = _ => ((Patch.UIManager)UIManager.instance)
                                                .UIGoToDynamicMenu(menu),
                                            Proceed = true
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
            MenuButton backButton = null;
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
                            Label = "Back",
                            CancelAction = GoToModListMenu,
                            SubmitAction = GoToModListMenu,
                            Proceed = true,
                            Style = MenuButtonStyle.VanillaStyle
                        },
                        out backButton
                    )
                );
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
                        ApplySetting = (_, i) => entry.saver(i),
                        RefreshSetting = (s, _) => s.optionList.SetOptionTo(entry.loader()),
                        CancelAction = GoToModListMenu,
                        Description = string.IsNullOrEmpty(entry.description) ? null : new DescriptionInfo
                        {
                            Text = entry.description,
                            Style = DescriptionStyle.SingleLineVanillaStyle
                        },
                        Label = entry.name,
                        Options = entry.values,
                        Style = HorizontalOptionStyle.VanillaStyle
                    }
                );
            }
        }
    }
}