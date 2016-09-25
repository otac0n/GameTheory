// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
    }
}
