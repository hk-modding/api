using System;
using System.Collections.Generic;
using System.Linq;
using Modding.Patches;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MenuSelectable = Modding.Patches.MenuSelectable;
using Object = UnityEngine.Object;

namespace Modding.Menu
{
    //Menu based mod manager by @KDT
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
            GameObject go = new GameObject();
            _fauxUim = go.AddComponent<FauxUIManager>();

            Log("Initialized");
        }

        private void SceneLoaded(Scene scene, LoadSceneMode lsm)
        {
            try
            {
                if (_uim != null || ModLoader.LoadedMods == null || UIManager.instance == null)
                {
                    return;
                }
            }
            catch (NullReferenceException)
            {
                //Do Nothing.  Something inside of UIManager.instance breaks even if you try to check for null on it. 
                return;
            }

            _uim = UIManager.instance;

            //ADD MODS TO OPTIONS MENU
            MenuButton defButton = (MenuButton) _uim.optionsMenuScreen.defaultHighlight;
            MenuButton modButton = Object.Instantiate(defButton.gameObject).GetComponent<MenuButton>();

            _uim.optionsMenuScreen.GetComponent<Patches.MenuButtonList>().AddSelectable(modButton, 5);

            modButton.name = "Mods";

            modButton.transform.SetParent(defButton.FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown()
                .FindSelectableOnDown().transform.parent);

            modButton.transform.localPosition = new Vector2(0, -120);
            modButton.transform.localScale = defButton.FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown()
                .FindSelectableOnDown().transform.localScale;

            Object.Destroy(modButton.gameObject.GetComponent<AutoLocalizeTextUI>());
            modButton.gameObject.transform.Find("Text").GetComponent<Text>().text = "Mods";
            //ADD MODS TO OPTIONS MENU

            //SETUP MOD MENU
            GameObject go = Object.Instantiate(_uim.optionsMenuScreen.gameObject);
            ModMenuScreen = go.GetComponent<MenuScreen>();
            ModMenuScreen.title = ModMenuScreen.gameObject.transform.Find("Title").GetComponent<CanvasGroup>();
            ModMenuScreen.topFleur = ModMenuScreen.gameObject.transform.Find("TopFleur").GetComponent<Animator>();
            ModMenuScreen.content = ModMenuScreen.gameObject.transform.Find("Content").GetComponent<CanvasGroup>();

            Patches.MenuButtonList modButtons = go.GetComponent<Patches.MenuButtonList>();
            modButtons.ClearSelectables();

            ModMenuScreen.title.gameObject.GetComponent<Text>().text = "Mods";
            Object.Destroy(ModMenuScreen.title.gameObject.GetComponent<AutoLocalizeTextUI>());

            ModMenuScreen.transform.SetParent(_uim.optionsMenuScreen.gameObject.transform.parent);
            ModMenuScreen.transform.localPosition = _uim.optionsMenuScreen.gameObject.transform.localPosition;
            ModMenuScreen.transform.localScale = _uim.optionsMenuScreen.gameObject.transform.localScale;

            List<ITogglableMod> managableMods = ModLoader.LoadedMods.Where(x => x is ITogglableMod).Select(x => x)
                .Cast<ITogglableMod>()
                .ToList();

            ModMenuScreen.defaultHighlight = ModMenuScreen.content.gameObject.transform.GetChild(0).GetChild(0)
                .GetComponent<MenuButton>();
            Object.Destroy(ModMenuScreen.defaultHighlight.FindSelectableOnDown().FindSelectableOnDown()
                .FindSelectableOnDown().FindSelectableOnDown().gameObject.transform.parent.gameObject);
            Object.Destroy(ModMenuScreen.defaultHighlight.FindSelectableOnDown().FindSelectableOnDown()
                .FindSelectableOnDown().gameObject.transform.parent.gameObject);
            Object.Destroy(ModMenuScreen.defaultHighlight.FindSelectableOnDown().FindSelectableOnDown().gameObject
                .transform.parent.gameObject);
            Object.Destroy(ModMenuScreen.defaultHighlight.FindSelectableOnDown().gameObject.transform.parent
                .gameObject);

            _back = ModMenuScreen.defaultHighlight.FindSelectableOnUp();
            GameObject item = _uim.videoMenuScreen.defaultHighlight.FindSelectableOnDown().gameObject;
            Object.DestroyImmediate(item.GetComponent<MenuOptionHorizontal>());
            Object.DestroyImmediate(item.GetComponent<MenuSetting>());
            Object.DestroyImmediate(ModMenuScreen.content.GetComponent<VerticalLayoutGroup>());
            Object.Destroy(ModMenuScreen.defaultHighlight.gameObject.transform.parent.gameObject);
            try
            {
                if (managableMods.Count > 0)
                {
                    _modArray = new Selectable[managableMods.Count];

                    for (int i = 0; i < managableMods.Count; i++)
                    {
                        GameObject menuItemParent = Object.Instantiate(item.gameObject);
                        FauxMenuOptionHorizontal menuItem = menuItemParent.AddComponent<FauxMenuOptionHorizontal>();

                        menuItem.navigation = new Navigation
                        {
                            mode = Navigation.Mode.Explicit
                        };

                        modButtons.AddSelectable(menuItem);

                        int modIndex = i;

                        //Manages what should happen when the menu option changes (the user clicks and the mod is toggled On/Off)
                        menuItem.OnUpdate += optionIndex =>
                        {
                            ITogglableMod mod = managableMods[modIndex];

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
                        menuItem.SelectedOptionIndex =
                            ModHooks.Instance.GlobalSettings.ModEnabledSettings[managableMods[i].GetName()] ? 0 : 1;
                        menuItem.LocalizeText = false;
                        menuItem.SheetTitle = managableMods[i].GetName();

                        Object.DestroyImmediate(menuItem.transform.Find("Label")
                            .GetComponent<AutoLocalizeTextUI>());
                        menuItem.transform.Find("Label").GetComponent<Text>().text = managableMods[i].GetName();

                        menuItem.leftCursor = menuItem.transform.Find("CursorLeft").GetComponent<Animator>();
                        menuItem.rightCursor = menuItem.transform.Find("CursorRight").GetComponent<Animator>();

                        menuItem.gameObject.name = managableMods[i].GetName();

                        RectTransform rt = menuItemParent.GetComponent<RectTransform>();

                        rt.SetParent(ModMenuScreen.content.transform);
                        rt.localScale = new Vector3(2, 2, 2);

                        rt.sizeDelta = new Vector2(960, 120);
                        rt.anchoredPosition = new Vector2(0, 766 / 2 - 90 - 150 * i);
                        rt.anchorMin = new Vector2(0.5f, 1.0f);
                        rt.anchorMax = new Vector2(0.5f, 1.0f);

                        menuItem.cancelAction = CancelAction.QuitModMenu;

                        _modArray[i] = menuItem;
                    }
                }
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
            EventTrigger backEvents = _back.gameObject.GetComponent<EventTrigger>();

            backEvents.triggers = new List<EventTrigger.Entry>();

            EventTrigger.Entry backSubmit = new EventTrigger.Entry {eventID = EventTriggerType.Submit};
            backSubmit.callback.AddListener(data => { _fauxUim.UIquitModMenu(); });
            backEvents.triggers.Add(backSubmit);

            EventTrigger.Entry backClick = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            backClick.callback.AddListener(data => { _fauxUim.UIquitModMenu(); });
            backEvents.triggers.Add(backClick);


            //SETUP MOD MENU
            LogDebug("About to add the events to the menu option");
            //SETUP MOD BUTTON TO RESPOND TO SUBMIT AND CANCEL EVENTS CORRECTLY
            EventTrigger events = modButton.gameObject.GetComponent<EventTrigger>();

            events.triggers = new List<EventTrigger.Entry>();

            EventTrigger.Entry submit = new EventTrigger.Entry {eventID = EventTriggerType.Submit};
            submit.callback.AddListener(data => { _fauxUim.UIloadModMenu(); });
            events.triggers.Add(submit);

            EventTrigger.Entry click = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            click.callback.AddListener(data => { _fauxUim.UIloadModMenu(); });
            events.triggers.Add(click);

            //SETUP MOD BUTTON TO RESPOND TO SUBMIT AND CANCEL EVENTS CORRECTLY
        }
    }
}