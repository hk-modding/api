using System;
using UnityEngine;
using UnityEngine.UI;
using Modding.Menu.Config;

namespace Modding.Menu
{
    /// <summary>
    /// A builder style class for creating in-game menus.
    /// </summary>
    public class MenuBuilder
    {
        /// <summary>
        /// The root game object of the menu.
        /// </summary>
        public GameObject MenuObject { get; set; }
        /// <summary>
        /// The <c>MenuScreen</c> component on <c>menuObject</c>.
        /// </summary>
        public MenuScreen Screen { get; set; }

        /// <summary>
        /// An event that gets called before content is added in <c>AddContent</c>.
        /// </summary>
        public event Action<MenuBuilder, ContentArea> BeforeAddContent;
        /// <summary>
        /// An event that gets called before content is added in <c>AddControls</c>.
        /// </summary>
        public event Action<MenuBuilder, ContentArea> BeforeAddControls;
        /// <summary>
        /// An event that gets called at the start of <c>Build</c>.
        /// </summary>
        public event Action<MenuBuilder> OnBuild;

        /// <summary>
        /// Creates a new <c>MenuBuilder</c> on a canvas.
        /// </summary>
        /// <param name="canvas">The canvas to make the root menu on</param>
        /// <param name="name">The name of the root menu</param>
        public MenuBuilder(GameObject canvas, string name)
        {
            MenuObject = new GameObject(name);
            GameObject.DontDestroyOnLoad(MenuObject);
            MenuObject.transform.SetParent(canvas.transform, false);
            MenuObject.SetActive(false);
            // MenuScreen
            Screen = MenuObject.AddComponent<MenuScreen>();
            // CanvasRenderer
            MenuObject.AddComponent<CanvasRenderer>();
            // RectTransform
            var rt = MenuObject.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 463f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(0f, 0f);
            rt.localScale = new Vector3(0.7f, 0.7f, 1f);
            // CanvasGroup
            MenuObject.AddComponent<CanvasGroup>();
        }

        /// <summary>
        /// Builds the menu, calling any <c>OnBuild</c> events and returning the screen.
        /// </summary>
        /// <returns></returns>
        public MenuScreen Build()
        {
            OnBuild?.Invoke(this);
            return Screen;
        }

        /// <summary>
        /// Adds "content" to the menu in a certain layout. <br/>
        /// If <c>CreateContentPane</c> has not been called yet, this method will immeddiately return.
        /// </summary>
        /// <param name="layout">The layout of the added content</param>
        /// <param name="action">The action that will get called to add the content</param>
        /// <returns></returns>
        public MenuBuilder AddContent(IContentLayout layout, Action<ContentArea> action)
        {
            if (Screen.content == null)
            {
                return this;
            }
            var ca = new ContentArea(Screen.content.gameObject, layout);
            BeforeAddContent?.Invoke(this, ca);
            action(ca);
            return this;
        }

        /// <summary>
        /// Adds "content" to the control pane in a certain layout. <br/>
        /// If <c>CreateControlPane</c> has not been called yet, this method will immeddiately return.
        /// </summary>
        /// <param name="layout">The layout to apply to the added content.</param>
        /// <param name="action">The action that will get called to add the content.</param>
        /// <returns></returns>
        public MenuBuilder AddControls(IContentLayout layout, Action<ContentArea> action)
        {
            if (Screen.controls == null)
            {
                return this;
            }
            var ca = new ContentArea(Screen.controls.gameObject, layout);
            BeforeAddControls?.Invoke(this, ca);
            action(ca);
            return this;
        }

        /// <summary>
        /// Adds a <c>MenuItemNav</c> component to the root menu object.
        /// </summary>
        /// <returns></returns>
        public MenuBuilder CreateAutoMenuNav()
        {
            var itemNavList = MenuObject.AddComponent<MenuItemNav>();
            BeforeAddContent += (self, c) => c.OnMenuItemAdd += itemNavList.Content.Add;
            BeforeAddControls += (self, c) => c.OnMenuItemAdd += itemNavList.Controls.Add;
            OnBuild += self => itemNavList.RecalculateNavigation();
            return this;
        }

