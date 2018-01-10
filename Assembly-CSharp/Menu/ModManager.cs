using System;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Modding.Menu
{
    //Menu based mod manager by @KDT
    internal class ModManager : Loggable
    {
        private static UIManager _uim;
        private static FauxUIManager _fauxUim;
        public static MenuScreen ModMenuScreen;

        public static Selectable[] ModArray;
        public static Selectable Back;

        public ModManager()
        {
            Log("Initializing");

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
            GameObject go = new GameObject();
            _fauxUim = go.AddComponent<FauxUIManager>();

            Log("Initialized");
        }

        public void DataDump(GameObject go, int depth)
        {
            LogDebug(new string('-', depth) + go.name);
            foreach (Component comp in go.GetComponents<Component>())
            {
                switch (comp.GetType().ToString())
                {
                    case "UnityEngine.RectTransform":
                        LogDebug(new string('+', depth) + comp.GetType() + " : " + ((RectTransform)comp).sizeDelta + ", " + ((RectTransform)comp).anchoredPosition + ", " + ((RectTransform)comp).anchorMin + ", " + ((RectTransform)comp).anchorMax);
                        break;
                    case "UnityEngine.UI.Text":
                        LogDebug(new string('+', depth) + comp.GetType() + " : " + ((Text)comp).text);
                        break;
                    default:
                        LogDebug(new string('+', depth) + comp.GetType());
                        break;
                }
            }
            foreach (Transform child in go.transform)
            {
                DataDump(child.gameObject, depth + 1);
            }
        }


        public static Sprite NullSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadRawTextureData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        }

        public static Sprite CreateSprite(byte[] data, int x, int y, int w, int h)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            tex.anisoLevel = 0;
            return Sprite.Create(tex, new Rect(x, y, w, h), Vector2.zero);
        }

        private void SceneLoaded(Scene scene, LoadSceneMode lsm)
        {
            try
            {
                if (_uim != null || ModLoader.LoadedMods == null || UIManager.instance == null) return;
            }
            catch (NullReferenceException)
            {
                //Do Nothing.  Something inside of UIManager.instance breaks even if you try to check for null on it. 
                return;
            }

            _uim = UIManager.instance;
            
            //ADD MODS TO OPTIONS MENU
            MenuButton defButton = (MenuButton)_uim.optionsMenuScreen.defaultHighlight;
            MenuButton modButton = Object.Instantiate(defButton.gameObject).GetComponent<MenuButton>();

            Navigation nav = modButton.navigation;
            nav.selectOnUp = defButton.FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown();
            nav.selectOnDown = defButton.FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown();
            modButton.navigation = nav;

            nav = modButton.FindSelectableOnUp().navigation;
            nav.selectOnDown = modButton;
            modButton.FindSelectableOnUp().navigation = nav;

            nav = modButton.FindSelectableOnDown().navigation;
            nav.selectOnUp = modButton;
            modButton.FindSelectableOnDown().navigation = nav;

            modButton.name = "Mods";

            modButton.transform.SetParent(modButton.FindSelectableOnUp().transform.parent);

            modButton.transform.localPosition = new Vector2(0, -120);
            modButton.transform.localScale = modButton.FindSelectableOnUp().transform.localScale;

            Object.Destroy(modButton.gameObject.GetComponent<AutoLocalizeTextUI>());
            modButton.gameObject.transform.FindChild("Text").GetComponent<Text>().text = "Mods";
            //ADD MODS TO OPTIONS MENU

            //SETUP MOD MENU
            GameObject go = Object.Instantiate(_uim.optionsMenuScreen.gameObject);
            ModMenuScreen = go.GetComponent<MenuScreen>();
            ModMenuScreen.title = ModMenuScreen.gameObject.transform.FindChild("Title").GetComponent<CanvasGroup>();
            ModMenuScreen.topFleur = ModMenuScreen.gameObject.transform.FindChild("TopFleur").GetComponent<Animator>();
            ModMenuScreen.content = ModMenuScreen.gameObject.transform.FindChild("Content").GetComponent<CanvasGroup>();

            ModMenuScreen.title.gameObject.GetComponent<Text>().text = "Mods";
            Object.Destroy(ModMenuScreen.title.gameObject.GetComponent<AutoLocalizeTextUI>());

            ModMenuScreen.transform.SetParent(_uim.optionsMenuScreen.gameObject.transform.parent);
            ModMenuScreen.transform.localPosition = _uim.optionsMenuScreen.gameObject.transform.localPosition;
            ModMenuScreen.transform.localScale = _uim.optionsMenuScreen.gameObject.transform.localScale;

            // ReSharper disable SuspiciousTypeConversion.Global
            List<ITogglableMod> managableMods = ModLoader.LoadedMods.Where(x => x is ITogglableMod).Select(x => x).Cast<ITogglableMod>().ToList();
            // ReSharper restore SuspiciousTypeConversion.Global

            //modMenuScreen.content = modMenuScreen.gameObject.transform.GetChild()
            ModMenuScreen.defaultHighlight = ModMenuScreen.content.gameObject.transform.GetChild(0).GetChild(0).GetComponent<MenuButton>();
            Object.Destroy(ModMenuScreen.defaultHighlight.FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown().gameObject.transform.parent.gameObject);
            Object.Destroy(ModMenuScreen.defaultHighlight.FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown().gameObject.transform.parent.gameObject);
            Object.Destroy(ModMenuScreen.defaultHighlight.FindSelectableOnDown().FindSelectableOnDown().FindSelectableOnDown().gameObject.transform.parent.gameObject);
            Object.Destroy(ModMenuScreen.defaultHighlight.FindSelectableOnDown().FindSelectableOnDown().gameObject.transform.parent.gameObject);
            Object.Destroy(ModMenuScreen.defaultHighlight.FindSelectableOnDown().gameObject.transform.parent.gameObject);
            
            Back = ModMenuScreen.defaultHighlight.FindSelectableOnUp();
            GameObject item = _uim.videoMenuScreen.defaultHighlight.FindSelectableOnDown().gameObject;
            Object.DestroyImmediate(item.GetComponent<MenuOptionHorizontal>());
            Object.DestroyImmediate(item.GetComponent<MenuSetting>());
            Object.DestroyImmediate(ModMenuScreen.content.GetComponent<VerticalLayoutGroup>());
            Object.Destroy(ModMenuScreen.defaultHighlight.gameObject.transform.parent.gameObject);

            if (managableMods.Count > 0)
            {

                ModArray = new Selectable[managableMods.Count];

                for (int i = 0; i < managableMods.Count; i++)
                {
                    GameObject menuItemParent = Object.Instantiate(item.gameObject);
                    FauxMenuOptionHorizontal menuItem = menuItemParent.AddComponent<FauxMenuOptionHorizontal>();
                    
                    menuItem.navigation = Navigation.defaultNavigation;
                    int modIndex = i;

                    //Manages what should happen when the menu option changes (the user clicks and the mod is toggled On/Off)
                    menuItem.OnUpdate += optionIndex =>
                    {
                        ITogglableMod mod = managableMods[modIndex];

                        string name = mod.GetName();

                        if (!ModHooks.Instance.GlobalSettings.ModEnabledSettings.ContainsKey(name))
                            ModHooks.Instance.GlobalSettings.ModEnabledSettings.Add(name, true);

                        if (optionIndex == 1)
                        {
                           ModLoader.UnloadMod(mod);
                        }
                        else
                        {
                           ModLoader.LoadMod(mod);
                        }
                    };
                    //dataDump(modArray[i].gameObject, 1);                    

                    menuItem.OptionList = new[] {"On", "Off"};
                    menuItem.OptionText = menuItem.gameObject.transform.GetChild(1).GetComponent<Text>();
                    menuItem.SelectedOptionIndex = ModHooks.Instance.GlobalSettings.ModEnabledSettings[managableMods[i].GetName()] ? 0 : 1;
                    menuItem.LocalizeText = false;
                    menuItem.SheetTitle = managableMods[i].GetName();

                    Object.DestroyImmediate(menuItem.transform.FindChild("Label")
                        .GetComponent<AutoLocalizeTextUI>());
                    menuItem.transform.FindChild("Label").GetComponent<Text>().text = managableMods[i].GetName();

                    menuItem.leftCursor = menuItem.transform.FindChild("CursorLeft").GetComponent<Animator>();
                    menuItem.rightCursor = menuItem.transform.FindChild("CursorRight").GetComponent<Animator>();

                    menuItem.gameObject.name = managableMods[i].GetName();

                    RectTransform rt = menuItemParent.GetComponent<RectTransform>();

                    rt.SetParent(ModMenuScreen.content.transform);
                    rt.localScale = new Vector3(2, 2, 2);

                    rt.sizeDelta = new Vector2(960, 120);
                    rt.anchoredPosition = new Vector2(0, (766 / 2) - 90 - (150 * i));
                    rt.anchorMin = new Vector2(0.5f, 1.0f);
                    rt.anchorMax = new Vector2(0.5f, 1.0f);

                    //Image img = menuItem.AddComponent<Image>();
                    //img.sprite = nullSprite();
                    
                    menuItem.cancelAction = CancelAction.DoNothing;

                    ModArray[i] = menuItem;

                    //AutoLocalizeTextUI localizeUI = modArray[i].GetComponent<AutoLocalizeTextUI>();
                    //modArray[i].transform.GetChild(0).GetComponent<Text>().text = mods[i];
                    //GameObject.Destroy(localizeUI);
                }

                Navigation[] navs = new Navigation[ModArray.Length];
                for (int i = 0; i < ModArray.Length; i++)
                {
                    navs[i] = new Navigation
                    {
                        mode = Navigation.Mode.Explicit,
                        selectOnUp = i == 0 ? Back : ModArray[i - 1],
                        selectOnDown = i == ModArray.Length - 1 ? Back : ModArray[i + 1]
                    };

                    ModArray[i].navigation = navs[i];
                }

                ModMenuScreen.defaultHighlight = ModArray[0];
                Navigation nav2 = Back.navigation;
                nav2.selectOnUp = ModArray[ModArray.Length - 1];
                nav2.selectOnDown = ModArray[0];
                Back.navigation = nav2;
            }
            

            ((MenuButton)Back).cancelAction = CancelAction.DoNothing;
            EventTrigger backEvents = Back.gameObject.GetComponent<EventTrigger>();

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
