using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameTheory.Games.Nessos.Moves
{
    public sealed class AcceptOfferMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptOfferMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public AcceptOfferMove(GameState state)
            : base(state, state.TargetPlayer)
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new[] { Resources.AcceptOffer };

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            if (state.OfferedCards.Count > 0)
            {
                yield return new AcceptOfferMove(state);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerToken = this.PlayerToken;
            var inventory = state.Inventory[playerToken];
            inventory = inventory.With(
                ownedCards: inventory.OwnedCards.AddRange(state.OfferedCards.Select(o => o.ActualCard)));
            state = state.With(
                inventory: state.Inventory.SetItem(playerToken, inventory),
                offeredCards: ImmutableList<OfferedCard>.Empty,
                targetPlayer: new Maybe<PlayerToken>(null),
                phase: Phase.Draw);
            return base.Apply(state);
        }
    }
}
