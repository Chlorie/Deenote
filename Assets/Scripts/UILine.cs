using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILine
{
    public GameObject gameObject;
    public RectTransform rectTransform;
    private int width;
    public Image image;
    public UILine(Vector3 point1, Vector3 point2, int wid, Color col, Sprite spr)
    {
        Vector2 p1 = new Vector2(point1.x, point1.y), p2 = new Vector2(point2.x, point2.y);
        gameObject = Object.Instantiate(Utility.emptyImage, Utility.cameraUICanvas);
        rectTransform = gameObject.GetComponent<RectTransform>();
        image = gameObject.GetComponent<Image>();
        image.sprite = spr;
        image.color = col;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Vertical;
        image.fillOrigin = (int)Image.OriginVertical.Bottom;
        rectTransform.anchoredPosition3D = Vector3.zero;
        rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
        rectTransform.anchorMin = Vector2.zero;
        MoveTo(p1, p2, wid);
    }
    public void MoveTo(Vector2 point1, Vector2 point2, int wid = -1)
    {
        Vector2 pivotPoint = (point1 + point2) / 2.0f, diff = point1 - point2;
        float height, angle;
        Vector2 bottomLeftCorner, topRightCorner;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        height = diff.magnitude;
        if (wid == -1) wid = width;
        bottomLeftCorner = new Vector2(pivotPoint.x - wid / 2.0f, pivotPoint.y - height / 2.0f);
        topRightCorner = bottomLeftCorner - new Vector2(Utility.stageWidth - wid, Utility.stageHeight - height);
        width = wid;
        if (height > 1.0f)
            angle = ((diff.x > 0) ? -1.0f : 1.0f) * Mathf.Acos(diff.y / height) * Mathf.Rad2Deg;
        else
            angle = 0.0f;
        rectTransform.offsetMax = topRightCorner;
        rectTransform.offsetMin = bottomLeftCorner;
        rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, angle);
    }
    public void SetActive(bool status)
    {
        gameObject.SetActive(status);
    }
    public void DestroyLine()
    {
        Object.Destroy(gameObject);
    }
}