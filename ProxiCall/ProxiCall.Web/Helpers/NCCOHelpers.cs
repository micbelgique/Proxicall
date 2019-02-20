﻿using Newtonsoft.Json.Linq;
using System.IO;

namespace ProxiCall.Web.Helpers
{
    public class NCCOHelpers
    {
        public void CreateTalkNCCO(string rootpath, string callText, string callVoice)
        {
            dynamic TalkNCCO = new JObject();
            TalkNCCO.action = "talk";
            TalkNCCO.text = callText;
            TalkNCCO.voiceName = callVoice;

            var pathToFile = Path.Combine(rootpath, "TalkNCCO.json");
            using (StreamWriter s = File.CreateText(pathToFile))
            {
                s.Write(TalkNCCO.ToString());
            }
        }

        public void CreateStreamNCCO(string rootpath, string[] streamUrl, int level, bool bargeIn, int loopTimes)
        {
            dynamic StreamNCCO = new JObject();
            StreamNCCO.action = "stream";
            StreamNCCO.streamUrl = new JArray { streamUrl };
            StreamNCCO.level = level;
            StreamNCCO.bargeIn = bargeIn;
            StreamNCCO.loop = loopTimes;

            var pathToFile = Path.Combine(rootpath, "StreamNCCO.json");
            using (StreamWriter s = File.CreateText(pathToFile))
            {
                s.Write(StreamNCCO.ToString());
            }
        }
    }
}
