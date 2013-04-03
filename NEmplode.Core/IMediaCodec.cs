using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NEmplode
{
    internal interface IMediaCodec
    {
        Stream InputStream { get; }
        Stream OutputStream { get; }

        Task Start(CancellationToken cancellationToken);
        
        event ErrorDataReceivedEventHandler ErrorDataReceived;
    }
}