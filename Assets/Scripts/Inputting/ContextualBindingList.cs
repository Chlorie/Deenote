#nullable enable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Deenote.Inputting
{
    [JsonConverter(typeof(ContextualKeyBindingListJsonConverter))]
    public readonly struct ContextualKeyBindingList
    {
        public const string GlobalContextKey = "[Global]";

        public Dictionary<string, List<KeyBinding>> Bindings { get; init; } = new();

        public ContextualKeyBindingList() { }

        public bool AddGlobalBinding(KeyBinding binding) => AddBinding(GlobalContextKey, binding);

        public bool AddBinding(string context, KeyBinding binding)
        {
            if (Bindings.TryGetValue(context, out var list)) {
                if (list.Contains(binding)) return false;
                list.Add(binding);
            }
            else
                Bindings[context] = new List<KeyBinding> { binding };
            return true;
        }

        public IReadOnlyList<KeyBinding> GetGlobalBindings() => GetContextualBindings(GlobalContextKey);

        public IReadOnlyList<KeyBinding> GetContextualBindings(string context) =>
            Bindings.TryGetValue(context, out var list)
                ? list
                : Array.Empty<KeyBinding>();

        public bool RemoveBinding(string context, KeyBinding binding) =>
            Bindings.TryGetValue(context, out var list) && list.Remove(binding);
    }

    internal class ContextualKeyBindingListJsonConverter : JsonConverter<ContextualKeyBindingList>
    {
        public override void WriteJson(JsonWriter writer, ContextualKeyBindingList value, JsonSerializer serializer) =>
            serializer.Serialize(writer, value.Bindings);

        public override ContextualKeyBindingList ReadJson(JsonReader reader, Type objectType,
            ContextualKeyBindingList existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            new() { Bindings = serializer.Deserialize<Dictionary<string, List<KeyBinding>>>(reader)! };

        public override bool CanRead => true;
    }
}