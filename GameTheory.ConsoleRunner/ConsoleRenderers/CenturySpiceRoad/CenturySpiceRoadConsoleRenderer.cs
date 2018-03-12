// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers.CenturySpiceRoad
{
    using System;
    using Games.CenturySpiceRoad;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">CenturySpiceRoad</see>.
    /// </summary>
    public class CenturySpiceRoadConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken)
        {
            new Templates(playerToken).RenderGameState((GameState)state, this.MakeRenderTokenWriter(state));
        }

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is Spice spice)
            {
                var color = ConsoleColor.White;
                switch (spice)
                {
                    case Spice.Turmeric:
                        color = ConsoleColor.Yellow;
                        break;

                    case Spice.Saffron:
                        color = ConsoleColor.Red;
                        break;

                    case Spice.Cardamom:
                        color = ConsoleColor.Green;
                        break;

                    case Spice.Cinnamon:
                        color = ConsoleColor.DarkRed;
                        break;
                }

                ConsoleInteraction.WithColor(color, () =>
                {
                    Console.Write(token);
                });
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
