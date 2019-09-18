// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull
{
    /// <summary>
    /// Represents the current phase of the game.
    /// </summary>
    public enum Phase
    {
        /// <summary>
        /// Players are adding cards to their stacks.
        /// </summary>
        AddingCards,

        /// <summary>
        /// A player has issued a challenge and more than one player may still bid.
        /// </summary>
        Bidding,

        /// <summary>
        /// The challenger is seeking flower cards to fulfill their bid.
        /// </summary>
        Challenge,

        /// <summary>
        /// The challenger revealed a skull and must lose a card.
        /// </summary>
        ChooseDiscard,

        /// <summary>
        /// The eliminated player must choose the next starting player.
        /// </summary>
        ChooseStartingPlayer,

        /// <summary>
        /// End of the game.
        /// </summary>
        End,
    }
}
