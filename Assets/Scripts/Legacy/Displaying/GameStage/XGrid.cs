using UnityEngine;

public class XGrid
{
    public Line line;
    public XGrid()
    {
        line = Utility.DrawLineInWorldSpace
        (
            // 32:notepanel的偏移
            new Vector3(0.0f, 0.0f, 32.0f + Parameters.maximumNoteRange),
            new Vector3(0.0f, 0.0f, 32.0f),
            new Color(42 / 255.0f, 42 / 255.0f, 42 / 255.0f, 0.75f),
            0.035f,
            0.75f
        );
        line.transform.SetParent(Utility.xGridParent);
        SetActive(false);
    }
    public void MoveTo(float x)
    {
        line.MoveTo(new Vector3(x, 0, 32 + Parameters.maximumNoteRange), new Vector3(x, 0, 32));
        line.AlphaMultiplier = 0.75f;
    }
    public void SetActive(bool status)
    {
        line.SetActive(status);
    }
}
