using EasyDump;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EasyDump
{
    public partial class Dumper
    {
        /// <summary>
        /// The dump type determines the kinds of information that are collected from the process.
        /// </summary>
        public enum DumpTypeOption
        {
            Full,       // The largest dump containing all memory including the module images.

            Heap,       // A large and relatively comprehensive dump containing module lists, thread lists, all 
                        // stacks, exception information, handle information, and all memory except for mapped images.

            Mini,       // A small dump containing module lists, thread lists, exception information and all stacks.
        }

        public Dumper()
        {
        }

        public int Collect(int processId, string output, bool diag, DumpTypeOption type)
        {
            if (processId == 0)
            {
                Console.Error.WriteLine("ProcessId is required.");
                return 1;
            }

            try
            {
                if (output == null)
                {
                    // Build timestamp based file path
                    string timestamp = $"{DateTime.Now:yyyyMMdd_HHmmss}";
                    output = Path.Combine(Directory.GetCurrentDirectory(), RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"dump_{timestamp}.dmp" : $"core_{timestamp}");
                }
                // Make sure the dump path is NOT relative. This path could be sent to the runtime 
                // process on Linux which may have a different current directory.
                output = Path.GetFullPath(output);

                // Display the type of dump and dump path
                string dumpTypeMessage = null;
                switch (type)
                {
                    case DumpTypeOption.Full:
                        dumpTypeMessage = "full";
                        break;
                    case DumpTypeOption.Heap:
                        dumpTypeMessage = "dump with heap";
                        break;
                    case DumpTypeOption.Mini:
                        dumpTypeMessage = "dump";
                        break;
                }
                Console.Out.WriteLine($"Writing {dumpTypeMessage} to {output}");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Get the process
                    Process process = Process.GetProcessById(processId);

                    Windows.CollectDump(process, output, type);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var client = new DiagnosticsClient(processId);

                    DumpType dumpType = DumpType.Normal;
                    switch (type)
                    {
                        case DumpTypeOption.Full:
                            dumpType = DumpType.Full;
                            break;
                        case DumpTypeOption.Heap:
                            dumpType = DumpType.WithHeap;
                            break;
                        case DumpTypeOption.Mini:
                            dumpType = DumpType.Normal;
                            break;
                    }

                    // Send the command to the runtime to initiate the core dump
                    client.WriteDump(dumpType, output, diag);
                }
                else
                {
                    throw new PlatformNotSupportedException($"Unsupported operating system: {RuntimeInformation.OSDescription}");
                }
            }
            catch (Exception ex) when
                (ex is FileNotFoundException ||
                 ex is DirectoryNotFoundException ||
                 ex is UnauthorizedAccessException ||
                 ex is PlatformNotSupportedException ||
                 ex is InvalidDataException ||
                 ex is InvalidOperationException ||
                 ex is NotSupportedException ||
                 ex is DiagnosticsClientException)
            {
                Console.Error.WriteLine($"{ex.Message}");
                return 1;
            }

            Console.Out.WriteLine($"Complete");
            return 0;
        }
    }
}
