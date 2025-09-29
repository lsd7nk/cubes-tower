using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace App
{
	public static class DelayUtility
	{
		private const int ONE_SECOND_DELAY_IN_MILLISECONDS = 1000;
		
		public static void InvokeNextFrame(Action callback)
		{
			InvokeAfterFrames(1, callback);
		}
		public static void InvokeAfterSecond(Action callback)
		{
			InvokeAfterSeconds(1f, callback);
		}

		public static async void InvokeAfterFrames(int skipFrames, Action callback)
		{
			await UniTask.DelayFrame(skipFrames);
			callback?.Invoke();
		}

		public static async void InvokeAfterSeconds(float seconds, Action callback)
		{
			await UniTask.Delay(Mathf.RoundToInt(ONE_SECOND_DELAY_IN_MILLISECONDS * seconds));
			callback?.Invoke();
		}
	}
}
