//
// Copyright (c) 2010-2026 Antmicro
// Copyright (c) 2011-2015 Realtime Embedded
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using Antmicro.OptionsParser;

namespace Antmicro.Renode.UI
{
    public class Options : IValidatedOptions
    {
        public bool Validate(out string error)
        {
            if(HideMonitor && Console)
            {
                error = "--hide-monitor and --console cannot be set at the same time";
                return false;
            }

            if(UI)
            {
                if(Console)
                {
                    error = "--ui and --console cannot be set at the same time";
                    return false;
                }
                ServerMode = true;
            }
            else if(DisableXwt)
            {
                HideMonitor = true;
            }

            if(ServerMode && !UI)
            {
                DisableXwt = true;
            }
            else if(ServerModePort != 21234 || !string.Equals(ServerModeWorkDir, "working-dir"))
            {
                error = "--server-mode-port and --server-mode-work-dir options are allowed only if --server-mode is enabled";
                return false;
            }
            if(ServerMode && RobotDebug)
            {
                error = "--server-mode (or --ui) and --robot-debug-on-error cannot be set at the same time";
                return false;
            }

            if(DebugSession)
            {
                if(string.IsNullOrEmpty(Elf) || GdbPort < 1 || GdbPort > 65535)
                {
                    error = "--model needs --elf and a --gdb-port between 1 and 65535";
                    return false;
                }
                if(MemoryPort == 0 || MemoryPort > 65535 || MemoryPort == GdbPort)
                {
                    error = "--memory-port must be between 1 and 65535 and differ from --gdb-port";
                    return false;
                }
                if(Console || UI || ServerMode || Port != null || FilePath != null || (Execute != null && Execute.Length > 0)
                    || RobotFrameworkRemoteServerPort != -1 || RobotDebug)
                {
                    error = "--model runs a debug session on its own; drop --console, --ui, --server-mode, --port, --robot-server-port, --robot-debug-on-error, -e and the script file";
                    return false;
                }
                // Resolved now: starting the Monitor later changes the working directory.
                try
                {
                    Elf = System.IO.Path.GetFullPath(Elf);
                }
                catch(System.Exception e) when(e is System.ArgumentException || e is System.NotSupportedException || e is System.IO.PathTooLongException)
                {
                    error = $"--elf: {e.Message}";
                    return false;
                }
                // A debug session is driven through GDB and the memory port only: no monitor
                // window and no monitor socket (which would otherwise default to port 1234).
                DisableXwt = true;
                HideMonitor = true;
                Port = -1;
            }
            else if(Elf != null || GdbPort != -1 || MemoryPort != -1)
            {
                error = "--elf, --gdb-port and --memory-port need --model";
                return false;
            }

            error = null;
            return true;
        }

        [Name('p', "plain"), DefaultValue(false), Description("Remove steering codes (e.g., colours) from output.")]
        public bool Plain { get; set; }

        [Name('P', "port"), Description("Instead of opening a window, listen for Monitor commands on the specified port. Specify 0 to automatically assign an unused port. Specify -1 to disable both the port and the GUI monitor.")]
        public int? Port { get; set; }

        [Name('e', "execute"), Description("Execute command on startup (executed after the optional script). May be used many times.")]
        public string[] Execute { get; set; }

        [Name("config"), Description("Use the configuration file from the provided path, or create one if it does not exist")]
        public string ConfigFile { get; set; }

        [Alias("disable-xwt"), Name("disable-gui"), DefaultValue(false), Description("Disable GUI support. It automatically sets HideMonitor.")]
        public bool DisableXwt { get; set; }

        [Name("ui"), DefaultValue(false), Description("Use the new Neutralino-based UI. Experimental!")]
        public bool UI { get; set; }

        [Name("file-to-include / snapshot"), PositionalArgument(0)]
        public string FilePath { get; set; }

        [Name("hide-monitor"), DefaultValue(false), Description("Do not show the Monitor window.")]
        public bool HideMonitor { get; set; }

        [Name("hide-log"), DefaultValue(false), Description("Do not show log messages in a console.")]
        public bool HideLog { get; set; }

        [Name("hide-analyzers"), DefaultValue(false), Description("Do not show analyzers.")]
        public bool HideAnalyzers { get; set; }

        [Name("pid-file"), Description("Write PID of the Renode instance to the provided file.")]
        public string PidFile { get; set; }

        [Name("robot-server-port"), DefaultValue(-1), Description("Start robot framework remote server on the specified port.")]
        public int RobotFrameworkRemoteServerPort { get; set; }

        [Name("robot-debug-on-error"), DefaultValue(false), Description("Initialize GUI for Robot tests debugging")]
        public bool RobotDebug { get; set; }

        [Name('v', "version"), DefaultValue(false), Description("Print version and exit.")]
        public bool Version { get; set; }

        [Name("model"), Description("Start a debug session on this model (a name from --list-models). Needs --elf and --gdb-port.")]
        public string Model { get; set; }

        [Name("elf"), Description("Debug session: the firmware ELF to load. Taken as given, so paths with spaces work.")]
        public string Elf { get; set; }

        [Name("gdb-port"), DefaultValue(-1), Description("Debug session: GDB server port on 127.0.0.1.")]
        public int GdbPort { get; set; }

        [Name("memory-port"), DefaultValue(-1), Description("Debug session: optional live memory port on 127.0.0.1 (one request per line: 'r <addr> <len>', 'w <addr> <hex>').")]
        public int MemoryPort { get; set; }

        [Name("list-models"), DefaultValue(false), Description("Print the models a debug session supports as a JSON array and exit.")]
        public bool ListModels { get; set; }

        [Name("include-incomplete"), DefaultValue(false), Description("With --list-models, also list models known to be incomplete.")]
        public bool IncludeIncomplete { get; set; }

        public bool DebugSession => Model != null;

        [Name("console"), Description("Run the Monitor in the console instead of a separate window")]
        public bool Console { get; set; }

        [Name("keep-temporary-files"), Description("Don't clean temporary files on exit")]
        public bool KeepTemporaryFiles { get; set; }

        [Name("server-mode"), DefaultValue(false), Description("Instead of opening a window, expose localhost:port/proxy endpoint")]
        public bool ServerMode { get; set; }

        [Name("server-mode-port"), DefaultValue(21234), Description("Set port for server mode")]
        public int ServerModePort { get; set; }

        [Name("server-mode-work-dir"), DefaultValue("working-dir"), Description("Set working directory for server mode (relative for renode)")]
        public string ServerModeWorkDir { get; set; }
    }
}
