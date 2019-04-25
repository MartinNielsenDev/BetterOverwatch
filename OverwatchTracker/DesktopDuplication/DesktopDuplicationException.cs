using System;

namespace BetterOverwatch.DesktopDuplication
{
    public class DesktopDuplicationException : Exception
    {
        public DesktopDuplicationException(string message)
            : base(message) { }
    }
}
