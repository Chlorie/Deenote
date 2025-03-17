#nullable enable

using Deenote.UIFramework.Controls;
using TMPro;
using UnityEngine;

namespace Deenote.UIFramework
{
    [CreateAssetMenu(
      fileName = nameof(UIThemeResources),
      menuName = $"Deenote.UIFramework/{nameof(UIThemeResources)}")]
    public sealed class UIThemeResources : ScriptableObject
    {
        [Header("Font")]
        public string PreferedFontName = default!;
        public string[] FallbackFontNames = default!;
        public TMP_FontAsset FinalFallbackFont = default!;
        [Header("CheckBox")]
        public Sprite CheckBoxCheckedIcon = default!;
        public Sprite CheckBoxIndeterminateIcon = default!;

        [Header("Prefabs")]
        public DropdownItem DropdownItemPrefab = default!;
    }
}