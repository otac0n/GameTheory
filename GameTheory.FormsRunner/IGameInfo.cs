// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    public interface IGameInfo
    {
        Catalogs.IGame Game { get; }

        IReadOnlyList<Catalogs.Player> Players { get; }

        IReadOnlyList<object> PlayerInstances { get; }

        ListViewItem Row { get; }

        IReadOnlyList<object> GameStates { get; }

        IReadOnlyList<IMove> Moves { get; }

        void Show();
    }
}
