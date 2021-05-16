using System;
using UnityEngine;
using Modding.Menu.Config;

namespace Modding.Menu
{
    /// <summary>
    /// Helper class for creating single object wrappers 
    /// </summary>
    public static class WrapperContent
    {
        /// <summary>
        /// Creates a wrapper on the content area.
        /// </summary>
        /// <remarks>
        /// This wrapper will have no size so all parent relative sizes will break.
        /// </remarks>
        /// <param name="content">The <c>ContentArea</c> to put the wrapper in</param>
        /// <param name="name">The name of the wrapper object</param>
        /// <param name="action">The action that will get called to add the inner object</param>
        /// <returns></returns>
        public static ContentArea AddWrappedItem(
            this ContentArea content,
            string name,
            Action<ContentArea> action
        ) => content.AddWrappedItem(name, action, out _);

        /// <summary>
        /// Creates a wrapper on the content area.
        /// </summary>
        /// <remarks>
        /// This wrapper will have no size so all parent relative sizes will break.
        /// </remarks>
        /// <param name="content">The <c>ContentArea</c> to put the wrapper in</param>
        /// <param name="name">The name of the wrapper object</param>
        /// <param name="action">The action that will get called to add the inner object</param>
        /// <param name="obj">The newly created wrapper object</param>
        /// <returns></returns>
        public static ContentArea AddWrappedItem(
            this ContentArea content,
            string name,
            Action<ContentArea> action,
            out GameObject obj
        )
        {
            // Wrapper
            var wrapper = new GameObject(name);
            GameObject.DontDestroyOnLoad(wrapper);
            wrapper.transform.SetParent(content.contentObject.transform, false);
            // RectTransform
            var wrapperRt = wrapper.AddComponent<RectTransform>();
            wrapperRt.sizeDelta = new Vector2(0f, 0f);
            wrapperRt.pivot = new Vector2(0.5f, 0.5f);
            wrapperRt.anchorMin = new Vector2(0.5f, 0.5f);
            wrapperRt.anchorMax = new Vector2(0.5f, 0.5f);
            content.layout.ModifyNext(wrapperRt);
            // CanvasRenderer
            wrapper.AddComponent<CanvasRenderer>();

            action(new ContentArea(wrapper, new SingleContentLayout(new Vector2(0.5f, 0.5f))).CopyEvents(content));

            obj = wrapper;
            return content;
        }
    }
}