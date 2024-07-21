using UnityEngine;

public class EditPanelController : MonoBehaviour
{
    public RectTransform editPanel;
    public RectTransform selection;
    public RectTransform playerSettings;
    public RectTransform basicCommands;
    public RectTransform noteAndGrid;
    public RectTransform curveForming;
    public RectTransform beatLine;
    public RectTransform concatenate;
    public LocalizedText_Legacy selectionButtonText;
    public LocalizedText_Legacy playerSettingsButtonText;
    public LocalizedText_Legacy basicCommandsButtonText;
    public LocalizedText_Legacy noteAndGridButtonText;
    public LocalizedText_Legacy curveFormingText;
    public LocalizedText_Legacy beatLineButtonText;
    public LocalizedText_Legacy concatenateText;
    public void ToggleSelectionPanel() => ExpandCollapse(selection, selectionButtonText);
    public void TogglePlayerSettingsPanel() => ExpandCollapse(playerSettings, playerSettingsButtonText);
    public void ToggleBasicCommandsPanel() => ExpandCollapse(basicCommands, basicCommandsButtonText);
    public void ToggleNoteAndGridPanel() => ExpandCollapse(noteAndGrid, noteAndGridButtonText);
    public void ToggleCurveFormingPanel() => ExpandCollapse(curveForming, curveFormingText);
    public void ToggleBeatLinePanel() => ExpandCollapse(beatLine, beatLineButtonText);
    public void ToggleConcatenatePanel() => ExpandCollapse(concatenate, concatenateText);
    private void ExpandCollapse(RectTransform tabTransform, LocalizedText_Legacy buttonText)
    {
        bool expanded = tabTransform.gameObject.activeSelf;
        expanded = !expanded;
        tabTransform.gameObject.SetActive(expanded);
        buttonText.SetStrings(expanded ? "Collapse" : "Expand", expanded ? "收起" : "展开");
        float size = tabTransform.sizeDelta.y;
        Vector2 sizeDelta = editPanel.sizeDelta;
        sizeDelta.y += size * (expanded ? 1 : -1);
        editPanel.sizeDelta = sizeDelta;
    }
}
