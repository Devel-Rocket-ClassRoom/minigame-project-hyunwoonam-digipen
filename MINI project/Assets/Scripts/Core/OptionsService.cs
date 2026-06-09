using System.IO;
using UnityEngine;

/// <summary>
/// 앱 수명 동안 유지되는 옵션(화면/볼륨/언어)을 로드·저장·적용한다.
/// persistentDataPath/options.json 에 독립적으로 저장되며 게임 런 save.json 과 무관하다.
/// </summary>
public sealed class OptionsService
{
    private const string FileName = "options.json";
    public const float DefaultMasterVolume = 0.7f;

    /// <summary>현재 적용된 옵션 스냅샷.</summary>
    public OptionSnapshot Current { get; private set; }

    /// <summary>현재 언어 코드(ko/en). Current null 이면 "ko".</summary>
    public string CurrentLanguage => Current?.LanguageCode ?? "ko";

    /// <summary>options.json 읽기. 파일 없으면 기본값 생성.</summary>
    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, FileName);
        if (File.Exists(path))
        {
            Current = JsonUtility.FromJson<OptionSnapshot>(File.ReadAllText(path));
        }

        if (Current == null)
        {
            Current = CreateDefault();
        }
    }

    /// <summary>현재 옵션을 options.json 에 저장.</summary>
    public void Save()
    {
        if (Current == null)
        {
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, FileName);
        File.WriteAllText(path, JsonUtility.ToJson(Current, true));
    }

    /// <summary>
    /// 스냅샷을 현재 옵션으로 설정하고 즉시 적용한다.
    /// Screen.fullScreenMode, AudioListener.volume, 언어 이벤트를 발행한다.
    /// </summary>
    public void Apply(OptionSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        Current = snapshot;
        Save();

        FullScreenMode mode = snapshot.Fullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
        int w =
            snapshot.ResolutionWidth > 0
                ? snapshot.ResolutionWidth
                : Screen.currentResolution.width;
        int h =
            snapshot.ResolutionHeight > 0
                ? snapshot.ResolutionHeight
                : Screen.currentResolution.height;
        Screen.SetResolution(w, h, mode);

        AudioListener.volume = Mathf.Clamp01(snapshot.MasterVolume);

        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
        {
            gsm.Events?.RaiseLanguageChanged(snapshot.LanguageCode);
        }

        GameLog.Log(
            $"[OptionsService] Applied — fullscreen={snapshot.Fullscreen} volume={snapshot.MasterVolume:F2} lang={snapshot.LanguageCode}"
        );
    }

    /// <summary>
    /// 옵션 패널 조작 중 저장 없이 마스터 볼륨만 미리 적용한다.
    /// </summary>
    public void PreviewMasterVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }

    private static OptionSnapshot CreateDefault()
    {
        return new OptionSnapshot
        {
            LanguageCode = "ko",
            MasterVolume = DefaultMasterVolume,
            Fullscreen = true,
            ResolutionWidth = 1920,
            ResolutionHeight = 1080,
        };
    }
}
