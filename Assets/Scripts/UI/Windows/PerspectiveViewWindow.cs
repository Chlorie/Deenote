using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Project.Models;
using Deenote.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed class PerspectiveViewWindow : MonoBehaviour
    {
        [SerializeField] Window _window;
        [SerializeField] GameStageController _gameStageController;
        [SerializeField] EditorController _editorController;

        [Header("UI")]
        [SerializeField] TMP_Text _musicNameText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] Slider _timeSlider;
        [SerializeField] Image _difficultyImage;
        [SerializeField] TMP_Text _levelText;
        [SerializeField] Button _pauseButton;
        [SerializeField] Image _backgroundBreathingMaskImage;
        [SerializeField] TMP_Text _backgroundStaveText;
        [SerializeField] TMP_Text _comboText;
        [SerializeField] TMP_Text _comboShadowText;
        [SerializeField] RawImage _cameraViewRawImage;
        [SerializeField] Transform _cameraViewTransform;

        [Header("Prefabs")]
        [SerializeField] Sprite _easyDifficultyIconSprite;
        [SerializeField] Color _easyLevelTextColor;
        [SerializeField] Sprite _normalDifficultyIconSprite;
        [SerializeField] Color _normalLevelTextColor;
        [SerializeField] Sprite _hardDifficultyIconSprite;
        [SerializeField] Color _hardLevelTextColor;
        [SerializeField] Sprite _extraDifficultyIconSprite;
        [SerializeField] Color _extraLevelTextColor;

        private ChartModel _chart;

        private float _tryPlayResetTime;

        private int _judgedNoteCount;
        private MouseActionState _mouseActionState;

        private void Awake()
        {
            _timeSlider.onValueChanged.AddListener(val => _gameStageController.CurrentMusicTime = val);

            _pauseButton.onClick.AddListener(OnPauseClicked);
        }

        private void Update()
        {
            DetectMouseMove();
            DetectLeftMouse();
            DetectRightMouse();
            DetectMouseWheel();
            DetectKeys();
        }

        #region Input

        private void DetectMouseMove()
        {
            if (TryGetCurrentMouseToNoteCoordInSelectionRange(out NoteCoord coord)) {
                _editorController.MoveNoteIndicator(coord);
            }
            else {
                _editorController.MoveNoteIndicator(null);
            }
        }

        private void DetectLeftMouse()
        {
            if (Input.GetMouseButtonDown(0)) {
                // This will cancel NotePlacing state
                _mouseActionState = MouseActionState.NoteSelecting;
                if (TryGetCurrentMouseToNoteCoordInSelectionRange(out var coord)) {
                    _editorController.StartNoteSelection(coord, deselectPrevious: !UnityUtils.IsFunctionalKeyHolding(ctrl: true));
                }
            }
            else if (_mouseActionState is MouseActionState.NoteSelecting && Input.GetMouseButton(0)) {
                if (TryGetCurrentMouseToNoteCoordInSelectionRange(out var coord)) {
                    _editorController.UpdateNoteSelection(coord);
                }
            }
            else if (_mouseActionState is MouseActionState.NoteSelecting && Input.GetMouseButtonUp(0)) {
                _mouseActionState = MouseActionState.None;
                _editorController.EndNoteSelection();
            }
        }

        private void DetectRightMouse()
        {
            if (_mouseActionState is not MouseActionState.NoteSelecting && Input.GetMouseButtonDown(1)) {
                _mouseActionState = MouseActionState.NotePlacing;
            }
            else if (_mouseActionState is MouseActionState.NotePlacing && Input.GetMouseButtonUp(1)) {
                _mouseActionState = MouseActionState.None;
                if (TryGetCurrentMouseToNoteCoordInSelectionRange(out var coord)) {
                    _editorController.PlaceNoteAt(coord);
                }
            }
        }

        private void DetectMouseWheel()
        {
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (wheel != 0f) {
                // TODO: Mouse sensitivity
                float deltaTime = wheel * MainSystem.GlobalSettings.MouseScrollSensitivity;
                _gameStageController.CurrentMusicTime -= deltaTime;
            }
        }

        private void DetectKeys()
        {
            // TODO:如果有两个View的话，考虑把这些东西扔到一个新的单例类里，不然会执行两次
            if (UnityUtils.IsKeyDown(KeyCode.Z, ctrl: true))
                _editorController.Undo();
            if (UnityUtils.IsKeyDown(KeyCode.Z, ctrl: true, shift: true) || UnityUtils.IsKeyDown(KeyCode.Y, ctrl: true))
                _editorController.Redo();
            //if(UnityUtils.IsKeyDown(KeyCode.C,ctrl:true))
            //if(UnityUtils.IsKeyDown(KeyCode.X,ctrl:true))
            //if(UnityUtils.IsKeyDown(KeyCode.V,ctrl:true))

            if (UnityUtils.IsKeyDown(KeyCode.Delete))
                _editorController.RemoveSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.G))
                _editorController.SnapToPositionGrid = _editorController.SnapToTimeGrid = !(_editorController.SnapToPositionGrid && _editorController.SnapToTimeGrid);
            if (UnityUtils.IsKeyDown(KeyCode.A, ctrl: true))
                _editorController.SelectAllNotes();
            //if(UnityUtils.IsKeyDown(KeyCode.L))
            //if(UnityUtils.IsKeyDown(KeyCode.U))
            if (UnityUtils.IsKeyDown(KeyCode.Q))
                _editorController.EditSelectedNotes(n => n.PositionCoord = _gameStageController.Grids.Quantize(n.PositionCoord, true, true));
            // TODO: Optimize?
            if (UnityUtils.IsKeyDown(KeyCode.W))
                _editorController.EditSelectedNotes(n => n.Time += 0.001f);
            else if (UnityUtils.IsKeyDown(KeyCode.W, alt: true))
                _editorController.EditSelectedNotes(n => n.Time += 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.W, shift: true))
                _editorController.EditSelectedNotes(n => n.Time = _gameStageController.Grids.CeilToNearestTimeGridTime(n.Time) ?? n.Time);
            if (UnityUtils.IsKeyDown(KeyCode.S))
                _editorController.EditSelectedNotes(n => n.Time -= 0.001f);
            else if (UnityUtils.IsKeyDown(KeyCode.S, alt: true))
                _editorController.EditSelectedNotes(n => n.Time -= 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.S, shift: true))
                _editorController.EditSelectedNotes(n => n.Time = _gameStageController.Grids.FloorToNearestTimeGridTime(n.Time) ?? n.Time);
            // ADZXM
            // IF

            // Stage
            if (UnityUtils.IsKeyDown(KeyCode.Return) || UnityUtils.IsKeyDown(KeyCode.KeypadEnter))
                _gameStageController.TogglePlayingState();
            if (UnityUtils.IsKeyDown(KeyCode.Space)) {
                _tryPlayResetTime = _gameStageController.CurrentMusicTime;
                _gameStageController.Play();
            }
            else if (UnityUtils.IsKeyUp(KeyCode.Space)) {
                _gameStageController.CurrentMusicTime = _tryPlayResetTime;
                _gameStageController.Pause();
            }
            if (UnityUtils.IsKeyDown(KeyCode.Home))
                _gameStageController.CurrentMusicTime = 0f;
            if (UnityUtils.IsKeyDown(KeyCode.End))
                _gameStageController.CurrentMusicTime = _gameStageController.MusicLength;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow, ctrl: true))
                _gameStageController.NoteSpeed += 1;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow, ctrl: true))
                _gameStageController.NoteSpeed -= 1;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow, alt: true))
                _gameStageController.MusicSpeed += 1;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow, alt: true))
                _gameStageController.MusicSpeed -= 1;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow))
                _gameStageController.StagePlaySpeed = -2.5f;
            else if (UnityUtils.IsKeyDown(KeyCode.UpArrow, shift: true))
                _gameStageController.StagePlaySpeed = -5.0f;
            else if (UnityUtils.IsKeyUp(KeyCode.UpArrow) || UnityUtils.IsKeyUp(KeyCode.UpArrow, shift: true))
                _gameStageController.StagePlaySpeed = 0f;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow))
                _gameStageController.StagePlaySpeed = 2.5f;
            else if (UnityUtils.IsKeyDown(KeyCode.DownArrow, shift: true))
                _gameStageController.StagePlaySpeed = 5.0f;
            else if (UnityUtils.IsKeyUp(KeyCode.DownArrow) || UnityUtils.IsKeyUp(KeyCode.DownArrow, shift: true))
                _gameStageController.StagePlaySpeed = 0f;
        }

        #endregion

        #region UI Events

        private void OnPauseClicked() => _gameStageController.TogglePlayingState();

        #endregion

        #region Notify

        public void NotifyMusicTimeChanged(float time)
        {
            _timeSlider.SetValueWithoutNotify(time);
        }

        public void NotifyChartChanged(ProjectModel project, ChartModel chart)
        {
            _chart = chart;

            _timeSlider.maxValue = project.AudioClip.length;
            _musicNameText.text = project.MusicName;
            // _difficultyImage.sprite
            // _levelText.color
            // _levelText.text
        }

        public void NotifyChartProgressChanged(int nextHitNoteIndex)
        {
            UpdateScore(nextHitNoteIndex);
            UpdateCombo(nextHitNoteIndex - 1);
        }

        public void NotifyStageEffectUpdate(bool isOn)
        {
            if (isOn) {
                const float BgPeriod = 2f;
                var ratio = Mathf.Sin(Time.time * (2 * Mathf.PI / BgPeriod));
                _backgroundBreathingMaskImage.color = new(1f, 1f, 1f, (ratio + 1f) / 2f);
            }
            else {
                _backgroundBreathingMaskImage.color = Color.white;
            }
        }

        #endregion

        private bool TryGetCurrentMouseToNoteCoordInSelectionRange(out NoteCoord coord)
        {
            var textureTransform = _cameraViewRawImage.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(textureTransform, Input.mousePosition, null, out var localPoint)) {
                coord = default;
                return false;
            }

            var viewPoint = new Vector2(
                  localPoint.x / textureTransform.rect.width,
                  localPoint.y / textureTransform.rect.height);

            if (viewPoint is not { x: >= 0f and <= 1f, y: >= 0f and <= 1f }) {
                coord = default;
                return false;
            }

            if (!_gameStageController.TryConvertViewPointToNoteCoord(viewPoint, out coord)) {
                return false;
            }

            // Ignore when press position is too far
            if (coord.Position is > 4f or < -4f) {
                return false;
            }

            return true;
        }

        private void UpdateScore(int judgedNoteCount)
        {
            if (judgedNoteCount <= 0) {
                _scoreText.text = "0.00 %";
                return;
            }

            // TODO: If both judged count and total count not changed
            if (_judgedNoteCount == judgedNoteCount)
                return;

            int noteCount = _chart.Notes.Count;
            float accScore = (float)judgedNoteCount / noteCount;
            // comboActual = Sum(1..judgeNoteCount);
            // comboTotal = Sum(1..noteCount)
            // comboScore = comboActual / comboTotal
            //            = ((1 + judged) * judged) / ((1 + count) * count)
            float comboScore = (float)((1 + judgedNoteCount) * judgedNoteCount) / ((1 + noteCount) * noteCount);

            float score = accScore * 80_00f + comboScore * 20_00f; ;
            _scoreText.text = $"{Mathf.Floor(score) / 100f:F2} %";
        }

        private void UpdateCombo(int prevHitNoteIndex)
        {
            if (prevHitNoteIndex < 0)
                return;
            var deltaTime = _gameStageController.CurrentMusicTime - _chart.Notes[prevHitNoteIndex].Data.Time;
            Debug.Assert(deltaTime >= 0, $"actual delta time:{deltaTime}");
            // TODO: Update combo
        }

        private enum MouseActionState
        {
            None,
            NoteSelecting = 1,
            NotePlacing = 2,
        }
    }
}
