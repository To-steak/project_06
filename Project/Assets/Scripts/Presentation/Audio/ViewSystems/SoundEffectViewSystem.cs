using System.Collections.Generic;
using Project06.Presentation.Pooling;
using UnityEngine;

namespace Project06.Presentation.Audio.ViewSystems
{
    // 소리를 낼 오브젝트와 상관없이 월드 위치에서 효과음을 재생한다. 소리를 낸 오브젝트가 꺼지거나 사라져도 끝까지 들린다.
    public static class SoundEffectViewSystem
    {
        // 풀이 처음 확보하는 AudioSource 수
        private const int DEFAULT_POOL_CAPACITY = 8;

        /// <summary>
        /// 효과음 AudioSource 풀을 만든다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="soundEffectPrefab">복제할 AudioSource 프리팹. null이면 효과음을 재생하지 않는다.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(AudioSource soundEffectPrefab, ViewContext context)
        {
            context.soundEffectPool = soundEffectPrefab != null ? new ViewPool<AudioSource>(soundEffectPrefab, DEFAULT_POOL_CAPACITY) : null;
            context.playingSoundEffects = new List<AudioSource>(DEFAULT_POOL_CAPACITY);
        }

        /// <summary>
        /// 풀에서 AudioSource를 꺼내 position으로 옮기고 clip을 처음부터 재생한다. 풀이 없거나 clip이 null이면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="clip">재생할 소리.</param>
        /// <param name="position">소리가 날 월드 위치.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Play(AudioClip clip, Vector3 position, ViewContext context)
        {
            if (context.soundEffectPool == null || clip == null)
            {
                return;
            }

            AudioSource audioSource = context.soundEffectPool.Get();
            audioSource.transform.position = position;
            audioSource.clip = clip;
            audioSource.Play();
            context.playingSoundEffects.Add(audioSource);
        }

        /// <summary>
        /// 재생이 끝난 AudioSource를 풀에 반납한다. LateUpdate에서 효과음을 재생하는 ViewSystem들 다음에 호출한다.
        /// </summary>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(ViewContext context)
        {
            List<AudioSource> playingSoundEffects = context.playingSoundEffects;
            for (int index = playingSoundEffects.Count - 1; index >= 0; index--)
            {
                AudioSource audioSource = playingSoundEffects[index];
                if (audioSource.isPlaying)
                {
                    continue;
                }

                playingSoundEffects.RemoveAt(index);
                context.soundEffectPool.Release(audioSource);
            }
        }
    }
}
