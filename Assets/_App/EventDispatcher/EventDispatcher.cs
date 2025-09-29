using System.Collections.Generic;
using System;

namespace _App
{
    public static class EventDispatcher
    {
		private static IDictionary<Type, IList<Delegate>> _listeners;

		static EventDispatcher()
		{
			_listeners = new Dictionary<Type, IList<Delegate>>();
		}

		public static void AddListener<T>(Action<T> callback) where T : IEvent
		{
			Type eventType = typeof(T);

			var list = GetListeners(eventType);

			if (list == null)
			{
				list = new List<Delegate>();
				_listeners.Add(eventType, list);
			}

			if (!list.Contains(callback))
			{
				list.Add(callback);
			}
		}

		public static void RemoveListener<T>(Action<T> callback) where T : IEvent
		{
			var eventType = typeof(T);

			if (_listeners.TryGetValue(eventType, out var list) && list != null)
			{
				if (list.Remove(callback) && list.Count == 0)
				{
					_listeners.Remove(eventType);
				}
			}
		}

		public static void Dispatch(IEvent e)
		{
			var list = GetListeners(e.GetType());

			if (list == null)
			{
				return;
			}
			
			var listeners = new List<Delegate>(list);

			foreach (var action in listeners)
			{
				action?.DynamicInvoke(e);
			}
		}

		private static IList<Delegate> GetListeners(Type eventType)
		{
			if (_listeners.TryGetValue(eventType, out var list))
			{
				return list;
			}

			return null;
		}
	}
}
