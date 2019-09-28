// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents the current state in a game of Positively Perfect Parfait Game.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        /// <summary>
        /// The maximum number of supported players.
        /// </summary>
        public const int MaxPlayers = 4;

        /// <summary>
        /// The minimum number of supported players.
        /// </summary>
        public const int MinPlayers = 2;

        private static readonly EnumCollection<Flavor> InitialScoops;

        static GameState()
        {
            InitialScoops = EnumCollection<Flavor>.Empty
                .Add(Flavor.Strawberry, 5)
                .Add(Flavor.Mint, 5)
                .Add(Flavor.Chocolate, 5)
                .Add(Flavor.FrenchVanilla, 5);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="players">The number of players.</param>
        /// <param name="playOut">A value indicating whether all players are allowed to complete their parfaits.</param>
        public GameState(int players = MinPlayers, bool playOut = false)
        {
            if (players < MinPlayers || players > MaxPlayers)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            this.PlayOut = playOut;
            this.Players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableArray();
            this.ActivePlayer = this.Players[0];
            this.Phase = Phase.Play;
            this.Parfaits = Enumerable.Range(0, players).ToImmutableDictionary(i => this.Players[i], i => Parfait.Empty);
            this.RemainingScoops = InitialScoops;
        }

        private GameState(
            bool playOut,
            ImmutableArray<PlayerToken> players,
            PlayerToken activePlayer,
            Phase phase,
            ImmutableDictionary<PlayerToken, Parfait> parfaits,
            EnumCollection<Flavor> remainingScoops)
        {
            this.PlayOut = playOut;
            this.Players = players;
            this.ActivePlayer = activePlayer;
            this.Phase = phase;
            this.Parfaits = parfaits;
            this.RemainingScoops = remainingScoops;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

        /// <summary>
        /// Gets the parfaits for all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, Parfait> Parfaits { get; }

        /// <summary>
        /// Gets the current phase of the game.
        /// </summary>
        public Phase Phase { get; }

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableArray<PlayerToken> Players { get; }

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets a value indicating whether all players are allowed to complete their parfaits.
        /// </summary>
        public bool PlayOut { get; }

        /// <summary>
        /// Gets the remaining scoops.
        /// </summary>
        public EnumCollection<Flavor> RemainingScoops { get; }

        /// <inheritdoc />
        public int CompareTo(IGameState<Move> other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            var state = other as GameState;
            if (object.ReferenceEquals(state, null))
            {
                return 1;
            }

            int comp;

            if ((comp = EnumComparer<Phase>.Default.Compare(this.Phase, state.Phase)) != 0 ||
                (comp = this.ActivePlayer.CompareTo(state.ActivePlayer)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0)
            {
                return comp;
            }

            if (this.Parfaits != state.Parfaits)
            {
                foreach (var player in this.Players)
                {
                    if ((comp = this.Parfaits[player].CompareTo(state.Parfaits[player])) != 0)
                    {
                        return comp;
                    }
                }
            }

            if (this.RemainingScoops != state.RemainingScoops)
            {
                if ((comp = this.RemainingScoops.CompareTo(state.RemainingScoops)) != 0)
                {
                    return comp;
                }
            }

            return 0;
        }

        /// <inheritdoc />
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            var moves = ImmutableList.CreateBuilder<Move>();

            switch (this.Phase)
            {
                case Phase.Play:
                    moves.AddRange(Moves.SpinMove.GenerateMoves(this));
                    break;

                case Phase.ChooseAFlavor:
                    moves.AddRange(Moves.ChooseAFlavorMove.GenerateMoves(this));
                    moves.AddRange(Moves.TakeACherryMove.GenerateMoves(this));
                    break;

                case Phase.SwitchAScoop:
                    moves.AddRange(Moves.SwitchAScoopMove.GenerateMoves(this));
                    break;

                case Phase.Oops:
                    moves.AddRange(Moves.LoseAFlavorMove.GenerateMoves(this));
                    break;

                case Phase.End:
                    break;
            }

            return moves.ToImmutable();
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            if (move.IsDeterministic)
            {
                yield return Weighted.Create(this.MakeMove(move), 1);
                yield break;
            }

            foreach (var outcome in move.GetOutcomes(this))
            {
                yield return outcome;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            yield return this;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.Phase != Phase.End)
            {
                return ImmutableList<PlayerToken>.Empty;
            }

            return this.Players
                .Where(p => this.Parfaits[p].Cherry)
                .ToImmutableList();
        }

        /// <inheritdoc />
        public IGameState<Move> MakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (this.CompareTo(move.GameState) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(move));
            }

            return move.Apply(this);
        }

        internal GameState With(
            PlayerToken activePlayer = null,
            Phase? phase = null,
            ImmutableDictionary<PlayerToken, Parfait> parfaits = null,
            EnumCollection<Flavor> remainingScoops = null)
        {
            return new GameState(
                this.PlayOut,
                this.Players,
                activePlayer ?? this.ActivePlayer,
                phase ?? this.Phase,
                parfaits ?? this.Parfaits,
                remainingScoops ?? this.RemainingScoops);
        }
    }
}
