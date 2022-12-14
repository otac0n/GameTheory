// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Console
{
    using System;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">CenturySpiceRoad</see>.
    /// </summary>
    public class CenturySpiceRoadConsoleRenderer : ConsoleRendererBase<GameState, Move>
    {
        /// <inheritdoc />
        public override void Show(GameState state, PlayerToken playerToken = null) => new Templates(playerToken).RenderGameState(state, this.MakeRenderTokenWriter(state));

        /// <inheritdoc/>
        protected override void RenderToken(GameState state, object token)
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
                    Console.Write(Resources.ResourceManager.GetEnumString(spice));
                });
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
