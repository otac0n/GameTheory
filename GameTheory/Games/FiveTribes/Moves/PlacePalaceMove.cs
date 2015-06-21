namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    public class PlacePalaceMove : Move
    {
        private Func<GameState, GameState> after;
        private Point point;

        public PlacePalaceMove(GameState state0, Point point)
            : this(state0, point, s => s)
        {
        }

        public PlacePalaceMove(GameState state0, Point point, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer, s1 =>
            {
                var square = s1.Sultanate[point];
                return after(s1.With(
                    sultanate: s1.Sultanate.SetItem(point, square.With(palaces: square.Palaces + 1))));
            })
        {
            this.point = point;
            this.after = after;
        }

        public Point Point
        {
            get { return this.point; }
        }

        public override string ToString()
        {
            return string.Format("Place a Palace at {0}", this.point);
        }

        internal Move With(GameState state, Point point)
        {
            return new PlacePalmTreeMove(state, point, this.after);
        }
    }
}
