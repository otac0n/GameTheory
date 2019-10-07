// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to switch a scoop with another player.
    /// </summary>
    public sealed class SwitchAScoopMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchAScoopMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="takenFlavor">The flavor of scoop taken from the other player.</param>
        /// <param name="givenFlavor">The flavor of scoop given to the other player.</param>
        /// <param name="otherPlayer">The player who will be exchanging scoops with the active player.</param>
        public SwitchAScoopMove(GameState state, Flavor takenFlavor, Flavor givenFlavor, PlayerToken otherPlayer)
            : base(state)
        {
            this.TakenFlavor = takenFlavor;
            this.GivenFlavor = givenFlavor;
            this.OtherPlayer = otherPlayer;
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.SwitchAScoop, this.GivenFlavor, this.TakenFlavor, this.OtherPlayer);

        /// <summary>
        /// Gets the flavor of scoop given to the other player.
        /// </summary>
        public Flavor GivenFlavor { get; private set; }

        /// <inheritdoc/>
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the player who will be exchanging scoops with the active player.
        /// </summary>
        public PlayerToken OtherPlayer { get; private set; }

        /// <summary>
        /// Gets the flavor of scoop taken from the other player.
        /// </summary>
        public Flavor TakenFlavor { get; private set; }

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            foreach (var player in state.Players.Except(state.ActivePlayer))
            {
                foreach (var takenFlavor in state.Parfaits[player].Flavors.Keys)
                {
                    foreach (var givenFlavor in state.Parfaits[state.ActivePlayer].Flavors.Keys)
                    {
                        yield return new SwitchAScoopMove(state, takenFlavor, givenFlavor, player);
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var activePlayer = this.PlayerToken;
            var parfaits = state.Parfaits;

            parfaits = parfaits
                .SetItem(activePlayer, parfaits[activePlayer].With(
                    flavors: parfaits[activePlayer].Flavors.Add(this.TakenFlavor).Remove(this.GivenFlavor)))
                .SetItem(this.OtherPlayer, parfaits[this.OtherPlayer].With(
                    flavors: parfaits[this.OtherPlayer].Flavors.Add(this.GivenFlavor).Remove(this.TakenFlavor)));

            return base.Apply(state.With(
                phase: Phase.Play,
                parfaits: parfaits));
        }
    }
}
