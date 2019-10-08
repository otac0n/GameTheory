// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;
    using GameTheory.Catalogs;
    using GameTheory.FormsRunner.Shared.Catalogs;
    using Unity;
    using Unity.ServiceLocation;

    /// <summary>
    /// Contains the main entry point for the application.
    /// </summary>
    public static class Program
    {
        static Program()
        {
            Container = new UnityContainer();
            var serviceLocator = new UnityServiceLocator(Container);
            DisplayCatalog = PluginLoader.LoadCatalogs<IDisplayCatalog>(d => new CompositeDisplayCatalog(d), serviceLocator: serviceLocator);
            GameCatalog = PluginLoader.LoadGameCatalogs(serviceLocator: serviceLocator);
            PlayerCatalog = PluginLoader.LoadPlayerCatalogs(serviceLocator: serviceLocator);
        }

        /// <summary>
        /// Gets the shared container for the application.
        /// </summary>
        public static UnityContainer Container { get; }

        /// <summary>
        /// Gets the shared static game catalog for the application.
        /// </summary>
        public static IDisplayCatalog DisplayCatalog { get; }

        /// <summary>
        /// Gets the shared static game catalog for the application.
        /// </summary>
        public static IGameCatalog GameCatalog { get; }

        /// <summary>
        /// Gets the shared static player catalong for the application.
        /// </summary>
        public static IPlayerCatalog PlayerCatalog { get; }

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
