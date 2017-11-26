// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a move to take one of the dealt <see cref="Djinn">Djinns</see> and discard the others.
    /// </summary>
    internal class TakeDealtDjinnMove : Move
    {
        private readonly ImmutableList<Djinn> dealt;

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeDealtDjinnMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="dealt">The <see cref="Djinn">Djinns</see> dealt to the player.</param>
        /// <param name="index">The index of the <see cref="Djinn"/> that will be taken.</param>
        public TakeDealtDjinnMove(GameState state, ImmutableList<Djinn> dealt, int index)
            : base(state)
        {
            this.dealt = dealt;
            this.Index = index;
        }

        /// <summary>
        /// Gets the <see cref="Djinn"/> that will be taken.
        /// </summary>
        public Djinn Djinn => this.dealt[this.Index];

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Take ", this.dealt[this.Index] };

        /// <summary>
        /// Gets the index of the <see cref="Djinn"/> that will be taken.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return state.With(
                djinnDiscards: state.DjinnDiscards.AddRange(this.dealt.RemoveAt(this.Index)),
                inventory: state.Inventory.SetItem(player, inventory.With(djinns: inventory.Djinns.Add(this.dealt[this.Index]))));
        }
    }
}
