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
        private const int TMPF_TRUETYPE = 4;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public static void Maximize()
        {
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle, SW_MAXIMIZE);
        }

        public static void SetConsoleFont(string fontName = "Lucida Console")
        {
            unsafe
            {
                var hnd = GetStdHandle(STD_OUTPUT_HANDLE);
                if (hnd != INVALID_HANDLE_VALUE)
                {
                    var info = default(CONSOLE_FONT_INFO_EX);
                    info.cbSize = (uint)Marshal.SizeOf(info);

                    var newInfo = default(CONSOLE_FONT_INFO_EX);
                    newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                    newInfo.FontFamily = TMPF_TRUETYPE;
                    var ptr = new IntPtr(newInfo.FaceName);
                    Marshal.Copy(fontName.ToCharArray(), 0, ptr, fontName.Length);

                    // Get some settings from current font.
                    newInfo.dwFontSize = new COORD(info.dwFontSize.X, info.dwFontSize.Y);
                    newInfo.FontWeight = info.FontWeight;
                    SetCurrentConsoleFontEx(hnd, false, ref newInfo);
                }
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

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
            internal uint cbSize;
            internal uint nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal fixed char FaceName[LF_FACESIZE];
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
