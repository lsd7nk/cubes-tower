using Cysharp.Threading.Tasks;
using System;

namespace _App
{
    public abstract class ISaveWrapper
    {
        public abstract void Save<T>(string key, T data, Action success, Action<string> fail);
        public abstract UniTask<T> Load<T>(string key, Action success, Action<string> fail);
        public abstract void Delete(string key, Action success, Action<string> fail);
    }
}