using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameTheory.Games.Nessos.Moves
{
    public sealed class RejectOfferMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RejectOfferMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public RejectOfferMove(GameState state)
            : base(state, state.TargetPlayer)
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new[] { Resources.RejectOffer };

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is RejectOfferMove move)
            {
                return this.PlayerToken.CompareTo(move.PlayerToken);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            if (state.OfferedCards.Count > 0)
            {
                yield return new RejectOfferMove(state);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerToken = state.OfferedCards.Last().SourcePlayer;
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
