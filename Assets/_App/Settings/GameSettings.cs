using System.Collections.Generic;
using UnityEngine;
using System;

namespace _App
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Utils/GameSettings", order = 0)]
    public sealed class GameSettings : ScriptableObject
    {
        public List<LevelSettings> LevelSettings;

        public LevelSettings GetLevelSettings(ELevelType levelType)
        {
            foreach (var levelSetting in LevelSettings)
            {
                if (levelSetting.LevelType == levelType)
                {
                    return levelSetting;
                }
            }
            
            return null;
        }
    }

    [Serializable]
    public sealed class LevelSettings
    {
        public ELevelType LevelType;
        public List<Color> Colors;
    }

    [Serializable]
    public enum ELevelType
    {
        None = 0,
        Default = 1,
        Infinite = 2,
        SameColor = 3,
    }
}