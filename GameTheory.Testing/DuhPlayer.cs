// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Testing
{
    using GameTheory.Players;
    using GameTheory.Strategies;

    public class DuhPlayer<TGameState, TMove> : StrategyPlayer<TGameState, TMove>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        public DuhPlayer(PlayerToken playerToken)
            : base(new ImmediateWinStrategy<TGameState, TMove>(), new RandomPlayer<TGameState, TMove>(playerToken))
        {
        }
    }
}
