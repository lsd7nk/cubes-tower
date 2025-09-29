using System;
using UnityEngine;
using UnityEngine.UI;

namespace _App
{
    [RequireComponent(typeof(Button))]
    public abstract class BaseButton : MonoBehaviour
    {
        protected virtual ENavigationTypes NavigationType => ENavigationTypes.None;
        protected virtual EPopupTypes PopupType => EPopupTypes.None;
        
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button?.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _button?.onClick.RemoveListener(OnButtonClicked);
        }

        protected virtual void OnButtonClicked()
        {
            if (NavigationType != ENavigationTypes.None)
            {
                EventDispatcher.Dispatch(new OnButtonPressed(NavigationType));
            }
            else if (PopupType != EPopupTypes.None)
            {
                EventDispatcher.Dispatch(new OnButtonPressed(PopupType));
            }
        }
        
        public virtual void Setup(ButtonSettings buttonSettings, Action onClickAction)
        {
            
        }
    }

    [Serializable]
    public class ButtonSettings
    {
        
    }
}