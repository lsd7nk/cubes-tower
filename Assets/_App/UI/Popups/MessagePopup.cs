using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace _App
{
    public class MessagePopup : BasePopup
    {
        [SerializeField] private TMP_Text _content;
        
        private IDisposable _closeTimerDisposable;

        public override void Setup(PopupSettings popupSettings = null)
        {
            if (popupSettings is not MessagePopupSettings messagePopupSettings)
                return;

            _content.text = messagePopupSettings.Content;
            messagePopupSettings.Action?.Invoke();

            CloseAfterDelay(TimeSpan.FromSeconds(0.5f));
        }
        
        private void CloseAfterDelay(TimeSpan delay)
        {
            _closeTimerDisposable?.Dispose();

            _closeTimerDisposable = Observable.Timer(delay)
                .Subscribe(_ =>
                {
                    base.Close();
                })
                .AddTo(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _closeTimerDisposable?.Dispose();
        }
    }

    public class MessagePopupSettings : PopupSettings
    {
        public string Content;
        public Action Action;
    }
}