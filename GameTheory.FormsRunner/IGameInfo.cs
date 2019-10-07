// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using GameTheory.Catalogs;

    public interface IGameInfo
    {
        event EventHandler<EventArgs> Move;

        ICatalogGame Game { get; }

        IReadOnlyList<object> GameStates { get; }

        IReadOnlyList<IMove> Moves { get; }

        IReadOnlyList<object> PlayerInstances { get; }

        IReadOnlyList<string> PlayerNames { get; }

        IReadOnlyList<ICatalogPlayer> Players { get; }

        IReadOnlyList<PlayerToken> PlayerTokens { get; }

        ListViewItem Row { get; }

        void Show();
    }
}
