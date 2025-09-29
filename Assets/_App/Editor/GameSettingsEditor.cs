using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace _App.Editor
{
    [CustomEditor(typeof(GameSettings))]
    public sealed class GameSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GameSettings gameSettings = (GameSettings)target;

            if (GUILayout.Button("Add Infinite Level with 50 Unique Colors"))
            {
                AddInfiniteLevelWithUniqueColors(gameSettings);
                EditorUtility.SetDirty(gameSettings);
                AssetDatabase.SaveAssets();
            }
        }

        private void AddInfiniteLevelWithUniqueColors(GameSettings gameSettings)
        {
            if (gameSettings.LevelSettings.Any(ls => ls.LevelType == ELevelType.Infinite))
            {
                Debug.LogError("An infinite level already exists.");
                return;
            }

            var newLevel = new LevelSettings()
            {
                LevelType = ELevelType.Infinite,
                Colors = GenerateUniqueColors(50)
            };

            gameSettings.LevelSettings.Add(newLevel);
            Debug.Log("Added new infinite level with 50 unique colors.");
        }

        private List<Color> GenerateUniqueColors(int numberOfColors)
        {
            List<Color> uniqueColors = new List<Color>();
            for (int i = 0; i < numberOfColors; i++)
            {
                Color newColor;
                do
                {
                    newColor = new Color(Random.value, Random.value, Random.value);
                } while (uniqueColors.Contains(newColor));

                uniqueColors.Add(newColor);
            }
            return uniqueColors;
        }
    }
}
