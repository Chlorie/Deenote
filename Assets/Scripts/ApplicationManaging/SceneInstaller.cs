using Deenote.Audio;
using Reflex.Core;
using UnityEngine;

namespace Deenote.ApplicationManaging
{
    public class SceneInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            builder.AddSingleton(_musicController);
        }

        [SerializeField] private MusicController _musicController = null!;
    }
}