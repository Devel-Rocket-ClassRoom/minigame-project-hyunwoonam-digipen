using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬 단위 배경음악 재생기. 키는 BackgroundMusicRoute가 결정한다.
/// </summary>
public sealed class BackgroundMusicPlayer : MonoBehaviour
{
    private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
    private AudioSource source;

    public string CurrentKey { get; private set; }

    private void Awake()
    {
        EnsureSource();
    }

    public void PlayForScene(SceneId sceneId, CombatContext context, ErosionStateModel erosion)
    {
        string key =
            sceneId == SceneId.Combat
                ? BackgroundMusicRoute.ForCombat(context?.Node, erosion)
                : BackgroundMusicRoute.ForScene(sceneId, Random.value);
        Play(key);
    }

    public void Play(string key)
    {
        EnsureSource();
        if (string.IsNullOrEmpty(key))
        {
            Stop();
            return;
        }

        if (CurrentKey == key && source.clip != null && source.isPlaying)
        {
            return;
        }

        AudioClip clip = LoadClip(key);
        if (clip == null)
        {
            GameLog.LogError(
                "[BackgroundMusicPlayer] BGM clip missing: Resources/"
                    + BackgroundMusicRoute.ResourceRoot
                    + key
            );
            return;
        }

        CurrentKey = key;
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.Play();
    }

    public void Stop()
    {
        EnsureSource();
        source.Stop();
        source.clip = null;
        CurrentKey = string.Empty;
    }

    private void EnsureSource()
    {
        if (source != null)
        {
            return;
        }

        source = GetComponent<AudioSource>();
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }

        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    private AudioClip LoadClip(string key)
    {
        if (clipCache.TryGetValue(key, out AudioClip cached))
        {
            return cached;
        }

        AudioClip clip = Resources.Load<AudioClip>(BackgroundMusicRoute.ResourceRoot + key);
        clipCache[key] = clip;
        return clip;
    }
}
