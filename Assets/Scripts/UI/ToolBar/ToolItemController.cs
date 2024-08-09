using Deenote.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.ToolBar
{
    public sealed class ToolItemController : MonoBehaviour
    {
        [SerializeField] Button _button;
        [SerializeField] LocalizedText _text;

        public Button Button => _button;

        public LocalizedText Text => _text;
    }
}