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
        /// An event that gets called at the start of <c>Build</c>.
        /// </summary>
        public event Action<MenuBuilder> OnBuild;

        /// <summary>
        /// The current default navigation graph that gets used for <c>AddContent</c> and <c>AddControls</c> calls.
        /// </summary>
        public INavigationGraph DefaultNavGraph { get; private set; } = new NullNavigationGraph();

        /// <summary>
        /// Creates a new <c>MenuBuilder</c> on the UIManager instance canvas.
        /// </summary>
        /// <param name="name">The name of the root menu.</param>
        public MenuBuilder(string name) : this(UIManager.instance.UICanvas.gameObject, name) { }

        /// <summary>
        /// Creates a new <c>MenuBuilder</c> on a canvas.
        /// </summary>
        /// <param name="canvas">The canvas to make the root menu on.</param>
        /// <param name="name">The name of the root menu.</param>
        public MenuBuilder(GameObject canvas, string name)
        {
            this.MenuObject = new GameObject(name);
            GameObject.DontDestroyOnLoad(this.MenuObject);
            this.MenuObject.transform.SetParent(canvas.transform, false);
            this.MenuObject.SetActive(false);
            // MenuScreen
            this.Screen = this.MenuObject.AddComponent<MenuScreen>();
            // CanvasRenderer
            this.MenuObject.AddComponent<CanvasRenderer>();
            // RectTransform
            var rt = this.MenuObject.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 463f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(0f, 0f);
            rt.localScale = new Vector3(0.7f, 0.7f, 1f);
            // CanvasGroup
            this.MenuObject.AddComponent<CanvasGroup>();
        }

        /// <summary>
        /// Builds the menu, calling any <c>OnBuild</c> events and returning the screen.
        /// </summary>
        /// <returns></returns>
        public MenuScreen Build()
        {
            this.OnBuild?.Invoke(this);
            if (this.DefaultNavGraph?.BuildNavigation() is Selectable sel)
            {
                this.MenuObject.AddComponent<Components.AutoSelector>().Start = sel;
            }
            return this.Screen;
        }

        /// <summary>
        /// Adds "content" to the menu in a certain layout. <br/>
        /// If <c>CreateContentPane</c> has not been called yet, this method will immeddiately return.
        /// </summary>
        /// <param name="layout">The layout of the added content</param>
        /// <param name="navgraph">The navigation graph to place the selectables in.</param>
        /// <param name="action">The action that will get called to add the content</param>
        /// <returns></returns>
        public MenuBuilder AddContent(IContentLayout layout, INavigationGraph navgraph, Action<ContentArea> action)
        {
            if (this.Screen.content == null)
            {
                return this;
            }
            action(new ContentArea(this.Screen.content.gameObject, layout, navgraph));
            return this;
        }

        /// <summary>
        /// Adds "content" to the menu in a certain layout with the default navigation graph.<br/>
        /// If <c>CreateContentPane</c> has not been called yet, this method will immeddiately return.
        /// </summary>
        /// <param name="layout">The layout of the added content</param>
        /// <param name="action">The action that will get called to add the content</param>
        /// <returns></returns>
        public MenuBuilder AddContent(
            IContentLayout layout,
            Action<ContentArea> action
        ) => this.AddContent(layout, this.DefaultNavGraph, action);

        /// <summary>
        /// Adds "content" to the control pane in a certain layout.<br/>
        /// If <c>CreateControlPane</c> has not been called yet, this method will immeddiately return.
        /// </summary>
        /// <param name="layout">The layout to apply to the added content.</param>
        /// <param name="navgraph">The navigation graph to place the selectables in.</param>
        /// <param name="action">The action that will get called to add the content.</param>
        /// <returns></returns>
        public MenuBuilder AddControls(IContentLayout layout, INavigationGraph navgraph, Action<ContentArea> action)
        {
            if (this.Screen.controls == null)
            {
                return this;
            }
            action(new ContentArea(this.Screen.controls.gameObject, layout, navgraph));
            return this;
        }

        /// <summary>
        /// Adds "content" to the control pane in a certain layout with the default navigation graph.<br/>
        /// If <c>CreateControlPane</c> has not been called yet, this method will immeddiately return.
        /// </summary>
        /// <param name="layout">The layout to apply to the added content.</param>
        /// <param name="action">The action that will get called to add the content.</param>
        /// <returns></returns>
        public MenuBuilder AddControls(
            IContentLayout layout,
            Action<ContentArea> action
        ) => this.AddControls(layout, this.DefaultNavGraph, action);

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
            titleObj.transform.SetParent(this.MenuObject.transform, false);
            // CanvasRenderer
            titleObj.AddComponent<CanvasRenderer>();
            // RectTransform
            var titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.sizeDelta = new Vector2(0f, 107f);
            titleRt.anchorMin = new Vector2(0f, 0.5f);
            titleRt.anchorMax = new Vector2(1f, 0.5f);
            style.Pos.Reposition(titleRt);
            // CanvasGroup
            this.Screen.title = titleObj.AddComponent<CanvasGroup>();
            // ZeroAlphaOnStart
            titleObj.AddComponent<ZeroAlphaOnStart>();
            // Text
            var titleText = titleObj.AddComponent<Text>();
            titleText.font = MenuResources.TrajanBold;
            titleText.fontSize = style.TextSize;
            titleText.resizeTextMaxSize = style.TextSize;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = title;
            titleText.supportRichText = true;
            titleText.verticalOverflow = VerticalWrapMode.Overflow;
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;

            // TopFleur
            var fleur = new GameObject("TopFleur");
            GameObject.DontDestroyOnLoad(fleur);
            fleur.transform.SetParent(this.MenuObject.transform, false);
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
            this.Screen.topFleur = fleurAnimator;
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
            content.transform.SetParent(this.MenuObject.transform, false);
            // RectTransform
            style.Apply(content.AddComponent<RectTransform>());
            // CanvasGroup
            this.Screen.content = content.AddComponent<CanvasGroup>();
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
            control.transform.SetParent(this.MenuObject.transform, false);
            // RectTransform
            style.Apply(control.AddComponent<RectTransform>());
            // CanvasGroup
            this.Screen.controls = control.AddComponent<CanvasGroup>();
            // Canvas Renderer
            control.AddComponent<CanvasRenderer>();
            // ZeroAlphaOnStart
            control.AddComponent<ZeroAlphaOnStart>();

            return this;
        }

        /// <summary>
        /// Sets the default navigation graph to use for <c>AddContent</c> and <c>AddControls</c> calls.
        /// </summary>
        /// <param name="navGraph">The default navigation graph to set.</param>
        /// <returns></returns>
        public MenuBuilder SetDefaultNavGraph(INavigationGraph navGraph)
        {
            this.DefaultNavGraph = navGraph ?? new NullNavigationGraph();
            return this;
        }
    }

    /// <summary>
    /// A class used for adding menu items to a canvas in a specific layout.
    /// </summary>
    public class ContentArea
    {
        /// <summary>
        /// The game object to place the new content in.
        /// </summary>
        public GameObject ContentObject { get; protected set; }

        /// <summary>
        /// The layout to apply to the content being added.
        /// </summary>
        public IContentLayout Layout { get; set; }

        /// <summary>
        /// The navigation graph builder to place selectables in.
        /// </summary>
        public INavigationGraph NavGraph { get; set; }

        /// <summary>
        /// Creates a new <c>ContentArea</c>.
        /// </summary>
        /// <param name="obj">The object to place the added content in.</param>
        /// <param name="layout">The layout to apply to the content being added.</param>
        /// <param name="navGraph">The navigation graph to place the selectables in.</param>
        public ContentArea(GameObject obj, IContentLayout layout, INavigationGraph navGraph)
        {
            this.ContentObject = obj;
            this.Layout = layout;
            this.NavGraph = navGraph;
        }

        /// <summary>
        /// Creates a new <c>ContentArea</c> with no navigation graph.
        /// </summary>
        /// <param name="obj">The object to place the added content in.</param>
        /// <param name="layout">The layout to apply to the content being added.</param>
        public ContentArea(GameObject obj, IContentLayout layout) : this(obj, layout, new NullNavigationGraph()) { }
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
            public static readonly MenuTitleStyle vanillaStyle = new MenuTitleStyle
            {
                Pos = new AnchoredPosition
                {
                    ChildAnchor = new Vector2(0.5f, 0.5f),
                    ParentAnchor = new Vector2(0.5f, 0.5f),
                    Offset = new Vector2(0f, 544f)
                },
                TextSize = 75
            };

            /// <summary>
            /// The position of the title.
            /// </summary>
            public AnchoredPosition Pos;
            /// <summary>
            /// The text size of the title.
            /// </summary>
            public int TextSize;
        }
    }
}