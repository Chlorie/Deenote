#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Library;
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using Deenote.Entities;
using Deenote.Entities.Models;

namespace Deenote.UI.Dialogs
{
    partial class NewProjectDialog
    {
        public async UniTask<ProjectModel?> OpenCreateNewAsync()
        {
            OpenSelfModalDialog();
            ResetDialog();

            _cts.Reset();

            try {
            ReAwaitButtonClick:

                await _createButton.OnClickAsync(_cts.Token);

                if (_projectResultPath is null) {
                    Debug.Assert(false, "Unexcepted value");
                    goto ReAwaitButtonClick;
                }

                string audioFilePath = _audioFilePath.Text;
                if (!File.Exists(audioFilePath)) {
                    await MainWindow.DialogManager.OpenMessageBoxAsync(_audioNotExistsMsgBoxArgs);
                    goto ReAwaitButtonClick;
                }
                if (Directory.Exists(_projectResultPath)) {
                    await MainWindow.DialogManager.OpenMessageBoxAsync(_dirExistsMsgBoxArgs);
                    goto ReAwaitButtonClick;
                }
                if (File.Exists(_projectResultPath)) {
                    var res = await MainWindow.DialogManager.OpenMessageBoxAsync(_fileExistsMsgBoxArgs);
                    if (res != 0)
                        goto ReAwaitButtonClick;
                }

                // Project creation

                {
                    // LoadAudio
                    // TODO: Lazy Load Audio
                    using var audioFs = File.OpenRead(audioFilePath);
                    AudioClip? clip;
                    SetBlockInput(true);
                    clip = await AudioUtils.TryLoadAsync(audioFs, Path.GetExtension(audioFilePath), _cts.Token);
                    if (clip is null) {
                        await MainWindow.DialogManager.OpenMessageBoxAsync(_audioLoadFailedMsgBoxArgs);
                        SetBlockInput(false);
                        goto ReAwaitButtonClick;
                    }

                    // CreateProject

                    byte[] audioBytes = new byte[audioFs.Length];
                    audioFs.Seek(0, SeekOrigin.Begin);
                    audioFs.Read(audioBytes);
                    var proj = new ProjectModel(_projectResultPath, audioBytes, Path.GetRelativePath(_projectResultPath, audioFilePath)) {
                        AudioClip = clip,
                        MusicName = _projectName.Text
                    };
                    proj.Charts.Add(new ChartModel(new()) {
                        Difficulty = Difficulty.Hard,
                        Level = "10",
                    });
                    return proj;
                }
            } catch (OperationCanceledException) {
                Debug.Log("Creating new project cancalled");
                return null;
            } finally {
                SetBlockInput(false);
                CloseSelfModalDialog();
            }
        }
    }
}