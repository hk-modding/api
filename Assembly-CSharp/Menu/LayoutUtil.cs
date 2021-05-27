using UnityEngine;

namespace Modding.Menu
{
    // unironically one of the most useful pieces of code I have ever written
    /// <summary>
    /// A struct to define anchored positioning relative to a parent.
    /// </summary>
    public struct AnchoredPosition
    {
        /// <summary>
        /// The normalized anchoring point on the parent rectangle that will get anchored to the child.
        /// </summary>
        /// <remarks>
        /// The lower left corner is (0, 0) and the upper right corner is (1, 1).
        /// </remarks>
        public Vector2 parentAnchor;

        /// <summary>
        /// The normalized anchoring point on this rectangle that will get anchored to the parent.
        /// </summary>
        /// <remarks>
        /// The lower left corner is (0, 0) and the upper right corner is (1, 1).
        /// </remarks>
        public Vector2 childAnchor;

        /// <summary>
        /// The offset in pixels of the <c>childAnchor</c> from the <c>parentAnchor</c>.
        /// </summary>
        public Vector2 offset;

        /// <summary>
        /// Creates a new <c>RectPosition</c>.
        /// </summary>
        /// <param name="parentAnchor">The normalized point on the parent to anchor the child on.</param>
        /// <param name="childAnchor">The normalized point on the child to anchor to the parent.</param>
        /// <param name="offset">The offset from the parent anchor to the child anchor.</param>
        public AnchoredPosition(Vector2 parentAnchor, Vector2 childAnchor, Vector2 offset = new Vector2())
        {
            this.parentAnchor = parentAnchor;
            this.childAnchor = childAnchor;
            this.offset = offset;
        }

        /// <summary>
        /// Translate a <c>RectTransform</c> based on the fields in this struct.
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c> to modify.</param>
        public void Reposition(RectTransform rt) => this.GetRepositioned(rt).Apply(rt);

        /// <summary>
        /// Get a translated <c>RectTransformData</c> based on the fields in this struct.
        /// </summary>
        /// <param name="rt">The <c>RectTransformData</c> to translate.</param>
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
        /// Creates a <c>RectPosition</c> from an anchor on a sibling rectangle.
        /// </summary>
        /// <param name="selfAnchor">The normalized point on a rect to anchor to the sibling.</param>
        /// <param name="sibling">The sibling rectangle to anchor to.</param>
        /// <param name="siblingAnchor">The normalized point on the sibling to anchor to.</param>
        /// <param name="offset">The offset in pixels of the <c>selfAnchor</c> to the sibling anchor point.</param>
        /// <returns></returns>
        public static AnchoredPosition FromSiblingAnchor(
            Vector2 selfAnchor,
            RectTransformData sibling,
            Vector2 siblingAnchor,
            Vector2 offset
        ) => new AnchoredPosition(
            ParentPointFromChild(sibling, siblingAnchor),
            selfAnchor,
            sibling.anchoredPosition + offset
        );

        /// <summary>
        /// Gets a normalized point on the parent from a normalized point on the child.
        /// </summary>
        /// <param name="child">The child rectangle.</param>
        /// <param name="childPoint">A normalized point on the child.</param>
        /// <returns></returns>
        public static Vector2 ParentPointFromChild(
            RectTransformData child,
            Vector2 childPoint
        ) => child.anchorMin + (child.anchorMax - child.anchorMin) * childPoint;

        /// <summary>
        /// Translates an anchored position by a relative vector.
        /// </summary>
        public static AnchoredPosition operator +(AnchoredPosition lhs, RelVector2 rhs) => new AnchoredPosition(
            lhs.parentAnchor + rhs.relative,
            lhs.childAnchor,
            lhs.offset + rhs.delta
        );
        /// <summary>
        /// Translates an anchored position by a relative vector.
        /// </summary>
        public static AnchoredPosition operator +(RelVector2 lhs, AnchoredPosition rhs) => rhs + lhs;
    }

    /// <summary>
    /// A struct to define size relative to a parent.
    /// </summary>
    public struct RelVector2
    {
        /// <summary>
        /// The size in pixels to increase the parent-relative size of the rect.
        /// </summary>
        public Vector2 delta;
        /// <summary>
        /// The normalized parent-relative size of the rect.
        /// </summary>
        public Vector2 relative;

