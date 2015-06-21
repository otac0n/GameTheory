namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class AssassinateMove : Move
    {
        private readonly EnumCollection<Meeple> meeples;
        private readonly Point point;

        public AssassinateMove(GameState state0, Point point, EnumCollection<Meeple> meeples, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var square = s1.Sultanate[point];
                var newSquare = square.With(meeples: square.Meeples.RemoveRange(meeples));
                var newState = s1.With(
                    bag: s1.Bag.AddRange(s1.InHand).AddRange(meeples),
                    sultanate: s1.Sultanate.SetItem(point, newSquare),
                    inHand: EnumCollection<Meeple>.Empty);

                foreach (var owner in newState.Players)
                {
                    foreach (var djinn in newState.Inventory[owner].Djinns)
                    {
                        newState = djinn.HandleAssassination(owner, newState, point, meeples);
                    }
                }

                if (newSquare.Meeples.Count == 0 && newSquare.Owner == null && newState.IsPlayerUnderCamelLimit(s1.ActivePlayer))
                {
                    return newState.WithMoves(t => new PlaceCamelMove(t, point, after));
                }
                else
                {
                    return after(newState);
                }
            })
        {
            this.point = point;
            this.meeples = meeples;
        }

        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        public Point Point
        {
            get { return this.point; }
        }

        public override string ToString()
        {
            return string.Format("Assissinate {0} at {1}", string.Join(",", this.meeples), this.point);
        }
    }
}
