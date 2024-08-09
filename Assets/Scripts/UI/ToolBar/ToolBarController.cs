using UnityEngine;

namespace Deenote.UI.ToolBar
{
    public sealed partial class ToolBarController : MonoBehaviour
    {
        [SerializeField] private bool __isActivated;
        public bool IsActivated
        {
            get => __isActivated;
            set {
                if (__isActivated == value)
                    return;

                __isActivated = value;
                gameObject.SetActive(__isActivated);
            }
        }

        [Header("UI")]
        [SerializeField] ToolItemController _undoItem;
        [SerializeField] ToolItemController _redoItem;

        [SerializeField] ToolItemController _cutItem;
        [SerializeField] ToolItemController _copyItem;
        [SerializeField] ToolItemController _pasteItem;

        [SerializeField] ToolItemController _linkItem;
        [SerializeField] ToolItemController _unlinkItem;
        [SerializeField] ToolItemController _soundItem;
        [SerializeField] ToolItemController _desoundItem;

        [SerializeField] ToolItemController _quantizeItem;
        [SerializeField] ToolItemController _mirrorItem;

        private void Awake()
        {
            _undoItem.Button.onClick.AddListener(MainSystem.Editor.Undo);
            _redoItem.Button.onClick.AddListener(MainSystem.Editor.Redo);
            _cutItem.Button.onClick.AddListener(MainSystem.Editor.CutSelectedNotes);
            _copyItem.Button.onClick.AddListener(MainSystem.Editor.CopySelectedNotes);
            _pasteItem.Button.onClick.AddListener(MainSystem.Editor.PasteNotes);
            _linkItem.Button.onClick.AddListener(MainSystem.Editor.LinkSelectedNotes);
            _unlinkItem.Button.onClick.AddListener(MainSystem.Editor.UnlinkSelectedNotes);
            _soundItem.Button.onClick.AddListener(MainSystem.Editor.SoundifySelectedNotes);
            _desoundItem.Button.onClick.AddListener(MainSystem.Editor.DesoundifySelectedNotes);
            _quantizeItem.Button.onClick.AddListener(() => MainSystem.Editor.EditSelectedNotesPositionCoord(coord => MainSystem.GameStage.Grids.Quantize(coord, true, true)));
            _mirrorItem.Button.onClick.AddListener(() => MainSystem.Editor.EditSelectedNotesPosition(pos => -pos));

            AwakePointer();
        }
    }
}