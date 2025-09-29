using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using Zenject;

namespace _App
{
    public sealed class NavigationService : IDisposable
    {
        private readonly Canvas _canvas;
        private readonly Dictionary<Type, ObjectPool<BaseView>> _screenPools;

        private BaseView _currentView;
        private BaseView _lastView;

        private const float AnimationDuration = 0.5f;

        [Inject]
        public NavigationService(Canvas canvas)
        {
            _canvas = canvas;
            _screenPools = new Dictionary<Type, ObjectPool<BaseView>>();

            Init().Forget();
        }

        private async UniTaskVoid Init()
        {
            await InitializePools();
            EventDispatcher.AddListener<OnButtonBackClicked>(CloseCurrentWindow);
        }

        public void Dispose()
        {
            EventDispatcher.RemoveListener<OnButtonBackClicked>(CloseCurrentWindow);
        }

        private UniTask InitializePools()
        {
            var prefabSet = SettingsProvider.Get<PrefabSet>();
            var viewsList = prefabSet?.GetAllScreens()!;

            if (viewsList == null)
            {
                return UniTask.CompletedTask;
            }

            foreach (var screenPrefab in viewsList)
            {
                var pool = new ObjectPool<BaseView>(screenPrefab, _canvas.transform);
                _screenPools.Add(screenPrefab.GetType(), pool);
                pool.GetObject().Register();
            }

            ScreenTransition<MenuView>();

            return UniTask.CompletedTask;
        }

        public void UpdateCanvas()
        {
            Canvas.ForceUpdateCanvases();
        }

        public void ScreenTransition<T>(ScreenSettings settings = null) where T : BaseView
        {
            if (!_screenPools.ContainsKey(typeof(T)))
            {
                Debug.LogError($"[{nameof(NavigationService)}] No pool found for screen type {typeof(T)}. Did you forget to add it to the PrefabSet?");
                return;
            }

            if (_currentView is T)
            {
                return;
            }

            _lastView = _currentView;

            BaseView newWindow = _screenPools[typeof(T)].Get();
            newWindow.Setup(settings);

            if (_currentView != null)
            {
                DeactivateCurrentWindow(() =>
                {
                    _currentView = newWindow;
                    ActivateNewWindow(newWindow);
                });
            }
            else
            {
                _currentView = newWindow;
                ActivateNewWindow(newWindow);
            }
        }

        private void ActivateNewWindow(BaseView newScreen)
        {
            newScreen.transform.localPosition = new Vector3(0, -Screen.height, 0);
            
            var canvasGroup = GetOrAddCanvasGroup(newScreen);
            canvasGroup.alpha = 0;
            
            newScreen.Activate();
            newScreen.gameObject.SetActive(true);
            AnimateWindow(newScreen.transform, 0, AnimationDuration, canvasGroup, 1, null);
        }

        private void DeactivateCurrentWindow(Action onComplete)
        {
            var canvasGroup = GetOrAddCanvasGroup(_currentView);

            AnimateWindow(_currentView.transform, -Screen.height, AnimationDuration, canvasGroup, 0, () =>
            {
                _screenPools[_currentView.GetType()].Return(_currentView);
                _currentView.Deactivate();
                onComplete?.Invoke();
            });
        }

        private void AnimateWindow(Transform windowTransform, float targetY, float duration, CanvasGroup canvasGroup, float targetAlpha, Action onComplete)
        {
            float startY = windowTransform.localPosition.y;
            float startAlpha = canvasGroup.alpha;

            var startScale = windowTransform.localScale;
            var targetScale = Vector3.one;
            
            float startTime = Time.time;

            Observable.EveryUpdate()
                .TakeWhile(_ => Time.time - startTime < duration)
                .Subscribe(_ =>
                {
                    float progress = (Time.time - startTime) / duration;

                    float newY = Mathf.Lerp(startY, targetY, progress);
                    windowTransform.localPosition = new Vector3(0, newY, 0);

                    canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

                    windowTransform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                }, () =>
                {
                    windowTransform.localPosition = new Vector3(0, targetY, 0);
                    canvasGroup.alpha = targetAlpha;
                    windowTransform.localScale = targetScale;
                    onComplete?.Invoke();
                })
                .AddTo(windowTransform);
        }

        private CanvasGroup GetOrAddCanvasGroup(BaseView target)
        {
            var canvasGroup = target.CanvasGroup;

            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }
            
            return canvasGroup;
        }

        private void CloseCurrentWindow(OnButtonBackClicked _)
        {
            if (_lastView != null && _currentView != null && _lastView != _currentView)
            {
                DeactivateCurrentWindow(() =>
                {
                    _currentView = _lastView;
                    ActivateNewWindow(_currentView);
                });
            }
            else
            {
                ScreenTransition<MenuView>();
            }
        }
    }
}