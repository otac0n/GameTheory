// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides basic rendering for more complex console renderers.
    /// </summary>
    /// <typeparam name="TMove">The type of moves in the game state.</typeparam>
    public abstract class BaseConsoleRenderer<TMove> : IConsoleRenderer<TMove>
        where TMove : IMove
    {
        /// <inheritdoc/>
        public abstract void Show(IGameState<TMove> state, PlayerToken playerToken = null);

        /// <inheritdoc/>
        public void Show(IGameState<TMove> state, IList<object> formatTokens)
        {
            foreach (var token in FormatUtilities.FlattenFormatTokens(formatTokens))
            {
                this.RenderToken(state, token);
            }
        }

        /// <inheritdoc/>
        public void Show(IGameState<TMove> state, ITokenFormattable tokenFormattable)
        {
            this.Show(state, tokenFormattable.FormatTokens);
        }

        /// <summary>
        /// Renders an atomic token.
        /// </summary>
        /// <param name="state">The context game state.</param>
        /// <param name="token">The token to be rendered.</param>
        protected virtual void RenderToken(IGameState<TMove> state, object token)
        {
            ITokenFormattable innerTokens;
            if (token is PlayerToken playerToken)
            {
                ConsoleInteraction.WithColor(ConsoleInteraction.GetPlayerColor(state, playerToken), () =>
                {
                    this.RenderToken(state, state.GetPlayerName(playerToken));
                });
            }
            else if ((innerTokens = token as ITokenFormattable) != null)
            {
                foreach (var innerToken in innerTokens.FormatTokens)
                {
                    this.RenderToken(state, innerToken);
                }
            }
            else
            {
                Console.Write(token);
            }
        }

        /// <summary>
        /// Creates a <see cref="TextWriter"/> that will invoke <see cref="RenderToken"/> for all interactions.
        /// </summary>
        /// <param name="state">The state used to invoke <see cref="RenderToken"/>.</param>
        /// <returns>A new <see cref="TextWriter"/>.</returns>
        protected TextWriter MakeRenderTokenWriter(IGameState<TMove> state)
        {
            return new ConsoleWriter(this, state);
        }

        private class ConsoleWriter : TextWriter
        {
            private readonly BaseConsoleRenderer<TMove> consoleRenderer;
            private readonly IGameState<TMove> state;

            public ConsoleWriter(BaseConsoleRenderer<TMove> consoleRenderer, IGameState<TMove> state)
            {
                this.consoleRenderer = consoleRenderer;
                this.state = state;
            }

            /// <inheritdoc />
            public override Encoding Encoding => Console.Out.Encoding;

            /// <inheritdoc />
            public override void Write(string value) => this.consoleRenderer.RenderToken(this.state, value);

            /// <inheritdoc />
            public override void Write(bool value) => this.consoleRenderer.RenderToken(this.state, value);

            /// <inheritdoc />
            public override void Write(char value) => this.consoleRenderer.RenderToken(this.state, value);

            /// <inheritdoc />
            public override void Write(decimal value) => this.consoleRenderer.RenderToken(this.state, value);

            /// <inheritdoc />
            public override void Write(double value) => this.consoleRenderer.RenderToken(this.state, value);

            /// <inheritdoc />
            public override void Write(float value) => this.consoleRenderer.RenderToken(this.state, value);

            /// <inheritdoc />
            public override void Write(int value) => this.consoleRenderer.RenderToken(this.state, value);

            /// <inheritdoc />
            public override void Write(long value) => this.consoleRenderer.RenderToken(this.state, value);

            /// <inheritdoc />
            public override void Write(object value) => this.consoleRenderer.RenderToken(this.state, value);

            /// <inheritdoc />
            public override void Write(uint value) => this.consoleRenderer.RenderToken(this.state, value);

            /// <inheritdoc />
            public override void Write(ulong value) => this.consoleRenderer.RenderToken(this.state, value);
        }
    }
}
