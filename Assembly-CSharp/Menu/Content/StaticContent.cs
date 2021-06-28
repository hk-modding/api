using System;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Modding.Menu
{
    /// <summary>
    /// A helper class for creating static content.
    /// </summary>
    public static class StaticContent
    {
        /// <summary>
        /// Creates a sized static panel with no other properties.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the panel in.</param>
        /// <param name="name">The name of the panel game object.</param>
        /// <param name="size">The size of the panel.</param>
        /// <param name="obj">The newly created panel.</param>
        /// <returns></returns>
        public static ContentArea AddStaticPanel(
            this ContentArea content,
            string name,
            RelVector2 size,
            out GameObject obj
        )
        {
            var go = new GameObject(name);
            GameObject.DontDestroyOnLoad(go);
            go.transform.SetParent(content.ContentObject.transform, false);
            // RectTransform
            var rt = go.AddComponent<RectTransform>();
            size.GetBaseTransformData().Apply(rt);
            content.Layout.ModifyNext(rt);
            // CanvasRenderer
            go.AddComponent<CanvasRenderer>();

            obj = go;
            return content;
        }

        /// <summary>
        /// Creates a text panel.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the text panel in.</param>
        /// <param name="name">The name of the text panel game object.</param>
        /// <param name="size">The size of the text panel.</param>
        /// <param name="config">The configuration options for the text panel.</param>
        /// <returns></returns>
        public static ContentArea AddTextPanel(
            this ContentArea content,
            string name,
            RelVector2 size,
            TextPanelConfig config
        ) => content.AddTextPanel(name, size, config, out _);

        /// <summary>
        /// Creates a text panel.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the text panel in.</param>
        /// <param name="name">The name of the text panel game object.</param>
        /// <param name="size">The size of the text panel.</param>
        /// <param name="config">The configuration options for the text panel.</param>
        /// <param name="text">The <c>Text</c> component on the created text panel.</param>
        /// <returns></returns>
        public static ContentArea AddTextPanel(
            this ContentArea content,
            string name,
            RelVector2 size,
            TextPanelConfig config,
            out Text text
        )
        {
            content.AddStaticPanel(name, size, out var go);
            text = go.AddComponent<Text>();
            text.text = config.Text;
            text.fontSize = config.Size;
            text.font = config.Font switch
            {
                TextPanelConfig.TextFont.TrajanRegular => MenuResources.TrajanRegular,
                TextPanelConfig.TextFont.TrajanBold => MenuResources.TrajanBold,
                TextPanelConfig.TextFont.Perpetua => MenuResources.Perpetua,
                _ => MenuResources.TrajanRegular
            };
            text.supportRichText = true;
            text.alignment = config.Anchor;

            return content;
        }

        /// <summary>
        /// Creates an image panel.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the text panel in.</param>
        /// <param name="name">The name of the image panel game object.</param>
        /// <param name="size">The size of the image panel.</param>
        /// <param name="image">The image to render.</param>
        /// <returns></returns>
        public static ContentArea AddImagePanel(
            this ContentArea content,
            string name,
            RelVector2 size,
            Sprite image
        ) => content.AddImagePanel(name, size, image, out _);

        /// <summary>
        /// Creates an image panel.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the text panel in.</param>
        /// <param name="name">The name of the image panel game object.</param>
        /// <param name="size">The size of the image panel.</param>
        /// <param name="sprite">The image to render.</param>
        /// <param name="image">The <c>Image</c> component on the created image panel.</param>
        /// <returns></returns>
        public static ContentArea AddImagePanel(
            this ContentArea content,
            string name,
            RelVector2 size,
            Sprite sprite,
            out Image image
        )
        {
            content.AddStaticPanel(name, size, out var go);
            image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            return content;
        }

        /// <summary>
        /// Creates a single item wrapper.
        /// </summary>
        /// <remarks>
        /// This wrapper will have no size so all parent relative sizes will break.
        /// </remarks>
        /// <param name="content">The <c>ContentArea</c> to put the wrapper in.</param>
        /// <param name="name">The name of the wrapper game object.</param>
        /// <param name="action">The action that will get called to add the inner object.</param>
        /// <returns></returns>
        public static ContentArea AddWrappedItem(
            this ContentArea content,
            string name,
            Action<ContentArea> action
        ) => content.AddWrappedItem(name, action, out _);

        /// <summary>
        /// Creates a single item wrapper.
        /// </summary>
        /// <remarks>
        /// This wrapper will have no size so all parent relative sizes will break.
        /// </remarks>
        /// <param name="content">The <c>ContentArea</c> to put the wrapper in.</param>
        /// <param name="name">The name of the wrapper game object.</param>
        /// <param name="action">The action that will get called to add the inner object.</param>
        /// <param name="wrapper">The newly created wrapper.</param>
        /// <returns></returns>
        public static ContentArea AddWrappedItem(
            this ContentArea content,
            string name,
            Action<ContentArea> action,
            out GameObject wrapper
        )
        {
            // Wrapper
            wrapper = new GameObject(name);
            GameObject.DontDestroyOnLoad(wrapper);
            wrapper.transform.SetParent(content.ContentObject.transform, false);
            // RectTransform
            var wrapperRt = wrapper.AddComponent<RectTransform>();
            wrapperRt.sizeDelta = new Vector2(0f, 0f);
            wrapperRt.pivot = new Vector2(0.5f, 0.5f);
            wrapperRt.anchorMin = new Vector2(0.5f, 0.5f);
            wrapperRt.anchorMax = new Vector2(0.5f, 0.5f);
            content.Layout.ModifyNext(wrapperRt);
            // CanvasRenderer
            wrapper.AddComponent<CanvasRenderer>();

            action(new ContentArea(wrapper, new SingleContentLayout(new Vector2(0.5f, 0.5f)), content.NavGraph));

            return content;
        }
    }

    namespace Config
    {
        /// <summary>
        /// Configuration options for creating a text panel.
        /// </summary>
        public struct TextPanelConfig
        {
            /// <summary>
            /// The text to render.
            /// </summary>
            public string Text;
            /// <summary>
            /// The font size of the text.
            /// </summary>
            public int Size;
            /// <summary>
            /// The font to render.
            /// </summary>
            public TextFont Font;
            /// <summary>
            /// The position where the text should be anchored to.
            /// </summary>
            public TextAnchor Anchor;

            /// <summary>
            /// The three main fonts that Hollow Knight uses in the menus.
            /// </summary>
            public enum TextFont
            {
                /// <summary>
                /// The Trajan regular font.
                /// </summary>
                TrajanRegular,
                /// <summary>
                /// The Trajan bold font.
                /// </summary>
                TrajanBold,
                /// <summary>
                /// The perpetua font.
                /// </summary>
                Perpetua
            }
        }
    }
}