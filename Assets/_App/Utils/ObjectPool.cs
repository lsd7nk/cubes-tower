using System.Collections.Generic;
using UnityEngine;

namespace _App
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly T _prefab;
        private readonly Transform _parentTransform;
        private readonly Queue<T> _pool = new Queue<T>();

        public ObjectPool(T prefab, Transform parentTransform)
        {
            _prefab = prefab;
            _parentTransform = parentTransform;

            T obj = Object.Instantiate(prefab, parentTransform);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        public T Get()
        {
            if (_pool.Count > 0)
            {
                T obj = _pool.Dequeue();
                return obj;
            }
            else
            {
                T obj = Object.Instantiate(_prefab, _parentTransform);
                return obj;
            }
        }

        public T GetObject() => _pool.Count > 0 ? _pool.Peek() : null;
        
        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}