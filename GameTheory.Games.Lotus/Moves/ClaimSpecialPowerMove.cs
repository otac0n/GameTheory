// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to claim a special power.
    /// </summary>
    public sealed class ClaimSpecialPowerMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimSpecialPowerMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        /// <param name="specialPower">The <see cref="Lotus.SpecialPowers"/> to be clamed.</param>
        public ClaimSpecialPowerMove(GameState state, PlayerToken player, SpecialPowers specialPower)
            : base(state, player)
        {
            this.SpecialPower = specialPower;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens =>
            FormatUtilities.ParseStringFormat(Resources.ClaimSpecialPowerFormat, this.SpecialPower);

        /// <summary>
        /// Gets the <see cref="Lotus.SpecialPowers"/> to be clamed.
        /// </summary>
        public SpecialPowers SpecialPower { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is ClaimSpecialPowerMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = EnumComparer<SpecialPowers>.Default.Compare(this.SpecialPower, move.SpecialPower)) != 0)
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

        internal static IEnumerable<ClaimSpecialPowerMove> GenerateMoves(GameState state)
        {
            foreach (var player in state.ChoosingPlayers.Take(1))
            {
                foreach (var specialPower in EnumUtilities<SpecialPowers>.GetValues())
                {
                    if (specialPower != SpecialPowers.None && (state.Inventory[player].SpecialPowers & specialPower) != specialPower)
                    {
                        yield return new ClaimSpecialPowerMove(state, player, specialPower);
                    }
                }
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
                        specialPowers: playerInventory.SpecialPowers | this.SpecialPower)),
                choosingPlayers: choosingPlayers,
                phase: choosingPlayers.Count != 0 ? Phase.ClaimReward : Phase.Play);

            return base.Apply(state);
        }
    }
}
