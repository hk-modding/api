using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modding.Menu.Components
{
    /// <summary>
    /// A component that scrolls a pane on select
    /// </summary>
    public class ScrollPaneSelector : MonoBehaviour, ISelectHandler
    {
        /// <summary>
        /// The pane that gets moved by the scrollbar.
        /// </summary>
        public RectTransform PaneRect { get; set; }
        /// <summary>
        /// The mask that is the visual size for the pane.
        /// </summary>
        public RectTransform MaskRect { get; set; }
        /// <summary>
        /// The scrollbar.
        /// </summary>
        public Scrollbar Scrollbar { get; set; }
        /// <summary>
        /// A function to get padding for the selection scrolling. The returned tuple is `(bottom, top)`.
        /// </summary>
        public Func<RectTransform, (float, float)> SelectionPadding { get; set; }

        /// <summary>
        /// Move the scrollbar to show the selected item.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public void OnSelect(BaseEventData eventData)
        {
            // this code is so bad wtf
            if (eventData is not AxisEventData) return;
            var rt = this.gameObject.GetComponent<RectTransform>();

            var (bottomPad, topPad) = SelectionPadding(rt);
            var thisToMask = this.MaskRect.worldToLocalMatrix * rt.localToWorldMatrix;
            var bottom = thisToMask.MultiplyPoint(new Vector3(0, rt.rect.yMin, 0)).y + bottomPad;
            var top = thisToMask.MultiplyPoint(new Vector3(0, rt.rect.yMax, 0)).y + topPad;
            
            if (bottom < this.MaskRect.rect.yMin)
            {
                var pos = this.PaneRect.anchoredPosition.y + this.MaskRect.rect.yMin - bottom;
                this.Scrollbar.value = pos / (this.PaneRect.rect.height - this.MaskRect.rect.height);
            }
            else if (top > this.MaskRect.rect.yMax)
            {
                var pos = this.PaneRect.anchoredPosition.y - (top - this.MaskRect.rect.yMax);
                this.Scrollbar.value = pos / (this.PaneRect.rect.height - this.MaskRect.rect.height);
            }
        }
    }
}