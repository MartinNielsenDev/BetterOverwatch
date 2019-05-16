namespace BetterOverwatch.DataObjects
{
    class ServerOutput
    {
        public class TokensOutput
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string privateToken { get; set; }
            public string publicToken { get; set; }
            public bool isLinked { get; set; }
        }
        public class OffsetOutput
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string offset { get; set; }
        }
    }
}
