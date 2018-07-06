// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers.Lotus
{
    using System;
    using Games.Lotus;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Splendor</see>.
    /// </summary>
    public class LotusConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken) => new Templates(playerToken).RenderGameState((GameState)state, this.MakeRenderTokenWriter(state));

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
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
                    Console.Write(Resources.ResourceManager.GetString($"FlowerType_{flowerType}"));
                });
            }
            else if (token is SpecialPowers specialPower)
            {
                Console.Write(Resources.ResourceManager.GetString($"SpecialPowers_{specialPower}"));
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
