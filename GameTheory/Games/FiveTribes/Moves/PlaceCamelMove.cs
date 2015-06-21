namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PlaceCamelMove : Move
    {
        private readonly Point point;

        public PlaceCamelMove(GameState state0, Point point)
            : this(state0, point, s => s)
        {
        }

        public PlaceCamelMove(GameState state0, Point point, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var owner = s1.ActivePlayer;

                return after(s1.With(
                    sultanate: s1.Sultanate.SetItem(point, s1.Sultanate[point].With(owner: owner))));
            })
        {
            this.point = point;
        }

        public Point Point
        {
            get { return this.point; }
        }

        public override string ToString()
        {
            return string.Format("Place a Camel at {0}", this.point);
        }
    }
}
