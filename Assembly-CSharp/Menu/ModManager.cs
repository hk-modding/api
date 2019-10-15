using System;
using System.Collections.Generic;
using System.Linq;
using Modding.Patches;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MenuSelectable = Modding.Patches.MenuSelectable;
using UObject = UnityEngine.Object;

namespace Modding.Menu
{
    // Menu based mod manager by @KDT
    internal class ModManager : Loggable
    {
        private static UIManager _uim;
        private static FauxUIManager _fauxUim;
        public static MenuScreen ModMenuScreen;

        private static Selectable[] _modArray;
        private static Selectable _back;

        public ModManager()
        {
            Log("Initializing");

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;

            var go = new GameObject();
            _fauxUim = go.AddComponent<FauxUIManager>();

            Log("Initialized");
        }

        private void SceneLoaded(Scene scene, LoadSceneMode lsm)
        {
            if
            (
                _uim                    != null
                || ModLoader.LoadedMods == null
                || UIManager.instance   == null
                || scene.name           != Constants.MENU_SCENE
            )
            {
                return;
            }

            _uim = UIManager.instance;

            var defButton = (MenuButton) _uim.optionsMenuScreen.defaultHighlight;

            var modButton = UObject.Instantiate(defButton.gameObject).GetComponent<MenuButton>();

            modButton.name = "Mods";
            
            _uim.optionsMenuScreen.GetComponent<Patches.MenuButtonList>().AddSelectable(modButton, 5);

            Selectable sel = FindSelectable(defButton, 4, FindSelectableOnDown);

            modButton.transform.parent = sel.transform.parent;
            modButton.transform.localPosition = new Vector2(0, -120);
            modButton.transform.localScale = sel.transform.localScale;

            UObject.Destroy(modButton.gameObject.GetComponent<AutoLocalizeTextUI>());

            modButton.GetComponentInChildren<Text>().text = "Mods";

            GameObject go = UObject.Instantiate(_uim.optionsMenuScreen.gameObject);
            ModMenuScreen = go.GetComponent<MenuScreen>();
            ModMenuScreen.title = ModMenuScreen.transform.Find("Title").GetComponent<CanvasGroup>();
            ModMenuScreen.topFleur = ModMenuScreen.transform.Find("TopFleur").GetComponent<Animator>();
            ModMenuScreen.content = ModMenuScreen.transform.Find("Content").GetComponent<CanvasGroup>();

            Patches.MenuButtonList modButtons = go.GetComponent<Patches.MenuButtonList>();
            modButtons.ClearSelectables();

            ModMenuScreen.title.gameObject.GetComponent<Text>().text = "Mods";

            UObject.Destroy(ModMenuScreen.title.gameObject.GetComponent<AutoLocalizeTextUI>());

            ModMenuScreen.transform.parent = _uim.optionsMenuScreen.transform.parent;
            ModMenuScreen.transform.localPosition = _uim.optionsMenuScreen.transform.localPosition;
            ModMenuScreen.transform.localScale = _uim.optionsMenuScreen.transform.localScale;

            List<ITogglableMod> managableMods = ModLoader.LoadedMods.OfType<ITogglableMod>().ToList();

            ModMenuScreen.defaultHighlight = ModMenuScreen.content.gameObject.transform.GetChild(0)
                                                          .GetChild(0)
                                                          .GetComponent<MenuButton>();

            for (int i = 4; i >= 1; i--)
            {
                DestroyParent
                (
                    FindSelectable(ModMenuScreen.defaultHighlight, i, FindSelectableOnDown)
                );
            }

            _back = ModMenuScreen.defaultHighlight.FindSelectableOnUp();

            GameObject item = _uim.videoMenuScreen.defaultHighlight.FindSelectableOnDown().gameObject;

            UObject.DestroyImmediate(item.GetComponent<MenuOptionHorizontal>());
            UObject.DestroyImmediate(item.GetComponent<MenuSetting>());
            UObject.DestroyImmediate(ModMenuScreen.content.GetComponent<VerticalLayoutGroup>());
            DestroyParent(ModMenuScreen.defaultHighlight);

            try
            {
                SetupMods(managableMods, item);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            _back.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit
                
            };
            
            modButtons.AddSelectable(_back);
            modButtons.RecalculateNavigation();

            ((MenuSelectable) _back).cancelAction = CancelAction.QuitModMenu;

            void Quit(BaseEventData data) => _fauxUim.UIquitModMenu();
            void Load(BaseEventData data) => _fauxUim.UIloadModMenu();

            EventTrigger[] ets =
            {
                _back.GetComponent<EventTrigger>(),
                modButton.GetComponent<EventTrigger>()
            };

            for (int i = 0; i < ets.Length; i++)
            {
                EventTrigger et = ets[i];
                et.triggers = new List<EventTrigger.Entry>();

                foreach (EventTriggerType type in new EventTriggerType[] {EventTriggerType.Submit, EventTriggerType.PointerClick})
                {
                    var trigger = new EventTrigger.Entry {eventID = type};
                    trigger.callback.AddListener
                    (
                        i == 0
                            ? (UnityAction<BaseEventData>) Quit
                            : Load
                    );
                    et.triggers.Add(trigger);
                }
            }
        }

