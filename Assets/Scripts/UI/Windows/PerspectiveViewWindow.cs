using System;
using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Project.Models;
using Deenote.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class PerspectiveViewWindow : SingletonBehavior<PerspectiveViewWindow>
    {
        [SerializeField] Window _window;
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
        [SerializeField] RawImage _cameraViewRawImage;
        [SerializeField] RectTransform _cameraViewTransform;

        [Header("Prefabs")]
        [SerializeField] Sprite _easyDifficultyIconSprite;
        [SerializeField] Color _easyLevelTextColor;
        [SerializeField] Sprite _normalDifficultyIconSprite;
        [SerializeField] Color _normalLevelTextColor;
        [SerializeField] Sprite _hardDifficultyIconSprite;
        [SerializeField] Color _hardLevelTextColor;
        [SerializeField] Sprite _extraDifficultyIconSprite;
        [SerializeField] Color _extraLevelTextColor;

        public Vector2 ViewSize => _viewSize.Value;
        public event Action<Vector2>? OnViewSizeChanged;

        private FrameCachedNotifyingProperty<Vector2> _viewSize = null!;
        private ChartModel _chart;

        private float _tryPlayResetTime;

        private int _judgedNoteCount;
        private MouseActionState _mouseActionState;

        protected override void Awake()
        {
            base.Awake();

            Vector2 GetViewSize()
            {
                Vector3[] corners = new Vector3[4];
                _cameraViewTransform.GetWorldCorners(corners);
                return corners[2] - corners[0];
            }

            void ViewSizeChanged(Vector2 _, Vector2 newSize) => OnViewSizeChanged?.Invoke(newSize);

            RenderTexture texture = new(1280, 720, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None);
            _cameraViewRawImage.texture = texture;
            _cameraViewRawImage.enabled = true;
            GameStageController.Instance.Camera.targetTexture = texture;
            _viewSize = new FrameCachedNotifyingProperty<Vector2>(GetViewSize);
            _viewSize.OnValueChanged += ViewSizeChanged;
            OnViewSizeChanged += ReplaceCameraRenderTexture;

            _timeSlider.onValueChanged.AddListener(val => GameStageController.Instance.CurrentMusicTime = val);
            _pauseButton.onClick.AddListener(OnPauseClicked);
        }

        private void ReplaceCameraRenderTexture(Vector2 newTargetSize)
        {
            int width = Mathf.RoundToInt(newTargetSize.x), height = Mathf.RoundToInt(newTargetSize.y);
            if (width <= 0 || height <= 0) return;
            var texture = GameStageController.Instance.Camera.targetTexture;
            if (texture.width == width && texture.height == height) return;
            texture.Release();
            texture.width = width;
            texture.height = height;
            texture.Create();
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
            if (TryGetCurrentMouseToNoteCoordInSelectionRange(out NoteCoord coord))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    _editorController.MoveNoteIndicator(coord, true);
                else
                    _editorController.MoveNoteIndicator(coord, false);
            }
            else
            {
                _editorController.HideNoteIndicator();
            }
        }

        private void DetectLeftMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // This will cancel NotePlacing state
                _mouseActionState = MouseActionState.NoteSelecting;
                if (TryGetCurrentMouseToNoteCoordInSelectionRange(out var coord))
                {
                    _editorController.StartNoteSelection(coord, toggleMode: UnityUtils.IsFunctionalKeyHolding(ctrl: true));
                }
            }
            else if (_mouseActionState is MouseActionState.NoteSelecting && Input.GetMouseButton(0))
            {
                if (TryGetCurrentMouseToNoteCoordInSelectionRange(out var coord))
                {
                    _editorController.UpdateNoteSelection(coord);
                }
            }
            else if (_mouseActionState is MouseActionState.NoteSelecting && Input.GetMouseButtonUp(0))
            {
                _mouseActionState = MouseActionState.None;
                _editorController.EndNoteSelection();
            }
        }

        private void DetectRightMouse()
        {
            if (_mouseActionState is not MouseActionState.NoteSelecting && Input.GetMouseButtonDown(1))
            {
                _mouseActionState = MouseActionState.NotePlacing;
            }
            else if (_mouseActionState is MouseActionState.NotePlacing && Input.GetMouseButtonUp(1))
            {
                _mouseActionState = MouseActionState.None;
                if (!TryGetCurrentMouseToNoteCoordInSelectionRange(out var coord)) return;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    _editorController.PlaceNoteAt(coord, true);
                else
                    _editorController.PlaceNoteAt(coord, false);
            }
        }

        private void DetectMouseWheel()
        {
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (wheel == 0f) return;
            float deltaTime = wheel * MainSystem.GlobalSettings.MouseScrollSensitivity;
            GameStageController.Instance.CurrentMusicTime -= deltaTime;
        }

        private void DetectKeys()
        {
            var gameStage = GameStageController.Instance;

            // TODO:如果有两个View的话，考虑把这些东西扔到一个新的单例类里，不然会执行两次
            // Operation
            if (UnityUtils.IsKeyDown(KeyCode.Z, ctrl: true))
                _editorController.Undo();
            if (UnityUtils.IsKeyDown(KeyCode.Z, ctrl: true, shift: true) || UnityUtils.IsKeyDown(KeyCode.Y, ctrl: true))
                _editorController.Redo();
            if (UnityUtils.IsKeyDown(KeyCode.C, ctrl: true))
                _editorController.CopySelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.X, ctrl: true))
                _editorController.CutSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.V, ctrl: true))
                _editorController.PasteNotes();

            // Edit
            if (UnityUtils.IsKeyDown(KeyCode.G))
                _editorController.SnapToPositionGrid = _editorController.SnapToTimeGrid =
                    _editorController is not { SnapToPositionGrid: true, SnapToTimeGrid: true };
            if (UnityUtils.IsKeyDown(KeyCode.Delete))
                _editorController.RemoveSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.A, ctrl: true))
                _editorController.SelectAllNotes();
            if (UnityUtils.IsKeyDown(KeyCode.L))
                _editorController.LinkSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.U))
                _editorController.UnlinkSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.P))
                _editorController.ToggleSoundOfSelectedNotes();
            if (UnityUtils.IsKeyDown(KeyCode.Q))
                _editorController.EditSelectedNotesPositionCoord(c => gameStage.Grids.Quantize(c, true, true));

            // Adjust
            if (UnityUtils.IsKeyDown(KeyCode.W))
                _editorController.EditSelectedNotesTime(t => t + 0.001f);
            else if (UnityUtils.IsKeyDown(KeyCode.W, alt: true))
                _editorController.EditSelectedNotesTime(t => t + 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.W, shift: true))
                _editorController.EditSelectedNotesTime(t => gameStage.Grids.CeilToNearestNextTimeGridTime(t) ?? t);
            if (UnityUtils.IsKeyDown(KeyCode.S))
                _editorController.EditSelectedNotesTime(t => t - 0.001f);
            else if (UnityUtils.IsKeyDown(KeyCode.S, alt: true))
                _editorController.EditSelectedNotesTime(t => t - 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.S, shift: true))
                _editorController.EditSelectedNotesTime(t => gameStage.Grids.FloorToNearestNextTimeGridTime(t) ?? t);
            if (UnityUtils.IsKeyDown(KeyCode.A))
                _editorController.EditSelectedNotesPosition(p => p - 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.A, alt: true))
                _editorController.EditSelectedNotesPosition(p => p - 0.1f);
            else if (UnityUtils.IsKeyDown(KeyCode.A, shift: true))
                _editorController.EditSelectedNotesPosition(p => gameStage.Grids.FloorToNearestNextVerticalGridPosition(p) ?? p);
            if (UnityUtils.IsKeyDown(KeyCode.D))
                _editorController.EditSelectedNotesPosition(p => p + 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.D, alt: true))
                _editorController.EditSelectedNotesPosition(p => p + 0.1f);
            else if (UnityUtils.IsKeyDown(KeyCode.D, shift: true))
                _editorController.EditSelectedNotesPosition(p => gameStage.Grids.CeilToNearestNextVerticalGridPosition(p) ?? p);
            if (UnityUtils.IsKeyDown(KeyCode.Z))
                _editorController.EditSelectedNotesSize(s => s - 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.Z, shift: true))
                _editorController.EditSelectedNotesSize(s => s - 0.1f);
            if (UnityUtils.IsKeyDown(KeyCode.X))
                _editorController.EditSelectedNotesSize(s => s + 0.01f);
            else if (UnityUtils.IsKeyDown(KeyCode.X))
                _editorController.EditSelectedNotesSize(s => s + 0.1f);
            if (UnityUtils.IsKeyDown(KeyCode.M))
                _editorController.EditSelectedNotesPosition(p => -p);
            // Curve
            // IF

            // Stage
            if (UnityUtils.IsKeyDown(KeyCode.Return) || UnityUtils.IsKeyDown(KeyCode.KeypadEnter))
                gameStage.TogglePlayingState();
            if (UnityUtils.IsKeyDown(KeyCode.Space))
            {
                _tryPlayResetTime = gameStage.CurrentMusicTime;
                gameStage.Play();
            }
            else if (UnityUtils.IsKeyUp(KeyCode.Space))
            {
                gameStage.CurrentMusicTime = _tryPlayResetTime;
                gameStage.Pause();
            }
            if (UnityUtils.IsKeyDown(KeyCode.Home))
                gameStage.CurrentMusicTime = 0f;
            if (UnityUtils.IsKeyDown(KeyCode.End))
                gameStage.CurrentMusicTime = gameStage.MusicLength;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow, ctrl: true))
                gameStage.NoteSpeed += 1;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow, ctrl: true))
                gameStage.NoteSpeed -= 1;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow, alt: true))
                gameStage.MusicSpeed += 1;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow, alt: true))
                gameStage.MusicSpeed -= 1;
            if (UnityUtils.IsKeyDown(KeyCode.UpArrow))
                gameStage.StagePlaySpeed = -2.5f;
            else if (UnityUtils.IsKeyDown(KeyCode.UpArrow, shift: true))
                gameStage.StagePlaySpeed = -5.0f;
            else if (UnityUtils.IsKeyUp(KeyCode.UpArrow) || UnityUtils.IsKeyUp(KeyCode.UpArrow, shift: true))
                gameStage.StagePlaySpeed = 0f;
            if (UnityUtils.IsKeyDown(KeyCode.DownArrow))
                gameStage.StagePlaySpeed = 2.5f;
            else if (UnityUtils.IsKeyDown(KeyCode.DownArrow, shift: true))
                gameStage.StagePlaySpeed = 5.0f;
            else if (UnityUtils.IsKeyUp(KeyCode.DownArrow) || UnityUtils.IsKeyUp(KeyCode.DownArrow, shift: true))
                gameStage.StagePlaySpeed = 0f;
        }

        #endregion

        #region UI Events

        private void OnPauseClicked() => GameStageController.Instance.TogglePlayingState();

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

        public void NotifyGameStageProgressChanged(int nextHitNoteIndex)
        {
            UpdateScore(nextHitNoteIndex);
            UpdateCombo(nextHitNoteIndex);
        }

        public void NotifyStageEffectUpdate(bool isOn)
        {
            if (isOn)
            {
                const float BgPeriod = 2f;
                var ratio = Mathf.Sin(Time.time * (2 * Mathf.PI / BgPeriod));
                _backgroundBreathingMaskImage.color = new(1f, 1f, 1f, (ratio + 1f) / 2f);
            }
            else
            {
                _backgroundBreathingMaskImage.color = Color.white;
            }
        }

        #endregion

        private bool TryGetCurrentMouseToNoteCoordInSelectionRange(out NoteCoord coord)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_cameraViewTransform, Input.mousePosition, null, out var localPoint))
            {
                coord = default;
                return false;
            }

            var viewPoint = new Vector2(
                localPoint.x / _cameraViewTransform.rect.width,
                localPoint.y / _cameraViewTransform.rect.height)
                + _cameraViewTransform.pivot;

            if (viewPoint is not { x: >= 0f and <= 1f, y: >= 0f and <= 1f })
            {
                coord = default;
                return false;
            }

            if (!GameStageController.Instance.TryConvertViewPointToNoteCoord(viewPoint, out coord))
            {
                return false;
            }

            // Ignore when press position is too far
            if (coord.Position is > 4f or < -4f)
            {
                return false;
            }

            return true;
        }

        private enum MouseActionState
        {
            None,
            NoteSelecting = 1,
            NotePlacing = 2,
        }
    }
}
