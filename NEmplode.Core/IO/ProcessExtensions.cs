using System;
using System.Diagnostics;
using System.Threading;

namespace NEmplode.IO
{
    internal static class ProcessExtensions
    {
        internal static void TerminateProcess(Process process)
        {
            try
            {
                process.CancelOutputRead();
                process.CancelErrorRead();
            }
            catch (InvalidOperationException)
            {
                // Eat it.
            }

            KillProcessTree(process);

            const int millisecondsToWaitForExitAfterKill = 5000;
            process.WaitForExit(millisecondsToWaitForExitAfterKill);
        }

        private static void KillProcessTree(Process process)
        {
            try
            {
                // TODO: Kill the process tree.
                process.Kill();
            }
            catch (InvalidOperationException)
            {
                // Eat it.
            }
        }

        internal static void WaitForExit(Process process)
        {
            process.WaitForExit();
            while (!process.HasExited)
                Thread.Sleep(50);

            try
            {
                process.CancelOutputRead();
                process.CancelErrorRead();
            }
            catch (InvalidOperationException)
            {
                // Eat it.
            }
        }
    }
}