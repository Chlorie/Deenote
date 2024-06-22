﻿using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    private List<Vector3> worldPoints;
    public LineRenderer line;
    public void SetActive(bool state) => gameObject.SetActive(state);
    public int Layer
    {
        get => line.sortingOrder;
        set => line.sortingOrder = value;
    }
    public Color Color
    {
        get
        {
            Gradient gradient = line.colorGradient;
            return gradient.colorKeys[0].color;
        }
        set
        {
            Gradient gradient = line.colorGradient;
            GradientColorKey[] colorKeys = gradient.colorKeys;
            for (int i = 0; i < gradient.colorKeys.Length; i++) colorKeys[i].color = value;
            gradient.colorKeys = colorKeys;
            line.colorGradient = gradient;
        }
    }
    public float Width
    {
        get => line.widthMultiplier;
        set => line.widthMultiplier = value;
    }
    public float AlphaMultiplier
    {
        set
        {
            Gradient gradient = line.colorGradient;
            GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
            for (int i = 0; i < gradient.alphaKeys.Length; i++) alphaKeys[i].alpha *= value;
            gradient.alphaKeys = alphaKeys;
            line.colorGradient = gradient;
        }
    }
    public void MoveTo(Vector3 point1, Vector3 point2) // The coordinates are in world space
    {
        worldPoints = new List<Vector3> { point1, point2 };
        Color color = Color;
        // t gird?
        if (point1.z - point2.z <= (Parameters.maximumNoteRange - Parameters.alpha1NoteRange) * Parameters.minAlphaDif)
        {
            float alpha;
            if (point2.z < Parameters.alpha1NoteRange) alpha = 1.0f;
            else alpha = (Parameters.maximumNoteRange - point2.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
            Gradient gradient = line.colorGradient;
            gradient.alphaKeys = new[] { new GradientAlphaKey { alpha = alpha, time = 0.0f } };
            line.colorGradient = gradient;
            line.positionCount = 2;
            line.SetPositions(new[] { Utility.WorldToScreenPoint(point1), Utility.WorldToScreenPoint(point2) });
        }
        // x
        else
        {
            // 抵消notepanel的偏移
            point1.z -= 32.0f; point2.z -= 32.0f;
            // p1长度大于notepanel长度
            if (point1.z > Parameters.maximumNoteRange)
            {
                // 入参直线与z=max的交点为x
                float x = point1.x + (point2.x - point1.x) * (point1.z - Parameters.maximumNoteRange) / (point1.z - point2.z);
                
                //p1在max之外时，重设为max
                point1 = new Vector3(x, 0.0f, Parameters.maximumNoteRange);
            }
            // p2在淡入段落以上
            if (point2.z > Parameters.alpha1NoteRange)
            {
                float farAlpha = (Parameters.maximumNoteRange - point1.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
                float nearAlpha = (Parameters.maximumNoteRange - point2.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
                point1.z += 32.0f; point2.z += 32.0f;
                Vector3 far = Utility.WorldToScreenPoint(point1);
                Vector3 near = Utility.WorldToScreenPoint(point2);
                Gradient gradient = line.colorGradient;
                gradient.alphaKeys = new[]
                {
                    new GradientAlphaKey { alpha = farAlpha, time = 0.0f },
                    new GradientAlphaKey { alpha = nearAlpha, time = 1.0f }
                };
                line.colorGradient = gradient;
                line.positionCount = 2;
                line.SetPositions(new[] { far, near });
            }
            else if (point1.z < Parameters.alpha1NoteRange)
            {
                point1.z += 32.0f; point2.z += 32.0f;
                Vector3 far = Utility.WorldToScreenPoint(point1);
                Vector3 near = Utility.WorldToScreenPoint(point2);
                Gradient gradient = line.colorGradient;
                gradient.alphaKeys = new[] { new GradientAlphaKey { alpha = 1.0f, time = 0.0f } };
                line.colorGradient = gradient;
                line.positionCount = 2;
                line.SetPositions(new[] { far, near });
            }
            else
            {
                float x = point1.x + (point2.x - point1.x) * (point1.z - Parameters.alpha1NoteRange) / (point1.z - point2.z);
                Vector3 cross = new Vector3(x, 0.0f, Parameters.alpha1NoteRange);
                float alpha = (Parameters.maximumNoteRange - point1.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
                point1.z += 32.0f; cross.z += 32.0f; point2.z += 32.0f;
                Vector3 far = Utility.WorldToScreenPoint(point1);
                Vector3 mid = Utility.WorldToScreenPoint(cross);
                Vector3 near = Utility.WorldToScreenPoint(point2);
                float midTime = (far - mid).magnitude / (far - near).magnitude;
                Gradient gradient = line.colorGradient;
                gradient.alphaKeys = new[]
                {
                    new GradientAlphaKey { alpha = alpha, time = 0.0f },
                    new GradientAlphaKey { alpha = 1.0f, time = midTime },
                    new GradientAlphaKey { alpha = 1.0f, time = 1.0f }
                };
                line.colorGradient = gradient;
                line.positionCount = 3;
                line.SetPositions(new[] { far, mid, near });
            }
        }
        line.sortingOrder = 0;
    }
    private float CalculateAlpha(Vector3 v)
    {
        float z = v.z - 32.0f;
        if (z < Parameters.alpha1NoteRange) return 1.0f;
        else return (Parameters.maximumNoteRange - z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
    }
    public void CurveMoveTo(List<Vector3> points) // The coordinates are in world space
    {
        worldPoints = points;
        // Assume that the points are all in stage
        int n = points.Count;
        if (n < 2) { MoveTo(Vector3.zero, Vector3.zero); return; }
        Vector3[] screenPoints = new Vector3[n];
        float[] sums = new float[n];
        for (int i = 0; i < n; i++)
        {
            screenPoints[i] = Utility.WorldToScreenPoint(points[i]);
            if (i == 0) sums[i] = 0.0f;
            else sums[i] = sums[i - 1] + (screenPoints[i] - screenPoints[i - 1]).magnitude;
        }
        Gradient gradient = line.colorGradient;
        GradientAlphaKey[] alphaKeys = null;
        bool outOfAlpha1 = false;
        for (int i = 0; i < n; i++)
            if (points[i].z > Parameters.alpha1NoteRange + 32.0f)
            {
                outOfAlpha1 = true;
                alphaKeys = new[]
                {
                    new GradientAlphaKey { alpha = CalculateAlpha(points[0]), time = 0.0f },
                    new GradientAlphaKey { alpha = CalculateAlpha(points[i]), time = sums[i] / sums[n - 1] },
                    new GradientAlphaKey { alpha = CalculateAlpha(points[n - 1]), time = 1.0f }
                };
                break;
            }
        if (!outOfAlpha1)
            alphaKeys = new[]
            {
                new GradientAlphaKey { alpha = 1.0f, time = 0.0f },
                new GradientAlphaKey { alpha = 1.0f, time = 1.0f }
            };
        gradient.alphaKeys = alphaKeys;
        line.colorGradient = gradient;
        line.positionCount = n;
        line.SetPositions(screenPoints);
        line.sortingOrder = 0;
    }
    public void ResolutionReset()
    {
        if (!gameObject.activeSelf) return;
        if (worldPoints.Count == 2) MoveTo(worldPoints[0], worldPoints[1]);
        else CurveMoveTo(worldPoints);
    }
}
