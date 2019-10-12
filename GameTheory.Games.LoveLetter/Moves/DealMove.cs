// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to reveal a player's hand.
    /// </summary>
    public sealed class DealMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DealMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public DealMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.Deal);

        /// <inheritdoc />
        public override bool IsDeterministic => false;

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is ContinueMove move)
            {
                return this.PlayerToken.CompareTo(move.PlayerToken);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<DealMove> GenerateMoves(GameState state)
        {
            yield return new DealMove(state);
        }

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory;
            GameState.DealNewRound(ref inventory, out var deck, out var hidden, out var inaccessible);

            state = state.With(
                hidden: hidden,
                inaccessible: inaccessible,
                inventory: inventory,
                deck: deck);

            return base.Apply(state);
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            var inaccessibleCombinations = state.Players.Length == GameState.MinPlayers
                ? new[] { Weighted.Create(EnumCollection<Card>.Empty, 1) }
                : GameState.StartingDeck.WeightedCombinations(3);

            foreach (var inaccessible in inaccessibleCombinations)
            {
                var accessibleDeck = GameState.StartingDeck.RemoveRange(inaccessible.Value);

                var toDeal = state.Players.Length + 1;
                foreach (var c in accessibleDeck.WeightedCombinations(toDeal))
                {
                    var combinationWeight = inaccessible.Weight * c.Weight;
                    foreach (var p in c.Value.WeightedPermutations(toDeal))
                    {
                        var inventoryBuilder = ImmutableDictionary.CreateBuilder<PlayerToken, Inventory>();
                        for (var i = 0; i < state.Players.Length; i++)
                        {
                            var player = state.Players[i];
                            var playerInventory = new Inventory(
                                hand: ImmutableArray.Create(p.Value[i]),
                                tokens: state.Inventory[player].Tokens,
                                discards: ImmutableStack<Card>.Empty,
                                handRevealed: false);
                            inventoryBuilder.Add(
                                player,
                                playerInventory);
                        }

                        var newState = state.With(
                            hidden: p.Value.Last(),
                            inaccessible: inaccessible.Value,
                            inventory: inventoryBuilder.ToImmutable(),
                            deck: accessibleDeck.RemoveRange(p.Value));

                        yield return Weighted.Create(base.Apply(state), combinationWeight * p.Weight);
                    }
                }
            }
        }
    }
}
