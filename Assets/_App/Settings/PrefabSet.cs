using System.Collections.Generic;
using UnityEngine;

namespace _App
{
    [CreateAssetMenu(fileName = "PrefabSet", menuName = "Utils/PrefabSet", order = 0)]
    public class PrefabSet : ScriptableObject
    {
        [field: SerializeField] public List<BaseView> Views { private set; get; } = new List<BaseView>();
        [field: SerializeField] public List<BasePopup> Popups { private set; get; } = new List<BasePopup>();

        public T GetScreen<T>() where T : BaseView
        {
            foreach (var setting in Views)
                if (setting is T window)
                    return window;
            return null;
        }

        public T GetPopup<T>() where T : BasePopup
        {
            foreach (var setting in Popups)
                if (setting is T popup)
                    return popup;
            return null;
        }

        public IList<BaseView> GetAllScreens()
        {
            return Views.IsNullOrEmpty() ? null : Views;
        }

        public IList<BasePopup> GetAllPopups()
        {
            return Popups.IsNullOrEmpty() ? null : Popups;
        }
    }
}