using System.Collections.Generic;
using UnityEngine;

namespace Modding.Menu
{
    /// <summary>
    /// An interface to place successive <c>RectTransform</c>s.
    /// </summary>
    public interface IContentLayout
    {
        /// <summary>
        /// Modifies a <c>RectTransform</c>.
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c> to modify.</param>
        public void ModifyNext(RectTransform rt);
    }

    /// <summary>
    /// A layout that does absolutely nothing.
    /// </summary>
    public struct NullContentLayout : IContentLayout
    {
        /// <inheritdoc/>
        public void ModifyNext(RectTransform rt) { }
    }

    /// <summary>
    /// A layout to place items in a grid pattern.
    /// </summary>
    public class RegularGridLayout : IContentLayout
    {
        /// <summary>
        /// The "size" of a cell in the grid.
        /// </summary>
        public RelVector2 ItemAdvance { get; set; }
        /// <summary>
        /// The starting position of the first cell.
        /// </summary>
        public AnchoredPosition Start { get; set; }
        /// <summary>
        /// The maximum number of columns to allow.
        /// </summary>
        public int Columns { get; set; }

        /// <summary>
        /// The "index" of the next item to be placed.
        /// </summary>
        public int Index { get; set; } = 0;

        /// <summary>
        /// The position in grid cells of the next item.
        /// </summary>
        public Vector2Int IndexPos { get => new Vector2Int(Index % Columns, Index / Columns); }

        /// <summary>
        /// Creates a new regular grid layout.
        /// </summary>
        /// <param name="start">The starting position of the first item in the grid.</param>
        /// <param name="itemAdvance">The "size" of a cell in the grid.</param>
        /// <param name="columns">The maximum number of columns to allow.</param>
        public RegularGridLayout(AnchoredPosition start, RelVector2 itemAdvance, int columns)
        {
            this.Start = start;
            this.ItemAdvance = itemAdvance;
            this.Columns = columns;
        }

        /// <summary>
        /// Creates a single column, top down vertical layout.
        /// </summary>
        /// <param name="itemHeight">The height of each item.</param>
        /// <param name="start">The starting position.</param>
        /// <returns></returns>
        public static RegularGridLayout CreateVerticalLayout(
            float itemHeight,
            Vector2 start = new Vector2()
        ) => new RegularGridLayout(
            new AnchoredPosition
            {
                ChildAnchor = new Vector2(0.5f, 1f),
                ParentAnchor = new Vector2(0.5f, 1f),
                Offset = start
            },
            new RelVector2(new Vector2(0, -itemHeight)),
            1
        );

        /// <summary>
        /// Modifies a <c>RectTransform</c> to place it in the next spot in the grid.
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c> to modify.</param>
        public void ModifyNext(RectTransform rt)
        {
            (this.Start + this.ItemAdvance * this.IndexPos).Reposition(rt);
            this.Index += 1;
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
        /// <param name="newSize">The new size of the grid element, or null to not change.</param>
        /// <param name="newAnchor">The normalized anchor on the new "width" to place the anchor.</param>
        public void ChangeColumns(
            int columns,
            float originalAnchor = 0.5f,
            RelVector2? newSize = null,
            float newAnchor = 0.5f
        )
        {
            var size = newSize ?? this.ItemAdvance;
            var height = this.ItemAdvance.y * this.IndexPos.y;
            var widthAdjust = this.ItemAdvance.x * this.Columns * originalAnchor - size.x * columns * newAnchor;
            // figure out the width from the first item to the (newAnchor) of the new grid
            var adjust = this.Start.ChildAnchor.x * size.x + widthAdjust;
            this.Index = 0;
            this.Columns = columns;
            this.Start += new RelVector2(adjust, height);
            this.ItemAdvance = size;
        }
    }

    /// <summary>
    /// A layout based on an enumerator to get successive <c>RectPosition</c>s.
    /// </summary>
    public class EnumeratorLayout : IContentLayout
    {
        private IEnumerator<AnchoredPosition> generator;

        /// <summary>
        /// Creates a layout from an <c>IEnumerable</c>.
        /// </summary>
        /// <param name="src">The emumerable object.</param>
        public EnumeratorLayout(IEnumerable<AnchoredPosition> src)
        {
            this.generator = src.GetEnumerator();
        }

        /// <summary>
        /// Creates a layout from an <c>IEnumerator</c>.
        /// </summary>
        /// <param name="generator">The enumerator.</param>
        public EnumeratorLayout(IEnumerator<AnchoredPosition> generator)
        {
            this.generator = generator;
        }

        /// <summary>
        /// Modifies a <c>RectTransform</c> to place it based on the next item of the enumerator.
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c> to modify.</param>
        public void ModifyNext(RectTransform rt)
        {
            if (generator.MoveNext()) generator.Current.Reposition(rt);
        }
    }

    /// <summary>
    /// A layout that places every object in the same position.
    /// </summary>
    public class SingleContentLayout : IContentLayout
    {
        /// <summary>
        /// The position to place the object in.
        /// </summary>
        public AnchoredPosition Position { get; set; }

        /// <summary>
        /// Creates a layout with the position anchoring the same spot on the child and parent together.
        /// </summary>
        /// <param name="anchor">The point to anchor the child to the parent.</param>
        public SingleContentLayout(Vector2 anchor) : this(new AnchoredPosition(anchor, anchor)) { }

        /// <summary>
        /// Creates a layout from a <c>RectPosition</c>.
        /// </summary>
        /// <param name="pos">The position to place the objects in.</param>
        public SingleContentLayout(AnchoredPosition pos)
        {
            this.Position = pos;
        }

        /// <summary>
        /// Modifies a <c>RectTransform</c> to place it in the specified location.
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c> to modify.</param>
        public void ModifyNext(RectTransform rt)
        {
            this.Position.Reposition(rt);
        }
    }
}