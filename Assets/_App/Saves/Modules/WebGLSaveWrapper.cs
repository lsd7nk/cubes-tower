using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace _App
{
    public sealed class WebGLSaveWrapper : ISaveWrapper
    {
        [DllImport("__Internal")]
        private static extern void saveData(string keyName, string userId, string data);

        [DllImport("__Internal")]
        private static extern string loadData(string keyName, string userId);

        [DllImport("__Internal")]
        private static extern void deleteData(string keyName, string userId);

        public override void Save<T>(string key, T data, Action success, Action<string> fail)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(data);
                saveData(key, jsonData,PlayerPrefs.GetString(PrefsKeys.GAME_ID));
                success?.Invoke();
            }
            catch (Exception e)
            {
                fail?.Invoke(e.Message);
            }
        }

        public override async UniTask<T> Load<T>(string key, Action success, Action<string> fail)
        {
            await UniTask.Yield();
            try
            {
                string jsonData = loadData(key,PlayerPrefs.GetString(PrefsKeys.GAME_ID));
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var result = JsonUtility.FromJson<T>(jsonData);
                    success?.Invoke();
                    return result;
                }

                fail?.Invoke("[SaveService] Load - File doesn't EXIST!");
                return default;
            }
            catch (Exception e)
            {
                fail?.Invoke(e.Message);
                return default;
            }
        }

        public override void Delete(string key, Action success, Action<string> fail)
        {
            try
            {
                deleteData(key, PlayerPrefs.GetString(PrefsKeys.GAME_ID));
                success?.Invoke();
            }
            catch (Exception e)
            {
                fail?.Invoke(e.Message);
            }
        }
    }
}