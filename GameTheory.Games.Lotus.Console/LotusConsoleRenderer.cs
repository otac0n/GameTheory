// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Console
{
    using System;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Splendor</see>.
    /// </summary>
    public class LotusConsoleRenderer : ConsoleRendererBase<GameState, Move>
    {
        /// <inheritdoc />
        public override void Show(GameState state, PlayerToken playerToken = null) => new Templates(playerToken).RenderGameState(state, this.MakeRenderTokenWriter(state));

        /// <inheritdoc/>
        protected override void RenderToken(GameState state, object token)
        {
            if (token is FlowerType flowerType)
            {
                var color = ConsoleColor.Green;
                switch (flowerType)
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
                    Console.Write(Resources.ResourceManager.GetEnumString(flowerType));
                });
            }
            else if (token is SpecialPowers specialPower)
            {
                Console.Write(Resources.ResourceManager.GetEnumString(specialPower));
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
