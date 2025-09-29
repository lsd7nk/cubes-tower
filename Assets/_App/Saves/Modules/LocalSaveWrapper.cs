using System.Runtime.Serialization.Formatters.Binary;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.IO;
using System;

namespace _App
{
    public sealed class LocalSaveWrapper : ISaveWrapper
    {
        private string GetPath(string key) => Application.persistentDataPath + "/" + key + ".save";

        public override void Save<T>(string key, T data, Action success, Action<string> fail)
        {
            string path = GetPath(key);
            
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, data);
                }
                
                success?.Invoke();
            }
            catch (Exception e)
            {
                fail?.Invoke(e.Message);
            }
        }

        public override UniTask<T> Load<T>(string key, Action success, Action<string> fail)
        {
            string path = GetPath(key);
            
            return UniTask.RunOnThreadPool(() =>
            {
                try
                {
                    if (File.Exists(path))
                    {
                        using (FileStream stream = new FileStream(path, FileMode.Open))
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            var deserializeStream = (T)formatter.Deserialize(stream);
                            success?.Invoke();
                            return deserializeStream;
                        }
                    }
                    fail?.Invoke("[SaveService] Load - File don't EXIST!");
                    return default;
                }
                catch (Exception e)
                {
                    fail?.Invoke(e.Message);
                    return default;
                }
            });
        }
        
        public override void Delete(string key, Action success, Action<string> fail)
        {
            try
            {
                string path = GetPath(key);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    success?.Invoke();
                }
                else
                {
                    fail?.Invoke("[SaveService] Delete - PlayerPrefs not found KEY!");
                }
            }
            catch (Exception e)
            {
                fail?.Invoke(e.Message);
            }
        }
    }
}