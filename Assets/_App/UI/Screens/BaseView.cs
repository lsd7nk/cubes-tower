using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

namespace _App
{
    public abstract class BaseView : MonoBehaviour
    {
        [field: Header("View")]
        [field: SerializeField] public GameObject MainView { get; private set; }

        [field: SerializeField] public GameObject TopView { get; private set; }
        
        [field: Header("Buttons")]
        [field: SerializeField] public virtual Button Back { get; protected set; } 
        [field: Header("CanvasGroup")]
        [field: SerializeField] public virtual CanvasGroup CanvasGroup { get; protected set; }

        public virtual void Register()
        {
            LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
        }

        public virtual void OnDestroy()
        {
            LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
            Back?.onClick.RemoveAllListeners();
        }

        protected virtual void OnLanguageChanged() {}
        
        public virtual void Setup(ScreenSettings settings)
        {
        }
        
        public virtual void Activate()
        {
            Back?.onClick.AddListener(BackClicked);
        }
        
        public virtual void Deactivate()
        {
            Back?.onClick.RemoveListener(BackClicked);
            gameObject.SetActive(false);
        }
        
        public virtual void UpdateScreen()
        {
        
        }

        private void BackClicked() 
        {
            Debug.Log($"[{name}] Back Clicked");
            EventDispatcher.Dispatch(new OnButtonBackClicked());
        }
        
    }

    public class ScreenSettings
    {
        
    }
}
