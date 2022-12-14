// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame.Console
{
    using System;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Positively Perfect Parfait Game</see>.
    /// </summary>
    public class PositivelyPerfectParfaitGameConsoleRenderer : ConsoleRendererBase<GameState, Move>
    {
        /// <inheritdoc />
        public override void Show(GameState state, PlayerToken playerToken = null) => new Templates().RenderGameState(state, this.MakeRenderTokenWriter(state));

        /// <inheritdoc/>
        protected override void RenderToken(GameState state, object token)
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
                    Console.Write(Resources.ResourceManager.GetEnumString(flavor));
                });
            }
            else if (token == Templates.CherrySentinel)
            {
                ConsoleInteraction.WithColor(ConsoleColor.Red, () =>
                {
                    Console.Write(Resources.Cherry);
                });
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
