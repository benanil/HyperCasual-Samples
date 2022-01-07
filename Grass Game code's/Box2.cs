using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using UnityEngine;

namespace OpenTK.Mathematics
{
    /// <summary>
    /// Defines an axis-aligned 2d box (rectangle).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Box2 : IEquatable<Box2>
    {
        private Vector2 _min;

        /// <summary>
        /// Gets or sets the minimum boundary of the structure.
        /// </summary>
        public Vector2 Min
        {
            get => _min;
            set
            {
                if (value.x > _max.x)
                {
                    _max.x = value.x;
                }
                if (value.y > _max.y)
                {
                    _max.y = value.y;
                }

                _min = value;
            }
        }

        private Vector2 _max;

        /// <summary>
        /// Gets or sets the maximum boundary of the structure.
        /// </summary>
        public Vector2 Max
        {
            get => _max;
            set
            {
                if (value.x < _min.x)
                {
                    _min.x = value.x;
                }
                if (value.y < _min.y)
                {
                    _min.y = value.y;
                }

                _max = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Box2"/> struct.
        /// </summary>
        /// <param name="min">The minimum point on the xy plane this box encloses.</param>
        /// <param name="max">The maximum point on the xy plane this box encloses.</param>
        public Box2(Vector2 min, Vector2 max)
        {
            _min = Vector2.Min(min, max);
            _max = Vector2.Max(min, max);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Box2"/> struct.
        /// </summary>
        /// <param name="minx">The minimum x value to be enclosed.</param>
        /// <param name="miny">The minimum y value to be enclosed.</param>
        /// <param name="maxx">The maximum x value to be enclosed.</param>
        /// <param name="maxy">The maximum y value to be enclosed.</param>
        public Box2(float minx, float miny, float maxx, float maxy)
            : this(new Vector2(minx, miny), new Vector2(maxx, maxy))
        {
        }

        /// <summary>
        /// Gets or sets a vector describing the size of the Box2 structure.
        /// </summary>
        public Vector2 CenteredSize
        {
            get => Max - Min;
            set
            {
                Vector2 center = Center;
                _min = center - (value * 0.5f);
                _max = center + (value * 0.5f);
            }
        }

        /// <summary>
        /// Gets or sets a vector describing half the size of the box.
        /// </summary>
        public Vector2 HalfSize
        {
            get => CenteredSize / 2;
            set => CenteredSize = value * 2;
        }

        /// <summary>
        /// Gets or sets a vector describing the center of the box.
        /// </summary>
        public Vector2 Center
        {
            get => HalfSize + _min;
            set => Translate(value - Center);
        }

        // --

        /// <summary>
        /// Gets or sets the width of the box.
        /// </summary>
        public float Width
        {
            get => _max.x - _min.x;
            set => _max.x = _min.x + value;
        }

        /// <summary>
        /// Gets or sets the height of the box.
        /// </summary>
        public float Height
        {
            get => _max.y - _min.y;
            set => _max.y = _min.y + value;
        }

        /// <summary>
        /// Gets or sets the left location of the box.
        /// </summary>
        public float Left
        {
            get => _min.x;
            set => _min.x = value;
        }

        /// <summary>
        /// Gets or sets the top location of the box.
        /// </summary>
        public float Top
        {
            get => _min.y;
            set => _min.y = value;
        }

        /// <summary>
        /// Gets or sets the right location of the box.
        /// </summary>
        public float Right
        {
            get => _max.x;
            set => _max.x = value;
        }

        /// <summary>
        /// Gets or sets the bottom location of the box.
        /// </summary>
        public float Bottom
        {
            get => _max.y;
            set => _max.y = value;
        }

        /// <summary>
        /// Gets or sets the x location of the box.
        /// </summary>
        public float x
        {
            get => _min.x;
            set => _min.x = value;
        }

        /// <summary>
        /// Gets or sets the y location of the box.
        /// </summary>
        public float y
        {
            get => _min.y;
            set => _min.y = value;
        }

        /// <summary>
        /// Gets or sets the horizontal size.
        /// </summary>
        public float Sizex
        {
            get => _max.x - _min.x;
            set => _max.x = _min.x + value;
        }

        /// <summary>
        /// Gets or sets the vertical size.
        /// </summary>
        public float Sizey
        {
            get => _max.y - _min.y;
            set => _max.y = _min.y + value;
        }

        /// <summary>
        /// Gets or sets the size of the box.
        /// </summary>
        public Vector2 Size
        {
            get => new Vector2(_max.x - _min.x, _max.y - _min.y);
            set
            {
                _max.x = _min.x + value.x;
                _max.y = _min.y + value.y;
            }
        }

        /// <summary>
        /// Gets the location of the box.
        /// </summary>
        public Vector2 Location => _min;

        /// <summary>
        /// Gets a value indicating whether all values are zero.
        /// </summary>
        public bool IsZero => _min.x == 0 && _min.y == 0 && _max.x == 0 && _max.y == 0;

        /// <summary>
        /// Gets a box with all components zero.
        /// </summary>
        public static readonly Box2 Empty = new Box2(0, 0, 0, 0);

        /// <summary>
        /// Gets a box with a location 0,0 with the a size of 1.
        /// </summary>
        public static readonly Box2 UnitSquare = new Box2(0, 0, 1, 1);

        /// <summary>
        /// Creates a box.
        /// </summary>
        /// <param name="location">The location of the box.</param>
        /// <param name="size">The size of the box.</param>
        /// <returns>A box.</returns>
        public static Box2 FromSize(Vector2 location, Vector2 size)
        {
            return new Box2(location, location + size);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Box2"/> struct.
        /// </summary>
        /// <param name="min">The minimum point on the xy plane this box encloses.</param>
        /// <param name="max">The maximum point on the xy plane this box encloses.</param>
        /// <returns>A box.</returns>
        public static Box2 FromPositions(Vector2 min, Vector2 max)
        {
            return new Box2(min, max);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Box2"/> struct.
        /// </summary>
        /// <param name="minx">The minimum x value to be enclosed.</param>
        /// <param name="miny">The minimum y value to be enclosed.</param>
        /// <param name="maxx">The maximum x value to be enclosed.</param>
        /// <param name="maxy">The maximum y value to be enclosed.</param>
        /// <returns>A box.</returns>
        public static Box2 FromPositions(float minx, float miny, float maxx, float maxy)
        {
            return new Box2(minx, miny, maxx, maxy);
        }

        /// <summary>
        /// Replaces this Box with the intersection of itself and the specified Box.
        /// </summary>
        /// <param name="other">The Box with which to intersect.</param>
        public void Intersect(Box2 other)
        {
            Box2 result = Intersect(other, this);

            x = result.x;
            y = result.y;
            Width = result.Width;
            Height = result.Height;
        }

        /// <summary>
        /// Returns the intersection of two Boxes.
        /// </summary>
        /// <param name="a">The first box.</param>
        /// <param name="b">The second box.</param>
        /// <returns>The intersection of two Boxes.</returns>
        public static Box2 Intersect(Box2 a, Box2 b)
        {
            float minx = a._min.x > b._min.x ? a._min.x : b._min.x;
            float miny = a._min.y > b._min.y ? a._min.y : b._min.y;
            float maxx = a._max.x < b._max.x ? a._max.x : b._max.x;
            float maxy = a._max.y < b._max.y ? a._max.y : b._max.y;

            if (maxx >= minx && maxy >= miny)
            {
                return new Box2(minx, miny, maxx, maxy);
            }
            return Box2.Empty;
        }

        /// <summary>
        /// Returns the intersection of itself and the specified Box.
        /// </summary>
        /// <param name="other">The Box with which to intersect.</param>
        /// <returns>The intersection of itself and the specified Box.</returns>
        public Box2 Intersected(Box2 other)
        {
            return Intersect(other, this);
        }

        /// <summary>
        /// Determines if this Box intersects with another Box.
        /// </summary>
        /// <param name="other">The Box to test.</param>
        /// <returns>This method returns true if there is any intersection, otherwise false.</returns>
        public bool IntersectsWith(Box2 other)
        {
            return other._min.x < _max.x
                && _min.x < other._max.x
                && other._min.y < _max.y
                && _min.y < other._max.y;
        }

        /// <summary>
        /// Determines if this Box intersects or touches with another Box.
        /// </summary>
        /// <param name="other">The Box to test.</param>
        /// <returns>This method returns true if there is any intersection or touches, otherwise false.</returns>
        public bool TouchWith(Box2 other)
        {
            return other._min.x <= _max.x
                && _min.x <= other._max.x
                && other._min.y <= _max.y
                && _min.y <= other._max.y;
        }

        /// <summary>
        /// Gets a Box structure that contains the union of two Box structures.
        /// </summary>
        /// <param name="a">A Box to union.</param>
        /// <param name="b">a box to union.</param>
        /// <returns>A Box structure that bounds the union of the two Box structures.</returns>
        public static Box2 Union(Box2 a, Box2 b)
        {
            float minx = a._min.x < b._min.x ? a._min.x : b._min.x;
            float miny = a._min.y < b._min.y ? a._min.y : b._min.y;
            float maxx = a._max.x > b._max.x ? a._max.x : b._max.x;
            float maxy = a._max.y > b._max.y ? a._max.y : b._max.y;

            return new Box2(minx, miny, maxx, maxy);
        }
        
        /// <summary>
        /// Returns whether the box contains the specified point (borders exclusive).
        /// </summary>
        /// <param name="point">The point to query.</param>
        /// <returns>Whether this box contains the point.</returns>
        [Pure]
        public bool Contains(Vector2 point)
        {
            return _min.x < point.x && point.x < _max.x &&
                   _min.y < point.y && point.y < _max.y;
        }

        /// <summary>
        /// Returns whether the box contains the specified point.
        /// </summary>
        /// <param name="point">The point to query.</param>
        /// <param name="boundaryInclusive">
        /// Whether points on the box boundary should be recognised as contained as well.
        /// </param>
        /// <returns>Whether this box contains the point.</returns>
        [Pure]
        public bool Contains(Vector2 point, bool boundaryInclusive)
        {
            if (boundaryInclusive)
            {
                return _min.x <= point.x && point.x <= _max.x &&
                       _min.y <= point.y && point.y <= _max.y;
            }
            return _min.x < point.x && point.x < _max.x &&
                   _min.y < point.y && point.y < _max.y;
        }

        /// <summary>
        /// Returns whether the box contains the specified box (borders inclusive).
        /// </summary>
        /// <param name="other">The box to query.</param>
        /// <returns>Whether this box contains the other box.</returns>
        [Pure]
        public bool Contains(Box2 other)
        {
            return _max.x >= other._min.x && _min.x <= other._max.x &&
                   _max.y >= other._min.y && _min.y <= other._max.y;
        }

        /// <summary>
        /// Returns the distance between the nearest edge and the specified point.
        /// </summary>
        /// <param name="point">The point to find distance for.</param>
        /// <returns>The distance between the specified point and the nearest edge.</returns>
        [Pure]
        public float DistanceToNearestEdge(Vector2 point)
        {
            var distx = new Vector2(
                Math.Max(0f, Math.Max(_min.x - point.x, point.x - _max.x)),
                Math.Max(0f, Math.Max(_min.y - point.y, point.y - _max.y)));
            return distx.magnitude;
        }

        /// <summary>
        /// Translates this Box2 by the given amount.
        /// </summary>
        /// <param name="distance">The distance to translate the box.</param>
        public void Translate(Vector2 distance)
        {
            _min += distance;
            _max += distance;
        }

        /// <summary>
        /// Returns a Box2 translated by the given amount.
        /// </summary>
        /// <param name="distance">The distance to translate the box.</param>
        /// <returns>The translated box.</returns>
        [Pure]
        public Box2 Translated(Vector2 distance)
        {
            // create a local copy of this box
            Box2 box = this;
            box.Translate(distance);
            return box;
        }

        /// <summary>
        /// Scales this Box2 by the given amount.
        /// </summary>
        /// <param name="scale">The scale to scale the box.</param>
        /// <param name="anchor">The anchor to scale the box from.</param>
        public void Scale(Vector2 scale, Vector2 anchor)
        {
            _min = anchor + ((_min - anchor) * scale);
            _max = anchor + ((_max - anchor) * scale);
        }

        /// <summary>
        /// Returns a Box2 scaled by a given amount from an anchor point.
        /// </summary>
        /// <param name="scale">The scale to scale the box.</param>
        /// <param name="anchor">The anchor to scale the box from.</param>
        /// <returns>The scaled box.</returns>
        [Pure]
        public Box2 Scaled(Vector2 scale, Vector2 anchor)
        {
            // create a local copy of this box
            Box2 box = this;
            box.Scale(scale, anchor);
            return box;
        }

        /// <summary>
        /// Inflate this Box2 to encapsulate a given point.
        /// </summary>
        /// <param name="point">The point to query.</param>
        public void Inflate(Vector2 point)
        {
            _min = Vector2.Min(_min, point);
            _max = Vector2.Max(_max, point);
        }

        /// <summary>
        /// Inflate this Box2 to encapsulate a given point.
        /// </summary>
        /// <param name="point">The point to query.</param>
        /// <returns>The inflated box.</returns>
        [Pure]
        public Box2 Inflated(Vector2 point)
        {
            // create a local copy of this box
            Box2 box = this;
            box.Inflate(point);
            return box;
        }

        /// <summary>
        /// Equality comparator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        public static bool operator ==(Box2 left, Box2 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality comparator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        public static bool operator !=(Box2 left, Box2 right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Box2 && Equals((Box2)obj);
        }

        /// <inheritdoc/>
        public bool Equals(Box2 other)
        {
            return _min.Equals(other._min) &&
                   _max.Equals(other._max);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Min} - {Max}";
        }
    }
}