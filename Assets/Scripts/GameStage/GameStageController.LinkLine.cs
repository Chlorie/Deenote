using UnityEngine;

namespace Deenote.GameStage
{
    partial class GameStageController
    {
        [Header("Link Line")]
        [SerializeField] bool __showLinkLines;

        public bool IsShowLinkLines
        {
            get => __showLinkLines;
            set {
                if (__showLinkLines == value) return;
                __showLinkLines = value;
                _editorController.NotifyIsShowLinkLinesChanged(__showLinkLines);
                _editorPropertiesWindow.NotifyIsShowLinksChanged(__showLinkLines);
            }
        }
    }
}