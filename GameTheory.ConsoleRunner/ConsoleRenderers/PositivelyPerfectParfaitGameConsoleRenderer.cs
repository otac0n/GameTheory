// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using Games.PositivelyPerfectParfaitGame;
    using PositivelyPerfectParfaitGame;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Positively Perfect Parfait Game</see>.
    /// </summary>
    public class PositivelyPerfectParfaitGameConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken)
        {
            new Templates().RenderGameState((GameState)state, this.MakeRenderTokenWriter(state));
        }

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is Flavor flavor)
            {
                var color = ConsoleColor.White;
                switch (flavor)
                {
                    case Flavor.Chocolate:
                        color = ConsoleColor.DarkYellow;
                        break;

                    case Flavor.FrenchVanilla:
                        color = ConsoleColor.White;
                        break;

                    case Flavor.Mint:
                        color = ConsoleColor.Green;
                        break;

                    case Flavor.Strawberry:
                        color = ConsoleColor.Magenta;
                        break;
                }

                ConsoleInteraction.WithColor(color, () =>
                {
                    Console.Write(token);
                });
            }
            else if (token == Templates.CherrySentinel)
            {
                ConsoleInteraction.WithColor(ConsoleColor.Red, () =>
                {
                    Console.Write("Cherry");
                });
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
