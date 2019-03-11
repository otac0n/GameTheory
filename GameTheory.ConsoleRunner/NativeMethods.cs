// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        private const int LF_FACESIZE = 32;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int SW_MAXIMIZE = 3;
        private const int SW_RESTORE = 9;
        private const int TMPF_TRUETYPE = 4;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public static void Maximize()
        {
            var hnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(hnd, SW_MAXIMIZE);
        }

        public static void Restore()
        {
            var hnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(hnd, SW_RESTORE);
        }

        public static void SetConsoleFont(string fontName = "Lucida Console", short xSize = 0, short ySize = 0)
        {
            var hnd = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hnd != INVALID_HANDLE_VALUE)
            {
                var newInfo = default(CONSOLE_FONT_INFO_EX);
                newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                GetCurrentConsoleFontEx(hnd, false, ref newInfo);
                newInfo.dwFontSize.X = xSize;
                newInfo.dwFontSize.Y = ySize;
                newInfo.FaceName = fontName;
                SetCurrentConsoleFontEx(hnd, false, ref newInfo);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetCurrentConsoleFontEx(
            IntPtr hConsoleOutput,
            bool bMaximumWindow,
            ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFont);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCurrentConsoleFontEx(
            IntPtr hConsoleOutput,
            [MarshalAs(UnmanagedType.Bool)] bool bMaximumWindow,
            ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private unsafe struct CONSOLE_FONT_INFO_EX
        {
            public uint cbSize;
            public uint nFont;
            public COORD dwFontSize;
            public int FontFamily;
            public int FontWeight;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FaceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            internal short X;
            internal short Y;

            internal COORD(short x, short y)
            {
                this.X = x;
                this.Y = y;
            }
        }
    }
}
