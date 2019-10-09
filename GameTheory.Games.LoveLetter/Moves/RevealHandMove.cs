// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to reveal a player's hand.
    /// </summary>
    public sealed class RevealHandMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RevealHandMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="player">The player drawing a card.</param>
        public RevealHandMove(GameState state, PlayerToken player)
            : base(state, player)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.RevealHand);

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is RevealHandMove move)
            {
                return this.PlayerToken.CompareTo(move.PlayerToken);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<RevealHandMove> GenerateMoves(GameState state)
        {
            foreach (var player in state.Players)
            {
                var inventory = state.Inventory[player];
                if (!inventory.HandRevealed && inventory.Hand.Length > 0)
                {
                    yield return new RevealHandMove(state, player);
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory;
            var playerInventory = inventory[this.PlayerToken];

            playerInventory = playerInventory.With(
                handRevealed: true);
            inventory = inventory.SetItem(
                this.PlayerToken,
                playerInventory);

            state = state.With(
                inventory: inventory);

            return base.Apply(state);
        }
    }
}
