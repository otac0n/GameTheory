// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to reveal a player's hand.
    /// </summary>
    public sealed class DealMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DealMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public DealMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.Deal);

        /// <inheritdoc />
        public override bool IsDeterministic => false;

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is ContinueMove move)
            {
                return this.PlayerToken.CompareTo(move.PlayerToken);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<DealMove> GenerateMoves(GameState state)
        {
            yield return new DealMove(state);
        }

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory;
            GameState.DealNewRound(ref inventory, out var deck, out var hidden, out var inaccessible);

            state = state.With(
                hidden: hidden,
                inaccessible: inaccessible,
                inventory: inventory,
                deck: deck);

            return base.Apply(state);
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            throw new NotImplementedException();
        }
    }
}
