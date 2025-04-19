#nullable enable

using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.Core.Project;
using Deenote.UI;

namespace Deenote.Plugin
{
    public sealed class DeenotePluginContext
    {
        internal static DeenotePluginContext Instance { get; } = new();

        public ProjectManager ProjectManager { get; }
        public GamePlayManager GameManager { get; }
        public StageChartEditor Editor { get; }
        public GlobalSettings GlobalSettings { get; }

        public IMainWindow UI { get; }

        private DeenotePluginContext()
        {
            ProjectManager = MainSystem.ProjectManager;
            GameManager = MainSystem.GamePlayManager;
            Editor = MainSystem.StageChartEditor;
            GlobalSettings = MainSystem.GlobalSettings;
            UI = MainWindow.Instance;
        }
    }
}