// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Cards
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a symbol on a card.
    /// </summary>
    public enum Symbol
    {
        /// <summary>
        /// The symbol for player A ('A').
        /// </summary>
        PlayerA = 0,

        /// <summary>
        /// The symbol for player B ('B').
        /// </summary>
        PlayerB = 1,

        /// <summary>
        /// The symbol for player C ('C').
        /// </summary>
        PlayerC = 2,

        /// <summary>
        /// The symbol for player D ('D').
        /// </summary>
        PlayerD = 3,

        /// <summary>
        /// The And symbol ('Λ').
        /// </summary>
        And,

        /// <summary>
        /// The Or symbol ('ν').
        /// </summary>
        Or,

        /// <summary>
        /// The Then symbol ('⊃').
        /// </summary>
        Then,

        /// <summary>
        /// The Not symbol ('~').
        /// </summary>
        Not,

        /// <summary>
        /// The Left Parenthesis symbol ('(').
        /// </summary>
        LeftParenthesis,

        /// <summary>
        /// The Rigth Parenthesis symbol (')').
        /// </summary>
        RightParenthesis,
    }

    /// <summary>
    /// Represents a symbol card.
    /// </summary>
    public sealed class SymbolCard : Card
    {
        /// <summary>
        /// The single instance of the And symbol card.
        /// </summary>
        public static readonly SymbolCard And = new SymbolCard(Symbol.And);

        /// <summary>
        /// The single instance of the Not symbol card.
        /// </summary>
        public static readonly SymbolCard Not = new SymbolCard(Symbol.Not);

        /// <summary>
        /// The single instance of the Or symbol card.
        /// </summary>
        public static readonly SymbolCard Or = new SymbolCard(Symbol.Or);

        /// <summary>
        /// The single instance of the Parenthesis symbol card.
        /// </summary>
        public static readonly SymbolCard Parenthesis = new SymbolCard(Symbol.LeftParenthesis, Symbol.RightParenthesis);

        /// <summary>
        /// The single instance of the Then symbol card.
        /// </summary>
        public static readonly SymbolCard Then = new SymbolCard(Symbol.Then);

        /// <summary>
        /// The single instance of the A symbol card.
        /// </summary>
        public static readonly SymbolCard VariableA = new SymbolCard(Symbol.PlayerA);

        /// <summary>
        /// The single instance of the B symbol card.
        /// </summary>
        public static readonly SymbolCard VariableB = new SymbolCard(Symbol.PlayerB);

        /// <summary>
        /// The single instance of the C symbol card.
        /// </summary>
        public static readonly SymbolCard VariableC = new SymbolCard(Symbol.PlayerC);

        /// <summary>
        /// The single instance of the D symbol card.
        /// </summary>
        public static readonly SymbolCard VariableD = new SymbolCard(Symbol.PlayerD);

        /// <summary>
        /// The single instance of the Wild Operator symbol card.
        /// </summary>
        public static readonly SymbolCard WildOperator = new SymbolCard(Symbol.And, Symbol.Or, Symbol.Then, Symbol.Not);

        /// <summary>
        /// The single instance of the Wild Variable symbol card.
        /// </summary>
        public static readonly SymbolCard WildVariable = new SymbolCard(Symbol.PlayerA, Symbol.PlayerB, Symbol.PlayerC, Symbol.PlayerD);

        private SymbolCard(params Symbol[] symbols)
        {
            this.Symbols = symbols.ToImmutableList();
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens
        {
            get
            {
                var tokens = new List<object>
                {
                    this.Symbols.Count == 1 ? "Symbol" : "Symbols",
                };

                foreach (var s in this.Symbols)
                {
                    tokens.Add(" ");
                    tokens.Add(s);
                }

                return tokens;
            }
        }

        /// <summary>
        /// Gets the symbols offered by this card.
        /// </summary>
        public ImmutableList<Symbol> Symbols { get; }

        /// <inheritdoc/>
        public override int CompareTo(Card other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }
            else if (other is SymbolCard symbolCard)
            {
                return CompareUtilities.CompareEnumLists(this.Symbols, symbolCard.Symbols);
            }
            else
            {
                return base.CompareTo(other);
            }
        }
    }
}
