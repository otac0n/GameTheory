﻿// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

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
        public override IEnumerable<Move> GetMoves(GameState state)
        {
            var owner = state.Players.SingleOrDefault(p => state.Inventory[p].Djinns.Contains(this));
            if (state.Phase == Phase.End && owner != null)
            {
                if (state.Inventory[owner].Resources[Resource.Slave] >= 2)
                {
                    foreach (var wild in Enum.GetValues(typeof(Resource)).Cast<Resource>().Except(Resource.Slave))
                    {
                        yield return new TradeSlavesForResourceMove(state, owner, wild);
                    }
                }
            }
        }
    }
}
