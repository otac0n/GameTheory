// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull.Console
{
    using System;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Skull</see>.
    /// </summary>
    public class SkullConsoleRenderer : ConsoleRendererBase<GameState, Move>
    {
        /// <inheritdoc />
        public override void Show(GameState state, PlayerToken playerToken = null) => new Templates(playerToken).RenderGameState(state, this.MakeRenderTokenWriter(state));

        /// <inheritdoc/>
        protected override void RenderToken(GameState state, object token)
        {
            if (token is Card card)
            {
                var color = ConsoleColor.White;
                switch (card)
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
                    Console.Write(Resources.ResourceManager.GetEnumString(card));
                });
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
