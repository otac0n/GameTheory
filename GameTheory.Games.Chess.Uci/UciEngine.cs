// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.Games.Chess.Uci.Protocol;

    public class UciEngine : IDisposable
    {
        private static readonly TimeSpan HeaderTimeout = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan PreambleDelay = TimeSpan.FromMilliseconds(100);
        private readonly TaskCompletionSource<string> authorSource = new TaskCompletionSource<string>();
        private readonly Queue<Command> commandQueue = new Queue<Command>();
        private readonly object commandSync = new object();
        private readonly LimitedAccessProcess engineProcess;
        private readonly TaskCompletionSource<string> nameSource = new TaskCompletionSource<string>();
        private readonly List<OptionCommand> options = new List<OptionCommand>();
        private readonly TaskCompletionSource<IReadOnlyList<OptionCommand>> optionsSource = new TaskCompletionSource<IReadOnlyList<OptionCommand>>();
        private readonly Task runTask;

        public UciEngine(string fileName, string arguments = null)
        {
            fileName = Path.GetFullPath(fileName);
            var directoryName = Path.GetDirectoryName(fileName);

            this.engineProcess = new LimitedAccessProcess(
                executable: fileName,
                arguments: arguments,
                workingDirectory: directoryName,
                createNoWindow: true,
                redirectInput: true,
                redirectOutput: true,
                redirectError: true);

            this.runTask = Task.Run(this.RunUci);
        }

        public event EventHandler<UnhandledCommandEventArgs> UnhandledCommand;

        private enum State : int
        {
            Preamble = 0,
            ReadHeader,
            UciReady,
            Readying,
            Ready,
            Quit,
        }

        public string Author => this.authorSource.Task.Result;

        public string Name => this.nameSource.Task.Result;

        public IReadOnlyList<OptionCommand> Options => this.optionsSource.Task.Result;

        public void Dispose()
        {
            this.Execute(QuitCommand.Instance);
            this.engineProcess.Dispose();
        }

        public void Execute(Command command)
        {
            lock (this.commandSync)
            {
                this.commandQueue.Enqueue(command);
                Monitor.Pulse(this.commandSync);
            }
        }

        private void RaiseUnhandledCommand(Command command)
        {
            this.UnhandledCommand?.Invoke(this, new UnhandledCommandEventArgs(command));
        }

        private async Task RunUci()
        {
            try
            {
                var state = State.Preamble;
                var stdIn = this.engineProcess.StandardInput;
                var stdOut = this.engineProcess.StandardOutput;
                var parser = new Parser();

                Task<string> Read() => Task.Run(() =>
                {
                    string line = null;
                    while (true)
                    {
                        try
                        {
                            line = stdOut.ReadLine();
                        }
                        catch (ObjectDisposedException)
                        {
                        }

                        if (line == null || line.Length > 0)
                        {
                            break;
                        }
                    }

                    Debug.WriteLine($"<= {line}");
                    return line;
                });

                void Write(Command command)
                {
                    var line = command.ToString();
                    Debug.WriteLine($"=> {line}");
                    stdIn.WriteLine(line);
                }

                Task<Command> Command() => Task.Run<Command>(() =>
                {
                    lock (this.commandSync)
                    {
                        while (state != State.Quit)
                        {
                            if (this.commandQueue.Count > 0)
                            {
                                return this.commandQueue.Dequeue();
                            }

                            Monitor.Wait(this.commandSync);
                        }
                    }

                    return null;
                });

                var readTask = Read();
                var preambleDelay = Task.Delay(PreambleDelay);
                Task headerTimeout = null;
                Task<Command> commandTask = null;
                var otherTask = preambleDelay;

                while (state != State.Quit)
                {
                    var task = await Task.WhenAny(readTask, otherTask).ConfigureAwait(false);

                    if (task == preambleDelay)
                    {
                        state = State.ReadHeader;
                        Write(UciCommand.Instance);
                        otherTask = headerTimeout = Task.Delay(HeaderTimeout);
                    }
                    else if (task == headerTimeout)
                    {
                        var timeout = new TimeoutException();
                        this.nameSource.TrySetException(timeout);
                        this.authorSource.TrySetException(timeout);
                        this.optionsSource.TrySetException(timeout);
                        state = State.Quit;
                        continue;
                    }
                    else if (task == commandTask)
                    {
                        var command = commandTask.Result;
                        switch (command)
                        {
                            case QuitCommand quitCommand:
                                // Send quit after leaving the loop with `continue`.
                                state = State.Quit;
                                continue;

                            case IsReadyCommand isReadyCommand when state == State.UciReady:
                                state = State.Readying;
                                break;
                        }

                        Write(command);
                        otherTask = commandTask = Command();
                    }
                    else if (task == readTask)
                    {
                        var line = readTask.Result;
                        if (line == null)
                        {
                            state = State.Quit;
                            lock (this.commandSync)
                            {
                                Monitor.Pulse(this.commandSync);
                            }

                            continue;
                        }
                        else
                        {
                            readTask = Read();
                        }

                        var command = parser.Parse(line);
                        switch (state)
                        {
                            case State.Preamble:
                                this.RaiseUnhandledCommand(command);
                                break;

                            case State.ReadHeader:
                                switch (command)
                                {
                                    case IdCommand idCommand when idCommand.Field == "name":
                                        this.nameSource.TrySetResult(idCommand.Value);
                                        break;

                                    case IdCommand idCommand when idCommand.Field == "author":
                                        this.authorSource.TrySetResult(idCommand.Value);
                                        break;

                                    case OptionCommand optionCommand:
                                        this.options.Add(optionCommand);
                                        break;

                                    case UciOkCommand uciOkCommand:
                                        state = State.UciReady;
                                        this.optionsSource.TrySetResult(this.options.AsReadOnly());
                                        otherTask = commandTask = Command();
                                        break;

                                    default:
                                        this.RaiseUnhandledCommand(command);
                                        break;
                                }

                                break;

                            case State.Readying:
                                switch (command)
                                {
                                    case ReadyOkCommand readyOkCommand:
                                        state = State.Ready;
                                        break;

                                    default:
                                        this.RaiseUnhandledCommand(command);
                                        break;
                                }

                                break;

                            case State.Ready:
                                this.RaiseUnhandledCommand(command);
                                break;

                            default:
                                this.RaiseUnhandledCommand(command);
                                break;
                        }
                    }
                }

                Write(QuitCommand.Instance);
            }
            catch (Exception ex)
            {
                this.nameSource.TrySetException(ex);
                this.authorSource.TrySetException(ex);
                this.optionsSource.TrySetException(ex);
            }
            finally
            {
                this.engineProcess.Dispose();
            }
        }
    }
}
