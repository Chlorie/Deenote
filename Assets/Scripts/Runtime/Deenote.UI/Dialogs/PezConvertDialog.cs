#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Entities.Models;
using Deenote.Entities.Storage;
using Deenote.Library.Components;
using Deenote.Library.IO;
using Deenote.Localization;
using Deenote.UIFramework.Controls;
using System;
using System.Collections.Immutable;
using System.IO;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    [RequireComponent(typeof(Dialog))]
    public partial class PezConvertDialog : ModalDialog
    {
        [SerializeField] Dialog _dialog = default!;

        [SerializeField] TextBox _inputFileInput = default!;
        [SerializeField] Button _inputFileExploreButton = default!;
        [SerializeField] TextBlock _inputFileErrorText = default!;

        [SerializeField] TextBox _outputDirInput = default!;
        [SerializeField] Button _outputDirExploreButton = default!;
        [SerializeField] TextBlock _outputDirErrorText = default!;

        [SerializeField] TextBox _outputFileNameInput = default!;
        [SerializeField] TextBlock _outputFileNameErrorText = default!;
        [SerializeField] TextBox _songNameInput = default!;
        [SerializeField] TextBox _composerNameInput = default!;
        [SerializeField] TextBox _charterNameInput = default!;
        [SerializeField] TextBox _coverPathInput = default!;
        [SerializeField] Button _coverPathExploreButton = default!;
        [SerializeField] TextBlock _coverPathErrorText = default!;
        [SerializeField] TextBox _illustratorInput = default!;
        [SerializeField] NumericStepper _speedNumericStepper = default!;

        [SerializeField] Slider _speedCoefficientSlider = default!;
        [SerializeField] TextBox _speedCoefficientInput = default!;
        [SerializeField] Vector2 _speedExponentRange = new(0f, 3f);
        [SerializeField] Slider _speedExponentSlider = default!;
        [SerializeField] TextBox _speedExponentInput = default!;
        [SerializeField] Slider _widthCoefficientSlider = default!;
        [SerializeField] TextBox _widthCoefficientInput = default!;
        [SerializeField] Vector2 _widthExponentRange = new(0f, 3f);
        [SerializeField] Slider _widthExponentSlider = default!;
        [SerializeField] TextBox _widthExponentInput = default!;
        [SerializeField] Vector2 _widthMultiplierRange = new(0f, 5f);
        [SerializeField] Slider _widthMultiplierSlider = default!;
        [SerializeField] TextBox _widthMultiplierInput = default!;
        [SerializeField] Vector2 _holdDragIntervalRange = new(0.005f, 0.5f);
        [SerializeField] Slider _holdDragIntervalSlider = default!;
        [SerializeField] TextBox _holdDragIntervalInput = default!;
        [SerializeField] Vector2 _holdAlphaRange = new(0, 255);
        [SerializeField] Slider _holdAlphaSlider = default!;
        [SerializeField] TextBox _holdAlphaInput = default!;
        [SerializeField] ToggleSwitch _needFlickClickToggle = default!;
        [SerializeField] ToggleSwitch _earlyDisplaySlowNotesToggle = default!;

        [SerializeField] Button _useDefaultButton = default!;
        [SerializeField] Button _convertButton = default!;
        [SerializeField] Button _cancelButton = default!;

        private Settings _settings = default!;
        private ProjectModel? _projectModel;

        private (bool IsValid, string Text) _inputFilePath;
        private (bool IsValid, string Text) _outputDir;
        private (bool IsValid, string Text) _outputFileName;
        private (bool IsValid, string Text) _coverPath;

        private readonly ImmutableArray<string> _inputFileExtensions = MainSystem.Args.SupportLoadProjectFileExtensions;
        private readonly ImmutableArray<string> _coverFileExtensions = ImmutableArray.Create(".png", ".jpg", ".jpeg");

        #region LocalizedTextKeys

        private const string DialogTitleKey = "Dialog_PezConvert_Title";
        private const string SelectInputFileExplorerTitleKey = "PezConvert_FileExplorer_SelectInputFile_Title";
        private const string SelectOutputDirExplorerTitleKey = "PezConvert_FileExplorer_SelectOutputDir_Title";
        private const string SelectCoverFileExplorerTitleKey = "PezConvert_FileExplorer_SelectCover_Title";

        private const string InputFileInvalidErrorKey = "PezConvert_InputFile_Error_Invalid";
        private const string InputFileLoadFailedErrorKey = "PezConvert_InputFile_Error_LoadFailed";
        private const string OutputDirInvalidErrorKey = "PezConvert_OutputDir_Error_Invalid";
        private const string OutputFileNameInvalidErrorKey = "PezConvert_OutputFileName_Error_Invalid";
        private const string CoverPathInvalidErrorKey = "PezConvert_CoverPath_Error_Invalid";

        #endregion

        protected override void Awake()
        {
            base.Awake();
            _settings = new();
            _coverPath = (true, string.Empty); // Cover is optional; treat empty as valid by default.
        }

        private void Start()
        {
            _useDefaultButton.Clicked += _settings.ResetToDefault;

            BindPathInput(_inputFileInput, _inputFileErrorText, InputFileInvalidErrorKey, val => _inputFilePath = val);
            _inputFileInput.EditSubmitted += async val =>
            {
                if (!_inputFilePath.IsValid) {
                    _projectModel = null;
                    UpdateConvertInteractable();
                    return;
                }

                var projectModel = val == MainSystem.ProjectManager.CurrentProject?.ProjectFilePath
                    ? MainSystem.ProjectManager.CurrentProject.CloneForSave()
                    : await ProjectIO.LoadAsync(val);
                UpdateProjectModel(projectModel);
            };
            _inputFileExploreButton.Clicked += UniTask.Action(async UniTaskVoid () =>
            {
                var res = await MainWindow.DialogManager.OpenFileExplorerSelectFileAsync(
                    LocalizableText.Localized(SelectInputFileExplorerTitleKey), _inputFileExtensions);
                if (res.IsCancelled) return;
                var projectModel = res.Path == MainSystem.ProjectManager.CurrentProject?.ProjectFilePath
                    ? MainSystem.ProjectManager.CurrentProject.CloneForSave()
                    : await ProjectIO.LoadAsync(res.Path);
                UpdateProjectModel(projectModel);
            });

            BindPathInput(_outputDirInput, _outputDirErrorText, OutputDirInvalidErrorKey, val => _outputDir = val);
            _outputDirInput.EditSubmitted += val =>
            {
                if (_outputDir.IsValid) _settings.OutputDirectory = val;
            };
            _outputDirExploreButton.Clicked += UniTask.Action(async UniTaskVoid () =>
            {
                var res = await MainWindow.DialogManager.OpenFileExplorerSelectDirectoryAsync(
                    LocalizableText.Localized(SelectOutputDirExplorerTitleKey));
                if (res.IsCancelled) return;
                _outputDirInput.Value = res.Path;
                _settings.OutputDirectory = res.Path;
            });
            _settings.RegisterNotificationAndInvoke(Settings.NotificationFlag.OutputDirectory,
                setting => _outputDirInput.Value = setting.OutputDirectory);

            _outputFileNameInput.ValueChanged += val =>
            {
                bool valid = !string.IsNullOrEmpty(val) && PathUtils.IsValidFileName(val);
                if (!valid) {
                    _outputFileNameErrorText.SetLocalizedText(OutputFileNameInvalidErrorKey);
                    _outputFileNameErrorText.gameObject.SetActive(true);
                }
                else {
                    _outputFileNameErrorText.gameObject.SetActive(false);
                }

                _outputFileName = (valid, val);
                UpdateConvertInteractable();
            };

            BindPathInput(_coverPathInput, _coverPathErrorText,
                CoverPathInvalidErrorKey, val => _coverPath = val, true);
            _coverPathExploreButton.Clicked += UniTask.Action(async UniTaskVoid () =>
            {
                var res = await MainWindow.DialogManager.OpenFileExplorerSelectFileAsync(
                    LocalizableText.Localized(SelectCoverFileExplorerTitleKey), _coverFileExtensions);
                if (res.IsCancelled) return;
                _coverPathInput.Value = res.Path;
            });

            _speedNumericStepper.SetInputParser(static input =>
                float.TryParse(input, out var val) ? Mathf.RoundToInt(val * 10f) : null);
            _speedNumericStepper.SetDisplayerTextSelector(static ival => $"{ival / 10}.{ival % 10}");
            _speedNumericStepper.ValueChanged += val => _settings.Speed = val / 10f;
            _settings.RegisterNotificationAndInvoke(Settings.NotificationFlag.Speed,
                setting => _speedNumericStepper.SetValueWithoutNotify(Mathf.RoundToInt(setting.Speed * 10f)));

            BindSliderInput(_speedCoefficientSlider, _speedCoefficientInput, new(0, 1),
                () => _settings.SpeedCoefficient,
                val => _settings.SpeedCoefficient = val,
                Settings.NotificationFlag.SpeedCoefficient);
            BindSliderInput(_speedExponentSlider, _speedExponentInput, _speedExponentRange,
                () => _settings.SpeedExponent,
                val => _settings.SpeedExponent = val,
                Settings.NotificationFlag.SpeedExponent);
            BindSliderInput(_widthCoefficientSlider, _widthCoefficientInput, new(0, 1),
                () => _settings.WidthCoefficient,
                val => _settings.WidthCoefficient = val,
                Settings.NotificationFlag.WidthCoefficient);
            BindSliderInput(_widthExponentSlider, _widthExponentInput, _widthExponentRange,
                () => _settings.WidthExponent,
                val => _settings.WidthExponent = val,
                Settings.NotificationFlag.WidthExponent);
            BindSliderInput(_widthMultiplierSlider, _widthMultiplierInput, _widthMultiplierRange,
                () => _settings.WidthMultiplier,
                val => _settings.WidthMultiplier = val,
                Settings.NotificationFlag.WidthMultiplier);
            BindSliderInput(_holdDragIntervalSlider, _holdDragIntervalInput, _holdDragIntervalRange,
                () => _settings.HoldDragInterval,
                val => _settings.HoldDragInterval = val,
                Settings.NotificationFlag.HoldDragInterval, "0.00");
            BindSliderInput(_holdAlphaSlider, _holdAlphaInput, _holdAlphaRange,
                () => _settings.HoldAlpha,
                val => _settings.HoldAlpha = Mathf.RoundToInt(val),
                Settings.NotificationFlag.HoldAlpha, "F0");

            _needFlickClickToggle.IsCheckedChanged += val => _settings.NeedFlickClick = val;
            _settings.RegisterNotificationAndInvoke(Settings.NotificationFlag.NeedFlickClick,
                setting => _needFlickClickToggle.SetIsCheckedWithoutNotify(setting.NeedFlickClick));

            _earlyDisplaySlowNotesToggle.IsCheckedChanged += val => _settings.EarlyDisplaySlowNotes = val;
            _settings.RegisterNotificationAndInvoke(Settings.NotificationFlag.EarlyDisplaySlowNotes,
                setting => _earlyDisplaySlowNotesToggle.SetIsCheckedWithoutNotify(setting.EarlyDisplaySlowNotes));
        }

        private void BindSliderInput(Slider slider, TextBox input, Vector2 range,
            Func<float> getter, Action<float> setter, Settings.NotificationFlag flag, string format = "0.0")
        {
            slider.ValueChanged += val => setter.Invoke(val * (range.y - range.x) + range.x);
            input.EditSubmitted += val =>
            {
                if (float.TryParse(val, out var fval)) setter.Invoke(Mathf.Clamp(fval, range.x, range.y));
                else UpdateUI();
            };
            _settings.RegisterNotificationAndInvoke(flag, _ => UpdateUI());
            return;

            void UpdateUI()
            {
                float val = getter.Invoke();
                input.SetValueWithoutNotify(val.ToString(format));
                slider.SetValueWithoutNotify((val - range.x) / (range.y - range.x));
            }
        }

        private void BindPathInput(TextBox input, TextBlock errorText, string errorKey,
            Action<(bool IsValid, string Text)> setter, bool allowEmpty = false)
        {
            input.ValueChanged += val =>
            {
                bool valid = (allowEmpty && string.IsNullOrEmpty(val)) || IsValidPath(val);
                if (!valid) errorText.SetLocalizedText(errorKey);
                errorText.gameObject.SetActive(!valid);
                setter.Invoke((valid, val));
                UpdateConvertInteractable();
            };
        }

        private void UpdateProjectModel(ProjectModel? projectModel)
        {
            _projectModel = projectModel;
            _inputFileInput.Value = _projectModel?.ProjectFilePath ?? string.Empty;
            if (_projectModel is not null) {
                _outputFileNameInput.Value = _projectModel.MusicName;
                _songNameInput.Value = _projectModel.MusicName;
                _composerNameInput.Value = _projectModel.Composer;
                _charterNameInput.Value = _projectModel.ChartDesigner;
                _inputFileErrorText.gameObject.SetActive(false);
            }
            else {
                _inputFileErrorText.SetLocalizedText(InputFileLoadFailedErrorKey);
                _inputFileErrorText.gameObject.SetActive(true);
                _inputFilePath.IsValid = false;
            }

            UpdateConvertInteractable();
        }
        
        private void UpdateConvertInteractable()
        {
            _convertButton.IsInteractable =
                _inputFilePath.IsValid &&
                _outputDir.IsValid &&
                _outputFileName.IsValid &&
                _coverPath.IsValid &&
                _projectModel is not null;
        }

        private static bool IsValidPath(string input)
            => !string.IsNullOrWhiteSpace(input) && PathUtils.IsValidPath(input) && Path.IsPathFullyQualified(input) &&
               (File.Exists(input) || Directory.Exists(input));

        public async UniTask<Result> OpenAsync()
        {
            OpenSelfModalDialog();
            _inputFileErrorText.gameObject.SetActive(false);
            _outputDirErrorText.gameObject.SetActive(false);
            _outputFileNameErrorText.gameObject.SetActive(false);
            _coverPathErrorText.gameObject.SetActive(false);

            UpdateConvertInteractable();

            _dialog.SetTitle(LocalizableText.Localized(DialogTitleKey));
            
            await UniTask.DelayFrame(1);
            UpdateProjectModel(MainSystem.ProjectManager.CurrentProject);

            var clickTask = UniTask.WhenAny(
                _convertButton.OnClickAsync(),
                _cancelButton.OnClickAsync(),
                _dialog.CloseButton.OnClickAsync());
            int click = await clickTask;
            CloseSelfModalDialog();

            if (_projectModel is not null) {
                _projectModel.MusicName = _songNameInput.Value;
                _projectModel.Composer = _composerNameInput.Value;
                _projectModel.ChartDesigner = _charterNameInput.Value;
            }

            return new Result {
                IsCancelled = click != 0,
                Project = _projectModel,
                CoverPath = _coverPath.Text,
                Illustrator = _illustratorInput.Value,
                OutputDirectory = _outputDir.Text,
                OutputFileName = _outputFileName.Text,
                Speed = _settings.Speed,
                SpeedCoefficient = _settings.SpeedCoefficient,
                SpeedExponent = _settings.SpeedExponent,
                WidthCoefficient = _settings.WidthCoefficient,
                WidthExponent = _settings.WidthExponent,
                WidthMultiplier = _settings.WidthMultiplier,
                HoldDragInterval = _settings.HoldDragInterval,
                HoldAlpha = _settings.HoldAlpha,
                NeedFlickClick = _settings.NeedFlickClick,
                EarlyDisplaySlowNotes = _settings.EarlyDisplaySlowNotes,
            };
        }

        public struct Result
        {
            public bool IsCancelled { get; set; }

            // Input project
            public ProjectModel? Project { get; set; }
            public string? CoverPath { get; set; }
            public string? Illustrator { get; set; }

            // Output settings
            public string OutputDirectory { get; set; }
            public string OutputFileName { get; set; }

            // Conversion settings
            public float Speed { get; set; }
            public float SpeedCoefficient { get; set; }
            public float SpeedExponent { get; set; }
            public float WidthCoefficient { get; set; }
            public float WidthExponent { get; set; }
            public float WidthMultiplier { get; set; }
            public float HoldDragInterval { get; set; }
            public int HoldAlpha { get; set; }
            public bool NeedFlickClick { get; set; }
            public bool EarlyDisplaySlowNotes { get; set; }
        }
    }
}