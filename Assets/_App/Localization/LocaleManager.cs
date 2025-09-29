using System;

namespace _App
{
	public static class LocaleManager
	{
		public static string FormatString(string key, params object[] args)
		{
			return string.Format(GetString(key), args);
		}

		public static string GetString(string key)
		{
			string result = I2.Loc.LocalizationManager.GetTranslation(key);
			return result ?? key;
		}

		public static int GetInt(string key, int default_value = 0)
		{
			try
			{
				return Convert.ToInt32(GetString(key));
			}
			catch
			{
				return default_value;
			}
		}

		public static float GetFloat(string key, float default_value = 0.0f)
		{
			try
			{
				return Convert.ToSingle(GetString(key));
			}
			catch
			{
				return default_value;
			}
		}
	}
}

