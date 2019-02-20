using System.IO;
using System.Threading;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Console_Speech.Services.Speech
{
    public class VoiceAudioInputStream : PullAudioInputStreamCallback
    {
        private readonly Stream _dataStream = new MemoryStream();
        // TODO ? private ManualResetEvent _waitForEmptyDataStream = null;

        public VoiceAudioInputStream(Stream dataStream)
        {
            _dataStream = dataStream;
        }

        public override int Read(byte[] dataBuffer, uint size)
        {
            return _dataStream.Read(dataBuffer, 0, dataBuffer.Length);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _dataStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            _dataStream.Dispose();
            base.Close();
        }
    }
}
