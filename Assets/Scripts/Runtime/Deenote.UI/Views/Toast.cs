#nullable enable

using Deenote.Localization;
using Deenote.UIFramework.Controls;
using System.Threading;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class Toast : MonoBehaviour
    {
        [SerializeField] ToastManager _toastManager = default!;
        [SerializeField] Button _closeButton = default!;
        [SerializeField] TextBlock _text = default!;

        internal int Uid { get; private set; }

        private void Awake()
        {
            _closeButton.Clicked += () =>
            {
                _toastManager.CloseToast(this);
            };
        }

        internal void OnInstantiate(ToastManager manager)
        {
            _toastManager = manager;
        }

        internal void Initialize(int uid, LocalizableText text)
        {
            Uid = uid;
            _text.SetText(text);
        }

        private void OnValidate()
        {
            _toastManager ??= transform.GetComponentInParent<ToastManager>();
        }
    }
}