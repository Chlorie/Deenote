#nullable enable

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Deenote.Entities.Models.Serialization
{
    internal static class ChartSerializer
    {
        private static readonly JsonSerializerSettings DeemoV2Settings = new() {
            ContractResolver = new ContractResolver(ChartSerializationVersions.DeemoV2, JsonPropertyHandlers.IgnoreEmptySounds)
        };
        private static readonly JsonSerializerSettings DeemoIIV2Settings = new() {
            ContractResolver = new ContractResolver(ChartSerializationVersions.DeemoIIV2, JsonPropertyHandlers.EmptySoundsToNull)
        };

        public static string Serialize(ChartModel chart, ChartSerializationVersions version)
        {
            var settings = version switch {
                ChartSerializationVersions.DeemoV2 => DeemoV2Settings,
                ChartSerializationVersions.DeemoIIV2 => DeemoIIV2Settings,
                _ => throw new NotImplementedException(),
            };
            return JsonConvert.SerializeObject(chart, settings);
        }

        private sealed class ContractResolver : DefaultContractResolver
        {
            private readonly ChartSerializationVersions _version;
            private readonly Action<JsonProperty>? _jpropHandler;

            public ContractResolver(ChartSerializationVersions versions, Action<JsonProperty>? jsonPropertyHandler)
            {
                _version = versions;
                _jpropHandler = jsonPropertyHandler;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var jp = base.CreateProperty(member, memberSerialization);
                var attr = member.GetCustomAttribute<ChartSerializationVersionAttribute>();

                if (attr?.Versions.HasFlag(_version) is false) {
                    jp.ShouldSerialize = _ => false;
                }
                else {
                    _jpropHandler?.Invoke(jp);
                }
                return jp;
            }
        }

        private static class JsonPropertyHandlers
        {
            public static readonly Action<JsonProperty> IgnoreEmptySounds = jp =>
            {
                if (jp.PropertyName == "sounds")
                    jp.ShouldSerialize = o => o is not NoteModel note || note.HasSounds;
            };

            public static readonly Action<JsonProperty> EmptySoundsToNull = jp =>
            {
                if (jp.PropertyName == "sounds")
                    jp.Converter = EmptyListToNullConverter<PianoSoundValueModel>.Instance;
            };

            private sealed class EmptyListToNullConverter<T> : JsonConverter
            {
                public static EmptyListToNullConverter<T> Instance = new();

                public override bool CanConvert(Type objectType) => true;
                public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
                    => existingValue;
                public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
                    => serializer.Serialize(writer, ((List<T>)value!).Count > 0 ? value : null);
            }
        }
    }
}