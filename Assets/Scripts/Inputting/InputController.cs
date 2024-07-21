using Deenote.Utilities;
using UnityEngine;

namespace Deenote.Inputting
{
    public sealed class InputController : MonoBehaviour
    {
        [SerializeField, Range(-10, 10)]
        private float __mouseScrollSensitivity;
        public float MouseScrollSensitivity
        {
            get => __mouseScrollSensitivity;
            set {
                if (__mouseScrollSensitivity == value)
                    return;
                __mouseScrollSensitivity = value;
                MainSystem.PreferenceWindow.NotifyMouseScrollSensitivityChanged(__mouseScrollSensitivity);
            }
        }

        private float _tryPlayResetTime;

        private void Update()
        {
            DetectKeys();
        }

        private void DetectKeys()
        {
            if (UnityUtils.IsKeyDown(KeyCode.Escape) && MainSystem.GameStage.PerspectiveView.IsFullScreen)
                MainSystem.GameStage.PerspectiveView.SetFullScreenState(false);

            // Operation
            if (UnityUtils.IsKeyDown(KeyCode.Z, ctrl: true))
                MainSystem.Editor.Undo();
            if (UnityUtils.IsKeyDown(KeyCode.Z, ctrl: true, shift: true) || UnityUtils.IsKeyDown(KeyCode.Y, ctrl: true))
                MainSystem.Editor.Redo();
            if (UnityUtils.IsKeyDown(KeyCode.C, ctrl: true))
                MainSystem.Editor.CopySelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.X, ctrl: true))
                MainSystem.Editor.CutSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.V, ctrl: true))
                MainSystem.Editor.PasteNotes();

            // Edit
            if (UnityUtils.IsKeyDown(KeyCode.G))
                MainSystem.Editor.SnapToPositionGrid = MainSystem.Editor.SnapToTimeGrid = !(MainSystem.Editor.SnapToPositionGrid && MainSystem.Editor.SnapToTimeGrid);
            if (UnityUtils.IsKeyDown(KeyCode.Delete))
                MainSystem.Editor.RemoveSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.A, ctrl: true))
                MainSystem.Editor.SelectAllNotes();
            if (UnityUtils.IsKeyDown(KeyCode.L))
                MainSystem.Editor.LinkSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.U))
                MainSystem.Editor.UnlinkSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.P))
                MainSystem.Editor.ToggleSoundOfSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.Q))
                MainSystem.Editor.EditSelectedNotesPositionCoord(c => MainSystem.GameStage.Grids.Quantize(c, true, true));

            // Adjust
            if (UnityUtils.IsKeyDown(KeyCode.W))
                MainSystem.Editor.EditSelectedNotesTime(t => t + 0.001f);
            else if (UnityUtils.IsKeyDown(KeyCode.W, alt: true))
                MainSystem.Editor.EditSelectedNotesTime(t => t + 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.W, shift: true))
                MainSystem.Editor.EditSelectedNotesTime(t => MainSystem.GameStage.Grids.CeilToNextNearestTimeGridTime(t) ?? t);
            if (UnityUtils.IsKeyDown(KeyCode.S))
                MainSystem.Editor.EditSelectedNotesTime(t => t - 0.001f);
            else if (UnityUtils.IsKeyDown(KeyCode.S, alt: true))
                MainSystem.Editor.EditSelectedNotesTime(t => t - 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.S, shift: true))
                MainSystem.Editor.EditSelectedNotesTime(t => MainSystem.GameStage.Grids.FloorToNextNearestTimeGridTime(t) ?? t);
            if (UnityUtils.IsKeyDown(KeyCode.A))
                MainSystem.Editor.EditSelectedNotesPosition(p => p - 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.A, alt: true))
                MainSystem.Editor.EditSelectedNotesPosition(p => p - 0.1f);
            else if (UnityUtils.IsKeyDown(KeyCode.A, shift: true))
                MainSystem.Editor.EditSelectedNotesPosition(p => MainSystem.GameStage.Grids.FloorToNearestNextVerticalGridPosition(p) ?? p);
            if (UnityUtils.IsKeyDown(KeyCode.D))
                MainSystem.Editor.EditSelectedNotesPosition(p => p + 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.D, alt: true))
                MainSystem.Editor.EditSelectedNotesPosition(p => p + 0.1f);
            else if (UnityUtils.IsKeyDown(KeyCode.D, shift: true))
                MainSystem.Editor.EditSelectedNotesPosition(p => MainSystem.GameStage.Grids.CeilToNearestNextVerticalGridPosition(p) ?? p);
            if (UnityUtils.IsKeyDown(KeyCode.Z))
                MainSystem.Editor.EditSelectedNotesSize(s => s - 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.Z, shift: true))
                MainSystem.Editor.EditSelectedNotesSize(s => s - 0.1f);
            if (UnityUtils.IsKeyDown(KeyCode.X))
                MainSystem.Editor.EditSelectedNotesSize(s => s + 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.X))
                MainSystem.Editor.EditSelectedNotesSize(s => s + 0.1f);
            if (UnityUtils.IsKeyDown(KeyCode.M))
                MainSystem.Editor.EditSelectedNotesPosition(p => -p);
            // Curve
            // IF

            // Stage
            if (UnityUtils.IsKeyDown(KeyCode.Return) || UnityUtils.IsKeyDown(KeyCode.KeypadEnter))
                MainSystem.GameStage.TogglePlayingState();
            if (UnityUtils.IsKeyDown(KeyCode.Space)) {
                _tryPlayResetTime = MainSystem.GameStage.CurrentMusicTime;
                MainSystem.GameStage.Play();
            }
            else if (UnityUtils.IsKeyUp(KeyCode.Space)) {
                MainSystem.GameStage.CurrentMusicTime = _tryPlayResetTime;
                MainSystem.GameStage.Pause();
            }
            if (UnityUtils.IsKeyDown(KeyCode.Home))
                MainSystem.GameStage.CurrentMusicTime = 0f;
            if (UnityUtils.IsKeyDown(KeyCode.End))
                MainSystem.GameStage.CurrentMusicTime = MainSystem.GameStage.MusicLength;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow, ctrl: true))
                MainSystem.GameStage.NoteSpeed += 1;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow, ctrl: true))
                MainSystem.GameStage.NoteSpeed -= 1;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow, alt: true))
                MainSystem.GameStage.MusicSpeed += 1;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow, alt: true))
                MainSystem.GameStage.MusicSpeed -= 1;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow))
                MainSystem.GameStage.StagePlaySpeed = -2.5f;
            else if (UnityUtils.IsKeyDown(KeyCode.UpArrow, shift: true))
                MainSystem.GameStage.StagePlaySpeed = -5.0f;
            else if (UnityUtils.IsKeyUp(KeyCode.UpArrow) || UnityUtils.IsKeyUp(KeyCode.UpArrow, shift: true))
                MainSystem.GameStage.StagePlaySpeed = 0f;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow))
                MainSystem.GameStage.StagePlaySpeed = 2.5f;
            else if (UnityUtils.IsKeyDown(KeyCode.DownArrow, shift: true))
                MainSystem.GameStage.StagePlaySpeed = 5.0f;
            else if (UnityUtils.IsKeyUp(KeyCode.DownArrow) || UnityUtils.IsKeyUp(KeyCode.DownArrow, shift: true))
                MainSystem.GameStage.StagePlaySpeed = 0f;
        }
    }
}