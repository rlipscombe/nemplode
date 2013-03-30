using System;

namespace NEmplode.IO
{
    public class ProcessOutputEventArgs : EventArgs
    {
        public ProcessOutputEventArgs(string data)
        {
            Data = data;
        }

        public string Data { get; private set; }
    }
}