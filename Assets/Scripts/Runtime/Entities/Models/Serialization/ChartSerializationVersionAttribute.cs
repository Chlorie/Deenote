#nullable enable

using System;

namespace Deenote.Entities.Models.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    internal sealed class ChartSerializationVersionAttribute : Attribute
    {
        public ChartSerializationVersions Versions { get; }

        public ChartSerializationVersionAttribute(ChartSerializationVersions versions)
        {
            Versions = versions;
        }
    }
}

[Flags]
internal enum ChartSerializationVersions
{
    None = 0,
    //DeemoV1 = 1,
    DeemoV2 = 1 << 1,
    //DeemoV3 = 1 << 2,
    //DeemoReborn = 1 << 3,
    //DeemoIIV1 = 1 << 4,
    DeemoIIV2 = 1 << 5,
    //DeemoII = DeemoIIV1 | DeemoIIV2,
    All = DeemoV2 | DeemoIIV2
}