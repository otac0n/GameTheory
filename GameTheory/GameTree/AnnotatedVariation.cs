// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.GameTree
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// A game or variation annotated with format tokens.
    /// </summary>
    /// <typeparam name="TGameState">The type of game that this variation will store.</typeparam>
    /// <typeparam name="TMove">The type of object that represents a move.</typeparam>
    /// <param name="startingPosition">The starting position of the variation.</param>
    /// <param name="objects">The format tokens, <typeparamref name="TMove">moves</typeparamref>, and <typeparamref name="TGameState">game states</typeparamref> in the variation.</param>
    public class AnnotatedVariation<TGameState, TMove>(TGameState startingPosition, IEnumerable<object> objects)
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Gets the starting position of the variation.
        /// </summary>
        public TGameState StartingPosition { get; } = startingPosition;

        /// <summary>
        /// Gets the format tokens, <typeparamref name="TMove">moves</typeparamref>, and <typeparamref name="TGameState">game states</typeparamref> in the variation.
        /// </summary>
        public IList<object> Objects { get; } = objects.ToImmutableList();
    }
}
