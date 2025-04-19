#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using System.Collections.Immutable;

namespace Deenote.Plugin
{
    public delegate UniTask DeenotePluginExecution(DeenotePluginContext context, DeenotePluginArgs args);

    public interface IDeenotePlugin
    {
        string GetName(string languageCode);
        string? GetDescription(string languageCode);
        UniTask ExecuteAsync(DeenotePluginContext context, DeenotePluginArgs args);
    }

    public interface IDeenotePluginGroup
    {
        string? GetGroupName(string LanguageCode);
        ImmutableArray<ImmutableArray<IDeenotePlugin>> Plugins { get; }
    }

    public struct DeenotePluginArgs
    {
        public LanguagePack CurrentLanguage { get; init; }
    }
}