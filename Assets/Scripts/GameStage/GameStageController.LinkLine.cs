using Deenote.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    partial class GameStageController
    {
        // TODO: this prefab could be same as linePrefab in GridController
        // Try to combine them?
        [Header("Link Line")]
        [SerializeField] LineRenderer _linkLinePrefab;
        [SerializeField] Transform _linkLineParentTransform;
        private ObjectPool<LineRenderer> _linkLinePool;

        [SerializeField] bool __showLinkLines;

        public bool IsShowLinkLines
        {
            get => __showLinkLines;
            set {
                if (__showLinkLines == value)
                    return;

                __showLinkLines = value;
                _linkLineParentTransform.gameObject.SetActive(__showLinkLines);
                _editorController.NotifyIsShowLinkLinesChanged(__showLinkLines);
                _editorPropertiesWindow.NotifyIsShowLinksChanged(__showLinkLines);
            }
        }

        private void AwakeLinkLine()
        {
            _linkLinePool = UnityUtils.CreateObjectPool(() =>
            {
                var line = Instantiate(_linkLinePrefab, _linkLineParentTransform);
                line.sortingOrder = -11;
                line.widthMultiplier = 0.035f;
                line.positionCount = 2;
                line.SetSolidColor(MainSystem.Args.LinkLineColor);
                return line;
            });
            UnityUtils.CreateObjectPool(_linkLinePrefab, _linkLineParentTransform, 0);
        }

        public LineRenderer GetLinkLine() => _linkLinePool.Get();

        public void ReleaseLinkLine(LineRenderer line) => _linkLinePool.Release(line);
    }
}