// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers.TwentyFortyEight
{
    using System;
    using Games.TwentyFortyEight;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">TwentyFortyEight</see>.
    /// </summary>
    public class TwentyFortyEightConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken = null) => this.Show((GameState)state);

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is MoveDirection move)
            {
                Console.Write(Resources.ResourceManager.GetEnumString(move));
            }
            else if (token is string str && int.TryParse(str, out var value))
            {
                ConsoleColor color;
                switch (value)
                {
                    case 2:
                    case 4:
                    default:
                        color = ConsoleColor.Gray;
                        break;

                    case 8:
                    case 16:
                        color = ConsoleColor.DarkYellow;
                        break;

                    case 32:
                    case 64:
                        color = ConsoleColor.Red;
                        break;

                    case 128:
                    case 256:
                    case 512:
                    case 1024:
                    case 2048:
                        color = ConsoleColor.Yellow;
                        break;

                    case var _ when value > 2048:
                        color = ConsoleColor.DarkGreen;
                        break;
                }

                ConsoleInteraction.WithColor(color, () =>
                {
                    base.RenderToken(state, token);
                });
            }
            else
            {
                base.RenderToken(state, token);
            }
        }

        private void Show(GameState state) => new Templates().RenderGameState(state, this.MakeRenderTokenWriter(state));
    }
}
