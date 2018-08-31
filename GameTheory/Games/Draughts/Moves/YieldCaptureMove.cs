// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to yield control of the turn to the other player while capturing.
    /// </summary>
    public sealed class YieldCaptureMove : Move
    {
        private YieldCaptureMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { Resources.StopCapturing };

        internal static IEnumerable<YieldCaptureMove> GenerateMoves(GameState state)
        {
            if (CaptureMove.CapturesRemaining(state))
            {
                yield return new YieldCaptureMove(state);
            }
        }

        internal override GameState Apply(GameState state)
        {
            return base.Apply(state.With(
                lastCapturingIndex: default(Maybe<int>)));
        }
    }
}
