#nullable enable

using Deenote.UIFramework.Controls;
using UnityEngine;

namespace Deenote.UIFramework
{
    [CreateAssetMenu(
      fileName = nameof(UIThemeResources),
      menuName = $"Deenote.UIFramework/{nameof(UIThemeResources)}")]
    public sealed class UIThemeResources : ScriptableObject
    {
        [Header("Icons")]
        [Header("CheckBox")]
        public Sprite CheckBoxCheckedIcon = default!;
        public Sprite CheckBoxIndeterminateIcon = default!;

        [Header("Prefabs")]
        public DropdownItem DropdownItemPrefab = default!;
    }
}