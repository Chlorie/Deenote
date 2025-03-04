#nullable enable

using Cysharp.Threading.Tasks;
using System;

namespace Deenote.Plugin
{
    public sealed class DelegatePlugin : IDeenotePlugin
    {
        private readonly Func<DeenotePluginContext, UniTask> _func;

        public DelegatePlugin(string name, Func<DeenotePluginContext, UniTask> func)
        {
            Name = name;
            _func = func;
            Description = null!;
        }

        public string Name { get; private set; }

        public string? Description { get; init; }

        public UniTask ExecuteAsync(DeenotePluginContext context) => _func.Invoke(context);
    }
}