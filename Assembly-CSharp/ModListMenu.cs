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
            this.screen = new MenuBuilder(UIManager.instance.UICanvas.gameObject, "ModListMenu")
                .CreateAutoMenuNav()
                .CreateTitle(
                    "Mods",
                    new MenuTitleStyle
                    {
                        pos = new RectPosition
                        {
                            childAnchor = new Vector2(0.5f, 0.5f),
                            parentAnchor = new Vector2(0.5f, 0.5f),
                            offset = new Vector2(0f, 544f)
                        },
                        textSize = 75
                    }
                )
                .CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RectSize(new Vector2(1920f, 903f)),
                    new RectPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ))
                .CreateControlPane(RectTransformData.FromSizeAndPos(
                    new RectSize(new Vector2(1920f, 259f)),
                    new RectPosition(
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
                            position = new RectPosition
                            {
                                childAnchor = new Vector2(0f, 1f),
                                parentAnchor = new Vector2(1f, 1f),
                                offset = new Vector2(-310f, 0f)
                            }
                        },
                        new ParentRelLength(0f),
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
                            }
                        }
                    )
                )
                .AddControls(
                    new SingleContentLayout(new RectPosition(
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
                            submitAction = self => ((Patch.UIManager)UIManager.instance).UIGoToDynamicMenu(this.screen),
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
    }
}