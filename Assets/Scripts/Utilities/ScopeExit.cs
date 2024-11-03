#nullable enable

using System;
using System.Threading.Tasks;

namespace Deenote.Utilities
{
    public readonly struct ScopeExit : IDisposable
    {
        public ScopeExit(Action cleanup) => _cleanup = cleanup;
        public void Dispose() => _cleanup();
        private readonly Action _cleanup;
    }

    public readonly struct AsyncScopeExit : IAsyncDisposable
    {
        public AsyncScopeExit(Func<ValueTask> cleanup) => _cleanup = cleanup;
        public ValueTask DisposeAsync() => _cleanup();
        private readonly Func<ValueTask> _cleanup;
    }
}