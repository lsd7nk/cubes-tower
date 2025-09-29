using System.Collections.Generic;
using UnityEngine;
using System;
using I2.Loc;

namespace _App
{
    [Serializable]
    public abstract class Data { }

    
    [Serializable]
    public sealed class UserSaveData : Data
    {
        private string _userID = IDManager.GetID();
        private string _country = LocalizationManager.CurrentLanguage;
        
        public string UserID { get => _userID; set => _userID = value; }
        public string Country { get => _country; set => _country = value; }
    }

    [Serializable]
    public sealed class ProgressSaveData : Data
    {
        private int _towerHeight = 0;
        private List<string> _towerColors = new List<string>(0);
        
        public int TowerHeight { get => _towerHeight; set => _towerHeight = value; }
        public List<Color> TowerColors
        {
            get
            {
                var colors = new List<Color>(_towerColors.Count);

                foreach (var hex in _towerColors)
                {
                    if (ColorUtility.TryParseHtmlString("#" + hex, out Color color))
                    {
                        colors.Add(color);
                    }
                }

                return colors;
            }
            set
            {
                _towerColors.Clear();

                foreach (var color in value)
                {
                    _towerColors.Add(ColorUtility.ToHtmlStringRGBA(color));
                }
            }
        }
    }
}
