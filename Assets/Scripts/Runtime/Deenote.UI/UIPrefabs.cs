#nullable enable

using Deenote.UIFramework.Controls;
using UnityEngine;

namespace Deenote.UI.Theme
{
    [CreateAssetMenu(
    fileName = nameof(UIPrefabs),
    menuName = $"{nameof(Deenote)}/UI/{nameof(UIPrefabs)}")]
    public sealed class UIPrefabs : ScriptableObject
    {
        public DropdownItem DropdownItem = default!;
    }
}