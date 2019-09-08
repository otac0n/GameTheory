// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using GameTheory.Catalogs;

    /// <summary>
    /// Contains the main entry point for the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The shared static game catalog for the application.
        /// </summary>
        public static readonly GameCatalog GameCatalog = GameCatalog.Default;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameManagerForm());
        }
    }
}
