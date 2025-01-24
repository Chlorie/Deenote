#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.GamePlay.UI;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deenote.GamePlay.Stage
{
    public sealed class GameStageSceneLoader : MonoBehaviour
    {
        private static Scene? _loadedStageScene;
        private static GameStageSceneLoader? _instance;

        public static event Action<GameStageSceneLoader>? StageLoaded;

        [field: SerializeField]
        public GameStageControllerBase StageController { get; private set; } = default!;

        [field: SerializeField]
        public GameStagePerspectiveCamera GameStagePerspectiveCamera { get; private set; } = default!;

        [field: SerializeField]
        public PerspectiveLinesRenderer PerspectiveLinesRenderer { get; private set; } = default!;

        [field: SerializeField] 
        public PerspectiveViewForegroundBase PerspectiveViewForeground { get; private set; } = default!;

        private void Awake()
        {
            _instance = this;
        }

        public static async UniTask<GameStageSceneLoader> LoadAsync(string scene)
        {
            var prevLoadedScene = _loadedStageScene;

            var loadOp = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            SceneManager.sceneLoaded += static (scene, mode) => _loadedStageScene = scene;
            await loadOp;
            if (prevLoadedScene is { } loadedScene) {
                _ = SceneManager.UnloadSceneAsync(loadedScene);
            }
            StageLoaded?.Invoke(_instance!);
            Debug.Assert(_instance != null);
            return _instance!;
        }
    }
}