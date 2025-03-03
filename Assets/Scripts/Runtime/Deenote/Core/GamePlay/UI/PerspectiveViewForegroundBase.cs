#nullable enable

using UnityEngine;

namespace Deenote.GamePlay.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class PerspectiveViewForegroundBase : MonoBehaviour
    {
        [field: SerializeField]
        public RectTransform RectTransform { get; private set; } = default!;
        [field: SerializeField]
        public GameStageUIArgs Args { get; private set; } = default!;

        protected abstract void Awake();

        protected virtual void OnValidate()
        {
            RectTransform ??= GetComponent<RectTransform>();
        }
    }
}