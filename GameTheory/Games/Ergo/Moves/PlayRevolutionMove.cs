// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using GameTheory.Games.Ergo.Cards;

    /// <summary>
    /// Represents a move to swap two proof cards using a Revolution card.
    /// </summary>
    public class PlayRevolutionMove : Move
    {
        private static readonly ImmutableDictionary<Card, int> RotationGroups;

        static PlayRevolutionMove()
        {
            RotationGroups = ImmutableDictionary.CreateRange(new Dictionary<Card, int>
            {
                [SymbolCard.VariableA] = 1,
                [SymbolCard.VariableB] = 1,
                [SymbolCard.VariableC] = 1,
                [SymbolCard.VariableD] = 1,
                [SymbolCard.WildVariable] = 1,
                [SymbolCard.And] = 2,
                [SymbolCard.Or] = 2,
                [SymbolCard.Then] = 2,
                [SymbolCard.WildOperator] = 2,
            });
        }

        private PlayRevolutionMove(GameState state, RevolutionCard card, int premiseIndex1, int cardIndex1, int symbolIndex1, int premiseIndex2, int cardIndex2, int symbolIndex2)
            : base(state)
        {
            this.Card = card;
            this.PremiseIndex1 = premiseIndex1;
            this.CardIndex1 = cardIndex1;
            this.SymbolIndex1 = symbolIndex1;
            this.PremiseIndex2 = premiseIndex2;
            this.CardIndex2 = cardIndex2;
            this.SymbolIndex2 = symbolIndex2;
        }

        /// <summary>
        /// Gets the card that will be played.
        /// </summary>
        public RevolutionCard Card { get; }

        /// <summary>
        /// Gets the index of the first card.
        /// </summary>
        public int CardIndex1 { get; }

        /// <summary>
        /// Gets the index of the second card.
        /// </summary>
        public int CardIndex2 { get; }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { "Move ", this.GameState.Proof[this.PremiseIndex2][this.CardIndex2].Card.Symbols[this.SymbolIndex2], " to ", this.PremiseIndex1, ":", this.CardIndex1, " and ", this.GameState.Proof[this.PremiseIndex1][this.CardIndex1].Card.Symbols[this.SymbolIndex1], " to ", this.PremiseIndex2, ":", this.CardIndex2, " using ", this.Card };

        /// <summary>
        /// Gets the premise containing the first card.
        /// </summary>
        public int PremiseIndex1 { get; }

        /// <summary>
        /// Gets the premise containing the second card.
        /// </summary>
        public int PremiseIndex2 { get; }

        /// <summary>
        /// Gets the index of the symbol on the first card that will be used when it is placed in place of the second card.
        /// </summary>
        public int SymbolIndex1 { get; }

        /// <summary>
        /// Gets the index of the symbol on the second card that will be used when it is placed in place of the first card.
        /// </summary>
        public int SymbolIndex2 { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is PlayRevolutionMove revolution)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(revolution.PlayerToken)) != 0 ||
                    (comp = this.PremiseIndex1.CompareTo(revolution.PremiseIndex1)) != 0 ||
                    (comp = this.CardIndex1.CompareTo(revolution.CardIndex1)) != 0 ||
                    (comp = this.SymbolIndex1.CompareTo(revolution.SymbolIndex1)) != 0 ||
                    (comp = this.PremiseIndex2.CompareTo(revolution.PremiseIndex2)) != 0 ||
                    (comp = this.CardIndex2.CompareTo(revolution.CardIndex2)) != 0 ||
                    (comp = this.SymbolIndex2.CompareTo(revolution.SymbolIndex2)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Proof[this.PremiseIndex1], revolution.GameState.Proof[revolution.PremiseIndex1])) != 0 ||
                    (this.PremiseIndex1 != this.PremiseIndex2 && (comp = CompareUtilities.CompareLists(this.GameState.Proof[this.PremiseIndex2], revolution.GameState.Proof[revolution.PremiseIndex2])) != 0))
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

        internal static IEnumerable<PlayRevolutionMove> GenerateMoves(GameState state)
        {
            if (state.Phase == Phase.Play)
            {
                var activePlayer = state.ActivePlayer;
                if (state.FallacyCounter[activePlayer] <= 0 && state.Hands[activePlayer].Contains(RevolutionCard.Instance))
                {
                    for (var p1 = 0; p1 < state.Proof.Count; p1++)
                    {
                        var premise1 = state.Proof[p1];
                        for (var i1 = 0; i1 < premise1.Count; i1++)
                        {
                            var card1 = premise1[i1].Card;
                            if (RotationGroups.TryGetValue(card1, out var group1))
                            {
                                for (var p2 = p1; p2 < state.Proof.Count; p2++)
                                {
                                    var premise2 = state.Proof[p2];
                                    for (var i2 = p2 == p1 ? i1 + 1 : 0; i2 < premise2.Count; i2++)
                                    {
                                        var card2 = premise2[i2].Card;
                                        if (RotationGroups.TryGetValue(card2, out var group2) && group1 == group2)
                                        {
                                            for (var s1 = 0; s1 < card1.Symbols.Count; s1++)
                                            {
                                                for (var s2 = 0; s2 < card2.Symbols.Count; s2++)
                                                {
                                                    yield return new PlayRevolutionMove(state, RevolutionCard.Instance, p1, i1, s1, p2, i2, s2);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var hand = state.Hands[this.PlayerToken];
            var proof = state.Proof;

            var replacement1 = new PlacedCard(proof[this.PremiseIndex1][this.CardIndex1].Card, this.SymbolIndex1);
            var replacement2 = new PlacedCard(proof[this.PremiseIndex2][this.CardIndex2].Card, this.SymbolIndex2);
            hand = hand.Remove(this.Card);
            proof = proof.SetItem(this.PremiseIndex1, proof[this.PremiseIndex1].SetItem(this.CardIndex1, replacement2));
            proof = proof.SetItem(this.PremiseIndex2, proof[this.PremiseIndex2].SetItem(this.CardIndex2, replacement1));

            state = state.With(
                hands: state.Hands.SetItem(this.PlayerToken, hand),
                proof: proof);

            return base.Apply(state);
        }
    }
}
