using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NEmplode.IO;

namespace NEmplode.MSBuild.Tasks
{
    public sealed class ExecProcess : Task, ICancelableTask
    {
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        [Required]
        public string FileName { get; set; }

        [Required]
        public string Arguments { get; set; }

        public override bool Execute()
        {
            using (
                var process = new CapturedProcess(FileName, Arguments))
            {
                process.LogMessage += (sender, e) => LogMessage(e.Message);
                process.OutputDataReceived += (sender, e) => LogOutput(e.Data);
                process.ErrorDataReceived += (sender, e) => LogError(e.Data);

                return process.Run(_cancel.Token);
            }
        }

        private void LogMessage(string message)
        {
            if (message != null)
                Log.LogMessage(message);
        }

        private void LogOutput(string message)
        {
            if (message != null)
                Log.LogMessage(MessageImportance.Normal, message, new object[] { });
        }

        private void LogError(string message)
        {
            if (message != null)
                Log.LogError(message, new object[] { });
        }

        public void Cancel()
        {
            Log.LogMessage("Cancelling...");
            _cancel.Cancel();
        }
    }
}