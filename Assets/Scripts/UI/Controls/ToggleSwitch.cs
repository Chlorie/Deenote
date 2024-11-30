#nullable enable

using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Controls
{
    public sealed partial class ToggleSwitch : MonoBehaviour
    {
        [SerializeField] Image _backgroundImage;
        [SerializeField] Image _borderImage;
        [SerializeField] Image _handleImage;
        
    }

    partial class ToggleSwitch : IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Visual")]
        [SerializeField] bool _isHovering;
        [SerializeField] bool _isPressed;

        public void OnPointerExit(PointerEventData eventData) => throw new System.NotImplementedException();
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) => throw new System.NotImplementedException();

    }
}