﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderOrOneSlave"/> so that when placing a Palace, you may drop it on any neighboring Tile instead.
    /// </summary>
    public sealed class Hagis : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="Hagis"/>.
        /// </summary>
        public static readonly Hagis Instance = new Hagis();

        private readonly string stateKey;

        private Hagis()
            : base(10)
        {
            this.stateKey = this.GetType().Name + "Used";
        }

        /// <inheritdoc />
        public override string Name => Resources.Hagis;

        /// <inheritdoc />
        public override IEnumerable<Move> GetAdditionalMoves(GameState state, IList<Move> moves)
        {
            if (state.Phase != Phase.End && state[this.stateKey] == null && state.Inventory[state.ActivePlayer].Djinns.Contains(this))
            {
                foreach (var move in moves.OfType<PlacePalaceMove>())
                {
                    var newMoves = Cost.OneElderOrOneSlave(state, s1 => s1.WithState(this.stateKey, "true").WithInterstitialState(new Paid(move.Point, move.Phase)));

                    foreach (var m in newMoves)
                    {
                        yield return m;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            ArgumentNullException.ThrowIfNull(oldState);
            ArgumentNullException.ThrowIfNull(newState);

            if (oldState.Phase == Phase.MerchandiseSale && newState.Phase == Phase.Bid && newState[this.stateKey] != null)
            {
                newState = newState.WithState(this.stateKey, null);
            }

            return newState;
        }

        private class Paid : InterstitialState
        {
            private readonly Phase? phase;
            private readonly Point point;

            public Paid(Point point, Phase? phase)
            {
                this.point = point;
                this.phase = phase;
            }

            public override int CompareTo(InterstitialState other)
            {
                if (other is Paid p)
                {
                    return this.point.CompareTo(p.point);
                }
                else
                {
                    return base.CompareTo(other);
                }
            }

            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                foreach (var point in Sultanate.GetSquarePoints(this.point))
                {
                    yield return new PlacePalaceMove(state, point, this.phase);
                }
            }
        }
    }
}
