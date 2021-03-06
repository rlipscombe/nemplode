﻿using System;
using System.Diagnostics;

namespace NEmplode.Processes
{
    internal static class ProcessExtensions
    {
        public static void Terminate(Process process)
        {
            try
            {
                process.Kill();
                process.WaitForExit(1000);
            }
            catch (Exception)
            {
                // Ignore it.
            }
        }
    }
}