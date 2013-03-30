open System
open System.Runtime.InteropServices

type FLAC__StreamDecoder = IntPtr
type FLAC__StreamDecoderReadStatus = int

[<DllImport("libflac.dll", CallingConvention = CallingConvention.Cdecl)>]
extern FLAC__StreamDecoder FLAC__stream_decoder_new();

[<DllImport("libflac.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void FLAC__stream_decoder_delete(FLAC__StreamDecoder decoder);

type FLAC__StreamDecoderReadCallback = delegate of decoder:FLAC__StreamDecoder * buffer:byte[] * bytes:IntPtr * client_data:IntPtr -> FLAC__StreamDecoderReadStatus
type FLAC__StreamDecoderSeekCallback = delegate of decoder:FLAC__StreamDecoder * absolute_byte_offset:UInt64 * client_data:IntPtr -> FLAC__StreamDecoderReadStatus
type FLAC__StreamDecoderTellCallback = delegate of decoder:FLAC__StreamDecoder * buffer:byte[] * bytes:IntPtr * client_data:IntPtr -> FLAC__StreamDecoderReadStatus
type FLAC__StreamDecoderLengthCallback = delegate of decoder:FLAC__StreamDecoder * buffer:byte[] * bytes:IntPtr * client_data:IntPtr -> FLAC__StreamDecoderReadStatus
type FLAC__StreamDecoderEofCallback = delegate of decoder:FLAC__StreamDecoder * buffer:byte[] * bytes:IntPtr * client_data:IntPtr -> FLAC__StreamDecoderReadStatus
type FLAC__StreamDecoderWriteCallback = delegate of decoder:FLAC__StreamDecoder * buffer:byte[] * bytes:IntPtr * client_data:IntPtr -> FLAC__StreamDecoderReadStatus
type FLAC__StreamDecoderMetadataCallback = delegate of decoder:FLAC__StreamDecoder * buffer:byte[] * bytes:IntPtr * client_data:IntPtr -> FLAC__StreamDecoderReadStatus
type FLAC__StreamDecoderErrorCallback = delegate of decoder:FLAC__StreamDecoder * buffer:byte[] * bytes:IntPtr * client_data:IntPtr -> FLAC__StreamDecoderReadStatus

[<DllImport("libflac.dll", CallingConvention = CallingConvention.Cdecl)>]
extern FLAC__StreamDecoderReadStatus
    FLAC__stream_decoder_init_stream(
        FLAC__StreamDecoder decoder,
        FLAC__StreamDecoderReadCallback read_callback,
        FLAC__StreamDecoderSeekCallback seek_callback,
        FLAC__StreamDecoderTellCallback tell_callback,
        FLAC__StreamDecoderLengthCallback length_callback,
        FLAC__StreamDecoderEofCallback eof_callback,
        FLAC__StreamDecoderWriteCallback write_callback,
        FLAC__StreamDecoderMetadataCallback metadata_callback,
        FLAC__StreamDecoderErrorCallback error_callback,
        IntPtr client_data);

[<EntryPoint>]
let main argv = 
    let ReadCallback decoder buffer bytes client_data =
        0

    let SeekCallback decoder (absolute_byte_offset:UInt64) client_data =
        0

    let decoder = FLAC__stream_decoder_new()
    printfn "%A" decoder

    let read_callback = FLAC__StreamDecoderReadCallback(ReadCallback)
    let seek_callback = FLAC__StreamDecoderSeekCallback(SeekCallback)
    let tell_callback = FLAC__StreamDecoderTellCallback(TellCallback)
    let length_callback = FLAC__StreamDecoderLengthCallback(LengthCallback)
    let eof_callback = FLAC__StreamDecoderEofCallback(EofCallback)
    let write_callback = FLAC__StreamDecoderWriteCallback(WriteCallback)
    let metadata_callback = FLAC__StreamDecoderMetadataCallback(MetadataCallback)
    let error_callback = FLAC__StreamDecoderErrorCallback(ErrorCallback)

    let client_data = IntPtr.Zero
    let status = FLAC__stream_decoder_init_stream(decoder, read_callback, seek_callback, tell_callback, length_callback, eof_callback, write_callback, metadata_callback, error_callback, client_data)

    FLAC__stream_decoder_delete decoder

    0 // return an integer exit code
