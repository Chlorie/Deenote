#nullable enable

using System;
using System.Diagnostics;
using System.Threading;

namespace Deenote.Library
{
    public sealed class ResetableCancellationTokenSource : IDisposable
    {
        private CancellationTokenSource? _cts;

        public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public void Cancel()
        {
            Debug.Assert(_cts != null);
            _cts!.Cancel();
        }

        public void Reset()
        {
            if (_cts is not null) {
                _cts.Cancel();
                _cts.Dispose();
            }
            _cts = new();
        }

        public void Dispose() => _cts?.Dispose();
    }
}