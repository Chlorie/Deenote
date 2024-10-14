using System;
using System.Collections.Generic;

namespace Deenote.UI.ComponentModel
{
    public interface INotifyPropertyChange<TSelf, TFlag> where TSelf : INotifyPropertyChange<TSelf, TFlag>
    {
        void RegisterPropertyChangeNotification(TFlag flag, Action<TSelf> action);
    }

    public struct PropertyChangeNotifier<TSender, TFlag>
    {
        private List<(TFlag, Action<TSender>)>? _invokers;

        public void AddListener(TFlag flag, Action<TSender> action)
        {
            (_invokers ??= new()).Add((flag, action));
        }

        public readonly void Invoke(TSender sender, TFlag flag)
        {
            if (_invokers is null)
                return;

            foreach (var (f, invoker) in _invokers) {
                if (EqualityComparer<TFlag>.Default.Equals(f, flag))
                    invoker.Invoke(sender);
            }
        }

        public readonly void Invoke(TSender sender, TFlag flag1, TFlag flag2)
        {
            if (_invokers is null)
                return;

            foreach (var (f, invoker) in _invokers) {
                if (EqualityComparer<TFlag>.Default.Equals(f, flag1) || EqualityComparer<TFlag>.Default.Equals(f, flag2))
                    invoker.Invoke(sender);
            }
        }
    }
}