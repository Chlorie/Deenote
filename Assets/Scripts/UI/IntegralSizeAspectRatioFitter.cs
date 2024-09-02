using Deenote.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI
{
    /// <summary>
    /// A simplified version of <see cref="AspectRatioFitter"/>,
    /// only supports <see cref="AspectRatioFitter.AspectMode.FitInParent"/>.
    /// Additionally, this size fitter forces the width and height value to be integers.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class IntegralSizeAspectRatioFitter : MonoBehaviour, ILayoutSelfController
    {
        public void SetLayoutHorizontal() { }
        public void SetLayoutVertical() { }

        public float AspectRatio
        {
            get => _aspectRatio;
            set {
                _aspectRatio = value;
                UpdateLayout();
            }
        }

        [SerializeField] private float _aspectRatio = 1.0f;
        private RectTransform? _rectTransform;
        private DrivenRectTransformTracker _tracker;
        private bool _layoutUpdateDelayed;

        private RectTransform Rect => this.MaybeGetComponent(ref _rectTransform);
        private RectTransform? Parent => Rect.parent as RectTransform;

        private void OnValidate()
        {
            AspectRatio = Mathf.Clamp(AspectRatio, 0.001f, 1000.0f);
            _layoutUpdateDelayed = true;
        }

        private void OnEnable() => UpdateLayout();
        private void OnTransformParentChanged() => UpdateLayout();
        private void OnRectTransformDimensionsChange() => UpdateLayout();

        private void Start()
        {
            if (Parent is null)
                enabled = false;
        }

        private void OnDisable()
        {
            _tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(Rect);
        }

        private void Update()
        {
            if (!_layoutUpdateDelayed) return;
            _layoutUpdateDelayed = false;
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (!isActiveAndEnabled || Parent is not { } parent) return;
            var rect = Rect;
            _tracker.Clear();
            _tracker.Add(this, rect,
                DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition |
                DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.SizeDeltaY);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;

            // Find the best fit first
            Vector2 sizeDelta = Vector2.zero;
            Vector2 parentSize = parent.rect.size;
            Vector2 delta = parentSize * (rect.anchorMax - rect.anchorMin);
            if (parentSize.y * AspectRatio >= parentSize.x)
                sizeDelta.y = parentSize.x / AspectRatio - delta.y;
            else
                sizeDelta.x = parentSize.y * AspectRatio - delta.x;

            // Truncate to integers
            Vector2 fitSize = parentSize + delta;
            sizeDelta.x -= fitSize.x - Mathf.Floor(fitSize.x);
            sizeDelta.y -= fitSize.y - Mathf.Floor(fitSize.y);
            rect.sizeDelta = sizeDelta;
        }
    }
}