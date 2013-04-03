namespace NEmplode.Codecs
{
    public class ErrorDataReceivedEventArgs
    {
        public ErrorDataReceivedEventArgs(string data)
        {
            Data = data;
        }

        public string Data { get; private set; }
    }

    public delegate void ErrorDataReceivedEventHandler(object sender, ErrorDataReceivedEventArgs e);
}