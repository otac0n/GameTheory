// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using GameTheory.Games.FiveTribes.Djinns;

    /// <summary>
    /// Represents a move to take a specified <see cref="Djinn"/>.
    /// </summary>
    public class TakeDjinnMove : Move
    {
        private readonly Func<GameState, GameState> after;

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeDjinnMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the <see cref="Djinn"/> that will be taken.</param>
        public TakeDjinnMove(GameState state, int index)
            : this(state, index, s => s)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeDjinnMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the <see cref="Djinn"/> that will be taken.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public TakeDjinnMove(GameState state, int index, Func<GameState, GameState> after)
            : base(state)
        {
            this.Index = index;
            this.after = after;
        }

        /// <summary>
        /// Gets the <see cref="Djinn"/> that will be taken.
        /// </summary>
        public Djinn Djinn => this.State.VisibleDjinns[this.Index];

        /// <summary>
        /// Gets the index of the <see cref="Djinn"/> that will be taken.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => $"Take {this.Djinn}";

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return this.after(state.With(
                inventory: state.Inventory.SetItem(player, inventory.With(djinns: inventory.Djinns.Add(state.VisibleDjinns[this.Index]))),
                visibleDjinns: state.VisibleDjinns.RemoveAt(this.Index)));
        }
    }
}
