using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Deenote.UI.Windows.Elements
{
    public sealed class ProjectPropertiesSubAudioFileController : MonoBehaviour
    {
        [SerializeField] Button _switchButton;
        [SerializeField] TMP_Text _fileText;
        [SerializeField] Button _removeButton;

        private UnityAction<ProjectPropertiesSubAudioFileController> _onRemove;

        public string FilePath;

        public void Initialize(string filePath, UnityAction<ProjectPropertiesSubAudioFileController> onRemove)
        {
            _fileText.text = Path.GetFileName(filePath);
            _onRemove = onRemove;
            _removeButton.onClick.AddListener(() => _onRemove.Invoke(this));
        }

        private void OnDisable()
        {
            _onRemove = null;
            _removeButton.onClick.RemoveAllListeners();
        }
    }
}