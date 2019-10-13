// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to draw a card from the player deck.
    /// </summary>
    public sealed class DrawCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        public DrawCardMove(GameState state, PlayerToken player)
            : base(state, player)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { Resources.DrawCard };

        /// <inheritdoc />
        public override bool IsDeterministic => this.GameState.Deck.Count <= 1;

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is DrawCardMove)
            {
                return this.PlayerToken.CompareTo(other.PlayerToken);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<DrawCardMove> GenerateMoves(GameState state)
        {
            if (state.Deck.Count > 0)
            {
                var player = state.FirstPlayer;
                do
                {
                    var inventory = state.Inventory[player];
                    if (inventory.Hand.Count < GameState.HandLimit && inventory.OwnedCards[Card.Charon] < GameState.PlayerCharonLimit)
                    {
                        yield return new DrawCardMove(state, player);
                        yield break;
                    }

                    player = state.GetNextPlayer(player);
                }
                while (player != state.FirstPlayer);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];

            var deck = state.Deck.Deal(out var dealt);
            playerInventory = playerInventory.With(
                hand: playerInventory.Hand.Add(dealt));

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory),
                deck: deck);

            return base.Apply(state);
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            foreach (var dealt in state.Deck.Keys)
            {
                var playerInventory = state.Inventory[this.PlayerToken];

                playerInventory = playerInventory.With(
                    hand: playerInventory.Hand.Add(dealt));

                var newState = state.With(
                    inventory: state.Inventory.SetItem(
                        this.PlayerToken,
                        playerInventory),
                    deck: state.Deck.Remove(dealt));

                yield return Weighted.Create(base.Apply(newState), state.Deck[dealt]);
            }
        }
    }
}
