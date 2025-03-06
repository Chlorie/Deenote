#nullable enable

using Deenote.Core.Editing;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Components;
using Deenote.UI;
using UnityEngine;

namespace Deenote.Inputting
{
    public sealed class InputController : FlagNotifiableMonoBehaviour<InputController, InputController.NotificationFlag>
    {
        private float? _musicResetTime;

        private void Update() => DetectKeys();

        private void DetectKeys()
        {
            var game = MainSystem.GamePlayManager;
            var editor = MainSystem.StageChartEditor;

            if (UnityUtils.IsKeyDown(KeyCode.Escape) && MainWindow.PerspectiveViewPanelView.IsFullScreen)
                MainWindow.PerspectiveViewPanelView.SetIsFullScreen(false);

            // Operation
            if (UnityUtils.IsKeyDown(KeyCode.Z, ctrl: true))
                editor.UndoOperation();
            if (UnityUtils.IsKeyDown(KeyCode.Z, ctrl: true, shift: true) || UnityUtils.IsKeyDown(KeyCode.Y, ctrl: true))
                editor.RedoOperation();
            if (UnityUtils.IsKeyDown(KeyCode.C, ctrl: true))
                editor.CopySelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.X, ctrl: true))
                editor.CutSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.V, ctrl: true))
                editor.PasteNotes();

            // Edit
            if (UnityUtils.IsKeyDown(KeyCode.J)) {
                editor.Placer.Options |= (StageNotePlacer.PlacementOptions.PlaceSlide | StageNotePlacer.PlacementOptions.PastingRememberPosition);
            }
            if (UnityUtils.IsKeyUp(KeyCode.J))
                editor.Placer.Options &= ~(StageNotePlacer.PlacementOptions.PlaceSlide | StageNotePlacer.PlacementOptions.PastingRememberPosition);

            if (UnityUtils.IsKeyDown(KeyCode.G))
                editor.Placer.SnapToPositionGrid = editor.Placer.SnapToTimeGrid = !(editor.Placer.SnapToPositionGrid && editor.Placer.SnapToTimeGrid);
            if (UnityUtils.IsKeyDown(KeyCode.Delete))
                editor.RemoveSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.A, ctrl: true))
                editor.Selector.SelectAll();
            if (UnityUtils.IsKeyDown(KeyCode.L))
                editor.EditSelectedNotesKind(NoteModel.NoteKind.Slide);
            if (UnityUtils.IsKeyDown(KeyCode.U))
                editor.EditSelectedNotesKind(NoteModel.NoteKind.Click);
            if (UnityUtils.IsKeyDown(KeyCode.P))
                editor.EditSelectedNoteSounds(true);
            else if (UnityUtils.IsKeyDown(KeyCode.P, ctrl: true))
                editor.EditSelectedNoteSounds(false);
            if (UnityUtils.IsKeyDown(KeyCode.Q))
                editor.EditSelectedNotesPositionCoord(c => MainSystem.GamePlayManager.Grids.Quantize(c, true, true));
            if (UnityUtils.IsKeyDown(KeyCode.M))
                editor.EditSelectedNotesPosition(p => -p);

            // Adjust
            if (UnityUtils.IsKeyDown(KeyCode.W))
                editor.EditSelectedNotesTime(t => t + 0.001f);
            else if (UnityUtils.IsKeyDown(KeyCode.W, alt: true))
                editor.EditSelectedNotesTime(t => t + 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.W, shift: true))
                editor.EditSelectedNotesTime(t => MainSystem.GamePlayManager.Grids.CeilToNextNearestTimeGridTime(t) ?? t);
            if (UnityUtils.IsKeyDown(KeyCode.S))
                editor.EditSelectedNotesTime(t => t - 0.001f);
            else if (UnityUtils.IsKeyDown(KeyCode.S, alt: true))
                editor.EditSelectedNotesTime(t => t - 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.S, shift: true))
                editor.EditSelectedNotesTime(t => MainSystem.GamePlayManager.Grids.FloorToNextNearestTimeGridTime(t) ?? t);
            if (UnityUtils.IsKeyDown(KeyCode.A))
                editor.EditSelectedNotesPosition(p => p - 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.A, alt: true))
                editor.EditSelectedNotesPosition(p => p - 0.1f);
            else if (UnityUtils.IsKeyDown(KeyCode.A, shift: true))
                editor.EditSelectedNotesPosition(p => MainSystem.GamePlayManager.Grids.FloorToNearestNextPositionGridPosition(p) ?? p);
            if (UnityUtils.IsKeyDown(KeyCode.D))
                editor.EditSelectedNotesPosition(p => p + 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.D, alt: true))
                editor.EditSelectedNotesPosition(p => p + 0.1f);
            else if (UnityUtils.IsKeyDown(KeyCode.D, shift: true))
                editor.EditSelectedNotesPosition(p => MainSystem.GamePlayManager.Grids.CeilToNearestNextPositionGridPosition(p) ?? p);
            if (UnityUtils.IsKeyDown(KeyCode.Z))
                editor.EditSelectedNotesSize(s => s - 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.Z, shift: true))
                editor.EditSelectedNotesSize(s => s - 0.1f);
            if (UnityUtils.IsKeyDown(KeyCode.X))
                editor.EditSelectedNotesSize(s => s + 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.X))
                editor.EditSelectedNotesSize(s => s + 0.1f);
            // Curve
            // IF

            // Stage
            if (UnityUtils.IsKeyDown(KeyCode.Space)) {
                _musicResetTime = game.MusicPlayer.Time;
                game.MusicPlayer.Play();
            }
            else if (_musicResetTime is { } mrt && UnityUtils.IsKeyUp(KeyCode.Space)) {
                game.MusicPlayer.Stop();
                game.MusicPlayer.Time = mrt;
            }
            if (UnityUtils.IsKeyDown(KeyCode.Home))
                game.MusicPlayer.Time = 0f;
            if (UnityUtils.IsKeyDown(KeyCode.End))
                game.MusicPlayer.Time = game.MusicPlayer.ClipLength;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow, ctrl: true))
                game.NoteFallSpeed += 5;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow, ctrl: true))
                game.NoteFallSpeed -= 5;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow, alt: true))
                game.MusicSpeed += 1;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow, alt: true))
                game.MusicSpeed -= 1;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow))
                game.SetManualPlaySpeed(-2.5f);
            else if (UnityUtils.IsKeyDown(KeyCode.UpArrow, shift: true))
                game.SetManualPlaySpeed(-5.0f);
            else if (UnityUtils.IsKeyUp(KeyCode.UpArrow) || UnityUtils.IsKeyUp(KeyCode.UpArrow, shift: true))
                game.SetManualPlaySpeed(null);
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow))
                game.SetManualPlaySpeed(2.5f);
            else if (UnityUtils.IsKeyDown(KeyCode.DownArrow, shift: true))
                game.SetManualPlaySpeed(5.0f);
            else if (UnityUtils.IsKeyUp(KeyCode.DownArrow) || UnityUtils.IsKeyUp(KeyCode.DownArrow, shift: true))
                game.SetManualPlaySpeed(null);
        }

        public enum NotificationFlag
        {
            MouseScrollSensitivity,
        }
    }
}