        /// <summary>
        /// The x component of this vector.
        /// </summary>
        public RelLength x
        {
            get => new RelLength(delta.x, relative.x);
            set
            {
                delta.x = value.delta;
                relative.x = value.relative;
            }
        }

        /// <summary>
        /// The y component of this vector.
        /// </summary>
        public RelLength y
        {
            get => new RelLength(delta.y, relative.y);
            set
            {
                delta.y = value.delta;
                relative.y = value.relative;
            }
        }

        /// <summary>
        /// Creates a <c>RectSize</c> from two parent-relative lengths.
        /// </summary>
        /// <param name="x">The length on the <c>x</c> axis.</param>
        /// <param name="y">The length on the <c>y</c> axis.</param>
        /// <returns></returns>
        public RelVector2(RelLength x, RelLength y)
        {
            this.delta = new Vector2(x.delta, y.delta);
            this.relative = new Vector2(x.relative, y.relative);
        }

        /// <summary>
        /// Creates a <c>RectSize</c> from a size delta and a normalized parent-relative size.
        /// </summary>
        /// <param name="sizeDelta">The size delta in pixels.</param>
        /// <param name="parentRelSize">The normalized parent-relative size.</param>
        public RelVector2(Vector2 sizeDelta, Vector2 parentRelSize)
        {
            this.delta = sizeDelta;
            this.relative = parentRelSize;
        }

        /// <summary>
        /// Creates a <c>RectSize</c> from an absolute size in pixels.
        /// </summary>
        /// <param name="size">The size in pixels.</param>
        public RelVector2(Vector2 size) : this(size, new Vector2()) { }


        /// <summary>
        /// Gets a <c>RectTransformData</c> with the correct sizing information.
        /// </summary>
        /// <returns></returns>
        public RectTransformData GetBaseTransformData() => new RectTransformData
        {
            sizeDelta = delta,
            anchorMin = new Vector2(),
            anchorMax = relative
        };

        /// <summary>  
        /// Negates each element in a <c>RelVector2</c>.
        /// </summary>
        public static RelVector2 operator -(RelVector2 self) => new RelVector2(
            -self.delta,
            -self.relative
        );
        /// <summary>
        /// Adds two <c>RelVector2</c>s together.
        /// </summary>
        public static RelVector2 operator +(RelVector2 lhs, RelVector2 rhs) => new RelVector2(
            lhs.delta + rhs.delta,
            lhs.relative + rhs.relative
        );
        /// <summary>
        /// Subtracts one <c>RelVector2</c> from another.
        /// </summary>
        public static RelVector2 operator -(RelVector2 lhs, RelVector2 rhs) => new RelVector2(
            lhs.delta - rhs.delta,
            lhs.relative - rhs.relative
        );
        /// <summary>
        /// Scales both dimensions of a <c>RelVector2</c> up by a constant factor.
        /// </summary>
        public static RelVector2 operator *(RelVector2 lhs, float rhs) => new RelVector2(
            lhs.delta * rhs,
            lhs.relative * rhs
        );
        /// <summary>
        /// Scales both dimensions of a <c>RelVector2</c> up by a constant factor.
        /// </summary>
        public static RelVector2 operator *(float lhs, RelVector2 rhs) => rhs * lhs;
        /// <summary>
        /// Scales both dimensions of a <c>RelVector2</c> up by the respective factor in a <c>Vector2</c>.
        /// </summary>
        public static RelVector2 operator *(RelVector2 lhs, Vector2 rhs) => new RelVector2(
            lhs.delta * rhs,
            lhs.relative * rhs
        );
        /// <summary>
        /// Scales both dimensions of a <c>RelVector2</c> down by a constant factor.
        /// </summary>
        public static RelVector2 operator /(RelVector2 lhs, float rhs) => new RelVector2(
            lhs.delta / rhs,
            lhs.relative / rhs
        );
    }

    /// <summary>
    /// A struct to define a scalar length relative to a parent.
    /// </summary>
    public struct RelLength
    {
        /// <summary>
        /// The length in pixels to increase the parent-relative length.
        /// </summary>
        public float delta;
        /// <summary>
        /// The normalized parent-relative length.
        /// </summary>
        public float relative;

