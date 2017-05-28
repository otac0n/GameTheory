namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    internal interface IConsoleRenderer<TMove>
        where TMove : IMove
    {
        void Show(PlayerToken playerToken, IGameState<TMove> state);
    }
}