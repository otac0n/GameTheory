// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Console
{
    using System;
    using GameTheory.ConsoleRunner.Shared;
    using GameTheory.Games.Ergo.Cards;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Splendor</see>.
    /// </summary>
    public class ErgoConsoleRenderer : ConsoleRendererBase<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken = null) => new Templates(playerToken).RenderGameState((GameState)state, this.MakeRenderTokenWriter(state));

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is PlayerToken playerToken)
            {
                ConsoleInteraction.WithColor(ConsoleInteraction.GetPlayerColor(state, playerToken), () =>
                {
                    Console.Write(string.Format(SharedResources.PlayerName, Resources.ResourceManager.GetEnumString((Symbol)state.Players.IndexOf(playerToken))));
                });
            }
            else if (token is Symbol symbol)
            {
                ConsoleColor? color = null;
                switch (symbol)
                {
                    case Symbol.PlayerA:
                    case Symbol.PlayerB:
                    case Symbol.PlayerC:
                    case Symbol.PlayerD:
                        var index = (int)symbol;
                        if (index < state.Players.Count)
                        {
                            color = ConsoleInteraction.GetPlayerColor(state, state.Players[index]);
                        }

                        break;
                }

                if (color != null)
                {
                    ConsoleInteraction.WithColor(color.Value, () =>
                    {
                        Console.Write(Resources.ResourceManager.GetEnumString(symbol));
                    });
                }
                else
                {
                    Console.Write(Resources.ResourceManager.GetEnumString(symbol));
                }
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
