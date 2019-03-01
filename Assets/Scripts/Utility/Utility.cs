using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Utility
{
    public const int NONE = 0;
    public const int CTRL = 1;
    public const int ALT = 2;
    public const int SHIFT = 4;
    public static Text debugText;
    public static Camera stageCamera;
    public static GameObject emptyImage;
    public static float stageHeight = 720.0f;
    public static float stageWidth = 960.0f;
    public static Transform cameraUICanvas;
    public static RectTransform xGridParent;
    public static RectTransform linkLineParent;
    public static LinePool linePool;
    public static Transform lineCanvas;
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
        bool ctrl, alt, shift;
        bool ctrlDown, altDown, shiftDown;
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
        int i = 0, temp;
        while (str[i] != '{') i++;
        GetSubStr(str, ref i); //"speed"
        jchart.speed = GetFloat(str, ref i);
        GetSubStr(str, ref i); //"notes"
        while (str[i] != ']')
        {
            GetSubStr(str, ref i); //"$id"
            JSONChart.note note = new JSONChart.note();
            temp = 0;
            note.id = GetInt(GetSubStr(str, ref i), ref temp);
            while (str[i] != '}')
            {
                string substr = GetSubStr(str, ref i);
                switch (substr)
                {
                    case "sounds":
                    {
                        while (str[i] != ']')
                        {
                            JSONChart.sound sound = new JSONChart.sound();
                            while (str[i] != '}')
                            {
                                substr = GetSubStr(str, ref i);
                                switch (substr)
                                {
                                    case "d":
                                        sound.d = GetFloat(str, ref i);
                                        break;
                                    case "p":
                                        sound.p = GetInt(str, ref i);
                                        break;
                                    case "v":
                                        sound.v = GetInt(str, ref i);
                                        break;
                                    case "w":
                                        sound.w = GetFloat(str, ref i);
                                        break;
                                }
                            }
                            i++;
                            note.sounds.Add(sound);
                        }
                        break;
                    }
                    case "pos":
                        note.pos = GetFloat(str, ref i);
                        break;
                    case "size":
                        note.size = GetFloat(str, ref i);
                        break;
                    case "_time":
                        note.time = GetFloat(str, ref i);
                        break;
                    case "shift":
                        note.shift = GetFloat(str, ref i);
                        break;
                    default:
                    {
                        while (str[i] != ',' && str[i] != '}') i++;
                        break;
                    }
                }
            }
            i++;
            jchart.notes.Add(note);
        } // "notes" complete
        GetSubStr(str, ref i); // "links"
        i += 2;
        while (str[i] != ']')
        {
            JSONChart.link link = new JSONChart.link();
            GetSubStr(str, ref i); // "notes"
            while (str[i] != ']')
            {
                GetSubStr(str, ref i); // "$ref"
                temp = 0;
                link.noteRef.Add(GetInt(GetSubStr(str, ref i), ref temp));
                i++;
            }
            i += 2;
            jchart.links.Add(link);
        }
        return jchart;
    }
    private static float GetFloat(string str, ref int i) // Search from str[i]
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
            if (str[i] == '.') { dec = true; i++; continue; }
            if (str[i] == 'e' || str[i] == 'E') { e = GetInt(str, ref i); break; }
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
        while (i < str.Length && IsNumberChar(str[i]) && str[i] != '.')
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
        while (str[i] != '"') i++;
        int j = i + 1;
        while (str[j] != '"') j++;
        string result = str.Substring(i + 1, j - i - 1);
        i = j + 1;
        return result;
    }
    public static Chart JCharttoChart(JSONChart jchart)
    {
        Chart chart = new Chart
        {
            speed = jchart.speed
        };
        while (chart.notes.Count > 0) chart.notes.RemoveAt(0);
        foreach (JSONChart.note jnote in jchart.notes)
        {
            Note note = new Note
            {
                position = jnote.pos,
                size = jnote.size,
                time = jnote.time,
                shift = jnote.shift
            };
            foreach (JSONChart.sound jsound in jnote.sounds)
            {
                PianoSound sound = new PianoSound
                {
                    delay = jsound.w,
                    duration = jsound.d,
                    pitch = jsound.p,
                    volume = jsound.v
                };
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
        int n = cytusChart.Length;
        jchart.speed = 5.0f;
        for (int i = 4; i < n; i++)
        {
            int j = 0;
            int ivalue;
            if (cytusChart[i][0] == 'N')
            {
                ivalue = GetInt(cytusChart[i], ref j);
                JSONChart.note note = new JSONChart.note
                {
                    id = ivalue - 3
                };
                float fvalue = GetFloat(cytusChart[i], ref j);
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
                    if (ivalue == -2147483648) continue;
                    link.noteRef.Add(ivalue + 1);
                    jchart.notes[ivalue].size = 0.8f;
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
            Note newNote = new Note
            {
                position = note.position,
                isLink = note.isLink,
                nextLink = note.nextLink,
                prevLink = note.prevLink,
                shift = note.shift,
                size = note.size
            };
            foreach (PianoSound sound in note.sounds)
            {
                PianoSound newSound = new PianoSound
                {
                    delay = sound.delay,
                    duration = sound.duration,
                    pitch = sound.pitch,
                    volume = sound.volume
                };
                newNote.sounds.Add(newSound);
            }
            newNote.time = note.time;
            copy.notes.Add(newNote);
        }
        copy.speed = chart.speed;
        return copy;
    }
    public static Line DrawLineInWorldSpace(Vector3 point1, Vector3 point2, Color color, float width, float alpha = 1.0f)
    {
        Line line = linePool.GetObject();
        line.Width = width;
        line.MoveTo(point1, point2);
        line.Color = color;
        line.AlphaMultiplier = alpha;
        line.SetActive(true);
        return line;
    }
    public static Vector3 GetMouseWorldPos()
    {
        Ray ray = stageCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit) ? hit.point : new Vector3(0, -10, 0);
    }
    public static Vector3 WorldToScreenPoint(Vector3 worldPos) // Result in 1280x720 resolution (same to the canvas)
    {
        Vector3 res = stageCamera.WorldToScreenPoint(worldPos);
        res *= (720.0f / stageHeight);
        res.z = 0.0f;
        return res;
    }
    public static void DebugText(string text) => debugText.text = text;
    public static void PlayerPrefsSetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);
    public static bool PlayerPrefsGetBool(string key, bool defaultValue) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
}
