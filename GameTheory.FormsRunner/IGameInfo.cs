// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public interface IGameInfo
    {
        event EventHandler<EventArgs> Move;

        Catalogs.IGame Game { get; }

        IReadOnlyList<object> GameStates { get; }

        IReadOnlyList<IMove> Moves { get; }

        IReadOnlyList<object> PlayerInstances { get; }

        IReadOnlyList<string> PlayerNames { get; }

        IReadOnlyList<Catalogs.Player> Players { get; }

        IReadOnlyList<PlayerToken> PlayerTokens { get; }

        ListViewItem Row { get; }

        void Show();
    }
}
