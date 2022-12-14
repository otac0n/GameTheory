// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to continue from the current state.
    /// </summary>
    public sealed class ContinueMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContinueMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public ContinueMove(GameState state)
            : base(state)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinueMove"/> class.
        /// </summary>
        /// <param name="player">The player playing the move.</param>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public ContinueMove(GameState state, PlayerToken player)
            : base(state, player)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.Continue);

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

        internal override GameState Apply(GameState state)
        {
            return base.Apply(state.WithInterstitialState(null));
        }
    }
}
