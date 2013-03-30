using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NEmplode.IO;

namespace NEmplode.MSBuild.Tasks
{
    public sealed class Pipe : Task, ICancelableTask
    {
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        [Required]
        public string SourceFileName { get; set; }

        [Required]
        public string SourceArguments { get; set; }

        [Required]
        public string DestinationFileName { get; set; }

        [Required]
        public string DestinationArguments { get; set; }

        public override bool Execute()
        {
            using (
                var pipeline = new Pipeline(SourceFileName, SourceArguments, DestinationFileName, DestinationArguments))
            {
                pipeline.LogMessage += (sender, e) => LogMessage(e.Message);
                pipeline.OutputDataReceived += (sender, e) => LogOutput(e.Data);
                pipeline.ErrorDataReceived += (sender, e) => LogError(e.Data);

                return pipeline.Run(_cancel.Token);
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