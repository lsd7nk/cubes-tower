using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _App
{
    public sealed class AudioService : IOperation
    {
        private readonly Dictionary<string, Audio> _music;
        private readonly Dictionary<string, Audio> _sfx;
        private readonly GameObject _audioSourceGameObject;

        public AudioService(
            [Inject(Id = "Music")] List<Audio> music,
            [Inject(Id = "SFX")] List<Audio> sfx)
        {
            _music = CreateSoundDictionary(music);
            _sfx = CreateSoundDictionary(sfx);
            _audioSourceGameObject = new GameObject("AudioSources");
            Object.DontDestroyOnLoad(_audioSourceGameObject);
        }

        public UniTask OperationInit()
        {
            InitializeAudio(_music.Values);
            InitializeAudio(_sfx.Values);
            return UniTask.CompletedTask;
        }

        private Dictionary<string, Audio> CreateSoundDictionary(List<Audio> soundList)
        {
            var dictionary = new Dictionary<string, Audio>();
            foreach (var sound in soundList)
            {
                dictionary[sound.Name] = sound;
            }
            return dictionary;
        }

        private void InitializeAudio(IEnumerable<Audio> soundList)
        {
            foreach (var sound in soundList)
            {
                sound.Source = _audioSourceGameObject.AddComponent<AudioSource>();
                sound.Source.clip = sound.Clip;
                sound.Source.volume = sound.Volume;
                sound.Source.pitch = sound.Pitch;
                sound.Source.loop = sound.Loop;
            }
        }

        public void PlayMusic(string name) => Play(_music, name);
        public void PlaySFX(string name) => Play(_sfx, name);

        public void StopMusic(string name) => Stop(_music, name);
        public void StopSFX(string name) => Stop(_sfx, name);

        public void PauseMusic(string name) => Pause(_music, name);
        public void PauseSFX(string name) => Pause(_sfx, name);

        public void SetVolumeMusic(string name, float volume) => SetVolume(_music, name, volume);
        public void SetVolumeSFX(string name, float volume) => SetVolume(_sfx, name, volume);

        public void SetPitchMusic(string name, float pitch) => SetPitch(_music, name, pitch);
        public void SetPitchSFX(string name, float pitch) => SetPitch(_sfx, name, pitch);

        public void SetMasterVolumeMusic(float volume) => SetMasterVolume(_music.Values, volume);
        public void SetMasterVolumeSFX(float volume) => SetMasterVolume(_sfx.Values, volume);

        private void Play(Dictionary<string, Audio> soundDict, string name)
        {
            if (TryGetSound(soundDict, name, out var sound))
                sound.Source.Play();
        }

        private void Stop(Dictionary<string, Audio> soundDict, string name)
        {
            if (TryGetSound(soundDict, name, out var sound))
                sound.Source.Stop();
        }

        private void Pause(Dictionary<string, Audio> soundDict, string name)
        {
            if (TryGetSound(soundDict, name, out var sound))
                sound.Source.Pause();
        }

        private void SetVolume(Dictionary<string, Audio> soundDict, string name, float volume)
        {
            if (TryGetSound(soundDict, name, out var sound))
                sound.Source.volume = volume;
        }

        private void SetPitch(Dictionary<string, Audio> soundDict, string name, float pitch)
        {
            if (TryGetSound(soundDict, name, out var sound))
                sound.Source.pitch = pitch;
        }

        private bool TryGetSound(Dictionary<string, Audio> soundDict, string name, out Audio sound)
        {
            if (soundDict.TryGetValue(name, out sound))
                return true;

            Debug.LogWarning($"Sound: {name} not found!");
            return false;
        }

        private void SetMasterVolume(IEnumerable<Audio> soundList, float volume)
        {
            foreach (var sound in soundList)
                sound.Source.volume = volume;
        }
    }
}