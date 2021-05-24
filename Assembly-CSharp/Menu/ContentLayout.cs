using System.Collections.Generic;
using UnityEngine;

namespace Modding.Menu
{
    /// <summary>
    /// An interface to place successive <c>RectTransform</c>s
    /// </summary>
    public interface ContentLayout
    {
        /// <summary>
        /// Modifies the passed in <c>RectTransform</c>.
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c></param>
        public void ModifyNext(RectTransform rt);
    }

    /// <summary>
    /// A layout that does absolutely nothing
    /// </summary>
    public struct NullContentLayout : ContentLayout
    {
        /// <inheritdoc/>
        public void ModifyNext(RectTransform rt) { }
    }

    /// <summary>
    /// A layout to place items in a grid pattern
    /// </summary>
    public class RegularGridLayout : ContentLayout
    {
        /// <summary>
        /// The "size" of a cell in the grid
        /// </summary>
        public RelVector2 itemAdvance { get; set; }
        /// <summary>
        /// The starting position of the first cell
        /// </summary>
        public AnchoredPosition start { get; set; }
        /// <summary>
        /// The maximum number of columns to allow
        /// </summary>
        public int columns { get; set; }

        /// <summary>
        /// The "index" of the next item to be placed
        /// </summary>
        public int index { get; set; } = 0;

        /// <summary>
        /// The position in grid cells of the next item
        /// </summary>
        public Vector2Int indexPos { get => new Vector2Int(index % columns, index / columns); }

        /// <summary>
        /// Creates a single column, top down vertical layout.
        /// </summary>
        /// <param name="itemHeight">The height of each item</param>
        /// <param name="start">The starting position</param>
        /// <returns></returns>
        public static RegularGridLayout CreateVerticalLayout(
            float itemHeight,
            Vector2 start = new Vector2()
        ) => new RegularGridLayout
        {
            itemAdvance = new RelVector2(new Vector2(0, -itemHeight)),
            start = new AnchoredPosition
            {
                childAnchor = new Vector2(0.5f, 1f),
                parentAnchor = new Vector2(0.5f, 1f),
                offset = start
            },
            columns = 1
        };

        /// <summary>
        /// Modifies the passed in <c>RectTransform</c> to be placed in the next spot in the grid
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c></param>
        public void ModifyNext(RectTransform rt)
        {
            (start + itemAdvance * this.indexPos).Reposition(rt);
            this.index += 1;
        }

        /// <summary>
        /// Changes the column width of the layout, continuing where the layout left off.<br/>
        /// This method should generally only be called at the end of a row,
        /// because otherwise it may cause an overlap of the currently placed menu items
        /// and the new menu items in the same row.<br/>
        /// Internally this method resets the actual index count so a copy of index should be saved before calling
        /// this method if needed.
        /// </summary>
        /// <param name="columns">The new number of columns.</param>
        /// <param name="originalAnchor">The normalized anchor on the original "width" to place the new grid.</param>
        /// <param name="newSize">The new size of the grid element, or null to not change</param>
        /// <param name="newAnchor">The normalized anchor on the new "width" to place the anchor.</param>
        public void ChangeColumns(
            int columns,
            float originalAnchor = 0.5f,
            RelVector2? newSize = null,
            float newAnchor = 0.5f
        )
        {
            var size = newSize ?? this.itemAdvance;
            var height = this.itemAdvance.y * this.indexPos.y;
            var widthAdjust = this.itemAdvance.x * this.columns * originalAnchor - size.x * columns * newAnchor;
            // figure out the width from the first item to the (newAnchor) of the new grid
            var adjust = this.start.childAnchor.x * size.x + widthAdjust;
            this.index = 0;
            this.columns = columns;
            this.start = this.start + new RelVector2(adjust, height);
            this.itemAdvance = size;
        }
    }

    /// <summary>
    /// A layout based on an enumerator to get successive <c>RectPosition</c>s
    /// </summary>
    public class EnumeratorLayout : ContentLayout
    {
        private IEnumerator<AnchoredPosition> generator;

        /// <summary>
        /// Creates a layout from an <c>IEnumerable</c>
        /// </summary>
        /// <param name="src">The emumerable object</param>
        public EnumeratorLayout(IEnumerable<AnchoredPosition> src)
        {
            this.generator = src.GetEnumerator();
        }

        /// <summary>
        /// Creates a layout from an <c>IEnumerator</c>
        /// </summary>
        /// <param name="generator">The enumerator</param>
        public EnumeratorLayout(IEnumerator<AnchoredPosition> generator)
        {
            this.generator = generator;
        }

        /// <summary>
        /// Modifies the passed in <c>RectTransform</c> based on the next item of the enumerator.
        /// </summary>
        /// <param name="rt">The passed in <c>RectTransform</c></param>
        public void ModifyNext(RectTransform rt)
        {
            generator.Current.Reposition(rt);
            generator.MoveNext();
        }
    }

    /// <summary>
    /// A layout that places every object in the same position
    /// </summary>
    public class SingleContentLayout : ContentLayout
    {
        /// <summary>
        /// The position to place the object in
        /// </summary>
        public AnchoredPosition pos { get; set; }

        /// <summary>
        /// Creates a layout with the position anchoring the same spot on the child and parent together
        /// </summary>
        public SingleContentLayout(Vector2 anchor) : this(new AnchoredPosition(anchor, anchor)) { }

        /// <summary>
        /// Creates a layout from a <c>RectPosition</c>
        /// </summary>
        /// <param name="pos">The position</param>
        public SingleContentLayout(AnchoredPosition pos)
        {
            this.pos = pos;
        }

        /// <summary>
        /// Places the passed in <c>RectTransform</c> based on <c>pos</c>
        /// </summary>
        public void ModifyNext(RectTransform rt)
        {
            pos.Reposition(rt);
        }
    }
}