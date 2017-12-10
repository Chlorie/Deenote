using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkLine
{
    private UILine near;
    private UILine far;
    public LinkLine()
    {
        near = new UILine(Vector3.zero, Vector3.zero, 3, Parameters.linkLineColor, Utility.cylinder);
        far = new UILine(Vector3.zero, Vector3.zero, 3, Parameters.linkLineColor, Utility.cylinderAlpha);
        near.rectTransform.SetParent(Utility.linkLineParent);
        far.rectTransform.SetParent(Utility.linkLineParent);
    }
    public void MoveTo(Vector3 point1, Vector3 point2)
    {
        near.image.color = Parameters.linkLineColor;
        Color finalColor = Parameters.linkLineColor;
        float slope, intercept;
        float farMinX, farMaxX, nearMinX, nearMaxX;
        float farMinZ, farMaxZ, nearMinZ, nearMaxZ;
        float alpha, farFillAmount, minAlpha;
        bool nearActive = false, farActive = false;
        if (point1.z - point2.z <= (Parameters.maximumNoteRange - Parameters.alpha1NoteRange) * Parameters.minAlphaDif)
        {
            if (point2.z < Parameters.alpha1NoteRange) alpha = 1.0f;
            else alpha = (Parameters.maximumNoteRange - point2.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
            far.SetActive(false);
            near.SetActive(true);
            point1.z += 32.0f; point2.z += 32.0f;
            finalColor.a = alpha;
            Utility.MoveLineInWorldSpace(near, point1, point2, finalColor);
            return;
        }
        slope = (point1.x - point2.x) / (point1.z - point2.z);
        intercept = (point2.x * point1.z - point1.x * point2.z) / (point1.z - point2.z);
        if (point1.z >= Parameters.maximumNoteRange)
        {
            farMaxZ = Parameters.maximumNoteRange;
            farMaxX = slope * farMaxZ + intercept;
            nearMaxZ = Parameters.alpha1NoteRange;
            nearMaxX = slope * nearMaxZ + intercept;
            minAlpha = 0.0f;
            farActive = true;
        }
        else if (point1.z >= Parameters.alpha1NoteRange)
        {
            farMaxZ = point1.z;
            farMaxX = point1.x;
            nearMaxZ = Parameters.alpha1NoteRange;
            nearMaxX = slope * nearMaxZ + intercept;
            minAlpha = (Parameters.maximumNoteRange - point1.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
            farActive = true;
        }
        else
        {
            farMaxZ = Parameters.maximumNoteRange;
            farMaxX = slope * farMaxZ + intercept;
            nearMaxZ = point1.z;
            nearMaxX = point1.x;
            minAlpha = 1.0f;
            farActive = false;
        }
        if (point2.z >= Parameters.alpha1NoteRange)
        {
            nearMinZ = point2.z;
            nearMinX = point2.x;
            farMinZ = point2.z;
            farMinX = point2.x;
            alpha = (Parameters.maximumNoteRange - point2.z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
            nearActive = false;
        }
        else
        {
            nearMinZ = point2.z;
            nearMinX = point2.x;
            farMinZ = Parameters.alpha1NoteRange;
            farMinX = slope * farMinZ + intercept;
            alpha = 1.0f;
            nearActive = true;
        }
        far.SetActive(farActive);
        near.SetActive(nearActive);
        if (nearActive)
            Utility.MoveLineInWorldSpace(near, new Vector3(nearMaxX, 0, nearMaxZ + 32), new Vector3(nearMinX, 0, nearMinZ + 32), Parameters.linkLineColor);
        if (farActive)
        {
            finalColor.a = alpha;
            farFillAmount = 1 - minAlpha / alpha;
            if (farFillAmount <= Parameters.minAlphaDif)
            {
                Utility.MoveLineInWorldSpace(far, new Vector3(farMaxX, 0, farMaxZ + 32), new Vector3(farMinX, 0, farMinZ + 32), finalColor);
                far.image.fillAmount = 1.0f;
            }
            else
            {
                Vector3 p1 = Utility.stageCamera.WorldToScreenPoint(new Vector3(farMaxX, 0, farMaxZ + 32));
                Vector3 p2 = Utility.stageCamera.WorldToScreenPoint(new Vector3(farMinX, 0, farMinZ + 32));
                p1 = (p1 - p2) / farFillAmount + p2;
                far.MoveTo(p1, p2);
                far.image.color = finalColor;
                far.image.fillAmount = farFillAmount;
            }
        }
    }
    public void AlphaMultiply(float multiplier)
    {
        Color nearColor = near.image.color, farColor = far.image.color;
        nearColor.a *= multiplier; farColor.a *= multiplier;
        near.image.color = nearColor; far.image.color = farColor;
    }
    public void SetActive(bool status)
    {
        near.SetActive(status);
        far.SetActive(status);
    }
}
