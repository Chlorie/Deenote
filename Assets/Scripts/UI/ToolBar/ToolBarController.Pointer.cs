using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.ToolBar
{
    partial class ToolBarController : IPointerEnterHandler, IPointerExitHandler
    {
        private const float CollapsedWidth = 44f;

        private float? __expandedSize;

        private float ExpandedSize
        {
            get {
                if (__expandedSize is not null)
                    return __expandedSize.Value;

                float maxTextSize = 0f;
                maxTextSize = Mathf.Max(maxTextSize, _undoItem.Text.TmpText.preferredWidth);
                maxTextSize = Mathf.Max(maxTextSize, _redoItem.Text.TmpText.preferredWidth);
                maxTextSize = Mathf.Max(maxTextSize, _cutItem.Text.TmpText.preferredWidth);
                maxTextSize = Mathf.Max(maxTextSize, _copyItem.Text.TmpText.preferredWidth);
                maxTextSize = Mathf.Max(maxTextSize, _pasteItem.Text.TmpText.preferredWidth);
                maxTextSize = Mathf.Max(maxTextSize, _linkItem.Text.TmpText.preferredWidth);
                maxTextSize = Mathf.Max(maxTextSize, _unlinkItem.Text.TmpText.preferredWidth);
                maxTextSize = Mathf.Max(maxTextSize, _soundItem.Text.TmpText.preferredWidth);
                maxTextSize = Mathf.Max(maxTextSize, _quantizeItem.Text.TmpText.preferredWidth);
                maxTextSize = Mathf.Max(maxTextSize, _mirrorItem.Text.TmpText.preferredWidth);

                __expandedSize = CollapsedWidth + 8 + maxTextSize;
                return __expandedSize.Value;
            }
        }

        private void AwakePointer()
        {
            MainSystem.Localization.OnLanguageChanged += () => __expandedSize = null;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            var transform = (RectTransform)gameObject.transform;
            var sizeDelta = transform.sizeDelta;
            sizeDelta.x = ExpandedSize;
            transform.sizeDelta = sizeDelta;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            var transform = (RectTransform)gameObject.transform;
            var sizeDelta = transform.sizeDelta;
            sizeDelta.x = CollapsedWidth;
            transform.sizeDelta = sizeDelta;
        }
    }
}