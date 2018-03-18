// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to reveal a card from another player's stack or all cards from the challengeing player's stack.
    /// </summary>
    public sealed class RevealCardsMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RevealCardsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="revealedPlayer">The player whose card or cards will be revealed.</param>
        public RevealCardsMove(GameState state, PlayerToken revealedPlayer)
            : base(state)
        {
            this.RevealedPlayer = revealedPlayer;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens =>
            this.GameState.ActivePlayer == this.RevealedPlayer
            ? new object[] { "Reveal all of your cards" }
            : new object[] { "Reveal a card from ", this.RevealedPlayer };

        /// <summary>
        /// Gets the player's bid.
        /// </summary>
        public PlayerToken RevealedPlayer { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is RevealCardsMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(move.PlayerToken)) != 0 ||
                    (comp = this.RevealedPlayer.CompareTo(move.RevealedPlayer)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Players, move.GameState.Players)) != 0)
                {
                    return comp;
                }

                foreach (var player in this.GameState.Players)
                {
                    var thisInventory = this.GameState.Inventory[player];
                    var moveInventory = move.GameState.Inventory[player];
                    if ((comp = thisInventory.Bid.CompareTo(moveInventory.Bid)) != 0 &&
                        (comp = thisInventory.Stack.Count.CompareTo(moveInventory.Stack.Count)) != 0)
                    {
                        return comp;
                    }
                }

                return 0;
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<RevealCardsMove> GenerateMoves(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            if (state.Inventory[activePlayer].Stack.Count >= 1)
            {
                yield return new RevealCardsMove(state, activePlayer);
            }
            else
            {
                foreach (var player in state.Players)
                {
                    if (player == activePlayer || state.Inventory[player].Stack.IsEmpty)
                    {
                        continue;
                    }

                    yield return new RevealCardsMove(state, player);
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var phase = state.Phase;
            var inventory = state.Inventory;
            var revealedInventory = inventory[this.RevealedPlayer];

            if (state.ActivePlayer == this.RevealedPlayer)
            {
                revealedInventory = revealedInventory.With(
                    revealed: new EnumCollection<Card>(revealedInventory.Stack),
                    stack: ImmutableList<Card>.Empty);
            }
            else
            {
                revealedInventory = revealedInventory.With(
                    revealed: revealedInventory.Revealed.Add(revealedInventory.Stack[revealedInventory.Stack.Count - 1]),
                    stack: revealedInventory.Stack.RemoveAt(revealedInventory.Stack.Count - 1));
            }

            inventory = inventory.SetItem(
                this.RevealedPlayer,
                revealedInventory);

            if (revealedInventory.Revealed[Card.Skull] > 0)
            {
                phase = Phase.ChooseDiscard;
            }
            else
            {
                var activePlayerInventory = inventory[state.ActivePlayer];
                if (inventory.Sum(kvp => kvp.Value.Revealed.Count) >= inventory[state.ActivePlayer].Bid)
                {
                    var challengesWon = activePlayerInventory.ChallengesWon + 1;
                    phase = challengesWon >= GameState.WinThreshold ? Phase.End : Phase.AddingCards;

                    inventory = inventory.SetItem(
                        state.ActivePlayer,
                        activePlayerInventory.With(
                            challengesWon: challengesWon));

                    inventory = inventory.SetItems(
                        from kvp in inventory
                        let playerInventory = kvp.Value
                        where playerInventory.Revealed.Count > 0 || playerInventory.Stack.Count > 0
                        let newHand = playerInventory.Hand.AddRange(playerInventory.Revealed).AddRange(playerInventory.Stack)
                        select new KeyValuePair<PlayerToken, Inventory>(kvp.Key, playerInventory.With(
                            bid: 0,
                            hand: newHand,
                            revealed: EnumCollection<Card>.Empty,
                            stack: ImmutableList<Card>.Empty)));
                }
            }

            state = state.With(
                phase: phase,
                inventory: inventory);

            return base.Apply(state);
        }
    }
}
