// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using GameTheory.Catalogs;

    /// <summary>
    /// Contains the main entry point for the application.
    /// </summary>
    public static class Program
    {
        private static readonly IReadOnlyList<Assembly> GameAssemblies =
            (from file in Directory.EnumerateFiles(Environment.CurrentDirectory, "GameTheory.Games.*.dll")
             select Assembly.LoadFrom(file)).ToList().AsReadOnly();

        private static readonly IReadOnlyList<Assembly> PlayerAssemblies =
            GameAssemblies.Concat(new[] { typeof(IGameState<>).Assembly, typeof(Players.WinFormsPlayer<>).Assembly }).ToList().AsReadOnly();

        /// <summary>
        /// Gets the shared static game catalog for the application.
        /// </summary>
        public static IGameCatalog GameCatalog { get; } = new AssemblyGameCatalog(GameAssemblies);

        /// <summary>
        /// Gets the shared static player catalong for the application.
        /// </summary>
        public static IPlayerCatalog PlayerCatalog { get; } = new AssemblyPlayerCatalog(PlayerAssemblies);

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
