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
            if (token is FlowerType)
            {
                var color = ConsoleColor.Green;
                var display = token.ToString();

                switch ((FlowerType)token)
                {
                    case FlowerType.Iris:
                        color = ConsoleColor.DarkMagenta;
                        display = Resources.FlowerType_Iris;
                        break;

                    case FlowerType.Primrose:
                        color = ConsoleColor.Yellow;
                        display = Resources.FlowerType_Primrose;
                        break;

                    case FlowerType.CherryBlossom:
                        color = ConsoleColor.Red;
                        display = Resources.FlowerType_CherryBlossom;
                        break;

                    case FlowerType.Lily:
                        color = ConsoleColor.White;
                        display = Resources.FlowerType_Lily;
                        break;

                    case FlowerType.Lotus:
                        color = ConsoleColor.Magenta;
                        display = Resources.FlowerType_Lotus;
                        break;
                }

                ConsoleInteraction.WithColor(color, () =>
                {
                    Console.Write(display);
                });
            }
            else if (token is SpecialPowers)
            {
                var display = token.ToString();

                switch ((SpecialPowers)token)
                {
                    case SpecialPowers.ElderGuardian:
                        display = Resources.SpecialPowers_ElderGuardian;
                        break;

                    case SpecialPowers.EnlightenedPath:
                        display = Resources.SpecialPowers_EnlightenedPath;
                        break;

                    case SpecialPowers.InfiniteGrowth:
                        display = Resources.SpecialPowers_InfiniteGrowth;
                        break;

                    case SpecialPowers.None:
                        display = Resources.SpecialPowers_None;
                        break;
                }

                Console.Write(display);
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
