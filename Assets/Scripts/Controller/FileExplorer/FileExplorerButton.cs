using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FileExplorerButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Text _buttonText;
    public Callback callback;
    public string ButtonText { set => _buttonText.text = value; }
    public Color ButtonTextColor { set => _buttonText.color = value; }
    public void OnPointerClick(PointerEventData eventData) => callback?.Invoke();
}
