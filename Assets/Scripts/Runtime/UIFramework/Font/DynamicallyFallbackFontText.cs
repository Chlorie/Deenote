#nullable enable

using TMPro;
using UnityEngine;

namespace Deenote.UIFramework.Font
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class DynamicallyFallbackFontText : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _text = default!;
        [SerializeField] DynamicallyFallbackFontAsset _fontAsset = default!;

        private void Awake()
        {
            _text.font = _fontAsset.FontAsset;
        }

        private void OnValidate()
        {
            _text ??= GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) {
                _text.font = _fontAsset.FontAsset;
            }
        }
    }
}