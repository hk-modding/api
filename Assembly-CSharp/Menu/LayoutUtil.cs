using UnityEngine;

namespace Modding.Menu
{
    // unironically one of the most useful pieces of code I have ever written
    /// <summary>
    /// A struct to define anchored positioning relative to a parent
    /// </summary>
    public struct RectPosition
    {
        /// <summary>
        /// The normalized anchoring point on this rectangle that will get anchored to the parent.<br/>
        /// </summary>
        /// <remarks>
        /// The lower left corner is (0, 0) and the upper right corner is (1, 1)
        /// </remarks>
        public Vector2 childAnchor;

        /// <summary>
        /// The normalized anchoring point on the parent rectangle that will get anchored to the child.<br/>
        /// </summary>
        /// <remarks>
        /// The lower left corner is (0, 0) and the upper right corner is (1, 1)
        /// </remarks>
        public Vector2 parentAnchor;

        /// <summary>
        /// The offset in pixels of the <c>childAnchor</c> from the <c>parentAnchor</c>
        /// </summary>
        public Vector2 offset;

        /// <summary>
        /// Creates a new <c>RectPosition</c>
        /// </summary>
        /// <param name="childAnchor">The child anchor</param>
        /// <param name="parentAnchor">The parent anchor</param>
        /// <param name="offset">The offset from the parent anchor to the child anchor</param>
        public RectPosition(Vector2 childAnchor, Vector2 parentAnchor, Vector2 offset = new Vector2())
        {
            this.childAnchor = childAnchor;
            this.parentAnchor = parentAnchor;
            this.offset = offset;
        }

        /// <summary>
        /// Translate a <c>RectTransform</c> based on the fields in this struct
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c> to modify</param>
        public void Reposition(RectTransform rt) => this.GetRepositioned(rt).Apply(rt);

        /// <summary>
        /// Get a translated <c>RectTransformData</c> based on the fields in this struct
        /// </summary>
        /// <param name="rt">The <c>RectTransformData</c> to translate</param>
        /// <returns></returns>
        public RectTransformData GetRepositioned(RectTransformData rt)
        {
            var del = this.parentAnchor - ParentPointFromChild(rt, childAnchor);
            rt.pivot = this.childAnchor;
            rt.anchorMin += del;
            rt.anchorMax += del;
            rt.anchoredPosition = offset;
            return rt;
        }

        /// <summary>
        /// Creates a <c>RectPosition</c> from an anchor on a sibling rect
        /// </summary>
        /// <param name="selfAnchor">The anchor on this rect</param>
        /// <param name="sibling">The sibling rect</param>
        /// <param name="siblingAnchor">The anchor on the sibling</param>
        /// <param name="offset">The offset in pixels of the <c>selfAnchor</c> from the <c>siblingAnchor</c></param>
        /// <returns></returns>
        public static RectPosition FromSiblingAnchor(
            Vector2 selfAnchor,
            RectTransformData sibling,
            Vector2 siblingAnchor,
            Vector2 offset
        ) => new RectPosition
        {
            childAnchor = selfAnchor,
            parentAnchor = ParentPointFromChild(sibling, siblingAnchor),
            offset = sibling.anchoredPosition + offset
        };

        /// <summary>
        /// Gets a normalized point on the parent from a normalized point on the child.
        /// </summary>
        /// <param name="child">The child rect</param>
        /// <param name="childPoint">A normalized point on <c>child</c></param>
        /// <returns></returns>
        public static Vector2 ParentPointFromChild(
            RectTransformData child,
            Vector2 childPoint
        )
        {
            return child.anchorMin + (child.anchorMax - child.anchorMin) * childPoint;
        }
    }

    /// <summary>
    /// A struct to define size relative to a parent
    /// </summary>
    public struct RectSize
    {
        /// <summary>
        /// The size in pixels to increase the parent-relative size of the rect.
        /// </summary>
        public Vector2 sizeDelta;
        /// <summary>
        /// The normalized parent-relative size of the rect.
        /// </summary>
        public Vector2 parentRelSize;

        /// <summary>
        /// Creates a <c>RectSize</c> from two parent relative lengths.
        /// </summary>
        /// <param name="x">The length on the <c>x</c> axis</param>
        /// <param name="y">The length on the <c>y</c> axis</param>
        /// <returns></returns>
        public RectSize(ParentRelLength x, ParentRelLength y)
        {
            this.sizeDelta = new Vector2(x.lengthDelta, y.lengthDelta);
            this.parentRelSize = new Vector2(x.parentRelLength, y.parentRelLength);
        }

