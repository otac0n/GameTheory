// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    public class GameTree<TMove, TScore>
        where TMove : IMove
    {
        private readonly ICache<TMove, TScore> cache;

        public GameTree(ICache<TMove, TScore> cache)
        {
            this.cache = cache;
        }

        public StateNode<TMove, TScore> GetOrAdd(IGameState<TMove> value)
        {
            if (!this.cache.TryGetValue(value, out var result))
            {
                this.cache.SetValue(value, result = new StateNode<TMove, TScore>(this, value));
            }

            return result;
        }

        public void Trim()
        {
            this.cache.Trim();
        }
    }
}
