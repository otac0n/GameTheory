// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Console
{
    using System;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Splendor</see>.
    /// </summary>
    public class SplendorConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken = null) => new Templates(playerToken).RenderGameState((GameState)state, this.MakeRenderTokenWriter(state));

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is Token splendorToken)
            {
                var color = ConsoleColor.White;
                switch (splendorToken)
                {
                    case Token.Emerald:
                        color = ConsoleColor.Green;
                        break;

                    case Token.Diamond:
                        color = ConsoleColor.White;
                        break;

                    case Token.Sapphire:
                        color = ConsoleColor.Blue;
                        break;

                    case Token.Onyx:
                        color = ConsoleColor.DarkGray;
                        break;

                    case Token.Ruby:
                        color = ConsoleColor.Red;
                        break;

                    case Token.GoldJoker:
                        color = ConsoleColor.Yellow;
                        break;
                }

                ConsoleInteraction.WithColor(color, () =>
                {
                    Console.Write(Resources.ResourceManager.GetEnumString(splendorToken));
                });
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
