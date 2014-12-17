// -----------------------------------------------------------------------
// <copyright file="Random.cs" company="(none)">
//   Copyright © 2014 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory
{
    using System;
    using System.Threading;

    /// <summary>
    /// Provides a thread-static instance of the <see cref="System.Random"/> class.
    /// </summary>
    public static class Random
    {
        [ThreadStatic]
        private static System.Random instance;

        /// <summary>
        /// Gets an instance of the <see cref="System.Random"/> class that is unique to the current thread.
        /// </summary>
        /// <remarks>
        /// Do not allow this instance to be used on other threads.
        /// </remarks>
        public static System.Random Instance
        {
            get { return instance ?? (instance = new System.Random(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId)); }
        }
    }
}
