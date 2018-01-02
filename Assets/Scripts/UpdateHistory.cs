using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateHistory : MonoBehaviour
{
    public GameObject panel;
    public Text text;
    public Text title;
    public Text checkUpdateText;
    public VersionChecker versionChecker;
    private List<string> versions = new List<string>
    {
        "Deenote 0.6",
        "Deenote 0.5.10",
        "Deenote 0.5.9",
        "Deenote 0.5.8",
        "Deenote 0.5.7",
        "Deenote 0.5.6",
        "Deenote 0.5.5",
        "Deenote 0.5.4",
        "Deenote 0.5.3",
        "Deenote 0.5.2",
        "Deenote 0.5.1",
        "Deenote 0.5",
        "Deenote 0.4",
        "Deenote 0.3.1",
        "Deenote 0.3 build 2",
        "Deenote 0.3 build 1",
        "Deenote 0.2.1",
        "Deenote 0.2",
        "Deenote 0.1",
        "Deemo Chart Editor 0.2",
        "Deemo Chart Editor 0.1"
    };
    private List<string> updateInfo = new List<string>
    {
        @"Added curve forming function.",
        @"Now saving and loading files won't block the main thread.
Fixed the serious bug about music playback repositioning.",
        @"Separated update history from about.
Added update checker.
Minor bug fixes about UI.",
        @"Now you can change the music file used in the project.
When creating a new file in the file selector, files with the target extension will appear.",
        @"Bug fix: File cannot be opened.
Bug fix: Link lines cannot be toggled off.
Deleted Schwarzer’s famous words because I don’t want to die. XD",
        @"Added drag-and-drop file opener. (Thanks to Schwarzer!)",
        @"Added support for mp3 music files.",
        @"Completely reworked on the code of line displaying.",
        @"Added toggle for VSync.
Now editor settings are saved as well.",
        @"Changed default volume of piano sounds from 127 to 0.
Now you can import ogg music files.",
        @"Fixed the bug when deleting slide notes the remaining slide notes are incorrectly linked.",
        @"Copy/Paste functions.
Quantize notes.",
        @"Full edit function of note properties.",
        @"File extension association.",
        @"Minor changes to beat line saving.
Edit panel UI redesigned.",
        @"Bug fixes.",
        @"Add new notes.
Link/Unlink selected notes.
Note placement indicator.",
        @"Added color tint for selected notes.
Remove notes.
Beat line filling field auto-fill.",
        @"Added all visual effects for chart viewing function.
Added a manual BPM calculator.
Volume control for the sounds.
Beat line filling and displaying.
Link lines between slide notes.
Undo/Redo functions.
Fixed a few bugs.
Select notes.
(Hidden feature: Convert Cytus v2 charts into Deemo charts. In my opinion no one would like to use this or even care about this.)",
        @"Added some shortcuts.
JSON file exporting.
""Save as"" feature.
Added some visual effects (Lowered alpha values for the notes that are far away, added frame-by-frame disappearing animation and shock wave/circle animation for notes that hit the judge line).
Fixed a few bugs.",
        @"Chart viewing function finished. No effects yet."
    };
    private int current = 0;
    private void UpdateContent()
    {
        text.text = updateInfo[current];
        title.text = "Update History - " + versions[current] + (current == 0 ? " (Current)" : "");
    }
    public void Activate()
    {
        panel.SetActive(true);
        CurrentState.ignoreAllInput = true;
        current = 0;
        UpdateContent();
    }
    public void Deactivate()
    {
        panel.SetActive(false);
        CurrentState.ignoreAllInput = false;
    }
    public void CheckUpdate()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            checkUpdateText.text = "No Internet connection";
            return;
        }
        versionChecker.CheckForUpdate();
    }
    public void PrevVersion()
    {
        if (current == versions.Count - 1) return;
        current++;
        UpdateContent();
    }
    public void NextVersion()
    {
        if (current == 0) return;
        current--;
        UpdateContent();
    }
}
