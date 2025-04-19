#nullable enable

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Plugin
{
    public sealed class DelegatePlugin : IDeenotePlugin
    {
        private readonly object _name;
        private (string, string)[]? _localizedNames;
        private readonly Delegate _func;

        public Func<string, string>? DescriptionGetter { get; init; }

        public DelegatePlugin(string name, DeenotePluginExecution func)
        {
            _name = name;
            _func = func;
        }

        public DelegatePlugin(string name, Func<DeenotePluginContext, UniTask> func)
        {
            _name = name;
            _func = func;
        }

        public DelegatePlugin(Func<string, string> nameGetter, DeenotePluginExecution func)
        {
            _name = nameGetter;
            _func = func;
        }

        public DelegatePlugin(Func<string, string> nameGetter, Func<DeenotePluginContext, UniTask> func)
        {
            _name = nameGetter;
            _func = func;
        }

        public DelegatePlugin(string name, (string, string)[] localizedNames, DeenotePluginExecution func)
        {
            _name = name;
            _localizedNames = localizedNames;
            _func = func;
        }

        public DelegatePlugin(string name, (string, string)[] localizedNames, Func<DeenotePluginContext, UniTask> func)
        {
            _name = name;
            _localizedNames = localizedNames;
            _func = func;
        }

        public string GetName(string languageCode)
        {
            if (_name is Func<string, string> getter)
                return getter.Invoke(languageCode);
            else {
                if (_localizedNames is not null) {
                    foreach (var n in _localizedNames) {
                        if (n.Item1 == languageCode)
                            return n.Item2;
                    }
                }
                Debug.Assert(_name is string);
                return _name.ToString();
            }
        }
        public string? GetDescription(string languageCode) => DescriptionGetter?.Invoke(languageCode);

        public UniTask ExecuteAsync(DeenotePluginContext context, DeenotePluginArgs args)
        {
            if (_func is Func<DeenotePluginContext, UniTask> contextFunc)
                return contextFunc.Invoke(context);
            else {
                var func = (DeenotePluginExecution)_func;
                return func.Invoke(context, args);
            }
        }
    }
}