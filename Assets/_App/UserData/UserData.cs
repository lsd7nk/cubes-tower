using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _App
{
    public sealed class UserData : IOperation
    {
        private readonly List<IDataHandler> _dataHandlers;

        public UserSaveData UserSaveData { get; private set; } = new UserSaveData();
        public ProgressSaveData DefaultProgressSaveData { get; private set; } = new ProgressSaveData();
        public ProgressSaveData InfiniteProgressSaveData { get; private set; } = new ProgressSaveData();
        public ProgressSaveData SameColorProgressSaveData { get; private set; } = new ProgressSaveData();

        [Inject]
        public UserData(SaveService saveService)
        {
            _dataHandlers = new List<IDataHandler>
            {
                new DataHandler<UserSaveData>(saveService, SaveKeys.USER_DATA, () => UserSaveData, data => UserSaveData = data),
                new DataHandler<ProgressSaveData>(saveService, SaveKeys.DEFAULT_PROGRESS_DATA, () => DefaultProgressSaveData, data => DefaultProgressSaveData = data),
                new DataHandler<ProgressSaveData>(saveService, SaveKeys.INFINITE_PROGRESS_DATA, () => InfiniteProgressSaveData, data => InfiniteProgressSaveData = data),
                new DataHandler<ProgressSaveData>(saveService, SaveKeys.SAMECOLOR_PROGRESS_DATA, () => SameColorProgressSaveData, data => SameColorProgressSaveData = data),
            };
        }

        public async UniTask OperationInit()
        {
            Debug.Log("[UserData] Init!!!");
            await LoadGameProgress();
        }
        
        private async UniTask LoadGameProgress()
        {
            foreach (var handler in _dataHandlers)
            {
                await handler.LoadData();
            }

            if (string.IsNullOrEmpty(PlayerPrefs.GetString(PrefsKeys.GAME_ID)) && UserSaveData == null)
            {
                Debug.Log("[SaveService] No progress found.");
                OnSaveData();
            }
        }

        private void OnSaveData()
        {
            foreach (var handler in _dataHandlers)
            {
                handler.SaveData();
            }
        }

        public async UniTask LoadData<T>() where T : Data
        {
            foreach (var handler in _dataHandlers)
            {
                if (handler is not DataHandler<T>)
                {
                    continue;
                }

                await handler.LoadData();
            }
        }
        
        public void SaveData<T>() where T : Data
        {
            foreach (var handler in _dataHandlers)
            {
                if (handler is not DataHandler<T>)
                {
                    continue;
                }

                handler.SaveData();
            }
        }

    }
}
