using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace _App
{
    public sealed class AudioSegment : MonoInstaller
    {
        [SerializeField] private List<Audio> _music;
        [SerializeField] private List<Audio> _sfx;

        public override void InstallBindings()
        {
            Container.BindInstance(_music).WithId("Music").WhenInjectedInto<AudioService>();
            Container.BindInstance(_sfx).WithId("SFX").WhenInjectedInto<AudioService>();
            Container.BindInterfacesAndSelfTo<AudioService>().AsSingle().NonLazy();
        }
    }
}