#nullable enable

using UnityEngine;

namespace Deenote.UI.Views.Panels
{
    public abstract class PerspectiveViewForegroundPanel : MonoBehaviour
    {
        public abstract GameStageViewArgs Args { get; }
    }
}