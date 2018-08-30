// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to shuffle the deck and deal hands to all players.
    /// </summary>
    public class DealMove : Move
    {
        private const int MaxOutcomes = 20;

        private DealMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { Resources.Deal };

        /// <inheritdoc/>
        public override bool IsDeterministic => false;

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is DealMove deal)
            {
                return this.PlayerToken.CompareTo(deal.PlayerToken);
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
            var deck = GameState.StartingDeck.Shuffle().ToImmutableList();
            var hands = ImmutableDictionary.CreateBuilder<PlayerToken, ImmutableList<Card>>();
            foreach (var player in state.Players)
            {
                var dealt = deck.GetRange(deck.Count - GameState.InitialHandSize, GameState.InitialHandSize);
                deck = deck.GetRange(0, deck.Count - GameState.InitialHandSize);
                hands.Add(player, dealt);
            }

            state = state.With(
                deck: deck,
                hands: hands.ToImmutable(),
                fallacyCounter: state.Players.ToImmutableDictionary(p => p, p => 0),
                proof: GameState.StartingProof,
                isRoundOver: false);

            return base.Apply(state);
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            var deck = GameState.StartingDeck;
            var hands = ImmutableDictionary.CreateBuilder<PlayerToken, ImmutableList<Card>>();
            foreach (var player in state.Players)
            {
                var dealt = deck.GetRange(deck.Count - GameState.InitialHandSize, GameState.InitialHandSize);
                deck = deck.GetRange(0, deck.Count - GameState.InitialHandSize);
                hands.Add(player, dealt);
            }

            state = base.Apply(state.With(
                deck: deck,
                hands: hands.ToImmutable(),
                fallacyCounter: state.Players.ToImmutableDictionary(p => p, p => 0),
                proof: GameState.StartingProof,
                isRoundOver: false));

            var shuffler = new GameShuffler<GameState>(state);

            for (var i = 0; i < state.Deck.Count; i++)
            {
                var index = i;
                shuffler.Add(
                    "Cards",
                    state.Deck[index],
                    (s, value) => s.With(
                        deck: s.Deck.SetItem(index, value)));
            }

            foreach (var p in state.Players)
            {
                var player = p;
                for (var i = 0; i < state.Hands[player].Count; i++)
                {
                    var index = i;
                    shuffler.Add(
                        "Cards",
                        state.Hands[player][index],
                        (s, value) => s.With(
                            hands: s.Hands.SetItem(player, s.Hands[player].SetItem(index, value))));
                }
            }

            foreach (var shuffled in shuffler.Take(MaxOutcomes))
            {
                yield return Weighted.Create(shuffled, 1);
            }
        }
    }
}
