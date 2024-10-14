using Deenote.UI.Themes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    public sealed class MainWindow : MonoBehaviour
    {
        [Header("UI Theme")]
        [SerializeField] UIColorTheme _colorTheme;

        [Header("UI Elements")]
        [SerializeField] Image _backgroundImage;

        private void OnValidate()
        {
            _colorTheme.ApplyWindowColor(_backgroundImage);
        }

        internal void UpdateUI()
        {
            _colorTheme.ApplyWindowColor(_backgroundImage);
        }
    }

    [CustomEditor(typeof(MainWindow))]
    public sealed class MainWindowUnityEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var window=(MainWindow)target;

            if(GUILayout.Button("Update UI")) {
                window.UpdateUI();
            }
        }
    }
}