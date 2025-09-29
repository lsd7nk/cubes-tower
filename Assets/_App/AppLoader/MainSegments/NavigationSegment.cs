using UnityEngine;
using Zenject;

namespace _App
{
    public sealed class NavigationSegment : MonoInstaller
    {
        [SerializeField] private Canvas _canvas;

        public override void InstallBindings()
        {
            Container.Bind<Canvas>().FromInstance(_canvas).AsSingle();

            BindingNavigationService();
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle().NonLazy();
            BindingPopupService();
        }

        private void BindingNavigationService()
        {
            Container.BindInterfacesAndSelfTo<NavigationService>().AsSingle().NonLazy();
        }

        private void BindingPopupService()
        {
            Container.BindInterfacesAndSelfTo<PopupService>().AsSingle().NonLazy();
        }
    }
}