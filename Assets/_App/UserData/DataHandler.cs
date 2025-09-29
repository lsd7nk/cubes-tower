using Cysharp.Threading.Tasks;
using System;

namespace _App
{
    public sealed class DataHandler<T> : IDataHandler where T : Data
    {
        private readonly SaveService _saveService;
        private readonly Func<T> _getDataFunc;
        private readonly Action<T> _setDataFunc;
        private readonly string _key;

        private T _intermediateData;

        public DataHandler(SaveService saveService, string key, Func<T> getDataFunc, Action<T> setDataFunc)
        {
            _saveService = saveService;
            _getDataFunc = getDataFunc;
            _setDataFunc = setDataFunc;
            _key = key;
        }

        public async UniTask LoadData()
        {
            if (_intermediateData != null)
            {
                _setDataFunc(_intermediateData);
                return;
            }

            T data = await _saveService.Load<T>(_key);

            if (data == null)
            {
                return;
            }

            _setDataFunc(data);
        }

        public void SaveData()
        {
            T data = _getDataFunc();

            if (data == null)
            {
                return;
            }

            _saveService.Save(_key, data);
            _intermediateData = data;
        }
    }

    public static class SaveKeys
    {
        public const string USER_DATA = "userData";
        public const string DEFAULT_PROGRESS_DATA = "defaultLevelData";
        public const string INFINITE_PROGRESS_DATA = "infiniteLevelData";
        public const string SAMECOLOR_PROGRESS_DATA = "sameColorLevelData";
    }
}