using Google.Android.PerformanceTuner;
using UnityEngine;

namespace _App
{
	public class PerformanceTunerInitializer : MonoBehaviour
	{
		public static AndroidPerformanceTuner<FidelityParams, Annotation> Tuner { get; private set; }

		private void Start()
		{
			if (Tuner != null)
			{
				DestroyImmediate(gameObject);
				return;
			}

			DontDestroyOnLoad(gameObject);

			Tuner = new AndroidPerformanceTuner<FidelityParams, Annotation>();

			ErrorCode startErrorCode = Tuner.Start();
			Debug.Log($"[{nameof(PerformanceTunerInitializer)}] Android Performance Tuner started with code: " + startErrorCode);

			Tuner.onReceiveUploadLog += request =>
			{
				Debug.Log($"[{nameof(PerformanceTunerInitializer)}] Telemetry uploaded with request name: " + request.name);
			};

	#if UNITY_ANDROID
			// Needed for Android Performance Tuner
			QualitySettings.vSyncCount = 1;
	#endif
			Application.targetFrameRate = 120; // If vSyncCount != 'Don't Sync' (0), the value of Application.targetFrameRate will be ignored.
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}
	}
}
