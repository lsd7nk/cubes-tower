using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace _App
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class LocalizableText : MonoBehaviour
	{
		internal static List<LocalizableText> _instances;

		public string value;
		
		private TextMeshProUGUI _textmesh;
		private Dictionary<string, string> _replaceValues;

		// Due to the ExecuteInEditMode attribute, this will be called
		private void Awake()
		{
			I2.Loc.LocalizationManager.OnLocalizeEvent += UpdateValue;
		}

		protected void Start ()
		{
			if (_instances == null)
				_instances = new List<LocalizableText>();
			_instances.Add(this);
			if (_textmesh == null)
			{
				_textmesh = GetComponent<TextMeshProUGUI>();
			}
			UpdateValue();
		}

		protected void OnDestroy()
		{
			_instances?.Remove(this);
		}

		private string GetText()
		{
			string text = LocaleManager.GetString(value);
			if (_replaceValues != null && !string.IsNullOrEmpty(text))
			{
				foreach (var pair in _replaceValues)
				{
					text = text.Replace(pair.Key, pair.Value);
				}
			}
			return text;
		}

		public void Replace(string key, string value, bool update = true)
		{
			_replaceValues ??= new Dictionary<string, string>();
			_replaceValues[key] = value;

			if (update)
			{
				UpdateValue();
			}
		}
		
		[ContextMenu("UpdateValue")]
		public void UpdateValue()
		{
			value = value.Trim();

			if (_textmesh == null)
				return;

			string text = GetText();
			if (string.IsNullOrEmpty(text))
				return;

			_textmesh.text = GetText();
			Material material = _textmesh.materialForRendering;
			if (material != null && material.shader != null)
			{
				Shader shader = Shader.Find(material.shader.name);
				if (shader != null)
				{
					material.shader = shader;
				}
			}
		}

		public static void UpdateAll()
		{
			if (_instances == null)
				return;

			foreach (LocalizableText item in _instances)
			{
				item.UpdateValue();
			}
		}
	}
}
