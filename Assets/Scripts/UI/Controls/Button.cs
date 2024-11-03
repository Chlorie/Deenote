#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityButton = UnityEngine.UI.Button;
using UnityImage = UnityEngine.UI.Image;

namespace Deenote.UI.Controls
{
    [RequireComponent(typeof(UnityButton))]
    public sealed class Button : MonoBehaviour
    {
        [SerializeField] UnityButton _button = default!;
        [SerializeField] UnityImage _image = default!;
        [SerializeField] LocalizedText _text = default!;

        public UnityButton UnityButton => _button;
        public UnityImage Image => _image;
        public LocalizedText Text => _text;

        public bool IsInteractable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        public UnityEvent OnClick => _button.onClick;

        public UniTask OnClickAsync(CancellationToken cancellationToken = default)
            => _button.OnClickAsync(cancellationToken);

        public IAsyncClickEventHandler GetAsyncClickEventHandler()
            => _button.GetAsyncClickEventHandler();
    }
}