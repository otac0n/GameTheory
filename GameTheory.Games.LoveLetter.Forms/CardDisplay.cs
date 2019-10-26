// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;

    public class CardDisplay : Display
    {
        public static readonly CardDisplay Default = new CardDisplay();

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
            }
            else
            {
                cardControl = new CardControl(card, showReverse);
            }

            return cardControl;
        }

        private static bool ShowReverse(Scope scope)
        {
            if (scope.Name == nameof(GameState.Hidden))
            {
                return true;
            }

            var parentScope = scope.Parent;
            switch (parentScope?.Name)
            {
                case nameof(GameState.Deck):
                    return true;

                case nameof(Inventory.Hand):
                    var inventoryScope = parentScope.Parent;
                    if (inventoryScope.Value is Inventory inventory && inventory.HandRevealed)
                    {
                        return false;
                    }

                    var player = scope.GetPropertyOrDefault<PlayerToken>(Scope.SharedProperties.PlayerToken, null);
                    var key = parentScope.GetPropertyOrDefault<PlayerToken>(Scope.SharedProperties.Key, null);
                    return player != null && player != key;
            }

            return false;
        }
    }
}
