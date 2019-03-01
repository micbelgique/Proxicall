using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxiCall.Web.Models
{
    public class StreamAudioFileDTO
    {
        public StreamAudioFileDTO(string streamUrl, int loop, string level)
        {
            StreamUrl = streamUrl;
            Loop = loop;
            Level = level;
        }

        public string StreamUrl { get; }
        public int Loop { get; }
        public string Level { get; }
    }
}
