/*
* Copyright (c) 2007-2009 SharpDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace PPDFramework
{
    /// <summary>
    /// Represents a vertex with a position and a texture coordinate.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ColoredTexturedVertex : IEquatable<ColoredTexturedVertex>
    {
        /// <summary>
        /// サイズ
        /// </summary>
        public static int Size = Marshal.SizeOf(typeof(ColoredTexturedVertex));

        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Gets or sets the color for the vertex.
        /// </summary>
        public int Color;

        /// <summary>
        /// Gets or sets the texture coordinate for the vertex.
        /// </summary>
        public Vector2 TextureCoordinates;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredTexturedVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        public ColoredTexturedVertex(Vector3 position)
            : this(position, PPDColors.White, Vector2.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredTexturedVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="textureCoordinates">textureCordinate</param>
        public ColoredTexturedVertex(Vector3 position, Vector2 textureCoordinates)
            : this(position, PPDColors.White, textureCoordinates)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredTexturedVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        /// <param name="textureCoordinates">textureCordinate</param>
        public ColoredTexturedVertex(Vector3 position, Color4 color, Vector2 textureCoordinates)
            : this()
        {
            Position = position;
            Color = color.ToBgra();
            TextureCoordinates = textureCoordinates;
        }

        /// <summary>
        /// Implements operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ColoredTexturedVertex left, ColoredTexturedVertex right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements operator !=.
        /// </summary>
        /// <param name="left">The left side of the operator.</param>
        /// <param name="right">The right side of the operator.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ColoredTexturedVertex left, ColoredTexturedVertex right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return Position.GetHashCode() + TextureCoordinates.GetHashCode();
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            return Equals((ColoredTexturedVertex)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(ColoredTexturedVertex other)
        {
            return (Position == other.Position && TextureCoordinates == other.TextureCoordinates);
        }
    }
}
