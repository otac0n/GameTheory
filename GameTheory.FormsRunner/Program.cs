// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
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
        public static readonly GameCatalog GameCatalog = new CompositeGameCatalog(
            GameCatalog.Default,
            new Gdl.GdlGameCatalog(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."))));

        /// <summary>
        /// The shared static player catalong for the application.
        /// </summary>
        public static readonly PlayerCatalog PlayerCatalog = new PlayerCatalog(
            Assembly.GetExecutingAssembly(),
            typeof(IGameState<>).Assembly);

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

        /// <summary>
        /// Extension method allowing conditional invoke usage.
        /// </summary>
        /// <param name="this">The object with which to synchronize.</param>
        /// <param name="action">The action to perform.</param>
        public static void InvokeIfRequired(this ISynchronizeInvoke @this, MethodInvoker action)
        {
            if (@this.InvokeRequired)
            {
                @this.Invoke(action, new object[0]);
            }
            else
            {
                action();
            }
        }
    }
}
