using System;
using UnityEngine;

namespace _App
{
    [Serializable]
    public sealed class Audio
    {
        public string Name;
        public AudioClip Clip; 

        [Range(0f, 1f)]
        public float Volume = 1f;
        [Range(0.1f, 3f)]
        public float Pitch = 1f;

        public bool Loop; 

        [HideInInspector]
        public AudioSource Source;
    }
}