        /// <summary>
        /// Adds a title and top fleur to the menu.
        /// </summary>
        /// <param name="title">The title to render on the menu.</param>
        /// <param name="style">The styling of the title.</param>
        /// <returns></returns>
        public MenuBuilder CreateTitle(string title, MenuTitleStyle style)
        {
            // Title
            var titleObj = new GameObject("Title");
            GameObject.DontDestroyOnLoad(titleObj);
            titleObj.transform.SetParent(MenuObject.transform, false);
            // CanvasRenderer
            titleObj.AddComponent<CanvasRenderer>();
            // RectTransform
            var titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.sizeDelta = new Vector2(0f, 107f);
            titleRt.anchorMin = new Vector2(0f, 0.5f);
            titleRt.anchorMax = new Vector2(1f, 0.5f);
            style.pos.Reposition(titleRt);
            // CanvasGroup
            Screen.title = titleObj.AddComponent<CanvasGroup>();
            // ZeroAlphaOnStart
            titleObj.AddComponent<ZeroAlphaOnStart>();
            // Text
            var titleText = titleObj.AddComponent<Text>();
            titleText.font = MenuResources.TrajanBold;
            titleText.fontSize = style.textSize;
            titleText.resizeTextMaxSize = style.textSize;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = title;
            titleText.supportRichText = true;

            // TopFleur
            var fleur = new GameObject("TopFleur");
            GameObject.DontDestroyOnLoad(fleur);
            fleur.transform.SetParent(MenuObject.transform, false);
            // CanvasRenderer
            fleur.AddComponent<CanvasRenderer>();
            // RectTransform
            var fleurRt = fleur.AddComponent<RectTransform>();
            fleurRt.sizeDelta = new Vector2(1087f, 98f);
            AnchoredPosition.FromSiblingAnchor(
                new Vector2(0.5f, 0.5f),
                titleRt,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -102.5f)
            ).Reposition(fleurRt);
            // Animator
            var fleurAnimator = fleur.AddComponent<Animator>();
            fleurAnimator.runtimeAnimatorController = MenuResources.MenuTopFleurAnimator;
            fleurAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            fleurAnimator.applyRootMotion = false;
            Screen.topFleur = fleurAnimator;
            // Image
            var image = fleur.AddComponent<Image>();

            return this;
        }

        /// <summary>
        /// Creates the content canvas group to hold the majority of items in the menu.
        /// </summary>
        /// <param name="style">The rect describing the size and position of the content pane.</param>
        /// <returns></returns>
        public MenuBuilder CreateContentPane(RectTransformData style)
        {
            var content = new GameObject("Content");
            GameObject.DontDestroyOnLoad(content);
            content.transform.SetParent(MenuObject.transform, false);
            // RectTransform
            style.Apply(content.AddComponent<RectTransform>());
            // CanvasGroup
            Screen.content = content.AddComponent<CanvasGroup>();
            // Canvas Renderer
            content.AddComponent<CanvasRenderer>();
            // ZeroAlphaOnStart
            content.AddComponent<ZeroAlphaOnStart>();

            return this;
        }

        /// <summary>
        /// Creates the control canvas group to hold the buttons at the bottom of the menu.
        /// </summary>
        /// <param name="style">The rect describing the size and position of the control pane.</param>
        /// <returns></returns>
        public MenuBuilder CreateControlPane(RectTransformData style)
        {
            var control = new GameObject("Control");
            GameObject.DontDestroyOnLoad(control);
            control.transform.SetParent(MenuObject.transform, false);
            // RectTransform
            style.Apply(control.AddComponent<RectTransform>());
            // CanvasGroup
            Screen.controls = control.AddComponent<CanvasGroup>();
            // Canvas Renderer
            control.AddComponent<CanvasRenderer>();
            // ZeroAlphaOnStart
            control.AddComponent<ZeroAlphaOnStart>();

            return this;
        }
    }

    /// <summary>
    /// A class used for adding menu items to a canvas in a specific layout.
    /// </summary>
    public class ContentArea
    {
        /// <summary>
        /// Event that gets called when a suitable <c>MenuSelectable</c> is added to the canvas.
        /// </summary>
        public event Action<MenuSelectable> OnMenuItemAdd;

        /// <summary>
        /// The game object to place the new content in.
        /// </summary>
        public GameObject contentObject { get; protected set; }

        /// <summary>
        /// The layout to apply to the content being added.
        /// </summary>
        public IContentLayout layout { get; set; }

        /// <summary>
        /// Creates a new <c>ContentArea</c>.
        /// </summary>
        /// <param name="obj">The object to place the added content in.</param>
        /// <param name="layout">The layout to applly to the content being added.</param>
        public ContentArea(GameObject obj, IContentLayout layout)
        {
            contentObject = obj;
            this.layout = layout;
        }

        /// <summary>
        /// Overwrite the events in this <c>ContentArea</c> with the events in another one.
        /// </summary>
        /// <param name="src">The source of the events to copy.</param>
        /// <returns></returns>
        public ContentArea CopyEvents(ContentArea src)
        {
            OnMenuItemAdd = src.OnMenuItemAdd;
            return this;
        }

        /// <summary>
        /// Registers a <c>MenuSelectable</c> to be added. Calls the <c>OnMenuItemAdd</c> event.
        /// </summary>
        /// <param name="sel">The menu item to add.</param>
        public void RegisterMenuItem(MenuSelectable sel) => OnMenuItemAdd?.Invoke(sel);
    }

    namespace Config
    {
        /// <summary>
        /// The styling options for the menu title.
        /// </summary>
        public struct MenuTitleStyle
        {
            /// <summary>
            /// The style preset of a standard menu title in the vanilla game.
            /// </summary>
            public static readonly MenuTitleStyle vanillaStyle = new()
			{
                pos = new AnchoredPosition
                {
                    ChildAnchor = new Vector2(0.5f, 0.5f),
                    ParentAnchor = new Vector2(0.5f, 0.5f),
                    Offset = new Vector2(0f, 544f)
                },
                textSize = 75
            };

            /// <summary>
            /// The position of the title.
            /// </summary>
            public AnchoredPosition pos;
            /// <summary>
            /// The text size of the title.
            /// </summary>
            public int textSize;
        }
    }
}