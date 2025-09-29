using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

namespace _App
{
    public abstract class BasePopup : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        
        private string _popupId = Guid.NewGuid().ToString();
        public string PopupId => _popupId;
        public int AnimationType { get; private set; }

        protected virtual void OnDestroy()
        {
            _closeButton?.onClick.RemoveAllListeners();
        }

        public virtual void Setup(PopupSettings popupSettings = null)
        {
            _closeButton?.onClick.AddListener(Close);
        }

        protected virtual void Close()
        {
            EventDispatcher.Dispatch(new PopupCloseEvent(PopupId));
            _closeButton?.onClick.RemoveListener(Close);
        }

        public void SetAnimationType(int value) => AnimationType = value;
    }

    [Serializable]
    public class PopupSettings
    {
    }
}