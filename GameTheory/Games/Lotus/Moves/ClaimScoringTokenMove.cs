// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to claim a scoring token.
    /// </summary>
    public class ClaimScoringTokenMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimScoringTokenMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        public ClaimScoringTokenMove(GameState state, PlayerToken player)
            : base(state, player)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new[] { "Claim a scoring token" };

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is ClaimScoringTokenMove)
            {
                return this.PlayerToken.CompareTo(other.PlayerToken);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            foreach (var player in state.ChoosingPlayers.Take(1))
            {
                yield return new ClaimScoringTokenMove(state, player);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];
            var choosingPlayers = state.ChoosingPlayers.Remove(this.PlayerToken);

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory.With(
                        scoringTokens: playerInventory.ScoringTokens + 1)),
                choosingPlayers: choosingPlayers,
                phase: choosingPlayers.Count != 0 ? Phase.ClaimReward : Phase.Play);

            return base.Apply(state);
        }
    }
}
