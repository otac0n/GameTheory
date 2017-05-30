﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    /// <summary>
    /// Represents a move to place a Camel at a specified <see cref="Point"/>.
    /// </summary>
    public class PlaceCamelMove : Move
    {
        private readonly Func<GameState, GameState> after;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceCamelMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="point">The <see cref="Point"/> where a Camel will be placed.</param>
        public PlaceCamelMove(GameState state, Point point)
            : this(state, point, s => s)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceCamelMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="point">The <see cref="Point"/> where a Camel will be placed.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PlaceCamelMove(GameState state, Point point, Func<GameState, GameState> after)
            : base(state)
        {
            this.after = after;
            this.Point = point;
        }

        /// <summary>
        /// Gets the <see cref="Point"/> where a Camel will be placed.
        /// </summary>
        public Point Point { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => $"Place a Camel at {this.Point}";

        internal override GameState Apply(GameState state)
        {
            var owner = state.ActivePlayer;

            return this.after(state.With(
                sultanate: state.Sultanate.SetItem(this.Point, state.Sultanate[this.Point].With(owner: owner))));
        }
    }
}
