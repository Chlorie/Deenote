using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Utilities;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.UI.StatusBar
{
    public sealed class StatusBarController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] LocalizedText _messageText;
        [SerializeField] LocalizedText _fpsText;

        [Header("Prefabs")]
        [SerializeField] Transform _toastMessageParentTransform;
        [SerializeField] ToastMessageController _toastMessagePrefab;

        private ObjectPool<ToastMessageController> _toastMessagePool;

        private int _messageToken;

        private float _fpsTimer;
        private int _fpsFrameCount;

        [field: SerializeField]
        private bool __showFps;

        public bool IsFpsShown
        {
            get => __showFps;
            set {
                if (__showFps == value)
                    return;
                __showFps = value;
                _fpsText.gameObject.SetActive(__showFps);
                MainSystem.PreferenceWindow.NotifyIsFpsShownChanged(__showFps);
            }
        }

        public async UniTaskVoid SetStatusMessageAsync(LocalizableText message, float duration_s)
        {
            unchecked { _messageToken++; }
            var token = _messageToken;
            _messageText.SetText(message);
            await UniTask.WaitForSeconds(duration_s, _messageText);

            if (_messageToken == token)
                _messageText.SetText(default);
        }

        public void SetStatusMessage(LocalizableText message, ReadOnlySpan<string> args = default)
        {
            unchecked { _messageToken++; }
            _messageText.SetText(message, args);
        }

        public async UniTaskVoid ShowToastAsync(LocalizableText message, float duration_s)
        {
            var toast = _toastMessagePool.Get();
            toast.Initialize(message);
            toast.transform.SetAsLastSibling();

            await UniTask.WaitForSeconds(duration_s);

            _toastMessagePool.Release(toast);
        }

        private void Awake()
        {
            _toastMessagePool = UnityUtils.CreateObjectPool(_toastMessagePrefab, _toastMessageParentTransform, 1, 10);
        }

        private void Update()
        {
            if (IsFpsShown) {
                _fpsFrameCount++;
                if (_fpsTimer.IncAndTryWrap(Time.deltaTime, 1f)) {
                    string fpsStr = _fpsFrameCount.ToString();
                    _fpsText.SetLocalizedText("Status_Fps", MemoryMarshal.CreateReadOnlySpan(ref fpsStr, 1));
                    _fpsFrameCount = 0;
                }
            }
        }
    }
}