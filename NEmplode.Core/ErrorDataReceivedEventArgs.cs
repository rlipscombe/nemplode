namespace NEmplode
{
    public class ErrorDataReceivedEventArgs
    {
        public ErrorDataReceivedEventArgs(string data)
        {
            Data = data;
        }

        public string Data { get; private set; }
    }
}