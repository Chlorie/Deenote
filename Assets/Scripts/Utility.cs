using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Utility: MonoBehaviour
{
    public const int NONE = 0;
    public const int CTRL = 1;
    public const int ALT = 2;
    public const int SHIFT = 4;
    public static Text debugText;
    public static Camera stageCamera;
    public static GameObject emptyImage;
    public static float stageHeight;
    public static float stageWidth;
    public static Sprite cylinder;
    public static Sprite cylinderAlpha;
    public static Transform cameraUICanvas;
    public static RectTransform xGridParent;
    public static RectTransform linkLineParent;
    public static Collider mouseHitDetector;
    public static void GetInGameNoteIDs(Chart chart, ref List<int> noteIDs)
    {
        int id = -1;
        while (noteIDs.Count > 0) noteIDs.RemoveAt(0);
        for (int i = 0; i < chart.notes.Count; i++)
        {
            Note note = chart.notes[i];
            float pos = note.position;
            if (pos <= 2.0f && pos >= -2.0f) id++;
            noteIDs.Add(id);
        }
    }
    public static bool DetectKeys(KeyCode key, int holdKeys = NONE)
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        return FunctionalKeysHeld(holdKeys) && Input.GetKeyDown(key) && (obj == null || obj.GetComponent<InputField>() == null);
    }
    public static bool HeldKeys(KeyCode key, int holdKeys = NONE)
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        return FunctionalKeysHeld(holdKeys) && Input.GetKey(key) && (obj == null || obj.GetComponent<InputField>() == null);
    }
    public static bool ReleaseKeys(KeyCode key, int holdKeys = NONE)
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        return FunctionalKeysHeld(holdKeys) && Input.GetKeyUp(key) && (obj == null || obj.GetComponent<InputField>() == null);
    }
    public static bool FunctionalKeysHeld(int holdKeys = NONE)
    {
        bool ctrl = false, alt = false, shift = false;
        bool ctrlDown = false, altDown = false, shiftDown = false;
        ctrl = holdKeys % 2 == 1; holdKeys >>= 1;
        alt = holdKeys % 2 == 1; holdKeys >>= 1;
        shift = holdKeys % 2 == 1;
        ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        altDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        return !(ctrl ^ ctrlDown || alt ^ altDown || shift ^ shiftDown);
    }
    public static JSONChart JSONtoJChart(string str)
    {
        JSONChart jchart = new JSONChart();
        /*
         * The json files in the official charts have variables name with "$id"
         * WT* are you using variables named with some strange characters?!!
         * So C# would be messed up with those strange variable names...
         * I'm parsing these values manually...
         */
        string substr;
        int i = 0, temp = 0;
        while (str[i] != '{') i++;
        substr = GetSubStr(str, ref i); //"speed"
        jchart.speed = GetFloat(str, ref i);
        substr = GetSubStr(str, ref i); //"notes"
        while (str[i] != ']')
        {
            substr = GetSubStr(str, ref i); //"$id"
            JSONChart.note note = new JSONChart.note();
            temp = 0;
            note.id = GetInt(GetSubStr(str, ref i), ref temp);
            while (str[i] != '}')
            {
                substr = GetSubStr(str, ref i);
                if (substr == "sounds")
                    while (str[i] != ']')
                    {
                        JSONChart.sound sound = new JSONChart.sound();
                        while (str[i] != '}')
                        {
                            substr = GetSubStr(str, ref i);
                            if (substr == "d")
                                sound.d = GetFloat(str, ref i);
                            else if (substr == "p")
                                sound.p = GetInt(str, ref i);
                            else if (substr == "v")
                                sound.v = GetInt(str, ref i);
                            else if (substr == "w")
                                sound.w = GetFloat(str, ref i);
                        }
                        i++;
                        note.sounds.Add(sound);
                    }
                else if (substr == "pos")
                    note.pos = GetFloat(str, ref i);
                else if (substr == "size")
                    note.size = GetFloat(str, ref i);
                else if (substr == "_time")
                    note.time = GetFloat(str, ref i);
                else if (substr == "shift")
                    note.shift = GetFloat(str, ref i);
                else
                    while (str[i] != ',' && str[i] != '}') i++;
            }
            i++;
            jchart.notes.Add(note);
        }//"notes" complete
        substr = GetSubStr(str, ref i); //"links"
        i += 2;
        while (str[i] != ']')
        {
            JSONChart.link link = new JSONChart.link();
            substr = GetSubStr(str, ref i); //"notes"
            while (str[i] != ']')
            {
                substr = GetSubStr(str, ref i); //"$ref"
                temp = 0;
                link.noteRef.Add(GetInt(GetSubStr(str, ref i), ref temp));
                i++;
            }
            i += 2;
            jchart.links.Add(link);
        }
        return jchart;
    }
    private static float GetFloat(string str, ref int i) //Search from str[i]
    {
        int sign = 1;
        float fnumber = 0.0f;
        float exp10 = 1.0f;
        int inumber = 0;
        int e = 0;
        bool dec = false;
        while (i < str.Length && !IsNumberChar(str[i])) i++;
        while (i < str.Length && IsNumberChar(str[i]))
        {
            if (str[i] == '-') { sign = -1; i++; continue; }
            else if (str[i] == '.') { dec = true; i++; continue; }
            else if (str[i] == 'e' || str[i] == 'E') { e = GetInt(str, ref i); break; }
            if (!dec)
            {
                inumber *= 10;
                inumber += str[i] - '0';
            }
            else
            {
                exp10 /= 10.0f;
                fnumber += (str[i] - '0') * exp10;
            }
            i++;
        }
        return sign * (inumber + fnumber) * Mathf.Pow(10, e);
    }
    private static int GetInt(string str, ref int i)
    {
        int sign = 1;
        int inumber = 0;
        while (i < str.Length && (!IsNumberChar(str[i]) || str[i] == 'e' || str[i] == 'E')) i++;
        if (i == str.Length) return -2147483648;
        while (i < str.Length && IsNumberChar(str[i]))
        {
            if (str[i] == '-') { sign = -1; i++; continue; }
            inumber *= 10;
            inumber += str[i] - '0';
            i++;
        }
        return sign * inumber;
    }
    public static float GetFloat(string str)
    {
        int temp = 0;
        return GetFloat(str, ref temp);
    }
    public static int GetInt(string str)
    {
        int temp = 0;
        return GetInt(str, ref temp);
    }
    private static bool IsNumberChar(char c)
    {
        if (c >= '0' && c <= '9') return true;
        if (c == '-') return true;
        if (c == '.') return true;
        if (c == 'e' || c == 'E') return true;
        return false;
    }
    private static string GetSubStr(string str, ref int i)
    {
        string result = "";
        int j = i;
        while (str[i] != '"') i++;
        j = i + 1;
        while (str[j] != '"') j++;
        result = str.Substring(i + 1, j - i - 1);
        i = j + 1;
        return result;
    }
    public static Chart JCharttoChart(JSONChart jchart)
    {
        Chart chart = new Chart();
        chart.speed = jchart.speed;
        while (chart.notes.Count > 0) chart.notes.RemoveAt(0);
        foreach (JSONChart.note jnote in jchart.notes)
        {
            Note note = new Note();
            note.position = jnote.pos;
            note.size = jnote.size;
            note.time = jnote.time;
            note.shift = jnote.shift;
            foreach (JSONChart.sound jsound in jnote.sounds)
            {
                PianoSound sound = new PianoSound();
                sound.delay = jsound.w;
                sound.duration = jsound.d;
                sound.pitch = jsound.p;
                sound.volume = jsound.v;
                note.sounds.Add(sound);
            }
            chart.notes.Add(note);
        }
        foreach (JSONChart.link jlink in jchart.links)
        {
            int prev = -1;
            foreach (int noteref in jlink.noteRef)
            {
                if (prev != -1) chart.notes[prev].nextLink = noteref - 1; //Note number in Chart starts by 0
                chart.notes[noteref - 1].isLink = true;
                chart.notes[noteref - 1].prevLink = prev;
                prev = noteref - 1;
            }
            if (prev != -1) chart.notes[prev].nextLink = -1;
        }
        return chart;
    }
    public static JSONChart CytusChartToJChart(string[] cytusChart)
    {
        JSONChart jchart = new JSONChart();
        int i, n = cytusChart.Length, j, ivalue;
        float fvalue;
        jchart.speed = 5.0f;
        for (i = 4; i < n; i++)
        {
            j = 0;
            if (cytusChart[i][0] == 'N')
            {
                ivalue = GetInt(cytusChart[i], ref j);
                JSONChart.note note = new JSONChart.note();
                note.id = ivalue - 3;
                fvalue = GetFloat(cytusChart[i], ref j);
                //note.time = fvalue - 0.08f;
                note.time = fvalue;
                fvalue = GetFloat(cytusChart[i], ref j);
                note.pos = fvalue * 4 - 2;
                jchart.notes.Add(note);
            }
            else
            {
                JSONChart.link link = new JSONChart.link();
                while (j < cytusChart[i].Length)
                {
                    ivalue = GetInt(cytusChart[i], ref j);
                    if (ivalue != -2147483648)
                    {
                        link.noteRef.Add(ivalue + 1);
                        jchart.notes[ivalue].size = 0.8f;
                    }
                }
                jchart.links.Add(link);
            }
        }
        return jchart;
    }
    public static void WriteCharttoJSON(Chart chart, FileStream fs)
    {
        //Write the file under Deemo 3.0 standard
        StreamWriter sw = new StreamWriter(fs);
        sw.Write("{\"speed\":" + chart.speed + ",\"notes\":[");
        for (int i = 0; i < chart.notes.Count; i++)
        {
            if (i > 0) sw.Write(",");
            sw.Write("{\"$id\":\"" + (i + 1) + "\",\"type\":0");
            List<PianoSound> sounds = chart.notes[i].sounds;
            if (sounds.Count > 0)
            {
                sw.Write(",\"sounds\":[");
                for (int j = 0; j < sounds.Count; j++)
                {
                    if (j > 0) sw.Write(",");
                    sw.Write("{");
                    sw.Write("\"w\":" + sounds[j].delay);
                    sw.Write(",\"d\":" + sounds[j].duration);
                    sw.Write(",\"p\":" + sounds[j].pitch);
                    sw.Write(",\"v\":" + sounds[j].volume);
                    sw.Write("}");
                }
                sw.Write("]");
            }
            sw.Write(",\"pos\":" + chart.notes[i].position);
            sw.Write(",\"size\":" + chart.notes[i].size);
            sw.Write(",\"_time\":" + chart.notes[i].time);
            sw.Write(",\"shift\":" + chart.notes[i].shift);
            sw.Write(",\"time\":" + chart.notes[i].time);
            sw.Write("}");
        }
        sw.Write("],\"links\":[");
        int k = 0; bool flag = true;
        while (k < chart.notes.Count)
        {
            while (k < chart.notes.Count && !(chart.notes[k].isLink && chart.notes[k].prevLink == -1)) k++;
            if (k == chart.notes.Count) break;
            int next = chart.notes[k].nextLink;
            if (!flag) sw.Write(",");
            flag = false;
            sw.Write("{\"notes\":[{\"$ref\":\"" + (k + 1) + "\"}");
            while (next != -1)
            {
                sw.Write(",{\"$ref\":\"" + (next + 1) + "\"}");
                next = chart.notes[next].nextLink;
            }
            sw.Write("]}");
            k++;
        }
        sw.Write("]}");
        sw.Flush();
        sw.Close();
    }
    public static Chart CopyChart(Chart chart)
    {
        Chart copy = new Chart();
        foreach (float time in chart.beats) copy.beats.Add(time);
        copy.difficulty = chart.difficulty;
        copy.level = chart.level;
        foreach (Note note in chart.notes)
        {
            Note newNote = new Note();
            newNote.position = note.position;
            newNote.isLink = note.isLink;
            newNote.nextLink = note.nextLink;
            newNote.prevLink = note.prevLink;
            newNote.shift = note.shift;
            newNote.size = note.size;
            foreach(PianoSound sound in note.sounds)
            {
                PianoSound newSound = new PianoSound();
                newSound.delay = sound.delay;
                newSound.duration = sound.duration;
                newSound.pitch = sound.pitch;
                newSound.volume = sound.volume;
                newNote.sounds.Add(newSound);
            }
            newNote.time = note.time;
            copy.notes.Add(newNote);
        }
        copy.speed = chart.speed;
        return copy;
    }
    public static UILine DrawLineInWorldSpace(Vector3 point1, Vector3 point2, Color color, Sprite sprite, int width)
    {
        Vector3 lPoint1 = stageCamera.WorldToScreenPoint(point1);
        Vector3 lPoint2 = stageCamera.WorldToScreenPoint(point2);
        UILine newLine = new UILine(lPoint1, lPoint2, width, color, sprite);
        return newLine;
    }
    public static void MoveLineInWorldSpace(UILine source, Vector3 point1, Vector3 point2, Color color, Sprite sprite = null, int width = -1)
    {
        Sprite spr = sprite;
        MoveLineInWorldSpace(source, point1, point2, width);
        source.image.color = color;
        if (spr == null) spr = source.image.sprite;
        source.image.sprite = spr;
    }
    public static void MoveLineInWorldSpace(UILine source, Vector3 point1, Vector3 point2, int width = -1)
    {
        Vector3 lPoint1 = stageCamera.WorldToScreenPoint(point1);
        Vector3 lPoint2 = stageCamera.WorldToScreenPoint(point2);
        source.MoveTo(lPoint1, lPoint2, width);
    }
    public static Vector3 GetMouseWorldPos()
    {
        Ray ray = stageCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
            return hit.point;
        else
            return new Vector3(0, -10, 0);
    }
    public static void DebugText(string text)
    {
        debugText.text = text;
    }
    public static void PlayerPrefsSetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
    public static bool PlayerPrefsGetBool(string key, bool defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }
}
