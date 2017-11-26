// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using Games.Lotus;
    using Lotus;

    /// <summary>
    /// Implements a console renderer for the game of <see cref="GameState">Splendor</see>.
    /// </summary>
    public class LotusConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken)
        {
            new Templates(playerToken).RenderGameState((GameState)state, this.MakeRenderTokenWriter(state));
        }

        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is FlowerType)
            {
                var color = ConsoleColor.Green;
                switch ((FlowerType)token)
                {
                    case FlowerType.Iris:
                        color = ConsoleColor.DarkMagenta;
                        break;

                    case FlowerType.Primrose:
                        color = ConsoleColor.Yellow;
                        break;

                    case FlowerType.CherryBlossom:
                        color = ConsoleColor.Red;
                        break;

                    case FlowerType.Lily:
                        color = ConsoleColor.White;
                        break;

                    case FlowerType.Lotus:
                        color = ConsoleColor.Magenta;
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
