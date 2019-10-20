// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Console
{
    using System;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">FiveTribes</see>.
    /// </summary>
    public class FiveTribesConsoleRenderer : ConsoleRendererBase<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken = null) => this.Show((GameState)state);

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is Meeple meeple)
            {
                var color = ConsoleColor.White;
                switch (meeple)
                {
                    case Meeple.Vizier:
                        color = ConsoleColor.Yellow;
                        break;

                    case Meeple.Assassin:
                        color = ConsoleColor.Red;
                        break;

                    case Meeple.Merchant:
                        color = ConsoleColor.Green;
                        break;

                    case Meeple.Builder:
                        color = ConsoleColor.Blue;
                        break;

                    case Meeple.Elder:
                        color = ConsoleColor.White;
                        break;
                }

                ConsoleInteraction.WithColor(color, () =>
                {
                    Console.Write(Resources.ResourceManager.GetEnumString(meeple));
                });
            }
            else
            {
                base.RenderToken(state, token);
            }
        }

        private void Show(GameState state) => new Templates(state).RenderGameState(state, this.MakeRenderTokenWriter(state));
    }
}
