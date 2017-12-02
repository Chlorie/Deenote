using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditPanelController : MonoBehaviour
{
    public RectTransform editPanel;
    public RectTransform selection;
    public RectTransform playerSettings;
    public RectTransform basicCommands;
    public RectTransform noteAndGrid;
    public RectTransform beatLine;
    public Text selectionButtonText;
    public Text playerSettingsButtonText;
    public Text basicCommandsButtonText;
    public Text noteAndGridButtonText;
    public Text beatLineButtonText;
    public void ToggleSelectionPanel()
    {
        ExpandCollapse(selection, selectionButtonText);
    }
    public void TogglePlayerSettingsPanel()
    {
        ExpandCollapse(playerSettings, playerSettingsButtonText);
    }
    public void ToggleBasicCommandsPanel()
    {
        ExpandCollapse(basicCommands, basicCommandsButtonText);
    }
    public void ToggleNoteAndGridPanel()
    {
        ExpandCollapse(noteAndGrid, noteAndGridButtonText);
    }
    public void ToggleBeatLinePanel()
    {
        ExpandCollapse(beatLine, beatLineButtonText);
    }
    private void ExpandCollapse(RectTransform tabTransform, Text buttonText)
    {
        bool expanded = tabTransform.gameObject.activeSelf;
        expanded = !expanded;
        tabTransform.gameObject.SetActive(expanded);
        buttonText.text = expanded ? "Collapse" : "Expand";
        float size = tabTransform.sizeDelta.y;
        Vector2 sizeDelta = editPanel.sizeDelta;
        sizeDelta.y += size * (expanded ? 1 : -1);
        editPanel.sizeDelta = sizeDelta;
    }
}
