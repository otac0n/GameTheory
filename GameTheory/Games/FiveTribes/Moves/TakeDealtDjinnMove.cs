// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Immutable;
    using GameTheory.Games.FiveTribes.Djinns;

    /// <summary>
    /// Represents a move to take one of the dealt <see cref="Djinn">Djinns</see> and discard the others.
    /// </summary>
    internal class TakeDealtDjinnMove : Move
    {
        private readonly ImmutableList<Djinn> dealt;
        private readonly int index;

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeDealtDjinnMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="dealt">The <see cref="Djinn">Djinns</see> dealt to the player.</param>
        /// <param name="index">The index of the <see cref="Djinn"/> that will be taken.</param>
        public TakeDealtDjinnMove(GameState state, ImmutableList<Djinn> dealt, int index)
            : base(state, state.ActivePlayer)
        {
            this.dealt = dealt;
            this.index = index;
        }

        /// <summary>
        /// Gets the <see cref="Djinn"/> that will be taken.
        /// </summary>
        public Djinn Djinn
        {
            get { return this.dealt[this.index]; }
        }

        /// <summary>
        /// Gets the index of the <see cref="Djinn"/> that will be taken.
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Take {this.dealt[this.index]}";
        }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return state.With(
                djinnDiscards: state.DjinnDiscards.AddRange(this.dealt.RemoveAt(this.index)),
                inventory: state.Inventory.SetItem(player, inventory.With(djinns: inventory.Djinns.Add(this.dealt[this.index]))));
        }
    }
}
