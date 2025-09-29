using System;
using UnityEngine;
using Zenject;

namespace _App
{
    public sealed class InputService : IDisposable
    {
        private readonly NavigationService _navigationService;
        private readonly PopupService _popupService;
        
        [Inject]
        public InputService(NavigationService navigationService, PopupService popupService)
        {
            _navigationService = navigationService;
            _popupService = popupService;
            
            Init();
        }
        
        private void Init()
        {
            EventDispatcher.AddListener<OnButtonPressed>(HandleButtonPress);
        }
        
        public void Dispose()
        {
            EventDispatcher.RemoveListener<OnButtonPressed>(HandleButtonPress);
        }
        
        private void HandleButtonPress(OnButtonPressed e)
        {
            switch (e.EnumType)
            {
                case ENavigationTypes navigationType:
                    HandleNavigationButton(navigationType);
                    break;
                case EPopupTypes popupType:
                    HandlePopupButton(popupType);
                    break;
                default:
                    Debug.LogWarning($"[{nameof(InputService)}] Unknown button enum type: {e.EnumType}");
                    break;
            }
        }

        private void HandleNavigationButton(ENavigationTypes navigationType)
        {
            Debug.Log($"[{nameof(InputService)}] Opening navigation type: {navigationType}");
            switch (navigationType)
            {
                case ENavigationTypes.None:
                default:
                    Debug.LogWarning($"[{nameof(InputService)}] No action defined for navigation type: {navigationType}");
                    break;
                case ENavigationTypes.Game:
                    _navigationService.ScreenTransition<GameView>();
                    break;
                case ENavigationTypes.Menu:
                    _navigationService.ScreenTransition<MenuView>();
                    break;
                case ENavigationTypes.Exit:
                    Debug.Log($"[{nameof(InputService)}] Exiting the game...");
                    Application.Quit();
                    break;
            }
        }

        private void HandlePopupButton(EPopupTypes popupType)
        {
            Debug.Log($"[{nameof(InputService)}] Opening popup type: {popupType}");
            switch (popupType)
            {
                case EPopupTypes.None:
                default:
                    Debug.LogWarning($"[{nameof(InputService)}] No action defined for popup type: {popupType}");
                    break;
                case EPopupTypes.Message:
                    _popupService.ShowPopup<MessagePopup>();
                    break;
            }
        }
    }
}