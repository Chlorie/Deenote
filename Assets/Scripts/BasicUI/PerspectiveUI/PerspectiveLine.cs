using System.Collections.Generic;
using UnityEngine;

public class PerspectiveLine : MonoBehaviour
{
    private List<Vector3> _worldPoints;
    [SerializeField] private LineRenderer _line;
    public void SetActive(bool state) => gameObject.SetActive(state);
    public int Layer
    {
        get { return _line.sortingOrder; }
        set { _line.sortingOrder = value; }
    }
    public Color Color
    {
        get { return _line.colorGradient.colorKeys[0].color; }
        set
        {
            Gradient gradient = _line.colorGradient;
            GradientColorKey[] colorKeys = gradient.colorKeys;
            for (int i = 0; i < gradient.colorKeys.Length; i++) colorKeys[i].color = value;
            gradient.colorKeys = colorKeys;
            _line.colorGradient = gradient;
        }
    }
    public float Width
    {
        get { return _line.widthMultiplier; }
        set { _line.widthMultiplier = value; }
    }
    public float AlphaMultiplier
    {
        set
        {
            Gradient gradient = _line.colorGradient;
            GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
            for (int i = 0; i < gradient.alphaKeys.Length; i++) alphaKeys[i].alpha *= value;
            gradient.alphaKeys = alphaKeys;
            _line.colorGradient = gradient;
        }
    }
    public void MoveTo(Vector3 point1, Vector3 point2) // The coordinates are in world space
    {
        _worldPoints = new List<Vector3> { point1, point2 };
        float maxDistance = Parameters.Params.perspectiveMaxDistance;
        float opaqueDistance = Parameters.Params.perspectiveOpaqueDistance;
        float minDeltaAlpha = Parameters.Params.minDeltaAlpha;
        if (point1.z - point2.z <= (maxDistance - opaqueDistance) * minDeltaAlpha)
        {
            float alpha = (point2.z < minDeltaAlpha) ? 1.0f : (maxDistance - point2.z) / (maxDistance - opaqueDistance);
            Gradient gradient = _line.colorGradient;
            gradient.alphaKeys = new[] { new GradientAlphaKey { alpha = alpha, time = 0.0f } };
            _line.colorGradient = gradient;
            _line.positionCount = 2;
            _line.SetPositions(new[]
            {
                PerspectiveView.Instance.WorldPointTransform(point1),
                PerspectiveView.Instance.WorldPointTransform(point2)
            });
        }
        else
        {
            if (point1.z > maxDistance)
            {
                float x = point1.x + (point2.x - point1.x) * (point1.z - maxDistance) / (point1.z - point2.z);
                point1 = new Vector3(x, 0.0f, maxDistance);
            }
            if (point2.z > opaqueDistance)
            {
                float farAlpha = (maxDistance - point1.z) / (maxDistance - opaqueDistance);
                float nearAlpha = (maxDistance - point2.z) / (maxDistance - opaqueDistance);
                Vector3 far = PerspectiveView.Instance.WorldPointTransform(point1);
                Vector3 near = PerspectiveView.Instance.WorldPointTransform(point2);
                Gradient gradient = _line.colorGradient;
                gradient.alphaKeys = new[]
                {
                    new GradientAlphaKey { alpha = farAlpha, time = 0.0f },
                    new GradientAlphaKey { alpha = nearAlpha, time = 1.0f }
                };
                _line.colorGradient = gradient;
                _line.positionCount = 2;
                _line.SetPositions(new[] { far, near });
            }
            else if (point1.z < opaqueDistance)
            {
                Vector3 far = PerspectiveView.Instance.WorldPointTransform(point1);
                Vector3 near = PerspectiveView.Instance.WorldPointTransform(point2);
                Gradient gradient = _line.colorGradient;
                gradient.alphaKeys = new[] { new GradientAlphaKey { alpha = 1.0f, time = 0.0f } };
                _line.colorGradient = gradient;
                _line.positionCount = 2;
                _line.SetPositions(new[] { far, near });
            }
            else
            {
                float x = point1.x + (point2.x - point1.x) * (point1.z - opaqueDistance) / (point1.z - point2.z);
                Vector3 cross = new Vector3(x, 0.0f, opaqueDistance);
                float alpha = (maxDistance - point1.z) / (maxDistance - opaqueDistance);
                Vector3 far = PerspectiveView.Instance.WorldPointTransform(point1);
                Vector3 mid = PerspectiveView.Instance.WorldPointTransform(cross);
                Vector3 near = PerspectiveView.Instance.WorldPointTransform(point2);
                float midTime = (far - mid).magnitude / (far - near).magnitude;
                Gradient gradient = _line.colorGradient;
                gradient.alphaKeys = new[]
                {
                    new GradientAlphaKey { alpha = alpha, time = 0.0f },
                    new GradientAlphaKey { alpha = 1.0f, time = midTime },
                    new GradientAlphaKey { alpha = 1.0f, time = 1.0f }
                };
                _line.colorGradient = gradient;
                _line.positionCount = 3;
                _line.SetPositions(new[] { far, mid, near });
            }
        }
        _line.sortingOrder = 0;
    }
    private float CalculateAlpha(Vector3 point)
    {
        float maxDistance = Parameters.Params.perspectiveMaxDistance;
        float opaqueDistance = Parameters.Params.perspectiveOpaqueDistance;
        if (point.z < opaqueDistance) return 1.0f;
        return (maxDistance - point.z) / (maxDistance - opaqueDistance);
    }
    public void CurveMoveTo(List<Vector3> points) // The coordinates are in world space
    {
        _worldPoints = points;
        // Assume that the points are all in stage
        int n = points.Count;
        if (n < 2) { MoveTo(Vector3.zero, Vector3.zero); return; }
        Vector3[] screenPoints = new Vector3[n];
        float[] sums = new float[n];
        for (int i = 0; i < n; i++)
        {
            screenPoints[i] = PerspectiveView.Instance.WorldPointTransform(points[i]);
            if (i == 0) sums[i] = 0.0f;
            else sums[i] = sums[i - 1] + (screenPoints[i] - screenPoints[i - 1]).magnitude;
        }
        Gradient gradient = _line.colorGradient;
        GradientAlphaKey[] alphaKeys = null;
        bool outOfOpaque = false;
        for (int i = 0; i < n; i++)
            if (points[i].z > Parameters.Params.perspectiveOpaqueDistance)
            {
                outOfOpaque = true;
                alphaKeys = new[]
                {
                    new GradientAlphaKey { alpha = CalculateAlpha(points[0]), time = 0.0f },
                    new GradientAlphaKey { alpha = CalculateAlpha(points[i]), time = sums[i] / sums[n - 1] },
                    new GradientAlphaKey { alpha = CalculateAlpha(points[n - 1]), time = 1.0f }
                };
                break;
            }
        if (!outOfOpaque)
            alphaKeys = new[]
            {
                new GradientAlphaKey { alpha = 1.0f, time = 0.0f },
                new GradientAlphaKey { alpha = 1.0f, time = 1.0f }
            };
        gradient.alphaKeys = alphaKeys;
        _line.colorGradient = gradient;
        _line.positionCount = n;
        _line.SetPositions(screenPoints);
        _line.sortingOrder = 0;
    }
    public void OnWindowResize()
    {
        if (!gameObject.activeSelf) return;
        if (_worldPoints.Count == 2)
            MoveTo(_worldPoints[0], _worldPoints[1]);
        else
            CurveMoveTo(_worldPoints);
    }
}
