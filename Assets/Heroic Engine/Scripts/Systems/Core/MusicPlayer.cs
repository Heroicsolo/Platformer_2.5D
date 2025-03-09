using HeroicEngine.Utils.Math;
using HeroicEngine.Systems.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HeroicEngine.Enums;

namespace HeroicEngine.Systems.Audio
{
    public class MusicPlayer : SystemBase, ISystem
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private List<MusicEntry> musicClips = new();
        [SerializeField] [Min(0f)] private float delayBetweenClips = 3f;

        private MusicEntryType currentEntryType;
        private AudioClip lastClip;
        private bool isPlaying;

        [Serializable]
        public struct MusicEntry
        {
            public MusicEntryType entryType;
            public AudioClip[] musicClips;
        }

        private AudioClip[] GetClipsByEntryType(MusicEntryType type)
        {
            int idx = musicClips.FindIndex(c => c.entryType == type);

            if (idx >= 0)
            {
                return musicClips[idx].musicClips;
            }
            else
            {
                return null;
            }
        }

        public void Play(MusicEntryType entryType)
        {
            if (isPlaying && entryType == currentEntryType)
            {
                return;
            }
            currentEntryType = entryType;
            PlayNextClip();
        }

        public void Stop()
        {
            isPlaying = false;
            musicSource.Stop();
        }

        private void PlayNextClip()
        {
            var clips = GetClipsByEntryType(currentEntryType);

            if (clips != null)
            {
                if (lastClip != null)
                {
                    musicSource.clip = clips.ToList().GetRandomElementExceptOne(lastClip);
                }
                else
                {
                    musicSource.clip = clips.ToList().GetRandomElement();
                }
                musicSource.Play();
                lastClip = musicSource.clip;
            }

            isPlaying = true;
        }

        private void Update()
        {
            if (isPlaying && !musicSource.isPlaying)
            {
                isPlaying = false;
                Invoke(nameof(PlayNextClip), delayBetweenClips);
            }
        }
    }
}