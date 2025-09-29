using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _App
{
    public sealed class AppBootstrapSegment : MonoInstaller
    {
        [SerializeField] private Slider _initializationSlider;
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        public override void InstallBindings()
        {
            Container.BindInstance(_initializationSlider).WhenInjectedInto<AppBootstrap>();
            Container.BindInstance(_loadingText).WhenInjectedInto<AppBootstrap>();
            Container.BindInstance(_canvasGroup).WhenInjectedInto<AppBootstrap>();
            Container.BindInterfacesAndSelfTo<AppBootstrap>().AsSingle().NonLazy();
        }
    }
}