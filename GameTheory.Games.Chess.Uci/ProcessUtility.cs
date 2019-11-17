// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Microsoft.Win32.SafeHandles;
    using static NativeMethods;

    public class LimitedAccessProcess : IDisposable
    {
        private readonly ProcessHandle processHandle;

        public LimitedAccessProcess(
            string executable,
            string arguments,
            string workingDirectory = null,
            bool createNoWindow = false,
            bool redirectInput = false,
            bool redirectOutput = false,
            bool redirectError = false,
            Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(executable))
            {
                throw new ArgumentNullException(nameof(executable));
            }

            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Environment.CurrentDirectory;
            }

            executable = executable.Trim();
            executable = executable.StartsWith("\"", StringComparison.Ordinal) && executable.EndsWith("\"", StringComparison.Ordinal)
                ? executable
                : $"\"{executable}\"";
            var commandLine = string.IsNullOrWhiteSpace(arguments)
                ? executable
                : $"{executable} {arguments}";

            var safeUserHandle = IntPtr.Zero;
            WithSaferCreateLevel(SaferScope.SAFER_SCOPEID_USER, SaferLevel.SAFER_LEVELID_UNTRUSTED, safeLevel =>
            {
                SaferComputeTokenFromLevel(safeLevel, IntPtr.Zero, out safeUserHandle, 0, IntPtr.Zero);
            });

            if (!IsValid(safeUserHandle))
            {
                throw new InvalidOperationException("Unable to create limited access user.");
            }

            var creationFlags = default(ProcessCreationFlags);

            if (createNoWindow)
            {
                creationFlags |= ProcessCreationFlags.CREATE_NO_WINDOW;
            }

            SafeFileHandle standardInputWritePipeHandle = null;
            SafeFileHandle standardOutputReadPipeHandle = null;
            SafeFileHandle standardErrorReadPipeHandle = null;
            var startupInfo = new STARTUPINFO();
            var processInfo = new PROCESS_INFORMATION();

            startupInfo.dwFlags = StartFlags.STARTF_UNTRUSTEDSOURCE;

            try
            {
                if (redirectInput || redirectOutput || redirectError)
                {
                    startupInfo.dwFlags |= StartFlags.STARTF_USESTDHANDLES;

                    this.CreateOrRedirectPipe(
                        redirectInput,
                        StdHandle.STD_INPUT_HANDLE,
                        FileAccess.Write,
                        out standardInputWritePipeHandle,
                        out startupInfo.hStdInput);

                    this.CreateOrRedirectPipe(
                        redirectOutput,
                        StdHandle.STD_OUTPUT_HANDLE,
                        FileAccess.Read,
                        out standardOutputReadPipeHandle,
                        out startupInfo.hStdOutput);

                    this.CreateOrRedirectPipe(
                        redirectError,
                        StdHandle.STD_ERROR_HANDLE,
                        FileAccess.Read,
                        out standardErrorReadPipeHandle,
                        out startupInfo.hStdError);
                }

                bool success;
                var processHandle = this.processHandle = new ProcessHandle();
                var threadHandle = new DelayedSafeHandle();
                var environment = default(GCHandle);
                try
                {
                    // Prevent UAC.
                    creationFlags |= ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT;
                    environment = GCHandle.Alloc(Encoding.Unicode.GetBytes("__COMPAT_LAYER=RUNASINVOKER\0\0"), GCHandleType.Pinned);
                    var environmentAddress = environment.AddrOfPinnedObject();

                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        success = CreateProcessAsUser(
                                safeUserHandle,
                                null,
                                commandLine,
                                null,
                                null,
                                true,
                                creationFlags,
                                environmentAddress,
                                workingDirectory,
                                startupInfo,
                                processInfo);

                        processHandle.DelayedSetHandle(processInfo.hProcess);
                        threadHandle.DelayedSetHandle(processInfo.hThread);
                    }
                }
                finally
                {
                    if (environment.IsAllocated)
                    {
                        environment.Free();
                    }

                    threadHandle.Close();
                }

                if (!success)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                if (processHandle.IsInvalid)
                {
                    throw new InvalidOperationException("Process creation failed.");
                }
            }
            finally
            {
                if (startupInfo.hStdInput?.IsInvalid == false)
                {
                    startupInfo.hStdInput.Close();
                }

                if (startupInfo.hStdOutput?.IsInvalid == false)
                {
                    startupInfo.hStdOutput.Close();
                }

                if (startupInfo.hStdError?.IsInvalid == false)
                {
                    startupInfo.hStdError.Close();
                }
            }

            if (redirectInput)
            {
                this.StandardInput = new StreamWriter(new FileStream(standardInputWritePipeHandle, FileAccess.Write, 4096, false), encoding ?? DefaultEncoding, 4096);
                this.StandardInput.AutoFlush = true;
            }

            if (redirectOutput)
            {
                this.StandardOutput = new StreamReader(new FileStream(standardOutputReadPipeHandle, FileAccess.Read, 4096, false), encoding ?? DefaultEncoding, true, 4096);
            }

            if (redirectError)
            {
                this.StandardError = new StreamReader(new FileStream(standardErrorReadPipeHandle, FileAccess.Read, 4096, false), encoding ?? DefaultEncoding, true, 4096);
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="LimitedAccessProcess"/> class.
        /// </summary>
        ~LimitedAccessProcess()
        {
            this.Dispose(false);
        }

        public StreamReader StandardError { get; }

        public StreamWriter StandardInput { get; }

        public StreamReader StandardOutput { get; }

        private static Encoding DefaultEncoding => Encoding.GetEncoding(28591);

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StandardInput?.Dispose();
                this.StandardOutput?.Dispose();
                this.StandardError?.Dispose();
            }

            this.processHandle.Dispose();
        }

        private void CreateOrRedirectPipe(bool redirect, StdHandle stdHandle, FileAccess access, out SafeFileHandle parentHandle, out SafeFileHandle childHandle)
        {
            if (redirect)
            {
                var securityAttributesParent = new SECURITY_ATTRIBUTES
                {
                    bInheritHandle = true,
                };

                SafeFileHandle hTmp = null;
                try
                {
                    if (access.HasFlag(FileAccess.Write))
                    {
                        CreatePipe(out childHandle, out parentHandle, securityAttributesParent, 0);
                    }
                    else
                    {
                        CreatePipe(out parentHandle, out childHandle, securityAttributesParent, 0);
                    }
                }
                finally
                {
                    if (hTmp?.IsInvalid == false)
                    {
                        hTmp.Close();
                    }
                }
            }
            else
            {
                childHandle = new SafeFileHandle(GetStdHandle(stdHandle), false);
                parentHandle = null;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        private class DelayedSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            internal DelayedSafeHandle()
                : base(true)
            {
            }

            public void DelayedSetHandle(IntPtr h) => this.SetHandle(h);

            protected override bool ReleaseHandle() => TryCloseHandle(this.handle);
        }

        [SuppressUnmanagedCodeSecurity]
        private class ProcessHandle : DelayedSafeHandle
        {
            internal ProcessHandle()
            {
            }

            protected override bool ReleaseHandle()
            {
                TerminateProcess(this.handle, 0);
                return base.ReleaseHandle();
            }
        }
    }
}