        /// <summary>
        /// Creates a new <c>ParentRelLength</c> from a length delta and a normalized parent-relative length.
        /// </summary>
        /// <param name="lengthDelta">The pixels to be added to the size from the scaled parent-relative length.</param>
        /// <param name="parentRelLength">The normalized parent-relative length.</param>
        public RelLength(float lengthDelta, float parentRelLength)
        {
            this.delta = lengthDelta;
            this.relative = parentRelLength;
        }

        /// <summary>
        /// Creates a new absolute <c>ParentRelLength</c> from a length in pixels.
        /// </summary>
        /// <param name="length">The length in pixels.</param>
        public RelLength(float length) : this(length, 0f) { }

        /// <summary>
        /// Negates the <c>RelLenght</c>.
        /// </summary>
        public static RelLength operator -(RelLength self) => new RelLength(
            -self.delta,
            -self.relative
        );
        /// <summary>
        /// Adds two <c>RelLength</c>s together.
        /// </summary>
        public static RelLength operator +(RelLength lhs, RelLength rhs) => new RelLength(
            lhs.delta + rhs.delta,
            lhs.relative + rhs.relative
        );
        /// <summary>
        /// Subtracts one <c>RelVector2</c> from another.
        /// </summary>
        public static RelLength operator -(RelLength lhs, RelLength rhs) => new RelLength(
            lhs.delta - rhs.delta,
            lhs.relative - rhs.relative
        );
        /// <summary>
        /// Scales both dimensions of a <c>RelVector2</c> up by a constant factor.
        /// </summary>
        public static RelLength operator *(RelLength lhs, float rhs) => new RelLength(
            lhs.delta * rhs,
            lhs.relative * rhs
        );
        /// <summary>
        /// Scales both dimensions of a <c>RelVector2</c> up by a constant factor.
        /// </summary>
        public static RelLength operator *(float lhs, RelLength rhs) => rhs * lhs;
        /// <summary>
        /// Scales both dimensions of a <c>RelVector2</c> down by a constant factor.
        /// </summary>
        public static RelLength operator /(RelLength lhs, float rhs) => new RelLength(
            lhs.delta / rhs,
            lhs.relative / rhs
        );
    }

    /// <summary>
    /// A struct to represent the data in a <c>RectTransform</c>.
    /// </summary>
    public struct RectTransformData
    {
        /// <summary>
        /// See <c>RectTransform.sizeDelta</c> in the unity docs.
        /// </summary>
        public Vector2 sizeDelta;
        /// <summary>
        /// See <c>RectTransform.anchorMin</c> in the unity docs.
        /// </summary>
        public Vector2 anchorMin;
        /// <summary>
        /// See <c>RectTransform.anchorMax</c> in the unity docs.
        /// </summary>
        public Vector2 anchorMax;
        /// <summary>
        /// See <c>RectTransform.anchoredPosition</c> in the unity docs.
        /// </summary>
        public Vector2 anchoredPosition;
        /// <summary>
        /// See <c>RectTransform.pivot</c> in the unity docs.
        /// </summary>
        public Vector2 pivot;

        /// <summary>
        /// Creates a <c>RectTransformData</c> from an existing <c>RectTransform</c>.
        /// </summary>
        /// <param name="rt">The source <c>RectTransform</c>.</param>
        /// <returns></returns>
        public RectTransformData(RectTransform rt)
        {
            this.sizeDelta = rt.sizeDelta;
            this.anchorMin = rt.anchorMin;
            this.anchorMax = rt.anchorMax;
            this.anchoredPosition = rt.anchoredPosition;
            this.pivot = rt.pivot;
        }

        /// <summary>
        /// Create a <c>RectTransformData</c> from a <c>RectSize</c> and <c>RectPosition</c>.
        /// </summary>
        /// <param name="size">The size parent-relative</param>
        /// <param name="pos">The anchored position</param>
        /// <returns></returns>
        public static RectTransformData FromSizeAndPos(
            RelVector2 size,
            AnchoredPosition pos
        ) => pos.GetRepositioned(size.GetBaseTransformData());

        /// <summary>
        /// Apply the data to an existing <c>RectTransform</c>.
        /// </summary>
        /// <param name="rt">The <c>RectTransform</c> to apply the data to.</param>
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
        /// Convenience conversion to get the data from a <c>RectTransform</c>.
        /// </summary>
        public static implicit operator RectTransformData(RectTransform r) => new RectTransformData(r);
    }
}