using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace _App
{
    public static class CustomUtils
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }

            if (enumerable is ICollection<T> collection)
            {
                return collection.Count < 1;
            }

            return !enumerable.Any();
        }

        public static T GetRandomElement<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            var list = collection as IList<T> ?? collection.ToList();

            if (list.Count == 0)
            {
                throw new InvalidOperationException("The collection is empty.");
            }

            return list[new Range(0, list.Count).GetRandom()];
        }

        public static List<T> GetRandomElements<T>(this IEnumerable<T> collection, int count)
        {
            var result = new List<T>();
            var collectionTemp = collection.ToList();
            for (int i = 0; i < count; i++)
            {
                var randomIndex = Random.Range(0, collectionTemp.Count);
                result.Add(collectionTemp[randomIndex]);
                collectionTemp.RemoveAt(randomIndex);
            }

            return result;
        }
    }

    [Serializable]
    public class FloatRange
    {
        public float Min;
        public float Max;

        public FloatRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public float GetRandom() => Random.Range(Min, Max);
    }

    [Serializable]
    public class Range
    {
        public int Min;
        public int Max;

        public Range(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public int GetRandom() => Random.Range(Min, Max);
    }
}