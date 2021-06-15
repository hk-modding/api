using System;
using UnityEngine;
using Modding.Menu.Config;

namespace Modding.Menu
{
    /// <summary>
    /// A helper class for creating single object wrappers.
    /// </summary>
    public static class WrapperContent
    {
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
}