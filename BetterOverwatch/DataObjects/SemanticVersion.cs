namespace BetterOverwatch.DataObjects
{
    class SemanticVersion
    {
        public int major;
        public int minor;
        public int patch;

        public SemanticVersion(string rawVersion)
        {
            string[] versions = rawVersion.Split('.');

            if (versions.Length == 3)
            {
                int.TryParse(versions[0], out major);
                int.TryParse(versions[1], out minor);
                int.TryParse(versions[2], out patch);
            }
        }
        public bool IsNewerThan(SemanticVersion version)
        {
            if (major > version.major)
            {
                return true;
            }
            if (major == version.major)
            {
                if (minor > version.minor)
                {
                    return true;
                }
                if (minor == version.minor && patch > version.patch)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
