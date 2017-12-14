using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public LineRenderer line;
    public void SetActive(bool state) { gameObject.SetActive(state); }
    public int Layer
    {
        get
        {
            return line.sortingOrder;
        }
        set
        {
            line.sortingOrder = value;
        }
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
        get { return line.widthMultiplier; }
        set { line.widthMultiplier = value; }
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
        Color color = Color;
        if (point1.z - point2.z <= (Parameters.maximumNoteRange - Parameters.alpha1NoteRange) * Parameters.minAlphaDif)
        {
            float alpha;
            if (point2.z < Parameters.alpha1NoteRange) alpha = 1.0f;
            else alpha = (Parameters.maximumNoteRange - point2.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
            Gradient gradient = line.colorGradient;
            gradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey { alpha = alpha, time = 0.0f } };
            line.colorGradient = gradient;
            line.positionCount = 2;
            line.SetPositions(new Vector3[] { Utility.WorldToScreenPoint(point1), Utility.WorldToScreenPoint(point2) });
        }
        else
        {
            point1.z -= 32.0f; point2.z -= 32.0f;
            if (point1.z > Parameters.maximumNoteRange)
            {
                float x = point1.x + (point2.x - point1.x) * (point1.z - Parameters.maximumNoteRange) / (point1.z - point2.z);
                point1 = new Vector3(x, 0.0f, Parameters.maximumNoteRange);
            }
            if (point2.z > Parameters.alpha1NoteRange)
            {
                Vector3 far, near;
                float farAlpha, nearAlpha;
                farAlpha = (Parameters.maximumNoteRange - point1.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
                nearAlpha = (Parameters.maximumNoteRange - point2.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
                point1.z += 32.0f; point2.z += 32.0f;
                far = Utility.WorldToScreenPoint(point1);
                near = Utility.WorldToScreenPoint(point2);
                Gradient gradient = line.colorGradient;
                gradient.alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey { alpha = farAlpha, time = 0.0f },
                    new GradientAlphaKey { alpha = nearAlpha, time = 1.0f }
                };
                line.colorGradient = gradient;
                line.positionCount = 2;
                line.SetPositions(new Vector3[] { far, near });
            }
            else if (point1.z < Parameters.alpha1NoteRange)
            {
                point1.z += 32.0f; point2.z += 32.0f;
                Vector3 far, near;
                far = Utility.WorldToScreenPoint(point1);
                near = Utility.WorldToScreenPoint(point2);
                Gradient gradient = line.colorGradient;
                gradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey { alpha = 1.0f, time = 0.0f } };
                line.colorGradient = gradient;
                line.positionCount = 2;
                line.SetPositions(new Vector3[] { far, near });
            }
            else
            {
                Vector3 cross;
                float x = point1.x + (point2.x - point1.x) * (point1.z - Parameters.alpha1NoteRange) / (point1.z - point2.z);
                cross = new Vector3(x, 0.0f, Parameters.alpha1NoteRange);
                float alpha = (Parameters.maximumNoteRange - point1.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
                point1.z += 32.0f; cross.z += 32.0f; point2.z += 32.0f;
                Vector3 far, mid, near;
                far = Utility.WorldToScreenPoint(point1);
                mid = Utility.WorldToScreenPoint(cross);
                near = Utility.WorldToScreenPoint(point2);
                float midTime = (far - mid).magnitude / (far - near).magnitude;
                Gradient gradient = line.colorGradient;
                gradient.alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey { alpha = alpha, time = 0.0f },
                    new GradientAlphaKey { alpha = 1.0f, time = midTime },
                    new GradientAlphaKey { alpha = 1.0f, time = 1.0f }
                };
                line.colorGradient = gradient;
                line.positionCount = 3;
                line.SetPositions(new Vector3[] { far, mid, near });
            }
        }
        line.sortingOrder = 0;
    }
}
