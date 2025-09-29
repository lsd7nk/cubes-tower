using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;

namespace _App
{
    [CreateAssetMenu(fileName = "SettingsProvider", menuName = "Utils/SettingsProvider", order = 0)]
    public sealed class SettingsProvider : ScriptableObject
    {
        [SerializeField] private List<ScriptableObject> _settingsList;
        public List<ScriptableObject> SettingsList => _settingsList;
        
        private static string _containerName = "SettingsProvider";
        private static Dictionary<Type, ScriptableObject> _settings;
        
        [ContextMenu("Check list for identical types")]
        public void CheckTypes()
        {
            var types = new List<Type>(_settingsList.Count);
            
            foreach (var settings in _settingsList)
            {
                var settingsType = settings.GetType();

                if (types.Contains(settingsType))
                {
                    Debug.LogError($"Found identical type: {types.Count()} - {settingsType}");
                }

                types.Add(settingsType);
            }
            
        }
        
        [ContextMenu("Sort alphabetically")]
        public void SortAlphabetically()
        {
#if UNITY_EDITOR
            _settingsList.Sort(CompareScriptableObjectsName);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        private int CompareScriptableObjectsName(ScriptableObject a, ScriptableObject b)
        {
            return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
        }
        
        private static void CheckSettings()
        {
            if (_settings != null)
            {
                return;
            }

            var settingsContainer = Resources.Load<SettingsProvider>(_containerName);
            SetupSettings(settingsContainer);
        }
        
        private static void SetupSettings(SettingsProvider settingsContainer)
        {
            try
            {
                _settings = settingsContainer.SettingsList.ToDictionary(x => x.GetType(), x => x);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public static T Get<T>() where T : ScriptableObject
        {
            CheckSettings();

            var type = typeof(T);

            if (_settings.ContainsKey(type))
            {
                return (T)_settings[type];
            }

            Debug.LogWarning($"Not found settings of type \"{type.FullName}\"");
            return null;
        }
    }
}