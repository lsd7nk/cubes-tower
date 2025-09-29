using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System;
using Zenject;

namespace _App
{
    public sealed class PopupService : IDisposable
    {
        private const float TWEENS_DURATION = 0.35f;

        private readonly AudioService _audioService;
        private readonly Canvas _canvas;
        private readonly Dictionary<string, BasePopup> _currentPopup;
        
        [Inject]
        public PopupService(AudioService audioService, Canvas canvas)
        {
            _audioService = audioService;
            _canvas = canvas;
            _currentPopup = new Dictionary<string, BasePopup>();

            Init();
        }

        private void Init()
        {
            EventDispatcher.AddListener<ShowPopupEvent>(HandleShowPopup);
            EventDispatcher.AddListener<PopupCloseEvent>(CloseThisPopup);
        }

        public void Dispose()
        {
            EventDispatcher.RemoveListener<ShowPopupEvent>(HandleShowPopup);
            EventDispatcher.RemoveListener<PopupCloseEvent>(CloseThisPopup);
        }

        private void HandleShowPopup(ShowPopupEvent e)
        {
            var method = typeof(PopupService).GetMethod("ShowPopup", new Type[] { typeof(bool), typeof(PopupSettings) });
            var genericMethod = method?.MakeGenericMethod(e.Type);
            
            genericMethod?.Invoke(this, new object[] { e.Animated, e.PopupSettings });
        }

        public void ShowPopup<T>(bool animated = true, PopupSettings popupSettings = null) where T : BasePopup
        {
            var prefabSet = SettingsProvider.Get<PrefabSet>();
            var popupPrefab = prefabSet.GetPopup<T>();

            if (popupPrefab == null)
            {
                Debug.LogError($"[{nameof(PopupService)}] No prefab found for popup type {typeof(T)}. Did you forget to add it to the PrefabSet?");
                return;
            }

            var popup = GameObject.Instantiate(popupPrefab, _canvas.transform);
            popup.Setup(popupSettings);
            
            _audioService.PlaySFX("Popup");
            
            _currentPopup[popup.PopupId] = popup;
            
            if (animated)
            {
                popup.SetAnimationType(Random.Range(1, 3));
                AnimatePopupOpen(popup, null);
            }
        }

        private void AnimatePopupOpen(BasePopup popup, TweenCallback onComplete)
        {
            popup.gameObject.SetActive(true);

            var sequence = DOTween.Sequence();
            
            switch (popup.AnimationType)
            {
                case 1:
                    popup.transform.localScale = Vector3.zero;
                    sequence.Append(popup.transform.DOScale(1, TWEENS_DURATION).SetEase(Ease.OutBack));
                    break;
                case 2:
                    var localPosition = popup.transform.localPosition.y;
                    popup.transform.localPosition = new Vector3(0, Screen.height, 0);
                    sequence.Append(popup.transform.DOLocalMoveY(localPosition, TWEENS_DURATION).SetEase(Ease.OutBack));
                    break;
            }

            sequence.OnComplete(onComplete);
            sequence.Play();
        }

        private void CloseThisPopup(PopupCloseEvent e)
        {
            if (_currentPopup.TryGetValue(e.PopupId, out var popup))
            {
                ClosePopup(popup, popup.AnimationType > 0, () =>
                {
                    _currentPopup.Remove(e.PopupId);
                    Object.Destroy(popup.gameObject);
                });
            }
            else
            {
                Debug.LogWarning($"[{nameof(PopupService)}] Popup with ID {e.PopupId} not found.");
            }
        }

        private void ClosePopup(BasePopup popup, bool animate, TweenCallback onComplete)
        {
            if (animate)
            {
                AnimatePopupClose(popup, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private void AnimatePopupClose(BasePopup popup, TweenCallback onComplete)
        {
            var sequence = DOTween.Sequence();

            switch (popup.AnimationType)
            {
                case 1:
                    sequence.Append(popup.transform.DOScale(0, TWEENS_DURATION).SetEase(Ease.InBack));
                    break;
                case 2:
                    sequence.Append(popup.transform.DOLocalMoveY(Screen.height, TWEENS_DURATION).SetEase(Ease.InBack));
                    break;
            }

            sequence.OnComplete(onComplete);
            sequence.Play();
        }
    }
}