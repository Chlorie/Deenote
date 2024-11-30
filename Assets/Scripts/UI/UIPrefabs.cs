#nullable enable

using Deenote.UI.Controls;
using UnityEngine;

namespace Deenote.UI.Themes
{
    [CreateAssetMenu(
    fileName = nameof(UIPrefabs),
    menuName = $"{nameof(Deenote)}/UI/{nameof(UIPrefabs)}")]
    public sealed class UIPrefabs : ScriptableObject
    {
        public DropdownItem DropdownItem = default!;
    }
}