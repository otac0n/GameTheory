﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to take a specified <see cref="Resource"/>.
    /// </summary>
    public sealed class TakeResourceMove : Move
    {
        private readonly Func<GameState, GameState> after;

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeResourceMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the <see cref="Resource"/> that will be taken.</param>
        public TakeResourceMove(GameState state, int index)
            : this(state, index, s => s)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeResourceMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the <see cref="Resource"/> that will be taken.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public TakeResourceMove(GameState state, int index, Func<GameState, GameState> after)
            : base(state)
        {
            this.Index = index;
            this.after = after;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.TakeItem, this.Resource);

        /// <summary>
        /// Gets the index of the <see cref="Resource"/> that will be taken.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the <see cref="Resource"/> that will be taken.
        /// </summary>
        public Resource Resource => this.GameState.VisibleResources[this.Index];

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return this.after(state.With(
                inventory: state.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.Add(state.VisibleResources[this.Index]))),
                visibleResources: state.VisibleResources.RemoveAt(this.Index)));
        }
    }
}
