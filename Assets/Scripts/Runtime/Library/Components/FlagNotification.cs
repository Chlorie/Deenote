#nullable enable

using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Library.Components
{
    public interface IFlagNotifiable<TSelf, TFlag> where TSelf : IFlagNotifiable<TSelf, TFlag>
    {
        void RegisterNotification(TFlag flag, Action<TSelf> action);
        void UnregisterNotification(TFlag flag, Action<TSelf> action);
    }

    public static class IFlagNotifiableExt
    {
        public static void RegisterNotificationAndInvoke<TSelf, TFlag>(this TSelf self, TFlag flag, Action<TSelf> action)
            where TSelf : IFlagNotifiable<TSelf, TFlag>
        {
            self.RegisterNotification(flag, action);
            action.Invoke(self);
        }

        public static void RegisterNotification<TSelf, TFlag>(this TSelf self, TFlag flag0, TFlag flag1, Action<TSelf> action)
            where TSelf : IFlagNotifiable<TSelf, TFlag>
        {
            self.RegisterNotification(flag0, action);
            self.RegisterNotification(flag1, action);
        }

        public static void UnregisterNotification<TSelf, TFlag>(this TSelf self, TFlag flag0, TFlag flag1, Action<TSelf> action)
            where TSelf : IFlagNotifiable<TSelf, TFlag>
        {
            self.UnregisterNotification(flag0, action);
            self.UnregisterNotification(flag1, action);
        }
    }

    public abstract class FlagNotifiable<TSelf, TFlag> : IFlagNotifiable<TSelf, TFlag>
        where TSelf : FlagNotifiable<TSelf, TFlag>
    {
        private readonly List<(TFlag, Action<TSelf>)> _invokers = new();

        public void RegisterNotification(TFlag flag, Action<TSelf> action) => _invokers.Add((flag, action));
        public void UnregisterNotification(TFlag flag, Action<TSelf> action) => _invokers.Remove((flag, action));

        protected void NotifyFlag(TFlag flag)
        {
            if (_invokers is null)
                return;

            if (typeof(TFlag).IsValueType) {
                var self = (TSelf)this;
                foreach (var (f, invoker) in _invokers) {
                    if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                        invoker.Invoke(self);
                }
            }
            else {
                var comparer = EqualityComparer<TFlag>.Default;
                var self = (TSelf)this;
                foreach (var (f, invoker) in _invokers) {
                    if (comparer.Equals(f, flag))
                        invoker.Invoke(self);
                }
            }
        }
    }

    public abstract class FlagNotifiableMonoBehaviour<TSelf, TFlag> : MonoBehaviour, IFlagNotifiable<TSelf, TFlag>
        where TSelf : FlagNotifiableMonoBehaviour<TSelf, TFlag>
    {
        private readonly List<(TFlag, Action<TSelf>)> _invokers = new();

        public void RegisterNotification(TFlag flag, Action<TSelf> action) => _invokers.Add((flag, action));
        public void UnregisterNotification(TFlag flag, Action<TSelf> action) => _invokers.Remove((flag, action));

        protected void NotifyFlag(TFlag flag)
        {
            if (_invokers is null)
                return;

            if (typeof(TFlag).IsValueType) {
                var self = (TSelf)this;
                // Use .AsSpan() to avoid exception when callback modifying invocation list,
                // which is acceptable as the new invocations may has differenct flags
                foreach (var (f, invoker) in _invokers.AsSpan()) {
                    if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                        invoker.Invoke(self);
                }
            }
            else {
                var comparer = EqualityComparer<TFlag>.Default;
                var self = (TSelf)this;
                foreach (var (f, invoker) in _invokers.AsSpan()) {
                    if (comparer.Equals(f, flag))
                        invoker.Invoke(self);
                }
            }
        }
    }

    public struct FlagNotifier<TSender, TFlag>
    {
        public List<(TFlag, Action<TSender>)>? _invokers;

        public void AddListener(TFlag flag, Action<TSender> action)
        {
            (_invokers ??= new()).Add((flag, action));
        }

        public readonly void Invoke(TSender sender, TFlag flag)
        {
            if (_invokers is null)
                return;

            if (typeof(TFlag).IsValueType) {
                foreach (var (f, invoker) in _invokers.AsSpan()) {
                    if (EqualityComparer<TFlag>.Default.Equals(flag, f))
                        invoker.Invoke(sender);
                }
            }
            else {
                var comparer = EqualityComparer<TFlag>.Default;
                foreach (var (f, invoker) in _invokers.AsSpan()) {
                    if (comparer.Equals(f, flag))
                        invoker.Invoke(sender);
                }
            }
        }
    }
}