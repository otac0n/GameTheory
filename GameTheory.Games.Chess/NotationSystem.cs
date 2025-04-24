// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.Chess.Moves;

    public abstract class NotationSystem
    {
        public virtual string FormatString(Move move) =>
            string.Concat(
                this.Format(move).Select(token =>
                token switch
                {
                    Pieces p => this.Format(p),
                    _ => token,
                }));

        public virtual IList<object> Format(Move move)
        {
            if (move is EnPassantCaptureMove enPassantCapture)
            {
                return this.FormatEnPassantCapture(enPassantCapture);
            }
            else if (move is PromotionMove promotion)
            {
                return this.FormatPromotion(promotion);
            }
            else if (move is BasicMove basicMove)
            {
                return this.FormatBasicMove(basicMove);
            }
            else if (move is CastleMove castle)
            {
                return this.FormatCastle(castle);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public abstract string Format(Pieces piece);

        public virtual IList<object> FormatBasicMove(BasicMove basicMove)
        {
            if (basicMove.GameState[basicMove.ToIndex] != Pieces.None)
            {
                return this.FormatCapture(basicMove);
            }
            else
            {
                return this.FormatMove(basicMove);
            }
        }

        public abstract IList<object> FormatCapture(BasicMove capture);

        public abstract IList<object> FormatCastle(CastleMove castle);

        public virtual IList<object> FormatEnPassantCapture(EnPassantCaptureMove enPassantCapture) => this.FormatCapture(enPassantCapture);

        public abstract IList<object> FormatMove(BasicMove capture);

        public abstract IList<object> FormatPromotion(PromotionMove promotion);
    }
}
