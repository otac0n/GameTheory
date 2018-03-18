// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers.Skull
{
    using System;
    using Games.Skull;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Skull</see>.
    /// </summary>
    public class SkullConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken) => new Templates(playerToken).RenderGameState((GameState)state, this.MakeRenderTokenWriter(state));

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is Card)
            {
                var color = ConsoleColor.White;
                switch ((Card)token)
                {
                    case Card.Flower:
                        color = ConsoleColor.Magenta;
                        break;

                    case Card.Skull:
                        color = ConsoleColor.Red;
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
