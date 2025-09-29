using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _App
{
    public sealed class SaveService : IOperation
    {
        private readonly ISaveWrapper _saveWrapper;

        [Inject]
        public SaveService(ISaveWrapper saveWrapper)
        {
            _saveWrapper = saveWrapper;
        }

        public UniTask OperationInit()
        {
            Debug.Log($"[SaveService] using {_saveWrapper.GetType()}");

            return UniTask.CompletedTask;
        }

        #region Save&Load

        public void Save<T>(string key, T data)
        {
            _saveWrapper.Save(key, data,
                () => Debug.Log("[SaveService] Save successful!"),
                (e) => Debug.Log($"[SaveService] Save failed! Message: \n {e}, {key}"));
        }

        public async UniTask<T> Load<T>(string key)
        {
            return await _saveWrapper.Load<T>(key,
                () => Debug.Log("[SaveService] Load successful!"),
                (e) => Debug.Log($"[SaveService] Load failed! Message: {e}, {key}"));
        }

        public void Delete(string key)
        {
            _saveWrapper.Delete(key,
                () => Debug.Log("[SaveService] Delete successful!"),
                (e) => Debug.Log($"[SaveService] Delete failed! Message: {e}, {key}"));
        }

        #endregion
    }
}