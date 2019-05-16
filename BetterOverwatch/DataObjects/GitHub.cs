using System.Collections.Generic;

namespace BetterOverwatch.DataObjects
{
    class GitHub
    {
        public class Json
        {
            public string tag_name { get; set; }
            public string body { get; set; }
            public List<Assets> assets { get; set; }
        }
        public class Assets
        {
            public string browser_download_url { get; set; }
            public int size { get; set; }
        }
    }
}
