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
    /// A layout to place items in a grid pattern
    /// </summary>
    public class RegularGridLayout : ContentLayout
    {
        /// <summary>
        /// The "size" of a cell in the grid
        /// </summary>
        public Vector2 itemAdvance { get; init; }
        /// <summary>
        /// The starting position of the first cell
        /// </summary>
        public RectPosition start { get; init; }
        /// <summary>
        /// The maximum number of columns to allow
        /// </summary>
        public int columns { get; init; }

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
            itemAdvance = new Vector2(0, -itemHeight),
            start = new RectPosition
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
            var transform = start;
            transform.offset += itemAdvance * this.indexPos;
            transform.Reposition(rt);
            this.index += 1;
        }
    }

    /// <summary>
    /// A layout based on an enumerator to get successive <c>RectPosition</c>s
    /// </summary>
    public class EnumeratorLayout : ContentLayout
    {
        private IEnumerator<RectPosition> generator;

        /// <summary>
        /// Creates a layout from an <c>IEnumerable</c>
        /// </summary>
        /// <param name="src">The emumerable object</param>
        public EnumeratorLayout(IEnumerable<RectPosition> src)
        {
            this.generator = src.GetEnumerator();
        }

        /// <summary>
        /// Creates a layout from an <c>IEnumerator</c>
        /// </summary>
        /// <param name="generator">The enumerator</param>
        public EnumeratorLayout(IEnumerator<RectPosition> generator)
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
        public RectPosition pos { get; set; }

        /// <summary>
        /// Creates a layout with the position anchoring the same spot on the child and parent together
        /// </summary>
        public SingleContentLayout(Vector2 anchor) : this(new RectPosition(anchor, anchor)) { }

        /// <summary>
        /// Creates a layout from a <c>RectPosition</c>
        /// </summary>
        /// <param name="pos">The position</param>
        public SingleContentLayout(RectPosition pos)
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