using System;

namespace NZ.RdpMaid.App.Exceptions
{
    public class AppSettingsNotFoundException : Exception
    {
        public string Hint { get; }

        public AppSettingsNotFoundException(string message, string hint)
            : base(message)
        {
            Hint = hint;
        }
    }
}
