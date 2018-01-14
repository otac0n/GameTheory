﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Moves
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to add a guardian to a flower.
    /// </summary>
    public sealed class MoveGuardianMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoveGuardianMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="flowerType">The <see cref="FlowerType">flower</see> that will receive the guardian.</param>
        public MoveGuardianMove(GameState state, FlowerType flowerType)
            : base(state)
        {
            this.FlowerType = flowerType;
        }

        /// <summary>
        /// Gets the <see cref="FlowerType">flower</see> that will receive the guardian.
        /// </summary>
        public FlowerType FlowerType { get; }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Move a guardian to the ", this.FlowerType };

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is MoveGuardianMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.FlowerType.CompareTo(move.FlowerType)) != 0)
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

        internal static IEnumerable<MoveGuardianMove> GenerateMoves(GameState state)
        {
            if (state.Inventory[state.ActivePlayer].Guardians > 0)
            {
                foreach (FlowerType flowerType in Enum.GetValues(typeof(FlowerType)))
                {
                    if (state.Field[flowerType].Petals.Count > 0)
                    {
                        yield return new MoveGuardianMove(state, flowerType);
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];
            var flower = state.Field[this.FlowerType];

            state = state.With(
                field: state.Field.SetItem(
                    this.FlowerType,
                    flower.With(
                        guardians: flower.Guardians.Add(this.PlayerToken))),
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory.With(
                        guardians: playerInventory.Guardians - 1)));

            return base.Apply(state);
        }
    }
}
