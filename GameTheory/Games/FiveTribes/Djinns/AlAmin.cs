// -----------------------------------------------------------------------
// <copyright file="AlAmin.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// At game end, each pair of Slaves you hold acts as 1 Wild Merchandise of your choice.
    /// </summary>
    public class AlAmin : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="AlAmin"/>.
        /// </summary>
        public static readonly AlAmin Instance = new AlAmin();

        private AlAmin()
            : base(5)
        {
        }

        /// <inheritdoc />
        public override string Name
        {
            get { return "Al-Amin"; }
        }

        /// <inheritdoc />
        public override IEnumerable<Move> GetMoves(GameState state0)
        {
            var owner = state0.Players.SingleOrDefault(p => state0.Inventory[p].Djinns.Contains(this));
            if (state0.Phase == Phase.End && owner != null)
            {
                if (state0.Inventory[owner].Resources[Resource.Slave] >= 2)
                {
                    foreach (var wild in Enum.GetValues(typeof(Resource)).Cast<Resource>().Except(Resource.Slave))
                    {
                        yield return new TradeSlavesForResourceMove(state0, owner, wild);
                    }
                }
            }
        }

        /// <summary>
        /// Represents a move to trade a pair of <see cref="Resource.Slave">Slaves</see> for the specified <see cref="Resource"/>.
        /// </summary>
        public class TradeSlavesForResourceMove : Move
        {
            private readonly Resource resource;

            /// <summary>
            /// Initializes a new instance of the <see cref="TradeSlavesForResourceMove"/> class.
            /// </summary>
            /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
            /// <param name="owner">The player who may perform this move.</param>
            /// <param name="resource">The <see cref="Resource"/> that will received by the player.</param>
            public TradeSlavesForResourceMove(GameState state0, PlayerToken owner, Resource resource)
                : base(state0, owner)
            {
                this.resource = resource;
            }

            /// <summary>
            /// Gets the <see cref="Resource"/> that will be received by the player.
            /// </summary>
            public Resource Resource
            {
                get { return this.resource; }
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return string.Format("Trade {0},{0} for {1}", Resource.Slave, this.resource);
            }

            internal override GameState Apply(GameState state0)
            {
                var inventory = state0.Inventory[this.Player];
                return state0.With(
                    inventory: state0.Inventory.SetItem(this.Player, inventory.With(resources: inventory.Resources.Remove(Resource.Slave, 2).Add(this.resource))));
            }
        }
    }
}
