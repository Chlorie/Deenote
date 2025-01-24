#nullable enable
#pragma warning disable CS0649

using System;
using System.Runtime.CompilerServices;

namespace Deenote.Library.Components
{
    public readonly struct DisposableGuard : IDisposable
    {
        private readonly IDisposable? _disposable;
        public void Dispose() => _disposable?.Dispose();
    }

    public static class DisposableGuardExt
    {
        public static T Set<T>(this in DisposableGuard scope, T value) where T : IDisposable
        {
            ref IDisposable? refv = ref Unsafe.As<DisposableGuard, IDisposable?>(ref Unsafe.AsRef(in scope));
            refv = value;
            return value;
        }
    }
}