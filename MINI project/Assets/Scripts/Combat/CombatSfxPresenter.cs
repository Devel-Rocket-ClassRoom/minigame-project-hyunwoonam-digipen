using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 액션 효과음 재생기. Resources/Sfx/{key} 의 AudioClip 을 1회 재생한다.
    /// 키 해석: Skill → SkillData.SfxKey, Attack → 몬스터의 AttackSfxKey.
    /// 빈 키면 재생하지 않는다(무음, 무손실). 마스터 볼륨은 OptionsService가 설정한
    /// AudioListener.volume 에 의해 자동 반영된다.
    /// </summary>
    public static class CombatSfxPresenter
    {
        private const string BasicAttackSfxKey = "basicattack";
        private const float DefaultVolumeScale = 1.75f;
        private const float GroupMendVolumeScale = 2.25f;
        private const float WardCircleVolumeScale = 5.5f;

        private static readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
        private static AudioSource source;

        public static void Play(CombatAction action)
        {
            string key = SfxKeyOf(action);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            AudioClip clip = LoadClip(key);
            if (clip == null)
            {
                GameLog.LogError("[CombatSfxPresenter] SFX clip missing: Resources/Sfx/" + key);
                return;
            }

            AudioSource audioSource = EnsureSource();
            if (audioSource == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, VolumeScaleOf(key));
        }

        private static string SfxKeyOf(CombatAction action)
        {
            if (action == null)
            {
                return string.Empty;
            }

            switch (action.Type)
            {
                case CombatActionType.Attack:
                    if (action.Actor is Monster monster && !string.IsNullOrEmpty(monster.AttackSfxKey))
                    {
                        return monster.AttackSfxKey;
                    }

                    return BasicAttackSfxKey;
                case CombatActionType.Skill:
                    return action.ResolvedSkillData?.SfxKey ?? string.Empty;
                default:
                    return string.Empty;
            }
        }

        private static AudioClip LoadClip(string key)
        {
            if (clipCache.TryGetValue(key, out AudioClip cached))
            {
                return cached;
            }

            AudioClip clip = Resources.Load<AudioClip>("Sfx/" + key);
            clipCache[key] = clip;
            return clip;
        }

        private static float VolumeScaleOf(string key)
        {
            switch (key)
            {
                case "group_mend":
                    return GroupMendVolumeScale;
                case "ward_circle":
                    return WardCircleVolumeScale;
                default:
                    return DefaultVolumeScale;
            }
        }

        private static AudioSource EnsureSource()
        {
            if (source != null)
            {
                return source;
            }

            source = FindExistingSource();
            if (source != null)
            {
                return source;
            }

            var host = new GameObject("[CombatSfxPresenter]");
            host.hideFlags = HideFlags.DontSave;
            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(host);
            }

            source = Configure(host.AddComponent<AudioSource>());
            return source;
        }

        private static AudioSource FindExistingSource()
        {
            AudioSource found = null;
            AudioSource[] sources = Resources.FindObjectsOfTypeAll<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource candidate = sources[i];
                if (candidate == null || candidate.gameObject.name != "[CombatSfxPresenter]")
                {
                    continue;
                }

                if (found == null)
                {
                    found = Configure(candidate);
                    continue;
                }

                DestroyDuplicate(candidate.gameObject);
            }

            return found;
        }

        private static AudioSource Configure(AudioSource audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;
            return audioSource;
        }

        private static void DestroyDuplicate(GameObject duplicate)
        {
            if (duplicate == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(duplicate);
            }
            else
            {
                Object.DestroyImmediate(duplicate);
            }
        }
    }
}
