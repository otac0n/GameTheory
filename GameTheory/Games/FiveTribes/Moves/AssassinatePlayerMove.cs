// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    /// <summary>
    /// Represents a move to assassinate specific <see cref="Meeple">Meeples</see> from a specific player's inventory.
    /// </summary>
    public class AssassinatePlayerMove : Move
    {
        private readonly Func<GameState, GameState> after;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinatePlayerMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="victim">The <see cref="PlayerToken"/> whose <see cref="Meeple">Meeples</see> will be assassinated.</param>
        /// <param name="meeples">The <see cref="Meeple">Meeples</see> that will be assassinated.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public AssassinatePlayerMove(GameState state, PlayerToken victim, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state)
        {
            this.after = after;
            this.Meeples = meeples;
            this.Victim = victim;
        }

        /// <summary>
        /// Gets the <see cref="Meeple">Meeples</see> that will be assassinated.
        /// </summary>
        public EnumCollection<Meeple> Meeples { get; }

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> whose <see cref="Meeple">Meeples</see> will be assassinated.
        /// </summary>
        public PlayerToken Victim { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => $"Assassinate {this.Victim}'s {this.Meeples}";

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory[this.Victim];
            var newState = state.With(
                bag: state.Bag.AddRange(state.InHand).AddRange(this.Meeples),
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state.Inventory.SetItem(this.Victim, inventory.With(meeples: inventory.Meeples.RemoveRange(this.Meeples))));

            foreach (var owner in newState.Players)
            {
                foreach (var djinn in newState.Inventory[owner].Djinns)
                {
                    newState = djinn.HandleAssassination(owner, newState, this.Victim, this.Meeples);
                }
            }

            return this.after(newState);
        }
    }
}
