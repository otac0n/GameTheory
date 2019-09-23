// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Games.Ergo.Cards;

    /// <summary>
    /// Represents the current state in a game of Ergo.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        /// <summary>
        /// The number of actions played per turn.
        /// </summary>
        public const int ActionsPerTurn = 2;

        /// <summary>
        /// The number of turns a fallacy is in effect.
        /// </summary>
        public const int FallacyTurns = 3;

        /// <summary>
        /// The initial size of a player's hand.
        /// </summary>
        public const int InitialHandSize = 5;

        /// <summary>
        /// The maximum number of supported players.
        /// </summary>
        public const int MaxPlayers = 4;

        /// <summary>
        /// The minimum number of supported players.
        /// </summary>
        public const int MinPlayers = 2;

        /// <summary>
        /// The number of premise lines in the proof.
        /// </summary>
        public const int PremiseLines = 4;

        /// <summary>
        /// The target number of points.
        /// </summary>
        public const int TargetPoints = 50;

        static GameState()
        {
            StartingDeck = ImmutableList.Create(new Card[]
            {
                SymbolCard.VariableA, SymbolCard.VariableA, SymbolCard.VariableA, SymbolCard.VariableA,
                SymbolCard.VariableB, SymbolCard.VariableB, SymbolCard.VariableB, SymbolCard.VariableB,
                SymbolCard.VariableC, SymbolCard.VariableC, SymbolCard.VariableC, SymbolCard.VariableC,
                SymbolCard.VariableD, SymbolCard.VariableD, SymbolCard.VariableD, SymbolCard.VariableD,
                SymbolCard.And, SymbolCard.And, SymbolCard.And, SymbolCard.And,
                SymbolCard.Or, SymbolCard.Or, SymbolCard.Or, SymbolCard.Or,
                SymbolCard.Then, SymbolCard.Then, SymbolCard.Then, SymbolCard.Then,
                SymbolCard.Not, SymbolCard.Not, SymbolCard.Not, SymbolCard.Not, SymbolCard.Not, SymbolCard.Not,
                SymbolCard.Parenthesis, SymbolCard.Parenthesis, SymbolCard.Parenthesis, SymbolCard.Parenthesis,
                SymbolCard.Parenthesis, SymbolCard.Parenthesis, SymbolCard.Parenthesis, SymbolCard.Parenthesis,
                FallacyCard.Instance, FallacyCard.Instance, FallacyCard.Instance,
                JustificationCard.Instance, JustificationCard.Instance, JustificationCard.Instance,
                ErgoCard.Instance, ErgoCard.Instance, ErgoCard.Instance,
                TabulaRasaCard.Instance,
                RevolutionCard.Instance,
                SymbolCard.WildVariable,
                SymbolCard.WildOperator,
            });

            StartingProof = Enumerable.Range(0, PremiseLines).Select(i => ImmutableList<PlacedCard>.Empty).ToImmutableList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="players">The number of players.</param>
        public GameState(int players = MinPlayers)
        {
            if (players < MinPlayers || players > MaxPlayers)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            this.Players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableArray();
            this.Dealer = this.Players.Last();
            this.ActivePlayer = this.Dealer;
            this.Phase = Phase.Deal;
            this.RemainingActions = ActionsPerTurn;
            this.IsRoundOver = false;
            this.Deck = ImmutableList<Card>.Empty;
            this.Scores = this.Players.ToImmutableDictionary(p => p, p => 0);
            this.Hands = this.Players.ToImmutableDictionary(p => p, p => ImmutableList<Card>.Empty);
            this.FallacyCounter = this.Players.ToImmutableDictionary(p => p, p => 0);
            this.Proof = StartingProof;
            this.IsProofValid = true;
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            Phase phase,
            int remainingActions,
            PlayerToken activePlayer,
            PlayerToken dealer,
            ImmutableList<Card> deck,
            ImmutableDictionary<PlayerToken, int> scores,
            ImmutableDictionary<PlayerToken, ImmutableList<Card>> hands,
            ImmutableDictionary<PlayerToken, int> fallacyCounter,
            ImmutableList<ImmutableList<PlacedCard>> proof,
            bool isRoundOver,
            bool isProofValid)
        {
            this.Players = players;
            this.Phase = phase;
            this.RemainingActions = remainingActions;
            this.ActivePlayer = activePlayer;
            this.Dealer = dealer;
            this.Deck = deck;
            this.Scores = scores;
            this.Hands = hands;
            this.FallacyCounter = fallacyCounter;
            this.Proof = proof;
            this.IsRoundOver = isRoundOver;
            this.IsProofValid = isProofValid;
        }

        /// <summary>
        /// Gets the starting deck.
        /// </summary>
        public static ImmutableList<Card> StartingDeck { get; }

        /// <summary>
        /// Gets the starting proof.
        /// </summary>
        public static ImmutableList<ImmutableList<PlacedCard>> StartingProof { get; }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

        /// <summary>
        /// Gets the dealer.
        /// </summary>
        public PlayerToken Dealer { get; }

        /// <summary>
        /// Gets the deck.
        /// </summary>
        public ImmutableList<Card> Deck { get; }

        /// <summary>
        /// Gets the fallacy counters.
        /// </summary>
        public ImmutableDictionary<PlayerToken, int> FallacyCounter { get; }

        /// <summary>
        /// Gets the player's hands.
        /// </summary>
        public ImmutableDictionary<PlayerToken, ImmutableList<Card>> Hands { get; }

        /// <summary>
        /// Gets a value indicating whether or not the round is over.
        /// </summary>
        public bool IsRoundOver { get; }

        /// <summary>
        /// Gets the current phase of the game.
        /// </summary>
        public Phase Phase { get; }

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableArray<PlayerToken> Players { get; }

        /// <inheritdoc />
        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the proof.
        /// </summary>
        public ImmutableList<ImmutableList<PlacedCard>> Proof { get; }

        /// <summary>
        /// Gets the active players remaining actions.
        /// </summary>
        public int RemainingActions { get; }

        /// <summary>
        /// Gets the player's scores.
        /// </summary>
        public ImmutableDictionary<PlayerToken, int> Scores { get; }

        internal bool IsProofValid { get; }

        /// <inheritdoc/>
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
                (comp = this.RemainingActions.CompareTo(state.RemainingActions)) != 0 ||
                (comp = this.IsRoundOver.CompareTo(state.IsRoundOver)) != 0 ||
                (comp = this.ActivePlayer.CompareTo(state.ActivePlayer)) != 0 ||
                (comp = this.Dealer.CompareTo(state.Dealer)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Deck, state.Deck)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0)
            {
                return comp;
            }

            for (var i = 0; i < StartingProof.Count; i++)
            {
                if ((comp = CompareUtilities.CompareLists(this.Proof[i], state.Proof[i])) != 0)
                {
                    return comp;
                }
            }

            foreach (var player in this.Players)
            {
                if ((comp = this.Scores[player].CompareTo(state.Scores[player])) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.Hands[player], state.Hands[player])) != 0)
                {
                    return comp;
                }
            }

            return 0;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            var moves = new List<Move>();

            switch (this.Phase)
            {
                case Phase.Deal:
                    moves.AddRange(Moves.DealMove.GenerateMoves(this));
                    break;

                case Phase.Draw:
                    moves.AddRange(Moves.DrawCardsMove.GenerateMoves(this));
                    break;

                case Phase.Play:
                    moves.AddRange(GetContinuationMoves(this));
                    moves.RemoveAll(m => !HasLegalContinuations(this, this.ActivePlayer, m));

                    if (this.IsProofValid)
                    {
                        moves.AddRange(Moves.PlayFallacyMove.GenerateMoves(this));
                        moves.AddRange(Moves.PlayErgoMove.GenerateMoves(this));
                        moves.AddRange(Moves.PlayJustificationMove.GenerateMoves(this));
                        moves.AddRange(Moves.DiscardMove.GenerateMoves(this));
                    }

                    break;
            }

            return moves.ToImmutableList();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            var shuffler = new GameShuffler<GameState>(this);

            shuffler.AddCollection(
                "Cards",
                this.Deck,
                (state, value) => state.With(
                    deck: value.ToImmutableList()));

            foreach (var p in this.Players)
            {
                var player = p;
                if (player == playerToken)
                {
                    continue;
                }

                shuffler.AddCollection(
                    "Cards",
                    this.Hands[player],
                    (state, value) => state.With(
                        hands: state.Hands.SetItem(player, value.ToImmutableList())));
            }

            return shuffler.Take(maxStates);
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.Phase != Phase.End)
            {
                return ImmutableList<PlayerToken>.Empty;
            }

            return this.Players.AllMaxBy(p => this.Scores[p]).ToImmutableList();
        }

        /// <inheritdoc />
        IGameState<Move> IGameState<Move>.MakeMove(Move move) => this.MakeMove(move);

        /// <summary>
        /// Applies the move to the current game state.
        /// </summary>
        /// <param name="move">The <see cref="Move"/> to apply.</param>
        /// <returns>The updated <see cref="GameState"/>.</returns>
        public GameState MakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (this.CompareTo(move.GameState) != 0)
            {
                var equivalentMove = this.GetAvailableMoves().Where(m => m.CompareTo(move) == 0).FirstOrDefault();
                if (equivalentMove != null)
                {
                    move = equivalentMove;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(move));
                }
            }

            return move.Apply(this);
        }

        internal GameState With(
            Phase? phase = null,
            int? remainingActions = null,
            PlayerToken activePlayer = null,
            PlayerToken dealer = null,
            ImmutableList<Card> deck = null,
            ImmutableDictionary<PlayerToken, int> scores = null,
            ImmutableDictionary<PlayerToken, ImmutableList<Card>> hands = null,
            ImmutableDictionary<PlayerToken, int> fallacyCounter = null,
            ImmutableList<ImmutableList<PlacedCard>> proof = null,
            bool? isRoundOver = null)
        {
            return new GameState(
                this.Players,
                phase ?? this.Phase,
                remainingActions ?? this.RemainingActions,
                activePlayer ?? this.ActivePlayer,
                dealer ?? this.Dealer,
                deck ?? this.Deck,
                scores ?? this.Scores,
                hands ?? this.Hands,
                fallacyCounter ?? this.FallacyCounter,
                proof ?? this.Proof,
                isRoundOver ?? this.IsRoundOver,
                proof == null ? this.IsProofValid : Compiler.IsValid(proof));
        }

        private static IEnumerable<Move> GetContinuationMoves(GameState state)
        {
            foreach (var move in Moves.PlaceSymbolMove.GenerateMoves(state))
            {
                yield return move;
            }

            foreach (var move in Moves.PlayTabulaRasaMove.GenerateMoves(state))
            {
                yield return move;
            }

            foreach (var move in Moves.PlayRevolutionMove.GenerateMoves(state))
            {
                yield return move;
            }
        }

        private static bool HasLegalContinuations(GameState state, PlayerToken player, Move move)
        {
            state = state.MakeMove(move);
            var valid = state.IsProofValid;

            if (!valid && state.ActivePlayer == player)
            {
                valid = GetContinuationMoves(state).Any(m => HasLegalContinuations(state, player, m));
            }

            return valid;
        }
    }
}
