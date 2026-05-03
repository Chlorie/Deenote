#nullable enable

using Deenote.Plugin;
using Deenote.Runtime.Plugins.ChartConversion.Rpe;
using System.Collections.Immutable;

namespace Deenote.Runtime.Plugins.ChartConversion
{
    public class ChartConversionPluginGroup : IDeenotePluginGroup
    {
        public string? GetGroupName(string languageCode)
        {
            return languageCode switch {
                "zh" => "谱面转换",
                "en" or _ => "Chart Conversion"
            };
        }

        public ImmutableArray<ImmutableArray<IDeenotePlugin>> Plugins { get; } = 
            ImmutableArray.Create(ImmutableArray.Create<IDeenotePlugin>(new ConvertToRpePlugin()));
    }
}