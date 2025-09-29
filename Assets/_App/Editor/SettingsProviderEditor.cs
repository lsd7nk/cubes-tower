using UnityEditor;
using UnityEngine;

namespace _App.Editor
{
    [CustomEditor(typeof(SettingsProvider))]
    public sealed class SettingsProviderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SettingsProvider settingsProvider = (SettingsProvider)target;

            if (GUILayout.Button("Sort Alphabetically"))
            {
                settingsProvider.SortAlphabetically();
                EditorUtility.SetDirty(settingsProvider);
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Check List for Identical Types"))
            {
                settingsProvider.CheckTypes();
            }
        }
    }
}