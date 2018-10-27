using UnityEngine;

public class TimeGridPerspective : MonoBehaviour
{
    [SerializeField] private PerspectiveLine _line;
    private TimeGridData _data;
    private int _id;
    public int Id
    {
        get { return _id; }
        set
        {
            _id = value;
            _data = ChartDisplayController.Instance.timeGrids[value];
            UpdateColor();
            Update();
        }
    }
    public bool IsShown => ChartDisplayController.Instance.TimeGridShownInPerspectiveView(_data.time);
    private void UpdatePosition()
    {
        float x = 2.0f * Parameters.Params.perspectiveHorizontalScale;
        float z = ChartDisplayController.Instance.PerspectiveTime(_data.time);
        _line.MoveTo(new Vector3(-x, 0.0f, z), new Vector3(x, 0.0f, z));
    }
    private void UpdateColor()
    {
        switch (_data.type)
        {
            case TimeGridData.Type.SubBeat:
                _line.Color = Parameters.Params.subBeatLineColor;
                break;
            case TimeGridData.Type.Beat:
                _line.Color = Parameters.Params.beatLineColor;
                break;
            case TimeGridData.Type.TempoChange:
                _line.Color = Parameters.Params.tempoChangeLineColor;
                break;
            case TimeGridData.Type.FreeTempo:
                _line.Color = Parameters.Params.freeTempoLineColor;
                break;
        }
    }
    public void Update() => UpdatePosition();
}
