#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Library;
using Deenote.Localization;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.UI.Views
{
    public sealed class ToastManager : MonoBehaviour
    {
        [Header("Args")]
        [SerializeField] Toast _toastPrefab = default!;

        private ObjectPool<Toast> _pool = default!;
        private int _toastUid;

        private void Awake()
        {
            _pool = UnityUtils.CreateObjectPool(_toastPrefab, transform, item => item.OnInstantiate(this), defaultCapacity: 1);
        }

        public void ShowToast(LocalizableText message)
        {
            var toast = ActivateToast(message);
        }

        public async UniTask ShowToastAsync(LocalizableText message, float duration_s)
        {
            var toast = ActivateToast(message);
            var id = toast.Uid;
            await UniTask.WaitForSeconds(duration_s);
            if (toast.gameObject.activeSelf && toast.Uid == id)
                CloseToast(toast);
            // The toast is close by its close button
            else
                return;
        }

        public UniTask ShowLocalizedToastAsync(string key, float duration_s)
            => ShowToastAsync(LocalizableText.Localized(key), duration_s);

        public void ShowLocalizedToast(string key)
            => ShowToast(LocalizableText.Localized(key));

        public void ShowRawTextToast(string text) => ShowToast(LocalizableText.Raw(text));

        public UniTask ShowRawTextToastAsync(string text, float duration_s) => ShowToastAsync(LocalizableText.Raw(text), duration_s);

        private Toast ActivateToast(LocalizableText text)
        {
            var toast = _pool.Get();
            toast.Initialize(_toastUid++, text);
            toast.transform.SetAsLastSibling();
            return toast;
        }

        internal void CloseToast(Toast toast)
        {
            _pool.Release(toast);
        }
    }
}