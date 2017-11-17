// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to play petal cards to a flower.
    /// </summary>
    public class PlayPetalCardsMove : Move
    {
        private const int CardLimit = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayPetalCardsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="cardIndices">The indices of cards being played.</param>
        public PlayPetalCardsMove(GameState state, ImmutableList<int> cardIndices)
            : base(state)
        {
            this.CardIndices = cardIndices;
        }

        /// <summary>
        /// Gets the indices of cards being played.
        /// </summary>
        public ImmutableList<int> CardIndices { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens
        {
            get
            {
                var tokens = new List<object>();

                var hand = this.State.Inventory[this.PlayerToken].Hand;
                for (var i = 0; i < this.CardIndices.Count; i++)
                {
                    if (i == 0)
                    {
                        tokens.Add("Play ");
                    }
                    else if (i == this.CardIndices.Count - 1)
                    {
                        tokens.Add(this.CardIndices.Count > 2 ? ", and " : " and ");
                    }
                    else
                    {
                        tokens.Add(", ");
                    }

                    tokens.Add(hand[this.CardIndices[i]]);
                }

                return tokens;
            }
        }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var playerInventory = state.Inventory[state.ActivePlayer];
            var hand = playerInventory.Hand;
            var flowerTypes = hand.Select(c => c.FlowerType).Distinct().ToList();
            foreach (var flowerType in flowerTypes)
            {
                var indices = Enumerable.Range(0, hand.Count).Where(i => hand[i].FlowerType == flowerType).ToList();

                var maxPlays = (int)flowerType - state.Field[flowerType].Petals.Count;
                maxPlays = Math.Min(maxPlays, indices.Count);
                if (!playerInventory.SpecialPowers.HasFlag(SpecialPower.InfiniteGrowth))
                {
                    maxPlays = Math.Min(maxPlays, CardLimit);
                }

                for (var c = 1; c <= maxPlays; c++)
                {
                    // TODO: Combinations.
                    yield return new PlayPetalCardsMove(state, indices.Take(c).ToImmutableList());
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];
            var hand = playerInventory.Hand;
            var played = this.CardIndices.Select(i => hand[i]).ToList();

            foreach (var i in this.CardIndices.OrderByDescending(i => i))
            {
                hand = hand.RemoveAt(i);
            }

            playerInventory = playerInventory.With(
                hand: hand);

            var flowerType = played[0].FlowerType;
            var flower = state.Field[flowerType];
            flower = flower.With(
                petals: flower.Petals.AddRange(played));

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory),
                field: state.Field.SetItem(
                    flowerType,
                    flower));

            return base.Apply(state);
        }
    }
}
