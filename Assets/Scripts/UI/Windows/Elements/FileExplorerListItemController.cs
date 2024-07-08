using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows.Elements
{
    public sealed class FileExplorerListItemController : MonoBehaviour
    {
        [SerializeField] Button _button;
        [SerializeField] TMP_Text _text;

        private FileExplorerWindow _dialog;

        public string Path { get; private set; }
        public bool IsDirectory { get; private set; }

        public string FileName => _text.text;

        public void OnCreated(FileExplorerWindow dialog)
        {
            _dialog = dialog;
        }

        public void Initialize(string path, bool isDirectory)
        {
            IsDirectory = isDirectory;
            Path = path;

            if (isDirectory) {
                _text.text = System.IO.Path.GetFileName(path);
                _text.color = new(0.5f, 0.5f, 0.5f, 1f);
            }
            else {
                _text.text = System.IO.Path.GetFileName(path);
                _text.color = Color.white;
            }
        }

        private void Awake()
        {
            _button.onClick.AddListener(() => _dialog.SelectItem(this));
        }
    }
}