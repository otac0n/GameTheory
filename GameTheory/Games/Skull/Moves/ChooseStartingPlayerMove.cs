// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a move to choose the next starting player.
    /// </summary>
    public sealed class ChooseStartingPlayerMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChooseStartingPlayerMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="startingPlayer">The player who will start the next round.</param>
        public ChooseStartingPlayerMove(GameState state, PlayerToken startingPlayer)
            : base(state)
        {
            this.StartingPlayer = startingPlayer;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.ChooseStartingPlayer, this.StartingPlayer);

        /// <summary>
        /// Gets the player who will start the next round.
        /// </summary>
        public PlayerToken StartingPlayer { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is ChooseStartingPlayerMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.StartingPlayer.CompareTo(move.StartingPlayer)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Players, move.GameState.Players)) != 0)
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

        internal static IEnumerable<ChooseStartingPlayerMove> GenerateMoves(GameState state)
        {
            foreach (var player in state.Players)
            {
                if (state.Inventory[player].Bid > Inventory.PassingBid)
                {
                    yield return new ChooseStartingPlayerMove(state, player);
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            state = state.With(
                phase: Phase.AddingCards,
                activePlayer: this.StartingPlayer);

            return base.Apply(state);
        }
    }
}
