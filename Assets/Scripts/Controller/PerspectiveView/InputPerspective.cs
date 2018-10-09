using UnityEngine;
using UnityEngine.EventSystems;

public class InputPerspective : MonoBehaviour, IPointerDownHandler
{
    public static InputPerspective Instance { get; private set; }
    private static Plane _xzPlane = new Plane(Vector3.up, Vector3.zero);
    [SerializeField] private RectTransform _transform;
    private Vector2 PointerLocalPosition(PointerEventData eventData)
    {
        Vector2 localPosition = _transform.InverseTransformPoint(eventData.position);
        Vector2 windowSize = PerspectiveView.Instance.Size;
        Vector2 normalizedPosition = new Vector2(localPosition.x / windowSize.x + 0.5f, localPosition.y / windowSize.y + 0.5f);
        Vector2 texturePosition = Vector2.Scale(normalizedPosition, Parameters.Params.perspectiveRenderTextureSize);
        return texturePosition;
    }
    private Vector3 LocalToWorldPosition(Vector2 localPosition)
    {
        Ray ray = PerspectiveView.Instance.perspectiveCamera.ScreenPointToRay(localPosition);
        float enter;
        return _xzPlane.Raycast(ray, out enter) ? ray.GetPoint(enter) : Vector3.positiveInfinity;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localPosition = PointerLocalPosition(eventData);
        Vector3 worldPosition = LocalToWorldPosition(localPosition);
        StatusBar.SetStrings($"Click Position: {localPosition}, World Position: {worldPosition}");
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of InputPerspective");
        }
    }
}
