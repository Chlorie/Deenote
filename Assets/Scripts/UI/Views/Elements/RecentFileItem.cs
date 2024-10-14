using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.UI.Controls;
using Deenote.Utilities;
using System;
using System.Collections.Immutable;
using System.IO;
using UnityEngine;

namespace Deenote.UI.Views.Elements
{
    public sealed class RecentFileItem : MonoBehaviour
    {
        [SerializeField] Button _button = default!;
        private string _filePath = default!;

        public MenuPageView Parent { get; internal set; } = default!;

        public string FilePath => _filePath;

        private static readonly LocalizableText[] _fileNotFoundMsgBtnTxts = new LocalizableText[] {
            LocalizableText.Raw("Remove"),
            LocalizableText.Raw("Explore"),
            LocalizableText.Raw("Cancel"),
        };

        // TODO: Localization
        private static readonly ImmutableArray<LocalizableText> _reselectFileMsgBtnTxts = ImmutableArray.Create(
            LocalizableText.Raw("Remove"),
            LocalizableText.Raw("Explore"),
            LocalizableText.Raw("Cancel"));

        private void Start()
        {
            _button.OnClick.AddListener(async UniTaskVoid () =>
            {
                // TODO: Localization and complete code
                if (File.Exists(_filePath)) {
                    // 打开项目，如果失败，给个提醒，成功就成功。
                    bool result = await MainSystem.ProjectManager.LoadProjectFileAsync(_filePath);
                    if (!result) {
                        MainSystem.MessageBox.ShowAsync(
                            LocalizableText.Raw("Open Failed"),
                            LocalizableText.Raw(""),
                            Array.Empty<LocalizableText>()).Forget();
                    }
                    return;
                }
                else {
                    // 确认移除该项/重新选择文件/取消
                    // 移除就移除
                    // 重选的话，选择成功就把当前改成选择路径，然后放到最新位置
                    var clicked = await MainSystem.MessageBox.ShowAsync(
                        LocalizableText.Raw("Load Project failed"),
                        LocalizableText.Raw("Remove this item?"),
                        _fileNotFoundMsgBtnTxts);
                    if (clicked == 0)
                        Parent.RemoveRecentFiles(this);
                    else if (clicked == 1) {
                        var res = await MainSystem.FileExplorerDialog.OpenSelectFileAsync(
                            MainSystem.Args.SupportLoadProjectFileExtensions,
                            Path.GetDirectoryName(FilePath));
                        if (res.IsCancelled)
                            return;
                        bool openRes = await MainSystem.ProjectManager.LoadProjectFileAsync(res.Path);
                        if (openRes) {
                            Parent.RemoveRecentFiles(this);
                            Parent.AddToRecentFiles(res.Path);
                        }
                    }
                }
            });
        }

        public void Initialize(string filePath)
        {
            _filePath = filePath;
            _button.Text.SetRawText(Path.GetFileName(filePath));
        }
    }
}