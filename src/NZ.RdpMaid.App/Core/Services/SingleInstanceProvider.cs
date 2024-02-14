using System;
using System.Threading;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class SingleInstanceProvider : IDisposable
    {
        private Mutex? _instanceMutex;

        public SingleInstanceProvider()
        {
            _instanceMutex = new(true, "NZ.RdpMaid.App.Instance", out bool isNew);
            IsRunning = !isNew;
        }

        public bool IsRunning { get; }

        public void Dispose()
        {
            _instanceMutex?.Dispose();
            _instanceMutex = null;
        }
    }
}