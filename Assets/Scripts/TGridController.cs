using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGridController: MonoBehaviour
{
    public TGridID id;
    private float time;
    private float curTime;
    public StageController stage;
    public Line grid;
    private void CheckForReturn()
    {
        if (time > curTime)
        {
            grid.SetActive(false);
            stage.SetPrevLineID(id);
            stage.ReturnLine(this);
        }
    }
    public void ForceReturn()
    {
        grid.SetActive(false);
        stage.SetPrevLineID(id);
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
    public void Activate(TGridID lineID, float lineTime, StageController stageController)
    {
        id = lineID;
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

public class TGridID
{
    public int id;
    public int sub;
    public int maxSub;
    public TGridID(int newId, int newSub, int newMaxSub)
    {
        id = newId;
        sub = newSub;
        maxSub = newMaxSub;
    }
    public static TGridID operator ++(TGridID cur)
    {
        TGridID res = new TGridID(cur.id, cur.sub + 1, cur.maxSub);
        if (res.sub >= res.maxSub) { res.id++; res.sub -= res.maxSub; }
        return res;
    }
    public static TGridID operator --(TGridID cur)
    {
        TGridID res = new TGridID(cur.id, cur.sub - 1, cur.maxSub);
        if (res.sub < 0) { res.id--; res.sub += res.maxSub; }
        return res;
    }
    public static bool operator <(TGridID a, TGridID b)
    {
        if (a.id < b.id) return true;
        if (a.id > b.id) return false;
        return a.sub < b.sub;
    }
    public static bool operator >(TGridID a, TGridID b)
    {
        if (a.id > b.id) return true;
        if (a.id < b.id) return false;
        return a.sub > b.sub;
    }
    public static bool operator <=(TGridID a, TGridID b)
    {
        return !(a > b);
    }
    public static bool operator >=(TGridID a, TGridID b)
    {
        return !(a < b);
    }
    public static implicit operator TGridID(int id)
    {
        return new TGridID(id, 0, 0);
    }
}