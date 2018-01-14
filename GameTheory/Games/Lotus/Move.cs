// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in Lotus.
    /// </summary>
    public abstract class Move : IMove, IComparable<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        protected Move(GameState state)
        {
            this.State = state ?? throw new ArgumentNullException(nameof(state));
            this.PlayerToken = state.ActivePlayer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        protected Move(GameState state, PlayerToken player)
        {
            this.State = state ?? throw new ArgumentNullException(nameof(state));
            this.PlayerToken = player;
        }

        /// <inheritdoc />
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc />
        public bool IsDeterministic => true;

        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
        public PlayerToken PlayerToken { get; }

        internal GameState State { get; }

        /// <inheritdoc />
        public virtual int CompareTo(Move other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            return this.GetType().Name.CompareTo(other.GetType().Name);
        }

        /// <inheritdoc />
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal virtual GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var activePlayerInventory = state.Inventory[activePlayer];

            if (state.Phase == Phase.Play)
            {
                foreach (FlowerType flowerType in Enum.GetValues(typeof(FlowerType)))
                {
                    var flower = state.Field[flowerType];
                    if (flower.Petals.Count == (int)flowerType)
                    {
                        var controllingPlayers = GameState.GetControllingPlayers(flower);

                        var inventory = state.Inventory.SetItem(
                            activePlayer,
                            activePlayerInventory.With(
                                scoringPile: activePlayerInventory.ScoringPile.AddRange(flower.Petals)));

                        foreach (var guardian in flower.Guardians)
                        {
                            var playerInventory = inventory[guardian];
                            inventory = inventory.SetItem(
                                guardian,
                                playerInventory.With(
                                    playerInventory.Guardians + 1));
                        }

                        return state.With(
                            inventory: inventory,
                            field: state.Field.SetItem(
                                flowerType,
                                new Flower()),
                            choosingPlayers: controllingPlayers,
                            phase: Phase.ClaimReward);
                    }
                }

                int remainingActions;
                if (activePlayerInventory.Guardians > 0 || activePlayerInventory.Hand.Count > 0)
                {
                    // Continue playing.
                    remainingActions = state.RemainingActions - 1;
                }
                else
                {
                    remainingActions = 0;
                }

                state = state.With(
                    remainingActions: remainingActions);

                if (state.RemainingActions == 0)
                {
                    if (activePlayerInventory.Deck.Count == 0)
                    {
                        foreach (FlowerType flowerType in Enum.GetValues(typeof(FlowerType)))
                        {
                            var inventory = state.Inventory;
                            var flower = state.Field[flowerType];
                            var petals = flower.Petals;

                            if (petals.Count > 0)
                            {
                                var controllingPlayers = GameState.GetControllingPlayers(flower);
                                var petalsPerPlayer = controllingPlayers.Count == 0
                                    ? 0
                                    : petals.Count / controllingPlayers.Count;

                                if (petalsPerPlayer > 0)
                                {
                                    foreach (var player in controllingPlayers)
                                    {
                                        var playerInventory = inventory[player];

                                        inventory = inventory.SetItem(
                                            player,
                                            playerInventory.With(
                                                scoringPile: playerInventory.ScoringPile.AddRange(petals.Take(petalsPerPlayer))));

                                        petals = petals.RemoveRange(0, petalsPerPlayer);
                                    }

                                    flower = flower.With(
                                        petals: petals);

                                    state = state.With(
                                        inventory: inventory,
                                        field: state.Field.SetItem(
                                            flowerType,
                                            flower));
                                }
                            }

                            if (flower.Guardians.Count > 0)
                            {
                                foreach (var guardian in flower.Guardians)
                                {
                                    var playerInventory = inventory[guardian];
                                    inventory = inventory.SetItem(
                                        guardian,
                                        playerInventory.With(
                                            playerInventory.Guardians + 1));
                                }

                                flower = flower.With(
                                    guardians: flower.Guardians.Clear());

                                state = state.With(
                                    inventory: inventory,
                                    field: state.Field.SetItem(
                                        flowerType,
                                        flower));
                            }
                        }

                        state = state.With(
                            phase: Phase.End);
                    }
                    else
                    {
                        state = state.With(
                            phase: Phase.Draw);
                    }
                }
            }

            if (state.Phase == Phase.Draw)
            {
                if ((activePlayerInventory.Deck.Count > 0 || state.AvailableWildflowers.Any(c => c != null)) && activePlayerInventory.Hand.Count < Inventory.StartingHandCount + (activePlayerInventory.SpecialPowers.HasFlag(SpecialPower.EnlightenedPath) ? 1 : 0))
                {
                    // Continue drawing.
                }
                else
                {
                    for (var i = 0; state.WildflowerDeck.Count > 0 && i < state.AvailableWildflowers.Count; i++)
                    {
                        if (state.AvailableWildflowers[i] == null)
                        {
                            var dealtIndex = state.WildflowerDeck.Count - 1;
                            var dealt = state.WildflowerDeck[dealtIndex];
                            state = state.With(
                                wildflowerDeck: state.WildflowerDeck.RemoveAt(dealtIndex),
                                availableWildflowers: state.AvailableWildflowers.SetItem(
                                    i,
                                    dealt));
                        }
                    }

                    state = state.With(
                        activePlayer: state.Players[(state.Players.IndexOf(activePlayer) + 1) % state.Players.Count],
                        remainingActions: GameState.ActionsPerTurn,
                        phase: Phase.Play);
                }
            }

            return state;
        }
    }
}
