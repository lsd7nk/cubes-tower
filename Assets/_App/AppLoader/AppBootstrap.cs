using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Debug = UnityEngine.Debug;

namespace _App
{
    public sealed class AppBootstrap : IDisposable
    {
        private readonly TMP_Text _loadingText;
        private readonly CanvasGroup _canvasGroup;

        private List<IOperation> _initializables = new List<IOperation>();
        private string _currentLoadingText;

        private readonly ReactiveProperty<float> _loadingProgress = new ReactiveProperty<float>(0f);
        
        [Inject]
        public AppBootstrap(
            Slider initializationSlider,
            TMP_Text loadingText,
            CanvasGroup canvasGroup)
        {
            _loadingText = loadingText;
            _canvasGroup = canvasGroup;

            _loadingProgress.Subscribe(progress =>
            {
                if (initializationSlider != null)
                    initializationSlider.value = progress;

                UpdateLoadingText(progress);
            }).AddTo(_canvasGroup);
        }
        
        [Inject]
        public void Construct(List<IOperation> initializables)
        {
            _initializables = initializables;
            Init().Forget();
        }

        public void Dispose()
        {
            _loadingProgress.Dispose();
        }
        
        private async UniTaskVoid Init()
        {
            _currentLoadingText = LocaleManager.GetString("LOADING_SCREEN");
            _loadingProgress.Value = 0;

            var totalLoading = new List<string>(2);

            Stopwatch stopwatch = Stopwatch.StartNew();
            await InitializeServicesAsync();

            stopwatch.Stop();
            totalLoading.Add($"[{nameof(AppBootstrap)}] Services initialized in {stopwatch.Elapsed.TotalSeconds:F2} sec.");
            stopwatch.Restart();

            await LoadNextSceneAsync("Main");

            stopwatch.Stop();
            totalLoading.Add($"[{nameof(AppBootstrap)}] Scene 'Main' loaded in {stopwatch.Elapsed.TotalSeconds:F2} sec.");

            string logMessage = string.Join("\n", totalLoading);
            Debug.Log($"[{nameof(AppBootstrap)}] - success!\n{logMessage}");
        }

        private async UniTask InitializeServicesAsync()
        {
            int totalServices = _initializables.Count;
            int initializedServices = 0;

            foreach (var service in _initializables)
            {
                await service.OperationInit();
                initializedServices++;

                _loadingProgress.Value = (float)initializedServices / totalServices;

                await UniTask.Yield();
            }
        }

        private async UniTask LoadNextSceneAsync(string sceneName)
        {
            Debug.Log($"[{nameof(AppBootstrap)}] Loading scene: {sceneName}");
            var loadSceneOperation = SceneManager.LoadSceneAsync(sceneName);

            while (loadSceneOperation is { isDone: false })
            {
                await UniTask.Yield();
            }

            await UniTask.Delay(100);
        }

        private void UpdateLoadingText(float progress)
        {
            int percentage = Mathf.RoundToInt(progress * 100);
            _loadingText.text = $"{_currentLoadingText}... {percentage}%";
        }
    }
}