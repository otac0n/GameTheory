// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;

    public class CardDisplay : Display
    {
        public static readonly CardDisplay Default = new CardDisplay();
        public static readonly CardDisplay Uncertain = new CardDisplay(showAsUncertain: true);

        private readonly bool showAsUncertain;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardDisplay"/> class.
        /// </summary>
        public CardDisplay()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CardDisplay"/> class.
        /// </summary>
        /// <param name="showAsUncertain">A value indicating whehter or not the cards shoule be show as uncertain.</param>
        public CardDisplay(bool showAsUncertain)
        {
            this.showAsUncertain = showAsUncertain;
        }

        /// <inheritdoc/>
        public override bool CanDisplay(Scope scope, Type type, object value) => typeof(Card).IsAssignableFrom(type);

        /// <inheritdoc/>
        protected override Control Update(Control control, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
        {
            var card = (Card)value;
            var showReverse = ShowReverse(scope);

            if (control is CardControl cardControl)
            {
                cardControl.Card = card;
                cardControl.ShowReverse = showReverse;
                cardControl.ShowAsUncertain = this.showAsUncertain;
            }
            else
            {
                cardControl = new CardControl(card, showReverse, this.showAsUncertain);
            }

            return cardControl;
        }

        private static bool ShowReverse(Scope scope)
        {
            var parentScope = scope.Parent;
            switch (parentScope.Name)
            {
                case nameof(GameState.Deck):
                    return true;

                case nameof(Inventory.Hand):
                    var player = scope.GetPropertyOrDefault<PlayerToken>(Scope.SharedProperties.PlayerToken, null);
                    var key = scope.GetPropertyOrDefault<PlayerToken>(Scope.SharedProperties.Key, null);
                    return player != null && player != key;
            }

            return false;
        }
    }
}