        /// <summary>
        /// Creates a <c>RectSize</c> from a size delta and a normalized parent-relative size
        /// </summary>
        /// <param name="sizeDelta">The size delta in pixels</param>
        /// <param name="parentRelSize">The normalized parent-relative size</param>
        public RectSize(Vector2 sizeDelta, Vector2 parentRelSize)
        {
            this.sizeDelta = sizeDelta;
            this.parentRelSize = parentRelSize;
        }

        /// <summary>
        /// Creates a <c>RectSize</c> from an absolute size in pixels
        /// </summary>
        /// <param name="size">The size in pixels</param>
        public RectSize(Vector2 size) : this(size, new Vector2()) { }


        /// <summary>
        /// Gets a <c>RectTransformData</c> with the correct sizing information.
        /// </summary>
        /// <returns></returns>
        public RectTransformData GetBaseTransformData() => new RectTransformData
        {
            sizeDelta = sizeDelta,
            anchorMin = new Vector2(),
            anchorMax = parentRelSize
        };

        /// <summary>
        /// Combines two parent relative lengths into a <c>RectSize</c>
        /// </summary>
        /// <param name="x">The length on the <c>x</c> axis</param>
        /// <param name="y">The length on the <c>y</c> axis</param>
        /// <returns></returns>
        public static RectSize FromParentRelLengths(ParentRelLength x, ParentRelLength y) => new RectSize
        {
            sizeDelta = new Vector2(x.lengthDelta, y.lengthDelta),
            parentRelSize = new Vector2(x.parentRelLength, y.parentRelLength)
        };
    }

    /// <summary>
    /// A struct to define a scalar length relative to a parent
    /// </summary>
    public struct ParentRelLength
    {
        /// <summary>
        /// The length in pixels to increase the parent-relative length
        /// </summary>
        public float lengthDelta;
        /// <summary>
        /// The normalized parent-relative length
        /// </summary>
        public float parentRelLength;

        /// <summary>
        /// Creates a new <c>ParentRelLength</c> from a length delta and a normalized parent-relative length
        /// </summary>
        /// <param name="lengthDelta">The length delta</param>
        /// <param name="parentRelLength">The normalized parent-relative length</param>
        public ParentRelLength(float lengthDelta, float parentRelLength)
        {
            this.lengthDelta = lengthDelta;
            this.parentRelLength = parentRelLength;
        }

        /// <summary>
        /// Creates a new absolute <c>ParentRelLength</c> from a length in pixels
        /// </summary>
        /// <param name="length">The length in pixels</param>
        public ParentRelLength(float length) : this(length, 0f) { }
    }

    /// <summary>
    /// A struct to represent the data in a <c>RectTransform</c>
    /// </summary>
    public struct RectTransformData
    {
        /// <summary>
        /// See <c>RectTransform.sizeDelta</c>
        /// </summary>
        public Vector2 sizeDelta;
        /// <summary>
        /// See <c>RectTransform.anchorMin</c>
        /// </summary>
        public Vector2 anchorMin;
        /// <summary>
        /// See <c>RectTransform.anchorMax</c>
        /// </summary>
        public Vector2 anchorMax;
        /// <summary>
        /// See <c>RectTransform.anchoredPosition</c>
        /// </summary>
        public Vector2 anchoredPosition;
        /// <summary>
        /// See <c>RectTransform.pivot</c>
        /// </summary>
        public Vector2 pivot;

        /// <summary>
        /// Get the data from an existing <c>RectTransform</c>.
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c></param>
        /// <returns></returns>
        public static RectTransformData FromRectTransform(RectTransform rt) => new RectTransformData
        {
            sizeDelta = rt.sizeDelta,
            anchorMin = rt.anchorMin,
            anchorMax = rt.anchorMax,
            anchoredPosition = rt.anchoredPosition,
            pivot = rt.pivot
        };

        /// <summary>
        /// Create a <c>RectTransformData</c> from a <c>RectSize</c> and <c>RectPosition</c>.
        /// </summary>
        /// <param name="size">The size parent-relative</param>
        /// <param name="pos">The anchored position</param>
        /// <returns></returns>
        public static RectTransformData FromSizeAndPos(
            RectSize size,
            RectPosition pos
        ) => pos.GetRepositioned(size.GetBaseTransformData());

        /// <summary>
        /// Apply the data to an existing <c>RectTransform</c>
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c></param>
        public void Apply(RectTransform rt)
        {
            rt.sizeDelta = this.sizeDelta;
            rt.anchorMin = this.anchorMin;
            rt.anchorMax = this.anchorMax;
            rt.anchoredPosition = this.anchoredPosition;
            rt.pivot = this.pivot;
        }

        // Is this a good idea? I think it probably is
        /// <summary>
        /// Convenience conversion to get the data from a <c>RectTransform</c>
        /// </summary>
        public static implicit operator RectTransformData(RectTransform r) => FromRectTransform(r);
    }
}