        private void SetupMods(IList<ITogglableMod> managableMods, GameObject item)
        {
            if (managableMods.Count <= 0) return;

            _modArray = new Selectable[managableMods.Count];

            for (int i = 0; i < managableMods.Count; i++)
            {
                ITogglableMod mod = managableMods[i];
                
                GameObject menuItemParent = UObject.Instantiate(item.gameObject);
                var menuItem = menuItemParent.AddComponent<FauxMenuOptionHorizontal>();

                menuItem.navigation = Navigation.defaultNavigation;
                
                // Manages what should happen when the menu option changes (the user clicks and the mod is toggled On/Off)
                menuItem.OnUpdate += optionIndex =>
                {
                    string name = mod.GetName();

                    if (!ModHooks.Instance.GlobalSettings.ModEnabledSettings.ContainsKey(name))
                    {
                        ModHooks.Instance.GlobalSettings.ModEnabledSettings.Add(name, true);
                    }

                    try
                    {
                        if (optionIndex == 1)
                        {
                            ModLoader.UnloadMod(mod);
                        }
                        else
                        {
                            ModLoader.LoadMod(mod, true);
                        }
                    }
                    catch (Exception e)
                    {
                        LogError($"Could not load/unload mod \"{name}\":\n{e}");
                    }
                };

                menuItem.OptionList = new[] {"On", "Off"};
                menuItem.OptionText = menuItem.gameObject.transform.GetChild(1).GetComponent<Text>();
                menuItem.SelectedOptionIndex = ModHooks.Instance.GlobalSettings.ModEnabledSettings[mod.GetName()] ? 0 : 1;
                menuItem.LocalizeText = false;
                menuItem.SheetTitle = mod.GetName();

                Transform label = menuItem.transform.Find("Label");
                
                UObject.DestroyImmediate(label.GetComponent<AutoLocalizeTextUI>());
                label.GetComponent<Text>().text = mod.GetName();

                menuItem.leftCursor = menuItem.transform.Find("CursorLeft").GetComponent<Animator>();
                menuItem.rightCursor = menuItem.transform.Find("CursorRight").GetComponent<Animator>();

                menuItem.gameObject.name = mod.GetName();

                var rt = menuItemParent.GetComponent<RectTransform>();

                rt.parent = ModMenuScreen.content.transform;
                rt.localScale = new Vector3(2, 2, 2);

                rt.sizeDelta = new Vector2(960, 120);
                rt.anchoredPosition = new Vector2(0, 766 / 2 - 90 - 150 * i);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1.0f);

                menuItem.cancelAction = CancelAction.QuitModMenu;

                _modArray[i] = menuItem;
            }
        }

        private static Selectable FindSelectable(Selectable s, int offset, Func<Selectable, Selectable> func)
        {
            for (int i = 0; i < offset; i++)
            {
                s = func(s);
            }

            return s;
        }

        private static Selectable FindSelectableOnDown(Selectable s) => s.FindSelectableOnDown();

        private static void DestroyParent(Component c) => UObject.Destroy(c.transform.parent.gameObject);
    }
}