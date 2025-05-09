﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides basic rendering for more complex console renderers.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states that will be displayed.</typeparam>
    /// <typeparam name="TMove">The type of moves in the game state.</typeparam>
    public abstract class ConsoleRendererBase<TGameState, TMove> : IConsoleRenderer<TGameState, TMove>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <inheritdoc/>
        public abstract void Show(TGameState state, PlayerToken playerToken = null);

        /// <inheritdoc/>
        public void Show(TGameState state, IList<object> formatTokens)
        {
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(formatTokens);

            foreach (var token in formatTokens)
            {
                this.RenderToken(state, token);
            }
        }

        /// <inheritdoc/>
        public void Show(TGameState state, ITokenFormattable tokenFormattable)
        {
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(tokenFormattable);

            this.Show(state, tokenFormattable.FormatTokens);
        }

        /// <summary>
        /// Creates a <see cref="TextWriter"/> that will invoke <see cref="RenderToken"/> for all interactions.
        /// </summary>
        /// <param name="state">The state used to invoke <see cref="RenderToken"/>.</param>
        /// <returns>A new <see cref="TextWriter"/>.</returns>
        protected TextWriter MakeRenderTokenWriter(TGameState state)
        {
            return new ConsoleWriter(this, state);
        }

        /// <summary>
        /// Renders an atomic token.
        /// </summary>
        /// <param name="state">The context game state.</param>
        /// <param name="token">The token to be rendered.</param>
        protected virtual void RenderToken(TGameState state, object token)
        {
            ITokenFormattable innerTokens;
            if (token is PlayerToken playerToken)
            {
                ConsoleInteraction.WithColor(ConsoleInteraction.GetPlayerColor<TGameState, TMove>(state, playerToken), () =>
                {
                    this.RenderToken(state, state.GetPlayerName<TGameState, TMove>(playerToken));
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

        private class ConsoleWriter : TextWriter
        {
            private readonly ConsoleRendererBase<TGameState, TMove> consoleRenderer;
            private readonly TGameState state;

            public ConsoleWriter(ConsoleRendererBase<TGameState, TMove> consoleRenderer, TGameState state)
            {
                this.consoleRenderer = consoleRenderer;
                this.state = state;
            }

            /// <inheritdoc />
            public override Encoding Encoding => Console.Out.Encoding;

            /// <inheritdoc />
            public override void Write(char[] buffer) => this.Write(new string(buffer));

            /// <inheritdoc />
            public override void Write(char[] buffer, int index, int count)
            {
                var sb = new StringBuilder(count);
                sb.Append(buffer, index, count);
                base.Write(sb.ToString());
            }

            /// <inheritdoc />
            public override void Write(string format, object arg0) => this.Write(format, new[] { arg0 });

            /// <inheritdoc />
            public override void Write(string format, object arg0, object arg1) => this.Write(format, new[] { arg0, arg1 });

            /// <inheritdoc />
            public override void Write(string format, object arg0, object arg1, object arg2) => this.Write(format, new[] { arg0, arg1, arg2 });

            /// <inheritdoc />
            public override void Write(string format, params object[] arg)
            {
                foreach (var token in FormatUtilities.ParseStringFormat(format, arg))
                {
                    this.Write(token);
                }
            }

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

            /// <inheritdoc />
            public override void WriteLine(char[] buffer, int index, int count)
            {
                var sb = new StringBuilder(count + this.CoreNewLine.Length);
                sb.Append(buffer, index, count);
                sb.Append(this.CoreNewLine);
                base.Write(sb.ToString());
            }

            /// <inheritdoc />
            public override void WriteLine(string format, object arg0) => this.WriteLine(format, new[] { arg0 });

            /// <inheritdoc />
            public override void WriteLine(string format, object arg0, object arg1) => this.WriteLine(format, new[] { arg0, arg1 });

            /// <inheritdoc />
            public override void WriteLine(string format, object arg0, object arg1, object arg2) => this.WriteLine(format, new[] { arg0, arg1, arg2 });

            /// <inheritdoc />
            public override void WriteLine(string format, params object[] arg)
            {
                this.Write(format, arg);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(string value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(bool value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(char value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(decimal value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(double value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(float value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(int value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(long value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(object value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(uint value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(ulong value)
            {
                this.Write(value);
                this.WriteLine();
            }

            /// <inheritdoc />
            public override void WriteLine(char[] buffer)
            {
                this.Write(buffer);
                this.WriteLine();
            }
        }
    }
}
