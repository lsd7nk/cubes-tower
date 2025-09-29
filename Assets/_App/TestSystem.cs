using UnityEngine;
using Zenject;

namespace _App
{
    public sealed class TestSystem : MonoBehaviour
    {
#if UNITY_EDITOR
        private AudioService _audioService;

        [Inject]
        private void Construct(AudioService audioService)
        {
            _audioService = audioService;
        }
        
        void Update()
        {
            if (Input.GetKey(KeyCode.Q))
            {
                if (Input.GetKeyUp(KeyCode.Alpha1))
                {
                    EventDispatcher.Dispatch(new OnButtonPressed(ENavigationTypes.Menu));
                }
                else if (Input.GetKeyUp(KeyCode.Alpha2))
                {
                    _audioService.PlayMusic("Popup");
                }
                else if (Input.GetKeyUp(KeyCode.Alpha3))
                {
                    EventDispatcher.Dispatch(new ShowPopupEvent(typeof(MessagePopup), true, new MessagePopupSettings
                    {
                        Content = "Test Message",
                        Action = null
                    }));
                }
            }

            if (Input.GetKey(KeyCode.A))
            {
                if (Input.GetKeyUp(KeyCode.Alpha1))
                {
                    _audioService.PlayMusic("Background");
                }

                if (Input.GetKeyUp(KeyCode.Alpha2))
                {
                    _audioService.StopMusic("Background");
                }

                if (Input.GetKeyUp(KeyCode.Alpha3))
                {
                    _audioService.PauseMusic("Background");
                }

                if (Input.GetKeyUp(KeyCode.Alpha4))
                {
                    _audioService.SetVolumeMusic("Background", 0.5f);
                }

                if (Input.GetKeyUp(KeyCode.Alpha5))
                {
                    _audioService.SetPitchMusic("Background", 1.2f);
                }

                if (Input.GetKeyUp(KeyCode.Alpha6))
                {
                    _audioService.SetMasterVolumeMusic(0.5f);
                }
            }
        }
#endif
    }
}