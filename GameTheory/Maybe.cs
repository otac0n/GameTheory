﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;

    /// <summary>
    /// Represents a possible value that can be either a reference or value type.
    /// </summary>
    /// <remarks>This is similar to <see cref="Nullable{T}"/>, but supports reference types.</remarks>
    /// <typeparam name="T">The type of the possible value.</typeparam>
    public struct Maybe<T>
    {
        private bool hasValue;
        private T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Maybe{T}"/> struct with the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public Maybe(T value)
        {
            this.hasValue = true;
            this.value = value;
        }

        /// <summary>
        /// Gets a value indicating whether or not there is a value.
        /// </summary>
        public bool HasValue => this.hasValue;

        /// <summary>
        /// Gets the possible value, or the defalut value of <typeparamref name="T"/>, if there is no value.
        /// </summary>
        public T ValueOrDefault => this.value;

        /// <summary>
        /// Gets the possible value.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no value.</exception>
        public T Value
        {
            get
            {
                if (!this.hasValue)
                {
                    throw new InvalidOperationException();
                }

                return this.value;
            }
        }

        /// <summary>
        /// Compares two <see cref="Maybe{T}"/> objects. The result specifies whether they are unequal.
        /// </summary>
        /// <param name="left">The first <see cref="Maybe{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="Maybe{T}"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Maybe<T> left, Maybe<T> right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="Maybe{T}"/> objects. The result specifies whether they are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Maybe{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="Maybe{T}"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Maybe<T> left, Maybe<T> right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is Maybe<T> other)
            {
                return other.hasValue == this.hasValue &&
                    (!other.hasValue || Equals(other.value, this.value));
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.hasValue ? this.value?.GetHashCode() ?? 0 : -1;
        }
    }
}
