using Zenject;

namespace _App
{
    public sealed class GameSegment : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindGameService();
            BindMenuService();
        }
        
        private void BindGameService()
        {
            Container.BindInterfacesAndSelfTo<GameService>().AsSingle().NonLazy();
            Container.Bind<GameView>().FromComponentInHierarchy().AsSingle();
        }
        
        private void BindMenuService()
        {
            Container.BindInterfacesAndSelfTo<MenuService>().AsSingle().NonLazy();
            Container.Bind<MenuView>().FromComponentInHierarchy().AsSingle();
            
        }
    }
}