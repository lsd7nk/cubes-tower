using UnityEngine;

namespace _App
{
    public class NavigationButton : BaseButton
    {
        [SerializeField] private ENavigationTypes navigationType;

        protected override ENavigationTypes NavigationType => navigationType;
    }
}