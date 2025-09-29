using UnityEngine;

namespace _App
{
    public class PopupButton : BaseButton
    {
        [SerializeField] private EPopupTypes popupType;

        protected override EPopupTypes PopupType => popupType;
    }
}