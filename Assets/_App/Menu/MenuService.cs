using UnityEngine;
using Zenject;

namespace _App
{
    public sealed class MenuService : IInitializable
    {
        private readonly LazyInject<MenuView> _menuView;
        private readonly GameService _gameService;
        private readonly AudioService _audioService;
        private readonly PopupService _popupService;

        public MenuService(LazyInject<MenuView> menuView, 
            GameService gameService, 
            PopupService popupService,
            AudioService audioService)
        {
            _menuView = menuView;
            _gameService = gameService;
            _audioService = audioService;
            _popupService = popupService;
        }
        
        public void Initialize()
        {
            var menuView = _menuView.Value;

            if (menuView == null)
            {
                Debug.LogError($"[{nameof(MenuService)}] MenuView is null");
                return;
            }

            menuView.Initialize(this);
            _audioService.PlayMusic("Background");
        }

        public void StartGame(ELevelType levelType)
        {
            var currentLevelSettings = SettingsProvider.Get<GameSettings>().GetLevelSettings(levelType);

            _popupService.ShowPopup<MessagePopup>(true, new MessagePopupSettings
            {
                Content = $"Starting level with type - {levelType}",
                Action = () => _gameService.StartLevel(currentLevelSettings)
            });
        }
    }
}

