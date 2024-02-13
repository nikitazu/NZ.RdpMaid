using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NZ.RdpMaid.App.Core.Services;

internal class DebouncingTriggerService
{
    public delegate Task Callback(CancellationToken ct);

    private static readonly TimeSpan _triggerDelay = TimeSpan.FromSeconds(10);

    private readonly Dictionary<string, Callback> _callbacks = [];
    private readonly Dictionary<string, CancellationTokenSource> _triggers = [];

    public void Register(string key, Callback callback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));

        _callbacks.Add(key, callback);
    }

    public void Trigger(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        if (!_callbacks.TryGetValue(key, out var callback))
        {
            throw new KeyNotFoundException("Триггер не зарегистрирован");
        }

        if (_triggers.TryGetValue(key, out var oldCts))
        {
            _triggers.Remove(key);
            oldCts.Cancel();
        }

        var newCts = new CancellationTokenSource();

        _triggers.Add(key, newCts);

        Task.Delay(_triggerDelay, newCts.Token)
            .ContinueWith(async _ => await callback(newCts.Token), newCts.Token)
            .ContinueWith(_ => _triggers.Remove(key));
    }
}