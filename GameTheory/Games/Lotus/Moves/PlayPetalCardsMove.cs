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
    public sealed class PlayPetalCardsMove : Move
    {
        private const int CardLimit = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayPetalCardsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="cardIndexes">The indexes of cards being played.</param>
        public PlayPetalCardsMove(GameState state, ImmutableList<int> cardIndexes)
            : base(state)
        {
            this.CardIndexes = cardIndexes;
        }

        /// <summary>
        /// Gets the indexes of the cards being played.
        /// </summary>
        public ImmutableList<int> CardIndexes { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens
        {
            get
            {
                var tokens = new List<object>();

                var hand = this.State.Inventory[this.PlayerToken].Hand;
                for (var i = 0; i < this.CardIndexes.Count; i++)
                {
                    if (i == 0)
                    {
                        tokens.Add("Play ");
                    }
                    else if (i == this.CardIndexes.Count - 1)
                    {
                        tokens.Add(this.CardIndexes.Count > 2 ? ", and " : " and ");
                    }
                    else
                    {
                        tokens.Add(", ");
                    }

                    tokens.Add(hand[this.CardIndexes[i]]);
                }

                return tokens;
            }
        }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is PlayPetalCardsMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = CompareUtilities.CompareValueLists(this.CardIndexes, move.CardIndexes)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.State.Inventory[this.PlayerToken].Hand, move.State.Inventory[move.PlayerToken].Hand)) != 0)
                {
                    return comp;
                }

                return 0;
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<PlayPetalCardsMove> GenerateMoves(GameState state)
        {
            var playerInventory = state.Inventory[state.ActivePlayer];
            var hand = playerInventory.Hand;
            var flowerTypes = hand.Select(c => c.FlowerType).Distinct().ToList();
            foreach (var flowerType in flowerTypes)
            {
                var indexes = Enumerable.Range(0, hand.Count).Where(i => hand[i].FlowerType == flowerType).ToList();

                var maxPlays = (int)flowerType - state.Field[flowerType].Petals.Count;
                maxPlays = Math.Min(maxPlays, indexes.Count);
                if (!playerInventory.SpecialPowers.HasFlag(SpecialPower.InfiniteGrowth))
                {
                    maxPlays = Math.Min(maxPlays, CardLimit);
                }

                var distinct = new List<ImmutableList<PetalCard>>();
                foreach (var combination in indexes.Combinations(maxPlays, includeSmaller: true))
                {
                    var key = combination.Select(c => hand[c]).OrderByDescending(c => c.Guardians).ToImmutableList();
                    if (!distinct.Any(d => d.SequenceEqual(key)))
                    {
                        distinct.Add(key);
                        yield return new PlayPetalCardsMove(state, combination);
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];
            var hand = playerInventory.Hand;
            var played = this.CardIndexes.Select(i => hand[i]).ToList();

            foreach (var i in this.CardIndexes.OrderByDescending(i => i))
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
