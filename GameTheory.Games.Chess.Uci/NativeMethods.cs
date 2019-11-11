// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.Win32.SafeHandles;

    internal static class NativeMethods
    {
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public enum ProcessCreationFlags : uint
        {
            DEBUG_PROCESS = 0x00000001,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            CREATE_SUSPENDED = 0x00000004,
            DETACHED_PROCESS = 0x00000008,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            INHERIT_PARENT_AFFINITY = 0x00010000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            CREATE_SECURE_PROCESS = 0x00400000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NO_WINDOW = 0x08000000,
        }

        [Flags]
        public enum SaferLevel : uint
        {
            SAFER_LEVELID_DISALLOWED = 0,
            SAFER_LEVELID_UNTRUSTED = 0x1000,
            SAFER_LEVELID_CONSTRAINED = 0x10000,
            SAFER_LEVELID_NORMALUSER = 0x20000,
            SAFER_LEVELID_FULLYTRUSTED = 0x40000,
        }

        public enum SaferOpenFlags : int
        {
            SAFER_LEVEL_OPEN = 1,
        }

        [Flags]
        public enum SaferScope : uint
        {
            SAFER_SCOPEID_MACHINE = 1,
            SAFER_SCOPEID_USER = 2,
        }

        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation,
        }

        public enum StartFlags : uint
        {
            STARTF_USESHOWWINDOW = 0x00000001,
            STARTF_USESIZE = 0x00000002,
            STARTF_USEPOSITION = 0x00000004,
            STARTF_USECOUNTCHARS = 0x00000008,
            STARTF_USEFILLATTRIBUTE = 0x00000010,
            STARTF_RUNFULLSCREEN = 0x00000020,
            STARTF_FORCEONFEEDBACK = 0x00000040,
            STARTF_FORCEOFFFEEDBACK = 0x00000080,
            STARTF_USESTDHANDLES = 0x00000100,
            STARTF_USEHOTKEY = 0x00000200,
            STARTF_TITLEISLINKNAME = 0x00000800,
            STARTF_TITLEISAPPID = 0x00001000,
            STARTF_PREVENTPINNING = 0x00002000,
            STARTF_UNTRUSTEDSOURCE = 0x00008000,
        }

        public enum StdHandle : int
        {
            STD_INPUT_HANDLE = -10,
            STD_ERROR_HANDLE = -12,
            STD_OUTPUT_HANDLE = -11,
        }

        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation,
        }

        public static void CloseHandle(IntPtr handle)
        {
            if (!TryCloseHandle(handle))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        public static void CreatePipe(out SafeFileHandle readHandle, out SafeFileHandle writeHandle, SECURITY_ATTRIBUTES pipeAttributes, int size)
        {
            if (!TryCreatePipe(out readHandle, out writeHandle, pipeAttributes, size))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            SECURITY_ATTRIBUTES lpProcessAttributes,
            SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            STARTUPINFO lpStartupInfo,
            PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(StdHandle nStdHandle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(IntPtr handle) => handle != INVALID_HANDLE_VALUE && handle != IntPtr.Zero;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool SaferComputeTokenFromLevel(IntPtr LevelHandle, IntPtr InAccessToken, out IntPtr OutAccessToken, int dwFlags, IntPtr lpReserved);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TryCloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", EntryPoint = "CreatePipe", SetLastError = true)]
        public static extern bool TryCreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, SECURITY_ATTRIBUTES lpPipeAttributes, int nSize);

        public static void WithSaferCreateLevel(SaferScope scope, SaferLevel level, Action<IntPtr> action)
        {
            var safer = IntPtr.Zero;
            try
            {
                if (!SaferCreateLevel(scope, level, SaferOpenFlags.SAFER_LEVEL_OPEN, out safer, IntPtr.Zero))
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                action(safer);
            }
            finally
            {
                if (safer != IntPtr.Zero)
                {
                    SaferCloseLevel(safer);
                }
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SaferCloseLevel(IntPtr hLevelHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SaferCreateLevel(SaferScope dwScopeId, SaferLevel dwLevelId, SaferOpenFlags OpenFlags, out IntPtr pLevelHandle, IntPtr lpReserved);

        [StructLayout(LayoutKind.Sequential)]
        public class PROCESS_INFORMATION
        {
            public IntPtr hProcess = IntPtr.Zero;
            public IntPtr hThread = IntPtr.Zero;
            public int dwProcessId = 0;
            public int dwThreadId = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SECURITY_ATTRIBUTES
        {
            public int nLength = 12;
            public IntPtr lpSecurityDescriptor = IntPtr.Zero;
            public bool bInheritHandle = false;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class STARTUPINFO
        {
            public int cb;
            public IntPtr lpReserved = IntPtr.Zero;
            public IntPtr lpDesktop = IntPtr.Zero;
            public IntPtr lpTitle = IntPtr.Zero;
            public int dwX = 0;
            public int dwY = 0;
            public int dwXSize = 0;
            public int dwYSize = 0;
            public int dwXCountChars = 0;
            public int dwYCountChars = 0;
            public int dwFillAttribute = 0;
            public StartFlags dwFlags = 0;
            public short wShowWindow = 0;
            public short cbReserved2 = 0;
            public IntPtr lpReserved2 = IntPtr.Zero;
            public SafeFileHandle hStdInput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdError = new SafeFileHandle(IntPtr.Zero, false);

            public STARTUPINFO()
            {
                this.cb = Marshal.SizeOf(this);
            }
        }
    }
}
