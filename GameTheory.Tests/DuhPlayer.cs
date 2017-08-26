// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests
{
    using GameTheory.Players;
    using GameTheory.Strategies;

    internal class DuhPlayer<TMove> : StrategyPlayer<TMove>
        where TMove : IMove
    {
        public DuhPlayer(PlayerToken playerToken)
            : base(new ImmediateWinStrategy<TMove>(), new RandomPlayer<TMove>(playerToken))
        {
        }
    }
}
