#nullable enable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace Deenote.Runtime.Plugins.ChartConversion.Rpe
{
    public class Info
    {
        public string Name { get; set; } = "UK";
        public float Difficulty { get; set; } = 10.0f;
        public string Level { get; set; } = "UK Lv.10";
        public string Charter { get; set; } = "UK";
        public string Composer { get; set; } = "UK";
        public string Illustrator { get; set; } = "UK";
        public string Chart { get; set; } = "chart.json";
        public string Music { get; set; } = "song.mp3";
        public string Illustration { get; set; } = "background.png";

        public string ToYamlString()
        {
            StringBuilder yaml = new();
            yaml.AppendLine($"name: {EscapeYamlValue(Name)}");
            yaml.AppendLine($"difficulty: {Difficulty:F1}");
            yaml.AppendLine($"level: {EscapeYamlValue(Level)}");
            yaml.AppendLine($"charter: {EscapeYamlValue(Charter)}");
            yaml.AppendLine($"composer: {EscapeYamlValue(Composer)}");
            yaml.AppendLine($"illustrator: {EscapeYamlValue(Illustrator)}");
            yaml.AppendLine($"chart: {EscapeYamlValue(Chart)}");
            yaml.AppendLine($"music: {EscapeYamlValue(Music)}");
            yaml.AppendLine($"illustration: {EscapeYamlValue(Illustration)}");
            return yaml.ToString();
        }

        private static string EscapeYamlValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";
            if (value.Contains(":") || value.Contains("#") || value.Contains("'") || value.Contains("\"")) {
                return $"\"{value.Replace("\"", "\\\"")}\"";
            }

            return value;
        }

        public void FillMeta(META meta)
        {
            meta.name = this.Name;
            meta.charter = this.Charter;
            meta.composer = this.Composer;
            meta.level = this.Level;
            meta.illustration = this.Illustrator;
            meta.song = this.Music;
            meta.background = this.Illustration;
        }
    }

    public class Note
    {
        public int above { get; set; } = 1;
        public int alpha { get; set; } = 255;
        public Beat endTime { get; set; }
        public Beat startTime { get; set; }
        public int isFake { get; set; } = 0;
        public float positionX { get; set; }
        public float size { get; set; } = 1.0f;
        public float speed { get; set; } = 1.0f;
        public int type { get; set; }
        public float visibleTime { get; set; } = 999999f;
        public float yOffset { get; set; } = 0;
        public float judgeArea { get; set; } = 1.0f;

        public Note(int type, float positionX, float size, Beat startTime, Beat? endTime = null)
        {
            this.type = type;
            this.positionX = positionX;
            this.size = size;
            this.judgeArea = size;
            this.startTime = startTime;
            this.endTime = endTime ?? startTime;
        }
    }

    public class EventLayer
    {
        public List<Event> moveXEvents { get; set; } = new();
        public List<Event> moveYEvents { get; set; } = new();
        public List<Event> rotateEvents { get; set; } = new();
        public List<Event> alphaEvents { get; set; } = new();
        public List<Event> speedEvents { get; set; } = new();

        public static EventLayer Default(float floorPosition, float speed)
        {
            var layer = new EventLayer();
            layer.moveYEvents.Add(Event.Default(floorPosition));
            layer.alphaEvents.Add(Event.Default(255));
            layer.speedEvents.Add(Event.Default(speed));
            return layer;
        }
    }

    public class Event
    {
        public float easingLeft { get; set; } = 0.0f;
        public float easingRight { get; set; } = 1.0f;
        public int easingType { get; set; } = 1;
        public int linkgroup { get; set; } = 0;
        public float start { get; set; }
        public Beat startTime { get; set; }
        public float end { get; set; }
        public Beat endTime { get; set; }

        public static Event Default(float value)
        {
            return new() { startTime = new(), endTime = new(), start = value, end = value };
        }
    }

    public class Extended
    {
        public static Extended Default() => new();
    }

    public class JudgeLine
    {
        public int Group { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Texture { get; set; } = "line.png";
        public float[] anchor { get; set; } = { 0.5f, 0.5f };
        public List<EventLayer> eventLayers { get; set; } = new();
        public Extended extended { get; set; } = new();
        public int father { get; set; } = -1;
        public int isCover { get; set; } = 1;
        public List<Note> notes { get; set; } = new();
        public int numOfNotes { get; set; }
        public int zOrder { get; set; }
    }

    public class META
    {
        public int RPEVersion { get; set; }
        public int offset { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public string song { get; set; } = string.Empty;
        public string background { get; set; } = string.Empty;
        public string composer { get; set; } = string.Empty;
        public string charter { get; set; } = string.Empty;
        public string level { get; set; } = string.Empty;
        public string illustration { get; set; } = string.Empty;
    }

    public class BPMListItem
    {
        public Beat startTime { get; set; } = new();
        public float bpm { get; set; }
    }

    public class RpeChart
    {
        public META META { get; set; } = new();
        public List<BPMListItem> BPMList { get; set; } = new();
        public List<JudgeLine> judgeLineList { get; set; } = new();
        public List<string> judgeLineGroup { get; set; } = new();
    }

    [JsonConverter(typeof(Converter))]
    public struct Beat
    {
        public const float DefaultBpm = 60f;
        
        public int bar { get; set; }
        public int numerator { get; set; }
        public int denominator { get; set; }

        public Beat()
        {
            bar = 0;
            numerator = 0;
            denominator = 1;
        }

        public Beat(int bar, int numerator, int denominator)
        {
            this.bar = bar;
            this.numerator = numerator;
            this.denominator = denominator;
        }

        /// <summary> Get the beat corresponding to the time under 60 bpm. </summary>
        public static Beat GetMilliBeat(float timeInSeconds)
        {
            var bar = Mathf.FloorToInt(timeInSeconds);
            var numerator = Mathf.RoundToInt(timeInSeconds * 1000) % 1000;
            const int denominator = 1000;
            return new(bar, numerator, denominator);
        }

        public class Converter : JsonConverter<Beat>
        {
            public override void WriteJson(JsonWriter writer, Beat value, JsonSerializer serializer)
            {
                writer.WriteStartArray();
                writer.WriteValue(value.bar);
                writer.WriteValue(value.numerator);
                writer.WriteValue(value.denominator);
                writer.WriteEndArray();
            }

            public override Beat ReadJson(JsonReader reader,
                Type objectType, Beat existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var array = serializer.Deserialize<int[]>(reader);
                if (array is null || array.Length != 3)
                    throw new JsonSerializationException("Expected [bar, numerator, denominator] array.");
                return new Beat(array[0], array[1], array[2]);
            }
        }
    }
}