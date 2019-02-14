using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Console_Speech.Services
{
    class FormatConvertor
    {
        public static async Task TurnAudioStreamToFile(byte[] bytes, string path)
        {
            using (MemoryStream audiostream = new MemoryStream(bytes))
            {
                Console.WriteLine("Your speech file is being written to file...");
                var pathCombined = Path.Combine(path, $"{ Guid.NewGuid() }.wav");
                using (var fileStream = new FileStream(pathCombined, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    await audiostream.CopyToAsync(fileStream).ConfigureAwait(false);
                    fileStream.Close();
                }
                Console.WriteLine("\nYour file is ready.");
            }
        }
    }
}
