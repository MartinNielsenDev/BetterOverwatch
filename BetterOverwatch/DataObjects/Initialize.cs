namespace BetterOverwatch.DataObjects
{
    internal class Initalize
    {
        public Initalize(string version, string host, string gitHubHost)
        {
            Version = version;
            Host = host;
            GitHubHost = gitHubHost;
        }
        public string Version { get; } = string.Empty;
        public string Host { get; } = string.Empty;
        public string GitHubHost { get; } = string.Empty;
    }
}
