using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.Audio
{
    public class SoundsManager : SystemBase, ISoundsManager
    {
        [SerializeField] private AudioSource audioSource;

        private List<AudioSource> temporaryAudioSources = new List<AudioSource>();

        public void PlayClip(AudioClip clip)
        {
            GetFreeAudioSource().PlayOneShot(clip);
        }

        public void StopAllSounds()
        {
            audioSource.Stop();
            temporaryAudioSources.ForEach(a => a.Stop());
        }

        private AudioSource GetFreeAudioSource()
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }

            AudioSource freeSource = temporaryAudioSources.Find(a => !a.isPlaying);

            if (freeSource == null)
            {
                freeSource = Instantiate(audioSource, audioSource.transform.parent);
                temporaryAudioSources.Add(freeSource);
            }

            return freeSource;
        }
    }
}