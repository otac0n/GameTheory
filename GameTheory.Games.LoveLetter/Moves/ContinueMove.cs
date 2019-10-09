// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to draw a card.
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

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.Continue);

        internal override GameState Apply(GameState state)
        {
            return base.Apply(state.WithInterstitialState(null));
        }
    }
}
