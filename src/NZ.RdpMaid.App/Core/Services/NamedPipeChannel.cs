using System;
using System.IO.Pipes;
using System.Linq;
using System.Threading;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class NamedPipeChannel : IDisposable
    {
        private const string _localMachine = ".";
        private const string _pipeName = "NZ.RdpMaid.Pipe";

        private static readonly byte[] _activateMainWindowMessage = [1, 3, 3, 7];

        private CancellationTokenSource? _cancellationTokenSource = new();
        private NamedPipeServerStream? _listener;

        public NamedPipeChannel()
        {
            _listener = new NamedPipeServerStream(
                pipeName: _pipeName,
                direction: PipeDirection.In,
                maxNumberOfServerInstances: 1
            );
        }

        public static void SendActivateMainWindowMessage()
        {
            using var pipe = new NamedPipeClientStream(
                serverName: _localMachine,
                pipeName: _pipeName,
                direction: PipeDirection.Out,
                options: PipeOptions.None,
                impersonationLevel: System.Security.Principal.TokenImpersonationLevel.None
            );

            pipe.Connect(TimeSpan.FromSeconds(2));
            pipe.Write(_activateMainWindowMessage);
        }

        public async void AddActivateMainWindowMessageHandler(Action handler)
        {
            ArgumentNullException.ThrowIfNull(handler, nameof(handler));

            if (_listener is null || _cancellationTokenSource is null)
            {
                throw new ObjectDisposedException(nameof(NamedPipeChannel));
            }

            var ct = _cancellationTokenSource.Token;
            var buffer = new byte[_activateMainWindowMessage.Length];

            while (!ct.IsCancellationRequested && _listener is not null)
            {
                await _listener.WaitForConnectionAsync(ct);
                await _listener.ReadExactlyAsync(buffer, ct);

                if (_listener.IsConnected)
                {
                    _listener.Disconnect();
                }

                if (Enumerable.SequenceEqual(buffer, _activateMainWindowMessage))
                {
                    handler();
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = null;
            }
            catch { }

            try
            {
                _listener?.Dispose();
                _listener = null;
            }
            catch { }
        }
    }
}