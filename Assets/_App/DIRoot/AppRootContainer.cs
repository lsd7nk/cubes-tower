using Zenject;

namespace _App
{
    public sealed class AppRootContainer : MonoInstaller<AppRootContainer>
    {
        public override void InstallBindings()
        {
            Container.Bind<ISaveWrapper>().To<LocalSaveWrapper>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SaveService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UserData>().AsSingle().NonLazy();
        }
    }
}