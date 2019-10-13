using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override IList<object> FormatTokens => throw new NotImplementedException();
    }
}
