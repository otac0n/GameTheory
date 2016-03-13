// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Each time your Assassins kill: a Merchant, draw 1 Resource card from the top of the Resource pile; a Builder, take the GCs that Builder would have taken; a Vizier or Elder, place it in front of you instead of killing it.
    /// </summary>
    public class Kandicha : Djinn
    {
        /// <summary>
        /// The singleton instance of <see cref="Kandicha"/>.
        /// </summary>
        public static readonly Kandicha Instance = new Kandicha();

        private Kandicha()
            : base(6)
        {
        }

        /// <inheritdoc />
        public override GameState HandleAssassination(PlayerToken owner, GameState state0, Point point, EnumCollection<Meeple> kill)
        {
            var s1 = state0;

            foreach (var meeple in kill.Keys)
            {
                var inventory = s1.Inventory[owner];

                switch (meeple)
                {
                    case Meeple.Vizier:
                    case Meeple.Elder:
                        {
                            s1 = s1.With(
                                bag: s1.Bag.Remove(meeple, kill[meeple]),
                                inventory: s1.Inventory.SetItem(owner, inventory.With(meeples: inventory.Meeples.Add(meeple, kill[meeple]))));
                            break;
                        }

                    case Meeple.Builder:
                        {
                            var blueTiles = Sultanate.GetSquarePoints(point).Count(p => s1.Sultanate[p].Tile.Color == TileColor.Blue);
                            var score = kill[meeple] * blueTiles * s1.ScoreTables[owner].BuilderMultiplier;

                            s1 = s1.With(
                                inventory: s1.Inventory.SetItem(owner, inventory.With(goldCoins: inventory.GoldCoins + score)));

                            break;
                        }

                    case Meeple.Merchant:
                        {
                            ImmutableList<Resource> dealt;
                            var newDiscards = s1.ResourceDiscards;
                            var newResourcesPile = s1.ResourcePile.Deal(kill[meeple], out dealt, ref newDiscards);
                            var newInventory = inventory.With(resources: inventory.Resources.AddRange(dealt));

                            s1 = s1.With(
                                inventory: s1.Inventory.SetItem(owner, newInventory),
                                resourceDiscards: newDiscards,
                                resourcePile: newResourcesPile);

                            break;
                        }
                }
            }

            return s1;
        }

        /// <inheritdoc />
        public override GameState HandleAssassination(PlayerToken owner, GameState state0, PlayerToken victim, EnumCollection<Meeple> kill)
        {
            return this.HandleAssassination(owner, state0, default(Point), kill);
        }
    }
}
