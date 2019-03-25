using System;
using System.IO;
using System.Threading.Tasks;

namespace ProxiCall.Web.Services
{
    class FormatConvertor
    {
        public static async Task TurnAudioURLStreamToFile(string url, string path)
        {
            byte[] audioData = null;
            using (var wc = new System.Net.WebClient())
            {
                audioData = wc.DownloadData(url);
            }

            using (MemoryStream audiostream = new MemoryStream(audioData))
            {
                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    await audiostream.CopyToAsync(fileStream).ConfigureAwait(false);
                    fileStream.Close();
                }
            }
        }

        public static async Task TurnAudioStreamToFile(byte[] bytes, string path)
        {
            using (MemoryStream audiostream = new MemoryStream(bytes))
            {
                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    await audiostream.CopyToAsync(fileStream).ConfigureAwait(false);
                    fileStream.Close();
                }
            }
        }
    }
}
