// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons
{
    /// <summary>
    /// Represents the color of a dragon.
    /// </summary>
    public enum Color : byte
    {
        /// <summary>
        /// The rainbow dragon which assumes all colors.
        /// </summary>
        Rainbow = 0,

        /// <summary>
        /// The red dragon.
        /// </summary>
        Red,

        /// <summary>
        /// The gold dragon.
        /// </summary>
        Gold,

        /// <summary>
        /// The blue dragon.
        /// </summary>
        Blue,

        /// <summary>
        /// The green dragon.
        /// </summary>
        Green,

        /// <summary>
        /// The black dragon.
        /// </summary>
        Black,

        /// <summary>
        /// The silver dragon will take the color of the top of the <see cref="GameState.DiscardPile"/> or <see cref="Rainbow"/> if there is none.
        /// </summary>
        Silver,
    }
}
