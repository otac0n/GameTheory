// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using Ergo;
    using Games.Ergo;
    using GameTheory.Games.Ergo.Cards;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Splendor</see>.
    /// </summary>
    public class ErgoConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken)
        {
            new Templates(playerToken).RenderGameState((GameState)state, this.MakeRenderTokenWriter(state));
        }

        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is PlayerToken playerToken)
            {
                ConsoleInteraction.WithColor(ConsoleInteraction.GetPlayerColor(state, playerToken), () =>
                {
                    Console.Write("Player " + (Symbol)state.Players.IndexOf(playerToken));
                });
            }
            else if (token is Symbol symbol)
            {
                var color = (ConsoleColor?)null;
                string display;

                switch (symbol)
                {
                    case Symbol.And:
                        display = "&";
                        break;

                    case Symbol.Or:
                        display = "|";
                        break;

                    case Symbol.Then:
                        display = "=>";
                        break;

                    case Symbol.Not:
                        display = "~";
                        break;

                    case Symbol.LeftParenthesis:
                        display = "(";
                        break;

                    case Symbol.RightParenthesis:
                        display = ")";
                        break;

                    case Symbol.A:
                    case Symbol.B:
                    case Symbol.C:
                    case Symbol.D:
                        var index = (int)symbol;
                        if (index < state.Players.Count)
                        {
                            color = ConsoleInteraction.GetPlayerColor(state, state.Players[index]);
                        }

                        goto default;

                    default:
                        display = symbol.ToString();
                        break;
                }

                if (color != null)
                {
                    ConsoleInteraction.WithColor(color.Value, () =>
                    {
                        Console.Write(display);
                    });
                }
                else
                {
                    Console.Write(display);
                }
            }
            else
            {
                base.RenderToken(state, token);
            }
        }
    }
}
