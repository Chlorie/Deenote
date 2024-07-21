using Deenote.Project.Models.Datas;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows.Elements
{
    [RequireComponent(typeof(Button))]
    public sealed class PianoSoundKeyController : MonoBehaviour
    {
        // Not Assigned in unity editor
        [SerializeField] PianoSoundEditWindow _editWindow;
        [SerializeField] Button _button;

        [Range(0, 127)]
        [SerializeField] int _pitch;

        private void Awake()
        {
            _button.onClick.AddListener(() =>
            {
                MainSystem.PianoSoundManager.PlaySoundAsync(_pitch, 95, null, 0f, 1f).Forget();
                _editWindow.AddSound(new PianoSoundValueData(0f, 0f, _pitch, 0));
            });
        }
    }
}