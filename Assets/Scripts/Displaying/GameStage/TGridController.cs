using UnityEngine;

public class TGridController : MonoBehaviour
{
    public TGridId id;
    private float time;
    private float curTime;
    public StageController stage;
    public Line grid;
    private void CheckForReturn()
    {
        if (time <= curTime) return;
        grid.SetActive(false);
        stage.SetPrevLineId(id);
        stage.ReturnLine(this);
    }
    public void ForceReturn()
    {
        grid.SetActive(false);
        stage.SetPrevLineId(id);
        stage.ReturnLine(this);
    }
    private void PositionUpdate()
    {
        float z = Parameters.maximumNoteRange / Parameters.NoteFallTime(stage.chartPlaySpeed) * (curTime - time);
        grid.MoveTo
        (
            new Vector3(-Parameters.maximumNoteWidth * 2, 0, z + 32),
            new Vector3(Parameters.maximumNoteWidth * 2, 0, z + 32)
        );
        if (id.sub == 0)
        {
            grid.Color = new Color(0.5f, 0.0f, 0.0f);
            grid.AlphaMultiplier = 1.0f;
        }
        else
        {
            grid.Color = new Color(42 / 255.0f, 42 / 255.0f, 42 / 255.0f);
            grid.AlphaMultiplier = 0.75f;
        }
    }
    public void Activate(TGridId lineId, float lineTime, StageController stageController)
    {
        id = lineId;
        curTime = lineTime;
        stage = stageController;
        if (id.sub == 0)
        {
            grid.Color = new Color(0.5f, 0.0f, 0.0f);
            grid.AlphaMultiplier = 1.0f;
        }
        else
        {
            grid.Color = new Color(42 / 255.0f, 42 / 255.0f, 42 / 255.0f);
            grid.AlphaMultiplier = 0.75f;
        }
        Update();
    }
    private void Update()
    {
        time = stage.timeSlider.value;
        CheckForReturn();
        PositionUpdate();
    }
}

public class TGridId
{
    public int id;
    public int sub;
    public int maxSub;
    public TGridId(int newId, int newSub, int newMaxSub)
    {
        id = newId;
        sub = newSub;
        maxSub = newMaxSub;
    }
    public static TGridId operator ++(TGridId cur)
    {
        TGridId res = new TGridId(cur.id, cur.sub + 1, cur.maxSub);
        if (res.sub >= res.maxSub) { res.id++; res.sub -= res.maxSub; }
        return res;
    }
    public static TGridId operator --(TGridId cur)
    {
        TGridId res = new TGridId(cur.id, cur.sub - 1, cur.maxSub);
        if (res.sub < 0) { res.id--; res.sub += res.maxSub; }
        return res;
    }
    public static bool operator <(TGridId a, TGridId b)
    {
        if (a.id < b.id) return true;
        if (a.id > b.id) return false;
        return a.sub < b.sub;
    }
    public static bool operator >(TGridId a, TGridId b)
    {
        if (a.id > b.id) return true;
        if (a.id < b.id) return false;
        return a.sub > b.sub;
    }
    public static bool operator <=(TGridId a, TGridId b) => !(a > b);
    public static bool operator >=(TGridId a, TGridId b) => !(a < b);
    public static implicit operator TGridId(int id) => new TGridId(id, 0, 0);
}