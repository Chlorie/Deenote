#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Localization
{
    public struct ArgedLocalizableText
    {
        LocalizableText _localizableText;
        object? _args;

        public readonly LocalizableText LocalizableText => _localizableText;

        public readonly bool IsLocalized => _localizableText.IsLocalized;
        public readonly string TextOrKey => _localizableText.TextOrKey;

        public readonly ReadOnlySpan<string> Args
        {
            get {
                if (_args is null)
                    return ReadOnlySpan<string>.Empty;
                if (_args is string[] arr)
                    return arr.AsSpan();

                Debug.Assert(_args is string);
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<object, string>(ref Unsafe.AsRef(in _args)), 1);
            }
        }

        private ArgedLocalizableText(LocalizableText text, object? args)
        {
            _localizableText = text;
            _args = args;
            Debug.Assert(args is null or string or string[]);
        }

        public static ArgedLocalizableText Raw(string text) => new(LocalizableText.Raw(text), null);
        public static ArgedLocalizableText Localized(string textKey) => new(LocalizableText.Localized(textKey), null);
        public static ArgedLocalizableText Localized(string textKey, string arg0) => new(LocalizableText.Localized(textKey), arg0);
        public static ArgedLocalizableText Localized(string textKey, params string[] args) => new(LocalizableText.Localized(textKey), args);

        public override readonly bool Equals(object? obj) => obj is ArgedLocalizableText text && this == text;
        public override readonly int GetHashCode() => HashCode.Combine(_localizableText, _args);

        public static implicit operator ArgedLocalizableText(LocalizableText text) => new(text, null);

        public static bool operator ==(ArgedLocalizableText left, ArgedLocalizableText right)
        {
            if (left._localizableText != right._localizableText)
                return false;

            if (left._args == right._args)
                return true;

            // Deep equal
            switch (left._args, right._args) {
                case (string l, string r):
                    return l == r;
                case (string[] { Length: 1 } l, string r):
                    return l[0] == r;
                case (string l, string[] { Length: 1 } r):
                    return l == r[0];
                case (string[] l, string[] r):
                    return l.AsSpan().SequenceEqual(r);
                default:
                    return false;
            }
        }

        public static bool operator !=(ArgedLocalizableText left, ArgedLocalizableText right) => !(left == right);

        public static bool operator ==(ArgedLocalizableText left, string right) => left == Raw(right);
        public static bool operator !=(ArgedLocalizableText left, string right) => left != Raw(right);

        public static bool operator ==(ArgedLocalizableText left, LocalizableText right) => left == new ArgedLocalizableText(right, null);
        public static bool operator !=(ArgedLocalizableText left, LocalizableText right) => left != new ArgedLocalizableText(right, null);
    }
}