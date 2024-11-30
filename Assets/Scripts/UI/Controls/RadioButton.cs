#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed partial class RadioButton : MonoBehaviour
    {
        [SerializeField] Image _backgroundImage;
        [SerializeField] Image _borderImage;
        [SerializeField] Image _handleImage;

        [Header("Control")]
        [SerializeField] RadioButtonGroup _group;
    }

    partial class RadioButton
    {
        [Header("Visual")]
        [SerializeField] bool _isHovering;
        [SerializeField] bool _isPressed;

        private void OnValidate()
        {
            
        }
    }
}