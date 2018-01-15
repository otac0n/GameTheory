// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Moves
{
    using System.Collections.Generic;
    using GameTheory.Games.Ergo.Cards;

    /// <summary>
    /// Represents a move to play a Fallacy card against another player.
    /// </summary>
    public class PlayFallacyMove : Move
    {
        private PlayFallacyMove(GameState state, FallacyCard card, PlayerToken victim)
            : base(state)
        {
            this.Card = card;
            this.Victim = victim;
        }

        /// <summary>
        /// Gets the card that will be played.
        /// </summary>
        public FallacyCard Card { get; }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { "Play ", this.Card, " on ", this.Victim };

        /// <summary>
        /// Gets the player who will be the victim of the fallacy.
        /// </summary>
        public PlayerToken Victim { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is PlayFallacyMove fallacy)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(fallacy.PlayerToken)) != 0 ||
                    (comp = this.Victim.CompareTo(fallacy.Victim)) != 0)
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

        internal static IEnumerable<PlayFallacyMove> GenerateMoves(GameState state)
        {
            if (state.Phase == Phase.Play)
            {
                var activePlayer = state.ActivePlayer;
                if (state.Hands[activePlayer].Contains(FallacyCard.Instance))
                {
                    foreach (var victim in state.Players)
                    {
                        if (victim == activePlayer || state.FallacyCounter[victim] < 0)
                        {
                            continue;
                        }

                        yield return new PlayFallacyMove(state, FallacyCard.Instance, victim);
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var hand = state.Hands[this.PlayerToken];

            hand = hand.Remove(this.Card);

            state = state.With(
                hands: state.Hands.SetItem(this.PlayerToken, hand),
                fallacyCounter: state.FallacyCounter.SetItem(this.Victim, GameState.FallacyTurns));

            return base.Apply(state);
        }
    }
}
